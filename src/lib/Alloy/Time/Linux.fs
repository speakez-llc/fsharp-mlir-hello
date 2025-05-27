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
    /// Unix tm structure for timezone calculations
    /// </summary>
    [<Struct>]
    type TmStruct =
        val mutable tm_sec: int32     // seconds (0-60)
        val mutable tm_min: int32     // minutes (0-59)
        val mutable tm_hour: int32    // hours (0-23)
        val mutable tm_mday: int32    // day of month (1-31)
        val mutable tm_mon: int32     // month (0-11)
        val mutable tm_year: int32    // year - 1900
        val mutable tm_wday: int32    // day of week (0-6, Sunday = 0)
        val mutable tm_yday: int32    // day in year (0-365)
        val mutable tm_isdst: int32   // daylight saving time
        val mutable tm_gmtoff: int64  // seconds east of UTC
        val mutable tm_zone: nativeint // timezone abbreviation

    /// <summary>
    /// Unix clock IDs
    /// </summary>
    type ClockID =
        | CLOCK_REALTIME = 0
        | CLOCK_MONOTONIC = 1
        | CLOCK_PROCESS_CPUTIME_ID = 2
        | CLOCK_THREAD_CPUTIME_ID = 3

    /// <summary>
    /// Linux time functions using enhanced P/Invoke-like API
    /// </summary>
    module LibC =
        let clockGettimeImport : NativeImport<int -> nativeint -> int> = 
            {
                LibraryName = "libc"
                FunctionName = "clock_gettime"
                CallingConvention = CallingConvention.Cdecl
                CharSet = CharSet.Ansi
                SupressErrorHandling = false
            }
            
        let nanosleepImport : NativeImport<nativeint -> nativeint -> int> = 
            {
                LibraryName = "libc"
                FunctionName = "nanosleep"
                CallingConvention = CallingConvention.Cdecl
                CharSet = CharSet.Ansi
                SupressErrorHandling = false
            }
            
        let localtimeRImport : NativeImport<nativeint -> nativeint -> nativeint> = 
            {
                LibraryName = "libc"
                FunctionName = "localtime_r"
                CallingConvention = CallingConvention.Cdecl
                CharSet = CharSet.Ansi
                SupressErrorHandling = false
            }
        
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
        /// Gets timezone information using localtime_r
        /// </summary>
        let getTimezoneOffset() =
            // Get current time
            let currentTime = clockGettime(ClockID.CLOCK_REALTIME)
            let unixTime = divide currentTime 10000000L // Convert ticks to seconds
            
            let timePtr = NativePtr.stackalloc<int64> 1
            NativePtr.set timePtr 0 unixTime
            
            let tmPtr = NativePtr.stackalloc<TmStruct> 1
            
            let result = invokeFunc2 localtimeRImport (NativePtr.toNativeInt timePtr) (NativePtr.toNativeInt tmPtr)
            
            if result <> nativeint 0 then
                let tm = NativePtr.read tmPtr
                // tm_gmtoff is seconds east of UTC, convert to minutes west of UTC
                int (-tm.tm_gmtoff / 60L)
            else
                // Fallback: assume UTC
                0

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
                1000000000L // nanosecond precision
            
            /// <summary>
            /// Sleeps for the specified number of milliseconds
            /// </summary>
            member _.Sleep(milliseconds) =
                let seconds = int64 (divide milliseconds 1000)
                let nanoseconds = int64 (multiply (modulo milliseconds 1000) 1000000)
                LibC.nanosleep(seconds, nanoseconds)
                
            /// <summary>
            /// Gets the current timezone offset from UTC in minutes
            /// </summary>
            member _.GetTimezoneOffsetMinutes() =
                try
                    LibC.getTimezoneOffset()
                with
                | _ -> 0 // Fallback to UTC if timezone detection fails

    /// <summary>
    /// Factory function to create a Linux time implementation
    /// </summary>
    let createImplementation() =
        LinuxTimeImplementation() :> IPlatformTime