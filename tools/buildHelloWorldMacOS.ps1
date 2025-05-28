#!/usr/bin/env pwsh
# Enhanced Cross-Platform Build script for macOS - HelloWorld example

# Define paths
$homeDir = if ($IsMacOS) { [System.Environment]::GetFolderPath("UserProfile") } else { $HOME }
$projectRoot = Join-Path $homeDir "repos/fsharp-mlir-hello"
$buildDir = Join-Path $projectRoot "build"

Write-Host "======================================================"
Write-Host "Fidelity/Firefly F# Compiler Build Script"
Write-Host "======================================================"
Write-Host "Project root: $projectRoot"
Write-Host "Build directory: $buildDir"

# Ensure the build directory exists
if (-not (Test-Path -Path $buildDir)) {
    New-Item -ItemType Directory -Path $buildDir -Force
    Write-Host "Created build directory: $buildDir"
}

# Detect .NET SDK version and find F# compiler
$dotnetVersion = dotnet --version
Write-Host "Detected .NET SDK version: $dotnetVersion"

$fscPath = "/usr/local/share/dotnet/sdk/$dotnetVersion/FSharp/fsc.dll"
if (-not (Test-Path -Path $fscPath)) {
    $fscPath = "/opt/homebrew/share/dotnet/sdk/$dotnetVersion/FSharp/fsc.dll"
    if (-not (Test-Path -Path $fscPath)) {
        Write-Error "F# compiler (fsc.dll) not found in expected locations"
        Write-Host "Searched locations:"
        Write-Host "  /usr/local/share/dotnet/sdk/$dotnetVersion/FSharp/fsc.dll"
        Write-Host "  /opt/homebrew/share/dotnet/sdk/$dotnetVersion/FSharp/fsc.dll"
        exit 1
    }
}

Write-Host "Found F# compiler at: $fscPath"

