module FSharpMLIR.Bindings.MLIR

open System.Runtime.InteropServices
open FSharpMLIR.PlatformUtils

// Ensure native environment is set up before any bindings are used
do setupNativeEnvironment()

// Float types
type MLIRFloatTypeKind =
    | BF16 = 0
    | F16 = 1
    | F32 = 2
    | F64 = 3
    | F80 = 4
    | F128 = 5

// MLIR Context - The central data structure
#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern nativeint mlirContextCreate()

#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern void mlirContextDestroy(nativeint context)

#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern void mlirContextSetAllowUnregisteredDialects(nativeint context, bool allow)

// MLIR Locations - For error reporting
#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern nativeint mlirLocationUnknownGet(nativeint context)

#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern nativeint mlirLocationFileLineColGet(nativeint context, string filename, uint32 line, uint32 col)

#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern void mlirLocationDestroy(nativeint location)

// MLIR Modules - Top-level containers
#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern nativeint mlirModuleCreateEmpty(nativeint location)

#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern void mlirModuleDestroy(nativeint moduleHandle)

#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern nativeint mlirModuleGetOperation(nativeint module')

// MLIR Operations
#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern nativeint mlirOperationCreate(
    string name, 
    nativeint location, 
    uint32 numResults, 
    nativeint[] resultTypes,
    uint32 numOperands, 
    nativeint[] operands,
    uint32 numAttributes, 
    nativeint[] attributes,
    uint32 numRegions, 
    nativeint[] regions)

#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern void mlirOperationDestroy(nativeint op)

#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern nativeint mlirOperationGetResult(nativeint op, uint32 pos)

#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern void mlirOperationSetAttributeByName(nativeint op, string name, nativeint attribute)

#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern bool mlirOperationVerify(nativeint op)

#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern nativeint mlirOperationGetAttributeByName(nativeint op, string name)

// MLIR Regions and Blocks
#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern nativeint mlirRegionCreate()

#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern void mlirRegionDestroy(nativeint region)

#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern nativeint mlirRegionGetFirstBlock(nativeint region)

#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern void mlirRegionAppendOwnedBlock(nativeint region, nativeint block)

#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern nativeint mlirBlockCreate(uint32 numArguments, nativeint[] argumentTypes, nativeint[] argumentLocs)

#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern void mlirBlockDestroy(nativeint block)

#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern void mlirBlockAppendOwnedOperation(nativeint block, nativeint operation)

#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern nativeint mlirBlockGetFirstOperation(nativeint block)

#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern nativeint mlirBlockGetTerminator(nativeint block)

// MLIR Types
#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern nativeint mlirIntegerTypeGet(nativeint context, uint32 width)

#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern nativeint mlirFloatTypeGet(nativeint context, MLIRFloatTypeKind kind)

#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern nativeint mlirFunctionTypeGet(
    nativeint context, 
    uint32 numInputs, 
    nativeint[] inputs, 
    uint32 numResults, 
    nativeint[] results)

#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern nativeint mlirArrayTypeGet(nativeint elementType, uint32 size)

#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern nativeint mlirStructTypeGet(nativeint context, uint32 numElements, nativeint[] elements, uint32 numNames, string[] names)

// MLIR Attributes
#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern nativeint mlirStringAttrGet(nativeint context, uint32 length, string value)

#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern nativeint mlirIntegerAttrGet(nativeint typeArg, int64 value)

#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern nativeint mlirFloatAttrGet(nativeint typeArg, double value)

#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern nativeint mlirBoolAttrGet(nativeint context, bool value)

#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern nativeint mlirTypeAttrGet(nativeint typeArg)

#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern nativeint mlirBlockAttributeGet(nativeint block)

#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern nativeint mlirNamedAttributeGet(nativeint name, nativeint attr)

#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern string mlirStringAttributeGetValue(nativeint attr)

// MLIR Dialects
#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern nativeint mlirDialectRegistryCreate()

#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern void mlirDialectRegistryDestroy(nativeint registry)

#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern void mlirContextAppendDialectRegistry(nativeint context, nativeint registry)

// Register standard dialects
#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern void mlirRegisterAllDialects(nativeint registry)

// Register specific dialects
#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern void mlirRegisterStandardDialect(nativeint registry)

#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern void mlirRegisterLLVMDialect(nativeint registry)

// MLIR Printing
#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern void mlirOperationPrint(nativeint operation, nativeint callback, nativeint userData)

#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern void mlirOperationDump(nativeint operation)

// MLIR LLVM integration 
#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern nativeint mlirLLVMDialectDataLayoutGet(nativeint operation)

// MLIR to LLVM conversion
#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern bool mlirTranslateModuleToLLVMIR(nativeint module', nativeint llvmModule, nativeint callback, nativeint userData)

// Pass management
#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern nativeint mlirPassManagerCreate(nativeint context)

#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern void mlirPassManagerDestroy(nativeint passManager)

#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern bool mlirPassManagerRun(nativeint passManager, nativeint module')

// Standard passes
#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern void mlirPassManagerAddCanonicalizer(nativeint passManager)

#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern void mlirPassManagerAddCSE(nativeint passManager)

#if WINDOWS
[<DllImport("MLIR.dll", CallingConvention = CallingConvention.Cdecl)>]
#elif MACOS
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl)>]
#else // Linux
[<DllImport("libMLIR.so", CallingConvention = CallingConvention.Cdecl)>]
#endif
extern void mlirPassManagerAddLowerToLLVM(nativeint passManager)