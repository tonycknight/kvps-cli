namespace kvps.tests

open System
open kvps
open FsUnit.Xunit
open FsCheck
open FsCheck.Xunit

module StringsTests =

  [<Xunit.Theory>]
  [<Xunit.InlineData("a", "b", "c", "", "abc")>]
  [<Xunit.InlineData("a", "b", "c", "+", "a+b+c")>]
  [<Xunit.InlineData("a", "b", "c", "***", "a***b***c")>]
  let ``Strings.join must concatenate`` (v1, v2, v3, delim, expected) =
    let r = [ v1; v2; v3 ] |> Strings.join delim

    r |> should equal expected

  [<Property(Verbose = true)>]
  let ``Strings.star should equal length`` (v: FsCheck.NonEmptyString) =
    let r = Strings.stars v.Get

    r.Length = v.Get.Length

  [<Property(Verbose = true)>]
  let ``Strings.star should be all stars`` (v: FsCheck.NonEmptyString) =
    let r = Strings.stars v.Get

    r |> Seq.exists (fun c -> c <> '*') |> not

  [<Property(Verbose = true)>]
  let ``Strings.padR should pad to length`` (chars: FsCheck.PositiveInt, padding: FsCheck.PositiveInt) =
    let padding = padding.Get
    let expectedLength = if padding < chars.Get then chars.Get else padding

    let s = new String('a', chars.Get)

    let r = Strings.padR padding s

    r.Length = expectedLength

  [<Property(Verbose = true)>]
  let ``Strings.padR should pad`` (s: FsCheck.NonEmptyString, padding: FsCheck.PositiveInt) =
    let padding = padding.Get
    let s = s.Get

    let expected =
      if padding < s.Length then
        s
      else
        let p = new String(' ', padding - s.Length)
        s + p

    Strings.padR padding s = expected

  [<Property(Verbose = true)>]
  let ``Strings.padRSeq should pad all`` (count: PositiveInt) =
    let ss = [| 1 .. count.Get |] |> Array.map (sprintf "aaa%i")

    let len = count.Get + 10

    let result = ss |> Strings.padRSeq len |> Array.ofSeq

    result |> Array.exists (fun s -> s.Length <> len && s.EndsWith(' ')) |> not

  [<Property(Verbose = true)>]
  let ``Strings.maxLength returns longest`` (values: NonEmptyString[]) =
    let values = values |> Array.map (fun s -> s.Get)

    let result = values |> Strings.maxLength

    let longest =
      values
      |> Array.sortByDescending (fun s -> s.Length)
      |> Array.tryHead
      |> Option.defaultValue ""

    result = longest.Length

  [<Property>]
  let ``toBase64 fromBase64 is symmetric`` (value: string) =

    let prop =
      Strings.bytes >> Strings.toBase64 >> Strings.fromBase64 >> Strings.fromBytes

    prop value = value
