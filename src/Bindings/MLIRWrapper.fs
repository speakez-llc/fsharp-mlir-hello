module FSharpMLIR.Bindings.MLIRWrapper

open System
open System.Runtime.InteropServices
open System.Text
open FSharpMLIR.Bindings.MLIR
open FSharpMLIR.PlatformUtils

/// <summary>
/// Delegate for callback functions used in MLIR printing operations
/// </summary>
type PrintCallback = delegate of nativeint * nativeint -> unit

/// <summary>
/// Wrapper for MLIR Context
/// </summary>
type MLIRContext(handle: nativeint) =
    /// <summary>
    /// The raw handle to the MLIR context
    /// </summary>
    member this.Handle = handle
    
    /// <summary>
    /// Allow unregistered dialects in this context
    /// </summary>
    member this.AllowUnregisteredDialects(allow: bool) =
        mlirContextSetAllowUnregisteredDialects(handle, allow)
    
    /// <summary>
    /// Create a location in this context
    /// </summary>
    member this.CreateUnknownLocation() =
        mlirLocationUnknownGet(handle)
    
    /// <summary>
    /// Create a file location in this context
    /// </summary>
    member this.CreateFileLocation(filename: string, line: int, column: int) =
        mlirLocationFileLineColGet(handle, filename, uint32 line, uint32 column)
    
    /// <summary>
    /// Create an integer type with the specified width
    /// </summary>
    member this.CreateIntegerType(width: int) =
        mlirIntegerTypeGet(handle, uint32 width)
    
    /// <summary>
    /// Create a float type of the specified kind
    /// </summary>
    member this.CreateFloatType(kind: MLIRFloatTypeKind) =
        mlirFloatTypeGet(handle, kind)
    
    /// <summary>
    /// Create a function type
    /// </summary>
    member this.CreateFunctionType(inputs: nativeint[], results: nativeint[]) =
        mlirFunctionTypeGet(
            handle, 
            uint32 inputs.Length, 
            inputs, 
            uint32 results.Length, 
            results)
    
    /// <summary>
    /// Create a string attribute
    /// </summary>
    member this.CreateStringAttribute(value: string) =
        mlirStringAttrGet(handle, uint32 value.Length, value)
    
    /// <summary>
    /// Create a boolean attribute
    /// </summary>
    member this.CreateBoolAttribute(value: bool) =
        mlirBoolAttrGet(handle, value)
    
    /// <summary>
    /// Create an integer attribute
    /// </summary>
    member this.CreateIntegerAttribute(``type``: nativeint, value: int64) =
        mlirIntegerAttrGet(``type``, value)
    
    /// <summary>
    /// Create a float attribute
    /// </summary>
    member this.CreateFloatAttribute(``type``: nativeint, value: double) =
        mlirFloatAttrGet(``type``, value)
        
    /// <summary>
    /// Create a type attribute
    /// </summary>
    member this.CreateTypeAttribute(``type``: nativeint) =
        mlirTypeAttrGet(``type``)
        
    /// <summary>
    /// Create a block attribute
    /// </summary>
    member this.CreateBlockAttribute(block: nativeint) =
        mlirBlockAttributeGet(block)
    
    interface IDisposable with
        member this.Dispose() = 
            mlirContextDestroy(handle)
    
    /// <summary>
    /// Factory method to create and initialize a context
    /// </summary>
    static member Create() =
        // Create the raw MLIR context
        let handle = mlirContextCreate()
        let context = new MLIRContext(handle)
        
        // Register standard MLIR dialects
        let registry = mlirDialectRegistryCreate()
        mlirRegisterAllDialects(registry)
        
        // Register specific dialects that are essential
        mlirRegisterStandardDialect(registry)
        mlirRegisterLLVMDialect(registry)
        
        mlirContextAppendDialectRegistry(handle, registry)
        mlirDialectRegistryDestroy(registry)
        
        // Return the wrapped context
        context

