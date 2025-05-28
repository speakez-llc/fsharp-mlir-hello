module FSharpMLIR.Bindings.MLIR

open System
open System.Runtime.InteropServices
open FSharpMLIR.PlatformUtils

// =============================================================================
// MLIR Type Definitions
// =============================================================================

// MLIR Opaque type handles
type MLIRContext = nativeint
type MLIRDialect = nativeint
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
type MLIROpPassManager = nativeint
type MLIRPass = nativeint
type MLIRExternalPass = nativeint
type MLIRNamedAttribute = nativeint
type MLIRTypeID = nativeint
type MLIRAffineMap = nativeint
type MLIRIntegerSet = nativeint
type MLIRDiagnostic = nativeint
type MLIRDialectHandle = nativeint
type MLIROpPrintingFlags = nativeint

// String reference structure for MLIR
[<StructLayout(LayoutKind.Sequential)>]
type MLIRStringRef =
    struct
        val mutable data: nativeint
        val mutable length: nativeint
        
        new(data: nativeint, length: nativeint) = { data = data; length = length }
        
        static member Create(str: string) =
            let bytes = System.Text.Encoding.UTF8.GetBytes(str)
            let ptr = Marshal.AllocHGlobal(bytes.Length)
            Marshal.Copy(bytes, 0, ptr, bytes.Length)
            MLIRStringRef(ptr, nativeint bytes.Length)
            
        member this.ToString() =
            if this.data <> nativeint 0 && this.length > nativeint 0 then
                let bytes = Array.zeroCreate<byte> (int this.length)
                Marshal.Copy(this.data, bytes, 0, int this.length)
                System.Text.Encoding.UTF8.GetString(bytes)
            else
                ""
                
        member this.Dispose() =
            if this.data <> nativeint 0 then
                Marshal.FreeHGlobal(this.data)
    end

[<StructLayout(LayoutKind.Sequential)>]
type MLIRLogicalResult =
    struct
        val mutable value: int8
        member this.IsSuccess = this.value = 1y
        member this.IsFailure = this.value = 0y
        static member Success = MLIRLogicalResult(value = 1y)
        static member Failure = MLIRLogicalResult(value = 0y)
        
        new(value: int8) = { value = value }
    end

// Diagnostic severity enum
type MLIRDiagnosticSeverity =
    | Error = 0
    | Warning = 1
    | Note = 2
    | Remark = 3

// Float types enumeration
type MLIRFloatTypeKind =
    | BF16 = 0
    | F16 = 1
    | F32 = 2
    | F64 = 3
    | F80 = 4
    | F128 = 5

// LLVM Calling Convention
type MLIRLLVMCConv =
    | C = 0
    | Fast = 8
    | Cold = 9
    | GHC = 10
    | HiPE = 11
    | AnyReg = 13
    | PreserveMost = 14
    | PreserveAll = 15
    | Swift = 16
    | CXX_FAST_TLS = 17
    | Tail = 18
    | CFGuard_Check = 19
    | SwiftTail = 20
    | X86_StdCall = 64
    | X86_FastCall = 65

// LLVM Linkage types
type MLIRLLVMLinkage =
    | External = 0
    | AvailableExternally = 1
    | Linkonce = 2
    | LinkonceODR = 3
    | Weak = 4
    | WeakODR = 5
    | Appending = 6
    | Internal = 7
    | Private = 8
    | ExternWeak = 9
    | Common = 10

// LLVM Type Encoding
type MLIRLLVMTypeEncoding =
    | Address = 0x1
    | Boolean = 0x2
    | ComplexFloat = 0x31
    | FloatT = 0x4
    | Signed = 0x5
    | SignedChar = 0x6
    | Unsigned = 0x7
    | UnsignedChar = 0x08

// Callback delegates
type MLIRStringCallback = delegate of MLIRStringRef * nativeint -> unit
type MLIRDiagnosticHandler = delegate of MLIRDiagnostic * nativeint -> MLIRLogicalResult

