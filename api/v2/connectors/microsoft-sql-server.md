# Microsoft SQL Server

This connector simplifies using Microsoft SQL Server in an application running on Cloud Foundry. The connector is built to work with `System.Data.SqlClient` and provides additional extension methods for using Entity Framework.

This connector provides a `IHealthContributor` which you can use in conjunction with the [Steeltoe Management Health](../management/health.md) check endpoint.

## Usage

You should know how the new .NET [Configuration service](https://docs.microsoft.com/aspnet/core/fundamentals/configuration) works before starting to use the connector. You need a basic understanding of the `ConfigurationBuilder` and how to add providers to the builder to configure the connector.

You should also know how the ASP.NET Core [Startup](https://docs.microsoft.com/aspnet/core/fundamentals/startup) class is used in configuring the application services. Pay particular attention to the usage of the `ConfigureServices()` method.

To use this connector:

1. Create a Microsoft SQL Service instance and bind it to your application.
1. Optionally, configure any Microsoft SQL Server client settings (such as `appsettings.json`) you need.
1. Add the Steeltoe Cloud Foundry configuration provider to your `ConfigurationBuilder`.
1. Add `SqlConnection` or `DbContext` to your `IServiceCollection`.

### Add NuGet Reference

To use the Microsoft SQL Server connector, add your choice of Microsoft SQL Server package between [System.Data.SqlClient](https://www.nuget.org/packages/System.Data.SqlClient/), [Entity Framework](https://www.nuget.org/packages/EntityFramework/) and [Microsoft.EntityFrameworkCore.SqlServer](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.SqlServer/), as you would if you weren't using Steeltoe. Then, [add a reference to the appropriate Steeltoe Connector NuGet package](usage.md#add-nuget-references).

### Configure Settings

The Microsoft SQL Server connector supports several configuration options. These settings can be used to develop or test an application locally and then be overridden during deployment.

The following Microsoft SQL Server connector configuration shows how to connect to SQL Server 2016 Express LocalDB:

```json
{
  ...
  "sqlserver": {
    "credentials": {
        "connectionString": "Server=(localdb)\\mssqllocaldb;database=Steeltoe;Trusted_Connection=True;"
    }
  }
  ...
}
```

The following table shows the available settings for the connector:

|Key|Description|Steeltoe Default|
|---|---|---|
|server|Hostname or IP Address of server|localhost|
|port|Port number of server|1433|
|username|Username for authentication|not set|
|password|Password for authentication|not set|
|database|Schema to which to connect|not set|
|connectionString|Full connection string|built from settings|
|integratedSecurity|Enable Windows Authentication (For local use only)|not set|
|urlEncodedCredentials|Set to `true` if your service broker provides URL-encoded credentials|false|

>IMPORTANT: All of the settings shown in the preceding table should be prefixed with `sqlserver:credentials:`.

The samples and most templates are already set up to read from `appsettings.json`.

>NOTE: If a ConnectionString is provided and VCAP_SERVICES are not detected (a typical scenario for local app development), the ConnectionString will be used exactly as provided.

### Cloud Foundry

To use Microsoft SQL Server on Cloud Foundry, you need a service instance bound to your application. If the [Microsoft SQL Server broker](https://github.com/cf-platform-eng/mssql-server-broker) is installed in your Cloud Foundry instance, use it to create a new service instance, as follows:

```bash
cf create-service SqlServer sharedVM mySqlServerService
```

An alternative to the broker is to use a user-provided service to explicitly provide connection information to the application, as shown in the following example:

```bash
cf cups mySqlServerService -p '{"pw": "|password|","uid": "|user id|","uri": "jdbc:sqlserver://|host|:|port|;databaseName=|database name|"}'
```

Version 2.1.1+ of this connector works with the [Azure Open Service Broker for PCF](https://docs.pivotal.io/partners/azure-open-service-broker-pcf/index.html). Be sure to set `sqlServer:client:urlEncodedCredentials` to `true` as this broker may provide credentials that have been URL Encoded.

If you are creating a service for an application that has already been deployed, you need to bind the service and restart or restage the application with the following commands:

```bash
# Bind service to `myApp`
cf bind-service myApp mySqlServerService

# Restage the app to pick up change
cf restage myApp
```

If you have not already deployed the application, a reference in the `manifest.yml` file can take care of the binding for you.

>NOTE: The commands shown in the preceding example may not exactly match the service or plan names available in your environment. You may have to adjust the `create-service` command to fit your environment. Use `cf marketplace` to see what is available.

Once the service is bound to your application, the connector's settings are available in `VCAP_SERVICES`.

### Add SqlConnection

To use a `SqlConnection` in your application, add it to the service container in the `ConfigureServices()` method of the `Startup` class, as shown in the following example:

```csharp
using Steeltoe.CloudFoundry.Connector.SqlServer;

public class Startup {
    ...
    public IConfiguration Configuration { get; private set; }
    public Startup(...)
    {
      ...
    }
    public void ConfigureServices(IServiceCollection services)
    {
        // Add SqlConnection configured from Configuration
        services.AddSqlServerConnection(Configuration);

        // Add framework services.
        services.AddMvc();
        ...
    }
    ...
```

The `AddSqlServerConnection(Configuration)` method call shown in the previous example configures the `SqlConnection` by using the configuration built by the application and adds the connection to the service container.

> By default, this extension method will automatically configure an `IHealthContributor` to report the health of this database connection. This behavior can be turned off by passing `false` for the parameter `addSteeltoeHealthChecks`

### Use SqlConnection

Once you have configured and added the connection to the service container, you can inject it and use it in a controller or a view, as shown in the following example:

```csharp
using System.Data.SqlClient;
...
public class HomeController : Controller
{
    public IActionResult SqlData([FromServices] SqlConnection dbConnection)
    {
        dbConnection.Open();

        MySqlCommand cmd = new MySqlCommand("SELECT * FROM TestData;", dbConnection);
        MySqlDataReader rdr = cmd.ExecuteReader();

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

>NOTE: The preceding code does not create a database or a table or insert data. As written, it fails unless you create the database, table, and data ahead of time.

### Add DbContext

#### Entity Framework 6

To use the Microsoft SQL connector with Entity Framework 6, inject a DbContext into your application using the AddDbContext<>() method (provided by Steeltoe) that takes an IConfiguration as a parameter, as shown in the following example:

```csharp
using Steeltoe.CloudFoundry.Connector.SqlServer.EF6;

public class Startup {
    ...
    public IConfiguration Configuration { get; private set; }
    public Startup(...)
    {
      ...
    }
    public void ConfigureServices(IServiceCollection services)
    {
        ...
        services.AddDbContext<TestContext>(Configuration);
        ...
    }
    ...
```

The `AddDbContext<TestContext>(..)` method call configures `TestContext` by using the configuration built earlier and then adds the `DbContext` (`TestContext`) to the service container.

> This extension method will automatically configure an `IHealthContributor` to report the health of this database connection.

Your `DbContext` does not need to be modified from a standard EF6 `DbContext` to work with Steeltoe:

```csharp
using System.Data.Entity;
...

public class TestContext : DbContext
{
    public TestContext(string connectionString) : base(connectionString)
    {
    }
    public DbSet<TestData> TestData { get; set; }
}
```

#### Entity Framework Core

To use the Microsoft SQL Server connector with Entity Framework Core, inject a `DbContext` into your application with the standard `AddDbContext<>()` method, substituting Steeltoe's `UseSqlServer` method that takes an `IConfiguration` as a parameter in the options configuration for the standard `UseSqlServer` method. This example demonstrates the basic usage:

```csharp
using Steeltoe.CloudFoundry.Connector.SqlServer.EFCore;

public class Startup {
    ...
    public IConfiguration Configuration { get; private set; }
    public Startup(...)
    {
      ...
    }
    public void ConfigureServices(IServiceCollection services)
    {
        ...
        services.AddDbContext<TestContext>(options => options.UseSqlServer(Configuration));

        // see note below explaining AddSqlServerHealthContributor
        services.AddSqlServerHealthContributor(Configuration);
        ...
    }
    ...
```

> This extension method will _NOT_ configure an `IHealthContributor` for this database connection. The NuGet package Steeltoe.CloudFoundry.ConnectorCore provides an `IServiceCollection` extension method that will. Directly add the health contributor with the code `services.AddSqlServerHealthContributor(Configuration)`

Your `DbContext` does not need to be modified from a standard `DbContext` to work with Steeltoe:

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

If you need to set additional properties for the `DbContext` like `MigrationsAssembly` or connection retry settings, create an `Action<SqlServerDbContextOptionsBuilder>` like this:

```csharp
Action<SqlServerDbContextOptionsBuilder> sqlServerOptionsAction = (o) =>
{
  o.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
  // Configuring Connection Resiliency: https://docs.microsoft.com/ef/core/miscellaneous/connection-resiliency
  o.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
};
```

Pass your new options action into the AddDbContext method:

```csharp
services.AddDbContext<TestContext>(options => options.UseSqlServer(Configuration, sqlServerOptionsAction));
```

### Use DbContext

Once you have configured and added the DbContext to the service container, you can inject it and use it in a controller or a view, as shown in the following example:

```csharp
using Project.Models;
...
public class HomeController : Controller
{
    public IActionResult SqlData([FromServices] TestContext context)
    {
        return View(context.TestData.ToList());
    }
```
