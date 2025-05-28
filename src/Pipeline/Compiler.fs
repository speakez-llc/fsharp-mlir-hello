module FSharpMLIR.Pipeline.Compiler

#nowarn "9" // Suppress warnings about unverifiable IL code with NativePtr
#nowarn "51" // Suppress warnings about native pointers
#nowarn "46" // Suppress warnings about 'process' being a reserved keyword

open System
open System.IO
open System.Diagnostics
open System.Reflection
open System.Runtime.InteropServices
open FSharp.NativeInterop // Required for NativePtr functions

open FSharp.Compiler.Syntax
open FSharp.Compiler.Text
open FSharp.Compiler.CodeAnalysis

open FSharpMLIR.Bindings.LLVM
open FSharpMLIR.Bindings.MLIR
open FSharpMLIR.Bindings.MLIRWrapper
open FSharpMLIR.Conversion
open FSharpMLIR.PlatformUtils

/// <summary>
/// F# parsing module for converting source code to AST
/// </summary>
module FSharpParser =
    open FSharp.Compiler.Syntax
    open FSharp.Reflection

    /// <summary>
    /// Parse F# code to AST
    /// </summary>
    let parseProgram (fileName: string) (fsharpCode: string) : option<SynModuleOrNamespace> =
        try
            // Create a SourceText from the code string
            let sourceText = SourceText.ofString fsharpCode
            
            // Create a default checker instance
            let checker = FSharpChecker.Create()
            
            // Get project options for a single file
            let projOptions, _ = 
                checker.GetProjectOptionsFromScript(
                    fileName,
                    sourceText,
                    assumeDotNetFramework = false)
                |> Async.RunSynchronously
            
            // Get parsing options - handling tuple return value for FCS 43.9.201
            let parsingOptions, diagnostics = checker.GetParsingOptionsFromProjectOptions(projOptions)
            
            // Check for diagnostics
            if not (List.isEmpty diagnostics) then
                printfn "Parsing options diagnostics:"
                diagnostics |> List.iter (fun diag -> printfn "%s" diag.Message)
                
            // Parse the file using our parsing options
            let parseFileResults = 
                checker.ParseFile(fileName, sourceText, parsingOptions)
                |> Async.RunSynchronously
            
            // Check for errors
            if parseFileResults.ParseHadErrors then
                printfn "Parsing errors:"
                parseFileResults.Diagnostics 
                |> Array.iter (fun diag -> printfn "%s" diag.Message)
                None
            else
                // Extract modules from parse tree
                match parseFileResults.ParseTree with
                | Some parseTree ->
                    match parseTree with
                    | ParsedInput.ImplFile(ParsedImplFileInput(_, _, _, _, _, modules, _)) ->
                        // Return the first module if available
                        match modules with
                        | [] -> 
                            printfn "No modules found in parsed input"
                            None
                        | firstModule :: _ -> 
                            Some firstModule
                    | _ ->
                        printfn "Not an implementation file"
                        None
                | None ->
                    printfn "Parse tree is empty"
                    None
        with ex ->
            printfn "Error parsing F# code: %s" ex.Message
            printfn "Stack trace: %s" ex.StackTrace
            None

/// <summary>
/// MLIR transformation passes
/// </summary>
module MLIRTransforms =
    /// <summary>
    /// Apply standard MLIR optimization passes to a module
    /// </summary>
    let applyOptimizationPasses (mlirModule: MLIRModule) : unit =
        printfn "Applying MLIR optimization passes"
        
        // Create a pass manager
        use passManager = MLIRPassManager.Create(mlirModule.Context)
        
        // Add standard optimization passes
        passManager.AddCanonicalizer()
            .AddCSE()
            |> ignore
        
        // Run the pass manager on the module
        let success = passManager.Run(mlirModule)
        
        if not success then
            printfn "Warning: Pass manager execution was not fully successful"
        else
            printfn "Successfully applied optimization passes"
    
    /// <summary>
    /// Apply MLIR to LLVM lowering passes
    /// </summary>
    let applyLowerToLLVMPasses (mlirModule: MLIRModule) : unit =
        printfn "Lowering MLIR to LLVM dialect"
        
        // Create a pass manager for lowering to LLVM
        use passManager = MLIRPassManager.Create(mlirModule.Context)
        
        // Add the LowerToLLVM pass
        passManager.AddLowerToLLVM() |> ignore
        
        // Run the pass manager on the module
        let success = passManager.Run(mlirModule)
        
        if not success then
            printfn "Warning: Lowering to LLVM dialect was not fully successful"
            mlirModule.Dump() // Dump the module for debugging
        else
            printfn "Successfully lowered MLIR to LLVM dialect"

