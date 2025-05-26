# Build script for TimeLoop example

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
Invoke-FSC --out:$buildDir\FSharpMLIRBindings.dll --target:library $projectRoot\src\Bindings\LLVMBindings.fs $projectRoot\src\Bindings\MLIRBindings.fs

# Compile the wrapper
Write-Host "Compiling wrapper module..."
Invoke-FSC --out:$buildDir\FSharpMLIRWrapper.dll --target:library --reference:$buildDir\FSharpMLIRBindings.dll $projectRoot\src\Bindings\MLIRWrapper.fs

# Compile Alloy base libraries (now completely dependency-free)
Write-Host "Compiling Alloy base libraries..."

# First compile the core Alloy modules (these need to be in dependency order)
Invoke-FSC --out:$buildDir\Alloy.dll --target:library $projectRoot\src\lib\Alloy\ValueOption.fs $projectRoot\src\lib\Alloy\Core.fs $projectRoot\src\lib\Alloy\Numerics.fs $projectRoot\src\lib\Alloy\Operators.fs

# Compile NativeInterop first (no dependencies)
Invoke-FSC --out:$buildDir\AlloyTimeNativeInterop.dll --target:library --reference:$buildDir\Alloy.dll $projectRoot\src\lib\Alloy\Time\NativeInterop.fs

# Compile Platform interface first (defines IPlatformTime)
Invoke-FSC --out:$buildDir\AlloyTimePlatform.dll --target:library --reference:$buildDir\Alloy.dll $projectRoot\src\lib\Alloy\Time\Platform.fs

# Compile all time implementations (can now reference IPlatformTime)
Invoke-FSC --out:$buildDir\AlloyTimeImplementations.dll --target:library --reference:$buildDir\Alloy.dll --reference:$buildDir\AlloyTimePlatform.dll --reference:$buildDir\AlloyTimeNativeInterop.dll $projectRoot\src\lib\Alloy\Time\Portable.fs $projectRoot\src\lib\Alloy\Time\Windows.fs $projectRoot\src\lib\Alloy\Time\Linux.fs

# Compile the main Time module
Invoke-FSC --out:$buildDir\AlloyTime.dll --target:library --reference:$buildDir\Alloy.dll --reference:$buildDir\AlloyTimePlatform.dll --reference:$buildDir\AlloyTimeNativeInterop.dll --reference:$buildDir\AlloyTimeImplementations.dll $projectRoot\src\lib\Alloy\Time.fs

# Compile conversion modules with reference to our custom library
Write-Host "Compiling conversion modules..."
Invoke-FSC --out:$buildDir\FSharpMLIRConversion.dll --target:library --reference:$buildDir\FSharpMLIRBindings.dll --reference:$buildDir\FSharpMLIRWrapper.dll --reference:$fscsPath $projectRoot\src\Conversion\ASTToMLIR.fs

# Compile the pipeline
Write-Host "Compiling pipeline modules..."
Invoke-FSC --out:$buildDir\FSharpMLIRPipeline.dll --target:library --reference:$buildDir\FSharpMLIRBindings.dll --reference:$buildDir\FSharpMLIRWrapper.dll --reference:$buildDir\FSharpMLIRConversion.dll --reference:$fscsPath $projectRoot\src\Pipeline\Compiler.fs

# Compile the TimeLoop example program
Write-Host "Compiling TimeLoop example program..."
Invoke-FSC --out:$buildDir\TimeLoop.exe --reference:$buildDir\Alloy.dll --reference:$buildDir\AlloyTimePlatform.dll --reference:$buildDir\AlloyTimeNativeInterop.dll --reference:$buildDir\AlloyTimeImplementations.dll --reference:$buildDir\AlloyTime.dll --reference:$buildDir\FSharpMLIRBindings.dll --reference:$buildDir\FSharpMLIRWrapper.dll --reference:$buildDir\FSharpMLIRConversion.dll --reference:$buildDir\FSharpMLIRPipeline.dll $projectRoot\src\Examples\TimeLoop.fs

Write-Host "TimeLoop build completed successfully."
Write-Host "Executable created at: $buildDir\TimeLoop.exe"

# Run the example
Write-Host "Running TimeLoop example..."
& "$buildDir\TimeLoop.exe"