#nowarn "9"

namespace Alloy.Time

open FSharp.NativeInterop
open Alloy

/// <summary>
/// Native interoperability layer for Alloy, providing P/Invoke-like functionality 
/// without System.Runtime.InteropServices dependencies in final compilation
/// (uses System.Runtime.InteropServices temporarily for proof of concept)
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

    /// <summary>
    /// Exception thrown when a native library cannot be loaded
    /// </summary>
    exception NativeLibraryNotFoundException of libraryName: string * errorMessage: string

    /// <summary>
    /// Exception thrown when a native function cannot be found
    /// </summary>
    exception NativeFunctionNotFoundException of libraryName: string * functionName: string * errorMessage: string

    // Temporary P/Invoke declarations for proof of concept
    // These will be replaced with MLIR/LLVM generated calls in final implementation
    module private TemporaryPInvoke =
        open System.Runtime.InteropServices
        
        // Windows API declarations
        [<DllImport("kernel32.dll", SetLastError = true)>]
        extern void GetSystemTimeAsFileTime(nativeint lpSystemTimeAsFileTime)
        
        [<DllImport("kernel32.dll", SetLastError = true)>]
        extern bool QueryPerformanceCounter(nativeint lpPerformanceCount)
        
        [<DllImport("kernel32.dll", SetLastError = true)>]
        extern bool QueryPerformanceFrequency(nativeint lpFrequency)
        
        [<DllImport("kernel32.dll", SetLastError = true)>]
        extern void Sleep(uint32 dwMilliseconds)

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
    let invokeFunc0<'TResult> (import: NativeImport<unit -> 'TResult>) : 'TResult =
        // For proof of concept, we'll use direct mapping to known functions
        // In final implementation, this will use MLIR/LLVM generated function calls
        failwith $"invokeFunc0 not implemented for {import.FunctionName}"

    /// <summary>
    /// Invokes a native function with one argument
    /// </summary>
    let invokeFunc1<'T1, 'TResult> (import: NativeImport<'T1 -> 'TResult>) (arg1: 'T1) : 'TResult =
        // Platform-agnostic dispatch based on function name
        // In final implementation, this will be MLIR/LLVM generated calls
        match import.LibraryName, import.FunctionName with
        | "kernel32", "GetSystemTimeAsFileTime" ->
            match box arg1 with
            | :? nativeint as ptr ->
                TemporaryPInvoke.GetSystemTimeAsFileTime(ptr)
                unbox<'TResult> (box ())
            | _ -> failwith "GetSystemTimeAsFileTime expects nativeint argument"
            
        | "kernel32", "QueryPerformanceCounter" ->
            match box arg1 with
            | :? nativeint as ptr ->
                let result = TemporaryPInvoke.QueryPerformanceCounter(ptr)
                unbox<'TResult> (box result)
            | _ -> failwith "QueryPerformanceCounter expects nativeint argument"
            
        | "kernel32", "QueryPerformanceFrequency" ->
            match box arg1 with
            | :? nativeint as ptr ->
                let result = TemporaryPInvoke.QueryPerformanceFrequency(ptr)
                unbox<'TResult> (box result)
            | _ -> failwith "QueryPerformanceFrequency expects nativeint argument"
            
        | "kernel32", "Sleep" ->
            match box arg1 with
            | :? uint32 as ms ->
                TemporaryPInvoke.Sleep(ms)
                unbox<'TResult> (box ())
            | _ -> failwith "Sleep expects uint32 argument"
            
        | libName, funcName ->
            failwith $"Native function {libName}.{funcName} not implemented in proof of concept"

    /// <summary>
    /// Invokes a native function with two arguments
    /// </summary>
    let invokeFunc2<'T1, 'T2, 'TResult> 
        (import: NativeImport<'T1 -> 'T2 -> 'TResult>) (arg1: 'T1) (arg2: 'T2) : 'TResult =
        failwith $"invokeFunc2 not implemented for {import.FunctionName}"
        
    /// <summary>
    /// Invokes a native function with three arguments
    /// </summary>
    let invokeFunc3<'T1, 'T2, 'T3, 'TResult> 
        (import: NativeImport<'T1 -> 'T2 -> 'T3 -> 'TResult>) (arg1: 'T1) (arg2: 'T2) (arg3: 'T3) : 'TResult =
        failwith $"invokeFunc3 not implemented for {import.FunctionName}"