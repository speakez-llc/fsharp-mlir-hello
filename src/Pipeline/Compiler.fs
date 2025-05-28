module FSharpMLIR.Pipeline.Compiler

#nowarn "9"   // Suppress warnings about unverifiable IL code with NativePtr  
#nowarn "51"  // Suppress warnings about native pointers
#nowarn "46"  // Suppress warnings about 'process' being a reserved keyword

open System
open System.IO
open System.Diagnostics
open System.Reflection
open System.Runtime.InteropServices
open FSharp.NativeInterop

open FSharp.Compiler.Syntax
open FSharp.Compiler.Text
open FSharp.Compiler.CodeAnalysis

open FSharpMLIR.Bindings.LLVM
open FSharpMLIR.Bindings.MLIR
open FSharpMLIR.Bindings.MLIRWrapper
open FSharpMLIR.Conversion
open FSharpMLIR.PlatformUtils

/// Simple module for LLVM operations without complex wrappers
module SimpleLLVM =
    /// Test basic LLVM library availability
    let testAvailability() : bool =
        try
            printfn "Testing LLVM library..."
            let triple = LLVMGetDefaultTargetTripleString()
            printfn "Target triple: %s" triple
            triple <> "unknown-unknown-unknown"
        with ex ->
            printfn "LLVM test failed: %s" ex.Message
            false

    /// Initialize LLVM
    let initialize() : bool =
        try
            printfn "Initializing LLVM..."
            LLVMInitializeAllTargets()
            LLVMInitializeAllTargetInfos()
            LLVMInitializeAllTargetMCs()
            LLVMInitializeAllAsmPrinters()
            
            let nativeResult = LLVMInitializeNativeTarget()
            let asmResult = LLVMInitializeNativeAsmPrinter()
            
            printfn "Native target init: %d, ASM printer init: %d" nativeResult asmResult
            nativeResult = 0 && asmResult = 0
        with ex ->
            printfn "LLVM initialization failed: %s" ex.Message
            false

/// F# parsing module for converting source code to AST
module FSharpParser =
    open FSharp.Compiler.Syntax

    /// Parse F# code to AST
    let parseProgram (fileName: string) (fsharpCode: string) : SynModuleOrNamespace option =
        try
            printfn "Parsing F# code from file: %s" fileName
            printfn "Code length: %d characters" fsharpCode.Length
            
            let sourceText = SourceText.ofString fsharpCode
            
            let checker = FSharpChecker.Create()
            
            let projOptions = 
                checker.GetProjectOptionsFromScript(
                    fileName,
                    sourceText,
                    assumeDotNetFramework = false)
                |> Async.RunSynchronously
                |> fst
            
            let parsingOptions = 
                checker.GetParsingOptionsFromProjectOptions(projOptions)
                |> fst
                
            let parseFileResults = 
                checker.ParseFile(fileName, sourceText, parsingOptions)
                |> Async.RunSynchronously
            
            if parseFileResults.ParseHadErrors then
                printfn "Parsing errors detected:"
                parseFileResults.Diagnostics 
                |> Array.iter (fun diag -> printfn "  %s at %A" diag.Message diag.Range)
                None
            else
                printfn "F# parsing completed successfully"
                match parseFileResults.ParseTree with
                | ParsedInput.ImplFile(ParsedImplFileInput(_, _, _, _, _, modules, _, _, _)) ->
                    match modules with
                    | [] -> 
                        printfn "No modules found in parsed input"
                        None
                    | firstModule :: _ -> 
                        printfn "Found module: %A" firstModule
                        Some firstModule
                | ParsedInput.SigFile _ ->
                    printfn "Signature files are not supported"
                    None
        with ex ->
            printfn "Error parsing F# code: %s" ex.Message
            printfn "Stack trace: %s" ex.StackTrace
            None

/// MLIR transformation passes
module MLIRTransforms =
    /// Apply standard MLIR optimization passes to a module
    let applyOptimizationPasses (mlirModule: MLIRModule) : unit =
        printfn "Applying MLIR optimization passes"
        
        try
            use passManager = MLIRPassManager.Create(mlirModule.Context)
            
            passManager.AddCanonicalizer()
                .AddCSE()
                |> ignore
            
            let success = passManager.Run(mlirModule)
            
            if not success then
                printfn "Warning: Pass manager execution was not fully successful"
                printfn "Module after optimization passes:"
                mlirModule.Dump()
            else
                printfn "Successfully applied optimization passes"
        with ex ->
            printfn "Error applying optimization passes: %s" ex.Message
            printfn "Stack trace: %s" ex.StackTrace
            printfn "Continuing without optimization..."

