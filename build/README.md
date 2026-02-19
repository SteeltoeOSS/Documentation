# Build API Browser metadata for Steeltoe 2, 3 and 4

> [!TIP]
> Building the API Browser documentation is not required for the site to run locally.

Generate API Browser documentation and process all markdown with the included PowerShell script [build-metadata.ps1](./build-metadata.ps1):

```pwsh
cd build
pwsh ./build-metadata.ps1
```

This process can take a while to complete, will display many warnings and may increase build time in subsequent local runs of Steeltoe.io.

> [!TIP]
> `build-metadata.ps1` takes an optional parameter to process one Steeltoe version at a time.
> For example, running `./build-metadata.ps1 4` will clone and build API Browser metadata for only v4.
> Or run `./build-metadata.ps1 source` to use local Steeltoe sources in a sibling directory.
>
> Passing a version to `build-metadata.ps1` is likely to produce failures during `docfx build`.
> These failures _can_ be ignored, but will result in 404 errors if you try to view API Browser pages that were not generated on your machine.

## What does the script do?

1. Download docfx binaries (temporary workaround for .NET 10 support)
1. Clone several copies of Steeltoe (using [metadata.conf](./metadata.conf) to identify checkout targets)
1. Run `docfx metadata api-v{version}.json` for each copy
   1. This command will produce many yaml files and place them in [docs](../docs), alongside the index pages for each API version
   1. The script offers a parameter to target a single version (for example: `build-metadata.ps1 3`)
1. Run `docfx build` with [docfx-all.json](../docs/docfx-all.json), which will copy the html files into `wwwroot` of the Steeltoe.io project
