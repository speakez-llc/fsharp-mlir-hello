module FSharpMLIR.Bindings.MLIRWrapper

open FSharpMLIR.Bindings.MLIR
open System

// Wrapper for MLIR Context
type MLIRContext(handle: nativeint) =
    // The raw handle to the MLIR context
    member this.Handle = handle
    
    // Implement IDisposable to clean up resources
    interface IDisposable with
        member this.Dispose() = 
            mlirContextDestroy(handle)
    
    // Factory method to create and initialize a context
    static member Create() =
        // Create the raw MLIR context
        let handle = mlirContextCreate()
        
        // Register standard MLIR dialects
        let registry = mlirDialectRegistryCreate()
        mlirRegisterAllDialects(registry)
        mlirContextAppendDialectRegistry(handle, registry)
        
        // Return the wrapped context
        new MLIRContext(handle)

// Wrapper for MLIR Module
type MLIRModule(context: MLIRContext, handle: nativeint) =
    // The raw handle to the MLIR module
    member this.Handle = handle
    
    // Reference to the parent context
    member this.Context = context
    
    // Implement IDisposable to clean up resources
    interface IDisposable with
        member this.Dispose() = 
            mlirModuleDestroy(handle)
    
    // Factory method to create an empty module
    static member CreateEmpty(context: MLIRContext) =
        // Create a location for error reporting
        let location = mlirLocationUnknownGet(context.Handle)
        
        // Create an empty MLIR module
        let moduleHandle = mlirModuleCreateEmpty(location)
        
        // Return the wrapped module
        new MLIRModule(context, moduleHandle)