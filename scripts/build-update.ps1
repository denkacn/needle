param(
    [string]$Runtime = "win-x64",
    [string]$Configuration = "Release",
    [string]$OutputDirectory = "artifacts/update",
    [string]$UpdateBaseUrl = "https://needle.bypuziki.com/updates"
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "src/Needle.Avalonia/Needle.Avalonia.csproj"
$output = Join-Path $root $OutputDirectory

[xml]$projectXml = Get-Content -LiteralPath $project
$version = $projectXml.Project.PropertyGroup.Version
if ([string]::IsNullOrWhiteSpace($version)) {
    throw "Cannot resolve Version from $project"
}

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

$sourceExe = Join-Path $output "Needle.exe"
$updateFileName = "Needle-$version-$Runtime.exe"
$updateExe = Join-Path $output $updateFileName
Move-Item -LiteralPath $sourceExe -Destination $updateExe -Force

$updateBaseUrl = $UpdateBaseUrl.TrimEnd("/")
$downloadUrl = "$updateBaseUrl/$updateFileName"
$appCastUrl = "$updateBaseUrl/appcast.xml"
$exe = Get-Item -LiteralPath $updateExe
$pubDate = (Get-Date).ToUniversalTime().ToString("r")
$appCastPath = Join-Path $output "appcast.xml"

$appCast = @"
<?xml version="1.0" encoding="utf-8"?>
<rss version="2.0" xmlns:sparkle="http://www.andymatuschak.org/xml-namespaces/sparkle">
  <channel>
    <title>Needle Updates</title>
    <link>$appCastUrl</link>
    <description>Needle release updates.</description>
    <language>en</language>
    <item>
      <title>Needle $version</title>
      <description>Needle $version update.</description>
      <pubDate>$pubDate</pubDate>
      <enclosure url="$downloadUrl"
                 sparkle:version="$version"
                 sparkle:shortVersionString="$version"
                 sparkle:os="windows"
                 length="$($exe.Length)"
                 type="application/octet-stream" />
    </item>
  </channel>
</rss>
"@

Set-Content -LiteralPath $appCastPath -Value $appCast.TrimStart() -Encoding UTF8

Get-ChildItem -LiteralPath $output | Select-Object Name,Length,LastWriteTime