// =============================================================================
// Dynamic Library Loading
// =============================================================================

module private NativeLibrary =
    let private getLibraryName() =
        match getOS() with
        | PlatformOS.Windows -> "MLIR.dll"
        | PlatformOS.MacOS -> "libMLIR.dylib" 
        | PlatformOS.Linux -> "libMLIR.so"
        | _ -> "libMLIR.so"
    
    // Windows DLL import functions
    [<DllImport("kernel32.dll", SetLastError = true)>]
    extern nativeint private LoadLibraryWindows(string lpFileName)
    
    [<DllImport("kernel32.dll", SetLastError = true)>]
    extern nativeint private GetProcAddressWindows(nativeint hModule, string lpProcName)
    
    // Unix/Linux/macOS dynamic library functions
    [<DllImport("libdl.dylib", SetLastError = true)>]
    extern nativeint private dlopenMacOS(string filename, int flags)
    
    [<DllImport("libdl.dylib", SetLastError = true)>]
    extern nativeint private dlsymMacOS(nativeint handle, string symbol)
    
    [<DllImport("libdl.so.2", SetLastError = true)>]
    extern nativeint private dlopenLinux(string filename, int flags)
    
    [<DllImport("libdl.so.2", SetLastError = true)>]
    extern nativeint private dlsymLinux(nativeint handle, string symbol)
    
    let private RTLD_LAZY = 1
    
    let private libraryHandle = 
        lazy (
            let libName = getLibraryName()
            let handle = 
                match getOS() with
                | PlatformOS.Windows -> LoadLibraryWindows(libName)
                | PlatformOS.MacOS -> dlopenMacOS(libName, RTLD_LAZY)
                | PlatformOS.Linux -> dlopenLinux(libName, RTLD_LAZY)
                | _ -> dlopenLinux(libName, RTLD_LAZY)
            
            if handle = nativeint 0 then
                failwithf "Failed to load MLIR library: %s" libName
            handle
        )
    
    let getFunction<'T> (name: string) : 'T =
        let funcPtr = 
            match getOS() with
            | PlatformOS.Windows -> GetProcAddressWindows(libraryHandle.Value, name)
            | PlatformOS.MacOS -> dlsymMacOS(libraryHandle.Value, name)
            | PlatformOS.Linux -> dlsymLinux(libraryHandle.Value, name)
            | _ -> dlsymLinux(libraryHandle.Value, name)
        
        if funcPtr = nativeint 0 then
            failwithf "Function %s not found in MLIR library" name
        
        Marshal.GetDelegateForFunctionPointer<'T>(funcPtr)

// =============================================================================
// Function Delegate Types
// =============================================================================

// Context Functions
type mlirContextCreateDelegate = delegate of unit -> MLIRContext
type mlirContextDestroyDelegate = delegate of MLIRContext -> unit
type mlirContextSetAllowUnregisteredDialectsDelegate = delegate of MLIRContext * bool -> unit
type mlirContextGetAllowUnregisteredDialectsDelegate = delegate of MLIRContext -> bool
type mlirContextGetNumRegisteredDialectsDelegate = delegate of MLIRContext -> nativeint
type mlirContextGetNumLoadedDialectsDelegate = delegate of MLIRContext -> nativeint
type mlirContextAppendDialectRegistryDelegate = delegate of MLIRContext * MLIRDialectRegistry -> unit

// Location Functions
type mlirLocationUnknownGetDelegate = delegate of MLIRContext -> MLIRLocation
type mlirLocationFileLineColGetDelegate = delegate of MLIRContext * MLIRStringRef * uint32 * uint32 -> MLIRLocation
type mlirLocationEqualDelegate = delegate of MLIRLocation * MLIRLocation -> bool
type mlirLocationPrintDelegate = delegate of MLIRLocation * MLIRStringCallback * nativeint -> unit

