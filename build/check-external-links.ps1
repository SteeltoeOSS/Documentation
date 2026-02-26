#!/usr/bin/env pwsh
# Check external links in markdown files, matching what the check-links CI job does.
# Run from anywhere in the repo. Requires lychee: https://lychee.cli.rs/guides/getting-started/

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not (Get-Command lychee -ErrorAction SilentlyContinue)) {
    throw 'lychee not found. Install: cargo install lychee, scoop install lychee, or winget install lycheeverse.lychee'
}

$baseDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Get-Item $baseDir).Parent.FullName
Push-Location $repoRoot

try {
    $mdFiles = Get-ChildItem -Path . -Filter '*.md' -Recurse -File
    if ($mdFiles.Count -eq 0) {
        throw 'No .md files found in the repository.'
    }

    Write-Output "Checking external links in $($mdFiles.Count) markdown files ..."

    & lychee `
        --cache `
        --exclude 'fortuneservice' `
        --exclude '\.internal' `
        --exclude 'consul-register-example' `
        --exclude-loopback `
        --exclude-path 'build.sources' `
        --host-concurrency 5 `
        --host-request-interval 100ms `
        --include-fragments `
        --require-https `
        --retry-wait-time 3 `
        --scheme http `
        --scheme https `
        '**/*.md'

    if ($LASTEXITCODE -ne 0) {
        throw "lychee exited with code $LASTEXITCODE"
    }
}
finally {
    Pop-Location
}
