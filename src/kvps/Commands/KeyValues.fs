namespace kvps.Commands

open kvps
open kvps.KeyValues
open McMaster.Extensions.CommandLineUtils

module KeyValues =

  let setValue
    (repo: IKeyValueRepository)
    (key: CommandArgument)
    (value: CommandOption)
    (isVisible: CommandOption)
    (isSecret: CommandOption)
    (tags: CommandOption)
    =
    task {
      let value =
        { KeyValue.key = key.Value |> Strings.trim
          value = value.Value() |> Strings.trim
          tags = (tags.Values |> Strings.trimSeq |> Seq.distinct |> Array.ofSeq)
          isSecret = (isSecret.HasValue()) || not (isVisible.HasValue()) }

      let! r = value |> repo.SetValueAsync

      return r |> Bool.toRc
    }

  let getValue
    (repo: IKeyValueRepository)
    (key: CommandArgument)
    (reveal: CommandOption)
    (valueOnly: CommandOption)
    (copyClipboard: CommandOption)
    =
    task {
      let! v = key.Value |> Strings.trim |> repo.GetValueAsync

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

  let deleteKey (repo: IKeyValueRepository) (key: CommandArgument) =
    task {
      let! r = key.Value |> Strings.trim |> repo.DeleteKeyAsync

      return r |> Bool.toRc
    }

  let listKeys (repo: IKeyValueRepository) (tags: CommandOption) =
    task {
      let! kvs =
        tags.Values
        |> Strings.trimSeq
        |> Seq.distinct
        |> Array.ofSeq
        |> repo.ListKeysAsync

      if kvs.Length > 0 then
        kvs |> Rendering.renderKvList |> Console.writeLines

      return true |> Bool.toRc
    }