// Module Functions
type mlirModuleCreateEmptyDelegate = delegate of MLIRLocation -> MLIRModule
type mlirModuleDestroyDelegate = delegate of MLIRModule -> unit
type mlirModuleGetOperationDelegate = delegate of MLIRModule -> MLIROperation

// Operation Functions
type mlirOperationCreateDelegate = delegate of MLIRStringRef * MLIRLocation * nativeint * nativeint * nativeint * nativeint * nativeint * nativeint * nativeint * nativeint -> MLIROperation
type mlirOperationDestroyDelegate = delegate of MLIROperation -> unit
type mlirOperationGetNumResultsDelegate = delegate of MLIROperation -> nativeint
type mlirOperationGetResultDelegate = delegate of MLIROperation * nativeint -> MLIRValue
type mlirOperationSetAttributeByNameDelegate = delegate of MLIROperation * MLIRStringRef * MLIRAttribute -> unit
type mlirOperationGetAttributeByNameDelegate = delegate of MLIROperation * MLIRStringRef -> MLIRAttribute
type mlirOperationVerifyDelegate = delegate of MLIROperation -> MLIRLogicalResult
type mlirOperationDumpDelegate = delegate of MLIROperation -> unit
type mlirOperationPrintDelegate = delegate of MLIROperation * MLIRStringCallback * nativeint -> unit

// Type Functions
type mlirIntegerTypeGetDelegate = delegate of MLIRContext * uint32 -> MLIRType
type mlirIntegerTypeSignedGetDelegate = delegate of MLIRContext * uint32 -> MLIRType
type mlirIntegerTypeUnsignedGetDelegate = delegate of MLIRContext * uint32 -> MLIRType
type mlirIntegerTypeGetWidthDelegate = delegate of MLIRType -> uint32
type mlirIndexTypeGetDelegate = delegate of MLIRContext -> MLIRType
type mlirBF16TypeGetDelegate = delegate of MLIRContext -> MLIRType
type mlirF16TypeGetDelegate = delegate of MLIRContext -> MLIRType
type mlirF32TypeGetDelegate = delegate of MLIRContext -> MLIRType
type mlirF64TypeGetDelegate = delegate of MLIRContext -> MLIRType
type mlirNoneTypeGetDelegate = delegate of MLIRContext -> MLIRType
type mlirFunctionTypeGetDelegate = delegate of MLIRContext * nativeint * nativeint * nativeint * nativeint -> MLIRType
type mlirFunctionTypeGetNumInputsDelegate = delegate of MLIRType -> nativeint
type mlirFunctionTypeGetNumResultsDelegate = delegate of MLIRType -> nativeint
type mlirFunctionTypeGetInputDelegate = delegate of MLIRType * nativeint -> MLIRType
type mlirFunctionTypeGetResultDelegate = delegate of MLIRType * nativeint -> MLIRType
type mlirTypeIsAIntegerDelegate = delegate of MLIRType -> bool
type mlirTypeIsAFloatDelegate = delegate of MLIRType -> bool
type mlirTypeIsAFunctionDelegate = delegate of MLIRType -> bool
type mlirTypeIsAIndexDelegate = delegate of MLIRType -> bool
type mlirFloatTypeGetWidthDelegate = delegate of MLIRType -> uint32
type mlirTypePrintDelegate = delegate of MLIRType * MLIRStringCallback * nativeint -> unit

