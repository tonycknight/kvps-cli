namespace kvps

open System
open kvps.KeyValues

module Rendering =
  let renderTags (kv: KeyValue) =
    kv.tags |> Seq.sort |> Seq.map Strings.green |> Strings.join ", "

  let renderDbInfo (value: DbInfo) =
    [ sprintf "Name:\t%s" value.name; sprintf "Count:\t%i" value.kvCount ]

  let renderKvValue showSecrets (kv: KeyValue) =
    if (not kv.isSecret) || showSecrets then
      kv.value
    else
      Strings.stars kv.value

  let renderKv renderValue (kv: KeyValue) =
    let lines =
      [ ("Key:", kv.key |> Strings.cyan)
        ("Value:", kv |> renderValue |> Strings.cyan)
        ("Tags:", kv |> renderTags) ]

    let len = lines |> Seq.map fst |> Strings.maxLength

    let lines =
      lines
      |> Seq.map (fun (k, v) ->
        let k = Strings.padR len k
        sprintf "%s\t%s" k v)

    let meta = if kv.isSecret then [ Strings.red "Secret" ] else []

    meta |> Seq.append lines |> Seq.filter (fun s -> s.Length > 0)

  let renderKvList (kvs: seq<KeyValue>) =
    let len = kvs |> Seq.map (fun kv -> kv.key) |> Strings.maxLength

    kvs
    |> Seq.map (fun kv -> sprintf "%s\t%s" (Strings.padR len kv.key) (renderTags kv))
