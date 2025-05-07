# Spring Boot Admin Client

Steeltoe Spring Boot Admin client provides a way to integrate with [Spring Boot Admin Server](https://github.com/codecentric/spring-boot-admin). This will enable the monitoring and management of applications in any environment.

## Add NuGet Reference

Add the following PackageReference to your .csproj file.

```xml
<ItemGroup>
...
    <PackageReference Include="Steeltoe.Management.EndpointBase" Version="3.2.0" />
...
</ItemGroup>
```

## Add Spring Boot Admin Client

The extension method `AddSpringBootAdminClient` adds an `IHostedService` and necessary components to register and un-register the application when it starts up and shuts down. The extension can be used in Startup.cs as follows:

```csharp
  public void ConfigureServices(IServiceCollection services)
  {
      ...
      // Register startup/shutdown interactions with Spring Boot Admin server
      services.AddSpringBootAdminClient();
      ...
  }
```

## Configure Settings

The following table describes the settings that you can apply to the client:

| Key | Description | Default |
| --- | --- | --- |
| `Url` | The Url of the Spring Boot Admin server. | `null` |
| `ApplicationName` | The name of the Steeltoe app being registered. | `IApplicationInstanceInfo.ApplicationName` |
| `BasePath` | Base path to find endpoints for integration. | `IApplicationInstanceInfo.Uris` |
| `ValidateCertificates` | Whether server certificates should be validated. | `true` |
| `ConnectionTimeoutMS` | Connection timeout (in milliseconds). | `100000` |
| `Metadata` | Dictionary of metadata to use when registering. | `new Dictionary<string, object> { { "startup", DateTime.Now } }` |

Here is an example settings file.

```json
{
  "Spring": {
    "Application": {
      "Name": "SteeltoeApp"
    },
    "Boot": {
      "Admin": {
        "Client": {
          "Url": "http://localhost:8080",
          "Metadata": {
            "user.name": "actuatorUser",
            "user.password": "actuatorPassword"
          }
        }
      }
    }
  }
}
```

For testing, you can use a version of spring boot admin server running locally.

```bash
docker run -d -p 8080:8080 steeltoeoss/spring-boot-admin
```
