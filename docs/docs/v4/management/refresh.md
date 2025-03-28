# Refresh

You can use the Steeltoe Refresh endpoint to reload the application's configuration and return the new keys currently in use.
The endpoint reloads the configuration using `IConfigurationRoot.Reload()`.

## Configure Settings

The following table describes the configuration settings that you can apply to the endpoint.
Each key must be prefixed with `Management:Endpoints:Refresh:`.

| Key | Description | Default |
| --- | --- | --- |
| `Enabled` | Whether the endpoint is enabled. | `true` |
| `ID` | The unique ID of the endpoint. | `refresh` |
| `Path` | The relative path at which the endpoint is exposed. | same as `ID` |
| `RequiredPermissions` | Permissions required to access the endpoint, when running on Cloud Foundry. | `Restricted` |
| `AllowedVerbs` | An array of HTTP verbs the endpoint is exposed at. | `POST` |
| `ReturnConfiguration` | Whether to return the configuration after refresh. | `true` |

> [!NOTE]
> Despite being *possible* to configure this endpoint to respond to `GET` requests,
> this is discouraged because it is not a [Safe HTTP Method](https://developer.mozilla.org/en-US/docs/Glossary/Safe/HTTP).

## Enable HTTP Access

The URL path to the endpoint is computed by combining the global `Management:Endpoints:Path` setting together with the `Path` setting described in the preceding section.
The default path is `/actuator/refresh`.

See the [Exposing Endpoints](./using-endpoints.md#exposing-endpoints) and [HTTP Access](./using-endpoints.md#http-access) sections for the overall steps required to enable HTTP access to endpoints in an ASP.NET Core application.

To add the actuator to the service container and map its route, use the `AddRefreshActuator` extension method.

Add the following code to `Program.cs` to use the actuator endpoint:

```csharp
using Steeltoe.Management.Endpoint.Actuators.Refresh;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRefreshActuator();
```

> [!TIP]
> It's recommended to use `AddAllActuators()` instead of adding individual actuators,
> which enables individually turning them on/off at runtime via configuration.

## Sample Output

This endpoint returns an array of keys obtained from `IConfiguration`.

The response will always be returned as JSON, like this:

```json
[
  "Management",
  "Management:Endpoints",
  "Management:Endpoints:Actuator",
  "Management:Endpoints:Actuator:Exposure",
  "Management:Endpoints:Actuator:Exposure:Include",
  "Management:Endpoints:Actuator:Exposure:Include:0",
]
```

> [!NOTE]
> If `ReturnConfiguration` is set to `false`, an empty array is returned.
