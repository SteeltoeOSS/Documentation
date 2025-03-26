# Documentation

## Overview

This is the home of the [Steeltoe website](https://steeltoe.io/), which hosts documentation and blog articles.

The website is built with [ASP.NET Core Blazor](https://learn.microsoft.com/aspnet/core/blazor/) and [docfx](https://dotnet.github.io/docfx).
docfx generates API documentation from triple-slash comments in Steeltoe source code and converts [Markdown](https://dotnet.github.io/docfx/docs/markdown.html) documents to HTML.

## Building and running the site

To run the website, open [Steeltoe.io.sln](src/Steeltoe.io.sln) in your preferred IDE, or use the command-line:

```shell
cd src\Steeltoe.io\
dotnet run
```

The site should now be running at <https://localhost:8080>.

> [!NOTE]
> If this is your first time running the site and you only follow this step, none of the static content will be processed and you will encounter 404 errors when browsing the site.

### Including DocFX content

If you want API Browser content available locally, see the [build README](build/README.md) before starting the site. Note that it takes several minutes to run.

For a faster feedback loop, you can skip building the API Browser content and only build the documentation YAML files with the following commands:

```shell
dotnet tool restore
dotnet docfx build docs/docfx.json
```

> [!NOTE]
> If you skipped building the API Browser content, `InvalidFileLink` warnings will appear in files at the path `docs/api`.