// Attribute Functions
type mlirStringAttrGetDelegate = delegate of MLIRContext * MLIRStringRef -> MLIRAttribute
type mlirBoolAttrGetDelegate = delegate of MLIRContext * int -> MLIRAttribute
type mlirIntegerAttrGetDelegate = delegate of MLIRType * int64 -> MLIRAttribute
type mlirFloatAttrDoubleGetDelegate = delegate of MLIRContext * MLIRType * double -> MLIRAttribute
type mlirTypeAttrGetDelegate = delegate of MLIRType -> MLIRAttribute
type mlirUnitAttrGetDelegate = delegate of MLIRContext -> MLIRAttribute
type mlirAttributeIsAStringDelegate = delegate of MLIRAttribute -> bool
type mlirAttributeIsABoolDelegate = delegate of MLIRAttribute -> bool
type mlirAttributeIsAIntegerDelegate = delegate of MLIRAttribute -> bool
type mlirAttributeIsAFloatDelegate = delegate of MLIRAttribute -> bool
type mlirAttributeIsATypeDelegate = delegate of MLIRAttribute -> bool
type mlirAttributeIsAUnitDelegate = delegate of MLIRAttribute -> bool
type mlirStringAttrGetValueDelegate = delegate of MLIRAttribute -> MLIRStringRef
type mlirBoolAttrGetValueDelegate = delegate of MLIRAttribute -> bool
type mlirIntegerAttrGetValueIntDelegate = delegate of MLIRAttribute -> int64
type mlirFloatAttrGetValueDoubleDelegate = delegate of MLIRAttribute -> double
type mlirTypeAttrGetValueDelegate = delegate of MLIRAttribute -> MLIRType
type mlirAttributePrintDelegate = delegate of MLIRAttribute * MLIRStringCallback * nativeint -> unit

// Value Functions
type mlirValueGetTypeDelegate = delegate of MLIRValue -> MLIRType
type mlirValuePrintDelegate = delegate of MLIRValue * MLIRStringCallback * nativeint -> unit

// Dialect Registry Functions
type mlirDialectRegistryCreateDelegate = delegate of unit -> MLIRDialectRegistry
type mlirDialectRegistryDestroyDelegate = delegate of MLIRDialectRegistry -> unit
type mlirRegisterAllDialectsDelegate = delegate of MLIRDialectRegistry -> unit

// Pass Manager Functions
type mlirPassManagerCreateDelegate = delegate of MLIRContext -> MLIRPassManager
type mlirPassManagerDestroyDelegate = delegate of MLIRPassManager -> unit
type mlirPassManagerRunOnOpDelegate = delegate of MLIRPassManager * MLIROperation -> MLIRLogicalResult
type mlirPassManagerEnableIRPrintingDelegate = delegate of MLIRPassManager * bool * bool * bool * bool * bool * MLIROpPrintingFlags * MLIRStringRef -> unit
type mlirPassManagerEnableVerifierDelegate = delegate of MLIRPassManager * bool -> unit
type mlirPassManagerGetNestedUnderDelegate = delegate of MLIRPassManager * MLIRStringRef -> MLIROpPassManager
type mlirOpPassManagerGetNestedUnderDelegate = delegate of MLIROpPassManager * MLIRStringRef -> MLIROpPassManager
type mlirOpPassManagerAddPipelineDelegate = delegate of MLIROpPassManager * MLIRStringRef * MLIRStringCallback * nativeint -> MLIRLogicalResult

// Printing Functions
type mlirOpPrintingFlagsCreateDelegate = delegate of unit -> MLIROpPrintingFlags
type mlirOpPrintingFlagsDestroyDelegate = delegate of MLIROpPrintingFlags -> unit

// Block and Region Functions
type mlirRegionCreateDelegate = delegate of unit -> MLIRRegion
type mlirRegionDestroyDelegate = delegate of MLIRRegion -> unit
type mlirRegionGetFirstBlockDelegate = delegate of MLIRRegion -> MLIRBlock
type mlirRegionAppendOwnedBlockDelegate = delegate of MLIRRegion * MLIRBlock -> unit
type mlirBlockCreateDelegate = delegate of nativeint * nativeint * MLIRLocation -> MLIRBlock
type mlirBlockDestroyDelegate = delegate of MLIRBlock -> unit
type mlirBlockGetParentRegionDelegate = delegate of MLIRBlock -> MLIRRegion
type mlirBlockAppendOwnedOperationDelegate = delegate of MLIRBlock * MLIROperation -> unit
type mlirBlockAttributeGetDelegate = delegate of MLIRBlock -> MLIRAttribute

