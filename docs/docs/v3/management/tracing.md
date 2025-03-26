# Tracing

The Steeltoe tracing endpoint provides the ability to view the last several requests made of your application.

When you activate the tracing endpoint, an `ITraceRepository` implementation is configured and created to hold `Trace` information that can be retrieved by using the endpoint.

## Configure Settings

The following table describes the settings that you can apply to the endpoint:

| Key | Description | Default |
| --- | --- | --- |
| `Id` | The ID of the trace endpoint. | `trace` |
| `Enabled` | Enable or disable trace management endpoint. | `true` |
| `Sensitive` | Currently not used. | `false` |
| `RequiredPermissions` | User permissions required on Cloud Foundry to access endpoint. | `RESTRICTED` |
| `Capacity` | Size of the circular buffer of traces. | 100 |
| `AddRequestHeaders` | Add request headers. | `true` |
| `AddResponseHeaders` | Add response headers. | `true` |
| `AddPathInfo` | Add path information. | `false` |
| `AddUserPrincipal` | Add user principal. | `false` |
| `AddParameters` | Add request parameters. | `false` |
| `AddQueryString` | Add query string. | `false` |
| `AddAuthType` | Add authentication type. | `false` |
| `AddRemoteAddress` | Add remote address of user. | `false` |
| `AddSessionId` | Add session id. | `false` |
| `AddTimeTaken` | Add time take. | `true` |

>Each setting must be prefixed with `Management:Endpoints:Trace`.

## Enable HTTP Access

The default path to the trace endpoint is computed by combining the global `Path` prefix setting together with the `Id` setting described in the preceding section. The default path is <[Context-Path](./hypermedia.md#base-context-path)>`/trace`.

See the [HTTP Access](./using-endpoints.md#http-access) section to see the overall steps required to enable HTTP access to endpoints in an ASP.NET Core application.

To add the actuator to the service container and map its route, use the `AddTraceActuator` extension method from `ManagementHostBuilderExtensions`.

Alternatively, first, add the trace actuator to the service container, using the `AddTraceActuator()` extension method from `EndpointServiceCollectionExtensions`.

Then, add the trace actuator middleware to the ASP.NET Core pipeline, using the `Map<TraceEndpoint>()` extension method from `ActuatorRouteBuilderExtensions`.
