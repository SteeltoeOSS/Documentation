# Documentation

## Overview

This is the home of Steeltoe website, documentation and blog articles.

[Steeltoe.io](src/Steeltoe.io/) is built with [ASP.NET Core Blazor](https://learn.microsoft.com/aspnet/core/blazor/) and [DocFX](https://dotnet.github.io/docfx).
DocFX is used to generate API documentation from triple-slash comments in Steeltoe and convert Markdown documents to HTML.

## Building and running the site

To run the website, use [Steeltoe.io.sln](src/Steeltoe.io.sln) and your preferred IDE, or your preferred shell with the equivalent of these commands:

```shell
cd src\Steeltoe.io\
dotnet run
```

The site should now be running at <https://localhost:8080>

> [!NOTE]
> If this is your first time running the site and you only follow this step, none of the static content will be processed and you will encounter 404 errors when browsing the site.

### Including DocFX content

If you want API browser content available on your machine, review the [build README](build/README.md) before starting the site.

For a faster feedback loop, exclude the API Browser content by building the static content with the following commands:

```shell
dotnet tool restore
dotnet docfx build docs/docfx.json
```

With the fast-feedback approach, it is expected to see `InvalidFileLink` warnings in files under the path `docs/api`.
