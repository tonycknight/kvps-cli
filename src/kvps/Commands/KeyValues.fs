namespace kvps.Commands

open kvps
open kvps.KeyValues
open McMaster.Extensions.CommandLineUtils

module KeyValues =

  let setValue (repo: IKeyValueRepository) (key: CommandArgument) (value: CommandOption) (isVisible: CommandOption) (isSecret: CommandOption) (tags: CommandOption) =
    task {
      let value =
          { KeyValue.key = key.Value |> Strings.trim
            value = value.Value() |> Strings.trim
            tags = (tags.Values |> Strings.trimSeq |> Seq.distinct |> Array.ofSeq)
            isSecret = (isSecret.HasValue()) || not (isVisible.HasValue()) }

      let! r = value |> repo.SetValueAsync

      return r |> Bool.toRc
    }