/// <summary>
/// Wrapper for MLIR Module
/// </summary>
type MLIRModule(context: MLIRContext, handle: nativeint) =
    /// <summary>
    /// The raw handle to the MLIR module
    /// </summary>
    member this.Handle = handle
    
    /// <summary>
    /// Reference to the parent context
    /// </summary>
    member this.Context = context
    
    /// <summary>
    /// Get the operation representing this module
    /// </summary>
    member this.GetOperation() =
        mlirModuleGetOperation(handle)
    
    /// <summary>
    /// Print the module to the console for debugging
    /// </summary>
    member this.Dump() =
        mlirOperationDump(this.GetOperation())
    
    /// <summary>
    /// Verify the module
    /// </summary>
    member this.Verify() =
        mlirOperationVerify(this.GetOperation())
    
    /// <summary>
    /// Print the module to a string
    /// </summary>
    member this.Print() =
        let mutable resultString = ""
        
        let callback = new PrintCallback(fun str len ->
            let buffer = Array.zeroCreate<byte> (int len)
            Marshal.Copy(str, buffer, 0, int len)
            let s = Encoding.UTF8.GetString(buffer)
            resultString <- resultString + s
        )
        
        let callbackPtr = Marshal.GetFunctionPointerForDelegate(callback)
        mlirOperationPrint(this.GetOperation(), callbackPtr, nativeint 0)
        
        resultString
    
    interface IDisposable with
        member this.Dispose() = 
            mlirModuleDestroy(handle)
    
    /// <summary>
    /// Factory method to create an empty module
    /// </summary>
    static member CreateEmpty(context: MLIRContext) =
        // Create a location for error reporting
        let location = context.CreateUnknownLocation()
        
        // Create an empty MLIR module
        let moduleHandle = mlirModuleCreateEmpty(location)
        
        // Clean up location
        mlirLocationDestroy(location)
        
        // Return the wrapped module
        new MLIRModule(context, moduleHandle)

