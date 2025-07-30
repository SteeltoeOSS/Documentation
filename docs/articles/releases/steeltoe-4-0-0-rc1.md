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
This release brings a modernized developer experience, compatibility with the latest .NET versions, improved performance and scalability, enhanced security and updated support for essential cloud platforms and third-party tools.
Whether you're starting something new or modernizing an existing app, 4.0 is the best way to build cloud-native .NET applications today.

## What's New Since 3.2?

Steeltoe 4.0 introduces changes across nearly every area of the framework and surrounding projects and is built for .NET 8+

For a deeper dive, check out the full changelog in the docs: [What's New in Steeltoe 4.0](https://steeltoe.io/docs/v4/welcome/whats-new.html)

### Enhanced Developer Experience

Steeltoe 4.0 brings modern .NET features and tooling improvements that make development faster and more productive:

* Streamlined packages (`*Base` and `*Core` packages have been combined), now with debug symbols embedded
* Async-first APIs
* Nullable annotations
* Embedded JSON schemas for IDE completion
* Simplified and consistent extension methods for dependency injection and configuration
* Updated developer container images for local development

### Cleaner APIs, Fewer Surprises

Steeltoe 4.0 delivers simpler APIs that align with modern .NET expectations, helping developers get started faster and reducing long-term friction.
Over the years, Steeltoe accumulated a large set of APIs that solved similar problems in slightly different ways.
Almost all of the APIs Steeltoe provided were public, with little to no guidance on when and how to extend Steeltoe.
This made it harder for developers to discover the "right" approach — and it made maintenance difficult.

With Steeltoe 4.0, we've taken a deliberate approach to address this:

* **Reviewed the entire public API** to eliminate duplication, improve clarity and consistency and hide details that were not designed to be extensible.
* **Aligned APIs more closely with Microsoft conventions**, reducing surprises for all developers.
* **Added internal tooling** to detect breaking changes across releases and ensure future stability.

Breaking changes are [documented](https://steeltoe.io/docs/v4/welcome/whats-new.html), and our [samples](https://github.com/SteeltoeOSS/Samples/tree/4.x) reflect the latest best practices.

#### Steeltoe Entrypoint Changes

The majority of Steeltoe's functionality can be added to an application via `IServiceCollection` and `IConfigurationBuilder` extension methods.
In some cases, prior versions of Steeltoe also included various flavors of extension methods on host builders.
These extension methods enabled adding Steeltoe components with a single line of code in `Program.cs` or `Startup.cs`.
These days, however, `HostApplicationBuilder` and `WebApplicationBuilder` provide simple and direct access to the configuration builder and service collection, allowing all app configuration code in `Program.cs`, so the extra layer doesn't add the same value.

For 4.0, we reviewed all extension methods to ensure that we don't unnecessarily offer multiple APIs to accomplish the same thing.
In places where host builder extensions are definitely useful, we've ensured they're available for all currently applicable options and worked to enhance the XML comments so that it's easier for you to determine which option to use.

### Dependency and Compatibility Updates

All dependencies have been updated to remove known vulnerabilities and ensure compatibility with the latest versions.
By upgrading to Steeltoe 4.0, you benefit from improved performance, resiliency and security across the stack.

Furthermore, all of Steeltoe's integrations with external systems have been retested against the latest versions of major enterprise and open-source platforms, such as:

* MongoDB, MySQL, PostgreSQL, Microsoft SQL Server
* RabbitMQ
* Redis and Valkey
* Spring Cloud Config Server
* Spring Cloud Netflix Eureka and HashiCorp Consul
* Tanzu Platform (Cloud Foundry)
* Spring Boot Admin
* Prometheus and Grafana

### Improved Documentation, Samples and Supporting Systems

Steeltoe 4.0 comes with updated docs, modernized samples, and improved tooling to help you get started faster and with more confidence. That means:

* Rewritten and reorganized samples that reflect modern .NET practices and highlight where Steeltoe fits in.
* An updated [Steeltoe Initializr](https://start.steeltoe.io/) that supports Steeltoe 4.0.
* Documentation improvements across all major areas.
* Improved test coverage and continuous integration pipelines to support faster, more reliable releases — with fewer surprises for users.
* Improved standards and automated tools for maintaining high levels of code quality.

### What's Gone

We've also used this release as an opportunity to simplify the project by removing several components that did not see great adoption and were very complex.
If you're looking for something that seems to have disappeared, check out the list of [dropped components](https://github.com/SteeltoeOSS/Steeltoe/issues/1244).

These decisions weren't taken lightly — but we believe they'll help keep Steeltoe focused and easier to use.

## Production-Ready and Supported

Although this release is labeled a "release candidate", Steeltoe 4.0.0 RC1 reflects our intent for the final 4.0.0 release and is classified as a go-live release, allowing you to use it in production environments.
It has passed our full suite of automated and manual tests, including cross-platform and integration scenarios.

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
We're excited to share this release with you — and eagerly await your feedback.
