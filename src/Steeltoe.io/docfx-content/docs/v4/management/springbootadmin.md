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
| `Url` | The URL of the Spring Boot Admin server. | |
| `ApplicationName` | The name of the Steeltoe app being registered. | computed |
| `BasePath` | Base path to find endpoints for integration. | computed |
| `ValidateCertificates` | Whether server certificates should be validated. | `true` |
| `ConnectionTimeoutMS` | Connection timeout (in milliseconds). | `100_000` |
| `Metadata` | Dictionary of metadata to use when registering. | |

## Connecting to dockerized Spring Boot Admin Server

For successful communication between your app and Spring Boot Admin Server running inside a docker container,
a few additional steps are needed:

- Register your app using `host.docker.internal`

  Once your app has registered itself with Spring Boot Admin Server, the server tries to connect to your app
  and send requests to its actuator endpoints. This fails when your app registers itself using `localhost`,
  because the server runs in a docker container that has its own network.
  Instead, you need to register using the special `host.docker.internal` address, which enables the server inside the container
  to connect to your app outside the container.

- Bind your app to a wildcard address

  By default, your app listens only on `localhost`, which is not accessible from the Spring Boot Admin Server running in a container.
  To make the app accessible, bind it to a wildcard address, which allows it to listen on all available network interfaces.
  This can be done by updating the `launchSettings.json` file.

- Use HTTP instead of HTTPS

  The server doesn't trust the self-signed certificate used by your app, so you need to use HTTP instead of HTTPS.
  This requires removing `app.UseHttpsRedirection()` from your `Program.cs` file.

- Register additional actuator endpoints

  For the server to report the app as "UP", you need to add at least the hypermedia and health actuators in `Program.cs`.

> [!TIP]
> You can use the [Steeltoe docker image for SBA](https://github.com/SteeltoeOSS/Samples/blob/main/CommonTasks.md#spring-boot-admin) for testing purposes.

Putting it all together, your `appsettings.Development.json` file should look like this:

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
          "BasePath": "http://host.docker.internal:5258"
        }
      }
    }
  }
}
```

With the following contents in `launchsettings.json`:

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
> If you'd like your app to listen on a different port number, replace `5258` in both files above with the desired port.

And the following code in `Program.cs`:

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

// In order to avoid trust issues with self-signed certificates, do not automatically redirect to https.
//app.UseHttpsRedirection();

app.MapControllers();

app.Run();
```

> [!TIP]
> To see all the Spring Boot Admin features in action, replace the `Add*Actuator()` calls in `Program.cs` with `AddAllActuators()` and [expose all endpoints](./using-endpoints.md#exposing-endpoints).
