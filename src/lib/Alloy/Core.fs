namespace Alloy.Core

// Import core fsil functionality
open Fsil

/// <summary>
/// Core operations and functions for the Alloy library, providing essential functionality
/// through statically resolved type parameters (SRTP) for maximum performance.
/// </summary>
[<AutoOpen>]
module Core =
    /// <summary>
    /// Applies a function to each element of a collection.
    /// </summary>
    /// <param name="f">The function to apply to each element.</param>
    /// <param name="x">The collection to iterate over.</param>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <typeparam name="Collection">The type of the collection.</typeparam>
    let inline iter f x = iter f x 

    /// <summary>
    /// Applies a function with the index to each element of a collection.
    /// </summary>
    /// <param name="f">The function to apply to each element index pair.</param>
    /// <param name="x">The collection to iterate over.</param>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <typeparam name="Collection">The type of the collection.</typeparam>
    let inline iteri f x = iteri f x

    /// <summary>
    /// Maps a function over a collection, producing a new collection.
    /// </summary>
    /// <param name="f">The mapping function to apply to each element.</param>
    /// <param name="x">The source collection.</param>
    /// <typeparam name="T">The type of elements in the source collection.</typeparam>
    /// <typeparam name="U">The type of elements in the result collection.</typeparam>
    /// <typeparam name="Collection">The type of the collection.</typeparam>
    /// <returns>A new collection with mapped elements.</returns>
    let inline map f x = map f x

    /// <summary>
    /// Maps a function with index over a collection, producing a new collection.
    /// </summary>
    /// <param name="f">The mapping function to apply to each element index pair.</param>
    /// <param name="x">The source collection.</param>
    /// <typeparam name="T">The type of elements in the source collection.</typeparam>
    /// <typeparam name="U">The type of elements in the result collection.</typeparam>
    /// <typeparam name="Collection">The type of the collection.</typeparam>
    /// <returns>A new collection with mapped elements.</returns>
    let inline mapi f x = mapi f x

    /// <summary>
    /// Gets the length of a collection.
    /// </summary>
    /// <param name="source">The collection to get the length of.</param>
    /// <typeparam name="Collection">The type of the collection.</typeparam>
    /// <returns>The number of elements in the collection.</returns>
    let inline len source = len source

    /// <summary>
    /// Gets the zero value for a type.
    /// </summary>
    /// <typeparam name="a">The type for which to get the zero value. Must provide a static Zero member.</typeparam>
    /// <returns>The zero value for the specified type.</returns>
    let inline zero<'a when 'a: (static member Zero: 'a)> = Abstract.zero<'a>

    /// <summary>
    /// Gets the one/unit value for a type.
    /// </summary>
    /// <typeparam name="a">The type for which to get the one value. Must provide a static One member.</typeparam>
    /// <returns>The one/unit value for the specified type.</returns>
    let inline one<'a when 'a: (static member One: 'a)> = Abstract.one<'a>

    /// <summary>
    /// Gets the default value for a type.
    /// </summary>
    /// <typeparam name="t">The type for which to get the default value. Must provide a static Default member.</typeparam>
    /// <returns>The default value for the specified type.</returns>
    let inline default_value<'t when (^t or Internal.Default): (static member Default: (^t -> unit) -> ^t)> = 
        Abstract._default<'t>

    /// <summary>
    /// Uses a fallback function if a value is None.
    /// </summary>
    /// <param name="f">The fallback function to generate a value if None.</param>
    /// <param name="x">The option value to check.</param>
    /// <typeparam name="T">The type of the value in the option.</typeparam>
    /// <returns>The value if Some, or the result of calling the fallback function if None.</returns>
    let inline default_with f x = default_with f x

    /// <summary>
    /// Converts a value to string.
    /// </summary>
    /// <param name="x">The value to convert.</param>
    /// <typeparam name="T">The type of the value to convert.</typeparam>
    /// <returns>A string representation of the value.</returns>
    let inline string x = string x

    /// <summary>
    /// Print a value to stdout.
    /// </summary>
    /// <param name="x">The value to print.</param>
    /// <typeparam name="T">The type of the value to print.</typeparam>
    let inline print x = print x

    // --------------------------------------------------
    // Option functions reexported from fsil
    // --------------------------------------------------

    /// <summary>
    /// Checks if an option contains a value.
    /// </summary>
    /// <param name="x">The option to check.</param>
    /// <typeparam name="T">The type of the value in the option.</typeparam>
    /// <returns>True if the option is Some, otherwise false.</returns>
    let inline is_some x = is_some x

    /// <summary>
    /// Checks if an option is None.
    /// </summary>
    /// <param name="x">The option to check.</param>
    /// <typeparam name="T">The type of the value in the option.</typeparam>
    /// <returns>True if the option is None, otherwise false.</returns>
    let inline is_none x = is_none x

    /// <summary>
    /// Gets the value from an option, throws if None.
    /// </summary>
    /// <param name="x">The option from which to get the value.</param>
    /// <typeparam name="T">The type of the value in the option.</typeparam>
    /// <returns>The value contained in the option.</returns>
    /// <exception cref="System.Exception">Thrown when the option is None.</exception>
    let inline value x = value x

    /// <summary>
    /// Creates a None option.
    /// </summary>
    /// <typeparam name="a">The type of the option value. Must provide a static None member.</typeparam>
    /// <returns>A None option of the specified type.</returns>
    let inline none<'a when 'a: (static member None: 'a)> = Abstract.none<'a>

    /// <summary>
    /// Wraps a value in Some.
    /// </summary>
    /// <param name="x">The value to wrap.</param>
    /// <typeparam name="T">The type of the value to wrap.</typeparam>
    /// <returns>A Some option containing the value.</returns>
    let inline some x = some x

    // --------------------------------------------------
    // Additional core functions
    // --------------------------------------------------

    /// <summary>
    /// Fold implementation for collections. This function applies an accumulator function to each element of a collection.
    /// </summary>
    /// <param name="folder">The function to update the state given the current state and element.</param>
    /// <param name="state">The initial state.</param>
    /// <param name="collection">The collection to fold over.</param>
    /// <typeparam name="State">The type of the state.</typeparam>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <typeparam name="Collection">The type of the collection.</typeparam>
    /// <returns>The final accumulated state.</returns>
    let inline fold (folder: 'State -> 'T -> 'State) (state: 'State) (collection: 'Collection) =
        ((^Collection or ^State) : (static member Fold : ('State -> 'T -> 'State) * 'State * 'Collection -> 'State)
            (folder, state, collection))

    /// <summary>
    /// Filter implementation for arrays. This function returns a new array containing only the elements that satisfy the predicate.
    /// </summary>
    /// <remarks>Note: This allocates a new array.</remarks>
    /// <param name="predicate">The function to test if an element should be included.</param>
    /// <param name="source">The source array.</param>
    /// <typeparam name="T">The type of elements in the array.</typeparam>
    /// <returns>A new array containing only the elements that satisfy the predicate.</returns>
    let inline filter (predicate: 'T -> bool) (source: 'T[]) =
        let mutable count = 0
        // First pass: count matching elements
        for i = 0 to source.Length - 1 do
            if predicate source[i] then
                count <- count + 1
        
        // Allocate result array of exact size
        let result = Array.zeroCreate count
        
        // Second pass: fill result array
        let mutable j = 0
        for i = 0 to source.Length - 1 do
            if predicate source[i] then
                result[j] <- source[i]
                j <- j + 1
        
        result

    /// <summary>
    /// Choose implementation for arrays. This function applies a chooser function to each element and collects the Some results.
    /// </summary>
    /// <remarks>Note: This allocates a new array.</remarks>
    /// <param name="chooser">The function to transform elements, potentially filtering them out.</param>
    /// <param name="source">The source array.</param>
    /// <typeparam name="T">The type of elements in the source array.</typeparam>
    /// <typeparam name="U">The type of elements in the result array.</typeparam>
    /// <returns>A new array containing the transformed elements for which the chooser function returned Some.</returns>
    let inline choose (chooser: 'T -> 'U option) (source: 'T[]) =
        // First determine exact result size
        let mutable count = 0
        for i = 0 to source.Length - 1 do
            match chooser source[i] with
            | Some _ -> count <- count + 1
            | None -> ()
        
        // Allocate result array of exact size
        let result = Array.zeroCreate count
        
        // Fill result array
        let mutable j = 0
        for i = 0 to source.Length - 1 do
            match chooser source[i] with
            | Some value -> 
                result[j] <- value
                j <- j + 1
            | None -> ()
        
        result

    /// <summary>
    /// Find an element in a collection that matches a predicate.
    /// </summary>
    /// <param name="predicate">The function to test if an element matches the criteria.</param>
    /// <param name="collection">The collection to search.</param>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <typeparam name="Collection">The type of the collection.</typeparam>
    /// <returns>The first element that satisfies the predicate.</returns>
    /// <exception cref="System.Exception">Thrown when no element satisfies the predicate.</exception>
    let inline find (predicate: 'T -> bool) (collection: 'Collection) =
        ((^Collection) : (static member Find : ('T -> bool) * 'Collection -> 'T)
            (predicate, collection))

    /// <summary>
    /// Try to find an element in a collection that matches a predicate.
    /// </summary>
    /// <param name="predicate">The function to test if an element matches the criteria.</param>
    /// <param name="collection">The collection to search.</param>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <typeparam name="Collection">The type of the collection.</typeparam>
    /// <returns>Some value if an element is found, otherwise None.</returns>
    let inline tryFind (predicate: 'T -> bool) (collection: 'Collection) =
        ((^Collection) : (static member TryFind : ('T -> bool) * 'Collection -> 'T option)
            (predicate, collection))

    /// <summary>
    /// Generic equality check.
    /// </summary>
    /// <param name="a">The first value to compare.</param>
    /// <param name="b">The second value to compare.</param>
    /// <typeparam name="T">The type of values to compare.</typeparam>
    /// <returns>True if the values are equal, otherwise false.</returns>
    let inline equals (a: 'T) (b: 'T) : bool =
        ((^T) : (static member Equals : 'T * 'T -> bool) (a, b))

    /// <summary>
    /// Generic inequality check.
    /// </summary>
    /// <param name="a">The first value to compare.</param>
    /// <param name="b">The second value to compare.</param>
    /// <typeparam name="T">The type of values to compare.</typeparam>
    /// <returns>True if the values are not equal, otherwise false.</returns>
    let inline not_equals (a: 'T) (b: 'T) : bool = 
        not (equals a b)