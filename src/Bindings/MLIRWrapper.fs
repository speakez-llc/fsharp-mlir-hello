module FSharpMLIR.Bindings.MLIRWrapper

open System
open System.Runtime.InteropServices
open System.Text
open FSharpMLIR.Bindings.MLIR
open FSharpMLIR.PlatformUtils

/// Exception thrown when MLIR operations fail
exception MLIRException of string

/// Helper module for working with MLIR string references
module internal MLIRStringUtils =
    let createStringRef (str: string) =
        let bytes = System.Text.Encoding.UTF8.GetBytes(str)
        let ptr = Marshal.AllocHGlobal(bytes.Length)
        Marshal.Copy(bytes, 0, ptr, bytes.Length)
        MLIRStringRef(data = ptr, length = nativeint bytes.Length)
    
    let freeStringRef (stringRef: MLIRStringRef) =
        if stringRef.data <> nativeint.Zero then
            Marshal.FreeHGlobal(stringRef.data)
    
    let stringFromStringRef (stringRef: MLIRStringRef) =
        if stringRef.data <> nativeint.Zero && stringRef.length > nativeint.Zero then
            let bytes = Array.zeroCreate<byte> (int stringRef.length)
            Marshal.Copy(stringRef.data, bytes, 0, int stringRef.length)
            System.Text.Encoding.UTF8.GetString(bytes)
        else
            ""

