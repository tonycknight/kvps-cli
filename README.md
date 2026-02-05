# kvps-cli

[![Build & Release](https://github.com/tonycknight/kvps-cli/actions/workflows/build.yml/badge.svg)](https://github.com/tonycknight/kvps-cli/actions/workflows/build.yml)

[![Nuget](https://img.shields.io/nuget/v/kvps-cli)](https://www.nuget.org/packages/kvps-cli/)

A dotnet CLI tool for key value pair management.

---

# Getting Started

## Dependenices

You'll need the [.Net 8 or .Net 10 runtime](https://dotnet.microsoft.com/en-us/download/dotnet) installed on your Windows machine.

Unfortunately this tool will not behave correctly under Linux or Mac OS.

## Installation

``kvps-cli`` is available from [Nuget](https://www.nuget.org/packages/kvps-cli/):

```
dotnet tool install kvps-cli -g
```

---

# How to use

## Help:

```
kvps -?
``` 

## Set a key value

```
kvps set -?
```

Set a key's value, with optional tags:

```
kvps set <KEY> -v <value> -t <TAG1> -t <TAG2>
```

Set a key's value as secret:

```
kvps set <KEY> -v <value> -s
```

Set a key's value as public (default):

```
kvps set <KEY> -v <value> -p
```

## Get a key value

Get help:

```
kvps get -?
```

Return the given key's value:

```
kvps get <KEY>
```

Return only the key's value without metadata, with the value masked if it's not public:

```
kvps get <KEY> -vo
```

Reveal the key's value:

```
kvps get <KEY> -r
```

Return only the key's value without metadata, with the value unmasked:

```
kvps get <KEY> -vo -r
```

Copy the value to the clipboard:

```
kvps get <KEY> -c
```



## Delete a key

```
kvps del -?
```

Delete a given key:

```
kvps del <KEY>
```



## List keys

```
kvps list -?
```

Display all keys:

```
kvps list
```

Display all keys that match any of the given tags:

```
kvps  list -t <TAG1> -t <TAG2>
```

---

# Powershell integration

## Single value

Store a key's value:

```
kvps set my_email_id -v joe@email.com
```

Later, extract the value:

```
$email = kvps get my_email_id -vo -r


$email
joe@email.com
```

## Json objects

Store some JSON:

```
kvps set json_test -v "{ myTenantId: '1234abcd' }" -p
```

Extract:

```
$j = kvps get json_test -vo -r | ConvertFrom-Json

$j.myTenantId
```

## Database management

```
kvps db -?
```

### Export all key values

:warning: **Although the database is encrypted, this will export all data in plain text JSON!**

```
kvps db export <FILENAME>
```

### Import key values

```
kvps db import <FILENAME>
```

### Switch databases

Show the current database:

```
kvps db show
```

Use a different database:

```
kvps db set my_new_db
```

Switch back to the original:

```
kvps db set kvps
```

---

# Security

:warning: **This tool is not a key vault equivalent!**
