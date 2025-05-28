namespace FSharpMLIR

open System
open System.Runtime.InteropServices

/// Platform utilities for handling platform-specific operations
module PlatformUtils =
    
    /// Operating system types
    type PlatformOS = 
        | Windows 
        | MacOS 
        | Linux 
        | Unknown
    
    /// CPU architecture types
    type PlatformArch = 
        | X86_64 
        | Arm64 
        | X86 
        | Unknown
        
    /// Determines the current operating system
    let getOS() =
        if RuntimeInformation.IsOSPlatform(OSPlatform.Windows) then PlatformOS.Windows
        elif RuntimeInformation.IsOSPlatform(OSPlatform.OSX) then PlatformOS.MacOS
        elif RuntimeInformation.IsOSPlatform(OSPlatform.Linux) then PlatformOS.Linux
        else PlatformOS.Unknown
        
    /// Determines the current CPU architecture
    let getArchitecture() =
        match RuntimeInformation.ProcessArchitecture with
        | Architecture.X64 -> PlatformArch.X86_64
        | Architecture.Arm64 -> PlatformArch.Arm64
        | Architecture.X86 -> PlatformArch.X86
        | _ -> PlatformArch.Unknown
    
    /// Gets the platform-specific name for a native library
    let getNativeLibraryName (baseName: string) =
        match getOS() with
        | PlatformOS.Windows -> sprintf "%s.dll" baseName
        | PlatformOS.MacOS -> sprintf "lib%s.dylib" baseName
        | PlatformOS.Linux -> sprintf "lib%s.so" baseName
        | PlatformOS.Unknown -> 
            if Environment.OSVersion.Platform = PlatformID.Win32NT then
                sprintf "%s.dll" baseName
            else
                sprintf "lib%s.so" baseName
    
    /// Gets the platform-specific search paths for native libraries
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
                "/usr/local/lib"
                "/opt/homebrew/lib"
                "/opt/homebrew/opt/llvm/lib"
                "/usr/local/opt/llvm/lib"
                Environment.CurrentDirectory
            ]
        | PlatformOS.Linux ->
            [
                "/usr/lib/llvm-14/lib"
                "/usr/lib/llvm-15/lib"
                "/usr/lib/llvm-16/lib"
                "/usr/lib/llvm-17/lib"
                "/usr/lib/llvm-18/lib"
                "/usr/local/lib"
                "/usr/lib"
                Environment.CurrentDirectory
            ]
        | PlatformOS.Unknown ->
            [ Environment.CurrentDirectory ]
    
    /// Platform-specific library loading
    module private NativeLibraryLoader =
        [<DllImport("kernel32.dll", SetLastError = true)>]
        extern IntPtr LoadLibrary(string lpFileName)
        
        [<DllImport("kernel32.dll", SetLastError = true)>]
        extern bool FreeLibrary(IntPtr hModule)
        
        [<DllImport("libdl.dylib")>]
        extern IntPtr dlopen(string filename, int flags)
        
        [<DllImport("libdl.dylib")>]
        extern int dlclose(IntPtr handle)
        
        [<DllImport("libdl.dylib")>]
        extern IntPtr dlerror()
        
        let RTLD_LAZY = 0x00001
        let RTLD_NOW = 0x00002
        let RTLD_GLOBAL = 0x00100
        
        /// Load a native library with platform-specific method
        let loadLibrary (path: string) =
            match getOS() with
            | PlatformOS.Windows ->
                LoadLibrary(path)
            | PlatformOS.MacOS | PlatformOS.Linux ->
                // Use RTLD_NOW | RTLD_GLOBAL to ensure symbols are available globally
                dlopen(path, RTLD_NOW ||| RTLD_GLOBAL)
            | _ ->
                IntPtr.Zero
        
        /// Get the last error message for library loading
        let getLastError() =
            match getOS() with
            | PlatformOS.Windows ->
                Marshal.GetLastWin32Error().ToString()
            | PlatformOS.MacOS | PlatformOS.Linux ->
                let errorPtr = dlerror()
                if errorPtr <> IntPtr.Zero then
                    Marshal.PtrToStringAnsi(errorPtr)
                else
                    "Unknown error"
            | _ ->
                "Platform not supported"
    
    /// Preload native libraries to resolve dependencies
    let private preloadNativeLibraries() =
        match getOS() with
        | PlatformOS.MacOS ->
            // On macOS, we need to load LLVM first due to rpath dependencies
            let llvmPaths = [
                "/usr/local/lib/libLLVM.dylib"
                "/opt/homebrew/opt/llvm/lib/libLLVM.dylib"
            ]
            
            let mutable llvmLoaded = false
            for path in llvmPaths do
                if System.IO.File.Exists(path) && not llvmLoaded then
                    let handle = NativeLibraryLoader.loadLibrary(path)
                    if handle <> IntPtr.Zero then
                        printfn "Successfully preloaded LLVM from: %s" path
                        llvmLoaded <- true
                    else
                        let error = NativeLibraryLoader.getLastError()
                        printfn "Failed to preload LLVM from %s: %s" path error
            
            if not llvmLoaded then
                printfn "Warning: Could not preload LLVM library"
        | _ -> ()
    
    /// Sets up environment for native library loading
    let setupNativeEnvironment() =
        try
            // First, preload dependencies
            preloadNativeLibraries()
            
            // Then set up paths
            let paths = getNativeLibraryPaths()
            
            // Add paths to PATH environment variable if they exist
            for path in paths do
                if System.IO.Directory.Exists(path) then
                    let currentPath = Environment.GetEnvironmentVariable("PATH")
                    let pathSeparator = if getOS() = PlatformOS.Windows then ";" else ":"
                    let newPath = 
                        if String.IsNullOrEmpty(currentPath) then path
                        else sprintf "%s%s%s" path pathSeparator currentPath
                    Environment.SetEnvironmentVariable("PATH", newPath)
                    
            // On macOS and Linux, also set library path environment variables
            match getOS() with
            | PlatformOS.MacOS ->
                let existingPaths = paths |> List.filter System.IO.Directory.Exists
                
                if not existingPaths.IsEmpty then
                    let pathSeparator = ":"
                    let newPath = existingPaths |> String.concat pathSeparator
                    
                    // Set DYLD_LIBRARY_PATH
                    let dyldPath = Environment.GetEnvironmentVariable("DYLD_LIBRARY_PATH")
                    let finalDyldPath =
                        if String.IsNullOrEmpty(dyldPath) then newPath
                        else sprintf "%s%s%s" newPath pathSeparator dyldPath
                    Environment.SetEnvironmentVariable("DYLD_LIBRARY_PATH", finalDyldPath)
                    
                    // Also set DYLD_FALLBACK_LIBRARY_PATH
                    let dyldFallbackPath = Environment.GetEnvironmentVariable("DYLD_FALLBACK_LIBRARY_PATH")
                    let finalFallbackPath =
                        if String.IsNullOrEmpty(dyldFallbackPath) then newPath
                        else sprintf "%s%s%s" newPath pathSeparator dyldFallbackPath
                    Environment.SetEnvironmentVariable("DYLD_FALLBACK_LIBRARY_PATH", finalFallbackPath)
            
            | PlatformOS.Linux ->
                let existingPaths = paths |> List.filter System.IO.Directory.Exists
                
                if not existingPaths.IsEmpty then
                    let pathSeparator = ":"
                    let newPath = existingPaths |> String.concat pathSeparator
                    
                    let ldPath = Environment.GetEnvironmentVariable("LD_LIBRARY_PATH")
                    let finalLdPath =
                        if String.IsNullOrEmpty(ldPath) then newPath
                        else sprintf "%s%s%s" newPath pathSeparator ldPath
                    Environment.SetEnvironmentVariable("LD_LIBRARY_PATH", finalLdPath)
            
            | _ -> ()
        with ex ->
            printfn "Warning: Error setting up native environment: %s" ex.Message
            printfn "Continuing anyway..."
    
    /// Gets the default LLVM target triple for the current platform
    let getDefaultTargetTripleFallback() =
        match getOS(), getArchitecture() with
        | PlatformOS.Windows, PlatformArch.X86_64 -> "x86_64-pc-windows-msvc"
        | PlatformOS.Windows, PlatformArch.X86 -> "i686-pc-windows-msvc"
        | PlatformOS.Windows, PlatformArch.Arm64 -> "aarch64-pc-windows-msvc"
        | PlatformOS.MacOS, PlatformArch.X86_64 -> "x86_64-apple-darwin"
        | PlatformOS.MacOS, PlatformArch.Arm64 -> "arm64-apple-darwin"
        | PlatformOS.Linux, PlatformArch.X86_64 -> "x86_64-unknown-linux-gnu"
        | PlatformOS.Linux, PlatformArch.Arm64 -> "aarch64-unknown-linux-gnu"
        | PlatformOS.Linux, PlatformArch.X86 -> "i686-unknown-linux-gnu"
        | _, _ -> "unknown-unknown-unknown"
    
    /// Gets the default LLVM target triple
    let getDefaultTargetTriple() =
        getDefaultTargetTripleFallback()
    
    /// Gets the path to a native library, searching in standard locations
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
    
    /// Checks if the required native libraries are available
    let checkNativeLibrariesAvailable() =
        let llvmAvailable = findNativeLibrary "LLVM" |> Option.isSome
        let mlirAvailable = findNativeLibrary "MLIR" |> Option.isSome
        
        (llvmAvailable, mlirAvailable)
    
    /// Gets diagnostic information about the current platform and library availability
    let getPlatformDiagnostics() =
        let os = getOS()
        let arch = getArchitecture()
        let targetTriple = getDefaultTargetTriple()
        let (llvmAvailable, mlirAvailable) = checkNativeLibrariesAvailable()
        let libraryPaths = getNativeLibraryPaths()
        
        [
            sprintf "Operating System: %A" os
            sprintf "Architecture: %A" arch
            sprintf "Target Triple: %s" targetTriple
            sprintf "LLVM Available: %b" llvmAvailable
            sprintf "MLIR Available: %b" mlirAvailable
            "Library Search Paths:"
        ] @
        (libraryPaths |> List.mapi (fun i path -> 
            let exists = System.IO.Directory.Exists(path)
            sprintf "  %d. %s (exists: %b)" (i+1) path exists))
    
    /// Prints platform diagnostics to the console
    let printPlatformDiagnostics() =
        getPlatformDiagnostics()
        |> List.iter (printfn "%s")