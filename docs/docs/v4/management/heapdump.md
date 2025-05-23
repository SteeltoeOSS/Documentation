# Heap Dump

You can use the Steeltoe Heap Dump endpoint to capture and download a memory dump of your application.
Depending on the type of dump and the operating system it was captured on, it can be analyzed in various tools, such as Visual Studio and [PerfView](https://github.com/microsoft/perfview).

> [!NOTE]
> The logic used by this endpoint is similar to the [dotnet-dump](https://learn.microsoft.com/dotnet/core/diagnostics/dotnet-dump) and [dotnet-gcdump](https://learn.microsoft.com/dotnet/core/diagnostics/dotnet-gcdump) tools.

## Configure Settings

The following table describes the configuration settings that you can apply to the endpoint.
Each key must be prefixed with `Management:Endpoints:HeapDump:`.

| Key | Description | Default |
| --- | --- | --- |
| `Enabled` | Whether the endpoint is enabled | `true` |
| `ID` | The unique ID of the endpoint | `heapdump` |
| `Path` | The relative path at which the endpoint is exposed | same as `ID` |
| `RequiredPermissions` | Permissions required to access the endpoint, when running on Cloud Foundry | `Restricted` |
| `AllowedVerbs` | An array of HTTP verbs at which the endpoint is exposed | `GET` |
| `HeapDumpType` | Sets the type of memory dump to capture | `Full`, `GCDump` on macOS |

The following table lists the memory dump types.
All types are supported on Windows, Linux, and macOS.

| HeapDumpType | Description |
| --- | --- |
| `Full` | The largest dump containing all memory including the module images. |
| `Heap` | A large and relatively comprehensive dump containing module lists, thread lists, all stacks, exception information, handle information, and all memory except for mapped images. |
| `Mini` | A small dump containing module lists, thread lists, exception information, and all stacks. |
| `Triage` | A small dump containing module lists, thread lists, exception information, all stacks and PII removed. |
| `GCDump` | A Garbage Collector dump, created by triggering a GC, turning on special events, and regenerating the graph of object roots from the event stream. |

## Enable HTTP Access

The URL path to the endpoint is computed by combining the global `Management:Endpoints:Path` setting with the `Path` setting described in the preceding section.
The default path is `/actuator/heapdump`.

See the [Exposing Endpoints](./using-endpoints.md#exposing-endpoints) and [HTTP Access](./using-endpoints.md#http-access) sections for the steps required to enable HTTP access to endpoints in an ASP.NET Core application.

To add the actuator to the service container and map its route, use the `AddHeapDumpActuator` extension method.

Add the following code to `Program.cs` to use the actuator endpoint:

```csharp
using Steeltoe.Management.Endpoint.Actuators.HeapDump;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHeapDumpActuator();
```

> [!TIP]
> It is recommended that you use `AddAllActuators()` instead of adding individual actuators;
> this enables individually turning them on/off at runtime via configuration.
