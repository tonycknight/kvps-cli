param(
    [Parameter(Mandatory=$false)]
    [string]$Target = "All"
)
$ErrorActionPreference = 'Stop'

& dotnet tool restore

& dotnet fake run "build.fsx" -t $Target
