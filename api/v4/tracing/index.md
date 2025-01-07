# Distributed Tracing

On the subject of distributed tracing for .NET applications, previous versions of Steeltoe offered either an implementation of OpenCensus or shortcuts for enabling distributed tracing with [OpenTelemetry](https://opentelemetry.io/).
As of this writing, the Steeltoe team believe that OpenTelemetry has evolved to the point that a major Steeltoe component is no longer necessary, and we recommend using OpenTelemetry directly.

Steeltoe continues to directly offer an option for [log correlation](#log-correlation). This page also provides direction for developers looking to achieve the same outcomes Steeltoe has previously provided more directly.

## Introduction

As the name implies, distributed tracing is a way of tracing requests through distributed systems.
Distributed tracing is typically accomplished by instrumenting components of the system to recognize and pass along metadata that is specific to a particular action or user request, and using another backend system to reconstruct the flow of the request through that metadata.

In the parlance of distributed tracing, a "span" is the basic unit of work. For example, sending an RPC is a new span, as is sending a response to an RPC.
Spans are identified by a unique ID for the span and by another ID for the "trace", which the span is part of.
Spans also have other data like descriptions, key-value annotations, the ID of the span that caused them, and process IDs.
Spans are started and stopped, and they keep track of their timing information. Once you create a span, you must stop it at some point in the future.
A set of spans form a tree-like structure called a "trace". For example, a trace might be formed by a POST request that adds an item to a shopping cart.

## Log correlation

Log correlation is all about taking log entries from disparate systems and bringing them together using some matching criteria (such as a distributed trace ID).
The process can be easier when important pieces of data are logged in the same format across different systems (such as .NET and Java apps communicating with each other).

Steeltoe provides the class `TracingLogProcessor`, which is an `IDynamicMessageProcessor` for correlating logs. The processor is built for use with a [Steeltoe Dynamic Logging provider](../logging/index.md).
The result is log entries that use the same trace format popularized by [Spring Cloud Sleuth](https://cloud.spring.io/spring-cloud-sleuth/reference/html/#log-correlation),
that include `[<ApplicationName>,<TraceId>,<SpanId>,<ParentSpanId>,<IsAllDataRequested>]`.

Consider this pair of log entries from the [Steeltoe Management sample applications](https://github.com/SteeltoeOSS/Samples/blob/latest/Management/src/):

```text
info: System.Net.Http.HttpClient.ActuatorApiClient.LogicalHandler[100]
       [ActuatorWeb,44ed2fe24a051bda2d1a56815448e9fb,8d51b985e3f0fd81,0000000000000000,true] Start processing HTTP request GET http://localhost:5140/weatherForecast?fromDate=2024-12-19&days=1

dbug: Microsoft.EntityFrameworkCore.Database.Command[20104]
       [ActuatorApi,44ed2fe24a051bda2d1a56815448e9fb,c32846ff227bed40,f315823f4c554816,true] Created DbCommand for 'ExecuteReader' (1ms).
```

Log correlation is easiest with a tool such as [Splunk](https://www.splunk.com/en_us/solutions/isolate-cloud-native-problems.html), [SumoLogic](https://www.sumologic.com/lp/log-analytics/) or [DataDog](https://www.datadoghq.com/dg/enterprise/log-management-analytics-security) (this is not an endorsement of any tool, only a pointer to some popular options).

### Using TracingLogProcessor

To use the processor, first add a reference to the `Steeltoe.Management.Tracing` NuGet package.

The only remaining step is to register the processor:

```csharp
using Steeltoe.Management.Tracing;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddTracingLogProcessor();
```

> [!NOTE]
> This extension method also ensures that implementations of `IApplicationInstanceInfo` and `IDynamicLoggerProvider` have been registered.
> If you wish to customize either of those or use non-default implementations, do so before calling `AddTracingLogProcessor`.

## OpenTelemetry

To use OpenTelemetry, you _can_ start by adding a reference to the `OpenTelemetry.Extensions.Hosting` NuGet package.
This package provides access to `OpenTelemetryBuilder`, which is the main entrypoint to OpenTelemetry, but depending on which packages you add later you can probably skip this step.
The rest of this section contains information on pieces of OpenTelemetry that could be configured by previous versions of Steeltoe.

### Sampler configuration

OpenTelemetry Provides the `Sampler` abstraction for configuring when traces should be recorded.
The simplest options are `AlwaysOnSampler` and `AlwaysOffSampler`, with their names describing exactly which traces will be recorded.

To replace the Steeltoe configuration for using these samplers, set the environment variable `OTEL_TRACES_SAMPLER` to `always_on` or `always_off`.

> [!TIP]
> OpenTelemetry is generally built to follow the [options pattern](https://learn.microsoft.com/dotnet/core/extensions/options).
> There are more ways to configure options than what is demonstrated on this page, these are only examples of the potential.

### Set Application Name

In order to use the Steeltoe name for your application with OpenTelemetry, call `SetResourceBuilder` and pass in a value from the registered `IApplicationInstanceInfo`:

```csharp
services.ConfigureOpenTelemetryTracerProvider((serviceProvider, tracerProviderBuilder) =>
{
    var appInfo = serviceProvider.GetRequiredService<IApplicationInstanceInfo>();
    tracerProviderBuilder.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(appInfo.ApplicationName!));
});
```

## Instrumenting applications

In order to maximize the benefit of collecting distributed traces, you'll want participation from the core components and frameworks of your application and some 3rd party components.
Some packages in the .NET ecosystem automatically support OpenTelemetry, others can be supported by the [collection of instrumentation libraries](https://opentelemetry.io/ecosystem/registry/?language=dotnet&component=instrumentation).
Steeltoe previously configured the instrumentation libraries for [HttpClient](#httpclient) and [ASP.NET Core](#aspnet-core).

### ASP.NET Core

To instrument requests coming into the application through ASP.NET Core, start by adding a reference to the `OpenTelemetry.Instrumentation.AspNetCore` NuGet package.

Next, add the instrumentation to the `TracerProviderBuilder`:

```csharp
services.AddOpenTelemetry().WithTracing(tracerProviderBuilder => tracerProviderBuilder.AddAspNetCoreInstrumentation());
```

In order to replicate the Steeltoe setting `IngressIgnorePattern` (a Regex pattern describing which incoming requests to ignore), configure the `AspNetCoreTraceInstrumentationOptions`:

```csharp
private const string DefaultIngressIgnorePattern = "/actuator/.*|/cloudfoundryapplication/.*|.*\\.png|.*\\.css|.*\\.js|.*\\.html|/favicon.ico|/hystrix.stream|.*\\.gif";
private static readonly Regex IngressPathMatcher = new(DefaultIngressIgnorePattern, RegexOptions.Compiled | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

services.PostConfigure<AspNetCoreTraceInstrumentationOptions>(aspNetCoreTraceInstrumentationOptions =>
{
    aspNetCoreTraceInstrumentationOptions.Filter += httpContext => !IngressPathMatcher.IsMatch(httpContext.Request.Path);
});
```

As an alternative to Regex, you can list out the paths to ignore in the Filter property (`Filter` is a `Func<HttpContext, bool>?`):

```csharp
services.PostConfigure<AspNetCoreTraceInstrumentationOptions>(aspNetCoreTraceInstrumentationOptions =>
{
    aspNetCoreTraceInstrumentationOptions.Filter += httpContext =>
        !httpContext.Request.Path.StartsWithSegments("/actuator", StringComparison.OrdinalIgnoreCase) &&
        !httpContext.Request.Path.StartsWithSegments("/cloudfoundryapplication", StringComparison.OrdinalIgnoreCase);
});
```

> [!TIP]
> By default, the ASP.NET Core instrumentation does not filter out any requests.
> The latter approach above may quickly prove unwieldy if there are many patterns to ignore, such as when listing many file types (notably missing from the example).

[Learn more about ASP.NET Core instrumentation for OpenTelemetry](https://github.com/open-telemetry/opentelemetry-dotnet-contrib/blob/main/src/OpenTelemetry.Instrumentation.AspNetCore)

### HttpClient

To instrument requests leaving the application through `HttpClient`, start by adding a reference to the `OpenTelemetry.Instrumentation.Http` NuGet package.

Next, add the instrumentation to the `TracerProviderBuilder`:

```csharp
services.AddOpenTelemetry().WithTracing(tracerProviderBuilder => tracerProviderBuilder.AddHttpClientInstrumentation());
```

In order to replicate the Steeltoe setting `EgressIgnorePattern` (a Regex pattern describing which outgoing HTTP requests to ignore), configure the `HttpClientTraceInstrumentationOptions`:

```csharp
private const string DefaultEgressIgnorePattern = "/api/v2/spans|/v2/apps/.*/permissions";
private static readonly Regex EgressPathMatcher = new(DefaultEgressIgnorePattern, RegexOptions.Compiled | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

services.PostConfigure<HttpClientTraceInstrumentationOptions>(httpClientTraceInstrumentationOptions =>
{
    httpClientTraceInstrumentationOptions.FilterHttpRequestMessage += httpRequestMessage => !EgressPathMatcher.IsMatch(httpRequestMessage.RequestUri?.PathAndQuery ?? string.Empty);
});
```

[Learn more about ASP.NET Core instrumentation for OpenTelemetry](https://github.com/open-telemetry/opentelemetry-dotnet-contrib/blob/main/src/OpenTelemetry.Instrumentation.AspNetCore)

## Propagating Trace Context

By default, OpenTelemetry uses the [W3C trace context](https://github.com/w3c/trace-context) for propagating traces.
Some systems like Cloud Foundry may still be configured for the Zipkin standard of [B3 propagation](https://github.com/openzipkin/b3-propagation).

In order to use B3 propagation, add a reference to the `OpenTelemetry.Extensions.Propagators` NuGet package.

Next, let the compiler know you want to use the `OpenTelemetry.Context.Propagation` namespace, but that the `B3Propagator` should come from the package reference you just added (rather than the deprecated class found in `OpenTelemetry.Context.Propagation`):

```csharp
using OpenTelemetry.Context.Propagation;
using B3Propagator = OpenTelemetry.Extensions.Propagators.B3Propagator;
```

Finally, register a `CompositeTextMapPropagator` that includes the `B3Propagator` and `BaggagePropagator`:

```csharp
services.ConfigureOpenTelemetryTracerProvider((serviceProvider, tracerProviderBuilder) =>
{
    List<TextMapPropagator> propagators =
    [
        new B3Propagator(),
        new BaggagePropagator()
    ];

    Sdk.SetDefaultTextMapPropagator(new CompositeTextMapPropagator(propagators));
});
```

By default, `B3Propagator` uses [multiple headers](https://github.com/openzipkin/b3-propagation?tab=readme-ov-file#multiple-headers). In order to use a [single B3 header](https://github.com/openzipkin/b3-propagation?tab=readme-ov-file#single-header), change the code above to pass `true` to the `B3Propagator` constructor.

## Exporting Distributed Traces

Previous versions of Steeltoe could automatically configure several different trace exporters, including [Zipkin](https://github.com/open-telemetry/opentelemetry-dotnet/tree/main/src/OpenTelemetry.Exporter.Zipkin), [OpenTelemetryProtocol (OTLP)](https://github.com/open-telemetry/opentelemetry-dotnet/tree/main/src/OpenTelemetry.Exporter.OpenTelemetryProtocol) and Jaeger.
The Jaeger exporter has been deprecated in favor of OTLP, which was only minimally configured by Steeltoe and is better described by [the official OTLP exporter documentation](https://opentelemetry.io/docs/languages/net/exporters/#otlp).

### Zipkin Server

To use the Zipkin Exporter, add a reference to the `OpenTelemetry.Exporter.Zipkin` NuGet package.

Next, use the extension `AddZipkinExporter`:

```csharp
services.AddOpenTelemetry().WithTracing(tracing => tracerProviderBuilder.AddZipkinExporter());
```

The Zipkin options class `ZipkinExporterOptions` works the same as Steeltoe settings with the same names in previous releases:

```csharp
services.PostConfigure<ZipkinExporterOptions>(options =>
{
    options.Endpoint = "http://localhost:9411";
    options.MaxPayloadSizeInBytes = 4096;
    options.UseShortTraceIds = true;
});
```

> [!TIP]
> The Zipkin endpoint can also be set with the environment variable "OTEL_EXPORTER_ZIPKIN_ENDPOINT".
