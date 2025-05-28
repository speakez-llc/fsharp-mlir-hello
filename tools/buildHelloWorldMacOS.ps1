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

# Verify FSharp.Compiler.Service exists
if (-not (Test-Path -Path $fscsPath)) {
    Write-Error "FSharp.Compiler.Service not found at: $fscsPath"
    Write-Host "Please build the FSharpCompilerService project first:"
    Write-Host "  cd $projectRoot/FSharpCompilerService"
    Write-Host "  dotnet build"
    exit 1
}

# Check for required native libraries
Write-Host "Checking for required native libraries..."
$mlirLibPath = "/usr/local/lib/libMLIR.dylib"
$llvmLibPath = "/usr/local/lib/libLLVM.dylib"

if (-not (Test-Path -Path $mlirLibPath)) {
    Write-Host "⚠ Warning: libMLIR.dylib not found at $mlirLibPath"
    Write-Host "This may cause the native compilation to fail."
    Write-Host "To install LLVM/MLIR on macOS:"
    Write-Host "  brew install llvm"
    Write-Host "  sudo ln -sf /opt/homebrew/Cellar/llvm/*/lib/libLLVM.dylib /usr/local/lib/"
    Write-Host "  sudo ln -sf /opt/homebrew/Cellar/llvm/*/lib/libMLIR.dylib /usr/local/lib/"
} else {
    Write-Host "✓ Found libMLIR.dylib"
}

if (-not (Test-Path -Path $llvmLibPath)) {
    Write-Host "⚠ Warning: libLLVM.dylib not found at $llvmLibPath"
    Write-Host "This may cause the native compilation to fail."
} else {
    Write-Host "✓ Found libLLVM.dylib"
}

# Compile platform utilities
Write-Host "Compiling platform utilities..."
Invoke-FSC --out:"$buildDir/FSharpMLIR.PlatformUtils.dll" --target:library "$projectRoot/src/Bindings/PlatformUtils.fs"

# Compile F# bindings library
Write-Host "Compiling F# bindings library..."
Invoke-FSC --out:"$buildDir/FSharpMLIR.dll" --target:library --reference:"$buildDir/FSharpMLIR.PlatformUtils.dll" "$projectRoot/src/Bindings/LLVMBindings.fs" "$projectRoot/src/Bindings/MLIRBindings.fs"

# Compile the wrapper
Write-Host "Compiling wrapper module..."
Invoke-FSC --out:"$buildDir/FSharpMLIRWrapper.dll" --target:library --reference:"$buildDir/FSharpMLIR.PlatformUtils.dll" --reference:"$buildDir/FSharpMLIR.dll" "$projectRoot/src/Bindings/MLIRWrapper.fs"

# Compile conversion modules
Write-Host "Compiling conversion modules..."
Invoke-FSC --out:"$buildDir/FSharpMLIRConversion.dll" --target:library --reference:"$buildDir/FSharpMLIR.PlatformUtils.dll" --reference:"$buildDir/FSharpMLIR.dll" --reference:"$buildDir/FSharpMLIRWrapper.dll" --reference:"$fscsPath" "$projectRoot/src/Conversion/ASTToMLIR.fs"

# Compile the pipeline as an EXECUTABLE - F# creates .dll files on macOS even for executables
Write-Host "Compiling pipeline executable..."
Invoke-FSC --out:"$buildDir/FSharpMLIRPipeline.dll" --target:exe --reference:"$buildDir/FSharpMLIR.PlatformUtils.dll" --reference:"$buildDir/FSharpMLIR.dll" --reference:"$buildDir/FSharpMLIRWrapper.dll" --reference:"$buildDir/FSharpMLIRConversion.dll" --reference:"$fscsPath" "$projectRoot/src/Pipeline/Compiler.fs"

# Verify the pipeline executable was created
if (-not (Test-Path -Path "$buildDir/FSharpMLIRPipeline.dll")) {
    Write-Error "Pipeline executable was not created successfully."
    exit 1
}

# Create the runtime configuration file that .NET needs
Write-Host "Creating runtime configuration..."
$runtimeConfig = @{
    "runtimeOptions" = @{
        "tfm" = "net8.0"
        "framework" = @{
            "name" = "Microsoft.NETCore.App"
            "version" = "8.0.0"
        }
        "configProperties" = @{
            "System.Runtime.Serialization.EnableUnsafeBinaryFormatterSerialization" = $false
            "System.GC.Server" = $true
        }
    }
} | ConvertTo-Json -Depth 10

$runtimeConfigPath = "$buildDir/FSharpMLIRPipeline.runtimeconfig.json"
$runtimeConfig | Out-File -FilePath $runtimeConfigPath -Encoding UTF8

Write-Host "Pipeline compiled successfully as: $buildDir/FSharpMLIRPipeline.dll"
Write-Host "Runtime config created at: $runtimeConfigPath"

# Test if the pipeline can run at all
Write-Host "Testing pipeline startup..."
$env:DOTNET_EnableWriteXorExecute = "0"  # Disable W^X for macOS compatibility
$testOutput = & "dotnet" "$buildDir/FSharpMLIRPipeline.dll" 2>&1
$testExitCode = $LASTEXITCODE

