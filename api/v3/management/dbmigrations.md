# Database Migrations

The Steeltoe `dbmigration` endpoint exposes information about data migrations that are available to an application's data source that has been built with Entity Framework Core (EF Core). EF Core migrations gives developers the ability to update an application's database schema while staying consistent with the application's data model without removing any existing data. 

>NOTE: <i>Please review Microsoft's [EF Core Migrations Overview](https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/?tabs=dotnet-core-cli) for more in-depth information</i>

## Configure Settings

The following table describes the settings that you can apply to the endpoint:

| Key | Description | Default |
| --- | --- | --- |
| `Enabled` | Whether to enable the dbmigrations management endpoint. | `true` |
| `Path` | Url path used to access dbmigrations management endpoint. | `dbmigrations` |

>Each setting above must be prefixed with `Management:Endpoints:DbMigrations`.

## Enable HTTP Access

The default path to the DbMigrations endpoint is computed by combining the global `Path` prefix setting together with the `Id` setting described in the preceding section. The default path is `/actuator/dbmigrations`.

See the [HTTP Access](./using-endpoints.md#http-access) section to see the overall steps required to enable HTTP access to endpoints in an ASP.NET Core application.

To add the actuator to the service container and map its route, use the `AddDbMigrationsActuator` extension method from `ManagementHostBuilderExtensions`.

The following example shows how to use the dbmigrations actuator endpoint:

<i>Program.cs</i>

```csharp
public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        })
        .AddDbMigrationsActuator();
```

Alternatively, first, add the DbMigrations actuator to the service container, using the `AddDbMigrationsActuator()` extension method from `EndpointServiceCollectionExtensions`.

Then, add the DbMigrations actuator middleware to the ASP.NET Core pipeline, use the `Map<DbMigrationsEndpoint>()` extension method from `ActuatorRouteBuilderExtensions`.

<i>Startup.cs</i>

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Add Db Migration Actuator
    services.AddDbMigrationsActuator();
    
    // Other registrations...
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    // Other application configuration methods...

    app.UseEndpoints(endpoints =>
    {
        endpoints.Map<DbMigrationsEndpoint>();

        // other endpoint routing methods...
    });
}
```

## Sample Output

<i>Default Endpoint: `/actuator/dbmigrations`</i>

```json
{
    "TestDataContext": 
    {
        "pendingMigrations": [
            "2021078158434_AddNewTable"
        ],
        "appliedMigrations": [
            "20210716155831_InitialModelCreate"
            "2021078155543_AddDbColumn"
        ]
    }
}
```
