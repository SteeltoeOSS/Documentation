# Metrics

The Steeltoe metrics endpoint configures built-in instrumentation of various aspects of the application and exposes them in Spring Boot Metrics format.

The following instrumentation is automatically configured:

* CLR Metrics
  * Heap memory, Garbage collections, Thread utilization
* HTTP Client Metrics
  * Request timings & counts
* HTTP Server Metrics
  * Request timings & counts

All of these metrics are tagged with values specific to the requests being processed, thereby giving multi-dimensional views of the collected metrics.

## Configure Settings

The following table describes the settings that you can apply to the endpoint:

| Key | Description | Default |
| --- | --- | --- |
| `Id` | The ID of the metrics endpoint. | `metrics` |
| `Enabled` | Whether to enable the metrics management endpoint. | `true` |
| `IngressIgnorePattern` | Regex pattern describing what incoming requests to ignore. | See `MetricsOptions` |
| `EgressIgnorePattern` | Regex pattern describing what outgoing requests to ignore. | See `MetricsOptions` |

>NOTE: Each setting above must be prefixed with `Management:Endpoints:Metrics`.

To configure Observers, see [Metric Observers](/docs/3/management/metric-observers)

## Enable HTTP Access

The default path to the metrics endpoint is computed by combining the global `Path` prefix setting together with the `Id` setting described in the preceding section. The default path is <[Context-Path](./hypermedia#base-context-path)>`/metrics`.

See the [HTTP Access](/docs/3/management/using-endpoints#http-access) section to see the overall steps required to enable HTTP access to endpoints in an ASP.NET Core application.

To add the metrics actuator to the service container, use the `AddMetricsActuator()` extension method from `EndpointServiceCollectionExtensions`.

To add the metrics actuator middleware to the ASP.NET Core pipeline, use the `UseMetricsActuator()` extension method from `EndpointApplicationBuilderExtensions`.

## Exporting

See [Prometheus](/docs/3/management/prometheus) to export metrics.

## Add NuGet References

To use the metrics actuator, you need to add a reference to the `Steeltoe.Management.EndpointCore` NuGet package.

To add this type of NuGet to your project, add a `PackageReference` resembling the following:

```xml
<ItemGroup>
...
    <PackageReference Include="Steeltoe.Management.EndpointCore" Version= "3.0.0"/>
...
</ItemGroup>
```

Alternatively, you can add the package through PowerShell:

```powershell
PM>Install-Package  Steeltoe.Management.EndpointCore -Version 3.0.0-
```

## Cloud Foundry Forwarder

 The [Metrics Forwarder for Pivotal Cloud Foundry (PCF)](https://docs.pivotal.io/metrics-forwarder/) is no longer supported. To export metrics to PCF, see [Prometheus](/docs/3/management/prometheus).

## ASP NET Core Example

The following example shows how to use the metrics actuator endpoint:

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
