# Info

The Steeltoe Info endpoint exposes information about the running application, such as its version and the Steeltoe version in use.

Information is collected from all `IInfoContributor` implementations registered in the application.
Steeltoe includes a couple contributor implementations out of the box that you can use.
Also, and perhaps more importantly, you can write your own.

## Configure Settings

The following table describes the configuration settings that you can apply to the endpoint.
Each key must be prefixed with `Management:Endpoints:Info:`.

| Key | Description | Default |
| --- | --- | --- |
| `Enabled` | Whether the endpoint is enabled. | `true` |
| `ID` | The unique ID of the endpoint. | `info` |
| `Path` | The relative path at which the endpoint is exposed. | same as `ID` |
| `RequiredPermissions` | Permissions required to access the endpoint, when running on Cloud Foundry. | `Restricted` |
| `AllowedVerbs` | An array of HTTP verbs the endpoint is exposed at. | `GET` |

## Enable HTTP Access

The URL path to the endpoint is computed by combining the global `Management:Endpoints:Path` setting together with the `Path` setting described in the preceding section.
The default path is `/actuator/info`.

See the [Exposing Endpoints](./using-endpoints.md#exposing-endpoints) and [HTTP Access](./using-endpoints.md#http-access) sections for the overall steps required to enable HTTP access to endpoints in an ASP.NET Core application.

To add the actuator to the service container and map its route, use the `AddInfoActuator` extension method.

Add the following code to `Program.cs` to use the actuator endpoint:

```csharp
using Steeltoe.Management.Endpoint.Actuators.Info;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddInfoActuator();
```

> [!TIP]
> It's recommended to use `AddAllActuators()` instead of adding individual actuators,
> which enables individually turning them on/off at runtime via configuration.

## Built-in Contributors

### Build info

This contributor exposes file/version info for both the application and the included version of Steeltoe.

### Configuration

This contributor exposes any values below the `Info` configuration key. For example:
```json
{
  "Info": {
    "Some": {
      "Example": {
        "Key": "some-example-value"
      }
    }
  }
}
```

> [!TIP]
> When combined with the [Placeholder Configuration Provider](../configuration/placeholder-provider.md),
> compound configuration values can be exposed originating from other places in configuration.

### Git properties

Exposes information from the `git.properties` Spring Boot file, if available.
Shows information from git, such as branch/tag name, commit hash, and remote.

> [!TIP]
> For an example of how to use this contributor within MSBuild using [GitInfo](https://github.com/devlooped/GitInfo), see the [Steeltoe Management sample](https://github.com/SteeltoeOSS/Samples/tree/main/Management/src).

## Sample Output

Depending on the registered contributors, this endpoint returns JSON such as this:

```json
{
  "git": {
    "branch": "main",
    "build": {
      "host": "examplehost",
      "time": "2024-10-11T18:44:28.9255701Z",
      "user": {
        "email": "user@email.com",
        "name": "testuser"
      },
      "version": "2.1.0"
    },
    "commit": {
      "id": "90d0870a363fafcb50981b7038608b763e527e05",
      "time": "2024-10-08T17:30:57Z"
    },
    "remote": {
      "origin": {
        "url": "https://github.com/SteeltoeOSS/Samples"
      }
    },
    "tags": "2.1.0-644-g90d0870a"
  },
  "Some": {
    "Example": {
      "Key": "some-example-value"
    }
  },
  "applicationVersionInfo": {
    "ProductName": "ExampleApp",
    "FileVersion": "1.0.0.0",
    "ProductVersion": "1.0.0+df774c38b734857909d54b796fffbb717eced4a4"
  },
  "steeltoeVersionInfo": {
    "ProductName": "Steeltoe.Management.Endpoint",
    "FileVersion": "4.0.519.27703",
    "ProductVersion": "4.0.519-alpha+6c377e2ac3"
  },
  "build": {
    "version": "1.0.0.0"
  }
}
```

## Custom Contributors

If you wish to provide custom information for your application, create a class that implements the `IInfoContributor` interface and then add it to the service container.

The following example contributor adds the local server time:

```csharp
using Steeltoe.Management.Endpoint.Actuators.Info;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddInfoActuator();
builder.Services.AddInfoContributor<ServerTimeInfoContributor>();

public class ServerTimeInfoContributor : IInfoContributor
{
    public Task ContributeAsync(InfoBuilder builder, CancellationToken cancellationToken)
    {
        builder.WithInfo("server-time", DateTime.Now.ToString("O"));
        return Task.CompletedTask;
    }
}
```

Which returns the following JSON fragment in the response:

```json
{
  "server-time": "2024-11-01T17:03:05.3490351+01:00"
}
```
