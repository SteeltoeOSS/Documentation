# Microsoft SQL Server

This connector simplifies accessing [Microsoft SQL Server](https://www.microsoft.com/sql-server) databases.
It supports the following .NET drivers:

- [Microsoft.Data.SqlClient](https://www.nuget.org/packages/Microsoft.Data.SqlClient), which provides an ADO.NET `DbConnection`
- [System.Data.SqlClient](https://www.nuget.org/packages/System.Data.SqlClient), which provides an ADO.NET `DbConnection`
- [Microsoft.EntityFrameworkCore.SqlServer](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.SqlServer), which provides [Entity Framework Core](https://learn.microsoft.com/ef/core) support

The remainder of this topic assumes that you are familiar with the basic concepts of Steeltoe Connectors. See [Overview](./usage.md) for more information.

## Using the Microsoft SQL Server Connector

To use this connector:

1. Create a SQL Server instance or use [SQL Server Express LocalDB](https://learn.microsoft.com/sql/database-engine/configure-windows/sql-server-express-localdb).
1. Add NuGet references to your project.
1. Configure your connection string in `appsettings.json`.
1. Initialize the Steeltoe Connector at startup.
1. Use the driver-specific connection/client instance.

### Add NuGet References

To use this connector, add a NuGet reference to `Steeltoe.Connectors`. If you're using Entity Framework Core, add a
NuGet reference to `Steeltoe.Connectors.EntityFrameworkCore` instead.

Also add a NuGet reference to one of the .NET drivers listed above, as you would if you were not using Steeltoe.

### Configure connection string

The available connection string parameters for SQL Server are documented in the [Microsoft documentation](https://learn.microsoft.com/dotnet/api/microsoft.data.sqlclient.sqlconnection.connectionstring#remarks).

The following example `appsettings.json` uses SQL Server Express LocalDB:

```json
{
  "Steeltoe": {
    "Client": {
      "SqlServer": {
        "Default": {
          "ConnectionString": "Server=(localdb)\\mssqllocaldb;Database=SampleDB"
        }
      }
    }
  }
}
```

### Initialize Steeltoe Connector

Update your `Program.cs` as below to initialize the Connector:

```csharp
using Steeltoe.Connectors.SqlServer;

var builder = WebApplication.CreateBuilder(args);
builder.AddSqlServer();
```

### Use SqlConnection

To obtain a `SqlConnection` instance in your application, inject the Steeltoe factory in a controller or view:

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Steeltoe.Connectors;
using Steeltoe.Connectors.SqlServer;

public class HomeController : Controller
{
    public async Task<IActionResult> Index(
        [FromServices] ConnectorFactory<SqlServerOptions, SqlConnection> connectorFactory)
    {
        var connector = connectorFactory.Get();
        await using SqlConnection connection = connector.GetConnection();
        await connection.OpenAsync();

        SqlCommand command = connection.CreateCommand();
        command.CommandText = "SELECT 1";
        object? result = await command.ExecuteScalarAsync();

        ViewData["Result"] = result;
        return View();
    }
}
```

### Use Entity Framework Core

To retrieve data from SQL Server in your app using Entity Framework Core, use the following steps:

1. Define your `DbContext` class:

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

1. Call the `UseSqlServer()` Steeltoe extension method from `Program.cs` to initialize Entity Framework Core:

    ```csharp
    using Steeltoe.Connectors.EntityFrameworkCore.SqlServer;
    using Steeltoe.Connectors.SqlServer;

    var builder = WebApplication.CreateBuilder(args);
    builder.AddSqlServer();

    builder.Services.AddDbContext<AppDbContext>(
        (serviceProvider, options) => options.UseSqlServer(serviceProvider));
    ```

1. After you have configured and added your `DbContext` to the service container,
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

A complete sample app that uses Entity Framework Core with SQL Server is provided at https://github.com/SteeltoeOSS/Samples/tree/main/Connectors/src/SqlServerEFCore.

## Cloud Foundry

This Connector supports the following service brokers:

- [Tanzu Cloud Service Broker for Microsoft Azure](https://techdocs.broadcom.com/us/en/vmware-tanzu/platform-services/tanzu-cloud-service-broker-for-microsoft-azure/1-13/csb-azure/reference-azure-mssql-db.html)
- [Tanzu Cloud Service Broker for AWS](https://techdocs.broadcom.com/us/en/vmware-tanzu/platform-services/tanzu-cloud-service-broker-for-aws/1-14/csb-aws/reference-aws-mssql.html)

You can create and bind an instance to your application by using the Cloud Foundry CLI.

1. Create SQL Server service:

   ```shell
   cf create-service csb-azure-mssql-db your-plan sampleSqlServerService
   ```

1. Bind service to your app:

   ```shell
   cf bind-service sampleApp sampleSqlServerService
   ```

1. Restage the app to pick up change:

   ```shell
   cf restage sampleApp
   ```

