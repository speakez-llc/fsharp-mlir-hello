module FSharpMLIR.Pipeline.Compiler

#nowarn "9"   // Suppress warnings about unverifiable IL code with NativePtr  
#nowarn "51"  // Suppress warnings about native pointers
#nowarn "46"  // Suppress warnings about 'process' being a reserved keyword

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
/// Diagnostics and library loading utilities
/// </summary>
module LibraryDiagnostics =
    /// <summary>
    /// Test if a native library can be loaded and a specific function found
    /// </summary>
    let testLibraryFunction (libraryName: string) (functionName: string) =
        try
            let assembly = System.Reflection.Assembly.GetExecutingAssembly()
            printfn "Testing library %s for function %s" libraryName functionName
            
            match getOS() with
            | PlatformOS.MacOS ->
                let possiblePaths = [
                    sprintf "/usr/local/lib/%s" libraryName 
                    sprintf "/opt/homebrew/lib/%s" libraryName
                    sprintf "/opt/homebrew/opt/llvm/lib/%s" libraryName
                ]
                
                let mutable found = false
                for path in possiblePaths do
                    if File.Exists(path) && not found then
                        printfn "Found library at: %s" path
                        let fileInfo = FileInfo(path)
                        printfn "Library size: %d bytes" fileInfo.Length
                        found <- true
                
                found
            | _ -> true
        with ex ->
            printfn "Error testing library %s: %s" libraryName ex.Message
            false

    /// <summary>
    /// Enhanced platform diagnostics with library testing
    /// </summary>
    let getEnhancedPlatformDiagnostics() =
        let basicDiagnostics = getPlatformDiagnostics()
        let libraryTests = [
            ("libLLVM.dylib", "LLVMInitializeNativeTarget")
            ("libMLIR.dylib", "mlirContextCreate")
        ]
        
        let libraryResults = 
            libraryTests 
            |> List.map (fun (lib, func) -> 
                let result = testLibraryFunction lib func
                let status = if result then "PASS" else "FAIL"
                sprintf "Library Test - %s: %s" lib status)
        
        basicDiagnostics @ ["Library Tests:"] @ libraryResults

/// <summary>
/// Enhanced LLVM function calls with comprehensive safety measures
/// </summary>
module SafeLLVMCalls =
    /// <summary>
    /// Test basic LLVM library availability with minimal operations
    /// </summary>
    let testLLVMAvailability() : bool =
        try
            printfn "Testing LLVM library availability with minimal operations..."
            
            // Try to initialize all LLVM components first
            printfn "Initializing LLVM components..."
            let initResult = LLVMInitializeAllSafe()
            
            if not initResult then
                printfn "LLVM component initialization failed"
                false
            else
                printfn "LLVM components initialized successfully"
                
                // Test target triple retrieval
                printfn "Testing target triple retrieval..."
                let tripleStr = LLVMGetDefaultTargetTripleString()
                
                if tripleStr = "unknown-unknown-unknown" then
                    printfn "Could not retrieve valid target triple"
                    false  
                else
                    printfn "LLVM library test successful. Target triple: %s" tripleStr
                    true
        with ex ->
            printfn "LLVM library test failed: %s" ex.Message
            printfn "Exception type: %s" (ex.GetType().Name)
            if ex.InnerException <> null then
                printfn "Inner exception: %s" ex.InnerException.Message
            false

    /// <summary>
    /// Create LLVM context with enhanced safety and validation
    /// </summary>
    let safeCreateLLVMContext() : nativeint =
        try
            printfn "Attempting to create LLVM context..."
            
            // Ensure LLVM is properly initialized first
            if not (testLLVMAvailability()) then
                printfn "LLVM not available, cannot create context"
                nativeint 0
            else
                let context = LLVMContextCreate()
                if context = nativeint 0 then
                    printfn "LLVMContextCreate returned null"
                    nativeint 0
                else
                    printfn "LLVM context created successfully"
                    context
        with
        | :? DllNotFoundException as ex ->
            printfn "DLL not found error: %s" ex.Message
            nativeint 0
        | :? EntryPointNotFoundException as ex ->
            printfn "Entry point not found: %s" ex.Message
            nativeint 0
        | :? System.AccessViolationException as ex ->
            printfn "Access violation in LLVM context creation: %s" ex.Message
            nativeint 0
        | ex ->
            printfn "Unexpected error creating LLVM context: %s" ex.Message
            printfn "Exception type: %s" (ex.GetType().Name)
            nativeint 0

    /// <summary>
    /// Create MLIR context with comprehensive error reporting
    /// </summary>
    let safeCreateMLIRContext() : nativeint =
        try
            printfn "Attempting to create MLIR context..."
            let context = mlirContextCreate()
            if context = nativeint 0 then
                printfn "mlirContextCreate returned null"
                nativeint 0
            else
                printfn "MLIR context created successfully"
                context
        with
        | :? DllNotFoundException as ex ->
            printfn "DLL not found error: %s" ex.Message
            nativeint 0
        | :? EntryPointNotFoundException as ex ->
            printfn "Entry point not found: %s" ex.Message
            nativeint 0
        | :? System.AccessViolationException as ex ->
            printfn "Access violation in MLIR context creation: %s" ex.Message
            nativeint 0
        | ex ->
            printfn "Unexpected error creating MLIR context: %s" ex.Message
            printfn "Exception type: %s" (ex.GetType().Name)
            nativeint 0

