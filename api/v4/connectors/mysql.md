# MySQL

This connector simplifies using MySQL ADO.NET providers in an application.

Currently, the connector supports the following providers:

* [Connector/NET](https://dev.mysql.com/doc/connector-net/en/)
* [MySqlConnector](https://mysql-net.github.io/MySqlConnector/)

The following Steeltoe sample applications can help you understand how to use this connector:

* [MusicStore](https://github.com/SteeltoeOSS/Samples/tree/master/MusicStore): A sample application showing how to use all of the Steeltoe components together in a ASP.NET Core application. This is a microservices based application built from the ASP.NET Core MusicStore reference app provided by Microsoft.
* [FreddysBBQ](https://github.com/SteeltoeOSS/Samples/tree/master/FreddysBBQ): A polyglot (Java and .NET) microservices-based sample application showing interoperability between Java- and .NET-based microservices running on Cloud Foundry, secured with OAuth2 Security Services and using Spring Cloud Services.

This connector provides a `IHealthContributor` object, which you can use in conjunction with the [Steeltoe Management Health](../management/health.md) check endpoint.

## Usage

You should know how the .NET [configuration service](https://docs.microsoft.com/aspnet/core/fundamentals/configuration) works before starting to use the connector. A basic understanding of the `ConfigurationBuilder` and how to add providers to the builder is necessary to configure the connector.

You should also know how the ASP.NET Core [Startup](https://docs.microsoft.com/aspnet/core/fundamentals/startup) class is used in configuring the application services for the app. Pay particular attention to the usage of the `ConfigureServices()` method.

To use this connector:

1. Create a MySQL Service instance and bind it to your application.
1. Optionally, configure any MySQL client settings (such as `appsettings.json`) you need.
1. Optionally, add the Steeltoe Cloud Foundry configuration provider to your `ConfigurationBuilder`.
1. Add `MySqlConnection` or `DbContext` to your `IServiceCollection`.

### Add NuGet Reference

To use the MySQL connector, add your choice of MySQL-specific package(s) between [MySql.Data](https://www.nuget.org/packages/MySql.Data)/[MySql.Data.Entity](https://www.nuget.org/packages/MySql.Data.Entity), [MySqlConnector](https://www.nuget.org/packages/MySqlConnector/), and [Pomelo.EntityFrameworkCore.MySql](https://www.nuget.org/packages/Pomelo.EntityFrameworkCore.MySql/) as you would if you were not using Steeltoe. Then [add a reference to the appropriate Steeltoe Connector NuGet package](usage.md#add-nuget-references).

### Configure Settings

The connector supports a variety of configuration options. You can use these settings to develop or test an application locally and override them during deployment.

The following MySQL connector configuration shows how to connect to a database at `myserver:3306`:

```json
{
  ...
  "MySql": {
    "Client": {
      "Server": "myserver",
      "Port": 3306
    }
  }
  ...
}
```

The following table describes the available settings for the connector. These settings are not specific to Steeltoe. They are passed through to the underlying data provider. See the [Oracle MySQL Connection String docs](https://dev.mysql.com/doc/connector-net/en/connector-net-connection-options.html) or [open source MySQL Connection String docs](https://mysql-net.github.io/MySqlConnector/connection-options/).

|Key|Description |Steeltoe Default|
| --- | --- |:---:|
| `Server` | Hostname or IP Address of the server. | `localhost` |
| `Port` | Port number of server. | 3306 |
| `Username` | Username for authentication. | not set |
| `Password` | Password for authentication. | not set |
| `Database` | Schema to which to connect. | not set |
| `ConnectionString` | Full connection string. | Built from settings |
| `SslMode` | SSL usage option. One of `None`, `Preferred`, or `Required`. | `None` |
| `AllowPublicKeyRetrieval` | Whether RSA public keys should be retrieved from the server. | not set |
| `AllowUserVariables` | Whether the provider expects user variables in the SQL. | not set |
| `ConnectionTimeout` |Seconds to wait for a connection before throwing an error. | not set |
| `ConnectionLifeTime` | The maximum length of time a connection to the server can be open. | not set |
| `ConnectionReset` | Whether the connection state is reset when it is retrieved from the pool. | not set |
| `ConvertZeroDateTime` | Whether to have MySqlDataReader.GetValue() and MySqlDataReader.GetDateTime() return DateTime.MinValue for date or datetime columns that have disallowed values. | not set |
| `DefaultCommandTimeout` | Seconds each command can execute before timing out. Use 0 to disable timeouts. | not set |
| `Keepalive` | TCP keep-alive idle time. | not set |
| `MaximumPoolsize` | Maximum number of connections allowed in the pool. | not set |
| `MinimumPoolsize` | Minimum number of connections to leave in the pool if ConnectionIdleTimeout is reached. | not set |
| `OldGuids` | Whether to use a GUID of data type BINARY(16)| not set |
| `PersistSecurityInfo` | Whether to allow the application to access to security-sensitive information, such as the password. **_(Not recommended)_**. | not set |
| `Pooling` | Enables connection pooling. | not set |
| `TreatTinyAsBoolean` | Whether to return tinyint(1) as a boolean. Set to `false` to return tinyint(1) as sbyte/byte. | not set |
| `UseAffectedRows` | Set to `false` to report found rows instead of changed (affected) rows. | not set |
| `UseCompression` | If `true` (and server-supported), packets sent between client and server are compressed. | not set |

>IMPORTANT: All of the settings described in the preceding table should be prefixed with `MySql:Client:`.

The samples and most templates are already set up to read from `appsettings.json`.

>If a ConnectionString is provided and VCAP_SERVICES are not detected (a typical scenario for local app development), the ConnectionString will be used exactly as provided.

### Cloud Foundry

To use MySQL on Cloud Foundry, you can create and bind an instance of MySQL to your application by using the Cloud Foundry CLI, as follows:

```bash
# Create MySQL service
cf create-service p-mysql 100mb myMySqlService

# Bind service to `myApp`
cf bind-service myApp myMySqlService

# Restage the app to pick up change
cf restage myApp
```

>The preceding commands assume you use [MySQL for VMware Tanzu](https://network.pivotal.io/products/pivotal-mysql/), provided by VMware on Tanzu. If you use a different service, you must adjust the `create-service` command to fit your environment.

This connector also works with the [Azure Service Broker](https://docs.pivotal.io/partners/azure-sb/).

Once the service is bound to your application, the connector's settings are available in `VCAP_SERVICES`.

### Add MySqlConnection

To use a `MySqlConnection` in your application, add it to the service container in the `ConfigureServices()` method of the `Startup` class:

```csharp
using Steeltoe.Connector.MySql;

public class Startup {
    ...
    public IConfiguration Configuration { get; private set; }
    public Startup(...)
    {
      ...
    }
    public void ConfigureServices(IServiceCollection services)
    {
        // Add MySqlConnection configured from Configuration
        services.AddMySqlConnection(Configuration);

        // Add framework services.
        services.AddMvc();
        ...
    }
    ...
}
```

The `AddMySqlConnection(Configuration)` method call configures the `MySqlConnection` by using the configuration built by the application and adds the connection to the service container.

> By default, this extension method will automatically configure an `IHealthContributor` to report the health of this database connection. This behavior can be turned off by passing `false` for the parameter `addSteeltoeHealthChecks`

### Use MySqlConnection

Once you have configured and added the connection to the service container, you can inject it and use it in a controller or a view:

```csharp
using MySql.Data.MySqlClient;
...
public class HomeController : Controller
{
    public IActionResult MySqlData([FromServices] MySqlConnection dbConnection)
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

### Add DbContext

This section describes how to add a DbContext with both Entity Framework 6 and Entity Framework Core

#### Entity Framework 6

To use the MySQL connector with Entity Framework 6, inject a `DbContext` into your application by using the `AddDbContext<>()` method (provided by Steeltoe) that takes an `IConfiguration` as a parameter:

```csharp
using Steeltoe.Connector.MySql.EF6;

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
}
```

The `AddDbContext<TestContext>(..)` method call configures `TestContext` by using the configuration built earlier and then adds the DbContext (called `TestContext`) to the service container.

> This extension method will automatically configure an `IHealthContributor` to report the health of this database connection.

Your `DbContext` does not need to be modified from a standard EF6 `DbContext` to work with Steeltoe:

```csharp
using MySql.Data.Entity;
using System.Data.Entity;
...

[DbConfigurationType(typeof(MySqlEFConfiguration))]
public class TestContext : DbContext
{
    public TestContext(string connectionString) : base(connectionString)
    {
    }
    public DbSet<TestData> TestData { get; set; }
}
```

#### Entity Framework Core

To use the MySQL connector with Entity Framework Core, inject a `DbContext` into your application with the standard `AddDbContext<>()` method, substituting Steeltoe's `UseMySql` method that takes an `IConfiguration` as a parameter in the options configuration for the standard `UseMySql` method. The following example demonstrates the basic usage:

```csharp
using Steeltoe.Connector.MySql.EFCore;

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
        services.AddDbContext<TestContext>(options => options.UseMySql(Configuration));
        ...
    }
    ...
}
```

> This extension method will _NOT_ configure an `IHealthContributor` for this database connection. The NuGet package Steeltoe.Connector.ConnectorCore provides an `IServiceCollection` extension method that will. Directly add the health contributor with the code `services.AddMySqlHealthContributor(Configuration)`

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

If you need to set additional properties for the `DbContext` like `MigrationsAssembly` or connection retry settings, create an `Action<MySqlDbContextOptionsBuilder>`:

```csharp
Action<MySqlDbContextOptionsBuilder> mySqlOptionsAction = (o) =>
{
  o.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
  // Configuring Connection Resiliency: https://docs.microsoft.com/ef/core/miscellaneous/connection-resiliency
  o.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
};
```

Then pass your new options action into the AddDbContext method:

```csharp
services.AddDbContext<TestContext>(options => options.UseMySql(Configuration, mySqlOptionsAction));
```

### Use DbContext

Once you have configured and added the `DbContext` object to the service container, inject and use it in a controller or a view:

```csharp
using Project.Models;
...
public class HomeController : Controller
{
    public IActionResult MySqlData([FromServices] TestContext context)
    {
        return View(context.TestData.ToList());
    }
}
```
