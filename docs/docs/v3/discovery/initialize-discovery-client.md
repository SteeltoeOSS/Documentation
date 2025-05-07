# Initialize Discovery Client

This section describes how to configure the Steeltoe discovery client.

Fundamentally, three things need to happen to use the clients:

1. Add NuGet References
1. Services need to be configured and added to the service collection
1. Services need to be started

## Add NuGet References

The simplest way to get started with Steeltoe Discovery is to add a reference to the package(s) containing the client technology you may wish to use. Any client package also includes all the relevant dependencies.

| Package | Description | .NET Target |
| --- | --- | --- |
| `Steeltoe.Discovery.Abstractions` | Interfaces and objects used for extensibility. | .NET Standard 2.0 |
| `Steeltoe.Discovery.ClientBase` |  Service Discovery base package. | .NET Standard 2.0 |
| `Steeltoe.Discovery.ClientCore` | Includes base. Adds WebHost compatibility. | ASP.NET Core 3.1+ |
| `Steeltoe.Discovery.Eureka` | Eureka Client functionality. Depends on ClientBase. | .NET Core 3.1+ |
| `Steeltoe.Discovery.Consul` | Consul Client functionality. Depends on ClientBase. | .NET Core 3.1+ |
| `Steeltoe.Discovery.Kubernetes` | Kubernetes Client functionality. Depends on ClientBase. | .NET Core 3.1+ |

To add this type of NuGet to your project, add an element resembling the following `PackageReference`:

```xml
<ItemGroup>
...
    <PackageReference Include="Steeltoe.Discovery.Consul" Version="3.2.0" />
...
</ItemGroup>
```

If you are using `WebHost` to run your application, you will need to also add a reference to `Steeltoe.Discovery.ClientCore`. If you would like the option of switching between clients using configuration, add a reference for each client you may wish to use.

>As of version 3.0, a direct reference to any applicable discovery client is required. If you find log messages saying `No discovery client has been configured...`, this is why.

## HostBuilder Extensions

After installing the NuGet package(s), the next step is to add the Steeltoe Discovery client to the service container and use it to cause the client to start communicating with the server.

You can do both at once with the extension methods for `IHostBuilder` and `IWebHostBuilder`. The following example uses `IHostBuilder`, and the usage is the same for `IWebHostBuilder`:

```csharp
public class Program
{
    public static void Main(string[] args)
    {
        BuildWebHost(args).Run();
    }
    public static IHost BuildHost(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .AddDiscoveryClient()
            .Build();
}
```

The `AddDiscoveryClient` extension shown above uses reflection to find assemblies containing a discovery client. Specifically, assemblies are located by the presence of the attribute `DiscoveryClientAssembly`, which contains a reference to an `IDiscoveryClientExtension` that does the work in configuring options and injecting the required service for the `IDiscoveryClient` to operate.

>If no discovery client package is found a [NoOpDiscoveryClient](https://github.com/SteeltoeOSS/Steeltoe/blob/release/3.2/src/Discovery/src/ClientBase/SimpleClients/NoOpDiscoveryClient.cs) will be used. This will happen when no client has been added or if you publish your application with [`/p:PublishSingleFile=true`](https://docs.microsoft.com/dotnet/core/deploying/single-file))

To avoid this reflection-based approach, use declarative configuration of the client(s) you plan to use, like this:

```csharp
public class Program
{
    public static void Main(string[] args)
    {
        BuildWebHost(args).Run();
    }
    public static IHost BuildHost(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .AddServiceDiscovery(options => options.UseEureka())
            .Build();
}
```

As of version 3.0.2, you may add multiple `Usexxx()` statements to this options builder, so long as only one is configured at runtime.

>If no extension is supplied for `AddServiceDiscovery`, a [NoOpDiscoveryClient](https://github.com/SteeltoeOSS/Steeltoe/blob/3.2/src/Discovery/src/ClientBase/SimpleClients/NoOpDiscoveryClient.cs) will be used.

## Other Extensions

Alternatively, you can register and activate the client in the `ConfigureServices()` and `Configure()` methods of the `Startup` class:

```csharp
using Steeltoe.Discovery.Client;

public class Startup {
    ...
    public IConfiguration Configuration { get; private set; }
    public Startup(...)
    {
      ...
    }
    public void ConfigureServices(IServiceCollection services)
    {
        // Add any NuGet-referenced Discovery Client services
        services.AddDiscoveryClient();
        // or add a specific Discovery Client
        services.AddServiceDiscovery(options => options.UseConsul());

        // Add framework services.
        services.AddMvc();
        ...
    }
    public void Configure(IApplicationBuilder app, ...)
    {
        ...
        app.UseStaticFiles();
        app.UseMvc();

        // Activate the Steeltoe Discovery Client service background thread
        // This line is not needed for Steeltoe 3.1.0 and above
        app.UseDiscoveryClient();
    }
    ...
}
```

## Registering Services

If you configured the client settings to register services, the service is automatically registered when the `UseDiscoveryClient()` method is called in the `Configure()` method. You need not do anything else to cause service registration.

## Enable Debug Logging

Sometimes, you may want to turn on debug logging in the discovery client. To do so, you can modify the `appsettings.json` file and turn on Debug level logging for the Steeltoe components:

The following example shows a typical `appsettings.json` file:

```json
{
  "Logging": {
    "IncludeScopes": false,
    "LogLevel": {
      "Default": "Warning",
      "Steeltoe": "Debug"
    }
  },
  ...
}
```
