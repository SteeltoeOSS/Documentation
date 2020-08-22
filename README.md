# Documentation

## Overview

This is the home of Steeltoe documentation and blog articles. The site uses [docfx](https://dotnet.github.io/docfx) to convert markdown to html as well site navigation. To get the docfx cli visit [this](https://dotnet.github.io/docfx/tutorial/docfx_getting_started.html) page to see the different distributions.

If you are using VS Code as your editor, there is a [super sweet plugin for docfx](https://marketplace.visualstudio.com/items?itemName=tintoy.docfx-assistant). All it really does is discover `UID`'s and provide intellisense with the `@` char is typed in markdown.

Also you can use *most* of the features included in Microsoft's [docs authoring pack](https://marketplace.visualstudio.com/items?itemName=docsmsft.docs-authoring-pack).

## Folders

- /api: holds the project documentation markdown and table of contents
  - /v2: version 2 documentation
  - /v3: version 3 documentation
- /apidoc: in the future this will hold generated api docs from src
- /articles: holds the markdown for blog posts
- /images: the images
- /template/steeltoe: odd files that overwrite the default docfx theme

## Markdown parser

Docfx offers a custom flavor of markdown with quite a few enhanced capabilities. To see examples and learn more, view the [docfx specifications](https://dotnet.github.io/docfx/spec/docfx_flavored_markdown.html?tabs=tabid-1%2Ctabid-a).

### Links and Cross References

As you get familiar with docfx you'll notice the addition of a yaml header in the markdown files. Values in this header let you control page design as well as set the pages `UID`. With this you can create `xref` as well as use docfx's `@` shorthand. Lean more about [linking in docfx](https://dotnet.github.io/docfx/tutorial/links_and_cross_references.html). **Note** is should be very rare that you hard code a link to an 'html' page with your markdown. Instead use it's `UID` and let the path get calculated as well links get validated when building the project.

## Creating a new blog post

Create a new `.md` file in the `articles` folder. Name the file something authentic that is URL safe. Then in `/articles/index.md` include a shorthand link to the document as well as a short description. If the post should also be included in Steeltoe's rss feed, add a link entry in `articles/rss.xml`.

Here is a starter blog post:

```markdown
---
type: markdown
title: My Very Authentic Blog Post Title
_description: A short description about my topic. Maybe 2 sentences long.
description: A short description about my topic. Maybe 2 sentences long.
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

## Creating a new api document

Similar to the blog post you're going to create a new markdown file, but in the `api` folder. The name needs to be URL safe. Notice in the api folder there is a `v2` and `v3` subfolder. Within each of those are folder for each component. Place your content accordingly. To include the file in the table of contents, add it in `api/(version)/toc.yml`. Notice in the examples below that the `topicHref` are not absolute paths. Docfx will calculate everything at build time.
An example api doc:

```markdown
---
uid: api/v2/circuitbreaker/hystrix
---

# Netflix Hystrix

Steeltoe's Hystrix implementation lets application developers isolate and manage back-end dependencies so that a single failing dependency does not take down the entire application. This is accomplished by wrapping all calls to external dependencies in a `HystrixCommand`, which runs in its own...

Here is an example cross reference link to config docs: @api/v2/configuration/cloud-foundry-provider
Or you could link to the v3 version of this doc: @api/v3/circuitbreaker/hystrix
Or do the same thing by provide custom link text: [view the v3 version](xref:api/v2/circuitbreaker/hystrix)
```

Corresponding entry in api/v2/toc.yml:

```yml
- name: Circuit Breakers
  items:
    - topicHref: circuitbreaker/hystrix.md
      name: Hystrix
```

## Building the site

Use docfx's [user manual](https://dotnet.github.io/docfx/tutorial/docfx.exe_user_manual.html), to build and run the site in a few different ways. The simplest way is to `cd` into the root folder of this project and run the following command. The site will build in a temp folder named `_site` and be served at http://localhost:8080.

```powershell
docfx build --serve --port 8081
```

You can also specify where the build output should land

```powershell
docfx build -o "../publish"
```
