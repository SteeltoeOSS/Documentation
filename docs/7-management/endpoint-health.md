### Health

The Steeltoe Health management endpoint can be used to check and return the status of your running application. It can often be used by monitoring software to alert someone if a production system goes down. The information exposed by the `health` endpoint depends on the `management:endpoints:health:showdetails` property which can be configured with one of the following values:

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

Health information is collected from all [IHealthContributor](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointBase/Health/IHealthContributor.cs) implementations provided to the [HealthEndpoint](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointBase/Health/HealthEndpoint.cs). Steeltoe includes several `IHealthContributor` implementations out of the box that you can use, and, more importantly, you can write your own.

By default, the final application health state is computed by the [IHealthAggregator](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointBase/Health/IHealthAggregator.cs) that is provided to the `HealthEndpoint`. The `IHealthAggregator` is responsible for sorting out all of the returned statuses from each `IHealthContributor` and deriving an overall application health state. The [DefaultHealthAggregator](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointBase/Health/DefaultHealthAggregator.cs) returns the `worst` status returned from all of the `IHealthContributors`.

#### Health Contributors

At present, Steeltoe provides the following `IHealthContributor` implementations you can choose from:

|Name|Description|
|---|---|
|[DiskSpaceContributor](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointBase/Health/Contributor/DiskSpaceContributor.cs)|checks for low disk space, configure using [DiskSpaceContributorOptions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointBase/Health/Contributor/DiskSpaceContributorOptions.cs) |
|[RabbitMQHealthContributor](https://github.com/SteeltoeOSS/Connectors/blob/master/src/Steeltoe.CloudFoundry.ConnectorBase/Queue/RabbitMQHealthContributor.cs)|checks RabbitMQ connection health|
|[RedisHealthContributor](https://github.com/SteeltoeOSS/Connectors/blob/master/src/Steeltoe.CloudFoundry.ConnectorBase/Cache/RedisHealthContributor.cs)|checks Redis cache connection health|
|[RelationalHealthContributor](https://github.com/SteeltoeOSS/Connectors/blob/master/src/Steeltoe.CloudFoundry.ConnectorBase/Relational/RelationalHealthContributor.cs)|checks relational database connection health (MySql, Postgres, SqlServer)|

Many of the above health contributors are located in the [Steeltoe Connectors](https://github.com/SteeltoeOSS/Connectors) package and are made available to your application when you reference the Connectors package.

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

#### Configure Settings

The following table describes the settings that you can apply to the endpoint.

|Key|Description|Default|
|---|---|---|
|id|The ID of the health endpoint|`health`|
|enabled|Whether to enable the health management endpoint|true|
|sensitive|Currently not used|false|
|requiredPermissions|The user permissions required on Cloud Foundry to access endpoint|RESTRICTED|

**Note**: **Each setting above must be prefixed with `management:endpoints:health`**.

#### Enable HTTP Access

The default path to the Health endpoint is computed by combining the global `path` prefix setting together with the `id` setting from above. The default path is `/health`.

The coding steps you take to enable HTTP access to the Health endpoint together with how to use custom Health contributors differs depending on the type of .NET application your are developing.  The sections which follow describe the steps needed for each of the supported application types.

##### ASP.NET Core App

Refer to the [HTTP Access ASP.NET Core](#http-access-asp-net-core) section below to see the overall steps required to enable HTTP access to endpoints in an ASP.NET Core application.

To add the Health actuator to the service container, use any one of the `AddHealthActuator()` extension methods from [EndpointServiceCollectionExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointCore/Health/EndpointServiceCollectionExtensions.cs).

To add the Health actuator middleware to the ASP.NET Core pipeline, use the `UseHealthActuator()` extension method from [EndpointApplicationBuilderExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointCore/Health/EndpointApplicationBuilderExtensions.cs).

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

##### ASP.NET 4.x App

Refer to the [HTTP Access ASP.NET 4.x](#http-access-asp-net-4-x) section below to see the overall steps required to enable HTTP access to endpoints in an ASP.NET 4.x application.

To add the Health actuator endpoint, use the `UseHealthActuator()` method from [ActuatorConfigurator](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointWeb/ActuatorConfigurator.cs). Optionally you can provide a custom `IIHealthAggregator` and a list of `IHealthContributor`s should you want to customize the actuator endpoint.  If none are provided, defaults will be provided.

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

##### ASP.NET OWIN App

Refer to the [HTTP Access ASP.NET OWIN](#http-access-asp-net-owin) section below to see the overall steps required to enable HTTP access to endpoints in an ASP.NET 4.x OWIN application.

To add the Health actuator middleware to the ASP.NET OWIN pipeline, use the `UseHealthActuator()` extension method from [HealthEndpointAppBuilderExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointOwin/Health/HealthEndpointAppBuilderExtensions.cs).

The following example shows how enable the Health endpoint and use the default `IIHealthAggregator` together with two `IHealthContributor`s.

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