/// MLIR to LLVM conversion
module MLIRToLLVM =
    /// Get the LLVM data layout string for the current platform
    let getLLVMDataLayout() =
        match getOS(), getArchitecture() with
        | PlatformOS.MacOS, PlatformArch.Arm64 ->
            "e-m:o-i64:64-i128:128-n32:64-S128"
        | PlatformOS.MacOS, PlatformArch.X86_64 ->
            "e-m:o-p270:32:32-p271:32:32-p272:64:64-i64:64-f80:128-n8:16:32:64-S128"
        | _ ->
            "e-m:e-p270:32:32-p271:32:32-p272:64:64-i64:64-f80:128-n8:16:32:64-S128"
    
    /// Convert MLIR to LLVM IR
    let convert (mlirModule: MLIRModule) : nativeint =
        printfn "Converting MLIR to LLVM IR"
        
        try
            printfn "Creating LLVM context..."
            let llvmContext = LLVMContextCreate()
            
            if llvmContext = nativeint 0 then
                printfn "Error: Failed to create LLVM context"
                nativeint 0
            else
                printfn "LLVM context created successfully"
                
                let moduleName = "fidelity_module"
                let llvmModule = LLVMModuleCreateWithNameInContext(moduleName, llvmContext)
                
                if llvmModule = nativeint 0 then
                    printfn "Error: Failed to create LLVM module"
                    LLVMContextDispose(llvmContext)
                    nativeint 0
                else
                    printfn "LLVM module created successfully"
                    
                    let targetTriple = LLVMGetDefaultTargetTripleString()
                    printfn "Setting target triple: %s" targetTriple
                    
                    let dataLayout = getLLVMDataLayout()
                    printfn "Setting data layout: %s" dataLayout
                    
                    LLVMSetDataLayout(llvmModule, dataLayout)
                    LLVMSetTarget(llvmModule, targetTriple)
                    
                    printfn "Translating MLIR to LLVM IR..."
                    match MLIRToLLVMConverter.convertModuleToLLVMIR mlirModule llvmModule with
                    | Ok() ->
                        printfn "Successfully converted MLIR to LLVM IR"
                        
                        // Verify the module
                        let mutable errorMessage = nativeint 0
                        use errorMessagePin = fixed &errorMessage
                        
                        let verifyResult = LLVMVerifyModule(llvmModule, LLVMVerifierFailureAction.ReturnStatus, NativePtr.toNativeInt errorMessagePin)
                        if verifyResult <> 0 then
                            let errorMsg = NativePtr.read errorMessagePin
                            if errorMsg <> nativeint 0 then
                                let errorStr = Marshal.PtrToStringAnsi(errorMsg)
                                printfn "Warning: LLVM module verification failed: %s" errorStr
                                LLVMDisposeMessage(errorMsg)
                        else
                            printfn "LLVM module verification successful"
                        
                        llvmModule
                    | Error msg ->
                        printfn "Error converting MLIR to LLVM IR: %s" msg
                        LLVMDisposeModule(llvmModule)
                        LLVMContextDispose(llvmContext)
                        nativeint 0
        with ex ->
            printfn "Exception during MLIR to LLVM conversion: %s" ex.Message
            printfn "Stack trace: %s" ex.StackTrace
            nativeint 0

