# kvps-cli

[![Build & Release](https://github.com/tonycknight/kvps-cli/actions/workflows/build.yml/badge.svg)](https://github.com/tonycknight/kvps-cli/actions/workflows/build.yml)

![Nuget](https://img.shields.io/nuget/v/kvps-cli)

A dotnet CLI tool for key value pair management.

:warning: **Very beta!**

---

# Getting Started

## Dependenices

You'll need the [.Net 6 runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) installed on your Windows machine.

:warning: **This tool will not behave correctly under Linux or Mac OS.**

## Installation

``kvps-cli`` is available from [Nuget](https://www.nuget.org/packages/kvps-cli/):

```
dotnet tool install kvps-cli -g
```

---

# How to use

## Help:

```
dotnet kvps
``` 

or if installed globally:

```
kvps -?
```



## Set a key value

```
dotnet kvps set -?
```

Set a key's value, with optional tags:

```
dotnet kvps set <KEY> -v <value> -t <TAG1> -t <TAG2>
```



## Get a key value

Get help:

```
dotnet kvps get -?
```

Return the given key's value:

```
dotnet kvps get <KEY>
```

Return only the key's value without metadata, with the value masked if it's not public:

```
dotnet kvps get <KEY> -vo
```

Reveal the key's value:

```
dotnet kvps get <KEY> -r
```

Return only the key's value without metadata, with the value unmasked:

```
dotnet kvps get <KEY> -vo -r
```



## Delete a key

```
dotnet kvps del -?
```

Delete a given key:

```
dotnet kvps del <KEY>
```



## List keys

```
dotnet kvps  list -?
```

Display all keys:

```
dotnet kvps list
```

Display all keys that match any of the given tags:

```
dotnet kvps  list -t <TAG1> -t <TAG2>
```

---

# Powershell integration

## Single value

Store a key's value:

```
dotnet kvps set my_email_id -v joe@email.com
```

Later, extract the value:

```
$email = dotnet kvps get my_email_id -vo -r

.\Get-Data.ps1 -Email $email
```

## Json objects

Store some JSON:

```
dotnet kvps set json_test -v "{ myTenantId: '1234abcd' }" -p
```

Extract:

```
$v = dotnet kvps get json_test -vo -r 

$j = $v | ConvertFrom-Json
```

or

```
$j = dotnet kvps get json_test -vo -r | ConvertFrom-Json

$j.myTenantId
```

---



---

# Security

:warning: **This tool is not a key vault equivalent. Please do not use this tool to store secrets!**


