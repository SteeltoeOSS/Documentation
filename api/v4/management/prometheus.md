# Prometheus

You can use the Prometheus endpoint to expose application metrics for collection by an external process.

The Steeltoe Prometheus endpoint configures the [OpenTelemetry Prometheus Exporter](https://opentelemetry.io/docs/languages/net/exporters/#prometheus) to behave like a Steeltoe management endpoint.

The Prometheus endpoint does not automatically instrument your application, but does make it easy to export metrics in the Prometheus metrics format, which can be used by tools like [Prometheus Server](https://prometheus.io/) and the [Metric Registrar for Tanzu Platform for Cloud Foundry](https://techdocs.broadcom.com/us/en/vmware-tanzu/platform/tanzu-platform-for-cloud-foundry/10-0/tpcf/metric-registrar-index.html).

The [Steeltoe Management samples](https://github.com/SteeltoeOSS/Samples/tree/main/Management/src/ActuatorWeb/README.md) can help you understand how to use this tool.

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
builder.Services.AddPrometheusActuator(true, pipeline => pipeline.UseAuthorization());
```

## Instrumentation

In order for the Prometheus endpoint to return metrics, the application and relevant libraries need to be instrumented.
This page will cover the basics for elements that previous versions of Steeltoe configured automatically.
Please refer to the [OpenTelemetry documentation](https://opentelemetry.io/docs/languages/net/instrumentation/) for more detailed information.

### ASP.NET Core

To instrument ASP.NET Core for metrics, start by adding a reference to the `OpenTelemetry.Instrumentation.AspNetCore` NuGet package.

Next, add the instrumentation to the `MeterProviderBuilder`:

```csharp
using OpenTelemetry.Metrics;

builder.Services.AddOpenTelemetry().WithMetrics(metrics => metrics.AddAspNetCoreInstrumentation());
```

[Learn more about ASP.NET Core instrumentation for OpenTelemetry](https://github.com/open-telemetry/opentelemetry-dotnet-contrib/blob/main/src/OpenTelemetry.Instrumentation.AspNetCore)

### HttpClient

To instrument `HttpClient`s for metrics, start by adding a reference to the `OpenTelemetry.Instrumentation.Http` NuGet package.

Next, add the instrumentation to the `MeterProviderBuilder`:

```csharp
using OpenTelemetry.Metrics;

builder.Services.AddOpenTelemetry().WithMetrics(metrics => metrics.AddHttpClientInstrumentation());
```

[Learn more about HttpClient instrumentation for OpenTelemetry](https://github.com/open-telemetry/opentelemetry-dotnet-contrib/tree/main/src/OpenTelemetry.Instrumentation.Http)

### .NET Runtime

To instrument the .NET Runtime for metrics, start by adding a reference to the `OpenTelemetry.Instrumentation.Runtime` NuGet package.

Next, add the instrumentation to the `MeterProviderBuilder`:

```csharp
using OpenTelemetry.Metrics;

builder.Services.AddOpenTelemetry().WithMetrics(metrics => metrics.AddRuntimeInstrumentation());
```

[Learn more about Runtime Instrumentation for OpenTelemetry .NET](https://github.com/open-telemetry/opentelemetry-dotnet-contrib/tree/main/src/OpenTelemetry.Instrumentation.Runtime)

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

The following example shows how to run Prometheus in Docker, referencing the configuration file from above:

```shell
docker run -d --name=prometheus -p 9090:9090 -v ./prometheus.yml:/etc/prometheus/prometheus.yml prom/prometheus --config.file=/etc/prometheus/prometheus.yml
```

### Tanzu Platform for Cloud Foundry

To emit custom metrics in Cloud Foundry, use [Metric Registrar](https://techdocs.broadcom.com/us/en/vmware-tanzu/platform/tanzu-platform-for-cloud-foundry/10-0/tpcf/metric-registrar-index.html).

> [!CAUTION]
> Authenticated endpoints are not supported with Metric Registrar.
> For this scenario, configure actuators to [use an alternate port](./using-endpoints.md#configure-global-settings) and use that private network port to offer the metrics.

The examples below expect that actuators are mapped to an alternate port (8091 in this case).
The specific port that is used is not important to Steeltoe, it only matters that the binding and Steeltoe configurations match.

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

## Sample Output

This endpoint returns information about the instrumented application metrics and their values.

If no instrumentation has been included, the response will be very short, like this:

```text
# EOF
```

With the addition of [.NET Runtime instrumentation](#net-runtime), the response will be like this:

```text
# TYPE process_runtime_dotnet_gc_collections_count_total counter
# HELP process_runtime_dotnet_gc_collections_count_total Number of garbage collections that have occurred since process start.
process_runtime_dotnet_gc_collections_count_total{otel_scope_name="OpenTelemetry.Instrumentation.Runtime",otel_scope_version="1.11.0",generation="gen2"} 0 1740147372796
process_runtime_dotnet_gc_collections_count_total{otel_scope_name="OpenTelemetry.Instrumentation.Runtime",otel_scope_version="1.11.0",generation="gen1"} 0 1740147372796
process_runtime_dotnet_gc_collections_count_total{otel_scope_name="OpenTelemetry.Instrumentation.Runtime",otel_scope_version="1.11.0",generation="gen0"} 0 1740147372796
# TYPE process_runtime_dotnet_gc_objects_size_bytes gauge
# UNIT process_runtime_dotnet_gc_objects_size_bytes bytes
# HELP process_runtime_dotnet_gc_objects_size_bytes Count of bytes currently in use by objects in the GC heap that haven't been collected yet. Fragmentation and other GC committed memory pools are excluded.
process_runtime_dotnet_gc_objects_size_bytes{otel_scope_name="OpenTelemetry.Instrumentation.Runtime",otel_scope_version="1.11.0"} 8830400 1740147372797
# TYPE process_runtime_dotnet_gc_allocations_size_bytes_total counter
# UNIT process_runtime_dotnet_gc_allocations_size_bytes_total bytes
# HELP process_runtime_dotnet_gc_allocations_size_bytes_total Count of bytes allocated on the managed GC heap since the process start. .NET objects are allocated from this heap. Object allocations from unmanaged languages such as C/C++ do not use this heap.
process_runtime_dotnet_gc_allocations_size_bytes_total{otel_scope_name="OpenTelemetry.Instrumentation.Runtime",otel_scope_version="1.11.0"} 8813792 1740147372797
# TYPE process_runtime_dotnet_gc_duration_nanoseconds_total counter
# UNIT process_runtime_dotnet_gc_duration_nanoseconds_total nanoseconds
# HELP process_runtime_dotnet_gc_duration_nanoseconds_total The total amount of time paused in GC since the process start.
process_runtime_dotnet_gc_duration_nanoseconds_total{otel_scope_name="OpenTelemetry.Instrumentation.Runtime",otel_scope_version="1.11.0"} 0 1740147372797
# TYPE process_runtime_dotnet_jit_il_compiled_size_bytes_total counter
# UNIT process_runtime_dotnet_jit_il_compiled_size_bytes_total bytes
# HELP process_runtime_dotnet_jit_il_compiled_size_bytes_total Count of bytes of intermediate language that have been compiled since the process start.
process_runtime_dotnet_jit_il_compiled_size_bytes_total{otel_scope_name="OpenTelemetry.Instrumentation.Runtime",otel_scope_version="1.11.0"} 272834 1740147372797
# TYPE process_runtime_dotnet_jit_methods_compiled_count_total counter
# HELP process_runtime_dotnet_jit_methods_compiled_count_total The number of times the JIT compiler compiled a method since the process start. The JIT compiler may be invoked multiple times for the same method to compile with different generic parameters, or because tiered compilation requested different optimization settings.
process_runtime_dotnet_jit_methods_compiled_count_total{otel_scope_name="OpenTelemetry.Instrumentation.Runtime",otel_scope_version="1.11.0"} 4597 1740147372797
# TYPE process_runtime_dotnet_jit_compilation_time_nanoseconds_total counter
# UNIT process_runtime_dotnet_jit_compilation_time_nanoseconds_total nanoseconds
# HELP process_runtime_dotnet_jit_compilation_time_nanoseconds_total The amount of time the JIT compiler has spent compiling methods since the process start.
process_runtime_dotnet_jit_compilation_time_nanoseconds_total{otel_scope_name="OpenTelemetry.Instrumentation.Runtime",otel_scope_version="1.11.0"} 562297300 1740147372797
# TYPE process_runtime_dotnet_monitor_lock_contention_count_total counter
# HELP process_runtime_dotnet_monitor_lock_contention_count_total The number of times there was contention when trying to acquire a monitor lock since the process start. Monitor locks are commonly acquired by using the lock keyword in C#, or by calling Monitor.Enter() and Monitor.TryEnter().
process_runtime_dotnet_monitor_lock_contention_count_total{otel_scope_name="OpenTelemetry.Instrumentation.Runtime",otel_scope_version="1.11.0"} 0 1740147372797
# TYPE process_runtime_dotnet_thread_pool_threads_count gauge
# HELP process_runtime_dotnet_thread_pool_threads_count The number of thread pool threads that currently exist.
process_runtime_dotnet_thread_pool_threads_count{otel_scope_name="OpenTelemetry.Instrumentation.Runtime",otel_scope_version="1.11.0"} 5 1740147372797
# TYPE process_runtime_dotnet_thread_pool_completed_items_count_total counter
# HELP process_runtime_dotnet_thread_pool_completed_items_count_total The number of work items that have been processed by the thread pool since the process start.
process_runtime_dotnet_thread_pool_completed_items_count_total{otel_scope_name="OpenTelemetry.Instrumentation.Runtime",otel_scope_version="1.11.0"} 62 1740147372797
# TYPE process_runtime_dotnet_thread_pool_queue_length gauge
# HELP process_runtime_dotnet_thread_pool_queue_length The number of work items that are currently queued to be processed by the thread pool.
process_runtime_dotnet_thread_pool_queue_length{otel_scope_name="OpenTelemetry.Instrumentation.Runtime",otel_scope_version="1.11.0"} 0 1740147372797
# TYPE process_runtime_dotnet_timer_count gauge
# HELP process_runtime_dotnet_timer_count The number of timer instances that are currently active. Timers can be created by many sources such as System.Threading.Timer, Task.Delay, or the timeout in a CancellationSource. An active timer is registered to tick at some point in the future and has not yet been canceled.
process_runtime_dotnet_timer_count{otel_scope_name="OpenTelemetry.Instrumentation.Runtime",otel_scope_version="1.11.0"} 2 1740147372797
# TYPE process_runtime_dotnet_assemblies_count gauge
# HELP process_runtime_dotnet_assemblies_count The number of .NET assemblies that are currently loaded.
process_runtime_dotnet_assemblies_count{otel_scope_name="OpenTelemetry.Instrumentation.Runtime",otel_scope_version="1.11.0"} 149 1740147372797
# EOF
```