// Additional Operations for AST conversion
type mlirArrayTypeGetDelegate = delegate of MLIRType * uint32 -> MLIRType
type mlirStringAttributeGetValueDelegate = delegate of MLIRAttribute -> string
type mlirNamedAttributeGetDelegate = delegate of MLIRAttribute * MLIRAttribute -> MLIRNamedAttribute

// =============================================================================
// Lazy-loaded Function Instances
// =============================================================================

// Context Functions
let mlirContextCreate = lazy (NativeLibrary.getFunction<mlirContextCreateDelegate> "mlirContextCreate")
let mlirContextDestroy = lazy (NativeLibrary.getFunction<mlirContextDestroyDelegate> "mlirContextDestroy")
let mlirContextSetAllowUnregisteredDialects = lazy (NativeLibrary.getFunction<mlirContextSetAllowUnregisteredDialectsDelegate> "mlirContextSetAllowUnregisteredDialects")
let mlirContextGetAllowUnregisteredDialects = lazy (NativeLibrary.getFunction<mlirContextGetAllowUnregisteredDialectsDelegate> "mlirContextGetAllowUnregisteredDialects")
let mlirContextGetNumRegisteredDialects = lazy (NativeLibrary.getFunction<mlirContextGetNumRegisteredDialectsDelegate> "mlirContextGetNumRegisteredDialects")
let mlirContextGetNumLoadedDialects = lazy (NativeLibrary.getFunction<mlirContextGetNumLoadedDialectsDelegate> "mlirContextGetNumLoadedDialects")
let mlirContextAppendDialectRegistry = lazy (NativeLibrary.getFunction<mlirContextAppendDialectRegistryDelegate> "mlirContextAppendDialectRegistry")

// Location Functions
let mlirLocationUnknownGet = lazy (NativeLibrary.getFunction<mlirLocationUnknownGetDelegate> "mlirLocationUnknownGet")
let mlirLocationFileLineColGet = lazy (NativeLibrary.getFunction<mlirLocationFileLineColGetDelegate> "mlirLocationFileLineColGet")
let mlirLocationEqual = lazy (NativeLibrary.getFunction<mlirLocationEqualDelegate> "mlirLocationEqual")
let mlirLocationPrint = lazy (NativeLibrary.getFunction<mlirLocationPrintDelegate> "mlirLocationPrint")
let mlirLocationDestroy = lazy (NativeLibrary.getFunction<mlirContextDestroyDelegate> "mlirLocationDestroy")

// Module Functions
let mlirModuleCreateEmpty = lazy (NativeLibrary.getFunction<mlirModuleCreateEmptyDelegate> "mlirModuleCreateEmpty")
let mlirModuleDestroy = lazy (NativeLibrary.getFunction<mlirModuleDestroyDelegate> "mlirModuleDestroy")
let mlirModuleGetOperation = lazy (NativeLibrary.getFunction<mlirModuleGetOperationDelegate> "mlirModuleGetOperation")

// Operation Functions
let mlirOperationCreate = lazy (NativeLibrary.getFunction<mlirOperationCreateDelegate> "mlirOperationCreate")
let mlirOperationDestroy = lazy (NativeLibrary.getFunction<mlirOperationDestroyDelegate> "mlirOperationDestroy")
let mlirOperationGetNumResults = lazy (NativeLibrary.getFunction<mlirOperationGetNumResultsDelegate> "mlirOperationGetNumResults")
let mlirOperationGetResult = lazy (NativeLibrary.getFunction<mlirOperationGetResultDelegate> "mlirOperationGetResult")
let mlirOperationSetAttributeByName = lazy (NativeLibrary.getFunction<mlirOperationSetAttributeByNameDelegate> "mlirOperationSetAttributeByName")
let mlirOperationGetAttributeByName = lazy (NativeLibrary.getFunction<mlirOperationGetAttributeByNameDelegate> "mlirOperationGetAttributeByName")
let mlirOperationVerify = lazy (NativeLibrary.getFunction<mlirOperationVerifyDelegate> "mlirOperationVerify")
let mlirOperationDump = lazy (NativeLibrary.getFunction<mlirOperationDumpDelegate> "mlirOperationDump")
let mlirOperationPrint = lazy (NativeLibrary.getFunction<mlirOperationPrintDelegate> "mlirOperationPrint")

