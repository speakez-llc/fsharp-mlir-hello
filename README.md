# Fidelity: F# to Native Executable Framework

<table>
  <tr>
    <td align="center" width="100%">
      <strong>⚠️ Caution: Experimental ⚠️</strong><br>
      This project is in early development and intended for production use.
    </td>
  </tr>
</table>

![alt text](FSharp-MLIR-LLVM.png)

## Overview

Fidelity represents a revolutionary approach to F# development: a completely dependency-free framework that compiles F# directly to native executables using MLIR and LLVM, bypassing the entire .NET ecosystem. The Firefly compiler transforms F# source code through a sophisticated pipeline that eliminates runtime dependencies while preserving functional programming paradigms.

At its core, Fidelity consists of three foundational libraries:
- **Alloy**: Base structures providing static resolution of functions and native F# replacements for BCL components
- **BAREWire**: Memory mapping, IPC, and network interchange using the BARE protocol for seamless process communication
- **Firefly**: The compiler pipeline that orchestrates the entire F# to native transformation

## Architecture

![alt text](<Screenshot 2025-03-13 211122.png>)

The compilation pipeline represents a fundamental reimagining of how functional languages can target native execution environments. By leveraging MLIR's multi-level intermediate representation capabilities and LLVM's battle-tested code generation, Fidelity achieves true zero-dependency native compilation.

### Compilation Pipeline
1. **F# Source Parsing** - Using a standalone F# Compiler Service
2. **AST Conversion** - Transforming F# constructs to MLIR-compatible representations  
3. **MLIR Dialect Generation** - Creating domain-specific intermediate representations
4. **LLVM IR Lowering** - Converting to LLVM's intermediate representation
5. **Native Code Generation** - Producing platform-specific executables

## Example Applications

### Hello World: Foundation Example

The Hello World example demonstrates Fidelity's fundamental capability to compile basic F# programs to native executables without any runtime dependencies. This seemingly simple example actually showcases the complete compilation pipeline working end-to-end, proving that F# syntax and semantics can be preserved while targeting native execution.

### TimeLoop: Native Interop Breakthrough

TimeLoop represents a step change in functional language native interop capabilities. This example goes far beyond basic compilation by demonstrating sophisticated interaction with system-level APIs and native libraries directly from F# code.

**What makes TimeLoop revolutionary:**

TimeLoop showcases Fidelity's ability to seamlessly integrate with native system calls and libraries without marshaling overhead or runtime intervention. Unlike traditional F# applications that require complex P/Invoke mechanisms and runtime marshaling, TimeLoop demonstrates direct native function calls with zero-cost abstractions.

The example illustrates how BAREWire's protocol implementation enables efficient memory mapping and IPC communication patterns that would be impossible or prohibitively expensive in traditional .NET environments. This capability opens entirely new architectural possibilities for F# applications, particularly in systems programming, embedded contexts, and high-performance computing scenarios.

**Technical significance:**

TimeLoop proves that functional programming paradigms can operate at the same performance tier as C/C++ while maintaining F#'s expressive type system and immutability guarantees. The native interop demonstrated here represents a fundamental shift from F# as a high-level application language to F# as a systems programming language capable of zero-overhead native integration.

## Performance Comparison

The compilation results speak to Fidelity's efficiency advantages:

### Standard .NET Debug Build
![alt text](<Screenshot 2025-03-13 224831.png>)

### Standard .NET Production Build
![alt text](<Screenshot 2025-03-13 230147.png>)
![alt text](<Screenshot 2025-03-13 225719.png>)

These comparisons demonstrate Fidelity's ability to produce significantly smaller, faster executables with zero runtime dependencies compared to traditional .NET compilation approaches.

## Development Environment Setup

### Toolchain Dependencies
- MLIR libraries
- LLVM infrastructure  
- F# Compiler Service
- PowerShell (for build scripting)
- MSYS2 or equivalent MinGW development environment

### MSYS2 Installation
```bash
pacman -Syu  # Update package database and core system packages
pacman -S mingw-w64-x86_64-toolchain
pacman -S mingw-w64-x86_64-llvm
pacman -S mingw-w64-x86_64-clang
pacman -S mingw-w64-x86_64-cmake
pacman -S mingw-w64-x86_64-ninja
```

### Project Structure
- `src/`: Source code for the compiler pipeline
  - `Bindings/`: Native bindings for MLIR and LLVM
  - `Conversion/`: AST conversion logic
  - `Pipeline/`: Compilation pipeline implementation
  - `Examples/`: Sample F# programs (Hello World, TimeLoop)
- `tools/`: Build and development scripts
- `build/`: Compiled output directory

## Building Fidelity

Execute the compilation pipeline:
```powershell
.\tools\build.ps1
```

**Note**: Verify path settings match your project and dependency locations.

### Build Process Stages
1. Compile F# MLIR Bindings
2. Compile MLIR Wrapper Modules
3. Generate Conversion Modules  
4. Build Compilation Pipeline
5. Create Example Executables

## Current Limitations and Future Directions

**Present constraints:**
- Experimental parsing mechanisms with limited language feature coverage
- No garbage collection implementation
- Minimal optimization strategies currently implemented

**Research directions:**
- Enhanced type system mapping for complex F# constructs
- Advanced optimization passes leveraging MLIR's transformation infrastructure
- Memory management strategies for functional programming patterns
- Comprehensive language construct support expansion

## Technical Implementation Notes

This implementation deliberately diverges from standard .NET compilation approaches. The F# Compiler Service operates as a standalone library, completely decoupled from .NET infrastructure. Build orchestration uses direct PowerShell coordination, and type resolution includes custom binding implementations where standard mechanisms prove inadequate.

**Research Prototype Disclaimer**: This represents experimental compiler technology and should be considered a research prototype exploring the boundaries of functional language compilation.
