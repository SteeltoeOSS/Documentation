#!/usr/bin/env pwsh

param (
    [string] $SteeltoeVersion
)

set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Write-Host "Starting value of DOCFX_SOURCE_BRANCH_NAME: '$env:DOCFX_SOURCE_BRANCH_NAME'"
$OriginalSourceBranchName = $env:DOCFX_SOURCE_BRANCH_NAME

# Get the script's directory
$baseDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Push-Location $baseDir

function EnsureDocfxBinaries() {
    # Temporary workaround until a proper DocFX build supporting .NET 10 is available.
    $zipFile = [IO.Path]::Combine($env:TEMP, 'docfx-net10-binaries.zip')

    if (!(Test-Path -Path 'docfx-net10-binaries')) {
        Invoke-WebRequest -Uri 'https://ent.box.com/shared/static/7uekfex8ugc1kijt60lwx6q4n1r3d1m6.zip' -Method 'GET' -OutFile $zipFile
        Expand-Archive $zipFile -Force
    }
}

EnsureDocfxBinaries

# Repository information
$gitSourcesUrl = 'https://github.com/SteeltoeOSS/Steeltoe'

function Clone-Source-Build-Metadata
{
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseApprovedVerbs', '', Scope = 'Function')]
    param (
        [string] $version
    )

    if ($version -eq 'source') {
        $apiFile = 'api-source.json'
        Push-Location ../../Steeltoe
        $branch = git rev-parse --abbrev-ref HEAD
        Pop-Location
        Write-Output "Using local sources from $branch branch"
    }
    else {
        $destination = Join-Path 'sources' "v$version"
        $pattern = "^$($version):"
        $branch = (Select-String -Path 'metadata.conf' -Pattern $pattern -Raw | ForEach-Object { ($_ -split ':')[1].Trim() })

        Write-Output "Cloning sources for $(Split-Path -Leaf $destination) and switching to $branch branch"
        if (Test-Path $destination)
        {
            Push-Location $destination
            git checkout $branch
            git pull
            Pop-Location
        }
        else {
            git clone $gitSourcesUrl $destination -b $branch
        }

        $apiFile = "api-v$version.json"
    }

    Write-Output "Setting DOCFX_SOURCE_BRANCH_NAME to '$branch'"
    $env:DOCFX_SOURCE_BRANCH_NAME = $branch
    Write-Output "Running command: dotnet exec docfx-net10-binaries/docfx.dll metadata $apiFile"
    dotnet exec docfx-net10-binaries/docfx.dll metadata $apiFile
}

$buildAll = $false;
if($PSBoundParameters.ContainsKey('SteeltoeVersion'))
{
    Write-Output "Building for single version: $SteeltoeVersion"
}
else
{
    $buildAll = $true
    Write-Output 'Building for all versions'
}

if ($buildAll -or $SteeltoeVersion -eq '2')
{
    Clone-Source-Build-Metadata 2
}
if ($buildAll -or $SteeltoeVersion -eq '3')
{
    Clone-Source-Build-Metadata 3
}
if ($buildAll -or $SteeltoeVersion -eq '4')
{
    Clone-Source-Build-Metadata 4
}
if ($SteeltoeVersion -eq 'source')
{
    Clone-Source-Build-Metadata 'source'
}

Write-Output "Setting DOCFX_SOURCE_BRANCH_NAME back to '$OriginalSourceBranchName'"
$env:DOCFX_SOURCE_BRANCH_NAME = $OriginalSourceBranchName

$buildArgs = @('exec', 'docfx-net10-binaries/docfx.dll', 'build', (Join-Path '..' 'docs' 'docfx-all.json'))
if ($buildAll -eq $true) {
    $buildArgs += '--warningsAsErrors'
    $buildArgs += 'true'
}

Write-Output "Running command: dotnet $buildArgs"
dotnet $buildArgs

if ($LastExitCode -ne 0)
{
    throw "docfx build failed with exit code $LastExitCode"
}

# Due to an apparent bug within docfx, Steeltoe's favicon.ico is overwritten by the docfx icon (when using any group with build content)
# Overwrite the file so the Steeltoe favicon is deployed.
Copy-Item -Path (Join-Path ".." "docs" "favicon.ico") -Destination (Join-Path ".." "src" "Steeltoe.io" "wwwroot")

Pop-Location
