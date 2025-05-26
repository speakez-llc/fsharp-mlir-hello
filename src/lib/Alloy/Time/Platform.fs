namespace Alloy.Time

open Alloy.ValueOption

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
        let get() =
            match implementation with
            | Some impl -> impl
            | None -> raise (PlatformNotSupportedException "No time platform implementation registered")
    
    /// <summary>
    /// Function to get the appropriate platform implementation
    /// </summary>
    let getImplementation() = 
        match PlatformRegistry.get() with
        | Some impl -> impl
        | None -> 
            // Auto-detect and register platform implementation
            let impl = 
                #if WINDOWS
                Windows.createImplementation()
                #elif LINUX
                Linux.createImplementation()
                #elif MACOS
                MacOS.createImplementation()
                #else
                Portable.createImplementation()
                #endif
            PlatformRegistry.register impl
            impl
    
    /// <summary>
    /// Registers a platform implementation
    /// </summary>
    let registerImplementation (impl: IPlatformTime) = PlatformRegistry.register impl