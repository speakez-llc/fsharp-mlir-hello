module FSharpMLIR.Bindings.MLIR

open System
open System.Runtime.InteropServices
open FSharpMLIR.PlatformUtils

// Note: setupNativeEnvironment() is now called explicitly by the compiler pipeline
// This prevents crashes during module initialization

// MLIR Opaque type handles
type MLIRContext = nativeint
type MLIRDialectRegistry = nativeint
type MLIRLocation = nativeint
type MLIRModule = nativeint
type MLIROperation = nativeint
type MLIRRegion = nativeint
type MLIRBlock = nativeint
type MLIRType = nativeint
type MLIRAttribute = nativeint
type MLIRValue = nativeint
type MLIRPassManager = nativeint
type MLIRNamedAttribute = nativeint

// Float types
type MLIRFloatTypeKind =
    | BF16 = 0
    | F16 = 1
    | F32 = 2
    | F64 = 3
    | F80 = 4
    | F128 = 5

// MLIR Context Functions
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern MLIRContext mlirContextCreate()

[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void mlirContextDestroy(MLIRContext context)

[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void mlirContextSetAllowUnregisteredDialects(MLIRContext context, bool allow)

// MLIR Location Functions
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern MLIRLocation mlirLocationUnknownGet(MLIRContext context)

[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern MLIRLocation mlirLocationFileLineColGet(MLIRContext context, [<MarshalAs(UnmanagedType.LPStr)>] string filename, uint32 line, uint32 col)

[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void mlirLocationDestroy(MLIRLocation location)

// MLIR Module Functions
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern MLIRModule mlirModuleCreateEmpty(MLIRLocation location)

[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void mlirModuleDestroy(MLIRModule moduleHandle)

[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern MLIROperation mlirModuleGetOperation(MLIRModule module')

// MLIR Operation Functions
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern MLIROperation mlirOperationCreate(
    [<MarshalAs(UnmanagedType.LPStr)>] string name, 
    MLIRLocation location, 
    uint32 numResults, 
    MLIRType[] resultTypes,
    uint32 numOperands, 
    MLIRValue[] operands,
    uint32 numAttributes, 
    MLIRNamedAttribute[] attributes,
    uint32 numRegions, 
    MLIRRegion[] regions)

[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void mlirOperationDestroy(MLIROperation op)

[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern MLIRValue mlirOperationGetResult(MLIROperation op, uint32 pos)

[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void mlirOperationSetAttributeByName(MLIROperation op, [<MarshalAs(UnmanagedType.LPStr)>] string name, MLIRAttribute attribute)

[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern bool mlirOperationVerify(MLIROperation op)

[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern MLIRAttribute mlirOperationGetAttributeByName(MLIROperation op, [<MarshalAs(UnmanagedType.LPStr)>] string name)

// MLIR Region Functions
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern MLIRRegion mlirRegionCreate()

[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void mlirRegionDestroy(MLIRRegion region)

[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern MLIRBlock mlirRegionGetFirstBlock(MLIRRegion region)

[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void mlirRegionAppendOwnedBlock(MLIRRegion region, MLIRBlock block)

// MLIR Block Functions
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern MLIRBlock mlirBlockCreate(uint32 numArguments, MLIRType[] argumentTypes, MLIRLocation[] argumentLocs)

[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void mlirBlockDestroy(MLIRBlock block)

[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void mlirBlockAppendOwnedOperation(MLIRBlock block, MLIROperation operation)

[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern MLIROperation mlirBlockGetFirstOperation(MLIRBlock block)

[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern MLIROperation mlirBlockGetTerminator(MLIRBlock block)

// MLIR Type Functions
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern MLIRType mlirIntegerTypeGet(MLIRContext context, uint32 width)

[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern MLIRType mlirFloatTypeGet(MLIRContext context, MLIRFloatTypeKind kind)

[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern MLIRType mlirFunctionTypeGet(
    MLIRContext context, 
    uint32 numInputs, 
    MLIRType[] inputs, 
    uint32 numResults, 
    MLIRType[] results)

[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern MLIRType mlirArrayTypeGet(MLIRType elementType, uint32 size)

[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern MLIRType mlirStructTypeGet(MLIRContext context, uint32 numElements, MLIRType[] elements, uint32 numNames, [<MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)>] string[] names)

// MLIR Attribute Functions
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern MLIRAttribute mlirStringAttrGet(MLIRContext context, uint32 length, [<MarshalAs(UnmanagedType.LPStr)>] string value)

[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern MLIRAttribute mlirIntegerAttrGet(MLIRType typeArg, int64 value)

[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern MLIRAttribute mlirFloatAttrGet(MLIRType typeArg, double value)

[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern MLIRAttribute mlirBoolAttrGet(MLIRContext context, bool value)

[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern MLIRAttribute mlirTypeAttrGet(MLIRType typeArg)

[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern MLIRAttribute mlirBlockAttributeGet(MLIRBlock block)

[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern MLIRNamedAttribute mlirNamedAttributeGet(MLIRAttribute name, MLIRAttribute attr)

[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern string mlirStringAttributeGetValue(MLIRAttribute attr)

// MLIR Dialect Functions
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern MLIRDialectRegistry mlirDialectRegistryCreate()

[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void mlirDialectRegistryDestroy(MLIRDialectRegistry registry)

[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void mlirContextAppendDialectRegistry(MLIRContext context, MLIRDialectRegistry registry)

[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void mlirRegisterAllDialects(MLIRDialectRegistry registry)

// MLIR Pass Manager Functions
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern MLIRPassManager mlirPassManagerCreate(MLIRContext context)

[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void mlirPassManagerDestroy(MLIRPassManager passManager)

[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern bool mlirPassManagerRun(MLIRPassManager passManager, MLIRModule module')

// MLIR Printing Functions
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void mlirOperationDump(MLIROperation operation)

// MLIR to LLVM Conversion
[<DllImport("libMLIR.dylib", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern bool mlirTranslateModuleToLLVMIR(MLIRModule module', nativeint llvmModule, nativeint callback, nativeint userData)