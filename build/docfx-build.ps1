#!/usr/bin/env pwsh

set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Restore docfx tool
dotnet tool restore

# Get the script's directory
$baseDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Push-Location $baseDir

try {
    $buildArgs = @('docfx', 'build', (Join-Path '..' 'docs' 'docfx.json'), '--warningsAsErrors', 'true')
    Write-Output "Running command: dotnet $buildArgs"
    dotnet $buildArgs
}
finally {
    Pop-Location
}
