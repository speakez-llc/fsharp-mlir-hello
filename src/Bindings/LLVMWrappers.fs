module FSharpMLIR.Bindings.LLVMWrapper

open System
open System.Runtime.InteropServices
open System.Text
open FSharpMLIR.Bindings.LLVM
open FSharpMLIR.PlatformUtils

/// Exception thrown when LLVM operations fail
exception LLVMException of string

/// Helper module for working with LLVM strings and memory management
module internal LLVMStringUtils =
    let stringFromNativePtr (ptr: nativeint) : string =
        if ptr = nativeint.Zero then
            ""
        else
            try
                let result = Marshal.PtrToStringAnsi(ptr)
                invoke llvmDisposeMessage ptr
                if result = null then "" else result
            with
            | ex ->
                printfn "Error converting LLVM string: %s" ex.Message
                ""
    
    let allocateStringArray (strings: string[]) : nativeint * (unit -> unit) =
        if strings.Length = 0 then
            (nativeint.Zero, fun () -> ())
        else
            let ptr = Marshal.AllocHGlobal(nativeint (strings.Length * sizeof<nativeint>))
            let cleanup = fun () -> Marshal.FreeHGlobal(ptr)
            try
                for i = 0 to strings.Length - 1 do
                    let str = Marshal.StringToHGlobalAnsi(strings.[i])
                    Marshal.WriteIntPtr(ptr, i * sizeof<nativeint>, str)
                (ptr, cleanup)
            with
            | ex ->
                cleanup()
                raise ex

/// Wrapper for LLVM Context
type LLVMContext(handle: LLVMContextRef) =
    let mutable disposed = false
    
    /// The raw handle to the LLVM context
    member this.Handle = handle
    
    /// Set whether to discard value names for optimization
    member this.DiscardValueNames(discard: bool) =
        if disposed then invalidOp "Context has been disposed"
        invoke llvmContextSetDiscardValueNames handle (if discard then 1 else 0)
    
    /// Get whether value names are being discarded
    member this.ShouldDiscardValueNames() =
        if disposed then invalidOp "Context has been disposed"
        (invoke llvmContextShouldDiscardValueNames handle) <> 0
    
    /// Create a module in this context
    member this.CreateModule(name: string) =
        if disposed then invalidOp "Context has been disposed"
        let moduleHandle = invoke llvmModuleCreateWithNameInContext name handle
        new LLVMModule(this, moduleHandle)
    
    /// Create an instruction builder in this context
    member this.CreateBuilder() =
        if disposed then invalidOp "Context has been disposed"
        let builderHandle = invoke llvmCreateBuilderInContext handle
        new LLVMBuilder(this, builderHandle)
    
    /// Create integer types
    member this.Int1Type() = invoke llvmInt1TypeInContext handle
    member this.Int8Type() = invoke llvmInt8TypeInContext handle
    member this.Int16Type() = invoke llvmInt16TypeInContext handle
    member this.Int32Type() = invoke llvmInt32TypeInContext handle
    member this.Int64Type() = invoke llvmInt64TypeInContext handle
    member this.Int128Type() = invoke llvmInt128TypeInContext handle
    member this.IntType(numBits: int) = invoke llvmIntTypeInContext handle (uint32 numBits)
    
    /// Create floating point types
    member this.HalfType() = invoke llvmHalfTypeInContext handle
    member this.BFloatType() = invoke llvmBFloatTypeInContext handle
    member this.FloatType() = invoke llvmFloatTypeInContext handle
    member this.DoubleType() = invoke llvmDoubleTypeInContext handle
    member this.X86FP80Type() = invoke llvmX86FP80TypeInContext handle
    member this.FP128Type() = invoke llvmFP128TypeInContext handle
    member this.PPCFP128Type() = invoke llvmPPCFP128TypeInContext handle
    
    /// Create other basic types
    member this.VoidType() = invoke llvmVoidTypeInContext handle
    member this.LabelType() = invoke llvmLabelTypeInContext handle
    member this.X86AMXType() = invoke llvmX86AMXTypeInContext handle
    member this.TokenType() = invoke llvmTokenTypeInContext handle
    member this.MetadataType() = invoke llvmMetadataTypeInContext handle
    
    /// Create pointer type
    member this.PointerType(addressSpace: int) = 
        invoke llvmPointerTypeInContext handle (uint32 addressSpace)
    
    /// Create function type
    member this.FunctionType(returnType: LLVMTypeRef, paramTypes: LLVMTypeRef[], isVarArg: bool) =
        let paramCount = uint32 paramTypes.Length
        let isVarArgBool = if isVarArg then 1 else 0
        
        if paramTypes.Length = 0 then
            invoke llvmFunctionType returnType nativeint.Zero 0u isVarArgBool
        else
            let paramTypesPtr = Marshal.AllocHGlobal(nativeint (paramTypes.Length * sizeof<nativeint>))
            try
                for i = 0 to paramTypes.Length - 1 do
                    Marshal.WriteIntPtr(paramTypesPtr, i * sizeof<nativeint>, paramTypes.[i])
                invoke llvmFunctionType returnType paramTypesPtr paramCount isVarArgBool
            finally
                Marshal.FreeHGlobal(paramTypesPtr)
    
    /// Create struct type
    member this.StructType(elementTypes: LLVMTypeRef[], packed: bool) =
        let elementCount = uint32 elementTypes.Length
        let packedBool = if packed then 1 else 0
        
        if elementTypes.Length = 0 then
            invoke llvmStructTypeInContext handle nativeint.Zero 0u packedBool
        else
            let elemTypesPtr = Marshal.AllocHGlobal(nativeint (elementTypes.Length * sizeof<nativeint>))
            try
                for i = 0 to elementTypes.Length - 1 do
                    Marshal.WriteIntPtr(elemTypesPtr, i * sizeof<nativeint>, elementTypes.[i])
                invoke llvmStructTypeInContext handle elemTypesPtr elementCount packedBool
            finally
                Marshal.FreeHGlobal(elemTypesPtr)
    
    /// Create named struct type
    member this.CreateNamedStruct(name: string) =
        invoke llvmStructCreateNamed handle name
    
    /// Create array type
    member this.ArrayType(elementType: LLVMTypeRef, elementCount: int) =
        invoke llvmArrayType elementType (uint32 elementCount)
    
    /// Create vector type
    member this.VectorType(elementType: LLVMTypeRef, elementCount: int) =
        invoke llvmVectorType elementType (uint32 elementCount)
    
    /// Create scalable vector type
    member this.ScalableVectorType(elementType: LLVMTypeRef, elementCount: int) =
        invoke llvmScalableVectorType elementType (uint32 elementCount)
    
    interface IDisposable with
        member this.Dispose() = 
            if not disposed then
                invoke llvmContextDispose handle
                disposed <- true
    
    /// Factory method to create a context
    static member Create() =
        let handle = invoke llvmContextCreate ()
        new LLVMContext(handle)
    
    /// Get the global context (not recommended for multi-threaded use)
    static member GetGlobal() =
        let handle = invoke llvmGetGlobalContext ()
        new LLVMContext(handle)

