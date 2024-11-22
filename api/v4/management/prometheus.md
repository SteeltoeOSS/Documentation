# Prometheus

The Steeltoe Prometheus endpoint exposes metrics collected via built-in instrumentation of various aspects of the application in the Prometheus format.  Similar to the [metrics endpoint](./metrics-endpoint.md), it automatically configures built-in instrumentation of various aspects of the application.

The metrics collected are the same as those collected by the [metrics endpoint](./metrics-endpoint.md).

## Add NuGet References

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

To configure Observers, see [metric observers](./metrics.md#metric-observers).

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

> [!TIP]
> It's recommended to use `AddAllActuators()` instead of adding individual actuators,
> which enables individually turning them on/off at runtime via configuration.
