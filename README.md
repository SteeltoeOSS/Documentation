# Documentation

## Overview

This is the home of Steeltoe documentation and blog articles. The site uses [DocFX](https://dotnet.github.io/docfx) to convert Markdown to HTML, generate API documentation from triple-slash comments in Steeltoe and generate site navigation.

## Site Contents

| Path | Description
| --- | ---
| `/api` | API documentation
| `/articles` | blog posts
| `/guides` | guides for getting started with Steeltoe
| `/template` | theming

## DocFX Markdown

DocFX offers an enhanced flavor of Markdown. To see examples and learn more, view the [DocFX Flavored Markdown](https://dotnet.github.io/docfx/spec/docfx_flavored_markdown.html) documentation.

Visual Studio Code users may find the [Docs Authoring Pack](https://marketplace.visualstudio.com/items?itemName=docsmsft.docs-authoring-pack) extension pack useful.

### Links and Cross References

As you get familiar with DocFX, you'll notice the addition of a YAML header in the markdown files. Values in this header let you control page design, as well as set the page's `UID`. With this, you can create `xref` as well as use DocFX's `@` shorthand. Learn more about [linking in DocFX](https://dotnet.github.io/docfx/tutorial/links_and_cross_references.html).

**Note** it should be very rare that you hardcode a link to an 'HTML' page with your markdown. Instead, use its `UID` and let the path get calculated, as well as get links validated when building the project.

### Page display options

In the YAML header of a page's markdown, you have options to turn page elements on or off. Below are those options.

|Yaml label  |Default value  |Description   |
|---------|---------|---------|
|_disableToc     |false|Turn off the left hand table of contents         |
|_disableAffix     |false|Turn off the right hand page navigation links         |
|_disableContribution     |false|Turn off right hand link to "edit this page"         |
|_disableFooter     |false|Don't show footer when guest scrolls to page bottom         |
|_enableSearch     |true|Show the search icon         |
|_enableNewTab     |true|All links on the page open in a new browser tab         |
|_disableNav     |false|Do not show top navigation links         |
|_hideTocVersionToggle|false     |Hide the version toggler in the table of contents         |
|_noindex     |false|Do not let search engines index the page         |
|_disableNavbar|false     |Do not show top bar of page         |

## Creating a new blog post

Create a new `.md` file in the `articles` directory. Name the file something that is URL safe. In `/articles/index.md` add a shorthand link to the document as well as a short description. If the post should also be included in Steeltoe's RSS feed, add a link entry in `articles/rss.xml`.

Here is a starter blog post:

```markdown
---
type: markdown
title: My Very Authentic Blog Post Title
description: A short description of my topic. Maybe 2 sentences long.
date: 01/01/2000
uid: articles/my-very-authentic-blog-post-title
tags: [ "modernize", 'something else", "and another thing" ]
author.name: Joe Montana
author.github: jmontana
author.twitter: thebigguy
---

# My Very Authentic Blog Post Title

Let's talk about something really cool...
```

## Creating a new API document

Create a new markdown file in the `api` directory. Name the file something URL safe. In the `api` directory there are `v2` and `v3` directories. Within each of those are directories for each component. Place your content accordingly. To include the document in the Table of Contents, add it to `api/(version)/toc.yml`.
An example API document:

An example API doc:

```markdown
---
uid: api/v2/circuitbreaker/hystrix
---

# Netflix Hystrix

Steeltoe's Hystrix implementation lets application developers isolate and manage back-end dependencies so that a single failing dependency does not take down the entire application. This is accomplished by wrapping all calls to external dependencies in a `HystrixCommand`, which runs in its own...

Here is an example cross-reference link to config docs: @api/v2/configuration/cloud-foundry-provider
Or you could link to the v3 version of this doc: @api/v3/circuitbreaker/hystrix
Or do the same thing by providing custom link text: [view the v3 version](xref:api/v2/circuitbreaker/hystrix)
```

Corresponding entry in `api/v2/toc.yml`:

```yaml
- name: Circuit Breakers
  items:
    - topicHref: circuitbreaker/hystrix.md
      name: Hystrix
```

## Installing DocFX

> [!NOTE]
> This project currently expects DocFX version 2.59.2 to be available.

### Install with Chocolatey

If you are using Windows, this is the easiest method. Run this command from an elevated shell: `choco install docfx -y --version 2.59.2`

### Download from DocFX

> [!IMPORTANT]
> If running on Linux or OS X, you will need to [install Mono](https://www.mono-project.com/docs/getting-started/install/) and use `mono` to execute the DocFX binary.

- Download DocFX [distribution](https://github.com/dotnet/docfx/releases/v2.59.2).
- Unzip to directory of your choosing and add that directory to your `PATH`.
- See the script in this repository at path `docfx/docfx` for an example wrapper script.

## Building and running the site

For working on any non-trivial changes, there are several ways to build and run the site locally.

### Basic build and run

The easiest way to build and run the site is this command: `docfx build --serve --port 8082`.

### Build API docs for Steeltoe 2 and 3

Building the API docs is not required for the site to run locally.

If needed, these commands will download the Steeltoe source code and generate API documentation from the triple-slash comments in the codebase.

```bash
git clone https://github.com/SteeltoeOSS/Steeltoe sources/v2 -b release/2.5
git clone https://github.com/SteeltoeOSS/Steeltoe sources/v3 -b release/3.2
git clean -fX api
docfx metadata api-v2.json
docfx metadata api-v3.json
docfx metadata api-all.json
```

> This process can take a while to complete, will display many warnings and may increase build time in subsequent runs of `docfx build`.

### Build the site docs

This documentation site is interconnected with Steeltoe's [main site](https://github.com/SteeltoeOSS/MainSite). In order to run the two together, the appropriate main-site.json file variant is used to identify where the main site is running.

```bash
# main site -> https://steeltoe.io
docfx build --globalMetadataFiles main-site.json

# main site -> https://dev.steeltoe.io
docfx build --globalMetadataFiles main-site.dev.json

# main site -> http://localhost:9081
docfx build --globalMetadataFiles main-site.localhost.json
```

### Run local HTTP server

```bash
docfx serve _site -p 9082
```