/// Wrapper for MLIR Context
type MLIRContext(handle: MLIRContext) =
    let mutable disposed = false
    
    /// The raw handle to the MLIR context
    member this.Handle = handle
    
    /// Allow unregistered dialects in this context
    member this.AllowUnregisteredDialects(allow: bool) =
        if disposed then invalidOp "Context has been disposed"
        invoke mlirContextSetAllowUnregisteredDialects handle allow
    
    /// Get whether unregistered dialects are allowed
    member this.GetAllowUnregisteredDialects() =
        if disposed then invalidOp "Context has been disposed"
        invoke mlirContextGetAllowUnregisteredDialects handle
    
    /// Get the number of registered dialects
    member this.GetNumRegisteredDialects() =
        if disposed then invalidOp "Context has been disposed"
        int (invoke mlirContextGetNumRegisteredDialects handle)
    
    /// Get the number of loaded dialects
    member this.GetNumLoadedDialects() =
        if disposed then invalidOp "Context has been disposed"
        int (invoke mlirContextGetNumLoadedDialects handle)
    
    /// Create an unknown location in this context
    member this.CreateUnknownLocation() =
        if disposed then invalidOp "Context has been disposed"
        MLIRLocation(invoke mlirLocationUnknownGet handle)
    
    /// Create a file location in this context
    member this.CreateFileLocation(filename: string, line: int, column: int) =
        if disposed then invalidOp "Context has been disposed"
        let filenameRef = MLIRStringUtils.createStringRef(filename)
        try
            let loc = invoke mlirLocationFileLineColGet handle filenameRef (uint32 line) (uint32 column)
            MLIRLocation(loc)
        finally
            MLIRStringUtils.freeStringRef(filenameRef)
    
    /// Create an integer type with the specified width
    member this.CreateIntegerType(width: int) =
        if disposed then invalidOp "Context has been disposed"
        MLIRType(invoke mlirIntegerTypeGet handle (uint32 width))
    
    /// Create a signed integer type with the specified width
    member this.CreateSignedIntegerType(width: int) =
        if disposed then invalidOp "Context has been disposed"
        MLIRType(invoke mlirIntegerTypeSignedGet handle (uint32 width))
    
    /// Create an unsigned integer type with the specified width
    member this.CreateUnsignedIntegerType(width: int) =
        if disposed then invalidOp "Context has been disposed"
        MLIRType(invoke mlirIntegerTypeUnsignedGet handle (uint32 width))
    
    /// Create an index type
    member this.CreateIndexType() =
        if disposed then invalidOp "Context has been disposed"
        MLIRType(invoke mlirIndexTypeGet handle)
    
    /// Create a float type of the specified kind
    member this.CreateFloatType(kind: MLIRFloatTypeKind) =
        if disposed then invalidOp "Context has been disposed"
        match kind with
        | MLIRFloatTypeKind.BF16 -> MLIRType(invoke mlirBF16TypeGet handle)
        | MLIRFloatTypeKind.F16 -> MLIRType(invoke mlirF16TypeGet handle)
        | MLIRFloatTypeKind.F32 -> MLIRType(invoke mlirF32TypeGet handle)
        | MLIRFloatTypeKind.F64 -> MLIRType(invoke mlirF64TypeGet handle)
        | _ -> raise (MLIRException($"Unsupported float type: {kind}"))
    
    /// Create a function type
    member this.CreateFunctionType(inputs: MLIRType[], results: MLIRType[]) =
        if disposed then invalidOp "Context has been disposed"
        let inputHandles = inputs |> Array.map (fun t -> t.Handle)
        let resultHandles = results |> Array.map (fun t -> t.Handle)
        let funcType = invoke mlirFunctionTypeGet 
                          handle 
                          (nativeint inputs.Length) 
                          (if inputHandles.Length = 0 then nativeint.Zero else Marshal.UnsafeAddrOfPinnedArrayElement(inputHandles, 0))
                          (nativeint results.Length) 
                          (if resultHandles.Length = 0 then nativeint.Zero else Marshal.UnsafeAddrOfPinnedArrayElement(resultHandles, 0))
        MLIRType(funcType)
    
    /// Create a none type
    member this.CreateNoneType() =
        if disposed then invalidOp "Context has been disposed"
        MLIRType(invoke mlirNoneTypeGet handle)
    
    /// Create a string attribute
    member this.CreateStringAttribute(value: string) =
        if disposed then invalidOp "Context has been disposed"
        let stringRef = MLIRStringUtils.createStringRef(value)
        try
            MLIRAttribute(invoke mlirStringAttrGet handle stringRef)
        finally
            MLIRStringUtils.freeStringRef(stringRef)
    
    /// Create a boolean attribute
    member this.CreateBoolAttribute(value: bool) =
        if disposed then invalidOp "Context has been disposed"
        MLIRAttribute(invoke mlirBoolAttrGet handle (if value then 1 else 0))
    
    /// Create an integer attribute
    member this.CreateIntegerAttribute(``type``: MLIRType, value: int64) =
        if disposed then invalidOp "Context has been disposed"
        MLIRAttribute(invoke mlirIntegerAttrGet ``type``.Handle value)
    
    /// Create a float attribute (double precision)
    member this.CreateFloatAttribute(``type``: MLIRType, value: double) =
        if disposed then invalidOp "Context has been disposed"
        MLIRAttribute(invoke mlirFloatAttrDoubleGet handle ``type``.Handle value)
        
    /// Create a type attribute
    member this.CreateTypeAttribute(``type``: MLIRType) =
        if disposed then invalidOp "Context has been disposed"
        MLIRAttribute(invoke mlirTypeAttrGet ``type``.Handle)
    
    /// Create a unit attribute
    member this.CreateUnitAttribute() =
        if disposed then invalidOp "Context has been disposed"
        MLIRAttribute(invoke mlirUnitAttrGet handle)
    
    /// Append a dialect registry to this context
    member this.AppendDialectRegistry(registry: MLIRDialectRegistry) =
        if disposed then invalidOp "Context has been disposed"
        invoke mlirContextAppendDialectRegistry handle registry.Handle
    
    interface IDisposable with
        member this.Dispose() = 
            if not disposed then
                invoke mlirContextDestroy handle
                disposed <- true
    
    /// Factory method to create and initialize a context
    static member Create() =
        let handle = invoke mlirContextCreate ()
        new MLIRContext(handle)
    
    /// Factory method to create a context with all dialects registered
    static member CreateWithAllDialects() =
        let context = MLIRContext.Create()
        let registry = MLIRDialectRegistry.Create()
        registry.RegisterAllDialects()
        context.AppendDialectRegistry(registry)
        registry.Dispose()
        context

/// Wrapper for MLIR Dialect Registry
and MLIRDialectRegistry(handle: MLIRDialectRegistry) =
    let mutable disposed = false
    
    /// The raw handle to the MLIR dialect registry
    member this.Handle = handle
    
    /// Register all standard dialects
    member this.RegisterAllDialects() =
        if disposed then invalidOp "Dialect registry has been disposed"
        invoke mlirRegisterAllDialects handle
    
    interface IDisposable with
        member this.Dispose() = 
            if not disposed then
                invoke mlirDialectRegistryDestroy handle
                disposed <- true
    
    /// Factory method to create a dialect registry
    static member Create() =
        let handle = invoke mlirDialectRegistryCreate ()
        new MLIRDialectRegistry(handle)

/// Wrapper for MLIR Location
and MLIRLocation(handle: MLIRLocation) =
    /// The raw handle to the MLIR location
    member this.Handle = handle
    
    /// Check if two locations are equal
    member this.Equals(other: MLIRLocation) =
        invoke mlirLocationEqual handle other.Handle
    
    /// Print this location to a string
    member this.Print() =
        let mutable result = ""
        let callback = MLIRStringCallback(fun stringRef userData ->
            result <- MLIRStringUtils.stringFromStringRef(stringRef))
        invoke mlirLocationPrint handle callback nativeint.Zero
        result
    
    override this.ToString() = this.Print()

