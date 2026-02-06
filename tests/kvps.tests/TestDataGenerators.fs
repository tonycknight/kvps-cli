namespace kvps.tests

open FsCheck
open FsCheck.FSharp

type Password =
  { value: string }

type Passwords =
  static member Generate() : Arbitrary<Password> =
    let isValid (s: string) = s.Length <= 16

    ArbMap.defaults
        |> ArbMap.generate<string>
        |> Gen.filter isValid
        |> Gen.map (fun s -> { value = s })
        |> Arb.fromGen
    