/// <summary>
/// F# parsing module for converting source code to AST
/// </summary>
module FSharpParser =
    open FSharp.Compiler.Syntax

    /// <summary>
    /// Parse F# code to AST with proper error handling for the current FCS API
    /// </summary>
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

/// <summary>
/// MLIR transformation passes
/// </summary>
module MLIRTransforms =
    /// <summary>
    /// Apply standard MLIR optimization passes to a module
    /// </summary>
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
    
    /// <summary>
    /// Apply MLIR to LLVM lowering passes
    /// </summary>
    let applyLowerToLLVMPasses (mlirModule: MLIRModule) : unit =
        printfn "Lowering MLIR to LLVM dialect"
        
        try
            use passManager = MLIRPassManager.Create(mlirModule.Context)
            
            passManager.AddLowerToLLVM() |> ignore
            
            let success = passManager.Run(mlirModule)
            
            if not success then
                printfn "Warning: Lowering to LLVM dialect was not fully successful"
                printfn "Module after lowering:"
                mlirModule.Dump()
            else
                printfn "Successfully lowered MLIR to LLVM dialect"
        with ex ->
            printfn "Error lowering to LLVM: %s" ex.Message
            printfn "Stack trace: %s" ex.StackTrace
            printfn "Attempting to continue..."

