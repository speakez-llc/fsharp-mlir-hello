# Define paths
$projectRoot = "D:\repos\fsharp-mlir-hello"
$buildDir = "$projectRoot\build"

# Path to F# compiler - this should be the Visual Studio F# compiler
# This is the typical location for the F# compiler with Visual Studio
$fscPath = "C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\CommonExtensions\Microsoft\FSharp\Tools\fsc.exe"

# Check if the F# compiler exists at the specified path
if (-not (Test-Path -Path $fscPath)) {
    Write-Error "F# compiler not found at: $fscPath"
    Write-Host "Looking for fsc.exe in other locations..."
    
    # Try to find fsc.exe in the system PATH
    $fscInPath = Get-Command fsc.exe -ErrorAction SilentlyContinue
    if ($fscInPath) {
        $fscPath = $fscInPath.Source
        Write-Host "Found F# compiler at: $fscPath"
    } else {
        # Try other common locations
        $possibleLocations = @(
            "C:\Program Files\dotnet\sdk\*\FSharp\fsc.exe",
            "C:\Program Files (x86)\Microsoft Visual Studio\*\*\Common7\IDE\CommonExtensions\Microsoft\FSharp\fsc.exe"
        )
        
        foreach ($location in $possibleLocations) {
            $resolvedPaths = Resolve-Path -Path $location -ErrorAction SilentlyContinue
            if ($resolvedPaths) {
                $fscPath = $resolvedPaths[-1].Path  # Take the latest version
                Write-Host "Found F# compiler at: $fscPath"
                break
            }
        }
    }
    
    if (-not (Test-Path -Path $fscPath)) {
        Write-Error "Could not find F# compiler (fsc.exe) on your system."
        Write-Error "Please install the F# compiler or provide the correct path to fsc.exe."
        exit 1
    }
}

# Path to our custom FSharpCompilerService.dll
$fscsPath = "$projectRoot\FSharpCompilerService\bin\Debug\net6.0\FSharp.Compiler.Service.dll"

# Create build directory if it doesn't exist
if (-not (Test-Path -Path $buildDir)) {
    New-Item -ItemType Directory -Path $buildDir
}

# Function to run the F# compiler (fsc.exe)
function Invoke-FSC {
    & $fscPath $args
    if (-not $?) {
        Write-Error "F# compilation failed. See errors above."
        exit 1
    }
}

# Compile F# bindings library
Write-Host "Compiling F# bindings library..."
Invoke-FSC --out:$buildDir\FSharpMLIR.dll --target:library $projectRoot\src\Bindings\MLIRBindings.fs

# Compile the wrapper
Write-Host "Compiling wrapper module..."
Invoke-FSC --out:$buildDir\FSharpMLIRWrapper.dll --target:library --reference:$buildDir\FSharpMLIR.dll $projectRoot\src\Bindings\MLIRWrapper.fs

# Compile conversion modules with reference to our custom library
Write-Host "Compiling conversion modules..."
Invoke-FSC --out:$buildDir\FSharpMLIRConversion.dll --target:library --reference:$buildDir\FSharpMLIR.dll --reference:$buildDir\FSharpMLIRWrapper.dll --reference:$fscsPath $projectRoot\src\Conversion\ASTToMLIR.fs

# Compile the pipeline
Write-Host "Compiling pipeline modules..."
Invoke-FSC --out:$buildDir\FSharpMLIRPipeline.dll --target:library --reference:$buildDir\FSharpMLIR.dll --reference:$buildDir\FSharpMLIRWrapper.dll --reference:$buildDir\FSharpMLIRConversion.dll --reference:$fscsPath $projectRoot\src\Pipeline\Compiler.fs

Write-Host "Building TimeLoop example..." -ForegroundColor Green

# Compile the new example
& fsc.exe `
    --target:exe `
    --out:build/TimeLoop.exe `
    src/Examples/TimeLoop.fs `
    --reference:FSharp.Core.dll

# Run the F# to MLIR compiler on the new example
& dotnet run --project src/Pipeline/Pipeline.fsproj -- `
    --input src/Examples/TimeLoop.fs `
    --output build/timeloop.mlir

# Convert MLIR to LLVM IR
& mlir-opt `
    --convert-func-to-llvm `
    build/timeloop.mlir | `
    mlir-translate --mlir-to-llvmir > build/timeloop.ll

# Compile to native executable
& clang `
    build/timeloop.ll `
    -o build/timeloop.exe

Write-Host "TimeLoop example built successfully!" -ForegroundColor Green
Write-Host "Run with: ./build/timeloop.exe" -ForegroundColor Yellow