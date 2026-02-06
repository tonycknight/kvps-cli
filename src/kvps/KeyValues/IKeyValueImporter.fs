namespace kvps.KeyValues

open System.Threading.Tasks

type IKeyValueImporter =
  abstract member ExportAsync: IKeyValueRepository -> string -> string -> Task<KeyValueExportResult>
  abstract member ImportAsync: IKeyValueRepository -> string -> string -> Task<KeyValueExportResult>
