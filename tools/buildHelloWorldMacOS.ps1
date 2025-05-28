#!/usr/bin/env pwsh
# Cross-Platform Build script for macOS - HelloWorld example

# Define paths - don't use $env:HOME as it might not be properly set in some PowerShell environments
$homeDir = if ($IsMacOS) { [System.Environment]::GetFolderPath("UserProfile") } else { $HOME }
$projectRoot = Join-Path $homeDir "repos/fsharp-mlir-hello"
$buildDir = Join-Path $projectRoot "build"

# Ensure the build directory exists
if (-not (Test-Path -Path $buildDir)) {
    New-Item -ItemType Directory -Path $buildDir -Force
}

# Detect .NET SDK version
$dotnetSdk = $null
try {
    $dotnetVersion = dotnet --version
    Write-Host "Detected .NET SDK version: $dotnetVersion"
    $dotnetSdk = $dotnetVersion
} catch {
    Write-Error "Unable to detect .NET SDK version. Make sure .NET SDK is installed."
    exit 1
}

# Dynamically determine the F# compiler path
$fscPath = $null
$possiblePaths = @(
    "/usr/local/share/dotnet/sdk/$dotnetSdk/FSharp/fsc.dll",
    "/opt/homebrew/share/dotnet/sdk/$dotnetSdk/FSharp/fsc.dll",
    "/usr/local/share/dotnet/sdk/*/FSharp/fsc.dll",
    "/opt/homebrew/share/dotnet/sdk/*/FSharp/fsc.dll"
)

foreach ($path in $possiblePaths) {
    $resolvedPaths = Resolve-Path -Path $path -ErrorAction SilentlyContinue
    if ($resolvedPaths) {
        $fscPath = $resolvedPaths[0].Path
        Write-Host "Found F# compiler at: $fscPath"
        break
    }
}

if (-not $fscPath) {
    Write-Error "Could not find F# compiler. Please ensure .NET SDK is installed."
    exit 1
}

# Function to run the F# compiler (fsc.dll)
function Invoke-FSC {
    param (
        [Parameter(Mandatory=$true)]
        [string[]]$Arguments
    )
    
    # Prepend fixed arguments
    $allArgs = @("$fscPath", "--nologo", "--targetprofile:netcore", "--define:MACOS") + $Arguments
    
    Write-Verbose "Executing: dotnet $allArgs"
    & "dotnet" $allArgs
    
    if (-not $?) {
        Write-Error "F# compilation failed. See errors above."
        exit 1
    }
}

# Path to our custom FSharpCompilerService.dll
$fscsPath = Join-Path $projectRoot "FSharpCompilerService/bin/Debug/net8.0/FSharp.Compiler.Service.dll"
if (-not (Test-Path -Path $fscsPath)) {
    Write-Warning "FSharp.Compiler.Service.dll not found at $fscsPath, checking for alternative paths..."
    
    # Try fallback to net6.0 path
    $fscsPath = Join-Path $projectRoot "FSharpCompilerService/bin/Debug/net6.0/FSharp.Compiler.Service.dll"
    if (-not (Test-Path -Path $fscsPath)) {
        Write-Error "Could not find FSharp.Compiler.Service.dll. Please build the FSharpCompilerService project first."
        exit 1
    }
}

Write-Host "Using FSharp.Compiler.Service from: $fscsPath"

# Ensure the PlatformUtils module is compiled first since other modules depend on it
Write-Host "Compiling platform utilities..."
Invoke-FSC @("--out:$buildDir/FSharpMLIR.PlatformUtils.dll", "--target:library", "$projectRoot/src/Bindings/PlatformUtils.fs")

# Compile LLVM Bindings
Write-Host "Compiling LLVM bindings..."
Invoke-FSC @("--out:$buildDir/FSharpMLIR.Bindings.LLVM.dll", "--target:library", "--reference:$buildDir/FSharpMLIR.PlatformUtils.dll", "$projectRoot/src/Bindings/LLVMBindings.fs")

# Compile MLIR Bindings
Write-Host "Compiling MLIR bindings..."
Invoke-FSC @("--out:$buildDir/FSharpMLIR.Bindings.MLIR.dll", "--target:library", "--reference:$buildDir/FSharpMLIR.PlatformUtils.dll", "$projectRoot/src/Bindings/MLIRBindings.fs")

# Compile MLIR Wrapper
Write-Host "Compiling MLIR wrapper..."
Invoke-FSC @("--out:$buildDir/FSharpMLIR.Bindings.MLIRWrapper.dll", "--target:library", "--reference:$buildDir/FSharpMLIR.PlatformUtils.dll", "--reference:$buildDir/FSharpMLIR.Bindings.MLIR.dll", "$projectRoot/src/Bindings/MLIRWrapper.fs")

# Compile AST to MLIR conversion module
Write-Host "Compiling AST to MLIR conversion..."
Invoke-FSC @("--out:$buildDir/FSharpMLIR.Conversion.dll", "--target:library", "--reference:$buildDir/FSharpMLIR.PlatformUtils.dll", "--reference:$buildDir/FSharpMLIR.Bindings.MLIR.dll", "--reference:$buildDir/FSharpMLIR.Bindings.MLIRWrapper.dll", "--reference:$fscsPath", "$projectRoot/src/Conversion/ASTToMLIR.fs")

# Compile the compiler pipeline
Write-Host "Compiling compiler pipeline..."
Invoke-FSC @("--out:$buildDir/FSharpMLIR.Pipeline.dll", "--target:library", "--reference:$buildDir/FSharpMLIR.PlatformUtils.dll", "--reference:$buildDir/FSharpMLIR.Bindings.LLVM.dll", "--reference:$buildDir/FSharpMLIR.Bindings.MLIR.dll", "--reference:$buildDir/FSharpMLIR.Bindings.MLIRWrapper.dll", "--reference:$buildDir/FSharpMLIR.Conversion.dll", "--reference:$fscsPath", "$projectRoot/src/Pipeline/Compiler.fs")

# Compile the HelloWorld example
Write-Host "Compiling HelloWorld example..."
Invoke-FSC @("--out:$buildDir/HelloWorld", "--target:exe", "--reference:$buildDir/FSharpMLIR.PlatformUtils.dll", "--reference:$buildDir/FSharpMLIR.Bindings.LLVM.dll", "--reference:$buildDir/FSharpMLIR.Bindings.MLIR.dll", "--reference:$buildDir/FSharpMLIR.Bindings.MLIRWrapper.dll", "--reference:$buildDir/FSharpMLIR.Conversion.dll", "--reference:$buildDir/FSharpMLIR.Pipeline.dll", "$projectRoot/src/Examples/HelloWorld.fs")

# Set executable permissions for the output
if (Test-Path "$buildDir/HelloWorld") {
    & chmod +x "$buildDir/HelloWorld"
    if ($?) {
        Write-Host "Set executable permissions on $buildDir/HelloWorld"
    } else {
        Write-Warning "Failed to set executable permissions on $buildDir/HelloWorld"
    }
}

Write-Host "Build completed successfully."
Write-Host "Run the program with: $buildDir/HelloWorld"