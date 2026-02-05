namespace kvps.tests.KeyValues

open System
open kvps.Config
open kvps.KeyValues
open FsUnit.Xunit
open NSubstitute
open Xunit

module LiteDbKeyValueRepositoryTests =

  let private config dbName =
    let s = Substitute.For<kvps.Config.IConfigProvider>()

    let c =
      { kvps.Config.Configuration.Default with
          dbName = dbName
      }

    s.Get().Returns(c) |> ignore
    s

  let private repo config =
    new LiteDbKeyValueRepository(config) :> IKeyValueRepository

  [<Fact>]
  let ``ctor smoke`` () =
    let config = config "test"
    let repo = repo config

    repo |> should not' (equal null)

  [<Fact>]
  let ``set and get are symmetric`` () =
    task {
      let config = Guid.NewGuid().ToString() |> config

      let repo = repo config

      let kv =
        {
          KeyValue.key = "aaa"
          value = "bbb"
          isSecret = false
          tags = [||]
        }

      let! r1 = repo.SetValueAsync kv

      let! r2 = repo.GetValueAsync kv.key

      r2 |> should equal (Some kv)

      return true
    }

  [<Fact>]
  let ``set can change tags`` () =
    task {
      let config = Guid.NewGuid().ToString() |> config

      let repo = repo config

      let tags1 = [| 1..3 |] |> Array.map (fun x -> x.ToString())

      let kv =
        {
          KeyValue.key = "aaa"
          value = "bbb"
          isSecret = false
          tags = tags1
        }

      let! r1 = repo.SetValueAsync kv

      let tags2 = [| 3..6 |] |> Array.map (fun x -> x.ToString())
      let kv2 = { kv with tags = tags2 }

      let! r2 = repo.SetValueAsync kv2

      let! r3 = repo.GetValueAsync kv.key

      r3 |> should equal (Some kv2)

      return true
    }

  [<Fact>]
  let ``set can retain tags`` () =
    task {
      let config = Guid.NewGuid().ToString() |> config

      let repo = repo config

      let tags = [| 1..3 |] |> Array.map (fun x -> x.ToString())

      let kv =
        {
          KeyValue.key = "aaa"
          value = "bbb"
          isSecret = false
          tags = tags
        }

      let! r1 = repo.SetValueAsync kv

      let kv2 = { kv with tags = [||] }

      let! r2 = repo.SetValueAsync kv2

      let! r3 = repo.GetValueAsync kv.key

      r3 |> should equal (Some kv)

      return true
    }

  [<Fact>]
  let ``list yields all`` () =
    task {
      let config = Guid.NewGuid().ToString() |> config

      let repo = repo config

      let kvs =
        [ 1..3 ]
        |> Seq.map (fun x ->
          {
            KeyValue.key = sprintf "aaa%i" x
            value = "bbb"
            isSecret = false
            tags = [||]
          })
        |> Array.ofSeq

      kvs
      |> Array.iter (fun kv -> (repo.SetValueAsync kv).GetAwaiter().GetResult() |> ignore)

      let! r2 = repo.ListKeysAsync [||]

      r2 |> should haveLength kvs.Length
      r2 |> should equal kvs

      return true
    }

  [<Fact>]
  let ``list by tag yields only tagged`` () =
    task {
      let maxCount = 3
      let config = Guid.NewGuid().ToString() |> config

      let repo = repo config

      let kvs =
        [ 1..maxCount ]
        |> Seq.map (fun x ->
          {
            KeyValue.key = sprintf "aaa%i" x
            value = "bbb"
            isSecret = false
            tags = [| x.ToString() |]
          })
        |> Array.ofSeq

      kvs
      |> Array.iter (fun kv -> (repo.SetValueAsync kv).GetAwaiter().GetResult() |> ignore)

      let ts = [| 2..maxCount |] |> Array.map (fun x -> x.ToString())

      let expected =
        kvs
        |> Array.filter (fun kv -> kv.tags |> Seq.exists (fun t -> ts |> Array.contains t))

      let! r1 = repo.ListKeysAsync ts

      r1 |> should haveLength expected.Length
      r1 |> should equal expected

      return true
    }

  [<Fact>]
  let ``del removes items`` () =
    task {
      let config = Guid.NewGuid().ToString() |> config

      let repo = repo config

      let kvs =
        [ 1..3 ]
        |> Seq.map (fun x ->
          {
            KeyValue.key = sprintf "aaa%i" x
            value = "bbb"
            isSecret = false
            tags = [||]
          })
        |> Array.ofSeq

      kvs
      |> Array.iter (fun kv -> (repo.SetValueAsync kv).GetAwaiter().GetResult() |> ignore)

      let kv = kvs |> Array.skip 1 |> Array.head

      let! r1 = repo.DeleteKeyAsync kv.key

      let! r2 = repo.ListKeysAsync [||]

      let expected = kvs |> Array.filter (fun k -> k <> kv)
      r1 |> should equal true
      expected |> should equal r2

      return true
    }

  [<Fact>]
  let ``fbinfo gives count`` () =
    task {
      let dbName = Guid.NewGuid().ToString()
      let config = dbName |> config

      let repo = repo config

      let kvs =
        [ 1..3 ]
        |> Seq.map (fun x ->
          {
            KeyValue.key = sprintf "aaa%i" x
            value = "bbb"
            isSecret = false
            tags = [||]
          })
        |> Array.ofSeq

      kvs
      |> Array.iter (fun kv -> (repo.SetValueAsync kv).GetAwaiter().GetResult() |> ignore)

      let! r2 = repo.GetDbInfoAsync()

      r2.kvCount |> should equal kvs.Length
      r2.name |> should equal dbName

      return true
    }
