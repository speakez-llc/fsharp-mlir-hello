namespace Alloy

/// <summary>
/// Provides zero-cost value-type option implementation that avoids heap allocations
/// through statically resolved type parameters. All functions are exposed through the 
/// AutoOpen attribute, making them accessible when opening the Alloy namespace.
/// </summary>
/// <typeparam name="T">The type of the optional value.</typeparam>
[<Struct>]
type ValueOption<'T> =
    private { 
        hasValue: bool
        value: 'T 
    }
    
    /// <summary>Gets whether this option has a value.</summary>
    member this.IsSome = this.hasValue
    
    /// <summary>Gets whether this option has no value.</summary>
    member this.IsNone = not this.hasValue
    
    /// <summary>Gets the value, or throws an exception if there is no value.</summary>
    /// <exception cref="System.Exception">Thrown when the option has no value.</exception>
    member this.Value = if this.hasValue then this.value else failwith "No value"
    
    /// <summary>Creates a ValueOption with the specified value.</summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A ValueOption containing the value.</returns>
    static member Some(value: 'T) = { hasValue = true; value = value }
    
    /// <summary>Creates a ValueOption with no value.</summary>
    /// <returns>A ValueOption with no value.</returns>
    static member None = { hasValue = false; value = Unchecked.defaultof<'T> }
    
    // Static members for SRTP support
    static member Zero = ValueOption<'T>.None
    static member DefaultValue = ValueOption<'T>.None
    
    /// <summary>Returns a string representation of the ValueOption.</summary>
    /// <returns>A string representing the option's value or None.</returns>
    override this.ToString() = 
        if this.hasValue then $"Some({this.value})" else "None"

[<AutoOpen>]
module ValueOptionPatterns =
    /// <summary>Pattern matching for ValueOption.</summary>
    let (|Some|None|) (opt: ValueOption<'T>) =
        if opt.IsSome then Some opt.Value else None

[<AutoOpen>]
module ValueOptionConstructors =
    /// <summary>Creates a Some value for ValueOption.</summary>
    let Some value = ValueOption.Some value
    
    /// <summary>Creates a None value for ValueOption.</summary>
    let None<'T> = ValueOption<'T>.None

/// <summary>
/// Functions for working with the ValueOption&lt;'T&gt; type.
/// </summary>
module ValueOption =
    /// <summary>Creates a Some value.</summary>
    let inline some v = ValueOption.Some v
    
    /// <summary>Returns the None value.</summary>
    let inline none<'T> = ValueOption<'T>.None
    
    /// <summary>Checks if an option has a value.</summary>
    let inline isSome (opt: ValueOption<'T>) = opt.IsSome
    
    /// <summary>Checks if an option has no value.</summary>
    let inline isNone (opt: ValueOption<'T>) = opt.IsNone
    
    /// <summary>Gets the value of a Some option, throws if None.</summary>
    let inline value (opt: ValueOption<'T>) = opt.Value
    
    /// <summary>Returns the value if Some, or the default value if None.</summary>
    let inline defaultValue defaultValue (opt: ValueOption<'T>) =
        if opt.IsSome then opt.Value else defaultValue
        
    /// <summary>Returns the value if Some, or calls the generator function if None.</summary>
    let inline defaultWith generator (opt: ValueOption<'T>) =
        if opt.IsSome then opt.Value else generator()
    
    /// <summary>Transforms the value inside an option.</summary>
    let inline map mapping (opt: ValueOption<'T>) =
        if opt.IsSome then ValueOption.Some(mapping opt.Value) 
        else ValueOption<_>.None
    
    /// <summary>Transforms an option with a function that returns an option.</summary>
    let inline bind binder (opt: ValueOption<'T>) =
        if opt.IsSome then binder opt.Value
        else ValueOption<_>.None
    
    /// <summary>Checks equality between two ValueOptions.</summary>
    let inline equals (a: ValueOption<'T>) (b: ValueOption<'T>) =
        if a.IsNone && b.IsNone then true
        elif a.IsSome && b.IsSome then a.Value = b.Value
        else false
        
    /// <summary>Converts a ValueOption to a standard F# option.</summary>
    let inline toOption (opt: ValueOption<'T>) =
        if opt.IsSome then Some opt.Value else None