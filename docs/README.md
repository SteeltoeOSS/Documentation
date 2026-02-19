# Directory Contents

The directories below this one are used with [docfx](https://dotnet.github.io/docfx) to generate static HTML files that are served from the `/src/Steeltoe.io/wwwroot` directory.

| Path | Description |
| --- | --- |
| `/api` | Index pages for API Browser content |
| `/articles` | Blog posts |
| `/docs` | Steeltoe documentation |
| `/guides` | Guides for getting started with Steeltoe |
| `/modern-steeltoe` | Customized copy of the modern docfx template |

If you are working with API Browser content, view [this README](../build/README.md).

## Installing docfx

docfx is automatically downloaded by the [build-metadata.ps1](../build/build-metadata.ps1) script when building API Browser metadata.

## docfx Markdown

docfx offers an enhanced flavor of Markdown. To see examples and learn more, view the [docfx Flavored Markdown](https://dotnet.github.io/docfx/docs/markdown.html) documentation.

Visual Studio Code users may find the [Docs Authoring Pack](https://marketplace.visualstudio.com/items?itemName=docsmsft.docs-authoring-pack) extension pack useful.

### Links and Cross References

Markdown files can contain a YAML header. Values in this header let you control page design and set the page's `UID`. The `UID` can be used to create a reference to the page using an `xref`. The shorthand `@` syntax is not recommended, because broken links aren't reliably validated when used. The easiest way to reference other pages is to use Markdown syntax, for example: `[link title](../example.md#section-title)`.

For more information, see [Links and Cross References](https://dotnet.github.io/docfx/docs/links-and-cross-references.html).

> [!NOTE]
> Internal links should point to `.md` files instead of `.html` files. Alternatively, use the `UID` of the target page. In both cases, docfx calculates the path and checks for broken links when building the project.

### Page display options

In the YAML header of a page's markdown, you have options to turn page elements on or off. Below are those options.

| Yaml label | Default value | Description |
| --- | --- | --- |
| _disableToc | false | Turn off the left hand table of contents |
| _disableAffix | false | Turn off the right hand page navigation links |
| _disableContribution | false | Turn off the "Edit this page" link at the bottom |
| _enableSearch | true | Show the search icon |
| _enableNewTab | true | All external links on the page open in a new browser tab |
| _disableNav | false | Do not show top navigation links |
| _hideTocVersionToggle | false | Hide the version toggler in the table of contents |
| _noindex | false | Do not let search engines index the page |
| _disableNavbar | false | Do not show top bar of page |

## Creating a new blog post

Create a new `.md` file in the `articles` directory. Name the file something that is URL-safe. In `/articles/index.md`, add a shorthand link to the document, as well as a short description.

Here is a starter blog post:

```markdown
---
title: My Very Authentic Blog Post Title
description: A short description of my topic. Maybe 2 sentences long.
date: 01/01/2000
uid: articles/my-very-authentic-blog-post-title
tags: [ "modernize", "something else", "and another thing" ]
author.name: Joe Montana
author.github: jmontana
author.twitter: thebigguy
---

# My Very Authentic Blog Post Title

Let's talk about something really cool...
```

## Creating a new documentation page

Create a new `.md` file in the `docs` directory. The name needs to be URL-safe. Notice there are subdirectories for the Steeltoe version, such as `v2`, `v3`, and `v4`. Each one contains directories for Steeltoe components. Place your content accordingly. To include the file in the table of contents, add it in the appropriate `toc.yml` file. Notice in the example below, that the `href` values are not absolute paths. docfx will calculate everything at build time.

An example API doc:

```markdown
---
uid: docs/v4/component/index
---

# Some New Item

Steeltoe's newest component cures developer toil by...
```

Corresponding entry in `docs/v4/toc.yml`:

```yaml
- name: Component
  items:
    - name: Introduction
      href: component/index.md
```
