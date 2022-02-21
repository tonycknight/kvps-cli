namespace kvps.tests.KeyValues

open System
open kvps
open kvps.KeyValues
open FsCheck.Xunit
open Xunit

module EntityMappingTests=
    
    [<Property(Verbose = true)>]
    let ``mapToKvData always has a version``(value: KeyValue)=
        let r = EntityMapping.mapToKvData value

        r.version |> Strings.notEmpty
        

    [<Property(Verbose = true)>]
    let ``mapToKvData maps the key``(value: KeyValue)=
        let r = EntityMapping.mapToKvData value

        r._id = value.key

    [<Property(Verbose = true)>]
    let ``mapToKvData maps the value``(value: KeyValue)=
        let r = EntityMapping.mapToKvData value

        r.value = value.value
    
    [<Property(Verbose = true)>]
    let ``mapToKvData maps isSecret``(value: KeyValue)=
        let r = EntityMapping.mapToKvData value

        r.isSecret = value.isSecret

    [<Property(Verbose = true)>]
    let ``mapToKvData maps the tags``(value: KeyValue)=
        let r = EntityMapping.mapToKvData value

        r.tags = value.tags


    
    [<Property(Verbose = true)>]
    let ``mapToKv maps the key``(value: KeyValueData)=
        let r = EntityMapping.mapToKv value

        r.key = value._id


    [<Property(Verbose = true)>]
    let ``mapToKv maps the value``(value: KeyValueData)=
        let r = EntityMapping.mapToKv value

        r.value = value.value
    
    [<Property(Verbose = true)>]
    let ``mapToKv maps isSecret``(value: KeyValueData)=
        let r = EntityMapping.mapToKv value

        r.isSecret = value.isSecret

    [<Property(Verbose = true)>]
    let ``mapToKv maps the tags``(value: KeyValueData)=
        let r = EntityMapping.mapToKv value

        r.tags = value.tags

    


    [<Property(Verbose = true)>]
    let ``mergeTags maps the tags``(value1: KeyValue) (value2: KeyValue)=
        let r = EntityMapping.mergeTags value1 value2

        r.tags = value1.tags
