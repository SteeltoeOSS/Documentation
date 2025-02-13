# Loggers

The Steeltoe Loggers endpoint provides the ability to view and change the minimum logging levels in your application at runtime by using Steeltoe dynamic logging.

You can view a list of all active loggers in an application and their current minimum levels. Each entry contains the effective minimum level, and optionally the original minimum level if overridden.

## Configure Settings

The following table describes the configuration settings that you can apply to the endpoint.
Each key must be prefixed with `Management:Endpoints:Loggers:`.

| Key | Description | Default |
| --- | --- | --- |
| `Enabled` | Whether the endpoint is enabled. | `true` |
| `ID` | The unique ID of the endpoint. | `loggers` |
| `Path` | The relative path at which the endpoint is exposed. | same as `ID` |
| `RequiredPermissions` | Permissions required to access the endpoint, when running on Cloud Foundry. | `Restricted` |
| `AllowedVerbs` | An array of HTTP verbs the endpoint is exposed at. | `GET`, `POST` |

## Enable HTTP Access

The URL path to the endpoint is computed by combining the global `Management:Endpoints:Path` setting together with the `Path` setting described in the preceding section.
The default path is `/actuator/loggers`.

See the [Exposing Endpoints](./using-endpoints.md#exposing-endpoints) and [HTTP Access](./using-endpoints.md#http-access) sections for the overall steps required to enable HTTP access to endpoints in an ASP.NET Core application.

To add the actuator to the service container and map its route, use the `AddInfoActuator` extension method.

Add the following code to `Program.cs` to use the actuator endpoint:

```csharp
using Steeltoe.Management.Endpoint.Actuators.Loggers;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddLoggersActuator();
```

> [!TIP]
> It's recommended to use `AddAllActuators()` instead of adding individual actuators,
> which enables individually turning them on/off at runtime via configuration.

> [!TIP]
> When `AddLoggersActuator` is called, it tries to register [Dynamic Console Logging](../logging/dynamic-console-logging.md).
> If you'd like to use Serilog instead, call `AddDynamicSerilog` *before* `AddLoggersActuator`.

## Retrieving Minimum Log Levels

The response below shows the loggers and their minimum levels for the following `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Steeltoe.Management": "Error"
    }
  }
}
```

To retrieve the currently active loggers and their minimum log levels, send an HTTP `GET` request to `/actuator/loggers`,
which returns the response below:

```json
{
  "levels": [
    "OFF",
    "FATAL",
    "ERROR",
    "WARN",
    "INFO",
    "DEBUG",
    "TRACE"
  ],
  "loggers": {
    "Default": {
      "effectiveLevel": "INFO"
    },
    "Microsoft": {
      "effectiveLevel": "INFO"
    },
    "Microsoft.AspNetCore": {
      "effectiveLevel": "WARN"
    },
    "Microsoft.AspNetCore.Cors": {
      "effectiveLevel": "WARN"
    },
    "Microsoft.Extensions.Hosting": {
      "effectiveLevel": "INFO"
    },
    "Steeltoe": {
      "effectiveLevel": "INFO"
    },
    "Steeltoe.Management": {
      "effectiveLevel": "ERROR"
    },
    "Steeltoe.Management.Endpoint": {
      "effectiveLevel": "ERROR"
    }
  }
}
```

The response above shows only effective levels, because there are no runtime overrides yet.

## Modifying Minimum Log Levels

Minimum log levels can be changed at runtime by adding the category to the URL of a `POST` request and the minimum level to assign in the request body.

The category is the fully-qualified name of the logger, which works like a prefix.
For example, changing the minimum log level of `Microsoft.AspNetCore` will affect all loggers whose category starts with
`Microsoft.AspNetCore`, such as `Microsoft.AspNetCore` and `Microsoft.AspNetCore.Cors` (but not `Microsoft.AspNetCoreExtra`).

> [!NOTE]
> [Spring Boot Admin](https://www.baeldung.com/spring-boot-changing-log-level-at-runtime) and [Tanzu Apps Manager](https://techdocs.broadcom.com/us/en/vmware-tanzu/platform/tanzu-platform-for-cloud-foundry/4-0/tpcf/using-actuators.html#manage-log-levels)
> provide a UI to change the minimum levels at runtime. For compatibility, the level names used by this actuator differ from the names used by .NET.
>
> | Actuator level | .NET level |
> | --- | --- |
> | `OFF` | `LogLevel.None` |
> | `FATAL` | `LogLevel.Critical` |
> | `ERROR` | `LogLevel.Error` |
> | `WARN` | `LogLevel.Warning` |
> | `INFO` | `LogLevel.Information` |
> | `DEBUG` | `LogLevel.Debug` |
> | `TRACE` | `LogLevel.Trace` |

Using the `appsettings.json` from above, change the minimum log level of the `Steeltoe.Management` category to `TRACE`
by sending a POST request to `/actuator/loggers/Steeltoe.Management` with the following request body:

```json
{
  "configuredLevel": "TRACE"
}
```

> [!TIP]
> Because the logger category is part of the request URL, avoid using colons in the name to prevent invalid HTTP requests.

To verify the change was applied, send a `GET` request to `/actuator/loggers` to see the updated loggers.
The response below shows that the `Steeltoe.Management` logger (and its descendants) now use a minimum level of `TRACE`.
Furthermore, the `Steeltoe.Management` entry contains both `configuredLevel` and `effectiveLevel`, which means the level can be reset.

```json
{
  "levels": [
    "OFF",
    "FATAL",
    "ERROR",
    "WARN",
    "INFO",
    "DEBUG",
    "TRACE"
  ],
  "loggers": {
    "Default": {
      "effectiveLevel": "INFO"
    },
    "Microsoft": {
      "effectiveLevel": "INFO"
    },
    "Microsoft.AspNetCore": {
      "effectiveLevel": "WARN"
    },
    "Microsoft.AspNetCore.Cors": {
      "effectiveLevel": "WARN"
    },
    "Microsoft.Extensions.Hosting": {
      "effectiveLevel": "INFO"
    },
    "Steeltoe": {
      "effectiveLevel": "INFO"
    },
    "Steeltoe.Management": {
      "configuredLevel": "ERROR",
      "effectiveLevel": "TRACE"
    },
    "Steeltoe.Management.Endpoint": {
      "effectiveLevel": "TRACE"
    }
  }
}
```

## Resettings Minimum Log Levels

To revert back to the original minimum level, send another `POST` request to `/actuator/loggers/Steeltoe.Management`, but with the following request body:

```json
{
  "configuredLevel": null
}
```

To verify the change was applied, send a `GET` request to `/actuator/loggers`, which returns the same JSON response as before overriding the minimum levels.

## Configuration Changes

Dynamic log levels in Steeltoe are implemented by wrapping an existing `ILoggerProvider` and intercepting calls to its `CreateLogger` method.
The returned `ILogger` instances are wrapped as well, so that the minimum level can be changed at runtime.

When changes to `appsettings.json` are saved, existing loggers are updated with the new minimum levels, unless overridden.
A reset of an overridden logger reverts to the updated level from `appsettings.json`.

> [!TIP]
> The Steeltoe dynamic logging provider can be combined with `BootstrapLoggerFactory` to upgrade loggers after startup.
