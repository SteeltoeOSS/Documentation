## HTTP Access ASP.NET Core

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

If you prefer to configure the actuators in `Startup.cs`, extensions are provided for `IServiceCollection` and `IApplicationBuilder` to configure and activate the actuator middlewares. If you wish to use all of the Steeltoe endpoints which integrate with the Pivotal Apps Manager, use `AddCloudFoundryActuators()` and `UseCloudFoundryActuators()` to add them all at once instead of including each individually, as shown in the following example:

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

>NOTE: The order in which you add middleware to the [ASP.NET Core pipeline](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-2.1&tabs=aspnetcore2x) is important. We recommend that you add the Steeltoe management endpoints before others to ensure proper operation.

## HTTP Access ASP.NET 4.x

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

If you wish to use all of the Steeltoe endpoints which integrate with the Pivotal Apps Manager, call `UseCloudFoundryActuators()` to configure them all at once instead of including each individually, as shown in the following example:

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

## HTTP Access ASP.NET OWIN

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
            <!-- Adjust the path value if you are not using Pivotal Apps Manager -->
            <add name="ApiURIs-ISAPI-Integrated-4.0"
                    path="cloudfoundryapplication/*"
                    verb="GET,POST,OPTIONS"
                    type="System.Web.Handlers.TransferRequestHandler"
                    preCondition="integratedMode,runtimeVersionv4.0" />
        </handlers>
    </system.webServer>
```

>NOTE: Each endpoint uses the same host and port as the application. The default path to each endpoint is specified in its section on this page, along with specific `Use` method name.

If you wish to use all of the Steeltoe endpoints which integrate with the Pivotal Apps Manager, use `UseCloudFoundryActuators()` to configure them all at once instead of including each individually, as shown in the following example:

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
