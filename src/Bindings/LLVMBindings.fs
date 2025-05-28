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

// LLVM Context
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
extern nativeint LLVMFunctionType(nativeint returnType, nativeint* paramTypes, uint32 paramCount, bool isVarArg)

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
extern nativeint LLVMConstInt(nativeint intType, uint64 value, bool signExtend)

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
extern nativeint LLVMConstString(string str, uint32 length, bool dontNullTerminate)

// LLVM Target Machine
#if WINDOWS
[<DllImport("LLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libLLVM.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern nativeint LLVMGetDefaultTargetTriple()

#if WINDOWS
[<DllImport("LLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libLLVM.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern bool LLVMInitializeNativeTarget()

#if WINDOWS
[<DllImport("LLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libLLVM.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern bool LLVMInitializeNativeAsmPrinter()

#if WINDOWS
[<DllImport("LLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libLLVM.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern nativeint LLVMGetTargetFromTriple(string triple, nativeint* target, nativeint* errorMessage)

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
extern nativeint LLVMPrintModuleToFile(nativeint moduleHandle, string filename, nativeint errorMsg)

#if WINDOWS
[<DllImport("LLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libLLVM.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern bool LLVMTargetMachineEmitToFile(
    nativeint targetMachine,
    nativeint module',
    string filename,
    LLVMCodeGenFileType fileType,
    nativeint* errorMessage)

// String cleanup
#if WINDOWS
[<DllImport("LLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libLLVM.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern void LLVMDisposeMessage(nativeint message)

// Verification
#if WINDOWS
[<DllImport("LLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libLLVM.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libLLVM.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern bool LLVMVerifyModule(nativeint module', LLVMVerifierFailureAction action, nativeint* outMessage)

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
extern bool LLVMRunPassManager(nativeint pm, nativeint m)

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