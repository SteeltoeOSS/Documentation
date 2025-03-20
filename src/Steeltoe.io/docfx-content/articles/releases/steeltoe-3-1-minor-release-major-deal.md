---
_disableBreadcrumb: true
_disableContribution: true
_showBackToBlogs: true

title: Steeltoe 3.1 is a minor release, but a major deal
description: All about Steeltoe 3.1
monikerRange: '== steeltoe-3.1'
type: markdown
date: 07/13/2021
uid: releases/steeltoe-3-1-minor-release-major-deal.html
tags: ["new-release"]
author.name: David Dieruf
author.github: ddieruf
author.twitter: dierufdavid
---

# Steeltoe 3.1 is a minor release, but a major deal

While Steeltoe 3.1 is a minor release, it sure feels like a major deal. The project recently passed [22 million downloads](https://www.nuget.org/profiles/SteeltoeOSS) and there’s no sign of slowing down. This release kept a very tight focus on two new features and includes a welcome contribution from the Steeltoe community.

As you may know, the vision of the Steeltoe project has been to provide .NET developers with the tools and frameworks that enable them to:

* Build production-grade microservices using common distributed systems patterns such as externalized configuration, service discovery, circuit-breaking, distributed tracing, etc.
* Build polyglot microservices-based applications with Java/Spring interoperability builtin.
* Build portable cloud based applications by provided programming abstractions on top of cloud provider "lock-in APIs"; "Your .NET code => on any cloud platform"

With the introduction of 3.1, Steeltoe now includes a new Stream programming model with full compatibility with Spring Cloud Data Flow. Teams can now use both Java and .NET to create resilient, high throughput polyglot data processing pipelines. For example, you can create a pipeline that uses a pre-made Java component with a RESTful endpoint for ingesting data into the pipeline. Then add a C# processor to the pipeline that makes decisions about the data - possibly transforming it to other formats. Finally, passing the data to a third Java component which acts as a sink sending the data to a database. The combination of Java and .NET in SCDF unlocks a true polyglot model for data pipeline developers.

Also included in 3.1 is an updated Messaging programming model that makes it very easy to create message based, event-driven microservices. With just a few lines of code a developer can standup a production-grade, highly resilent, message driven microservice.

Let’s look at all the new features of Steeltoe 3.1 and how it’s going to get your microservices to production even faster.

## Steeltoe Messaging with RabbitMQ

[Steeltoe Messaging](/api/v3/messaging/index.md) (which was introduced in 3.0) brought three main features to simplify developing event-driven microservices:

* Listener container - a high-level abstraction for asynchronous processing of inbound messages
* RabbitTemplate - an abstraction for sending and receiving messages
* RabbitAdmin - for automatically declaring queues, exchanges, and bindings

Steeltoe 3.1 extends those features with a variety of performance improvements, integrations, and bug fixes. Now, you only need to add one set of RabbitMQ configuration properties within your application settings and both the RabbitMQ Connector and RabbitMQ Messaging services can be leveraged automatically.

Wiring up RabbitMQ services is now even simpler with RabbitMQHost. This removes the need to add the standard service registrations and configuration bindings within your application startup. RabbitMQHost does all the work of adding the custom service registrations that are specific to your application. (I.e., queues, listeners, etc.)

## Abstract the message broker with Steeltoe Streams

Very few developers enjoy locking their applications into one vendor. To keep portability at its peak, you want to create abstractions (i.e., interfaces) that aren’t specific to any certain provider. With this model, you can "swap out" the actual provider’s library for another. Just simply implement the interface the application is expecting.

The goal of Steeltoe Messaging is to make interactions with a specific broker very easy. Steeltoe Streams builds on that, to abstract broker specifics and make them more pluggable. Streams offer features like:

* Binder abstraction - a flexible programming model to dynamically choose message destinations at runtime
* Persistent pub/sub semantics - makes sharing a topic easy for both producer and consumers
* Consumer groups - scaling up consumer microservices while maintaining the integrity of the stream processing operation
* Partitioning support - partition between multiple instances for stateful processing (even when the broker does not natively support it)

All of these concepts are covered in depth in [the documentation](/api/v3/stream/index.md) where you can get in-depth with each concept. The goal is to take the burden of learning message semantics away and help you focus on writing your microservice. Have a look at our [quick start guide](/guides/stream/quick-start.md) and get going with message streaming in a flash!

## Support for Spring Cloud Data Flow

If writing multiple microservices and plumbing the connectivity between them is a challenge, the runtime requirement to monitor, scale, and orchestrate them appears downright impossible. Spring Cloud Data Flow meets this challenge by providing tools to create and orchestrate complex topologies for streaming and batch data pipelines. Officially, it's described as "Microservice based Streaming and Batch data processing for Cloud Foundry and Kubernetes."

SCDF brings the following features to support stream-based applications:

* A selection of pre-built stream and batch starter apps for various data integration and processing scenarios.
* Custom Stream applications can be built using Steeltoe Stream or Spring Cloud Stream.
* A simple stream pipeline DSL makes it easy to specify which apps to deploy and how to connect inputs and outputs.
* The dashboard offers a graphical editor for building pipelines interactively.
* SCDF server exposes a REST API for composing and deploying data pipelines. A separate shell makes it easy to work with the API from command line.

Assume you have an edge application that is constantly posting data back to the main system. That system needs to be resilient and always on. The system must ingest data faster than what the edge application is capable of sending. It’s not just about processing the data quickly, it’s about never missing a single posted byte of data - the processing can happen afterward.

SCDF introduces the concept of sources, processors, and sinks. A source is how the data enters the pipeline; think HTTP, FTP, file, cache, etc. The data is then added to a message bus and the (optional) processor is notified. The processor is another microservice that takes in the data and does something meaningful with it; think ML models, augmentation, logging, etc. When processing is complete, the result is added to a second message bus where the sink is notified. The sink is a third microservice that is responsible for sending the data to some meaningful destination; think database, cache, FTP, HTTP, etc.

That’s really just the beginning of SCDF. With data pipelines, you can have multiple sources, processors, or sinks. You can scale each microservice independently. And with the introduction of Steeltoe support, you can mix both Java and .NET services together in one pipeline!

These microservices are a little more "micro" than your typical service. They could be a single .cs that does a small job. They have special attributes that take care of all the message bus management as well as the incoming and outgoing data. To learn more have a look at the [Steeltoe stream docs for SCDF](/api/v3/stream/data-flow-stream.md).

## Steeltoe Bootstrap

Remember the days when you had multiple `using` statements  and a variety of extension methods to bring in all those Steeltoe libraries? Well, we have good news: those days are now behind you. Community member Andrew Stakhov contributed an idea to the incubator called bootstrap and we think it's pretty darn cool. He created a HostBuilder extension that looks for select Steeltoe packages your project references and automatically initializes them in the project. What 'select' packages are included, you might be asking? There is a [full table](/api/v3/bootstrap/index.md#supported-steeltoe-packages) in our documentation. Simply add a reference to any (or all) of those packages and then call `AddSteeltoe()` on the HostBuilder and your microservice will get the best of cloud-native practices. Here is an example of using the bootstrapper:

```csharp
public static IHostBuilder CreateHostBuilder(string[] args) =>
  Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webBuilder =>
    {
      webBuilder.UseStartup<Startup>();
    })
    .AddSteeltoe();
```

## Get Started Today

To get started with any Steeltoe projects, head over to the [getting started guides](../../guides/index.md). Combine this with the samples in the [Steeltoe GitHub repo](https://github.com/SteeltoeOSS/Samples/tree/3.x), and you’ll have .NET microservices up and running before you know it!