/// Wrapper for MLIR Type
and MLIRType(handle: MLIRType) =
    /// The raw handle to the MLIR type
    member this.Handle = handle
    
    /// Check if this is an integer type
    member this.IsInteger() = invoke mlirTypeIsAInteger handle
    
    /// Check if this is a float type
    member this.IsFloat() = invoke mlirTypeIsAFloat handle
    
    /// Check if this is a function type
    member this.IsFunction() = invoke mlirTypeIsAFunction handle
    
    /// Check if this is an index type
    member this.IsIndex() = invoke mlirTypeIsAIndex handle
    
    /// Get the width of an integer type
    member this.GetIntegerWidth() =
        if not (this.IsInteger()) then
            raise (MLIRException("Type is not an integer type"))
        int (invoke mlirIntegerTypeGetWidth handle)
    
    /// Get the width of a float type
    member this.GetFloatWidth() =
        if not (this.IsFloat()) then
            raise (MLIRException("Type is not a float type"))
        int (invoke mlirFloatTypeGetWidth handle)
    
    /// Get the number of inputs for a function type
    member this.GetFunctionNumInputs() =
        if not (this.IsFunction()) then
            raise (MLIRException("Type is not a function type"))
        int (invoke mlirFunctionTypeGetNumInputs handle)
    
    /// Get the number of results for a function type
    member this.GetFunctionNumResults() =
        if not (this.IsFunction()) then
            raise (MLIRException("Type is not a function type"))
        int (invoke mlirFunctionTypeGetNumResults handle)
    
    /// Get an input type from a function type
    member this.GetFunctionInput(pos: int) =
        if not (this.IsFunction()) then
            raise (MLIRException("Type is not a function type"))
        MLIRType(invoke mlirFunctionTypeGetInput handle (nativeint pos))
    
    /// Get a result type from a function type
    member this.GetFunctionResult(pos: int) =
        if not (this.IsFunction()) then
            raise (MLIRException("Type is not a function type"))
        MLIRType(invoke mlirFunctionTypeGetResult handle (nativeint pos))
    
    /// Print this type to a string
    member this.Print() =
        let mutable result = ""
        let callback = MLIRStringCallback(fun stringRef userData ->
            result <- MLIRStringUtils.stringFromStringRef(stringRef))
        invoke mlirTypePrint handle callback nativeint.Zero
        result
    
    override this.ToString() = this.Print()

/// Wrapper for MLIR Attribute
and MLIRAttribute(handle: MLIRAttribute) =
    /// The raw handle to the MLIR attribute
    member this.Handle = handle
    
    /// Check if this is a string attribute
    member this.IsString() = invoke mlirAttributeIsAString handle
    
    /// Check if this is a bool attribute
    member this.IsBool() = invoke mlirAttributeIsABool handle
    
    /// Check if this is an integer attribute
    member this.IsInteger() = invoke mlirAttributeIsAInteger handle
    
    /// Check if this is a float attribute
    member this.IsFloat() = invoke mlirAttributeIsAFloat handle
    
    /// Check if this is a type attribute
    member this.IsType() = invoke mlirAttributeIsAType handle
    
    /// Check if this is a unit attribute
    member this.IsUnit() = invoke mlirAttributeIsAUnit handle
    
    /// Get the string value (if this is a string attribute)
    member this.GetStringValue() =
        if not (this.IsString()) then
            raise (MLIRException("Attribute is not a string attribute"))
        let stringRef = invoke mlirStringAttrGetValue handle
        MLIRStringUtils.stringFromStringRef(stringRef)
    
    /// Get the boolean value (if this is a bool attribute)
    member this.GetBoolValue() =
        if not (this.IsBool()) then
            raise (MLIRException("Attribute is not a bool attribute"))
        invoke mlirBoolAttrGetValue handle
    
    /// Get the integer value (if this is an integer attribute)
    member this.GetIntValue() =
        if not (this.IsInteger()) then
            raise (MLIRException("Attribute is not an integer attribute"))
        invoke mlirIntegerAttrGetValueInt handle
    
    /// Get the float value (if this is a float attribute)
    member this.GetFloatValue() =
        if not (this.IsFloat()) then
            raise (MLIRException("Attribute is not a float attribute"))
        invoke mlirFloatAttrGetValueDouble handle
    
    /// Get the type value (if this is a type attribute)
    member this.GetTypeValue() =
        if not (this.IsType()) then
            raise (MLIRException("Attribute is not a type attribute"))
        MLIRType(invoke mlirTypeAttrGetValue handle)
    
    /// Print this attribute to a string
    member this.Print() =
        let mutable result = ""
        let callback = MLIRStringCallback(fun stringRef userData ->
            result <- MLIRStringUtils.stringFromStringRef(stringRef))
        invoke mlirAttributePrint handle callback nativeint.Zero
        result
    
    override this.ToString() = this.Print()