# Function to run the F# compiler (fsc.dll) with enhanced error handling
function Invoke-FSC {
    Write-Host "Compiling: $($args -join ' ')"
    $output = & "dotnet" $fscPath --nologo --targetprofile:netcore --define:MACOS $args 2>&1
    $exitCode = $LASTEXITCODE
    
    if ($output) {
        Write-Host "Compiler output:"
        $output | ForEach-Object { Write-Host "  $_" }
    }
    
    if ($exitCode -ne 0) {
        Write-Error "F# compilation failed with exit code: $exitCode"
        Write-Host "Command that failed: dotnet $fscPath --nologo --targetprofile:netcore --define:MACOS $($args -join ' ')"
        exit 1
    }
    
    Write-Host "✓ Compilation successful"
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

# Enhanced LLVM/MLIR library checking
Write-Host "======================================================"
Write-Host "Checking for required native libraries..."
Write-Host "======================================================"

$mlirLibPath = "/usr/local/lib/libMLIR.dylib"
$llvmLibPath = "/usr/local/lib/libLLVM.dylib"

# Alternative paths to check
$alternativePaths = @(
    "/opt/homebrew/lib/libMLIR.dylib"
    "/opt/homebrew/lib/libLLVM.dylib"
    "/opt/homebrew/opt/llvm/lib/libMLIR.dylib"
    "/opt/homebrew/opt/llvm/lib/libLLVM.dylib"
    "/usr/local/opt/llvm/lib/libMLIR.dylib"
    "/usr/local/opt/llvm/lib/libLLVM.dylib"
)

function Test-Library {
    param($libraryPath, $libraryName)
    
    if (Test-Path -Path $libraryPath) {
        $libInfo = Get-Item $libraryPath
        Write-Host "✓ Found $libraryName at: $libraryPath"
        Write-Host "  Size: $([math]::Round($libInfo.Length/1MB, 2)) MB"
        Write-Host "  Modified: $($libInfo.LastWriteTime)"
        
        # Test library dependencies
        try {
            $otoolOutput = & "otool" "-L" $libraryPath 2>/dev/null | Select-Object -First 5
            Write-Host "  Dependencies:"
            $otoolOutput | ForEach-Object { Write-Host "    $_" }
        } catch {
            Write-Host "  Could not analyze dependencies"
        }
        
        return $true
    } else {
        Write-Host "✗ $libraryName not found at: $libraryPath"
        return $false
    }
}

$llvmFound = Test-Library $llvmLibPath "libLLVM.dylib"
$mlirFound = Test-Library $mlirLibPath "libMLIR.dylib"

if (-not $llvmFound) {
    Write-Host "Checking alternative paths for libLLVM.dylib..."
    foreach ($altPath in $alternativePaths | Where-Object { $_ -like "*libLLVM*" }) {
        if (Test-Path -Path $altPath) {
            Write-Host "Found libLLVM.dylib at alternative location: $altPath"
            $llvmFound = $true
            break
        }
    }
}

if (-not $mlirFound) {
    Write-Host "Checking alternative paths for libMLIR.dylib..."
    foreach ($altPath in $alternativePaths | Where-Object { $_ -like "*libMLIR*" }) {
        if (Test-Path -Path $altPath) {
            Write-Host "Found libMLIR.dylib at alternative location: $altPath"
            $mlirFound = $true
            break
        }
    }
}

if (-not $llvmFound -or -not $mlirFound) {
    Write-Host "======================================================"
    Write-Host "⚠ WARNING: Required libraries not found"
    Write-Host "======================================================"
    Write-Host "To install LLVM/MLIR on macOS:"
    Write-Host "  brew install llvm"
    Write-Host ""
    Write-Host "Then create symlinks:"
    Write-Host "  sudo ln -sf /opt/homebrew/Cellar/llvm/*/lib/libLLVM.dylib /usr/local/lib/"
    Write-Host "  sudo ln -sf /opt/homebrew/Cellar/llvm/*/lib/libMLIR.dylib /usr/local/lib/"
    Write-Host ""
    Write-Host "Or update your DYLD_LIBRARY_PATH to include the LLVM library directory"
    Write-Host "======================================================"
    
    # Don't exit - continue with compilation but warn about potential runtime issues
    Write-Host "Continuing with compilation - native code generation may fail at runtime"
}

# Test for specific LLVM symbols
if ($llvmFound) {
    Write-Host "Testing for required LLVM symbols..."
    try {
        $nmOutput = & "nm" "-gU" $llvmLibPath 2>/dev/null | Select-String -Pattern "LLVMInitializeNativeTarget|LLVMContextCreate|LLVMGetDefaultTargetTriple" | Select-Object -First 3
        if ($nmOutput) {
            Write-Host "✓ Found required LLVM symbols:"
            $nmOutput | ForEach-Object { Write-Host "  $_" }
        } else {
            Write-Host "⚠ Could not find expected LLVM symbols - may cause runtime issues"
        }
    } catch {
        Write-Host "⚠ Could not analyze LLVM symbols"
    }
}

Write-Host "======================================================"
Write-Host "Starting compilation process..."
Write-Host "======================================================"

# Step 1: Compile platform utilities
Write-Host "Step 1: Compiling platform utilities..."
Invoke-FSC --out:"$buildDir/FSharpMLIR.PlatformUtils.dll" --target:library --define:MACOS "$projectRoot/src/Bindings/PlatformUtils.fs"

# Step 2: Compile F# bindings library
Write-Host "Step 2: Compiling F# bindings library..."
Invoke-FSC --out:"$buildDir/FSharpMLIR.dll" --target:library --reference:"$buildDir/FSharpMLIR.PlatformUtils.dll" --define:MACOS "$projectRoot/src/Bindings/LLVMBindings.fs" "$projectRoot/src/Bindings/MLIRBindings.fs"

# Step 3: Compile the wrapper
Write-Host "Step 3: Compiling wrapper module..."
Invoke-FSC --out:"$buildDir/FSharpMLIRWrapper.dll" --target:library --reference:"$buildDir/FSharpMLIR.PlatformUtils.dll" --reference:"$buildDir/FSharpMLIR.dll" --define:MACOS "$projectRoot/src/Bindings/MLIRWrapper.fs"

# Step 4: Compile conversion modules
Write-Host "Step 4: Compiling conversion modules..."
Invoke-FSC --out:"$buildDir/FSharpMLIRConversion.dll" --target:library --reference:"$buildDir/FSharpMLIR.PlatformUtils.dll" --reference:"$buildDir/FSharpMLIR.dll" --reference:"$buildDir/FSharpMLIRWrapper.dll" --reference:"$fscsPath" --define:MACOS "$projectRoot/src/Conversion/ASTToMLIR.fs"

# Step 5: Compile the pipeline as an EXECUTABLE
Write-Host "Step 5: Compiling pipeline executable..."
Invoke-FSC --out:"$buildDir/FSharpMLIRPipeline.dll" --target:exe --reference:"$buildDir/FSharpMLIR.PlatformUtils.dll" --reference:"$buildDir/FSharpMLIR.dll" --reference:"$buildDir/FSharpMLIRWrapper.dll" --reference:"$buildDir/FSharpMLIRConversion.dll" --reference:"$fscsPath" --define:MACOS "$projectRoot/src/Pipeline/Compiler.fs"

# Verify the pipeline executable was created
if (-not (Test-Path -Path "$buildDir/FSharpMLIRPipeline.dll")) {
    Write-Error "Pipeline executable was not created successfully."
    exit 1
}

# Step 6: Create the runtime configuration file that .NET needs
Write-Host "Step 6: Creating runtime configuration..."
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

Write-Host "✓ Pipeline compiled successfully as: $buildDir/FSharpMLIRPipeline.dll"
Write-Host "✓ Runtime config created at: $runtimeConfigPath"

# Step 7: Test if the pipeline can run at all
Write-Host "======================================================"
Write-Host "Step 7: Testing pipeline startup..."
Write-Host "======================================================"

$env:DOTNET_EnableWriteXorExecute = "0"  # Disable W^X for macOS compatibility

# Set up library path for better native library discovery
$env:DYLD_LIBRARY_PATH = "/usr/local/lib:/opt/homebrew/lib:/opt/homebrew/opt/llvm/lib:$env:DYLD_LIBRARY_PATH"
$env:DYLD_FALLBACK_LIBRARY_PATH = "/usr/local/lib:/opt/homebrew/lib:/opt/homebrew/opt/llvm/lib:$env:DYLD_FALLBACK_LIBRARY_PATH"

Write-Host "Environment setup:"
Write-Host "  DOTNET_EnableWriteXorExecute: $env:DOTNET_EnableWriteXorExecute"
Write-Host "  DYLD_LIBRARY_PATH: $env:DYLD_LIBRARY_PATH"

# Test with no arguments (should fail gracefully with usage message)
Write-Host "Running pipeline with no arguments (should show usage)..."
$testOutput = & "dotnet" "$buildDir/FSharpMLIRPipeline.dll" 2>&1
$testExitCode = $LASTEXITCODE

Write-Host "Pipeline startup test exit code: $testExitCode"

if ($testOutput) {
    Write-Host "Pipeline startup test output:"
    $testOutput | ForEach-Object { Write-Host "  $_" }
}

# Analyze the test results
if ($testExitCode -eq 1 -and $testOutput -match "No input file specified") {
    Write-Host "✓ Pipeline startup test successful (expected usage message)"
} elseif ($testExitCode -eq 0) {
    Write-Host "✓ Pipeline startup test successful"
} elseif ($testExitCode -eq 134) {
    Write-Host "❌ Pipeline crashed with SIGABRT (exit code 134)"
    Write-Host "This indicates a serious native library or P/Invoke issue"
    Write-Host "Common causes:"
    Write-Host "  - Missing native libraries"
    Write-Host "  - Incorrect function signatures in P/Invoke declarations"
    Write-Host "  - Library version incompatibility"
    Write-Host "  - Memory access violations"
    Write-Host ""
    Write-Host "Continuing with compilation attempt..."
} else {
    Write-Host "⚠ Pipeline startup test returned unexpected exit code: $testExitCode"
    Write-Host "This may indicate issues with the native libraries or P/Invoke bindings"
    Write-Host "Continuing with compilation attempt..."
}

# Step 8: Use the MLIR/LLVM compilation pipeline to compile HelloWorld.fs to native binary
Write-Host "======================================================"
Write-Host "Step 8: Compiling HelloWorld.fs through MLIR/LLVM pipeline..."
Write-Host "======================================================"

# Verify all required files exist before running
Write-Host "Pre-flight checks:"
Write-Host "  ✓ Pipeline DLL: $(Test-Path -Path "$buildDir/FSharpMLIRPipeline.dll")"
Write-Host "  ✓ Runtime config: $(Test-Path -Path "$buildDir/FSharpMLIRPipeline.runtimeconfig.json")"
Write-Host "  ✓ Input file: $(Test-Path -Path "$projectRoot/src/Examples/HelloWorld.fs")"

# Check input file content
if (Test-Path -Path "$projectRoot/src/Examples/HelloWorld.fs") {
    $inputContent = Get-Content "$projectRoot/src/Examples/HelloWorld.fs" -Raw
    Write-Host "Input file content preview:"
    $inputContent.Split("`n") | Select-Object -First 5 | ForEach-Object { Write-Host "  $_" }
    Write-Host "  ... (total length: $($inputContent.Length) characters)"
}

# Run the pipeline executable with dotnet
$pipelineArgs = @(
    "$projectRoot/src/Examples/HelloWorld.fs"
    "-o"
    "$buildDir/HelloWorld"
    "--verbose"
)

Write-Host "Running command:"
Write-Host "  dotnet $buildDir/FSharpMLIRPipeline.dll $($pipelineArgs -join ' ')"
Write-Host "Working directory: $(Get-Location)"

# Set environment variables for better compatibility and run from build directory
Push-Location $buildDir

try {
    # Capture both stdout and stderr with timestamps
    Write-Host "Starting pipeline execution at: $(Get-Date)"
    $pipelineOutput = & "dotnet" "FSharpMLIRPipeline.dll" $pipelineArgs 2>&1
    $pipelineExitCode = $LASTEXITCODE
    Write-Host "Pipeline execution completed at: $(Get-Date)"
} catch {
    Write-Host "Exception during pipeline execution: $_"
    $pipelineExitCode = -1
    $pipelineOutput = @("Exception: $_")
} finally {
    Pop-Location
}

Write-Host "======================================================"
Write-Host "Pipeline execution results:"
Write-Host "======================================================"
Write-Host "Exit code: $pipelineExitCode"

if ($pipelineOutput) {
    Write-Host "Pipeline output:"
    $pipelineOutput | ForEach-Object { Write-Host "  $_" }
} else {
    Write-Host "No output captured from pipeline"
}

# Step 9: Analyze what was created
Write-Host "======================================================"
Write-Host "Step 9: Analyzing compilation results..."
Write-Host "======================================================"

Write-Host "Files in build directory:"
$buildFiles = Get-ChildItem -Path "$buildDir" | Sort-Object Name
foreach ($file in $buildFiles) {
    $sizeStr = if ($file.Length -gt 1024*1024) { 
        "$([math]::Round($file.Length/(1024*1024), 2)) MB" 
    } elseif ($file.Length -gt 1024) { 
        "$([math]::Round($file.Length/1024, 1)) KB" 
    } else { 
        "$($file.Length) bytes" 
    }
    $typeStr = if ($file.Extension) { $file.Extension } else { "no extension" }
    Write-Host "  $($file.Name) - Size: $sizeStr - Type: $typeStr"
}

Write-Host ""

# Step 10: Check if the native binary was created
Write-Host "======================================================"
Write-Host "Step 10: Checking for native binary..."
Write-Host "======================================================"

if (Test-Path -Path "$buildDir/HelloWorld") {
    $helloWorldFile = Get-Item "$buildDir/HelloWorld"
    Write-Host "✓ HelloWorld file found"
    Write-Host "  Size: $($helloWorldFile.Length) bytes"
    Write-Host "  Created: $($helloWorldFile.CreationTime)"
    Write-Host "  Modified: $($helloWorldFile.LastWriteTime)"
    
    # Check if it's actually a native binary by examining its content
    try {
        $fileOutput = & "file" "$buildDir/HelloWorld" 2>&1
        Write-Host "  File type: $fileOutput"
        
        if ($fileOutput -match "Mach-O.*executable") {
            Write-Host "✓ Native Mach-O executable detected"
            
            # Make it executable
            & "chmod" "+x" "$buildDir/HelloWorld"
            Write-Host "✓ Set executable permissions"
            
            # Test the binary
            Write-Host "Testing the native binary..."
            try {
                $execOutput = & "$buildDir/HelloWorld" 2>&1
                $execExitCode = $LASTEXITCODE
                Write-Host "Native binary execution:"
                Write-Host "  Exit code: $execExitCode"
                if ($execOutput) {
                    Write-Host "  Output:"
                    $execOutput | ForEach-Object { Write-Host "    $_" }
                }
                
                if ($execExitCode -eq 0) {
                    Write-Host "✓ Native binary executed successfully!"
                } else {
                    Write-Host "⚠ Native binary exited with code $execExitCode"
                }
            } catch {
                Write-Host "❌ Error running native binary: $_"
            }
        } elseif ($fileOutput -match "ASCII text" -or $helloWorldFile.Length -lt 1000) {
            Write-Host "⚠ File appears to be text or very small - likely not a native binary"
            Write-Host "Content preview:"
            Get-Content "$buildDir/HelloWorld" -TotalCount 10 | ForEach-Object { Write-Host "    $_" }
        } else {
            Write-Host "⚠ File type not recognized as native binary"
            Write-Host "Attempting to run anyway..."
            try {
                & "chmod" "+x" "$buildDir/HelloWorld"
                $execOutput = & "$buildDir/HelloWorld" 2>&1
                $execExitCode = $LASTEXITCODE
                Write-Host "Execution result: exit code $execExitCode"
                if ($execOutput) {
                    $execOutput | ForEach-Object { Write-Host "  $_" }
                }
            } catch {
                Write-Host "Failed to execute: $_"
            }
        }
    } catch {
        Write-Host "Error analyzing file type: $_"
    }
} else {
    Write-Host "❌ HelloWorld binary was not created."
    
    # Check for .NET assemblies that might have been created as fallback
    $fallbackFiles = @(
        "$buildDir/HelloWorld.dll"
        "$buildDir/HelloWorld.exe"
        "$buildDir/output.dll"
        "$buildDir/a.out"
    )
    
    $foundFallback = $false
    foreach ($fallbackFile in $fallbackFiles) {
        if (Test-Path -Path $fallbackFile) {
            Write-Host "Found potential fallback output: $fallbackFile"
            $fallbackInfo = Get-Item $fallbackFile
            Write-Host "  Size: $($fallbackInfo.Length) bytes"
            
            if ($fallbackFile -like "*.dll") {
                Write-Host "Testing .NET assembly..."
                try {
                    & "dotnet" $fallbackFile
                } catch {
                    Write-Host "Failed to run .NET assembly: $_"
                }
            }
            
            $foundFallback = $true
        }
    }
    
    if (-not $foundFallback) {
        Write-Host "No output files found. Pipeline may have failed completely."
        Write-Host "Check the pipeline output above for specific error messages."
    }
}

Write-Host "======================================================"
Write-Host "Build process completed."
Write-Host "======================================================"

# Summary
if (Test-Path -Path "$buildDir/HelloWorld") {
    $finalFile = Get-Item "$buildDir/HelloWorld"
    if ($finalFile.Length -gt 10000) {
        Write-Host "✓ SUCCESS: Native binary appears to have been created successfully"
        Write-Host "  Location: $buildDir/HelloWorld"
        Write-Host "  Size: $($finalFile.Length) bytes"
    } else {
        Write-Host "⚠ PARTIAL SUCCESS: Output file created but may not be a native binary"
        Write-Host "  Location: $buildDir/HelloWorld"
        Write-Host "  Size: $($finalFile.Length) bytes (unusually small)"
    }
} else {
    Write-Host "❌ FAILURE: No native binary was created"
    Write-Host "  Check the error messages above for troubleshooting steps"
}

Write-Host "======================================================"