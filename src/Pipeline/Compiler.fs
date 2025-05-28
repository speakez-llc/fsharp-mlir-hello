module FSharpMLIR.Pipeline.Compiler

open System
open System.IO
open System.Diagnostics
open System.Reflection
open System.Runtime.InteropServices

open FSharp.Compiler.Syntax
open FSharp.Compiler.Text
open FSharp.Compiler.SyntaxTree
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
            
            // Parse the file
            let parseFileResults = 
                checker.ParseFile(fileName, sourceText, projOptions)
                |> Async.RunSynchronously
            
            // Check for errors
            if parseFileResults.ParseHadErrors then
                printfn "Parsing errors:"
                parseFileResults.Diagnostics 
                |> Array.iter (fun diag -> printfn "%s" diag.Message)
                None
            else
                // Get the parsed AST
                match parseFileResults.ParseTree with
                | Some(ParsedInput.ImplFile(ParsedImplFileInput(_, _, _, _, _, modules, _))) ->
                    // Return the first module
                    modules |> List.tryHead
                | _ -> 
                    printfn "Could not extract module from parsed file"
                    None
        with ex ->
            printfn "Error parsing F# code: %s" ex.Message
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
    /// Get the LLVM data layout string for the current platform
    /// </summary>
    let getLLVMDataLayout() =
        match getOS(), getArchitecture() with
        | OS.Windows, Architecture.X86_64 ->
            "e-m:w-p270:32:32-p271:32:32-p272:64:64-i64:64-f80:128-n8:16:32:64-S128"
        | OS.Windows, Architecture.X86 ->
            "e-m:w-p:32:32-i64:64-f80:32-n8:16:32-S32"
        | OS.Windows, Architecture.Arm64 ->
            "e-m:w-p:64:64-i32:32-i64:64-i128:128-n32:64-S128"
        | OS.MacOS, Architecture.X86_64 ->
            "e-m:o-p270:32:32-p271:32:32-p272:64:64-i64:64-f80:128-n8:16:32:64-S128"
        | OS.MacOS, Architecture.Arm64 ->
            "e-m:o-i64:64-i128:128-n32:64-S128"
        | OS.Linux, Architecture.X86_64 ->
            "e-m:e-p270:32:32-p271:32:32-p272:64:64-i64:64-f80:128-n8:16:32:64-S128"
        | OS.Linux, Architecture.Arm64 ->
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
                let cpu = getCPUName()
                let features = getTargetFeatures()
                
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
    /// Get the CPU name for the current platform
    /// </summary>
    let getCPUName() =
        match getOS(), getArchitecture() with
        | OS.MacOS, Architecture.Arm64 -> "apple-m1" // For M1 Macs
        | OS.MacOS, Architecture.X86_64 -> "core-avx2" // For Intel Macs
        | OS.Windows, Architecture.X86_64 -> "x86-64" // For Windows x64
        | OS.Windows, Architecture.Arm64 -> "generic" // For Windows ARM64
        | OS.Linux, Architecture.X86_64 -> "x86-64" // For Linux x64
        | OS.Linux, Architecture.Arm64 -> "generic" // For Linux ARM64
        | _ -> "" // Empty string means use default CPU
        
    /// <summary>
    /// Get the target features for the current platform
    /// </summary>
    let getTargetFeatures() =
        match getOS(), getArchitecture() with
        | OS.MacOS, Architecture.Arm64 -> "+v8.5a,+fp-armv8,+neon,+crc,+crypto" // For M1 Macs
        | OS.MacOS, Architecture.X86_64 -> "+avx2,+fma,+bmi,+bmi2,+popcnt,+sse4.2" // For Intel Macs
        | OS.Windows, Architecture.X86_64 -> "+avx2,+sse4.2" // For Windows x64
        | OS.Windows, Architecture.Arm64 -> "+neon" // For Windows ARM64
        | OS.Linux, Architecture.X86_64 -> "+avx2,+sse4.2" // For Linux x64
        | OS.Linux, Architecture.Arm64 -> "+neon" // For Linux ARM64
        | _ -> "" // Empty string means use default features

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
            // Use platform-specific linker
            match getOS() with
            | OS.Windows ->
                // Use MSVC linker (link.exe)
                let linkerPath = findWindowsLinker()
                let linkerArgs = sprintf "/nologo /subsystem:console /out:\"%s\" \"%s\"" outputPath objectFile
                
                let processInfo = ProcessStartInfo(
                    FileName = linkerPath,
                    Arguments = linkerArgs,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true)
                
                try
                    use process = Process.Start(processInfo)
                    process.WaitForExit()
                    
                    if process.ExitCode <> 0 then
                        printfn "Linker error: %s" (process.StandardError.ReadToEnd())
                        false
                    else
                        printfn "Successfully linked executable: %s" outputPath
                        true
                with ex ->
                    printfn "Error running linker: %s" ex.Message
                    false
                
            | OS.MacOS ->
                // Use clang as the linker on macOS
                let linkerPath = "clang"
                let linkerArgs = sprintf "-o \"%s\" \"%s\"" outputPath objectFile
                
                let processInfo = ProcessStartInfo(
                    FileName = linkerPath,
                    Arguments = linkerArgs,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true)
                
                try
                    use process = Process.Start(processInfo)
                    process.WaitForExit()
                    
                    if process.ExitCode <> 0 then
                        printfn "Linker error: %s" (process.StandardError.ReadToEnd())
                        false
                    else
                        // Make the output executable
                        try
                            if File.Exists(outputPath) then
                                // Set executable permission (chmod +x)
                                let chmodInfo = ProcessStartInfo(
                                    FileName = "chmod",
                                    Arguments = sprintf "+x \"%s\"" outputPath,
                                    UseShellExecute = false,
                                    CreateNoWindow = true)
                                
                                use chmodProcess = Process.Start(chmodInfo)
                                chmodProcess.WaitForExit()
                                
                                if chmodProcess.ExitCode <> 0 then
                                    printfn "Warning: Failed to set executable permissions"
                        with ex ->
                            printfn "Warning: Could not set executable permissions: %s" ex.Message
                        
                        printfn "Successfully linked executable: %s" outputPath
                        true
                with ex ->
                    printfn "Error running linker: %s" ex.Message
                    false
                
            | OS.Linux ->
                // Use gcc as the linker on Linux
                let linkerPath = "gcc"
                let linkerArgs = sprintf "-o \"%s\" \"%s\"" outputPath objectFile
                
                let processInfo = ProcessStartInfo(
                    FileName = linkerPath,
                    Arguments = linkerArgs,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true)
                
                try
                    use process = Process.Start(processInfo)
                    process.WaitForExit()
                    
                    if process.ExitCode <> 0 then
                        printfn "Linker error: %s" (process.StandardError.ReadToEnd())
                        false
                    else
                        // Make the output executable
                        try
                            if File.Exists(outputPath) then
                                // Set executable permission (chmod +x)
                                let chmodInfo = ProcessStartInfo(
                                    FileName = "chmod",
                                    Arguments = sprintf "+x \"%s\"" outputPath,
                                    UseShellExecute = false,
                                    CreateNoWindow = true)
                                
                                use chmodProcess = Process.Start(chmodInfo)
                                chmodProcess.WaitForExit()
                                
                                if chmodProcess.ExitCode <> 0 then
                                    printfn "Warning: Failed to set executable permissions"
                        with ex ->
                            printfn "Warning: Could not set executable permissions: %s" ex.Message
                        
                        printfn "Successfully linked executable: %s" outputPath
                        true
                with ex ->
                    printfn "Error running linker: %s" ex.Message
                    false
                
            | _ ->
                printfn "Error: Unsupported platform for linking"
                false
                
    /// <summary>
    /// Find the Windows linker (link.exe)
    /// </summary>
    let findWindowsLinker() =
        // Check common paths for link.exe
        let commonPaths = [
            @"C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Tools\MSVC\14.34.31933\bin\Hostx64\x64\link.exe"
            @"C:\Program Files\Microsoft Visual Studio\2022\Professional\VC\Tools\MSVC\14.34.31933\bin\Hostx64\x64\link.exe"
            @"C:\Program Files\Microsoft Visual Studio\2022\Enterprise\VC\Tools\MSVC\14.34.31933\bin\Hostx64\x64\link.exe"
            @"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\VC\Tools\MSVC\14.29.30133\bin\Hostx64\x64\link.exe"
            @"C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\VC\Tools\MSVC\14.29.30133\bin\Hostx64\x64\link.exe"
            @"C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\VC\Tools\MSVC\14.29.30133\bin\Hostx64\x64\link.exe"
        ]
        
        let foundPath = commonPaths |> List.tryFind File.Exists
        
        match foundPath with
        | Some path -> path
        | None ->
            // Try to find link.exe in the PATH
            let pathVar = Environment.GetEnvironmentVariable("PATH")
            let paths = pathVar.Split(Path.PathSeparator)
            
            let exePath = 
                paths 
                |> Array.map (fun p -> Path.Combine(p, "link.exe"))
                |> Array.tryFind File.Exists
                
            match exePath with
            | Some path -> path
            | None -> "link.exe" // Just use the command name and hope it's in the PATH

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
                    | OS.Windows -> ".exe"
                    | _ -> ""
                baseName + extension
        
        if options.Verbose then
            printfn "Compiling %s to %s" options.InputFile outputFile
            printfn "Optimization level: %d" options.OptimizationLevel
        
        let success = compileFile options.InputFile outputFile
        
        if success then 0 else 1