# PostgreSQL

This connector simplifies accessing [PostgreSQL](https://www.postgresql.org/) databases.
It supports the following .NET drivers:
- [Npgsql](https://www.nuget.org/packages/Npgsql), which provides an ADO.NET `DbConnection`.
- [Npgsql.EntityFrameworkCore.PostgreSQL](https://www.nuget.org/packages/Npgsql.EntityFrameworkCore.PostgreSQL), which provides [Entity Framework Core](https://learn.microsoft.com/ef/core) support.

The remainder of this page assumes you're familiar with the [basic concepts of Steeltoe Connectors](./usage.md).

## Usage

To use this connector:

1. Create a PostgreSQL server instance or use a [docker container](https://github.com/SteeltoeOSS/Samples/blob/main/CommonTasks.md#postgresql).
1. Add NuGet references to your project.
1. Configure your connection string in `appsettings.json`.
1. Initialize the Steeltoe Connector at startup.
1. Use the driver-specific connection/client instance.

### Add NuGet References

To use this connector, add a NuGet reference to `Steeltoe.Connectors`. If you're using Entity Framework Core, add a
NuGet reference to `Steeltoe.Connectors.EntityFrameworkCore` instead.

Also add a NuGet reference to one of the .NET drivers listed above, as you would if you were not using Steeltoe.

### Configure connection string

The available connection string parameters for PostgreSQL are documented [here](https://www.npgsql.org/doc/connection-string-parameters.html).

The following example `appsettings.json` uses the docker container from above:

```json
{
  "Steeltoe": {
    "Client": {
      "PostgreSql": {
        "Default": {
          "ConnectionString": "Server=localhost;Database=steeltoe;Uid=steeltoe;Pwd=steeltoe"
        }
      }
    }
  }
}
```

### Initialize Steeltoe Connector

Update your `Program.cs` as below to initialize the Connector:

```csharp
using Steeltoe.Connectors.PostgreSql;

var builder = WebApplication.CreateBuilder(args);
builder.AddPostgreSql();
```

### Use NpgsqlConnection

To obtain an `NpgsqlConnection` instance in your application, inject the Steeltoe factory in a controller or view:

```csharp
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Steeltoe.Connectors;
using Steeltoe.Connectors.PostgreSql;

public class HomeController : Controller
{
    public async Task<IActionResult> Index(
        [FromServices] ConnectorFactory<PostgreSqlOptions, NpgsqlConnection> connectorFactory)
    {
        var connector = connectorFactory.Get();
        await using NpgsqlConnection connection = connector.GetConnection();
        await connection.OpenAsync();

        NpgsqlCommand command = connection.CreateCommand();
        command.CommandText = "SELECT 1";
        object? result = await command.ExecuteScalarAsync();

        ViewData["Result"] = result;
        return View();
    }
}
```

A complete sample app that uses `NpgsqlConnection` is provided at https://github.com/SteeltoeOSS/Samples/tree/main/Connectors/src/PostgreSql.

### Use Entity Framework Core

Start by defining your `DbContext` class:
```csharp
public class AppDbContext : DbContext
{
    public DbSet<SampleEntity> SampleEntities => Set<SampleEntity>();

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
}

public class SampleEntity
{
    public long Id { get; set; }
    public string? Text { get; set; }
}
```

Next, call the `UseNpgsql()` Steeltoe extension method from `Program.cs` to initialize Entity Framework Core:

```csharp
using Steeltoe.Connectors.EntityFrameworkCore.PostgreSql;
using Steeltoe.Connectors.PostgreSql;

var builder = WebApplication.CreateBuilder(args);
builder.AddPostgreSql();

builder.Services.AddDbContext<AppDbContext>(
    (serviceProvider, options) => options.UseNpgsql(serviceProvider));
```

Once you have configured and added your `DbContext` to the service container,
you can inject it and use it in a controller or view:

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class HomeController : Controller
{
    public async Task<IActionResult> Index([FromServices] AppDbContext appDbContext)
    {
        List<SampleEntity> entities = await appDbContext.SampleEntities.ToListAsync();
        return View(entities);
    }
}
```

A complete sample app that uses Entity Framework Core with PostgreSQL is provided at https://github.com/SteeltoeOSS/Samples/tree/main/Connectors/src/PostgreSqlEFCore.

## Cloud Foundry

This Connector supports the following service brokers:
- [VMware Tanzu Cloud Service Broker for Azure](https://docs.vmware.com/en/Tanzu-Cloud-Service-Broker-for-Azure/1.4/csb-azure/GUID-index.html)
- [VMware Tanzu Cloud Service Broker for GCP](https://docs.vmware.com/en/Tanzu-Cloud-Service-Broker-for-GCP/1.2/csb-gcp/GUID-index.html)

You can create and bind an instance to your application by using the Cloud Foundry CLI:

```bash
# Create PostgreSQL service
cf create-service csb-azure-postgresql small myPostgreSqlService

# Bind service to your app
cf bind-service myApp myPostgreSqlService

# Restage the app to pick up change
cf restage myApp
```

## Kubernetes

This Connector supports the [Service Binding Specification for Kubernetes](https://github.com/servicebinding/spec).
It can be used through the Bitnami [Services Toolkit](https://docs.vmware.com/en/VMware-Tanzu-Application-Platform/1.5/tap/services-toolkit-install-services-toolkit.html).

For details on how to use this, see the instructions at https://github.com/SteeltoeOSS/Samples/tree/main/Connectors/src/PostgreSql#running-on-tanzu-application-platform-tap.
