#nowarn "64" 
#nowarn "77" 
namespace Alloy

open FSharp.NativeInterop

/// <summary>
/// Pure native math implementations with zero dependencies
/// All functions work directly with binary representations and bit manipulation
/// </summary>
[<RequireQualifiedAccess>]
module NativeMath =
    
    // ===================================
    // Constants and helper functions
    // ===================================
    
    /// <summary>
    /// IEEE 754 double precision constants
    /// </summary>
    module DoubleConstants =
        let [<Literal>] SIGN_MASK = 0x8000000000000000UL
        let [<Literal>] EXPONENT_MASK = 0x7FF0000000000000UL
        let [<Literal>] MANTISSA_MASK = 0x000FFFFFFFFFFFFFUL
        let [<Literal>] EXPONENT_BIAS = 1023
        let [<Literal>] MANTISSA_BITS = 52
        let [<Literal>] EXPONENT_SHIFT = 52
    
    /// <summary>
    /// IEEE 754 single precision constants  
    /// </summary>
    module FloatConstants =
        let [<Literal>] SIGN_MASK = 0x80000000u
        let [<Literal>] EXPONENT_MASK = 0x7F800000u
        let [<Literal>] MANTISSA_MASK = 0x007FFFFFu
        let [<Literal>] EXPONENT_BIAS = 127
        let [<Literal>] MANTISSA_BITS = 23
        let [<Literal>] EXPONENT_SHIFT = 23
    
    /// <summary>
    /// Convert double to its bit representation using F# NativePtr
    /// </summary>
    let inline doubleToBits (value: float) : uint64 =
        let mutable v = value
        let ptr = &&v |> NativePtr.toNativeInt |> NativePtr.ofNativeInt<uint64>
        NativePtr.read ptr
    
    /// <summary>
    /// Convert bit representation to double using F# NativePtr
    /// </summary>
    let inline bitsToDouble (bits: uint64) : float =
        let mutable v = bits
        let ptr = &&v |> NativePtr.toNativeInt |> NativePtr.ofNativeInt<float>
        NativePtr.read ptr
    
    /// <summary>
    /// Convert float to its bit representation using F# NativePtr
    /// </summary>
    let inline floatToBits (value: float32) : uint32 =
        let mutable v = value
        let ptr = &&v |> NativePtr.toNativeInt |> NativePtr.ofNativeInt<uint32>
        NativePtr.read ptr
    
    /// <summary>
    /// Convert bit representation to float using F# NativePtr
    /// </summary>
    let inline bitsToFloat (bits: uint32) : float32 =
        let mutable v = bits
        let ptr = &&v |> NativePtr.toNativeInt |> NativePtr.ofNativeInt<float32>
        NativePtr.read ptr
    
    // ===================================
    // Power functions - dependency-free
    // ===================================
    
    /// <summary>
    /// Fast integer power using exponentiation by squaring
    /// </summary>
    let rec intPower (base': int) (exp: int) : int =
        if exp = 0 then 1
        elif exp = 1 then base'
        elif exp % 2 = 0 then
            let half = intPower base' (exp / 2)
            half * half
        else
            base' * intPower base' (exp - 1)
    
    /// <summary>
    /// Fast 64-bit integer power
    /// </summary>
    let rec int64Power (base': int64) (exp: int) : int64 =
        if exp = 0 then 1L
        elif exp = 1 then base'
        elif exp % 2 = 0 then
            let half = int64Power base' (exp / 2)
            half * half
        else
            base' * int64Power base' (exp - 1)
    
    /// <summary>
    /// Decimal power (integer exponent only)
    /// </summary>
    let rec decimalPower (base': decimal) (exp: int) : decimal =
        if exp = 0 then 1M
        elif exp = 1 then base'
        elif exp > 0 then
            if exp % 2 = 0 then
                let half = decimalPower base' (exp / 2)
                half * half
            else
                base' * decimalPower base' (exp - 1)
        else
            // Negative exponent: 1 / (base^(-exp))
            1M / decimalPower base' (-exp)
    
    /// <summary>
    /// Fast floating point power for integer exponents
    /// For fractional exponents, use Taylor series or other approximations
    /// </summary>
    let floatIntPower (base': float) (exp: int) : float =
        if exp = 0 then 1.0
        elif exp = 1 then base'
        elif exp > 0 then
            let rec loop acc b e =
                if e = 0 then acc
                elif e % 2 = 0 then loop acc (b * b) (e / 2)
                else loop (acc * b) b (e - 1)
            loop 1.0 base' exp
        else
            // Negative exponent
            1.0 / floatIntPower base' (-exp)
    
    // ===================================
    // Ceiling/Floor/Round - bit manipulation
    // ===================================
    
    /// <summary>
    /// Fast ceiling function using bit manipulation
    /// </summary>
    let ceiling (value: float) : float =
        if value <> value then value // NaN
        else
            let bits = doubleToBits value
            let exponent = int ((bits &&& DoubleConstants.EXPONENT_MASK) >>> DoubleConstants.EXPONENT_SHIFT) - DoubleConstants.EXPONENT_BIAS
            
            if exponent >= DoubleConstants.MANTISSA_BITS then
                // Already an integer or too large
                value
            elif exponent < 0 then
                // Absolute value < 1
                if bits &&& DoubleConstants.SIGN_MASK = 0UL then 1.0 else 0.0
            else
                // Extract integer part
                let mantissaShift = DoubleConstants.MANTISSA_BITS - exponent
                let integerMask = ~~~((1UL <<< mantissaShift) - 1UL)
                let integerPart = bits &&& integerMask
                
                if integerPart = bits then
                    // Already an integer
                    value
                else
                    // Has fractional part
                    let result = bitsToDouble integerPart
                    if bits &&& DoubleConstants.SIGN_MASK = 0UL then
                        result + 1.0  // Positive: round up
                    else
                        result        // Negative: round toward zero
    
    /// <summary>
    /// Fast floor function using bit manipulation
    /// </summary>
    let floor (value: float) : float =
        if value <> value then value // NaN
        else
            let bits = doubleToBits value
            let exponent = int ((bits &&& DoubleConstants.EXPONENT_MASK) >>> DoubleConstants.EXPONENT_SHIFT) - DoubleConstants.EXPONENT_BIAS
            
            if exponent >= DoubleConstants.MANTISSA_BITS then
                // Already an integer or too large
                value
            elif exponent < 0 then
                // Absolute value < 1
                if bits &&& DoubleConstants.SIGN_MASK = 0UL then 0.0 else -1.0
            else
                // Extract integer part
                let mantissaShift = DoubleConstants.MANTISSA_BITS - exponent
                let integerMask = ~~~((1UL <<< mantissaShift) - 1UL)
                let integerPart = bits &&& integerMask
                
                if integerPart = bits then
                    // Already an integer
                    value
                else
                    // Has fractional part
                    let result = bitsToDouble integerPart
                    if bits &&& DoubleConstants.SIGN_MASK = 0UL then
                        result        // Positive: round toward zero
                    else
                        result - 1.0  // Negative: round down
    
    /// <summary>
    /// Fast round function (banker's rounding)
    /// </summary>
    let round (value: float) : float =
        let f = floor value
        let diff = value - f
        
        if diff < 0.5 then
            f
        elif diff > 0.5 then
            f + 1.0
        else
            // Exactly 0.5 - use banker's rounding (round to even)
            if (int64 f) % 2L = 0L then f else f + 1.0
    
    // ===================================
    // Single precision versions
    // ===================================
    
    /// <summary>
    /// Single precision ceiling
    /// </summary>
    let ceilingf (value: float32) : float32 =
        if value <> value then value // NaN
        else
            let bits = floatToBits value
            let exponent = int ((bits &&& FloatConstants.EXPONENT_MASK) >>> FloatConstants.EXPONENT_SHIFT) - FloatConstants.EXPONENT_BIAS
            
            if exponent >= FloatConstants.MANTISSA_BITS then
                value
            elif exponent < 0 then
                if bits &&& FloatConstants.SIGN_MASK = 0u then 1.0f else 0.0f
            else
                let mantissaShift = FloatConstants.MANTISSA_BITS - exponent
                let integerMask = ~~~((1u <<< mantissaShift) - 1u)
                let integerPart = bits &&& integerMask
                
                if integerPart = bits then
                    value
                else
                    let result = bitsToFloat integerPart
                    if bits &&& FloatConstants.SIGN_MASK = 0u then
                        result + 1.0f
                    else
                        result
    
    /// <summary>
    /// Single precision floor
    /// </summary>
    let floorf (value: float32) : float32 =
        if value <> value then value // NaN
        else
            let bits = floatToBits value
            let exponent = int ((bits &&& FloatConstants.EXPONENT_MASK) >>> FloatConstants.EXPONENT_SHIFT) - FloatConstants.EXPONENT_BIAS
            
            if exponent >= FloatConstants.MANTISSA_BITS then
                value
            elif exponent < 0 then
                if bits &&& FloatConstants.SIGN_MASK = 0u then 0.0f else -1.0f
            else
                let mantissaShift = FloatConstants.MANTISSA_BITS - exponent
                let integerMask = ~~~((1u <<< mantissaShift) - 1u)
                let integerPart = bits &&& integerMask
                
                if integerPart = bits then
                    value
                else
                    let result = bitsToFloat integerPart
                    if bits &&& FloatConstants.SIGN_MASK = 0u then
                        result
                    else
                        result - 1.0f
    
    /// <summary>
    /// Single precision round
    /// </summary>
    let roundf (value: float32) : float32 =
        let f = floorf value
        let diff = value - f
        
        if diff < 0.5f then
            f
        elif diff > 0.5f then
            f + 1.0f
        else
            // Banker's rounding
            if (int f) % 2 = 0 then f else f + 1.0f
    
    // ===================================
    // Decimal versions (simpler - no bit manipulation needed)
    // ===================================
    
    /// <summary>
    /// Decimal ceiling
    /// </summary>
    let ceilingDecimal (value: decimal) : decimal =
        let truncated = decimal (int64 value)
        if value = truncated then
            value
        elif value > 0M then
            truncated + 1M
        else
            truncated
    
    /// <summary>
    /// Decimal floor  
    /// </summary>
    let floorDecimal (value: decimal) : decimal =
        let truncated = decimal (int64 value)
        if value = truncated then
            value
        elif value > 0M then
            truncated
        else
            truncated - 1M
    
    /// <summary>
    /// Decimal round
    /// </summary>
    let roundDecimal (value: decimal) : decimal =
        let truncated = decimal (int64 value)
        let fractional = value - truncated
        
        if fractional = 0M then
            value
        elif fractional > 0.5M || fractional < -0.5M then
            if value > 0M then truncated + 1M else truncated - 1M
        elif fractional = 0.5M || fractional = -0.5M then
            // Banker's rounding
            if (int64 truncated) % 2L = 0L then truncated 
            else if value > 0M then truncated + 1M else truncated - 1M
        else
            truncated

/// <summary>
/// Provides zero-cost numeric operations through statically resolved type parameters.
/// All functions are exposed through the AutoOpen attribute, making them accessible
/// when opening the Alloy namespace. NOW COMPLETELY DEPENDENCY-FREE.
/// </summary>
[<AutoOpen>]
module Numerics =
    // ----------------------
    // Type implementations
    // ----------------------
    
    /// <summary>
    /// Provides basic numeric operations for primitive types
    /// </summary>
    [<AbstractClass; Sealed>]
    type BasicOps =
        // Add implementations
        static member inline Add(a: int, b: int) = a + b
        static member inline Add(a: float, b: float) = a + b
        static member inline Add(a: int64, b: int64) = a + b
        static member inline Add(a: uint64, b: uint64) = a + b
        static member inline Add(a: float32, b: float32) = a + b
        static member inline Add(a: decimal, b: decimal) = a + b
        static member inline Add(a: byte, b: byte) = a + b
        static member inline Add(a: uint32, b: uint32) = a + b
        static member inline Add(a: int16, b: int16) = a + b
        static member inline Add(a: uint16, b: uint16) = a + b
        
        static member inline Add(a: string, b: string) = 
            let aChars = if isNull a then [||] else Array.init a.Length (fun i -> a.[i])
            let bChars = if isNull b then [||] else Array.init b.Length (fun i -> b.[i])
            let result = Array.zeroCreate (aChars.Length + bChars.Length)
            for i = 0 to aChars.Length - 1 do
                result.[i] <- aChars.[i]
            for i = 0 to bChars.Length - 1 do
                result.[i + aChars.Length] <- bChars.[i]
            new string(result)
            
        // Subtract implementations
        static member inline Subtract(a: int, b: int) = a - b
        static member inline Subtract(a: float, b: float) = a - b
        static member inline Subtract(a: int64, b: int64) = a - b
        static member inline Subtract(a: uint64, b: uint64) = a - b
        static member inline Subtract(a: float32, b: float32) = a - b
        static member inline Subtract(a: decimal, b: decimal) = a - b
        static member inline Subtract(a: byte, b: byte) = a - b
        static member inline Subtract(a: uint32, b: uint32) = a - b
        static member inline Subtract(a: int16, b: int16) = a - b
        static member inline Subtract(a: uint16, b: uint16) = a - b
        
        // Multiply implementations
        static member inline Multiply(a: int, b: int) = a * b
        static member inline Multiply(a: float, b: float) = a * b
        static member inline Multiply(a: int64, b: int64) = a * b
        static member inline Multiply(a: uint64, b: uint64) = a * b
        static member inline Multiply(a: float32, b: float32) = a * b
        static member inline Multiply(a: decimal, b: decimal) = a * b
        static member inline Multiply(a: byte, b: byte) = a * b
        static member inline Multiply(a: uint32, b: uint32) = a * b
        static member inline Multiply(a: int16, b: int16) = a * b
        static member inline Multiply(a: uint16, b: uint16) = a * b
        
        // Divide implementations
        static member inline Divide(a: int, b: int) = a / b
        static member inline Divide(a: float, b: float) = a / b
        static member inline Divide(a: int64, b: int64) = a / b
        static member inline Divide(a: uint64, b: uint64) = a / b
        static member inline Divide(a: float32, b: float32) = a / b
        static member inline Divide(a: decimal, b: decimal) = a / b
        static member inline Divide(a: byte, b: byte) = a / b
        static member inline Divide(a: uint32, b: uint32) = a / b
        static member inline Divide(a: int16, b: int16) = a / b
        static member inline Divide(a: uint16, b: uint16) = a / b

        // Modulo implementations
        static member inline Modulo(a: int, b: int) = a % b
        static member inline Modulo(a: float, b: float) = a % b
        static member inline Modulo(a: int64, b: int64) = a % b
        static member inline Modulo(a: uint64, b: uint64) = a % b
        static member inline Modulo(a: float32, b: float32) = a % b
        static member inline Modulo(a: decimal, b: decimal) = a % b
        static member inline Modulo(a: byte, b: byte) = a % b
        static member inline Modulo(a: uint32, b: uint32) = a % b
        static member inline Modulo(a: int16, b: int16) = a % b
        static member inline Modulo(a: uint16, b: uint16) = a % b

        // Power implementations - NOW DEPENDENCY FREE
        static member inline Power(a: int, b: int) = NativeMath.intPower a b
        static member inline Power(a: float, b: float) = NativeMath.floatIntPower a (int b) // For now, only integer exponents
        static member inline Power(a: float32, b: float32) = float32 (NativeMath.floatIntPower (float a) (int b))
        static member inline Power(a: int64, b: int) = NativeMath.int64Power a b
        static member inline Power(a: decimal, b: int) = NativeMath.decimalPower a b

        // Absolute value implementations
        static member inline Abs(a: int) = abs a
        static member inline Abs(a: float) = abs a
        static member inline Abs(a: int64) = abs a
        static member inline Abs(a: float32) = abs a
        static member inline Abs(a: decimal) = abs a
        static member inline Abs(a: int16) = abs a

        // Sign implementations
        static member inline Sign(a: int) = sign a
        static member inline Sign(a: float) = sign a
        static member inline Sign(a: int64) = sign a
        static member inline Sign(a: float32) = sign a
        static member inline Sign(a: decimal) = sign a
        static member inline Sign(a: int16) = sign a

        // Rounding implementations - NOW DEPENDENCY FREE
        static member inline Ceiling(a: float) = NativeMath.ceiling a
        static member inline Ceiling(a: decimal) = NativeMath.ceilingDecimal a
        static member inline Ceiling(a: float32) = NativeMath.ceilingf a

        static member inline Floor(a: float) = NativeMath.floor a
        static member inline Floor(a: decimal) = NativeMath.floorDecimal a
        static member inline Floor(a: float32) = NativeMath.floorf a

        static member inline Round(a: float) = NativeMath.round a
        static member inline Round(a: decimal) = NativeMath.roundDecimal a
        static member inline Round(a: float32) = NativeMath.roundf a
    
    /// <summary>
    /// Provides operations for unit-of-measure types with same measure
    /// </summary>
    [<AbstractClass; Sealed>]
    type MeasureOps =
        // Add implementations for unit-of-measure types
        static member inline AddInt(a: int<'u>, b: int<'u>) = a + b
        static member inline AddFloat(a: float<'u>, b: float<'u>) = a + b
        static member inline AddInt64(a: int64<'u>, b: int64<'u>) = a + b
        static member inline AddUInt64(a: uint64<'u>, b: uint64<'u>) = a + b
        static member inline AddFloat32(a: float32<'u>, b: float32<'u>) = a + b
        static member inline AddDecimal(a: decimal<'u>, b: decimal<'u>) = a + b
        
        // Subtract implementations for unit-of-measure types
        static member inline SubtractInt(a: int<'u>, b: int<'u>) = a - b
        static member inline SubtractFloat(a: float<'u>, b: float<'u>) = a - b
        static member inline SubtractInt64(a: int64<'u>, b: int64<'u>) = a - b
        static member inline SubtractUInt64(a: uint64<'u>, b: uint64<'u>) = a - b
        static member inline SubtractFloat32(a: float32<'u>, b: float32<'u>) = a - b
        static member inline SubtractDecimal(a: decimal<'u>, b: decimal<'u>) = a - b
        
        // Multiply: unit * scalar implementations
        static member inline MultiplyIntScalar(a: int<'u>, b: int) = a * b
        static member inline MultiplyFloatScalar(a: float<'u>, b: float) = a * b
        static member inline MultiplyInt64Scalar(a: int64<'u>, b: int64) = a * b
        static member inline MultiplyUInt64Scalar(a: uint64<'u>, b: uint64) = a * b
        static member inline MultiplyFloat32Scalar(a: float32<'u>, b: float32) = a * b
        static member inline MultiplyDecimalScalar(a: decimal<'u>, b: decimal) = a * b
        
        // Multiply: scalar * unit implementations
        static member inline MultiplyScalarInt(a: int, b: int<'u>) = a * b
        static member inline MultiplyScalarFloat(a: float, b: float<'u>) = a * b
        static member inline MultiplyScalarInt64(a: int64, b: int64<'u>) = a * b
        static member inline MultiplyScalarUInt64(a: uint64, b: uint64<'u>) = a * b
        static member inline MultiplyScalarFloat32(a: float32, b: float32<'u>) = a * b
        static member inline MultiplyScalarDecimal(a: decimal, b: decimal<'u>) = a * b
        
        // Divide: unit / scalar implementations
        static member inline DivideIntScalar(a: int<'u>, b: int) = a / b
        static member inline DivideFloatScalar(a: float<'u>, b: float) = a / b
        static member inline DivideInt64Scalar(a: int64<'u>, b: int64) = a / b
        static member inline DivideUInt64Scalar(a: uint64<'u>, b: uint64) = a / b
        static member inline DivideFloat32Scalar(a: float32<'u>, b: float32) = a / b
        static member inline DivideDecimalScalar(a: decimal<'u>, b: decimal) = a / b
        
        // Divide: scalar / unit implementations
        static member inline DivideScalarInt(a: int, b: int<'u>) = a / b
        static member inline DivideScalarFloat(a: float, b: float<'u>) = a / b
        static member inline DivideScalarInt64(a: int64, b: int64<'u>) = a / b
        static member inline DivideScalarUInt64(a: uint64, b: uint64<'u>) = a / b
        static member inline DivideScalarFloat32(a: float32, b: float32<'u>) = a / b
        static member inline DivideScalarDecimal(a: decimal, b: decimal<'u>) = a / b

        // Modulo: unit % scalar implementations
        static member inline ModuloIntScalar(a: int<'u>, b: int) = a % b
        static member inline ModuloFloatScalar(a: float<'u>, b: float) = a % b
        static member inline ModuloInt64Scalar(a: int64<'u>, b: int64) = a % b
        static member inline ModuloUInt64Scalar(a: uint64<'u>, b: uint64) = a % b
        static member inline ModuloFloat32Scalar(a: float32<'u>, b: float32) = a % b
        static member inline ModuloDecimalScalar(a: decimal<'u>, b: decimal) = a % b

        // Modulo: unit % unit implementations (same unit)
        static member inline ModuloInt(a: int<'u>, b: int<'u>) = a % b
        static member inline ModuloFloat(a: float<'u>, b: float<'u>) = a % b
        static member inline ModuloInt64(a: int64<'u>, b: int64<'u>) = a % b
        static member inline ModuloUInt64(a: uint64<'u>, b: uint64<'u>) = a % b
        static member inline ModuloFloat32(a: float32<'u>, b: float32<'u>) = a % b
        static member inline ModuloDecimal(a: decimal<'u>, b: decimal<'u>) = a % b

        // Absolute value implementations for unit-of-measure types
        static member inline AbsInt(a: int<'u>) = abs a
        static member inline AbsFloat(a: float<'u>) = abs a
        static member inline AbsInt64(a: int64<'u>) = abs a
        static member inline AbsFloat32(a: float32<'u>) = abs a
        static member inline AbsDecimal(a: decimal<'u>) = abs a

        // Sign implementations for unit-of-measure types
        static member inline SignInt(a: int<'u>) = sign a
        static member inline SignFloat(a: float<'u>) = sign a
        static member inline SignInt64(a: int64<'u>) = sign a
        static member inline SignFloat32(a: float32<'u>) = sign a
        static member inline SignDecimal(a: decimal<'u>) = sign a

        // Rounding implementations for unit-of-measure types - NOW DEPENDENCY FREE
        static member inline CeilingFloat(a: float<'u>) = LanguagePrimitives.FloatWithMeasure<'u>(NativeMath.ceiling (float a))
        static member inline CeilingDecimal(a: decimal<'u>) = LanguagePrimitives.DecimalWithMeasure<'u>(NativeMath.ceilingDecimal (decimal a))
        static member inline CeilingFloat32(a: float32<'u>) = LanguagePrimitives.Float32WithMeasure<'u>(NativeMath.ceilingf (float32 a))

        static member inline FloorFloat(a: float<'u>) = LanguagePrimitives.FloatWithMeasure<'u>(NativeMath.floor (float a))
        static member inline FloorDecimal(a: decimal<'u>) = LanguagePrimitives.DecimalWithMeasure<'u>(NativeMath.floorDecimal (decimal a))
        static member inline FloorFloat32(a: float32<'u>) = LanguagePrimitives.Float32WithMeasure<'u>(NativeMath.floorf (float32 a))

        static member inline RoundFloat(a: float<'u>) = LanguagePrimitives.FloatWithMeasure<'u>(NativeMath.round (float a))
        static member inline RoundDecimal(a: decimal<'u>) = LanguagePrimitives.DecimalWithMeasure<'u>(NativeMath.roundDecimal (decimal a))
        static member inline RoundFloat32(a: float32<'u>) = LanguagePrimitives.Float32WithMeasure<'u>(NativeMath.roundf (float32 a))
    
    /// <summary>
    /// Provides operations for unit-of-measure types with different measures
    /// </summary>
    [<AbstractClass; Sealed>]
    type MeasureMeasureOps =
        // Multiply implementations for different unit-of-measure types
        static member inline MultiplyIntUnits(a: int<'u1>, b: int<'u2>) = a * b
        static member inline MultiplyFloatUnits(a: float<'u1>, b: float<'u2>) = a * b
        static member inline MultiplyInt64Units(a: int64<'u1>, b: int64<'u2>) = a * b
        static member inline MultiplyUInt64Units(a: uint64<'u1>, b: uint64<'u2>) = a * b
        static member inline MultiplyFloat32Units(a: float32<'u1>, b: float32<'u2>) = a * b
        static member inline MultiplyDecimalUnits(a: decimal<'u1>, b: decimal<'u2>) = a * b
        
        // Divide implementations for different unit-of-measure types
        static member inline DivideIntUnits(a: int<'u1>, b: int<'u2>) = a / b
        static member inline DivideFloatUnits(a: float<'u1>, b: float<'u2>) = a / b
        static member inline DivideInt64Units(a: int64<'u1>, b: int64<'u2>) = a / b
        static member inline DivideUInt64Units(a: uint64<'u1>, b: uint64<'u2>) = a / b
        static member inline DivideFloat32Units(a: float32<'u1>, b: float32<'u2>) = a / b
        static member inline DivideDecimalUnits(a: decimal<'u1>, b: decimal<'u2>) = a / b

        // Modulo implementations for different unit-of-measure types
        static member inline ModuloIntUnits(a: int<'u1>, b: int<'u2>) = a % b
        static member inline ModuloFloatUnits(a: float<'u1>, b: float<'u2>) = a % b
        static member inline ModuloInt64Units(a: int64<'u1>, b: int64<'u2>) = a % b
        static member inline ModuloUInt64Units(a: uint64<'u1>, b: uint64<'u2>) = a % b
        static member inline ModuloFloat32Units(a: float32<'u1>, b: float32<'u2>) = a % b
        static member inline ModuloDecimalUnits(a: decimal<'u1>, b: decimal<'u2>) = a % b
    
    // ... (ArrayOps, MeasureArrayOps, MinMaxOperations remain the same) ...
    
    /// <summary>
    /// Provides collection operations for arrays of primitive types
    /// </summary>
    [<AbstractClass; Sealed>]
    type ArrayOps =
        static member inline Sum(xs: int[]) = 
            let mutable sum = 0
            for i = 0 to xs.Length - 1 do sum <- sum + xs.[i]
            sum
            
        static member inline Sum(xs: float[]) = 
            let mutable sum = 0.0
            for i = 0 to xs.Length - 1 do sum <- sum + xs.[i]
            sum
            
        static member inline Sum(xs: int64[]) = 
            let mutable sum = 0L
            for i = 0 to xs.Length - 1 do sum <- sum + xs.[i]
            sum
            
        static member inline Sum(xs: float32[]) = 
            let mutable sum = 0.0f
            for i = 0 to xs.Length - 1 do sum <- sum + xs.[i]
            sum
            
        static member inline Sum(xs: decimal[]) = 
            let mutable sum = 0M
            for i = 0 to xs.Length - 1 do sum <- sum + xs.[i]
            sum
            
        static member inline Sum(xs: byte[]) = 
            let mutable sum = 0uy
            for i = 0 to xs.Length - 1 do sum <- sum + xs.[i]
            sum
            
        static member inline Sum(xs: uint32[]) = 
            let mutable sum = 0u
            for i = 0 to xs.Length - 1 do sum <- sum + xs.[i]
            sum
            
        static member inline Sum(xs: int16[]) = 
            let mutable sum = 0s
            for i = 0 to xs.Length - 1 do sum <- sum + xs.[i]
            sum
            
        static member inline Sum(xs: uint16[]) = 
            let mutable sum = 0us
            for i = 0 to xs.Length - 1 do sum <- sum + xs.[i]
            sum
            
        static member inline Average(xs: int[]) = 
            if xs.Length = 0 then 0
            else
                let mutable sum = 0
                for i = 0 to xs.Length - 1 do sum <- sum + xs.[i]
                sum / xs.Length
                
        static member inline Average(xs: float[]) = 
            if xs.Length = 0 then 0.0
            else
                let mutable sum = 0.0
                for i = 0 to xs.Length - 1 do sum <- sum + xs.[i]
                sum / float xs.Length
                
        static member inline Average(xs: int64[]) = 
            if xs.Length = 0 then 0L
            else
                let mutable sum = 0L
                for i = 0 to xs.Length - 1 do sum <- sum + xs.[i]
                sum / int64 xs.Length
                
        static member inline Average(xs: float32[]) = 
            if xs.Length = 0 then 0.0f
            else
                let mutable sum = 0.0f
                for i = 0 to xs.Length - 1 do sum <- sum + xs.[i]
                sum / float32 xs.Length
                
        static member inline Average(xs: decimal[]) = 
            if xs.Length = 0 then 0M
            else
                let mutable sum = 0M
                for i = 0 to xs.Length - 1 do sum <- sum + xs.[i]
                sum / decimal xs.Length
                
        static member inline Average(xs: byte[]) = 
            if xs.Length = 0 then 0uy
            else
                let mutable sum = 0uy
                for i = 0 to xs.Length - 1 do sum <- sum + xs.[i]
                sum / byte xs.Length
                
        static member inline Average(xs: uint32[]) = 
            if xs.Length = 0 then 0u
            else
                let mutable sum = 0u
                for i = 0 to xs.Length - 1 do sum <- sum + xs.[i]
                sum / uint32 xs.Length
                
        static member inline Average(xs: int16[]) = 
            if xs.Length = 0 then 0s
            else
                let mutable sum = 0s
                for i = 0 to xs.Length - 1 do sum <- sum + xs.[i]
                sum / int16 xs.Length
                
        static member inline Average(xs: uint16[]) = 
            if xs.Length = 0 then 0us
            else
                let mutable sum = 0us
                for i = 0 to xs.Length - 1 do sum <- sum + xs.[i]
                sum / uint16 xs.Length

        // Product implementation for arrays
        static member inline Product(xs: int[]) = 
            if xs.Length = 0 then 1
            else
                let mutable product = 1
                for i = 0 to xs.Length - 1 do product <- product * xs.[i]
                product
            
        static member inline Product(xs: float[]) = 
            if xs.Length = 0 then 1.0
            else
                let mutable product = 1.0
                for i = 0 to xs.Length - 1 do product <- product * xs.[i]
                product
            
        static member inline Product(xs: int64[]) = 
            if xs.Length = 0 then 1L
            else
                let mutable product = 1L
                for i = 0 to xs.Length - 1 do product <- product * xs.[i]
                product
            
        static member inline Product(xs: float32[]) = 
            if xs.Length = 0 then 1.0f
            else
                let mutable product = 1.0f
                for i = 0 to xs.Length - 1 do product <- product * xs.[i]
                product
                
        static member inline Product(xs: decimal[]) = 
            if xs.Length = 0 then 1M
            else
                let mutable product = 1M
                for i = 0 to xs.Length - 1 do product <- product * xs.[i]
                product
    
    /// <summary>
    /// Provides collection operations for arrays of unit-of-measure types
    /// </summary>
    [<AbstractClass; Sealed>]
    type MeasureArrayOps =
        static member inline SumInt(xs: int<'u>[]) = 
            if xs.Length = 0 then LanguagePrimitives.GenericZero<int<'u>>
            else
                let mutable sum = xs.[0]
                for i = 1 to xs.Length - 1 do sum <- sum + xs.[i]
                sum
            
        static member inline SumFloat(xs: float<'u>[]) = 
            if xs.Length = 0 then LanguagePrimitives.GenericZero<float<'u>>
            else
                let mutable sum = xs.[0]
                for i = 1 to xs.Length - 1 do sum <- sum + xs.[i]
                sum
            
        static member inline SumInt64(xs: int64<'u>[]) = 
            if xs.Length = 0 then LanguagePrimitives.GenericZero<int64<'u>>
            else
                let mutable sum = xs.[0]
                for i = 1 to xs.Length - 1 do sum <- sum + xs.[i]
                sum
            
        static member inline SumFloat32(xs: float32<'u>[]) = 
            if xs.Length = 0 then LanguagePrimitives.GenericZero<float32<'u>>
            else
                let mutable sum = xs.[0]
                for i = 1 to xs.Length - 1 do sum <- sum + xs.[i]
                sum
                
        static member inline SumDecimal(xs: decimal<'u>[]) = 
            if xs.Length = 0 then LanguagePrimitives.GenericZero<decimal<'u>>
            else
                let mutable sum = xs.[0]
                for i = 1 to xs.Length - 1 do sum <- sum + xs.[i]
                sum
                
        static member inline AverageInt(xs: int<'u>[]) = 
            if xs.Length = 0 then LanguagePrimitives.GenericZero<int<'u>>
            else
                let mutable sum = xs.[0]
                for i = 1 to xs.Length - 1 do sum <- sum + xs.[i]
                sum / xs.Length
                
        static member inline AverageFloat(xs: float<'u>[]) = 
            if xs.Length = 0 then LanguagePrimitives.GenericZero<float<'u>>
            else
                let mutable sum = xs.[0]
                for i = 1 to xs.Length - 1 do sum <- sum + xs.[i]
                sum / float xs.Length
                
        static member inline AverageInt64(xs: int64<'u>[]) = 
            if xs.Length = 0 then LanguagePrimitives.GenericZero<int64<'u>>
            else
                let mutable sum = xs.[0]
                for i = 1 to xs.Length - 1 do sum <- sum + xs.[i]
                sum / int64 xs.Length
                
        static member inline AverageFloat32(xs: float32<'u>[]) = 
            if xs.Length = 0 then LanguagePrimitives.GenericZero<float32<'u>>
            else
                let mutable sum = xs.[0]
                for i = 1 to xs.Length - 1 do sum <- sum + xs.[i]
                sum / float32 xs.Length
                
        static member inline AverageDecimal(xs: decimal<'u>[]) = 
            if xs.Length = 0 then LanguagePrimitives.GenericZero<decimal<'u>>
            else
                let mutable sum = xs.[0]
                for i = 1 to xs.Length - 1 do sum <- sum + xs.[i]
                sum / decimal xs.Length

        // Product implementation for unit-of-measure arrays
        static member inline ProductInt(xs: int<'u>[]) = 
            if xs.Length = 0 then LanguagePrimitives.GenericOne<int<'u>>
            else
                let mutable product = xs.[0]
                for i = 1 to xs.Length - 1 do product <- product * LanguagePrimitives.Int32WithMeasure<'u>(int xs.[i])
                product
            
        static member inline ProductFloat(xs: float<'u>[]) = 
            if xs.Length = 0 then LanguagePrimitives.GenericOne<float<'u>>
            else
                let mutable product = xs.[0]
                for i = 1 to xs.Length - 1 do product <- product * LanguagePrimitives.FloatWithMeasure<'u>(float xs.[i])
                product
            
        static member inline ProductInt64(xs: int64<'u>[]) = 
            if xs.Length = 0 then LanguagePrimitives.GenericOne<int64<'u>>
            else
                let mutable product = xs.[0]
                for i = 1 to xs.Length - 1 do product <- product * LanguagePrimitives.Int64WithMeasure<'u>(int64 xs.[i])
                product
            
        static member inline ProductFloat32(xs: float32<'u>[]) = 
            if xs.Length = 0 then LanguagePrimitives.GenericOne<float32<'u>>
            else
                let mutable product = xs.[0]
                for i = 1 to xs.Length - 1 do product <- product * LanguagePrimitives.Float32WithMeasure<'u>(float32 xs.[i])
                product
                
        static member inline ProductDecimal(xs: decimal<'u>[]) = 
            if xs.Length = 0 then LanguagePrimitives.GenericOne<decimal<'u>>
            else
                let mutable product = xs.[0]
                for i = 1 to xs.Length - 1 do product <- product * LanguagePrimitives.DecimalWithMeasure<'u>(decimal xs.[i])
                product
    
    /// <summary>
    /// Min/Max operations for various numeric types
    /// </summary>
    module MinMaxOperations =
        [<AbstractClass; Sealed>]
        type MinImpl =
            static member Min(a: int, b: int) : int = if a < b then a else b
            static member Min(a: float, b: float) : float = if a < b then a else b
            static member Min(a: int64, b: int64) : int64 = if a < b then a else b
            static member Min(a: uint64, b: uint64) : uint64 = if a < b then a else b
            static member Min(a: float32, b: float32) : float32 = if a < b then a else b
            static member Min(a: decimal, b: decimal) : decimal = if a < b then a else b
            static member Min(a: byte, b: byte) : byte = if a < b then a else b
            static member Min(a: uint32, b: uint32) : uint32 = if a < b then a else b
            static member Min(a: int16, b: int16) : int16 = if a < b then a else b
            static member Min(a: uint16, b: uint16) : uint16 = if a < b then a else b
        
        [<AbstractClass; Sealed>]
        type MaxImpl =
            static member Max(a: int, b: int) : int = if a > b then a else b
            static member Max(a: float, b: float) : float = if a > b then a else b
            static member Max(a: int64, b: int64) : int64 = if a > b then a else b
            static member Max(a: uint64, b: uint64) : uint64 = if a > b then a else b
            static member Max(a: float32, b: float32) : float32 = if a > b then a else b
            static member Max(a: decimal, b: decimal) : decimal = if a > b then a else b
            static member Max(a: byte, b: byte) : byte = if a > b then a else b
            static member Max(a: uint32, b: uint32) : uint32 = if a > b then a else b
            static member Max(a: int16, b: int16) : int16 = if a > b then a else b
            static member Max(a: uint16, b: uint16) : uint16 = if a > b then a else b
        
        // Min/Max operations for unit-of-measure types
        
        [<AbstractClass; Sealed>]
        type MeasureMinImpl =
            static member MinInt(a: int<'u>, b: int<'u>) : int<'u> = if a < b then a else b
            static member MinFloat(a: float<'u>, b: float<'u>) : float<'u> = if a < b then a else b
            static member MinInt64(a: int64<'u>, b: int64<'u>) : int64<'u> = if a < b then a else b
            static member MinUInt64(a: uint64<'u>, b: uint64<'u>) : uint64<'u> = if a < b then a else b
            static member MinFloat32(a: float32<'u>, b: float32<'u>) : float32<'u> = if a < b then a else b
            static member MinDecimal(a: decimal<'u>, b: decimal<'u>) : decimal<'u> = if a < b then a else b
        
        [<AbstractClass; Sealed>]
        type MeasureMaxImpl =
            static member MaxInt(a: int<'u>, b: int<'u>) : int<'u> = if a > b then a else b
            static member MaxFloat(a: float<'u>, b: float<'u>) : float<'u> = if a > b then a else b
            static member MaxInt64(a: int64<'u>, b: int64<'u>) : int64<'u> = if a > b then a else b
            static member MaxUInt64(a: uint64<'u>, b: uint64<'u>) : uint64<'u> = if a > b then a else b
            static member MaxFloat32(a: float32<'u>, b: float32<'u>) : float32<'u> = if a > b then a else b
            static member MaxDecimal(a: decimal<'u>, b: decimal<'u>) : decimal<'u> = if a > b then a else b
    
    // --------------------------
    // Entry point type interfaces (same as before)
    // --------------------------
    
    /// <summary>
    /// Entry point for addition operations
    /// </summary>
    [<AbstractClass; Sealed>]
    type Add =
        static member inline Invoke(a, b) =
            ((^a or ^b or BasicOps or MeasureOps) : (static member Add: ^a * ^b -> ^r) (a, b))
    
    /// <summary>
    /// Entry point for subtraction operations
    /// </summary>
    [<AbstractClass; Sealed>]
    type Subtract =
        static member inline Invoke(a, b) =
            ((^a or ^b or BasicOps or MeasureOps) : (static member Subtract: ^a * ^b -> ^r) (a, b))
    
    /// <summary>
    /// Entry point for multiplication operations
    /// </summary>
    [<AbstractClass; Sealed>]
    type Multiply =
        static member inline Invoke(a, b) =
            ((^a or ^b or BasicOps or MeasureOps or MeasureMeasureOps) : 
                (static member Multiply: ^a * ^b -> ^r) (a, b))
    
    /// <summary>
    /// Entry point for division operations
    /// </summary>
    [<AbstractClass; Sealed>]
    type Divide =
        static member inline Invoke(a, b) =
            ((^a or ^b or BasicOps or MeasureOps or MeasureMeasureOps) : 
                (static member Divide: ^a * ^b -> ^r) (a, b))
    
    /// <summary>
    /// Entry point for modulo operations
    /// </summary>
    [<AbstractClass; Sealed>]
    type Modulo =
        static member inline Invoke(a, b) =
            ((^a or ^b or BasicOps or MeasureOps or MeasureMeasureOps) : 
                (static member Modulo: ^a * ^b -> ^r) (a, b))

    /// <summary>
    /// Entry point for power operations
    /// </summary>
    [<AbstractClass; Sealed>]
    type Power =
        static member inline Invoke(a, b) =
            ((^a or ^b or BasicOps) : (static member Power: ^a * ^b -> ^r) (a, b))

    /// <summary>
    /// Entry point for absolute value operations
    /// </summary>
    [<AbstractClass; Sealed>]
    type Abs =
        static member inline Invoke(a) =
            ((^a or BasicOps or MeasureOps) : (static member Abs: ^a -> ^r) a)

    /// <summary>
    /// Entry point for sign operations
    /// </summary>
    [<AbstractClass; Sealed>]
    type Sign =
        static member inline Invoke(a) =
            ((^a or BasicOps or MeasureOps) : (static member Sign: ^a -> int) a)

    /// <summary>
    /// Entry point for ceiling operations
    /// </summary>
    [<AbstractClass; Sealed>]
    type Ceiling =
        static member inline Invoke(a) =
            ((^a or BasicOps or MeasureOps) : (static member Ceiling: ^a -> ^r) a)

    /// <summary>
    /// Entry point for floor operations
    /// </summary>
    [<AbstractClass; Sealed>]
    type Floor =
        static member inline Invoke(a) =
            ((^a or BasicOps or MeasureOps) : (static member Floor: ^a -> ^r) a)

    /// <summary>
    /// Entry point for round operations
    /// </summary>
    [<AbstractClass; Sealed>]
    type Round =
        static member inline Invoke(a) =
            ((^a or BasicOps or MeasureOps) : (static member Round: ^a -> ^r) a)
    
    /// <summary>
    /// Entry point for sum operations
    /// </summary>
    [<AbstractClass; Sealed>]
    type Sum =
        static member inline Invoke(collection) =
            ((^collection or ^t or ArrayOps or MeasureArrayOps) : 
                (static member Sum: ^collection -> ^t) collection)
    
    /// <summary>
    /// Entry point for average operations
    /// </summary>
    [<AbstractClass; Sealed>]
    type Average =
        static member inline Invoke(collection) =
            ((^collection or ^t or ArrayOps or MeasureArrayOps) : 
                (static member Average: ^collection -> ^t) collection)

    /// <summary>
    /// Entry point for product operations
    /// </summary>
    [<AbstractClass; Sealed>]
    type Product =
        static member inline Invoke(collection) =
            ((^collection or ^t or ArrayOps or MeasureArrayOps) : 
                (static member Product: ^collection -> ^t) collection)
    
    // Comparison operations
    
    /// <summary>
    /// Entry point for less than operations
    /// </summary>
    [<AbstractClass; Sealed>]
    type LessThan = 
        static member inline Invoke(a, b) = a < b
    
    /// <summary>
    /// Entry point for greater than operations
    /// </summary>
    [<AbstractClass; Sealed>]
    type GreaterThan = 
        static member inline Invoke(a, b) = a > b
    
    /// <summary>
    /// Entry point for less than or equal operations
    /// </summary>
    [<AbstractClass; Sealed>]
    type LessThanOrEqual = 
        static member inline Invoke(a, b) = a <= b
    
    /// <summary>
    /// Entry point for greater than or equal operations
    /// </summary>
    [<AbstractClass; Sealed>]
    type GreaterThanOrEqual = 
        static member inline Invoke(a, b) = a >= b
    
    /// <summary>
    /// Entry point for equality operations
    /// </summary>
    [<AbstractClass; Sealed>]
    type Equals = 
        static member inline Invoke(a, b) = a = b
    
    [<Measure>] type internal TestUnit
    
    // Register implementations for static resolution
    let private registerMinMaxImplementations() =
        let _ = MinMaxOperations.MinImpl.Min(0, 0)
        let _ = MinMaxOperations.MaxImpl.Max(0, 0)
        let _ = MinMaxOperations.MinImpl.Min(0.0, 0.0)
        let _ = MinMaxOperations.MaxImpl.Max(0.0, 0.0)
        let _ = MinMaxOperations.MinImpl.Min(0L, 0L)
        let _ = MinMaxOperations.MaxImpl.Max(0L, 0L)
        let _ = MinMaxOperations.MinImpl.Min(0UL, 0UL)
        let _ = MinMaxOperations.MaxImpl.Max(0UL, 0UL)
        let _ = MinMaxOperations.MinImpl.Min(0.0f, 0.0f)
        let _ = MinMaxOperations.MaxImpl.Max(0.0f, 0.0f)
        let _ = MinMaxOperations.MinImpl.Min(0M, 0M)
        let _ = MinMaxOperations.MaxImpl.Max(0M, 0M)
        let _ = MinMaxOperations.MinImpl.Min(0uy, 0uy)
        let _ = MinMaxOperations.MaxImpl.Max(0uy, 0uy)
        let _ = MinMaxOperations.MinImpl.Min(0u, 0u)
        let _ = MinMaxOperations.MaxImpl.Max(0u, 0u)
        let _ = MinMaxOperations.MinImpl.Min(0s, 0s)
        let _ = MinMaxOperations.MaxImpl.Max(0s, 0s)
        let _ = MinMaxOperations.MinImpl.Min(0us, 0us)
        let _ = MinMaxOperations.MaxImpl.Max(0us, 0us)
        let _ = MinMaxOperations.MeasureMinImpl.MinInt(LanguagePrimitives.Int32WithMeasure<TestUnit>(0), LanguagePrimitives.Int32WithMeasure<TestUnit>(0))
        let _ = MinMaxOperations.MeasureMaxImpl.MaxInt(LanguagePrimitives.Int32WithMeasure<TestUnit>(0), LanguagePrimitives.Int32WithMeasure<TestUnit>(0))
        ()
    
    do registerMinMaxImplementations()

    // Internal helpers for unit conversions
    let inline intWithUnitInternal<[<Measure>] 'u> (value: int) : int<'u> = 
        LanguagePrimitives.Int32WithMeasure<'u>(value)

    let inline floatWithUnitInternal<[<Measure>] 'u> (value: float) : float<'u> = 
        LanguagePrimitives.FloatWithMeasure<'u>(value)

    let inline int64WithUnitInternal<[<Measure>] 'u> (value: int64) : int64<'u> = 
        LanguagePrimitives.Int64WithMeasure<'u>(value)

    let inline float32WithUnitInternal<[<Measure>] 'u> (value: float32) : float32<'u> = 
        LanguagePrimitives.Float32WithMeasure<'u>(value)

    let inline decimalWithUnitInternal<[<Measure>] 'u> (value: decimal) : decimal<'u> = 
        LanguagePrimitives.DecimalWithMeasure<'u>(value)

    // --------------------------
    // Public API
    // --------------------------
    
    /// <summary>
    /// Adds two values of the same type.
    /// </summary>
    /// <param name="a">The first value.</param>
    /// <param name="b">The second value.</param>
    /// <typeparam name="T">The type of values to add.</typeparam>
    /// <returns>The result of adding a and b.</returns>
    let inline add a b = Add.Invoke(a, b)

    /// <summary>
    /// Subtracts the second value from the first value.
    /// </summary>
    /// <param name="a">The value to subtract from.</param>
    /// <param name="b">The value to subtract.</param>
    /// <typeparam name="T">The type of values to subtract.</typeparam>
    /// <returns>The result of subtracting b from a.</returns>
    let inline subtract a b = Subtract.Invoke(a, b)

    /// <summary>
    /// Multiplies two values.
    /// </summary>
    /// <param name="a">The first value.</param>
    /// <param name="b">The second value.</param>
    /// <typeparam name="T">The type of the first value.</typeparam>
    /// <typeparam name="U">The type of the second value.</typeparam>
    /// <typeparam name="V">The resulting type.</typeparam>
    /// <returns>The result of multiplying a and b.</returns>
    let inline multiply a b = Multiply.Invoke(a, b)

    /// <summary>
    /// Divides the first value by the second value.
    /// </summary>
    /// <param name="a">The dividend.</param>
    /// <param name="b">The divisor.</param>
    /// <typeparam name="T">The type of the dividend.</typeparam>
    /// <typeparam name="U">The type of the divisor.</typeparam>
    /// <typeparam name="V">The resulting type.</typeparam>
    /// <returns>The result of dividing a by b.</returns>
    let inline divide a b = Divide.Invoke(a, b)

    /// <summary>
    /// Computes the remainder when dividing the first value by the second value.
    /// </summary>
    /// <param name="a">The dividend.</param>
    /// <param name="b">The divisor.</param>
    /// <typeparam name="T">The type of the dividend.</typeparam>
    /// <typeparam name="U">The type of the divisor.</typeparam>
    /// <typeparam name="V">The resulting type.</typeparam>
    /// <returns>The remainder of dividing a by b.</returns>
    let inline modulo a b = Modulo.Invoke(a, b)

    /// <summary>
    /// Raises the first value to the power of the second value.
    /// </summary>
    /// <param name="a">The base value.</param>
    /// <param name="b">The exponent value.</param>
    /// <typeparam name="T">The type of the base value.</typeparam>
    /// <typeparam name="U">The type of the exponent value.</typeparam>
    /// <typeparam name="V">The resulting type.</typeparam>
    /// <returns>The result of raising a to the power of b.</returns>
    let inline power a b = Power.Invoke(a, b)

    /// <summary>
    /// Computes the absolute value of a number.
    /// </summary>
    /// <param name="value">The input value.</param>
    /// <typeparam name="T">The type of the input value.</typeparam>
    /// <returns>The absolute value of the input.</returns>
    let inline abs value = Abs.Invoke(value)

    /// <summary>
    /// Computes the sign of a number.
    /// </summary>
    /// <param name="value">The input value.</param>
    /// <typeparam name="T">The type of the input value.</typeparam>
    /// <returns>-1 if the value is negative, 0 if it's zero, and 1 if it's positive.</returns>
    let inline sign value = Sign.Invoke(value)

    /// <summary>
    /// Computes the smallest integer greater than or equal to the given value.
    /// </summary>
    /// <param name="value">The input value.</param>
    /// <typeparam name="T">The type of the input value.</typeparam>
    /// <returns>The ceiling of the input value.</returns>
    let inline ceiling value = Ceiling.Invoke(value)

    /// <summary>
    /// Computes the largest integer less than or equal to the given value.
    /// </summary>
    /// <param name="value">The input value.</param>
    /// <typeparam name="T">The type of the input value.</typeparam>
    /// <returns>The floor of the input value.</returns>
    let inline floor value = Floor.Invoke(value)

    /// <summary>
    /// Rounds the given value to the nearest integer.
    /// </summary>
    /// <param name="value">The input value.</param>
    /// <typeparam name="T">The type of the input value.</typeparam>
    /// <returns>The rounded value.</returns>
    let inline round value = Round.Invoke(value)

    /// <summary>
    /// Computes the sum of all elements in a collection.
    /// </summary>
    /// <param name="collection">The collection to sum.</param>
    /// <typeparam name="Collection">The type of the collection.</typeparam>
    /// <typeparam name="T">The type of the elements in the collection.</typeparam>
    /// <returns>The sum of all elements in the collection.</returns>
    let inline sum collection = Sum.Invoke(collection)

    /// <summary>
    /// Computes the average of all elements in a collection.
    /// </summary>
    /// <param name="collection">The collection to average.</param>
    /// <typeparam name="Collection">The type of the collection.</typeparam>
    /// <typeparam name="T">The type of the elements in the collection.</typeparam>
    /// <returns>The average of all elements in the collection.</returns>
    let inline average collection = Average.Invoke(collection)

    /// <summary>
    /// Computes the product of all elements in a collection.
    /// </summary>
    /// <param name="collection">The collection to multiply.</param>
    /// <typeparam name="Collection">The type of the collection.</typeparam>
    /// <typeparam name="T">The type of the elements in the collection.</typeparam>
    /// <returns>The product of all elements in the collection.</returns>
    let inline product collection = Product.Invoke(collection)
    
    /// <summary>
    /// Checks if the first value is less than the second value.
    /// </summary>
    /// <param name="a">The first value.</param>
    /// <param name="b">The second value.</param>
    /// <returns>True if a is less than b, otherwise false.</returns>
    let inline lessThan a b = LessThan.Invoke(a, b)
    
    /// <summary>
    /// Checks if the first value is greater than the second value.
    /// </summary>
    /// <param name="a">The first value.</param>
    /// <param name="b">The second value.</param>
    /// <returns>True if a is greater than b, otherwise false.</returns>
    let inline greaterThan a b = GreaterThan.Invoke(a, b)
    
    /// <summary>
    /// Checks if the first value is less than or equal to the second value.
    /// </summary>
    /// <param name="a">The first value.</param>
    /// <param name="b">The second value.</param>
    /// <returns>True if a is less than or equal to b, otherwise false.</returns>
    let inline lessThanOrEqual a b = LessThanOrEqual.Invoke(a, b)
    
    /// <summary>
    /// Checks if the first value is greater than or equal to the second value.
    /// </summary>
    /// <param name="a">The first value.</param>
    /// <param name="b">The second value.</param>
    /// <returns>True if a is greater than or equal to b, otherwise false.</returns>
    let inline greaterThanOrEqual a b = GreaterThanOrEqual.Invoke(a, b)
    
    /// <summary>
    /// Checks if two values are equal.
    /// </summary>
    /// <param name="a">The first value.</param>
    /// <param name="b">The second value.</param>
    /// <returns>True if the values are equal, otherwise false.</returns>
    let inline equals a b = Equals.Invoke(a, b)

    /// <summary>
    /// Attaches a unit of measure to an integer value.
    /// </summary>
    /// <param name="value">The value to attach the unit to.</param>
    /// <typeparam name="u">The unit of measure to attach.</typeparam>
    /// <returns>The value with the unit of measure attached.</returns>
    let inline intWithUnit<[<Measure>] 'u> (value: int) : int<'u> = 
        LanguagePrimitives.Int32WithMeasure<'u>(value)

    /// <summary>
    /// Attaches a unit of measure to a floating-point value.
    /// </summary>
    /// <param name="value">The value to attach the unit to.</param>
    /// <typeparam name="u">The unit of measure to attach.</typeparam>
    /// <returns>The value with the unit of measure attached.</returns>
    let inline floatWithUnit<[<Measure>] 'u> (value: float) : float<'u> = 
        LanguagePrimitives.FloatWithMeasure<'u>(value)

    /// <summary>
    /// Attaches a unit of measure to a 64-bit integer value.
    /// </summary>
    /// <param name="value">The value to attach the unit to.</param>
    /// <typeparam name="u">The unit of measure to attach.</typeparam>
    /// <returns>The value with the unit of measure attached.</returns>
    let inline int64WithUnit<[<Measure>] 'u> (value: int64) : int64<'u> = 
        LanguagePrimitives.Int64WithMeasure<'u>(value)

    /// <summary>
    /// Attaches a unit of measure to a 32-bit floating-point value.
    /// </summary>
    /// <param name="value">The value to attach the unit to.</param>
    /// <typeparam name="u">The unit of measure to attach.</typeparam>
    /// <returns>The value with the unit of measure attached.</returns>
    let inline float32WithUnit<[<Measure>] 'u> (value: float32) : float32<'u> = 
        LanguagePrimitives.Float32WithMeasure<'u>(value)

    /// <summary>
    /// Attaches a unit of measure to a decimal value.
    /// </summary>
    /// <param name="value">The value to attach the unit to.</param>
    /// <typeparam name="u">The unit of measure to attach.</typeparam>
    /// <returns>The value with the unit of measure attached.</returns>
    let inline decimalWithUnit<[<Measure>] 'u> (value: decimal) : decimal<'u> = 
        LanguagePrimitives.DecimalWithMeasure<'u>(value)

    /// <summary>
    /// Removes the unit of measure from an integer value.
    /// </summary>
    /// <param name="value">The value from which to remove the unit.</param>
    /// <typeparam name="u">The unit of measure to remove.</typeparam>
    /// <returns>The value without the unit of measure.</returns>
    let inline intFromUnit<[<Measure>] 'u> (value: int<'u>) : int = int value

    /// <summary>
    /// Removes the unit of measure from a floating-point value.
    /// </summary>
    /// <param name="value">The value from which to remove the unit.</param>
    /// <typeparam name="u">The unit of measure to remove.</typeparam>
    /// <returns>The value without the unit of measure.</returns>
    let inline floatFromUnit<[<Measure>] 'u> (value: float<'u>) : float = float value

    /// <summary>
    /// Removes the unit of measure from a 64-bit integer value.
    /// </summary>
    /// <param name="value">The value from which to remove the unit.</param>
    /// <typeparam name="u">The unit of measure to remove.</typeparam>
    /// <returns>The value without the unit of measure.</returns>
    let inline int64FromUnit<[<Measure>] 'u> (value: int64<'u>) : int64 = int64 value

    /// <summary>
    /// Removes the unit of measure from a 32-bit floating-point value.
    /// </summary>
    /// <param name="value">The value from which to remove the unit.</param>
    /// <typeparam name="u">The unit of measure to remove.</typeparam>
    /// <returns>The value without the unit of measure.</returns>
    let inline float32FromUnit<[<Measure>] 'u> (value: float32<'u>) : float32 = float32 value

    /// <summary>
    /// Removes the unit of measure from a decimal value.
    /// </summary>
    /// <param name="value">The value from which to remove the unit.</param>
    /// <typeparam name="u">The unit of measure to remove.</typeparam>
    /// <returns>The value without the unit of measure.</returns>
    let inline decimalFromUnit<[<Measure>] 'u> (value: decimal<'u>) : decimal = decimal value