module FSharpMLIR.Bindings.LLVM

open System
open System.Runtime.InteropServices
open FSharpMLIR.PlatformUtils

// =============================================================================
// LLVM Type Definitions
// =============================================================================

// LLVM Boolean type - uses int32 internally
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
type LLVMTargetDataRef = nativeint
type LLVMMemoryBufferRef = nativeint
type LLVMAttributeRef = nativeint
type LLVMMetadataRef = nativeint
type LLVMUseRef = nativeint
type LLVMNamedMDNodeRef = nativeint
type LLVMModuleFlagEntry = nativeint
type LLVMValueMetadataEntry = nativeint
type LLVMDiagnosticInfoRef = nativeint
type LLVMPassBuilderOptionsRef = nativeint
type LLVMErrorRef = nativeint
type LLVMOperandBundleRef = nativeint
type LLVMDbgRecordRef = nativeint

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

// Verification actions
type LLVMVerifierFailureAction =
    | AbortProcess = 0
    | PrintMessage = 1
    | ReturnStatus = 2

// Integer predicates
type LLVMIntPredicate =
    | EQ = 32
    | NE = 33
    | UGT = 34
    | UGE = 35
    | ULT = 36
    | ULE = 37
    | SGT = 38
    | SGE = 39
    | SLT = 40
    | SLE = 41

// Float predicates
type LLVMRealPredicate =
    | PredicateFalse = 0
    | OEQ = 1
    | OGT = 2
    | OGE = 3
    | OLT = 4
    | OLE = 5
    | ONE = 6
    | ORD = 7
    | UNO = 8
    | UEQ = 9
    | UGT = 10
    | UGE = 11
    | ULT = 12
    | ULE = 13
    | UNE = 14
    | PredicateTrue = 15

// Atomic orderings
type LLVMAtomicOrdering =
    | NotAtomic = 0
    | Unordered = 1
    | Monotonic = 2
    | Acquire = 4
    | Release = 5
    | AcquireRelease = 6
    | SequentiallyConsistent = 7

// Atomic RMW binary operations
type LLVMAtomicRMWBinOp =
    | Xchg = 0
    | Add = 1
    | Sub = 2
    | And = 3
    | Nand = 4
    | Or = 5
    | Xor = 6
    | Max = 7
    | Min = 8
    | UMax = 9
    | UMin = 10

// Opcodes
type LLVMOpcode =
    | Ret = 1
    | Br = 2
    | Switch = 3
    | IndirectBr = 4
    | Invoke = 5
    | Unreachable = 7
    | Add = 8
    | FAdd = 9
    | Sub = 10
    | FSub = 11
    | Mul = 12
    | FMul = 13
    | UDiv = 14
    | SDiv = 15
    | FDiv = 16
    | URem = 17
    | SRem = 18
    | FRem = 19
    | Shl = 20
    | LShr = 21
    | AShr = 22
    | And = 23
    | Or = 24
    | Xor = 25
    | Alloca = 26
    | Load = 27
    | Store = 28
    | GetElementPtr = 29
    | Trunc = 30
    | ZExt = 31
    | SExt = 32
    | FPToUI = 33
    | FPToSI = 34
    | UIToFP = 35
    | SIToFP = 36
    | FPTrunc = 37
    | FPExt = 38
    | PtrToInt = 39
    | IntToPtr = 40
    | BitCast = 41
    | ICmp = 42
    | FCmp = 43
    | PHI = 44
    | Call = 45
    | Select = 46
    | ExtractElement = 50
    | InsertElement = 51
    | ShuffleVector = 52
    | ExtractValue = 53
    | InsertValue = 54

// Linkage types
type LLVMLinkage =
    | External = 0
    | AvailableExternally = 1
    | LinkOnceAny = 2
    | LinkOnceODR = 3
    | LinkOnceODRAutoHide = 4
    | WeakAny = 5
    | WeakODR = 6
    | Appending = 7
    | Internal = 8
    | Private = 9
    | DLLImport = 10
    | DLLExport = 11
    | ExternalWeak = 12
    | Ghost = 13
    | Common = 14
    | LinkerPrivate = 15
    | LinkerPrivateWeak = 16

// Visibility styles
type LLVMVisibility =
    | Default = 0
    | Hidden = 1
    | Protected = 2

// Calling conventions
type LLVMCallConv =
    | C = 0
    | Fast = 8
    | Cold = 9
    | GHC = 10
    | HiPE = 11
    | WebKitJS = 12
    | AnyReg = 13
    | PreserveMost = 14
    | PreserveAll = 15
    | Swift = 16
    | CXXFastTLS = 17
    | X86Stdcall = 64
    | X86Fastcall = 65
    | ARMAPCS = 66
    | ARMAAPCS = 67
    | ARMAAPCS_VFP = 68
    | MSP430INTR = 69
    | X86ThisCall = 70
    | PTXKernel = 71
    | PTXDevice = 72

// Type kinds
type LLVMTypeKind =
    | Void = 0
    | Half = 1
    | Float = 2
    | Double = 3
    | X86_FP80 = 4
    | FP128 = 5
    | PPC_FP128 = 6
    | Label = 7
    | Integer = 8
    | Function = 9
    | Struct = 10
    | Array = 11
    | Pointer = 12
    | Vector = 13
    | Metadata = 14
    | Token = 16
    | ScalableVector = 17
    | BFloat = 18
    | X86_AMX = 19
    | TargetExt = 20

// =============================================================================
// Dynamic Library Loading
// =============================================================================

module private NativeLibrary =
    let private getLibraryName() =
        match getOS() with
        | PlatformOS.Windows -> "LLVM.dll"
        | PlatformOS.MacOS -> "libLLVM.dylib"
        | PlatformOS.Linux -> "libLLVM.so"
        | _ -> "libLLVM.so"
    
    let private libraryHandle = 
        lazy (
            let libName = getLibraryName()
            let handle = 
                if Environment.OSVersion.Platform = PlatformID.Win32NT then
                    LoadLibrary(libName)
                else
                    dlopen(libName, 1) // RTLD_LAZY
            
            if handle = nativeint.Zero then
                failwithf "Failed to load LLVM library: %s" libName
            handle
        )
    
    [<DllImport("kernel32.dll", SetLastError = true)>]
    extern nativeint LoadLibrary(string lpFileName)
    
    [<DllImport("kernel32.dll", SetLastError = true)>]
    extern nativeint GetProcAddress(nativeint hModule, string lpProcName)
    
    [<DllImport("libdl.so", SetLastError = true)>]
    extern nativeint dlopen(string filename, int flags)
    
    [<DllImport("libdl.so", SetLastError = true)>]
    extern nativeint dlsym(nativeint handle, string symbol)
    
    let getFunction<'T> (name: string) : 'T =
        let funcPtr = 
            if Environment.OSVersion.Platform = PlatformID.Win32NT then
                GetProcAddress(libraryHandle.Value, name)
            else
                dlsym(libraryHandle.Value, name)
        
        if funcPtr = nativeint.Zero then
            failwithf "Function %s not found in LLVM library" name
        
        Marshal.GetDelegateForFunctionPointer<'T>(funcPtr)

// =============================================================================
// Function Delegate Types
// =============================================================================

// Context Functions
type LLVMContextCreateDelegate = delegate of unit -> LLVMContextRef
type LLVMGetGlobalContextDelegate = delegate of unit -> LLVMContextRef
type LLVMContextDisposeDelegate = delegate of LLVMContextRef -> unit
type LLVMContextSetDiscardValueNamesDelegate = delegate of LLVMContextRef * LLVMBool -> unit
type LLVMContextShouldDiscardValueNamesDelegate = delegate of LLVMContextRef -> LLVMBool

// Module Functions
type LLVMModuleCreateWithNameDelegate = delegate of string -> LLVMModuleRef
type LLVMModuleCreateWithNameInContextDelegate = delegate of string * LLVMContextRef -> LLVMModuleRef
type LLVMCloneModuleDelegate = delegate of LLVMModuleRef -> LLVMModuleRef
type LLVMDisposeModuleDelegate = delegate of LLVMModuleRef -> unit
type LLVMGetModuleIdentifierDelegate = delegate of LLVMModuleRef * nativeint -> nativeint
type LLVMSetModuleIdentifierDelegate = delegate of LLVMModuleRef * string * uint32 -> unit
type LLVMGetSourceFileNameDelegate = delegate of LLVMModuleRef * nativeint -> nativeint
type LLVMSetSourceFileNameDelegate = delegate of LLVMModuleRef * string * uint32 -> unit
type LLVMGetDataLayoutStrDelegate = delegate of LLVMModuleRef -> nativeint
type LLVMSetDataLayoutDelegate = delegate of LLVMModuleRef * string -> unit
type LLVMGetTargetDelegate = delegate of LLVMModuleRef -> nativeint
type LLVMSetTargetDelegate = delegate of LLVMModuleRef * string -> unit
type LLVMDumpModuleDelegate = delegate of LLVMModuleRef -> unit
type LLVMPrintModuleToFileDelegate = delegate of LLVMModuleRef * string * nativeint -> LLVMBool
type LLVMPrintModuleToStringDelegate = delegate of LLVMModuleRef -> nativeint
type LLVMVerifyModuleDelegate = delegate of LLVMModuleRef * LLVMVerifierFailureAction * nativeint -> LLVMBool
type LLVMGetModuleContextDelegate = delegate of LLVMModuleRef -> LLVMContextRef

// Type Functions
type LLVMGetTypeKindDelegate = delegate of LLVMTypeRef -> LLVMTypeKind
type LLVMTypeIsSizedDelegate = delegate of LLVMTypeRef -> LLVMBool
type LLVMGetTypeContextDelegate = delegate of LLVMTypeRef -> LLVMContextRef
type LLVMDumpTypeDelegate = delegate of LLVMTypeRef -> unit
type LLVMPrintTypeToStringDelegate = delegate of LLVMTypeRef -> nativeint

// Integer types
type LLVMInt1TypeInContextDelegate = delegate of LLVMContextRef -> LLVMTypeRef
type LLVMInt8TypeInContextDelegate = delegate of LLVMContextRef -> LLVMTypeRef
type LLVMInt16TypeInContextDelegate = delegate of LLVMContextRef -> LLVMTypeRef
type LLVMInt32TypeInContextDelegate = delegate of LLVMContextRef -> LLVMTypeRef
type LLVMInt64TypeInContextDelegate = delegate of LLVMContextRef -> LLVMTypeRef
type LLVMInt128TypeInContextDelegate = delegate of LLVMContextRef -> LLVMTypeRef
type LLVMIntTypeInContextDelegate = delegate of LLVMContextRef * uint32 -> LLVMTypeRef
type LLVMGetIntTypeWidthDelegate = delegate of LLVMTypeRef -> uint32

// Floating point types
type LLVMHalfTypeInContextDelegate = delegate of LLVMContextRef -> LLVMTypeRef
type LLVMBFloatTypeInContextDelegate = delegate of LLVMContextRef -> LLVMTypeRef
type LLVMFloatTypeInContextDelegate = delegate of LLVMContextRef -> LLVMTypeRef
type LLVMDoubleTypeInContextDelegate = delegate of LLVMContextRef -> LLVMTypeRef
type LLVMX86FP80TypeInContextDelegate = delegate of LLVMContextRef -> LLVMTypeRef
type LLVMFP128TypeInContextDelegate = delegate of LLVMContextRef -> LLVMTypeRef
type LLVMPPCFP128TypeInContextDelegate = delegate of LLVMContextRef -> LLVMTypeRef

// Other types
type LLVMVoidTypeInContextDelegate = delegate of LLVMContextRef -> LLVMTypeRef
type LLVMLabelTypeInContextDelegate = delegate of LLVMContextRef -> LLVMTypeRef
type LLVMX86AMXTypeInContextDelegate = delegate of LLVMContextRef -> LLVMTypeRef
type LLVMTokenTypeInContextDelegate = delegate of LLVMContextRef -> LLVMTypeRef
type LLVMMetadataTypeInContextDelegate = delegate of LLVMContextRef -> LLVMTypeRef

// Pointer types
type LLVMPointerTypeDelegate = delegate of LLVMTypeRef * uint32 -> LLVMTypeRef
type LLVMPointerTypeInContextDelegate = delegate of LLVMContextRef * uint32 -> LLVMTypeRef
type LLVMPointerTypeIsOpaqueDelegate = delegate of LLVMTypeRef -> LLVMBool
type LLVMGetPointerAddressSpaceDelegate = delegate of LLVMTypeRef -> uint32

// Function types
type LLVMFunctionTypeDelegate = delegate of LLVMTypeRef * nativeint * uint32 * LLVMBool -> LLVMTypeRef
type LLVMIsFunctionVarArgDelegate = delegate of LLVMTypeRef -> LLVMBool
type LLVMGetReturnTypeDelegate = delegate of LLVMTypeRef -> LLVMTypeRef
type LLVMCountParamTypesDelegate = delegate of LLVMTypeRef -> uint32
type LLVMGetParamTypesDelegate = delegate of LLVMTypeRef * nativeint -> unit

// Array types
type LLVMArrayTypeDelegate = delegate of LLVMTypeRef * uint32 -> LLVMTypeRef
type LLVMArrayType2Delegate = delegate of LLVMTypeRef * uint64 -> LLVMTypeRef
type LLVMGetArrayLengthDelegate = delegate of LLVMTypeRef -> uint32
type LLVMGetArrayLength2Delegate = delegate of LLVMTypeRef -> uint64

// Vector types
type LLVMVectorTypeDelegate = delegate of LLVMTypeRef * uint32 -> LLVMTypeRef
type LLVMScalableVectorTypeDelegate = delegate of LLVMTypeRef * uint32 -> LLVMTypeRef
type LLVMGetVectorSizeDelegate = delegate of LLVMTypeRef -> uint32

// Sequential types
type LLVMGetElementTypeDelegate = delegate of LLVMTypeRef -> LLVMTypeRef
type LLVMGetSubtypesDelegate = delegate of LLVMTypeRef * nativeint -> unit
type LLVMGetNumContainedTypesDelegate = delegate of LLVMTypeRef -> uint32

// Struct types
type LLVMStructTypeInContextDelegate = delegate of LLVMContextRef * nativeint * uint32 * LLVMBool -> LLVMTypeRef
type LLVMStructTypeDelegate = delegate of nativeint * uint32 * LLVMBool -> LLVMTypeRef
type LLVMStructCreateNamedDelegate = delegate of LLVMContextRef * string -> LLVMTypeRef
type LLVMGetStructNameDelegate = delegate of LLVMTypeRef -> nativeint
type LLVMStructSetBodyDelegate = delegate of LLVMTypeRef * nativeint * uint32 * LLVMBool -> unit
type LLVMCountStructElementTypesDelegate = delegate of LLVMTypeRef -> uint32
type LLVMGetStructElementTypesDelegate = delegate of LLVMTypeRef * nativeint -> unit
type LLVMStructGetTypeAtIndexDelegate = delegate of LLVMTypeRef * uint32 -> LLVMTypeRef
type LLVMIsPackedStructDelegate = delegate of LLVMTypeRef -> LLVMBool
type LLVMIsOpaqueStructDelegate = delegate of LLVMTypeRef -> LLVMBool
type LLVMIsLiteralStructDelegate = delegate of LLVMTypeRef -> LLVMBool

