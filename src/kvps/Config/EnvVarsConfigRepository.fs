namespace kvps.Config

open System
open System.Diagnostics.CodeAnalysis

[<ExcludeFromCodeCoverage>]
type EnvVarsConfigRepository()=
    let keyName name = sprintf "kvps_config_%s" name

    // Only Windows supports per-user environment variables:
    // https://docs.microsoft.com/en-us/dotnet/api/system.environment.getenvironmentvariable?view=net-6.0
    let getUserEnvVar key = Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.User)
    let setUserEnvVar (key, value) = Environment.SetEnvironmentVariable(key, value, EnvironmentVariableTarget.User)

    interface IConfigRepository with
        member this.Value(key) = key |> keyName |> getUserEnvVar            

        member this.Set(key, value) = (keyName key, value) |> setUserEnvVar            