/// Wrapper for MLIR Module
type MLIRModule(context: MLIRContext, handle: MLIRModule) =
    let mutable disposed = false
    
    /// The raw handle to the MLIR module
    member this.Handle = handle
    
    /// Reference to the parent context
    member this.Context = context
    
    /// Get the operation representing this module
    member this.GetOperation() =
        if disposed then invalidOp "Module has been disposed"
        MLIROperation(invoke mlirModuleGetOperation handle)
    
    /// Print the module to the console for debugging
    member this.Dump() =
        if disposed then invalidOp "Module has been disposed"
        let op = this.GetOperation()
        op.Dump()
    
    /// Verify the module
    member this.Verify() =
        if disposed then invalidOp "Module has been disposed"
        let op = this.GetOperation()
        op.Verify()
    
    /// Print this module to a string
    member this.Print() =
        if disposed then invalidOp "Module has been disposed"
        let op = this.GetOperation()
        op.Print()
    
    interface IDisposable with
        member this.Dispose() = 
            if not disposed then
                invoke mlirModuleDestroy handle
                disposed <- true
    
    /// Factory method to create an empty module
    static member CreateEmpty(context: MLIRContext) =
        let location = context.CreateUnknownLocation()
        let moduleHandle = invoke mlirModuleCreateEmpty location.Handle
        new MLIRModule(context, moduleHandle)

/// Wrapper for MLIR Operation
and MLIROperation(handle: MLIROperation) =
    /// The raw handle to the MLIR operation
    member this.Handle = handle
    
    /// Verify this operation
    member this.Verify() =
        let result = invoke mlirOperationVerify handle
        result.IsSuccess
    
    /// Get the number of results
    member this.GetNumResults() =
        int (invoke mlirOperationGetNumResults handle)
    
    /// Get a result by position
    member this.GetResult(pos: int) =
        MLIRValue(invoke mlirOperationGetResult handle (nativeint pos))
    
    /// Set an attribute by name
    member this.SetAttribute(name: string, attr: MLIRAttribute) =
        let nameRef = MLIRStringUtils.createStringRef(name)
        try
            invoke mlirOperationSetAttributeByName handle nameRef attr.Handle
        finally
            MLIRStringUtils.freeStringRef(nameRef)
    
    /// Get an attribute by name
    member this.GetAttribute(name: string) =
        let nameRef = MLIRStringUtils.createStringRef(name)
        try
            let attrHandle = invoke mlirOperationGetAttributeByName handle nameRef
            if attrHandle <> nativeint.Zero then
                Some(MLIRAttribute(attrHandle))
            else
                None
        finally
            MLIRStringUtils.freeStringRef(nameRef)
    
    /// Print this operation for debugging
    member this.Dump() =
        invoke mlirOperationDump handle
    
    /// Print this operation to a string
    member this.Print() =
        let mutable result = ""
        let callback = MLIRStringCallback(fun stringRef userData ->
            result <- MLIRStringUtils.stringFromStringRef(stringRef))
        invoke mlirOperationPrint handle callback nativeint.Zero
        result
    
    override this.ToString() = this.Print()

/// Wrapper for MLIR Value
and MLIRValue(handle: MLIRValue) =
    /// The raw handle to the MLIR value
    member this.Handle = handle
    
    /// Get the type of this value
    member this.GetType() =
        MLIRType(invoke mlirValueGetType handle)
    
    /// Print this value to a string
    member this.Print() =
        let mutable result = ""
        let callback = MLIRStringCallback(fun stringRef userData ->
            result <- MLIRStringUtils.stringFromStringRef(stringRef))
        invoke mlirValuePrint handle callback nativeint.Zero
        result
    
    override this.ToString() = this.Print()

