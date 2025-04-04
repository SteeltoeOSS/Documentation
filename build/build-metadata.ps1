#!/usr/bin/env pwsh

param (
    [int]$SteeltoeVersion
)

set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# Restore docfx tool
dotnet tool restore

# Get the script's directory
$baseDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Push-Location $baseDir

# Repository information
$gitSourcesUrl = "https://github.com/SteeltoeOSS/Steeltoe"

function Clone-Source-Build-Metadata
{
    param (
        [int]$version
    )

    $destination = Join-Path "sources" "v$version"
    $pattern = "^$($version):"
    $branch = (Select-String -Path "metadata.conf" -Pattern $pattern -Raw | ForEach-Object { ($_ -split ':')[1].Trim() })

    Write-Output "$(Split-Path -Leaf $destination) sources from $branch"
    if (Test-Path $destination)
    {
        Push-Location $destination
        git pull
        Pop-Location
    }
    else {
        git clone $gitSourcesUrl $destination -b $branch
    }

    $apiFile = "api-v$version.json"
    Write-Host "Running command: dotnet docfx metadata $apiFile"
    dotnet docfx metadata $apiFile
}

$buildAll = $false;
if($PSBoundParameters.ContainsKey("SteeltoeVersion"))
{
    Write-Output "Cloning Steeltoe repository at v$SteeltoeVersion"
}
else
{
    $buildAll = $true
    Write-Output 'Cloning Steeltoe repository at each version'
}

if ($buildAll -or $SteeltoeVersion -ieq "2")
{
    Clone-Source-Build-Metadata 2
}
if ($buildAll -or $SteeltoeVersion -ieq "3")
{
    Clone-Source-Build-Metadata 3
}
if ($buildAll -or $SteeltoeVersion -ieq "4")
{
    Clone-Source-Build-Metadata 4
}


Write-Host 'Running command: dotnet docfx build (Join-Path ".." "docs" "docfx-all.json")'
dotnet docfx build (Join-Path ".." "docs" "docfx-all.json")
Pop-Location
