namespace kvps

open Tk.Nuget

module Nuget =
    [<Literal>]
    let packageId = "kvps-cli"

    let private client () = new NugetClient()

    let getUpgradeVersion (currentVersion) =
        match client () |> _.GetUpgradeVersionAsync(packageId, currentVersion, false).Result with
        | null -> None
        | x -> Some x
