#nowarn "9"

namespace Alloy.Time

open FSharp.NativeInterop
open Alloy.Time.NativeInterop
open Alloy.Time.Platform
open Alloy.Numerics

/// <summary>
/// Windows platform-specific time implementation
/// </summary>
module Windows =
    /// <summary>
    /// Windows time structures
    /// </summary>
    [<Struct>]
    type FILETIME =
        val mutable dwLowDateTime: uint32
        val mutable dwHighDateTime: uint32
        
        /// <summary>
        /// Converts FILETIME to 64-bit integer
        /// </summary>
        member this.ToInt64() =
            let low = uint64 this.dwLowDateTime
            let high = uint64 this.dwHighDateTime
            int64 ((high <<< 32) ||| low)

    [<Struct>]
    type SYSTEMTIME =
        val mutable wYear: uint16
        val mutable wMonth: uint16
        val mutable wDayOfWeek: uint16
        val mutable wDay: uint16
        val mutable wHour: uint16
        val mutable wMinute: uint16
        val mutable wSecond: uint16
        val mutable wMilliseconds: uint16

    /// <summary>
    /// Proper Windows TIME_ZONE_INFORMATION structure (172 bytes total)
    /// </summary>
    [<Struct>]
    type TIME_ZONE_INFORMATION =
        val mutable Bias: int32                    // Minutes west of UTC
        // StandardName: WCHAR[32] = 64 bytes
        val mutable StandardName1: int64           // 8 bytes * 8 = 64 bytes
        val mutable StandardName2: int64
        val mutable StandardName3: int64
        val mutable StandardName4: int64
        val mutable StandardName5: int64
        val mutable StandardName6: int64
        val mutable StandardName7: int64
        val mutable StandardName8: int64
        val mutable StandardDate: SYSTEMTIME      // 16 bytes
        val mutable StandardBias: int32            // Additional bias for standard time
        // DaylightName: WCHAR[32] = 64 bytes
        val mutable DaylightName1: int64           // 8 bytes * 8 = 64 bytes
        val mutable DaylightName2: int64
        val mutable DaylightName3: int64
        val mutable DaylightName4: int64
        val mutable DaylightName5: int64
        val mutable DaylightName6: int64
        val mutable DaylightName7: int64
        val mutable DaylightName8: int64
        val mutable DaylightDate: SYSTEMTIME      // 16 bytes
        val mutable DaylightBias: int32            // Additional bias for daylight time (usually negative)

    /// <summary>
    /// Time zone ID constants
    /// </summary>
    type TimeZoneId =
        | Unknown = 0u
        | Standard = 1u
        | Daylight = 2u
        | Invalid = 0xFFFFFFFFu

    /// <summary>
    /// Windows time functions using the enhanced P/Invoke-like API
    /// </summary>
    module Kernel32 =
        let getSystemTimeAsFileTimeImport : NativeImport<nativeint -> unit> = 
            {
                LibraryName = "kernel32"
                FunctionName = "GetSystemTimeAsFileTime"
                CallingConvention = CallingConvention.Cdecl
                CharSet = CharSet.Ansi
                SupressErrorHandling = false
            }
            
        let getSystemTimeImport : NativeImport<nativeint -> unit> = 
            {
                LibraryName = "kernel32"
                FunctionName = "GetSystemTime"
                CallingConvention = CallingConvention.Cdecl
                CharSet = CharSet.Ansi
                SupressErrorHandling = false
            }
            
        let queryPerformanceCounterImport : NativeImport<nativeint -> bool> = 
            {
                LibraryName = "kernel32"
                FunctionName = "QueryPerformanceCounter"
                CallingConvention = CallingConvention.Cdecl
                CharSet = CharSet.Ansi
                SupressErrorHandling = false
            }
            
        let queryPerformanceFrequencyImport : NativeImport<nativeint -> bool> = 
            {
                LibraryName = "kernel32"
                FunctionName = "QueryPerformanceFrequency"
                CallingConvention = CallingConvention.Cdecl
                CharSet = CharSet.Ansi
                SupressErrorHandling = false
            }
            
        let sleepImport : NativeImport<uint32 -> unit> = 
            {
                LibraryName = "kernel32"
                FunctionName = "Sleep"
                CallingConvention = CallingConvention.Cdecl
                CharSet = CharSet.Ansi
                SupressErrorHandling = false
            }
            
        let getTimeZoneInformationImport : NativeImport<nativeint -> uint32> = 
            {
                LibraryName = "kernel32"
                FunctionName = "GetTimeZoneInformation"
                CallingConvention = CallingConvention.Cdecl
                CharSet = CharSet.Ansi
                SupressErrorHandling = false
            }
        
        /// <summary>
        /// Gets the current system time as a FILETIME
        /// </summary>
        let getSystemTimeAsFileTime() =
            let fileTime = NativePtr.stackalloc<FILETIME> 1
            invokeFunc1 getSystemTimeAsFileTimeImport (NativePtr.toNativeInt fileTime)
            let result = NativePtr.read fileTime
            result.ToInt64()
        
        /// <summary>
        /// Gets the current value of the high-resolution performance counter
        /// </summary>
        let queryPerformanceCounter() =
            let counter = NativePtr.stackalloc<int64> 1
            let success = invokeFunc1 queryPerformanceCounterImport (NativePtr.toNativeInt counter)
            if success then NativePtr.read counter
            else failwith "QueryPerformanceCounter failed"
        
        /// <summary>
        /// Gets the frequency of the high-resolution performance counter
        /// </summary>
        let queryPerformanceFrequency() =
            let frequency = NativePtr.stackalloc<int64> 1
            let success = invokeFunc1 queryPerformanceFrequencyImport (NativePtr.toNativeInt frequency)
            if success then NativePtr.read frequency
            else failwith "QueryPerformanceFrequency failed"
        
        /// <summary>
        /// Suspends the execution of the current thread for the specified time
        /// </summary>
        let sleep(milliseconds: uint32) =
            invokeFunc1 sleepImport milliseconds
            
        /// <summary>
        /// Gets the current time zone information
        /// </summary>
        let getTimeZoneInformation() =
            let tzi = NativePtr.stackalloc<TIME_ZONE_INFORMATION> 1
            let result = invokeFunc1 getTimeZoneInformationImport (NativePtr.toNativeInt tzi)
            let timeZoneInfo = NativePtr.read tzi
            // Convert uint32 result to TimeZoneId enum, handle the conversion properly
            let timeZoneId = 
                if result = 0u then TimeZoneId.Unknown
                elif result = 1u then TimeZoneId.Standard  
                elif result = 2u then TimeZoneId.Daylight
                else TimeZoneId.Invalid
            (timeZoneId, timeZoneInfo)

    /// <summary>
    /// Helper functions to convert between time formats
    /// </summary>
    module TimeConversion =
        let private windowsToNetTicksOffset = 504911232000000000L
        
        /// <summary>
        /// Converts a Windows file time to .NET ticks
        /// </summary>
        let fileTimeToTicks (fileTime: int64) =
            add fileTime windowsToNetTicksOffset
        
        /// <summary>
        /// Converts .NET ticks to a Windows file time
        /// </summary>
        let ticksToFileTime (ticks: int64) =
            subtract ticks windowsToNetTicksOffset
            
        /// <summary>
        /// Gets the current time zone offset in minutes from UTC
        /// Simple approach: get base bias, adjust for daylight time if needed
        /// </summary>
        let getCurrentTimeZoneOffsetMinutes() : int =
            let (timeZoneId, tzi) = Kernel32.getTimeZoneInformation()
            
            match timeZoneId with
            | TimeZoneId.Daylight ->
                // In daylight time: subtract 1 hour (60 minutes) from base bias
                tzi.Bias - 60
            | _ ->
                // Standard time or unknown: use base bias
                tzi.Bias

    /// <summary>
    /// Windows platform implementation of IPlatformTime
    /// </summary>
    type WindowsTimeImplementation() =
        interface IPlatformTime with
            /// <summary>
            /// Gets the current time in ticks (100-nanosecond intervals since January 1, 0001)
            /// </summary>
            member _.GetCurrentTicks() =
                let fileTime = Kernel32.getSystemTimeAsFileTime()
                TimeConversion.fileTimeToTicks fileTime
            
            /// <summary>
            /// Gets the current UTC time in ticks
            /// </summary>
            member _.GetUtcNow() =
                let fileTime = Kernel32.getSystemTimeAsFileTime()
                TimeConversion.fileTimeToTicks fileTime
            
            /// <summary>
            /// Gets the system time as file time (100-nanosecond intervals since January 1, 1601)
            /// </summary>
            member _.GetSystemTimeAsFileTime() =
                Kernel32.getSystemTimeAsFileTime()
            
            /// <summary>
            /// Gets high-resolution performance counter ticks
            /// </summary>
            member _.GetHighResolutionTicks() =
                Kernel32.queryPerformanceCounter()
            
            /// <summary>
            /// Gets the frequency of the high-resolution performance counter
            /// </summary>
            member _.GetTickFrequency() =
                Kernel32.queryPerformanceFrequency()
            
            /// <summary>
            /// Sleeps for the specified number of milliseconds
            /// </summary>
            member _.Sleep(milliseconds) =
                Kernel32.sleep(uint32 milliseconds)
                
            /// <summary>
            /// Gets the current timezone offset from UTC in minutes
            /// </summary>
            member _.GetTimezoneOffsetMinutes() =
                TimeConversion.getCurrentTimeZoneOffsetMinutes()

    /// <summary>
    /// Factory function to create a Windows time implementation
    /// </summary>
    let createImplementation() =
        WindowsTimeImplementation() :> IPlatformTime
        
    /// <summary>
    /// Gets the current local time zone offset from UTC in minutes
    /// Returns positive values for west of UTC, negative for east of UTC
    /// </summary>
    let getCurrentTimeZoneOffsetMinutes() : int =
        TimeConversion.getCurrentTimeZoneOffsetMinutes()