// Global context versions
type LLVMInt1TypeDelegate = delegate of unit -> LLVMTypeRef
type LLVMInt8TypeDelegate = delegate of unit -> LLVMTypeRef
type LLVMInt16TypeDelegate = delegate of unit -> LLVMTypeRef
type LLVMInt32TypeDelegate = delegate of unit -> LLVMTypeRef
type LLVMInt64TypeDelegate = delegate of unit -> LLVMTypeRef
type LLVMInt128TypeDelegate = delegate of unit -> LLVMTypeRef
type LLVMIntTypeDelegate = delegate of uint32 -> LLVMTypeRef
type LLVMHalfTypeDelegate = delegate of unit -> LLVMTypeRef
type LLVMBFloatTypeDelegate = delegate of unit -> LLVMTypeRef
type LLVMFloatTypeDelegate = delegate of unit -> LLVMTypeRef
type LLVMDoubleTypeDelegate = delegate of unit -> LLVMTypeRef
type LLVMX86FP80TypeDelegate = delegate of unit -> LLVMTypeRef
type LLVMFP128TypeDelegate = delegate of unit -> LLVMTypeRef
type LLVMPPCFP128TypeDelegate = delegate of unit -> LLVMTypeRef
type LLVMVoidTypeDelegate = delegate of unit -> LLVMTypeRef
type LLVMLabelTypeDelegate = delegate of unit -> LLVMTypeRef
type LLVMX86AMXTypeDelegate = delegate of unit -> LLVMTypeRef

// Value and Constant Functions
type LLVMTypeOfDelegate = delegate of LLVMValueRef -> LLVMTypeRef
type LLVMDumpValueDelegate = delegate of LLVMValueRef -> unit
type LLVMPrintValueToStringDelegate = delegate of LLVMValueRef -> nativeint
type LLVMGetValueName2Delegate = delegate of LLVMValueRef * nativeint -> nativeint
type LLVMSetValueName2Delegate = delegate of LLVMValueRef * string * uint32 -> unit
type LLVMGetValueContextDelegate = delegate of LLVMValueRef -> LLVMContextRef
type LLVMReplaceAllUsesWithDelegate = delegate of LLVMValueRef * LLVMValueRef -> unit
type LLVMIsConstantDelegate = delegate of LLVMValueRef -> LLVMBool
type LLVMIsUndefDelegate = delegate of LLVMValueRef -> LLVMBool
type LLVMIsPoisonDelegate = delegate of LLVMValueRef -> LLVMBool

// Constants
type LLVMConstNullDelegate = delegate of LLVMTypeRef -> LLVMValueRef
type LLVMConstAllOnesDelegate = delegate of LLVMTypeRef -> LLVMValueRef
type LLVMGetUndefDelegate = delegate of LLVMTypeRef -> LLVMValueRef
type LLVMGetPoisonDelegate = delegate of LLVMTypeRef -> LLVMValueRef
type LLVMIsNullDelegate = delegate of LLVMValueRef -> LLVMBool
type LLVMConstPointerNullDelegate = delegate of LLVMTypeRef -> LLVMValueRef

// Scalar constants
type LLVMConstIntDelegate = delegate of LLVMTypeRef * uint64 * LLVMBool -> LLVMValueRef
type LLVMConstIntOfArbitraryPrecisionDelegate = delegate of LLVMTypeRef * uint32 * nativeint -> LLVMValueRef
type LLVMConstIntOfStringDelegate = delegate of LLVMTypeRef * string * uint8 -> LLVMValueRef
type LLVMConstIntOfStringAndSizeDelegate = delegate of LLVMTypeRef * string * uint32 * uint8 -> LLVMValueRef
type LLVMConstRealDelegate = delegate of LLVMTypeRef * double -> LLVMValueRef
type LLVMConstRealOfStringDelegate = delegate of LLVMTypeRef * string -> LLVMValueRef
type LLVMConstRealOfStringAndSizeDelegate = delegate of LLVMTypeRef * string * uint32 -> LLVMValueRef
type LLVMConstIntGetZExtValueDelegate = delegate of LLVMValueRef -> uint64
type LLVMConstIntGetSExtValueDelegate = delegate of LLVMValueRef -> int64
type LLVMConstRealGetDoubleDelegate = delegate of LLVMValueRef * nativeint -> double

// Composite constants
type LLVMConstStringInContextDelegate = delegate of LLVMContextRef * string * uint32 * LLVMBool -> LLVMValueRef
type LLVMConstStringInContext2Delegate = delegate of LLVMContextRef * string * uint32 * LLVMBool -> LLVMValueRef
type LLVMConstStringDelegate = delegate of string * uint32 * LLVMBool -> LLVMValueRef
type LLVMIsConstantStringDelegate = delegate of LLVMValueRef -> LLVMBool
type LLVMGetAsStringDelegate = delegate of LLVMValueRef * nativeint -> nativeint
type LLVMConstStructInContextDelegate = delegate of LLVMContextRef * nativeint * uint32 * LLVMBool -> LLVMValueRef
type LLVMConstStructDelegate = delegate of nativeint * uint32 * LLVMBool -> LLVMValueRef
type LLVMConstNamedStructDelegate = delegate of LLVMTypeRef * nativeint * uint32 -> LLVMValueRef
type LLVMGetAggregateElementDelegate = delegate of LLVMValueRef * uint32 -> LLVMValueRef
type LLVMConstArrayDelegate = delegate of LLVMTypeRef * nativeint * uint32 -> LLVMValueRef
type LLVMConstArray2Delegate = delegate of LLVMTypeRef * nativeint * uint64 -> LLVMValueRef
type LLVMConstVectorDelegate = delegate of nativeint * uint32 -> LLVMValueRef

// Global Values
type LLVMGetGlobalParentDelegate = delegate of LLVMValueRef -> LLVMModuleRef
type LLVMIsDeclarationDelegate = delegate of LLVMValueRef -> LLVMBool
type LLVMGetLinkageDelegate = delegate of LLVMValueRef -> LLVMLinkage
type LLVMSetLinkageDelegate = delegate of LLVMValueRef * LLVMLinkage -> unit
type LLVMGetSectionDelegate = delegate of LLVMValueRef -> nativeint
type LLVMSetSectionDelegate = delegate of LLVMValueRef * string -> unit
type LLVMGetVisibilityDelegate = delegate of LLVMValueRef -> LLVMVisibility
type LLVMSetVisibilityDelegate = delegate of LLVMValueRef * LLVMVisibility -> unit
type LLVMGlobalGetValueTypeDelegate = delegate of LLVMValueRef -> LLVMTypeRef
type LLVMGetAlignmentDelegate = delegate of LLVMValueRef -> uint32
type LLVMSetAlignmentDelegate = delegate of LLVMValueRef * uint32 -> unit

// Global variables
type LLVMAddGlobalDelegate = delegate of LLVMModuleRef * LLVMTypeRef * string -> LLVMValueRef
type LLVMAddGlobalInAddressSpaceDelegate = delegate of LLVMModuleRef * LLVMTypeRef * string * uint32 -> LLVMValueRef
type LLVMGetNamedGlobalDelegate = delegate of LLVMModuleRef * string -> LLVMValueRef
type LLVMGetFirstGlobalDelegate = delegate of LLVMModuleRef -> LLVMValueRef
type LLVMGetLastGlobalDelegate = delegate of LLVMModuleRef -> LLVMValueRef
type LLVMGetNextGlobalDelegate = delegate of LLVMValueRef -> LLVMValueRef
type LLVMGetPreviousGlobalDelegate = delegate of LLVMValueRef -> LLVMValueRef
type LLVMDeleteGlobalDelegate = delegate of LLVMValueRef -> unit
type LLVMGetInitializerDelegate = delegate of LLVMValueRef -> LLVMValueRef
type LLVMSetInitializerDelegate = delegate of LLVMValueRef * LLVMValueRef -> unit
type LLVMIsThreadLocalDelegate = delegate of LLVMValueRef -> LLVMBool
type LLVMSetThreadLocalDelegate = delegate of LLVMValueRef * LLVMBool -> unit
type LLVMIsGlobalConstantDelegate = delegate of LLVMValueRef -> LLVMBool
type LLVMSetGlobalConstantDelegate = delegate of LLVMValueRef * LLVMBool -> unit

// Functions
type LLVMAddFunctionDelegate = delegate of LLVMModuleRef * string * LLVMTypeRef -> LLVMValueRef
type LLVMGetNamedFunctionDelegate = delegate of LLVMModuleRef * string -> LLVMValueRef
type LLVMGetFirstFunctionDelegate = delegate of LLVMModuleRef -> LLVMValueRef
type LLVMGetLastFunctionDelegate = delegate of LLVMModuleRef -> LLVMValueRef
type LLVMGetNextFunctionDelegate = delegate of LLVMValueRef -> LLVMValueRef
type LLVMGetPreviousFunctionDelegate = delegate of LLVMValueRef -> LLVMValueRef
type LLVMDeleteFunctionDelegate = delegate of LLVMValueRef -> unit
type LLVMHasPersonalityFnDelegate = delegate of LLVMValueRef -> LLVMBool
type LLVMGetPersonalityFnDelegate = delegate of LLVMValueRef -> LLVMValueRef
type LLVMSetPersonalityFnDelegate = delegate of LLVMValueRef * LLVMValueRef -> unit
type LLVMGetIntrinsicIDDelegate = delegate of LLVMValueRef -> uint32
type LLVMGetFunctionCallConvDelegate = delegate of LLVMValueRef -> uint32
type LLVMSetFunctionCallConvDelegate = delegate of LLVMValueRef * uint32 -> unit
type LLVMGetGCDelegate = delegate of LLVMValueRef -> nativeint
type LLVMSetGCDelegate = delegate of LLVMValueRef * string -> unit
type LLVMCountParamsDelegate = delegate of LLVMValueRef -> uint32
type LLVMGetParamsDelegate = delegate of LLVMValueRef * nativeint -> unit
type LLVMGetParamDelegate = delegate of LLVMValueRef * uint32 -> LLVMValueRef
type LLVMGetParamParentDelegate = delegate of LLVMValueRef -> LLVMValueRef
type LLVMGetFirstParamDelegate = delegate of LLVMValueRef -> LLVMValueRef
type LLVMGetLastParamDelegate = delegate of LLVMValueRef -> LLVMValueRef
type LLVMGetNextParamDelegate = delegate of LLVMValueRef -> LLVMValueRef
type LLVMGetPreviousParamDelegate = delegate of LLVMValueRef -> LLVMValueRef
type LLVMSetParamAlignmentDelegate = delegate of LLVMValueRef * uint32 -> unit

// Basic Block Functions
type LLVMBasicBlockAsValueDelegate = delegate of LLVMBasicBlockRef -> LLVMValueRef
type LLVMValueIsBasicBlockDelegate = delegate of LLVMValueRef -> LLVMBool
type LLVMValueAsBasicBlockDelegate = delegate of LLVMValueRef -> LLVMBasicBlockRef
type LLVMGetBasicBlockNameDelegate = delegate of LLVMBasicBlockRef -> nativeint
type LLVMGetBasicBlockParentDelegate = delegate of LLVMBasicBlockRef -> LLVMValueRef
type LLVMGetBasicBlockTerminatorDelegate = delegate of LLVMBasicBlockRef -> LLVMValueRef
type LLVMCountBasicBlocksDelegate = delegate of LLVMValueRef -> uint32
type LLVMGetBasicBlocksDelegate = delegate of LLVMValueRef * nativeint -> unit
type LLVMGetFirstBasicBlockDelegate = delegate of LLVMValueRef -> LLVMBasicBlockRef
type LLVMGetLastBasicBlockDelegate = delegate of LLVMValueRef -> LLVMBasicBlockRef
type LLVMGetNextBasicBlockDelegate = delegate of LLVMBasicBlockRef -> LLVMBasicBlockRef
type LLVMGetPreviousBasicBlockDelegate = delegate of LLVMBasicBlockRef -> LLVMBasicBlockRef
type LLVMGetEntryBasicBlockDelegate = delegate of LLVMValueRef -> LLVMBasicBlockRef
type LLVMInsertExistingBasicBlockAfterInsertBlockDelegate = delegate of LLVMBuilderRef * LLVMBasicBlockRef -> unit
type LLVMAppendExistingBasicBlockDelegate = delegate of LLVMValueRef * LLVMBasicBlockRef -> unit
type LLVMCreateBasicBlockInContextDelegate = delegate of LLVMContextRef * string -> LLVMBasicBlockRef
type LLVMAppendBasicBlockInContextDelegate = delegate of LLVMContextRef * LLVMValueRef * string -> LLVMBasicBlockRef
type LLVMAppendBasicBlockDelegate = delegate of LLVMValueRef * string -> LLVMBasicBlockRef
type LLVMInsertBasicBlockInContextDelegate = delegate of LLVMContextRef * LLVMBasicBlockRef * string -> LLVMBasicBlockRef
type LLVMInsertBasicBlockDelegate = delegate of LLVMBasicBlockRef * string -> LLVMBasicBlockRef
type LLVMDeleteBasicBlockDelegate = delegate of LLVMBasicBlockRef -> unit
type LLVMRemoveBasicBlockFromParentDelegate = delegate of LLVMBasicBlockRef -> unit
type LLVMMoveBasicBlockBeforeDelegate = delegate of LLVMBasicBlockRef * LLVMBasicBlockRef -> unit
type LLVMMoveBasicBlockAfterDelegate = delegate of LLVMBasicBlockRef * LLVMBasicBlockRef -> unit
type LLVMGetFirstInstructionDelegate = delegate of LLVMBasicBlockRef -> LLVMValueRef
type LLVMGetLastInstructionDelegate = delegate of LLVMBasicBlockRef -> LLVMValueRef

// Instruction Builder Functions
type LLVMCreateBuilderInContextDelegate = delegate of LLVMContextRef -> LLVMBuilderRef
type LLVMCreateBuilderDelegate = delegate of unit -> LLVMBuilderRef
type LLVMPositionBuilderDelegate = delegate of LLVMBuilderRef * LLVMBasicBlockRef * LLVMValueRef -> unit
type LLVMPositionBuilderBeforeDelegate = delegate of LLVMBuilderRef * LLVMValueRef -> unit
type LLVMPositionBuilderAtEndDelegate = delegate of LLVMBuilderRef * LLVMBasicBlockRef -> unit
type LLVMGetInsertBlockDelegate = delegate of LLVMBuilderRef -> LLVMBasicBlockRef
type LLVMClearInsertionPositionDelegate = delegate of LLVMBuilderRef -> unit
type LLVMInsertIntoBuilderDelegate = delegate of LLVMBuilderRef * LLVMValueRef -> unit
type LLVMInsertIntoBuilderWithNameDelegate = delegate of LLVMBuilderRef * LLVMValueRef * string -> unit
type LLVMDisposeBuilderDelegate = delegate of LLVMBuilderRef -> unit
type LLVMGetBuilderContextDelegate = delegate of LLVMBuilderRef -> LLVMContextRef

// Metadata
type LLVMGetCurrentDebugLocation2Delegate = delegate of LLVMBuilderRef -> LLVMMetadataRef
type LLVMSetCurrentDebugLocation2Delegate = delegate of LLVMBuilderRef * LLVMMetadataRef -> unit
type LLVMAddMetadataToInstDelegate = delegate of LLVMBuilderRef * LLVMValueRef -> unit

