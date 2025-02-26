# Cloud Foundry Integration

Integration with Apps Manager is accomplished by adding the Cloud Foundry management endpoint to your application. When used, this endpoint enables the following additional functionality on Cloud Foundry:

* Provides an alternate, secured route to the endpoints expected by Apps Manager and configured in your application.
* Exposes an endpoint that can be queried to return the IDs of and links to the enabled management endpoints in the application.
* Adds Cloud Foundry security middleware to the request pipeline, to secure access to the management endpoints by using security tokens acquired from the UAA.
* Adds extension methods that simplify adding the Steeltoe management endpoints necessary for Apps Manager integration with HTTP access to the application.

>NOTE: The Cloud Foundry integration will not work unless the [Cloud Foundry Configuration Provider](../configuration/cloud-foundry-provider.md) has also been configured.

## Security

When adding this management endpoint to your application, the Cloud Foundry security middleware is added to the request processing pipeline of your application to enforce that, when a request is made of any of the management endpoints, a valid UAA access token is provided as part of that request. Additionally, the security middleware uses the token to determine whether the authenticated user has permission to access the management endpoint.

>NOTE: The Cloud Foundry security middleware is automatically disabled when your application is not running on Cloud Foundry (for example, running locally on your desktop).

## External access

When running in Cloud Foundry, it is possible to access the endpoints via the [hypermedia](./hypermedia.md) context path which defaults to `/actuator`. In other words you can also access all your endpoints from this global path. For example the [Info](./info.md) endpoint would be accessible at `/actuator/info`.

While the endpoints provided on the `/cloudfoundryapplication` path are secured as described above, the endpoints provided on the `/actuator` path are not. For this reason, only health and info are exposed by default and others would have to be exposed explicitly. In addition the endpoints may be secured by whatever security mechanism the application itself uses. For more details see [Securing Actuators](./using-endpoints.md#securing-endpoints)

## Configure Settings

Typically, you need not do any additional configuration. However, the following table describes the additional settings that you could apply to the Cloud Foundry endpoint:

| Key | Description | Default |
| --- | --- | --- |
| `Id` | The ID of the Cloud Foundry endpoint. | "" |
| `Enabled` | Whether to enable Cloud Foundry management endpoint. | `true` |
| `ValidateCertificates` | Whether to validate server certificates. | `true` |
| `ApplicationId` | The ID of the application used in permissions check. | VCAP settings |
| `CloudFoundryApi` | The URL of the Cloud Foundry API. | VCAP settings |

>Each setting in the preceding table must be prefixed with `Management:Endpoints:CloudFoundry`.

## Enable HTTP Access

The default path to the Cloud Foundry endpoint is computed by combining the global `Path` prefix setting together with the `Id` setting described in the table above. The global path is always set to `/cloudfoundryapplication`. Apps Manager expects this endpoint to be available at `/cloudfoundryapplication`.

See the [HTTP Access](./using-endpoints.md#http-access) section to see the overall steps required to enable HTTP access to endpoints in an ASP.NET Core application.

The Cloud Foundry actuator, the actuator's security middleware, and the CORS policy can be added to the application in either `program.cs` or `startup.cs`. This can be done individually or in a grouping with all the other actuators. All of these options will function effectively the same way, so this is a style choice.

>Of all the options provided, the approach of using `AddAllActuators()` on the `HostBuilder` is most recommended.

### Program.cs

If you prefer maximum convenience, extension methods for both `IHostBuilder` and `IWebHostBuilder` can configure all actuators at once with a single line of code:

```csharp
using Steeltoe.Management.CloudFoundry; // for .AddCloudFoundryActuators()
using Steeltoe.Management.Endpoint;     // for other options
...
    public static void Main(string[] args)
    {
        BuildWebHost(args).Run();
    }
    public static IHost BuildHost(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .AddAllActuators()            // add CF + security (when deployed to CF) and all others
            //.AddCloudFoundryActuators() // add CF + security + others, deprecated in 3.1.0
            //.AddCloudFoundryActuator()  // add just CF + security
            .Build();
```

### Startup.cs

If you prefer to configure the services and activate the middleware separately, more code is required, but you may use these extensions methods for `IServiceCollection` and `IApplicationBuilder`:

```csharp
using Steeltoe.Management.Endpoint;
using Steeltoe.Management.Endpoint.CloudFoundry;
using Steeltoe.Management.Endpoint.Health;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }
    public void ConfigureServices(IServiceCollection services)
    {
        // for versions >= 3.1.0-rc1, add all actuators and Cloud Foundry integration pieces
        services.AddAllActuators(Configuration);

        // for versions < 3.1.0, add all actuators and Cloud Foundry integration pieces
        //services.AddCloudFoundryActuators(Configuration);
        // if you prefer to add individual actuators
        //services.AddCloudFoundryActuator(Configuration);
        ...
    }
    public void Configure(IApplicationBuilder app)
    {
        // because Apps Manager interacts with actuators through your browser, CORS must be configured
        app.UseCors("SteeltoeManagement");
        // activate the CF security middleware
        app.UseCloudFoundrySecurity();

        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapAllActuators()
        }
        // Initializes health readiness and liveness probes
        app.ApplicationServices.InitializeAvailability();
    }
}
```
