## Overview

Steeltoe Connectors provide access to both low-level connection strings and higher-level driver-specific .NET
connection/client instances. Local connection strings are merged with settings from the cloud.
[Health checks](../management/health.md) are registered for use with Steeltoe Management Actuators.
Additional extension methods are provided for integration with Entity Framework Core.

This page covers the basic concepts of Connectors, how they work, and how to use them.
The rest of the pages cover information that is specific to the various .NET drivers.

### Connection strings

At its core, a Steeltoe Connector reads a connection string from appsettings and exposes it via the
[ASP.NET Options pattern](https://learn.microsoft.com/aspnet/core/fundamentals/configuration/options).

For example, given the next `appsettings.json`:

```json
{
  "Steeltoe": {
    "Client": {
      "PostgreSql": {
        "Default": {
          "ConnectionString":
            "Server=localhost;Database=steeltoe;Uid=steeltoe;Pwd=steeltoe;Log Parameters=true"
        }
      }
    }
  }
}
```

and NuGet references to `Steeltoe.Connectors` and [Npgsql](https://www.nuget.org/packages/Npgsql),
the code fragment below reads the PostgreSQL connection string and prints it.

```csharp
using Microsoft.Extensions.Options;
using Steeltoe.Connectors.PostgreSql;

var builder = WebApplication.CreateBuilder();
builder.AddPostgreSql();

WebApplication app = builder.Build();

var options = app.Services.GetRequiredService<IOptions<PostgreSqlOptions>>();
Console.WriteLine(options.Value.ConnectionString);
```

This outputs:

```
Host=localhost;Database=steeltoe;Username=steeltoe;Password=steeltoe;Log Parameters=True
```

Notice that the parameter names are slightly different in the printed output.
This is because Steeltoe relies on the [DbConnectionStringBuilder](https://learn.microsoft.com/dotnet/api/system.data.common.dbconnectionstringbuilder)
of the [PostgreSQL .NET driver](https://www.npgsql.org/doc/api/Npgsql.NpgsqlConnectionStringBuilder.html), which normalizes the parameter names.
The advantage of this approach is that you're free to use any driver-specific connection string parameters,
without the need for Steeltoe to understand them first.

> [!NOTE]
> Earlier versions of Steeltoe assigned default values (such as "localhost") for required connection string parameters that were not specified
in configuration. This is no longer the case. In practice, this means you'll need to configure a connection string in most cases to run locally.

### Cloud-hosted applications

When your app is running in Cloud Foundry or Kubernetes, the Connector enriches the local connection string from `appsettings.json`
by merging any cloud-provided parameters into it. The resulting connection string is exposed via the Options pattern in the
same way as described above.

The Connector automatically detects when merging is needed:
- When running in Cloud Foundry, it reads the connection parameters from the JSON in the `VCAP_SERVICES` environment variable.
  For more information on `VCAP_SERVICES`, see the [Cloud Foundry documentation](https://docs.cloudfoundry.org/services/binding-credentials.html).
- When running in Kubernetes, it reads the secret files from the directory that the `SERVICE_BINDING_ROOT` environment variable points to.
  For more information on `SERVICE_BINDING_ROOT`, see the [Service Binding Specification for Kubernetes](https://github.com/servicebinding/spec).


### Named connection strings

Multiple service bindings can be accessed using named options. Just replace "Default" in your `appsettings.json` with the name
of the service binding.

For example, the next `appsettings.json` file contains two named connection strings:

```json
{
  "Steeltoe": {
    "Client": {
      "PostgreSql": {
        "MyServiceOne": {
          "ConnectionString":
            "Server=host1;Database=db1;Uid=user1;Pwd=pass1;Include Error Detail=true"
        },
        "MyServiceTwo": {
          "ConnectionString":
            "Server=host1;Database=db2;Uid=user2;Pwd=pass2;Log Parameters=true"
        }
      }
    }
  }
}
```

which can be accessed using the code fragment below.

```csharp
using Microsoft.Extensions.Options;
using Steeltoe.Connectors.PostgreSql;

var builder = WebApplication.CreateBuilder();
builder.AddPostgreSql();

WebApplication app = builder.Build();

var optionsMonitor = app.Services.GetRequiredService<IOptionsMonitor<PostgreSqlOptions>>();
Console.WriteLine(optionsMonitor.Get("MyServiceOne").ConnectionString);
Console.WriteLine(optionsMonitor.Get("MyServiceTwo").ConnectionString);
```

This outputs:

```
Host=host1;Database=db1;Username=user1;Password=pass1;Include Error Detail=True
Host=host1;Database=db2;Username=user2;Password=pass2;Log Parameters=True
```

When using cloud hosting, the local connection string names need to match the service binding names in the cloud for proper
merging to take place.

However, for convenience, if you have a single named service binding in the cloud, you can just use "Default" locally.
This is useful if you don't know the cloud-hosted service binding name upfront while developing your application.

## Connection/client instances

On top of the exposed connection string, a Steeltoe Connector provides a factory to obtain driver-specific connection/client instances.

The example below uses the same `appsettings.json` file from before, but obtains a connection from the factory:

```csharp
using Npgsql;
using Steeltoe.Connectors;
using Steeltoe.Connectors.PostgreSql;

var builder = WebApplication.CreateBuilder(args);
builder.AddPostgreSql();

WebApplication app = builder.Build();

var factory = app.Services.GetRequiredService<ConnectorFactory<PostgreSqlOptions, NpgsqlConnection>>();
var connector = factory.Get();

using NpgsqlConnection connection = connector.GetConnection();
connection.Open();
```

Likewise, the connection for a named service binding can be obtained by passing in its name:

```csharp
var connectorOne = factory.Get("MyServiceOne");

using NpgsqlConnection connectionOne = connectorOne.GetConnection();
connectionOne.Open();
```

Per named service, `ConnectorFactory` either returns a new connection/client instance each time or caches the first one,
based on documented best practices for the specific .NET driver.

## Legacy host builders

Steeltoe provides direct extension methods on `WebApplicationBuilder` for ease of use.

If you're using the legacy [IHostBuilder](https://learn.microsoft.com/aspnet/core/fundamentals/host/generic-host) or
[IWebHostBuilder](https://learn.microsoft.com/aspnet/core/fundamentals/host/web-host), you need to call two methods instead.

For example, when using MySQL with `IWebHostBuilder`, add a NuGet reference to
[MySqlConnector](https://www.nuget.org/packages/MySqlConnector) and call both `ConfigureMySql()` and `AddMySql()`:

```csharp
using Microsoft.AspNetCore;
using Steeltoe.Connectors.MySql;

internal static class Program
{
    public static void Main(string[] args)
    {
        CreateWebHostBuilder(args).Build().Run();
    }

    private static IWebHostBuilder CreateWebHostBuilder(string[] args)
    {
        return WebHost.CreateDefaultBuilder(args)
            .UseStartup<Startup>()
            .ConfigureAppConfiguration(
                configurationBuilder => configurationBuilder.ConfigureMySql())
            .ConfigureServices(
                (context, services) => services.AddMySql(context.Configuration));
    }
}
```

## Entity Framework Core

To use the merged connection string with Entity Framework Core, add a NuGet reference to `Steeltoe.Connectors.EntityFrameworkCore`
and call the corresponding `Use*` Steeltoe method, in addition to adding the Connector to the host builder as shown before.

For example, given the next `appsettings.json`:

```json
{
  "Steeltoe": {
    "Client": {
      "SqlServer": {
        "Default": {
          "ConnectionString":
            "Server=(localdb)\\mssqllocaldb;Database=ExampleDB;Pooling=true"
        }
      }
    }
  }
}
```

After adding a NuGet reference to `Steeltoe.Connectors.EntityFrameworkCore` and [Microsoft.EntityFrameworkCore.SqlServer](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.SqlServer),
the code fragment below sets the SQL Server connection string in Entity Framework Core and prints it.

```csharp
using Microsoft.EntityFrameworkCore;
using Steeltoe.Connectors.EntityFrameworkCore.SqlServer;
using Steeltoe.Connectors.SqlServer;

var builder = WebApplication.CreateBuilder(args);
builder.AddSqlServer();

builder.Services.AddDbContext<AppDbContext>(
    (serviceProvider, options) => options.UseSqlServer(serviceProvider));

WebApplication app = builder.Build();

await using AsyncServiceScope scope = app.Services.CreateAsyncScope();
var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
Console.WriteLine(dbContext.Database.GetConnectionString());

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
}
```

This outputs:

```
Data Source=(localdb)\mssqllocaldb;Initial Catalog=ExampleDB;Pooling=True
```

Notice that the `UseSqlServer()` method from the `Steeltoe.Connectors.SqlServer` namespace is used,
instead of the one provided by Microsoft. The Steeltoe method obtains the connection string, and then delegates to
the Microsoft-provided method.

To use multiple named service bindings, use an overload that takes the service binding name. For example:

```csharp
builder.Services.AddDbContext<AppDbContextOne>(
    (serviceProvider, options) => options.UseMySql(serviceProvider, "MyServiceOne"));

builder.Services.AddDbContext<AppDbContextTwo>(
    (serviceProvider, options) => options.UseMySql(serviceProvider, "MyServiceTwo"));
```

## Advanced settings

Usage of the ASP.NET Options pattern by Steeltoe Connectors enables configuring them using custom code.

For example, the code below adds the application name to the MongoDB connection URL at runtime:

```csharp
using MongoDB.Driver;
using Steeltoe.Connectors.MongoDb;

var builder = WebApplication.CreateBuilder(args);
builder.AddMongoDb();

builder.Services.Configure<MongoDbOptions>(options =>
{
    var urlBuilder = new MongoUrlBuilder(options.ConnectionString)
    {
        ApplicationName = "mongodb-example-app"
    };

    options.ConnectionString = urlBuilder.ToString();
});
```

The Steeltoe Connector extension methods provide overloads to influence how they work. This enables to hook in custom logic
to construct the connection/client instance and override its lifetime. Or override if and how health checks are registered.
See the documentation on `ConnectorConfigureOptionsBuilder` and `ConnectorAddOptionsBuilder` for details.

The example below overrides the creation of the Redis `ConnectionMultiplexer` instance and turns off health checks:

```csharp
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Steeltoe.Connectors.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.AddRedis(null, addOptions =>
{
    // Override the built-in logic that creates the ConnectionMultiplexer instance.
    addOptions.CreateConnection = (serviceProvider, serviceBindingName) =>
    {
        // Obtain connection string from named options.
        var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<RedisOptions>>();
        RedisOptions options = optionsMonitor.Get(serviceBindingName);

        // Create ConnectionMultiplexer with a custom time-out.
        ConfigurationOptions redisOptions = ConfigurationOptions.Parse(options.ConnectionString);
        redisOptions.ConnectTimeout = 30;
        return ConnectionMultiplexer.Connect(redisOptions);
    };

    // Turn off health checks.
    addOptions.EnableHealthChecks = false;
});
```

The Steeltoe Connector extension methods for Entity Framework Core take an optional parameter to configure the driver-specific
settings. But because Steeltoe has no compile-time reference to the drivers, you need to upcast its parameter first.

The next example activates the retry policy on the SQL Server provider for Entity Framework Core:

```csharp
using Microsoft.EntityFrameworkCore.Infrastructure;
using Steeltoe.Connectors.EntityFrameworkCore.SqlServer;
using Steeltoe.Connectors.SqlServer;

var builder = WebApplication.CreateBuilder(args);
builder.AddSqlServer();

builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
    options.UseSqlServer(serviceProvider, null, untypedOptions =>
    {
        var sqlServerOptions = (SqlServerDbContextOptionsBuilder)untypedOptions;
        sqlServerOptions.EnableRetryOnFailure();
    }));
```
