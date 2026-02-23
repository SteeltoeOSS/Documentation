#!/usr/bin/env pwsh
# Check internal links in the built site output, matching what build-and-stage CI does.
# Run from repo root after docfx build. Requires lychee: https://lychee.cli.rs/installation/

param(
    [string] $SitePath = 'src/Steeltoe.io/wwwroot'
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Get-Item $PSScriptRoot).Parent.FullName
Push-Location $repoRoot

try {
    if (-not (Get-Command lychee -ErrorAction SilentlyContinue)) {
        Write-Error 'lychee not found. Install: cargo install lychee, scoop install lychee, or winget install lycheeverse.lychee'
    }

    $resolvedPath = (Resolve-Path -LiteralPath $SitePath -ErrorAction Stop).Path
    if (-not (Test-Path $resolvedPath)) {
        Write-Error "Site path not found: $SitePath. Run the docfx build first."
    }

    $htmlFiles = Get-ChildItem -Path $resolvedPath -Filter '*.html' -Recurse -File
    if ($htmlFiles.Count -eq 0) {
        Write-Error "No .html files found under $SitePath. Run the docfx build first."
    }

    Write-Host "Checking internal links under $resolvedPath ($($htmlFiles.Count) HTML files) ..." -ForegroundColor Cyan

    $fileList = New-TemporaryFile
    $htmlFiles | ForEach-Object { "./$($_.FullName.Substring($repoRoot.Length + 1) -replace '\\','/')" } |
        Set-Content -LiteralPath $fileList -Encoding utf8NoBOM

    & lychee `
        --no-progress `
        --root-dir $resolvedPath `
        --exclude '^https?://' `
        --exclude '^file:///[^#?]*/[^.#?/]+$' `
        --exclude-path '.git' `
        --max-concurrency 5 `
        --max-retries 6 `
        --retry-wait-time 15 `
        --timeout 30 `
        --files-from $fileList

    $exitCode = $LASTEXITCODE
    Remove-Item -LiteralPath $fileList -ErrorAction SilentlyContinue
    exit $exitCode
}
finally {
    Pop-Location
}
