# Cloud Foundry Integration

Integration with Apps Manager is accomplished by adding the Cloud Foundry management endpoint to your application. When used, this endpoint enables the following additional functionality on Cloud Foundry:

* Provides an alternate, secured route to the endpoints expected by Apps Manager and configured in your application.
* Exposes an endpoint that can be queried to return the IDs of and links to the enabled management endpoints in the application.
* Adds Cloud Foundry security middleware to the request pipeline, to secure access to the management endpoints by using security tokens acquired from the UAA.
* Adds extension methods that simplify adding the Steeltoe management endpoints necessary for Apps Manager integration with HTTP access to the application.

## Security

When adding this management endpoint to your application, the Cloud Foundry security middleware is added to the request processing pipeline of your application to enforce that, when a request is made of any of the management endpoints, a valid UAA access token is provided as part of that request. Additionally, the security middleware uses the token to determine whether the authenticated user has permission to access the management endpoint.

>The Cloud Foundry security middleware is automatically disabled when your application is not running on Cloud Foundry (for example, running locally on your desktop).

## External access

When running in Cloud Foundry, it is possible to access the endpoints via the [hypermedia](/docs/3/management/hypermedia) context path which defaults to `/actuator`. In other words you can also access all your endpoints from this global path. For example the [Info](/docs/3/management/info) endpoint would be accessible at `/actuator/info`.

While the endpoints provided on the `/cloudfoundryapplication` path are secured as described above, the endpoints provided on the `/actuator` path are not. For this reason, only health and info are exposed by default and others would have to be exposed explicitly. In addition the endpoints may be secured by whatever security mechanism the application itself uses.

## Configure Settings

Typically, you need not do any additional configuration. However, the following table describes the additional settings that you could apply to the Cloud Foundry endpoint:

| Key | Description | Default |
| --- | --- | --- |
| `Id` | The ID of the Cloud Foundry endpoint. | "" |
| `Enabled` | Whether to enable Cloud Foundry management endpoint. | `true` |
| `ValidateCertificates` | Whether to validate server certificates. | `true` |
| `ApplicationId` | The ID of the application used in permissions check. | VCAP settings |
| `CloudFoundryApi` | The URL of the Cloud Foundry API. | VCAP settings |

>Each setting in the preceding table must be prefixed with `Management:Endpoints:cloudfoundry`.

## Enable HTTP Access

The default path to the Cloud Foundry endpoint is computed by combining the global `Path` prefix setting together with the `Id` setting described in the table above. The default path is `/cloudfoundryapplication` which is used by Apps Manager.

See the [HTTP Access](/docs/3/management/using-endpoints#http-access) section to see the overall steps required to enable HTTP access to endpoints in an ASP.NET Core application.

To add the actuator to the service container and map its route, you can use the `AddCloudFoundryActuator` extension method from `ManagementHostBuilderExtensions`.

Alternatively, first, add the Cloud Foundry actuator to the service container, using the `AddCloudFoundryActuator()` extension method from `EndpointServiceCollectionExtensions`.

Then, add the Cloud Foundry actuator and security middleware to the ASP.NET Core pipeline, using the `UseCloudFoundrySecurity()` extension methods from `EndpointApplicationBuilderExtensions` and `Map<CloudFoundryEndpoint>()` from `ActuatorRouteBuilderExtensions`

Extensions for both `IHostBuilder` and `IWebHostBuilder` are included to configure all actuators including CloudFoundry, Hypermedia and others, with a single line of code in `Program.cs`:

```csharp
    public static void Main(string[] args)
    {
        BuildWebHost(args).Run();
    }
    public static IHost BuildHost(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .AddCloudFoundryActuators()
            .Build();
```