// Terminator Instructions
type LLVMBuildRetVoidDelegate = delegate of LLVMBuilderRef -> LLVMValueRef
type LLVMBuildRetDelegate = delegate of LLVMBuilderRef * LLVMValueRef -> LLVMValueRef
type LLVMBuildAggregateRetDelegate = delegate of LLVMBuilderRef * nativeint * uint32 -> LLVMValueRef
type LLVMBuildBrDelegate = delegate of LLVMBuilderRef * LLVMBasicBlockRef -> LLVMValueRef
type LLVMBuildCondBrDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMBasicBlockRef * LLVMBasicBlockRef -> LLVMValueRef
type LLVMBuildSwitchDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMBasicBlockRef * uint32 -> LLVMValueRef
type LLVMBuildIndirectBrDelegate = delegate of LLVMBuilderRef * LLVMValueRef * uint32 -> LLVMValueRef
type LLVMBuildInvoke2Delegate = delegate of LLVMBuilderRef * LLVMTypeRef * LLVMValueRef * nativeint * uint32 * LLVMBasicBlockRef * LLVMBasicBlockRef * string -> LLVMValueRef
type LLVMBuildUnreachableDelegate = delegate of LLVMBuilderRef -> LLVMValueRef

// Exception Handling
type LLVMBuildResumeDelegate = delegate of LLVMBuilderRef * LLVMValueRef -> LLVMValueRef
type LLVMBuildLandingPadDelegate = delegate of LLVMBuilderRef * LLVMTypeRef * LLVMValueRef * uint32 * string -> LLVMValueRef
type LLVMBuildCleanupRetDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMBasicBlockRef -> LLVMValueRef
type LLVMBuildCatchRetDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMBasicBlockRef -> LLVMValueRef
type LLVMBuildCatchPadDelegate = delegate of LLVMBuilderRef * LLVMValueRef * nativeint * uint32 * string -> LLVMValueRef
type LLVMBuildCleanupPadDelegate = delegate of LLVMBuilderRef * LLVMValueRef * nativeint * uint32 * string -> LLVMValueRef
type LLVMBuildCatchSwitchDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMBasicBlockRef * uint32 * string -> LLVMValueRef

// Control flow helpers
type LLVMAddCaseDelegate = delegate of LLVMValueRef * LLVMValueRef * LLVMBasicBlockRef -> unit
type LLVMAddDestinationDelegate = delegate of LLVMValueRef * LLVMBasicBlockRef -> unit

// Arithmetic Instructions
type LLVMBuildAddDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMValueRef * string -> LLVMValueRef
type LLVMBuildNSWAddDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMValueRef * string -> LLVMValueRef
type LLVMBuildNUWAddDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMValueRef * string -> LLVMValueRef
type LLVMBuildFAddDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMValueRef * string -> LLVMValueRef
type LLVMBuildSubDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMValueRef * string -> LLVMValueRef
type LLVMBuildNSWSubDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMValueRef * string -> LLVMValueRef
type LLVMBuildNUWSubDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMValueRef * string -> LLVMValueRef
type LLVMBuildFSubDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMValueRef * string -> LLVMValueRef
type LLVMBuildMulDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMValueRef * string -> LLVMValueRef
type LLVMBuildNSWMulDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMValueRef * string -> LLVMValueRef
type LLVMBuildNUWMulDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMValueRef * string -> LLVMValueRef
type LLVMBuildFMulDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMValueRef * string -> LLVMValueRef
type LLVMBuildUDivDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMValueRef * string -> LLVMValueRef
type LLVMBuildExactUDivDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMValueRef * string -> LLVMValueRef
type LLVMBuildSDivDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMValueRef * string -> LLVMValueRef
type LLVMBuildExactSDivDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMValueRef * string -> LLVMValueRef
type LLVMBuildFDivDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMValueRef * string -> LLVMValueRef
type LLVMBuildURemDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMValueRef * string -> LLVMValueRef
type LLVMBuildSRemDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMValueRef * string -> LLVMValueRef
type LLVMBuildFRemDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMValueRef * string -> LLVMValueRef
type LLVMBuildShlDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMValueRef * string -> LLVMValueRef
type LLVMBuildLShrDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMValueRef * string -> LLVMValueRef
type LLVMBuildAShrDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMValueRef * string -> LLVMValueRef
type LLVMBuildAndDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMValueRef * string -> LLVMValueRef
type LLVMBuildOrDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMValueRef * string -> LLVMValueRef
type LLVMBuildXorDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMValueRef * string -> LLVMValueRef
type LLVMBuildBinOpDelegate = delegate of LLVMBuilderRef * LLVMOpcode * LLVMValueRef * LLVMValueRef * string -> LLVMValueRef
type LLVMBuildNegDelegate = delegate of LLVMBuilderRef * LLVMValueRef * string -> LLVMValueRef
type LLVMBuildNSWNegDelegate = delegate of LLVMBuilderRef * LLVMValueRef * string -> LLVMValueRef
type LLVMBuildFNegDelegate = delegate of LLVMBuilderRef * LLVMValueRef * string -> LLVMValueRef
type LLVMBuildNotDelegate = delegate of LLVMBuilderRef * LLVMValueRef * string -> LLVMValueRef

// Arithmetic flags
type LLVMGetNUWDelegate = delegate of LLVMValueRef -> LLVMBool
type LLVMSetNUWDelegate = delegate of LLVMValueRef * LLVMBool -> unit
type LLVMGetNSWDelegate = delegate of LLVMValueRef -> LLVMBool
type LLVMSetNSWDelegate = delegate of LLVMValueRef * LLVMBool -> unit
type LLVMGetExactDelegate = delegate of LLVMValueRef -> LLVMBool
type LLVMSetExactDelegate = delegate of LLVMValueRef * LLVMBool -> unit

// Memory Instructions
type LLVMBuildAllocaDelegate = delegate of LLVMBuilderRef * LLVMTypeRef * string -> LLVMValueRef
type LLVMBuildArrayAllocaDelegate = delegate of LLVMBuilderRef * LLVMTypeRef * LLVMValueRef * string -> LLVMValueRef
type LLVMBuildLoad2Delegate = delegate of LLVMBuilderRef * LLVMTypeRef * LLVMValueRef * string -> LLVMValueRef
type LLVMBuildStoreDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMValueRef -> LLVMValueRef
type LLVMBuildGEP2Delegate = delegate of LLVMBuilderRef * LLVMTypeRef * LLVMValueRef * nativeint * uint32 * string -> LLVMValueRef
type LLVMBuildInBoundsGEP2Delegate = delegate of LLVMBuilderRef * LLVMTypeRef * LLVMValueRef * nativeint * uint32 * string -> LLVMValueRef
type LLVMBuildStructGEP2Delegate = delegate of LLVMBuilderRef * LLVMTypeRef * LLVMValueRef * uint32 * string -> LLVMValueRef
type LLVMBuildGlobalStringDelegate = delegate of LLVMBuilderRef * string * string -> LLVMValueRef
type LLVMBuildGlobalStringPtrDelegate = delegate of LLVMBuilderRef * string * string -> LLVMValueRef
type LLVMGetVolatileDelegate = delegate of LLVMValueRef -> LLVMBool
type LLVMSetVolatileDelegate = delegate of LLVMValueRef * LLVMBool -> unit
type LLVMGetOrderingDelegate = delegate of LLVMValueRef -> LLVMAtomicOrdering
type LLVMSetOrderingDelegate = delegate of LLVMValueRef * LLVMAtomicOrdering -> unit

// Cast Instructions
type LLVMBuildTruncDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMTypeRef * string -> LLVMValueRef
type LLVMBuildZExtDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMTypeRef * string -> LLVMValueRef
type LLVMBuildSExtDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMTypeRef * string -> LLVMValueRef
type LLVMBuildFPToUIDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMTypeRef * string -> LLVMValueRef
type LLVMBuildFPToSIDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMTypeRef * string -> LLVMValueRef
type LLVMBuildUIToFPDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMTypeRef * string -> LLVMValueRef
type LLVMBuildSIToFPDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMTypeRef * string -> LLVMValueRef
type LLVMBuildFPTruncDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMTypeRef * string -> LLVMValueRef
type LLVMBuildFPExtDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMTypeRef * string -> LLVMValueRef
type LLVMBuildPtrToIntDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMTypeRef * string -> LLVMValueRef
type LLVMBuildIntToPtrDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMTypeRef * string -> LLVMValueRef
type LLVMBuildBitCastDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMTypeRef * string -> LLVMValueRef
type LLVMBuildAddrSpaceCastDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMTypeRef * string -> LLVMValueRef
type LLVMBuildPointerCastDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMTypeRef * string -> LLVMValueRef
type LLVMBuildIntCast2Delegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMTypeRef * LLVMBool * string -> LLVMValueRef
type LLVMBuildFPCastDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMTypeRef * string -> LLVMValueRef
type LLVMBuildCastDelegate = delegate of LLVMBuilderRef * LLVMOpcode * LLVMValueRef * LLVMTypeRef * string -> LLVMValueRef
type LLVMGetCastOpcodeDelegate = delegate of LLVMValueRef * LLVMBool * LLVMTypeRef * LLVMBool -> LLVMOpcode

// Comparison Instructions
type LLVMBuildICmpDelegate = delegate of LLVMBuilderRef * LLVMIntPredicate * LLVMValueRef * LLVMValueRef * string -> LLVMValueRef
type LLVMBuildFCmpDelegate = delegate of LLVMBuilderRef * LLVMRealPredicate * LLVMValueRef * LLVMValueRef * string -> LLVMValueRef

// Other Instructions
type LLVMBuildPhiDelegate = delegate of LLVMBuilderRef * LLVMTypeRef * string -> LLVMValueRef
type LLVMBuildCall2Delegate = delegate of LLVMBuilderRef * LLVMTypeRef * LLVMValueRef * nativeint * uint32 * string -> LLVMValueRef
type LLVMBuildSelectDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMValueRef * LLVMValueRef * string -> LLVMValueRef
type LLVMBuildVAArgDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMTypeRef * string -> LLVMValueRef
type LLVMBuildExtractElementDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMValueRef * string -> LLVMValueRef
type LLVMBuildInsertElementDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMValueRef * LLVMValueRef * string -> LLVMValueRef
type LLVMBuildShuffleVectorDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMValueRef * LLVMValueRef * string -> LLVMValueRef
type LLVMBuildExtractValueDelegate = delegate of LLVMBuilderRef * LLVMValueRef * uint32 * string -> LLVMValueRef
type LLVMBuildInsertValueDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMValueRef * uint32 * string -> LLVMValueRef
type LLVMBuildFreezeDelegate = delegate of LLVMBuilderRef * LLVMValueRef * string -> LLVMValueRef
type LLVMBuildIsNullDelegate = delegate of LLVMBuilderRef * LLVMValueRef * string -> LLVMValueRef
type LLVMBuildIsNotNullDelegate = delegate of LLVMBuilderRef * LLVMValueRef * string -> LLVMValueRef
type LLVMBuildPtrDiff2Delegate = delegate of LLVMBuilderRef * LLVMTypeRef * LLVMValueRef * LLVMValueRef * string -> LLVMValueRef
type LLVMBuildFenceDelegate = delegate of LLVMBuilderRef * LLVMAtomicOrdering * LLVMBool * string -> LLVMValueRef
type LLVMBuildAtomicRMWDelegate = delegate of LLVMBuilderRef * LLVMAtomicRMWBinOp * LLVMValueRef * LLVMValueRef * LLVMAtomicOrdering * LLVMBool -> LLVMValueRef
type LLVMBuildAtomicCmpXchgDelegate = delegate of LLVMBuilderRef * LLVMValueRef * LLVMValueRef * LLVMValueRef * LLVMAtomicOrdering * LLVMAtomicOrdering * LLVMBool -> LLVMValueRef

// Atomic helpers
type LLVMIsAtomicSingleThreadDelegate = delegate of LLVMValueRef -> LLVMBool
type LLVMSetAtomicSingleThreadDelegate = delegate of LLVMValueRef * LLVMBool -> unit

// PHI Node Functions
type LLVMAddIncomingDelegate = delegate of LLVMValueRef * nativeint * nativeint * uint32 -> unit
type LLVMCountIncomingDelegate = delegate of LLVMValueRef -> uint32
type LLVMGetIncomingValueDelegate = delegate of LLVMValueRef * uint32 -> LLVMValueRef
type LLVMGetIncomingBlockDelegate = delegate of LLVMValueRef * uint32 -> LLVMBasicBlockRef

// Instruction Functions
type LLVMHasMetadataDelegate = delegate of LLVMValueRef -> int
type LLVMGetMetadataDelegate = delegate of LLVMValueRef * uint32 -> LLVMValueRef
type LLVMSetMetadataDelegate = delegate of LLVMValueRef * uint32 * LLVMValueRef -> unit
type LLVMGetInstructionParentDelegate = delegate of LLVMValueRef -> LLVMBasicBlockRef
type LLVMGetNextInstructionDelegate = delegate of LLVMValueRef -> LLVMValueRef
type LLVMGetPreviousInstructionDelegate = delegate of LLVMValueRef -> LLVMValueRef
type LLVMInstructionRemoveFromParentDelegate = delegate of LLVMValueRef -> unit
type LLVMInstructionEraseFromParentDelegate = delegate of LLVMValueRef -> unit
type LLVMDeleteInstructionDelegate = delegate of LLVMValueRef -> unit
type LLVMGetInstructionOpcodeDelegate = delegate of LLVMValueRef -> LLVMOpcode
type LLVMGetICmpPredicateDelegate = delegate of LLVMValueRef -> LLVMIntPredicate
type LLVMGetFCmpPredicateDelegate = delegate of LLVMValueRef -> LLVMRealPredicate
type LLVMInstructionCloneDelegate = delegate of LLVMValueRef -> LLVMValueRef

// Call site functions
type LLVMGetNumArgOperandsDelegate = delegate of LLVMValueRef -> uint32
type LLVMSetInstructionCallConvDelegate = delegate of LLVMValueRef * uint32 -> unit
type LLVMGetInstructionCallConvDelegate = delegate of LLVMValueRef -> uint32
type LLVMGetCalledFunctionTypeDelegate = delegate of LLVMValueRef -> LLVMTypeRef
type LLVMGetCalledValueDelegate = delegate of LLVMValueRef -> LLVMValueRef
type LLVMIsTailCallDelegate = delegate of LLVMValueRef -> LLVMBool
type LLVMSetTailCallDelegate = delegate of LLVMValueRef * LLVMBool -> unit

// Terminator functions
type LLVMGetNumSuccessorsDelegate = delegate of LLVMValueRef -> uint32
type LLVMGetSuccessorDelegate = delegate of LLVMValueRef * uint32 -> LLVMBasicBlockRef
type LLVMSetSuccessorDelegate = delegate of LLVMValueRef * uint32 * LLVMBasicBlockRef -> unit
type LLVMIsConditionalDelegate = delegate of LLVMValueRef -> LLVMBool
type LLVMGetConditionDelegate = delegate of LLVMValueRef -> LLVMValueRef
type LLVMSetConditionDelegate = delegate of LLVMValueRef * LLVMValueRef -> unit
type LLVMGetSwitchDefaultDestDelegate = delegate of LLVMValueRef -> LLVMBasicBlockRef

// Alloca functions
type LLVMGetAllocatedTypeDelegate = delegate of LLVMValueRef -> LLVMTypeRef

// GEP functions
type LLVMIsInBoundsDelegate = delegate of LLVMValueRef -> LLVMBool
type LLVMSetIsInBoundsDelegate = delegate of LLVMValueRef * LLVMBool -> unit
type LLVMGetGEPSourceElementTypeDelegate = delegate of LLVMValueRef -> LLVMTypeRef

// Extract/Insert value functions
type LLVMGetNumIndicesDelegate = delegate of LLVMValueRef -> uint32
type LLVMGetIndicesDelegate = delegate of LLVMValueRef -> nativeint

