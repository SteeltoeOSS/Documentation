# Metric Observers

Adding either the [metrics](/docs/3/management/metrics) or [prometheus](/docs/3/management/prometheus) endpoints automatically configures built-in instrumentation of various aspects of the application.

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

>NOTE: Each setting below must be prefixed with `Management:Metrics:Observer`.

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

## Hystrix Event Source

Adding either the prometheus or metrics endpoints automatically adds the observers to service container for all the configured options. To get CircuitBreaker metrics, Hystrix Metrics EventSource must be added when configuring CircuitBreaker using `AddHystrixMetricsEventSource` extension.
