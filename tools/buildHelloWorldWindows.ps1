# Define paths
$projectRoot = "D:\repos\fsharp-mlir-hello"
$buildDir = "$projectRoot\build"

# Path to F# compiler - this should be the Visual Studio F# compiler
# This is the typical location for the F# compiler with Visual Studio
$fscPath = "C:\Program Files\dotnet\sdk\6.0.428\FSharp\fsc.dll"

# Function to run the F# compiler (fsc.dll)
function Invoke-FSC {
    & "dotnet" $fscPath $args
    if (-not $?) {
        Write-Error "F# compilation failed. See errors above."
        exit 1
    }
}

# Path to our custom FSharpCompilerService.dll
$fscsPath = "$projectRoot\FSharpCompilerService\bin\Debug\net6.0\FSharp.Compiler.Service.dll"

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

# Compile the example program
Write-Host "Compiling example program..."
Invoke-FSC --out:$buildDir\HelloWorld.exe --reference:$buildDir\FSharpMLIR.dll --reference:$buildDir\FSharpMLIRWrapper.dll --reference:$buildDir\FSharpMLIRConversion.dll --reference:$buildDir\FSharpMLIRPipeline.dll $projectRoot\src\Examples\HelloWorld.fs

Write-Host "Build completed successfully."