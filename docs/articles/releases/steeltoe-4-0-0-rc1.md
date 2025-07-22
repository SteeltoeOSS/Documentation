---
_disableContribution: true
_hideTocVersionToggle: true

description:
date: 07/23/2025
uid: steeltoe-4-0-0-rc1.html
tags: ["new-release"]
author.name: Tim Hess
---

# Steeltoe 4.0.0 RC1: A Major Maintenance Release

Steeltoe 4.0.0 RC1 is now available — while it's our first non-patch release in quite a while, it represents _much_ more than just a version bump.
This is a major maintenance release: the culmination of deep internal cleanup, thoughtful breakage (in order to reduce friction over the long term), and an across-the-board modernization of the project.

This release marks a giant step toward easier, faster and even more reliable future development of Steeltoe.

## What's New Since 3.2?

Steeltoe 4.0 introduces changes across nearly every area of the framework and surrounding projects. This includes:

* Support for .NET 8+ and `WebApplicationBuilder`
* Revised samples rebuilt from the latest .NET templates
* Improved documentation throughout the codebase and website
* Behind-the-scenes improvements to API consistency and maintainability

Check out the full changelog in the docs: [What's New in Steeltoe 4.0](https://steeltoe.io/docs/v4/welcome/whats-new.html)

## Breaking Changes: Why Now?

Over the years, Steeltoe accumulated a large set of APIs that solved similar problems in slightly different ways.
Almost all of the APIs Steeltoe used were public, with little to no guidance on when and how to extend Steeltoe.
This made it harder for developers to discover the "right" approach — and it made maintenance difficult.

With Steeltoe 4.0, we've taken a deliberate approach to address this:

* **Reviewed the entire public API** to eliminate duplication, improve clarity and hide details that were not designed to be extensible
* **Aligned APIs more closely with Microsoft conventions and within Steeltoe**, reducing surprises for experienced .NET developers
* **Added tooling** to detect breaking changes across releases and ensure future stability

We know breaking changes can be frustrating, but we've tried to make this the last time it's necessary for a long while.

Every breaking change is [documented](https://steeltoe.io/docs/v4/welcome/breaking-changes.html), and our [samples](https://github.com/SteeltoeOSS/Samples/tree/4.x) reflect the latest best practices.

## Modern .NET Support: Simpler App Setup

Steeltoe remains easy to use and integrates naturally with modern .NET patterns, now including first-class support for `WebApplicationBuilder` and `Minimal APIs`.
That means you can write something like this:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.AddCloudFoundryConfiguration();
builder.AddConfigServer();
builder.AddServiceDiscovery();

builder.Services.AddControllers();
builder.Services.AddAllActuators();

var app = builder.Build();

app.MapControllers();
app.Run();
```

Steeltoe components can always be added to an application with `IServiceCollection` and `IConfigurationBuilder` extensions.
Application and host builder extensions are now only provided where they make a difference (usually in cases where multiple lines of code would otherwise be required).

## Better Project Infrastructure

A major focus for 4.0 has been making Steeltoe more maintainable — not just for contributors, but for the broader community. That means:

* Rewritten and reorganized samples that reflect modern .NET practices and highlight where Steeltoe fits in
* An updated [Steeltoe Initializr](https://start.steeltoe.io/) that supports 4.0
* Documentation improvements across all major areas
* Rebuilt testing and pipelines to support faster, more reliable releases — with fewer surprises for users

We've done everything we can to ensure that future updates arrive more quickly and with greater clarity.

## What's Gone

We've also used this release as an opportunity to simplify the project by removing several components that are no longer being maintained.
If you're looking for something that seems to have disappeared, check out the list of [dropped components](https://github.com/SteeltoeOSS/Steeltoe/issues/1244).

These decisions weren't taken lightly — but we believe they'll help keep Steeltoe focused and easier to use.

## Try It Out — and Help Shape 4.0

Steeltoe 4.0.0 RC1 is available on NuGet now.
This is the perfect time to give it a try on a new app or upgrade your existing apps.
[Open an issue](https://github.com/SteeltoeOSS/Steeltoe/issues) if you hit a snag.

We're planning to release the general availability version later this summer. No RC2 is currently planned — so your feedback now is especially valuable.

* 📚 Browse the updated [samples](https://github.com/SteeltoeOSS/Samples/tree/4.x)
* 🔗 Ask questions and join the conversation on [Slack](https://slack.steeltoe.io/)
* 📢 Reach us on [Twitter/X](https://x.com/SteeltoeOSS) and [Bluesky](https://bsky.app/profile/steeltoe.io)

Thanks to everyone in the community who has tested pre-releases, filed issues, and contributed code, docs, and encouragement.
We're excited to share this release with you — and even more excited about what comes next.
