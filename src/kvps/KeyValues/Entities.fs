namespace kvps.KeyValues

type KeyValue =
  { key: string
    value: string
    isSecret: bool
    tags: string[] }

type KeyValueExport =
  { version: string
    data: KeyValue[] }

  static member empty =
    { KeyValueExport.version = "1.0"
      data = [||] }

type KeyValueExportResult =
  { filePath: string
    successes: int
    failures: int }

[<CLIMutable>]
type KeyValueData =
  { _id: string
    version: string
    value: string
    isSecret: bool
    tags: string[] }

type DbInfo = { name: string; kvCount: int }

module EntityMapping =
  open kvps

  [<Literal>]
  let internal V1KeyValueData = "v1"

  [<Literal>]
  let internal V2KeyValueData = "v2"

  let private mapToKvV1 (kv: KeyValueData) =
    { KeyValue.key = kv._id
      value = kv.value
      tags = kv.tags
      isSecret = kv.isSecret }

  let private mapToKvV2 (kv: KeyValueData) =
    { KeyValue.key = kv._id
      value = Encryption.dpapiDecrypt kv.value
      tags = kv.tags
      isSecret = kv.isSecret }

  let private mapToKvFunc version =
    match version with
    | V1KeyValueData -> mapToKvV1
    | V2KeyValueData -> mapToKvV2
    | _ -> invalidOp $"Unrecognised version {version}"

  let mapToKv (kv: KeyValueData) =
    let map = mapToKvFunc kv.version

    map kv

  let mapToKvData (kv: KeyValue) =
    { KeyValueData._id = kv.key
      version = V2KeyValueData
      value = Encryption.dpapiEncrypt kv.value
      tags = kv.tags
      isSecret = kv.isSecret }

  let mergeTags (ekv: KeyValue) (kv: KeyValue) = { kv with tags = ekv.tags }
