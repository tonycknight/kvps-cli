namespace kvps

open System
open McMaster.Extensions.CommandLineUtils
open Microsoft.Extensions.DependencyInjection

module ProgramBootstrap=
    open kvps.Reflection

    let internal serviceCollection()=
        let c = new ServiceCollection()
        
        c.AddSingleton(typedefof<Config.IConfigRepository>, typedefof<Config.EnvVarsConfigRepository>)
         .AddTransient(typedefof<Config.IConfigProvider>, typedefof<Config.AggregateConfigProvider>)
         .AddSingleton(typedefof<KeyValues.IKeyValueRepository>, typedefof<KeyValues.LiteDbKeyValueRepository>)            
         .BuildServiceProvider()

    let internal appDescription()=        
        let attrs = getAsm() |> getAttrs 
        let vsn = getVersionValue attrs
        let copyright = getCopyrightValue attrs
                
        let header = 
            [
                "kvps-cli" |> Strings.magenta;
                "A key/value pair management tool" |> Strings.yellow;
                "";
            ]

        let meta = [    vsn |> Option.map (sprintf "Version %s" >> Strings.cyan) ; 
                        copyright |> Option.map Strings.yellow
                    ] |> Seq.flattenSomes |> List.ofSeq

        let footer = [
                "Repo: https://github.com/tonycknight/kvps-cli" |> Strings.yellow;
            ]            

        footer
            |> List.append meta
            |> List.append header
            |> Strings.join Environment.NewLine

module Program=
            
    
    [<EntryPoint>]
    let main argv = 
        
        try                        
            use app = new CommandLineApplication()
            app.Name <- "kvps"
            app.Description <- ProgramBootstrap.appDescription()
            
            let opt = app.HelpOption(true) 
            
            let sp = ProgramBootstrap.serviceCollection()

            app.Command("set", Commands.setValueCmd sp) |> ignore
            app.Command("get", Commands.getValueCmd sp) |> ignore
            app.Command("del", Commands.deleteKeyCmd sp) |> ignore
            app.Command("list", Commands.listKeysCmd sp) |> ignore
            
            app.Command("db", Commands.dbCmd sp) |> ignore

            app.OnExecute(fun () -> app.ShowHelp())

            argv |> app.Execute
            
        with
            | ex ->              
                ex.Message |> System.Console.Error.WriteLine
                Bool.toRc false
    
    