module Examples.TimeLoop

open Alloy

let displayTime() =
    // Register Windows implementation for REAL system time
    Alloy.Time.Platform.registerImplementation(Alloy.Time.Windows.createImplementation())
    
    let mutable counter = 0
    
    printf "=== TimeLoop Demo - REAL Local Time ===\n\n"
    
    // Get the system's current time zone offset (platform-agnostic)
    let timeZoneOffsetMinutes = Alloy.Time.getCurrentTimezoneOffsetMinutes()
      
    // Get the start time for reference
    let startTicks = Alloy.Time.highResolutionTicks()
    
    while counter < 5 do
        let currentTicks = Alloy.Time.highResolutionTicks()
        let utcNow : Alloy.Time.DateTime = Alloy.Time.now()
        
        // Calculate elapsed time since start
        let elapsedTicks = currentTicks - startTicks
        let freq = Alloy.Time.tickFrequency()
        let elapsedSeconds = float elapsedTicks / float freq
        
        // Convert UTC to local time: subtract the bias (minutes west of UTC)
        let utcTotalMinutes = utcNow.Hour * 60 + utcNow.Minute
        let localTotalMinutes = utcTotalMinutes - timeZoneOffsetMinutes
        
        // Handle day rollover
        let (adjustedHour, adjustedMinute, adjustedDay) = 
            if localTotalMinutes < 0 then
                let correctedMinutes = localTotalMinutes + (24 * 60)
                (correctedMinutes / 60, correctedMinutes % 60, utcNow.Day - 1)
            elif localTotalMinutes >= 24 * 60 then
                let correctedMinutes = localTotalMinutes - (24 * 60)
                (correctedMinutes / 60, correctedMinutes % 60, utcNow.Day + 1)
            else
                (localTotalMinutes / 60, localTotalMinutes % 60, utcNow.Day)
        
        // Create local DateTime
        let localTime : Alloy.Time.DateTime = Alloy.Time.createDateTime 
                                                  utcNow.Year 
                                                  utcNow.Month 
                                                  adjustedDay
                                                  adjustedHour
                                                  adjustedMinute
                                                  utcNow.Second 
                                                  utcNow.Millisecond
        
        let pad2 n = 
            if n < 10 then "0" + string n
            else string n
        
        let timeStr = 
            string localTime.Year + "-" + pad2 localTime.Month + "-" + pad2 localTime.Day + 
            " " + pad2 localTime.Hour + ":" + pad2 localTime.Minute + ":" + pad2 localTime.Second
        
        printf "Time %d: %s | Elapsed: %.3f sec\n" 
               counter timeStr elapsedSeconds
        
        // Use REAL Windows Sleep API
        Alloy.Time.sleep 1000
        
        counter <- counter + 1

displayTime()