# Spring Boot Admin Client

The Steeltoe Spring Boot Admin client provides a way to integrate with [Spring Boot Admin Server](https://github.com/codecentric/spring-boot-admin), which enables monitoring and management of applications in any environment.

## Add NuGet Reference

Add a reference to the `Steeltoe.Management.Endpoint` NuGet package.

## Add Spring Boot Admin Client

The extension method `AddSpringBootAdminClient` adds an `IHostedService` and necessary components to register and un-register the application when it starts up and shuts down. It can be used in `Program.cs` as follows:

```csharp
using Steeltoe.Management.Endpoint.SpringBootAdminClient;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSpringBootAdminClient();
```

## Configure Settings

The following table describes the configuration settings that you can apply to the client.
Each key must be prefixed with `Spring:Boot:Admin:Client:`.

| Key | Description | Default |
| --- | --- | --- |
| `Url` | The URL of the Spring Boot Admin server | |
| `ApplicationName` | The name of the Steeltoe app being registered | computed |
| `BaseUrl` | The base URL to find endpoints for integration | computed |
| `BaseScheme` | The scheme (`http` or `https`) to use in `BaseUrl` | |
| `BaseHost` | The hostname or IP address to use in `BaseUrl` | |
| `BasePort` | The port number to use in `BaseUrl` | |
| `BasePath` | The path to use in `BaseUrl` | |
| `UseNetworkInterfaces` | Query the operating system for network interfaces to determine `BaseHost` | `false` |
| `PreferIPAddress` | Whether to register with IP address instead of hostname | `false` |
| `ValidateCertificates` | Whether server certificates should be validated | `true` |
| `ConnectionTimeoutMS` | Connection timeout (in milliseconds) | `5_000` |
| `RefreshInterval` | How often to re-register with the Spring Boot Admin server | `00:00:15` |
| `Metadata` | Dictionary of metadata to use when registering | |

At the minimum, `Url` must be configured. The other settings are optional and can be used to customize the registration process.
For example, if your app runs behind a reverse proxy or API gateway.

> [!TIP]
> By default, your app re-registers with the Spring Boot Admin server every 15 seconds.
> To register only once at startup, set `RefreshInterval` to `0`.

When not configured, `BaseUrl` is determined from:
- The `BaseScheme`, `BaseHost`, `BasePort`, and `BasePath` settings, if configured.
- If management endpoints are exposed on an alternate port, its scheme and port are used, combined with the local hostname or IP address.
- The scheme, non-wildcard host, and port from the ASP.NET Core bindings your app listens on. Dynamic port bindings are supported.
  - If multiple bindings exist and `BaseScheme` is configured, incompatible bindings are discarded.
  - If multiple bindings exist and `PreferIPAddress` is set to `true`, entries with an IP address are preferred.
  - If multiple bindings exist and `BaseScheme` is not configured, entries with `https` are preferred.
  - If multiple bindings exist, entries with a non-wildcard host are preferred.
  - A wildcard host is replaced with the local hostname or IP address.

In ASP.NET Core bindings, anything not recognized as a valid IP address or `localhost` is treated as a wildcard host
that binds to all IPv4 and IPv6 addresses. Some people like to use `*` or `+` to be more explicit.
See the [Microsoft documentation on URL formats](https://learn.microsoft.com/aspnet/core/fundamentals/servers/kestrel/endpoints#url-formats) for details.

> [!CAUTION]
> Earlier versions of Steeltoe expected `BasePath` to contain the full URL. When upgrading, set `BaseUrl` instead.
> The default connection timeout has changed from 100 seconds to 5 seconds.

## Connecting to containerized Spring Boot Admin Server

For successful communication between your app and Spring Boot Admin Server running inside a container,
a few additional steps are needed:

- Register your app using `host.docker.internal` (docker) or `host.containers.internal` (podman)

  After your app has registered itself with Spring Boot Admin Server, the server tries to connect to your app
  and send requests to its actuator endpoints. This fails when your app registers itself using `localhost`,
  because the server runs in a container that has its own network.
  Instead, you must register using the special address, which enables the server inside the container
  to connect to your app outside the container.

- Bind your app to a wildcard address

  By default, your app listens only on `localhost`, which is not accessible from the Spring Boot Admin Server running in a container.
  To make the app accessible, bind it to a wildcard address, which allows it to listen on all available network interfaces.
  To do this, update the `launchSettings.json` file.

- Use HTTP instead of HTTPS

  The server doesn't trust the self-signed certificate used by your app, so you must use HTTP instead of HTTPS.
  This requires removing `app.UseHttpsRedirection()` from your `Program.cs` file.

- Register additional actuator endpoints

  For the server to report the app as `UP`, you must add at least the hypermedia and health actuators in `Program.cs`.

> [!TIP]
> For testing, you can use the [Steeltoe docker image for SBA](https://github.com/SteeltoeOSS/Samples/blob/main/CommonTasks.md#spring-boot-admin).

Putting it all together, your config files contain the contents shown in the following:

- In `appsettings.Development.json`:

    ```json
    {
      "Spring": {
        "Application": {
          "Name": "ExampleSteeltoeApp"
        },
        "Boot": {
          "Admin": {
            "Client": {
              // This is the URL of the Spring Boot Admin Server.
              "Url": "http://localhost:9099",
              // This is what the Spring Boot Admin Server uses to connect to your app.
              "BaseHost": "host.docker.internal"
            }
          }
        }
      }
    }
    ```

- In `launchsettings.json`:

    ```json
    {
      "$schema": "http://json.schemastore.org/launchsettings.json",
      "profiles": {
        "http": {
          "commandName": "Project",
          "dotnetRunMessages": true,
          // Listen on all network interfaces by using a wildcard for the hostname.
          "applicationUrl": "http://*:5258",
          "environmentVariables": {
            "ASPNETCORE_ENVIRONMENT": "Development"
          }
        }
      }
    }
    ```

    > [!NOTE]
    > If you want your app to listen on a different port number, replace `5258` with the desired port.

- In `Program.cs`:

    ```csharp
    using Steeltoe.Management.Endpoint.Actuators.Health;
    using Steeltoe.Management.Endpoint.Actuators.Hypermedia;
    using Steeltoe.Management.Endpoint.SpringBootAdminClient;

    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.

    builder.Services.AddSpringBootAdminClient();
    builder.Services.AddHypermediaActuator();
    builder.Services.AddHealthActuator();

    builder.Services.AddControllers();

    var app = builder.Build();

    // To avoid trust issues with self-signed certificates, do not automatically redirect to https.
    //app.UseHttpsRedirection();

    app.MapControllers();

    app.Run();
    ```

> [!TIP]
> To see all the Spring Boot Admin features in action, replace the `Add*Actuator()` calls in `Program.cs` with `AddAllActuators()` and expose all endpoints. See [Exposing endpoints](./using-endpoints.md#exposing-endpoints).