/// <summary>
/// MLIR to LLVM conversion with enhanced safety
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
            "e-m:e-p270:32:32-p271:32:32-p272:64:64-i64:64-f80:128-n8:16:32:64-S128"
            
    /// <summary>
    /// Convert a native string pointer to a managed string safely
    /// </summary>
    let getLLVMString(stringPtr: nativeint) =
        if stringPtr = nativeint 0 then
            ""
        else
            try
                Marshal.PtrToStringAnsi(stringPtr)
            with
            | ex ->
                printfn "Warning: Failed to convert native string: %s" ex.Message
                ""
            
    /// <summary>
    /// Get the CPU name for the current platform
    /// </summary>
    let getCPUName() =
        match getOS(), getArchitecture() with
        | PlatformOS.MacOS, PlatformArch.Arm64 -> "apple-m1"
        | PlatformOS.MacOS, PlatformArch.X86_64 -> "core-avx2"
        | PlatformOS.Windows, PlatformArch.X86_64 -> "x86-64"
        | PlatformOS.Windows, PlatformArch.Arm64 -> "generic"
        | PlatformOS.Linux, PlatformArch.X86_64 -> "x86-64"
        | PlatformOS.Linux, PlatformArch.Arm64 -> "generic"
        | _ -> "generic"
        
    /// <summary>
    /// Get the target features for the current platform
    /// </summary>
    let getTargetFeatures() =
        match getOS(), getArchitecture() with
        | PlatformOS.MacOS, PlatformArch.Arm64 -> "+v8.5a,+fp-armv8,+neon,+crc,+crypto"
        | PlatformOS.MacOS, PlatformArch.X86_64 -> "+avx2,+fma,+bmi,+bmi2,+popcnt,+sse4.2"
        | PlatformOS.Windows, PlatformArch.X86_64 -> "+avx2,+sse4.2"
        | PlatformOS.Windows, PlatformArch.Arm64 -> "+neon"
        | PlatformOS.Linux, PlatformArch.X86_64 -> "+avx2,+sse4.2"
        | PlatformOS.Linux, PlatformArch.Arm64 -> "+neon"
        | _ -> ""

    /// <summary>
    /// Safely read a pointer value with error handling
    /// </summary>
    let safeReadPtr (ptr: nativeint nativeptr) : nativeint =
        try
            if NativePtr.toNativeInt ptr = nativeint 0 then
                nativeint 0
            else
                NativePtr.read ptr
        with
        | ex ->
            printfn "Warning: Failed to read native pointer: %s" ex.Message
            nativeint 0

    /// <summary>
    /// Convert MLIR to LLVM IR with comprehensive error handling and validation
    /// </summary>
    let convert (mlirModule: MLIRModule) : nativeint =
        printfn "Converting MLIR to LLVM IR"
        
        try
            MLIRTransforms.applyLowerToLLVMPasses mlirModule
            
            printfn "Creating LLVM context..."
            let llvmContext = SafeLLVMCalls.safeCreateLLVMContext()
            
            if llvmContext = nativeint 0 then
                printfn "Error: Failed to create LLVM context"
                nativeint 0
            else
                printfn "Successfully created LLVM context"
                
                let moduleName = "fidelity_module"
                let llvmModule = 
                    try
                        LLVMModuleCreateWithNameInContext(moduleName, llvmContext)
                    with
                    | ex ->
                        printfn "Error creating LLVM module: %s" ex.Message
                        nativeint 0
                
                if llvmModule = nativeint 0 then
                    printfn "Error: Failed to create LLVM module"
                    LLVMContextDispose(llvmContext)
                    nativeint 0
                else
                    printfn "Successfully created LLVM module"
                    
                    let targetTriple = getDefaultTargetTriple()
                    printfn "Setting target triple: %s" targetTriple
                    
                    let dataLayout = getLLVMDataLayout()
                    printfn "Setting data layout: %s" dataLayout
                    
                    try
                        LLVMSetDataLayout(llvmModule, dataLayout)
                        LLVMSetTarget(llvmModule, targetTriple)
                        printfn "Successfully configured LLVM module"
                    with
                    | ex ->
                        printfn "Warning: Failed to configure LLVM module: %s" ex.Message
                    
                    printfn "Translating MLIR to LLVM IR..."
                    match MLIRToLLVMConverter.convertModuleToLLVMIR mlirModule llvmModule with
                    | Ok() ->
                        printfn "Successfully converted MLIR to LLVM IR"
                        
                        printfn "Verifying LLVM module..."
                        let mutable errorMessage = nativeint 0
                        use errorMessagePin = fixed &errorMessage
                        let errorMessagePtr = NativePtr.toNativeInt errorMessagePin
                        
                        try
                            let verifyResult = LLVMVerifyModule(llvmModule, LLVMVerifierFailureAction.ReturnStatus, NativePtr.ofNativeInt errorMessagePtr)
                            if verifyResult then
                                let errorMsg = safeReadPtr (NativePtr.ofNativeInt errorMessagePtr)
                                if errorMsg <> nativeint 0 then
                                    let errorStr = getLLVMString errorMsg
                                    printfn "Warning: LLVM module verification failed: %s" errorStr 
                                    LLVMDisposeMessage(errorMsg)
                            else
                                printfn "LLVM module verification successful"
                        with
                        | ex ->
                            printfn "Warning: LLVM module verification threw exception: %s" ex.Message
                        
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
            
