# Prometheus

The Steeltoe Prometheus endpoint configures application metrics collection using the open source [OpenCensus](https://opencensus.io/) project. Similar to the [Metrics Endpoint](./metrics.md), it automatically configures built-in instrumentation of various aspects of the application and exposes the collected metrics in the prometheus format.

The metrics collected are the same as those collected by the [Metrics Endpoint](./metrics.md).

## Configure Settings

The following table describes the settings that you can apply to the endpoint:

|Key|Description|Default|
|---|---|---|
|id|The ID of the metrics endpoint|`prometheus`|
|enabled|Whether to enable the metrics management endpoint|true|

**Note**: **Each setting above must be prefixed with `management:endpoints:prometheus`**.

## Enable HTTP Access

The default path to the Prometheus endpoint is computed by combining the global `path` prefix setting together with the `id` setting from above. The default path is  `/actuator/prometheus`.

### ASP.NET Core App

To add the Prometheus actuator to the service container, use the `AddPrometheusActuator()` extension method from `EndpointServiceCollectionExtensions`.

To add the Prometheus actuator middleware to the ASP.NET Core pipeline, use the `UsePrometheusActuator()` extension method from `EndpointApplicationBuilderExtensions`.

## Exporting

Prometheus metrics are typically configured to be scraped by registering the Prometheus with prometheus server. At this time the push model is not supported.

## Add NuGet References

To use the prometheus endpoint, you need to add a reference to `Steetoe.Management.EndpointCore`. To add this NuGet to your project, add a `PackageReference` resembling the following:

```xml
<ItemGroup>
...
    <PackageReference Include="Steeltoe.Management.EndpointCore" Version="2.5.2" />
...
</ItemGroup>
```

or

```powershell
PM>Install-Package  Steeltoe.Management.EndpointCore -Version 2.5.2
```

## Cloud Foundry Forwarder

The [Metrics Forwarder for TAS](https://docs.pivotal.io/metrics-forwarder/) is no longer supported on Tanzu Application Service (TAS) v2.5 and later. To emit custom metrics on PAS v2.5 or later, use the Metric Registrar. For more information about enabling and configuring the Metric Registrar, see [Configuring the Metric Registrar](https://docs.pivotal.io/platform/application-service/2-8/metric-registrar/index.html).

To register your endpoint for metrics collection install the metrics-registrar plugin and use it to register your endpoint.

`cf install-plugin -r CF-Community "metric-registrar"`

`cf register-metrics-endpoint your-dotnet-app /actuator/prometheus`

## Prometheus Server

[Prometheus Server](https://prometheus.io/) can be set up to scrape this endpoint by registering your application in the server's configuration. For example, this prometheus.yml expects a Steeltoe-enabled app running on port 8000 with the actuator management path at the default of /actuator:

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
Running Prometheus server with this configuration will allow you view metrics in the built-in UI. Other visualization tools such as [Grafana](https://grafana.com/docs/grafana/latest/features/datasources/prometheus/) can then be configured to use Prometheus as a datasource.

```docker
docker run -d  --name=prometheus -p 9090:9090 -v <Absolute-Path>/prometheus.yml:/etc/prometheus/prometheus.yml prom/prometheus --config.file=/etc/prometheus/prometheus.yml
```