/// <summary>
/// MLIR to LLVM conversion
/// </summary>
module MLIRToLLVM =
    /// <summary>
    /// Get the LLVM data layout string for the current platform
    /// </summary>
    let getLLVMDataLayout() =
        match getOS(), getArchitecture() with
        | PlatformOS.Windows, PlatformArch.X86_64 ->
            "e-m:w-p270:32:32-p271:32:32-p272:64:64-i64:64-f80:128-n8:16:32:64-S128"
        | PlatformOS.Windows, PlatformArch.X86 ->
            "e-m:w-p:32:32-i64:64-f80:32-n8:16:32-S32"
        | PlatformOS.Windows, PlatformArch.Arm64 ->
            "e-m:w-p:64:64-i32:32-i64:64-i128:128-n32:64-S128"
        | PlatformOS.MacOS, PlatformArch.X86_64 ->
            "e-m:o-p270:32:32-p271:32:32-p272:64:64-i64:64-f80:128-n8:16:32:64-S128"
        | PlatformOS.MacOS, PlatformArch.Arm64 ->
            "e-m:o-i64:64-i128:128-n32:64-S128"
        | PlatformOS.Linux, PlatformArch.X86_64 ->
            "e-m:e-p270:32:32-p271:32:32-p272:64:64-i64:64-f80:128-n8:16:32:64-S128"
        | PlatformOS.Linux, PlatformArch.Arm64 ->
            "e-m:e-i8:8:32-i16:16:32-i64:64-i128:128-n32:64-S128"
        | _ ->
            // Default data layout
            "e-m:e-p270:32:32-p271:32:32-p272:64:64-i64:64-f80:128-n8:16:32:64-S128"
            
    /// <summary>
    /// Convert a native string pointer to a managed string
    /// </summary>
    let getLLVMString(stringPtr: nativeint) =
        if stringPtr = nativeint 0 then
            ""
        else
            Marshal.PtrToStringAnsi(stringPtr)
            
    /// <summary>
    /// Get the CPU name for the current platform
    /// </summary>
    let getCPUName() =
        match getOS(), getArchitecture() with
        | PlatformOS.MacOS, PlatformArch.Arm64 -> "apple-m1" // For M1 Macs
        | PlatformOS.MacOS, PlatformArch.X86_64 -> "core-avx2" // For Intel Macs
        | PlatformOS.Windows, PlatformArch.X86_64 -> "x86-64" // For Windows x64
        | PlatformOS.Windows, PlatformArch.Arm64 -> "generic" // For Windows ARM64
        | PlatformOS.Linux, PlatformArch.X86_64 -> "x86-64" // For Linux x64
        | PlatformOS.Linux, PlatformArch.Arm64 -> "generic" // For Linux ARM64
        | _ -> "" // Empty string means use default CPU
        
    /// <summary>
    /// Get the target features for the current platform
    /// </summary>
    let getTargetFeatures() =
        match getOS(), getArchitecture() with
        | PlatformOS.MacOS, PlatformArch.Arm64 -> "+v8.5a,+fp-armv8,+neon,+crc,+crypto" // For M1 Macs
        | PlatformOS.MacOS, PlatformArch.X86_64 -> "+avx2,+fma,+bmi,+bmi2,+popcnt,+sse4.2" // For Intel Macs
        | PlatformOS.Windows, PlatformArch.X86_64 -> "+avx2,+sse4.2" // For Windows x64
        | PlatformOS.Windows, PlatformArch.Arm64 -> "+neon" // For Windows ARM64
        | PlatformOS.Linux, PlatformArch.X86_64 -> "+avx2,+sse4.2" // For Linux x64
        | PlatformOS.Linux, PlatformArch.Arm64 -> "+neon" // For Linux ARM64
        | _ -> "" // Empty string means use default features

    /// <summary>
    /// Convert MLIR to LLVM IR
    /// </summary>
    let convert (mlirModule: MLIRModule) : nativeint =
        printfn "Converting MLIR to LLVM IR"
        
        // First, apply LLVM dialect lowering
        MLIRTransforms.applyLowerToLLVMPasses mlirModule
        
        // Initialize the LLVM target information
        if not (LLVMInitializeNativeTarget()) then
            printfn "Warning: Failed to initialize native target"
        
        if not (LLVMInitializeNativeAsmPrinter()) then
            printfn "Warning: Failed to initialize native assembly printer"
        
        // Create an LLVM context
        let llvmContext = LLVMContextCreate()
        
        // Create an LLVM module
        let moduleName = "fidelity_module"
        let llvmModule = LLVMModuleCreateWithNameInContext(moduleName, llvmContext)
        
        // Set the target triple
        let targetTriple = getDefaultTargetTriple()
        
        // Set the data layout appropriate for the target
        let dataLayout = getLLVMDataLayout()
        LLVMSetDataLayout(llvmModule, dataLayout)
        LLVMSetTarget(llvmModule, targetTriple)
        
        // Convert MLIR module to LLVM IR
        match MLIRToLLVMConverter.convertModuleToLLVMIR mlirModule llvmModule with
        | Ok() ->
            printfn "Successfully converted MLIR to LLVM IR"
            
            // Verify the module
            let mutable errorMessage = nativeint 0
            let errorMessagePtr = &&errorMessage |> NativePtr.toNativeInt |> NativePtr.ofNativeInt<nativeint>
            
            if LLVMVerifyModule(llvmModule, LLVMVerifierFailureAction.ReturnStatus, errorMessagePtr) then
                printfn "Warning: LLVM module verification failed: %s" (getLLVMString errorMessage)
                LLVMDisposeMessage(errorMessage)
            else
                printfn "LLVM module verification successful"
            
            // Return the LLVM module
            llvmModule
        | Error msg ->
            printfn "Error converting MLIR to LLVM IR: %s" msg
            // Clean up and return a null pointer
            LLVMDisposeModule(llvmModule)
            LLVMContextDispose(llvmContext)
            nativeint 0
            