// Pass Manager Functions
type LLVMCreatePassManagerDelegate = delegate of unit -> LLVMPassManagerRef
type LLVMCreateFunctionPassManagerForModuleDelegate = delegate of LLVMModuleRef -> LLVMPassManagerRef
type LLVMRunPassManagerDelegate = delegate of LLVMPassManagerRef * LLVMModuleRef -> LLVMBool
type LLVMInitializeFunctionPassManagerDelegate = delegate of LLVMPassManagerRef -> LLVMBool
type LLVMRunFunctionPassManagerDelegate = delegate of LLVMPassManagerRef * LLVMValueRef -> LLVMBool
type LLVMFinalizeFunctionPassManagerDelegate = delegate of LLVMPassManagerRef -> LLVMBool
type LLVMDisposePassManagerDelegate = delegate of LLVMPassManagerRef -> unit

// Optimization Passes
type LLVMAddInstructionCombiningPassDelegate = delegate of LLVMPassManagerRef -> unit
type LLVMAddPromoteMemoryToRegisterPassDelegate = delegate of LLVMPassManagerRef -> unit
type LLVMAddGVNPassDelegate = delegate of LLVMPassManagerRef -> unit
type LLVMAddCFGSimplificationPassDelegate = delegate of LLVMPassManagerRef -> unit

// Target Functions
type LLVMGetDefaultTargetTripleDelegate = delegate of unit -> nativeint
type LLVMGetHostCPUNameDelegate = delegate of unit -> nativeint
type LLVMGetHostCPUFeaturesDelegate = delegate of unit -> nativeint
type LLVMInitializeNativeTargetDelegate = delegate of unit -> LLVMBool
type LLVMInitializeNativeAsmPrinterDelegate = delegate of unit -> LLVMBool
type LLVMGetTargetFromTripleDelegate = delegate of string * nativeint * nativeint -> LLVMBool
type LLVMGetTargetFromNameDelegate = delegate of string -> LLVMTargetRef
type LLVMGetTargetNameDelegate = delegate of LLVMTargetRef -> nativeint
type LLVMGetTargetDescriptionDelegate = delegate of LLVMTargetRef -> nativeint
type LLVMTargetHasJITDelegate = delegate of LLVMTargetRef -> LLVMBool
type LLVMTargetHasTargetMachineDelegate = delegate of LLVMTargetRef -> LLVMBool
type LLVMTargetHasAsmBackendDelegate = delegate of LLVMTargetRef -> LLVMBool
type LLVMCreateTargetMachineDelegate = delegate of LLVMTargetRef * string * string * string * LLVMCodeGenOptLevel * LLVMRelocMode * LLVMCodeModel -> LLVMTargetMachineRef
type LLVMDisposeTargetMachineDelegate = delegate of LLVMTargetMachineRef -> unit
type LLVMGetTargetMachineTargetDelegate = delegate of LLVMTargetMachineRef -> LLVMTargetRef
type LLVMGetTargetMachineTripleDelegate = delegate of LLVMTargetMachineRef -> nativeint
type LLVMGetTargetMachineCPUDelegate = delegate of LLVMTargetMachineRef -> nativeint
type LLVMGetTargetMachineFeatureStringDelegate = delegate of LLVMTargetMachineRef -> nativeint
type LLVMCreateTargetDataLayoutDelegate = delegate of LLVMTargetMachineRef -> LLVMTargetDataRef
type LLVMTargetMachineEmitToFileDelegate = delegate of LLVMTargetMachineRef * LLVMModuleRef * string * LLVMCodeGenFileType * nativeint -> LLVMBool
type LLVMTargetMachineEmitToMemoryBufferDelegate = delegate of LLVMTargetMachineRef * LLVMModuleRef * LLVMCodeGenFileType * nativeint * nativeint -> LLVMBool

// Target Data Functions
type LLVMGetModuleDataLayoutDelegate = delegate of LLVMModuleRef -> LLVMTargetDataRef
type LLVMSetModuleDataLayoutDelegate = delegate of LLVMModuleRef * LLVMTargetDataRef -> unit
type LLVMCreateTargetDataDelegate = delegate of string -> LLVMTargetDataRef
type LLVMDisposeTargetDataDelegate = delegate of LLVMTargetDataRef -> unit
type LLVMCopyStringRepOfTargetDataDelegate = delegate of LLVMTargetDataRef -> nativeint
type LLVMPointerSizeDelegate = delegate of LLVMTargetDataRef -> uint32
type LLVMPointerSizeForASDelegate = delegate of LLVMTargetDataRef * uint32 -> uint32
type LLVMIntPtrTypeDelegate = delegate of LLVMTargetDataRef -> LLVMTypeRef
type LLVMIntPtrTypeForASDelegate = delegate of LLVMTargetDataRef * uint32 -> LLVMTypeRef
type LLVMIntPtrTypeInContextDelegate = delegate of LLVMContextRef * LLVMTargetDataRef -> LLVMTypeRef
type LLVMIntPtrTypeForASInContextDelegate = delegate of LLVMContextRef * LLVMTargetDataRef * uint32 -> LLVMTypeRef
type LLVMSizeOfTypeInBitsDelegate = delegate of LLVMTargetDataRef * LLVMTypeRef -> uint64
type LLVMStoreSizeOfTypeDelegate = delegate of LLVMTargetDataRef * LLVMTypeRef -> uint64
type LLVMABISizeOfTypeDelegate = delegate of LLVMTargetDataRef * LLVMTypeRef -> uint64
type LLVMABIAlignmentOfTypeDelegate = delegate of LLVMTargetDataRef * LLVMTypeRef -> uint32
type LLVMCallFrameAlignmentOfTypeDelegate = delegate of LLVMTargetDataRef * LLVMTypeRef -> uint32
type LLVMPreferredAlignmentOfTypeDelegate = delegate of LLVMTargetDataRef * LLVMTypeRef -> uint32
type LLVMPreferredAlignmentOfGlobalDelegate = delegate of LLVMTargetDataRef * LLVMValueRef -> uint32
type LLVMElementAtOffsetDelegate = delegate of LLVMTargetDataRef * LLVMTypeRef * uint64 -> uint32
type LLVMOffsetOfElementDelegate = delegate of LLVMTargetDataRef * LLVMTypeRef * uint32 -> uint64

// Initialization Functions
type LLVMInitializeAllTargetInfosDelegate = delegate of unit -> unit
type LLVMInitializeAllTargetsDelegate = delegate of unit -> unit
type LLVMInitializeAllTargetMCsDelegate = delegate of unit -> unit
type LLVMInitializeAllAsmPrintersDelegate = delegate of unit -> unit
type LLVMInitializeAllAsmParsersDelegate = delegate of unit -> unit
type LLVMInitializeAllDisassemblersDelegate = delegate of unit -> unit

// Memory Management Functions
type LLVMDisposeMessageDelegate = delegate of nativeint -> unit
type LLVMShutdownDelegate = delegate of unit -> unit

// Version Functions
type LLVMGetVersionDelegate = delegate of nativeint * nativeint * nativeint -> unit

// Error Handling
type LLVMCreateMessageDelegate = delegate of string -> nativeint

// New Pass Manager Functions (PassBuilder)
type LLVMRunPassesDelegate = delegate of LLVMModuleRef * string * LLVMTargetMachineRef * LLVMPassBuilderOptionsRef -> LLVMErrorRef
type LLVMRunPassesOnFunctionDelegate = delegate of LLVMValueRef * string * LLVMTargetMachineRef * LLVMPassBuilderOptionsRef -> LLVMErrorRef
type LLVMCreatePassBuilderOptionsDelegate = delegate of unit -> LLVMPassBuilderOptionsRef
type LLVMPassBuilderOptionsSetVerifyEachDelegate = delegate of LLVMPassBuilderOptionsRef * LLVMBool -> unit
type LLVMPassBuilderOptionsSetDebugLoggingDelegate = delegate of LLVMPassBuilderOptionsRef * LLVMBool -> unit
type LLVMDisposePassBuilderOptionsDelegate = delegate of LLVMPassBuilderOptionsRef -> unit

// Memory Buffer Functions
type LLVMCreateMemoryBufferWithContentsOfFileDelegate = delegate of string * nativeint * nativeint -> LLVMBool
type LLVMCreateMemoryBufferWithSTDINDelegate = delegate of nativeint * nativeint -> LLVMBool
type LLVMCreateMemoryBufferWithMemoryRangeDelegate = delegate of string * uint32 * string * LLVMBool -> LLVMMemoryBufferRef
type LLVMCreateMemoryBufferWithMemoryRangeCopyDelegate = delegate of string * uint32 * string -> LLVMMemoryBufferRef
type LLVMGetBufferStartDelegate = delegate of LLVMMemoryBufferRef -> nativeint
type LLVMGetBufferSizeDelegate = delegate of LLVMMemoryBufferRef -> uint32
type LLVMDisposeMemoryBufferDelegate = delegate of LLVMMemoryBufferRef -> unit

// =============================================================================
// Lazy-loaded Function Instances
// =============================================================================

// Context Functions
let llvmContextCreate = lazy (NativeLibrary.getFunction<LLVMContextCreateDelegate> "LLVMContextCreate")
let llvmGetGlobalContext = lazy (NativeLibrary.getFunction<LLVMGetGlobalContextDelegate> "LLVMGetGlobalContext")
let llvmContextDispose = lazy (NativeLibrary.getFunction<LLVMContextDisposeDelegate> "LLVMContextDispose")
let llvmContextSetDiscardValueNames = lazy (NativeLibrary.getFunction<LLVMContextSetDiscardValueNamesDelegate> "LLVMContextSetDiscardValueNames")
let llvmContextShouldDiscardValueNames = lazy (NativeLibrary.getFunction<LLVMContextShouldDiscardValueNamesDelegate> "LLVMContextShouldDiscardValueNames")

// Module Functions
let llvmModuleCreateWithName = lazy (NativeLibrary.getFunction<LLVMModuleCreateWithNameDelegate> "LLVMModuleCreateWithName")
let llvmModuleCreateWithNameInContext = lazy (NativeLibrary.getFunction<LLVMModuleCreateWithNameInContextDelegate> "LLVMModuleCreateWithNameInContext")
let llvmCloneModule = lazy (NativeLibrary.getFunction<LLVMCloneModuleDelegate> "LLVMCloneModule")
let llvmDisposeModule = lazy (NativeLibrary.getFunction<LLVMDisposeModuleDelegate> "LLVMDisposeModule")
let llvmGetModuleIdentifier = lazy (NativeLibrary.getFunction<LLVMGetModuleIdentifierDelegate> "LLVMGetModuleIdentifier")
let llvmSetModuleIdentifier = lazy (NativeLibrary.getFunction<LLVMSetModuleIdentifierDelegate> "LLVMSetModuleIdentifier")
let llvmGetSourceFileName = lazy (NativeLibrary.getFunction<LLVMGetSourceFileNameDelegate> "LLVMGetSourceFileName")
let llvmSetSourceFileName = lazy (NativeLibrary.getFunction<LLVMSetSourceFileNameDelegate> "LLVMSetSourceFileName")
let llvmGetDataLayoutStr = lazy (NativeLibrary.getFunction<LLVMGetDataLayoutStrDelegate> "LLVMGetDataLayoutStr")
let llvmSetDataLayout = lazy (NativeLibrary.getFunction<LLVMSetDataLayoutDelegate> "LLVMSetDataLayout")
let llvmGetTarget = lazy (NativeLibrary.getFunction<LLVMGetTargetDelegate> "LLVMGetTarget")
let llvmSetTarget = lazy (NativeLibrary.getFunction<LLVMSetTargetDelegate> "LLVMSetTarget")
let llvmDumpModule = lazy (NativeLibrary.getFunction<LLVMDumpModuleDelegate> "LLVMDumpModule")
let llvmPrintModuleToFile = lazy (NativeLibrary.getFunction<LLVMPrintModuleToFileDelegate> "LLVMPrintModuleToFile")
let llvmPrintModuleToString = lazy (NativeLibrary.getFunction<LLVMPrintModuleToStringDelegate> "LLVMPrintModuleToString")
let llvmVerifyModule = lazy (NativeLibrary.getFunction<LLVMVerifyModuleDelegate> "LLVMVerifyModule")
let llvmGetModuleContext = lazy (NativeLibrary.getFunction<LLVMGetModuleContextDelegate> "LLVMGetModuleContext")

// Type Functions
let llvmGetTypeKind = lazy (NativeLibrary.getFunction<LLVMGetTypeKindDelegate> "LLVMGetTypeKind")
let llvmTypeIsSized = lazy (NativeLibrary.getFunction<LLVMTypeIsSizedDelegate> "LLVMTypeIsSized")
let llvmGetTypeContext = lazy (NativeLibrary.getFunction<LLVMGetTypeContextDelegate> "LLVMGetTypeContext")
let llvmDumpType = lazy (NativeLibrary.getFunction<LLVMDumpTypeDelegate> "LLVMDumpType")
let llvmPrintTypeToString = lazy (NativeLibrary.getFunction<LLVMPrintTypeToStringDelegate> "LLVMPrintTypeToString")

// Integer types
let llvmInt1TypeInContext = lazy (NativeLibrary.getFunction<LLVMInt1TypeInContextDelegate> "LLVMInt1TypeInContext")
let llvmInt8TypeInContext = lazy (NativeLibrary.getFunction<LLVMInt8TypeInContextDelegate> "LLVMInt8TypeInContext")
let llvmInt16TypeInContext = lazy (NativeLibrary.getFunction<LLVMInt16TypeInContextDelegate> "LLVMInt16TypeInContext")
let llvmInt32TypeInContext = lazy (NativeLibrary.getFunction<LLVMInt32TypeInContextDelegate> "LLVMInt32TypeInContext")
let llvmInt64TypeInContext = lazy (NativeLibrary.getFunction<LLVMInt64TypeInContextDelegate> "LLVMInt64TypeInContext")
let llvmInt128TypeInContext = lazy (NativeLibrary.getFunction<LLVMInt128TypeInContextDelegate> "LLVMInt128TypeInContext")
let llvmIntTypeInContext = lazy (NativeLibrary.getFunction<LLVMIntTypeInContextDelegate> "LLVMIntTypeInContext")
let llvmGetIntTypeWidth = lazy (NativeLibrary.getFunction<LLVMGetIntTypeWidthDelegate> "LLVMGetIntTypeWidth")

// Floating point types
let llvmHalfTypeInContext = lazy (NativeLibrary.getFunction<LLVMHalfTypeInContextDelegate> "LLVMHalfTypeInContext")
let llvmBFloatTypeInContext = lazy (NativeLibrary.getFunction<LLVMBFloatTypeInContextDelegate> "LLVMBFloatTypeInContext")
let llvmFloatTypeInContext = lazy (NativeLibrary.getFunction<LLVMFloatTypeInContextDelegate> "LLVMFloatTypeInContext")
let llvmDoubleTypeInContext = lazy (NativeLibrary.getFunction<LLVMDoubleTypeInContextDelegate> "LLVMDoubleTypeInContext")
let llvmX86FP80TypeInContext = lazy (NativeLibrary.getFunction<LLVMX86FP80TypeInContextDelegate> "LLVMX86FP80TypeInContext")
let llvmFP128TypeInContext = lazy (NativeLibrary.getFunction<LLVMFP128TypeInContextDelegate> "LLVMFP128TypeInContext")
let llvmPPCFP128TypeInContext = lazy (NativeLibrary.getFunction<LLVMPPCFP128TypeInContextDelegate> "LLVMPPCFP128TypeInContext")

