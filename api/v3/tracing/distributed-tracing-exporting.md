# Exporting Distributed Traces

Steeltoe is able to automatically configure several exporters provided by the [OpenTelemetry](https://opentelemetry.io) project, including [Zipkin](https://github.com/open-telemetry/opentelemetry-dotnet/tree/main/src/OpenTelemetry.Exporter.Zipkin), [Jaeger](https://github.com/open-telemetry/opentelemetry-dotnet/tree/main/src/OpenTelemetry.Exporter.Jaeger) and [OpenTelemetryProtocol](https://github.com/open-telemetry/opentelemetry-dotnet/tree/main/src/OpenTelemetry.Exporter.OpenTelemetryProtocol), if a NuGet reference to the desired exporter is included. The `Steeltoe.Management.ExporterBase` and  `Steeltoe.Management.ExporterCore` packages are no longer required.

## Common Settings

As of version 3.1.0, the exporter settings class has been merged with [tracing settings](./index.md#configure-settings). This table includes settings that are only relevant to exporting traces:

| Key | Description | Default |
| --- | --- | --- |
| `ExporterEndpoint` | Defines an endpoint traces should be sent to. | not set |
| `MaxPayloadSizeInBytes` | Maximum payload size to export, in bytes. | 4096 |
| `UseShortTraceIds` | Whether to truncate the IDs to 8 bytes instead of 16. Used for backwards compatibility. | `false` |

**Note**: **Each setting above must be prefixed with `Management:Tracing`**

## Zipkin Server

Zipkin is a popular distributed tracing system that has been around for several years. It is composed of client libraries for instrumenting application code and a backend server for collecting and viewing the collected data. For more information on Zipkin, we encourage you to review the [documentation](https://zipkin.io/). See the [Quickstart](https://zipkin.io/pages/quickstart) guide for details on how to set up a server.

### Configure Zipkin Options

In addition to the [common exporter settings](#common-settings), you may configure ExportProcessorType and BatchExportProcessorOptions in code:

```csharp
services.PostConfigure<ZipkinExporterOptions>(options =>
{
    options.ExportProcessorType = ExportProcessorType.Batch;
    options.BatchExportProcessorOptions.ExporterTimeoutMilliseconds = 1000;
});
```

### Use Zipkin Exporter

Steeltoe will discover and automatically configure the Zipkin exporter when a standard NuGet reference is used:

```xml
<PackageReference Include="OpenTelemetry.Exporter.Zipkin" Version="1.1.0-rc1" />
```

## Jaeger Server

Jaeger is another popular distributed tracing system that has been around for several years. For more information on Jaeger, we encourage you to visit the [Jaeger site](https://www.jaegertracing.io/). See the [Getting Started](https://www.jaegertracing.io/docs/1.24/getting-started/) guide for details on how to quickly set up a server.

### Configure Jaeger Options

In addition to the [common exporter settings](#common-settings), you may configure ExportProcessorType and BatchExportProcessorOptions in code:

```csharp
services.PostConfigure<JaegerExporterOptions>(options =>
{
    options.ExportProcessorType = ExportProcessorType.Batch;
    options.BatchExportProcessorOptions.ExporterTimeoutMilliseconds = 1000;
});
```

### Use Jaeger Exporter

Steeltoe will discover and automatically configure the Jaeger exporter when a standard NuGet reference is used:

```xml
<PackageReference Include="OpenTelemetry.Exporter.Jaeger" Version="1.1.0-rc1" />
```

## Open Telemetry Protocol

The OTLP (OpenTelemetry Protocol) is a vendor-agnostic way to export traces. Steeltoe will configure [this exporter](https://github.com/open-telemetry/opentelemetry-dotnet/tree/main/src/OpenTelemetry.Exporter.OpenTelemetryProtocol) to communicate to an [OpenTelemetry Collector](https://opentelemetry.io/docs/collector/) through a gRPC protocol.

### Configure Open Telemetry Protocol Options

In addition to the [common exporter settings](#common-settings), you may configure Headers, ExportProcessorType and BatchExportProcessorOptions in code:

```csharp
services.PostConfigure<OtlpExporterOptions>(options =>
{
    options.ExportProcessorType = ExportProcessorType.Batch;
    options.BatchExportProcessorOptions.ExporterTimeoutMilliseconds = 1000;
});
```

### Use Open Telemetry Protocol Exporter

Steeltoe will discover and automatically configure the Open Telemetry Protocol exporter when a standard NuGet reference is used:

```xml
<PackageReference Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.1.0-rc1" />
```