// Type Functions
let mlirIntegerTypeGet = lazy (NativeLibrary.getFunction<mlirIntegerTypeGetDelegate> "mlirIntegerTypeGet")
let mlirIntegerTypeSignedGet = lazy (NativeLibrary.getFunction<mlirIntegerTypeSignedGetDelegate> "mlirIntegerTypeSignedGet")
let mlirIntegerTypeUnsignedGet = lazy (NativeLibrary.getFunction<mlirIntegerTypeUnsignedGetDelegate> "mlirIntegerTypeUnsignedGet")
let mlirIntegerTypeGetWidth = lazy (NativeLibrary.getFunction<mlirIntegerTypeGetWidthDelegate> "mlirIntegerTypeGetWidth")
let mlirIndexTypeGet = lazy (NativeLibrary.getFunction<mlirIndexTypeGetDelegate> "mlirIndexTypeGet")
let mlirBF16TypeGet = lazy (NativeLibrary.getFunction<mlirBF16TypeGetDelegate> "mlirBF16TypeGet")
let mlirF16TypeGet = lazy (NativeLibrary.getFunction<mlirF16TypeGetDelegate> "mlirF16TypeGet")
let mlirF32TypeGet = lazy (NativeLibrary.getFunction<mlirF32TypeGetDelegate> "mlirF32TypeGet")
let mlirF64TypeGet = lazy (NativeLibrary.getFunction<mlirF64TypeGetDelegate> "mlirF64TypeGet")
let mlirNoneTypeGet = lazy (NativeLibrary.getFunction<mlirNoneTypeGetDelegate> "mlirNoneTypeGet")
let mlirFunctionTypeGet = lazy (NativeLibrary.getFunction<mlirFunctionTypeGetDelegate> "mlirFunctionTypeGet")
let mlirFunctionTypeGetNumInputs = lazy (NativeLibrary.getFunction<mlirFunctionTypeGetNumInputsDelegate> "mlirFunctionTypeGetNumInputs")
let mlirFunctionTypeGetNumResults = lazy (NativeLibrary.getFunction<mlirFunctionTypeGetNumResultsDelegate> "mlirFunctionTypeGetNumResults")
let mlirFunctionTypeGetInput = lazy (NativeLibrary.getFunction<mlirFunctionTypeGetInputDelegate> "mlirFunctionTypeGetInput")
let mlirFunctionTypeGetResult = lazy (NativeLibrary.getFunction<mlirFunctionTypeGetResultDelegate> "mlirFunctionTypeGetResult")
let mlirTypeIsAInteger = lazy (NativeLibrary.getFunction<mlirTypeIsAIntegerDelegate> "mlirTypeIsAInteger")
let mlirTypeIsAFloat = lazy (NativeLibrary.getFunction<mlirTypeIsAFloatDelegate> "mlirTypeIsAFloat")
let mlirTypeIsAFunction = lazy (NativeLibrary.getFunction<mlirTypeIsAFunctionDelegate> "mlirTypeIsAFunction")
let mlirTypeIsAIndex = lazy (NativeLibrary.getFunction<mlirTypeIsAIndexDelegate> "mlirTypeIsAIndex")
let mlirFloatTypeGetWidth = lazy (NativeLibrary.getFunction<mlirFloatTypeGetWidthDelegate> "mlirFloatTypeGetWidth")
let mlirTypePrint = lazy (NativeLibrary.getFunction<mlirTypePrintDelegate> "mlirTypePrint")

