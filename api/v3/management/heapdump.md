# Heap Dump

You can use the Steeltoe heap dump endpoint to generate and download a mini-dump of your application. The mini-dump can then be read into Visual Studio for analysis.

## Configure Settings

The following table describes the settings that you can apply to the endpoint:

| Key | Description | Default |
| --- | --- | --- |
| `Id` | The ID of the heap dump endpoint. | `heapdump` |
| `Enabled` | Whether to enable the heap dump management endpoint. | `true` |
| `Sensitive` | Currently not used. | `false` |

>NOTE: Each setting above must be prefixed with `Management:Endpoints:Heapdump`.

## Enable HTTP Access

The default path to the heap dump endpoint is computed by combining the global `Path` prefix setting together with the `Id` setting described in the previous section. The default path is `/actuator/heapdump`.

See the [HTTP Access](./using-endpoints.html#http-access) section to see the overall steps required to enable HTTP access to endpoints in an ASP.NET Core application.

To add the actuator to the service container and map its route, you can use the `AddHeapDumpActuator` extension method from `ManagementHostBuilderExtensions`.

Alternatively, first,  add the heap dump actuator to the service container, use the `AddHeapDumpActuator()` extension method from `EndpointServiceCollectionExtensions`.

Then, add the heap dump actuator middleware to the ASP.NET Core pipeline, using the `Map<HeapDumpEndpoint>()` extension method from `ActuatorRouteBuilderExtensions`.
