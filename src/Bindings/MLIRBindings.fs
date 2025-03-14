module FSharpMLIR.Bindings.MLIR

open System.Runtime.InteropServices

// MLIR Context - The central data structure
[<DllImport("libMLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
extern nativeint mlirContextCreate()

[<DllImport("libMLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
extern void mlirContextDestroy(nativeint context)

// MLIR Locations - For error reporting
[<DllImport("libMLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
extern nativeint mlirLocationUnknownGet(nativeint context)

// MLIR Modules - Top-level containers
[<DllImport("libMLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
extern nativeint mlirModuleCreateEmpty(nativeint location)

[<DllImport("libMLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
extern void mlirModuleDestroy(nativeint moduleHandle)

// MLIR Operations
[<DllImport("libMLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
extern nativeint mlirOperationCreate(string name, nativeint location, 
                                   uint32 numResults, nativeint[] resultTypes,
                                   uint32 numOperands, nativeint[] operands,
                                   uint32 numAttributes, nativeint[] attributes,
                                   uint32 numRegions, nativeint[] regions)

// MLIR Types
[<DllImport("libMLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
extern nativeint mlirIntegerTypeGet(nativeint context, uint32 width)

[<DllImport("libMLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
extern nativeint mlirFunctionTypeGet(nativeint context, uint32 numInputs, 
                                   nativeint[] inputs, uint32 numResults, 
                                   nativeint[] results)

// MLIR Dialects
[<DllImport("libMLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
extern nativeint mlirDialectRegistryCreate()

[<DllImport("libMLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
extern void mlirContextAppendDialectRegistry(nativeint context, nativeint registry)

// Register standard dialects
[<DllImport("libMLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
extern void mlirRegisterAllDialects(nativeint registry)

// MLIR Printing
[<DllImport("libMLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
extern void mlirOperationPrint(nativeint operation, 
                             nativeint callback, nativeint userData)