namespace kvps.Config

type IConfigProvider=
    abstract member Get : unit -> Configuration

