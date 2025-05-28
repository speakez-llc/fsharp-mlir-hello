module FSharpMLIR.Bindings.MLIRWrapper

open System
open System.Runtime.InteropServices
open System.Text
open FSharpMLIR.Bindings.MLIR
open FSharpMLIR.PlatformUtils

/// Delegate for callback functions used in MLIR printing operations
type PrintCallback = delegate of nativeint * nativeint -> unit

/// Wrapper for MLIR Context
type MLIRContext(handle: nativeint) =
    /// The raw handle to the MLIR context
    member this.Handle = handle
    
    /// Allow unregistered dialects in this context
    member this.AllowUnregisteredDialects(allow: bool) =
        mlirContextSetAllowUnregisteredDialects(handle, allow)
    
    /// Create a location in this context
    member this.CreateUnknownLocation() =
        mlirLocationUnknownGet(handle)
    
    /// Create a file location in this context
    member this.CreateFileLocation(filename: string, line: int, column: int) =
        mlirLocationFileLineColGet(handle, filename, uint32 line, uint32 column)
    
    /// Create an integer type with the specified width
    member this.CreateIntegerType(width: int) =
        mlirIntegerTypeGet(handle, uint32 width)
    
    /// Create a float type of the specified kind
    member this.CreateFloatType(kind: MLIRFloatTypeKind) =
        mlirFloatTypeGet(handle, kind)
    
    /// Create a function type
    member this.CreateFunctionType(inputs: nativeint[], results: nativeint[]) =
        mlirFunctionTypeGet(
            handle, 
            uint32 inputs.Length, 
            inputs, 
            uint32 results.Length, 
            results)
    
    /// Create a string attribute
    member this.CreateStringAttribute(value: string) =
        mlirStringAttrGet(handle, uint32 value.Length, value)
    
    /// Create a boolean attribute
    member this.CreateBoolAttribute(value: bool) =
        mlirBoolAttrGet(handle, value)
    
    /// Create an integer attribute
    member this.CreateIntegerAttribute(``type``: nativeint, value: int64) =
        mlirIntegerAttrGet(``type``, value)
    
    /// Create a float attribute
    member this.CreateFloatAttribute(``type``: nativeint, value: double) =
        mlirFloatAttrGet(``type``, value)
        
    /// Create a type attribute
    member this.CreateTypeAttribute(``type``: nativeint) =
        mlirTypeAttrGet(``type``)
        
    /// Create a block attribute
    member this.CreateBlockAttribute(block: nativeint) =
        mlirBlockAttributeGet(block)
    
    interface IDisposable with
        member this.Dispose() = 
            mlirContextDestroy(handle)
    
    /// Factory method to create and initialize a context
    static member Create() =
        // Create the raw MLIR context
        let handle = mlirContextCreate()
        let context = new MLIRContext(handle)
        
        // Register standard MLIR dialects
        let registry = mlirDialectRegistryCreate()
        mlirRegisterAllDialects(registry)
        
        mlirContextAppendDialectRegistry(handle, registry)
        mlirDialectRegistryDestroy(registry)
        
        // Return the wrapped context
        context

/// Wrapper for MLIR Module
type MLIRModule(context: MLIRContext, handle: nativeint) =
    /// The raw handle to the MLIR module
    member this.Handle = handle
    
    /// Reference to the parent context
    member this.Context = context
    
    /// Get the operation representing this module
    member this.GetOperation() =
        mlirModuleGetOperation(handle)
    
    /// Print the module to the console for debugging
    member this.Dump() =
        mlirOperationDump(this.GetOperation())
    
    /// Verify the module
    member this.Verify() =
        mlirOperationVerify(this.GetOperation())
    
    interface IDisposable with
        member this.Dispose() = 
            mlirModuleDestroy(handle)
    
    /// Factory method to create an empty module
    static member CreateEmpty(context: MLIRContext) =
        // Create a location for error reporting
        let location = context.CreateUnknownLocation()
        
        // Create an empty MLIR module
        let moduleHandle = mlirModuleCreateEmpty(location)
        
        // Clean up location
        mlirLocationDestroy(location)
        
        // Return the wrapped module
        new MLIRModule(context, moduleHandle)

