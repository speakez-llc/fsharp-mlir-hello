#nowarn "77"
namespace Alloy.Core

/// <summary>
/// Core operations and functions for the Alloy library, providing essential functionality
/// through statically resolved type parameters (SRTP) for maximum performance.
/// This is a completely dependency-free implementation for the Fidelity framework.
/// </summary>
[<AutoOpen>]
module Core =
    
    // ===================================
    // Basic collection operations
    // ===================================
    
    /// <summary>
    /// Applies a function to each element of an array.
    /// </summary>
    /// <param name="f">The function to apply to each element.</param>
    /// <param name="array">The array to iterate over.</param>
    let inline iter (f: 'T -> unit) (array: 'T[]) =
        for i = 0 to array.Length - 1 do
            f array.[i]

    /// <summary>
    /// Applies a function with the index to each element of an array.
    /// </summary>
    /// <param name="f">The function to apply to each element index pair.</param>
    /// <param name="array">The array to iterate over.</param>
    let inline iteri (f: int -> 'T -> unit) (array: 'T[]) =
        for i = 0 to array.Length - 1 do
            f i array.[i]

    /// <summary>
    /// Maps a function over an array, producing a new array.
    /// </summary>
    /// <param name="f">The mapping function to apply to each element.</param>
    /// <param name="array">The source array.</param>
    /// <returns>A new array with mapped elements.</returns>
    let inline map (f: 'T -> 'U) (array: 'T[]) =
        Array.init array.Length (fun i -> f array.[i])

    /// <summary>
    /// Maps a function with index over an array, producing a new array.
    /// </summary>
    /// <param name="f">The mapping function to apply to each element index pair.</param>
    /// <param name="array">The source array.</param>
    /// <returns>A new array with mapped elements.</returns>
    let inline mapi (f: int -> 'T -> 'U) (array: 'T[]) =
        Array.init array.Length (fun i -> f i array.[i])

    /// <summary>
    /// Gets the length of an array.
    /// </summary>
    /// <param name="array">The array to get the length of.</param>
    /// <returns>The number of elements in the array.</returns>
    let inline len (array: 'T[]) = array.Length

    // ===================================
    // Zero/One/Default operations
    // ===================================
    
    /// <summary>
    /// Gets the zero value for a type.
    /// </summary>
    let inline zero<'T when 'T: (static member Zero: 'T)> = 
        (^T: (static member Zero: 'T) ())

    /// <summary>
    /// Gets the one/unit value for a type.
    /// </summary>
    let inline one<'T when 'T: (static member One: 'T)> = 
        (^T: (static member One: 'T) ())

    /// <summary>
    /// Gets the default value for a type.
    /// </summary>
    let inline default_value<'T> = Unchecked.defaultof<'T>

    /// <summary>
    /// Uses a fallback function if a value is None.
    /// </summary>
    /// <param name="f">The fallback function to generate a value if None.</param>
    /// <param name="x">The option value to check.</param>
    /// <returns>The value if Some, or the result of calling the fallback function if None.</returns>
    let inline default_with (f: unit -> 'T) (x: 'T option) =
        match x with
        | Some value -> value
        | None -> f ()

    /// <summary>
    /// Converts a value to string.
    /// </summary>
    /// <param name="x">The value to convert.</param>
    /// <returns>A string representation of the value.</returns>
    let inline string (x: 'T) = x.ToString()

    /// <summary>
    /// Print a value to stdout.
    /// </summary>
    /// <param name="x">The value to print.</param>
    let inline print (x: 'T) = printf "%A" x

    // ===================================
    // Option functions
    // ===================================

    /// <summary>
    /// Checks if an option contains a value.
    /// </summary>
    /// <param name="x">The option to check.</param>
    /// <returns>True if the option is Some, otherwise false.</returns>
    let inline is_some (x: 'T option) =
        match x with
        | Some _ -> true
        | None -> false

    /// <summary>
    /// Checks if an option is None.
    /// </summary>
    /// <param name="x">The option to check.</param>
    /// <returns>True if the option is None, otherwise false.</returns>
    let inline is_none (x: 'T option) =
        match x with
        | Some _ -> false
        | None -> true

    /// <summary>
    /// Gets the value from an option, throws if None.
    /// </summary>
    /// <param name="x">The option from which to get the value.</param>
    /// <returns>The value contained in the option.</returns>
    /// <exception cref="System.Exception">Thrown when the option is None.</exception>
    let inline value (x: 'T option) =
        match x with
        | Some v -> v
        | None -> failwith "Option was None"

    /// <summary>
    /// Creates a None option.
    /// </summary>
    let inline none<'T> : 'T option = None

    /// <summary>
    /// Wraps a value in Some.
    /// </summary>
    /// <param name="x">The value to wrap.</param>
    /// <returns>A Some option containing the value.</returns>
    let inline some (x: 'T) = Some x

    // ===================================
    // Collection operations with explicit implementations
    // ===================================

    /// <summary>
    /// Fold implementation for arrays.
    /// </summary>
    /// <param name="folder">The function to update the state given the current state and element.</param>
    /// <param name="state">The initial state.</param>
    /// <param name="array">The array to fold over.</param>
    /// <returns>The final accumulated state.</returns>
    let inline fold (folder: 'State -> 'T -> 'State) (state: 'State) (array: 'T[]) =
        let mutable acc = state
        for i = 0 to array.Length - 1 do
            acc <- folder acc array.[i]
        acc

    /// <summary>
    /// Filter implementation for arrays.
    /// </summary>
    /// <param name="predicate">The function to test if an element should be included.</param>
    /// <param name="source">The source array.</param>
    /// <returns>A new array containing only the elements that satisfy the predicate.</returns>
    let inline filter (predicate: 'T -> bool) (source: 'T[]) =
        let mutable count = 0
        // First pass: count matching elements
        for i = 0 to source.Length - 1 do
            if predicate source.[i] then
                count <- count + 1
        
        // Allocate result array of exact size
        let result = Array.zeroCreate count
        
        // Second pass: fill result array
        let mutable j = 0
        for i = 0 to source.Length - 1 do
            if predicate source.[i] then
                result.[j] <- source.[i]
                j <- j + 1
        
        result

    /// <summary>
    /// Choose implementation for arrays.
    /// </summary>
    /// <param name="chooser">The function to transform elements, potentially filtering them out.</param>
    /// <param name="source">The source array.</param>
    /// <returns>A new array containing the transformed elements for which the chooser function returned Some.</returns>
    let inline choose (chooser: 'T -> 'U option) (source: 'T[]) =
        // First determine exact result size
        let mutable count = 0
        for i = 0 to source.Length - 1 do
            match chooser source.[i] with
            | Some _ -> count <- count + 1
            | None -> ()
        
        // Allocate result array of exact size
        let result = Array.zeroCreate count
        
        // Fill result array
        let mutable j = 0
        for i = 0 to source.Length - 1 do
            match chooser source.[i] with
            | Some value -> 
                result.[j] <- value
                j <- j + 1
            | None -> ()
        
        result

    /// <summary>
    /// Find an element in an array that matches a predicate.
    /// </summary>
    /// <param name="predicate">The function to test if an element matches the criteria.</param>
    /// <param name="array">The array to search.</param>
    /// <returns>The first element that satisfies the predicate.</returns>
    /// <exception cref="System.Exception">Thrown when no element satisfies the predicate.</exception>
    let inline find (predicate: 'T -> bool) (array: 'T[]) =
        let mutable found = false
        let mutable result = Unchecked.defaultof<'T>
        let mutable i = 0
        while not found && i < array.Length do
            if predicate array.[i] then
                found <- true
                result <- array.[i]
            i <- i + 1
        
        if found then result
        else failwith "No element found matching the predicate"

    /// <summary>
    /// Try to find an element in an array that matches a predicate.
    /// </summary>
    /// <param name="predicate">The function to test if an element matches the criteria.</param>
    /// <param name="array">The array to search.</param>
    /// <returns>Some value if an element is found, otherwise None.</returns>
    let inline tryFind (predicate: 'T -> bool) (array: 'T[]) =
        let mutable found = false
        let mutable result = Unchecked.defaultof<'T>
        let mutable i = 0
        while not found && i < array.Length do
            if predicate array.[i] then
                found <- true
                result <- array.[i]
            i <- i + 1
        
        if found then Some result
        else None

    /// <summary>
    /// Generic equality check.
    /// </summary>
    /// <param name="a">The first value to compare.</param>
    /// <param name="b">The second value to compare.</param>
    /// <returns>True if the values are equal, otherwise false.</returns>
    let inline equals (a: 'T) (b: 'T) : bool = a = b

    /// <summary>
    /// Generic inequality check.
    /// </summary>
    /// <param name="a">The first value to compare.</param>
    /// <param name="b">The second value to compare.</param>
    /// <returns>True if the values are not equal, otherwise false.</returns>
    let inline not_equals (a: 'T) (b: 'T) : bool = a <> b