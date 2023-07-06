# Refresh

You can use the Steeltoe refresh endpoint to cause the application's configuration to be reloaded and return the new values and keys currently in use in your application. The endpoint reloads the configuration by using the application's `IConfigurationRoot`.

## Configure Settings

The following table describes the settings that you can apply to the endpoint:

| Key | Description | Default |
| --- | --- | --- |
| `Id` | The ID of the refresh endpoint. | `refresh` |
| `Enabled` | Whether to enable the refresh management endpoint. | `true` |

>Each setting must be prefixed with `Management:Endpoints:Refresh`.

## Enable HTTP Access

The default path to the refresh endpoint is computed by combining the global `Path` prefix setting together with the `Id` setting described in the preceding section. The default path is `/actuator/refresh`.

See the [HTTP Access](./using-endpoints.md#http-access) section to see the overall steps required to enable HTTP access to endpoints in an ASP.NET Core application.

To add the actuator to the service container and map its route, use the `AddRefreshActuator` extension method from `ManagementHostBuilderExtensions`.

Alternatively, first, add the refresh actuator to the service container, using the `AddRefreshActuator()` extension method from `EndpointServiceCollectionExtensions`.

Then, add the refresh actuator middleware to the ASP.NET Core pipeline, use the `Map<RefreshEndpoint>()` extension method from `ActuatorRouteBuilderExtensions`.
