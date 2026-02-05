# Copilot Instructions for Steeltoe Documentation

## Overview

This repository hosts the [Steeltoe website](https://steeltoe.io/), which contains documentation and blog articles. The site is built with ASP.NET Core Blazor and is **heavily dependent on docfx**.

## Technology Stack

- **ASP.NET Core Blazor**: Web framework for the site
- **docfx**: Documentation generation tool (primary dependency)
  - Generates API documentation from triple-slash comments in Steeltoe source code
  - Converts Markdown documents to HTML
  - Uses custom templates in `docs/modern-steeltoe` directory
- **PowerShell**: Build scripts
- **.NET**: Core runtime and tooling

## Repository Structure

- `/docs`: Content source files
  - `/api`: Index pages for API Browser content
  - `/articles`: Blog posts
  - `/docs`: Steeltoe documentation (versioned: v2, v3, v4)
  - `/guides`: Getting started guides
  - `/modern-steeltoe`: Customized docfx template
- `/src`: Blazor website source code
  - `/Steeltoe.io`: Main web application project
- `/build`: Build scripts and configuration
  - `build-metadata.ps1`: Generates API Browser metadata
  - `metadata.conf`: Configuration for Steeltoe source versions
  - API configuration files: `api-v{version}.json`, `api-filter-v{version}.yml`

## Building and Running

### Quick Start (Local Development)

```shell
dotnet run --project ./src/Steeltoe.io
```

Site runs at https://localhost:8080

> Note: First-time runs without processing static content will show warnings and 404 errors.

### Full Build Process

1. **Build API Browser metadata** (optional but recommended):
   ```pwsh
   pwsh ./build/build-metadata.ps1
   ```
   - Clones Steeltoe sources
   - Generates API documentation for all versions
   - Can target specific version: `./build-metadata.ps1 4`
   - Can use local sources: `./build-metadata.ps1 source`

2. **Process Markdown files only**:
   ```shell
   dotnet tool restore && dotnet docfx build docs/docfx.json --warningsAsErrors true
   ```

### Clean Build

```shell
git clean -xdff
```

## docfx Specifics

### Configuration Files

- `docs/docfx.json`: Local development config (excludes API Browser)
- `docs/docfx-all.json`: Full build config (includes API Browser)
- Output directory: `src/Steeltoe.io/wwwroot`

### Markdown Guidelines

- Use [docfx Flavored Markdown](https://dotnet.github.io/docfx/docs/markdown.html)
- Internal links should use `.md` extensions, not `.html`
- Cross-references: Use `[link title](../example.md#section-title)` or UID-based xrefs
- Files can include YAML front matter for metadata and display options

### YAML Front Matter Options

- `_disableToc`: Hide left table of contents
- `_disableAffix`: Hide right page navigation
- `_disableContribution`: Hide "Edit this page" link
- `_enableSearch`: Show/hide search icon
- `_enableNewTab`: Open external links in new tab
- `_hideTocVersionToggle`: Hide version toggler
- `_noindex`: Prevent search engine indexing
- `_disableNavbar`: Hide top navigation bar

### Creating Content

#### New Blog Post

1. Create `.md` file in `articles/` directory with URL-safe name
2. Add entry to `/articles/index.md`
3. Include required front matter:
   ```yaml
   ---
   title: Post Title
   description: Brief description
   date: MM/DD/YYYY
   uid: articles/url-safe-name
   tags: [ "tag1", "tag2" ]
   author.name: Author Name
   author.github: username
   author.twitter: handle
   ---
   ```

#### New Documentation Page

1. Create `.md` file in appropriate version directory (e.g., `docs/v4/component/`)
2. Add to relevant `toc.yml` file
3. Use UID format: `docs/v{version}/component/page-name`

## Testing and Validation

- Run `dotnet docfx build docs/docfx.json --warningsAsErrors true` to validate Markdown
- Check for `InvalidFileLink` warnings
- Test site locally before committing
- Verify links and cross-references work correctly

## Linting

- Markdown linting configured in `.markdownlint.json`
- Editor configuration in `.editorconfig`

## CI/CD

- GitHub Actions workflows in `.github/workflows/`
- `build-and-stage.yml`: Main build and deployment
- Environment variable: `DOCFX_SOURCE_BRANCH_NAME` used during builds

## Common Tasks

### Update API Documentation

```pwsh
cd build
pwsh ./build-metadata.ps1
```

### Add New Steeltoe Version

1. Update `build/metadata.conf`
2. Create `build/api-v{version}.json`
3. Create `build/api-filter-v{version}.yml`
4. Add version directories in `docs/`

### Modify Templates

Custom templates are in `docs/modern-steeltoe/` directory. Changes here affect site-wide styling and layout.

## Best Practices

1. **Always validate links**: Use `.md` extensions for internal links
2. **Test docfx builds**: Run build command to catch errors before committing
3. **Respect versioning**: Keep documentation organized by Steeltoe version
4. **Follow Markdown conventions**: Use docfx-flavored Markdown features
5. **Minimal changes**: This is a documentation site; changes should be surgical and precise
6. **Don't modify working code**: Unless fixing a bug related to your task
7. **Use existing tooling**: Leverage docfx commands for validation

## Support and Resources

- [docfx documentation](https://dotnet.github.io/docfx)
- [docfx Flavored Markdown](https://dotnet.github.io/docfx/docs/markdown.html)
- [Steeltoe website](https://steeltoe.io)
- [ASP.NET Core Blazor docs](https://learn.microsoft.com/aspnet/core/blazor/)
