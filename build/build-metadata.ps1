#!/usr/bin/env pwsh

param (
    [int]$SteeltoeVersion
)

set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
Write-Host "'DOCFX_SOURCE_BRANCH_NAME' starting value: '$env:DOCFX_SOURCE_BRANCH_NAME'"
$OriginalSourceBranchName = $env:DOCFX_SOURCE_BRANCH_NAME

# Restore docfx tool
dotnet tool restore

# Get the script's directory
$baseDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Push-Location $baseDir

# Repository information
$gitSourcesUrl = "https://github.com/SteeltoeOSS/Steeltoe"

function Clone-Source-Build-Metadata
{
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseApprovedVerbs', '', Scope = 'Function')]
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
    Write-Host "Setting 'DOCFX_SOURCE_BRANCH_NAME' to '$branch'"
    $env:DOCFX_SOURCE_BRANCH_NAME = $branch
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

Write-Host "Setting 'DOCFX_SOURCE_BRANCH_NAME' back to '$OriginalSourceBranchName'"
$env:DOCFX_SOURCE_BRANCH_NAME = $OriginalSourceBranchName

Write-Host 'Running command: dotnet docfx build (Join-Path ".." "docs" "docfx-all.json") --warningsAsErrors true'
dotnet docfx build (Join-Path ".." "docs" "docfx-all.json") --warningsAsErrors true

if ($LastExitCode -ne 0)
{
    throw "Exit code is $LastExitCode"
}

# Due to an apparent bug within docfx, Steeltoe's favicon.ico is overwritten by the docfx icon (when using any group with build content)
# Overwrite the file so the Steeltoe favicon is deployed.
Copy-Item -Path (Join-Path ".." "docs" "favicon.ico") -Destination (Join-Path ".." "src" "Steeltoe.io" "wwwroot")

Pop-Location
