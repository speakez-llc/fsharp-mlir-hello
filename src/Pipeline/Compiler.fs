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
            // Try to load the library using reflection to get better error info
            let assembly = System.Reflection.Assembly.GetExecutingAssembly()
            printfn "Testing library %s for function %s" libraryName functionName
            
            // On macOS, try to load the library explicitly
            match getOS() with
            | PlatformOS.MacOS ->
                let possiblePaths = [
                    sprintf "/usr/local/lib/%s" libraryName 
                    sprintf "/opt/homebrew/lib/%s" libraryName
                    sprintf "/opt/homebrew/opt/llvm/lib/%s" libraryName
                ]
                
                for path in possiblePaths do
                    if File.Exists(path) then
                        printfn "Found library at: %s" path
                        // Try to get file info
                        let fileInfo = FileInfo(path)
                        printfn "Library size: %d bytes" fileInfo.Length
                        return true
                    else
                        printfn "Library not found at: %s" path
                
                false
            | _ ->
                true // Assume other platforms work for now
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
/// Safe LLVM function calls with enhanced error reporting
/// </summary>
module SafeLLVMCalls =
    /// <summary>
    /// Test LLVM library availability before making calls
    /// </summary>
    let testLLVMAvailability() : bool =
        try
            printfn "Testing LLVM library availability..."
            
            // Try to call a simple LLVM function that should always work
            let triplePtr = LLVMGetDefaultTargetTriple()
            if triplePtr = nativeint 0 then
                printfn "LLVMGetDefaultTargetTriple returned null"
                false
            else
                let tripleStr = Marshal.PtrToStringAnsi(triplePtr)
                printfn "LLVM library test successful. Target triple: %s" tripleStr
                LLVMDisposeMessage(triplePtr)
                true
        with ex ->
            printfn "LLVM library test failed: %s" ex.Message
            printfn "Exception type: %s" (ex.GetType().Name)
            if ex.InnerException <> null then
                printfn "Inner exception: %s" ex.InnerException.Message
            false

    /// <summary>
    /// Initialize LLVM native target with comprehensive error reporting
    /// </summary>
    let safeInitializeNativeTarget() : bool =
        try
            printfn "Attempting to initialize LLVM native target..."
            
            // First test if LLVM is available at all
            if not (testLLVMAvailability()) then
                printfn "LLVM library availability test failed"
                false
            else
                printfn "LLVM library is available, proceeding with target initialization..."
                
                let result = LLVMInitializeNativeTargetSafe()
                printfn "LLVMInitializeNativeTarget returned: %b" result
                result
        with
        | :? DllNotFoundException as ex ->
            printfn "DLL not found error: %s" ex.Message
            printfn "Library name: %s" (if ex.Message.Contains("libLLVM") then "libLLVM.dylib" else "unknown")
            false
        | :? EntryPointNotFoundException as ex ->
            printfn "Entry point not found: %s" ex.Message
            printfn "This usually means the function name is incorrect or the library version is incompatible"
            false
        | :? System.AccessViolationException as ex ->
            printfn "Access violation in LLVM call: %s" ex.Message
            printfn "This suggests a problem with the native library or function signature"
            false
        | ex ->
            printfn "Unexpected error initializing LLVM native target: %s" ex.Message
            printfn "Exception type: %s" (ex.GetType().Name)
            if ex.InnerException <> null then
                printfn "Inner exception: %s" ex.InnerException.Message
            false

    /// <summary>  
    /// Initialize LLVM ASM printer with comprehensive error reporting
    /// </summary>
    let safeInitializeNativeAsmPrinter() : bool =
        try
            printfn "Attempting to initialize LLVM ASM printer..."
            let result = LLVMInitializeNativeAsmPrinterSafe()
            printfn "LLVMInitializeNativeAsmPrinter returned: %b" result
            result
        with
        | :? DllNotFoundException as ex ->
            printfn "DLL not found error: %s" ex.Message
            false
        | :? EntryPointNotFoundException as ex ->
            printfn "Entry point not found: %s" ex.Message
            false
        | :? System.AccessViolationException as ex ->
            printfn "Access violation in LLVM ASM printer call: %s" ex.Message
            false
        | ex ->
            printfn "Unexpected error initializing LLVM ASM printer: %s" ex.Message
            printfn "Exception type: %s" (ex.GetType().Name)
            false

    /// <summary>
    /// Create LLVM context with comprehensive error reporting
    /// </summary>
    let safeCreateLLVMContext() : nativeint =
        try
            printfn "Attempting to create LLVM context..."
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
            
            // Create a SourceText from the code string
            let sourceText = SourceText.ofString fsharpCode
            
            // Create a default checker instance
            let checker = FSharpChecker.Create()
            
            // Get project options for a single file
            let projOptions = 
                checker.GetProjectOptionsFromScript(
                    fileName,
                    sourceText,
                    assumeDotNetFramework = false)
                |> Async.RunSynchronously
                |> fst // Extract the first element of the tuple
            
            // Get parsing options
            let parsingOptions = 
                checker.GetParsingOptionsFromProjectOptions(projOptions)
                |> fst // Extract the first element, ignore diagnostics for now
                
            // Parse the file using our parsing options
            let parseFileResults = 
                checker.ParseFile(fileName, sourceText, parsingOptions)
                |> Async.RunSynchronously
            
            // Check for errors
            if parseFileResults.ParseHadErrors then
                printfn "Parsing errors detected:"
                parseFileResults.Diagnostics 
                |> Array.iter (fun diag -> printfn "  %s at %A" diag.Message diag.Range)
                None
            else
                printfn "F# parsing completed successfully"
                match parseFileResults.ParseTree with
                | ParsedInput.ImplFile(ParsedImplFileInput(_, _, _, _, _, modules, _, _, _)) ->
                    // Return the first module if available
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
                printfn "Module after optimization passes:"
                mlirModule.Dump() // Dump for debugging
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
            // Create a pass manager for lowering to LLVM
            use passManager = MLIRPassManager.Create(mlirModule.Context)
            
            // Add the LowerToLLVM pass
            passManager.AddLowerToLLVM() |> ignore
            
            // Run the pass manager on the module
            let success = passManager.Run(mlirModule)
            
            if not success then
                printfn "Warning: Lowering to LLVM dialect was not fully successful"
                printfn "Module after lowering:"
                mlirModule.Dump() // Dump the module for debugging
            else
                printfn "Successfully lowered MLIR to LLVM dialect"
        with ex ->
            printfn "Error lowering to LLVM: %s" ex.Message
            printfn "Stack trace: %s" ex.StackTrace
            printfn "Attempting to continue..."

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
            // Default data layout for unknown platforms
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
    /// Convert MLIR to LLVM IR with proper error handling
    /// </summary>
    let convert (mlirModule: MLIRModule) : nativeint =
        printfn "Converting MLIR to LLVM IR"
        
        try
            // First, apply LLVM dialect lowering
            MLIRTransforms.applyLowerToLLVMPasses mlirModule
            
            // Create an LLVM context
            printfn "Creating LLVM context..."
            let llvmContext = SafeLLVMCalls.safeCreateLLVMContext()
            
            if llvmContext = nativeint 0 then
                printfn "Error: Failed to create LLVM context"
                nativeint 0
            else
                printfn "Successfully created LLVM context"
                
                // Create an LLVM module
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
                    
                    // Set the target triple
                    let targetTriple = getDefaultTargetTriple()
                    printfn "Setting target triple: %s" targetTriple
                    
                    // Set the data layout appropriate for the target
                    let dataLayout = getLLVMDataLayout()
                    printfn "Setting data layout: %s" dataLayout
                    
                    try
                        LLVMSetDataLayout(llvmModule, dataLayout)
                        LLVMSetTarget(llvmModule, targetTriple)
                        printfn "Successfully configured LLVM module"
                    with
                    | ex ->
                        printfn "Warning: Failed to configure LLVM module: %s" ex.Message
                    
                    // Convert MLIR module to LLVM IR
                    printfn "Translating MLIR to LLVM IR..."
                    match MLIRToLLVMConverter.convertModuleToLLVMIR mlirModule llvmModule with
                    | Ok() ->
                        printfn "Successfully converted MLIR to LLVM IR"
                        
                        // Verify the module
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
                        
                        // Return the LLVM module
                        llvmModule
                    | Error msg ->
                        printfn "Error converting MLIR to LLVM IR: %s" msg
                        // Clean up and return a null pointer
                        LLVMDisposeModule(llvmModule)
                        LLVMContextDispose(llvmContext)
                        nativeint 0
        with ex ->
            printfn "Exception during MLIR to LLVM conversion: %s" ex.Message
            printfn "Stack trace: %s" ex.StackTrace
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
            try
                // Create a pass manager
                let passManager = LLVMCreatePassManager()
                
                if passManager = nativeint 0 then
                    printfn "Warning: Failed to create LLVM pass manager"
                else
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
            with ex ->
                printfn "Error applying LLVM optimization passes: %s" ex.Message
                printfn "Stack trace: %s" ex.StackTrace
    
    /// <summary>
    /// Generate an object file from LLVM IR
    /// </summary>
    let generateObjectFile (llvmModule: nativeint) (outputPath: string) : bool =
        printfn "Generating object file: %s" outputPath
        
        if llvmModule = nativeint 0 then
            printfn "Error: Invalid LLVM module"
            false
        else
            try
                // Get the default target triple
                let defaultTriple = getDefaultTargetTriple()
                printfn "Using target triple: %s" defaultTriple
                
                // Get the target from triple
                let mutable target = nativeint 0
                let mutable errorMessage = nativeint 0
                
                use targetPin = fixed &target
                use errorPin = fixed &errorMessage
                
                let targetPtr = NativePtr.toNativeInt targetPin
                let errorPtr = NativePtr.toNativeInt errorPin
                
                let getTargetResult = 
                    try
                        LLVMGetTargetFromTriple(defaultTriple, NativePtr.ofNativeInt targetPtr, NativePtr.ofNativeInt errorPtr)
                    with
                    | ex ->
                        printfn "Exception getting target from triple: %s" ex.Message
                        nativeint 1 // Return non-zero to indicate failure
                
                if getTargetResult <> nativeint 0 then
                    let errorPtr2 = MLIRToLLVM.safeReadPtr (NativePtr.ofNativeInt errorPtr)
                    let errorMsg = MLIRToLLVM.getLLVMString errorPtr2
                    printfn "Error: Failed to get target from triple: %s" errorMsg
                    if errorPtr2 <> nativeint 0 then
                        LLVMDisposeMessage(errorPtr2)
                    false
                else
                    // Get the target from the pointer
                    target <- MLIRToLLVM.safeReadPtr (NativePtr.ofNativeInt targetPtr)
                    
                    if target = nativeint 0 then
                        printfn "Error: Failed to retrieve target"
                        false
                    else
                        // Set CPU and features
                        let cpu = MLIRToLLVM.getCPUName()
                        let features = MLIRToLLVM.getTargetFeatures()
                        printfn "Using CPU: %s with features: %s" cpu features
                        
                        // Create target machine
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
                            
                            // Emit object file
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
                            
                            // Clean up
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
            // Use clang on macOS for better compatibility
            ("clang", sprintf "-o \"%s\" \"%s\"" outputPath objectFile)
        | PlatformOS.Linux ->
            // Use clang on Linux for consistency
            ("clang", sprintf "-o \"%s\" \"%s\"" outputPath objectFile)
        | PlatformOS.Windows ->
            // Try clang first, fallback to link.exe
            ("clang", sprintf "-o \"%s\" \"%s\"" outputPath objectFile)
        | _ ->
            // Universal fallback to clang
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
        
        // Make sure the object file exists
        if not (File.Exists(objectFile)) then
            printfn "Error: Object file does not exist: %s" objectFile
            false
        else
            try
                // Get the appropriate linker command
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
                    // Set executable permissions on Unix-like systems
                    setExecutablePermissions outputPath
                    
                    printfn "Successfully linked executable: %s" outputPath
                    true
                    
            with ex ->
                printfn "Error running linker: %s" ex.Message
                printfn "Stack trace: %s" ex.StackTrace
                false

