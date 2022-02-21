namespace kvps.KeyValues

type KeyValue= {
    key:        string
    value:      string
    isSecret:   bool
    tags:       string[]
    }
    
[<CLIMutable>]
type KeyValueData= {
    _id:        string
    value:      string
    isSecret:   bool
    tags:       string[]
    }

type DbInfo = {
    name:       string
    kvCount:    int
    }