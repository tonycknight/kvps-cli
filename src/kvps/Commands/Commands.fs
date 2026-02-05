namespace kvps

open McMaster.Extensions.CommandLineUtils
open Microsoft.Extensions.DependencyInjection
open kvps.KeyValues

module Commands =

  let private descr description (cla: CommandLineApplication) =
    cla.Description <- description
    cla

  let private nameArg (cla: CommandLineApplication) =
    cla.Argument("<NAME>", "The name.", false).IsRequired()

  let private keyArg (cla: CommandLineApplication) =
    cla.Argument("<KEY>", "The key.", false).IsRequired()

  let private fileNameArg (cla: CommandLineApplication) =
    cla.Argument("<FILENAME>", "The file's name.", false).IsRequired()

  let private valueOption (cla: CommandLineApplication) =
    let opt =
      cla.Option("-v", "The key's value.", CommandOptionType.SingleValue).IsRequired()

    opt.LongName <- "value"
    opt

  let private publicOption (cla: CommandLineApplication) =
    let opt =
      cla.Option("-p", "The value is public and always revealed.", CommandOptionType.NoValue)

    opt.LongName <- "public"
    opt

  let private secretOption (cla: CommandLineApplication) =
    let opt = cla.Option("-s", "The value is secret.", CommandOptionType.NoValue)
    opt.LongName <- "secret"
    opt

  let private tagsOption (cla: CommandLineApplication) =
    let opt =
      cla.Option(
        "-t",
        "A tag to associate with the key. Multiple tags can be applied.",
        CommandOptionType.MultipleValue
      )

    opt.LongName <- "tag"
    opt

  let private revealOption (cla: CommandLineApplication) =
    let opt =
      cla.Option("-r", "Reveal the value if it's not public.", CommandOptionType.NoValue)

    opt.LongName <- "reveal"
    opt

  let private valueOnlyOption (cla: CommandLineApplication) =
    let opt = cla.Option("-vo", "Show only the value.", CommandOptionType.NoValue)
    opt.LongName <- "valueonly"
    opt

  let private copyToClipboardOption (cla: CommandLineApplication) =
    let opt = cla.Option("-c", "Copy to the clipboard.", CommandOptionType.NoValue)
    opt.LongName <- "clipboard"
    opt

  let private repo (sp: ServiceProvider) = sp.GetService<IKeyValueRepository>()

  let private configRepo (sp: ServiceProvider) =
    sp.GetService<Config.IConfigRepository>()

  let setValueCmd (sp: ServiceProvider) (cla: CommandLineApplication) =
    let cla = cla |> descr "Set a key value pair."

    let key = keyArg cla
    let value = valueOption cla
    let isVisible = publicOption cla
    let isSecret = secretOption cla
    let tags = tagsOption cla

    let exec (cts) =
      task {
        let kvRepo = repo sp

        let value =
          { KeyValue.key = key.Value |> Strings.trim
            value = value.Value() |> Strings.trim
            tags = (tags.Values |> Strings.trimSeq |> Seq.distinct |> Array.ofSeq)
            isSecret = (isSecret.HasValue()) || not (isVisible.HasValue()) }

        let! r = value |> kvRepo.SetValueAsync

        return r |> Bool.toRc
      }

    cla.OnExecuteAsync(exec)

  let getValueCmd (sp: ServiceProvider) (cla: CommandLineApplication) =
    let cla = cla |> descr "Get a key's value."

    let key = keyArg cla
    let reveal = revealOption cla
    let valueOnly = valueOnlyOption cla
    let copyClipboard = copyToClipboardOption cla

    let exec (cts) =
      task {
        let kvRepo = repo sp
        let! v = key.Value |> Strings.trim |> kvRepo.GetValueAsync

        let msg =
          match v with
          | Some kv ->
            let revealValue = reveal.HasValue()
            let renderValue = Rendering.renderKvValue revealValue

            match valueOnly.HasValue() with
            | false -> Rendering.renderKv renderValue kv
            | true -> [ renderValue kv ]
          | _ -> []

        Console.writeLines msg

        match (v, copyClipboard.HasValue()) with
        | (None, _)
        | (Some _, false) -> ignore 0
        | (Some kv, true) ->
          Clipboard.set kv.value
          Console.writeLine "Copied to clipboard."

        return true |> Bool.toRc
      }

    cla.OnExecuteAsync(exec)

  let deleteKeyCmd (sp: ServiceProvider) (cla: CommandLineApplication) =
    let cla = cla |> descr "Delete a key value."

    let key = keyArg cla

    let exec (cts) =
      task {
        let kvRepo = repo sp
        let! r = key.Value |> Strings.trim |> kvRepo.DeleteKeyAsync

        return r |> Bool.toRc
      }

    cla.OnExecuteAsync(exec)

  let listKeysCmd (sp: ServiceProvider) (cla: CommandLineApplication) =
    let cla = cla |> descr "List all keys."

    let tags = tagsOption cla

    let exec (cts) =
      task {
        let kvRepo = repo sp

        let! kvs =
          tags.Values
          |> Strings.trimSeq
          |> Seq.distinct
          |> Array.ofSeq
          |> kvRepo.ListKeysAsync

        if kvs.Length > 0 then
          kvs |> Rendering.renderKvList |> Console.writeLines

        return true |> Bool.toRc
      }

    cla.OnExecuteAsync(exec)

  let dbCmd (sp: ServiceProvider) (cla: CommandLineApplication) =
    let cla = cla |> descr "DB management."

    cla.Command(
      "show",
      (fun a ->
        a |> descr "Show current DB details." |> ignore

        let exec (cts) = DatabaseCommands.show (repo sp)

        a.OnExecuteAsync(exec))
    )
    |> ignore

    cla.Command(
      "set",
      (fun a ->
        a |> descr "Set the current DB." |> ignore

        let name = nameArg a

        let exec (cts) = DatabaseCommands.set (configRepo sp) (repo sp) name

        a.OnExecuteAsync(exec))
    )
    |> ignore

    cla.Command(
      "export",
      (fun a ->
        a |> descr "Exports the DB contents to a file." |> ignore

        let fileName = fileNameArg a

        let exec (cts) =
          DatabaseCommands.export (repo sp) fileName

        a.OnExecuteAsync(exec))
    )
    |> ignore

    cla.Command(
      "import",
      (fun a ->
        a |> descr "Imports data into the DB." |> ignore

        let fileName = fileNameArg a

        let exec (cts) =
          DatabaseCommands.import (repo sp) fileName

        a.OnExecuteAsync(exec))
    )
    |> ignore

    cla.OnExecute(fun () -> cla.ShowHelp())
