# Using Endpoints

Steeltoe provides a basic set of HTTP endpoints (also known as actuators), which are implemented using ASP.NET Core middleware.

## Reference Materials

In this section, it is helpful to understand the following:

* How the [.NET Configuration System](https://learn.microsoft.com/aspnet/core/fundamentals/configuration) works.
* How the [ASP.NET Core Startup](https://learn.microsoft.com/aspnet/core/fundamentals/startup) is used to register services and middleware.

## Endpoint Listing

The following table describes the available Steeltoe management endpoints that can be used in an application:

| ID|Description |
| --- | --- |
| [cloudfoundry](./cloud-foundry.md) | Enables the management endpoint integration with Cloud Foundry. |
| [dbmigrations](./dbmigrations.md) | Provides the ability to see current and pending database migrations for an application data source. |
| [env](./env.md) | Reports the keys and values from the application's configuration. |
| [health](./health.md) | Customizable endpoint that gathers application health information. |
| [heapdump](./heapdump.md) | Generates and downloads a mini-dump of the application (Windows and Linux only). |
| [httpexchanges](./httpexchanges.md) | Gathers recently processed HTTP requests. |
| [hypermedia](./hypermedia.md) | Lists the active management endpoints and their links. |
| [info](./info.md) | Customizable endpoint that gathers arbitrary application information (such as app version). |
| [loggers](./loggers.md) | Gathers existing logger categories and allows changing their minimum levels at runtime. |
| [mappings](./mappings.md) | Reports the configured ASP.NET routes and route templates. |
| [prometheus](./prometheus.md) | Exposes metrics collected via built-in instrumentation of various aspects of the application in the Prometheus format. |
| [refresh](./refresh.md) | Triggers a reload of the application configuration. |
| [services](./services.md) | Lists the contents of the .NET dependency injection service container. |
| [threaddump](./threaddump.md)  | Generates and reports a snapshot of the application's threads (Windows only). |

Each endpoint has an associated ID. When you want to expose an endpoint over HTTP, its ID is used in the mapped URL that exposes the endpoint. For example, the `health` endpoint is mapped to `/actuator/health`.

## Add NuGet Reference

To use the management endpoints, you need to add a reference to the `Steeltoe.Management.Endpoints` NuGet package.

## Configure Global Settings

Endpoints can be configured using the [.NET Configuration System](https://learn.microsoft.com/aspnet/core/fundamentals/configuration). You can globally configure settings that apply to all endpoints, as well as configure settings that are specific to a particular endpoint.

All management endpoint settings should be placed under the configuration key prefix `Management:Endpoints`. Any settings found under this prefix apply to all endpoints globally.

Settings that you want to apply to specific endpoints should be placed under the configuration key prefix `Management:Endpoints:<ID>`, where `<ID>` is the ID of the endpoint (for example, `Management:Endpoints:Health`). Any settings you apply to a specific endpoint override the configuration settings applied globally.

The following table describes the configuration settings that you can apply globally:

| Key | Description | Default |
| --- | --- | --- |
| `Enabled` | Whether to enable management endpoints. | `true` |
| `Path` | The HTTP route prefix applied to all endpoints. | `/actuator` |
| `Port` | Expose management endpoints on an alternate HTTP port. [^1] | |
| `SslEnabled` | Whether `Port` applies to HTTP or HTTPS requests. [^1] | `false` |
| `UseStatusCodeFromResponse` | Reflect the actuator outcome in the HTTP response status code.  | `true` |
| `SerializerOptions` | Customize JSON serialization options. | use camelCase properties |
| `CustomJsonConverters` | Additional [`JsonConverter`](https://learn.microsoft.com/dotnet/standard/serialization/system-text-json/converters-how-to)s to use (see below). | |

[^1]: Using an alternate port does not apply to `/cloudfoundryapplication` endpoints.

> [!NOTE]
> When running an application in IIS or with the HWC buildpack, response body content is automatically filtered out when the HTTP response code is 503. Some actuator responses intentionally return a code of 503 in failure scenarios. Setting `UseStatusCodeFromResponse` to `false` will return status code 200 instead. This switch does not affect the status code of responses outside of Steeltoe.

## Configure Endpoint-specific Settings

The following table describes the configuration settings that are common to all endpoint-specific settings:

| Key | Description | Default |
| --- | --- | --- |
| `Enabled` | Whether the endpoint is enabled. | `true` |
| `ID` | The unique ID of the endpoint. | |
| `Path` | The relative path at which the endpoint is exposed. | same as `ID` |
| `RequiredPermissions` | Permissions required to access the endpoint, when running on Cloud Foundry. | `Restricted` |
| `AllowedVerbs` | An array of HTTP verbs the endpoint is exposed at. | |

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

Since endpoints may contain sensitive information, only health and info are exposed by default. To change which endpoints are exposed, use the `Include` and `Exclude` properties.

| Property | Default |
| --- | --- |
| `Exposure:Include` | [`info`, `health`] |
| `Exposure:Exclude` | |

Each key in the table above must be prefixed with `Management:Endpoints:Actuator`. Use the actuator ID to specify the endpoint.
To expose all endpoints, `*` can be used. For example, to expose everything except `env` and `refresh`, use the following:

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

> [!NOTE]
> When running on Cloud Foundry, exposure settings *only* have an effect on requests starting with `/actuator`.
> They are ignored for requests starting with `/cloudfoundryapplication`, where access control is [handled differently](./cloud-foundry.md#security).
> Individual endpoints can be turned off by setting `Enabled` to `false`, which applies to both URLs.

## HTTP Access

To expose any of the management endpoints over HTTP in an ASP.NET Core application:

1. Add a NuGet package reference to `Steeltoe.Management.Endpoint`.
1. Configure endpoint settings, as needed (typically in `appsettings.json`).
1. Add the actuator endpoint(s) to the service container.
1. Optional: Add any additional health/info contributors to the service container.
1. Optional: Customize the CORS policy.
1. Optional: Secure endpoints.
1. Optional: Override the middleware pipeline setup.

> [!CAUTION]
> By default, actuator endpoints are exposed on the same host(s) and port(s) as the application (which can be configured as described [here](https://learn.microsoft.com/aspnet/core/fundamentals/servers/kestrel/endpoints) and [here](https://andrewlock.net/8-ways-to-set-the-urls-for-an-aspnetcore-app/)).
> Use the `Port` and `SslEnabled` settings described above to isolate management endpoints from regular application endpoints.

The example below adds all actuators:

```csharp
using Steeltoe.Management.Endpoint.Actuators.All;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAllActuators();
```

> [!TIP]
> It's recommended to use `AddAllActuators()` instead of adding individual actuators,
> which enables individually turning them on/off at runtime via configuration.

Alternatively, individual actuators can be added:

```csharp
using Steeltoe.Management.Endpoint.Actuators.Hypermedia;
using Steeltoe.Management.Endpoint.Actuators.Loggers;
using Steeltoe.Management.Endpoint.Actuators.Refresh;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHypermediaActuator().AddLoggersActuator().AddRefreshActuator();
```

> [!NOTE]
> `AddAllActuators()` and `AddLoggingActuator()` automatically configure [Dynamic Console Logging](../logging/dynamic-console-logging.md). To use [Dynamic Serilog Logging](../logging/dynamic-serilog-logging.md), be sure to do so *before* adding actuators. For example:
>
> ```csharp
> using Steeltoe.Logging.DynamicSerilog;
> using Steeltoe.Management.Endpoint.Actuators.All;
>
> var builder = WebApplication.CreateBuilder(args);
> builder.Logging.AddDynamicSerilog();
> builder.Services.AddAllActuators();
> ```

## Adding additional contributors

The `health` and `info` endpoints can be extended with custom contributors. For example:

```csharp
using Steeltoe.Management.Endpoint.Actuators.Health;
using Steeltoe.Management.Endpoint.Actuators.Info;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHealthActuator();
builder.Services.AddHealthContributor<CustomHealthContributor>();
builder.Services.AddInfoActuator();
builder.Services.AddInfoContributor<CustomInfoContributor>()
```

## Customizing the CORS policy

By default, any origin is allowed to access the actuator endpoints. To customize the CORS policy, use the `ConfigureActuatorsCorsPolicy` extension method:

```csharp
using Steeltoe.Management.Endpoint;
using Steeltoe.Management.Endpoint.Actuators.All;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAllActuators();
builder.Services.ConfigureActuatorsCorsPolicy(policy => policy.WithOrigins("http://www.example.com"));
```

## Securing Endpoints

Endpoints can be customized with `IEndpointConventionBuilder`. This allows calling [`RequireAuthorization()`](https://learn.microsoft.com/aspnet/core/security/authorization/policies?#apply-policies-to-endpoints) to configure the Authorization middleware:

```csharp
using Steeltoe.Management.Endpoint;
using Steeltoe.Management.Endpoint.Actuators.All;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAllActuators();
builder.Services.ConfigureActuatorEndpoints(endpoints => endpoints.RequireAuthorization());
```

When `RequireAuthorization()` is called without arguments, the default profile is used. Other overloads allow passing a profile or a profile name.

## Overriding the middleware pipeline setup

All `Add*Actuator` methods provide an overload that takes a boolean `configureMiddleware`, which enables to skip adding middleware to the ASP.NET Core pipeline.
While this provides full control over the pipeline contents and order, it requires manual addition of the appropriate middleware for actuators to work correctly.

```csharp
using Steeltoe.Management.Endpoint;
using Steeltoe.Management.Endpoint.Actuators.All;
using Steeltoe.Management.Endpoint.Actuators.CloudFoundry;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAllActuators(configureMiddleware: false);
await using WebApplication app = builder.Build();

app.UseManagementPort(); // required to block actuator requests on the app port
app.UseRouting();
app.UseActuatorsCorsPolicy(); // required to activate the CORS policy for actuators
app.UseCloudFoundrySecurity(); // required by AddCloudFoundryActuator()
app.UseActuatorEndpoints(); // maps the actuator endpoints

await app.StartAsync();
```

While the order above must not be changed (and it's not recommended to leave out entries), additional middleware can be inserted as appropriate.

### Conventional routing

Applications that use the legacy [conventional routing](https://learn.microsoft.com/aspnet/core/mvc/controllers/routing#conventional-routing)
are still supported by the `Add*Actuator` methods.
However, they won't show up in the [route mappings actuator](./mappings.md) anymore.
