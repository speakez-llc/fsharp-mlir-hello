namespace Alloy.Time

/// <summary>
/// Platform interface definitions for Alloy time functions
/// </summary>
module Platform =
    /// <summary>
    /// Interface for platform-specific time operations
    /// </summary>
    type IPlatformTime =
        /// <summary>
        /// Gets the current time in ticks (100-nanosecond intervals since January 1, 0001)
        /// </summary>
        abstract member GetCurrentTicks: unit -> int64
        
        /// <summary>
        /// Gets the current UTC time in ticks
        /// </summary>
        abstract member GetUtcNow: unit -> int64
        
        /// <summary>
        /// Gets the system time as file time (100-nanosecond intervals since January 1, 1601)
        /// </summary>
        abstract member GetSystemTimeAsFileTime: unit -> int64
        
        /// <summary>
        /// Gets high-resolution performance counter ticks
        /// </summary>
        abstract member GetHighResolutionTicks: unit -> int64
        
        /// <summary>
        /// Gets the frequency of the high-resolution performance counter
        /// </summary>
        abstract member GetTickFrequency: unit -> int64
        
        /// <summary>
        /// Sleeps for the specified number of milliseconds
        /// </summary>
        abstract member Sleep: milliseconds:int -> unit

    /// <summary>
    /// Exception thrown when platform implementation cannot be determined
    /// </summary>
    exception PlatformNotSupportedException of string

    /// <summary>
    /// Simple portable time implementation as a fallback
    /// </summary>
    type private SimplePortableTimeImplementation() =
        let mutable tickCounter = 0L
        let ticksPerSecond = 10000000L // 100-nanosecond intervals
        // Correct ticks for May 26, 2025 (approximately)
        let startTime = 638513280000000000L // May 26, 2025
        
        interface IPlatformTime with
            member _.GetCurrentTicks() =
                tickCounter <- tickCounter + ticksPerSecond
                startTime + tickCounter
            
            member _.GetUtcNow() =
                tickCounter <- tickCounter + ticksPerSecond
                startTime + tickCounter
            
            member _.GetSystemTimeAsFileTime() =
                let currentTicks = startTime + tickCounter
                tickCounter <- tickCounter + ticksPerSecond
                // Convert from .NET epoch to Windows file time epoch
                currentTicks - 504911232000000000L
            
            member _.GetHighResolutionTicks() =
                tickCounter <- tickCounter + 1L
                tickCounter
            
            member _.GetTickFrequency() =
                ticksPerSecond
            
            member _.Sleep(milliseconds) =
                // Simulate sleep by advancing the tick counter
                let ticksToAdd = int64 milliseconds * (ticksPerSecond / 1000L)
                tickCounter <- tickCounter + ticksToAdd

    /// <summary>
    /// Registry for platform implementations
    /// </summary>
    module private PlatformRegistry =
        let mutable private implementation : IPlatformTime option = None
        
        /// <summary>
        /// Registers a platform implementation
        /// </summary>
        let register (impl: IPlatformTime) =
            implementation <- Some impl
            
        /// <summary>
        /// Gets the registered implementation
        /// </summary>
        let get() = implementation
        
        /// <summary>
        /// Gets or creates a default implementation
        /// </summary>
        let getOrCreateDefault() =
            match implementation with
            | Some impl -> impl
            | None ->
                // Default to portable implementation 
                // (Windows implementation can be registered manually if needed)
                let impl = SimplePortableTimeImplementation() :> IPlatformTime
                implementation <- Some impl
                impl

    /// <summary>
    /// Function to get the appropriate platform implementation
    /// </summary>
    let getImplementation() = PlatformRegistry.getOrCreateDefault()
    
    /// <summary>
    /// Registers a platform implementation
    /// </summary>
    let registerImplementation (impl: IPlatformTime) = PlatformRegistry.register impl