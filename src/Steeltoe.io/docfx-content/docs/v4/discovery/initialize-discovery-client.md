# Discovery clients

This section describes how to activate the Steeltoe discovery client(s), which is a prerequisite for resolving friendly names.
Your app can use multiple clients, but is limited to a single instance per server type.

Fundamentally, several things need to happen:

1. Add NuGet package references to your project
1. Register the desired discovery client(s) in the dependency container
1. Configure the discovery client(s)
1. Optional: enable debug logging

## Add NuGet packages

To get started with Steeltoe Discovery, add a reference to the package(s) containing the discovery technology you may wish to use.
Each package also includes all the relevant dependencies.

| Package | Description |
| --- | --- |
| `Steeltoe.Discovery.Configuration` | Query app instances stored in .NET configuration |
| `Steeltoe.Discovery.Consul` | Use [HashiCorp Consul](https://www.consul.io/) server |
| `Steeltoe.Discovery.Eureka` | Use [Spring Cloud Eureka](https://projects.spring.io/spring-cloud/docs/1.0.3/spring-cloud.html#spring-cloud-eureka-server) server |

## ServiceCollection extension methods

After installing the NuGet package(s), the next step is to add the Steeltoe discovery client(s) to the service container.
Update your `Program.cs` as shown below:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Steeltoe: Add service discovery clients for Consul, Eureka, and/or configuration-based.
builder.Services.AddConsulDiscoveryClient();
builder.Services.AddEurekaDiscoveryClient();
builder.Services.AddConfigurationDiscoveryClient();

var app = builder.Build();
```

> [!TIP]
> Alternatively, `builder.AddSteeltoe()` (Steeltoe Bootstrap Auto Configuration) can be used, which uses reflection to determine
> which discovery assemblies are loaded, adding the appropriate clients automatically.

## Client configuration

Discovery clients need to be explicitly configured to fetch application instances and/or register your app with the discovery server.
See the sub-page for the discovery technology of choice on how to set it up.

## Enable debug logging

Sometimes, you may want to turn on debug logging for service discovery.
To do so, you can modify the `appsettings.json` file and turn on debug logging for the Steeltoe components.

The following example shows a typical `appsettings.json` file:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Steeltoe.Discovery": "Debug"
    }
  }
}
```
