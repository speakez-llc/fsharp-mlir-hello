module Examples.TimeLoop

open Alloy
open Alloy.Time

let displayTime() =
    let mutable counter = 0
    while counter < 5 do
        let now = Time.now()
        
        let year = now.Year
        let month = now.Month  
        let day = now.Day
        let hour = now.Hour
        let minute = now.Minute
        let second = now.Second
        
        let pad2 n = 
            if n < 10 then 
                String.concat "0" (string n)
            else 
                string n
        
        let dateStr = 
            String.concat3
                (string year)
                "-"
                (pad2 month)
            |> String.concat3
                "-"
                (pad2 day)
            |> String.concat3
                " "
                (pad2 hour)
            |> String.concat3
                ":"
                (pad2 minute)
            |> String.concat3
                ":"
                (pad2 second)
        
        printf "Time %d: %s\n" counter dateStr
        
        Time.sleep 1000
        
        counter <- counter + 1

displayTime()