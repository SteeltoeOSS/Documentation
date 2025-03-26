# Heap Dump

You can use the Steeltoe Heap Dump endpoint to capture and download a mini-dump of your application. The mini-dump can then be read into Visual Studio for analysis.

## Configure Settings

The following table describes the configuration settings that you can apply to the endpoint.
Each key must be prefixed with `Management:Endpoints:HeapDump:`.

| Key | Description | Default |
| --- | --- | --- |
| `Enabled` | Whether the endpoint is enabled. | `true` |
| `ID` | The unique ID of the endpoint. | `heapdump` |
| `Path` | The relative path at which the endpoint is exposed. | same as `ID` |
| `RequiredPermissions` | Permissions required to access the endpoint, when running on Cloud Foundry. | `Restricted` |
| `AllowedVerbs` | An array of HTTP verbs the endpoint is exposed at. | `GET` |
| `HeapDumpType` | Sets the type of heap dump to capture. | `Full` |

The `HeapDumpType` setting is supported on Linux and Windows only.
Acceptable values are `Normal`, `WithHeap`, `Triage`, and `Full`.
On macOS, this setting is ignored and a gcdump is always captured.

## Enable HTTP Access

The URL path to the endpoint is computed by combining the global `Management:Endpoints:Path` setting together with the `Path` setting described in the preceding section.
The default path is `/actuator/heapdump`.

See the [Exposing Endpoints](./using-endpoints.md#exposing-endpoints) and [HTTP Access](./using-endpoints.md#http-access) sections for the overall steps required to enable HTTP access to endpoints in an ASP.NET Core application.

To add the actuator to the service container and map its route, use the `AddHeapDumpActuator` extension method.

Add the following code to `Program.cs` to use the actuator endpoint:

```csharp
using Steeltoe.Management.Endpoint.Actuators.HeapDump;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHeapDumpActuator();
```

> [!TIP]
> It's recommended to use `AddAllActuators()` instead of adding individual actuators,
> which enables individually turning them on/off at runtime via configuration.
