# Documentation

## Overview

This is the home of Steeltoe website, documentation and blog articles.

The site uses [DocFX](https://dotnet.github.io/docfx) to convert Markdown to HTML and generate API documentation from triple-slash comments in Steeltoe.

## Building and running the site

### Basic build and run

The easiest way to build and run the site is to open [Steeltoe.io.sln](src/Steeltoe.io.sln) from your preferred IDE.

Alternatively, run the site from your preferred shell with the equivalent of these commands:

```shell
cd src\Steeltoe.io\
dotnet run
```

### Building and running with API Browser content

If you want API browser content available on your machine, review the [api-browser-generation README](src/api-browser-generation/README.md) before starting the site.
