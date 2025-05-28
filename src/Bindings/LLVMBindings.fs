module FSharpMLIR.Bindings.LLVM

open System.Runtime.InteropServices
open FSharpMLIR.PlatformUtils

// Ensure native environment is set up before any bindings are used
do setupNativeEnvironment()

// Code generation optimization levels
type LLVMCodeGenOptLevel =
    | None = 0
    | Less = 1
    | Default = 2
    | Aggressive = 3

// Relocation models
type LLVMRelocMode =
    | Default = 0
    | Static = 1
    | PIC = 2
    | DynamicNoPic = 3
    | ROPI = 4
    | RWPI = 5
    | ROPI_RWPI = 6

// Code models
type LLVMCodeModel =
    | Default = 0
    | JITDefault = 1
    | Tiny = 2
    | Small = 3
    | Kernel = 4
    | Medium = 5
    | Large = 6

// File types for code generation
type LLVMCodeGenFileType =
    | AssemblyFile = 0
    | ObjectFile = 1

// Verification action
type LLVMVerifierFailureAction =
    | AbortProcess = 0
    | PrintMessage = 1
    | ReturnStatus = 2

// Boolean type for LLVM (uses int32 internally)
type LLVMBool = int32

// LLVM Context - Fixed signatures
#if WINDOWS
[<DllImport("LLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libLLVM.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern nativeint LLVMContextCreate()

#if WINDOWS
[<DllImport("LLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libLLVM.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern void LLVMContextDispose(nativeint context)

// LLVM Modules
#if WINDOWS
[<DllImport("LLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libLLVM.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern nativeint LLVMModuleCreateWithNameInContext(string moduleName, nativeint context)

#if WINDOWS
[<DllImport("LLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libLLVM.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern void LLVMDisposeModule(nativeint moduleHandle)

#if WINDOWS
[<DllImport("LLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libLLVM.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern void LLVMSetTarget(nativeint module', string triple)

#if WINDOWS
[<DllImport("LLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libLLVM.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern void LLVMSetDataLayout(nativeint module', string dataLayout)

// LLVM Types
#if WINDOWS
[<DllImport("LLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libLLVM.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern nativeint LLVMInt32TypeInContext(nativeint context)

#if WINDOWS
[<DllImport("LLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libLLVM.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern nativeint LLVMPointerType(nativeint elementType, uint32 addressSpace)

// LLVM Functions
#if WINDOWS
[<DllImport("LLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libLLVM.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern nativeint LLVMAddFunction(nativeint moduleHandle, string name, nativeint functionType)

// LLVM Function Types
#if WINDOWS
[<DllImport("LLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libLLVM.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern nativeint LLVMFunctionType(nativeint returnType, nativeint* paramTypes, uint32 paramCount, LLVMBool isVarArg)

// LLVM Basic Blocks
#if WINDOWS
[<DllImport("LLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libLLVM.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern nativeint LLVMAppendBasicBlockInContext(nativeint context, nativeint func, string name)

// LLVM Builder
#if WINDOWS
[<DllImport("LLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libLLVM.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern nativeint LLVMCreateBuilderInContext(nativeint context)

#if WINDOWS
[<DllImport("LLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libLLVM.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern void LLVMPositionBuilderAtEnd(nativeint builder, nativeint basicBlock)

#if WINDOWS
[<DllImport("LLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libLLVM.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern void LLVMDisposeBuilder(nativeint builder)

// LLVM Instruction Building
#if WINDOWS
[<DllImport("LLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libLLVM.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern nativeint LLVMBuildRet(nativeint builder, nativeint value)

#if WINDOWS
[<DllImport("LLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libLLVM.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern nativeint LLVMBuildRetVoid(nativeint builder)

#if WINDOWS
[<DllImport("LLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libLLVM.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern nativeint LLVMBuildCall2(nativeint builder, nativeint functionType, nativeint func, nativeint* args, uint32 numArgs, string name)

// LLVM Constants
#if WINDOWS
[<DllImport("LLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libLLVM.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern nativeint LLVMConstInt(nativeint intType, uint64 value, LLVMBool signExtend)

#if WINDOWS
[<DllImport("LLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libLLVM.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern nativeint LLVMConstReal(nativeint realType, double value)

#if WINDOWS
[<DllImport("LLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libLLVM.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern nativeint LLVMConstString(string str, uint32 length, LLVMBool dontNullTerminate)

// LLVM Target Machine - CORRECTED FUNCTION SIGNATURES
#if WINDOWS
[<DllImport("LLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libLLVM.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern nativeint LLVMGetDefaultTargetTriple()

// CORRECTED: These functions return LLVMBool (int32), not int
#if WINDOWS
[<DllImport("LLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libLLVM.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern LLVMBool LLVMInitializeNativeTarget()

#if WINDOWS
[<DllImport("LLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libLLVM.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern LLVMBool LLVMInitializeNativeAsmPrinter()

// Safe wrapper for LLVMInitializeNativeTarget
let LLVMInitializeNativeTargetSafe() : bool =
    try
        let result = LLVMInitializeNativeTarget()
        result = 0 // LLVM returns 0 for success
    with
    | ex ->
        printfn "Error calling LLVMInitializeNativeTarget: %s" ex.Message
        printfn "Exception type: %s" (ex.GetType().Name)
        if ex.InnerException <> null then
            printfn "Inner exception: %s" ex.InnerException.Message
        false

// Safe wrapper for LLVMInitializeNativeAsmPrinter
let LLVMInitializeNativeAsmPrinterSafe() : bool =
    try
        let result = LLVMInitializeNativeAsmPrinter()
        result = 0 // LLVM returns 0 for success
    with
    | ex ->
        printfn "Error calling LLVMInitializeNativeAsmPrinter: %s" ex.Message
        printfn "Exception type: %s" (ex.GetType().Name)
        if ex.InnerException <> null then
            printfn "Inner exception: %s" ex.InnerException.Message
        false

#if WINDOWS
[<DllImport("LLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libLLVM.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern LLVMBool LLVMGetTargetFromTriple(string triple, nativeint* target, nativeint* errorMessage)

#if WINDOWS
[<DllImport("LLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libLLVM.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern nativeint LLVMCreateTargetMachine(
    nativeint target,
    string triple,
    string cpu,
    string features,
    LLVMCodeGenOptLevel level,
    LLVMRelocMode reloc,
    LLVMCodeModel codeModel)

#if WINDOWS
[<DllImport("LLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libLLVM.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern void LLVMDisposeTargetMachine(nativeint targetMachine)

// LLVM Write to file
#if WINDOWS
[<DllImport("LLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libLLVM.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern LLVMBool LLVMPrintModuleToFile(nativeint moduleHandle, string filename, nativeint* errorMsg)

#if WINDOWS
[<DllImport("LLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libLLVM.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern LLVMBool LLVMTargetMachineEmitToFileRaw(
    nativeint targetMachine,
    nativeint module',
    string filename,
    LLVMCodeGenFileType fileType,
    nativeint* errorMessage)

// Safe wrapper for LLVMTargetMachineEmitToFile
let LLVMTargetMachineEmitToFile(targetMachine: nativeint, module': nativeint, filename: string, fileType: LLVMCodeGenFileType, errorMessage: nativeint nativeptr) : bool =
    try
        let result = LLVMTargetMachineEmitToFileRaw(targetMachine, module', filename, fileType, errorMessage)
        result = 0 // LLVM returns 0 for success
    with
    | ex ->
        printfn "Error calling LLVMTargetMachineEmitToFile: %s" ex.Message
        printfn "Exception type: %s" (ex.GetType().Name)
        if ex.InnerException <> null then
            printfn "Inner exception: %s" ex.InnerException.Message
        false

// String cleanup
#if WINDOWS
[<DllImport("LLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libLLVM.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern void LLVMDisposeMessage(nativeint message)

// Verification - CORRECTED return type
#if WINDOWS
[<DllImport("LLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libLLVM.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern LLVMBool LLVMVerifyModuleRaw(nativeint module', LLVMVerifierFailureAction action, nativeint* outMessage)

// Safe wrapper for LLVMVerifyModule
let LLVMVerifyModule(module': nativeint, action: LLVMVerifierFailureAction, outMessage: nativeint nativeptr) : bool =
    try
        let result = LLVMVerifyModuleRaw(module', action, outMessage)
        result <> 0 // For verification, non-zero means error
    with
    | ex ->
        printfn "Error calling LLVMVerifyModule: %s" ex.Message
        printfn "Exception type: %s" (ex.GetType().Name)
        if ex.InnerException <> null then
            printfn "Inner exception: %s" ex.InnerException.Message
        true // Assume error if we can't verify

// LLVM Pass Manager
#if WINDOWS
[<DllImport("LLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libLLVM.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern nativeint LLVMCreatePassManager()

#if WINDOWS
[<DllImport("LLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libLLVM.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern void LLVMDisposePassManager(nativeint pm)

#if WINDOWS
[<DllImport("LLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libLLVM.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern LLVMBool LLVMRunPassManagerRaw(nativeint pm, nativeint m)

// Safe wrapper for LLVMRunPassManager
let LLVMRunPassManager(pm: nativeint, m: nativeint) : bool =
    try
        let result = LLVMRunPassManagerRaw(pm, m)
        result <> 0 // Non-zero indicates changes were made (success)
    with
    | ex ->
        printfn "Error calling LLVMRunPassManager: %s" ex.Message
        printfn "Exception type: %s" (ex.GetType().Name)
        if ex.InnerException <> null then
            printfn "Inner exception: %s" ex.InnerException.Message
        false

// Common optimization passes
#if WINDOWS
[<DllImport("LLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libLLVM.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern void LLVMAddInstructionCombiningPass(nativeint pm)

#if WINDOWS
[<DllImport("LLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libLLVM.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern void LLVMAddPromoteMemoryToRegisterPass(nativeint pm)

#if WINDOWS
[<DllImport("LLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libLLVM.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern void LLVMAddGVNPass(nativeint pm)

#if WINDOWS
[<DllImport("LLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libLLVM.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern void LLVMAddCFGSimplificationPass(nativeint pm)

// Get string from target triple with enhanced safety
let LLVMGetDefaultTargetTripleString() : string =
    try
        let ptr = LLVMGetDefaultTargetTriple()
        if ptr = nativeint 0 then
            "unknown-unknown-unknown"
        else
            let result = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(ptr)
            LLVMDisposeMessage(ptr) // Clean up the string
            if System.String.IsNullOrEmpty(result) then "unknown-unknown-unknown" else result
    with
    | ex ->
        printfn "Error getting default target triple: %s" ex.Message
        printfn "Exception type: %s" (ex.GetType().Name)
        if ex.InnerException <> null then
            printfn "Inner exception: %s" ex.InnerException.Message
        "unknown-unknown-unknown"

// Additional initialization functions that may be needed
#if WINDOWS
[<DllImport("LLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libLLVM.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern void LLVMInitializeAllTargetInfos()

#if WINDOWS
[<DllImport("LLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libLLVM.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern void LLVMInitializeAllTargets()

#if WINDOWS
[<DllImport("LLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libLLVM.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern void LLVMInitializeAllTargetMCs()

#if WINDOWS
[<DllImport("LLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libLLVM.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern void LLVMInitializeAllAsmPrinters()

// Enhanced initialization function
let LLVMInitializeAllSafe() : bool =
    try
        printfn "Initializing all LLVM targets and components..."
        LLVMInitializeAllTargetInfos()
        LLVMInitializeAllTargets()
        LLVMInitializeAllTargetMCs()
        LLVMInitializeAllAsmPrinters()
        
        let nativeTargetResult = LLVMInitializeNativeTargetSafe()
        let asmPrinterResult = LLVMInitializeNativeAsmPrinterSafe()
        
        printfn "LLVM initialization complete: NativeTarget=%b, AsmPrinter=%b" nativeTargetResult asmPrinterResult
        nativeTargetResult && asmPrinterResult
    with
    | ex ->
        printfn "Error during LLVM initialization: %s" ex.Message
        printfn "Exception type: %s" (ex.GetType().Name)
        if ex.InnerException <> null then
            printfn "Inner exception: %s" ex.InnerException.Message
        false