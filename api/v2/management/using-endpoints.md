# Using Endpoints

Steeltoe provides a base set of endpoint functionality, along with several implementations for exposing the endpoints over HTTP. HTTP implementations are provided with ASP.NET Core middleware, OWIN middleware and HTTP Modules. Should you wish to expose the core endpoint functionality over some protocol other than HTTPS, you are free to provide your own implementation.

## Reference Materials

In this section, it is helpful to understand the following:

* How the .NET [Configuration service](https://docs.microsoft.com/aspnet/core/fundamentals/configuration) works and an understanding of the `ConfigurationBuilder` and how to add providers to the builder to configure the endpoints.
* How the ASP.NET Core [`Startup`](https://docs.microsoft.com/aspnet/core/fundamentals/startup) class is used in configuring the application services for the app. Pay particular attention to the usage of the `ConfigureServices()` and `Configure()` methods.

When adding Steeltoe Management endpoints to your ASP.NET 4.x applications, you can choose between using HTTP modules and OWIN middleware. If you select HTTP modules, you should be familiar with `Global.asax.cs` and how it is used in initializing and configuring your application. If you select the OWIN middleware approach, you should be familiar with how the Startup class is used in configuring application middleware. The rest of this document will refer to the HTTP Module implementation simply as ASP.NET 4.x, and the OWIN implementation as ASP.NET OWIN.

>NOTE: You may wish to select the OWIN implementation for your ASP.NET 4.x application when you don't want to depend on `System.Web` or you also plan to use Steeltoe security providers for authentication/authorization on Cloud Foundry.

## Endpoint Listing

The following table describes the available Steeltoe management endpoints that can be used in an application:

|ID|Description|
|---|---|
| [cloudfoundry](./cloud-foundry.md) | Enables the management endpoint integration with Cloud Foundry. |
| [dump](./dump.md)  | Generates and reports a snapshot of the application's threads (Windows only). |
| [env](./env.md) | Reports the keys and values from the application's configuration. |
| [health](./health.md) | Customizable endpoint that gathers application health information. |
| [heapdump](./heapdump.md) | Generates and downloads a mini-dump of the application (Windows and Linux only). |
| [hypermedia](./hypermedia.md) | Provides the hypermedia endpoint for discovery of all available endpoints. |
| [info](./info.md) | Customizable endpoint that gathers arbitrary application information (such as Git Build info). |
| [loggers](./loggers.md) | Gathers existing loggers and allows modification of logging levels. |
| [mappings](./mappings.md) | Reports the configured ASP.NET routes and route templates. |
| [metrics](./metrics.md) | Reports the collected metrics for the application. |
| [refresh](./refresh.md) | Triggers the application configuration to be reloaded. |
| [trace](./tracing.md) | Gathers a configurable set of trace information (such as the last 100 HTTP requests). |

Each endpoint has an associated ID. When you want to expose that endpoint over HTTP, that ID is used in the mapped URL that exposes the endpoint. For example, the `health` endpoint below is mapped to `/health`.

>NOTE: When you want to integrate with the [TAS Apps Manager](https://docs.pivotal.io/pivotalcf/2-0/console/index.html), you need to configure the global management path prefix to be `/cloudfoundryapplication`. To do so, add `management:endpoints:path=/cloudfoundryapplication` to your configuration.

## Add NuGet References

To use the management endpoints, you need to add a reference to the appropriate Steeltoe NuGet based on the type of the application you are building and what Dependency Injector you have chosen, if any.

The following table describes the available packages:

|App Type|Package|Description|
|---|---|---|
|All|`Steeltoe.Management.EndpointBase`|Base functionality, no dependency injection, no HTTP middleware.|
|ASP.NET Core|`Steeltoe.Management.EndpointCore`|Includes `EndpointBase`, adds ASP.NET Core DI, includes HTTP middleware,  no TAS Apps Manager integration. |
|ASP.NET Core|`Steeltoe.Management.CloudFoundryCore`|Includes `EndpointCore`, enables TAS Apps Manager integration. |
|ASP.NET 4.x|`Steeltoe.Management.EndpointWeb`|Includes `EndpointBase`, enables TAS Apps Manager integration.|
|ASP.NET 4.x OWIN|`Steeltoe.Management.EndpointOwin`|Includes `EndpointBase`, enables TAS Apps Manager integration.|
|ASP.NET 4.x OWIN with Autofac|`Steeltoe.Management.EndpointOwinAutofac`|Includes `EndpointOwin`, adds Autofac DI, enables TAS Apps Manager integration.|

To add this type of NuGet to your project, add a `PackageReference` resembling the following:

```xml
<ItemGroup>
...
    <PackageReference Include="Steeltoe.Management.EndpointCore" Version="2.5.2" />
...
</ItemGroup>
```

or

```powershell
PM>Install-Package  Steeltoe.Management.EndpointWeb -Version 2.5.2
```

## Configure Global Settings

Endpoints can be configured by using the normal .NET [Configuration service](https://docs.microsoft.com/aspnet/core/fundamentals/configuration). You can globally configure settings that apply to all endpoints as well as configure settings that are specific to a particular endpoint.

All management endpoint settings should be placed under the prefix with the key `management:endpoints`. Any settings found under this prefix apply to all endpoints globally.

Settings that you want to apply to specific endpoints should be placed under the prefix with the key `management:endpoints:` + ID (for example, `management:endpoints:health`). Any settings you apply to a specific endpoint override any settings applied globally.

The following table describes the settings that you can apply globally:

|Key|Description|Default|
|---|---|---|
|enabled|Whether to enable all management endpoints|`true`|
|path|The path prefix applied to all endpoints when exposed over HTTP|`/actuator`|
|useStatusCodeFromResponse|Whether or not to use accurate status codes in some responses.|`true`|

>When running an application in IIS or with the HWC buildpack, response body content is automatically filtered out when the HTTP response code is 503. Some actuator responses intentionally return a code of 503 in failure scenarios. Setting `useStatusCodeFromResponse` to `false` will allow the response body to be returned by using a status code of 200 instead. This switch will not affect the status code of responses outside of Steeltoe.

When you want to integrate with the [TAS Apps Manager](https://docs.pivotal.io/pivotalcf/2-0/console/index.html), you need to configure the global management path prefix to be `/cloudfoundryapplication`.

### Exposing Endpoints

Since endpoints may contain sensitive information, only Health and Info are exposed by default. To change which endpoints are exposed, use the `include` and `exclude` properties:

|Property|Default|
|---|---|
|exposure:include | [`info`, `health`]|
|exposure:exclude | |

**Note**: **Each setting above must be prefixed with `management:endpoints:actuator`**. To select all endpoints,
`*`  can be used. For example, to expose everything except `env` and `refresh`, use the following property:

```json
"management": {
    "endpoints": {
        "actuator":{
            "exposure": {
                "include": [ "*" ],
                "exclude": [ "env", "refresh"]
            }
        }
    }
}
```

>NOTE: The exposure settings do not apply to endpoint routes mapped to the /cloudfoundryapplication context. If you add the Cloud Foundry endpoint, it will provide a route to access all endpoints without respecting the exposure settings through either the global path specified or its default of "/actuator". On the contrary, if you do not add either the Cloud Foundry or Hypermedia actuators, the default settings still apply. Adding endpoints other than health and info will require you to explicitly set the exposure setting.

The upcoming sections show the settings that you can apply to specific endpoints.

### HTTP Access ASP.NET Core

To expose any of the management endpoints over HTTP in an ASP.NET Core application:

1. Add a reference to `Steeltoe.Management.EndpointCore` or `Steeltoe.Management.CloudFoundryCore`.
1. Configure endpoint settings, as needed (for example, `appsettings.json`).
1. Add any additional "contributors" to the service container. (for example, `AddSingleton<IHealthContributor, CustomHealthContributor>()`)
1. `Add` the actuator endpoint to the service container (for example, `AddHealthActuator()`).
1. `Use` the actuator middleware to provide HTTP access (for example, `UseInfoActuator()`).

>NOTE: Each endpoint uses the same host and port as the application. The default path to each endpoint is specified in its section on this page, along with specific `Add` and `Use` method names.

Starting in Steeltoe version 2.4.0, extensions for both `IHostBuilder` and `IWebHostBuilder` are included to configure actuators with a single line of code in `program.cs`:

```csharp
    public static void Main(string[] args)
    {
        BuildWebHost(args).Run();
    }
    public static IHost BuildHost(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .AddCloudFoundryActuators()
            .AddRefreshActuator()
            .Build();
```

>NOTE: `AddCloudFoundryActuators()` and `AddLoggingActuator()` will automatically configure the DynamicConsoleLogger. If you wish to use the dynamic Serilog console logger, be sure to do so before adding actuators.

If you prefer to configure the actuators in `Startup.cs`, extensions are provided for `IServiceCollection` and `IApplicationBuilder` to configure and activate the actuator middlewares. If you wish to use all of the Steeltoe endpoints which integrate with the TAS Apps Manager, use `AddCloudFoundryActuators()` and `UseCloudFoundryActuators()` to add them all at once instead of including each individually, as shown in the following example:

```csharp
public class Startup
{
    public IConfiguration Configuration { get; }
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }
    public void ConfigureServices(IServiceCollection services)
    {
        ...
        // Add management endpoint services like this
        services.AddCloudFoundryActuators(Configuration);
        services.AddRefreshActuator(Configuration);
        ...
    }
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
        ...
        // Add management endpoints into pipeline like this
        app.UseCloudFoundryActuators();
        app.UseRefreshActuator();

        // Add ASP.NET Core MVC middleware to pipeline
        app.UseMvc();
        ...
    }
}
```

>NOTE: The order in which you add middleware to the [ASP.NET Core pipeline](https://docs.microsoft.com/aspnet/core/fundamentals/middleware/?view=aspnetcore-2.1&tabs=aspnetcore2x) is important. We recommend that you add the Steeltoe management endpoints before others to ensure proper operation.

### HTTP Access ASP.NET 4.x

To expose any of the management endpoints over HTTP in an ASP.NET 4.x application:

1. Add a reference to `Steeltoe.Management.EndpointWeb`.
1. Configure endpoint settings, as needed (for example, `appsettings.json`).
1. `Use` the middleware to provide HTTP access (for example, `UseInfoActuator()`).
1. If using Metrics, start/stop Diagnostics and MetricsExporting (for example, `DiagnosticsManager.Instance.Start()`)
1. Update web.config to allow extensionless requests to reach the actuators

```xml
<system.webServer>
    <handlers>
        <!--  This example is overly broad, it does not need to be used exactly as-is  -->
        <!-- Allow at least GET, POST and OPTIONS requests to go past IIS to actuators -->
        <add name="ExtensionlessUrlHandler-Integrated-4.0"
                path="*."
                verb="*"
                type="System.Web.Handlers.TransferRequestHandler"
                preCondition="integratedMode,runtimeVersionv4.0" />
    </handlers>
</system.webServer>
```

>NOTE: Each endpoint uses the same host and port as the application. The default path to each endpoint is specified in its section on this page, along with specific `Use` method name.

If you wish to use all of the Steeltoe endpoints which integrate with the TAS Apps Manager, call `UseCloudFoundryActuators()` to configure them all at once instead of including each individually, as shown in the following example:

```csharp
public class ManagementConfig
{
    public static IMetricsExporter MetricsExporter { get; set; }

    public static void ConfigureActuators(
        IConfiguration config,
        ILoggerProvider logger,
        IEnumerable<IHealthContributor> contrib,
        IApiExplorer api,
        ILoggerFactory factory = null)
    {
        ActuatorConfigurator.UseCloudFoundryActuators(config, logger, contrib, api, factory);
    }

    public static void Start()
    {
        DiagnosticsManager.Instance.Start();
        if (MetricsExporter != null)
        {
            MetricsExporter.Start();
        }
    }

    public static void Stop()
    {
        DiagnosticsManager.Instance.Stop();
        if (MetricsExporter != null)
        {
            MetricsExporter.Stop();
        }
    }
}
```

The above static methods should be called in `Global.asax.cs`.  In the `Application_Start()` method call `ConfigureActuators()`and `Start()` and in `Application_Stop()` call `Stop()`.  See the [Steeltoe Samples repository](https://github.com/SteeltoeOSS/Samples/tree/dev/Management/src/AspDotNet4) for more details.

### HTTP Access ASP.NET OWIN

To expose any of the management endpoints over HTTP in an ASP.NET 4.x application:

1. Add a reference to `Steeltoe.Management.EndpointOwin`.
1. Configure endpoint settings, as needed (for example, `appsettings.json`).
1. `Use` the middleware to provide HTTP access (for example, `UseInfoActuator()`).
1. If using Metrics, start/stop Diagnostics and MetricsExporting (for example, `DiagnosticsManager.Instance.Start()`)
1. If not self-hosting, add/update web.config entries to ensure OWIN startup and allow requests to reach the actuators

```xml
    <appSettings>
        <add key="owin:AutomaticAppStartup" value="true" />
    </appSettings>
    <system.webServer>
        <handlers>
            <!-- Allow GET, POST and OPTIONS requests to go past IIS to actuators -->
            <!-- Adjust the path value if you are not using TAS Apps Manager -->
            <add name="ApiURIs-ISAPI-Integrated-4.0"
                    path="cloudfoundryapplication/*"
                    verb="GET,POST,OPTIONS"
                    type="System.Web.Handlers.TransferRequestHandler"
                    preCondition="integratedMode,runtimeVersionv4.0" />
        </handlers>
    </system.webServer>
```

>NOTE: Each endpoint uses the same host and port as the application. The default path to each endpoint is specified in its section on this page, along with specific `Use` method name.

If you wish to use all of the Steeltoe endpoints which integrate with the TAS Apps Manager, use `UseCloudFoundryActuators()` to configure them all at once instead of including each individually, as shown in the following example:

```csharp
public class Startup
{
    private IMetricsExporter MetricsExporter { get; set; }

    public void Configuration(IAppBuilder app)
    {
        var config = GlobalConfiguration.Configuration;

        app.UseCloudFoundryActuators(
            ApplicationConfig.Configuration,
            GetHealthContributors(ApplicationConfig.Configuration),
            config.Services.GetApiExplorer(),
            LoggingConfig.LoggerProvider,
            LoggingConfig.LoggerFactory);

        Start();
    }

    private void Start()
    {
        DiagnosticsManager.Instance.Start();
        if (MetricsExporter != null)
        {
            MetricsExporter.Start();
        }
    }

    public void Stop()
    {
        DiagnosticsManager.Instance.Stop();
        if (MetricsExporter != null)
        {
            MetricsExporter.Stop();
        }
    }
}
```