/// Wrapper for LLVM Module
and LLVMModule(context: LLVMContext, handle: LLVMModuleRef) =
    let mutable disposed = false
    
    /// The raw handle to the LLVM module
    member this.Handle = handle
    
    /// Reference to the parent context
    member this.Context = context
    
    /// Set the target triple for this module
    member this.SetTarget(triple: string) =
        if disposed then invalidOp "Module has been disposed"
        invoke llvmSetTarget handle triple
    
    /// Get the target triple for this module
    member this.GetTarget() =
        if disposed then invalidOp "Module has been disposed"
        let ptr = invoke llvmGetTarget handle
        LLVMStringUtils.stringFromNativePtr(ptr)
    
    /// Set the data layout for this module
    member this.SetDataLayout(layout: string) =
        if disposed then invalidOp "Module has been disposed"
        invoke llvmSetDataLayout handle layout
    
    /// Get the data layout for this module
    member this.GetDataLayout() =
        if disposed then invalidOp "Module has been disposed"
        let ptr = invoke llvmGetDataLayoutStr handle
        LLVMStringUtils.stringFromNativePtr(ptr)
    
    /// Set module identifier
    member this.SetIdentifier(identifier: string) =
        if disposed then invalidOp "Module has been disposed"
        invoke llvmSetModuleIdentifier handle identifier (uint32 identifier.Length)
    
    /// Set source filename
    member this.SetSourceFileName(filename: string) =
        if disposed then invalidOp "Module has been disposed"
        invoke llvmSetSourceFileName handle filename (uint32 filename.Length)
    
    /// Add a function to the module
    member this.AddFunction(name: string, functionType: LLVMTypeRef) =
        if disposed then invalidOp "Module has been disposed"
        let functionHandle = invoke llvmAddFunction handle name functionType
        new LLVMFunction(functionHandle)
    
    /// Get a named function from the module
    member this.GetNamedFunction(name: string) =
        if disposed then invalidOp "Module has been disposed"
        let functionHandle = invoke llvmGetNamedFunction handle name
        if functionHandle = nativeint.Zero then
            None
        else
            Some(new LLVMFunction(functionHandle))
    
    /// Add a global variable to the module
    member this.AddGlobal(``type``: LLVMTypeRef, name: string) =
        if disposed then invalidOp "Module has been disposed"
        let globalHandle = invoke llvmAddGlobal handle ``type`` name
        new LLVMGlobalVariable(globalHandle)
    
    /// Get a named global variable from the module
    member this.GetNamedGlobal(name: string) =
        if disposed then invalidOp "Module has been disposed"
        let globalHandle = invoke llvmGetNamedGlobal handle name
        if globalHandle = nativeint.Zero then
            None
        else
            Some(new LLVMGlobalVariable(globalHandle))
    
    /// Verify the module
    member this.Verify() =
        if disposed then invalidOp "Module has been disposed"
        let errorPtr = Marshal.AllocHGlobal(sizeof<nativeint>)
        try
            let result = invoke llvmVerifyModule handle LLVMVerifierFailureAction.ReturnStatus errorPtr
            if result <> 0 then
                let errorMsgPtr = Marshal.ReadIntPtr(errorPtr)
                if errorMsgPtr <> nativeint.Zero then
                    let errorMsg = Marshal.PtrToStringAnsi(errorMsgPtr)
                    invoke llvmDisposeMessage errorMsgPtr
                    Error(errorMsg)
                else
                    Error("Module verification failed with unknown error")
            else
                Ok(())
        finally
            Marshal.FreeHGlobal(errorPtr)
    
    /// Print the module to the console for debugging
    member this.Dump() =
        if disposed then invalidOp "Module has been disposed"
        invoke llvmDumpModule handle
    
    /// Print the module to a string
    member this.Print() =
        if disposed then invalidOp "Module has been disposed"
        let ptr = invoke llvmPrintModuleToString handle
        LLVMStringUtils.stringFromNativePtr(ptr)
    
    /// Print the module to a file
    member this.PrintToFile(filename: string) =
        if disposed then invalidOp "Module has been disposed"
        let errorPtr = Marshal.AllocHGlobal(sizeof<nativeint>)
        try
            let result = invoke llvmPrintModuleToFile handle filename errorPtr
            if result <> 0 then
                let errorMsgPtr = Marshal.ReadIntPtr(errorPtr)
                if errorMsgPtr <> nativeint.Zero then
                    let errorMsg = Marshal.PtrToStringAnsi(errorMsgPtr)
                    invoke llvmDisposeMessage errorMsgPtr
                    Error(errorMsg)
                else
                    Error("Failed to print module to file")
            else
                Ok(())
        finally
            Marshal.FreeHGlobal(errorPtr)
    
    interface IDisposable with
        member this.Dispose() = 
            if not disposed then
                invoke llvmDisposeModule handle
                disposed <- true

