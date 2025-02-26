# Environment

The Steeltoe Environment endpoint can be used to query the configuration providers and their keys and values currently in use in your application. The endpoint retrieves this information from the application's `IConfiguration`.

## Configure Settings

The following table describes the configuration settings that you can apply to the endpoint.
Each key must be prefixed with `Management:Endpoints:Env:`.

| Key | Description | Default |
| --- | --- | --- |
| `Enabled` | Whether the endpoint is enabled. | `true` |
| `ID` | The unique ID of the endpoint. | `env` |
| `Path` | The relative path at which the endpoint is exposed. | same as `ID` |
| `RequiredPermissions` | Permissions required to access the endpoint, when running on Cloud Foundry. | `Restricted` |
| `AllowedVerbs` | An array of HTTP verbs the endpoint is exposed at. | `GET` |
| `KeysToSanitize` | An array of keys to sanitize. [^1] | `[ "password", "secret", "key", "token", ".*credentials.*", "vcap_services" ]` |

[^1]: A key can be a simple string that the property must end with, or a regular expression. A case-insensitive match is always performed. Use a single-element empty string to disable sanitization.

## Enable HTTP Access

The URL path to the endpoint is computed by combining the global `Management:Endpoints:Path` setting together with the `Path` setting described in the preceding section.
The default path is `/actuator/env`.

See the [Exposing Endpoints](./using-endpoints.md#exposing-endpoints) and [HTTP Access](./using-endpoints.md#http-access) sections for the overall steps required to enable HTTP access to endpoints in an ASP.NET Core application.

To add the actuator to the service container and map its route, use the `AddEnvironmentActuator` extension method.

Add the following code to `Program.cs` to use the actuator endpoint:

```csharp
using Steeltoe.Management.Endpoint.Actuators.Environment;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEnvironmentActuator();
```

> [!TIP]
> It's recommended to use `AddAllActuators()` instead of adding individual actuators,
> which enables individually turning them on/off at runtime via configuration.

## Sample Output

This endpoint returns a list of objects representing information from `IConfiguration`.

The response will always be returned as JSON, like this:

```json
{
  "activeProfiles": [
    "Development"
  ],
  "propertySources": [
    {
      "name": "JsonConfigurationProvider: [appsettings.json]",
      "properties": {
        "AllowedHosts": {
          "value": "*"
        },
        "Logging:LogLevel:Default": {
          "value": "Information"
        },
        "Logging:LogLevel:Microsoft.AspNetCore": {
          "value": "Warning"
        }
      }
    }
  ]
}
```