/// Wrapper for MLIR Pass Manager
type MLIRPassManager(context: MLIRContext, handle: MLIRPassManager) =
    let mutable disposed = false
    
    /// The raw handle to the MLIR pass manager
    member this.Handle = handle
    
    /// Enable IR printing
    member this.EnableIRPrinting(printBeforeAll: bool, printAfterAll: bool, printModuleScope: bool, printAfterOnlyOnChange: bool, printAfterOnlyOnFailure: bool) =
        if disposed then invalidOp "Pass manager has been disposed"
        let flags = invoke mlirOpPrintingFlagsCreate ()
        let treePrintingPath = MLIRStringUtils.createStringRef("")
        try
            invoke mlirPassManagerEnableIRPrinting handle printBeforeAll printAfterAll printModuleScope printAfterOnlyOnChange printAfterOnlyOnFailure flags treePrintingPath
        finally
            MLIRStringUtils.freeStringRef(treePrintingPath)
            invoke mlirOpPrintingFlagsDestroy flags
    
    /// Enable verification
    member this.EnableVerifier(enable: bool) =
        if disposed then invalidOp "Pass manager has been disposed"
        invoke mlirPassManagerEnableVerifier handle enable
    
    /// Get nested pass manager for an operation type
    member this.GetNestedUnder(operationName: string) =
        if disposed then invalidOp "Pass manager has been disposed"
        let nameRef = MLIRStringUtils.createStringRef(operationName)
        try
            let nestedHandle = invoke mlirPassManagerGetNestedUnder handle nameRef
            MLIROpPassManager(nestedHandle)
        finally
            MLIRStringUtils.freeStringRef(nameRef)
    
    /// Run the pass manager on a module
    member this.Run(module': MLIRModule) =
        if disposed then invalidOp "Pass manager has been disposed"
        let result = invoke mlirPassManagerRunOnOp handle module'.GetOperation().Handle
        result.IsSuccess
    
    interface IDisposable with
        member this.Dispose() = 
            if not disposed then
                invoke mlirPassManagerDestroy handle
                disposed <- true
    
    /// Factory method to create a pass manager
    static member Create(context: MLIRContext) =
        let handle = invoke mlirPassManagerCreate context.Handle
        new MLIRPassManager(context, handle)

/// Wrapper for MLIR Op Pass Manager
and MLIROpPassManager(handle: MLIROpPassManager) =
    /// The raw handle to the MLIR op pass manager
    member this.Handle = handle
    
    /// Get nested pass manager for an operation type
    member this.GetNestedUnder(operationName: string) =
        let nameRef = MLIRStringUtils.createStringRef(operationName)
        try
            let nestedHandle = invoke mlirOpPassManagerGetNestedUnder handle nameRef
            MLIROpPassManager(nestedHandle)
        finally
            MLIRStringUtils.freeStringRef(nameRef)
    
    /// Add a pipeline from string
    member this.AddPipeline(pipelineElements: string) =
        let pipelineRef = MLIRStringUtils.createStringRef(pipelineElements)
        let mutable errorMessage = ""
        let callback = MLIRStringCallback(fun stringRef userData ->
            errorMessage <- MLIRStringUtils.stringFromStringRef(stringRef))
        try
            let result = invoke mlirOpPassManagerAddPipeline handle pipelineRef callback nativeint.Zero
            if result.IsFailure then
                raise (MLIRException($"Failed to add pipeline: {errorMessage}"))
        finally
            MLIRStringUtils.freeStringRef(pipelineRef)

/// Helper module for creating common MLIR constructs
module MLIRBuilders =
    /// Create a simple function declaration
    let createFunctionDeclaration (context: MLIRContext) (name: string) (inputTypes: MLIRType[]) (resultTypes: MLIRType[]) =
        let location = context.CreateUnknownLocation()
        let funcType = context.CreateFunctionType(inputTypes, resultTypes)
        
        // For now, return basic function type - full function creation would require more MLIR bindings
        funcType
    
    /// Create a constant integer operation
    let createConstantInt (context: MLIRContext) (value: int64) (bitWidth: int) =
        let intType = context.CreateIntegerType(bitWidth)
        let valueAttr = context.CreateIntegerAttribute(intType, value)
        // Return the attribute for now - full operation creation would require more bindings
        valueAttr
    
    /// Create a constant float operation  
    let createConstantFloat (context: MLIRContext) (value: double) (floatKind: MLIRFloatTypeKind) =
        let floatType = context.CreateFloatType(floatKind)
        let valueAttr = context.CreateFloatAttribute(floatType, value)
        // Return the attribute for now - full operation creation would require more bindings
        valueAttr

/// Module for MLIR to LLVM conversion
module MLIRToLLVMConverter =
    /// Convert an MLIR module to LLVM IR (placeholder)
    let convertModuleToLLVMIR (mlirModule: MLIRModule) =
        // This would use the actual MLIR to LLVM translation
        // For now, we'll indicate this needs to be implemented with the full MLIR bindings
        if mlirModule.Verify() then
            printfn "Module verification passed - ready for LLVM conversion"
            Ok()
        else
            Error("Module verification failed")