# Documentation

## Overview

This is the home of Steeltoe documentation and blog articles. The site uses [DocFX](https://dotnet.github.io/docfx) to convert Markdown to HTML and generate site navigation.

## Directories

| Path | Description
| --- | ---
| `/api` | documentation Markdown and Table of Contents
| `/api/v2` | version 2 documentation
| `/api/v3` | version 3 documentation
| `/articles` | blog post Markdown
| `/images` | images
| `/template` | theming

## DocFX Markdown

DocFX offers an enhanced flavor of Markdown. To see examples and learn more, view the [DocFX Flavored Markdown](https://dotnet.github.io/docfx/spec/docfx_flavored_markdown.html) documentation.

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

Install DocFX using one of 2 options: download from DocFX or use Docker image.

### Download from DocFX

Download DocFX [distribution](https://github.com/dotnet/docfx/releases/).
Unzip to directory of your choosing and add that directory to your `PATH`.
If running on Linux or OS X, you will need to [install Mono](https://www.mono-project.com/docs/getting-started/install/) and use `mono` to execute the DoxFX binary.
See [docfx/docfx](docfx/docfx) for an example wrapper script.

### Docker Image

You can build a Docker image with the DocFX binary and use the Powershell script `docfx.ps` to run the image.
`docfx.ps1` mounts the project directory in the Docker container and passes any arguments to the `docfx` command in the container.

To build the Docker image:

```
$ docker build -t docfx doxfx
```

Sample invocation:

```
$ ./docfx.ps1 --version
docfx 2.59.2.0
Copyright (C) 2022 ? Microsoft Corporation. All rights reserved.
This is open-source software under MIT License.
```

## Building the site

Build API docs for Steeltoe 2 and 3

```
$ git clone https://github.com/SteeltoeOSS/Steeltoe sources/v3 -b release/3.2
$ git clone https://github.com/SteeltoeOSS/Steeltoe sources/v2 -b release/2.5
$ git clean -fX api
$ docfx metadata api-v3.json
$ docfx metadata api-v2.json
```

Build the site docs

```
# main site -> https://steeltoe.io
$ docfx build --globalMetadataFiles main-site.json

# main site -> https://dev.steeltoe.io
$ docfx build --globalMetadataFiles main-site.dev.json

# main site -> http://localhost:9081
$ docfx build --globalMetadataFiles main-site.localhost.json
```

Run local HTTP server

```
$ docfx serve _site -p 9082
```
