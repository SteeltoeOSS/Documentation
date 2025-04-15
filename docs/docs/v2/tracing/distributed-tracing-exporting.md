# Distributed Tracing with Zipkin

By default when you enable distributed tracing in your application you do *NOT* automatically enable exporting of those traces to a backend system. Currently, Steeltoe supports exporting traces to a backend Zipkin server.

To enable exporting you will need to do the following:

* Add appropriate NuGet package reference to your project.
* Configure the settings the exporter will use during export.
* Add and Use the exporter service in the application

### Add NuGet Reference

All of the exporters can be found in the `Steeltoe.Management.ExporterBase` and in `Steeltoe.Management.OpenCensus`.

To use an exporter in a ASP.NET Core application, then add the following `PackageReference` to your `.csproj` file.

```xml
<ItemGroup>
...
    <PackageReference Include="Steeltoe.Management.ExporterCore" Version="2.5.2" />
...
</ItemGroup>
```

or

```powershell
PM>Install-Package  Steeltoe.Management.ExporterCore -Version 2.5.2
```

### Zipkin Server

Zipkin is a popular distributed tracing system which has been around for several years. It is composed of client libraries for instrumenting application code and a backend server for collecting and viewing the collected data. For more information on Zipkin we encourage you to review the [documentation](https://zipkin.io/).  Check out the [Quickstart](https://zipkin.io/pages/quickstart) guide for details on how to set up a server.

Steeltoe provides an exporter that will send all captured traces to a Zipkin server.  The following sections outline how to enable the exporter in your application.

#### Configure Settings

The following table describes the settings that you can apply to the exporter:

|Key|Description|Default|
|---|---|---|
|endpoint|the uri used to POST traces|`http://localhost:9411/api/v2/spans`|
|validateCertificates|validate SSL certificates received from exporter service|true|
|timeoutSeconds|timeout used in seconds for each POST request|3|
|serviceName|app name used in log messages|null|
|useShortTraceIds|truncate the ids to 8 bytes instead of 16, use for backwards compatibility with Spring Sleuth, PCF Metrics, etc.|true|

**Note**: **Each setting above must be prefixed with `management:tracing:exporter:zipkin`**.

#### Add and Use Zipkin Exporter

There are two steps needed to use the Zipkin exporter:

1. Add the exporter to the service container. Use the `AddZipkinExporter()` extension method from `ZipkinExporterServiceCollectionExtensions`.
1. Start the exporter background thread. Use the `UseTracingExporter()` extension method from `ZipkinExporterApplicationBuilderExtensions`.

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
        services.AddDistributedTracing(Configuration);

        // Export traces to Zipkin
        services.AddZipkinExporter(Configuration);
        ...
    }
    public void Configure(IApplicationBuilder app)
    {
        app.UseStaticFiles();
        app.UseMvc();

        // Start up trace exporter
        app.UseTracingExporter();
    }
}
```
