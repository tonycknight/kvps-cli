namespace kvps.Commands

open System
open McMaster.Extensions.CommandLineUtils
open kvps
open kvps.KeyValues

module Database =

  let private renderImportResults (result: KeyValueExportResult) =
    let lines =
      [ sprintf "File: %s" result.filePath
        sprintf "Successes: %i" result.successes
        sprintf "Failures: %i" result.failures ]

    lines |> Console.writeLines

    (result.failures = 0)

  let show (repo: IKeyValueRepository) =
    task {
      let! info = repo.GetDbInfoAsync()

      info |> Rendering.renderDbInfo |> Console.writeLines

      return true |> Bool.toRc
    }

  let set (configRepo: Config.IConfigRepository) (repo: IKeyValueRepository) (name: CommandArgument) =
    task {
      configRepo.Set(nameof Unchecked.defaultof<Config.Configuration>.dbName, name.Value)

      let! info = repo.GetDbInfoAsync()

      info |> Rendering.renderDbInfo |> Console.writeLines

      return true |> Bool.toRc
    }

  let export
    (importer: IKeyValueImporter)
    (repo: IKeyValueRepository)
    (fileName: CommandArgument)
    (password: CommandOption)
    =
    task {
      let! result = importer.ExportAsync repo (password.Value()) (fileName.Value)

      return result |> renderImportResults |> Bool.toRc
    }

  let import
    (importer: IKeyValueImporter)
    (repo: IKeyValueRepository)
    (fileName: CommandArgument)
    (password: CommandOption)
    =
    task {
      let! result = importer.ImportAsync repo (password.Value()) (fileName.Value)

      return result |> renderImportResults |> Bool.toRc
    }
