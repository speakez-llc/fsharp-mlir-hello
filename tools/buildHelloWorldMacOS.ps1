# Cross-Platform Build script for macOS - HelloWorld example

# Define paths
$projectRoot = "$env:HOME/repos/fsharp-mlir-hello"
$buildDir = "$projectRoot/build"

# Ensure the build directory exists
if (-not (Test-Path -Path $buildDir)) {
    New-Item -ItemType Directory -Path $buildDir -Force
}

# Path to F# compiler - attempt to find the correct location
$dotnetSdk = (dotnet --version)
if ($dotnetSdk) {
    Write-Host "Detected .NET SDK version: $dotnetSdk"
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
    & "dotnet" $fscPath --nologo --targetprofile:netcore --define:MACOS $args
    if (-not $?) {
        Write-Error "F# compilation failed. See errors above."
        exit 1
    }
}

# Path to our custom FSharpCompilerService.dll
$fscsPath = "$projectRoot/FSharpCompilerService/bin/Debug/net8.0/FSharp.Compiler.Service.dll"

# Ensure the PlatformUtils module is compiled first since other modules depend on it
Write-Host "Compiling platform utilities..."
Invoke-FSC --out:$buildDir/FSharpMLIR.PlatformUtils.dll --target:library $projectRoot/src/Bindings/PlatformUtils.fs

# Compile LLVM Bindings
Write-Host "Compiling LLVM bindings..."
Invoke-FSC --out:$buildDir/FSharpMLIR.Bindings.LLVM.dll --target:library --reference:$buildDir/FSharpMLIR.PlatformUtils.dll $projectRoot/src/Bindings/LLVMBindings.fs

# Compile MLIR Bindings
Write-Host "Compiling MLIR bindings..."
Invoke-FSC --out:$buildDir/FSharpMLIR.Bindings.MLIR.dll --target:library --reference:$buildDir/FSharpMLIR.PlatformUtils.dll $projectRoot/src/Bindings/MLIRBindings.fs

# Compile MLIR Wrapper
Write-Host "Compiling MLIR wrapper..."
Invoke-FSC --out:$buildDir/FSharpMLIR.Bindings.MLIRWrapper.dll --target:library --reference:$buildDir/FSharpMLIR.PlatformUtils.dll --reference:$buildDir/FSharpMLIR.Bindings.MLIR.dll $projectRoot/src/Bindings/MLIRWrapper.fs

# Compile AST to MLIR conversion module
Write-Host "Compiling AST to MLIR conversion..."
Invoke-FSC --out:$buildDir/FSharpMLIR.Conversion.dll --target:library --reference:$buildDir/FSharpMLIR.PlatformUtils.dll --reference:$buildDir/FSharpMLIR.Bindings.MLIR.dll --reference:$buildDir/FSharpMLIR.Bindings.MLIRWrapper.dll --reference:$fscsPath $projectRoot/src/Conversion/ASTToMLIR.fs

# Compile the compiler pipeline
Write-Host "Compiling compiler pipeline..."
Invoke-FSC --out:$buildDir/FSharpMLIR.Pipeline.dll --target:library --reference:$buildDir/FSharpMLIR.PlatformUtils.dll --reference:$buildDir/FSharpMLIR.Bindings.LLVM.dll --reference:$buildDir/FSharpMLIR.Bindings.MLIR.dll --reference:$buildDir/FSharpMLIR.Bindings.MLIRWrapper.dll --reference:$buildDir/FSharpMLIR.Conversion.dll --reference:$fscsPath $projectRoot/src/Pipeline/Compiler.fs

# Compile the HelloWorld example
Write-Host "Compiling HelloWorld example..."
Invoke-FSC --out:$buildDir/HelloWorld --target:exe --reference:$buildDir/FSharpMLIR.PlatformUtils.dll --reference:$buildDir/FSharpMLIR.Bindings.LLVM.dll --reference:$buildDir/FSharpMLIR.Bindings.MLIR.dll --reference:$buildDir/FSharpMLIR.Bindings.MLIRWrapper.dll --reference:$buildDir/FSharpMLIR.Conversion.dll --reference:$buildDir/FSharpMLIR.Pipeline.dll $projectRoot/src/Examples/HelloWorld.fs

# Set executable permissions for the output
& chmod +x "$buildDir/HelloWorld"

Write-Host "Build completed successfully."
Write-Host "Run the program with: $buildDir/HelloWorld"