/// Wrapper for LLVM Function
and LLVMFunction(handle: LLVMValueRef) =
    /// The raw handle to the LLVM function
    member this.Handle = handle
    
    /// Set the calling convention
    member this.SetCallConv(cc: LLVMCallConv) =
        invoke llvmSetFunctionCallConv handle (uint32 cc)
    
    /// Get the calling convention
    member this.GetCallConv() =
        let cc = invoke llvmGetFunctionCallConv handle
        enum<LLVMCallConv>(int cc)
    
    /// Set the linkage type
    member this.SetLinkage(linkage: LLVMLinkage) =
        invoke llvmSetLinkage handle linkage
    
    /// Get the linkage type
    member this.GetLinkage() =
        invoke llvmGetLinkage handle
    
    /// Set the visibility
    member this.SetVisibility(visibility: LLVMVisibility) =
        invoke llvmSetVisibility handle visibility
    
    /// Get the visibility
    member this.GetVisibility() =
        invoke llvmGetVisibility handle
    
    /// Get the number of parameters
    member this.GetParamCount() =
        int (invoke llvmCountParams handle)
    
    /// Get a parameter by index
    member this.GetParam(index: int) =
        let paramHandle = invoke llvmGetParam handle (uint32 index)
        new LLVMValue(paramHandle)
    
    /// Get all parameters
    member this.GetParams() =
        let count = this.GetParamCount()
        [| for i in 0 .. count - 1 -> this.GetParam(i) |]
    
    /// Append a basic block to this function
    member this.AppendBasicBlock(name: string) =
        let blockHandle = invoke llvmAppendBasicBlock handle name
        new LLVMBasicBlock(blockHandle)
    
    /// Get the entry basic block
    member this.GetEntryBasicBlock() =
        let blockHandle = invoke llvmGetEntryBasicBlock handle
        if blockHandle = nativeint.Zero then
            None
        else
            Some(new LLVMBasicBlock(blockHandle))
    
    /// Get all basic blocks
    member this.GetBasicBlocks() =
        let count = int (invoke llvmCountBasicBlocks handle)
        let blocks = Array.zeroCreate<LLVMBasicBlockRef> count
        if count > 0 then
            let blocksPtr = Marshal.AllocHGlobal(nativeint (count * sizeof<nativeint>))
            try
                invoke llvmGetBasicBlocks handle blocksPtr
                for i = 0 to count - 1 do
                    blocks.[i] <- Marshal.ReadIntPtr(blocksPtr, i * sizeof<nativeint>)
            finally
                Marshal.FreeHGlobal(blocksPtr)
        blocks |> Array.map (fun h -> new LLVMBasicBlock(h))
    
    /// Set the value name
    member this.SetName(name: string) =
        invoke llvmSetValueName2 handle name (uint32 name.Length)
    
    /// Get the value name
    member this.GetName() =
        let lengthPtr = Marshal.AllocHGlobal(sizeof<nativeint>)
        try
            let namePtr = invoke llvmGetValueName2 handle lengthPtr
            let length = Marshal.ReadIntPtr(lengthPtr)
            if namePtr = nativeint.Zero || length = nativeint.Zero then
                ""
            else
                let bytes = Array.zeroCreate<byte> (int length)
                Marshal.Copy(namePtr, bytes, 0, int length)
                System.Text.Encoding.UTF8.GetString(bytes)
        finally
            Marshal.FreeHGlobal(lengthPtr)

/// Wrapper for LLVM Global Variable
and LLVMGlobalVariable(handle: LLVMValueRef) =
    /// The raw handle to the LLVM global variable
    member this.Handle = handle
    
    /// Set the initializer
    member this.SetInitializer(value: LLVMValueRef) =
        invoke llvmSetInitializer handle value
    
    /// Get the initializer
    member this.GetInitializer() =
        let initHandle = invoke llvmGetInitializer handle
        if initHandle = nativeint.Zero then
            None
        else
            Some(new LLVMValue(initHandle))
    
    /// Set whether this is a constant
    member this.SetConstant(isConstant: bool) =
        invoke llvmSetGlobalConstant handle (if isConstant then 1 else 0)
    
    /// Check if this is a constant
    member this.IsConstant() =
        (invoke llvmIsGlobalConstant handle) <> 0
    
    /// Set whether this is thread local
    member this.SetThreadLocal(isThreadLocal: bool) =
        invoke llvmSetThreadLocal handle (if isThreadLocal then 1 else 0)
    
    /// Check if this is thread local
    member this.IsThreadLocal() =
        (invoke llvmIsThreadLocal handle) <> 0
    
    /// Set the linkage type
    member this.SetLinkage(linkage: LLVMLinkage) =
        invoke llvmSetLinkage handle linkage
    
    /// Get the linkage type
    member this.GetLinkage() =
        invoke llvmGetLinkage handle

/// Wrapper for LLVM Value
and LLVMValue(handle: LLVMValueRef) =
    /// The raw handle to the LLVM value
    member this.Handle = handle
    
    /// Get the type of this value
    member this.GetType() =
        invoke llvmTypeOf handle
    
    /// Set the value name
    member this.SetName(name: string) =
        invoke llvmSetValueName2 handle name (uint32 name.Length)
    
    /// Get the value name
    member this.GetName() =
        let lengthPtr = Marshal.AllocHGlobal(sizeof<nativeint>)
        try
            let namePtr = invoke llvmGetValueName2 handle lengthPtr
            let length = Marshal.ReadIntPtr(lengthPtr)
            if namePtr = nativeint.Zero || length = nativeint.Zero then
                ""
            else
                let bytes = Array.zeroCreate<byte> (int length)
                Marshal.Copy(namePtr, bytes, 0, int length)
                System.Text.Encoding.UTF8.GetString(bytes)
        finally
            Marshal.FreeHGlobal(lengthPtr)
    
    /// Check if this is a constant
    member this.IsConstant() =
        (invoke llvmIsConstant handle) <> 0
    
    /// Check if this is undef
    member this.IsUndef() =
        (invoke llvmIsUndef handle) <> 0
    
    /// Check if this is poison
    member this.IsPoison() =
        (invoke llvmIsPoison handle) <> 0
    
    /// Print this value for debugging
    member this.Dump() =
        invoke llvmDumpValue handle
    
    /// Print this value to a string
    member this.Print() =
        let ptr = invoke llvmPrintValueToString handle
        LLVMStringUtils.stringFromNativePtr(ptr)
    
    override this.ToString() = this.Print()

