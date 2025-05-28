module FSharpMLIR.Bindings.LLVM

open System
open System.Runtime.InteropServices
open FSharpMLIR.PlatformUtils

// Note: setupNativeEnvironment() is now called explicitly by the compiler pipeline
// This prevents crashes during module initialization

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

// LLVM Opaque type handles
type LLVMContextRef = nativeint
type LLVMModuleRef = nativeint
type LLVMTypeRef = nativeint
type LLVMValueRef = nativeint
type LLVMBasicBlockRef = nativeint
type LLVMBuilderRef = nativeint
type LLVMPassManagerRef = nativeint
type LLVMTargetRef = nativeint
type LLVMTargetMachineRef = nativeint

// LLVM Context Functions
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMContextRef LLVMContextCreate()

[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMContextDispose(LLVMContextRef context)

// LLVM Module Functions
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMModuleRef LLVMModuleCreateWithNameInContext([<MarshalAs(UnmanagedType.LPStr)>] string moduleName, LLVMContextRef context)

[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMDisposeModule(LLVMModuleRef moduleHandle)

[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMSetTarget(LLVMModuleRef module', [<MarshalAs(UnmanagedType.LPStr)>] string triple)

[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMSetDataLayout(LLVMModuleRef module', [<MarshalAs(UnmanagedType.LPStr)>] string dataLayout)

// LLVM Type Functions
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMInt32TypeInContext(LLVMContextRef context)

[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMPointerType(LLVMTypeRef elementType, uint32 addressSpace)

// LLVM Function Functions
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMAddFunction(LLVMModuleRef moduleHandle, [<MarshalAs(UnmanagedType.LPStr)>] string name, LLVMTypeRef functionType)

[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMFunctionType(LLVMTypeRef returnType, nativeint paramTypes, uint32 paramCount, LLVMBool isVarArg)

// LLVM Basic Block Functions
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBasicBlockRef LLVMAppendBasicBlockInContext(LLVMContextRef context, LLVMValueRef func, [<MarshalAs(UnmanagedType.LPStr)>] string name)

// LLVM Builder Functions
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBuilderRef LLVMCreateBuilderInContext(LLVMContextRef context)

[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMPositionBuilderAtEnd(LLVMBuilderRef builder, LLVMBasicBlockRef basicBlock)

[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMDisposeBuilder(LLVMBuilderRef builder)

// LLVM Instruction Building Functions
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildRet(LLVMBuilderRef builder, LLVMValueRef value)

[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildRetVoid(LLVMBuilderRef builder)

[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildCall2(LLVMBuilderRef builder, LLVMTypeRef functionType, LLVMValueRef func, nativeint args, uint32 numArgs, [<MarshalAs(UnmanagedType.LPStr)>] string name)

// LLVM Constant Functions
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMConstInt(LLVMTypeRef intType, uint64 value, LLVMBool signExtend)

[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMConstReal(LLVMTypeRef realType, double value)

[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMConstString([<MarshalAs(UnmanagedType.LPStr)>] string str, uint32 length, LLVMBool dontNullTerminate)

// LLVM Target Functions
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern nativeint LLVMGetDefaultTargetTriple()

[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBool LLVMInitializeNativeTarget()

[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBool LLVMInitializeNativeAsmPrinter()

[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBool LLVMGetTargetFromTriple([<MarshalAs(UnmanagedType.LPStr)>] string triple, nativeint target, nativeint errorMessage)

[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTargetMachineRef LLVMCreateTargetMachine(
    LLVMTargetRef target,
    [<MarshalAs(UnmanagedType.LPStr)>] string triple,
    [<MarshalAs(UnmanagedType.LPStr)>] string cpu,
    [<MarshalAs(UnmanagedType.LPStr)>] string features,
    LLVMCodeGenOptLevel level,
    LLVMRelocMode reloc,
    LLVMCodeModel codeModel)

[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMDisposeTargetMachine(LLVMTargetMachineRef targetMachine)

// LLVM Module Output Functions
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBool LLVMPrintModuleToFile(LLVMModuleRef moduleHandle, [<MarshalAs(UnmanagedType.LPStr)>] string filename, nativeint errorMsg)

[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBool LLVMTargetMachineEmitToFile(
    LLVMTargetMachineRef targetMachine,
    LLVMModuleRef module',
    [<MarshalAs(UnmanagedType.LPStr)>] string filename,
    LLVMCodeGenFileType fileType,
    nativeint errorMessage)

// LLVM Memory Management
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMDisposeMessage(nativeint message)

// LLVM Module Verification
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBool LLVMVerifyModule(LLVMModuleRef module', LLVMVerifierFailureAction action, nativeint outMessage)

// LLVM Pass Manager Functions
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMPassManagerRef LLVMCreatePassManager()

[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMDisposePassManager(LLVMPassManagerRef pm)

[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBool LLVMRunPassManager(LLVMPassManagerRef pm, LLVMModuleRef m)

// LLVM Optimization Pass Functions
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMAddInstructionCombiningPass(LLVMPassManagerRef pm)

[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMAddPromoteMemoryToRegisterPass(LLVMPassManagerRef pm)

[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMAddGVNPass(LLVMPassManagerRef pm)

[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMAddCFGSimplificationPass(LLVMPassManagerRef pm)

// LLVM Initialization Functions
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMInitializeAllTargetInfos()

[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMInitializeAllTargets()

[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMInitializeAllTargetMCs()

[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMInitializeAllAsmPrinters()

// Get string from target triple
let LLVMGetDefaultTargetTripleString() : string =
    let ptr = LLVMGetDefaultTargetTriple()
    if ptr = nativeint 0 then
        "unknown-unknown-unknown"
    else
        let result = Marshal.PtrToStringAnsi(ptr)
        LLVMDisposeMessage(ptr)
        if String.IsNullOrEmpty(result) then "unknown-unknown-unknown" else result

// Enhanced initialization function
let LLVMInitializeAllSafe() : bool =
    try
        printfn "Initializing LLVM targets..."
        LLVMInitializeAllTargetInfos()
        LLVMInitializeAllTargets()
        LLVMInitializeAllTargetMCs()
        LLVMInitializeAllAsmPrinters()
        
        let nativeTargetResult = LLVMInitializeNativeTarget()
        let asmPrinterResult = LLVMInitializeNativeAsmPrinter()
        
        printfn "LLVM initialization results: NativeTarget=%d, AsmPrinter=%d" nativeTargetResult asmPrinterResult
        nativeTargetResult = 0 && asmPrinterResult = 0
    with
    | ex ->
        printfn "Error during LLVM initialization: %s" ex.Message
        printfn "Exception type: %s" (ex.GetType().Name)
        if ex.InnerException <> null then
            printfn "Inner exception: %s" ex.InnerException.Message
        false