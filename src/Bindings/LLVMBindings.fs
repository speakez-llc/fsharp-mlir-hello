module FSharpMLIR.Bindings.LLVM

open System
open System.Runtime.InteropServices
open FSharpMLIR.PlatformUtils

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

// Get the native library name for the current platform
let private getNativeLibraryName() =
    match getOS() with
    | PlatformOS.Windows -> "LLVM.dll"
    | PlatformOS.MacOS -> "libLLVM.dylib"
    | PlatformOS.Linux -> "libLLVM.so"
    | _ -> "libLLVM.so"

let private libraryName = getNativeLibraryName()

// =============================================================================
// LLVM Context Functions
// =============================================================================

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMContextRef LLVMContextCreate()

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMContextRef LLVMGetGlobalContext()

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMContextDispose(LLVMContextRef context)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMContextSetDiscardValueNames(LLVMContextRef C, LLVMBool Discard)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBool LLVMContextShouldDiscardValueNames(LLVMContextRef C)

// =============================================================================
// LLVM Module Functions
// =============================================================================

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMModuleRef LLVMModuleCreateWithName([<MarshalAs(UnmanagedType.LPStr)>] string ModuleID)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMModuleRef LLVMModuleCreateWithNameInContext([<MarshalAs(UnmanagedType.LPStr)>] string ModuleID, LLVMContextRef C)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMModuleRef LLVMCloneModule(LLVMModuleRef M)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMDisposeModule(LLVMModuleRef M)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern nativeint LLVMGetModuleIdentifier(LLVMModuleRef M, nativeint Len)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMSetModuleIdentifier(LLVMModuleRef M, [<MarshalAs(UnmanagedType.LPStr)>] string Ident, uint32 Len)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern nativeint LLVMGetSourceFileName(LLVMModuleRef M, nativeint Len)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMSetSourceFileName(LLVMModuleRef M, [<MarshalAs(UnmanagedType.LPStr)>] string Name, uint32 Len)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern nativeint LLVMGetDataLayoutStr(LLVMModuleRef M)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMSetDataLayout(LLVMModuleRef M, [<MarshalAs(UnmanagedType.LPStr)>] string DataLayoutStr)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern nativeint LLVMGetTarget(LLVMModuleRef M)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMSetTarget(LLVMModuleRef M, [<MarshalAs(UnmanagedType.LPStr)>] string Triple)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMDumpModule(LLVMModuleRef M)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBool LLVMPrintModuleToFile(LLVMModuleRef M, [<MarshalAs(UnmanagedType.LPStr)>] string Filename, nativeint ErrorMessage)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern nativeint LLVMPrintModuleToString(LLVMModuleRef M)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBool LLVMVerifyModule(LLVMModuleRef M, LLVMVerifierFailureAction Action, nativeint OutMessage)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMContextRef LLVMGetModuleContext(LLVMModuleRef M)

// =============================================================================
// LLVM Type Functions
// =============================================================================

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeKind LLVMGetTypeKind(LLVMTypeRef Ty)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBool LLVMTypeIsSized(LLVMTypeRef Ty)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMContextRef LLVMGetTypeContext(LLVMTypeRef Ty)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMDumpType(LLVMTypeRef Ty)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern nativeint LLVMPrintTypeToString(LLVMTypeRef Ty)

