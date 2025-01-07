# Metrics

Steeltoe provides two ways of collecting and exporting application metrics.
The Steeltoe Metrics endpoint (described on this page) is built into Steeltoe and does not require additional dependencies, while the [Prometheus endpoint](./prometheus.md) is built on top of [OpenTelemetry .NET](https://github.com/open-telemetry/opentelemetry-dotnet).

The Metrics endpoint uses [Metric Observers](#metric-observers) to automatically instrument the application for metric collection and makes it easy to export to destinations that accept the Spring metrics format such as [Metric Registrar for Tanzu Platform for Cloud Foundry](https://techdocs.broadcom.com/us/en/vmware-tanzu/platform/tanzu-platform-for-cloud-foundry/10-0/tpcf/metric-registrar-index.html), [Tanzu App Live View](https://techdocs.broadcom.com/us/en/vmware-tanzu/standalone-components/tanzu-application-platform/1-12/tap/app-live-view-about-app-live-view.html) and [Spring Boot Admin](https://docs.spring-boot-admin.com/).

## Configure Settings

The following table describes the configuration settings that you can apply to the endpoint.
Each key must be prefixed with `Management:Endpoints:Metrics:`.

| Key | Description | Default |
| --- | --- | --- |
| `Enabled` | Whether the endpoint is enabled. | `true` |
| `ID` | The unique ID of the endpoint. | `metrics` |
| `Path` | The relative path at which the endpoint is exposed. | same as `ID` |
| `RequiredPermissions` | Permissions required to access the endpoint, when running on Cloud Foundry. | `Restricted` |
| `AllowedVerbs` | An array of HTTP verbs the endpoint is exposed at. | `GET` |
| `CacheDurationMilliseconds` | The duration that metrics are cached for. | `500` |
| `MaxTimeSeries` | The maximum number of time series to return. | `100` |
| `MaxHistograms` | The maximum number of histograms to return. | `100` |
| `IncludedMetrics` | An array of additional [metric names](https://learn.microsoft.com/dotnet/core/diagnostics/available-counters#systemruntime-counters) to include. | |

## Enable HTTP Access

The URL path to the endpoint is computed by combining the global `Management:Endpoints:Path` setting together with the `Path` setting described in the preceding section.
The default path is `/actuator/metrics`.

See the [Exposing Endpoints](./using-endpoints.md#exposing-endpoints) and [HTTP Access](./using-endpoints.md#http-access) sections for the overall steps required to enable HTTP access to endpoints in an ASP.NET Core application.

To add the actuator to the service container and map its route, use the `AddMetricsActuator` extension method.

Add the following code to `Program.cs` to use the actuator endpoint:

```csharp
using Steeltoe.Management.Endpoint.Actuators.Metrics;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMetricsActuator();
```

> [!TIP]
> It's recommended to use `AddAllActuators()` instead of adding individual actuators,
> which enables individually turning them on/off at runtime via configuration.

## Sample Output

This endpoint returns the metric names.

The response will always be returned as JSON, like this:

```json
{
  "names": [
    "clr.gc.collections",
    "http.client.request.time",
    "clr.process.uptime",
    "clr.memory.used",
    "http.server.requests.seconds",
    "clr.cpu.count",
    "clr.threadpool.avail",
    "http.client.request.count",
    "clr.threadpool.active",
    "http.server.requests.count"
  ]
}
```

The names returned can be used to obtain metric values.
For example, a `GET` request to `actuator/metrics/http.server.requests.count` returns something like:

```json
{
  "name": "http.server.requests.count",
  "measurements": [
    {
      "statistic": "Total",
      "value": 4
    }
  ],
  "availableTags": [
    {
      "tag": "exception",
      "values": [
        "None"
      ]
    },
    {
      "tag": "method",
      "values": [
        "GET"
      ]
    },
    {
      "tag": "status",
      "values": [
        "200"
      ]
    },
    {
      "tag": "uri",
      "values": [
        "/actuator/metrics/http.server.requests.count",
        "/actuator/metrics"
      ]
    }
  ]
}
```

## Metric Observers

Adding the metrics endpoint automatically configures built-in instrumentation of various aspects of the application.

The following instrumentation is available:

| Metrics Type | Description |
| --- | --- |
| CLR | Heap memory, garbage collections, thread utilization. |
| HTTPClient | Request timings and counts. |
| ASP.NET Core | Request timings and counts. |
| Event Counter | CPU, Memory. |

All of the above metrics are tagged with values specific to the requests being processed, thereby giving multi-dimensional views of the collected metrics.

### Configure Observer Settings

The following table describes the configuration settings that you can apply to the observers.
Each key must be prefixed with `Management:Metrics:Observer`.

| Key | Description | Default |
| --- | --- | --- |
| `IngressIgnorePattern` | Regex pattern describing what incoming requests to ignore. | See `MetricsObserverOptions` |
| `EgressIgnorePattern` | Regex pattern describing what outgoing requests to ignore. | See `MetricsObserverOptions` |
| `AspNetCoreHosting` | Enable Http Server Metrics. | `true` |
| `GCEvents` | Enable Garbage collector Metrics. | `true` |
| `ThreadPoolEvents` | Enable Thread Pool Metrics. | `true` |
| `EventCounterEvents` | Enable Event Counter Metrics. | `false` |
| `HttpClient` | Capture outgoing HTTP requests using [`HttpClient`](https://learn.microsoft.com//dotnet/api/system.net.http.httpclient). | `false` |
| `ExcludedMetrics` | An array of metric names to exclude from capture (see note below). | |

> [!NOTE]
> The ExcludedMetrics option only applies to [these events](https://learn.microsoft.com/dotnet/core/diagnostics/available-counters#systemruntime-counters), which are captured from counters in the runtime. The observer that reports these metrics is controlled by the `EventCounterEvents` setting above.

## Metric Exporters

For exporting to Prometheus server, see the [Prometheus endpoint](./prometheus.md).

### Tanzu Platform for Cloud Foundry

To emit custom metrics in Cloud Foundry, use [Metric Registrar](https://techdocs.broadcom.com/us/en/vmware-tanzu/platform/tanzu-platform-for-cloud-foundry/10-0/tpcf/metric-registrar-index.html).

> [!CAUTION]
> Authenticated endpoints are not supported with Metric Registrar.
> For this scenario, consider configuring actuators to [use an alternate port](./using-endpoints.md#configure-global-settings) and use that private network port to offer the metrics.

There are two methods available to register your endpoint for metrics collection

#### Metrics Registrar Plugin

Install the metrics-registrar plugin and use it to register your endpoint:

```shell
cf install-plugin -r CF-Community "metric-registrar"
cf register-metrics-endpoint APP-NAME /actuator/metrics --internal-port 8091
```

> [!CAUTION]
> Due to an issue with the cf cli plugin interface, some variations on this command do not work on Windows.
> If you are a Windows user, you should either use the metric registrar plugin from WSL or use another method.

### Create User Provided Service

The result of using the metrics registrar plugin is a user-provided service, which can also be created and bound manually.

```shell
cf create-user-provided-service APP-NAME -l secure-endpoint://:8091/actuator/metrics
cf bind-service APP-NAME SERVICE-NAME
```

### Tanzu App Live View

 [App Live View](https://techdocs.broadcom.com/us/en/vmware-tanzu/standalone-components/tanzu-application-platform/1-12/tap/app-live-view-about-app-live-view.html) has the ability to display and query metrics, in addition to other management endpoints.

### Spring Boot Admin

 The metrics endpoint is also compatible with [Spring Boot Admin](https://github.com/codecentric/spring-boot-admin). See [Spring Boot Admin client](springbootadmin.md) for details.
