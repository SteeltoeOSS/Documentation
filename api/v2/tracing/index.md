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

## Usage

You should understand how the .NET [Configuration service](https://docs.microsoft.com/aspnet/core/fundamentals/configuration) works before starting to use the management endpoints. You need at least a basic understanding of the `ConfigurationBuilder` and how to add providers to the builder to configure the endpoints.

When developing ASP.NET Core applications, you should also understand how the ASP.NET Core [Startup](https://docs.microsoft.com/aspnet/core/fundamentals/startup) class is used in configuring the application services for the app. Pay particular attention to the usage of the `ConfigureServices()` and `Configure()` methods.

Steeltoe distributed tracing automatically applies instrumentation at key ingress and egress points in your ASP.NET Core application so that you are able to get meaningful traces without having to do any instrumentation yourself. These points include:

* HTTP Server
  * Request Start & Finish
  * Unhandled and Handled exceptions
  * MVC Action Start & Finish
  * MVC View Start & Finish
* HTTP Client (Desktop and Core)
  * Outgoing Request Start & Finish
  * Unhandled and Handled exceptions

### Add NuGet References

To use the distributed tracing exporters, you need to add a reference to the appropriate Steeltoe NuGet based on the type of the application you are building and what Dependency Injector you have chosen, if any.

The following table describes the available packages:

|App Type|Package|Description|
|---|---|---|
|All|`Steeltoe.Management.TracingBase`|Base functionality, no dependency injection|
|ASP.NET Core|`Steeltoe.Management.TracingCore`|Includes `TracingBase`, adds ASP.NET Core DI|

To add this type of NuGet to your project, add a `PackageReference` resembling the following:

```xml
<ItemGroup>
...
    <PackageReference Include="Steeltoe.Management.TracingCore" Version="2.5.4" />
...
</ItemGroup>
```

or

```powershell
PM>Install-Package  Steeltoe.Management.TracingCore -Version 2.5.2
```

### Configure Settings

Distributed tracing can be configured by using the normal .NET [Configuration service](https://docs.microsoft.com/aspnet/core/fundamentals/configuration).

All settings should be placed under the prefix with the key `management:tracing:`.

|Key|Description|Default|
|---|---|---|
|name|the name of the application|spring:application:name, Cloud Foundry name, or "Unknown"|
|ingressIgnorePattern|Regex pattern describing what incoming requests to ignore|See `TracingOptions`|
|egressIgnorePattern|Regex pattern describing what outgoing requests to ignore|See `TracingOptions`|
|maxNumberOfAttributes|max attributes attachable to OpenCensus span|32|
|maxNumberOfAnnotations|max annotations attachable to OpenCensus span|32|
|maxNumberOfMessageEvents|max events attachable to OpenCensus span|128|
|maxNumberOfLinks|max links attachable to OpenCensus span|128|
|alwaysSample|enable the OpenCensus AlwaysSampler|OpenCensus ProbabilitySampler|
|neverSample|enable the OpenCensus NeverSampler|OpenCensus ProbabilitySampler|
|useShortTraceIds|truncate the ids to 8 bytes instead of 16, use for backwards compatibility with Spring Sleuth, PCF Metrics, etc.|true|

### Enabling Log Correlation

If you want to use distributed tracing together with log correlation, then you must utilize the `Steeltoe Logging provider` in your application.

<!-- TODO Update links Follow these [instructions](https://steeltoe.io/docs/steeltoe-logging/#1-0-dynamic-logging-provider) for how to enable the provider in your application. -->

Once that is done, then whenever your application issues any log statements, the Steeltoe logger will add additional trace information to each log message if there is an active trace context. The format of that information is of the form:

* `[app name, trace id, span id, trace flags]`  (for example, `[service1,2485ec27856c56f4,2485ec27856c56f4,true]`)

### Propagating Trace Context

When working with distributed tracing systems you will find that a trace context (for example, trace state information) must get propagated to all child processes to ensure that child spans originating from a root trace get collected and correlated into a single trace in the end.  The current trace and span IDs are just one piece of the required information that must get propagated.

Steeltoe distributed tracing handles this for you by default when using the .NET HttpClient. When a downstream HTTP call is made, the current trace context is encoded as request headers and sent along with the request automatically.  Currently, Steeltoe encodes the context using [Zipkin B3 Propagation](https://github.com/openzipkin/b3-propagation) encodings. As a result, you will find that Steeltoe tracing is interoperable with several other instrumentation libraries such as [Spring Cloud Sleuth](https://cloud.spring.io/spring-cloud-sleuth/2.0.x/single/spring-cloud-sleuth.html).

### Add Distributed Tracing

To enable distributed tracing all you need to to do is add the service to the container. To do this use the `AddDistributedTracing()` extension method from `TracingServiceCollectionExtensions`.

```csharp
public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; set; }

    public void ConfigureServices(IServiceCollection services)
    {
        ...
        // Add Distributed tracing
        // Available through Steeltoe.Management.Tracing namespace
        services.AddDistributedTracing(Configuration);

        // Add framework services.
        services.AddMvc();
    }
    public void Configure(IApplicationBuilder app)
    {
        app.UseStaticFiles();

        app.UseMvc();
    }
}
```
