namespace kvps.tests.Config

open System
open System.Diagnostics.CodeAnalysis
open kvps.Config
open FsUnit.Xunit
open Xunit

[<Trait("OS", "Windows")>]
[<ExcludeFromCodeCoverage>]
module EnvVarsConfigRepositoryTests =

  [<Fact>]
  let ``Value() returns null value`` () =
    let repo = new EnvVarsConfigRepository()
    let name = Guid.NewGuid().ToString()

    let r = (repo :> IConfigRepository).Value(name)

    Object.ReferenceEquals(r, null) |> should be True

  [<Fact>]
  let ``Value() & Set() are symmetric`` () =
    let repo = new EnvVarsConfigRepository() :> IConfigRepository
    let name = Guid.NewGuid().ToString()
    let value = Guid.NewGuid().ToString()

    repo.Set(name, value)

    let r = repo.Value(name)

    r |> should equal value
