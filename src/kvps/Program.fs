﻿namespace kvps

open System
open McMaster.Extensions.CommandLineUtils
open Microsoft.Extensions.DependencyInjection

module ProgramBootstrap =
    open System.Runtime.InteropServices
    open kvps.Reflection

    let internal isSupportedOs () =
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows)

    let internal checkIfSupportedOs () =
        if isSupportedOs () |> not then
            failwith "Unsupported OS! Apologies, only Windows is supported."

    let internal serviceCollection () =
        let c = new ServiceCollection()

        c
            .AddSingleton(typedefof<Config.IConfigRepository>, typedefof<Config.EnvVarsConfigRepository>)
            .AddTransient(typedefof<Config.IConfigProvider>, typedefof<Config.AggregateConfigProvider>)
            .AddSingleton(typedefof<KeyValues.IKeyValueRepository>, typedefof<KeyValues.LiteDbKeyValueRepository>)
            .BuildServiceProvider()

    let internal appDescription () =
        let attrs = getAsm () |> getAttrs
        let copyright = getCopyrightValue attrs
        let vsn = getVersionValue attrs

        let currentVersion =
            match vsn with
            | Some vsn ->
                // Trim versions that are suffixed with a commit hash.
                let vsn =
                    match vsn.IndexOf('+') with
                    | x when x >= 0 -> vsn.Substring(0, x)
                    | _ -> vsn

                new System.Version(vsn)
            | None -> new System.Version()

        let nugetLatest = NugetClient.getLatestVersion ()

        let nugetInfo =
            if nugetLatest > currentVersion then
                sprintf "An upgrade is available: %A" nugetLatest |> Some
            else
                None

        let header =
            [ "kvps-cli" |> Strings.magenta
              "A key/value pair management tool" |> Strings.yellow
              "" ]

        let meta =
            [ vsn
              |> Option.map (fun v ->
                  let v = v |> sprintf "Version %s" |> Strings.yellow
                  let beta = Strings.cyan "beta"
                  sprintf "%s %s" v beta)
              nugetInfo |> Option.map Strings.yellow
              copyright |> Option.map Strings.yellow ]
            |> Seq.flattenSomes
            |> List.ofSeq

        let footer = [ "Repo: https://github.com/tonycknight/kvps-cli" |> Strings.yellow ]

        footer
        |> List.append meta
        |> List.append header
        |> Strings.join Environment.NewLine

module Program =

    [<EntryPoint>]
    let main argv =

        let appName = "kvps"

        try
            ProgramBootstrap.checkIfSupportedOs ()

            use app = new CommandLineApplication()
            app.Name <- appName
            app.Description <- ProgramBootstrap.appDescription ()
            app.UnrecognizedArgumentHandling <- UnrecognizedArgumentHandling.Throw
            app.MakeSuggestionsInErrorMessage <- true

            let opt = app.HelpOption(true)

            let sp = ProgramBootstrap.serviceCollection ()

            app.Command("set", Commands.setValueCmd sp) |> ignore
            app.Command("get", Commands.getValueCmd sp) |> ignore
            app.Command("del", Commands.deleteKeyCmd sp) |> ignore
            app.Command("list", Commands.listKeysCmd sp) |> ignore
            app.Command("db", Commands.dbCmd sp) |> ignore

            app.OnExecute(fun () -> app.ShowHelp())

            argv |> app.Execute

        with
        | :? UnrecognizedCommandParsingException as ex ->
            ex.Message |> Strings.red |> System.Console.Error.WriteLine

            let cmd =
                match ex.Command.Parent with
                | null -> appName
                | _ -> sprintf "%s %s" appName ex.Command.Name

            let matches = ex.NearestMatches |> Seq.map (sprintf "%s %s" cmd) |> Array.ofSeq

            if matches.Length > 0 then
                System.Console.Error.WriteLine "Did you mean one of these commands?"
                matches |> Seq.iter System.Console.Error.WriteLine

            Bool.toRc false
        | ex ->
            ex.Message |> Strings.red |> System.Console.Error.WriteLine
            Bool.toRc false
