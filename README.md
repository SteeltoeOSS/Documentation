# Documentation

## Overview

This is the home of the [Steeltoe website](https://steeltoe.io/), which hosts documentation and blog articles.

The website is built with [ASP.NET Core Blazor](https://learn.microsoft.com/aspnet/core/blazor/) and [docfx](https://dotnet.github.io/docfx).
docfx generates API documentation from triple-slash comments in Steeltoe source code and converts [Markdown](https://dotnet.github.io/docfx/docs/markdown.html) documents to HTML.

## Building and running the site

Follow the steps below to run/debug locally. The optional steps take longer, but provide a more complete experience.

> [!TIP]
> To start fresh, delete all files that are not part of this repository (including the cloned Steeltoe sources) using the following command:
>
> ```shell
> git clean -xdff
> ```

1. Optional: Build API Browser metadata (clones Steeltoe sources) and process Markdown files in `docs`:

   ```pwsh
   pwsh .\build\build-metadata.ps1
   ```
   See [build/README.md](build/README.md) for command-line switches.

1. Optional: Only process Markdown files in `docs`:

   ```shell
   dotnet tool restore && dotnet docfx build docs/docfx.json --warningsAsErrors true
   ```

1. Open [Steeltoe.io.sln](src/Steeltoe.io.sln) in your preferred IDE, or run from the command line:

   ```shell
   dotnet run --project .\src\Steeltoe.io
   ```

The site should now be running at <https://localhost:8080>.

> [!NOTE]
> If this is your first time running the site and you skip steps 1 and 2, none of the static content will be processed.
> You will encounter `InvalidFileLink` warnings from files at the path `docs/api` and 404 errors when browsing the site.