// Attribute Functions
let mlirStringAttrGet = lazy (NativeLibrary.getFunction<mlirStringAttrGetDelegate> "mlirStringAttrGet")
let mlirBoolAttrGet = lazy (NativeLibrary.getFunction<mlirBoolAttrGetDelegate> "mlirBoolAttrGet")
let mlirIntegerAttrGet = lazy (NativeLibrary.getFunction<mlirIntegerAttrGetDelegate> "mlirIntegerAttrGet")
let mlirFloatAttrDoubleGet = lazy (NativeLibrary.getFunction<mlirFloatAttrDoubleGetDelegate> "mlirFloatAttrDoubleGet")
let mlirTypeAttrGet = lazy (NativeLibrary.getFunction<mlirTypeAttrGetDelegate> "mlirTypeAttrGet")
let mlirUnitAttrGet = lazy (NativeLibrary.getFunction<mlirUnitAttrGetDelegate> "mlirUnitAttrGet")
let mlirAttributeIsAString = lazy (NativeLibrary.getFunction<mlirAttributeIsAStringDelegate> "mlirAttributeIsAString")
let mlirAttributeIsABool = lazy (NativeLibrary.getFunction<mlirAttributeIsABoolDelegate> "mlirAttributeIsABool")
let mlirAttributeIsAInteger = lazy (NativeLibrary.getFunction<mlirAttributeIsAIntegerDelegate> "mlirAttributeIsAInteger")
let mlirAttributeIsAFloat = lazy (NativeLibrary.getFunction<mlirAttributeIsAFloatDelegate> "mlirAttributeIsAFloat")
let mlirAttributeIsAType = lazy (NativeLibrary.getFunction<mlirAttributeIsATypeDelegate> "mlirAttributeIsAType")
let mlirAttributeIsAUnit = lazy (NativeLibrary.getFunction<mlirAttributeIsAUnitDelegate> "mlirAttributeIsAUnit")
let mlirStringAttrGetValue = lazy (NativeLibrary.getFunction<mlirStringAttrGetValueDelegate> "mlirStringAttrGetValue")
let mlirBoolAttrGetValue = lazy (NativeLibrary.getFunction<mlirBoolAttrGetValueDelegate> "mlirBoolAttrGetValue")
let mlirIntegerAttrGetValueInt = lazy (NativeLibrary.getFunction<mlirIntegerAttrGetValueIntDelegate> "mlirIntegerAttrGetValueInt")
let mlirFloatAttrGetValueDouble = lazy (NativeLibrary.getFunction<mlirFloatAttrGetValueDoubleDelegate> "mlirFloatAttrGetValueDouble")
let mlirTypeAttrGetValue = lazy (NativeLibrary.getFunction<mlirTypeAttrGetValueDelegate> "mlirTypeAttrGetValue")
let mlirAttributePrint = lazy (NativeLibrary.getFunction<mlirAttributePrintDelegate> "mlirAttributePrint")

// Value Functions
let mlirValueGetType = lazy (NativeLibrary.getFunction<mlirValueGetTypeDelegate> "mlirValueGetType")
let mlirValuePrint = lazy (NativeLibrary.getFunction<mlirValuePrintDelegate> "mlirValuePrint")

// Dialect Registry Functions
let mlirDialectRegistryCreate = lazy (NativeLibrary.getFunction<mlirDialectRegistryCreateDelegate> "mlirDialectRegistryCreate")
let mlirDialectRegistryDestroy = lazy (NativeLibrary.getFunction<mlirDialectRegistryDestroyDelegate> "mlirDialectRegistryDestroy")
let mlirRegisterAllDialects = lazy (NativeLibrary.getFunction<mlirRegisterAllDialectsDelegate> "mlirRegisterAllDialects")