/// <summary>
/// LLVM optimization and code generation
/// </summary>
module LLVMCodeGen =
    /// <summary>
    /// Apply standard LLVM optimization passes
    /// </summary>
    let applyOptimizationPasses (llvmModule: nativeint) : unit =
        printfn "Applying LLVM optimization passes"
        
        if llvmModule <> nativeint 0 then
            // Create a pass manager
            let passManager = LLVMCreatePassManager()
            
            // Add standard optimization passes
            LLVMAddInstructionCombiningPass(passManager)
            LLVMAddPromoteMemoryToRegisterPass(passManager)
            LLVMAddGVNPass(passManager)
            LLVMAddCFGSimplificationPass(passManager)
            
            // Run the pass manager
            let success = LLVMRunPassManager(passManager, llvmModule)
            
            // Clean up
            LLVMDisposePassManager(passManager)
            
            if not success then
                printfn "Warning: LLVM optimization passes did not complete successfully"
            else
                printfn "Successfully applied LLVM optimization passes"
    
    /// <summary>
    /// Generate an object file from LLVM IR
    /// </summary>
    let generateObjectFile (llvmModule: nativeint) (outputPath: string) : bool =
        printfn "Generating object file: %s" outputPath
        
        if llvmModule = nativeint 0 then
            printfn "Error: Invalid LLVM module"
            false
        else
            // Get the default target triple
            let defaultTriple = getDefaultTargetTriple()
            printfn "Using target triple: %s" defaultTriple
            
            // Get the target from triple
            let mutable target = nativeint 0
            let mutable errorMessage = nativeint 0
            let targetPtr = &&target |> NativePtr.toNativeInt |> NativePtr.ofNativeInt<nativeint>
            let errorPtr = &&errorMessage |> NativePtr.toNativeInt |> NativePtr.ofNativeInt<nativeint>
            
            if LLVMGetTargetFromTriple(defaultTriple, targetPtr, errorPtr) <> nativeint 0 then
                printfn "Error: Failed to get target from triple"
                if errorMessage <> nativeint 0 then
                    printfn "  %s" (Marshal.PtrToStringAnsi(errorMessage))
                    LLVMDisposeMessage(errorMessage)
                false
            else
                // Get the target from the pointer
                target <- NativePtr.read targetPtr
                
                // Set CPU and features
                let cpu = MLIRToLLVM.getCPUName()
                let features = MLIRToLLVM.getTargetFeatures()
                
                // Create target machine
                let targetMachine = LLVMCreateTargetMachine(
                    target,
                    defaultTriple,
                    cpu,
                    features,
                    LLVMCodeGenOptLevel.Default,
                    LLVMRelocMode.Default,
                    LLVMCodeModel.Default)
                
                if targetMachine = nativeint 0 then
                    printfn "Error: Failed to create target machine"
                    false
                else
                    // Emit object file
                    let mutable emitError = nativeint 0
                    let emitErrorPtr = &&emitError |> NativePtr.toNativeInt |> NativePtr.ofNativeInt<nativeint>
                    
                    let success = LLVMTargetMachineEmitToFile(
                        targetMachine,
                        llvmModule,
                        outputPath,
                        LLVMCodeGenFileType.ObjectFile,
                        emitErrorPtr)
                    
                    // Clean up
                    LLVMDisposeTargetMachine(targetMachine)
                    
                    if not success then
                        printfn "Error: Failed to emit object file"
                        if emitError <> nativeint 0 then
                            printfn "  %s" (Marshal.PtrToStringAnsi(emitError))
                            LLVMDisposeMessage(emitError)
                        false
                    else
                        printfn "Successfully generated object file: %s" outputPath
                        true

