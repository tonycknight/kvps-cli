namespace kvps.Config

type IConfigRepository =
    abstract member Value: string -> string
    abstract member Set: string * string -> unit
