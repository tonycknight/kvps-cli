namespace kvps.KeyValues

open System
open kvps

type KeyValueImporter() =
  let cleanTags (kv: KeyValue) =
          kv.tags
          |> Option.nullToOption
          |> Option.map (fun xs -> xs |> Seq.filter (String.IsNullOrWhiteSpace >> not) |> Array.ofSeq)
          |> Option.defaultValue [||]

  let validateKeyValues values =
          values
          |> Seq.map (fun kv ->
            if String.IsNullOrWhiteSpace(kv.key) || String.IsNullOrWhiteSpace(kv.value) then
              Some "Invalid key/value data found."
            else
              None)
          |> Seq.flattenSomes
          |> Seq.truncate 1
          |> Strings.join Environment.NewLine

  let apply (repo: IKeyValueRepository) values =
    task {
      let mutable count = 0
      let mutable errorCount = 0

      for kv in values do
        let! x = repo.SetValueAsync(kv)

        if x then
          count <- count + 1
        else
          errorCount <- errorCount + 1

      return (count, errorCount)
    }

  interface IKeyValueImporter with
    member this.ExportAsync repo password path =
      task {
        let! kvs = repo.ListKeysAsync([||])
        let export = { KeyValueExport.empty with data = kvs }
        let path = path |> Io.resolvePath

        export
        |> Newtonsoft.Json.JsonConvert.SerializeObject
        |> Encryption.encrypt password
        |> Strings.toBase64
        |> Io.writeFile path

        return
          { KeyValueExportResult.filePath = path
            successes = kvs.Length
            failures = 0 }
      }

    member this.ImportAsync repo password path =
      task {
        let path = path |> Io.resolvePath |> Io.readFile

        let import =
          path
          |> Strings.fromBase64
          |> Encryption.decrypt password
          |> Newtonsoft.Json.JsonConvert.DeserializeObject<KeyValueExport>

        if
          Object.ReferenceEquals(import, null)
          || Object.ReferenceEquals(import.data, null)
        then
          invalidOp "The file was not a valid import file, or the password is incorrect."

        let data =
          { import with
              data = import.data |> Array.map (fun kv -> { kv with tags = cleanTags kv }) }
                      
        let errors = validateKeyValues data.data
          
        if errors.Length > 0 then
          failwith errors

        let! (count, errorCount) = apply repo data.data
        
        return
          { KeyValueExportResult.filePath = path
            successes = count 
            failures = errorCount }
      }
