
namespace kvps.KeyValues

open System.Diagnostics.CodeAnalysis

[<ExcludeFromCodeCoverage>]
type MemoryKeyValueRepository()=   
    
    let mutable dict = new System.Collections.Generic.Dictionary<string, KeyValue>(System.StringComparer.InvariantCultureIgnoreCase)

    do dict["aaa"] <- { key = "aaa"; value = "vvv"; isSecret = false; tags = [||] }

    interface IKeyValueRepository with  
        member this.GetValueAsync(key) =
            task {                
                return match dict.TryGetValue(key) with
                        | (false, _) -> None
                        | (true, kvp) -> Some kvp
            }

        member this.SetValueAsync(kv) =
            task {
                dict[kv.key] <- kv

                return true
            }

        member this.ListKeysAsync(tags)=
            task {
                return dict.Values |> Array.ofSeq
            }

        member this.DeleteKeyAsync(key)=
            task{
                return dict.Remove(key)
            }

        member this.GetDbInfoAsync()=
            task {                
                return { DbInfo.name = "memory"; kvCount = dict.Count }
            }
