# Prometheus

The Steeltoe prometheus endpoint exposes metrics collected via built-in instrumentation of various aspects of the application in the prometheus format.  Similar to the [Metrics Endpoint](./metrics.md), it automatically configures built-in instrumentation of various aspects of the application.

The metrics collected are the same as those collected by the [metrics endpoint](./metrics.md).

## Configure Settings

The following table describes the settings that you can apply to the endpoint:

| Key | Description | Default |
| --- | --- | --- |
| `Id` | The ID of the metrics endpoint. | `prometheus` |
| `Enabled` | Whether to enable the metrics management endpoint. | `true` |

>Each setting must be prefixed with `Management:Endpoints:Prometheus`.

To configure Observers, see [Metric Observers](./metric-observers.md)

## Enable HTTP Access

The default path to the Prometheus endpoint is computed by combining the global `Path` prefix setting together with the `Id` setting described in the preceding section. The default path is `/actuator/prometheus`.

See the [HTTP Access](./using-endpoints.md#http-access) section to see the overall steps required to enable HTTP access to endpoints in an ASP.NET Core application.

To add the actuator to the service container and map its route, use the `AddPrometheusActuator` extension method from `ManagementHostBuilderExtensions`.

The following example shows how to use the metrics actuator endpoint:

```csharp
 public static IHost BuildHost(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .AddPrometheusActuator()
            .Build();
```

Alternatively, first,add the Prometheus actuator to the service container, using the `AddPrometheusActuator()` extension method from `EndpointServiceCollectionExtensions`.

Then, add the Prometheus actuator middleware to the ASP.NET Core pipeline, use the `Map<PrometheusEndpoint>()` extension method from `ActuatorRouteBuilderExtensions`.

## Exporting

Prometheus metrics are typically configured to be scraped by registering the Prometheus with Prometheus server. At this time, the push model is not supported.

## Add NuGet References

To use the Prometheus endpoint, you need to add a reference to `Steeltoe.Management.EndpointCore`. To add this NuGet to your project, add a `PackageReference` resembling the following:

```xml
<ItemGroup>
...
    <PackageReference Include="Steeltoe.Management.EndpointCore" Version="3.0.2"/>
...
</ItemGroup>
```

Alternatively, you can use PowerShell:

```powershell
PM>Install-Package  Steeltoe.Management.EndpointCore -Version 3.0.2
```

## Cloud Foundry

The [Metrics Forwarder for Pivotal Cloud Foundry (PCF)](https://docs.pivotal.io/metrics-forwarder/) is no longer supported on Pivotal Application Service (PAS) v2.5 and later. To emit custom metrics on PAS v2.5 or later, use the Metric Registrar. For more information about enabling and configuring the Metric Registrar, see [Configuring the Metric Registrar](https://docs.pivotal.io/platform/application-service/2-8/metric-registrar/index.html).

To register your endpoint for metrics collection, install the metrics-registrar plugin and use it to register your endpoint:

`cf install-plugin -r CF-Community "metric-registrar"`

`cf register-metrics-endpoint your-dotnet-app /actuator/prometheus`

## Prometheus Server

You can set up [Prometheus Server](https://prometheus.io/) to scrape this endpoint by registering your application in the server's configuration. For example, the following `prometheus.yml` file expects a Steeltoe-enabled application to be running on port 8000 with the actuator management path at the default of `/actuator`:

```yml
global:
  scrape_interval: 15s # Set the scrape interval to every 15 seconds. Default is every 1 minute.
  evaluation_interval: 15s # Evaluate rules every 15 seconds. The default is every 1 minute.
  # scrape_timeout is set to the global default (10s).
scrape_configs:
  # The job name is added as a label `job=<job_name>` to any timeseries scraped from this config.
  - job_name: 'steeltoe-prometheus'
    metrics_path: '/actuator/prometheus'
    scrape_interval: 5s
    static_configs:
      - targets: ['host.docker.internal:8000']
```

Running Prometheus server with this configuration lets you view metrics in the built-in UI. You can then configure other visualization tools, such as [Grafana](https://grafana.com/docs/grafana/latest/features/datasources/prometheus/), to use Prometheus as a datasource. The following example shows how to run Prometheus in Docker:

```docker
docker run -d  --name=prometheus -p 9090:9090 -v <Absolute-Path>/prometheus.yml:/etc/prometheus/prometheus.yml prom/prometheus --config.file=/etc/prometheus/prometheus.yml
```
