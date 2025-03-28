# Services

The Steeltoe Services endpoint returns the services registered in the application's dependency container.

## Configure Settings

The following table describes the configuration settings that you can apply to the endpoint.
Each key must be prefixed with `Management:Endpoints:Services:`.

| Key | Description | Default |
| --- | ----------- | ------- |
| `Enabled` | Whether the endpoint is enabled | `true` |
| `ID` | The unique ID of the endpoint | `beans` |
| `Path` | The relative path at which the endpoint is exposed | same as `ID` |
| `RequiredPermissions` | Permissions required to access the endpoint when running on Cloud Foundry | `Restricted` |
| `AllowedVerbs` | An array of HTTP verbs at which the endpoint is exposed | `GET` |

> [!NOTE]
> The `ID` of this endpoint defaults to `beans` for compatibility with the [beans actuator from Spring](https://docs.spring.io/spring-boot/api/rest/actuator/beans.html).

## Enable HTTP Access

The URL path to the endpoint is computed by combining the global `Management:Endpoints:Path` setting with the `Path` setting described in the preceding section.
The default path is `/actuator/beans`.

See the [Exposing Endpoints](./using-endpoints.md#exposing-endpoints) and [HTTP Access](./using-endpoints.md#http-access) sections for the steps required to enable HTTP access to endpoints in an ASP.NET Core application.

To add the actuator to the service container and map its route, use the `AddServicesActuator` extension method.

Add the following code to `Program.cs` to use the actuator endpoint:

```csharp
using Steeltoe.Management.Endpoint.Actuators.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddServicesActuator();
```

> [!TIP]
> It is recommended that you use `AddAllActuators()` instead of adding individual actuators;
> this enables individually turning them on/off at runtime via configuration.

## Sample Output

This endpoint returns the contents of `IServiceCollection`.

The response is always returned as JSON, like this:

```json
{
  "contexts": {
    "application": {
      "beans": {
        "Microsoft.Extensions.Options.IOptions`1": {
          "scope": "Singleton",
          "type": "Microsoft.Extensions.Options.UnnamedOptionsManager`1[TOptions]",
          "resource": "Microsoft.Extensions.Options, Version=8.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60",
          "dependencies": [
            "Microsoft.Extensions.Options.IOptionsFactory`1[TOptions]"
          ]
        },
        "Microsoft.Extensions.Options.IOptionsSnapshot`1": {
          "scope": "Scoped",
          "type": "Microsoft.Extensions.Options.OptionsManager`1[TOptions]",
          "resource": "Microsoft.Extensions.Options, Version=8.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60",
          "dependencies": [
            "Microsoft.Extensions.Options.IOptionsFactory`1[TOptions]"
          ]
        },
        "Microsoft.Extensions.Options.IOptionsMonitor`1": {
          "scope": "Singleton",
          "type": "Microsoft.Extensions.Options.OptionsMonitor`1[TOptions]",
          "resource": "Microsoft.Extensions.Options, Version=8.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60",
          "dependencies": [
            "Microsoft.Extensions.Options.IOptionsFactory`1[TOptions]",
            "System.Collections.Generic.IEnumerable`1[Microsoft.Extensions.Options.IOptionsChangeTokenSource`1[TOptions]]",
            "Microsoft.Extensions.Options.IOptionsMonitorCache`1[TOptions]"
          ]
        }
      }
    }
  }
}
```