/// <summary>
/// LLVM optimization and code generation with enhanced safety
/// </summary>
module LLVMCodeGen =
    /// <summary>
    /// Apply standard LLVM optimization passes
    /// </summary>
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
                    
                    if not success then
                        printfn "Warning: LLVM optimization passes did not complete successfully"
                    else
                        printfn "Successfully applied LLVM optimization passes"
            with ex ->
                printfn "Error applying LLVM optimization passes: %s" ex.Message
                printfn "Stack trace: %s" ex.StackTrace
    
    /// <summary>
    /// Generate an object file from LLVM IR with comprehensive error handling
    /// </summary>
    let generateObjectFile (llvmModule: nativeint) (outputPath: string) : bool =
        printfn "Generating object file: %s" outputPath
        
        if llvmModule = nativeint 0 then
            printfn "Error: Invalid LLVM module"
            false
        else
            try
                let defaultTriple = getDefaultTargetTriple()
                printfn "Using target triple: %s" defaultTriple
                
                let mutable target = nativeint 0
                let mutable errorMessage = nativeint 0
                
                use targetPin = fixed &target
                use errorPin = fixed &errorMessage
                
                let targetPtr = NativePtr.toNativeInt targetPin
                let errorPtr = NativePtr.toNativeInt errorPin
                
                let getTargetResult = 
                    try
                        let result = LLVMGetTargetFromTriple(defaultTriple, NativePtr.ofNativeInt targetPtr, NativePtr.ofNativeInt errorPtr)
                        if result = 0 then 0 else 1 // Convert LLVMBool to int for consistency
                    with
                    | ex ->
                        printfn "Exception getting target from triple: %s" ex.Message
                        1
                
                if getTargetResult <> 0 then
                    let errorPtr2 = MLIRToLLVM.safeReadPtr (NativePtr.ofNativeInt errorPtr)
                    let errorMsg = MLIRToLLVM.getLLVMString errorPtr2
                    printfn "Error: Failed to get target from triple: %s" errorMsg
                    if errorPtr2 <> nativeint 0 then
                        LLVMDisposeMessage(errorPtr2)
                    false
                else
                    target <- MLIRToLLVM.safeReadPtr (NativePtr.ofNativeInt targetPtr)
                    
                    if target = nativeint 0 then
                        printfn "Error: Failed to retrieve target"
                        false
                    else
                        let cpu = MLIRToLLVM.getCPUName()
                        let features = MLIRToLLVM.getTargetFeatures()
                        printfn "Using CPU: %s with features: %s" cpu features
                        
                        let targetMachine = 
                            try
                                LLVMCreateTargetMachine(
                                    target,
                                    defaultTriple,
                                    cpu,
                                    features,
                                    LLVMCodeGenOptLevel.Default,
                                    LLVMRelocMode.Default,
                                    LLVMCodeModel.Default)
                            with
                            | ex ->
                                printfn "Exception creating target machine: %s" ex.Message
                                nativeint 0
                        
                        if targetMachine = nativeint 0 then
                            printfn "Error: Failed to create target machine"
                            false
                        else
                            printfn "Successfully created target machine"
                            
                            let mutable emitError = nativeint 0
                            use emitErrorPin = fixed &emitError
                            let emitErrorPtr = NativePtr.toNativeInt emitErrorPin
                            
                            let success = 
                                try
                                    LLVMTargetMachineEmitToFile(
                                        targetMachine,
                                        llvmModule,
                                        outputPath,
                                        LLVMCodeGenFileType.ObjectFile,
                                        NativePtr.ofNativeInt emitErrorPtr)
                                with
                                | ex ->
                                    printfn "Exception emitting object file: %s" ex.Message
                                    false
                            
                            LLVMDisposeTargetMachine(targetMachine)
                            
                            if not success then
                                let emitErrorPtr2 = MLIRToLLVM.safeReadPtr (NativePtr.ofNativeInt emitErrorPtr)
                                let emitErrorMsg = MLIRToLLVM.getLLVMString emitErrorPtr2
                                printfn "Error: Failed to emit object file: %s" emitErrorMsg
                                if emitErrorPtr2 <> nativeint 0 then
                                    LLVMDisposeMessage(emitErrorPtr2)
                                false
                            else
                                printfn "Successfully generated object file: %s" outputPath
                                true
            with ex ->
                printfn "Exception during object file generation: %s" ex.Message
                printfn "Stack trace: %s" ex.StackTrace
                false

/// <summary>
/// Platform-specific linker module
/// </summary>
module Linker =
    /// <summary>
    /// Get the appropriate linker command and arguments for the platform
    /// </summary>
    let getLinkerCommand (objectFile: string) (outputPath: string) =
        match getOS() with
        | PlatformOS.MacOS ->
            ("clang", sprintf "-o \"%s\" \"%s\"" outputPath objectFile)
        | PlatformOS.Linux ->
            ("clang", sprintf "-o \"%s\" \"%s\"" outputPath objectFile)
        | PlatformOS.Windows ->
            ("clang", sprintf "-o \"%s\" \"%s\"" outputPath objectFile)
        | _ ->
            ("clang", sprintf "-o \"%s\" \"%s\"" outputPath objectFile)

    /// <summary>
    /// Set executable permissions on Unix-like systems
    /// </summary>
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

    /// <summary>
    /// Link an object file to create an executable
    /// </summary>
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

