namespace kvps.tests.Config

open System
open kvps.Config
open FsUnit.Xunit
open NSubstitute
open Xunit

module AggregateConfigProviderTests =

    [<Fact>]
    let ``Get() returns dbName from repo`` () =
        let dbName = Guid.NewGuid().ToString()

        let repo = Substitute.For<IConfigRepository>()
        repo.Value(Arg.Is("dbName")).Returns(dbName) |> ignore

        let p = new AggregateConfigProvider(repo) :> IConfigProvider

        let r = p.Get()

        r.dbName |> should equal dbName

    [<Fact>]
    let ``Get() returns dbName from default`` () =
        let dbName: string = null

        let repo = Substitute.For<IConfigRepository>()
        repo.Value(Arg.Is("dbName")).Returns(dbName) |> ignore

        let p = new AggregateConfigProvider(repo) :> IConfigProvider

        let r = p.Get()

        r.dbName |> should equal Configuration.Default.dbName
