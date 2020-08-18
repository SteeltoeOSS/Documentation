---
_disableContribution: true

title: The Easy Way to Modernize and Transform Your .NET Apps with Steeltoe
_description: How do you plan to modernize your .NET application estate?
description: How do you plan to modernize your .NET application estate?
type: markdown
date: 01/03/2019
uid: articles/the-easy-way-to-modernize-and-transform-your-dotnet-apps-with-steeltoe
tags: [ "modernize" ]
author.name: David Dieruf
author.github: ddieruf
author.twitter: dierufdavid
---

# The Easy Way to Modernize and Transform Your .NET Apps with Steeltoe

“How do you plan to modernize your .NET application estate?” When I’ve asked this question to enterprises, the answer usually goes something like this: “We know we need to do something, but we’re not sure what.” Another common answer is: “Given how interwoven we are with IIS and Windows Server features, how could our apps possibly be made cloud-native?”

Truth is, there is no single solution for modernizing applications. But there are some very [clearly defined paths](https://content.pivotal.io/blog/help-has-arrived-how-pcf-2-1-changes-how-you-look-at-windows-and-net). And the .NET app modernization playbook looks a lot like the playbook for Java apps. If your goal is greater feature velocity for your .NET apps, then you need to adopt tried-and-true patterns like microservices, service discovery, circuit breakers, 12-factor and modern platforms. \
 \
Perhaps it’s not a surprise that Java and .NET modernization look so similar. After all, any experienced developer will tell you that it’s better to use proven tech from others wherever possible. That way, software engineers can focus on building great code that differentiates the business, rather than redundant scaffolding. 

How do you empower your developers to write amazing code? Give them the right tools for the job. ([In fact, this is a big reason why enterprises adopt microservices](https://content.pivotal.io/blog/should-that-be-a-microservice-keep-these-six-factors-in-mind).) The exclusive use of any given runtime is expensive and limiting. Instead, you should foster an ecosystem with best-of-breed technologies for different business needs inside your organization.

The Java and Linux world have their modern tools. Let’s now take a look at what’s happening in the .NET and Windows side of the enterprise.

## Steeltoe Is .NET for the Modern Age

Which brings us to [Steeltoe](https://steeltoe.io), one of the most prominent open source .NET projects to hit the scene in recent years (it has over 1.5 *million* downloads on NuGet). Steeltoe brings proven patterns, and can transform an application’s cloud suitability in just a few hours. It can also foster decoupled services and statelessness to IIS-dependent apps that are especially challenging.

“How on earth does it do this?!” you might ask. Three components hold the answer: service discovery, circuit breakers and config servers.

### Service Discovery

Service discovery is how your microservices discover each other. It’s a modern alternative to using a service’s URL directly. With the help of Eureka, part of Spring Cloud Netflix, [Steeltoe has a library](https://steeltoe.io/docs/steeltoe-discovery/) for both .NET Core and Framework apps. Here, your apps register themselves in the Eureka service registry. Your apps also look to the registry for _their_ dependent services. Eureka keeps tabs on all the instances of your microservices, and ensures they can all find each other. The runtime of your services doesn’t matter, because it’s a microservice and the interface is HTTPS-based. From a C# point of view, you still call external services with things like WebRequest, but alternatively, you would use a Eureka-provided URL.

### Circuit Breakers

Modern applications need to be resilient, even as they change constantly. That means elegantly handling a complex web of dependencies. Often, a product team won’t know the precise nature of the dependencies for even a small slice of code. What’s more, that team won’t have control over the availability of dependent services. It’s a challenging thing for your enterprise to manage — and awhich is a simply reality for your engineering teams day to day.

The flip side of that same reality? Your success as a software-driven business depends on dozens of teams rapidly delivering wonderful features for customers at scale. Each team has different SLAs and are on different release cycles. Some apps are older and their dependability is questionable. How do you reconcile these two realities? How do you insulate an app from failure, even when you don’t control its backing services? Use circuit breakers.

Similar to the service discovery, our friends at Netflix have also solved this challenge for us. With Hystrix server, you can bring circuit breaker patterns to your system. Instead of an app calling another app and wrapping in a try/catch (or worse just hoping for the best), you create a “circuit.” The circuit not only includes a call to another app but also intelligence about what to do if the downstream app fails, and how often to poll that downstream app for availability.

Through the Spring Cloud Netflix libraries, [Steeltoe offers](https://steeltoe.io/docs/steeltoe-circuitbreaker/) a package for interacting with the Hystrix server. You can create these patterns in .NET. You can even perform all kinds of awesome failover actions automatically! For example you can use Steeltoe to bundle [circuit open, circuit closed](https://steeltoe.io/docs/steeltoe-circuitbreaker/) scenarios in an app’s integration tests. The end result: a highly available resilient application that automatically handles issues with its dependent services.

### Config Server

The twelve-factor method tells us to “[store config in the environment](https://12factor.net/config).” It’s common to start using this pattern to store environment variables. Complexity can quickly grow. Soon you may need to hold the dynamic global values that all app instances reference. Should one of those values change, all running instances need to be notified. 

You’ll also need a single configuration service that can distinguish between different environments, from testing to prod. Once again, Steeltoe brings a solution to .NET developers. Standing on the shoulders of Spring Cloud Config Server, [Steeltoe Config Server](https://steeltoe.io/docs/steeltoe-configuration/#2-0-config-server-provider/) brings incredible utility to .NET apps. 

Here’s how it works:

1. You create a separate Git repo holding the configuration values; 
2. Configure your app to rely on retrieving values from that doc for configuration; 
3. Now each time a commit is made to the repo holding the doc, all apps will be notified and refreshed.

For more information about config server and lots of sample code, check out the [Steeltoe docs](https://steeltoe.io/docs/steeltoe-configuration/#2-0-config-server-provider).

## Taking Steeltoe Further

Extending the Spring Cloud suite of services is just the beginning of what Steeltoe can offer. Dynamic logging levels, automatically created management endpoints, cloud connectors and security providers also can take your .NET microservices to the next level.

Getting started with Steeltoe is also very easy. [The site](https://steeltoe.io) has all kinds of code examples and [documentation](https://steeltoe.io/docs/steeltoe-configuration/) about each of the framework’s packages. There are step-by-step instructions to deploy a [sample app](https://github.com/SteeltoeOSS/Samples) for each feature or there is a [single app](https://github.com/SteeltoeOSS/Samples/tree/dev/MusicStore) you can deploy using [almost] all the framework’s features.

You can deploy samples on your desktop, or to include the Cloud Foundry options, you can use Pivotal’s [free trial services](https://run.pivotal.io/). Once you have a good understanding of how the sample apps are designed and deployed, it will be a breeze to get your app going.

And remember: While Pivotal is the main contributor to Steeltoe, it is a community-driven project in the [.NET foundation](https://dotnetfoundation.org/Projects?searchquery=steeltoe&type=project). You can see all the source code and develop new features to help continue its growth.

Steeltoe brings tremendous value to a portfolio of .NET apps in need of modernization, as well as to new .NET apps that are being created for the cloud. With such little overhead — simply adding the Nuget distributed packages to a .NET framework (4.x) or .NET core app — and in turn getting such huge cloud-native goodness, the decision to give Steeltoe a try should be an easy one. 
