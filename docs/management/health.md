# Health

You can use the Steeltoe `health` management endpoint to check and return the status of your running application. It is often used to monitor software and alert someone if a production system goes down. The information exposed by the `health` endpoint depends on the `Management:Endpoints:Health:ShowDetails` property, which can be configured with one of the following values:

| Name | Description |
| --- | --- |
| `Never` | Details are never shown. |
| `Whenauthorized` | Details are shown only to authorized users. |
| `Always` | Details are always shown. |

The default value is `always`. Authorized roles can be configured by using `Management:Endpoints:Health:Claim` or `Management:Endpoints:Health:Role`. A user is considered to be authorized when they are in the given role or have the specified claim. The following example uses `Management:Endpoints:Health:Claim`:

```json
{
    "Management": {
        "Endpoints": {
            "Health": {
                "ShowDetails": "whenauthorized",
                "Claim": {
                    "Type": "health_actuator",
                    "Value": "see_details"
                }
            }
        }
    }
}
```

Health information is collected from all `IHealthContributor` implementations provided to the `HealthEndpoint`. Steeltoe includes several `IHealthContributor` implementations out of the box that you can use. Also, and perhaps more importantly, you can write your own.

By default, the final application health state is computed by the `IHealthAggregator` that is provided to the `HealthEndpoint`. The `IHealthAggregator` is responsible for sorting out all of the returned statuses from each `IHealthContributor` and deriving an overall application health state. The `DefaultHealthAggregator` returns the `worst` status returned from all of the `IHealthContributors`.

## Configure Settings

The following table describes the settings that you can apply to the endpoint.

| Key | Description | Default |
| --- | --- | --- |
| `Id` | The ID of the `health` endpoint. | `health` |
| `Enabled` | Whether to enable the health management endpoint. | `true` |
| `Sensitive` | Currently not used. | `false` |
| `RequiredPermissions` | The user permissions required on Cloud Foundry to access endpoint. | `RESTRICTED` |

>NOTE: Each setting above must be prefixed with `Management:Endpoints:Health`.

## Enable HTTP Access

The default path to the `health` endpoint is computed by combining the global `Path` prefix setting together with the `Id` setting described in the previous section. The default path is `/health`.

See the [HTTP Access](/docs/3/management/using-endpoints#http-access) section to see the overall steps required to enable HTTP access to endpoints in an ASP.NET Core application.

To add the health actuator to the service container, use any one of the `AddHealthActuator()` extension methods from `EndpointServiceCollectionExtensions`.

To add the health actuator middleware to the ASP.NET Core pipeline, use the `UseHealthActuator()` extension method from `EndpointApplicationBuilderExtensions`.

The following example shows how to enable the `health` endpoint and add a custom `IHealthContributor` to the service container by adding `CustomHealthContributor` as a singleton. Once that is done, the `health` endpoint discovers and uses it during health checks.

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

>NOTE: When you use any of the Steeltoe Connectors in your application, we automatically add the corresponding health contributors to the service container.

## Steeltoe Health Contributors

At present, Steeltoe provides the following `IHealthContributor` implementations that you can choose from:

| Name | Description |
| --- | --- |
| `DiskSpaceContributor` | Checks for low disk space, configure using `DiskSpaceContributorOptions`. |
| `RabbitMQHealthContributor` | Checks RabbitMQ connection health. |
| `RedisHealthContributor` | Checks Redis cache connection health. |
| `RelationalHealthContributor` | Checks relational database connection health (MySql, Postgres, SqlServer). |

Each of these contributors are located in the `Steeltoe.ConnectorBase` package and are made available to your application when you reference the connector package.

If you want to use any one of the `IHealthContributor` instances in an ASP.NET Core application, make use of the corresponding connector as you would normally. By doing so, the contributor is automatically added to the service container for you and is automatically discovered and used by the `health` endpoint.

## Creating a Custom Health Contributor

If you wish to provide custom health information for your application, create a class that implements the `IHealthContributor` interface and then add that to the `HealthEndpoint`.

The following example `IHealthContributor` always returns a `HealthStatus` of `UP`:

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

## ASP NET Core Health Checks

ASP.NET Core also offers [middleware and libraries](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks) and abstractions for reporting health. There is wide community support for these abstractions from libraries such as [AspNetCore.Diagnostics.HealthChecks](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks). You can now use these community-provided health checks and make them available over this management health endpoint (for integration with PCF or any other infrastructure that depends on this format). In addition, Steeltoe connectors now expose functionality to get connection information, which is needed to setup these community health checks.

For example, to use the Steeltoe MySql connector but instead use ASP.NET Core community health checks, make the following changes to Startup.cs:

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

>NOTE: `AddMySqlConnection` defaults to the ASP.NET Core health check if found in the service container. You can toggle off this behavior by passing `AddMySqlConnection(Configuration, addSteeltoeHealthChecks: true)`, which adds both health checks. Be warned that this makes the Health check endpoint slower by calling multiple health checks for the same service.

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
}
```

A complete example is available [here](https://github.com/SteeltoeOSS/Samples/tree/master/Management/src/AspDotNetCore/MicrosoftHealthChecks).
