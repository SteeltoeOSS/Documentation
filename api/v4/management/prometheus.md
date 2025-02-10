# Prometheus

The Steeltoe Prometheus endpoint configures the [OpenTelemetry Prometheus Exporter](https://opentelemetry.io/docs/languages/net/exporters/#prometheus) to behave like a Steeltoe management endpoint.

The Prometheus endpoint does not automatically instrument your application, but does make it easy to export metrics in the Prometheus metrics format, which can be used by tools like [Prometheus Server](https://prometheus.io/) and the [Metric Registrar for Tanzu Platform for Cloud Foundry](https://techdocs.broadcom.com/us/en/vmware-tanzu/platform/tanzu-platform-for-cloud-foundry/10-0/tpcf/metric-registrar-index.html).

## Add NuGet Reference

To use the Prometheus endpoint, you need to add a reference to the `Steeltoe.Management.Prometheus` NuGet package.

## Configure Settings

The following table describes the configuration settings that you can apply to the endpoint.
Each key must be prefixed with `Management:Endpoints:Prometheus:`.

| Key | Description | Default |
| --- | --- | --- |
| `Enabled` | Whether the endpoint is enabled. | `true` |
| `ID` | The unique ID of the endpoint. | `prometheus` |
| `Path` | The relative path at which the endpoint is exposed. | same as `ID` |
| `RequiredPermissions` | Permissions required to access the endpoint, when running on Cloud Foundry. | `Restricted` |
| `AllowedVerbs` | An array of HTTP verbs the endpoint is exposed at. | `GET` |

> [!NOTE]
> The `AllowedVerbs` setting is inherited from Steeltoe's `EndpointOptions`, but is not intended to work for the Prometheus exporter, which is only registered to respond to `GET` requests.

## Enable HTTP Access

The URL path to the endpoint is computed by combining the global `Management:Endpoints:Path` setting together with the `Path` setting described in the preceding section.
The default path is `/actuator/prometheus`.

See the [Exposing Endpoints](./using-endpoints.md#exposing-endpoints) and [HTTP Access](./using-endpoints.md#http-access) sections for the overall steps required to enable HTTP access to endpoints in an ASP.NET Core application.

To add the actuator to the service container and map its route, use the `AddPrometheusActuator` extension method.

Add the following code to `Program.cs` to use the actuator endpoint:

```csharp
using Steeltoe.Management.Prometheus;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddPrometheusActuator();
```

### Configuring the request pipeline for Prometheus

In addition to the options described in [using endpoints](./using-endpoints.md), `AddPrometheusActuator` exposes an `Action<IApplicationBuilder>?` that can be used to configure the branched request pipeline that is used in the underlying OpenTelemetry package.
This pipeline would need to be configured if, as an example, you are configuring an authorization policy.

```csharp
builder.Services.AddPrometheusActuator(true, configurePrometheusPipeline =>
{
    configurePrometheusPipeline.UseAuthorization();
});
```

## Instrumentation

In order for the Prometheus endpoint to return metrics, the application and relevant libraries need to be instrumented.
This page will cover the basics for elements that previous versions of Steeltoe configured automatically.
Please refer to the [OpenTelemetry documentation](https://opentelemetry.io/docs/languages/net/instrumentation/) for more detailed information.

### ASP.NET Core

To instrument ASP.NET Core for metrics, start by adding a reference to the `OpenTelemetry.Instrumentation.AspNetCore` NuGet package.

Next, add the instrumentation to the `MeterProviderBuilder`:

```csharp
services.AddOpenTelemetry().WithMetrics(meterProviderBuilder =>
{
    meterProviderBuilder.AddAspNetCoreInstrumentation();
})
```

[Learn more about ASP.NET Core instrumentation for OpenTelemetry](https://github.com/open-telemetry/opentelemetry-dotnet-contrib/blob/main/src/OpenTelemetry.Instrumentation.AspNetCore)

### HttpClient

To instrument `HttpClient`s for metrics, start by adding a reference to the `OpenTelemetry.Instrumentation.Http` NuGet package.

Next, add the instrumentation to the `MeterProviderBuilder`:

```csharp
services.AddOpenTelemetry().WithMetrics(meterProviderBuilder =>
{
    meterProviderBuilder.AddHttpClientInstrumentation();
})
```

[Learn more about HttpClient instrumentation for OpenTelemetry](https://github.com/open-telemetry/opentelemetry-dotnet-contrib/tree/main/src/OpenTelemetry.Instrumentation.Http)

### .NET Runtime

To instrument the .NET Runtime for metrics, start by adding a reference to the `OpenTelemetry.Instrumentation.Runtime` NuGet package.

Next, add the instrumentation to the `MeterProviderBuilder`:

```csharp
services.AddOpenTelemetry().WithMetrics(meterProviderBuilder =>
{
    meterProviderBuilder.AddRuntimeInstrumentation();
})
```

[Learn more about Runtime Instrumentation for OpenTelemetry .NET](https://github.com/open-telemetry/opentelemetry-dotnet-contrib/tree/main/src/OpenTelemetry.Instrumentation.Runtime)

## Exporting Metrics

For exporting in Spring metrics format, see the [Metrics endpoint](./metrics.md).

### Prometheus Server

You can set up Prometheus to scrape this endpoint by registering your application in the server's configuration.

As an example, the following `prometheus.yml` file configures metric scraping for a Steeltoe-enabled application listening on port 8091 with the default actuator path:

```yml
global:
  scrape_interval: 15s # Set the scrape interval to every 15 seconds. The default is every 1 minute.
  evaluation_interval: 15s # Evaluate rules every 15 seconds. The default is every 1 minute.
  # scrape_timeout is set to the global default (10s).
scrape_configs:
  # The job name is added as a label `job=<job_name>` to any timeseries scraped from this config.
  - job_name: 'steeltoe-prometheus'
    metrics_path: '/actuator/prometheus'
    scrape_interval: 5s
    static_configs:
      - targets: ['host.docker.internal:8091']
```

Running the Prometheus server with this configuration lets you view metrics in the built-in UI.
You can then configure other visualization tools, such as [Grafana](https://grafana.com/docs/grafana/latest/features/datasources/prometheus/), to use Prometheus as a data source.

The following example shows how to run Prometheus in Docker:

```shell
docker run -d --name=prometheus -p 9090:9090 -v ./prometheus.yml:/etc/prometheus/prometheus.yml prom/prometheus --config.file=/etc/prometheus/prometheus.yml
```

### Tanzu Platform for Cloud Foundry

To emit custom metrics in Cloud Foundry, use [Metric Registrar](https://techdocs.broadcom.com/us/en/vmware-tanzu/platform/tanzu-platform-for-cloud-foundry/10-0/tpcf/metric-registrar-index.html).

> [!CAUTION]
> Authenticated endpoints are not supported with Metric Registrar.
> For this scenario, consider configuring actuators to [use an alternate port](./using-endpoints.md#configure-global-settings) and use that private network port to offer the metrics.

#### Metrics Registrar Plugin

Install the metrics-registrar plugin and use it to register your endpoint:

```shell
cf install-plugin -r CF-Community "metric-registrar"
cf register-metrics-endpoint APP-NAME /actuator/prometheus --internal-port 8091
```

> [!CAUTION]
> Due to an issue with the Cloud Foundry CLI plugin interface, some variations on this command do not work on Windows.
> If you are a Windows user, you should either use the metric registrar plugin from WSL or use another method.

#### Create User Provided Service

The result of using the metrics registrar plugin is a user-provided service, which can also be created and bound manually.

```shell
cf create-user-provided-service APP-NAME -l secure-endpoint://:8091/actuator/prometheus
cf bind-service APP-NAME SERVICE-NAME
```
