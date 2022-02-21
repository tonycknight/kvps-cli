namespace kvps.KeyValues

open System
open System.IO
open LiteDB
open kvps

type LiteDbKeyValueRepository(config: Config.IConfigProvider)=
    
    let dbName() = config.Get().dbName

    let cn() =        
        dbName() |> sprintf "%s.db" |> Io.dataFilePath |> sprintf "Filename=%s"
        // TODO: to encrypt: sprintf "Filename=%s;Password=%s" path "pw"
        
    let db(cn: string) =         
        let r = new LiteDatabase(cn)
        r

    let col(db: ILiteDatabase) =
        let col = db.GetCollection<KeyValueData>("kvps")        
        col.EnsureIndex(fun x -> x._id) |> ignore
        col.EnsureIndex(fun x -> x.tags) |> ignore
        col
            
    let mergeTags (ekv: KeyValue) (kv: KeyValue)= { kv with tags = ekv.tags}

    let tagsExpr tags =
        let s = tags |> Seq.map (sprintf "tags[*] ANY = '%s'") |> Strings.join " OR "        
        BsonExpression.Create(s)

    let getValueAsyncDb (col: ILiteCollection<KeyValueData>) key =
        task {
            let result = col.Query().Where(fun kv -> kv._id = key).FirstOrDefault()

            return result |> Option.nullToOption |> Option.map EntityMapping.mapToKv            
        }

    let getValueAsync(key) =
        task {                
            use db = cn() |> db
            let col = col db
            
            return! getValueAsyncDb col key        
        }

    let setValueAsync(kv) =
        task {
            use db = cn() |> db
            let col = col db

            let! existingKv = getValueAsyncDb col kv.key
            
            let kvd = match existingKv with
                        | Some ekv when kv.tags.Length = 0 ->   kv |> mergeTags ekv |> EntityMapping.mapToKvData
                        | _ ->                                  EntityMapping.mapToKvData kv

            col.Upsert(kvd) |> ignore

            return true
        }

    let listKeysAsync(tags: string[])=
        task {                
            use db = cn() |> db
            let col = col db

            let query = col.Query()
            
            let query = match tags with
                            | [||] ->   query
                            | ts   ->   let expr = tagsExpr ts
                                        query.Where(expr)
                                        
            return query.ToArray() |> Array.map EntityMapping.mapToKv
        }

    let deleteKeyAsync(key: string)=
        task{
            use db = cn() |> db
            let col = col db

            col.Delete(key) |> ignore
            
            return true
        }

    let getDbInfoAsync() =
        task {
            use db = cn() |> db
            let col = col db
                        
            return { DbInfo.name = dbName(); 
                            kvCount = col.Count() }
        }

    interface IKeyValueRepository with  
        member this.GetValueAsync(key) = getValueAsync(key)

        member this.SetValueAsync(kv) = setValueAsync(kv)

        member this.ListKeysAsync(tags)= listKeysAsync(tags)

        member this.DeleteKeyAsync(key)= deleteKeyAsync(key)

        member this.GetDbInfoAsync()= getDbInfoAsync()

