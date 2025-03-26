# Health

The Steeltoe health management endpoint can be used to check and return the status of your running application. It can often be used by monitoring software to alert someone if a production system goes down. The information exposed by the `health` endpoint depends on the `management:endpoints:health:showdetails` property which can be configured with one of the following values:

|Name|Description|
|---|---|
|`never`|Details are never shown.|
|`whenauthorized`|Details are only shown to authorized users. |
|`always`|Details are always shown.|

The default value is `always`. Authorized roles can be configured using `management:endpoints:health:claim or management:endpoints:health:role`. A user is considered to be authorized when they are in the given role or have the specified claim. For example:

```json
"management": {
    "endpoints": {
        "health": {
            "showdetails": "whenauthorized",
            "claim": {
                "type": "health_actuator",
                "value": "see_details"
            }
        }
    }
}
```

Health information is collected from all `IHealthContributor` implementations provided to the `HealthEndpoint`. Steeltoe includes several `IHealthContributor` implementations out of the box that you can use, and, more importantly, you can write your own.

By default, the final application health state is computed by the `IHealthAggregator` that is provided to the `HealthEndpoint`. The `IHealthAggregator` is responsible for sorting out all of the returned statuses from each `IHealthContributor` and deriving an overall application health state. The `DefaultHealthAggregator` returns the `worst` status returned from all of the `IHealthContributors`.

## Steeltoe Health Contributors

At present, Steeltoe provides the following `IHealthContributor` implementations you can choose from:

|Name|Description|
|---|---|
|`DiskSpaceContributor`|checks for low disk space, configure using `DiskSpaceContributorOptions`|
|`RabbitMQHealthContributor`|checks RabbitMQ connection health|
|`RedisHealthContributor`|checks Redis cache connection health|
|`RelationalHealthContributor`|checks relational database connection health (MySql, Postgres, SqlServer)|

Each of these contributors are located in the `Steeltoe.CloudFoundry.ConnectorBase` package and are made available to your application when you reference the connector package.

If you want to use any one of the `IHealthContributor`s above in an ASP.NET Core application, simply make use of the corresponding connector as you would normally. By doing so, the contributor is automatically added to the service container for you and is automatically discovered and used by the Health endpoint.

If you want to make use of any of the contributors in an ASP.NET 4.x application, where no service container exists, you must construct an instance of it using a factory method contained in the contributor and then provide it to the Health endpoint.

## Creating a Custom Health Contributor

If you wish to provide custom health information for your application, create a class that implements the `IHealthContributor` interface and then add that to the `HealthEndpoint`. Details on how to add a contributor to the endpoint is provided below.

The following is an example `IHealthContributor` that always returns a `HealthStatus` of `UP`.

```csharp
public class CustomHealthContributor : IHealthContributor
{
    public string Id => "CustomHealthContributor";

    public HealthCheckResult Health()
    {
        var result = new HealthCheckResult {
            // this is used as part of the aggregate, it is not directly part of the middleware response
            Status = HealthStatus.UP,
            Description = "This health check does not check anything"
        };
        result.Details.Add("status", HealthStatus.UP.ToString());
        return result;
    }
}
```

## Configure Settings

The following table describes the settings that you can apply to the endpoint.

|Key|Description|Default|
|---|---|---|
|id|The ID of the health endpoint|`health`|
|enabled|Whether to enable the health management endpoint|true|
|sensitive|Currently not used|false|
|requiredPermissions|The user permissions required on Cloud Foundry to access endpoint|RESTRICTED|

**Note**: **Each setting above must be prefixed with `management:endpoints:health`**.

## Enable HTTP Access

The default path to the Health endpoint is computed by combining the global `path` prefix setting together with the `id` setting from above. The default path is The default path is `/actuator/health`.

The coding steps you take to enable HTTP access to the Health endpoint together with how to use custom Health contributors differs depending on the type of .NET application your are developing.  The sections which follow describe the steps needed for each of the supported application types.

### ASP.NET Core App

