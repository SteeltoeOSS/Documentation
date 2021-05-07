# Distributed Tracing

Steeltoe distributed tracing implements a solution for .NET applications using the open source [OpenTelemetry](https://opentelemetry.io/) project. For most users, implementing and using distributed tracing should be invisible, and many of the interactions with external systems should be instrumented automatically. You can capture trace data in logs or by sending it to a remote collector service.

A "span" is the basic unit of work. For example, sending an RPC is a new span, as is sending a response to an RPC. Spans are identified by a unique 64-bit ID for the span and by another 64-bit ID for the trace of which the span is a part. Spans also have other data, such as descriptions, key-value annotations, the ID of the span that caused them, and process IDs (normally an IP address). Spans are started and stopped, and they keep track of their timing information. Once you create a span, you must stop it at some point in the future. A set of spans form a tree-like structure called a "trace". For example, if you run a distributed big-data store, a trace might be formed by a PUT request.

Steeltoe distributed tracing:

* Adds trace and span IDs to the application log messages, so you can extract all the logs from a given trace or span in a log aggregator.
* Using the  [OpenTelemetry](https://opentelemetry.io/) APIs, provides an abstraction over common distributed tracing data models: traces, spans (forming a DAG), annotations, and key-value annotations.
* Automatically instruments common ingress and egress points from .NET applications (such as MVC Controllers, Views, HTTP clients).
* Optionally generates, collects, and exports Zipkin-compatible traces over HTTP.

## Usage

You should understand how the .NET [configuration service](https://docs.microsoft.com/aspnet/core/fundamentals/configuration) works before starting to use the management endpoints. You need at least a basic understanding of the `ConfigurationBuilder` and how to add providers to the builder to configure the endpoints.

When developing ASP.NET Core applications, you should also understand how the ASP.NET Core [`Startup`](https://docs.microsoft.com/aspnet/core/fundamentals/startup) class is used in configuring the application services for the application. Pay particular attention to the usage of the `ConfigureServices()` and `Configure()` methods.

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

To use the distributed tracing exporters, you need to add a reference to the appropriate Steeltoe NuGet based on the type of the application you are building and what dependency injector you have chosen (if any).

The following table describes the available packages:

| Package | Description | .NET Target |
| --- | --- | --- |
| `Steeltoe.Management.TracingBase` | Base functionality, no dependency injection. | .NET Standard 2.0 |
| `Steeltoe.Management.TracingCore` | Includes `TracingBase`, adds ASP.NET Core DI. | ASP.NET Core 3.1+ |

To add this type of NuGet to your project, add a `PackageReference` resembling the following:

```xml
<ItemGroup>
...
    <PackageReference Include="Steeltoe.Management.TracingCore" Version="3.0.2"/>
...
</ItemGroup>
```

Alternative, you can add it with PowerShell:

```powershell
PM>Install-Package  Steeltoe.Management.TracingCore -Version 3.0.2
```

### Configure Settings

You can configure distributed tracing by using the normal .NET [Configuration service](https://docs.microsoft.com/aspnet/core/fundamentals/configuration).

All settings should be placed under the prefix with a key of `Management:tracing:`.
The following table describes the available settings:

| Key | Description | Default |
| --- | --- | --- |
| `Name` | The name of the application. | `Spring:Application:Name`, Cloud Foundry name, or `Unknown` |
| `IngressIgnorePattern` |Regex pattern describing what incoming requests to ignore. | See `TracingOptions` |
| `EgressIgnorePattern` |Regex pattern describing what outgoing requests to ignore. | See `TracingOptions` |
| `MaxNumberOfAttributes` |Max attributes attachable to OpenTelemetry span. | 32 |
| `MaxNumberOfAnnotations` |Max annotations attachable to OpenTelemetry span. | 32 |
| `MaxNumberOfMessageEvents` |Max events attachable to OpenTelemetry span. | 128 |
| `MaxNumberOfLinks` |max links attachable to OpenTelemetry span. | 128 |
| `AlwaysSample` | Whether to enable the OpenTelemetry `AlwaysOnSampler`. | OpenTelemetry `Sampler` |
| `NeverSample` | Whether to enable the OpenTelemetry `AlwaysOffSampler`. | OpenTelemetry `Sampler` |
| `UseShortTraceIds` | Whether to truncate the IDs to 8 bytes instead of 16. Use it for backwards compatibility with Spring Sleuth, PCF Metrics, and others. | `true` |

### Enabling Log Correlation

To use distributed tracing together with log correlation, you must use the `Steeltoe Logging provider` in your application.

<!-- TODO Update links Follow these [instructions](https://steeltoe.io/docs/steeltoe-logging/#1-0-dynamic-logging-provider) for how to enable the provider in your application. -->

Once that is done, whenever your application issues any log statements, the Steeltoe logger adds additional trace information to each log message if there is an active trace context. The format of that information is of the form `[app name, trace id, span id, trace flags]` (for example, `[service1,2485ec27856c56f4,2485ec27856c56f4,true]`).

### Propagating Trace Context

When working with distributed tracing systems, you will find that a trace context (for example, trace state information) must get propagated to all child processes to ensure that child spans originating from a root trace get collected and correlated into a single trace in the end. The current trace and span IDs are just one piece of the required information that must get propagated.

Steeltoe distributed tracing handles this for you by default when using the .NET HttpClient. When a downstream HTTP call is made, the current trace context is encoded as request headers and sent along with the request automatically. Currently, Steeltoe encodes the context by using [Zipkin B3 Propagation](https://github.com/openzipkin/b3-propagation) encodings. As a result, you will find that Steeltoe tracing is interoperable with several other instrumentation libraries, such as [Spring Cloud Sleuth](https://cloud.spring.io/spring-cloud-sleuth/2.0.x/single/spring-cloud-sleuth.html).

### Add Distributed Tracing

To enable distributed tracing, add the service to the container. To do so, use the `AddDistributedTracing()` extension method from `TracingServiceCollectionExtensions`:

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
        services.AddDistributedTracing(Configuration, builder => builder.UseZipkinWithTraceOptions(services));

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