// Other types
let llvmVoidTypeInContext = lazy (NativeLibrary.getFunction<LLVMVoidTypeInContextDelegate> "LLVMVoidTypeInContext")
let llvmLabelTypeInContext = lazy (NativeLibrary.getFunction<LLVMLabelTypeInContextDelegate> "LLVMLabelTypeInContext")
let llvmX86AMXTypeInContext = lazy (NativeLibrary.getFunction<LLVMX86AMXTypeInContextDelegate> "LLVMX86AMXTypeInContext")
let llvmTokenTypeInContext = lazy (NativeLibrary.getFunction<LLVMTokenTypeInContextDelegate> "LLVMTokenTypeInContext")
let llvmMetadataTypeInContext = lazy (NativeLibrary.getFunction<LLVMMetadataTypeInContextDelegate> "LLVMMetadataTypeInContext")

// Pointer types
let llvmPointerType = lazy (NativeLibrary.getFunction<LLVMPointerTypeDelegate> "LLVMPointerType")
let llvmPointerTypeInContext = lazy (NativeLibrary.getFunction<LLVMPointerTypeInContextDelegate> "LLVMPointerTypeInContext")
let llvmPointerTypeIsOpaque = lazy (NativeLibrary.getFunction<LLVMPointerTypeIsOpaqueDelegate> "LLVMPointerTypeIsOpaque")
let llvmGetPointerAddressSpace = lazy (NativeLibrary.getFunction<LLVMGetPointerAddressSpaceDelegate> "LLVMGetPointerAddressSpace")

// Function types
let llvmFunctionType = lazy (NativeLibrary.getFunction<LLVMFunctionTypeDelegate> "LLVMFunctionType")
let llvmIsFunctionVarArg = lazy (NativeLibrary.getFunction<LLVMIsFunctionVarArgDelegate> "LLVMIsFunctionVarArg")
let llvmGetReturnType = lazy (NativeLibrary.getFunction<LLVMGetReturnTypeDelegate> "LLVMGetReturnType")
let llvmCountParamTypes = lazy (NativeLibrary.getFunction<LLVMCountParamTypesDelegate> "LLVMCountParamTypes")
let llvmGetParamTypes = lazy (NativeLibrary.getFunction<LLVMGetParamTypesDelegate> "LLVMGetParamTypes")

// Array types
let llvmArrayType = lazy (NativeLibrary.getFunction<LLVMArrayTypeDelegate> "LLVMArrayType")
let llvmArrayType2 = lazy (NativeLibrary.getFunction<LLVMArrayType2Delegate> "LLVMArrayType2")
let llvmGetArrayLength = lazy (NativeLibrary.getFunction<LLVMGetArrayLengthDelegate> "LLVMGetArrayLength")
let llvmGetArrayLength2 = lazy (NativeLibrary.getFunction<LLVMGetArrayLength2Delegate> "LLVMGetArrayLength2")

// Vector types
let llvmVectorType = lazy (NativeLibrary.getFunction<LLVMVectorTypeDelegate> "LLVMVectorType")
let llvmScalableVectorType = lazy (NativeLibrary.getFunction<LLVMScalableVectorTypeDelegate> "LLVMScalableVectorType")
let llvmGetVectorSize = lazy (NativeLibrary.getFunction<LLVMGetVectorSizeDelegate> "LLVMGetVectorSize")

// Sequential types
let llvmGetElementType = lazy (NativeLibrary.getFunction<LLVMGetElementTypeDelegate> "LLVMGetElementType")
let llvmGetSubtypes = lazy (NativeLibrary.getFunction<LLVMGetSubtypesDelegate> "LLVMGetSubtypes")
let llvmGetNumContainedTypes = lazy (NativeLibrary.getFunction<LLVMGetNumContainedTypesDelegate> "LLVMGetNumContainedTypes")

// Struct types
let llvmStructTypeInContext = lazy (NativeLibrary.getFunction<LLVMStructTypeInContextDelegate> "LLVMStructTypeInContext")
let llvmStructType = lazy (NativeLibrary.getFunction<LLVMStructTypeDelegate> "LLVMStructType")
let llvmStructCreateNamed = lazy (NativeLibrary.getFunction<LLVMStructCreateNamedDelegate> "LLVMStructCreateNamed")
let llvmGetStructName = lazy (NativeLibrary.getFunction<LLVMGetStructNameDelegate> "LLVMGetStructName")
let llvmStructSetBody = lazy (NativeLibrary.getFunction<LLVMStructSetBodyDelegate> "LLVMStructSetBody")
let llvmCountStructElementTypes = lazy (NativeLibrary.getFunction<LLVMCountStructElementTypesDelegate> "LLVMCountStructElementTypes")
let llvmGetStructElementTypes = lazy (NativeLibrary.getFunction<LLVMGetStructElementTypesDelegate> "LLVMGetStructElementTypes")
let llvmStructGetTypeAtIndex = lazy (NativeLibrary.getFunction<LLVMStructGetTypeAtIndexDelegate> "LLVMStructGetTypeAtIndex")
let llvmIsPackedStruct = lazy (NativeLibrary.getFunction<LLVMIsPackedStructDelegate> "LLVMIsPackedStruct")
let llvmIsOpaqueStruct = lazy (NativeLibrary.getFunction<LLVMIsOpaqueStructDelegate> "LLVMIsOpaqueStruct")
let llvmIsLiteralStruct = lazy (NativeLibrary.getFunction<LLVMIsLiteralStructDelegate> "LLVMIsLiteralStruct")

// Global context versions
let llvmInt1Type = lazy (NativeLibrary.getFunction<LLVMInt1TypeDelegate> "LLVMInt1Type")
let llvmInt8Type = lazy (NativeLibrary.getFunction<LLVMInt8TypeDelegate> "LLVMInt8Type")
let llvmInt16Type = lazy (NativeLibrary.getFunction<LLVMInt16TypeDelegate> "LLVMInt16Type")
let llvmInt32Type = lazy (NativeLibrary.getFunction<LLVMInt32TypeDelegate> "LLVMInt32Type")
let llvmInt64Type = lazy (NativeLibrary.getFunction<LLVMInt64TypeDelegate> "LLVMInt64Type")
let llvmInt128Type = lazy (NativeLibrary.getFunction<LLVMInt128TypeDelegate> "LLVMInt128Type")
let llvmIntType = lazy (NativeLibrary.getFunction<LLVMIntTypeDelegate> "LLVMIntType")
let llvmHalfType = lazy (NativeLibrary.getFunction<LLVMHalfTypeDelegate> "LLVMHalfType")
let llvmBFloatType = lazy (NativeLibrary.getFunction<LLVMBFloatTypeDelegate> "LLVMBFloatType")
let llvmFloatType = lazy (NativeLibrary.getFunction<LLVMFloatTypeDelegate> "LLVMFloatType")
let llvmDoubleType = lazy (NativeLibrary.getFunction<LLVMDoubleTypeDelegate> "LLVMDoubleType")
let llvmX86FP80Type = lazy (NativeLibrary.getFunction<LLVMX86FP80TypeDelegate> "LLVMX86FP80Type")
let llvmFP128Type = lazy (NativeLibrary.getFunction<LLVMFP128TypeDelegate> "LLVMFP128Type")
let llvmPPCFP128Type = lazy (NativeLibrary.getFunction<LLVMPPCFP128TypeDelegate> "LLVMPPCFP128Type")
let llvmVoidType = lazy (NativeLibrary.getFunction<LLVMVoidTypeDelegate> "LLVMVoidType")
let llvmLabelType = lazy (NativeLibrary.getFunction<LLVMLabelTypeDelegate> "LLVMLabelType")
let llvmX86AMXType = lazy (NativeLibrary.getFunction<LLVMX86AMXTypeDelegate> "LLVMX86AMXType")

// Value and Constant Functions
let llvmTypeOf = lazy (NativeLibrary.getFunction<LLVMTypeOfDelegate> "LLVMTypeOf")
let llvmDumpValue = lazy (NativeLibrary.getFunction<LLVMDumpValueDelegate> "LLVMDumpValue")
let llvmPrintValueToString = lazy (NativeLibrary.getFunction<LLVMPrintValueToStringDelegate> "LLVMPrintValueToString")
let llvmGetValueName2 = lazy (NativeLibrary.getFunction<LLVMGetValueName2Delegate> "LLVMGetValueName2")
let llvmSetValueName2 = lazy (NativeLibrary.getFunction<LLVMSetValueName2Delegate> "LLVMSetValueName2")
let llvmGetValueContext = lazy (NativeLibrary.getFunction<LLVMGetValueContextDelegate> "LLVMGetValueContext")
let llvmReplaceAllUsesWith = lazy (NativeLibrary.getFunction<LLVMReplaceAllUsesWithDelegate> "LLVMReplaceAllUsesWith")
let llvmIsConstant = lazy (NativeLibrary.getFunction<LLVMIsConstantDelegate> "LLVMIsConstant")
let llvmIsUndef = lazy (NativeLibrary.getFunction<LLVMIsUndefDelegate> "LLVMIsUndef")
let llvmIsPoison = lazy (NativeLibrary.getFunction<LLVMIsPoisonDelegate> "LLVMIsPoison")

// Constants
let llvmConstNull = lazy (NativeLibrary.getFunction<LLVMConstNullDelegate> "LLVMConstNull")
let llvmConstAllOnes = lazy (NativeLibrary.getFunction<LLVMConstAllOnesDelegate> "LLVMConstAllOnes")
let llvmGetUndef = lazy (NativeLibrary.getFunction<LLVMGetUndefDelegate> "LLVMGetUndef")
let llvmGetPoison = lazy (NativeLibrary.getFunction<LLVMGetPoisonDelegate> "LLVMGetPoison")
let llvmIsNull = lazy (NativeLibrary.getFunction<LLVMIsNullDelegate> "LLVMIsNull")
let llvmConstPointerNull = lazy (NativeLibrary.getFunction<LLVMConstPointerNullDelegate> "LLVMConstPointerNull")

// Scalar constants
let llvmConstInt = lazy (NativeLibrary.getFunction<LLVMConstIntDelegate> "LLVMConstInt")
let llvmConstIntOfArbitraryPrecision = lazy (NativeLibrary.getFunction<LLVMConstIntOfArbitraryPrecisionDelegate> "LLVMConstIntOfArbitraryPrecision")
let llvmConstIntOfString = lazy (NativeLibrary.getFunction<LLVMConstIntOfStringDelegate> "LLVMConstIntOfString")
let llvmConstIntOfStringAndSize = lazy (NativeLibrary.getFunction<LLVMConstIntOfStringAndSizeDelegate> "LLVMConstIntOfStringAndSize")
let llvmConstReal = lazy (NativeLibrary.getFunction<LLVMConstRealDelegate> "LLVMConstReal")
let llvmConstRealOfString = lazy (NativeLibrary.getFunction<LLVMConstRealOfStringDelegate> "LLVMConstRealOfString")
let llvmConstRealOfStringAndSize = lazy (NativeLibrary.getFunction<LLVMConstRealOfStringAndSizeDelegate> "LLVMConstRealOfStringAndSize")
let llvmConstIntGetZExtValue = lazy (NativeLibrary.getFunction<LLVMConstIntGetZExtValueDelegate> "LLVMConstIntGetZExtValue")
let llvmConstIntGetSExtValue = lazy (NativeLibrary.getFunction<LLVMConstIntGetSExtValueDelegate> "LLVMConstIntGetSExtValue")
let llvmConstRealGetDouble = lazy (NativeLibrary.getFunction<LLVMConstRealGetDoubleDelegate> "LLVMConstRealGetDouble")

// Composite constants
let llvmConstStringInContext = lazy (NativeLibrary.getFunction<LLVMConstStringInContextDelegate> "LLVMConstStringInContext")
let llvmConstStringInContext2 = lazy (NativeLibrary.getFunction<LLVMConstStringInContext2Delegate> "LLVMConstStringInContext2")
let llvmConstString = lazy (NativeLibrary.getFunction<LLVMConstStringDelegate> "LLVMConstString")
let llvmIsConstantString = lazy (NativeLibrary.getFunction<LLVMIsConstantStringDelegate> "LLVMIsConstantString")
let llvmGetAsString = lazy (NativeLibrary.getFunction<LLVMGetAsStringDelegate> "LLVMGetAsString")
let llvmConstStructInContext = lazy (NativeLibrary.getFunction<LLVMConstStructInContextDelegate> "LLVMConstStructInContext")
let llvmConstStruct = lazy (NativeLibrary.getFunction<LLVMConstStructDelegate> "LLVMConstStruct")
let llvmConstNamedStruct = lazy (NativeLibrary.getFunction<LLVMConstNamedStructDelegate> "LLVMConstNamedStruct")
let llvmGetAggregateElement = lazy (NativeLibrary.getFunction<LLVMGetAggregateElementDelegate> "LLVMGetAggregateElement")
let llvmConstArray = lazy (NativeLibrary.getFunction<LLVMConstArrayDelegate> "LLVMConstArray")
let llvmConstArray2 = lazy (NativeLibrary.getFunction<LLVMConstArray2Delegate> "LLVMConstArray2")
let llvmConstVector = lazy (NativeLibrary.getFunction<LLVMConstVectorDelegate> "LLVMConstVector")

// Global Values
let llvmGetGlobalParent = lazy (NativeLibrary.getFunction<LLVMGetGlobalParentDelegate> "LLVMGetGlobalParent")
let llvmIsDeclaration = lazy (NativeLibrary.getFunction<LLVMIsDeclarationDelegate> "LLVMIsDeclaration")
let llvmGetLinkage = lazy (NativeLibrary.getFunction<LLVMGetLinkageDelegate> "LLVMGetLinkage")
let llvmSetLinkage = lazy (NativeLibrary.getFunction<LLVMSetLinkageDelegate> "LLVMSetLinkage")
let llvmGetSection = lazy (NativeLibrary.getFunction<LLVMGetSectionDelegate> "LLVMGetSection")
let llvmSetSection = lazy (NativeLibrary.getFunction<LLVMSetSectionDelegate> "LLVMSetSection")
let llvmGetVisibility = lazy (NativeLibrary.getFunction<LLVMGetVisibilityDelegate> "LLVMGetVisibility")
let llvmSetVisibility = lazy (NativeLibrary.getFunction<LLVMSetVisibilityDelegate> "LLVMSetVisibility")
let llvmGlobalGetValueType = lazy (NativeLibrary.getFunction<LLVMGlobalGetValueTypeDelegate> "LLVMGlobalGetValueType")
let llvmGetAlignment = lazy (NativeLibrary.getFunction<LLVMGetAlignmentDelegate> "LLVMGetAlignment")
let llvmSetAlignment = lazy (NativeLibrary.getFunction<LLVMSetAlignmentDelegate> "LLVMSetAlignment")

