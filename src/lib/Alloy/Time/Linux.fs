#nowarn "9"

namespace Alloy.Time

open FSharp.NativeInterop
open Alloy.Time.NativeInterop
open Alloy.Time.Platform
open Alloy.Numerics

/// <summary>
/// Linux platform-specific time implementation
/// </summary>
module Linux =
    /// <summary>
    /// Unix time structures
    /// </summary>
    [<Struct>]
    type Timespec =
        val mutable tv_sec: int64
        val mutable tv_nsec: int64
        
        /// <summary>
        /// Converts Timespec to ticks (100-nanosecond intervals)
        /// </summary>
        member this.ToTicks() =
            add (multiply this.tv_sec 10000000L) (divide this.tv_nsec 100L)

    /// <summary>
    /// Unix clock IDs
    /// </summary>
    type ClockID =
        | CLOCK_REALTIME = 0
        | CLOCK_MONOTONIC = 1
        | CLOCK_PROCESS_CPUTIME_ID = 2
        | CLOCK_THREAD_CPUTIME_ID = 3

    /// <summary>
    /// Unix time functions using enhanced P/Invoke-like API
    /// </summary>
    module LibC =
        let inline importFunc2<'T1, 'T2, 'TResult> libraryName functionName =
            {
                LibraryName = libraryName
                FunctionName = functionName
                CallingConvention = CallingConvention.Cdecl
                CharSet = CharSet.Ansi
                SupressErrorHandling = false
            } : NativeImport<'T1 -> 'T2 -> 'TResult>
        
        let clockGettimeImport = 
            importFunc2<int, nativeint, int> "libc" "clock_gettime"
            
        let nanosleepImport = 
            importFunc2<nativeint, nativeint, int> "libc" "nanosleep"
        
        /// <summary>
        /// Gets the current time from the specified clock
        /// </summary>
        let clockGettime(clockId: ClockID) =
            let timespec = NativePtr.stackalloc<Timespec> 1
            let result = invokeFunc2 
                            clockGettimeImport
                            (int clockId) 
                            (NativePtr.toNativeInt timespec)
            
            if equals result 0 then
                let ts = NativePtr.read timespec
                ts.ToTicks()
            else
                failwith $"clock_gettime failed with error code {result}"
        
        /// <summary>
        /// Suspends execution for the specified time interval
        /// </summary>
        let nanosleep(seconds: int64, nanoseconds: int64) =
            let req = NativePtr.stackalloc<Timespec> 1
            let mutable reqTs = NativePtr.get req 0
            reqTs.tv_sec <- seconds
            reqTs.tv_nsec <- nanoseconds
            NativePtr.set req 0 reqTs
            
            let rem = NativePtr.stackalloc<Timespec> 1
            
            let result = invokeFunc2 
                            nanosleepImport
                            (NativePtr.toNativeInt req) 
                            (NativePtr.toNativeInt rem)
                            
            if lessThan result 0 then
                failwith "nanosleep failed"

    /// <summary>
    /// Helper constants for time conversion
    /// </summary>
    module TimeConversion =
        let private unixToNetTicksOffset = 621355968000000000L
        
        /// <summary>
        /// Convert Unix time (seconds since 1970) to .NET ticks
        /// </summary>
        let unixTimeToTicks (unixTime: int64) =
            add (multiply unixTime 10000000L) unixToNetTicksOffset
        
        /// <summary>
        /// Convert .NET ticks to Unix time
        /// </summary>
        let ticksToUnixTime (ticks: int64) =
            divide (subtract ticks unixToNetTicksOffset) 10000000L

    /// <summary>
    /// Linux platform implementation of IPlatformTime
    /// </summary>
    type LinuxTimeImplementation() =
        interface IPlatformTime with
            /// <summary>
            /// Gets the current time in ticks (100-nanosecond intervals since January 1, 0001)
            /// </summary>
            member _.GetCurrentTicks() =
                let unixTime = LibC.clockGettime(ClockID.CLOCK_REALTIME)
                add (TimeConversion.unixTimeToTicks(divide unixTime 10000000L)) (modulo unixTime 10000000L)
            
            /// <summary>
            /// Gets the current UTC time in ticks
            /// </summary>
            member _.GetUtcNow() =
                let unixTime = LibC.clockGettime(ClockID.CLOCK_REALTIME)
                add (TimeConversion.unixTimeToTicks(divide unixTime 10000000L)) (modulo unixTime 10000000L)
            
            /// <summary>
            /// Gets the system time as file time (100-nanosecond intervals since January 1, 1601)
            /// </summary>
            member _.GetSystemTimeAsFileTime() =
                let unixTime = LibC.clockGettime(ClockID.CLOCK_REALTIME)
                add unixTime 116444736000000000L
            
            /// <summary>
            /// Gets high-resolution performance counter ticks
            /// </summary>
            member _.GetHighResolutionTicks() =
                LibC.clockGettime(ClockID.CLOCK_MONOTONIC)
            
            /// <summary>
            /// Gets the frequency of the high-resolution performance counter
            /// </summary>
            member _.GetTickFrequency() =
                10000000L
            
            /// <summary>
            /// Sleeps for the specified number of milliseconds
            /// </summary>
            member _.Sleep(milliseconds) =
                let seconds = int64 (divide milliseconds 1000)
                let nanoseconds = int64 (multiply (modulo milliseconds 1000) 1000000)
                LibC.nanosleep(seconds, nanoseconds)

    /// <summary>
    /// Factory function to create a Linux time implementation
    /// </summary>
    let createImplementation() =
        LinuxTimeImplementation() :> IPlatformTime