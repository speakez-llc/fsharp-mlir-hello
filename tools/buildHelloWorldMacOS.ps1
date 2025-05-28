# Define paths
$projectRoot = "$env:HOME/repos/fsharp-mlir-hello"
$buildDir = "$projectRoot/build"

# Path to F# compiler - macOS dotnet SDK location
$fscPath = "/usr/local/share/dotnet/sdk/8.0.100/FSharp/fsc.dll"

# Function to run the F# compiler (fsc.dll)
function Invoke-FSC {
    & "dotnet" $fscPath --targetprofile:netcore $args
    if (-not $?) {
        Write-Error "F# compilation failed. See errors above."
        exit 1
    }
}

# Path to our custom FSharpCompilerService.dll
$fscsPath = "$projectRoot/FSharpCompilerService/bin/Debug/net8.0/FSharp.Compiler.Service.dll"

# Compile F# bindings library
Write-Host "Compiling F# bindings library..."
Invoke-FSC --out:$buildDir/FSharpMLIR.dll --target:library $projectRoot/src/Bindings/MLIRBindings.fs

# Compile the wrapper
Write-Host "Compiling wrapper module..."
Invoke-FSC --out:$buildDir/FSharpMLIRWrapper.dll --target:library --reference:$buildDir/FSharpMLIR.dll $projectRoot/src/Bindings/MLIRWrapper.fs

# Compile conversion modules with reference to our custom library
Write-Host "Compiling conversion modules..."
Invoke-FSC --out:$buildDir/FSharpMLIRConversion.dll --target:library --reference:$buildDir/FSharpMLIR.dll --reference:$buildDir/FSharpMLIRWrapper.dll --reference:$fscsPath $projectRoot/src/Conversion/ASTToMLIR.fs

# Compile the pipeline
Write-Host "Compiling pipeline modules..."
Invoke-FSC --out:$buildDir/FSharpMLIRPipeline.dll --target:library --reference:$buildDir/FSharpMLIR.dll --reference:$buildDir/FSharpMLIRWrapper.dll --reference:$buildDir/FSharpMLIRConversion.dll --reference:$fscsPath $projectRoot/src/Pipeline/Compiler.fs

# Compile the example program
Write-Host "Compiling example program..."
Invoke-FSC --out:$buildDir/HelloWorld.exe --reference:$buildDir/FSharpMLIR.dll --reference:$buildDir/FSharpMLIRWrapper.dll --reference:$buildDir/FSharpMLIRConversion.dll --reference:$buildDir/FSharpMLIRPipeline.dll $projectRoot/src/Examples/HelloWorld.fs

Write-Host "Build completed successfully."