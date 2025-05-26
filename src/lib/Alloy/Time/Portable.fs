namespace Alloy.Time

open Alloy.Time.Platform
open Alloy.Numerics

/// <summary>
/// Portable time implementation using minimal system dependencies
/// </summary>
module Portable =
    /// <summary>
    /// Portable implementation of IPlatformTime that works across platforms
    /// </summary>
    type PortableTimeImplementation() =
        // We'll use a simple counter for now - in production this would use platform APIs
        let mutable tickCounter = 0L
        let ticksPerSecond = 10000000L // 100-nanosecond intervals
        let startTime = 638585088000000000L // Approximate ticks for 2025-01-01
        
        interface IPlatformTime with
            /// <summary>
            /// Gets the current time in ticks (100-nanosecond intervals since January 1, 0001)
            /// </summary>
            member _.GetCurrentTicks() =
                tickCounter <- add tickCounter ticksPerSecond
                add startTime tickCounter
            
            /// <summary>
            /// Gets the current UTC time in ticks
            /// </summary>
            member _.GetUtcNow() =
                tickCounter <- add tickCounter ticksPerSecond
                add startTime tickCounter
            
            /// <summary>
            /// Gets the system time as file time (100-nanosecond intervals since January 1, 1601)
            /// </summary>
            member _.GetSystemTimeAsFileTime() =
                let currentTicks = add startTime tickCounter
                tickCounter <- add tickCounter ticksPerSecond
                // Convert from .NET epoch to Windows file time epoch
                subtract currentTicks 504911232000000000L
            
            /// <summary>
            /// Gets high-resolution performance counter ticks
            /// </summary>
            member _.GetHighResolutionTicks() =
                tickCounter <- add tickCounter 1L
                tickCounter
            
            /// <summary>
            /// Gets the frequency of the high-resolution performance counter
            /// </summary>
            member _.GetTickFrequency() =
                ticksPerSecond
            
            /// <summary>
            /// Sleeps for the specified number of milliseconds
            /// </summary>
            member _.Sleep(milliseconds) =
                // Simulate sleep by advancing the tick counter
                let ticksToAdd = multiply (int64 milliseconds) (ticksPerSecond / 1000L)
                tickCounter <- add tickCounter ticksToAdd

    /// <summary>
    /// Factory function to create a portable time implementation
    /// </summary>
    let createImplementation() =
        PortableTimeImplementation() :> IPlatformTime