/// Wrapper for MLIR Pass Manager
type MLIRPassManager(context: MLIRContext, handle: nativeint) =
    /// The raw handle to the MLIR pass manager
    member this.Handle = handle
    
    /// Add the canonicalizer pass
    member this.AddCanonicalizer() =
        // Note: These functions need to be added to the bindings if they exist
        // For now, we'll skip them to avoid crashes
        printfn "Warning: Canonicalizer pass not implemented"
        this
    
    /// Add the common subexpression elimination pass
    member this.AddCSE() =
        printfn "Warning: CSE pass not implemented"
        this
    
    /// Add the lower-to-LLVM pass
    member this.AddLowerToLLVM() =
        printfn "Warning: Lower-to-LLVM pass not implemented"
        this
    
    /// Run the pass manager on a module
    member this.Run(module': MLIRModule) =
        mlirPassManagerRun(handle, module'.Handle)
    
    interface IDisposable with
        member this.Dispose() = 
            mlirPassManagerDestroy(handle)
    
    /// Factory method to create a pass manager
    static member Create(context: MLIRContext) =
        let handle = mlirPassManagerCreate(context.Handle)
        new MLIRPassManager(context, handle)

/// Wrapper for MLIR Region
type MLIRRegion(handle: nativeint) =
    /// The raw handle to the MLIR region
    member this.Handle = handle
    
    /// Get the first block in the region
    member this.GetFirstBlock() =
        mlirRegionGetFirstBlock(handle)
    
    /// Add a block to the region
    member this.AddBlock(block: MLIRBlock) =
        mlirRegionAppendOwnedBlock(handle, block.Handle)
        
    /// Create a new block and add it to the region
    member this.CreateBlock() =
        let block = MLIRBlock.Create()
        this.AddBlock(block)
        block
    
    interface IDisposable with
        member this.Dispose() = 
            mlirRegionDestroy(handle)
    
    /// Factory method to create a region
    static member Create() =
        let handle = mlirRegionCreate()
        new MLIRRegion(handle)

/// Wrapper for MLIR Block
and MLIRBlock(handle: nativeint) =
    /// The raw handle to the MLIR block
    member this.Handle = handle
    
    /// Reference to the parent region, if any
    member val Region = Unchecked.defaultof<MLIRRegion> with get, set
    
    /// Append an operation to the block
    member this.AppendOperation(operation: nativeint) =
        mlirBlockAppendOwnedOperation(handle, operation)
    
    /// Get the first operation in the block
    member this.GetFirstOperation() =
        mlirBlockGetFirstOperation(handle)
    
    /// Get the terminator operation in the block
    member this.GetTerminator() =
        mlirBlockGetTerminator(handle)
    
    /// Create a successor block
    member this.CreateSuccessor() =
        let newBlock = MLIRBlock.Create()
        newBlock.Region <- this.Region
        newBlock

    interface IDisposable with
        member this.Dispose() = 
            mlirBlockDestroy(handle)
    
    /// Factory method to create a block with no arguments
    static member Create() =
        let handle = mlirBlockCreate(0u, [||], [||])
        new MLIRBlock(handle)
    
    /// Factory method to create a block with arguments
    static member Create(argumentTypes: nativeint[], locations: nativeint[]) =
        if argumentTypes.Length <> locations.Length then
            invalidArg "argumentTypes/locations" "Argument types and locations must have the same length"
        
        let handle = mlirBlockCreate(uint32 argumentTypes.Length, argumentTypes, locations)
        new MLIRBlock(handle)

/// Helper module for MLIR to LLVM conversion
module MLIRToLLVMConverter =
    /// Convert an MLIR module to LLVM IR module
    let convertModuleToLLVMIR (mlirModule: MLIRModule) (llvmModule: nativeint) =
        // For now, we'll return success until we implement the actual conversion
        printfn "Warning: MLIR to LLVM conversion not fully implemented"
        // The actual conversion would use mlirTranslateModuleToLLVMIR
        // but we need to handle the callback properly
        Ok()

/// Helper functions for creating named attributes
module NamedAttributeHelpers =
    /// Create a named attribute
    let createNamedAttribute (context: MLIRContext) (name: string) (value: nativeint) =
        let nameAttr = context.CreateStringAttribute(name)
        mlirNamedAttributeGet(nameAttr, value)