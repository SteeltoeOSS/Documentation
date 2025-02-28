# Distributed Tracing

This topic describes distributed tracing for .NET applications.

Previous versions of Steeltoe offered either an implementation of OpenCensus or shortcuts for enabling distributed tracing with [OpenTelemetry](https://opentelemetry.io/) to integrate with the Tanzu platform. Steeltoe 3 has a strong dependency on OpenTelemetry packages, but with the breaking changes in the latest versions of OpenTelemetry, Steeltoe couldn't adapt without resorting to reflection. While we do that in other places, reflection is slow, and instrumentation is performance-sensitive.

OpenTelemetry has evolved to the point that only a few lines of code are needed. So instead of providing a Steeltoe component, the guidance below is offered to accomplish the same. The benefit of this approach is greater flexibility: when OpenTelemetry changes, there's no need to wait for a new Steeltoe version before using it.

Steeltoe continues to directly offer an option for [log correlation](#log-correlation). This topic provides direction for developers looking to achieve the same outcomes Steeltoe has previously provided more directly.

## About distributed tracing

As the name implies, distributed tracing is a way of tracing requests through distributed systems.
Distributed tracing is typically accomplished by instrumenting components of the system to allow recognizing and passing along metadata that is specific to a particular action or user request, and using another backend system to reconstruct the flow of the request through that metadata.

In the parlance of distributed tracing, a "span" is the basic unit of work. For example, sending an HTTP request creates a new span, as does sending a response.
Each span is identified by a unique ID and contains the ID for the "trace" it is part of.
Spans also have other data like descriptions, key-value annotations, the ID of the span that initiated the execution flow, and process IDs.
Spans are started and stopped, and they keep track of their timing information. After you create a span, you must stop it at some point in the future.
A set of spans form a tree-like structure called a "trace." For example, a trace might be formed by a POST request that adds an item to a shopping cart, which results in calling several backend services.

## Log correlation

Log correlation refers to the process of taking log entries from disparate systems and bringing them together using some matching criteria (such as a distributed trace ID).
The process is easier when important pieces of data are logged in the same format across different systems (such as .NET and Java apps communicating with each other).

Steeltoe provides the class `TracingLogProcessor`, which is an `IDynamicMessageProcessor` for correlating logs. The processor is built for use with a [Steeltoe Dynamic Logging provider](../logging/index.md).
It enriches log entries with correlation data using the same trace format popularized by [Spring Cloud Sleuth](https://cloud.spring.io/spring-cloud-sleuth/reference/html/#log-correlation),
that include `[<ApplicationName>,<TraceId>,<SpanId>,<ParentSpanId>,<IsAllDataRequested>]`.

Consider this pair of log entries from the [Steeltoe Management sample applications](https://github.com/SteeltoeOSS/Samples/blob/main/Management/src/):

```text
info: System.Net.Http.HttpClient.ActuatorApiClient.LogicalHandler[100]
       [ActuatorWeb,44ed2fe24a051bda2d1a56815448e9fb,8d51b985e3f0fd81,0000000000000000,true] Start processing HTTP request GET http://localhost:5140/weatherForecast?fromDate=2024-12-19&days=1

dbug: Microsoft.EntityFrameworkCore.Database.Command[20104]
       [ActuatorApi,44ed2fe24a051bda2d1a56815448e9fb,c32846ff227bed40,f315823f4c554816,true] Created DbCommand for 'ExecuteReader' (1ms).
```

Log correlation is easiest with a tool such as [Splunk](https://www.splunk.com/en_us/solutions/isolate-cloud-native-problems.html), [SumoLogic](https://www.sumologic.com/lp/log-analytics/), or [DataDog](https://www.datadoghq.com/dg/enterprise/log-management-analytics-security). (This is not an endorsement of any tool, only a pointer to some popular options).

### Using TracingLogProcessor

To use the processor:

1. Add a reference to the `Steeltoe.Management.Tracing` NuGet package.

2. Register the processor:

    ```csharp
    using Steeltoe.Management.Tracing;

    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddTracingLogProcessor();
    ```

> [!NOTE]
> This extension method also ensures that implementations of `IApplicationInstanceInfo` and `IDynamicLoggerProvider` have been registered.
> If you wish to customize either of these or use non-default implementations, make these changes before calling `AddTracingLogProcessor`.

## OpenTelemetry

To use OpenTelemetry, start by adding a reference to the `OpenTelemetry.Extensions.Hosting` NuGet package.
Other package references will probably be necessary, but these depend on your specific application needs.
This package provides access to `OpenTelemetryBuilder`, which is the main entrypoint to OpenTelemetry.

### Add Open Telemetry Tracing

To add Open Telemetry Tracing, use the following:

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenTelemetry().WithTracing();
```

> [!NOTE]
> At this point, not much has changed for the application, but you will continue by making changes to add [instrumentation](#instrumenting-applications) and [exporting of traces](#exporting-distributed-traces) to this line.

### Sampler configuration

OpenTelemetry Provides the `Sampler` abstraction for configuring when traces should be recorded.
The simplest options are `AlwaysOnSampler` and `AlwaysOffSampler`, with their names describing exactly which traces will be recorded.

As a replacement for what Steeltoe used to provide for using these samplers, set the environment variable `OTEL_TRACES_SAMPLER` to:

* `always_on`
* `always_off`

> [!TIP]
> OpenTelemetry is generally built to follow the [options pattern](https://learn.microsoft.com/dotnet/core/extensions/options).
> There are more ways to configure options than demonstrated on this page; these are just examples to help you get started.

### Set Application Name

To use the Steeltoe name for your application with OpenTelemetry, call `SetResourceBuilder` and pass in a value from the registered `IApplicationInstanceInfo`:

```csharp
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Steeltoe.Common;

builder.Services.ConfigureOpenTelemetryTracerProvider((serviceProvider, tracerProviderBuilder) =>
{
    var appInfo = serviceProvider.GetRequiredService<IApplicationInstanceInfo>();
    tracerProviderBuilder.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(appInfo.ApplicationName!));
});
```

The above example assumes you are already using some other Steeltoe component that adds `IApplicationInstanceInfo` to the IoC container. If that is not the case, follow these steps to register the default implementation:

1. Add a NuGet package reference to `Steeltoe.Common`.
2. Call `AddApplicationInstanceInfo`.

    ```csharp
    using Steeltoe.Common.Extensions;

    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddApplicationInstanceInfo();
    ```

### Instrumenting applications

To maximize the benefit of collecting distributed traces, ideally, you will have participation from the core components and frameworks of your application and some third-party components.
Some packages in the .NET ecosystem automatically support OpenTelemetry; others can be supported by the [collection of instrumentation libraries](https://opentelemetry.io/ecosystem/registry/?language=dotnet&component=instrumentation).
Steeltoe previously configured the instrumentation libraries for [ASP.NET Core](#aspnet-core) and [HttpClient](#httpclient).

#### ASP.NET Core

To instrument requests coming into the application through ASP.NET Core:

1. Add a reference to the `OpenTelemetry.Instrumentation.AspNetCore` NuGet package.

1. Add the instrumentation to the `TracerProviderBuilder` by updating the existing call from the earlier example to:

    ```csharp
    using OpenTelemetry.Trace;

    builder.Services.AddOpenTelemetry().WithTracing(tracerProviderBuilder => tracerProviderBuilder.AddAspNetCoreInstrumentation());
    ```

1. To replicate the Steeltoe setting `IngressIgnorePattern` (a Regex pattern describing which incoming requests to ignore), configure the `AspNetCoreTraceInstrumentationOptions`:

    ```csharp
    using System.Text.RegularExpressions;
    using OpenTelemetry.Instrumentation.AspNetCore;

    builder.Services.Configure<AspNetCoreTraceInstrumentationOptions>(options =>
    {
        const string defaultIngressIgnorePattern = @"/actuator/.*|/cloudfoundryapplication/.*|.*\.png|.*\.css|.*\.js|.*\.html|/favicon.ico|.*\.gif";
        Regex ingressPathMatcher = new(defaultIngressIgnorePattern, RegexOptions.Compiled | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
        options.Filter += httpContext => !ingressPathMatcher.IsMatch(httpContext.Request.Path);
    });
    ```

Alternatively, rather than using a regular expression, you can list the paths to ignore in the Filter property (`Filter` is a `Func<HttpContext, bool>?`):

```csharp
using OpenTelemetry.Instrumentation.AspNetCore;

builder.Services.Configure<AspNetCoreTraceInstrumentationOptions>(options =>
{
    options.Filter += httpContext =>
        !httpContext.Request.Path.StartsWithSegments("/actuator", StringComparison.OrdinalIgnoreCase) &&
        !httpContext.Request.Path.StartsWithSegments("/cloudfoundryapplication", StringComparison.OrdinalIgnoreCase);
});
```

> [!TIP]
> By default, the ASP.NET Core instrumentation does not filter out any requests.
> The alternative approach described can quickly prove unwieldy if there are many patterns to ignore, such as when listing many file types.

To learn more about ASP.NET Core instrumentation for OpenTelemetry see the [OpenTelemetry documentation](https://github.com/open-telemetry/opentelemetry-dotnet-contrib/blob/main/src/OpenTelemetry.Instrumentation.AspNetCore).

#### HttpClient

To instrument requests leaving the application through `HttpClient`:

1. Add a reference to the `OpenTelemetry.Instrumentation.Http` NuGet package.

1. Add the instrumentation to the `TracerProviderBuilder` by updating the existing call from above to:

    ```csharp
    using OpenTelemetry.Trace;

    builder.Services.AddOpenTelemetry().WithTracing(tracerProviderBuilder => tracerProviderBuilder.AddHttpClientInstrumentation());
    ```

1. To replicate the Steeltoe setting `EgressIgnorePattern` (a Regex pattern describing which outgoing HTTP requests to ignore), configure the `HttpClientTraceInstrumentationOptions`:

    ```csharp
    using OpenTelemetry.Instrumentation.Http;
    using System.Text.RegularExpressions;

    builder.Services.Configure<HttpClientTraceInstrumentationOptions>(options =>
    {
        const string defaultEgressIgnorePattern = "/api/v2/spans|/v2/apps/.*/permissions";
        Regex egressPathMatcher = new(defaultEgressIgnorePattern, RegexOptions.Compiled | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
        options.FilterHttpRequestMessage += httpRequestMessage => !egressPathMatcher.IsMatch(httpRequestMessage.RequestUri?.PathAndQuery ?? string.Empty);
    });
    ```

To learn more about HttpClient instrumentation for OpenTelemetry see the [OpenTelemetry documentation](https://github.com/open-telemetry/opentelemetry-dotnet-contrib/blob/main/src/OpenTelemetry.Instrumentation.Http).

### Propagating Trace Context

By default, OpenTelemetry uses the [W3C trace context](https://github.com/w3c/trace-context) for propagating traces.
Some systems like Cloud Foundry may still be configured for the Zipkin standard of [B3 propagation](https://github.com/openzipkin/b3-propagation).

To use B3 propagation:

1. Add a reference to the `OpenTelemetry.Extensions.Propagators` NuGet package.

1. Let the compiler know that the `B3Propagator` should come from the package reference you just added (rather than the deprecated class found in `OpenTelemetry.Context.Propagation`):

    ```csharp
    using B3Propagator = OpenTelemetry.Extensions.Propagators.B3Propagator;
    ```

2. Register a `CompositeTextMapPropagator` that includes the `B3Propagator` and `BaggagePropagator`:

    ```csharp
    using OpenTelemetry;
    using OpenTelemetry.Context.Propagation;
    using OpenTelemetry.Trace;

    builder.Services.ConfigureOpenTelemetryTracerProvider((_, _) =>
    {
        List<TextMapPropagator> propagators =
        [
            new B3Propagator(),
            new BaggagePropagator()
        ];

        Sdk.SetDefaultTextMapPropagator(new CompositeTextMapPropagator(propagators));
    });
    ```

### Exporting Distributed Traces

Previous versions of Steeltoe could automatically configure several different trace exporters, including:

* [Zipkin](https://github.com/open-telemetry/opentelemetry-dotnet/tree/main/src/OpenTelemetry.Exporter.Zipkin)
* [OpenTelemetryProtocol (OTLP)](https://github.com/open-telemetry/opentelemetry-dotnet/tree/main/src/OpenTelemetry.Exporter.OpenTelemetryProtocol)
* Jaeger. The Jaeger exporter has been deprecated in favor of OTLP, which was only minimally configured by Steeltoe. For more information, see the [OTLP exporter documentation](https://opentelemetry.io/docs/languages/net/exporters/#otlp).

#### Zipkin Server

To use the Zipkin Exporter:

1. Add a reference to the `OpenTelemetry.Exporter.Zipkin` NuGet package.

1. Use the extension method `AddZipkinExporter` by updating the existing call from above to:

    ```csharp
    builder.Services.AddOpenTelemetry().WithTracing(tracerProviderBuilder => tracerProviderBuilder.AddZipkinExporter());
    ```

The Zipkin options class `ZipkinExporterOptions` works nearly the same as Steeltoe settings with the same names in previous releases:

```csharp
using OpenTelemetry.Exporter;

builder.Services.Configure<ZipkinExporterOptions>(options =>
{
    options.Endpoint = new Uri("http://localhost:9411");
    options.MaxPayloadSizeInBytes = 4096;
    options.UseShortTraceIds = true;
});
```

> [!NOTE]
> The Zipkin endpoint can also be set with the environment variable `OTEL_EXPORTER_ZIPKIN_ENDPOINT`.