// Integer types
[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMInt1TypeInContext(LLVMContextRef C)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMInt8TypeInContext(LLVMContextRef C)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMInt16TypeInContext(LLVMContextRef C)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMInt32TypeInContext(LLVMContextRef C)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMInt64TypeInContext(LLVMContextRef C)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMInt128TypeInContext(LLVMContextRef C)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMIntTypeInContext(LLVMContextRef C, uint32 NumBits)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern uint32 LLVMGetIntTypeWidth(LLVMTypeRef IntegerTy)

// Floating point types
[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMHalfTypeInContext(LLVMContextRef C)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMBFloatTypeInContext(LLVMContextRef C)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMFloatTypeInContext(LLVMContextRef C)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMDoubleTypeInContext(LLVMContextRef C)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMX86FP80TypeInContext(LLVMContextRef C)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMFP128TypeInContext(LLVMContextRef C)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMPPCFP128TypeInContext(LLVMContextRef C)

// Other types
[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMVoidTypeInContext(LLVMContextRef C)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMLabelTypeInContext(LLVMContextRef C)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMX86AMXTypeInContext(LLVMContextRef C)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMTokenTypeInContext(LLVMContextRef C)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMMetadataTypeInContext(LLVMContextRef C)

// Pointer types
[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMPointerType(LLVMTypeRef ElementType, uint32 AddressSpace)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMPointerTypeInContext(LLVMContextRef C, uint32 AddressSpace)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBool LLVMPointerTypeIsOpaque(LLVMTypeRef Ty)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern uint32 LLVMGetPointerAddressSpace(LLVMTypeRef PointerTy)

// Function types
[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMFunctionType(LLVMTypeRef ReturnType, nativeint ParamTypes, uint32 ParamCount, LLVMBool IsVarArg)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBool LLVMIsFunctionVarArg(LLVMTypeRef FunctionTy)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMGetReturnType(LLVMTypeRef FunctionTy)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern uint32 LLVMCountParamTypes(LLVMTypeRef FunctionTy)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMGetParamTypes(LLVMTypeRef FunctionTy, nativeint Dest)

// Array types
[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMArrayType(LLVMTypeRef ElementType, uint32 ElementCount)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMArrayType2(LLVMTypeRef ElementType, uint64 ElementCount)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern uint32 LLVMGetArrayLength(LLVMTypeRef ArrayTy)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern uint64 LLVMGetArrayLength2(LLVMTypeRef ArrayTy)

// Vector types
[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMVectorType(LLVMTypeRef ElementType, uint32 ElementCount)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMScalableVectorType(LLVMTypeRef ElementType, uint32 ElementCount)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern uint32 LLVMGetVectorSize(LLVMTypeRef VectorTy)

// Sequential types
[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMGetElementType(LLVMTypeRef Ty)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMGetSubtypes(LLVMTypeRef Tp, nativeint Arr)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern uint32 LLVMGetNumContainedTypes(LLVMTypeRef Tp)

// Struct types
[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMStructTypeInContext(LLVMContextRef C, nativeint ElementTypes, uint32 ElementCount, LLVMBool Packed)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMStructType(nativeint ElementTypes, uint32 ElementCount, LLVMBool Packed)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMStructCreateNamed(LLVMContextRef C, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern nativeint LLVMGetStructName(LLVMTypeRef Ty)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMStructSetBody(LLVMTypeRef StructTy, nativeint ElementTypes, uint32 ElementCount, LLVMBool Packed)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern uint32 LLVMCountStructElementTypes(LLVMTypeRef StructTy)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMGetStructElementTypes(LLVMTypeRef StructTy, nativeint Dest)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMStructGetTypeAtIndex(LLVMTypeRef StructTy, uint32 i)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBool LLVMIsPackedStruct(LLVMTypeRef StructTy)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBool LLVMIsOpaqueStruct(LLVMTypeRef StructTy)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBool LLVMIsLiteralStruct(LLVMTypeRef StructTy)

// Global context versions
[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMInt1Type()

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMInt8Type()

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMInt16Type()

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMInt32Type()

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMInt64Type()

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMInt128Type()

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMIntType(uint32 NumBits)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMHalfType()

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMBFloatType()

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMFloatType()

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMDoubleType()

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMX86FP80Type()

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMFP128Type()

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMPPCFP128Type()

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMVoidType()

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMLabelType()

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMX86AMXType()

// =============================================================================
// LLVM Value and Constant Functions
// =============================================================================

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMTypeOf(LLVMValueRef Val)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMDumpValue(LLVMValueRef Val)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern nativeint LLVMPrintValueToString(LLVMValueRef Val)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern nativeint LLVMGetValueName2(LLVMValueRef Val, nativeint Length)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMSetValueName2(LLVMValueRef Val, [<MarshalAs(UnmanagedType.LPStr)>] string Name, uint32 NameLen)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMContextRef LLVMGetValueContext(LLVMValueRef Val)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMReplaceAllUsesWith(LLVMValueRef OldVal, LLVMValueRef NewVal)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBool LLVMIsConstant(LLVMValueRef Val)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBool LLVMIsUndef(LLVMValueRef Val)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBool LLVMIsPoison(LLVMValueRef Val)

// Constants
[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMConstNull(LLVMTypeRef Ty)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMConstAllOnes(LLVMTypeRef Ty)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMGetUndef(LLVMTypeRef Ty)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMGetPoison(LLVMTypeRef Ty)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBool LLVMIsNull(LLVMValueRef Val)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMConstPointerNull(LLVMTypeRef Ty)

// Scalar constants
[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMConstInt(LLVMTypeRef IntTy, uint64 N, LLVMBool SignExtend)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMConstIntOfArbitraryPrecision(LLVMTypeRef IntTy, uint32 NumWords, nativeint Words)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMConstIntOfString(LLVMTypeRef IntTy, [<MarshalAs(UnmanagedType.LPStr)>] string Text, uint8 Radix)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMConstIntOfStringAndSize(LLVMTypeRef IntTy, [<MarshalAs(UnmanagedType.LPStr)>] string Text, uint32 SLen, uint8 Radix)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMConstReal(LLVMTypeRef RealTy, double N)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMConstRealOfString(LLVMTypeRef RealTy, [<MarshalAs(UnmanagedType.LPStr)>] string Text)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMConstRealOfStringAndSize(LLVMTypeRef RealTy, [<MarshalAs(UnmanagedType.LPStr)>] string Text, uint32 SLen)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern uint64 LLVMConstIntGetZExtValue(LLVMValueRef ConstantVal)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern int64 LLVMConstIntGetSExtValue(LLVMValueRef ConstantVal)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern double LLVMConstRealGetDouble(LLVMValueRef ConstantVal, nativeint LosesInfo)

// Composite constants
[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMConstStringInContext(LLVMContextRef C, [<MarshalAs(UnmanagedType.LPStr)>] string Str, uint32 Length, LLVMBool DontNullTerminate)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMConstStringInContext2(LLVMContextRef C, [<MarshalAs(UnmanagedType.LPStr)>] string Str, uint32 Length, LLVMBool DontNullTerminate)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMConstString([<MarshalAs(UnmanagedType.LPStr)>] string Str, uint32 Length, LLVMBool DontNullTerminate)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBool LLVMIsConstantString(LLVMValueRef C)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern nativeint LLVMGetAsString(LLVMValueRef C, nativeint Length)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMConstStructInContext(LLVMContextRef C, nativeint ConstantVals, uint32 Count, LLVMBool Packed)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMConstStruct(nativeint ConstantVals, uint32 Count, LLVMBool Packed)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMConstNamedStruct(LLVMTypeRef StructTy, nativeint ConstantVals, uint32 Count)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMGetAggregateElement(LLVMValueRef C, uint32 Idx)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMConstArray(LLVMTypeRef ElementTy, nativeint ConstantVals, uint32 Length)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMConstArray2(LLVMTypeRef ElementTy, nativeint ConstantVals, uint64 Length)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMConstVector(nativeint ScalarConstantVals, uint32 Size)

// Constant expressions
[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMOpcode LLVMGetConstOpcode(LLVMValueRef ConstantVal)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMAlignOf(LLVMTypeRef Ty)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMSizeOf(LLVMTypeRef Ty)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMConstNeg(LLVMValueRef ConstantVal)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMConstNSWNeg(LLVMValueRef ConstantVal)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMConstNot(LLVMValueRef ConstantVal)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMConstAdd(LLVMValueRef LHSConstant, LLVMValueRef RHSConstant)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMConstNSWAdd(LLVMValueRef LHSConstant, LLVMValueRef RHSConstant)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMConstNUWAdd(LLVMValueRef LHSConstant, LLVMValueRef RHSConstant)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMConstSub(LLVMValueRef LHSConstant, LLVMValueRef RHSConstant)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMConstNSWSub(LLVMValueRef LHSConstant, LLVMValueRef RHSConstant)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMConstNUWSub(LLVMValueRef LHSConstant, LLVMValueRef RHSConstant)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMConstXor(LLVMValueRef LHSConstant, LLVMValueRef RHSConstant)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMConstGEP2(LLVMTypeRef Ty, LLVMValueRef ConstantVal, nativeint ConstantIndices, uint32 NumIndices)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMConstInBoundsGEP2(LLVMTypeRef Ty, LLVMValueRef ConstantVal, nativeint ConstantIndices, uint32 NumIndices)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMConstTrunc(LLVMValueRef ConstantVal, LLVMTypeRef ToType)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMConstPtrToInt(LLVMValueRef ConstantVal, LLVMTypeRef ToType)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMConstIntToPtr(LLVMValueRef ConstantVal, LLVMTypeRef ToType)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMConstBitCast(LLVMValueRef ConstantVal, LLVMTypeRef ToType)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMConstAddrSpaceCast(LLVMValueRef ConstantVal, LLVMTypeRef ToType)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMConstTruncOrBitCast(LLVMValueRef ConstantVal, LLVMTypeRef ToType)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMConstPointerCast(LLVMValueRef ConstantVal, LLVMTypeRef ToType)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMConstExtractElement(LLVMValueRef VectorConstant, LLVMValueRef IndexConstant)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMConstInsertElement(LLVMValueRef VectorConstant, LLVMValueRef ElementValueConstant, LLVMValueRef IndexConstant)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMConstShuffleVector(LLVMValueRef VectorAConstant, LLVMValueRef VectorBConstant, LLVMValueRef MaskConstant)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBlockAddress(LLVMValueRef F, LLVMBasicBlockRef BB)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMGetBlockAddressFunction(LLVMValueRef BlockAddr)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBasicBlockRef LLVMGetBlockAddressBasicBlock(LLVMValueRef BlockAddr)

// =============================================================================
// LLVM Global Values
// =============================================================================

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMModuleRef LLVMGetGlobalParent(LLVMValueRef Global)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBool LLVMIsDeclaration(LLVMValueRef Global)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMLinkage LLVMGetLinkage(LLVMValueRef Global)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMSetLinkage(LLVMValueRef Global, LLVMLinkage Linkage)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern nativeint LLVMGetSection(LLVMValueRef Global)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMSetSection(LLVMValueRef Global, [<MarshalAs(UnmanagedType.LPStr)>] string Section)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMVisibility LLVMGetVisibility(LLVMValueRef Global)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMSetVisibility(LLVMValueRef Global, LLVMVisibility Viz)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMGlobalGetValueType(LLVMValueRef Global)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern uint32 LLVMGetAlignment(LLVMValueRef V)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMSetAlignment(LLVMValueRef V, uint32 Bytes)

// Global variables
[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMAddGlobal(LLVMModuleRef M, LLVMTypeRef Ty, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMAddGlobalInAddressSpace(LLVMModuleRef M, LLVMTypeRef Ty, [<MarshalAs(UnmanagedType.LPStr)>] string Name, uint32 AddressSpace)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMGetNamedGlobal(LLVMModuleRef M, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMGetFirstGlobal(LLVMModuleRef M)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMGetLastGlobal(LLVMModuleRef M)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMGetNextGlobal(LLVMValueRef GlobalVar)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMGetPreviousGlobal(LLVMValueRef GlobalVar)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMDeleteGlobal(LLVMValueRef GlobalVar)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMGetInitializer(LLVMValueRef GlobalVar)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMSetInitializer(LLVMValueRef GlobalVar, LLVMValueRef ConstantVal)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBool LLVMIsThreadLocal(LLVMValueRef GlobalVar)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMSetThreadLocal(LLVMValueRef GlobalVar, LLVMBool IsThreadLocal)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBool LLVMIsGlobalConstant(LLVMValueRef GlobalVar)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMSetGlobalConstant(LLVMValueRef GlobalVar, LLVMBool IsConstant)

// Functions
[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMAddFunction(LLVMModuleRef M, [<MarshalAs(UnmanagedType.LPStr)>] string Name, LLVMTypeRef FunctionTy)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMGetNamedFunction(LLVMModuleRef M, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMGetFirstFunction(LLVMModuleRef M)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMGetLastFunction(LLVMModuleRef M)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMGetNextFunction(LLVMValueRef Fn)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMGetPreviousFunction(LLVMValueRef Fn)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMDeleteFunction(LLVMValueRef Fn)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBool LLVMHasPersonalityFn(LLVMValueRef Fn)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMGetPersonalityFn(LLVMValueRef Fn)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMSetPersonalityFn(LLVMValueRef Fn, LLVMValueRef PersonalityFn)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern uint32 LLVMGetIntrinsicID(LLVMValueRef Fn)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern uint32 LLVMGetFunctionCallConv(LLVMValueRef Fn)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMSetFunctionCallConv(LLVMValueRef Fn, uint32 CC)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern nativeint LLVMGetGC(LLVMValueRef Fn)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMSetGC(LLVMValueRef Fn, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern uint32 LLVMCountParams(LLVMValueRef Fn)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMGetParams(LLVMValueRef Fn, nativeint Params)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMGetParam(LLVMValueRef Fn, uint32 Index)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMGetParamParent(LLVMValueRef Inst)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMGetFirstParam(LLVMValueRef Fn)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMGetLastParam(LLVMValueRef Fn)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMGetNextParam(LLVMValueRef Arg)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMGetPreviousParam(LLVMValueRef Arg)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMSetParamAlignment(LLVMValueRef Arg, uint32 Align)

// =============================================================================
// LLVM Basic Block Functions
// =============================================================================

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBasicBlockAsValue(LLVMBasicBlockRef BB)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBool LLVMValueIsBasicBlock(LLVMValueRef Val)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBasicBlockRef LLVMValueAsBasicBlock(LLVMValueRef Val)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern nativeint LLVMGetBasicBlockName(LLVMBasicBlockRef BB)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMGetBasicBlockParent(LLVMBasicBlockRef BB)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMGetBasicBlockTerminator(LLVMBasicBlockRef BB)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern uint32 LLVMCountBasicBlocks(LLVMValueRef Fn)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMGetBasicBlocks(LLVMValueRef Fn, nativeint BasicBlocks)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBasicBlockRef LLVMGetFirstBasicBlock(LLVMValueRef Fn)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBasicBlockRef LLVMGetLastBasicBlock(LLVMValueRef Fn)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBasicBlockRef LLVMGetNextBasicBlock(LLVMBasicBlockRef BB)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBasicBlockRef LLVMGetPreviousBasicBlock(LLVMBasicBlockRef BB)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBasicBlockRef LLVMGetEntryBasicBlock(LLVMValueRef Fn)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMInsertExistingBasicBlockAfterInsertBlock(LLVMBuilderRef Builder, LLVMBasicBlockRef BB)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMAppendExistingBasicBlock(LLVMValueRef Fn, LLVMBasicBlockRef BB)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBasicBlockRef LLVMCreateBasicBlockInContext(LLVMContextRef C, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBasicBlockRef LLVMAppendBasicBlockInContext(LLVMContextRef C, LLVMValueRef Fn, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBasicBlockRef LLVMAppendBasicBlock(LLVMValueRef Fn, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBasicBlockRef LLVMInsertBasicBlockInContext(LLVMContextRef C, LLVMBasicBlockRef BB, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBasicBlockRef LLVMInsertBasicBlock(LLVMBasicBlockRef InsertBeforeBB, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMDeleteBasicBlock(LLVMBasicBlockRef BB)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMRemoveBasicBlockFromParent(LLVMBasicBlockRef BB)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMMoveBasicBlockBefore(LLVMBasicBlockRef BB, LLVMBasicBlockRef MovePos)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMMoveBasicBlockAfter(LLVMBasicBlockRef BB, LLVMBasicBlockRef MovePos)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMGetFirstInstruction(LLVMBasicBlockRef BB)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMGetLastInstruction(LLVMBasicBlockRef BB)

// =============================================================================
// LLVM Instruction Builder Functions
// =============================================================================

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBuilderRef LLVMCreateBuilderInContext(LLVMContextRef C)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBuilderRef LLVMCreateBuilder()

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMPositionBuilder(LLVMBuilderRef Builder, LLVMBasicBlockRef Block, LLVMValueRef Instr)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMPositionBuilderBefore(LLVMBuilderRef Builder, LLVMValueRef Instr)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMPositionBuilderAtEnd(LLVMBuilderRef Builder, LLVMBasicBlockRef Block)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBasicBlockRef LLVMGetInsertBlock(LLVMBuilderRef Builder)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMClearInsertionPosition(LLVMBuilderRef Builder)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMInsertIntoBuilder(LLVMBuilderRef Builder, LLVMValueRef Instr)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMInsertIntoBuilderWithName(LLVMBuilderRef Builder, LLVMValueRef Instr, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMDisposeBuilder(LLVMBuilderRef Builder)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMContextRef LLVMGetBuilderContext(LLVMBuilderRef Builder)

// Metadata
[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMMetadataRef LLVMGetCurrentDebugLocation2(LLVMBuilderRef Builder)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMSetCurrentDebugLocation2(LLVMBuilderRef Builder, LLVMMetadataRef Loc)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMAddMetadataToInst(LLVMBuilderRef Builder, LLVMValueRef Inst)

// Terminator Instructions
[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildRetVoid(LLVMBuilderRef Builder)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildRet(LLVMBuilderRef Builder, LLVMValueRef V)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildAggregateRet(LLVMBuilderRef Builder, nativeint RetVals, uint32 N)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildBr(LLVMBuilderRef Builder, LLVMBasicBlockRef Dest)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildCondBr(LLVMBuilderRef Builder, LLVMValueRef If, LLVMBasicBlockRef Then, LLVMBasicBlockRef Else)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildSwitch(LLVMBuilderRef Builder, LLVMValueRef V, LLVMBasicBlockRef Else, uint32 NumCases)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildIndirectBr(LLVMBuilderRef B, LLVMValueRef Addr, uint32 NumDests)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildInvoke2(LLVMBuilderRef Builder, LLVMTypeRef Ty, LLVMValueRef Fn, nativeint Args, uint32 NumArgs, LLVMBasicBlockRef Then, LLVMBasicBlockRef Catch, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildUnreachable(LLVMBuilderRef Builder)

// Exception Handling
[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildResume(LLVMBuilderRef B, LLVMValueRef Exn)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildLandingPad(LLVMBuilderRef B, LLVMTypeRef Ty, LLVMValueRef PersFn, uint32 NumClauses, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildCleanupRet(LLVMBuilderRef B, LLVMValueRef CatchPad, LLVMBasicBlockRef BB)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildCatchRet(LLVMBuilderRef B, LLVMValueRef CatchPad, LLVMBasicBlockRef BB)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildCatchPad(LLVMBuilderRef B, LLVMValueRef ParentPad, nativeint Args, uint32 NumArgs, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildCleanupPad(LLVMBuilderRef B, LLVMValueRef ParentPad, nativeint Args, uint32 NumArgs, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildCatchSwitch(LLVMBuilderRef B, LLVMValueRef ParentPad, LLVMBasicBlockRef UnwindBB, uint32 NumHandlers, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

// Control flow helpers
[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMAddCase(LLVMValueRef Switch, LLVMValueRef OnVal, LLVMBasicBlockRef Dest)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMAddDestination(LLVMValueRef IndirectBr, LLVMBasicBlockRef Dest)

// Arithmetic Instructions
[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildAdd(LLVMBuilderRef Builder, LLVMValueRef LHS, LLVMValueRef RHS, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildNSWAdd(LLVMBuilderRef Builder, LLVMValueRef LHS, LLVMValueRef RHS, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildNUWAdd(LLVMBuilderRef Builder, LLVMValueRef LHS, LLVMValueRef RHS, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildFAdd(LLVMBuilderRef Builder, LLVMValueRef LHS, LLVMValueRef RHS, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildSub(LLVMBuilderRef Builder, LLVMValueRef LHS, LLVMValueRef RHS, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildNSWSub(LLVMBuilderRef Builder, LLVMValueRef LHS, LLVMValueRef RHS, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildNUWSub(LLVMBuilderRef Builder, LLVMValueRef LHS, LLVMValueRef RHS, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildFSub(LLVMBuilderRef Builder, LLVMValueRef LHS, LLVMValueRef RHS, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildMul(LLVMBuilderRef Builder, LLVMValueRef LHS, LLVMValueRef RHS, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildNSWMul(LLVMBuilderRef Builder, LLVMValueRef LHS, LLVMValueRef RHS, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildNUWMul(LLVMBuilderRef Builder, LLVMValueRef LHS, LLVMValueRef RHS, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildFMul(LLVMBuilderRef Builder, LLVMValueRef LHS, LLVMValueRef RHS, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildUDiv(LLVMBuilderRef Builder, LLVMValueRef LHS, LLVMValueRef RHS, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildExactUDiv(LLVMBuilderRef Builder, LLVMValueRef LHS, LLVMValueRef RHS, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildSDiv(LLVMBuilderRef Builder, LLVMValueRef LHS, LLVMValueRef RHS, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildExactSDiv(LLVMBuilderRef Builder, LLVMValueRef LHS, LLVMValueRef RHS, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildFDiv(LLVMBuilderRef Builder, LLVMValueRef LHS, LLVMValueRef RHS, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildURem(LLVMBuilderRef Builder, LLVMValueRef LHS, LLVMValueRef RHS, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildSRem(LLVMBuilderRef Builder, LLVMValueRef LHS, LLVMValueRef RHS, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildFRem(LLVMBuilderRef Builder, LLVMValueRef LHS, LLVMValueRef RHS, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildShl(LLVMBuilderRef Builder, LLVMValueRef LHS, LLVMValueRef RHS, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildLShr(LLVMBuilderRef Builder, LLVMValueRef LHS, LLVMValueRef RHS, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildAShr(LLVMBuilderRef Builder, LLVMValueRef LHS, LLVMValueRef RHS, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildAnd(LLVMBuilderRef Builder, LLVMValueRef LHS, LLVMValueRef RHS, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildOr(LLVMBuilderRef Builder, LLVMValueRef LHS, LLVMValueRef RHS, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildXor(LLVMBuilderRef Builder, LLVMValueRef LHS, LLVMValueRef RHS, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildBinOp(LLVMBuilderRef B, LLVMOpcode Op, LLVMValueRef LHS, LLVMValueRef RHS, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildNeg(LLVMBuilderRef Builder, LLVMValueRef V, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildNSWNeg(LLVMBuilderRef B, LLVMValueRef V, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildFNeg(LLVMBuilderRef Builder, LLVMValueRef V, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildNot(LLVMBuilderRef Builder, LLVMValueRef V, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

// Arithmetic flags
[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBool LLVMGetNUW(LLVMValueRef ArithInst)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMSetNUW(LLVMValueRef ArithInst, LLVMBool HasNUW)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBool LLVMGetNSW(LLVMValueRef ArithInst)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMSetNSW(LLVMValueRef ArithInst, LLVMBool HasNSW)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBool LLVMGetExact(LLVMValueRef DivOrShrInst)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMSetExact(LLVMValueRef DivOrShrInst, LLVMBool IsExact)

// Memory Instructions
[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildAlloca(LLVMBuilderRef Builder, LLVMTypeRef Ty, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildArrayAlloca(LLVMBuilderRef Builder, LLVMTypeRef Ty, LLVMValueRef Val, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildLoad2(LLVMBuilderRef Builder, LLVMTypeRef Ty, LLVMValueRef PointerVal, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildStore(LLVMBuilderRef Builder, LLVMValueRef Val, LLVMValueRef Ptr)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildGEP2(LLVMBuilderRef B, LLVMTypeRef Ty, LLVMValueRef Pointer, nativeint Indices, uint32 NumIndices, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildInBoundsGEP2(LLVMBuilderRef B, LLVMTypeRef Ty, LLVMValueRef Pointer, nativeint Indices, uint32 NumIndices, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildStructGEP2(LLVMBuilderRef B, LLVMTypeRef Ty, LLVMValueRef Pointer, uint32 Idx, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildGlobalString(LLVMBuilderRef B, [<MarshalAs(UnmanagedType.LPStr)>] string Str, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildGlobalStringPtr(LLVMBuilderRef B, [<MarshalAs(UnmanagedType.LPStr)>] string Str, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBool LLVMGetVolatile(LLVMValueRef MemoryAccessInst)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMSetVolatile(LLVMValueRef MemoryAccessInst, LLVMBool IsVolatile)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMAtomicOrdering LLVMGetOrdering(LLVMValueRef MemoryAccessInst)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMSetOrdering(LLVMValueRef MemoryAccessInst, LLVMAtomicOrdering Ordering)

// Cast Instructions
[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildTrunc(LLVMBuilderRef Builder, LLVMValueRef Val, LLVMTypeRef DestTy, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildZExt(LLVMBuilderRef Builder, LLVMValueRef Val, LLVMTypeRef DestTy, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildSExt(LLVMBuilderRef Builder, LLVMValueRef Val, LLVMTypeRef DestTy, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildFPToUI(LLVMBuilderRef Builder, LLVMValueRef Val, LLVMTypeRef DestTy, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildFPToSI(LLVMBuilderRef Builder, LLVMValueRef Val, LLVMTypeRef DestTy, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]  
extern LLVMValueRef LLVMBuildUIToFP(LLVMBuilderRef Builder, LLVMValueRef Val, LLVMTypeRef DestTy, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildSIToFP(LLVMBuilderRef Builder, LLVMValueRef Val, LLVMTypeRef DestTy, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildFPTrunc(LLVMBuilderRef Builder, LLVMValueRef Val, LLVMTypeRef DestTy, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildFPExt(LLVMBuilderRef Builder, LLVMValueRef Val, LLVMTypeRef DestTy, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildPtrToInt(LLVMBuilderRef Builder, LLVMValueRef Val, LLVMTypeRef DestTy, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildIntToPtr(LLVMBuilderRef Builder, LLVMValueRef Val, LLVMTypeRef DestTy, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildBitCast(LLVMBuilderRef Builder, LLVMValueRef Val, LLVMTypeRef DestTy, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildAddrSpaceCast(LLVMBuilderRef Builder, LLVMValueRef Val, LLVMTypeRef DestTy, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildPointerCast(LLVMBuilderRef Builder, LLVMValueRef Val, LLVMTypeRef DestTy, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildIntCast2(LLVMBuilderRef Builder, LLVMValueRef Val, LLVMTypeRef DestTy, LLVMBool IsSigned, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildFPCast(LLVMBuilderRef Builder, LLVMValueRef Val, LLVMTypeRef DestTy, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildCast(LLVMBuilderRef B, LLVMOpcode Op, LLVMValueRef Val, LLVMTypeRef DestTy, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMOpcode LLVMGetCastOpcode(LLVMValueRef Src, LLVMBool SrcIsSigned, LLVMTypeRef DestTy, LLVMBool DestIsSigned)

// Comparison Instructions
[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildICmp(LLVMBuilderRef Builder, LLVMIntPredicate Op, LLVMValueRef LHS, LLVMValueRef RHS, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildFCmp(LLVMBuilderRef Builder, LLVMRealPredicate Op, LLVMValueRef LHS, LLVMValueRef RHS, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

// Other Instructions
[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildPhi(LLVMBuilderRef Builder, LLVMTypeRef Ty, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildCall2(LLVMBuilderRef Builder, LLVMTypeRef Ty, LLVMValueRef Fn, nativeint Args, uint32 NumArgs, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildSelect(LLVMBuilderRef Builder, LLVMValueRef If, LLVMValueRef Then, LLVMValueRef Else, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildVAArg(LLVMBuilderRef Builder, LLVMValueRef List, LLVMTypeRef Ty, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildExtractElement(LLVMBuilderRef Builder, LLVMValueRef VecVal, LLVMValueRef Index, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildInsertElement(LLVMBuilderRef Builder, LLVMValueRef VecVal, LLVMValueRef EltVal, LLVMValueRef Index, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildShuffleVector(LLVMBuilderRef Builder, LLVMValueRef V1, LLVMValueRef V2, LLVMValueRef Mask, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildExtractValue(LLVMBuilderRef Builder, LLVMValueRef AggVal, uint32 Index, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildInsertValue(LLVMBuilderRef Builder, LLVMValueRef AggVal, LLVMValueRef EltVal, uint32 Index, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildFreeze(LLVMBuilderRef Builder, LLVMValueRef Val, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildIsNull(LLVMBuilderRef Builder, LLVMValueRef Val, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildIsNotNull(LLVMBuilderRef Builder, LLVMValueRef Val, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildPtrDiff2(LLVMBuilderRef Builder, LLVMTypeRef ElemTy, LLVMValueRef LHS, LLVMValueRef RHS, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildFence(LLVMBuilderRef B, LLVMAtomicOrdering ordering, LLVMBool singleThread, [<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildAtomicRMW(LLVMBuilderRef B, LLVMAtomicRMWBinOp op, LLVMValueRef PTR, LLVMValueRef Val, LLVMAtomicOrdering ordering, LLVMBool singleThread)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMBuildAtomicCmpXchg(LLVMBuilderRef B, LLVMValueRef Ptr, LLVMValueRef Cmp, LLVMValueRef New, LLVMAtomicOrdering SuccessOrdering, LLVMAtomicOrdering FailureOrdering, LLVMBool SingleThread)

// Atomic helpers
[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBool LLVMIsAtomicSingleThread(LLVMValueRef AtomicInst)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMSetAtomicSingleThread(LLVMValueRef AtomicInst, LLVMBool SingleThread)

// PHI Node Functions
[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMAddIncoming(LLVMValueRef PhiNode, nativeint IncomingValues, nativeint IncomingBlocks, uint32 Count)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern uint32 LLVMCountIncoming(LLVMValueRef PhiNode)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMGetIncomingValue(LLVMValueRef PhiNode, uint32 Index)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBasicBlockRef LLVMGetIncomingBlock(LLVMValueRef PhiNode, uint32 Index)

// =============================================================================
// LLVM Instruction Functions
// =============================================================================

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern int LLVMHasMetadata(LLVMValueRef Val)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMGetMetadata(LLVMValueRef Val, uint32 KindID)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMSetMetadata(LLVMValueRef Val, uint32 KindID, LLVMValueRef Node)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBasicBlockRef LLVMGetInstructionParent(LLVMValueRef Inst)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMGetNextInstruction(LLVMValueRef Inst)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMGetPreviousInstruction(LLVMValueRef Inst)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMInstructionRemoveFromParent(LLVMValueRef Inst)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMInstructionEraseFromParent(LLVMValueRef Inst)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMDeleteInstruction(LLVMValueRef Inst)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMOpcode LLVMGetInstructionOpcode(LLVMValueRef Inst)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMIntPredicate LLVMGetICmpPredicate(LLVMValueRef Inst)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMRealPredicate LLVMGetFCmpPredicate(LLVMValueRef Inst)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMInstructionClone(LLVMValueRef Inst)

// Call site functions
[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern uint32 LLVMGetNumArgOperands(LLVMValueRef Instr)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMSetInstructionCallConv(LLVMValueRef Instr, uint32 CC)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern uint32 LLVMGetInstructionCallConv(LLVMValueRef Instr)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMGetCalledFunctionType(LLVMValueRef C)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMGetCalledValue(LLVMValueRef Instr)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBool LLVMIsTailCall(LLVMValueRef CallInst)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMSetTailCall(LLVMValueRef CallInst, LLVMBool IsTailCall)

// Terminator functions
[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern uint32 LLVMGetNumSuccessors(LLVMValueRef Term)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBasicBlockRef LLVMGetSuccessor(LLVMValueRef Term, uint32 i)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMSetSuccessor(LLVMValueRef Term, uint32 i, LLVMBasicBlockRef block)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBool LLVMIsConditional(LLVMValueRef Branch)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMValueRef LLVMGetCondition(LLVMValueRef Branch)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMSetCondition(LLVMValueRef Branch, LLVMValueRef Cond)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBasicBlockRef LLVMGetSwitchDefaultDest(LLVMValueRef SwitchInstr)

// Alloca functions
[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMGetAllocatedType(LLVMValueRef Alloca)

// GEP functions
[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBool LLVMIsInBounds(LLVMValueRef GEP)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMSetIsInBounds(LLVMValueRef GEP, LLVMBool InBounds)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMGetGEPSourceElementType(LLVMValueRef GEP)

// Extract/Insert value functions
[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern uint32 LLVMGetNumIndices(LLVMValueRef Inst)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern nativeint LLVMGetIndices(LLVMValueRef Inst)

// =============================================================================
// LLVM Pass Manager Functions
// =============================================================================

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMPassManagerRef LLVMCreatePassManager()

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMPassManagerRef LLVMCreateFunctionPassManagerForModule(LLVMModuleRef M)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBool LLVMRunPassManager(LLVMPassManagerRef PM, LLVMModuleRef M)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBool LLVMInitializeFunctionPassManager(LLVMPassManagerRef FPM)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBool LLVMRunFunctionPassManager(LLVMPassManagerRef FPM, LLVMValueRef F)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBool LLVMFinalizeFunctionPassManager(LLVMPassManagerRef FPM)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMDisposePassManager(LLVMPassManagerRef PM)

// Optimization Passes
[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMAddInstructionCombiningPass(LLVMPassManagerRef PM)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMAddPromoteMemoryToRegisterPass(LLVMPassManagerRef PM)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMAddGVNPass(LLVMPassManagerRef PM)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMAddCFGSimplificationPass(LLVMPassManagerRef PM)

// =============================================================================
// LLVM Target Functions
// =============================================================================

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern nativeint LLVMGetDefaultTargetTriple()

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern nativeint LLVMGetHostCPUName()

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern nativeint LLVMGetHostCPUFeatures()

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBool LLVMInitializeNativeTarget()

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBool LLVMInitializeNativeAsmPrinter()

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBool LLVMGetTargetFromTriple([<MarshalAs(UnmanagedType.LPStr)>] string Triple, nativeint T, nativeint ErrorMessage)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTargetRef LLVMGetTargetFromName([<MarshalAs(UnmanagedType.LPStr)>] string Name)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern nativeint LLVMGetTargetName(LLVMTargetRef T)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern nativeint LLVMGetTargetDescription(LLVMTargetRef T)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBool LLVMTargetHasJIT(LLVMTargetRef T)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBool LLVMTargetHasTargetMachine(LLVMTargetRef T)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBool LLVMTargetHasAsmBackend(LLVMTargetRef T)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTargetMachineRef LLVMCreateTargetMachine(
    LLVMTargetRef T,
    [<MarshalAs(UnmanagedType.LPStr)>] string Triple,
    [<MarshalAs(UnmanagedType.LPStr)>] string CPU,
    [<MarshalAs(UnmanagedType.LPStr)>] string Features,
    LLVMCodeGenOptLevel Level,
    LLVMRelocMode Reloc,
    LLVMCodeModel CodeModel)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMDisposeTargetMachine(LLVMTargetMachineRef T)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTargetRef LLVMGetTargetMachineTarget(LLVMTargetMachineRef T)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern nativeint LLVMGetTargetMachineTriple(LLVMTargetMachineRef T)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern nativeint LLVMGetTargetMachineCPU(LLVMTargetMachineRef T)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern nativeint LLVMGetTargetMachineFeatureString(LLVMTargetMachineRef T)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTargetDataRef LLVMCreateTargetDataLayout(LLVMTargetMachineRef T)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBool LLVMTargetMachineEmitToFile(
    LLVMTargetMachineRef T,
    LLVMModuleRef M,
    [<MarshalAs(UnmanagedType.LPStr)>] string Filename,
    LLVMCodeGenFileType Codegen,
    nativeint ErrorMessage)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBool LLVMTargetMachineEmitToMemoryBuffer(LLVMTargetMachineRef T, LLVMModuleRef M, LLVMCodeGenFileType codegen, nativeint ErrorMessage, nativeint OutMemBuf)

// =============================================================================
// LLVM Target Data Functions
// =============================================================================

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTargetDataRef LLVMGetModuleDataLayout(LLVMModuleRef M)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMSetModuleDataLayout(LLVMModuleRef M, LLVMTargetDataRef DL)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTargetDataRef LLVMCreateTargetData([<MarshalAs(UnmanagedType.LPStr)>] string StringRep)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMDisposeTargetData(LLVMTargetDataRef TD)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern nativeint LLVMCopyStringRepOfTargetData(LLVMTargetDataRef TD)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern uint32 LLVMPointerSize(LLVMTargetDataRef TD)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern uint32 LLVMPointerSizeForAS(LLVMTargetDataRef TD, uint32 AS)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMIntPtrType(LLVMTargetDataRef TD)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMIntPtrTypeForAS(LLVMTargetDataRef TD, uint32 AS)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMIntPtrTypeInContext(LLVMContextRef C, LLVMTargetDataRef TD)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMTypeRef LLVMIntPtrTypeForASInContext(LLVMContextRef C, LLVMTargetDataRef TD, uint32 AS)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern uint64 LLVMSizeOfTypeInBits(LLVMTargetDataRef TD, LLVMTypeRef Ty)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern uint64 LLVMStoreSizeOfType(LLVMTargetDataRef TD, LLVMTypeRef Ty)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern uint64 LLVMABISizeOfType(LLVMTargetDataRef TD, LLVMTypeRef Ty)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern uint32 LLVMABIAlignmentOfType(LLVMTargetDataRef TD, LLVMTypeRef Ty)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern uint32 LLVMCallFrameAlignmentOfType(LLVMTargetDataRef TD, LLVMTypeRef Ty)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern uint32 LLVMPreferredAlignmentOfType(LLVMTargetDataRef TD, LLVMTypeRef Ty)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern uint32 LLVMPreferredAlignmentOfGlobal(LLVMTargetDataRef TD, LLVMValueRef GlobalVar)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern uint32 LLVMElementAtOffset(LLVMTargetDataRef TD, LLVMTypeRef StructTy, uint64 Offset)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern uint64 LLVMOffsetOfElement(LLVMTargetDataRef TD, LLVMTypeRef StructTy, uint32 Element)

// =============================================================================
// LLVM Initialization Functions
// =============================================================================

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMInitializeAllTargetInfos()

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMInitializeAllTargets()

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMInitializeAllTargetMCs()

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMInitializeAllAsmPrinters()

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMInitializeAllAsmParsers()

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMInitializeAllDisassemblers()

// =============================================================================
// LLVM Memory Management Functions
// =============================================================================

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMDisposeMessage(nativeint Message)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMShutdown()

// =============================================================================
// LLVM Version Functions
// =============================================================================

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMGetVersion(nativeint Major, nativeint Minor, nativeint Patch)

// =============================================================================
// LLVM Error Handling
// =============================================================================

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern nativeint LLVMCreateMessage([<MarshalAs(UnmanagedType.LPStr)>] string Message)

// =============================================================================
// LLVM New Pass Manager Functions (PassBuilder)
// =============================================================================

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMErrorRef LLVMRunPasses(LLVMModuleRef M, [<MarshalAs(UnmanagedType.LPStr)>] string Passes, LLVMTargetMachineRef TM, LLVMPassBuilderOptionsRef Options)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMErrorRef LLVMRunPassesOnFunction(LLVMValueRef F, [<MarshalAs(UnmanagedType.LPStr)>] string Passes, LLVMTargetMachineRef TM, LLVMPassBuilderOptionsRef Options)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMPassBuilderOptionsRef LLVMCreatePassBuilderOptions()

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMPassBuilderOptionsSetVerifyEach(LLVMPassBuilderOptionsRef Options, LLVMBool VerifyEach)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMPassBuilderOptionsSetDebugLogging(LLVMPassBuilderOptionsRef Options, LLVMBool DebugLogging)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMDisposePassBuilderOptions(LLVMPassBuilderOptionsRef Options)

// =============================================================================
// LLVM Memory Buffer Functions  
// =============================================================================

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBool LLVMCreateMemoryBufferWithContentsOfFile([<MarshalAs(UnmanagedType.LPStr)>] string Path, nativeint OutMemBuf, nativeint OutMessage)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMBool LLVMCreateMemoryBufferWithSTDIN(nativeint OutMemBuf, nativeint OutMessage)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMMemoryBufferRef LLVMCreateMemoryBufferWithMemoryRange([<MarshalAs(UnmanagedType.LPStr)>] string InputData, uint32 InputDataLength, [<MarshalAs(UnmanagedType.LPStr)>] string BufferName, LLVMBool RequiresNullTerminator)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern LLVMMemoryBufferRef LLVMCreateMemoryBufferWithMemoryRangeCopy([<MarshalAs(UnmanagedType.LPStr)>] string InputData, uint32 InputDataLength, [<MarshalAs(UnmanagedType.LPStr)>] string BufferName)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern nativeint LLVMGetBufferStart(LLVMMemoryBufferRef MemBuf)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern uint32 LLVMGetBufferSize(LLVMMemoryBufferRef MemBuf)

[<DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern void LLVMDisposeMemoryBuffer(LLVMMemoryBufferRef MemBuf)

// =============================================================================
// Helper Functions for F# Integration
// =============================================================================

/// Get string from target triple pointer, handling disposal
let LLVMGetDefaultTargetTripleString() : string =
    try
        let ptr = LLVMGetDefaultTargetTriple()
        if ptr = nativeint 0 then
            "unknown-unknown-unknown"
        else
            let result = Marshal.PtrToStringAnsi(ptr)
            LLVMDisposeMessage(ptr)
            if String.IsNullOrEmpty(result) then "unknown-unknown-unknown" else result
    with
    | ex ->
        printfn "Error getting default target triple: %s" ex.Message
        "unknown-unknown-unknown"

/// Get string from host CPU name pointer, handling disposal
let LLVMGetHostCPUNameString() : string =
    try
        let ptr = LLVMGetHostCPUName()
        if ptr = nativeint 0 then
            "generic"
        else
            let result = Marshal.PtrToStringAnsi(ptr)
            LLVMDisposeMessage(ptr)
            if String.IsNullOrEmpty(result) then "generic" else result
    with
    | ex ->
        printfn "Error getting host CPU name: %s" ex.Message
        "generic"

/// Get string from host CPU features pointer, handling disposal
let LLVMGetHostCPUFeaturesString() : string =
    try
        let ptr = LLVMGetHostCPUFeatures()
        if ptr = nativeint 0 then
            ""
        else
            let result = Marshal.PtrToStringAnsi(ptr)
            LLVMDisposeMessage(ptr)
            if String.IsNullOrEmpty(result) then "" else result
    with
    | ex ->
        printfn "Error getting host CPU features: %s" ex.Message
        ""

/// Initialize LLVM with comprehensive error handling
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

/// Test LLVM availability with detailed error reporting
let LLVMTestAvailability() : bool =
    try
        printfn "Testing LLVM library availability..."
        
        // Test basic context creation
        let context = LLVMContextCreate()
        if context = nativeint 0 then
            printfn "Failed to create LLVM context"
            false
        else
            printfn "✓ LLVM context creation successful"
            
            // Test module creation
            let testModule = LLVMModuleCreateWithNameInContext("test_module", context)
            if testModule = nativeint 0 then
                printfn "Failed to create LLVM module"
                LLVMContextDispose(context)
                false
            else
                printfn "✓ LLVM module creation successful"
                
                // Test target triple retrieval
                let triple = LLVMGetDefaultTargetTripleString()
                printfn "✓ Target triple: %s" triple
                
                // Clean up
                LLVMDisposeModule(testModule)
                LLVMContextDispose(context)
                
                triple <> "unknown-unknown-unknown"
    with
    | ex ->
        printfn "LLVM availability test failed: %s" ex.Message
        printfn "Stack trace: %s" ex.StackTrace
        false

/// Create a function type with parameter types
let LLVMCreateFunctionType (returnType: LLVMTypeRef) (paramTypes: LLVMTypeRef[]) (isVarArg: bool) : LLVMTypeRef =
    let paramCount = uint32 paramTypes.Length
    let isVarArgBool = if isVarArg then 1 else 0
    
    if paramTypes.Length = 0 then
        LLVMFunctionType(returnType, nativeint 0, 0u, isVarArgBool)
    else
        let paramTypesPtr = Marshal.AllocHGlobal(nativeint (paramTypes.Length * sizeof<nativeint>))
        try
            for i = 0 to paramTypes.Length - 1 do
                Marshal.WriteIntPtr(paramTypesPtr, i * sizeof<nativeint>, paramTypes.[i])
            
            LLVMFunctionType(returnType, paramTypesPtr, paramCount, isVarArgBool)
        finally
            Marshal.FreeHGlobal(paramTypesPtr)

/// Safe wrapper for getting target from triple
let LLVMGetTargetFromTripleSafe (triple: string) : Result<LLVMTargetRef, string> =
    try
        let targetPtr = Marshal.AllocHGlobal(sizeof<nativeint>)
        let errorPtr = Marshal.AllocHGlobal(sizeof<nativeint>)
        
        try
            let result = LLVMGetTargetFromTriple(triple, targetPtr, errorPtr)
            
            if result <> 0 then
                let errorMsgPtr = Marshal.ReadIntPtr(errorPtr)
                if errorMsgPtr <> nativeint 0 then
                    let errorMsg = Marshal.PtrToStringAnsi(errorMsgPtr)
                    LLVMDisposeMessage(errorMsgPtr)
                    Error(sprintf "Failed to get target for triple '%s': %s" triple errorMsg)
                else
                    Error(sprintf "Failed to get target for triple '%s': Unknown error" triple)
            else
                let target = Marshal.ReadIntPtr(targetPtr)
                Ok(target)
        finally
            Marshal.FreeHGlobal(targetPtr)
            Marshal.FreeHGlobal(errorPtr)
    with
    | ex ->
        Error(sprintf "Exception getting target from triple '%s': %s" triple ex.Message)

/// Build a call instruction with proper argument marshaling
let LLVMBuildCall2Safe (builder: LLVMBuilderRef) (fnType: LLVMTypeRef) (fn: LLVMValueRef) (args: LLVMValueRef[]) (name: string) : LLVMValueRef =
    let numArgs = uint32 args.Length
    
    if args.Length = 0 then
        LLVMBuildCall2(builder, fnType, fn, nativeint 0, 0u, name)
    else
        let argsPtr = Marshal.AllocHGlobal(nativeint (args.Length * sizeof<nativeint>))
        try
            for i = 0 to args.Length - 1 do
                Marshal.WriteIntPtr(argsPtr, i * sizeof<nativeint>, args.[i])
            
            LLVMBuildCall2(builder, fnType, fn, argsPtr, numArgs, name)
        finally
            Marshal.FreeHGlobal(argsPtr)

/// Build GEP instruction with proper index marshaling
let LLVMBuildGEP2Safe (builder: LLVMBuilderRef) (ty: LLVMTypeRef) (pointer: LLVMValueRef) (indices: LLVMValueRef[]) (name: string) : LLVMValueRef =
    let numIndices = uint32 indices.Length
    
    if indices.Length = 0 then
        LLVMBuildGEP2(builder, ty, pointer, nativeint 0, 0u, name)
    else
        let indicesPtr = Marshal.AllocHGlobal(nativeint (indices.Length * sizeof<nativeint>))
        try
            for i = 0 to indices.Length - 1 do
                Marshal.WriteIntPtr(indicesPtr, i * sizeof<nativeint>, indices.[i])
            
            LLVMBuildGEP2(builder, ty, pointer, indicesPtr, numIndices, name)
        finally
            Marshal.FreeHGlobal(indicesPtr)

/// Add incoming values to PHI node with proper marshaling
let LLVMAddIncomingSafe (phiNode: LLVMValueRef) (incomingValues: LLVMValueRef[]) (incomingBlocks: LLVMBasicBlockRef[]) : unit =
    if incomingValues.Length <> incomingBlocks.Length then
        invalidArg "incomingValues/incomingBlocks" "Arrays must have the same length"
    
    let count = uint32 incomingValues.Length
    
    if incomingValues.Length > 0 then
        let valuesPtr = Marshal.AllocHGlobal(nativeint (incomingValues.Length * sizeof<nativeint>))
        let blocksPtr = Marshal.AllocHGlobal(nativeint (incomingBlocks.Length * sizeof<nativeint>))
        
        try
            for i = 0 to incomingValues.Length - 1 do
                Marshal.WriteIntPtr(valuesPtr, i * sizeof<nativeint>, incomingValues.[i])
                Marshal.WriteIntPtr(blocksPtr, i * sizeof<nativeint>, incomingBlocks.[i])
            
            LLVMAddIncoming(phiNode, valuesPtr, blocksPtr, count)
        finally
            Marshal.FreeHGlobal(valuesPtr)
            Marshal.FreeHGlobal(blocksPtr)

/// Create struct type with proper marshaling
let LLVMCreateStructType (context: LLVMContextRef) (elementTypes: LLVMTypeRef[]) (packed: bool) : LLVMTypeRef =
    let elementCount = uint32 elementTypes.Length
    let packedBool = if packed then 1 else 0
    
    if elementTypes.Length = 0 then
        LLVMStructTypeInContext(context, nativeint 0, 0u, packedBool)
    else
        let elemTypesPtr = Marshal.AllocHGlobal(nativeint (elementTypes.Length * sizeof<nativeint>))
        try
            for i = 0 to elementTypes.Length - 1 do
                Marshal.WriteIntPtr(elemTypesPtr, i * sizeof<nativeint>, elementTypes.[i])
            
            LLVMStructTypeInContext(context, elemTypesPtr, elementCount, packedBool)
        finally
            Marshal.FreeHGlobal(elemTypesPtr)

/// Create array constant with proper marshaling
let LLVMCreateConstantArray (elementType: LLVMTypeRef) (constantValues: LLVMValueRef[]) : LLVMValueRef =
    let length = uint32 constantValues.Length
    
    if constantValues.Length = 0 then
        LLVMConstArray(elementType, nativeint 0, 0u)
    else
        let valuesPtr = Marshal.AllocHGlobal(nativeint (constantValues.Length * sizeof<nativeint>))
        try
            for i = 0 to constantValues.Length - 1 do
                Marshal.WriteIntPtr(valuesPtr, i * sizeof<nativeint>, constantValues.[i])
            
            LLVMConstArray(elementType, valuesPtr, length)
        finally
            Marshal.FreeHGlobal(valuesPtr)

/// Create vector constant with proper marshaling
let LLVMCreateConstantVector (scalarConstants: LLVMValueRef[]) : LLVMValueRef =
    let size = uint32 scalarConstants.Length
    
    if scalarConstants.Length = 0 then
        LLVMConstVector(nativeint 0, 0u)
    else
        let valuesPtr = Marshal.AllocHGlobal(nativeint (scalarConstants.Length * sizeof<nativeint>))
        try
            for i = 0 to scalarConstants.Length - 1 do
                Marshal.WriteIntPtr(valuesPtr, i * sizeof<nativeint>, scalarConstants.[i])
            
            LLVMConstVector(valuesPtr, size)
        finally
            Marshal.FreeHGlobal(valuesPtr)

/// Get string from LLVM string pointer with safe disposal
let LLVMGetStringAndDispose (ptr: nativeint) : string =
    if ptr = nativeint 0 then
        ""
    else
        try
            let result = Marshal.PtrToStringAnsi(ptr)
            LLVMDisposeMessage(ptr)
            if result = null then "" else result
        with
        | ex ->
            printfn "Error converting LLVM string: %s" ex.Message
            ""

/// Safe module verification with error message handling
let LLVMVerifyModuleSafe (moduleRef: LLVMModuleRef) : Result<unit, string> =
    try
        let errorPtr = Marshal.AllocHGlobal(sizeof<nativeint>)
        try
            let result = LLVMVerifyModule(moduleRef, LLVMVerifierFailureAction.ReturnStatus, errorPtr)
            if result <> 0 then
                let errorMsgPtr = Marshal.ReadIntPtr(errorPtr)
                if errorMsgPtr <> nativeint 0 then
                    let errorMsg = Marshal.PtrToStringAnsi(errorMsgPtr)
                    LLVMDisposeMessage(errorMsgPtr)
                    Error(errorMsg)
                else
                    Error("Module verification failed with unknown error")
            else
                Ok(())
        finally
            Marshal.FreeHGlobal(errorPtr)
    with
    | ex ->
        Error(sprintf "Exception during module verification: %s" ex.Message)

/// Safe target machine creation for Fidelity compiler
let LLVMCreateTargetMachineSafe (triple: string) (cpu: string) (features: string) (optLevel: LLVMCodeGenOptLevel) (relocMode: LLVMRelocMode) (codeModel: LLVMCodeModel) : Result<LLVMTargetMachineRef, string> =
    match LLVMGetTargetFromTripleSafe(triple) with
    | Error(err) -> Error(err)
    | Ok(target) ->
        try
            let targetMachine = LLVMCreateTargetMachine(target, triple, cpu, features, optLevel, relocMode, codeModel)
            if targetMachine = nativeint 0 then
                Error("Failed to create target machine")
            else
                Ok(targetMachine)
        with
        | ex ->
            Error(sprintf "Exception creating target machine: %s" ex.Message)

/// Safe code generation to file for Fidelity compiler
let LLVMEmitToFileSafe (targetMachine: LLVMTargetMachineRef) (moduleRef: LLVMModuleRef) (filename: string) (fileType: LLVMCodeGenFileType) : Result<unit, string> =
    try
        let errorPtr = Marshal.AllocHGlobal(sizeof<nativeint>)
        try
            let result = LLVMTargetMachineEmitToFile(targetMachine, moduleRef, filename, fileType, errorPtr)
            if result <> 0 then
                let errorMsgPtr = Marshal.ReadIntPtr(errorPtr)
                if errorMsgPtr <> nativeint 0 then
                    let errorMsg = Marshal.PtrToStringAnsi(errorMsgPtr)
                    LLVMDisposeMessage(errorMsgPtr)
                    Error(sprintf "Code generation failed: %s" errorMsg)
                else
                    Error("Code generation failed with unknown error")
            else
                Ok(())
        finally
            Marshal.FreeHGlobal(errorPtr)
    with
    | ex ->
        Error(sprintf "Exception during code generation: %s" ex.Message)

/// Comprehensive cleanup function for LLVM resources
let LLVMCleanupResources (resources: (string * (unit -> unit)) list) : unit =
    resources
    |> List.rev  // Dispose in reverse order
    |> List.iter (fun (name, dispose) ->
        try
            dispose()
            printfn "✓ Disposed %s" name
        with
        | ex ->
            printfn "✗ Error disposing %s: %s" name ex.Message)

/// Create a complete compilation pipeline for Fidelity
type FidelityLLVMPipeline = {
    Context: LLVMContextRef
    Module: LLVMModuleRef
    Builder: LLVMBuilderRef
    TargetMachine: LLVMTargetMachineRef option
    PassManager: LLVMPassManagerRef option
}

/// Initialize a complete LLVM pipeline for Fidelity compiler
let LLVMCreateFidelityPipeline (moduleName: string) (targetTriple: string option) : Result<FidelityLLVMPipeline, string> =
    try
        // Initialize LLVM
        if not (LLVMInitializeAllSafe()) then
            Error("Failed to initialize LLVM")
        else
            let context = LLVMContextCreate()
            if context = nativeint 0 then
                Error("Failed to create LLVM context")
            else
                let moduleRef = LLVMModuleCreateWithNameInContext(moduleName, context)
                if moduleRef = nativeint 0 then
                    LLVMContextDispose(context)
                    Error("Failed to create LLVM module")
                else
                    let builder = LLVMCreateBuilderInContext(context)
                    if builder = nativeint 0 then
                        LLVMDisposeModule(moduleRef)
                        LLVMContextDispose(context)
                        Error("Failed to create LLVM builder")
                    else
                        // Set target triple
                        let triple = 
                            match targetTriple with
                            | Some(t) -> t
                            | None -> LLVMGetDefaultTargetTripleString()
                        
                        LLVMSetTarget(moduleRef, triple)
                        
                        // Optionally create target machine
                        let targetMachineResult = 
                            match LLVMCreateTargetMachineSafe(triple, "generic", "", LLVMCodeGenOptLevel.Default, LLVMRelocMode.Default, LLVMCodeModel.Default) with
                            | Ok(tm) -> 
                                // Set data layout from target machine
                                let dataLayout = LLVMCreateTargetDataLayout(tm)
                                LLVMSetModuleDataLayout(moduleRef, dataLayout)
                                LLVMDisposeTargetData(dataLayout)
                                Some(tm)
                            | Error(_) -> None
                        
                        Ok({
                            Context = context
                            Module = moduleRef
                            Builder = builder
                            TargetMachine = targetMachineResult
                            PassManager = None
                        })
    with
    | ex ->
        Error(sprintf "Exception creating Fidelity pipeline: %s" ex.Message)

/// Dispose of a Fidelity LLVM pipeline
let LLVMDisposeFidelityPipeline (pipeline: FidelityLLVMPipeline) : unit =
    let resources = [
        ("Pass Manager", fun () -> 
            match pipeline.PassManager with 
            | Some(pm) -> LLVMDisposePassManager(pm) 
            | None -> ())
        ("Target Machine", fun () -> 
            match pipeline.TargetMachine with 
            | Some(tm) -> LLVMDisposeTargetMachine(tm) 
            | None -> ())
        ("Builder", fun () -> LLVMDisposeBuilder(pipeline.Builder))
        ("Module", fun () -> LLVMDisposeModule(pipeline.Module))
        ("Context", fun () -> LLVMContextDispose(pipeline.Context))
    ]
    LLVMCleanupResources(resources)