/// <summary>
/// Wrapper for MLIR Pass Manager
/// </summary>
type MLIRPassManager(context: MLIRContext, handle: nativeint) =
    /// <summary>
    /// The raw handle to the MLIR pass manager
    /// </summary>
    member this.Handle = handle
    
    /// <summary>
    /// Add the canonicalizer pass
    /// </summary>
    member this.AddCanonicalizer() =
        mlirPassManagerAddCanonicalizer(handle)
        this
    
    /// <summary>
    /// Add the common subexpression elimination pass
    /// </summary>
    member this.AddCSE() =
        mlirPassManagerAddCSE(handle)
        this
    
    /// <summary>
    /// Add the lower-to-LLVM pass
    /// </summary>
    member this.AddLowerToLLVM() =
        mlirPassManagerAddLowerToLLVM(handle)
        this
    
    /// <summary>
    /// Run the pass manager on a module
    /// </summary>
    member this.Run(module': MLIRModule) =
        mlirPassManagerRun(handle, module'.Handle)
    
    interface IDisposable with
        member this.Dispose() = 
            mlirPassManagerDestroy(handle)
    
    /// <summary>
    /// Factory method to create a pass manager
    /// </summary>
    static member Create(context: MLIRContext) =
        let handle = mlirPassManagerCreate(context.Handle)
        new MLIRPassManager(context, handle)

/// <summary>
/// Wrapper for MLIR Region
/// </summary>
type MLIRRegion(handle: nativeint) =
    /// <summary>
    /// The raw handle to the MLIR region
    /// </summary>
    member this.Handle = handle
    
    /// <summary>
    /// Get the first block in the region
    /// </summary>
    member this.GetFirstBlock() =
        mlirRegionGetFirstBlock(handle)
    
    /// <summary>
    /// Add a block to the region
    /// </summary>
    member this.AddBlock(block: MLIRBlock) =
        mlirRegionAppendOwnedBlock(handle, block.Handle)
        
    /// <summary>
    /// Create a new block and add it to the region
    /// </summary>
    member this.CreateBlock() =
        let block = MLIRBlock.Create()
        this.AddBlock(block)
        block
    
    interface IDisposable with
        member this.Dispose() = 
            mlirRegionDestroy(handle)
    
    /// <summary>
    /// Factory method to create a region
    /// </summary>
    static member Create() =
        let handle = mlirRegionCreate()
        new MLIRRegion(handle)

/// <summary>
/// Wrapper for MLIR Block
/// </summary>
and MLIRBlock(handle: nativeint) =
    /// <summary>
    /// The raw handle to the MLIR block
    /// </summary>
    member this.Handle = handle
    
    /// <summary>
    /// Reference to the parent region, if any
    /// </summary>
    member val Region = Unchecked.defaultof<MLIRRegion> with get, set
    
    /// <summary>
    /// Append an operation to the block
    /// </summary>
    member this.AppendOperation(operation: nativeint) =
        mlirBlockAppendOwnedOperation(handle, operation)
    
    /// <summary>
    /// Get the first operation in the block
    /// </summary>
    member this.GetFirstOperation() =
        mlirBlockGetFirstOperation(handle)
    
    /// <summary>
    /// Get the terminator operation in the block
    /// </summary>
    member this.GetTerminator() =
        mlirBlockGetTerminator(handle)
    
    /// <summary>
    /// Create a successor block
    /// </summary>
    member this.CreateSuccessor() =
        let newBlock = MLIRBlock.Create()
        newBlock.Region <- this.Region
        newBlock

    interface IDisposable with
        member this.Dispose() = 
            mlirBlockDestroy(handle)
    
    /// <summary>
    /// Factory method to create a block with no arguments
    /// </summary>
    static member Create() =
        let handle = mlirBlockCreate(0u, [||], [||])
        new MLIRBlock(handle)
    
    /// <summary>
    /// Factory method to create a block with arguments
    /// </summary>
    static member Create(argumentTypes: nativeint[], locations: nativeint[]) =
        if argumentTypes.Length <> locations.Length then
            invalidArg "argumentTypes/locations" "Argument types and locations must have the same length"
        
        let handle = mlirBlockCreate(uint32 argumentTypes.Length, argumentTypes, locations)
        new MLIRBlock(handle)

/// <summary>
/// Helper module for MLIR to LLVM conversion
/// </summary>
module MLIRToLLVMConverter =
    /// <summary>
    /// Callback delegate for error reporting during translation
    /// </summary>
    type ErrorCallback = delegate of nativeint * nativeint -> unit
    
    /// <summary>
    /// Convert an MLIR module to LLVM IR module
    /// </summary>
    let convertModuleToLLVMIR (mlirModule: MLIRModule) (llvmModule: nativeint) =
        let mutable errorMessage = ""
        
        let callback = new ErrorCallback(fun str len ->
            let buffer = Array.zeroCreate<byte> (int len)
            Marshal.Copy(str, buffer, 0, int len)
            let s = Encoding.UTF8.GetString(buffer)
            errorMessage <- errorMessage + s
        )
        
        let callbackPtr = Marshal.GetFunctionPointerForDelegate(callback)
        
        let success = mlirTranslateModuleToLLVMIR(mlirModule.Handle, llvmModule, callbackPtr, nativeint 0)
        
        if success then Ok()
        else Error errorMessage

/// <summary>
/// Helper functions for creating named attributes
/// </summary>
module NamedAttributeHelpers =
    /// <summary>
    /// Create a named attribute
    /// </summary>
    let createNamedAttribute (context: MLIRContext) (name: string) (value: nativeint) =
        let nameAttr = context.CreateStringAttribute(name)
        mlirNamedAttributeGet(nameAttr, value)
        
    /// <summary>
    /// Create multiple named attributes
    /// </summary>
    let createNamedAttributes (context: MLIRContext) (attributes: (string * nativeint)[]) =
        [| for (name, value) in attributes -> createNamedAttribute context name value |]

/// <summary>
/// Helper module for creating MLIR types
/// </summary>
module TypeHelpers =
    /// <summary>
    /// Create an array type
    /// </summary>
    let createArrayType (context: MLIRContext) (elementType: nativeint) (size: int) =
        mlirArrayTypeGet(elementType, uint32 size)
        
    /// <summary>
    /// Create a pointer type for a given element type
    /// </summary>
    let createPointerType (context: MLIRContext) (elementType: nativeint) =
        // In MLIR, pointer types are represented in different ways depending on the dialect
        // For LLVM dialect, we use !llvm.ptr
        let location = context.CreateUnknownLocation()
        let ptrTypeAttr = context.CreateStringAttribute("ptr")
        
        // Create the MLIR type for !llvm.ptr
        let ptrType = mlirOperationCreate(
            "llvm.ptr",
            location,
            1u,  // One result (the type)
            [| elementType |],  // The element type
            0u,
            [||],
            1u,
            [| NamedAttributeHelpers.createNamedAttribute context "element_type" ptrTypeAttr |],
            0u,
            [||])
            
        let result = mlirOperationGetResult(ptrType, 0u)
        mlirLocationDestroy(location)
        result
        
    /// <summary>
    /// Create a struct type
    /// </summary>
    let createStructType (context: MLIRContext) (elementTypes: nativeint[]) (fieldNames: string[]) =
        if elementTypes.Length <> fieldNames.Length && fieldNames.Length > 0 then
            invalidArg "elementTypes/fieldNames" "Element types and field names must have the same length"
            
        mlirStructTypeGet(context.Handle, uint32 elementTypes.Length, elementTypes, uint32 fieldNames.Length, fieldNames)

/// <summary>
/// Helper module for creating common MLIR operations
/// </summary>
module OperationHelpers =
    /// <summary>
    /// Create a return operation
    /// </summary>
    let createReturnOp (context: MLIRContext) (location: nativeint) (values: nativeint[]) =
        mlirOperationCreate(
            "func.return",
            location,
            0u,
            [||],
            uint32 values.Length,
            values,
            0u,
            [||],
            0u,
            [||])
            
    /// <summary>
    /// Create a branch operation
    /// </summary>
    let createBranchOp (context: MLIRContext) (location: nativeint) (targetBlock: nativeint) (operands: nativeint[]) =
        mlirOperationCreate(
            "cf.br",
            location,
            0u,
            [||],
            uint32 operands.Length,
            operands,
            1u,
            [| NamedAttributeHelpers.createNamedAttribute context "dest" (context.CreateBlockAttribute(targetBlock)) |],
            0u,
            [||])
            
    /// <summary>
    /// Create a conditional branch operation
    /// </summary>
    let createCondBranchOp (context: MLIRContext) (location: nativeint) (condition: nativeint) 
                          (trueBlock: nativeint) (falseBlock: nativeint) 
                          (trueOperands: nativeint[]) (falseOperands: nativeint[]) =
        let trueDestAttr = context.CreateBlockAttribute(trueBlock)
        let falseDestAttr = context.CreateBlockAttribute(falseBlock)
        
        mlirOperationCreate(
            "cf.cond_br",
            location,
            0u,
            [||],
            1u + uint32 trueOperands.Length + uint32 falseOperands.Length,
            Array.concat [[|condition|]; trueOperands; falseOperands],
            2u,
            [| 
                NamedAttributeHelpers.createNamedAttribute context "trueDest" trueDestAttr
                NamedAttributeHelpers.createNamedAttribute context "falseDest" falseDestAttr
            |],
            0u,
            [||])
            
    /// <summary>
    /// Create a constant integer operation
    /// </summary>
    let createConstantIntOp (context: MLIRContext) (location: nativeint) (value: int) (width: int) =
        let intType = context.CreateIntegerType(width)
        let valueAttr = context.CreateIntegerAttribute(intType, int64 value)
        
        mlirOperationCreate(
            "llvm.constant",
            location,
            1u,
            [| intType |],
            0u,
            [||],
            1u,
            [| NamedAttributeHelpers.createNamedAttribute context "value" valueAttr |],
            0u,
            [||])
            
    /// <summary>
    /// Create a call operation
    /// </summary>
    let createCallOp (context: MLIRContext) (location: nativeint) (callee: string) 
                     (arguments: nativeint[]) (resultTypes: nativeint[]) =
        let calleeAttr = context.CreateStringAttribute(callee)
        
        mlirOperationCreate(
            "func.call",
            location,
            uint32 resultTypes.Length,
            resultTypes,
            uint32 arguments.Length,
            arguments,
            1u,
            [| NamedAttributeHelpers.createNamedAttribute context "callee" calleeAttr |],
            0u,
            [||])
            
    /// <summary>
    /// Create a function operation
    /// </summary>
    let createFunctionOp (context: MLIRContext) (location: nativeint) (name: string) 
                         (functionType: nativeint) (isPublic: bool) =
        let symNameAttr = context.CreateStringAttribute(name)
        let typeAttr = context.CreateTypeAttribute(functionType)
        let publicAttr = context.CreateBoolAttribute(isPublic)
        
        // Create a region for the function body
        let region = MLIRRegion.Create()
        
        mlirOperationCreate(
            "func.func",
            location,
            0u,
            [||],
            0u,
            [||],
            3u,
            [| 
                NamedAttributeHelpers.createNamedAttribute context "sym_name" symNameAttr
                NamedAttributeHelpers.createNamedAttribute context "type" typeAttr
                NamedAttributeHelpers.createNamedAttribute context "public" publicAttr
            |],
            1u,
            [| region.Handle |])
            
    /// <summary>
    /// Create a global string operation
    /// </summary>
    let createGlobalStringOp (context: MLIRContext) (location: nativeint) (name: string) (value: string) =
        let symNameAttr = context.CreateStringAttribute(name)
        let constAttr = context.CreateBoolAttribute(true)
        let valueAttr = context.CreateStringAttribute(value)
        
        // Create the string type (array of i8)
        let i8Type = context.CreateIntegerType(8)
        let arraySize = uint32 (value.Length + 1) // +1 for null terminator
        let arrayType = TypeHelpers.createArrayType context i8Type (int arraySize)
        
        mlirOperationCreate(
            "llvm.global",
            location,
            0u,
            [||],
            0u,
            [||],
            3u,
            [|
                NamedAttributeHelpers.createNamedAttribute context "sym_name" symNameAttr
                NamedAttributeHelpers.createNamedAttribute context "constant" constAttr
                NamedAttributeHelpers.createNamedAttribute context "value" valueAttr
            |],
            0u,
            [||])