# Cross-Platform Build script for HelloWorld example

# Set paths based on platform
if ($IsMacOS) {
    $projectRoot = "$env:HOME/repos/fsharp-mlir-hello"
    $fscPath = "fsc"  # Our wrapper
} else {
    $projectRoot = "D:\repos\fsharp-mlir-hello"
    $fscPath = "C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\CommonExtensions\Microsoft\FSharp\Tools\fsc.exe"
}

$buildDir = Join-Path $projectRoot "build"
$fscsPath = Join-Path $projectRoot "FSharpCompilerService/bin/Debug/net6.0/FSharp.Compiler.Service.dll"

# Create build directory
New-Item -ItemType Directory -Path $buildDir -Force -ErrorAction SilentlyContinue

# Function to run F# compiler
function Invoke-FSC {
    & $fscPath $args
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Build failed"
        exit $LASTEXITCODE
    }
}

# Source paths
$mlirBindingsPath = Join-Path $projectRoot "src/Bindings/MLIRBindings.fs"
$mlirWrapperPath = Join-Path $projectRoot "src/Bindings/MLIRWrapper.fs"
$astToMLIRPath = Join-Path $projectRoot "src/Conversion/ASTToMLIR.fs"
$compilerPath = Join-Path $projectRoot "src/Pipeline/Compiler.fs"
$helloWorldPath = Join-Path $projectRoot "src/Examples/HelloWorld.fs"

# Build output paths
$mlirDll = Join-Path $buildDir "FSharpMLIR.dll"
$wrapperDll = Join-Path $buildDir "FSharpMLIRWrapper.dll"
$conversionDll = Join-Path $buildDir "FSharpMLIRConversion.dll"
$pipelineDll = Join-Path $buildDir "FSharpMLIRPipeline.dll"
$helloWorldExe = Join-Path $buildDir "HelloWorld.exe"

# Build steps
Write-Host "Building F# MLIR bindings..."
Invoke-FSC --out:$mlirDll --target:library $mlirBindingsPath

Write-Host "Building MLIR wrapper..."
Invoke-FSC --out:$wrapperDll --target:library --reference:$mlirDll $mlirWrapperPath

Write-Host "Building AST conversion..."
Invoke-FSC --out:$conversionDll --target:library --reference:$mlirDll --reference:$wrapperDll --reference:$fscsPath $astToMLIRPath

Write-Host "Building compiler pipeline..."
Invoke-FSC --out:$pipelineDll --target:library --reference:$mlirDll --reference:$wrapperDll --reference:$conversionDll --reference:$fscsPath $compilerPath

Write-Host "Building HelloWorld example..."
Invoke-FSC --out:$helloWorldExe --reference:$mlirDll --reference:$wrapperDll --reference:$conversionDll --reference:$pipelineDll $helloWorldPath

Write-Host "Build completed successfully."