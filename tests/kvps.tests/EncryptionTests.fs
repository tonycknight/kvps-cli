namespace kvps.tests

open kvps
open FsCheck.Xunit

module Encryption = 

  [<Property(Arbitrary = [| typeof<Passwords> |], Verbose = true)>]
  let ``encrypt decrypt is symmetric`` (value: string) (password: Password)=
    
    let prop = Encryption.encrypt password.value >> Encryption.decrypt password.value

    prop value = value