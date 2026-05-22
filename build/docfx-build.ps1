#!/usr/bin/env pwsh

set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function EnsureDocfxBinaries() {
    # Temporary workaround until a proper DocFX build supporting .NET 10 is available.
    $zipFile = [IO.Path]::Combine($env:TEMP, 'docfx-net10-binaries.zip')

    if (!(Test-Path -Path 'docfx-net10-binaries')) {
        Invoke-WebRequest -Uri 'https://ent.box.com/shared/static/3b9s1j71jmcsbjgug0yjel8fohk1n720.zip' -Method 'GET' -OutFile $zipFile
        Expand-Archive $zipFile -Force
    }
}

# Get the script's directory
$baseDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Push-Location $baseDir

try {
    EnsureDocfxBinaries

    $buildArgs = @('exec', 'docfx-net10-binaries/docfx.dll', 'build', (Join-Path '..' 'docs' 'docfx.json'), '--warningsAsErrors', 'true')
    Write-Output "Running command: dotnet $buildArgs"
    dotnet $buildArgs
}
finally {
    Pop-Location
}
