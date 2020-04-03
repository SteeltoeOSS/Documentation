### Metrics

The Steeltoe Metrics endpoint configures application metrics collection using the open source [OpenTelemetry](https://opentelemetry.io/) project. It automatically configures built-in instrumentation of various aspects of the application and exposes the collected metrics via the endpoint.

**Note**: **OpenTelemetry is under active development and subject to change and not suitable for production deployment.**.

The following instrumentation is automatically configured:

* CLR Metrics
  * Heap memory, Garbage collections, Thread utilization
* HTTP Client Metrics
  * Request timings & counts
* HTTP Server Metrics
  * Request timings & counts

All of the above metrics are tagged with values specific to the requests being processed; thereby giving multi-dimensional views of the collected metrics.

#### Configure Settings

The following table describes the settings that you can apply to the endpoint:

|Key|Description|Default|
|---|---|---|
|id|The ID of the metrics endpoint|`metrics`|
|enabled|Whether to enable the metrics management endpoint|true|
|ingressIgnorePattern|Regex pattern describing what incoming requests to ignore|See `MetricsOptions`|
|egressIgnorePattern|Regex pattern describing what outgoing requests to ignore|See `MetricsOptions`|

**Note**: **Each setting above must be prefixed with `management:endpoints:metrics`**.

#### Enable HTTP Access

The default path to the Metrics endpoint is computed by combining the global `path` prefix setting together with the `id` setting from above. The default path is `/metrics`.

The coding steps you take to enable HTTP access to the Metrics endpoint differs depending on the type of .NET application your are developing.  The sections which follow describe the steps needed for each of the supported application types.

##### ASP.NET Core App

To add the Metrics actuator to the service container, use the `AddMetricsActuator()` extension method from `EndpointServiceCollectionExtensions`.

To add the Metrics actuator middleware to the ASP.NET Core pipeline, use the `UseMetricsActuator()` extension method from `EndpointApplicationBuilderExtensions`.


#### Exporting

Prior versions of Steeltoe supported exporting metrics to The [Metrics Forwarder for Pivotal Cloud Foundry (PCF)](https://docs.pivotal.io/metrics-forwarder/) which is no longer supported. See [Prometheus](prometheus) to export metrics

##### Add NuGet References

To use the metrics actuator, you need to add a reference to the `Steeltoe.Management.EndpointCore` NuGet package.

To add this type of NuGet to your project, add a `PackageReference` resembling the following:

```xml
<ItemGroup>
...
    <PackageReference Include="Steeltoe.Management.EndpointCore" Version= "3.0.0-m2"/>
...
</ItemGroup>
```

or

```powershell
PM>Install-Package  Steeltoe.Management.EndpointCore -Version 3.0.0-m2
```
##### Cloud Foundry Forwarder

 The [Metrics Forwarder for Pivotal Cloud Foundry (PCF)](https://docs.pivotal.io/metrics-forwarder/) is not longer supported. To export metrics to PCF, see [Prometheus](prometheus). 
##### ASP.NET Core App

To use the Metrics actuator endpoint do the following.

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

        ...
    }
    public void Configure(IApplicationBuilder app)
    {
        app.UseStaticFiles();

        // Expose Metrics endpoint
        app.UseMetricsActuator();

        app.UseMvc();

    }
}
```


