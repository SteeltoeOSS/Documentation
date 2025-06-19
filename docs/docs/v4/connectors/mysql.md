# MySQL

This connector simplifies accessing [MySQL](https://www.mysql.com/) databases.
It supports the following .NET drivers:

- [MySqlConnector](https://www.nuget.org/packages/MySqlConnector), which provides an ADO.NET `DbConnection`
- [MySql.Data](https://www.nuget.org/packages/MySql.Data), which provides an ADO.NET `DbConnection`
- [Pomelo.EntityFrameworkCore.MySql](https://www.nuget.org/packages/Pomelo.EntityFrameworkCore.MySql), which provides [Entity Framework Core](https://learn.microsoft.com/ef/core) support
- [MySql.EntityFrameworkCore](https://www.nuget.org/packages/MySql.EntityFrameworkCore), which provides [Entity Framework Core](https://learn.microsoft.com/ef/core) support

The remainder of this topic assumes that you are familiar with the basic concepts of Steeltoe Connectors. See [Overview](./usage.md) for more information.

## Using the MySQL connector

To use this connector:

1. Create a MySQL server instance or use a [docker container](https://github.com/SteeltoeOSS/Samples/blob/main/CommonTasks.md#mysql).
1. Add NuGet references to your project.
1. Configure your connection string in `appsettings.json`.
1. Initialize the Steeltoe Connector at startup.
1. Use the driver-specific connection/client instance.

### Add NuGet References

To use this connector, add a NuGet reference to `Steeltoe.Connectors`.
If you're using Entity Framework Core, add a NuGet reference to `Steeltoe.Connectors.EntityFrameworkCore` instead.

Also add a NuGet reference to one of the .NET drivers listed above, as you would if you were not using Steeltoe.

### Configure connection string

For the available connection string parameters for MySQL, see:

* [MySQL Connector documentation](https://mysqlconnector.net/connection-options/)
* [MySQL documentation](https://dev.mysql.com/doc/refman/8.0/en/connecting-using-uri-or-key-value-pairs.html#connection-parameters-base)

The following example `appsettings.json` uses the docker container referred to earlier:

```json
{
  "Steeltoe": {
    "Client": {
      "MySql": {
        "Default": {
          "ConnectionString": "Server=localhost;Database=steeltoe;Uid=steeltoe;Pwd=steeltoe"
        }
      }
    }
  }
}
```

### Initialize Steeltoe Connector

Update your `Program.cs` as shown here to initialize the Connector:

```csharp
using Steeltoe.Connectors.MySql;

var builder = WebApplication.CreateBuilder(args);
builder.AddMySql();
```

### Use MySqlConnection

To obtain a `MySqlConnection` instance in your application, inject the Steeltoe factory in a controller or view:

```csharp
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using Steeltoe.Connectors;
using Steeltoe.Connectors.MySql;

public class HomeController : Controller
{
    public async Task<IActionResult> Index(
        [FromServices] ConnectorFactory<MySqlOptions, MySqlConnection> connectorFactory)
    {
        var connector = connectorFactory.Get();
        await using MySqlConnection connection = connector.GetConnection();
        await connection.OpenAsync();

        MySqlCommand command = connection.CreateCommand();
        command.CommandText = "SELECT 1";
        object? result = await command.ExecuteScalarAsync();

        ViewData["Result"] = result;
        return View();
    }
}
```

A complete sample app that uses `MySqlConnection` is provided at https://github.com/SteeltoeOSS/Samples/tree/main/Connectors/src/MySql.

### Use Entity Framework Core

To retrieve data from MySQL in your app using Entity Framework Core, use the following steps:

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

1. Call the `UseMySql()` Steeltoe extension method from `Program.cs` to initialize Entity Framework Core:

    ```csharp
    using Steeltoe.Connectors.EntityFrameworkCore.MySql;
    using Steeltoe.Connectors.MySql;

    var builder = WebApplication.CreateBuilder(args);
    builder.AddMySql();

    builder.Services.AddDbContext<AppDbContext>(
        (serviceProvider, options) => options.UseMySql(serviceProvider));
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

A complete sample app that uses Entity Framework Core with MySQL is provided at https://github.com/SteeltoeOSS/Samples/tree/main/Connectors/src/MySqlEFCore.

## Cloud Foundry

This Connector supports the following service brokers:

- [Tanzu for MySQL on Cloud Foundry](https://techdocs.broadcom.com/us/en/vmware-tanzu/data-solutions/tanzu-for-mysql-on-cloud-foundry/10-0/mysql-for-tpcf/use.html)
- [Tanzu Cloud Service Broker for GCP](https://techdocs.broadcom.com/us/en/vmware-tanzu/platform-services/tanzu-cloud-service-broker-for-gcp/1-9/csb-gcp/reference-gcp-mysql.html)
- [Tanzu Cloud Service Broker for AWS](https://techdocs.broadcom.com/us/en/vmware-tanzu/platform-services/tanzu-cloud-service-broker-for-aws/1-14/csb-aws/reference-aws-mysql.html)

You can create and bind an instance to your application by using the Cloud Foundry CLI.

1. Create MySQL service:

   ```shell
   cf create-service p.mysql db-small myMySqlService
   ```

1. Bind service to your app:

   ```shell
   cf bind-service myApp myMySqlService
   ```

1. Restage the app to pick up change:

   ```shell
   cf restage myApp
   ```

## Kubernetes

This Connector supports the [Service Binding Specification for Kubernetes](https://github.com/servicebinding/spec).
It can be used through the [Services Toolkit](https://techdocs.broadcom.com/us/en/vmware-tanzu/standalone-components/tanzu-application-platform/1-12/tap/services-toolkit-install-services-toolkit.html).

For details on how to use this, see the instructions at https://github.com/SteeltoeOSS/Samples/tree/main/Connectors/src/MySql#running-on-tanzu-platform-for-kubernetes.
