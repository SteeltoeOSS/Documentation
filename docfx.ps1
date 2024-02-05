#!/usr/bin/env pwsh

Invoke-Expression "docker run -it --rm --volume ${pwd}:/work docfx:local $args"
