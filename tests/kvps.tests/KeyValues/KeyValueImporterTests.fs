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

  [<Property(Arbitrary = [|typeof<Passwords>; typeof<UniqueKeyValues>|])>]
  let ``ExportAsync ImportAsync are symmetric`` (db1Name: Guid, db2Name: Guid, password: Password, exportName: Guid, keyValue: KeyValue) =
    task {
      let repo1 = db1Name.ToString() |> config |> repo
      let repo2 = db2Name.ToString() |> config |> repo

      // build repos      
      let! r1 = repo1.SetValueAsync keyValue
      r1 |> should equal true

      let importer = new KeyValueImporter() :> IKeyValueImporter

      // export it
      let exportPath = exportName.ToString() |> exportPath      
      let! exportResult = importer.ExportAsync repo1 password.value exportPath
      exportResult.failures |> should equal 0
      exportResult.successes |> should equal 1
      exportResult.filePath |> should equal exportPath

      // import it
      let! importResult = importer.ImportAsync repo2 password.value exportPath

      // verify
      importResult.failures |> should equal 0
      importResult.successes |> should equal 1
      importResult.filePath |> should equal exportPath

      let! kv2 = repo2.GetValueAsync keyValue.key
      
      kv2.Value.value |> should equal keyValue.value

      return true
    }


  // TODO: invalid passwords!