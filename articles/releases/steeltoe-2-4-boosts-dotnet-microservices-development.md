---
title: Steeltoe 2.4 Boosts .NET Microservices Development
_description: Application generation, a new site, and the Steeltoe CLI - Get familiar with Steeltoe 2.4.
description: Application generation, a new site, and the Steeltoe CLI - Get familiar with Steeltoe 2.4.
type: markdown
date: 01/03/2019
uid: releases/steeltoe-2-4-boosts-dotnet-microservices-development
tags: [new-release]
author.name: David Dieruf
author.github: ddieruf
author.twitter: dierufdavid
---

# Steeltoe 2.4 Boosts .NET Microservices Development with a Code Generator, New Getting Started Guides, and More

The Steeltoe framework helps .NET developers create cloud-native applications. As its feature set grows, so does its popularity (5.8 MM downloads and counting). Much of this innovation comes from your feedback, community contributions, and all-round improvements in the .NET runtime. But it’s not always about the bits. Developers need documentation and other supporting tools to be productive! 

To this end, we’ve cooked up some nifty things to help you do more with Steeltoe:

*   **A new [Initializr site](https://steeltoe.io/initializr) gets you going faster.** Use this tool, and your starting codeset can include common dependencies, like internal single sign-on libraries and helper utilities. 
*   **Getting Started Guides for each component.** Be more productive in your first few minutes with Steeltoe features! The new [Steeltoe.io](https://steeltoe.io) offers intuitive guides for each feature in the framework. Each guide is a working project with clearly defined code on how to implement that feature. Invest 5 minutes and deploy a “hello world” app using that capability!

Both of these enhancements warrant a closer look, so read on! 

## Application Code Generation with Steeltoe Initializr

The [Initializr](https://steeltoe.io/initializr) is an application generator that speeds the creation of cloud-native .NET projects. 

How many times have you run `dotnet new`, added in Steeltoe libraries, and your own custom dependencies? You may have repeated the action enough to say “I should create a template for this.” That’s the Steeltoe Initializer, a big time saver. Like its sibling [Spring Initializr](https://start.spring.io), the Steeltoe version offers .NET templates with a variety of dependencies already “baked in.” And this tool comes with the project team’s seal of approval for cloud-native best practices! [Try the Initializr now!](https://steeltoe.io/initializr)


## Getting Started Guides on Steeltoe.io Help You Ramp Up

To make your life easier, we made a simple getting started guide for every component. Each guide includes detailed step-by-step instructions for using the feature in your local environment, on Kubernetes, as well as other select platforms. [Browse the site](https://steeltoe.io), or choose a [getting started guide](https://steeltoe.io/get-started) and get hands-on.


## Steeltoe CLI Brings Dev/Prod Parity One Step Closer to Reality

The [Steeltoe CLI](https://github.com/SteeltoeOSS/Tooling) aims to help you write better code by improving parity across your environments. Say your app is using a cache. Why should you have to mock up a store locally, only to find a whole host of challenges when it’s bound to a cache on your chosen runtime? The CLI provides a manifest-driven experience where you can do a “push” locally. This mimics what you do during a real deployment, bringing your desktop closer with production. Head over to the [git repo and learn more](https://github.com/SteeltoeOSS/Tooling).


## Official Support for .NET Core 3.0 and Other Enhancements

Steeltoe 2.4 adds support for [.NET Core 3.0](https://devblogs.microsoft.com/dotnet/announcing-net-core-3-0/). ASP.NET Core recently adopted the GenericHost for all app scenarios. So we thought it an opportune moment to improve the component setup for Steeltoe. In previous versions, many Steeltoe components needed to be wired up in a two-step process (in startup.cs). First, you add components to the service container in ConfigureServices. Then you activate them in the Configure method. This flow could result in irritating issues that were often hard to diagnose. 

This process gets easier in Steeltoe 2.4. Here’s how.

The release includes a variety of HostBuilder extensions that provide one-liners for adding Steeltoe components. These simple setup instructions should reduce middleware sequencing bugs. 

We’ve also improved the experience with the Logging Actuator, by auto-wiring dynamic logging. The auto-wiring process will also remove the Microsoft Console Logger (if present) to avoid the potential for duplicate log entries on the console. Learn more about the new “AddCloudFoundryActuators()” builder and “AddLoggingActuator()” builder in [Cloud Management](https://steeltoe.io/cloud-management).


## Getting Started with Steeltoe has Never Been Easier

Head over to the [getting started guides](https://steeltoe.io/get-started). Combine this with the samples in the [Steeltoe Github repo](https://github.com/SteeltoeOSS/Samples), and you’ll have .NET microservices up and running before you know it! 

Want a complete runtime? Try [Pivotal Web Services](https://run.pivotal.io/) for free! This will give you access to a complete modern app runtime. 

# Steeltoe Training

Want to get deeper into creating cloud-native .NET apps with Steeltoe? Attend the Pivotal Platform Acceleration Lab’s [4 day .NET developer course](https://pivotal.io/platform-acceleration-lab/pal-for-developers-net). You’ll get hands-on cloud-native .NET training, learn best practices when creating microservices, and become a Steeltoe ninja!
