# Metrics

The Steeltoe Metrics endpoint configures built-in instrumentation of various aspects of the application and exposes them in Spring Boot Metrics format.

The following instrumentation is automatically configured:

* CLR metrics
  * Heap memory, garbage collections, thread utilization
* ASP.NET metrics
  * Request timings and counts
* HttpClient metrics
  * Request timings and counts

All of these metrics are tagged with values specific to the request being processed, thereby giving multi-dimensional views of the collected metrics.

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
