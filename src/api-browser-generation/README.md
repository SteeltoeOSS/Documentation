# API Browser for Steeltoe 2, 3 and 4

> [!TIP]
> Building the API Browser documentation is not required for the site to run locally.

The recommended approach to generate API Browser documentation is to run the included [build-metadata.ps1](./build-metadata.ps1).

The script will:

1. Install DocFX (`dotnet tool install -g docfx`)
1. Clone several copies of Steeltoe (using [metadata.conf](./metadata.conf) to identify checkout targets)
1. Run `docfx metadata api-all.json`
   1. This command will produce many yaml files and place them in the path `./yaml`
1. Copy the resulting files over to their expected location in the Steeltoe.io project

> [!NOTE]
> This process can take a while to complete, will display many warnings and may increase build time in subsequent local runs of Steeltoe.io.
