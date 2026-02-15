namespace kvps.tests.KeyValues

open System
open kvps.Config
open kvps.KeyValues
open kvps.tests
open FsCheck.Xunit
open FsUnit.Xunit
open NSubstitute

module KeyValueImporterTests =

  let private config dbName =
    let s = Substitute.For<kvps.Config.IConfigProvider>()

    let c =
      { kvps.Config.Configuration.Default with
          dbName = dbName }

    s.Get().Returns(c) |> ignore
    s

  let private repo config =
    new LiteDbKeyValueRepository(config) :> IKeyValueRepository

  let private exportPath name =
    let dir = System.Environment.CurrentDirectory
    let file = "dbexport.data"
    let path = System.IO.Path.Join [| dir; "testdata"; name; file |]
    path

  let rec private setMany (repo: IKeyValueRepository) (keyvalues: KeyValue list) (results: bool list) =
    task {
      return!
        match keyvalues with
        | [] -> 
          task { return results }
        | h::t -> 
          task {
            let! r = repo.SetValueAsync h
            return! setMany repo t (r :: results)
          }
    }

  let private getMany (repo: IKeyValueRepository) (keys: string seq) =
    let rec getManyInner (repo: IKeyValueRepository) (keys: string list) (results: KeyValue list)=
      task {
        return!
          match keys with
          | [] -> task { return results }
          | h::t ->
            task {
              let! kv = repo.GetValueAsync h
              let results = kv |> Option.map (fun k -> k :: results) |> Option.defaultValue results

              return! getManyInner repo t results
            }
      }

    task {
      let! results = getManyInner repo (keys |> List.ofSeq) []
      return results |> List.sortBy _.key
    }


  [<Property(Arbitrary = [|typeof<Passwords>; typeof<UniqueKeyValues>|])>]
  let ``ExportAsync ImportAsync are symmetric`` (db1Name: Guid, db2Name: Guid, password: Password, exportName: Guid, keyValues: KeyValue[]) =
    task {
      let keyValues = keyValues |> Array.sortBy _.key
      let repo1 = db1Name.ToString() |> config |> repo
      let repo2 = db2Name.ToString() |> config |> repo

      // build repo
      let! setResults = setMany repo1 (List.ofSeq keyValues) []
      setResults |> Seq.forall id |> should equal true
      
      let importer = new KeyValueImporter() :> IKeyValueImporter

      // export it
      let exportPath = exportName.ToString() |> exportPath      
      let! exportResult = importer.ExportAsync repo1 password.value exportPath
      exportResult.failures |> should equal 0
      exportResult.successes |> should equal keyValues.Length
      exportResult.filePath |> should equal exportPath

      // import it
      let! importResult = importer.ImportAsync repo2 password.value exportPath

      // verify
      importResult.failures |> should equal 0
      importResult.successes |> should equal keyValues.Length
      importResult.filePath |> should equal exportPath


      let keys = keyValues |> Seq.map _.key
      let! keyValues2 = getMany repo2 keys
            
      keyValues 
      |> Seq.compareWith (fun x y -> match x = y with | true -> 0 | false  -> 1 ) keyValues2 
      |> should equal 0
      
      return true
    }


  // TODO: invalid passwords!