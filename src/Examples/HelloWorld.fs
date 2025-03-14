module Examples.HelloWorld

let hello() =
    printf "Enter your name: "
    let name = stdin.ReadLine()
    printfn "Hello, %s!" name

hello()