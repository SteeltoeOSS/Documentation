# Build API Browser metadata for Steeltoe 2, 3 and 4

> [!TIP]
> Building the API Browser documentation is not required for the site to run locally.

The recommended approach to generate API Browser documentation is to run the included [build-metadata.ps1](./build-metadata.ps1).

The script will:

1. Install DocFX (by running `dotnet tool restore`, which installs tools defined in [dotnet-tools.json](../.config/dotnet-tools.json))
1. Clone several copies of Steeltoe (using [metadata.conf](./metadata.conf) to identify checkout targets)
1. Run `docfx metadata api-all.json`
   1. This command will produce many yaml files and place them in [docs](../docs), alongside the index pages for each API version
1. Run `docfx build` with [docfx-all.json](../docs/docfx-all.json), which will copy the html files into `wwwroot` of the Steeltoe.io project

> [!NOTE]
> This process can take a while to complete, will display many warnings and may increase build time in subsequent local runs of Steeltoe.io.
