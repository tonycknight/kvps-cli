namespace kvps.tests

open System
open kvps
open FsUnit.Xunit
open Xunit

[<Trait("OS", "Windows")>]
module IoTests =

    [<Theory>]
    [<InlineData("abc.txt")>]
    let ``Io.filePath creates directory`` (fileName) =
        let fullPath = Io.filePath "folder" fileName

        let dir = System.IO.Path.GetDirectoryName(fullPath)

        System.IO.Directory.Exists(dir) |> should be True

    [<Theory>]
    [<InlineData("abc.txt")>]
    let ``Io.filePath returns path with name`` (fileName) =
        let fullPath = Io.filePath "folder" fileName

        let result = System.IO.Path.GetFileName(fullPath)

        result |> should equal fileName