/// Wrapper for LLVM Basic Block
and LLVMBasicBlock(handle: LLVMBasicBlockRef) =
    /// The raw handle to the LLVM basic block
    member this.Handle = handle
    
    /// Get the parent function
    member this.GetParent() =
        let functionHandle = invoke llvmGetBasicBlockParent handle
        if functionHandle = nativeint.Zero then
            None
        else
            Some(new LLVMFunction(functionHandle))
    
    /// Get the terminator instruction
    member this.GetTerminator() =
        let termHandle = invoke llvmGetBasicBlockTerminator handle
        if termHandle = nativeint.Zero then
            None
        else
            Some(new LLVMValue(termHandle))
    
    /// Get the first instruction
    member this.GetFirstInstruction() =
        let instHandle = invoke llvmGetFirstInstruction handle
        if instHandle = nativeint.Zero then
            None
        else
            Some(new LLVMValue(instHandle))
    
    /// Get the last instruction
    member this.GetLastInstruction() =
        let instHandle = invoke llvmGetLastInstruction handle
        if instHandle = nativeint.Zero then
            None
        else
            Some(new LLVMValue(instHandle))
    
    /// Convert to a value
    member this.AsValue() =
        let valueHandle = invoke llvmBasicBlockAsValue handle
        new LLVMValue(valueHandle)

