param(
    [string]$Runtime = "win-x64",
    [string]$Configuration = "Release",
    [string]$OutputDirectory = "artifacts/initial"
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "src/Needle.Avalonia/Needle.Avalonia.csproj"
$output = Join-Path $root $OutputDirectory

if (Test-Path -LiteralPath $output) {
    Remove-Item -LiteralPath $output -Recurse -Force
}

$env:AVALONIA_TELEMETRY_OPTOUT = "1"

dotnet publish $project `
    -c $Configuration `
    -r $Runtime `
    --self-contained true `
    --no-restore `
    -p:PublishSingleFile=true `
    -p:EnableCompressionInSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:DebugType=None `
    -p:DebugSymbols=false `
    -o $output

Remove-Item -LiteralPath `
    (Join-Path $output "libHarfBuzzSharp.pdb"), `
    (Join-Path $output "libSkiaSharp.pdb") `
    -Force `
    -ErrorAction SilentlyContinue

Get-ChildItem -LiteralPath $output | Select-Object Name,Length,LastWriteTime
