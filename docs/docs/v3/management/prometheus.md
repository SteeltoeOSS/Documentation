# Prometheus

The Steeltoe prometheus endpoint exposes metrics collected via built-in instrumentation of various aspects of the application in the prometheus format.  Similar to the [Metrics Endpoint](./metrics-endpoint.md), it automatically configures built-in instrumentation of various aspects of the application.

The metrics collected are the same as those collected by the [metrics endpoint](./metrics-endpoint.md).

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

### Add NuGet References

To use the Prometheus endpoint, you need to add a reference to `Steeltoe.Management.EndpointCore`. To add this NuGet to your project, add a `PackageReference` resembling the following:

```xml
<ItemGroup>
...
    <PackageReference Include="Steeltoe.Management.EndpointCore" Version="3.2.0"/>
...
</ItemGroup>
```

Alternatively, you can use PowerShell:

```powershell
PM>Install-Package  Steeltoe.Management.EndpointCore
```
