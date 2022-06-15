# Distributed Tracing

Steeltoe distributed tracing implements a solution for .NET applications using the open source [OpenTelemetry](https://opentelemetry.io/) project. For most users, implementing and using distributed tracing should be invisible, and many of the interactions with external systems should be instrumented automatically. You can capture trace data in logs or by sending it to a remote collector service.

A "span" is the basic unit of work. For example, sending an RPC is a new span, as is sending a response to an RPC. Spans are identified by a unique 64-bit ID for the span and by another 64-bit ID for the trace of which the span is a part. Spans also have other data, such as descriptions, key-value annotations, the ID of the span that caused them, and process IDs (normally an IP address). Spans are started and stopped, and they keep track of their timing information. Once you create a span, you must stop it at some point in the future. A set of spans form a tree-like structure called a "trace". For example, if you run a distributed big-data store, a trace might be formed by a PUT request.

Steeltoe distributed tracing:

* Adds trace and span IDs to the application log messages, so you can extract all the logs from a given trace or span in a log aggregator.
* Using the [OpenTelemetry](https://opentelemetry.io/) APIs, provides an abstraction over common distributed tracing data models: traces, spans (forming a DAG), annotations, and key-value annotations.
* Automatically configures instrumentations of common ingress and egress points from .NET applications (such as ASP.NET Core and HTTP client).
* Automatically configures trace exporters (when a relevant NuGet reference is included).

## Usage

You should understand how the .NET [configuration service](https://docs.microsoft.com/aspnet/core/fundamentals/configuration) works before starting to use the management endpoints. You need at least a basic understanding of the `ConfigurationBuilder` and how to add providers to the builder to configure the endpoints.

Steeltoe distributed tracing automatically applies instrumentation at key ingress and egress points in your ASP.NET Core application so that you are able to get meaningful traces without having to do any instrumentation yourself. These points include:

* HTTP Server
  * Request Start & Finish
  * Unhandled and Handled exceptions
* HTTP Client
  * Outgoing Request Start & Finish
  * Unhandled and Handled exceptions

### Add NuGet References

To use the distributed tracing exporters, you need to add a reference to the appropriate Steeltoe NuGet based on the type of the application you are building and what dependency injector you have chosen (if any).

The following table describes the available packages:

| Package | Description | .NET Target |
| --- | --- | --- |
| `Steeltoe.Management.TracingBase` | Base functionality. | .NET Standard 2.0 |
| `Steeltoe.Management.TracingCore` | Includes `TracingBase`, adds ASP.NET Core instrumentation. | ASP.NET Core 3.1+ |

To add this type of NuGet to your project, add a `PackageReference` resembling the following:

```xml
<ItemGroup>
...
    <PackageReference Include="Steeltoe.Management.TracingCore" Version="3.2.0"/>
...
</ItemGroup>
```

Alternative, you can add it with PowerShell:

```powershell
PM>Install-Package Steeltoe.Management.TracingCore
```

### Configure Settings

You can configure distributed tracing by using the normal .NET [Configuration service](https://docs.microsoft.com/aspnet/core/fundamentals/configuration).

All settings should be placed under the prefix with a key of `Management:Tracing:`.
The following table describes the available settings:

| Key | Description | Default |
| --- | --- | --- |
| `Name` | The name of the application. | `Spring:Application:Name`, Cloud Foundry name, or `Unknown` |
| `IngressIgnorePattern` | Regex pattern describing what incoming requests to ignore. | See `TracingOptions` |
| `EgressIgnorePattern` | Regex pattern describing what outgoing requests to ignore. | See `TracingOptions` |
| `MaxPayloadSizeInBytes` | Maximum payload size to export, in bytes. | 4096 |
| `AlwaysSample` | Whether to enable the OpenTelemetry `AlwaysOnSampler`. | OpenTelemetry `Sampler` |
| `NeverSample` | Whether to enable the OpenTelemetry `AlwaysOffSampler`. | OpenTelemetry `Sampler` |
| `UseShortTraceIds` | Whether to truncate the IDs to 8 bytes instead of 16. Use it for backwards compatibility with Spring Sleuth, PCF Metrics, and others. | `false` |
| `PropagationType` | Propagation format that should be used. `B3` and `W3C` are supported. | `B3` |
| `SingleB3Header` | Defines whether B3 information is sent in one or multiple headers. | `true` |
| `EnableGrpcAspNetCoreSupport` | Defines if GRPC requests should participate in tracing. | `true` |
| `ExporterEndpoint` | Defines an endpoint traces should be sent to. | not set |

### Enabling Log Correlation

To use distributed tracing together with log correlation, you can use a [Steeltoe Dynamic Logging provider](../logging/index.md) in your application.

Once that is done, whenever your application issues any log statements, the Steeltoe logger adds additional trace information to each log message if there is an active trace context. The format of that information is of the form `[app name, trace id, span id, trace flags]` (for example, `[service1,2485ec27856c56f4,2485ec27856c56f4,true]`).

### Propagating Trace Context

When working with distributed tracing systems, you will find that a trace context (for example, trace state information) must get propagated to all child processes to ensure that child spans originating from a root trace get collected and correlated into a single trace in the end. The current trace and span IDs are just one piece of the required information that must get propagated.

Steeltoe makes this easy by automatically configuring some of the instrumentation packages provided by Open Telemetry.

* TracingBase configures [instrumentation on outbound requests](https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Instrumentation.Http/README.md)
* TracingCore builds on top of TracingBase, also configuring [instrumentation on inbound requests through ASP.NET Core and Grpc.AspNetCore](https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Instrumentation.AspNetCore/README.md)
* Additional instrumentation libraries can be added with the [`Action<TracerProviderBuilder>` parameter](#adding-to-tracerproviderbuilder)

 Steeltoe currently uses [Zipkin B3 Propagation](https://github.com/openzipkin/b3-propagation) by default, but can be configured to use [W3C trace context](https://www.w3.org/TR/trace-context/). As a result, you will find that Steeltoe tracing is interoperable with several other instrumentation libraries, such as [Spring Cloud Sleuth](https://spring.io/projects/spring-cloud-sleuth).

### Add Distributed Tracing

To enable distributed tracing, add the service to the container. To do so, use either `AddDistributedTracing()` or `AddDistributedTracingAspNetCore()` from `TracingServiceCollectionExtensions`:

```csharp
var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        // services.AddDistributedTracing();
        //     or
        services.AddDistributedTracingAspNetCore();
    })
```

`AddDistributedTracing()` is included in `Steeltoe.Management.TracingBase`, configures OpenTelemetry, `HttpClient` instrumentation and [exporters](./distributed-tracing-exporting.md).
`AddDistributedTracingAspNetCore()` is included in `Steeltoe.Management.TracingCore`, and calls `AddDistributedTracing()` with the addition of `ASP.NET Core` and `Grpc.AspNetCore` instrumentation.

### Code-based Instrumentation Configuration

Some of the options for HttpClient and ASP.NET Core instrumentation must be configured in code. These can be accessed using IOptions configuration methods like [`PostConfigure`](https://docs.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.optionsservicecollectionextensions.postconfigure):

```csharp
services.PostConfigure<AspNetCoreInstrumentationOptions>(options =>
{
    options.Enrich = (activity, eventName, rawObject) =>
    {
        if (eventName.Equals("OnStartActivity"))
        {
            if (rawObject is HttpRequest httpRequest)
            {
                activity.SetTag("requestProtocol", httpRequest.Protocol);
            }
        }
    };
});
```

### Adding to TracerProviderBuilder

There are additional instrumentation libraries for OpenTelemetry, and other settings you may wish to configure that Steeltoe does directly address. For these cases, an `Action<TracerProviderBuilder>` is available.

For example, if you wanted to add SQL Server instrumentation and a custom sampler, your code could look like this:

```csharp
var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
      services.AddDistributedTracingAspNetCore(trace =>
      {
          trace
              .SetSampler(new MyCustomSampler())
              .AddSqlClientInstrumentation();
      });
    })
```

## Next Steps

Once you've setup all the instrumentation, you'll want to [configure an exporter](./distributed-tracing-exporting.md)