/// <summary>
/// Platform-specific linker module
/// </summary>
module Linker =
    /// <summary>
    /// Link an object file to create an executable
    /// </summary>
    let linkObjectFile (objectFile: string) (outputPath: string) : bool =
        printfn "Linking object file %s to %s" objectFile outputPath
        
        // Make sure the object file exists
        if not (File.Exists(objectFile)) then
            printfn "Error: Object file does not exist: %s" objectFile
            false
        else
            // Use LLVM's lld linker
            let linkerName = 
                match getOS() with
                | PlatformOS.Windows -> "lld-link"
                | PlatformOS.MacOS -> "ld.lld"
                | PlatformOS.Linux -> "ld.lld"
                | _ -> "ld.lld" // Default to lld

            // Prepare linker arguments based on platform
            let linkerArgs = 
                match getOS() with
                | PlatformOS.Windows ->
                    sprintf "/nologo /subsystem:console /out:\"%s\" \"%s\"" outputPath objectFile
                | PlatformOS.MacOS ->
                    sprintf "-o \"%s\" \"%s\"" outputPath objectFile
                | _ -> // Linux and others
                    sprintf "-o \"%s\" \"%s\"" outputPath objectFile
            
            let processStartInfo = ProcessStartInfo(
                FileName = linkerName,
                Arguments = linkerArgs,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true)
            
            try
                use process = Process.Start(processStartInfo)
                process.WaitForExit()
                
                if process.ExitCode <> 0 then
                    printfn "LLVM linker error: %s" (process.StandardError.ReadToEnd())
                    // Fall back to clang as a more user-friendly LLVM frontend
                    printfn "Falling back to clang for linking..."
                    
                    let clangStartInfo = ProcessStartInfo(
                        FileName = "clang",
                        Arguments = sprintf "-o \"%s\" \"%s\"" outputPath objectFile,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true)
                    
                    use clangProcess = Process.Start(clangStartInfo)
                    clangProcess.WaitForExit()
                    
                    if clangProcess.ExitCode <> 0 then
                        printfn "Clang linker error: %s" (clangProcess.StandardError.ReadToEnd())
                        false
                    else
                        // Make the output executable on Unix-like systems
                        if getOS() <> PlatformOS.Windows && File.Exists(outputPath) then
                            try
                                let chmodInfo = ProcessStartInfo(
                                    FileName = "chmod",
                                    Arguments = sprintf "+x \"%s\"" outputPath,
                                    UseShellExecute = false,
                                    CreateNoWindow = true)
                                
                                use chmodProcess = Process.Start(chmodInfo)
                                chmodProcess.WaitForExit()
                            with ex ->
                                printfn "Warning: Could not set executable permissions: %s" ex.Message
                                
                        printfn "Successfully linked executable with clang: %s" outputPath
                        true
                else
                    // Make the output executable on Unix-like systems
                    if getOS() <> PlatformOS.Windows && File.Exists(outputPath) then
                        try
                            let chmodInfo = ProcessStartInfo(
                                FileName = "chmod",
                                Arguments = sprintf "+x \"%s\"" outputPath,
                                UseShellExecute = false,
                                CreateNoWindow = true)
                            
                            use chmodProcess = Process.Start(chmodInfo)
                            chmodProcess.WaitForExit()
                        with ex ->
                            printfn "Warning: Could not set executable permissions: %s" ex.Message
                            
                    printfn "Successfully linked executable with LLVM: %s" outputPath
                    true
            with ex ->
                printfn "Error running LLVM linker: %s" ex.Message
                printfn "Falling back to clang for linking..."
                
                try
                    let clangStartInfo = ProcessStartInfo(
                        FileName = "clang",
                        Arguments = sprintf "-o \"%s\" \"%s\"" outputPath objectFile,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true)
                    
                    use clangProcess = Process.Start(clangStartInfo)
                    clangProcess.WaitForExit()
                    
                    if clangProcess.ExitCode <> 0 then
                        printfn "Clang linker error: %s" (clangProcess.StandardError.ReadToEnd())
                        false
                    else
                        // Make the output executable on Unix-like systems
                        if getOS() <> PlatformOS.Windows && File.Exists(outputPath) then
                            try
                                let chmodInfo = ProcessStartInfo(
                                    FileName = "chmod",
                                    Arguments = sprintf "+x \"%s\"" outputPath,
                                    UseShellExecute = false,
                                    CreateNoWindow = true)
                                
                                use chmodProcess = Process.Start(chmodInfo)
                                chmodProcess.WaitForExit()
                            with ex ->
                                printfn "Warning: Could not set executable permissions: %s" ex.Message
                                
                        printfn "Successfully linked executable with clang: %s" outputPath
                        true
                with innerEx ->
                    printfn "Error running clang linker: %s" innerEx.Message
                    false

