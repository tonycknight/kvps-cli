# kvps-cli

[![Build & Release](https://github.com/tonycknight/kvps-cli/actions/workflows/build.yml/badge.svg)](https://github.com/tonycknight/kvps-cli/actions/workflows/build.yml)

![Nuget](https://img.shields.io/nuget/v/kvps-cli)

A dotnet CLI tool for key value pair management.

---

# How to use

### Help:

``dotnet kvps`` or ``dotnet kvps -?``


### Set a key value

``dotnet set -?``

``dotnet set <KEY> -v <value> -t <TAG1> -t <TAG2>``


### Get a key value

``dotnet get -?``

``dotnet get <KEY>``

``dotnet get <KEY> -vo``

``dotnet get <KEY> -r``


### Delete a key

``dotnet del -?``

``dotnet del <KEY>``


### List keys

``dotnet list -?``

``dotnet list``

``dotnet list -t <TAG1> -t <TAG2>``
