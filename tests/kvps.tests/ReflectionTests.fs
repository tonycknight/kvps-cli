namespace kvps.tests

open System
open kvps
open FsUnit.Xunit
open Xunit

module ReflectionTests=

    [<Fact>]
    let ``getAsm() returns assembly``()=
        let asm = Reflection.getAsm()

        asm |> should not' (equal null)

    [<Fact>]
    let ``getAttrs returns attributes``()=
        let attrs = Reflection.getAsm()
                        |> Reflection.getAttrs 
                        |> Array.ofSeq

        attrs |> should not' (haveLength 0)

    [<Fact>]
    let ``getCopyrightValue returns non-empty``()=
        let attr = Reflection.getAsm()
                    |> Reflection.getAttrs 
                    |> Reflection.getCopyrightValue
                            
        attr |> should not' (equal None)
        attr.Value |> should not' (be Strings.notEmpty)

    [<Fact>]
    let ``getVersionValue returns non-empty``()=
        let attr = Reflection.getAsm()
                    |> Reflection.getAttrs 
                    |> Reflection.getVersionValue
                            
        attr |> should not' (equal None)
        attr.Value |> should not' (be Strings.notEmpty)
