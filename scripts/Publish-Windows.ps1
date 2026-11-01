param(
    [ValidateSet('win-x64', 'win-arm64')]
    [string]$Runtime = 'win-x64',
    [switch]$SelfContained
)

$ErrorActionPreference = 'Stop'
$project = Join-Path $PSScriptRoot '..\src\NexusMiner.Desktop\NexusMiner.Desktop.csproj'
$publishRoot = Join-Path $PSScriptRoot '..\artifacts\publish'
$output = Join-Path $publishRoot $Runtime
$selfContainedValue = if ($SelfContained) { 'true' } else { 'false' }

dotnet publish $project --configuration Release --runtime $Runtime --self-contained $selfContainedValue -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o $output
if ($LASTEXITCODE -ne 0) { throw "La publicación falló (exit code $LASTEXITCODE)." }
Write-Host "Publicación creada en: $output"
