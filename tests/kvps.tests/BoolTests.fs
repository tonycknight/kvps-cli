namespace kvps.tests

open kvps
open FsCheck.Xunit

module BoolTests =
  [<Property(Verbose = true)>]
  let ``toRc returns integer`` (value: bool) =
    let f = if value then (=) 0 else (<) 0

    value |> Bool.toRc |> f