/// Wrapper for LLVM Instruction Builder
and LLVMBuilder(context: LLVMContext, handle: LLVMBuilderRef) =
    let mutable disposed = false
    
    /// The raw handle to the LLVM builder
    member this.Handle = handle
    
    /// Reference to the parent context
    member this.Context = context
    
    /// Position the builder at the end of a basic block
    member this.PositionAtEnd(block: LLVMBasicBlock) =
        if disposed then invalidOp "Builder has been disposed"
        invoke llvmPositionBuilderAtEnd handle block.Handle
    
    /// Position the builder before an instruction
    member this.PositionBefore(instruction: LLVMValue) =
        if disposed then invalidOp "Builder has been disposed"
        invoke llvmPositionBuilderBefore handle instruction.Handle
    
    /// Get the current insert block
    member this.GetInsertBlock() =
        if disposed then invalidOp "Builder has been disposed"
        let blockHandle = invoke llvmGetInsertBlock handle
        if blockHandle = nativeint.Zero then
            None
        else
            Some(new LLVMBasicBlock(blockHandle))
    
    /// Clear the insertion position
    member this.ClearInsertionPosition() =
        if disposed then invalidOp "Builder has been disposed"
        invoke llvmClearInsertionPosition handle
    
    // Terminator Instructions
    
    /// Build a return void instruction
    member this.BuildRetVoid() =
        if disposed then invalidOp "Builder has been disposed"
        let retHandle = invoke llvmBuildRetVoid handle
        new LLVMValue(retHandle)
    
    /// Build a return instruction
    member this.BuildRet(value: LLVMValue) =
        if disposed then invalidOp "Builder has been disposed"
        let retHandle = invoke llvmBuildRet handle value.Handle
        new LLVMValue(retHandle)
    
    /// Build an unconditional branch instruction
    member this.BuildBr(dest: LLVMBasicBlock) =
        if disposed then invalidOp "Builder has been disposed"
        let brHandle = invoke llvmBuildBr handle dest.Handle
        new LLVMValue(brHandle)
    
    /// Build a conditional branch instruction
    member this.BuildCondBr(condition: LLVMValue, thenBlock: LLVMBasicBlock, elseBlock: LLVMBasicBlock) =
        if disposed then invalidOp "Builder has been disposed"
        let brHandle = invoke llvmBuildCondBr handle condition.Handle thenBlock.Handle elseBlock.Handle
        new LLVMValue(brHandle)
    
    /// Build an unreachable instruction
    member this.BuildUnreachable() =
        if disposed then invalidOp "Builder has been disposed"
        let unreachableHandle = invoke llvmBuildUnreachable handle
        new LLVMValue(unreachableHandle)
    
    // Arithmetic Instructions
    
    /// Build an add instruction
    member this.BuildAdd(lhs: LLVMValue, rhs: LLVMValue, name: string) =
        if disposed then invalidOp "Builder has been disposed"
        let addHandle = invoke llvmBuildAdd handle lhs.Handle rhs.Handle name
        new LLVMValue(addHandle)
    
    /// Build a subtract instruction
    member this.BuildSub(lhs: LLVMValue, rhs: LLVMValue, name: string) =
        if disposed then invalidOp "Builder has been disposed"
        let subHandle = invoke llvmBuildSub handle lhs.Handle rhs.Handle name
        new LLVMValue(subHandle)
    
    /// Build a multiply instruction
    member this.BuildMul(lhs: LLVMValue, rhs: LLVMValue, name: string) =
        if disposed then invalidOp "Builder has been disposed"
        let mulHandle = invoke llvmBuildMul handle lhs.Handle rhs.Handle name
        new LLVMValue(mulHandle)
    
    /// Build an unsigned divide instruction
    member this.BuildUDiv(lhs: LLVMValue, rhs: LLVMValue, name: string) =
        if disposed then invalidOp "Builder has been disposed"
        let divHandle = invoke llvmBuildUDiv handle lhs.Handle rhs.Handle name
        new LLVMValue(divHandle)
    
    /// Build a signed divide instruction
    member this.BuildSDiv(lhs: LLVMValue, rhs: LLVMValue, name: string) =
        if disposed then invalidOp "Builder has been disposed"
        let divHandle = invoke llvmBuildSDiv handle lhs.Handle rhs.Handle name
        new LLVMValue(divHandle)
    
    /// Build a floating point add instruction
    member this.BuildFAdd(lhs: LLVMValue, rhs: LLVMValue, name: string) =
        if disposed then invalidOp "Builder has been disposed"
        let addHandle = invoke llvmBuildFAdd handle lhs.Handle rhs.Handle name
        new LLVMValue(addHandle)
    
    /// Build a floating point subtract instruction
    member this.BuildFSub(lhs: LLVMValue, rhs: LLVMValue, name: string) =
        if disposed then invalidOp "Builder has been disposed"
        let subHandle = invoke llvmBuildFSub handle lhs.Handle rhs.Handle name
        new LLVMValue(subHandle)
    
    /// Build a floating point multiply instruction
    member this.BuildFMul(lhs: LLVMValue, rhs: LLVMValue, name: string) =
        if disposed then invalidOp "Builder has been disposed"
        let mulHandle = invoke llvmBuildFMul handle lhs.Handle rhs.Handle name
        new LLVMValue(mulHandle)
    
    /// Build a floating point divide instruction
    member this.BuildFDiv(lhs: LLVMValue, rhs: LLVMValue, name: string) =
        if disposed then invalidOp "Builder has been disposed"
        let divHandle = invoke llvmBuildFDiv handle lhs.Handle rhs.Handle name
        new LLVMValue(divHandle)
    
    // Bitwise Instructions
    
    /// Build an and instruction
    member this.BuildAnd(lhs: LLVMValue, rhs: LLVMValue, name: string) =
        if disposed then invalidOp "Builder has been disposed"
        let andHandle = invoke llvmBuildAnd handle lhs.Handle rhs.Handle name
        new LLVMValue(andHandle)
    
    /// Build an or instruction
    member this.BuildOr(lhs: LLVMValue, rhs: LLVMValue, name: string) =
        if disposed then invalidOp "Builder has been disposed"
        let orHandle = invoke llvmBuildOr handle lhs.Handle rhs.Handle name
        new LLVMValue(orHandle)
    
    /// Build an xor instruction
    member this.BuildXor(lhs: LLVMValue, rhs: LLVMValue, name: string) =
        if disposed then invalidOp "Builder has been disposed"
        let xorHandle = invoke llvmBuildXor handle lhs.Handle rhs.Handle name
        new LLVMValue(xorHandle)
    
    /// Build a not instruction
    member this.BuildNot(value: LLVMValue, name: string) =
        if disposed then invalidOp "Builder has been disposed"
        let notHandle = invoke llvmBuildNot handle value.Handle name
        new LLVMValue(notHandle)
    
    // Memory Instructions
    
    /// Build an alloca instruction
    member this.BuildAlloca(``type``: LLVMTypeRef, name: string) =
        if disposed then invalidOp "Builder has been disposed"
        let allocaHandle = invoke llvmBuildAlloca handle ``type`` name
        new LLVMValue(allocaHandle)
    
    /// Build a load instruction
    member this.BuildLoad(``type``: LLVMTypeRef, ptr: LLVMValue, name: string) =
        if disposed then invalidOp "Builder has been disposed"
        let loadHandle = invoke llvmBuildLoad2 handle ``type`` ptr.Handle name
        new LLVMValue(loadHandle)
    
    /// Build a store instruction
    member this.BuildStore(value: LLVMValue, ptr: LLVMValue) =
        if disposed then invalidOp "Builder has been disposed"
        let storeHandle = invoke llvmBuildStore handle value.Handle ptr.Handle
        new LLVMValue(storeHandle)
    
    /// Build a GEP instruction
    member this.BuildGEP(``type``: LLVMTypeRef, ptr: LLVMValue, indices: LLVMValue[], name: string) =
        if disposed then invalidOp "Builder has been disposed"
        let numIndices = uint32 indices.Length
        
        if indices.Length = 0 then
            let gepHandle = invoke llvmBuildGEP2 handle ``type`` ptr.Handle nativeint.Zero 0u name
            new LLVMValue(gepHandle)
        else
            let indicesPtr = Marshal.AllocHGlobal(nativeint (indices.Length * sizeof<nativeint>))
            try
                for i = 0 to indices.Length - 1 do
                    Marshal.WriteIntPtr(indicesPtr, i * sizeof<nativeint>, indices.[i].Handle)
                let gepHandle = invoke llvmBuildGEP2 handle ``type`` ptr.Handle indicesPtr numIndices name
                new LLVMValue(gepHandle)
            finally
                Marshal.FreeHGlobal(indicesPtr)
    
    /// Build an in-bounds GEP instruction
    member this.BuildInBoundsGEP(``type``: LLVMTypeRef, ptr: LLVMValue, indices: LLVMValue[], name: string) =
        if disposed then invalidOp "Builder has been disposed"
        let numIndices = uint32 indices.Length
        
        if indices.Length = 0 then
            let gepHandle = invoke llvmBuildInBoundsGEP2 handle ``type`` ptr.Handle nativeint.Zero 0u name
            new LLVMValue(gepHandle)
        else
            let indicesPtr = Marshal.AllocHGlobal(nativeint (indices.Length * sizeof<nativeint>))
            try
                for i = 0 to indices.Length - 1 do
                    Marshal.WriteIntPtr(indicesPtr, i * sizeof<nativeint>, indices.[i].Handle)
                let gepHandle = invoke llvmBuildInBoundsGEP2 handle ``type`` ptr.Handle indicesPtr numIndices name
                new LLVMValue(gepHandle)
            finally
                Marshal.FreeHGlobal(indicesPtr)
    
    /// Build a struct GEP instruction
    member this.BuildStructGEP(``type``: LLVMTypeRef, ptr: LLVMValue, index: int, name: string) =
        if disposed then invalidOp "Builder has been disposed"
        let gepHandle = invoke llvmBuildStructGEP2 handle ``type`` ptr.Handle (uint32 index) name
        new LLVMValue(gepHandle)
    
    // Comparison Instructions
    
    /// Build an integer comparison instruction
    member this.BuildICmp(predicate: LLVMIntPredicate, lhs: LLVMValue, rhs: LLVMValue, name: string) =
        if disposed then invalidOp "Builder has been disposed"
        let cmpHandle = invoke llvmBuildICmp handle predicate lhs.Handle rhs.Handle name
        new LLVMValue(cmpHandle)
    
    /// Build a floating point comparison instruction
    member this.BuildFCmp(predicate: LLVMRealPredicate, lhs: LLVMValue, rhs: LLVMValue, name: string) =
        if disposed then invalidOp "Builder has been disposed"
        let cmpHandle = invoke llvmBuildFCmp handle predicate lhs.Handle rhs.Handle name
        new LLVMValue(cmpHandle)
    
    // Cast Instructions
    
    /// Build a truncate instruction
    member this.BuildTrunc(value: LLVMValue, destType: LLVMTypeRef, name: string) =
        if disposed then invalidOp "Builder has been disposed"
        let truncHandle = invoke llvmBuildTrunc handle value.Handle destType name
        new LLVMValue(truncHandle)
    
    /// Build a zero extend instruction
    member this.BuildZExt(value: LLVMValue, destType: LLVMTypeRef, name: string) =
        if disposed then invalidOp "Builder has been disposed"
        let zextHandle = invoke llvmBuildZExt handle value.Handle destType name
        new LLVMValue(zextHandle)
    
    /// Build a sign extend instruction
    member this.BuildSExt(value: LLVMValue, destType: LLVMTypeRef, name: string) =
        if disposed then invalidOp "Builder has been disposed"
        let sextHandle = invoke llvmBuildSExt handle value.Handle destType name
        new LLVMValue(sextHandle)
    
    /// Build a bitcast instruction
    member this.BuildBitCast(value: LLVMValue, destType: LLVMTypeRef, name: string) =
        if disposed then invalidOp "Builder has been disposed"
        let castHandle = invoke llvmBuildBitCast handle value.Handle destType name
        new LLVMValue(castHandle)
    
    // Other Instructions
    
    /// Build a PHI instruction
    member this.BuildPhi(``type``: LLVMTypeRef, name: string) =
        if disposed then invalidOp "Builder has been disposed"
        let phiHandle = invoke llvmBuildPhi handle ``type`` name
        new LLVMPhi(phiHandle)
    
    /// Build a call instruction
    member this.BuildCall(functionType: LLVMTypeRef, function: LLVMValue, args: LLVMValue[], name: string) =
        if disposed then invalidOp "Builder has been disposed"
        let numArgs = uint32 args.Length
        
        if args.Length = 0 then
            let callHandle = invoke llvmBuildCall2 handle functionType function.Handle nativeint.Zero 0u name
            new LLVMValue(callHandle)
        else
            let argsPtr = Marshal.AllocHGlobal(nativeint (args.Length * sizeof<nativeint>))
            try
                for i = 0 to args.Length - 1 do
                    Marshal.WriteIntPtr(argsPtr, i * sizeof<nativeint>, args.[i].Handle)
                let callHandle = invoke llvmBuildCall2 handle functionType function.Handle argsPtr numArgs name
                new LLVMValue(callHandle)
            finally
                Marshal.FreeHGlobal(argsPtr)
    
    /// Build a select instruction
    member this.BuildSelect(condition: LLVMValue, thenValue: LLVMValue, elseValue: LLVMValue, name: string) =
        if disposed then invalidOp "Builder has been disposed"
        let selectHandle = invoke llvmBuildSelect handle condition.Handle thenValue.Handle elseValue.Handle name
        new LLVMValue(selectHandle)
    
    interface IDisposable with
        member this.Dispose() = 
            if not disposed then
                invoke llvmDisposeBuilder handle
                disposed <- true