// Global variables
let llvmAddGlobal = lazy (NativeLibrary.getFunction<LLVMAddGlobalDelegate> "LLVMAddGlobal")
let llvmAddGlobalInAddressSpace = lazy (NativeLibrary.getFunction<LLVMAddGlobalInAddressSpaceDelegate> "LLVMAddGlobalInAddressSpace")
let llvmGetNamedGlobal = lazy (NativeLibrary.getFunction<LLVMGetNamedGlobalDelegate> "LLVMGetNamedGlobal")
let llvmGetFirstGlobal = lazy (NativeLibrary.getFunction<LLVMGetFirstGlobalDelegate> "LLVMGetFirstGlobal")
let llvmGetLastGlobal = lazy (NativeLibrary.getFunction<LLVMGetLastGlobalDelegate> "LLVMGetLastGlobal")
let llvmGetNextGlobal = lazy (NativeLibrary.getFunction<LLVMGetNextGlobalDelegate> "LLVMGetNextGlobal")
let llvmGetPreviousGlobal = lazy (NativeLibrary.getFunction<LLVMGetPreviousGlobalDelegate> "LLVMGetPreviousGlobal")
let llvmDeleteGlobal = lazy (NativeLibrary.getFunction<LLVMDeleteGlobalDelegate> "LLVMDeleteGlobal")
let llvmGetInitializer = lazy (NativeLibrary.getFunction<LLVMGetInitializerDelegate> "LLVMGetInitializer")
let llvmSetInitializer = lazy (NativeLibrary.getFunction<LLVMSetInitializerDelegate> "LLVMSetInitializer")
let llvmIsThreadLocal = lazy (NativeLibrary.getFunction<LLVMIsThreadLocalDelegate> "LLVMIsThreadLocal")
let llvmSetThreadLocal = lazy (NativeLibrary.getFunction<LLVMSetThreadLocalDelegate> "LLVMSetThreadLocal")
let llvmIsGlobalConstant = lazy (NativeLibrary.getFunction<LLVMIsGlobalConstantDelegate> "LLVMIsGlobalConstant")
let llvmSetGlobalConstant = lazy (NativeLibrary.getFunction<LLVMSetGlobalConstantDelegate> "LLVMSetGlobalConstant")

// Functions
let llvmAddFunction = lazy (NativeLibrary.getFunction<LLVMAddFunctionDelegate> "LLVMAddFunction")
let llvmGetNamedFunction = lazy (NativeLibrary.getFunction<LLVMGetNamedFunctionDelegate> "LLVMGetNamedFunction")
let llvmGetFirstFunction = lazy (NativeLibrary.getFunction<LLVMGetFirstFunctionDelegate> "LLVMGetFirstFunction")
let llvmGetLastFunction = lazy (NativeLibrary.getFunction<LLVMGetLastFunctionDelegate> "LLVMGetLastFunction")
let llvmGetNextFunction = lazy (NativeLibrary.getFunction<LLVMGetNextFunctionDelegate> "LLVMGetNextFunction")
let llvmGetPreviousFunction = lazy (NativeLibrary.getFunction<LLVMGetPreviousFunctionDelegate> "LLVMGetPreviousFunction")
let llvmDeleteFunction = lazy (NativeLibrary.getFunction<LLVMDeleteFunctionDelegate> "LLVMDeleteFunction")
let llvmHasPersonalityFn = lazy (NativeLibrary.getFunction<LLVMHasPersonalityFnDelegate> "LLVMHasPersonalityFn")
let llvmGetPersonalityFn = lazy (NativeLibrary.getFunction<LLVMGetPersonalityFnDelegate> "LLVMGetPersonalityFn")
let llvmSetPersonalityFn = lazy (NativeLibrary.getFunction<LLVMSetPersonalityFnDelegate> "LLVMSetPersonalityFn")
let llvmGetIntrinsicID = lazy (NativeLibrary.getFunction<LLVMGetIntrinsicIDDelegate> "LLVMGetIntrinsicID")
let llvmGetFunctionCallConv = lazy (NativeLibrary.getFunction<LLVMGetFunctionCallConvDelegate> "LLVMGetFunctionCallConv")
let llvmSetFunctionCallConv = lazy (NativeLibrary.getFunction<LLVMSetFunctionCallConvDelegate> "LLVMSetFunctionCallConv")
let llvmGetGC = lazy (NativeLibrary.getFunction<LLVMGetGCDelegate> "LLVMGetGC")
let llvmSetGC = lazy (NativeLibrary.getFunction<LLVMSetGCDelegate> "LLVMSetGC")
let llvmCountParams = lazy (NativeLibrary.getFunction<LLVMCountParamsDelegate> "LLVMCountParams")
let llvmGetParams = lazy (NativeLibrary.getFunction<LLVMGetParamsDelegate> "LLVMGetParams")
let llvmGetParam = lazy (NativeLibrary.getFunction<LLVMGetParamDelegate> "LLVMGetParam")
let llvmGetParamParent = lazy (NativeLibrary.getFunction<LLVMGetParamParentDelegate> "LLVMGetParamParent")
let llvmGetFirstParam = lazy (NativeLibrary.getFunction<LLVMGetFirstParamDelegate> "LLVMGetFirstParam")
let llvmGetLastParam = lazy (NativeLibrary.getFunction<LLVMGetLastParamDelegate> "LLVMGetLastParam")
let llvmGetNextParam = lazy (NativeLibrary.getFunction<LLVMGetNextParamDelegate> "LLVMGetNextParam")
let llvmGetPreviousParam = lazy (NativeLibrary.getFunction<LLVMGetPreviousParamDelegate> "LLVMGetPreviousParam")
let llvmSetParamAlignment = lazy (NativeLibrary.getFunction<LLVMSetParamAlignmentDelegate> "LLVMSetParamAlignment")

// Basic Block Functions
let llvmBasicBlockAsValue = lazy (NativeLibrary.getFunction<LLVMBasicBlockAsValueDelegate> "LLVMBasicBlockAsValue")
let llvmValueIsBasicBlock = lazy (NativeLibrary.getFunction<LLVMValueIsBasicBlockDelegate> "LLVMValueIsBasicBlock")
let llvmValueAsBasicBlock = lazy (NativeLibrary.getFunction<LLVMValueAsBasicBlockDelegate> "LLVMValueAsBasicBlock")
let llvmGetBasicBlockName = lazy (NativeLibrary.getFunction<LLVMGetBasicBlockNameDelegate> "LLVMGetBasicBlockName")
let llvmGetBasicBlockParent = lazy (NativeLibrary.getFunction<LLVMGetBasicBlockParentDelegate> "LLVMGetBasicBlockParent")
let llvmGetBasicBlockTerminator = lazy (NativeLibrary.getFunction<LLVMGetBasicBlockTerminatorDelegate> "LLVMGetBasicBlockTerminator")
let llvmCountBasicBlocks = lazy (NativeLibrary.getFunction<LLVMCountBasicBlocksDelegate> "LLVMCountBasicBlocks")
let llvmGetBasicBlocks = lazy (NativeLibrary.getFunction<LLVMGetBasicBlocksDelegate> "LLVMGetBasicBlocks")
let llvmGetFirstBasicBlock = lazy (NativeLibrary.getFunction<LLVMGetFirstBasicBlockDelegate> "LLVMGetFirstBasicBlock")
let llvmGetLastBasicBlock = lazy (NativeLibrary.getFunction<LLVMGetLastBasicBlockDelegate> "LLVMGetLastBasicBlock")
let llvmGetNextBasicBlock = lazy (NativeLibrary.getFunction<LLVMGetNextBasicBlockDelegate> "LLVMGetNextBasicBlock")
let llvmGetPreviousBasicBlock = lazy (NativeLibrary.getFunction<LLVMGetPreviousBasicBlockDelegate> "LLVMGetPreviousBasicBlock")
let llvmGetEntryBasicBlock = lazy (NativeLibrary.getFunction<LLVMGetEntryBasicBlockDelegate> "LLVMGetEntryBasicBlock")
let llvmInsertExistingBasicBlockAfterInsertBlock = lazy (NativeLibrary.getFunction<LLVMInsertExistingBasicBlockAfterInsertBlockDelegate> "LLVMInsertExistingBasicBlockAfterInsertBlock")
let llvmAppendExistingBasicBlock = lazy (NativeLibrary.getFunction<LLVMAppendExistingBasicBlockDelegate> "LLVMAppendExistingBasicBlock")
let llvmCreateBasicBlockInContext = lazy (NativeLibrary.getFunction<LLVMCreateBasicBlockInContextDelegate> "LLVMCreateBasicBlockInContext")
let llvmAppendBasicBlockInContext = lazy (NativeLibrary.getFunction<LLVMAppendBasicBlockInContextDelegate> "LLVMAppendBasicBlockInContext")
let llvmAppendBasicBlock = lazy (NativeLibrary.getFunction<LLVMAppendBasicBlockDelegate> "LLVMAppendBasicBlock")
let llvmInsertBasicBlockInContext = lazy (NativeLibrary.getFunction<LLVMInsertBasicBlockInContextDelegate> "LLVMInsertBasicBlockInContext")
let llvmInsertBasicBlock = lazy (NativeLibrary.getFunction<LLVMInsertBasicBlockDelegate> "LLVMInsertBasicBlock")
let llvmDeleteBasicBlock = lazy (NativeLibrary.getFunction<LLVMDeleteBasicBlockDelegate> "LLVMDeleteBasicBlock")
let llvmRemoveBasicBlockFromParent = lazy (NativeLibrary.getFunction<LLVMRemoveBasicBlockFromParentDelegate> "LLVMRemoveBasicBlockFromParent")
let llvmMoveBasicBlockBefore = lazy (NativeLibrary.getFunction<LLVMMoveBasicBlockBeforeDelegate> "LLVMMoveBasicBlockBefore")
let llvmMoveBasicBlockAfter = lazy (NativeLibrary.getFunction<LLVMMoveBasicBlockAfterDelegate> "LLVMMoveBasicBlockAfter")
let llvmGetFirstInstruction = lazy (NativeLibrary.getFunction<LLVMGetFirstInstructionDelegate> "LLVMGetFirstInstruction")
let llvmGetLastInstruction = lazy (NativeLibrary.getFunction<LLVMGetLastInstructionDelegate> "LLVMGetLastInstruction")

// Instruction Builder Functions
let llvmCreateBuilderInContext = lazy (NativeLibrary.getFunction<LLVMCreateBuilderInContextDelegate> "LLVMCreateBuilderInContext")
let llvmCreateBuilder = lazy (NativeLibrary.getFunction<LLVMCreateBuilderDelegate> "LLVMCreateBuilder")
let llvmPositionBuilder = lazy (NativeLibrary.getFunction<LLVMPositionBuilderDelegate> "LLVMPositionBuilder")
let llvmPositionBuilderBefore = lazy (NativeLibrary.getFunction<LLVMPositionBuilderBeforeDelegate> "LLVMPositionBuilderBefore")
let llvmPositionBuilderAtEnd = lazy (NativeLibrary.getFunction<LLVMPositionBuilderAtEndDelegate> "LLVMPositionBuilderAtEnd")
let llvmGetInsertBlock = lazy (NativeLibrary.getFunction<LLVMGetInsertBlockDelegate> "LLVMGetInsertBlock")
let llvmClearInsertionPosition = lazy (NativeLibrary.getFunction<LLVMClearInsertionPositionDelegate> "LLVMClearInsertionPosition")
let llvmInsertIntoBuilder = lazy (NativeLibrary.getFunction<LLVMInsertIntoBuilderDelegate> "LLVMInsertIntoBuilder")
let llvmInsertIntoBuilderWithName = lazy (NativeLibrary.getFunction<LLVMInsertIntoBuilderWithNameDelegate> "LLVMInsertIntoBuilderWithName")
let llvmDisposeBuilder = lazy (NativeLibrary.getFunction<LLVMDisposeBuilderDelegate> "LLVMDisposeBuilder")
let llvmGetBuilderContext = lazy (NativeLibrary.getFunction<LLVMGetBuilderContextDelegate> "LLVMGetBuilderContext")

// Metadata
let llvmGetCurrentDebugLocation2 = lazy (NativeLibrary.getFunction<LLVMGetCurrentDebugLocation2Delegate> "LLVMGetCurrentDebugLocation2")
let llvmSetCurrentDebugLocation2 = lazy (NativeLibrary.getFunction<LLVMSetCurrentDebugLocation2Delegate> "LLVMSetCurrentDebugLocation2")
let llvmAddMetadataToInst = lazy (NativeLibrary.getFunction<LLVMAddMetadataToInstDelegate> "LLVMAddMetadataToInst")

// Terminator Instructions
let llvmBuildRetVoid = lazy (NativeLibrary.getFunction<LLVMBuildRetVoidDelegate> "LLVMBuildRetVoid")
let llvmBuildRet = lazy (NativeLibrary.getFunction<LLVMBuildRetDelegate> "LLVMBuildRet")
let llvmBuildAggregateRet = lazy (NativeLibrary.getFunction<LLVMBuildAggregateRetDelegate> "LLVMBuildAggregateRet")
let llvmBuildBr = lazy (NativeLibrary.getFunction<LLVMBuildBrDelegate> "LLVMBuildBr")
let llvmBuildCondBr = lazy (NativeLibrary.getFunction<LLVMBuildCondBrDelegate> "LLVMBuildCondBr")
let llvmBuildSwitch = lazy (NativeLibrary.getFunction<LLVMBuildSwitchDelegate> "LLVMBuildSwitch")
let llvmBuildIndirectBr = lazy (NativeLibrary.getFunction<LLVMBuildIndirectBrDelegate> "LLVMBuildIndirectBr")
let llvmBuildInvoke2 = lazy (NativeLibrary.getFunction<LLVMBuildInvoke2Delegate> "LLVMBuildInvoke2")
let llvmBuildUnreachable = lazy (NativeLibrary.getFunction<LLVMBuildUnreachableDelegate> "LLVMBuildUnreachable")

// Exception Handling
let llvmBuildResume = lazy (NativeLibrary.getFunction<LLVMBuildResumeDelegate> "LLVMBuildResume")
let llvmBuildLandingPad = lazy (NativeLibrary.getFunction<LLVMBuildLandingPadDelegate> "LLVMBuildLandingPad")
let llvmBuildCleanupRet = lazy (NativeLibrary.getFunction<LLVMBuildCleanupRetDelegate> "LLVMBuildCleanupRet")
let llvmBuildCatchRet = lazy (NativeLibrary.getFunction<LLVMBuildCatchRetDelegate> "LLVMBuildCatchRet")
let llvmBuildCatchPad = lazy (NativeLibrary.getFunction<LLVMBuildCatchPadDelegate> "LLVMBuildCatchPad")
let llvmBuildCleanupPad = lazy (NativeLibrary.getFunction<LLVMBuildCleanupPadDelegate> "LLVMBuildCleanupPad")
let llvmBuildCatchSwitch = lazy (NativeLibrary.getFunction<LLVMBuildCatchSwitchDelegate> "LLVMBuildCatchSwitch")

// Control flow helpers
let llvmAddCase = lazy (NativeLibrary.getFunction<LLVMAddCaseDelegate> "LLVMAddCase")
let llvmAddDestination = lazy (NativeLibrary.getFunction<LLVMAddDestinationDelegate> "LLVMAddDestination")

