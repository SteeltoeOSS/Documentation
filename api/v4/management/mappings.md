# Mappings

You can use the Steeltoe mappings endpoint to return the MVC and WebAPI routes and route templates that are used by the application.

## Configure Settings

The following table describes the settings that you can apply to the endpoint:

| Key | Description | Default |
| --- | --- | --- |
| `Id` | The ID of the mappings endpoint. | `mappings` |
| `Enabled` | Whether to enable the mappings management endpoint. | `true` |

>Each setting above must be prefixed with `Management:Endpoints:Mappings`.

## Enable HTTP Access

The default path to the mappings endpoint is computed by combining the global `Path` prefix setting together with the `Id` setting described in the preceding section. The default path is `/actuator/mappings`.

See the [HTTP Access](./using-endpoints.md#http-access) section to see the overall steps required to enable HTTP access to endpoints in an ASP.NET Core application.

To add the actuator to the service container and map its route, use the `hostBuilder.AddMappingsActuator` extension method from `ManagementHostBuilderExtensions`.

Alternatively, first, add the mappings actuator to the service container, use the `AddMappingsActuator()` extension method from `EndpointServiceCollectionExtensions`.

Then add the mappings actuator middleware to the ASP.NET Core pipeline, using the `Map<MappingsEndpoint>()` extension method from `ActuatorRouteBuilderExtensions`.