/// <summary>
/// Main compiler pipeline
/// </summary>
let compile (fileName: string) (fsharpCode: string) (outputPath: string) : bool =
    // Set up the native environment (PATH, etc.)
    setupNativeEnvironment()
    
    // Parse F# code to get AST
    printfn "Parsing F# code from %s" fileName
    let astOption = FSharpParser.parseProgram fileName fsharpCode
    
    match astOption with
    | None ->
        printfn "Error: Failed to parse F# code"
        false
    | Some ast ->
        // Create MLIR context and module
        use context = MLIRContext.Create()
        use mlirModule = MLIRModule.CreateEmpty(context)
        
        // Convert F# AST to MLIR
        printfn "Converting F# AST to MLIR"
        let converter = ASTToMLIR.Converter(context)
        converter.ConvertModule(mlirModule, ast)
        
        // Verify the MLIR module
        if not (mlirModule.Verify()) then
            printfn "Warning: MLIR module verification failed"
            mlirModule.Dump()
        else
            printfn "MLIR module verification successful"
            
        // Apply MLIR optimization passes
        MLIRTransforms.applyOptimizationPasses mlirModule
        
        // Convert MLIR to LLVM IR
        let llvmModule = MLIRToLLVM.convert mlirModule
        
        if llvmModule = nativeint 0 then
            printfn "Error: Failed to convert MLIR to LLVM IR"
            false
        else
            // Apply LLVM optimization passes
            LLVMCodeGen.applyOptimizationPasses llvmModule
            
            // Create temporary directory if it doesn't exist
            let tempDir = Path.Combine(Path.GetTempPath(), "FSharpMLIR")
            if not (Directory.Exists(tempDir)) then
                Directory.CreateDirectory(tempDir) |> ignore
                
            // Generate temporary object file
            let objFile = Path.Combine(tempDir, Path.GetFileNameWithoutExtension(outputPath) + ".o")
            
            let objSuccess = LLVMCodeGen.generateObjectFile llvmModule objFile
            
            // Clean up LLVM module
            LLVMDisposeModule(llvmModule)
            
            if not objSuccess then
                printfn "Error: Failed to generate object file"
                false
            else
                // Link to produce executable
                let linkSuccess = Linker.linkObjectFile objFile outputPath
                
                // Clean up temporary object file
                try
                    if File.Exists(objFile) then
                        File.Delete(objFile)
                with ex ->
                    printfn "Warning: Could not delete temporary object file: %s" ex.Message
                
                // Clean up temporary directory if it's empty
                try
                    if Directory.Exists(tempDir) && Directory.GetFiles(tempDir).Length = 0 then
                        Directory.Delete(tempDir)
                with ex ->
                    printfn "Warning: Could not delete temporary directory: %s" ex.Message
                
                if not linkSuccess then
                    printfn "Error: Failed to link executable"
                    false
                else
                    printfn "Successfully compiled %s to %s" fileName outputPath
                    true

