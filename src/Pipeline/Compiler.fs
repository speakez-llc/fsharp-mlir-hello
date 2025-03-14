module FSharpMLIR.Pipeline.Compiler

open FSharp.Compiler.Syntax
open FSharpMLIR.Bindings.MLIRWrapper
open FSharpMLIR.Bindings.MLIR
open FSharpMLIR.Conversion

let compile (fsharpCode: string) (outputPath: string) =
    // Parse F# code to get AST
    let ast = FSharpParser.parseProgram fsharpCode
    
    // Create MLIR context and module
    use context = MLIRContext.Create()
    use mlirModule = MLIRModule.CreateEmpty(context)
    
    // Convert F# AST to MLIR
    let converter = ASTToMLIR.Converter(context)
    converter.ConvertModule(mlirModule, ast)
    
    // Apply MLIR transformations (lowering to LLVM dialect)
    MLIRTransforms.applyLoweringPasses(mlirModule)
    
    // Convert MLIR to LLVM IR
    let llvmIR = MLIRToLLVM.convert(mlirModule)
    
    // Optimize and generate object file
    LLVMCodeGen.generateObjectFile(llvmIR, "output.o")
    
    // Link to produce executable
    Linker.linkObjectFile("output.o", outputPath)