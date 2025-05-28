#!/usr/bin/env pwsh
# Cross-Platform Build script for macOS - HelloWorld example

# Define paths
$homeDir = if ($IsMacOS) { [System.Environment]::GetFolderPath("UserProfile") } else { $HOME }
$projectRoot = Join-Path $homeDir "repos/fsharp-mlir-hello"
$buildDir = Join-Path $projectRoot "build"

# Ensure the build directory exists
if (-not (Test-Path -Path $buildDir)) {
    New-Item -ItemType Directory -Path $buildDir -Force
}

# Detect .NET SDK version and find F# compiler
$dotnetVersion = dotnet --version
Write-Host "Detected .NET SDK version: $dotnetVersion"

$fscPath = "/usr/local/share/dotnet/sdk/$dotnetVersion/FSharp/fsc.dll"
if (-not (Test-Path -Path $fscPath)) {
    $fscPath = "/opt/homebrew/share/dotnet/sdk/$dotnetVersion/FSharp/fsc.dll"
}

Write-Host "Found F# compiler at: $fscPath"

# Function to run the F# compiler (fsc.dll)
function Invoke-FSC {
    & "dotnet" $fscPath --nologo --targetprofile:netcore --define:MACOS $args
    if (-not $?) {
        Write-Error "F# compilation failed. See errors above."
        exit 1
    }
}

# Path to our custom FSharpCompilerService.dll
$fscsPath = Join-Path $projectRoot "FSharpCompilerService/bin/Debug/net8.0/FSharp.Compiler.Service.dll"
Write-Host "Using FSharp.Compiler.Service from: $fscsPath"

# Compile platform utilities
Write-Host "Compiling platform utilities..."
Invoke-FSC --out:$buildDir/FSharpMLIR.PlatformUtils.dll --target:library $projectRoot/src/Bindings/PlatformUtils.fs

# Compile F# bindings library
Write-Host "Compiling F# bindings library..."
Invoke-FSC --out:$buildDir/FSharpMLIR.dll --target:library --reference:$buildDir/FSharpMLIR.PlatformUtils.dll $projectRoot/src/Bindings/LLVMBindings.fs $projectRoot/src/Bindings/MLIRBindings.fs

# Compile the wrapper
Write-Host "Compiling wrapper module..."
Invoke-FSC --out:$buildDir/FSharpMLIRWrapper.dll --target:library --reference:$buildDir/FSharpMLIR.PlatformUtils.dll --reference:$buildDir/FSharpMLIR.dll $projectRoot/src/Bindings/MLIRWrapper.fs

# Compile conversion modules
Write-Host "Compiling conversion modules..."
Invoke-FSC --out:$buildDir/FSharpMLIRConversion.dll --target:library --reference:$buildDir/FSharpMLIR.PlatformUtils.dll --reference:$buildDir/FSharpMLIR.dll --reference:$buildDir/FSharpMLIRWrapper.dll --reference:$fscsPath $projectRoot/src/Conversion/ASTToMLIR.fs

# Compile the pipeline
Write-Host "Compiling pipeline modules..."
Invoke-FSC --out:$buildDir/FSharpMLIRPipeline.exe --target:exe --target:library --reference:$buildDir/FSharpMLIR.PlatformUtils.dll --reference:$buildDir/FSharpMLIR.dll --reference:$buildDir/FSharpMLIRWrapper.dll --reference:$buildDir/FSharpMLIRConversion.dll --reference:$fscsPath $projectRoot/src/Pipeline/Compiler.fs

# Use the MLIR/LLVM compilation pipeline to compile HelloWorld.fs to native binary
Write-Host "Compiling HelloWorld.fs through MLIR/LLVM pipeline..."
& "dotnet" "$buildDir/FSharpMLIRPipeline.dll" "$projectRoot/src/Examples/HelloWorld.fs" "-o" "$buildDir/HelloWorld"

Write-Host "Build completed successfully."