/// <summary>
/// Compile a file to an executable
/// </summary>
let compileFile (inputPath: string) (outputPath: string) : bool =
    if not (File.Exists(inputPath)) then
        printfn "Error: Input file does not exist: %s" inputPath
        false
    else
        try
            let code = File.ReadAllText(inputPath)
            compile inputPath code outputPath
        with ex ->
            printfn "Error reading input file: %s" ex.Message
            false
            
/// <summary>
/// Command-line interface for the compiler
/// </summary>
type CompilerOptions = {
    InputFile: string
    OutputFile: string option
    OptimizationLevel: int
    Verbose: bool
}

/// <summary>
/// Parse command-line arguments
/// </summary>
let parseCommandLine (args: string[]) =
    let mutable inputFile = ""
    let mutable outputFile = None
    let mutable optimizationLevel = 1
    let mutable verbose = false
    
    let rec parseArgs (i: int) =
        if i >= args.Length then
            { InputFile = inputFile; OutputFile = outputFile; OptimizationLevel = optimizationLevel; Verbose = verbose }
        else
            match args.[i] with
            | "-o" | "--output" when i + 1 < args.Length ->
                outputFile <- Some args.[i + 1]
                parseArgs (i + 2)
            | "-O0" ->
                optimizationLevel <- 0
                parseArgs (i + 1)
            | "-O1" ->
                optimizationLevel <- 1
                parseArgs (i + 1)
            | "-O2" ->
                optimizationLevel <- 2
                parseArgs (i + 1)
            | "-O3" ->
                optimizationLevel <- 3
                parseArgs (i + 1)
            | "-v" | "--verbose" ->
                verbose <- true
                parseArgs (i + 1)
            | arg when not (arg.StartsWith("-")) ->
                inputFile <- arg
                parseArgs (i + 1)
            | _ ->
                printfn "Unknown option: %s" args.[i]
                parseArgs (i + 1)
    
    parseArgs 0

/// <summary>
/// Main entry point for the compiler
/// </summary>
let main (args: string[]) =
    let options = parseCommandLine args
    
    if String.IsNullOrEmpty(options.InputFile) then
        printfn "Error: No input file specified"
        1
    else
        let outputFile = 
            match options.OutputFile with
            | Some file -> file
            | None ->
                let baseName = Path.GetFileNameWithoutExtension(options.InputFile)
                let extension = 
                    match getOS() with
                    | PlatformOS.Windows -> ".exe"
                    | _ -> ""
                baseName + extension
        
        if options.Verbose then
            printfn "Compiling %s to %s" options.InputFile outputFile
            printfn "Optimization level: %d" options.OptimizationLevel
        
        let success = compileFile options.InputFile outputFile
        
        if success then 0 else 1