/// LLVM optimization and code generation
module LLVMCodeGen =
    /// Apply standard LLVM optimization passes
    let applyOptimizationPasses (llvmModule: nativeint) : unit =
        printfn "Applying LLVM optimization passes"
        
        if llvmModule <> nativeint 0 then
            try
                let passManager = LLVMCreatePassManager()
                
                if passManager = nativeint 0 then
                    printfn "Warning: Failed to create LLVM pass manager"
                else
                    LLVMAddInstructionCombiningPass(passManager)
                    LLVMAddPromoteMemoryToRegisterPass(passManager)
                    LLVMAddGVNPass(passManager)
                    LLVMAddCFGSimplificationPass(passManager)
                    
                    let success = LLVMRunPassManager(passManager, llvmModule)
                    
                    LLVMDisposePassManager(passManager)
                    
                    if success = 0 then
                        printfn "Warning: LLVM optimization passes did not make changes"
                    else
                        printfn "Successfully applied LLVM optimization passes"
            with ex ->
                printfn "Error applying LLVM optimization passes: %s" ex.Message
                printfn "Stack trace: %s" ex.StackTrace
    
    /// Generate an object file from LLVM IR
    let generateObjectFile (llvmModule: nativeint) (outputPath: string) : bool =
        printfn "Generating object file: %s" outputPath
        
        if llvmModule = nativeint 0 then
            printfn "Error: Invalid LLVM module"
            false
        else
            try
                let defaultTriple = LLVMGetDefaultTargetTripleString()
                printfn "Using target triple: %s" defaultTriple
                
                let mutable target = nativeint 0
                let mutable errorMessage = nativeint 0
                
                use targetPin = fixed &target
                use errorPin = fixed &errorMessage
                
                let getTargetResult = LLVMGetTargetFromTriple(defaultTriple, NativePtr.toNativeInt targetPin, NativePtr.toNativeInt errorPin)
                
                if getTargetResult <> 0 then
                    let errorMsg = NativePtr.read errorPin
                    if errorMsg <> nativeint 0 then
                        let errorStr = Marshal.PtrToStringAnsi(errorMsg)
                        printfn "Error: Failed to get target from triple: %s" errorStr
                        LLVMDisposeMessage(errorMsg)
                    false
                else
                    target <- NativePtr.read targetPin
                    
                    let cpu = if getArchitecture() = PlatformArch.Arm64 then "apple-m1" else "generic"
                    let features = ""
                    printfn "Using CPU: %s" cpu
                    
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
                        printfn "Successfully created target machine"
                        
                        let mutable emitError = nativeint 0
                        use emitErrorPin = fixed &emitError
                        
                        let success = LLVMTargetMachineEmitToFile(
                            targetMachine,
                            llvmModule,
                            outputPath,
                            LLVMCodeGenFileType.ObjectFile,
                            NativePtr.toNativeInt emitErrorPin)
                        
                        LLVMDisposeTargetMachine(targetMachine)
                        
                        if success <> 0 then
                            let emitErrorMsg = NativePtr.read emitErrorPin
                            if emitErrorMsg <> nativeint 0 then
                                let emitErrorStr = Marshal.PtrToStringAnsi(emitErrorMsg)
                                printfn "Error: Failed to emit object file: %s" emitErrorStr
                                LLVMDisposeMessage(emitErrorMsg)
                            false
                        else
                            printfn "Successfully generated object file: %s" outputPath
                            true
            with ex ->
                printfn "Exception during object file generation: %s" ex.Message
                printfn "Stack trace: %s" ex.StackTrace
                false

/// Platform-specific linker module
module Linker =
    /// Get the appropriate linker command and arguments for the platform
    let getLinkerCommand (objectFile: string) (outputPath: string) =
        ("clang", sprintf "-o \"%s\" \"%s\"" outputPath objectFile)

    /// Set executable permissions on Unix-like systems
    let setExecutablePermissions (filePath: string) =
        if getOS() <> PlatformOS.Windows && File.Exists(filePath) then
            try
                let chmodInfo = ProcessStartInfo(
                    FileName = "chmod",
                    Arguments = sprintf "+x \"%s\"" filePath,
                    UseShellExecute = false,
                    CreateNoWindow = true)
                
                use chmodProcess = Process.Start(chmodInfo)
                chmodProcess.WaitForExit()
                
                if chmodProcess.ExitCode <> 0 then
                    printfn "Warning: chmod operation exited with code %d" chmodProcess.ExitCode
            with ex ->
                printfn "Warning: Could not set executable permissions: %s" ex.Message

    /// Link an object file to create an executable
    let linkObjectFile (objectFile: string) (outputPath: string) : bool =
        printfn "Linking object file %s to %s" objectFile outputPath
        
        if not (File.Exists(objectFile)) then
            printfn "Error: Object file does not exist: %s" objectFile
            false
        else
            try
                let (linkerCmd, linkerArgs) = getLinkerCommand objectFile outputPath
                
                let processStartInfo = ProcessStartInfo(
                    FileName = linkerCmd,
                    Arguments = linkerArgs,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true)
                
                printfn "Running linker: %s %s" linkerCmd linkerArgs
                
                use processInstance = Process.Start(processStartInfo)
                processInstance.WaitForExit()
                
                let stdout = processInstance.StandardOutput.ReadToEnd()
                let stderr = processInstance.StandardError.ReadToEnd()
                
                if not (String.IsNullOrWhiteSpace(stdout)) then
                    printfn "Linker stdout: %s" stdout
                if not (String.IsNullOrWhiteSpace(stderr)) then
                    printfn "Linker stderr: %s" stderr
                
                if processInstance.ExitCode <> 0 then
                    printfn "%s linker error (exit code %d)" linkerCmd processInstance.ExitCode
                    false
                else
                    setExecutablePermissions outputPath
                    
                    printfn "Successfully linked executable: %s" outputPath
                    true
                    
            with ex ->
                printfn "Error running linker: %s" ex.Message
                printfn "Stack trace: %s" ex.StackTrace
                false

