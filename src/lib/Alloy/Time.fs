namespace Alloy

open Alloy.Time.Platform

/// <summary>
/// Pure F# time utilities with no System.DateTime dependencies
/// </summary>
[<RequireQualifiedAccess>]
module Time =
    
    // Private backing fields
    let private unixEpochTicks = 621355968000000000L // Constant representing 1970-01-01
    let private ticksPerSecond = 10000000L
    let private ticksPerMillisecond = 10000L
    let private ticksPerMinute = multiply 60L ticksPerSecond
    let private ticksPerHour = multiply 60L ticksPerMinute
    let private ticksPerDay = multiply 24L ticksPerHour
    let private daysPerYear = 365L
    let private daysPerLeapYear = 366L
    let private daysInMonthArray = [|31; 28; 31; 30; 31; 30; 31; 31; 30; 31; 30; 31|]
    let private daysInMonthLeapYearArray = [|31; 29; 31; 30; 31; 30; 31; 31; 30; 31; 30; 31|]
    
    /// <summary>
    /// Represents a date and time structure
    /// </summary>
    [<Struct>]
    type DateTime = {
        /// <summary>The year component (1-9999)</summary>
        Year: int
        /// <summary>The month component (1-12)</summary>
        Month: int
        /// <summary>The day component (1-31)</summary>
        Day: int
        /// <summary>The hour component (0-23)</summary>
        Hour: int
        /// <summary>The minute component (0-59)</summary>
        Minute: int
        /// <summary>The second component (0-59)</summary>
        Second: int
        /// <summary>The millisecond component (0-999)</summary>
        Millisecond: int
    }
    
    /// <summary>
    /// Represents a time span structure
    /// </summary>
    [<Struct>]
    type TimeSpan = {
        /// <summary>The days component</summary>
        Days: int
        /// <summary>The hours component</summary>
        Hours: int
        /// <summary>The minutes component</summary>
        Minutes: int
        /// <summary>The seconds component</summary>
        Seconds: int
        /// <summary>The milliseconds component</summary>
        Milliseconds: int
    }

    /// <summary>
    /// Represents an instant in time as seconds since Unix epoch
    /// </summary>
    [<Struct>]
    type Timestamp = {
        /// <summary>Seconds since Unix epoch (1970-01-01)</summary>
        Seconds: int64
        /// <summary>Nanoseconds part (0-999,999,999)</summary>
        Nanoseconds: int
    }
    
    /// <summary>
    /// Gets the platform time implementation
    /// </summary>
    let private platformTime = getImplementation()
    
    /// <summary>
    /// Checks if a year is a leap year
    /// </summary>
    /// <param name="year">The year to check</param>
    /// <returns>True if the year is a leap year, false otherwise</returns>
    let isLeapYear (year: int): bool =
        (modulo year 4 = 0 && modulo year 100 <> 0) || (modulo year 400 = 0)
    
    /// <summary>
    /// Gets the current time in ticks from the platform
    /// </summary>
    /// <returns>The current time in ticks</returns>
    let currentTicks (): int64 =
        platformTime.GetCurrentTicks()
    
    /// <summary>
    /// Gets the current Unix timestamp (seconds since 1970-01-01)
    /// </summary>
    /// <returns>The current Unix timestamp</returns>
    let currentUnixTimestamp (): int64 =
        let ticks = currentTicks()
        divide (subtract ticks unixEpochTicks) ticksPerSecond
    
    /// <summary>
    /// Gets the current Unix timestamp with nanosecond precision
    /// </summary>
    /// <returns>The current timestamp with nanosecond precision</returns>
    let currentTimestamp (): Timestamp =
        let ticks = currentTicks()
        let ticksSinceEpoch = subtract ticks unixEpochTicks
        let seconds = divide ticksSinceEpoch ticksPerSecond
        let tickRemainder = modulo ticksSinceEpoch ticksPerSecond
        let nanoseconds = int (multiply tickRemainder 100L) // Convert 100ns ticks to nanoseconds
        { Seconds = seconds; Nanoseconds = nanoseconds }
    
    /// <summary>
    /// Gets high-resolution performance counter ticks
    /// </summary>
    /// <returns>The current high-resolution ticks</returns>
    let highResolutionTicks (): int64 =
        platformTime.GetHighResolutionTicks()
        
    /// <summary>
    /// Gets the frequency of the high-resolution performance counter
    /// </summary>
    /// <returns>The frequency in ticks per second</returns>
    let tickFrequency (): int64 =
        platformTime.GetTickFrequency()
        
    /// <summary>
    /// Calculates elapsed time between two high-resolution tick values
    /// </summary>
    /// <param name="startTicks">The starting tick count</param>
    /// <param name="endTicks">The ending tick count</param>
    /// <returns>A TimeSpan representing the elapsed time</returns>
    let elapsedTime (startTicks: int64) (endTicks: int64): TimeSpan =
        let elapsedTicks = subtract endTicks startTicks
        let freq = tickFrequency()
        
        // Convert to milliseconds
        let totalMilliseconds = divide (multiply elapsedTicks 1000L) freq
        
        // Calculate components
        let totalSeconds = divide totalMilliseconds 1000L
        let ms = int (modulo totalMilliseconds 1000L)
        
        let totalMinutes = divide totalSeconds 60L
        let s = int (modulo totalSeconds 60L)
        
        let totalHours = divide totalMinutes 60L
        let m = int (modulo totalMinutes 60L)
        
        let h = int (modulo totalHours 24L)
        let d = int (divide totalHours 24L)
        
        { Days = d; Hours = h; Minutes = m; Seconds = s; Milliseconds = ms }
    
    /// <summary>
    /// Converts Unix timestamp to ticks
    /// </summary>
    /// <param name="timestamp">The Unix timestamp in seconds</param>
    /// <returns>The timestamp in ticks</returns>
    let unixTimestampToTicks (timestamp: int64): int64 =
        add unixEpochTicks (multiply timestamp ticksPerSecond)
    
    /// <summary>
    /// Converts ticks to Unix timestamp
    /// </summary>
    /// <param name="ticks">The timestamp in ticks</param>
    /// <returns>The Unix timestamp in seconds</returns>
    let ticksToUnixTimestamp (ticks: int64): int64 =
        divide (subtract ticks unixEpochTicks) ticksPerSecond
    
    /// <summary>
    /// Converts a Unix timestamp to a DateTime structure
    /// </summary>
    /// <param name="timestamp">The Unix timestamp in seconds</param>
    /// <returns>A DateTime structure representing the timestamp</returns>
    let unixTimestampToDateTime (timestamp: int64): DateTime =
        let mutable remainingSecs = timestamp
        let days = divide remainingSecs 86400L // seconds per day
        remainingSecs <- modulo remainingSecs 86400L

        let hours = int (divide remainingSecs 3600L)
        remainingSecs <- modulo remainingSecs 3600L
        
        let minutes = int (divide remainingSecs 60L)
        remainingSecs <- modulo remainingSecs 60L
        
        let seconds = int remainingSecs

        let mutable year = 1970
        let mutable remainingDays = days

        while greaterThanOrEqual remainingDays (if isLeapYear year then daysPerLeapYear else daysPerYear) do
            let daysInYear = if isLeapYear year then daysPerLeapYear else daysPerYear
            remainingDays <- subtract remainingDays daysInYear
            year <- add year 1

        let monthDays = if isLeapYear year then daysInMonthLeapYearArray else daysInMonthArray
        
        let mutable month = 0
        while lessThan month 12 && greaterThanOrEqual remainingDays (int64 monthDays.[month]) do
            remainingDays <- subtract remainingDays (int64 monthDays.[month])
            month <- add month 1
        
        {
            Year = year
            Month = add month 1
            Day = add (int remainingDays) 1
            Hour = hours
            Minute = minutes
            Second = seconds
            Millisecond = 0
        }
    
    /// <summary>
    /// Converts a DateTime structure to a Unix timestamp
    /// </summary>
    /// <param name="dateTime">The DateTime to convert</param>
    /// <returns>The Unix timestamp in seconds</returns>
    let dateTimeToUnixTimestamp (dateTime: DateTime): int64 =
        // Count days from epoch to start of year
        let mutable days = 0L
        for y = 1970 to subtract dateTime.Year 1 do
            days <- add days (if isLeapYear y then daysPerLeapYear else daysPerYear)
        
        // Add days for months in current year
        let monthDays = if isLeapYear dateTime.Year then daysInMonthLeapYearArray else daysInMonthArray
        
        for m = 0 to subtract dateTime.Month 2 do
            days <- add days (int64 monthDays.[m])
        
        // Add days in current month
        days <- add days (int64 (subtract dateTime.Day 1))
        
        // Calculate total seconds
        let totalSeconds = 
            add (multiply days 86400L)
                (add (multiply (int64 dateTime.Hour) 3600L)
                     (add (multiply (int64 dateTime.Minute) 60L)
                          (int64 dateTime.Second)))
        
        totalSeconds
    
    /// <summary>
    /// Creates a DateTime structure
    /// </summary>
    /// <param name="year">The year</param>
    /// <param name="month">The month</param>
    /// <param name="day">The day</param>
    /// <param name="hour">The hour</param>
    /// <param name="minute">The minute</param>
    /// <param name="second">The second</param>
    /// <param name="millisecond">The millisecond</param>
    /// <returns>A new DateTime structure</returns>
    /// <exception cref="System.Exception">Thrown when the parameters are invalid</exception>
    let createDateTime (year: int) (month: int) (day: int) (hour: int) (minute: int) (second: int) (millisecond: int): DateTime =
        // Validate parameters
        if lessThan year 1 || greaterThan year 9999 then
            failwith $"Year out of range: {year}"
        
        if lessThan month 1 || greaterThan month 12 then
            failwith $"Month out of range: {month}"
        
        let daysInCurrentMonth = 
            if isLeapYear year then
                daysInMonthLeapYearArray.[subtract month 1]
            else
                daysInMonthArray.[subtract month 1]
                
        if lessThan day 1 || greaterThan day daysInCurrentMonth then
            failwith $"Day out of range: {day}"
        
        if lessThan hour 0 || greaterThan hour 23 then
            failwith $"Hour out of range: {hour}"
        
        if lessThan minute 0 || greaterThan minute 59 then
            failwith $"Minute out of range: {minute}"
        
        if lessThan second 0 || greaterThan second 59 then
            failwith $"Second out of range: {second}"
        
        if lessThan millisecond 0 || greaterThan millisecond 999 then
            failwith $"Millisecond out of range: {millisecond}"
        
        {
            Year = year
            Month = month
            Day = day
            Hour = hour
            Minute = minute
            Second = second
            Millisecond = millisecond
        }
    
    /// <summary>
    /// Gets the current time as a DateTime structure
    /// </summary>
    /// <returns>The current time</returns>
    let now (): DateTime =
        let timestamp = currentUnixTimestamp()
        unixTimestampToDateTime timestamp
    
    /// <summary>
    /// Gets the current UTC time as a DateTime structure
    /// </summary>
    /// <returns>The current UTC time</returns>
    let utcNow (): DateTime =
        let ticks = platformTime.GetUtcNow()
        let timestamp = divide (subtract ticks unixEpochTicks) ticksPerSecond
        unixTimestampToDateTime timestamp
    
    /// <summary>
    /// Sleeps for the specified number of milliseconds
    /// </summary>
    /// <param name="milliseconds">The number of milliseconds to sleep</param>
    let sleep (milliseconds: int): unit =
        if lessThan milliseconds 0 then
            invalidArg "milliseconds" "Sleep duration must be non-negative"
        
        platformTime.Sleep(milliseconds)