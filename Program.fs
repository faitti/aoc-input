open System.Net.Http
open System.IO
open System.Text.RegularExpressions

let build : bool = true

// Session token from AOC cookies
let get_env_var (var : string) : string = System.Environment.GetEnvironmentVariable var

let build_string : string = "53616c7465645f5f8bd5613404d95bac31a9d95aee120d1170c41fa0af150d65cbf1994c71ad2092cf750736d7c041ddef70b6d7af2966ba844e1d32347770aa"

// Base string to get input from day n of year m
let input_path (day : string) (year : string) : string = 
    $"https://adventofcode.com/{year}/day/{day}/input"

// Parses .env file and defines the environment variables
let loadEnv() =
    if not (File.Exists "./.env") then ()
    File.ReadAllLines("./.env")
    |> Array.iter(fun line ->
        // Remove comments
        let replaced_line = Regex.Replace(line, "#.*", "")

        // Split into key=value pair
        let kv_pairs = 
            replaced_line.Split([|'='|])
            |> Array.map(fun str -> str.Trim())
        
        if not (kv_pairs.Length = 2) then ()

        // Define the environment variable
        System.Environment.SetEnvironmentVariable(kv_pairs[0], kv_pairs[1])
    )

// Fetch input from the AoC input page
let get_input (uri : string) =
    task {
        // Check if file exists
        if File.Exists "./input" then
            printfn "File already exists"
            // Return unit type
            return ()

        // File handle to the input file
        use file = File.OpenWrite("./input")
        use client = new HttpClient()

        // Get session token from env variables
        let mutable session_token : string = ""
        if not build then
            session_token <- get_env_var "SESSION_TOKEN"
        else
            session_token <- build_string

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

// Parses argv to a list of 2 elements [|day; year|]
let parse_args (argv : string[]) : string[] =
    match argv with
        | n when n.Length > 1 -> [|n[0]; n[1]|]
        | n when n.Length = 1 -> parse_one_arg n[0]
        | _ -> [|"1"; "2021"|]

[<EntryPoint>]
let main argv = 
    // Load .env file if not build
    if not build then
        loadEnv()

    // Parse day and year from command line arguments
    // Usage: dotnet run day year || ./binary day year
    let data = parse_args argv
    let input_url : string = input_path data[0] data[1]

    printfn "%A" data
    
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