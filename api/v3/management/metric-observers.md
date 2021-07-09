# Metric Observers

Adding either the [metrics](./metrics.md) or [prometheus](./prometheus.md) endpoints automatically configures built-in instrumentation of various aspects of the application.

The following instrumentation is available:

| Metrics Type | Description |
| --- | --- |
| CLR | Heap memory, Garbage collections, Thread utilization. |
| HTTP Client | Request timings & counts. |
| HTTP Server | Request timings & counts. |
| Event Counter | CPU, Memory. |
| Hystrix Events | Circuit Breaker metrics. |

All of the above metrics are tagged with values specific to the requests being processed; thereby giving multi-dimensional views of the collected metrics.

## Configure Settings

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

## Hystrix Event Source

Adding either the prometheus or metrics endpoints automatically adds the observers to service container for all the configured options. To get CircuitBreaker metrics, Hystrix Metrics EventSource must be added when configuring CircuitBreaker using `AddHystrixMetricsEventSource` extension.