/// Wrapper for LLVM PHI instruction
and LLVMPhi(handle: LLVMValueRef) =
    inherit LLVMValue(handle)
    
    /// Add incoming values to the PHI node
    member this.AddIncoming(values: LLVMValue[], blocks: LLVMBasicBlock[]) =
        if values.Length <> blocks.Length then
            invalidArg "values/blocks" "Arrays must have the same length"
        
        let count = uint32 values.Length
        
        if values.Length > 0 then
            let valuesPtr = Marshal.AllocHGlobal(nativeint (values.Length * sizeof<nativeint>))
            let blocksPtr = Marshal.AllocHGlobal(nativeint (blocks.Length * sizeof<nativeint>))
            
            try
                for i = 0 to values.Length - 1 do
                    Marshal.WriteIntPtr(valuesPtr, i * sizeof<nativeint>, values.[i].Handle)
                    Marshal.WriteIntPtr(blocksPtr, i * sizeof<nativeint>, blocks.[i].Handle)
                
                invoke llvmAddIncoming handle valuesPtr blocksPtr count
            finally
                Marshal.FreeHGlobal(valuesPtr)
                Marshal.FreeHGlobal(blocksPtr)
    
    /// Get the number of incoming values
    member this.GetIncomingCount() =
        int (invoke llvmCountIncoming handle)
    
    /// Get an incoming value by index
    member this.GetIncomingValue(index: int) =
        let valueHandle = invoke llvmGetIncomingValue handle (uint32 index)
        new LLVMValue(valueHandle)
    
    /// Get an incoming block by index
    member this.GetIncomingBlock(index: int) =
        let blockHandle = invoke llvmGetIncomingBlock handle (uint32 index)
        new LLVMBasicBlock(blockHandle)

