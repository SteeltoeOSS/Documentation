#!/usr/bin/env pwsh

set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# Install dotnet tools (DocFX)
dotnet tool restore

# Get the script's directory
$baseDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Push-Location $baseDir

# Repository information
$gitSourcesUrl = "https://github.com/SteeltoeOSS/Steeltoe"

# Read metadata configuration
$v2Sources = (Select-String -Path "metadata.conf" -Pattern '^2:' -Raw | ForEach-Object { ($_ -split ':')[1].Trim() })
$v3Sources = (Select-String -Path "metadata.conf" -Pattern '^3:' -Raw | ForEach-Object { ($_ -split ':')[1].Trim() })
$v4Sources = (Select-String -Path "metadata.conf" -Pattern '^4:' -Raw | ForEach-Object { ($_ -split ':')[1].Trim() })

# Function to clone sources
function Get-Sources {
    param (
        [string]$destDir,
        [string]$branch
    )

    Write-Output "$(Split-Path -Leaf $destDir) sources from $branch"
    if (Test-Path $destDir) {
        Remove-Item -Recurse -Force $destDir
    }
    git clone $gitSourcesUrl $destDir -b $branch
}

Write-Output "Cloning Steeltoe repository at each version"
Get-Sources (Join-Path "sources" "v2") $v2Sources
Get-Sources (Join-Path "sources" "v3") $v3Sources
Get-Sources (Join-Path "sources" "v4") $v4Sources

Write-Host 'Running command: dotnet docfx metadata api-all.json'
dotnet docfx metadata api-all.json

Write-Host 'Running command: dotnet docfx build (Join-Path ".." "docs" "docfx-all.json")'
dotnet docfx build (Join-Path ".." "docs" "docfx-all.json")
Pop-Location
