---
_disableContribution: true
_hideTocVersionToggle: true

description:
date: 07/29/2025
uid: steeltoe-4-0-0-rc1.html
tags: ["new-release"]
author.name: Tim Hess
---

# Steeltoe 4.0.0 RC1: A Major Maintenance Release

Steeltoe 4.0.0 RC1 is now available — and it's production-ready.
This release brings a modernized developer experience, compatibility with the latest .NET versions, embedded schema support for better tooling, and updated support for essential cloud platforms and tools.
Whether you're starting something new or modernizing an existing app, 4.0 is the best way to build cloud-native .NET applications today.

## What's New Since 3.2?

Steeltoe 4.0 introduces changes across nearly every area of the framework and surrounding projects and is built for .NET 8+

For a deeper dive, check out the full changelog in the docs: [What's New in Steeltoe 4.0](https://steeltoe.io/docs/v4/welcome/whats-new.html)

### Enhanced Developer Experience

Steeltoe 4.0 brings modern .NET features and tooling improvements that make development faster and more productive:

* Streamlined packages (`*Base` and `*Core` packages have been combined), now with debug symbols and JSON schemas embedded
* Async-first APIs
* Nullable annotations
* Embedded JSON schemas for IDE completion
* Simplified and consistent extensions for dependency injection and configuration
* Updated developer container images for testing local environments

### Cleaner APIs, Fewer Surprises

Steeltoe 4.0 delivers simpler APIs that align with modern .NET expectations, helping developers get started faster and reducing long-term friction.
Over the years, Steeltoe accumulated a large set of APIs that solved similar problems in slightly different ways.
Almost all of the APIs Steeltoe used were public, with little to no guidance on when and how to extend Steeltoe.
This made it harder for developers to discover the "right" approach — and it made maintenance difficult.

With Steeltoe 4.0, we've taken a deliberate approach to address this:

* **Reviewed the entire public API** to eliminate duplication, improve clarity and hide details that were not designed to be extensible.
* **Aligned APIs more closely with Microsoft conventions and within Steeltoe**, reducing surprises for experienced .NET developers.
* **Added tooling** to detect breaking changes across releases and ensure future stability.

We know breaking changes can be frustrating, but we've tried to make this the last time it's necessary for a long while.

Breaking changes are [documented](https://steeltoe.io/docs/v4/welcome/whats-new.html), and our [samples](https://github.com/SteeltoeOSS/Samples/tree/4.x) reflect the latest best practices.

#### Steeltoe Entrypoint Changes

The majority of Steeltoe's functionality can be added to an application via `IServiceCollection` and `IConfigurationBuilder` extension methods.
In some cases, prior versions of Steeltoe also included various flavors of `HostBuilder` extensions, providing the option of adding Steeltoe components with a single line of top-level code.
While that extra layer doesn't add much complexity (`HostBuilder` extensions typically called the same `IServiceCollection` and/or `IConfigurationBuilder` extensions under the covers), it also didn't commonly add much value and introduces another place to update any time Microsoft adds another flavor of `HostBuilder`.
The value is further brought into question when you consider that `HostApplicationBuilder` and `WebApplicationBuilder` provide direct access to `IServiceCollection` and `IConfigurationBuilder`.

For 4.0, we reviewed all of these extension methods to ensure we don't have situations where these additional options don't add value.
In places where `HostBuilder` level extensions are definitely useful, we've made sure they're available for all currently-applicable options and worked to enhance the XML comments so it's easier for you to know which option to use.

### Dependency and Compatibility Updates

All dependencies have been updated to remove known vulnerabilities and ensure compatibility with the latest secure versions.
By upgrading to Steeltoe 4.0, you benefit from improved resilience and security across the stack.

In order to take advantage of performance, security, and feature updates in other libraries, all of Steeltoe's NuGet dependencies have been updated.
Furthermore, all of Steeltoe's integrations with external systems have been retested against the latest versions of major enterprise and open-source platforms:

* MySQL
* PostgreSQL
* Prometheus
* RabbitMQ
* Redis and Valkey
* Spring Boot Admin
* Spring Cloud Config Server
* Spring Cloud Netflix Eureka
* Tanzu Platform (Cloud Foundry)

### Improved Documentation, Samples and Supporting Systems

Steeltoe 4.0 comes with updated docs, modernized samples, and improved tooling to help you get started faster and with more confidence. That means:

* Rewritten and reorganized samples that reflect modern .NET practices and highlight where Steeltoe fits in.
* An updated [Steeltoe Initializr](https://start.steeltoe.io/) that supports 4.0.
* Documentation improvements across all major areas.
* Rebuilt testing and pipelines to support faster, more reliable releases — with fewer surprises for users

We've done everything we can to ensure that future updates arrive more quickly and with greater clarity.

### What's Gone

We've also used this release as an opportunity to simplify the project by removing several components that are no longer being maintained.
If you're looking for something that seems to have disappeared, check out the list of [dropped components](https://github.com/SteeltoeOSS/Steeltoe/issues/1244).

These decisions weren't taken lightly — but we believe they'll help keep Steeltoe focused and easier to use.

## Production-Ready and Supported

Although this release is labeled a "release candidate", Steeltoe 4.0.0 RC1 reflects our intent for the final 4.0.0 release and is classified as a go-live release so that you can use it in production environments.
It has passed our full suite of automated and manual tests, including cross-platform and integration scenarios.

If you are planning to run RC1 in production, we recommend:

* Locking to specific NuGet versions until GA
* Reporting any issues via [GitHub Issues](https://github.com/SteeltoeOSS/Steeltoe/issues)
* Joining our [Slack community](https://slack.steeltoe.io/) to stay informed

We do not expect further breaking changes before GA.

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
