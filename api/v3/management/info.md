# Info

The Steeltoe `Info` management endpoint exposes various application information collected from all the `IInfoContributor` instances that have been provided to the `InfoEndpoint`.

Steeltoe includes a couple `IInfoContributor` implementations out of the box that you can use. Most importantly, you can also write your own.

## Info Contributors

The following table describes the `IInfoContributor` implementations provided by Steeltoe:

| Name | Description |
| --- | --- |
| `AppSettingsInfoContributor` | Exposes any values under the `info` key (for example, `info:cat:hat=cathat`) that is in your apps configuration (for example, `appsettings.json`). |
| `BuildInfoContributor` | Exposes file/version info for both the included version of Steeltoe and the application. |
| `GitInfoContributor` | Exposes git information (if a `git.properties` file is available). |

For an example of how to use the above `GitInfoContributor` within MSBuild using [GitInfo](https://github.com/kzu/GitInfo), see the [Steeltoe management sample](https://github.com/SteeltoeOSS/Samples/tree/3.x/Management/src/CloudFoundry) and the [CloudFoundry.csproj](https://github.com/SteeltoeOSS/Samples/blob/3.x/Management/src/CloudFoundry/CloudFoundry.csproj) file.

If you wish to provide custom information for your application, create a class that implements the `IInfoContributor` interface and then add that to the `InfoEndpoint`. Details on how to add a contributor to the endpoint is provided later in this section.

The following `IInfoContributor` example adds `someProperty=someValue` to the application's information.

```csharp
public class ArbitraryInfoContributor : IInfoContributor
{
    public void Contribute(IInfoBuilder builder)
    {
        // pass in the info
        builder.WithInfo("arbitraryInfo", new { someProperty = "someValue" });
    }
}
```

>Custom `IInfoContributor` implementations must be retrievable from the DI container by interface in order for Steeltoe to find them.

## Configure Settings

The following table describes the settings that you can apply to the endpoint:

| Key | Description | Default |
| --- | --- | --- |
| `Id` | The ID of the info endpoint. | `info` |
| `Enabled` | Whether to enable info management endpoint. | `true` |
| `Sensitive` | Currently not used. | `false` |
| `RequiredPermissions` | User permissions required on Cloud Foundry to access endpoint. | `RESTRICTED` |

>Each setting above must be prefixed with `Management:Endpoints:Info`.

## Enable HTTP Access

The default path to the Info endpoint is computed by combining the global `Path` prefix setting together with the `Id` setting from above. The default path is `/actuator/info`.

See the [HTTP Access](./using-endpoints.md#http-access) section to see the overall steps required to enable HTTP access to endpoints in an ASP.NET Core application.

To add the actuator to the service container and map its route, use any of the `AddInfoActuator` extension methods from `ManagementHostBuilderExtensions`.

Alternatively, first, add the Info actuator to the service container, use any of the `AddInfoActuator()` extension methods from `EndpointServiceCollectionExtensions`.

Then add the Info actuator middleware to the ASP.NET Core pipeline, use the `Map<InfoEndpoint>()` extension method from `ActuatorRouteBuilderExtensions`.

The following example shows how to enable the info endpoint and how to add a custom `IInfoContributor` to the service container by adding `ArbitraryInfoContributor` as a singleton. Once that is done, the info endpoint discovers and uses it during info requests.

```csharp
public class Startup
{
    ...
    public void ConfigureServices(IServiceCollection services)
    {
        // Add custom info contributor, specifying the interface type
        services.AddSingleton<IInfoContributor, ArbitraryInfoContributor>();

        // Add Info actuator
        services.AddInfoActuator(Configuration);

        // Add framework services.
        services.AddMvc();
    }
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
        app.UseStaticFiles();

       app.UseEndpoints(endpoints =>
            {
                // Add management endpoints into pipeline like this
                endpoints.Map<InfoEndpoint>();

                // ... Other mappings
            });
    }
}
```
