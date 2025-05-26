module Examples.TimeLoop

open Alloy

let displayTime() =
    // Register Windows implementation for REAL system time
    Alloy.Time.Platform.registerImplementation(Alloy.Time.Windows.createImplementation())
    
    let mutable counter = 0
    
    // Print info about the time implementation
    printf "=== TimeLoop Demo - REAL Windows System Time ===\n"
    printf "Using Windows API: GetSystemTimeAsFileTime, QueryPerformanceCounter\n"
    printf "This is REAL wall-clock time from the operating system!\n"
    printf "=================================================\n\n"
    
    // Get the start time for reference
    let startTicks = Alloy.Time.highResolutionTicks()
    
    while counter < 5 do
        // Get fresh time values each iteration
        let currentTicks = Alloy.Time.highResolutionTicks()
        let utcNow : Alloy.Time.DateTime = Alloy.Time.now()
        
        // Calculate elapsed time since start
        let elapsedTicks = currentTicks - startTicks
        let freq = Alloy.Time.tickFrequency()
        let elapsedSeconds = float elapsedTicks / float freq
        
        // Convert to local time (EDT = UTC-4 for May)
        let timezoneOffsetHours = -4  // EDT (Eastern Daylight Time)
        let localHour = utcNow.Hour + timezoneOffsetHours
        
        // Handle day rollover
        let (adjustedHour, adjustedDay) = 
            if localHour < 0 then (localHour + 24, utcNow.Day - 1)
            elif localHour >= 24 then (localHour - 24, utcNow.Day + 1)
            else (localHour, utcNow.Day)
        
        // Create local DateTime
        let localTime : Alloy.Time.DateTime = Alloy.Time.createDateTime 
                                                  utcNow.Year 
                                                  utcNow.Month 
                                                  adjustedDay
                                                  adjustedHour
                                                  utcNow.Minute 
                                                  utcNow.Second 
                                                  utcNow.Millisecond
        
        let pad2 n = 
            if n < 10 then "0" + string n
            else string n
        
        let timeStr = 
            string localTime.Year + "-" + pad2 localTime.Month + "-" + pad2 localTime.Day + 
            " " + pad2 localTime.Hour + ":" + pad2 localTime.Minute + ":" + pad2 localTime.Second
        
        printf "Time %d (Local): %s | Elapsed: %.3f sec | Ticks: %d\n" 
               counter timeStr elapsedSeconds currentTicks
        
        // Use REAL Windows Sleep API - you should feel a 1-second delay
        Alloy.Time.sleep 1000
        
        counter <- counter + 1

displayTime()