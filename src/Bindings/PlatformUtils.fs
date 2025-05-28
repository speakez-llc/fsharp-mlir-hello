namespace FSharpMLIR

/// <summary>
/// Utilities for handling platform-specific operations
/// </summary>
module PlatformUtils =
    open System
    open System.Runtime.InteropServices
    
    /// <summary>
    /// Operating system types
    /// </summary>
    type PlatformOS = 
        | Windows 
        | MacOS 
        | Linux 
        | Unknown
    
    /// <summary>
    /// CPU architecture types
    /// </summary>
    type PlatformArch = 
        | X86_64 
        | Arm64 
        | X86 
        | Unknown
        
    /// <summary>
    /// Determines the current operating system
    /// </summary>
    let getOS() =
        if RuntimeInformation.IsOSPlatform(OSPlatform.Windows) then PlatformOS.Windows
        elif RuntimeInformation.IsOSPlatform(OSPlatform.OSX) then PlatformOS.MacOS
        elif RuntimeInformation.IsOSPlatform(OSPlatform.Linux) then PlatformOS.Linux
        else PlatformOS.Unknown
        
    /// <summary>
    /// Determines the current CPU architecture
    /// </summary>
    let getArchitecture() =
        match RuntimeInformation.ProcessArchitecture with
        | Architecture.X64 -> PlatformArch.X86_64
        | Architecture.Arm64 -> PlatformArch.Arm64
        | Architecture.X86 -> PlatformArch.X86
        | _ -> PlatformArch.Unknown
    
    /// <summary>
    /// Gets the platform-specific name for a native library
    /// </summary>
    let getNativeLibraryName (baseName: string) =
        match getOS() with
        | PlatformOS.Windows -> $"{baseName}.dll"
        | PlatformOS.MacOS -> $"lib{baseName}.dylib"
        | PlatformOS.Linux -> $"lib{baseName}.so"
        | PlatformOS.Unknown -> 
            // Default fallback strategy
            if Environment.OSVersion.Platform = PlatformID.Win32NT then
                $"{baseName}.dll"
            else
                $"lib{baseName}.so"
    
    /// <summary>
    /// Gets the platform-specific search paths for native libraries
    /// </summary>
    let getNativeLibraryPaths() =
        match getOS() with
        | PlatformOS.Windows -> 
            [
                @"C:\Program Files\LLVM\bin"
                @"C:\Program Files (x86)\LLVM\bin"
                Environment.CurrentDirectory
            ]
        | PlatformOS.MacOS ->
            [
                "/usr/local/opt/llvm/lib"
                "/opt/homebrew/opt/llvm/lib"
                "/usr/local/lib"
                Environment.CurrentDirectory
            ]
        | PlatformOS.Linux ->
            [
                "/usr/lib/llvm-14/lib"
                "/usr/local/lib"
                "/usr/lib"
                Environment.CurrentDirectory
            ]
        | PlatformOS.Unknown ->
            [ Environment.CurrentDirectory ]
    
    /// <summary>
    /// Gets the default LLVM target triple for the current platform
    /// </summary>
    let getDefaultTargetTriple() =
        match getOS(), getArchitecture() with
        | PlatformOS.Windows, PlatformArch.X86_64 -> "x86_64-pc-windows-msvc"
        | PlatformOS.Windows, PlatformArch.X86 -> "i686-pc-windows-msvc"
        | PlatformOS.Windows, PlatformArch.Arm64 -> "aarch64-pc-windows-msvc"
        | PlatformOS.MacOS, PlatformArch.X86_64 -> "x86_64-apple-darwin"
        | PlatformOS.MacOS, PlatformArch.Arm64 -> "arm64-apple-darwin"
        | PlatformOS.Linux, PlatformArch.X86_64 -> "x86_64-unknown-linux-gnu"
        | PlatformOS.Linux, PlatformArch.Arm64 -> "aarch64-unknown-linux-gnu"
        | PlatformOS.Linux, PlatformArch.X86 -> "i686-unknown-linux-gnu"
        | _, _ -> "unknown-unknown-unknown" // Default fallback
    
    /// <summary>
    /// Sets up environment for native library loading
    /// </summary>
    let setupNativeEnvironment() =
        let paths = getNativeLibraryPaths()
        
        // Add paths to PATH environment variable if they exist
        for path in paths do
            if System.IO.Directory.Exists(path) then
                let currentPath = Environment.GetEnvironmentVariable("PATH")
                let newPath = 
                    if String.IsNullOrEmpty(currentPath) then path
                    else $"{path}{System.IO.Path.PathSeparator}{currentPath}"
                Environment.SetEnvironmentVariable("PATH", newPath)
                
        // On macOS and Linux, may also need to set DYLD_LIBRARY_PATH or LD_LIBRARY_PATH
        match getOS() with
        | PlatformOS.MacOS ->
            let dyldPath = Environment.GetEnvironmentVariable("DYLD_LIBRARY_PATH")
            let newDyldPath = 
                paths 
                |> List.filter System.IO.Directory.Exists
                |> String.concat (string System.IO.Path.PathSeparator)
            
            let finalDyldPath =
                if String.IsNullOrEmpty(dyldPath) then newDyldPath
                else $"{newDyldPath}{System.IO.Path.PathSeparator}{dyldPath}"
                
            Environment.SetEnvironmentVariable("DYLD_LIBRARY_PATH", finalDyldPath)
            
        | PlatformOS.Linux ->
            let ldPath = Environment.GetEnvironmentVariable("LD_LIBRARY_PATH")
            let newLdPath = 
                paths 
                |> List.filter System.IO.Directory.Exists
                |> String.concat (string System.IO.Path.PathSeparator)
            
            let finalLdPath =
                if String.IsNullOrEmpty(ldPath) then newLdPath
                else $"{newLdPath}{System.IO.Path.PathSeparator}{ldPath}"
                
            Environment.SetEnvironmentVariable("LD_LIBRARY_PATH", finalLdPath)
            
        | _ -> () // No special handling for Windows
    
    /// <summary>
    /// Gets the path to a native library, searching in standard locations
    /// </summary>
    let findNativeLibrary (libraryName: string) =
        let fileName = getNativeLibraryName libraryName
        let paths = getNativeLibraryPaths()
        
        let candidates = 
            paths 
            |> List.map (fun path -> System.IO.Path.Combine(path, fileName))
            |> List.filter System.IO.File.Exists
        
        match candidates with
        | head :: _ -> Some head
        | [] -> None