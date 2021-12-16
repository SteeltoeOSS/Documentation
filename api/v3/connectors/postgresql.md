# PostgreSQL

This connector simplifies using PostgreSQL. Currently, the connector supports the [Npgsql](https://www.npgsql.org/) provider.

This connector provides an `IHealthContributor`, which you can use in conjunction with the [Steeltoe Management Health](../management/health.md) check endpoint.

## Usage

You should know how the .NET [Configuration service](https://docs.microsoft.com/aspnet/core/fundamentals/configuration) works before starting to use the connector. You need a basic understanding of `ConfigurationBuilder` and how to add providers to the builder to configure the connector.

You should also know how the ASP.NET Core [Startup](https://docs.microsoft.com/aspnet/core/fundamentals/startup) class is used to configure the application services for the app. Pay particular attention to the `ConfigureServices()` method.

To use this connector:

1. Create a PostgreSQL Service instance and bind it to your application.
1. Optionally, Configure any PostgreSQL client settings (such as `appsettings.json`).
1. Optionally, add the Steeltoe Cloud Foundry config provider to your `ConfigurationBuilder`.
1. Add `NpgsqlConnection` or `DbContext` to your `IServiceCollection`.

### Add NuGet Reference

To use the PostgreSQL connector, add your choice of PostgreSQL package between [Npgsql](https://www.nuget.org/packages/Npgsql/) and [Npgsql.EntityFrameworkCore.PostgreSQL](https://www.nuget.org/packages/Npgsql.EntityFrameworkCore.PostgreSQL/) as you would if you were not using Steeltoe. Then [add a reference to the appropriate Steeltoe Connector NuGet package](usage.md#add-nuget-references).

>Steeltoe does not currently include direct support for PostgreSQL with Entity Framework 6.

### Configure Settings

The PostgreSQL connector supports several settings for creating the `NpgsqlConnection` to a database. This can be useful when you develop and test an application locally and need to configure the connector for non-default settings.

The following example shows a PostgreSQL connector configuration (in JSON) to set up a connection to a database at `myserver:5432`:

```json
{
  ...
  "Postgres": {
    "Client": {
      "Host": "myserver",
      "Port": 5432
    }
  }
  ...
}
```

The following table describes all of the possible settings for the connector:

|Key|Description |Default
| --- | --- | --- |
| `Host` | Hostname or IP Address of server. | `localhost` |
| `Port` | Port number of server. | 5432 |
| `Username` | Username for authentication. | not set |
| `Password` | Password for authentication. | not set |
| `Database` | Schema to which to connect. | not set |
| `ConnectionString` | Full connection string. | Built from settings |

>IMPORTANT: All of these settings should be prefixed with `Postgres:Client:`.

The samples and most templates are already set up to read from `appsettings.json`.

>If a `ConnectionString` is provided and `VCAP_SERVICES` are not detected (a typical scenario for local application development), the `ConnectionString` is used exactly as provided.

### Cloud Foundry

To use PostgreSQL on Cloud Foundry, after a PostgreSQL service is installed, you can create and bind an instance of it to your application by using the Cloud Foundry CLI:

```bash
# Create PostgreSQL service
cf create-service EDB-Shared-PostgreSQL "Basic PostgreSQL Plan" myPostgres

# Bind service to `myApp`
cf bind-service myApp myPostgres

# Restage the app to pick up change
cf restage myApp
```

>The preceding commands work for the PostgreSQL service provided by EDB on Cloud Foundry. For another service, adjust the `create-service` command to fit your environment.

This connector also works with the [Azure Service Broker](https://docs.pivotal.io/partners/azure-sb/).

Once the service is bound to your application, the connector's settings are available in `VCAP_SERVICES`.

### Add NpgsqlConnection

To use a `NpgsqlConnection` in your application, add it to the service container in the `ConfigureServices()` method of the `Startup` class:

```csharp
using Steeltoe.Connector.PostgreSql;

public class Startup {
    ...
    public IConfiguration Configuration { get; private set; }
    public Startup(...)
    {
      ...
    }
    public void ConfigureServices(IServiceCollection services)
    {
        // Add NpgsqlConnection
        services.AddPostgresConnection(Configuration);

        // Add framework services.
        services.AddMvc();
        ...
    }
    ...
}
```

The `AddPostgresConnection(Configuration)` method call configures the `NpgsqlConnection` by using the configuration built by the application and adds the connection to the service container.

> By default, this extension method will automatically configure an `IHealthContributor` to report the health of this database connection. This behavior can be turned off by passing `false` for the parameter `addSteeltoeHealthChecks`

### Use NpgsqlConnection

Once the connection is configured and added to the service container, you can inject it and use it in a controller or a view:

```csharp
using Npgsql;
...
public class HomeController : Controller
{
    public IActionResult PostgresData([FromServices] NpgsqlConnection dbConnection)
    {
        dbConnection.Open();

        NpgsqlCommand cmd = new NpgsqlCommand("SELECT * FROM TestData;", dbConnection);
        var rdr = cmd.ExecuteReader();

        while (rdr.Read())
        {
            ViewData["Key" + rdr[0]] = rdr[1];
        }

        rdr.Close();
        dbConnection.Close();

        return View();
    }
}
```

### Add DbContext

To use Entity Framework, inject and use a `DbContext` in your application instead of a `NpgsqlConnection` through the `AddDbContext<>()` method:

```csharp
using Steeltoe.Connector.PostgreSql.EFCore;

public class Startup {
    public IConfiguration Configuration { get; private set; }
    public Startup(...)
    {
      ...
    }
    public void ConfigureServices(IServiceCollection services)
    {
        // Add EFCore TestContext configured with a PostgreSQL configuration
        services.AddDbContext<TestContext>(options => options.UseNpgsql(Configuration));

        // see note below explaining AddPostgresHealthContributor
        services.AddPostgresHealthContributor(Configuration);
        ...
    }
}
```

The `AddDbContext<TestContext>(options => options.UseNpgsql(Configuration));` method call configures the `TestContext` by using the configuration built by the application and adds the context to the service container.

> This extension method will _NOT_ automatically configure an `IHealthContributor` to report the health of this database connection. The package Steeltoe.Connector.ConnectorCore provides an `IServiceCollection` extension method that will. Directly add the health contributor with the code `services.AddPostgresHealthContributor(Configuration)`

The following example shows how you would define the `DbContext`:

```csharp
using Microsoft.EntityFrameworkCore;
...

public class TestContext : DbContext
{
    public TestContext(DbContextOptions options) : base(options)
    {
    }
    public DbSet<TestData> TestData { get; set; }
}
```

If you need to set additional properties for the `DbContext` like `MigrationsAssembly` or connection retry settings, create an `Action<NpgsqlDbContextOptionsBuilder>`:

```csharp
Action<NpgsqlDbContextOptionsBuilder> npgsqlOptionsAction = (o) =>
{
  o.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
  // Configuring Connection Resiliency: https://docs.microsoft.com/ef/core/miscellaneous/connection-resiliency
  o.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
};
```

Pass your new options action into the AddDbContext method:

```csharp
services.AddDbContext<TestContext>(options => options.UseNpgsql(Configuration, npgsqlOptionsAction));
```

### Use DbContext

Once you have configured and added the context to the service container, you can inject it and use it in a controller or a view:

```csharp
using Project.Models;
...
public class HomeController : Controller
{
    public IActionResult PostgresData([FromServices] TestContext context)
    {
        return View(context.TestData.ToList());
    }
}
```
