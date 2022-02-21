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
    version:    string
    value:      string
    isSecret:   bool
    tags:       string[]
    }

type DbInfo = {
    name:       string
    kvCount:    int
    }

module EntityMapping =
    let mapToKv (kv: KeyValueData)=
        { KeyValue.key = kv._id; value = kv.value; tags = kv.tags; isSecret = kv.isSecret}

    let mapToKvData (kv: KeyValue)=
        { KeyValueData._id = kv.key; 
                        version = "v1";
                        value = kv.value; 
                        tags = kv.tags; 
                        isSecret = kv.isSecret}

    let mergeTags (ekv: KeyValue) (kv: KeyValue)= { kv with tags = ekv.tags}