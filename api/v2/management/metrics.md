# Metrics

The Steeltoe metrics endpoint configures application metrics collection using the open source [OpenCensus](https://opencensus.io/) project. It automatically configures built-in instrumentation of various aspects of the application and exposes the collected metrics via the endpoint.

The following instrumentation is automatically configured:

* CLR Metrics
  * Heap memory, Garbage collections, Thread utilization
* HTTP Client Metrics
  * Request timings & counts
* HTTP Server Metrics
  * Request timings & counts

All of the above metrics are tagged with values specific to the requests being processed; thereby giving multi-dimensional views of the collected metrics.

## Configure Settings

The following table describes the settings that you can apply to the endpoint:

|Key|Description|Default|
|---|---|---|
|id|The ID of the metrics endpoint|`metrics`|
|enabled|Whether to enable the metrics management endpoint|true|
|ingressIgnorePattern|Regex pattern describing what incoming requests to ignore|See `MetricsOptions`|
|egressIgnorePattern|Regex pattern describing what outgoing requests to ignore|See `MetricsOptions`|

**Note**: **Each setting above must be prefixed with `management:endpoints:metrics`**.

## Enable HTTP Access

The default path to the Metrics endpoint is computed by combining the global `path` prefix setting together with the `id` setting from above. The default path is  `/actuator/metrics`.

The coding steps you take to enable HTTP access to the Metrics endpoint differs depending on the type of .NET application your are developing.  The sections which follow describe the steps needed for each of the supported application types.

### ASP.NET Core App

To add the Metrics actuator to the service container, use the `AddMetricsActuator()` extension method from `EndpointServiceCollectionExtensions`.

To add the Metrics actuator middleware to the ASP.NET Core pipeline, use the `UseMetricsActuator()` extension method from `EndpointApplicationBuilderExtensions`.

### ASP.NET 4.x App

To add the Metrics actuator endpoint, use the `UseMetricsActuator()` method from `ActuatorConfigurator`.

### ASP.NET OWIN App

To add the Metrics actuator middleware to the ASP.NET OWIN pipeline, use the `UseMetricsActuator()` extension method from `MetricsEndpointAppBuilderExtensions`.

## Exporting

By default when you enable metrics collection in your application you do *NOT* automatically enable exporting of those metrics to a backend system.

The coding steps you take to enable metrics exporting differs depending on what backend system you are targeting and the type of .NET application your are developing.  The sections which follow describe the steps needed for each of the backend systems and supported application types.

## Add NuGet References

To use the metrics exporters, you need to add a reference to the appropriate Steeltoe NuGet based on the type of the application you are building and what Dependency Injector you have chosen, if any.

The following table describes the available packages:

|App Type|Package|Description|
|---|---|---|
|All|`Steeltoe.Management.ExporterBase`|Base functionality, no dependency injection|
|ASP.NET Core|`Steeltoe.Management.ExporterCore`|Includes `ExporterBase`, adds ASP.NET Core DI|

To add this type of NuGet to your project, add a `PackageReference` resembling the following:

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

## Cloud Foundry Forwarder

The [Metrics Forwarder for TAS](https://docs.pivotal.io/metrics-forwarder/) is a service that allows apps to emit metrics to the [Loggregator](https://docs.pivotal.io/pivotalcf/2-2/loggregator/architecture.html) system and consume those metrics from the [Loggregator Firehose](https://docs.pivotal.io/pivotalcf/2-2/loggregator/architecture.html#firehose).

You can interact with the service through the Cloud Foundry Command Line Interface (cf CLI), [TAS Apps Manager](https://docs.pivotal.io/pivotalcf/2-0/console/manage-apps.html), and an [HTTP API](https://docs.pivotal.io/metrics-forwarder/api/). See the [documentation](https://docs.pivotal.io/metrics-forwarder/using.html) for details on how to use the service in your application.

[Metrics Forwarder for TAS](https://docs.pivotal.io/metrics-forwarder/)  enables users to do the following:

* Configure apps to emit custom metrics to [Loggregator](https://docs.pivotal.io/pivotalcf/2-2/loggregator/architecture.html) system.
* Read custom metrics from the [Loggregator Firehose](https://docs.pivotal.io/pivotalcf/2-2/loggregator/architecture.html#firehose) using a Firehose consumer of their choice, including [community](https://github.com/cloudfoundry/loggregator-release/blob/develop/docs/community-nozzles.md) and third-party nozzles.

There are many third-party products you can choose from, including [PCF Metrics](https://docs.pivotal.io/pcf-metrics/1-4/).

## Configure Settings

The following table describes the settings that you can apply to the exporter:

|Key|Description|Default|
|---|---|---|
|endpoint|the uri used to POST metrics|null|
|accessToken|the authentication token needed to access the endpoint|null|
|rateMilli|delay in milliseconds between metrics POSTs|60000|
|validateCertificates|validate SSL certificates received from exporter service|true|
|timeoutSeconds|timeout used in seconds for each POST request|3|
|applicationId|cloud foundry application ID the POST applies to|null|
|instanceId|cloud foundry application instance ID the POST applies to|null|
|instanceIndex|cloud foundry application instance index the POST applies to|null|
|micrometerMetricWriter|emit metrics using Spring Boot 2.x format |false|

**Note**: **The `endpoint`, `accessToken`,`applicationId`, `instanceId` and `instanceIndex` settings above will be automatically picked up from the Metrics Forwarder service binding found for your application.**

### ASP.NET Core App

There are three steps needed to use the Metrics Forwarder for TAS service:

1. Create and bind a forwarder service to your application. Follow the steps in the Metrics Forwarder for PCF [documentation](https://docs.pivotal.io/metrics-forwarder/using.html).
1. Add the exporter to the service container. Use the `AddMetricsForwarderExporter()` extension method from `EndpointServiceCollectionExtensions`.
1. Start the exporter background thread. Use the `UseMetricsExporter()` extension method from `EndpointApplicationBuilderExtensions`.

```csharp
public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }
    public void ConfigureServices(IServiceCollection services)
    {
        // Add Metrics collection
        services.AddMetricsActuator(Configuration);

        // Export metrics to Cloud Foundry forwarder
        services.AddMetricsForwarderExporter(Configuration);
        ...
    }
    public void Configure(IApplicationBuilder app)
    {
        app.UseStaticFiles();

        // Expose Metrics endpoint
        app.UseMetricsActuator();

        app.UseMvc();

        // Start up metrics exporter
        app.UseMetricsExporter();
    }
}
```

### ASP.NET 4.x App

There are two steps needed to use the Metrics Forwarder for TAS service:

1. Create and bind a forwarder service to your application. Follow the steps in the Metrics Forwarder for PCF [documentation](https://docs.pivotal.io/metrics-forwarder/using.html).
1. Configure and start the exporter background thread.

```csharp
public class ManagementConfig
{
    public static IMetricsExporter MetricsExporter { get; set; }

    public static void UseCloudFoundryMetricsExporter(IConfiguration configuration, ILoggerFactory loggerFactory = null)
    {
        var options = new CloudFoundryForwarderOptions(configuration);
        MetricsExporter = new CloudFoundryForwarderExporter(
            options,
            OpenCensusStats.Instance,
            loggerFactory != null ? loggerFactory.CreateLogger<CloudFoundryForwarderExporter>() : null);
    }
    public static void Start()
    {
        DiagnosticsManager.Instance.Start();
        if (MetricsExporter != null)
        {
            MetricsExporter.Start();
        }
    }
    public static void Stop()
    {
        DiagnosticsManager.Instance.Stop();
        if (MetricsExporter != null)
        {
            MetricsExporter.Stop();
        }
    }
}
```

