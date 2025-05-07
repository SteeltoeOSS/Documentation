# Hypermedia

The purpose of this endpoint is to list the available management endpoints configured in your application.
It returns their IDs and the links to them in JSON format.

## Configure Settings

The following table describes the configuration settings that you can apply to the endpoint.
Each key must be prefixed with `Management:Endpoints:Actuator:`. Note this key differs from the convention used by other actuators.

| Key | Description | Default |
| --- | ----------- | ------- |
| `Enabled` | Whether the endpoint is enabled | `true` |
| `ID` | The unique ID of the endpoint | `""` |
| `Path` | The relative path at which the endpoint is exposed | same as `ID` |
| `RequiredPermissions` | Permissions required to access the endpoint when running on Cloud Foundry | `Restricted` |
| `AllowedVerbs` | An array of HTTP verbs at which the endpoint is exposed | `GET` |

> [!NOTE]
> This endpoint is exposed automatically because its ID is empty. To reference this actuator in exposure settings,
> start by configuring a non-empty ID. Because the Path is the same as ID unless otherwise specified, set it to empty explicitly:
> ```json
> {
>   "Management": {
>     "Endpoints": {
>       "Actuator": {
>         "Id": "hypermedia",
>         "Path": "",
>         "Exposure": {
>           "Exclude": [ "hypermedia" ]
>         }
>       }
>     }
>   }
> }
> ```

## Enable HTTP Access

The URL path to the endpoint is computed by combining the global `Management:Endpoints:Path` setting with the `Path` setting described in the preceding section.
The default path is `/actuator`.

> [!IMPORTANT]
> When running on Cloud Foundry, the [Cloud Foundry Actuator](./cloud-foundry.md) should be used instead.
> Its default path is `/cloudfoundryapplication`.

See the [Exposing Endpoints](./using-endpoints.md#exposing-endpoints) and [HTTP Access](./using-endpoints.md#http-access) sections for the steps required to enable HTTP access to endpoints in an ASP.NET Core application.

To add the actuator to the service container and map its route, use the `AddHypermediaActuator` extension method.

Add the following code to `Program.cs` to use the actuator endpoint:

```csharp
using Steeltoe.Management.Endpoint.Actuators.Hypermedia;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHypermediaActuator();
```

> [!TIP]
> It is recommended that you use `AddAllActuators()` instead of adding individual actuators;
> this enables individually turning them on/off at runtime via configuration.

## Sample Output

This endpoint returns a list of management endpoints, including itself.

The response is always returned as JSON:

```json
{
  "type": "steeltoe",
  "_links": {
    "info": {
      "href": "https://localhost:7105/actuator/info",
      "templated": false
    },
    "health": {
      "href": "https://localhost:7105/actuator/health",
      "templated": false
    },
    "self": {
      "href": "https://localhost:7105/actuator",
      "templated": false
    }
  }
}
```
