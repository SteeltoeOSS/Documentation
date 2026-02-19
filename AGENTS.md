# Agent instructions for Steeltoe Documentation

## Scope and constraints

- **This repo is primarily documentation.** Prefer minimal, surgical edits. Do not change application or build code unless the task explicitly requires it.
- **When editing docs only:** do not modify `.github/workflows/` or `src/`.
- **Version focus:** Steeltoe v4 is current; v2 and v3 are legacy. Prefer v4 paths and examples unless the task targets an older version.

## Do / Don't

| Do | Don't |
| --- | --- |
| Use `.md` extensions for internal links | Use `.html` in internal links |
| Run the docfx build (see [README.md](README.md)) before committing doc changes; fix `InvalidFileLink` and similar warnings | Commit doc changes without validating |
| Add new pages to the relevant `toc.yml` (see [docs/README.md](docs/README.md)) | Add new API version config without updating `build/metadata.conf` and the corresponding build API config files |
| Use [docfx Flavored Markdown](https://dotnet.github.io/docfx/docs/markdown.html) | Change Blazor or workflow files unless the task explicitly requires it |

## Where to find instructions

**Use the READMEs as the source of truth.** Update them when build or content steps change; AGENTS.md only points to them.

- **[README.md](README.md)** — Build order, commands to run the site locally, clean build (`git clean -xdff`). Run docfx before `dotnet run` or you get 404s and `InvalidFileLink` from `docs/api`.
- **[build/README.md](build/README.md)** — API Browser: script, version/source switches, what the script does.
- **[docs/README.md](docs/README.md)** — docfx layout, links and cross-references, YAML front matter options, creating blog posts and doc pages (including `toc.yml` examples).

## Overview and layout

This repository hosts the [Steeltoe website](https://steeltoe.io/): documentation and blog articles, built with ASP.NET Core Blazor and docfx. Key locations:

- **`docs/`** — Content (see [docs/README.md](docs/README.md) for subdirs and content rules).
- **`src/Steeltoe.io/`** — Blazor app; docfx output goes to `wwwroot`.
- **`build/`** — API Browser script and config ([build/README.md](build/README.md)).
- **CI** — `.github/workflows/build-and-stage.yml`.

## Linting and config

- Markdown: `.markdownlint.json`
- Editor: `.editorconfig`

## Resources

- **Canonical instructions:** [README.md](README.md), [build/README.md](build/README.md), [docs/README.md](docs/README.md).
- [docfx documentation](https://dotnet.github.io/docfx) · [docfx Flavored Markdown](https://dotnet.github.io/docfx/docs/markdown.html)
- [Steeltoe website](https://steeltoe.io) · [ASP.NET Core Blazor](https://learn.microsoft.com/aspnet/core/blazor/)