/// Main compiler pipeline
let compile (fileName: string) (fsharpCode: string) (outputPath: string) : bool =
    try
        printfn "=== Starting compilation pipeline ==="
        printfn "Input: %s" fileName
        printfn "Output: %s" outputPath
        printfn "Platform: %A %A" (getOS()) (getArchitecture())
        
        // Initialize the native environment before any P/Invoke calls
        printfn "=== Setting up native environment ==="
        setupNativeEnvironment()
        printfn "✓ Native environment initialized"
        
        printfn "=== Testing LLVM availability ==="
        if not (SimpleLLVM.testAvailability()) then
            printfn "❌ LLVM library test failed"
            false
        else
            printfn "✓ LLVM availability test passed"
            
            printfn "=== Initializing LLVM ==="
            if not (SimpleLLVM.initialize()) then
                printfn "❌ LLVM initialization failed"
                false
            else
                printfn "✓ LLVM initialized successfully"
                
                printfn "=== Parsing F# code ==="
                let astOption = FSharpParser.parseProgram fileName fsharpCode
                
                match astOption with
                | None ->
                    printfn "❌ Failed to parse F# code"
                    false
                | Some ast ->
                    printfn "✓ F# parsing successful"
                    
                    printfn "=== Creating MLIR context ==="
                    let mlirContext = mlirContextCreate()
                    
                    if mlirContext = nativeint 0 then
                        printfn "❌ Failed to create MLIR context"
                        false
                    else
                        use context = new MLIRContext(mlirContext)
                        printfn "✓ MLIR context created"
                        
                        printfn "=== Creating MLIR module ==="
                        use mlirModule = MLIRModule.CreateEmpty(context)
                        printfn "✓ MLIR module created"
                        
                        printfn "=== Converting F# AST to MLIR ==="
                        let converter = ASTToMLIR.Converter(context)
                        converter.ConvertModule(mlirModule, ast)
                        printfn "✓ AST to MLIR conversion completed"
                        
                        printfn "=== Verifying MLIR module ==="
                        if not (mlirModule.Verify()) then
                            printfn "⚠ Warning: MLIR module verification failed"
                            printfn "MLIR module content:"
                            mlirModule.Dump()
                        else
                            printfn "✓ MLIR module verification successful"
                            
                        printfn "=== Applying MLIR optimization passes ==="
                        MLIRTransforms.applyOptimizationPasses mlirModule
                        printfn "✓ MLIR optimization passes completed"
                        
                        printfn "=== Converting MLIR to LLVM IR ==="
                        let llvmModule = MLIRToLLVM.convert mlirModule
                        
                        if llvmModule = nativeint 0 then
                            printfn "❌ Failed to convert MLIR to LLVM IR"
                            false
                        else
                            printfn "✓ MLIR to LLVM conversion successful"
                            
                            printfn "=== Applying LLVM optimization passes ==="
                            LLVMCodeGen.applyOptimizationPasses llvmModule
                            printfn "✓ LLVM optimization passes completed"
                            
                            let tempDir = Path.Combine(Path.GetTempPath(), "FSharpMLIR")
                            if not (Directory.Exists(tempDir)) then
                                Directory.CreateDirectory(tempDir) |> ignore
                                
                            let objFile = Path.Combine(tempDir, Path.GetFileNameWithoutExtension(outputPath) + ".o")
                            
                            printfn "=== Generating object file ==="
                            printfn "Object file path: %s" objFile
                            let objSuccess = LLVMCodeGen.generateObjectFile llvmModule objFile
                            
                            try
                                LLVMDisposeModule(llvmModule)
                            with
                            | ex ->
                                printfn "Warning: Error disposing LLVM module: %s" ex.Message
                            
                            if not objSuccess then
                                printfn "❌ Failed to generate object file"
                                false
                            else
                                printfn "✓ Object file generation successful"
                                
                                if File.Exists(objFile) then
                                    let objFileInfo = FileInfo(objFile)
                                    printfn "Object file size: %d bytes" objFileInfo.Length
                                
                                printfn "=== Linking to create executable ==="
                                let linkSuccess = Linker.linkObjectFile objFile outputPath
                                
                                try
                                    if File.Exists(objFile) then
                                        File.Delete(objFile)
                                with ex ->
                                    printfn "Warning: Could not delete temporary object file: %s" ex.Message
                                
                                if not linkSuccess then
                                    printfn "❌ Failed to link executable"
                                    false
                                else
                                    if File.Exists(outputPath) then
                                        let execInfo = FileInfo(outputPath)
                                        printfn "✓ Successfully compiled %s to %s" fileName outputPath
                                        printfn "Final executable size: %d bytes" execInfo.Length
                                        true
                                    else
                                        printfn "❌ Executable file was not created"
                                        false
    with ex ->
        printfn "Fatal error in compilation pipeline: %s" ex.Message
        printfn "Stack trace: %s" ex.StackTrace
        false

