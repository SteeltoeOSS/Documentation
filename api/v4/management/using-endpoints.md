# Using Endpoints

Steeltoe provides a basic set of HTTP endpoints (also known as actuators), which are implemented using ASP.NET Core middleware.

## Reference Materials

In this section, it is helpful to understand the following:

* How the [.NET Configuration System](https://learn.microsoft.com/aspnet/core/fundamentals/configuration) works.
* How the [the ASP.NET Core Startup](https://learn.microsoft.com/aspnet/core/fundamentals/startup) is used to register services and middleware.

## Endpoint Listing

The following table describes the available Steeltoe management endpoints that can be used in an application:

| ID|Description |
| --- | --- |
| [cloudfoundry](./cloud-foundry.md) | Enables the management endpoint integration with Cloud Foundry. |
| [dbmigrations](./dbmigrations.md) | Provides ability to see current and pending database migrations for an application data source. |
| [env](./env.md) | Reports the keys and values from the application's configuration. |
| [health](./health.md) | Customizable endpoint that gathers application health information. |
| [heapdump](./heapdump.md) | Generates and downloads a mini-dump of the application (Windows and Linux only). |
| [httpexchanges](./httpexchanges.md) | Gathers recently processed HTTP requests. |
| [hypermedia](./hypermedia.md) | Lists the active management endpoints and their links. |
| [info](./info.md) | Customizable endpoint that gathers arbitrary application information (such as app version). |
| [loggers](./loggers.md) | Gathers existing loggers and allows changing their minimum levels at runtime. |
| [mappings](./mappings.md) | Reports the configured ASP.NET routes and route templates. |
| [metrics](./metrics.md) | Reports the collected metrics for the application. |
| [prometheus](./prometheus.md) | Exposes metrics collected via built-in instrumentation of various aspects of the application in the Prometheus format. |
| [refresh](./refresh.md) | Triggers a reload of the application configuration. |
| [services](./services.md) | Lists the contents of the .NET dependency injection service container. |
| [threaddump](./threaddump.md)  | Generates and reports a snapshot of the application's threads (Windows only). |

Each endpoint has an associated ID. When you want to expose that endpoint over HTTP, that ID is used in the mapped URL that exposes the endpoint. For example, the `health` endpoint is mapped to `/actuator/health`.


## Add NuGet Reference

To use the management endpoints, you need to add a reference to the `Steeltoe.Management.Endpoints` NuGet package.

## Configure Global Settings

Endpoints can be configured using the [.NET Configuration System](https://learn.microsoft.com/aspnet/core/fundamentals/configuration). You can globally configure settings that apply to all endpoints, as well as configure settings that are specific to a particular endpoint.

All management endpoint settings should be placed under the configuration key prefix `Management:Endpoints`. Any settings found under this prefix apply to all endpoints globally.

Settings that you want to apply to specific endpoints should be placed under the configuration key prefix `Management:Endpoints:`, followed by the ID of the endpoint (for example, `Management:Endpoints:Health`). Any settings you apply to a specific endpoint override the settings applied globally. Subsequent pages describe the actuator-specific settings.

The following table describes the settings that you can apply globally:

| Key | Description | Default |
| --- | --- | --- |
| `Enabled` | Whether to enable management endpoints. | `true` |
| `Path` | The HTTP route prefix applied to all endpoints. | `/actuator` |
| `Port` | Expose management endpoints on an alternate HTTP port. | |
| `SslEnabled` | Whether `Port` applies to HTTP or HTTPS requests. | `false` |
| `UseStatusCodeFromResponse` | Reflect the actuator outcome in the HTTP response status code.  | `true` |
| `SerializerOptions` | Customize JSON serialization options. | use camelCase properties |
| `CustomJsonConverters` | Additional [`JsonConverter`](https://learn.microsoft.com/dotnet/standard/serialization/system-text-json/converters-how-to)s to use (see below). | |

> [!NOTE]
> When running an application in IIS or with the HWC buildpack, response body content is automatically filtered out when the HTTP response code is 503. Some actuator responses intentionally return a code of 503 in failure scenarios. Setting `UseStatusCodeFromResponse` to `false` will return status code 200 instead. This switch does not affect the status code of responses outside of Steeltoe.

### Custom JSON Serialization Options

The `JsonSerializerOptions` used to serialize actuator responses are configurable, and custom `JsonConverter`s can be used by adding the [assembly-qualified type](https://learn.microsoft.com/dotnet/api/system.type.assemblyqualifiedname").

For example, to pretty-print all JSON and serialize `DateTime` values as epoch times:

```json
{
  "Management": {
    "Endpoints": {
      "SerializerOptions": {
        "WriteIndented": true
      },
      "CustomJsonConverters": [
        "Steeltoe.Management.Endpoint.Actuators.Info.EpochSecondsDateTimeConverter"
      ]
    }
  }
}
```

## Exposing Endpoints

Since endpoints may contain sensitive information, only health and info are exposed by default. To change which endpoints are exposed, use the `Include` and `Exclude` properties:

| Property | Default |
| --- | --- |
| `Exposure:Include` | [`info`, `health`] |
| `Exposure:Exclude` | |

Each setting in the table above must be prefixed with `Management:Endpoints:Actuator`. Use the actuator ID to specify the endpoint.
To select all endpoints, `*` can be used. For example, to expose everything except `env` and `refresh`, use the following:

```json
"Management": {
    "Endpoints": {
        "Actuator":{
            "Exposure": {
                "Include": [ "*" ],
                "Exclude": [ "env", "refresh"]
            }
        }
    }
}
```

## HTTP Access

To expose any of the management endpoints over HTTP in an ASP.NET Core application:

1. Add a NuGet package reference to `Steeltoe.Management.Endpoint`.
1. Configure endpoint settings, as needed (typically in `appsettings.json`).
1. Add the actuator endpoint(s) to the host builder.
1. Optional: Add any additional "contributors" to the service container (for example, `builder.Services.AddHealthContributor<CustomHealthContributor>()` or `builder.Services.AddInfoContributor<CustomInfoContributor>()`).

> [!CAUTION]
> By default, actuator endpoints are exposed on the same host(s) and port(s) as the application (which can be configured as described [here](https://learn.microsoft.com/aspnet/core/fundamentals/servers/kestrel/endpoints?view=aspnetcore-8.0) and [here](https://andrewlock.net/8-ways-to-set-the-urls-for-an-aspnetcore-app/)).
> Use the `Port` and `SslEnabled` settings described above to isolate management endpoints from regular application endpoints.

The example below adds all actuators to the host builder:

```csharp
using Steeltoe.Management.Endpoint;

var builder = WebApplication.CreateBuilder(args);
builder.AddAllActuators();
```

Alternatively, individual actuators can be added:

```csharp
using Steeltoe.Management.Endpoint;

var builder = WebApplication.CreateBuilder(args);
builder.AddHypermediaActuator().AddLoggersActuator().AddRefreshActuator();
```

> [!NOTE]
> `AddAllActuators()` and `AddLoggingActuator()` automatically configure the [Dynamic Logging Provider](../logging/dynamic-logging-provider.md). To use the [Serilog Dynamic Logger](../logging/serilog-logger.md), be sure to do so *before* adding actuators. For example:
> ```csharp
> using Steeltoe.Logging.DynamicSerilog;
> using Steeltoe.Management.Endpoint;
> 
> var builder = WebApplication.CreateBuilder(args);
> builder.Logging.AddDynamicSerilog();
> builder.AddAllActuators();
> ```

## Securing Endpoints

Endpoints can be customized with `IEndpointConventionBuilder`. This allows calling `RequireAuthorization()` to run Authorization middleware on them.

When using the host builder extensions, it can be added in the following way:

```csharp
using Steeltoe.Management.Endpoint;

var builder = WebApplication.CreateBuilder(args);
builder.AddAllActuators(endpointConventionBuilder => endpointConventionBuilder.RequireAuthorization());
```

For the `IEndpointRouteBuilder` extensions, it can be added as shown:

```csharp
app.MapAllActuators().RequireAuthorization();
```

When called without arguments, the default profile is used. Other overloads allow passing a profile or a profile name.
