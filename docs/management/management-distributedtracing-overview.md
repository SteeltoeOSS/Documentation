# Distributed Tracing

Steeltoe distributed tracing implements a solution for .NET applications based on the open source [OpenCensus](https://opencensus.io/) project. For most users implementing and using distributed tracing should be invisible, and many of the interactions with external systems should be instrumented automatically. You can capture trace data in logs, or by sending it to a remote collector service.

>NOTE: The OpenCensus implementation used in Steeltoe (for example, `Steeltoe.Management.OpenCensus`) has been contributed to the OpenCensus community. At some point in the near future the distributed tracing functionality will move to using it, instead of the Steeltoe version.

A Span is the basic unit of work. For example, sending an RPC is a new span, as is sending a response to an RPC. Span’s are identified by a unique 64-bit ID for the span and another 64-bit ID for the trace the span is a part of. Spans also have other data, such as descriptions, key-value annotations, the ID of the span that caused them, and process ID’s (normally IP address). Spans are started and stopped, and they keep track of their timing information. Once you create a span, you must stop it at some point in the future. A set of spans forming a tree-like structure called a Trace. For example, if you are running a distributed big-data store, a trace might be formed by a put request.

Features:

* Adds trace and span ids to the application log messages, so you can extract all the logs from a given trace or span in a log aggregator.
* Using the  [OpenCensus](https://opencensus.io/) APIs we provide an abstraction over common distributed tracing data models: traces, spans (forming a DAG), annotations, key-value annotations.
* Automatically instruments common ingress and egress points from .NET applications (e.g MVC Controllers, Views, Http clients).
* Optionally generate, collect and export Zipkin-compatible traces via HTTP.

>NOTE: Currently, distributed tracing is only supported in ASP.NET Core applications.