// Pass Manager Functions
let mlirPassManagerCreate = lazy (NativeLibrary.getFunction<mlirPassManagerCreateDelegate> "mlirPassManagerCreate")
let mlirPassManagerDestroy = lazy (NativeLibrary.getFunction<mlirPassManagerDestroyDelegate> "mlirPassManagerDestroy")
let mlirPassManagerRunOnOp = lazy (NativeLibrary.getFunction<mlirPassManagerRunOnOpDelegate> "mlirPassManagerRunOnOp")
let mlirPassManagerEnableIRPrinting = lazy (NativeLibrary.getFunction<mlirPassManagerEnableIRPrintingDelegate> "mlirPassManagerEnableIRPrinting")
let mlirPassManagerEnableVerifier = lazy (NativeLibrary.getFunction<mlirPassManagerEnableVerifierDelegate> "mlirPassManagerEnableVerifier")
let mlirPassManagerGetNestedUnder = lazy (NativeLibrary.getFunction<mlirPassManagerGetNestedUnderDelegate> "mlirPassManagerGetNestedUnder")
let mlirOpPassManagerGetNestedUnder = lazy (NativeLibrary.getFunction<mlirOpPassManagerGetNestedUnderDelegate> "mlirOpPassManagerGetNestedUnder")
let mlirOpPassManagerAddPipeline = lazy (NativeLibrary.getFunction<mlirOpPassManagerAddPipelineDelegate> "mlirOpPassManagerAddPipeline")

// Printing Functions
let mlirOpPrintingFlagsCreate = lazy (NativeLibrary.getFunction<mlirOpPrintingFlagsCreateDelegate> "mlirOpPrintingFlagsCreate")
let mlirOpPrintingFlagsDestroy = lazy (NativeLibrary.getFunction<mlirOpPrintingFlagsDestroyDelegate> "mlirOpPrintingFlagsDestroy")

// Block and Region Functions
let mlirRegionCreate = lazy (NativeLibrary.getFunction<mlirRegionCreateDelegate> "mlirRegionCreate")
let mlirRegionDestroy = lazy (NativeLibrary.getFunction<mlirRegionDestroyDelegate> "mlirRegionDestroy")
let mlirRegionGetFirstBlock = lazy (NativeLibrary.getFunction<mlirRegionGetFirstBlockDelegate> "mlirRegionGetFirstBlock")
let mlirRegionAppendOwnedBlock = lazy (NativeLibrary.getFunction<mlirRegionAppendOwnedBlockDelegate> "mlirRegionAppendOwnedBlock")
let mlirBlockCreate = lazy (NativeLibrary.getFunction<mlirBlockCreateDelegate> "mlirBlockCreate")
let mlirBlockDestroy = lazy (NativeLibrary.getFunction<mlirBlockDestroyDelegate> "mlirBlockDestroy")
let mlirBlockGetParentRegion = lazy (NativeLibrary.getFunction<mlirBlockGetParentRegionDelegate> "mlirBlockGetParentRegion")
let mlirBlockAppendOwnedOperation = lazy (NativeLibrary.getFunction<mlirBlockAppendOwnedOperationDelegate> "mlirBlockAppendOwnedOperation")
let mlirBlockAttributeGet = lazy (NativeLibrary.getFunction<mlirBlockAttributeGetDelegate> "mlirBlockAttributeGet")

// Additional Operations
let mlirArrayTypeGet = lazy (NativeLibrary.getFunction<mlirArrayTypeGetDelegate> "mlirArrayTypeGet")
let mlirStringAttributeGetValue = lazy (NativeLibrary.getFunction<mlirStringAttributeGetValueDelegate> "mlirStringAttributeGetValue")
let mlirNamedAttributeGet = lazy (NativeLibrary.getFunction<mlirNamedAttributeGetDelegate> "mlirNamedAttributeGet")

// =============================================================================
// Helper Functions
// =============================================================================

/// Create a string reference from an F# string
let createStringRef (str: string) =
    MLIRStringRef.Create(str)

/// Free memory allocated for a string reference
let freeStringRef (stringRef: MLIRStringRef) =
    stringRef.Dispose()

/// Convert MLIR string reference to F# string
let stringFromStringRef (stringRef: MLIRStringRef) =
    stringRef.ToString()

/// Convenience function to invoke lazy-loaded functions
let inline invoke (lazyFunc: Lazy<'T>) = lazyFunc.Value