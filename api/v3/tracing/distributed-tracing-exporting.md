# Distributed Tracing with Zipkin

Steeltoe now uses the Zipkin Exporter provided by the [OpenTelemetry](opentelemetry.io) project. Previous versions required additional packages. The `Steeltoe.Management.ExporterBase` and  `Steeltoe.Management.ExporterCore` packages are no longer required.

## Zipkin Server

Zipkin is a popular distributed tracing system that has been around for several years. It is composed of client libraries for instrumenting application code and a backend server for collecting and viewing the collected data. For more information on Zipkin, we encourage you to review the [documentation](https://zipkin.io/). See the [Quickstart](https://zipkin.io/pages/quickstart) guide for details on how to set up a server.

### Configure Settings

The following table describes the settings that you can apply to the exporter:

|Key|Description|Default|
|---|---|---|
|`endpoint`|The URI used to POST traces.|`http://localhost:9411/api/v2/spans`|
|`validateCertificates`|Whether to validate SSL certificates received from exporter service.|`true`|
|`timeoutSeconds`|The timeout used in seconds for each POST request.|3|
|`serviceName`|The application name used in log messages.|`null`|
|`useShortTraceIds`|Whether to truncate the IDs to 8 bytes instead of 16. Use it for backwards compatibility with Spring Sleuth, PCF Metrics, and others.|`true`|

### Use Zipkin Exporter

The following example uses the Zipkin exporter:

```csharp
public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }
    public void ConfigureServices(IServiceCollection services)
    {
        // Add Distributed tracing
        services.AddDistributedTracing(Configuration, builder => builer.UseZipkinWithTraceOptions(services));

        ...
    }
    public void Configure(IApplicationBuilder app)
    {
        app.UseStaticFiles();
        app.UseMvc();
    }
}
```
