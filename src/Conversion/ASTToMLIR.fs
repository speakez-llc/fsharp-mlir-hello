module FSharpMLIR.Conversion.ASTToMLIR

open FSharp.Compiler.Syntax
open FSharpMLIR.Bindings.MLIR
open FSharpMLIR.Bindings.MLIRWrapper

// Converter to transform F# AST to MLIR
type Converter(context: MLIRContext) =
    // Helper to extract the text of an identifier safely
    let getIdentText (ident: obj) =
        match ident with
        | :? SynIdent as synIdent -> 
            match synIdent with
            | SynIdent.SynIdent(id, _) -> id.idText
        | _ -> "unknown_ident"

    // Main entry point to convert an F# module to MLIR
    member this.ConvertModule(mlirModule: MLIRModule, parsedModule: obj) =
        printfn "Converting F# module to MLIR"
        
        // Use reflection to handle complex module structure
        let moduleType = parsedModule.GetType()
        let declarationsProperty = moduleType.GetProperty("Declarations")
        
        if declarationsProperty <> null then
            let declarations = declarationsProperty.GetValue(parsedModule) :?> obj list
            for decl in declarations do
                this.ConvertDeclaration(mlirModule, decl)
    
    // Convert a module declaration to MLIR operations
    member this.ConvertDeclaration(mlirModule: MLIRModule, decl: obj) =
        let declType = decl.GetType()
        
        // Check if this is a Let declaration
        if declType.Name = "SynModuleDecl+Let" then
            // Use reflection to extract bindings
            let bindingsProperty = declType.GetProperty("Bindings")
            let bindings = bindingsProperty.GetValue(decl) :?> obj list
            
            for binding in bindings do
                this.ConvertBinding(mlirModule, binding)
        else
            printfn "Unsupported declaration type: %A" decl
    
    // Convert a value binding to an MLIR function
    member this.ConvertBinding(mlirModule: MLIRModule, binding: obj) =
        let bindingType = binding.GetType()

        let isMutableProp = bindingType.GetProperty("IsMutable")
        let isMutable = if isMutableProp <> null then isMutableProp.GetValue(binding) :?> bool else false
    
        
        // Safely extract function name using reflection
        let functionName = 
            try
                let patProperty = bindingType.GetProperty("Pattern")
                let pat = patProperty.GetValue(binding)
                let patType = pat.GetType()
                
                if patType.Name = "SynPat+Named" then
                    let identProperty = patType.GetProperty("Ident")
                    let ident = identProperty.GetValue(pat)
                    getIdentText ident
                else
                    "unnamed_function"
            with 
            | _ -> "unnamed_function"
        
        printfn "Converting function: %s (mutable: %b)" functionName isMutable
        
        let functionType = nativeint 0 // Placeholder
        let functionOp = this.CreateFunctionOp(mlirModule, functionName, functionType)
        
        // Process the function body using reflection
        try
            let exprProperty = bindingType.GetProperty("Expr")
            let expr = exprProperty.GetValue(binding)
            this.ConvertExpression(expr) 
            
            // Check for printf-like expressions
            let exprType = expr.GetType()
            if exprType.Name = "SynExpr+App" then
                // Use reflection to check for printf
                let funProperty = exprType.GetProperty("Func")
                let argProperty = exprType.GetProperty("Arg")
                
                let func = funProperty.GetValue(expr)
                let arg = argProperty.GetValue(expr)
                
                let funcType = func.GetType()
                if funcType.Name = "SynExpr+Ident" then
                    let identProperty = funcType.GetProperty("Ident")
                    let ident = identProperty.GetValue(func)
                    
                    if getIdentText ident = "printf" then
                        this.ConvertPrintf(functionOp, arg)
        with 
        | _ -> printfn "Could not process function body"

    member this.ConvertExpression(expr: obj) =
        let exprType = expr.GetType()
        printfn "Converting expression type: %s" exprType.Name
        
        match exprType.Name with
        | "SynExpr+While" ->
            // Handle while loop
            try
                let whileExprProp = exprType.GetProperty("WhileExpr") 
                let doExprProp = exprType.GetProperty("DoExpr")
                
                let condition = whileExprProp.GetValue(expr)
                let body = doExprProp.GetValue(expr)
                
                printfn "  Found while loop"
                printfn "  TODO: Generate MLIR blocks for loop"
                
                // For now, just recurse on body
                this.ConvertExpression(body)
            with _ -> printfn "  Error processing while loop"
            
        | "SynExpr+Sequential" ->
            // Handle sequence of expressions
            try
                let expr1Prop = exprType.GetProperty("Expr1")
                let expr2Prop = exprType.GetProperty("Expr2")
                
                let expr1 = expr1Prop.GetValue(expr)
                let expr2 = expr2Prop.GetValue(expr)
                
                this.ConvertExpression(expr1)
                this.ConvertExpression(expr2)
            with _ -> printfn "  Error processing sequence"
            
        | "SynExpr+App" ->
            // Handle function application
            this.ConvertApp(expr)
            
        | "SynExpr+LongIdentSet" ->
            // Handle assignment (x <- x + 1)
            printfn "  Found assignment"
            
        | "SynExpr+Ident" ->
            // Handle identifier
            printfn "  Found identifier"
            
        | _ ->
            printfn "  Unhandled expression: %s" exprType.Name
    
    // Convert a function application expression (SynExpr+App)
    member this.ConvertApp(expr: obj) =
        try
            let exprType = expr.GetType()
            
            // Extract function and argument from App expression
            let funProperty = exprType.GetProperty("Func")
            let argProperty = exprType.GetProperty("Arg")
            
            if funProperty <> null && argProperty <> null then
                let func = funProperty.GetValue(expr)
                let arg = argProperty.GetValue(expr)
                
                // Get the function name
                let funcName = this.ExtractFunctionName(func)
                
                // Handle special cases first
                match funcName with
                | "printf" | "printfn" ->
                    this.ConvertPrintf(nativeint 0, arg) // Use placeholder for functionOp
                | _ ->
                    // General function call - extract arguments and convert
                    let args = this.ExtractArguments(arg)
                    this.ConvertFunctionCall(funcName, args)
            else
                printfn "Could not extract function or argument from App expression"
        with 
        | ex -> printfn "Error converting App expression: %s" ex.Message
    
    // Helper method to extract function name from various expression types
    member this.ExtractFunctionName(funcExpr: obj) =
        try
            let funcType = funcExpr.GetType()
            match funcType.Name with
            | "SynExpr+Ident" ->
                // Simple identifier
                let identProperty = funcType.GetProperty("Ident")
                if identProperty <> null then
                    let ident = identProperty.GetValue(funcExpr)
                    getIdentText ident
                else
                    "unknown_function"
            
            | "SynExpr+LongIdent" ->
                // Qualified identifier (e.g., Time.now, String.concat)
                let longIdentProperty = funcType.GetProperty("LongIdent")
                if longIdentProperty <> null then
                    let longIdent = longIdentProperty.GetValue(funcExpr)
                    // For now, just use the last part of the identifier
                    // In a full implementation, you'd want to preserve the full qualified name
                    this.ExtractQualifiedName(longIdent)
                else
                    "unknown_qualified_function"
            
            | _ ->
                printfn "Unhandled function expression type: %s" funcType.Name
                "unknown_function_type"
        with
        | ex -> 
            printfn "Error extracting function name: %s" ex.Message
            "unknown_function"
    
    // Helper method to extract qualified names (e.g., Time.now -> "Time.now")
    member this.ExtractQualifiedName(longIdent: obj) =
        try
            // LongIdent is typically a list of identifiers
            let longIdentType = longIdent.GetType()
            if longIdentType.IsGenericType then
                // Handle list of identifiers
                let identList = longIdent :?> obj list
                let parts = 
                    identList 
                    |> List.map getIdentText
                    |> String.concat "."
                parts
            else
                "unknown_qualified"
        with
        | ex ->
            printfn "Error extracting qualified name: %s" ex.Message
            "unknown_qualified"
    
    // Helper method to extract arguments from argument expressions
    member this.ExtractArguments(argExpr: obj) : obj list =
        try
            let argType = argExpr.GetType()
            match argType.Name with
            | "SynExpr+Tuple" ->
                // Multiple arguments in a tuple
                let elementsProperty = argType.GetProperty("Elements")
                if elementsProperty <> null then
                    elementsProperty.GetValue(argExpr) :?> obj list
                else
                    [argExpr]
            
            | "SynExpr+Paren" ->
                // Parenthesized expression - extract the inner expression
                let exprProperty = argType.GetProperty("Expr")
                if exprProperty <> null then
                    let innerExpr = exprProperty.GetValue(argExpr)
                    this.ExtractArguments(innerExpr)
                else
                    [argExpr]
            
            | _ ->
                // Single argument
                [argExpr]
        with
        | ex ->
            printfn "Error extracting arguments: %s" ex.Message
            [argExpr]
    
    // Helper to create a function operation in MLIR
    member this.CreateFunctionOp(mlirModule: MLIRModule, name: string, funcType: nativeint) =
        printfn "Creating MLIR function: %s" name
        // In a real implementation, you would create an MLIR function operation
        mlirModule.Handle

    member this.ConvertFunctionCall(funcName: string, args: obj list) =
        match funcName with
        | "Time.currentUnixTimestamp" ->
            printfn "Found call to Alloy Time.currentUnixTimestamp"
            // Generate: %result = llvm.call @Alloy_Time_currentUnixTimestamp() : () -> i64
            
        | "Time.sleep" ->
            printfn "Found call to Alloy Time.sleep"
            // Generate: llvm.call @Alloy_Time_sleep(%arg) : (i32) -> ()
            
        | "Time.now" ->
            printfn "Found call to Alloy Time.now"
            // Generate: %result = llvm.call @Alloy_Time_now() : () -> Alloy_Time_DateTime
            
        | "String.concat" | "String.concat3" ->
            printfn "Found call to string concatenation: %s" funcName
            // Generate appropriate string concatenation MLIR
            
        | _ ->
            // Handle other functions
            printfn "Found call to: %s with %d arguments" funcName args.Length
    
    // Helper to convert printf expressions to MLIR operations
    member this.ConvertPrintf(functionOp: nativeint, arg: obj) =
        try
            let argType = arg.GetType()
            if argType.Name = "SynExpr+Const" then
                let constProperty = argType.GetProperty("Constant")
                let constant = constProperty.GetValue(arg)
                let constType = constant.GetType()
                
                if constType.Name = "SynConst+String" then
                    let stringProperty = constType.GetProperty("text")
                    let formatString = stringProperty.GetValue(arg) :?> string
                    printfn "Printf with format string: %s" formatString
        with 
        | _ -> printfn "Could not extract printf argument"
    
    // Helper to get or declare an external function
    member this.GetOrDeclareFunction(name: string, returnType: nativeint) =
        printfn "Getting or declaring function: %s" name
        nativeint 0 // Placeholder