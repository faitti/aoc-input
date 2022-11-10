open System.Net.Http
open System.IO

// Session token from AOC cookies
let session_token : string = System.Environment.GetEnvironmentVariable "SESSION_TOKEN"

// Base string to get input from day n of year m
let input_path (day : string) (year : string) : string = 
    $"https://adventofcode.com/{year}/day/{day}/input"

// Fetch input from the AoC input page
let get_input (uri : string) =
    task {
        // Check if file exists
        if File.Exists "./input" then
            printfn "%s" session_token
            printfn "File exists already"
            // Return unit type
            return ()

        // File handle to the input file
        use file = File.OpenWrite("./input")
        use client = new HttpClient()
        
        // Set session token to cookie header
        client.DefaultRequestHeaders.Add("cookie", $"session={session_token}")

        // Fetch the input data from given uri
        let! response = client.GetStreamAsync(uri)

        // Copy response contents to the input file
        do! response.CopyToAsync(file)

        // File and HttpClient handles will be disposed automatically
        // After they get out of scope
    }

// Parses the one argument if only one argument is given
let parse_one_arg (n : string) : string[] =
    match n |> int with
        | n_day when 0 < n_day && n_day < 32 -> [|n; "2022"|]
        | n_year when 2014 < n_year && n_year < 2023 -> [|"1"; n|]
        | _ -> [|"1"; "2021"|]

// Parses argv to list of 2 elements [|day; year|]
let parse_args (argv : string[]) : string[] =
    match argv with
        | [|day; year|] -> [|day ; year|]
        | n when n.Length = 1 -> parse_one_arg n[0]
        | _ -> [|"1"; "2021"|]

[<EntryPoint>]
let main argv = 
    // Parse day and year from command line arguments
    // Usage: dotnet run day year || ./binary day year
    let data = parse_args argv
    let input_url : string = input_path data[0] data[1]
    
    // Pipe formatted input_url to get_input
    // Pipe the returned task to run it synchronously
    input_url 
        |> get_input 
        |> Async.AwaitTask 
        |> Async.RunSynchronously

    printfn 
        "Input fetched successfully\n\
        INFO: { day: %s, year: %s }" data[0] data[1]
    0