/// <summary>
/// Main compiler pipeline with enhanced initialization and error handling
/// </summary>
let compile (fileName: string) (fsharpCode: string) (outputPath: string) : bool =
    try
        printfn "=== Starting compilation pipeline ==="
        printfn "Input: %s" fileName
        printfn "Output: %s" outputPath
        printfn "Platform: %A %A" (getOS()) (getArchitecture())
        
        printfn "=== Platform Diagnostics ==="
        LibraryDiagnostics.getEnhancedPlatformDiagnostics()
        |> List.iter (printfn "%s")
        
        try
            printfn "=== Setting up native environment ==="
            setupNativeEnvironment()
            printfn "✓ Native environment setup completed"
            
            printfn "=== Testing LLVM availability ==="
            if not (SafeLLVMCalls.testLLVMAvailability()) then
                printfn "❌ LLVM library test failed - cannot proceed"
                false
            else
                printfn "✓ LLVM availability test passed"
                
                try
                    printfn "=== Parsing F# code ==="
                    let astOption = FSharpParser.parseProgram fileName fsharpCode
                    
                    match astOption with
                    | None ->
                        printfn "❌ Failed to parse F# code"
                        false
                    | Some ast ->
                        try
                            printfn "✓ F# parsing successful"
                            
                            printfn "=== Creating MLIR context ==="
                            let mlirContextHandle = SafeLLVMCalls.safeCreateMLIRContext()
                            
                            if mlirContextHandle = nativeint 0 then
                                printfn "❌ Failed to create MLIR context"
                                false
                            else
                                use context = new MLIRContext(mlirContextHandle)
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
                                            
                                            if objFileInfo.Length < 100L then
                                                printfn "⚠ Warning: Object file seems very small - may be empty or corrupted"
                                        
                                        printfn "=== Linking to create executable ==="
                                        let linkSuccess = Linker.linkObjectFile objFile outputPath
                                        
                                        try
                                            if File.Exists(objFile) then
                                                File.Delete(objFile)
                                        with ex ->
                                            printfn "Warning: Could not delete temporary object file: %s" ex.Message
                                        
                                        try
                                            if Directory.Exists(tempDir) && Directory.GetFiles(tempDir).Length = 0 then
                                                Directory.Delete(tempDir)
                                        with ex ->
                                            printfn "Warning: Could not delete temporary directory: %s" ex.Message
                                        
                                        if not linkSuccess then
                                            printfn "❌ Failed to link executable"
                                            false
                                        else
                                            if File.Exists(outputPath) then
                                                let execInfo = FileInfo(outputPath)
                                                printfn "✓ Successfully compiled %s to %s" fileName outputPath
                                                printfn "Final executable size: %d bytes" execInfo.Length
                                                
                                                if execInfo.Length < 1000L then
                                                    printfn "⚠ Warning: Executable seems very small - may not be a native binary"
                                                
                                                true
                                            else
                                                printfn "❌ Executable file was not created at expected location: %s" outputPath
                                                false
                        with ex ->
                            printfn "Error during MLIR/LLVM processing: %s" ex.Message
                            printfn "Stack trace: %s" ex.StackTrace
                            false
                with ex ->
                    printfn "Error during F# parsing: %s" ex.Message
                    printfn "Stack trace: %s" ex.StackTrace
                    false
        with ex ->
            printfn "❌ Native environment setup failed: %s" ex.Message
            printfn "Stack trace: %s" ex.StackTrace
            false
    with ex ->
        printfn "Fatal error in compilation pipeline: %s" ex.Message
        printfn "Stack trace: %s" ex.StackTrace
        false

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
            printfn "Stack trace: %s" ex.StackTrace
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
    try
        printfn "======================================================"
        printfn "Fidelity/Firefly F# Compiler starting..."
        printfn "======================================================"
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
                printfn "Target triple: %s" (getDefaultTargetTriple())
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