// Arithmetic Instructions
let llvmBuildAdd = lazy (NativeLibrary.getFunction<LLVMBuildAddDelegate> "LLVMBuildAdd")
let llvmBuildNSWAdd = lazy (NativeLibrary.getFunction<LLVMBuildNSWAddDelegate> "LLVMBuildNSWAdd")
let llvmBuildNUWAdd = lazy (NativeLibrary.getFunction<LLVMBuildNUWAddDelegate> "LLVMBuildNUWAdd")
let llvmBuildFAdd = lazy (NativeLibrary.getFunction<LLVMBuildFAddDelegate> "LLVMBuildFAdd")
let llvmBuildSub = lazy (NativeLibrary.getFunction<LLVMBuildSubDelegate> "LLVMBuildSub")
let llvmBuildNSWSub = lazy (NativeLibrary.getFunction<LLVMBuildNSWSubDelegate> "LLVMBuildNSWSub")
let llvmBuildNUWSub = lazy (NativeLibrary.getFunction<LLVMBuildNUWSubDelegate> "LLVMBuildNUWSub")
let llvmBuildFSub = lazy (NativeLibrary.getFunction<LLVMBuildFSubDelegate> "LLVMBuildFSub")
let llvmBuildMul = lazy (NativeLibrary.getFunction<LLVMBuildMulDelegate> "LLVMBuildMul")
let llvmBuildNSWMul = lazy (NativeLibrary.getFunction<LLVMBuildNSWMulDelegate> "LLVMBuildNSWMul")
let llvmBuildNUWMul = lazy (NativeLibrary.getFunction<LLVMBuildNUWMulDelegate> "LLVMBuildNUWMul")
let llvmBuildFMul = lazy (NativeLibrary.getFunction<LLVMBuildFMulDelegate> "LLVMBuildFMul")
let llvmBuildUDiv = lazy (NativeLibrary.getFunction<LLVMBuildUDivDelegate> "LLVMBuildUDiv")
let llvmBuildExactUDiv = lazy (NativeLibrary.getFunction<LLVMBuildExactUDivDelegate> "LLVMBuildExactUDiv")
let llvmBuildSDiv = lazy (NativeLibrary.getFunction<LLVMBuildSDivDelegate> "LLVMBuildSDiv")
let llvmBuildExactSDiv = lazy (NativeLibrary.getFunction<LLVMBuildExactSDivDelegate> "LLVMBuildExactSDiv")
let llvmBuildFDiv = lazy (NativeLibrary.getFunction<LLVMBuildFDivDelegate> "LLVMBuildFDiv")
let llvmBuildURem = lazy (NativeLibrary.getFunction<LLVMBuildURemDelegate> "LLVMBuildURem")
let llvmBuildSRem = lazy (NativeLibrary.getFunction<LLVMBuildSRemDelegate> "LLVMBuildSRem")
let llvmBuildFRem = lazy (NativeLibrary.getFunction<LLVMBuildFRemDelegate> "LLVMBuildFRem")
let llvmBuildShl = lazy (NativeLibrary.getFunction<LLVMBuildShlDelegate> "LLVMBuildShl")
let llvmBuildLShr = lazy (NativeLibrary.getFunction<LLVMBuildLShrDelegate> "LLVMBuildLShr")
let llvmBuildAShr = lazy (NativeLibrary.getFunction<LLVMBuildAShrDelegate> "LLVMBuildAShr")
let llvmBuildAnd = lazy (NativeLibrary.getFunction<LLVMBuildAndDelegate> "LLVMBuildAnd")
let llvmBuildOr = lazy (NativeLibrary.getFunction<LLVMBuildOrDelegate> "LLVMBuildOr")
let llvmBuildXor = lazy (NativeLibrary.getFunction<LLVMBuildXorDelegate> "LLVMBuildXor")
let llvmBuildBinOp = lazy (NativeLibrary.getFunction<LLVMBuildBinOpDelegate> "LLVMBuildBinOp")
let llvmBuildNeg = lazy (NativeLibrary.getFunction<LLVMBuildNegDelegate> "LLVMBuildNeg")
let llvmBuildNSWNeg = lazy (NativeLibrary.getFunction<LLVMBuildNSWNegDelegate> "LLVMBuildNSWNeg")
let llvmBuildFNeg = lazy (NativeLibrary.getFunction<LLVMBuildFNegDelegate> "LLVMBuildFNeg")
let llvmBuildNot = lazy (NativeLibrary.getFunction<LLVMBuildNotDelegate> "LLVMBuildNot")

// Arithmetic flags
let llvmGetNUW = lazy (NativeLibrary.getFunction<LLVMGetNUWDelegate> "LLVMGetNUW")
let llvmSetNUW = lazy (NativeLibrary.getFunction<LLVMSetNUWDelegate> "LLVMSetNUW")
let llvmGetNSW = lazy (NativeLibrary.getFunction<LLVMGetNSWDelegate> "LLVMGetNSW")
let llvmSetNSW = lazy (NativeLibrary.getFunction<LLVMSetNSWDelegate> "LLVMSetNSW")
let llvmGetExact = lazy (NativeLibrary.getFunction<LLVMGetExactDelegate> "LLVMGetExact")
let llvmSetExact = lazy (NativeLibrary.getFunction<LLVMSetExactDelegate> "LLVMSetExact")

// Memory Instructions
let llvmBuildAlloca = lazy (NativeLibrary.getFunction<LLVMBuildAllocaDelegate> "LLVMBuildAlloca")
let llvmBuildArrayAlloca = lazy (NativeLibrary.getFunction<LLVMBuildArrayAllocaDelegate> "LLVMBuildArrayAlloca")
let llvmBuildLoad2 = lazy (NativeLibrary.getFunction<LLVMBuildLoad2Delegate> "LLVMBuildLoad2")
let llvmBuildStore = lazy (NativeLibrary.getFunction<LLVMBuildStoreDelegate> "LLVMBuildStore")
let llvmBuildGEP2 = lazy (NativeLibrary.getFunction<LLVMBuildGEP2Delegate> "LLVMBuildGEP2")
let llvmBuildInBoundsGEP2 = lazy (NativeLibrary.getFunction<LLVMBuildInBoundsGEP2Delegate> "LLVMBuildInBoundsGEP2")
let llvmBuildStructGEP2 = lazy (NativeLibrary.getFunction<LLVMBuildStructGEP2Delegate> "LLVMBuildStructGEP2")
let llvmBuildGlobalString = lazy (NativeLibrary.getFunction<LLVMBuildGlobalStringDelegate> "LLVMBuildGlobalString")
let llvmBuildGlobalStringPtr = lazy (NativeLibrary.getFunction<LLVMBuildGlobalStringPtrDelegate> "LLVMBuildGlobalStringPtr")
let llvmGetVolatile = lazy (NativeLibrary.getFunction<LLVMGetVolatileDelegate> "LLVMGetVolatile")
let llvmSetVolatile = lazy (NativeLibrary.getFunction<LLVMSetVolatileDelegate> "LLVMSetVolatile")
let llvmGetOrdering = lazy (NativeLibrary.getFunction<LLVMGetOrderingDelegate> "LLVMGetOrdering")
let llvmSetOrdering = lazy (NativeLibrary.getFunction<LLVMSetOrderingDelegate> "LLVMSetOrdering")

// Cast Instructions
let llvmBuildTrunc = lazy (NativeLibrary.getFunction<LLVMBuildTruncDelegate> "LLVMBuildTrunc")
let llvmBuildZExt = lazy (NativeLibrary.getFunction<LLVMBuildZExtDelegate> "LLVMBuildZExt")
let llvmBuildSExt = lazy (NativeLibrary.getFunction<LLVMBuildSExtDelegate> "LLVMBuildSExt")
let llvmBuildFPToUI = lazy (NativeLibrary.getFunction<LLVMBuildFPToUIDelegate> "LLVMBuildFPToUI")
let llvmBuildFPToSI = lazy (NativeLibrary.getFunction<LLVMBuildFPToSIDelegate> "LLVMBuildFPToSI")
let llvmBuildUIToFP = lazy (NativeLibrary.getFunction<LLVMBuildUIToFPDelegate> "LLVMBuildUIToFP")
let llvmBuildSIToFP = lazy (NativeLibrary.getFunction<LLVMBuildSIToFPDelegate> "LLVMBuildSIToFP")
let llvmBuildFPTrunc = lazy (NativeLibrary.getFunction<LLVMBuildFPTruncDelegate> "LLVMBuildFPTrunc")
let llvmBuildFPExt = lazy (NativeLibrary.getFunction<LLVMBuildFPExtDelegate> "LLVMBuildFPExt")
let llvmBuildPtrToInt = lazy (NativeLibrary.getFunction<LLVMBuildPtrToIntDelegate> "LLVMBuildPtrToInt")
let llvmBuildIntToPtr = lazy (NativeLibrary.getFunction<LLVMBuildIntToPtrDelegate> "LLVMBuildIntToPtr")
let llvmBuildBitCast = lazy (NativeLibrary.getFunction<LLVMBuildBitCastDelegate> "LLVMBuildBitCast")
let llvmBuildAddrSpaceCast = lazy (NativeLibrary.getFunction<LLVMBuildAddrSpaceCastDelegate> "LLVMBuildAddrSpaceCast")
let llvmBuildPointerCast = lazy (NativeLibrary.getFunction<LLVMBuildPointerCastDelegate> "LLVMBuildPointerCast")
let llvmBuildIntCast2 = lazy (NativeLibrary.getFunction<LLVMBuildIntCast2Delegate> "LLVMBuildIntCast2")
let llvmBuildFPCast = lazy (NativeLibrary.getFunction<LLVMBuildFPCastDelegate> "LLVMBuildFPCast")
let llvmBuildCast = lazy (NativeLibrary.getFunction<LLVMBuildCastDelegate> "LLVMBuildCast")
let llvmGetCastOpcode = lazy (NativeLibrary.getFunction<LLVMGetCastOpcodeDelegate> "LLVMGetCastOpcode")

// Comparison Instructions
let llvmBuildICmp = lazy (NativeLibrary.getFunction<LLVMBuildICmpDelegate> "LLVMBuildICmp")
let llvmBuildFCmp = lazy (NativeLibrary.getFunction<LLVMBuildFCmpDelegate> "LLVMBuildFCmp")

// Other Instructions
let llvmBuildPhi = lazy (NativeLibrary.getFunction<LLVMBuildPhiDelegate> "LLVMBuildPhi")
let llvmBuildCall2 = lazy (NativeLibrary.getFunction<LLVMBuildCall2Delegate> "LLVMBuildCall2")
let llvmBuildSelect = lazy (NativeLibrary.getFunction<LLVMBuildSelectDelegate> "LLVMBuildSelect")
let llvmBuildVAArg = lazy (NativeLibrary.getFunction<LLVMBuildVAArgDelegate> "LLVMBuildVAArg")
let llvmBuildExtractElement = lazy (NativeLibrary.getFunction<LLVMBuildExtractElementDelegate> "LLVMBuildExtractElement")
let llvmBuildInsertElement = lazy (NativeLibrary.getFunction<LLVMBuildInsertElementDelegate> "LLVMBuildInsertElement")
let llvmBuildShuffleVector = lazy (NativeLibrary.getFunction<LLVMBuildShuffleVectorDelegate> "LLVMBuildShuffleVector")
let llvmBuildExtractValue = lazy (NativeLibrary.getFunction<LLVMBuildExtractValueDelegate> "LLVMBuildExtractValue")
let llvmBuildInsertValue = lazy (NativeLibrary.getFunction<LLVMBuildInsertValueDelegate> "LLVMBuildInsertValue")
let llvmBuildFreeze = lazy (NativeLibrary.getFunction<LLVMBuildFreezeDelegate> "LLVMBuildFreeze")
let llvmBuildIsNull = lazy (NativeLibrary.getFunction<LLVMBuildIsNullDelegate> "LLVMBuildIsNull")
let llvmBuildIsNotNull = lazy (NativeLibrary.getFunction<LLVMBuildIsNotNullDelegate> "LLVMBuildIsNotNull")
let llvmBuildPtrDiff2 = lazy (NativeLibrary.getFunction<LLVMBuildPtrDiff2Delegate> "LLVMBuildPtrDiff2")
let llvmBuildFence = lazy (NativeLibrary.getFunction<LLVMBuildFenceDelegate> "LLVMBuildFence")
let llvmBuildAtomicRMW = lazy (NativeLibrary.getFunction<LLVMBuildAtomicRMWDelegate> "LLVMBuildAtomicRMW")
let llvmBuildAtomicCmpXchg = lazy (NativeLibrary.getFunction<LLVMBuildAtomicCmpXchgDelegate> "LLVMBuildAtomicCmpXchg")

// Atomic helpers
let llvmIsAtomicSingleThread = lazy (NativeLibrary.getFunction<LLVMIsAtomicSingleThreadDelegate> "LLVMIsAtomicSingleThread")
let llvmSetAtomicSingleThread = lazy (NativeLibrary.getFunction<LLVMSetAtomicSingleThreadDelegate> "LLVMSetAtomicSingleThread")

// PHI Node Functions
let llvmAddIncoming = lazy (NativeLibrary.getFunction<LLVMAddIncomingDelegate> "LLVMAddIncoming")
let llvmCountIncoming = lazy (NativeLibrary.getFunction<LLVMCountIncomingDelegate> "LLVMCountIncoming")
let llvmGetIncomingValue = lazy (NativeLibrary.getFunction<LLVMGetIncomingValueDelegate> "LLVMGetIncomingValue")
let llvmGetIncomingBlock = lazy (NativeLibrary.getFunction<LLVMGetIncomingBlockDelegate> "LLVMGetIncomingBlock")

// Instruction Functions
let llvmHasMetadata = lazy (NativeLibrary.getFunction<LLVMHasMetadataDelegate> "LLVMHasMetadata")
let llvmGetMetadata = lazy (NativeLibrary.getFunction<LLVMGetMetadataDelegate> "LLVMGetMetadata")
let llvmSetMetadata = lazy (NativeLibrary.getFunction<LLVMSetMetadataDelegate> "LLVMSetMetadata")
let llvmGetInstructionParent = lazy (NativeLibrary.getFunction<LLVMGetInstructionParentDelegate> "LLVMGetInstructionParent")
let llvmGetNextInstruction = lazy (NativeLibrary.getFunction<LLVMGetNextInstructionDelegate> "LLVMGetNextInstruction")
let llvmGetPreviousInstruction = lazy (NativeLibrary.getFunction<LLVMGetPreviousInstructionDelegate> "LLVMGetPreviousInstruction")
let llvmInstructionRemoveFromParent = lazy (NativeLibrary.getFunction<LLVMInstructionRemoveFromParentDelegate> "LLVMInstructionRemoveFromParent")
let llvmInstructionEraseFromParent = lazy (NativeLibrary.getFunction<LLVMInstructionEraseFromParentDelegate> "LLVMInstructionEraseFromParent")
let llvmDeleteInstruction = lazy (NativeLibrary.getFunction<LLVMDeleteInstructionDelegate> "LLVMDeleteInstruction")
let llvmGetInstructionOpcode = lazy (NativeLibrary.getFunction<LLVMGetInstructionOpcodeDelegate> "LLVMGetInstructionOpcode")
let llvmGetICmpPredicate = lazy (NativeLibrary.getFunction<LLVMGetICmpPredicateDelegate> "LLVMGetICmpPredicate")
let llvmGetFCmpPredicate = lazy (NativeLibrary.getFunction<LLVMGetFCmpPredicateDelegate> "LLVMGetFCmpPredicate")
let llvmInstructionClone = lazy (NativeLibrary.getFunction<LLVMInstructionCloneDelegate> "LLVMInstructionClone")

// Call site functions
let llvmGetNumArgOperands = lazy (NativeLibrary.getFunction<LLVMGetNumArgOperandsDelegate> "LLVMGetNumArgOperands")
let llvmSetInstructionCallConv = lazy (NativeLibrary.getFunction<LLVMSetInstructionCallConvDelegate> "LLVMSetInstructionCallConv")
let llvmGetInstructionCallConv = lazy (NativeLibrary.getFunction<LLVMGetInstructionCallConvDelegate> "LLVMGetInstructionCallConv")
let llvmGetCalledFunctionType = lazy (NativeLibrary.getFunction<LLVMGetCalledFunctionTypeDelegate> "LLVMGetCalledFunctionType")
let llvmGetCalledValue = lazy (NativeLibrary.getFunction<LLVMGetCalledValueDelegate> "LLVMGetCalledValue")
let llvmIsTailCall = lazy (NativeLibrary.getFunction<LLVMIsTailCallDelegate> "LLVMIsTailCall")
let llvmSetTailCall = lazy (NativeLibrary.getFunction<LLVMSetTailCallDelegate> "LLVMSetTailCall")