/// Wrapper for LLVM Target Machine
type LLVMTargetMachine(handle: LLVMTargetMachineRef) =
    let mutable disposed = false
    
    /// The raw handle to the LLVM target machine
    member this.Handle = handle
    
    /// Get the target triple
    member this.GetTriple() =
        if disposed then invalidOp "Target machine has been disposed"
        let ptr = invoke llvmGetTargetMachineTriple handle
        LLVMStringUtils.stringFromNativePtr(ptr)
    
    /// Get the CPU name
    member this.GetCPU() =
        if disposed then invalidOp "Target machine has been disposed"
        let ptr = invoke llvmGetTargetMachineCPU handle
        LLVMStringUtils.stringFromNativePtr(ptr)
    
    /// Get the feature string
    member this.GetFeatureString() =
        if disposed then invalidOp "Target machine has been disposed"
        let ptr = invoke llvmGetTargetMachineFeatureString handle
        LLVMStringUtils.stringFromNativePtr(ptr)
    
    /// Create target data layout
    member this.CreateTargetDataLayout() =
        if disposed then invalidOp "Target machine has been disposed"
        let dataLayoutHandle = invoke llvmCreateTargetDataLayout handle
        new LLVMTargetData(dataLayoutHandle)
    
    /// Emit module to file
    member this.EmitToFile(module': LLVMModule, filename: string, fileType: LLVMCodeGenFileType) =
        if disposed then invalidOp "Target machine has been disposed"
        let errorPtr = Marshal.AllocHGlobal(sizeof<nativeint>)
        try
            let result = invoke llvmTargetMachineEmitToFile handle module'.Handle filename fileType errorPtr
            if result <> 0 then
                let errorMsgPtr = Marshal.ReadIntPtr(errorPtr)
                if errorMsgPtr <> nativeint.Zero then
                    let errorMsg = Marshal.PtrToStringAnsi(errorMsgPtr)
                    invoke llvmDisposeMessage errorMsgPtr
                    Error(sprintf "Code generation failed: %s" errorMsg)
                else
                    Error("Code generation failed with unknown error")
            else
                Ok(())
        finally
            Marshal.FreeHGlobal(errorPtr)
    
    interface IDisposable with
        member this.Dispose() = 
            if not disposed then
                invoke llvmDisposeTargetMachine handle
                disposed <- true
    
    /// Create a target machine from triple
    static member Create(triple: string, cpu: string, features: string, optLevel: LLVMCodeGenOptLevel, relocMode: LLVMRelocMode, codeModel: LLVMCodeModel) =
        // Get target from triple
        let targetPtr = Marshal.AllocHGlobal(sizeof<nativeint>)
        let errorPtr = Marshal.AllocHGlobal(sizeof<nativeint>)
        
        try
            let result = invoke llvmGetTargetFromTriple triple targetPtr errorPtr
            
            if result <> 0 then
                let errorMsgPtr = Marshal.ReadIntPtr(errorPtr)
                if errorMsgPtr <> nativeint.Zero then
                    let errorMsg = Marshal.PtrToStringAnsi(errorMsgPtr)
                    invoke llvmDisposeMessage errorMsgPtr
                    Error(sprintf "Failed to get target for triple '%s': %s" triple errorMsg)
                else
                    Error(sprintf "Failed to get target for triple '%s': Unknown error" triple)
            else
                let target = Marshal.ReadIntPtr(targetPtr)
                let targetMachine = invoke llvmCreateTargetMachine target triple cpu features optLevel relocMode codeModel
                if targetMachine = nativeint.Zero then
                    Error("Failed to create target machine")
                else
                    Ok(new LLVMTargetMachine(targetMachine))
        finally
            Marshal.FreeHGlobal(targetPtr)
            Marshal.FreeHGlobal(errorPtr)

/// Wrapper for LLVM Target Data
and LLVMTargetData(handle: LLVMTargetDataRef) =
    let mutable disposed = false
    
    /// The raw handle to the LLVM target data
    member this.Handle = handle
    
    /// Get the pointer size in bytes
    member this.GetPointerSize() =
        if disposed then invalidOp "Target data has been disposed"
        int (invoke llvmPointerSize handle)
    
    /// Get the pointer size for address space
    member this.GetPointerSizeForAS(addressSpace: int) =
        if disposed then invalidOp "Target data has been disposed"
        int (invoke llvmPointerSizeForAS handle (uint32 addressSpace))
    
    /// Get the ABI size of a type in bytes
    member this.GetABISize(``type``: LLVMTypeRef) =
        if disposed then invalidOp "Target data has been disposed"
        invoke llvmABISizeOfType handle ``type``
    
    /// Get the ABI alignment of a type in bytes
    member this.GetABIAlignment(``type``: LLVMTypeRef) =
        if disposed then invalidOp "Target data has been disposed"
        int (invoke llvmABIAlignmentOfType handle ``type``)
    
    /// Get the preferred alignment of a type in bytes
    member this.GetPreferredAlignment(``type``: LLVMTypeRef) =
        if disposed then invalidOp "Target data has been disposed"
        int (invoke llvmPreferredAlignmentOfType handle ``type``)
    
    /// Get the size of a type in bits
    member this.GetSizeInBits(``type``: LLVMTypeRef) =
        if disposed then invalidOp "Target data has been disposed"
        invoke llvmSizeOfTypeInBits handle ``type``
    
    interface IDisposable with
        member this.Dispose() = 
            if not disposed then
                invoke llvmDisposeTargetData handle
                disposed <- true

/// Helper module for creating LLVM constants
module LLVMConstants =
    /// Create an integer constant
    let createInt (``type``: LLVMTypeRef) (value: uint64) (signExtend: bool) =
        let signExtendBool = if signExtend then 1 else 0
        let constHandle = invoke llvmConstInt ``type`` value signExtendBool
        new LLVMValue(constHandle)
    
    /// Create a floating point constant
    let createReal (``type``: LLVMTypeRef) (value: double) =
        let constHandle = invoke llvmConstReal ``type`` value
        new LLVMValue(constHandle)
    
    /// Create a null constant
    let createNull (``type``: LLVMTypeRef) =
        let constHandle = invoke llvmConstNull ``type``
        new LLVMValue(constHandle)
    
    /// Create an all-ones constant
    let createAllOnes (``type``: LLVMTypeRef) =
        let constHandle = invoke llvmConstAllOnes ``type``
        new LLVMValue(constHandle)
    
    /// Create an undef constant
    let createUndef (``type``: LLVMTypeRef) =
        let constHandle = invoke llvmGetUndef ``type``
        new LLVMValue(constHandle)
    
    /// Create a poison constant
    let createPoison (``type``: LLVMTypeRef) =
        let constHandle = invoke llvmGetPoison ``type``
        new LLVMValue(constHandle)
    
    /// Create a string constant
    let createString (context: LLVMContext) (value: string) (nullTerminate: bool) =
        let nullTerminateBool = if nullTerminate then 0 else 1
        let constHandle = invoke llvmConstStringInContext context.Handle value (uint32 value.Length) nullTerminateBool
        new LLVMValue(constHandle)
    
    /// Create an array constant
    let createArray (elementType: LLVMTypeRef) (values: LLVMValue[]) =
        let length = uint32 values.Length
        
        if values.Length = 0 then
            let constHandle = invoke llvmConstArray elementType nativeint.Zero 0u
            new LLVMValue(constHandle)
        else
            let valuesPtr = Marshal.AllocHGlobal(nativeint (values.Length * sizeof<nativeint>))
            try
                for i = 0 to values.Length - 1 do
                    Marshal.WriteIntPtr(valuesPtr, i * sizeof<nativeint>, values.[i].Handle)
                let constHandle = invoke llvmConstArray elementType valuesPtr length
                new LLVMValue(constHandle)
            finally
                Marshal.FreeHGlobal(valuesPtr)
    
    /// Create a struct constant
    let createStruct (context: LLVMContext) (values: LLVMValue[]) (packed: bool) =
        let count = uint32 values.Length
        let packedBool = if packed then 1 else 0
        
        if values.Length = 0 then
            let constHandle = invoke llvmConstStructInContext context.Handle nativeint.Zero 0u packedBool
            new LLVMValue(constHandle)
        else
            let valuesPtr = Marshal.AllocHGlobal(nativeint (values.Length * sizeof<nativeint>))
            try
                for i = 0 to values.Length - 1 do
                    Marshal.WriteIntPtr(valuesPtr, i * sizeof<nativeint>, values.[i].Handle)
                let constHandle = invoke llvmConstStructInContext context.Handle valuesPtr count packedBool
                new LLVMValue(constHandle)
            finally
                Marshal.FreeHGlobal(valuesPtr)

/// Helper module for LLVM initialization and utility functions
module LLVMUtils =
    /// Initialize all LLVM targets
    let initializeAllTargets() =
        invoke llvmInitializeAllTargetInfos ()
        invoke llvmInitializeAllTargets ()
        invoke llvmInitializeAllTargetMCs ()
        invoke llvmInitializeAllAsmPrinters ()
        invoke llvmInitializeAllAsmParsers ()
        invoke llvmInitializeAllDisassemblers ()
    
    /// Initialize native target
    let initializeNativeTarget() =
        let targetResult = invoke llvmInitializeNativeTarget ()
        let asmPrinterResult = invoke llvmInitializeNativeAsmPrinter ()
        targetResult = 0 && asmPrinterResult = 0
    
    /// Get the default target triple
    let getDefaultTargetTriple() =
        let ptr = invoke llvmGetDefaultTargetTriple ()
        LLVMStringUtils.stringFromNativePtr(ptr)
    
    /// Get the host CPU name
    let getHostCPUName() =
        let ptr = invoke llvmGetHostCPUName ()
        LLVMStringUtils.stringFromNativePtr(ptr)
    
    /// Get the host CPU features
    let getHostCPUFeatures() =
        let ptr = invoke llvmGetHostCPUFeatures ()
        LLVMStringUtils.stringFromNativePtr(ptr)
    
    /// Test LLVM availability
    let testAvailability() =
        try
            printfn "Testing LLVM library availability..."
            
            // Test basic context creation
            use context = LLVMContext.Create()
            printfn "✓ LLVM context creation successful"
            
            // Test module creation
            use testModule = context.CreateModule("test_module")
            printfn "✓ LLVM module creation successful"
            
            // Test target triple retrieval
            let triple = getDefaultTargetTriple()
            printfn "✓ Target triple: %s" triple
            
            triple <> "unknown-unknown-unknown"
        with
        | ex ->
            printfn "LLVM availability test failed: %s" ex.Message
            printfn "Stack trace: %s" ex.StackTrace
            false
    
    /// Initialize LLVM with comprehensive error handling
    let initializeAll() =
        try
            printfn "Initializing LLVM targets..."
            initializeAllTargets()
            
            let nativeTargetResult = initializeNativeTarget()
            
            printfn "LLVM initialization results: NativeTarget=%b" nativeTargetResult
            nativeTargetResult
        with
        | ex ->
            printfn "Error during LLVM initialization: %s" ex.Message
            printfn "Exception type: %s" (ex.GetType().Name)
            if ex.InnerException <> null then
                printfn "Inner exception: %s" ex.InnerException.Message
            false

/// Complete compilation pipeline for Fidelity
type FidelityLLVMPipeline = {
    Context: LLVMContext
    Module: LLVMModule
    Builder: LLVMBuilder
    TargetMachine: LLVMTargetMachine option
}

/// Helper module for creating Fidelity compilation pipelines
module FidelityPipeline =    
    /// Initialize a complete LLVM pipeline for Fidelity compiler
    let create (moduleName: string) (targetTriple: string option) : Result<FidelityLLVMPipeline, string> =
        try
            // Initialize LLVM
            if not (LLVMUtils.initializeAll()) then
                Error("Failed to initialize LLVM")
            else
                let context = LLVMContext.Create()
                
                let moduleRef = context.CreateModule(moduleName)
                
                let builder = context.CreateBuilder()
                
                // Set target triple
                let triple = 
                    match targetTriple with
                    | Some(t) -> t
                    | None -> LLVMUtils.getDefaultTargetTriple()
                
                moduleRef.SetTarget(triple)
                
                // Optionally create target machine
                let targetMachineResult = 
                    match LLVMTargetMachine.Create(triple, "generic", "", LLVMCodeGenOptLevel.Default, LLVMRelocMode.Default, LLVMCodeModel.Default) with
                    | Ok(tm) -> 
                        // Set data layout from target machine
                        use dataLayout = tm.CreateTargetDataLayout()
                        let layoutStr = LLVMStringUtils.stringFromNativePtr(invoke llvmCopyStringRepOfTargetData dataLayout.Handle)
                        moduleRef.SetDataLayout(layoutStr)
                        Some(tm)
                    | Error(_) -> None
                
                Ok({
                    Context = context
                    Module = moduleRef
                    Builder = builder
                    TargetMachine = targetMachineResult
                })
        with
        | ex ->
            Error(sprintf "Exception creating Fidelity pipeline: %s" ex.Message)
    
    /// Dispose of a Fidelity LLVM pipeline
    let dispose (pipeline: FidelityLLVMPipeline) : unit =
        try
            match pipeline.TargetMachine with 
            | Some(tm) -> tm.Dispose()
            | None -> ()
            
            pipeline.Builder.Dispose()
            pipeline.Module.Dispose()
            pipeline.Context.Dispose()
            printfn "✓ Disposed Fidelity LLVM pipeline"
        with
        | ex ->
            printfn "✗ Error disposing Fidelity pipeline: %s" ex.Message