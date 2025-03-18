# Usage

This page will cover some of the general details on how Connectors work, how to use them generally and some platform-specific details. The rest of the pages will cover information specific to each supported backing service type.

In order to use any Steeltoe Connector, there are several steps to follow:

1. Add a NuGet reference for the backing technology (for example: Redis, MySQL, RabbitMQ, etc)
1. Add Steeltoe NuGet Reference(s)
1. Use Steeltoe to get a connection
1. Optionally provide configuration details (see the page for any specific service type for more information)

## Add NuGet References

Depending on what functionality you wish to use, you may need one or more package references. This table provides a list of Steeltoe Connector packages, a brief description and the .NET framework of each:

| Package | Description | .NET Target |
| --- | --- | --- |
| `Steeltoe.Connector.Abstractions` | Interfaces and objects used for extensibility. | .NET Standard 2.0 |
| `Steeltoe.Connector.ConnectorBase` | Includes abstractions. Connectors base package. | .NET Standard 2.0 |
| `Steeltoe.Connector.ConnectorCore` | Includes base. Adds ServiceCollection compatibility. | .NET Core 3.1+ |
| `Steeltoe.Connector.EFCore` | Includes base. Adds compatibility with Entity Framework Core | .NET Core 3.1+ |
| `Steeltoe.Connector.EF6Core` | Includes base. Adds compatibility with Entity Framework 6 | .NET Core 3.1+ |
| `Steeltoe.Connector.CloudFoundry` | Includes base. Adds Cloud Foundry compatibility | .NET Standard 2.0 |

>As of Steeltoe 3.0, an extra NuGet reference such as `Steeltoe.Connector.CloudFoundry` may be required for platform-specific support

## ServiceCollection Extensions

`ServiceCollection` extensions are provided in `Steeltoe.Connector.ConnectorCore`, `Steeltoe.Connector.EFCore` and `Steeltoe.Connector.EF6Core`. These extensions will add all of the requirements for retrieving clients for the various supported technologies from a service container later on in your application. Additionally, they will typically add an `IHealthContributor` that will automatically include health checks for the connected service instance when used in conjunction with Steeltoe Management Actuators. These extensions are generally built on top of functionality provided by the underlying drivers for a given backing service and typically built the connection string for the underlying provider. As such, their usage will typically be quite similar to usage of the backing service without Steeltoe. For example, here's a comparison between adding an Entity Framework `IDbContext` with and without Steeltoe:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // using Steeltoe, passing in the IConfiguration
    services.AddDbContext<TestContext>(options => options.UseNpgsql(Configuration));

    // without Steeltoe, passing in the connection string more directly
    services.AddDbContext<TestContext>(options => options.UseNpgsql(Configuration.GetConnectionString("myPostgresConnection")));
}
```

## ConnectionString Configuration Provider

If you are only looking for the functionality of mapping credentials in configuration into connection strings, Connector functionality is now also available as an `IConfigurationProvider`.

This example shows adding the ConnectionString Configuration provider:

```csharp
return Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(builder => builder.AddConnectionStrings())
            .Build();
```

With the ConfigurationProvider for Connectors in place, connection strings will be built behind the scenes and made available (by type name or service binding name) as though they had been directly included in the configuration's ConnectionString block:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Using Steeltoe configuration provider, pass in the connection string that was build from configuration
    // by service type name
    services.AddDbContext<TestContext>(options => options.UseNpgsql(Configuration.GetConnectionString("postgres")));
    // or by service binding name
    services.AddDbContext<TestContext>(options => options.UseNpgsql(Configuration.GetConnectionString("myPostgresConnection")));
}
```

## ConnectionManager

The `ConnectionStringConfigurationProvider` is built on top of another class named `ConnectionStringManager`, which is also accessible should you wish for yet another programming model. This class will retrieve service-typed 'IConnectionInfo`, where the connection string is available as a property.

```csharp
var connStringManager = new ConnectionStringManager(Configuration);
var mongoInfo = connStringManager.Get<MongoDbConnectionInfo>();
var mysqlInfo = connStringManager.Get<MySqlConnectionInfo>();
```

## Cloud Foundry

Steeltoe Connectors were originally created to provide out-of-the-box support for discovering common services on Cloud Foundry, and as such they've historically had a pretty direct connection to each other. As of version 3.0, in preparation of support for other platforms, the Cloud Foundry specific pieces have been extracted to a separate NuGet package. No new code is required** to activate Cloud Foundry portion of Connectors (assuming the `CloudFoundryConfigurationProvider` has already been added), only the addition of a NuGet reference for `Steeltoe.Connector.CloudFoundry`.

>** If you [publish your application as a single file](https://docs.microsoft.com/dotnet/core/deploying/single-file) prior to deployment, this assembly will not be included in the output unless there is a reference to it. Package version 3.0.2 adds the no-op method `CloudFoundryConnector.EnsureAssemblyIsLoaded()`, which can be called from anywhere in your application, to support this scenario.

With the addition of this package, all connectors can use configuration information from Cloud Foundry's `VCAP_SERVICES` environment variable to detect and configure the available services. This example shows how to make connection strings built from Cloud Foundry service bindings available for `.GetConnectionString("connectionType")` later on:

```csharp
return Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(builder =>
    {
        builder.AddCloudFoundry();
        builder.AddConnectionStrings();
    )
    .Build();
```

`VCAP_SERVICES` is a Cloud Foundry standard that is used to hold connection and identification information for all service instances that have been bound to Cloud Foundry applications. For more information on `VCAP_SERVICES`, see the Cloud Foundry [documentation](https://docs.cloudfoundry.org/services/overview.html).

>Depending on your hosting environment, service instances you create for the purpose of exploring the Quick Starts on these pages may have a cost associated.

## Kubernetes

While enhanced support will be provided in the next major version of Steeltoe, preliminary support for the [Service Binding Specification for Kubernetes](https://github.com/servicebinding/spec) was [added in the 3.2.2 release](../../../articles/steeltoe-3-2-2-adds-kube-service-bindings.md) with a new `IConfigurationProvider`.

The current version of `Steeltoe.Extensions.Configuration.Kubernetes.ServiceBinding` can read [many types of bindings](https://github.com/SteeltoeOSS/Steeltoe/blob/release/3.2/src/Configuration/src/Kubernetes.ServiceBinding/PostProcessors.cs) into configuration and will transform the bindings for MongoDb, MySQL, PostgreSQL, RabbitMQ and Redis into the formats required to work automatically with Steeltoe Connectors.

In order to use Steeltoe's Service Bindings for Kubernetes, you need to do the following:

1. Add the NuGet package reference to your project.
1. Enable service binding support in configuration.
1. Add the provider to the configuration builder

### Add NuGet Reference

Add a `PackageReference` to `Steeltoe.Extensions.Configuration.Kubernetes.ServiceBinding`.

### Configure Settings

Due to the experimental nature of this feature, bindings must be enabled in the application's configuration before they are applied.

```json
{
  "Steeltoe": {
    "Kubernetes": {
      "Bindings": {
        "Enable": true
      }
    }
  }
}
```

### Add Configuration Provider

```csharp
using Steeltoe.Extensions.Configuration.Kubernetes.ServiceBinding;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddKubernetesServiceBindings();
```

### Additional Resources

To see the binding support in action, review the [Steeltoe PostgreSql EF Core Connector sample](https://github.com/SteeltoeOSS/Samples/tree/3.x/Connectors/src/PostgreSqlEFCore).