Refer to the [Exposing Endpoints](./using-endpoints.md#exposing-endpoints) section to see the overall steps required to enable HTTP access to endpoints in an ASP.NET Core application.

To add the Health actuator to the service container, use any one of the `AddHealthActuator()` extension methods from `EndpointServiceCollectionExtensions`.

To add the Health actuator middleware to the ASP.NET Core pipeline, use the `UseHealthActuator()` extension method from `EndpointApplicationBuilderExtensions`.

The following example shows how enable the Health endpoint and to add a custom `IHealthContributor` to the service container by adding `CustomHealthContributor` as a singleton. Once that's done the Health endpoint will discover and use it during health checks.

```csharp
public class Startup
{
    ...
    public void ConfigureServices(IServiceCollection services)
    {
        // Add your own IHealthContributor, registered with the interface
        services.AddSingleton<IHealthContributor, CustomHealthContributor>();

        // Add health actuator
        services.AddHealthActuator(Configuration);

        // Add framework services.
        services.AddMvc();
    }
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
        app.UseStaticFiles();

        // Add management endpoint into pipeline
        app.UseHealthActuator();
    }
}
```

>NOTE: When you use any of the Steeltoe Connectors in your application we automatically add the corresponding health contributors to the service container.

### ASP.NET 4.x App

To add the Health actuator endpoint, use the `UseHealthActuator()` method from `ActuatorConfigurator`. Optionally you can provide a custom `IHealthAggregator` and a list of `IHealthContributor`s should you want to customize the actuator endpoint.  If none are provided, defaults will be provided.

The following example shows how enable the Health endpoint and use the default `IIHealthAggregator` together with two `IHealthContributor`s.

```csharp
public class ManagementConfig
{
    public static void ConfigureManagementActuators(IConfiguration configuration, ILoggerFactory loggerFactory = null)
    {
        ...
        ActuatorConfigurator.UseHealthActuator(
            configuration,
            new DefaultHealthAggregator(),
            GetHealthContributors(configuration),
            loggerFactory);
        ...
    }
    private static IEnumerable<IHealthContributor> GetHealthContributors(IConfiguration configuration)
    {
        var healthContributors = new List<IHealthContributor>
        {
            new DiskSpaceContributor(),
            RelationalHealthContributor.GetMySqlContributor(configuration)
        };
        return healthContributors;
    }
```

### ASP.NET OWIN App

To add the Health actuator middleware to the ASP.NET OWIN pipeline, use the `UseHealthActuator()` extension method from `HealthEndpointAppBuilderExtensions`.

The following example shows how enable the Health endpoint and use the default `IHealthAggregator` together with two `IHealthContributor`s.

```csharp
public class Startup
{
    ...
    public void Configuration(IAppBuilder app)
    {
        ...
        app.UseHealthActuator(
            new HealthOptions(ApplicationConfig.Configuration),
            new DefaultHealthAggregator(),
            GetHealthContributors(ApplicationConfig.Configuration),
            LoggingConfig.LoggerFactory);
        ...
    }
    private static IEnumerable<IHealthContributor> GetHealthContributors(IConfiguration configuration)
    {
        var healthContributors = new List<IHealthContributor>
        {
            new DiskSpaceContributor(),
            RelationalHealthContributor.GetMySqlContributor(configuration)
        };
        return healthContributors;
    }
}
```

## ASP NET Core Health Checks

ASP.NET Core also offers [Middleware and libraries](https://docs.microsoft.com/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-2.
) and abstractions for reporting health. There is wide community support for these abstractions from libraries such as [AspNetCore.Diagnostics.HealthChecks](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks). It is now possible to use these community provided health checks and make them available via this management health endpoint (for integration with PCF or any other infrastructure that depends on this format). In addition, Steeltoe connectors now exposes functionality to easily get connection information which is needed to setup these community health checks.

For example, to use the Steeltoe MySql connector but instead use ASP.NET Core community health checks, make these changes to Startup.cs:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Get connection information from Steeltoe helper
    var cm = new ConnectionStringManager(Configuration);
    var connectionString = cm.Get<MySqlConnectionInfo>().ConnectionString;

    // Add microsoft community health checks from xabaril
    services.AddHealthChecks().AddMySql(connectionString);

    // Add in a MySql connection (this method also adds an IHealthContributor for it)
    services.AddMySqlConnection(Configuration); // will now use community health check instead of Steeltoe health check

    // Add  Steeltoe Management endpoint services
    services.AddCloudFoundryActuators(Configuration);

    services.AddHealthChecksUI(); // Optionally use the health checks UI

    // Add framework services.
    services.AddMvc();
}
```

>NOTE: AddMySqlConnection will default to the ASP.NET Core health check if found in the service container. This behavior can be toggled off by passing AddMySqlConnection(Configuration, addSteeltoeHealthChecks: true) which will add both health checks. Be warned that this will make the Health check endpoint slower by calling multiple health checks for the same service.

  ```csharp
public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
{
  ...

  // Optionally use ASP.NET Core health middleware for community health checks at /Health
  app.UseHealthChecks("/Health", new HealthCheckOptions()
  {
      Predicate = _ => true,
      ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
  });

  // Optionally use health checks ui at /healthchecks-ui
  app.UseHealthChecksUI();

  // Add management endpoints into pipeline
  // Steeltoe health check shows up at /cloudfoundryapplication/health
  app.UseCloudFoundryActuators();
  ...
```

A complete example is available [here](https://github.com/SteeltoeOSS/Samples/tree/2.x/Management/src/AspDotNetCore/MicrosoftHealthChecks).
