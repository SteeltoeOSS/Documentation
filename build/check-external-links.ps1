#!/usr/bin/env pwsh
# Check external links in markdown files, matching what the check-links CI job does.
# Run from anywhere in the repo. Requires lychee: https://lychee.cli.rs/guides/getting-started/

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$baseDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Get-Item $baseDir).Parent.FullName
Push-Location $repoRoot

try {
    if (-not (Get-Command lychee -ErrorAction SilentlyContinue)) {
        throw 'lychee not found. Install: cargo install lychee, scoop install lychee, or winget install lycheeverse.lychee'
    }

    $mdFiles = Get-ChildItem -Path . -Filter '*.md' -Recurse -File
    if ($mdFiles.Count -eq 0) {
        throw 'No .md files found in the repository.'
    }

    Write-Output "Checking external links in $($mdFiles.Count) markdown files ..."

    & lychee `
        --verbose `
        --no-progress `
        --cache `
        --max-cache-age 1d `
        --accept '100..=103,200..=299,403,429' `
        --exclude 'localhost' `
        --exclude 'fortuneservice' `
        --exclude '\.internal' `
        --exclude 'consul-register-example' `
        --exclude '^docker://' `
        --exclude-path '.git' `
        --max-concurrency 5 `
        --max-retries 6 `
        --retry-wait-time 15 `
        --timeout 30 `
        '**/*.md'

    if ($LASTEXITCODE -ne 0) {
        throw "lychee exited with code $LASTEXITCODE"
    }
}
finally {
    Pop-Location
}
