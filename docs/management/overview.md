# Cloud Management

This section describes the various management endpoints that you can configure Steeltoe to expose.

## Endpoints

Steeltoe includes a number of optional features that you can add to your applications to aid in monitoring and managing it while it runs in production. These features are implemented as a number of management endpoints that you can easily add to your application.

The way the endpoints are exposed and used depends on the type of technology you choose in exposing the functionality of each endpoint. Out of the box, Steeltoe provides several easy ways to expose these endpoints over HTTP in .NET applications. Of course, you can build and use whatever you would like to meet your needs.

When you expose the endpoints over HTTP, you can also integrate the endpoints with the [Pivotal Apps Manager](https://docs.pivotal.io/pivotalcf/2-0/console/index.html). You should read the [Using Actuators with Apps Manager section](https://docs.pivotal.io/pivotalcf/2-0/console/using-actuators.html) of the Pivotal Cloud Foundry documentation for more details.

>NOTE: Depending on your hosting environment, service instances you create for the purpose of exploring the Quick Starts on this page may have a cost associated.

## Distributed Tracing

Steeltoe distributed tracing implements a solution for .NET applications based on the open source [OpenTelemetry](https://opentelemetry.io/) project. For most users implementing and using distributed tracing should be invisible, and many of the interactions with external systems should be instrumented automatically. You can capture trace data in logs, or by sending it to a remote collector service.

A "span" is the basic unit of work. For example, sending an RPC is a new span, as is sending a response to an RPC. Each span is identified by a unique 64-bit ID for the span, and the trace of which the span is a part is identified by another 64-bit ID. Spans also have other data, such as descriptions, key-value annotations, the ID of the span that caused them, and process ID’s (normally an IP address). Spans are started and stopped, and they keep track of their timing information. Once you create a span, you must stop it at some point in the future. A set of spans form a tree-like structure called a "trace". For example, if you are running a distributed big-data store, a trace might be formed by a put request.

Features:

* Adds trace and span ids to the application log messages, so you can extract all the logs from a given trace or span in a log aggregator.
* Using the  [OpenTelemetry](https://opentelemetry.io/) APIs we provide an abstraction over common distributed tracing data models: traces, spans (forming a DAG), annotations, key-value annotations.
* Automatically instruments common ingress and egress points from .NET applications (e.g MVC Controllers, Views, Http clients).
* Optionally generate, collect and export Zipkin-compatible traces via HTTP.

>NOTE: Currently, distributed tracing is supported only in ASP.NET Core applications.

## Management Tasks

Steeltoe Management Tasks provide a means of running administrative tasks for ASP.NET Core applications with the same context as the running version of your application. The original use case for this feature is managing database migrations with a bound database service on Cloud Foundry, but the framework is extensible for you to create your own tasks.
