﻿namespace kvps.KeyValues

open LiteDB
open kvps

type LiteDbKeyValueRepository(config: Config.IConfigProvider) =

    let dbName () = config.Get().dbName

    let db = dbName >> LiteDb.filePath >> LiteDb.connection >> LiteDb.db

    let col (db: ILiteDatabase) =
        let col = db.GetCollection<KeyValueData>("kvps")
        col.EnsureIndex(fun x -> x._id) |> ignore
        col.EnsureIndex(fun x -> x.tags) |> ignore
        col

    let tagsExpr tags =
        let s = tags |> Seq.map (sprintf "tags[*] ANY = '%s'") |> Strings.join " OR "
        BsonExpression.Create(s)

    let getValueAsyncDb key (col: ILiteCollection<KeyValueData>) =
        task {
            let result = col.Query().Where(fun kv -> kv._id = key).FirstOrDefault()

            return result |> Option.nullToOption |> Option.map EntityMapping.mapToKv
        }

    let getValueAsync (key) =
        task {
            use db = db ()

            return! col db |> getValueAsyncDb key
        }

    let setValueAsync (kv) =
        task {
            use db = db ()
            let col = col db

            let! existingKv = col |> getValueAsyncDb kv.key

            let kvd =
                match existingKv with
                | Some ekv when kv.tags.Length = 0 -> kv |> EntityMapping.mergeTags ekv |> EntityMapping.mapToKvData
                | _ -> EntityMapping.mapToKvData kv

            col.Upsert(kvd) |> ignore

            return true
        }

    let listKeysAsync (tags: string[]) =
        task {
            use db = db ()
            let col = col db

            let query = col.Query()

            let query =
                match tags with
                | [||] -> query
                | ts ->
                    let expr = tagsExpr ts
                    query.Where(expr)

            return query.ToArray() |> Array.map EntityMapping.mapToKv
        }

    let deleteKeyAsync (key: string) =
        task {
            use db = db ()
            let col = col db

            col.Delete(key) |> ignore

            return true
        }

    let getDbInfoAsync () =
        task {
            use db = db ()
            let col = col db

            return
                { DbInfo.name = dbName ()
                  kvCount = col.Count() }
        }

    interface IKeyValueRepository with
        member this.GetValueAsync(key) = getValueAsync (key)

        member this.SetValueAsync(kv) = setValueAsync (kv)

        member this.ListKeysAsync(tags) = listKeysAsync (tags)

        member this.DeleteKeyAsync(key) = deleteKeyAsync (key)

        member this.GetDbInfoAsync() = getDbInfoAsync ()
