# Initialize Discovery Client

This section describes how to configure the Steeltoe discovery client.

Fundamentally, two things need to happen to activate the client:

1. Services need to be configured and added to the service collection
1. Services need to be started

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

Should you wish to avoide this reflection operation, you also have the option of declaratively configuring the client you plan to use, like this:

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

        // Use the Steeltoe Discovery Client service
        app.UseDiscoveryClient();
    }
    ...
}
```

## Registering Services

If you configured the client settings to register services, the service is automatically registered when the `UseDiscoveryClient()` method is called in the `Configure()` method. You need not do anything else to cause service registration.
