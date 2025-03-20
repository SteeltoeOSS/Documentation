#!/usr/bin/env pwsh

set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# Install DocFX tool
dotnet tool install -g docfx

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

# Clone repositories
Get-Sources (Join-Path "sources" "v2") $v2Sources
Get-Sources (Join-Path "sources" "v3") $v3Sources
Get-Sources (Join-Path "sources" "v4") $v4Sources

# Write-Output "building all metadata"
docfx metadata api-all.json

# Copy yaml files to website
Copy-Item -Path (Join-Path "yaml" "*") -Destination (Join-Path ".." "Steeltoe.io" "docfx-content" "api") -Recurse -Force
docfx build (Join-Path ".." "Steeltoe.io" "docfx.json")
Pop-Location
