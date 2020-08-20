## Metric Observers

Adding either the [metrics](/docs/management/metrics) or [prometheus](/docs/management/prometheus) endpoints automatically configures built-in instrumentation of various aspects of the application.

The following instrumentation is available:

* CLR Metrics
  * Heap memory, Garbage collections, Thread utilization
* HTTP Client Metrics
  * Request timings & counts
* HTTP Server Metrics
  * Request timings & counts
* Event Counter Metrics
  * CPU, Memory
* Hystrix Events Metrics
  * Circuit Breaker metrics

All of the above metrics are tagged with values specific to the requests being processed; thereby giving multi-dimensional views of the collected metrics.

### Configure Settings

The following table describes the settings that you can apply to the observers:

The following table describes the settings that you can apply to the observes.

|Key|Description|Default|
|---|---|---|
|ingressIgnorePattern|Regex pattern describing what incoming requests to ignore|See `MetricsObserverOptions`|
|egressIgnorePattern|Regex pattern describing what outgoing requests to ignore|See `MetricsObserverOptions`|
|AspNetCoreHosting|Enable Http Server Metrics| `true`|
|GCEvents|Enable Garbage collector Metrics| `true`|
|ThreadPoolEvents| Enable Thread Pool Metrics|`true`|
|EventCounterEvents| Enable Event Counter Metrics | `false`|
|HttpClientCore| Enable Http Client Metrics| `false`|
|HttpClientDesktop| Enable Http Client Desktop Metrics| `false`|
|HystrixEvents| Enable Circuit Breaker Metrics| `true`|


**Note**: Each setting above must be prefixed with `management:metrics:observer`.

### Add Hystrix EventStream

Adding either the prometheus or metrics endpoints automatically adds the observers to service container for all the configured options. To get CircuitBreaker metrics, Hystrix Metrics EventSource must be added when configuring CircuitBreaker using `AddHystrixMetricsEventSource` extension.



