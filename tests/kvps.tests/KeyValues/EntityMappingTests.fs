namespace kvps.tests.KeyValues

open kvps
open kvps.KeyValues
open kvps.tests
open FsCheck.Xunit

module EntityMappingTests =

  [<Property(Verbose = true)>]
  let ``mapToKvData / mapToKv symmetrically maps the value`` (value: KeyValue) =
    let r = value |> EntityMapping.mapToKvData |> EntityMapping.mapToKv

    r.value = value.value

  [<Property(Verbose = true)>]
  let ``mapToKvData always has a version`` (value: KeyValue) =
    let r = EntityMapping.mapToKvData value

    r.version |> Strings.notEmpty

  [<Property(Verbose = true)>]
  let ``mapToKvData maps the key`` (value: KeyValue) =
    let r = EntityMapping.mapToKvData value

    r._id = value.key

  [<Property(Verbose = true)>]
  let ``mapToKvData maps isSecret`` (value: KeyValue) =
    let r = EntityMapping.mapToKvData value

    r.isSecret = value.isSecret

  [<Property(Verbose = true)>]
  let ``mapToKvData maps the tags`` (value: KeyValue) =
    let r = EntityMapping.mapToKvData value

    r.tags = value.tags

  [<Property(Arbitrary = [| typeof<V1KeyValueDataArbitrary> |], Verbose = true)>]
  let ``mapToKv v1 maps the key`` (value: KeyValueData) =
    let r = EntityMapping.mapToKv value

    r.key = value._id

  [<Property(Arbitrary = [| typeof<V1KeyValueDataArbitrary> |], Verbose = true)>]
  let ``mapToKv v1 maps the value`` (value: KeyValueData) =
    let r = EntityMapping.mapToKv value

    r.value = value.value

  [<Property(Arbitrary = [| typeof<V1KeyValueDataArbitrary> |], Verbose = true)>]
  let ``mapToKv v1 maps isSecret`` (value: KeyValueData) =
    let r = EntityMapping.mapToKv value

    r.isSecret = value.isSecret

  [<Property(Arbitrary = [| typeof<V1KeyValueDataArbitrary> |], Verbose = true)>]
  let ``mapToKv v1 maps the tags`` (value: KeyValueData) =
    let r = EntityMapping.mapToKv value

    r.tags = value.tags

  [<Property(Arbitrary = [| typeof<V1KeyValueDataArbitrary> |], Verbose = true)>]
  let ``mapToKv v1/v2 upgrade maps the key`` (value: KeyValueData) =
    let r = value |> EntityMapping.mapToKv |> EntityMapping.mapToKvData

    r._id = value._id

  [<Property(Arbitrary = [| typeof<V1KeyValueDataArbitrary> |], Verbose = true)>]
  let ``mapToKv v1/v2 upgrade maps the version`` (value: KeyValueData) =
    let r = value |> EntityMapping.mapToKv |> EntityMapping.mapToKvData

    r.version = EntityMapping.V2KeyValueData

  [<Property(Arbitrary = [| typeof<V1KeyValueDataArbitrary> |], Verbose = true)>]
  let ``mapToKv v1/v2 upgrade maps isSecret`` (value: KeyValueData) =
    let r = value |> EntityMapping.mapToKv |> EntityMapping.mapToKvData

    r.isSecret = value.isSecret

  [<Property(Arbitrary = [| typeof<V1KeyValueDataArbitrary> |], Verbose = true)>]
  let ``mapToKv v1/v2 upgrade maps the tags`` (value: KeyValueData) =
    let r = value |> EntityMapping.mapToKv |> EntityMapping.mapToKvData

    r.tags = value.tags

  [<Property(Arbitrary = [| typeof<V1KeyValueDataArbitrary> |], Verbose = true)>]
  let ``mapToKv v1/v2 upgrade maps the value`` (value: KeyValueData) =
    let r =
      value
      |> EntityMapping.mapToKv
      |> EntityMapping.mapToKvData
      |> EntityMapping.mapToKv

    r.value = value.value

  [<Property(Arbitrary = [| typeof<LatestVersionKeyValueDataArbitrary> |], Verbose = true)>]
  let ``mapToKv maps the key`` (value: KeyValueData) =
    let r = EntityMapping.mapToKv value

    r.key = value._id

  [<Property(Arbitrary = [| typeof<LatestVersionKeyValueDataArbitrary> |], Verbose = true)>]
  let ``mapToKv maps isSecret`` (value: KeyValueData) =
    let r = EntityMapping.mapToKv value

    r.isSecret = value.isSecret

  [<Property(Arbitrary = [| typeof<LatestVersionKeyValueDataArbitrary> |], Verbose = true)>]
  let ``mapToKv maps the tags`` (value: KeyValueData) =
    let r = EntityMapping.mapToKv value

    r.tags = value.tags

  [<Property(Verbose = true)>]
  let ``mergeTags maps the tags`` (value1: KeyValue) (value2: KeyValue) =
    let r = EntityMapping.mergeTags value1 value2

    r.tags = value1.tags
