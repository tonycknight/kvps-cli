namespace kvps.KeyValues

open System.Threading.Tasks

type IKeyValueImporter =
  abstract member ExportAsync:
    repo: IKeyValueRepository -> password: string -> filePath: string -> Task<KeyValueExportResult>

  abstract member ImportAsync:
    repo: IKeyValueRepository -> password: string -> filePath: string -> Task<KeyValueExportResult>
