#!/usr/bin/env pwsh

Invoke-Expression "docker run -it --rm --volume $(Get-Location):/work docfx $args"
