namespace kvps.tests

open kvps
open FsCheck.FSharp
open FsCheck.Xunit
open Xunit

module Encryption =

  [<Property(Arbitrary = [| typeof<Passwords> |], Verbose = true, MaxTest = 1000)>]
  let ``encrypt decrypt is symmetric`` (value: string) (password: Password) =

    let prop = Encryption.encrypt password.value >> Encryption.decrypt password.value

    prop value = value

  [<Property(Verbose = true, MaxTest = 1000)>]
  [<Trait("OS", "Windows")>]
  let ``dpapiEncrypt dpapiDecrypt is symmetric`` (value: string) =
    let prop = Encryption.dpapiEncrypt >> Encryption.dpapiDecrypt

    value = prop value

  [<Property(Verbose = true, MaxTest = 1000)>]
  [<Trait("OS", "Windows")>]
  let ``dpapiEncrypt is not deterministic`` () =
    let prop value =
      let results = [| 0..11 |] |> Array.map (fun _ -> Encryption.dpapiEncrypt value)

      let grps = results |> Array.groupBy id

      Seq.length grps = results.Length

    Prop.forAll
      (ArbMap.defaults
       |> ArbMap.generate<string>
       |> Gen.filter (fun s -> s.Length > 0)
       |> Arb.fromGen)
      prop
