namespace kvps.Config

type Configuration =
    { dbName: string }

    static member Default = { dbName = "kvps" }
