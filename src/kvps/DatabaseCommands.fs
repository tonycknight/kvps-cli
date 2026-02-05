namespace kvps

open System
open McMaster.Extensions.CommandLineUtils
open kvps.KeyValues

module DatabaseCommands =

  let export (repo: IKeyValueRepository) (fileName: CommandArgument) =
    task {
      let! kvs = repo.ListKeysAsync([||])
      let data = { KeyValueExport.empty with data = kvs }

      let js = Newtonsoft.Json.JsonConvert.SerializeObject(data)
      let f = fileName.Value |> Io.resolvePath
      js |> Io.writeFile f
      f |> sprintf "Written to %s" |> Console.writeLine

      return true |> Bool.toRc
    }

  let import (repo: IKeyValueRepository) (fileName: CommandArgument) =
    task {
      let js = fileName.Value |> Io.resolvePath |> Io.readFile

      let data = Newtonsoft.Json.JsonConvert.DeserializeObject<KeyValueExport>(js)

      if Object.ReferenceEquals(data, null) || Object.ReferenceEquals(data.data, null) then
        invalidOp "The file was not a valid JSON file."

      let cleanTags (kv: KeyValue) =
        kv.tags
        |> Option.nullToOption
        |> Option.map (fun xs -> xs |> Seq.filter (String.IsNullOrWhiteSpace >> not) |> Array.ofSeq)
        |> Option.defaultValue [||]

      let data =
        { data with
            data = data.data |> Array.map (fun kv -> { kv with tags = cleanTags kv }) }

      let validationErrors =
        data.data
        |> Seq.map (fun kv ->
          if String.IsNullOrWhiteSpace(kv.key) || String.IsNullOrWhiteSpace(kv.value) then
            Some "Invalid key/value data found."
          else
            None)
        |> Seq.flattenSomes
        |> Seq.truncate 1
        |> Strings.join Environment.NewLine

      if validationErrors.Length > 0 then
        failwith validationErrors

      let mutable count = 0

      for kv in data.data do
        let! x = repo.SetValueAsync(kv)

        if x then
          count <- count + 1

      count |> sprintf "%i value(s) imported." |> Console.writeLine

      return true |> Bool.toRc
    }