/// Compile a file to an executable
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
            printfn "Stack trace: %s" ex.StackTrace
            false

/// Command-line options
type CompilerOptions = {
    InputFile: string
    OutputFile: string option
    OptimizationLevel: int
    Verbose: bool
}

/// Parse command-line arguments
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

/// Main entry point for the compiler
let main (args: string[]) =
    try
        printfn "======================================================"
        printfn "Fidelity/Firefly F# Compiler starting..."
        printfn "======================================================"
        
        // Initialize native environment before processing any commands
        let initializationSuccessful =
            try
                setupNativeEnvironment()
                true
            with ex ->
                printfn "Failed to initialize native environment: %s" ex.Message
                false
        
        if not initializationSuccessful then
            1
        else
            printfn "Platform: %A %A" (getOS()) (getArchitecture())
            printfn "Arguments: %A" args
            printfn "Current directory: %s" (Directory.GetCurrentDirectory())
            printfn "======================================================"
            
            let options = parseCommandLine args
            
            if String.IsNullOrEmpty(options.InputFile) then
                printfn "Error: No input file specified"
                printfn "Usage: Firefly <input.fs> [-o <output>] [-O0|-O1|-O2|-O3] [-v|--verbose]"
                1
            else
                let outputFile = 
                    match options.OutputFile with
                    | Some file -> file
                    | None ->
                        let baseName = Path.GetFileNameWithoutExtension(options.InputFile)
                        match getOS() with
                        | PlatformOS.Windows -> baseName + ".exe"
                        | _ -> baseName
                
                if options.Verbose then
                    printfn "======================================================"
                    printfn "Fidelity/Firefly F# Compiler - Verbose Mode"
                    printfn "======================================================"
                    printfn "Input file: %s" options.InputFile
                    printfn "Output file: %s" outputFile
                    printfn "Optimization level: %d" options.OptimizationLevel
                    printfn "Target platform: %A %A" (getOS()) (getArchitecture())
                    printfn "Target triple: %s" (LLVMGetDefaultTargetTripleString())
                    printfn "======================================================"
                
                let success = compileFile options.InputFile outputFile
                
                if success then 
                    printfn "======================================================"
                    printfn "✓ Compilation completed successfully"
                    printfn "Output: %s" outputFile
                    printfn "======================================================"
                    0 
                else 
                    printfn "======================================================"
                    printfn "❌ Compilation failed"
                    printfn "======================================================"
                    1
    with ex ->
        printfn "======================================================"
        printfn "💥 Fatal error: %s" ex.Message
        if ex.StackTrace <> null then
            printfn "Stack trace: %s" ex.StackTrace
        printfn "======================================================"
        1

[<EntryPoint>]
let programMain args = main args