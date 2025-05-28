module FSharpMLIR.Conversion.ASTToMLIR

open System
open System.Collections.Generic
open System.Reflection
open FSharp.Compiler.Syntax
open FSharpMLIR.Bindings.MLIR
open FSharpMLIR.Bindings.MLIRWrapper

/// <summary>
/// Converter to transform F# AST to MLIR
/// </summary>
type Converter(context: MLIRContext) =
    // Dictionary to cache function operations by name
    let functionOps = Dictionary<string, nativeint>()
    
    // Dictionary to cache global string operations by value
    let globalStrings = Dictionary<string, nativeint>()
    
    // Helper to extract the text of an identifier safely
    let getIdentText (ident: obj) =
        match ident with
        | :? SynIdent as synIdent -> 
            match synIdent with
            | SynIdent.SynIdent(id, _) -> id.idText
        | _ -> "unknown_ident"

    // Create pointer type for a given element type
    let createPointerType (elementType: nativeint) =
        // In MLIR, pointer types are represented in different ways depending on the dialect
        // For LLVM dialect, we use !llvm.ptr
        let location = context.CreateUnknownLocation()
        let ptrTypeAttr = context.CreateStringAttribute("ptr")
        
        // Create the MLIR type for !llvm.ptr
        let ptrType = mlirOperationCreate(
            "llvm.ptr",
            location,
            1u,  // One result (the type)
            [| elementType |],  // The element type
            0u,
            [||],
            1u,
            [| ptrTypeAttr |],
            0u,
            [||])
            
        mlirOperationGetResult(ptrType, 0u)
    
    // Helper to create and register a function operation
    let createFunctionOp (name: string) (returnType: nativeint) (paramTypes: nativeint[]) =
        let location = context.CreateUnknownLocation()
        
        // Create function type
        let funcType = context.CreateFunctionType(paramTypes, [| returnType |])
        
        // Create function operation attributes
        let funcNameAttr = context.CreateStringAttribute(name)
        let typeAttr = context.CreateTypeAttribute(funcType)
        let publicAttr = context.CreateBoolAttribute(true)
        
        let attributes = [| 
            ("sym_name", funcNameAttr)
            ("function_type", typeAttr)
            ("public", publicAttr)
        |]
        
        // Create empty region for the function body
        let region = MLIRRegion.Create()
        
        // Create the entry block for the function
        let entryBlock = MLIRBlock.Create()
        region.AddBlock(entryBlock)
        
        // Create the function operation
        let funcOp = mlirOperationCreate(
            "func.func", 
            location, 
            0u, 
            [||], 
            0u, 
            [||], 
            uint32 attributes.Length, 
            [| for (name, attr) in attributes -> createNamedAttribute(name, attr) |], 
            1u, 
            [| region.Handle |])
        
        // Cache the function operation
        functionOps.[name] <- funcOp
        
        // Clean up resources
        mlirLocationDestroy(location)
        
        (funcOp, entryBlock)
    
    // Helper to create a named attribute
    and createNamedAttribute (name: string) (value: nativeint) =
        let nameAttr = context.CreateStringAttribute(name)
        mlirNamedAttributeGet(nameAttr, value)
        
    // Helper to create a global string constant
    let createGlobalString (value: string) =
        if globalStrings.ContainsKey(value) then
            globalStrings.[value]
        else
            let location = context.CreateUnknownLocation()
            
            // Create the string attribute
            let stringAttr = context.CreateStringAttribute(value)
            
            // Create the string type (array of i8)
            let i8Type = context.CreateIntegerType(8)
            let arraySize = uint32 (value.Length + 1) // +1 for null terminator
            let arrayType = mlirArrayTypeGet(i8Type, arraySize)
            
            // Create the global operation
            let globalName = $"__str_{globalStrings.Count}"
            let nameAttr = context.CreateStringAttribute(globalName)
            let constAttr = context.CreateBoolAttribute(true)
            let valueAttr = context.CreateStringAttribute(value)
            
            let attributes = [|
                ("sym_name", nameAttr)
                ("constant", constAttr)
                ("value", valueAttr)
            |]
            
            let globalOp = mlirOperationCreate(
                "llvm.global",
                location,
                0u,
                [||],
                0u,
                [||],
                uint32 attributes.Length,
                [| for (name, attr) in attributes -> createNamedAttribute(name, attr) |],
                0u,
                [||])
                
            // Store for reuse
            globalStrings.[value] <- globalOp
            
            mlirLocationDestroy(location)
            globalOp
            
    // Helper to create an address-of operation for a global
    let createAddressOf (block: MLIRBlock) (globalOp: nativeint) =
        let location = context.CreateUnknownLocation()
        
        // Get the global name
        let nameAttr = mlirOperationGetAttributeByName(globalOp, "sym_name")
        let globalName = mlirStringAttributeGetValue(nameAttr)
        
        // Create the LLVM address-of operation
        let addrType = createPointerType(context.CreateIntegerType(8))
        let nameRefAttr = context.CreateStringAttribute(globalName)
        
        let attributes = [|
            ("global_name", nameRefAttr)
        |]
        
        let addrOp = mlirOperationCreate(
            "llvm.address_of",
            location,
            1u,
            [| addrType |],
            0u,
            [||],
            uint32 attributes.Length,
            [| for (name, attr) in attributes -> createNamedAttribute(name, attr) |],
            0u,
            [||])
            
        // Add to block
        block.AppendOperation(addrOp)
        
        mlirLocationDestroy(location)
        mlirOperationGetResult(addrOp, 0u)

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
        
        // Create function operation and register it in the module
        let i32Type = context.CreateIntegerType(32)
        let (functionOp, entryBlock) = createFunctionOp functionName i32Type [||]
        
        // Process the function body using reflection
        try
            let exprProperty = bindingType.GetProperty("Expr")
            let expr = exprProperty.GetValue(binding)
            this.ConvertExpression(expr, entryBlock) 
            
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
                    
                    if getIdentText ident = "printf" || getIdentText ident = "printfn" then
                        this.ConvertPrintf(entryBlock, arg)
            
            // Add a return operation to the function body
            let returnValue = this.CreateConstantInt(entryBlock, 0)
            this.CreateReturnOp(entryBlock, [| returnValue |])
        with 
        | ex -> 
            printfn "Could not process function body: %s" ex.Message
            
            // Add a default return operation
            let returnValue = this.CreateConstantInt(entryBlock, 0)
            this.CreateReturnOp(entryBlock, [| returnValue |])
        
        // Add the function operation to the module
        let moduleOp = mlirModule.GetOperation()
        let moduleBlock = mlirRegionGetFirstBlock(moduleOp)
        mlirBlockAppendOwnedOperation(moduleBlock, functionOp)

    // Create a constant integer value
    member this.CreateConstantInt(block: MLIRBlock, value: int) =
        let location = context.CreateUnknownLocation()
        let i32Type = context.CreateIntegerType(32)
        let valueAttr = context.CreateIntegerAttribute(i32Type, int64 value)
        
        let constOp = mlirOperationCreate(
            "llvm.constant",
            location,
            1u,
            [| i32Type |],
            0u,
            [||],
            1u,
            [| createNamedAttribute("value", valueAttr) |],
            0u,
            [||])
            
        block.AppendOperation(constOp)
        
        mlirLocationDestroy(location)
        mlirOperationGetResult(constOp, 0u)
    
    // Create a return operation
    member this.CreateReturnOp(block: MLIRBlock, values: nativeint[]) =
        let location = context.CreateUnknownLocation()
        
        let returnOp = mlirOperationCreate(
            "func.return",
            location,
            0u,
            [||],
            uint32 values.Length,
            values,
            0u,
            [||],
            0u,
            [||])
            
        block.AppendOperation(returnOp)
        mlirLocationDestroy(location)
        returnOp

    member this.ConvertExpression(expr: obj, block: MLIRBlock) =
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
                
                // Create the MLIR blocks for the loop
                let headerBlock = MLIRBlock.Create()
                let bodyBlock = MLIRBlock.Create()
                let exitBlock = MLIRBlock.Create()
                
                // Branch to the header block
                this.CreateBranchOp(block, headerBlock.Handle, [||])
                
                // Add the header block to the region
                block.Region.AddBlock(headerBlock)
                
                // Convert the condition expression
                let condValue = this.ConvertConditionExpression(condition, headerBlock)
                
                // Create conditional branch
                this.CreateCondBranchOp(headerBlock, condValue, bodyBlock.Handle, exitBlock.Handle)
                
                // Add and populate the body block
                block.Region.AddBlock(bodyBlock)
                this.ConvertExpression(body, bodyBlock)
                this.CreateBranchOp(bodyBlock, headerBlock.Handle, [||])
                
                // Add the exit block
                block.Region.AddBlock(exitBlock)
            with ex -> printfn "  Error processing while loop: %s" ex.Message
            
        | "SynExpr+Sequential" ->
            // Handle sequence of expressions
            try
                let expr1Prop = exprType.GetProperty("Expr1")
                let expr2Prop = exprType.GetProperty("Expr2")
                
                let expr1 = expr1Prop.GetValue(expr)
                let expr2 = expr2Prop.GetValue(expr)
                
                this.ConvertExpression(expr1, block)
                this.ConvertExpression(expr2, block)
            with ex -> printfn "  Error processing sequence: %s" ex.Message
            
        | "SynExpr+App" ->
            // Handle function application
            this.ConvertApp(expr, block)
            
        | "SynExpr+LongIdentSet" ->
            // Handle assignment (x <- x + 1)
            printfn "  Found assignment"
            try
                let lidProp = exprType.GetProperty("LongIdent")
                let exprProp = exprType.GetProperty("Expr")
                
                let lid = lidProp.GetValue(expr)
                let valExpr = exprProp.GetValue(expr)
                
                // Get variable name from LongIdent
                let varName = this.ExtractQualifiedName(lid)
                
                // Convert the right-hand side expression
                let rhs = this.ConvertExpressionToValue(valExpr, block)
                
                // Store the value to the variable
                this.CreateStoreOp(block, rhs, varName)
            with ex -> printfn "  Error processing assignment: %s" ex.Message
            
        | "SynExpr+Ident" ->
            // Handle identifier
            printfn "  Found identifier"
            try
                let identProp = exprType.GetProperty("Ident")
                let ident = identProp.GetValue(expr)
                let name = getIdentText ident
                
                // Load the value of the identifier
                this.CreateLoadOp(block, name)
            with ex -> printfn "  Error processing identifier: %s" ex.Message
            
        | _ ->
            printfn "  Unhandled expression: %s" exprType.Name
    
    // Convert a condition expression to a boolean value
    member this.ConvertConditionExpression(expr: obj, block: MLIRBlock) =
        // This is a simplified implementation
        // In a full implementation, you would handle different condition expression types
        let i1Type = context.CreateIntegerType(1)
        let trueValue = this.CreateConstantInt(block, 1)
        
        // For now, just return a constant true
        trueValue
    
    // Convert an expression to a value
    member this.ConvertExpressionToValue(expr: obj, block: MLIRBlock) =
        // This is a simplified implementation
        // In a full implementation, you would handle different expression types
        let i32Type = context.CreateIntegerType(32)
        let value = this.CreateConstantInt(block, 42)
        
        // For now, just return a constant value
        value
    
    // Create a branch operation
    member this.CreateBranchOp(block: MLIRBlock, targetBlock: nativeint, operands: nativeint[]) =
        let location = context.CreateUnknownLocation()
        
        let branchOp = mlirOperationCreate(
            "cf.br",
            location,
            0u,
            [||],
            uint32 operands.Length,
            operands,
            1u,
            [| createNamedAttribute("dest", mlirBlockAttributeGet(targetBlock)) |],
            0u,
            [||])
            
        block.AppendOperation(branchOp)
        mlirLocationDestroy(location)
        branchOp
    
    // Create a conditional branch operation
    member this.CreateCondBranchOp(block: MLIRBlock, condition: nativeint, trueBlock: nativeint, falseBlock: nativeint) =
        let location = context.CreateUnknownLocation()
        
        let condBranchOp = mlirOperationCreate(
            "cf.cond_br",
            location,
            0u,
            [||],
            1u,
            [| condition |],
            2u,
            [| 
                createNamedAttribute("trueDest", mlirBlockAttributeGet(trueBlock))
                createNamedAttribute("falseDest", mlirBlockAttributeGet(falseBlock))
            |],
            0u,
            [||])
            
        block.AppendOperation(condBranchOp)
        mlirLocationDestroy(location)
        condBranchOp
    
    // Create a store operation
    member this.CreateStoreOp(block: MLIRBlock, value: nativeint, varName: string) =
        let location = context.CreateUnknownLocation()
        
        // In a real implementation, you would create a proper variable address
        // For now, we'll just print a message
        printfn "  Would store value to variable: %s" varName
        
        mlirLocationDestroy(location)
        nativeint 0
    
    // Create a load operation
    member this.CreateLoadOp(block: MLIRBlock, varName: string) =
        let location = context.CreateUnknownLocation()
        
        // In a real implementation, you would create a proper variable address and load
        // For now, we'll just return a constant value
        let value = this.CreateConstantInt(block, 42)
        printfn "  Would load value from variable: %s" varName
        
        mlirLocationDestroy(location)
        value
    
    // Convert a function application expression (SynExpr+App)
    member this.ConvertApp(expr: obj, block: MLIRBlock) =
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
                    this.ConvertPrintf(block, arg)
                | _ ->
                    // General function call - extract arguments and convert
                    let args = this.ExtractArguments(arg)
                    this.ConvertFunctionCall(block, funcName, args)
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
    
    // Convert a function call to MLIR operations
    member this.ConvertFunctionCall(block: MLIRBlock, funcName: string, args: obj list) =
        match funcName with
        | "Time.currentUnixTimestamp" ->
            printfn "Found call to Alloy Time.currentUnixTimestamp"
            // Generate: %result = llvm.call @Alloy_Time_currentUnixTimestamp() : () -> i64
            this.GenerateExternalFunctionCall(block, "Alloy_Time_currentUnixTimestamp", args, context.CreateIntegerType(64))
            
        | "Time.sleep" ->
            printfn "Found call to Alloy Time.sleep"
            // Generate: llvm.call @Alloy_Time_sleep(%arg) : (i32) -> ()
            this.GenerateExternalFunctionCall(block, "Alloy_Time_sleep", args, nativeint 0)
            
        | "Time.now" ->
            printfn "Found call to Alloy Time.now"
            // Generate: %result = llvm.call @Alloy_Time_now() : () -> Alloy_Time_DateTime
            this.GenerateExternalFunctionCall(block, "Alloy_Time_now", args, nativeint 0) // Placeholder for DateTime type
            
        | "String.concat" | "String.concat3" ->
            printfn "Found call to string concatenation: %s" funcName
            // Generate appropriate string concatenation MLIR
            this.GenerateExternalFunctionCall(block, "Alloy_String_concat", args, createPointerType(context.CreateIntegerType(8)))
            
        | _ ->
            // Handle other functions
            printfn "Found call to: %s with %d arguments" funcName args.Length
            this.GenerateExternalFunctionCall(block, funcName, args, context.CreateIntegerType(32))
    
    // Helper to convert printf expressions to MLIR operations
    member this.ConvertPrintf(block: MLIRBlock, arg: obj) =
        try
            let argType = arg.GetType()
            if argType.Name = "SynExpr+Const" then
                let constProperty = argType.GetProperty("Constant")
                let constant = constProperty.GetValue(arg)
                let constType = constant.GetType()
                
                if constType.Name = "SynConst+String" then
                    let stringProperty = constType.GetProperty("text")
                    let formatString = stringProperty.GetValue(constant) :?> string
                    printfn "Printf with format string: %s" formatString
                    
                    // Create a global string constant
                    let globalOp = createGlobalString formatString
                    
                    // Get the address of the global string
                    let strPtr = createAddressOf block globalOp
                    
                    // Call printf
                    let i32Type = context.CreateIntegerType(32)
                    this.GenerateExternalFunctionCall(block, "printf", [arg], i32Type, [|strPtr|])
        with 
        | ex -> printfn "Could not extract printf argument: %s" ex.Message
    
    // Helper to generate external function calls
    member this.GenerateExternalFunctionCall(block: MLIRBlock, funcName: string, args: obj list, returnType: nativeint, ?mlirArgs: nativeint[]) =
        let location = context.CreateUnknownLocation()
        
        // Determine if we need to declare the function
        let funcOp =
            if functionOps.ContainsKey(funcName) then
                functionOps.[funcName]
            else
                // In a real implementation, we would create the correct parameter types
                // For now, we'll use a simplified approach
                let paramTypes = 
                    match mlirArgs with
                    | Some args -> Array.map (fun _ -> context.CreateIntegerType(32)) args
                    | None -> [||]
                
                let (op, _) = createFunctionOp funcName returnType paramTypes
                op
        
        // Create the call operation
        let argValues = 
            match mlirArgs with
            | Some args -> args
            | None -> 
                // In a real implementation, we would convert the F# arguments to MLIR values
                // For now, we'll just create constant values
                [| for i in 0..args.Length-1 -> this.CreateConstantInt(block, i) |]
        
        let callOp = mlirOperationCreate(
            "func.call",
            location,
            if returnType <> nativeint 0 then 1u else 0u,
            if returnType <> nativeint 0 then [|returnType|] else [||],
            uint32 argValues.Length,
            argValues,
            1u,
            [| createNamedAttribute("callee", context.CreateStringAttribute(funcName)) |],
            0u,
            [||])
            
        block.AppendOperation(callOp)
        
        mlirLocationDestroy(location)
        
        // Return the result if there is one
        if returnType <> nativeint 0 then
            mlirOperationGetResult(callOp, 0u)
        else
            nativeint 0