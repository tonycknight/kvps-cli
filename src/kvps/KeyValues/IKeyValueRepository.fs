namespace kvps.KeyValues

open System
open System.Threading.Tasks

type IKeyValueRepository=
    abstract member GetValueAsync : string -> Task<KeyValue option>
    abstract member SetValueAsync : KeyValue -> Task<bool>
    abstract member ListKeysAsync : string[] -> Task<KeyValue[]>
    abstract member DeleteKeyAsync : string -> Task<bool>
    abstract member GetDbInfoAsync : unit -> Task<DbInfo>