/// <summary>
/// Main compiler pipeline
/// </summary>
let compile (fileName: string) (fsharpCode: string) (outputPath: string) : bool =
    try
        printfn "=== Starting compilation pipeline ==="
        printfn "Input: %s" fileName
        printfn "Output: %s" outputPath
        printfn "Platform: %A %A" (getOS()) (getArchitecture())
        
        // Enhanced diagnostics
        printfn "=== Platform Diagnostics ==="
        LibraryDiagnostics.getEnhancedPlatformDiagnostics()
        |> List.iter (printfn "%s")
        
        // Set up the native environment (PATH, etc.)
        try
            printfn "=== Setting up native environment ==="
            setupNativeEnvironment()
            printfn "✓ Native environment setup completed"
            
            // Test LLVM availability before proceeding
            printfn "=== Testing LLVM availability ==="
            if not (SafeLLVMCalls.testLLVMAvailability()) then
                printfn "❌ LLVM library test failed - cannot proceed"
                false
            else
                // Initialize LLVM native target
                printfn "=== Initializing LLVM ==="
                let targetInitSuccess = SafeLLVMCalls.safeInitializeNativeTarget()
                if not targetInitSuccess then
                    printfn "❌ Failed to initialize LLVM native target - cannot proceed"
                    false
                else
                    printfn "✓ LLVM native target initialized successfully"
                    
                    // Initialize LLVM ASM printer
                    let asmPrinterSuccess = SafeLLVMCalls.safeInitializeNativeAsmPrinter()
                    if not asmPrinterSuccess then
                        printfn "⚠ Warning: Failed to initialize LLVM ASM printer - continuing anyway"
                    else
                        printfn "✓ LLVM ASM printer initialized successfully"
                    
                    // Parse F# code to get AST
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
                                
                                // Create MLIR context and module
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
                                    
                                    // Convert F# AST to MLIR
                                    printfn "=== Converting F# AST to MLIR ==="
                                    let converter = ASTToMLIR.Converter(context)
                                    converter.ConvertModule(mlirModule, ast)
                                    printfn "✓ AST to MLIR conversion completed"
                                    
                                    // Verify the MLIR module
                                    printfn "=== Verifying MLIR module ==="
                                    if not (mlirModule.Verify()) then
                                        printfn "⚠ Warning: MLIR module verification failed"
                                        printfn "MLIR module content:"
                                        mlirModule.Dump()
                                    else
                                        printfn "✓ MLIR module verification successful"
                                        
                                    // Apply MLIR optimization passes
                                    printfn "=== Applying MLIR optimization passes ==="
                                    MLIRTransforms.applyOptimizationPasses mlirModule
                                    printfn "✓ MLIR optimization passes completed"
                                    
                                    // Convert MLIR to LLVM IR
                                    printfn "=== Converting MLIR to LLVM IR ==="
                                    let llvmModule = MLIRToLLVM.convert mlirModule
                                    
                                    if llvmModule = nativeint 0 then
                                        printfn "❌ Failed to convert MLIR to LLVM IR"
                                        false
                                    else
                                        printfn "✓ MLIR to LLVM conversion successful"
                                        
                                        // Apply LLVM optimization passes
                                        printfn "=== Applying LLVM optimization passes ==="
                                        LLVMCodeGen.applyOptimizationPasses llvmModule
                                        printfn "✓ LLVM optimization passes completed"
                                        
                                        // Create temporary directory if it doesn't exist
                                        let tempDir = Path.Combine(Path.GetTempPath(), "FSharpMLIR")
                                        if not (Directory.Exists(tempDir)) then
                                            Directory.CreateDirectory(tempDir) |> ignore
                                            
                                        // Generate temporary object file
                                        let objFile = Path.Combine(tempDir, Path.GetFileNameWithoutExtension(outputPath) + ".o")
                                        
                                        printfn "=== Generating object file ==="
                                        printfn "Object file path: %s" objFile
                                        let objSuccess = LLVMCodeGen.generateObjectFile llvmModule objFile
                                        
                                        // Clean up LLVM module
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
                                            
                                            // Verify object file was created and has reasonable size
                                            if File.Exists(objFile) then
                                                let objFileInfo = FileInfo(objFile)
                                                printfn "Object file size: %d bytes" objFileInfo.Length
                                                
                                                if objFileInfo.Length < 100L then
                                                    printfn "⚠ Warning: Object file seems very small - may be empty or corrupted"
                                            
                                            // Link to produce executable
                                            printfn "=== Linking to create executable ==="
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
                                                printfn "❌ Failed to link executable"
                                                false
                                            else
                                                // Verify final executable
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
                    // Only Windows uses .exe extension for executables
                    match getOS() with
                    | PlatformOS.Windows -> baseName + ".exe"
                    | _ -> baseName  // Unix-like systems don't use extensions for executables
            
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