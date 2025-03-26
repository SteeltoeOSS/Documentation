# Initialize Discovery Client

### ASP NET Core

The next step is to add the Steeltoe Discovery client to the service container and use it to cause the client to start communicating with the server.

As of version 2.4.0, you can do both of these two things at once with extension methods that have been added for both `IHostBuilder` and `IWebHostBuilder`. The following example uses `IHostBuilder`, and usage is the same for `IWebHostBuilder`:

```csharp
public class Program
{
    public static void Main(string[] args)
    {
        BuildWebHost(args).Run();
    }
    public static IHost BuildHost(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .AddServiceDiscovery()
            .Build();
```

These new extensions provide a convenient way to register and activate the discovery client. You may alternatively register and activate the client in the `ConfigureServices()` and `Configure()` methods of the `Startup` class, as shown in the following example:

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
        // Add Steeltoe Discovery Client service
        services.AddDiscoveryClient(Configuration);

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
```

> NOTE: The `Pivotal.Discovery.*` packages have been deprecated, with all functionality rolled into the Steeltoe packages. Update your references and using statements to use the Steeltoe packages.

### Registering Services

If you configured the clients settings to register services, the service is automatically registered when the `UseDiscoveryClient()` method is called in the `Configure()` method. You do not need to do anything else to cause service registration.
