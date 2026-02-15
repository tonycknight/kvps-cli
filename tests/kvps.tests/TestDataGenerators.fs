namespace kvps.tests

open System
open FsCheck
open FsCheck.FSharp
open kvps.KeyValues

type Password = { value: string }

type AlphanumericStrings =
  static member Generate() : Arbitrary<string> =
    let isValid (s: string) = (String.IsNullOrEmpty s |> not) && s |> Seq.forall Char.IsLetterOrDigit
    ArbMap.defaults
    |> ArbMap.generate<string>
    |> Gen.filter isValid
    |> Arb.fromGen

type Passwords =
  static member Generate() : Arbitrary<Password> =
    let isValid (s: string) = s.Length <= 16

    ArbMap.defaults
    |> ArbMap.generate<string>
    |> Gen.filter isValid
    |> Gen.map (fun s -> { value = s })
    |> Arb.fromGen

type KeyValueArbitrary =
  static member Generate() : Arbitrary<KeyValue> =
    let names = AlphanumericStrings.Generate().Generator |> Gen.map (fun s -> $"name-{s}")
    let values = AlphanumericStrings.Generate().Generator |> Gen.map (fun s -> $"value-{s}")
        
    names
    |> Gen.zip values
    |> Gen.map (fun (v,n) -> { KeyValue.key = n; KeyValue.value = v; KeyValue.isSecret = false; KeyValue.tags = [||] })
    |> Arb.fromGen

type UniqueKeyValues =
  static member Generate() : Arbitrary<KeyValue> =
    let kvs = KeyValueArbitrary.Generate().Generator
    let guids = ArbMap.defaults |> ArbMap.generate<Guid>

    KeyValueArbitrary.Generate().Generator
    |> Gen.zip guids
    |> Gen.map (fun (g,kv) -> { kv with key = $"{kv.key}{g}"} )      
    |> Arb.fromGen