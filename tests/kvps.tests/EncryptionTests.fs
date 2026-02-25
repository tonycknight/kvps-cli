namespace kvps.tests

open kvps
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
