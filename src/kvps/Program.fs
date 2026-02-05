namespace kvps

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

        let nugetInfo =
            vsn
            |> Option.bind Nuget.getUpgradeVersion
            |> Option.map (sprintf "An upgrade is available: %s")

        let header =
            [ Nuget.packageId |> Strings.cyan
              "A key/value pair management tool" |> Strings.yellow
              "" ]

        let meta =
            [ vsn |> Option.map (sprintf "Version %s" >> Strings.yellow)
              nugetInfo |> Option.map Strings.green
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