if ($testExitCode -eq 1) {
    Write-Host "✓ Pipeline startup successful (expected exit code 1 for no arguments)"
} elseif ($testExitCode -eq 0) {
    Write-Host "✓ Pipeline startup successful"
} else {
    Write-Host "⚠ Pipeline startup test failed with exit code: $testExitCode"
    if ($testOutput) {
        Write-Host "Test output:"
        $testOutput | ForEach-Object { Write-Host "  $_" }
    }
    
    # If the basic test fails, try to get more information about the libraries
    Write-Host "Additional diagnostics:"
    Write-Host "Checking library dependencies..."
    
    $otoolOutput = & "otool" "-L" "/usr/local/lib/libLLVM.dylib" 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "libLLVM.dylib dependencies:"
        $otoolOutput | Select-Object -First 10 | ForEach-Object { Write-Host "  $_" }
    }
    
    Write-Host "Checking for required symbols..."
    $nmOutput = & "nm" "-D" "/usr/local/lib/libLLVM.dylib" 2>&1 | Select-String "LLVMInitializeNativeTarget" -Context 0,0
    if ($nmOutput) {
        Write-Host "Found LLVMInitializeNativeTarget: $nmOutput"
    } else {
        Write-Host "⚠ LLVMInitializeNativeTarget symbol not found"
    }
}

# Use the MLIR/LLVM compilation pipeline to compile HelloWorld.fs to native binary
Write-Host "Compiling HelloWorld.fs through MLIR/LLVM pipeline..."

# Verify all required files exist before running
Write-Host "Pre-flight checks:"
Write-Host "  ✓ Pipeline DLL: $(Test-Path -Path "$buildDir/FSharpMLIRPipeline.dll")"
Write-Host "  ✓ Runtime config: $(Test-Path -Path "$buildDir/FSharpMLIRPipeline.runtimeconfig.json")"
Write-Host "  ✓ Input file: $(Test-Path -Path "$projectRoot/src/Examples/HelloWorld.fs")"

# Run the pipeline executable with dotnet
$pipelineArgs = @(
    "$projectRoot/src/Examples/HelloWorld.fs"
    "-o"
    "$buildDir/HelloWorld"
    "--verbose"
)

Write-Host "Running: dotnet $buildDir/FSharpMLIRPipeline.dll $($pipelineArgs -join ' ')"
Write-Host "Working directory: $(Get-Location)"

# Set environment variable for macOS compatibility and run from build directory
Push-Location $buildDir
$env:DOTNET_EnableWriteXorExecute = "0"  # Disable W^X for macOS compatibility

# Capture both stdout and stderr
$pipelineOutput = & "dotnet" "FSharpMLIRPipeline.dll" $pipelineArgs 2>&1

$pipelineExitCode = $LASTEXITCODE
Pop-Location

Write-Host "Pipeline exit code: $pipelineExitCode"

if ($pipelineOutput) {
    Write-Host "Pipeline output:"
    $pipelineOutput | ForEach-Object { Write-Host "  $_" }
}

# Analyze what was created
Write-Host "Analyzing compilation results..."
Write-Host "Files in build directory:"
Get-ChildItem -Path "$buildDir" | ForEach-Object {
    $size = if ($_.Length -gt 1024) { "$([math]::Round($_.Length/1024, 1)) KB" } else { "$($_.Length) bytes" }
    Write-Host "  $($_.Name) - Size: $size - Type: $(if ($_.Extension) { $_.Extension } else { 'no extension' })"
}

Write-Host ""# Check if the native binary was created
if (Test-Path -Path "$buildDir/HelloWorld") {
    $helloWorldFile = Get-Item "$buildDir/HelloWorld"
    Write-Host "HelloWorld file found - Size: $($helloWorldFile.Length) bytes"
    
    # Check if it's actually a native binary by examining its content
    $fileOutput = & "file" "$buildDir/HelloWorld" 2>&1
    Write-Host "File type: $fileOutput"
    
    if ($fileOutput -match "Mach-O.*executable") {
        Write-Host "✓ Native Mach-O executable detected"
        
        # Make it executable
        & "chmod" "+x" "$buildDir/HelloWorld"
        
        # Test the binary
        Write-Host "Testing the native binary..."
        try {
            & "$buildDir/HelloWorld"
        } catch {
            Write-Host "Error running native binary: $_"
        }
    } elseif ($fileOutput -match "ASCII text" -or $helloWorldFile.Length -lt 1000) {
        Write-Host "⚠ File appears to be text or very small - likely not a native binary"
        Write-Host "Content preview:"
        Get-Content "$buildDir/HelloWorld" -TotalCount 10
    } else {
        Write-Host "⚠ File type not recognized as native binary"
        Write-Host "Attempting to run anyway..."
        try {
            & "chmod" "+x" "$buildDir/HelloWorld"
            & "$buildDir/HelloWorld"
        } catch {
            Write-Host "Failed to execute: $_"
        }
    }
} else {
    Write-Host "❌ HelloWorld binary was not created."
    
    # Check for .NET assemblies that might have been created as fallback
    if (Test-Path -Path "$buildDir/HelloWorld.dll") {
        Write-Host "Found .NET assembly fallback at: $buildDir/HelloWorld.dll"
        Write-Host "Testing the .NET assembly..."
        & "dotnet" "$buildDir/HelloWorld.dll"
    } elseif (Test-Path -Path "$buildDir/HelloWorld.exe") {
        Write-Host "Found .NET executable fallback at: $buildDir/HelloWorld.exe"  
        Write-Host "Testing the .NET executable..."
        & "dotnet" "$buildDir/HelloWorld.exe"
    } else {
        Write-Host "No output files found. Pipeline may have failed completely."
        Write-Host "Check the pipeline output above for specific error messages."
    }
}

Write-Host "Build completed."