// Terminator functions
let llvmGetNumSuccessors = lazy (NativeLibrary.getFunction<LLVMGetNumSuccessorsDelegate> "LLVMGetNumSuccessors")
let llvmGetSuccessor = lazy (NativeLibrary.getFunction<LLVMGetSuccessorDelegate> "LLVMGetSuccessor")
let llvmSetSuccessor = lazy (NativeLibrary.getFunction<LLVMSetSuccessorDelegate> "LLVMSetSuccessor")
let llvmIsConditional = lazy (NativeLibrary.getFunction<LLVMIsConditionalDelegate> "LLVMIsConditional")
let llvmGetCondition = lazy (NativeLibrary.getFunction<LLVMGetConditionDelegate> "LLVMGetCondition")
let llvmSetCondition = lazy (NativeLibrary.getFunction<LLVMSetConditionDelegate> "LLVMSetCondition")
let llvmGetSwitchDefaultDest = lazy (NativeLibrary.getFunction<LLVMGetSwitchDefaultDestDelegate> "LLVMGetSwitchDefaultDest")

// Alloca functions
let llvmGetAllocatedType = lazy (NativeLibrary.getFunction<LLVMGetAllocatedTypeDelegate> "LLVMGetAllocatedType")

// GEP functions
let llvmIsInBounds = lazy (NativeLibrary.getFunction<LLVMIsInBoundsDelegate> "LLVMIsInBounds")
let llvmSetIsInBounds = lazy (NativeLibrary.getFunction<LLVMSetIsInBoundsDelegate> "LLVMSetIsInBounds")
let llvmGetGEPSourceElementType = lazy (NativeLibrary.getFunction<LLVMGetGEPSourceElementTypeDelegate> "LLVMGetGEPSourceElementType")

// Extract/Insert value functions
let llvmGetNumIndices = lazy (NativeLibrary.getFunction<LLVMGetNumIndicesDelegate> "LLVMGetNumIndices")
let llvmGetIndices = lazy (NativeLibrary.getFunction<LLVMGetIndicesDelegate> "LLVMGetIndices")

// Pass Manager Functions
let llvmCreatePassManager = lazy (NativeLibrary.getFunction<LLVMCreatePassManagerDelegate> "LLVMCreatePassManager")
let llvmCreateFunctionPassManagerForModule = lazy (NativeLibrary.getFunction<LLVMCreateFunctionPassManagerForModuleDelegate> "LLVMCreateFunctionPassManagerForModule")
let llvmRunPassManager = lazy (NativeLibrary.getFunction<LLVMRunPassManagerDelegate> "LLVMRunPassManager")
let llvmInitializeFunctionPassManager = lazy (NativeLibrary.getFunction<LLVMInitializeFunctionPassManagerDelegate> "LLVMInitializeFunctionPassManager")
let llvmRunFunctionPassManager = lazy (NativeLibrary.getFunction<LLVMRunFunctionPassManagerDelegate> "LLVMRunFunctionPassManager")
let llvmFinalizeFunctionPassManager = lazy (NativeLibrary.getFunction<LLVMFinalizeFunctionPassManagerDelegate> "LLVMFinalizeFunctionPassManager")
let llvmDisposePassManager = lazy (NativeLibrary.getFunction<LLVMDisposePassManagerDelegate> "LLVMDisposePassManager")

// Optimization Passes
let llvmAddInstructionCombiningPass = lazy (NativeLibrary.getFunction<LLVMAddInstructionCombiningPassDelegate> "LLVMAddInstructionCombiningPass")
let llvmAddPromoteMemoryToRegisterPass = lazy (NativeLibrary.getFunction<LLVMAddPromoteMemoryToRegisterPassDelegate> "LLVMAddPromoteMemoryToRegisterPass")
let llvmAddGVNPass = lazy (NativeLibrary.getFunction<LLVMAddGVNPassDelegate> "LLVMAddGVNPass")
let llvmAddCFGSimplificationPass = lazy (NativeLibrary.getFunction<LLVMAddCFGSimplificationPassDelegate> "LLVMAddCFGSimplificationPass")

// Target Functions
let llvmGetDefaultTargetTriple = lazy (NativeLibrary.getFunction<LLVMGetDefaultTargetTripleDelegate> "LLVMGetDefaultTargetTriple")
let llvmGetHostCPUName = lazy (NativeLibrary.getFunction<LLVMGetHostCPUNameDelegate> "LLVMGetHostCPUName")
let llvmGetHostCPUFeatures = lazy (NativeLibrary.getFunction<LLVMGetHostCPUFeaturesDelegate> "LLVMGetHostCPUFeatures")
let llvmInitializeNativeTarget = lazy (NativeLibrary.getFunction<LLVMInitializeNativeTargetDelegate> "LLVMInitializeNativeTarget")
let llvmInitializeNativeAsmPrinter = lazy (NativeLibrary.getFunction<LLVMInitializeNativeAsmPrinterDelegate> "LLVMInitializeNativeAsmPrinter")
let llvmGetTargetFromTriple = lazy (NativeLibrary.getFunction<LLVMGetTargetFromTripleDelegate> "LLVMGetTargetFromTriple")
let llvmGetTargetFromName = lazy (NativeLibrary.getFunction<LLVMGetTargetFromNameDelegate> "LLVMGetTargetFromName")
let llvmGetTargetName = lazy (NativeLibrary.getFunction<LLVMGetTargetNameDelegate> "LLVMGetTargetName")
let llvmGetTargetDescription = lazy (NativeLibrary.getFunction<LLVMGetTargetDescriptionDelegate> "LLVMGetTargetDescription")
let llvmTargetHasJIT = lazy (NativeLibrary.getFunction<LLVMTargetHasJITDelegate> "LLVMTargetHasJIT")
let llvmTargetHasTargetMachine = lazy (NativeLibrary.getFunction<LLVMTargetHasTargetMachineDelegate> "LLVMTargetHasTargetMachine")
let llvmTargetHasAsmBackend = lazy (NativeLibrary.getFunction<LLVMTargetHasAsmBackendDelegate> "LLVMTargetHasAsmBackend")
let llvmCreateTargetMachine = lazy (NativeLibrary.getFunction<LLVMCreateTargetMachineDelegate> "LLVMCreateTargetMachine")
let llvmDisposeTargetMachine = lazy (NativeLibrary.getFunction<LLVMDisposeTargetMachineDelegate> "LLVMDisposeTargetMachine")
let llvmGetTargetMachineTarget = lazy (NativeLibrary.getFunction<LLVMGetTargetMachineTargetDelegate> "LLVMGetTargetMachineTarget")
let llvmGetTargetMachineTriple = lazy (NativeLibrary.getFunction<LLVMGetTargetMachineTripleDelegate> "LLVMGetTargetMachineTriple")
let llvmGetTargetMachineCPU = lazy (NativeLibrary.getFunction<LLVMGetTargetMachineCPUDelegate> "LLVMGetTargetMachineCPU")
let llvmGetTargetMachineFeatureString = lazy (NativeLibrary.getFunction<LLVMGetTargetMachineFeatureStringDelegate> "LLVMGetTargetMachineFeatureString")
let llvmCreateTargetDataLayout = lazy (NativeLibrary.getFunction<LLVMCreateTargetDataLayoutDelegate> "LLVMCreateTargetDataLayout")
let llvmTargetMachineEmitToFile = lazy (NativeLibrary.getFunction<LLVMTargetMachineEmitToFileDelegate> "LLVMTargetMachineEmitToFile")
let llvmTargetMachineEmitToMemoryBuffer = lazy (NativeLibrary.getFunction<LLVMTargetMachineEmitToMemoryBufferDelegate> "LLVMTargetMachineEmitToMemoryBuffer")

// Target Data Functions
let llvmGetModuleDataLayout = lazy (NativeLibrary.getFunction<LLVMGetModuleDataLayoutDelegate> "LLVMGetModuleDataLayout")
let llvmSetModuleDataLayout = lazy (NativeLibrary.getFunction<LLVMSetModuleDataLayoutDelegate> "LLVMSetModuleDataLayout")
let llvmCreateTargetData = lazy (NativeLibrary.getFunction<LLVMCreateTargetDataDelegate> "LLVMCreateTargetData")
let llvmDisposeTargetData = lazy (NativeLibrary.getFunction<LLVMDisposeTargetDataDelegate> "LLVMDisposeTargetData")
let llvmCopyStringRepOfTargetData = lazy (NativeLibrary.getFunction<LLVMCopyStringRepOfTargetDataDelegate> "LLVMCopyStringRepOfTargetData")
let llvmPointerSize = lazy (NativeLibrary.getFunction<LLVMPointerSizeDelegate> "LLVMPointerSize")
let llvmPointerSizeForAS = lazy (NativeLibrary.getFunction<LLVMPointerSizeForASDelegate> "LLVMPointerSizeForAS")
let llvmIntPtrType = lazy (NativeLibrary.getFunction<LLVMIntPtrTypeDelegate> "LLVMIntPtrType")
let llvmIntPtrTypeForAS = lazy (NativeLibrary.getFunction<LLVMIntPtrTypeForASDelegate> "LLVMIntPtrTypeForAS")
let llvmIntPtrTypeInContext = lazy (NativeLibrary.getFunction<LLVMIntPtrTypeInContextDelegate> "LLVMIntPtrTypeInContext")
let llvmIntPtrTypeForASInContext = lazy (NativeLibrary.getFunction<LLVMIntPtrTypeForASInContextDelegate> "LLVMIntPtrTypeForASInContext")
let llvmSizeOfTypeInBits = lazy (NativeLibrary.getFunction<LLVMSizeOfTypeInBitsDelegate> "LLVMSizeOfTypeInBits")
let llvmStoreSizeOfType = lazy (NativeLibrary.getFunction<LLVMStoreSizeOfTypeDelegate> "LLVMStoreSizeOfType")
let llvmABISizeOfType = lazy (NativeLibrary.getFunction<LLVMABISizeOfTypeDelegate> "LLVMABISizeOfType")
let llvmABIAlignmentOfType = lazy (NativeLibrary.getFunction<LLVMABIAlignmentOfTypeDelegate> "LLVMABIAlignmentOfType")
let llvmCallFrameAlignmentOfType = lazy (NativeLibrary.getFunction<LLVMCallFrameAlignmentOfTypeDelegate> "LLVMCallFrameAlignmentOfType")
let llvmPreferredAlignmentOfType = lazy (NativeLibrary.getFunction<LLVMPreferredAlignmentOfTypeDelegate> "LLVMPreferredAlignmentOfType")
let llvmPreferredAlignmentOfGlobal = lazy (NativeLibrary.getFunction<LLVMPreferredAlignmentOfGlobalDelegate> "LLVMPreferredAlignmentOfGlobal")
let llvmElementAtOffset = lazy (NativeLibrary.getFunction<LLVMElementAtOffsetDelegate> "LLVMElementAtOffset")
let llvmOffsetOfElement = lazy (NativeLibrary.getFunction<LLVMOffsetOfElementDelegate> "LLVMOffsetOfElement")

// Initialization Functions
let llvmInitializeAllTargetInfos = lazy (NativeLibrary.getFunction<LLVMInitializeAllTargetInfosDelegate> "LLVMInitializeAllTargetInfos")
let llvmInitializeAllTargets = lazy (NativeLibrary.getFunction<LLVMInitializeAllTargetsDelegate> "LLVMInitializeAllTargets")
let llvmInitializeAllTargetMCs = lazy (NativeLibrary.getFunction<LLVMInitializeAllTargetMCsDelegate> "LLVMInitializeAllTargetMCs")
let llvmInitializeAllAsmPrinters = lazy (NativeLibrary.getFunction<LLVMInitializeAllAsmPrintersDelegate> "LLVMInitializeAllAsmPrinters")
let llvmInitializeAllAsmParsers = lazy (NativeLibrary.getFunction<LLVMInitializeAllAsmParsersDelegate> "LLVMInitializeAllAsmParsers")
let llvmInitializeAllDisassemblers = lazy (NativeLibrary.getFunction<LLVMInitializeAllDisassemblersDelegate> "LLVMInitializeAllDisassemblers")

// Memory Management Functions
let llvmDisposeMessage = lazy (NativeLibrary.getFunction<LLVMDisposeMessageDelegate> "LLVMDisposeMessage")
let llvmShutdown = lazy (NativeLibrary.getFunction<LLVMShutdownDelegate> "LLVMShutdown")

// Version Functions
let llvmGetVersion = lazy (NativeLibrary.getFunction<LLVMGetVersionDelegate> "LLVMGetVersion")

// Error Handling
let llvmCreateMessage = lazy (NativeLibrary.getFunction<LLVMCreateMessageDelegate> "LLVMCreateMessage")

// New Pass Manager Functions (PassBuilder)
let llvmRunPasses = lazy (NativeLibrary.getFunction<LLVMRunPassesDelegate> "LLVMRunPasses")
let llvmRunPassesOnFunction = lazy (NativeLibrary.getFunction<LLVMRunPassesOnFunctionDelegate> "LLVMRunPassesOnFunction")
let llvmCreatePassBuilderOptions = lazy (NativeLibrary.getFunction<LLVMCreatePassBuilderOptionsDelegate> "LLVMCreatePassBuilderOptions")
let llvmPassBuilderOptionsSetVerifyEach = lazy (NativeLibrary.getFunction<LLVMPassBuilderOptionsSetVerifyEachDelegate> "LLVMPassBuilderOptionsSetVerifyEach")
let llvmPassBuilderOptionsSetDebugLogging = lazy (NativeLibrary.getFunction<LLVMPassBuilderOptionsSetDebugLoggingDelegate> "LLVMPassBuilderOptionsSetDebugLogging")
let llvmDisposePassBuilderOptions = lazy (NativeLibrary.getFunction<LLVMDisposePassBuilderOptionsDelegate> "LLVMDisposePassBuilderOptions")

// Memory Buffer Functions
let llvmCreateMemoryBufferWithContentsOfFile = lazy (NativeLibrary.getFunction<LLVMCreateMemoryBufferWithContentsOfFileDelegate> "LLVMCreateMemoryBufferWithContentsOfFile")
let llvmCreateMemoryBufferWithSTDIN = lazy (NativeLibrary.getFunction<LLVMCreateMemoryBufferWithSTDINDelegate> "LLVMCreateMemoryBufferWithSTDIN")
let llvmCreateMemoryBufferWithMemoryRange = lazy (NativeLibrary.getFunction<LLVMCreateMemoryBufferWithMemoryRangeDelegate> "LLVMCreateMemoryBufferWithMemoryRange")
let llvmCreateMemoryBufferWithMemoryRangeCopy = lazy (NativeLibrary.getFunction<LLVMCreateMemoryBufferWithMemoryRangeCopyDelegate> "LLVMCreateMemoryBufferWithMemoryRangeCopy")
let llvmGetBufferStart = lazy (NativeLibrary.getFunction<LLVMGetBufferStartDelegate> "LLVMGetBufferStart")
let llvmGetBufferSize = lazy (NativeLibrary.getFunction<LLVMGetBufferSizeDelegate> "LLVMGetBufferSize")
let llvmDisposeMemoryBuffer = lazy (NativeLibrary.getFunction<LLVMDisposeMemoryBufferDelegate> "LLVMDisposeMemoryBuffer")

// =============================================================================
// Helper Functions
// =============================================================================

/// Convenience function to invoke lazy-loaded functions
let inline invoke (lazyFunc: Lazy<'T>) = lazyFunc.Value