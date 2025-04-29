# Build API Browser metadata for Steeltoe 2, 3 and 4

> [!TIP]
> Building the API Browser documentation is not required for the site to run locally.

The recommended approach to generate API Browser documentation is to run the included [build-metadata.ps1](./build-metadata.ps1).

The script will:

1. Install DocFX (by running `dotnet tool restore`, which installs tools defined in [dotnet-tools.json](../.config/dotnet-tools.json))
1. Clone several copies of Steeltoe (using [metadata.conf](./metadata.conf) to identify checkout targets)
1. Run `docfx metadata api-v{version}.json` for each copy
   1. This command will produce many yaml files and place them in [docs](../docs), alongside the index pages for each API version
   1. The script offers a parameter to target a single version (for example: `build-metadata.ps1 3`)
1. Run `docfx build` with [docfx-all.json](../docs/docfx-all.json), which will copy the html files into `wwwroot` of the Steeltoe.io project

> [!NOTE]
> This process can take a while to complete, will display many warnings and may increase build time in subsequent local runs of Steeltoe.io.
> Additionally, you may experience build failures during the `docfx build` step when passing a version to `build-metadata.ps1`. These failures
> _can_ be ignored, but will result in 404 errors if you try to view API Browser pages that were not generated on your machine.
