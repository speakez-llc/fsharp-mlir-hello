module FSharpMLIR.Bindings.LLVM

open System.Runtime.InteropServices

// LLVM Context
[<DllImport("libLLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
extern nativeint LLVMContextCreate()

[<DllImport("libLLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
extern void LLVMContextDispose(nativeint context)

// LLVM Modules
[<DllImport("libLLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
extern nativeint LLVMModuleCreateWithNameInContext(string moduleName, nativeint context)

[<DllImport("libLLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
extern void LLVMDisposeModule(nativeint moduleHandle)

// LLVM Types
[<DllImport("libLLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
extern nativeint LLVMInt32TypeInContext(nativeint context)

[<DllImport("libLLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
extern nativeint LLVMPointerType(nativeint elementType, uint32 addressSpace)

// LLVM Functions
[<DllImport("libLLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
extern nativeint LLVMAddFunction(nativeint moduleHandle, string name, nativeint functionType)

// LLVM Target Machine
[<DllImport("libLLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
extern nativeint LLVMGetDefaultTargetTriple()

// LLVM Write to file
[<DllImport("libLLVM.dll", CallingConvention = CallingConvention.Cdecl)>]
extern nativeint LLVMPrintModuleToFile(nativeint moduleHandle, string filename, nativeint errorMsg)