#nowarn "9"

namespace Alloy.Time

open FSharp.NativeInterop
open Alloy
open Alloy.ValueOption

/// <summary>
/// Native interoperability layer for Alloy, providing P/Invoke-like functionality 
/// without System.Runtime.InteropServices dependencies
/// </summary>
module NativeInterop =
    /// <summary>
    /// Calling convention for native functions
    /// </summary>
    type CallingConvention =
        | Cdecl = 0
        | StdCall = 1
        | ThisCall = 2
        | FastCall = 3
        | WinApi = 4

    /// <summary>
    /// Character set to use for string marshalling
    /// </summary>
    type CharSet =
        | Ansi = 0
        | Unicode = 1
        | Auto = 2

    /// <summary>
    /// Native function import definition
    /// </summary>
    type NativeImport<'TDelegate> = {
        /// <summary>The name of the library containing the function</summary>
        LibraryName: string
        /// <summary>The name of the function to import</summary>
        FunctionName: string
        /// <summary>Optional calling convention (defaults to Cdecl)</summary>
        CallingConvention: CallingConvention
        /// <summary>Optional character set for string marshalling (defaults to Ansi)</summary>
        CharSet: CharSet
        /// <summary>Set to true to suppress the standard error handling</summary>
        SupressErrorHandling: bool
    }

    type private LibraryHandle = { 
        LibraryName: string
        Handle: nativeint 
    }

    /// <summary>
    /// Exception thrown when a native library cannot be loaded
    /// </summary>
    exception NativeLibraryNotFoundException of libraryName: string * errorMessage: string

    /// <summary>
    /// Exception thrown when a native function cannot be found
    /// </summary>
    exception NativeFunctionNotFoundException of libraryName: string * functionName: string * errorMessage: string

    module NativeLibrary =
        let private libraryHandles = ref ([| |]: LibraryHandle array)
        
        let private findLibrary (libraryName: string) : nativeint option =
            let handles = !libraryHandles
            let rec findInArray idx =
                if idx >= handles.Length then None
                elif handles.[idx].LibraryName = libraryName then Some handles.[idx].Handle
                else findInArray (add idx 1)
            findInArray 0
        
        let private addLibrary (libraryName: string) (handle: nativeint) : unit =
            let handles = !libraryHandles
            let newHandles = Array.append handles [| { LibraryName = libraryName; Handle = handle } |]
            libraryHandles := newHandles
        
        /// <summary>
        /// Load a native library by name - placeholder implementation
        /// </summary>
        let load (libraryPath: string) : nativeint =
            match findLibrary libraryPath with
            | Some handle -> handle
            | None ->
                // In production, this would use platform-specific loading
                // For now, return a placeholder handle
                let handle = nativeint (hash libraryPath)
                addLibrary libraryPath handle
                handle
        
        /// <summary>
        /// Get a function pointer from a library handle - placeholder implementation
        /// </summary>
        let getFunctionPointer (handle: nativeint) (functionName: string) : nativeint =
            // In production, this would resolve actual function pointers
            // For now, return a placeholder
            nativeint (hash (handle, functionName))

    module private FunctionPointerUtility =
        [<Struct>]
        type FunctionPointer = 
            val mutable Pointer: nativeint
            new(ptr) = { Pointer = ptr }

        let inline getFunctionDelegate0<'TResult> (fnPtr: nativeint) : (unit -> 'TResult) =
            fun () -> Unchecked.defaultof<'TResult>

        let inline getFunctionDelegate1<'T1, 'TResult> (fnPtr: nativeint) : ('T1 -> 'TResult) =
            fun (arg1: 'T1) -> Unchecked.defaultof<'TResult>

        let inline getFunctionDelegate2<'T1, 'T2, 'TResult> (fnPtr: nativeint) : ('T1 -> 'T2 -> 'TResult) =
            fun (arg1: 'T1) (arg2: 'T2) -> Unchecked.defaultof<'TResult>

    /// <summary>
    /// Creates a native function import definition
    /// </summary>
    let inline dllImport<'TDelegate> libraryName functionName =
        {
            LibraryName = libraryName
            FunctionName = functionName
            CallingConvention = CallingConvention.Cdecl
            CharSet = CharSet.Ansi
            SupressErrorHandling = false
        }

    /// <summary>
    /// Invokes a native function with no arguments
    /// </summary>
    let inline invokeFunc0<'TResult> (import: NativeImport<unit -> 'TResult>) : 'TResult =
        let fnPtr = NativeLibrary.load import.LibraryName
                    |> fun handle -> NativeLibrary.getFunctionPointer handle import.FunctionName
        let fn = FunctionPointerUtility.getFunctionDelegate0<'TResult> fnPtr
        fn()

    /// <summary>
    /// Invokes a native function with one argument
    /// </summary>
    let inline invokeFunc1<'T1, 'TResult> (import: NativeImport<'T1 -> 'TResult>) (arg1: 'T1) : 'TResult =
        let fnPtr = NativeLibrary.load import.LibraryName
                    |> fun handle -> NativeLibrary.getFunctionPointer handle import.FunctionName
        let fn = FunctionPointerUtility.getFunctionDelegate1<'T1, 'TResult> fnPtr
        fn arg1

    /// <summary>
    /// Invokes a native function with two arguments
    /// </summary>
    let inline invokeFunc2<'T1, 'T2, 'TResult> 
        (import: NativeImport<'T1 -> 'T2 -> 'TResult>) (arg1: 'T1) (arg2: 'T2) : 'TResult =
        let fnPtr = NativeLibrary.load import.LibraryName
                    |> fun handle -> NativeLibrary.getFunctionPointer handle import.FunctionName
        let fn = FunctionPointerUtility.getFunctionDelegate2<'T1, 'T2, 'TResult> fnPtr
        fn arg1 arg2
        
    /// <summary>
    /// Invokes a native function with three arguments
    /// </summary>
    let inline invokeFunc3<'T1, 'T2, 'T3, 'TResult> 
        (import: NativeImport<'T1 -> 'T2 -> 'T3 -> 'TResult>) (arg1: 'T1) (arg2: 'T2) (arg3: 'T3) : 'TResult =
        let fnPtr = NativeLibrary.load import.LibraryName
                    |> fun handle -> NativeLibrary.getFunctionPointer handle import.FunctionName
        let fn = FunctionPointerUtility.getFunctionDelegate3<'T1, 'T2, 'T3, 'TResult> fnPtr
        fn arg1 arg2 arg3