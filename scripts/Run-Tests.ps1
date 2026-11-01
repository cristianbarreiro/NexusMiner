$ErrorActionPreference = 'Stop'
$solution = Join-Path $PSScriptRoot '..\NexusMiner.sln'
dotnet test $solution --configuration Release
if ($LASTEXITCODE -ne 0) { throw "Las pruebas fallaron (exit code $LASTEXITCODE)." }
