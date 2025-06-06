# Route Mappings

You can use the Steeltoe Route Mappings actuator to retrieve the HTTP endpoints in your app that originate from ASP.NET Minimal APIs, API/MVC Controllers, Razor Pages, and Blazor.

## Configure Settings

The following table describes the configuration settings that you can apply to the endpoint.
Each key must be prefixed with `Management:Endpoints:Mappings:`.

| Key | Description | Default |
| --- | --- | --- |
| `Enabled` | Whether the endpoint is enabled | `true` |
| `ID` | The unique ID of the endpoint | `mappings` |
| `Path` | The relative path at which the endpoint is exposed | same as `ID` |
| `RequiredPermissions` | Permissions required to access the endpoint when running on Cloud Foundry | `Restricted` |
| `AllowedVerbs` | An array of HTTP verbs at which the endpoint is exposed | `GET` |
| `IncludeActuators` | Whether to include actuator endpoints in the response | `true` |

## Enable HTTP Access

The URL path to the endpoint is computed by combining the global `Management:Endpoints:Path` setting with the `Path` setting described in the preceding section.
The default path is `/actuator/mappings`.

See the [Exposing Endpoints](./using-endpoints.md#exposing-endpoints) and [HTTP Access](./using-endpoints.md#http-access) sections for the steps required to enable HTTP access to endpoints in an ASP.NET Core application.

To add the actuator to the service container and map its route, use the `AddRouteMappingsActuator` extension method.

Add the following code to `Program.cs` to use the actuator endpoint:

```csharp
using Steeltoe.Management.Endpoint.Actuators.RouteMappings;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRouteMappingsActuator();
```

> [!TIP]
> It is recommended that you use `AddAllActuators()` instead of adding individual actuators;
> this enables individually turning them on/off at runtime via configuration.

## Sample Output

This endpoint returns a list of route mappings and their parameters.

The response is always returned as JSON:

```json
{
  "contexts": {
    "application": {
      "mappings": {
        "dispatcherServlets": {
          "dispatcherServlet": [
            {
              "handler": "SampleApp.Controllers.WeatherForecastController.Get (SampleApp)",
              "predicate": "{GET [WeatherForecast], produces [text/plain || application/json || text/json]}",
              "details": {
                "handlerMethod": {
                  "className": "SampleApp.Controllers.WeatherForecastController",
                  "name": "Get",
                  "descriptor": "System.Collections.Generic.IEnumerable`1[SampleApp.WeatherForecast] Get()"
                },
                "requestMappingConditions": {
                  "patterns": [
                    "WeatherForecast"
                  ],
                  "methods": [
                    "GET"
                  ],
                  "consumes": [],
                  "produces": [
                    {
                      "mediaType": "text/plain",
                      "negated": false
                    },
                    {
                      "mediaType": "application/json",
                      "negated": false
                    },
                    {
                      "mediaType": "text/json",
                      "negated": false
                    }
                  ],
                  "headers": [],
                  "params": []
                }
              }
            }
          ]
        }
      }
    }
  }
}
```
