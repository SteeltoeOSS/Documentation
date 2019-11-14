## Initialize Discovery Client

### ASP.NET Core

<!-- TODO: rewrite this section to account for Pivotal packages going away, mention Autofac -->
The next step is to add the Steeltoe Discovery client to the service container and use it to cause the client to start communicating with the server.

You do these two things in the `ConfigureServices()` and `Configure()` methods of the `Startup` class, as shown in the following example:

```csharp
using Pivotal.Discovery.Client;
// or
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

>NOTE: If you use the `Pivotal.Discovery.ClientCore` package, you need to add a `using Pivotal.Discovery.Client;`.  If you use the `Steeltoe.Discovery.ClientCore`, you need to add a `Steeltoe.Discovery.Client;`. Doing so is required to gain access to the extension methods described later.

### ASP.NET

### Registering Services

If you configured the clients settings to register services, the service is automatically registered when the `UseDiscoveryClient()` method is called in the `Configure()` method. You do not need to do anything else to cause service registration.

See the [Eureka Client Settings](#2-2-2-eureka-client-settings) or [Consul Client Settings](#3-0-hashicorp-consul)

