# Metrics

The metrics functionality in Steeltoe is built on top of the OpenTelemetry project. Steeltoe extends its functionality by providing additional Instrumentation (via Metric Observers) and making it easy to exporting to additional destinations such as CloudFoundry, Spring Boot Admin, App Live View & Tanzu Observability.

## Add NuGet References

To use any of the metrics functionality, you need to add a reference to the `Steeltoe.Management.EndpointCore` NuGet package.

To add this type of NuGet to your project, add a `PackageReference` resembling the following:

```xml
<ItemGroup>
...
    <PackageReference Include="Steeltoe.Management.EndpointCore" Version="3.2.0"/>
...
</ItemGroup>
```

Alternatively, you can add the package through PowerShell:

```powershell
PM>Install-Package  Steeltoe.Management.EndpointCore -Version 3.2.0
```

## Metric Observers

Adding either the [metrics](./metrics.md) or [prometheus](./prometheus.md) endpoints automatically configures built-in instrumentation of various aspects of the application.

The following instrumentation is available:

| Metrics Type | Description |
| --- | --- |
| CLR | Heap memory, Garbage collections, Thread utilization. |
| HTTP Client | Request timings & counts. |
| HTTP Server | Request timings & counts. |
| Event Counter | CPU, Memory. |
| Hystrix Events | Circuit Breaker metrics. |

All of the above metrics are tagged with values specific to the requests being processed, thereby giving multi-dimensional views of the collected metrics.

### Configure Settings

The following table describes the settings that you can apply to the observers:

>Each setting below must be prefixed with `Management:Metrics:Observer`.

| Key | Description | Default |
| --- | --- | --- |
| `IngressIgnorePattern` | Regex pattern describing what incoming requests to ignore. | See `MetricsObserverOptions` |
| `EgressIgnorePattern` | Regex pattern describing what outgoing requests to ignore. | See `MetricsObserverOptions` |
| `AspNetCoreHosting` | Enable Http Server Metrics. | `true` |
| `GCEvents` | Enable Garbage collector Metrics. | `true` |
| `ThreadPoolEvents` | Enable Thread Pool Metrics. | `true` |
| `EventCounterEvents` | Enable Event Counter Metrics. | `false` |
| `HttpClientCore` | Enable Http Client Metrics. | `false` |
| `HttpClientDesktop` | Enable Http Client Desktop Metrics. | `false` |
| `HystrixEvents` | Enable Circuit Breaker Metrics. | `false` |
| `ExcludedMetrics` | Specify a list of metrics that should not be captured | none |

> The ExcludedMetrics option is new in 3.1.0 and only applies to [these events](https://docs.microsoft.com/dotnet/core/diagnostics/available-counters#systemruntime-counters), which are captured from counters in the runtime. Tne observer that reports these metrics is controlled by the `EventCounterEvents` setting above.

### Hystrix Event Source

Adding either the prometheus or metrics endpoints automatically adds the observers to service container for all the configured options. To get CircuitBreaker metrics, Hystrix Metrics EventSource must be added when configuring CircuitBreaker using `AddHystrixMetricsEventSource` extension.

## Metric Exporters

Steeltoe supports both pull and push-based configuration for exporting metrics. The pull-based mechanism is supported by the [metrics](./metrics.md) or [prometheus](./prometheus.md) endpoints. The push-based mechanism is supported by Wavefront Exporter.

### Tanzu Observability (Wavefront)

[Tanzu Observability](https://docs.wavefront.com/wavefront_introduction.html) is an observability platform for distributed applications that can ingest metric & trace data. A free trial is available [here](https://tanzu.vmware.com/observability-trial) to try it.

To add the Wavefront metric exporter, you can use any of the available extension methods:

* `hostBuilder.AddWavefrontMetrics` extension method from `ManagementHostBuilderExtensions`.
* `AddWavefrontMetrics()` extension method from `ManagementWebHostBuilderExtensions`.
* `AddWavefrontMetrics()` extension method from `ManagementWebApplicationBuilderExtensions`.

In addition the following settings are available to be configured. Note that these settings are shared between tracing and metrics.

| Key | Description | Default |
| --- | --- | --- |
| `ApiToken` | Your Tanzu Observability [API Token](https://docs.wavefront.com/users_account_managing.html#generate-an-api-token) | not set |
| `Uri` | The Uri of your Wavefront Instance | not set |
| `Step` | The interval between reporting to Wavefront  | 30000  |
| `BatchSize` | The max batch of data sent per flush interval | 10000 |
| `MaxQueueSize` |  the size of internal buffer beyond which data is dropped | 500000

**Note**: **Each setting above must be prefixed with `Management:Metrics:Export:Wavefront`**

If using the a proxy, the `apiToken` is not needed and the `uri` would be `proxy://<ProxyHost>:<ProxyPort>`

In addition, the following settings can be used to set the application and service names:

| Key | Description | Default |
| --- | --- | --- |
| `Source`| Unique identifier for the app instance that is publishing metrics to Wavefront | DNS name
| `Name` | Name of the application | SteeltoeApp |
| `Service` | Name of the service | SteeltoeService |

**Note**: **Each setting above must be prefixed with `Wavefront:Application`**

### Tanzu Application Service (TAS for VMs)

To emit custom metrics on TAS for VMs v2.5 or later, use the Metric Registrar. For more information about enabling and configuring the Metric Registrar, see [Configuring the Metric Registrar](https://docs.pivotal.io/application-service/2-11/metric-registrar/index.html).

To register your endpoint for metrics collection, install the metrics-registrar plugin and use it to register your endpoint:

```bash
cf install-plugin -r CF-Community "metric-registrar"`
cf register-metrics-endpoint your-dotnet-app /actuator/prometheus
```

### Prometheus Server

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

```bash
docker run -d  --name=prometheus -p 9090:9090 -v <Absolute-Path>/prometheus.yml:/etc/prometheus/prometheus.yml prom/prometheus --config.file=/etc/prometheus/prometheus.yml
```

### Tanzu App Live View

 [App Live View](https://docs.vmware.com/en/Application-Live-View-for-VMware-Tanzu/1.0/docs/GUID-index.html) has the ability to display and query metrics via the [Metrics endpoint](metrics-endpoint.md) in addition to other management endpoints.

### Spring Boot Admin

 The [Metrics endpoint](metrics-endpoint.md) is also compatible with  [Spring Boot Admin](https://github.com/codecentric/spring-boot-admin). See [Spring Boot Admin client](springbootadmin.md) for details.
