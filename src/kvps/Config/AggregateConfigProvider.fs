namespace kvps.Config

open kvps

type AggregateConfigProvider(configRepo: IConfigRepository) =

  let getFromRepo () =
    let config = Configuration.Default

    { config with
        dbName =
          configRepo.Value(nameof Unchecked.defaultof<Configuration>.dbName)
          |> Option.nullToDefault config.dbName
    }

  interface IConfigProvider with
    member this.Get() = getFromRepo ()
