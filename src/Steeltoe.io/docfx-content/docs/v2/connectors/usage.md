# Usage

This page will cover some of the general details on how Connectors work, how to use them generally and some platform-specific details. The rest of the pages will cover information specific to each supported backing service type.

In order to use any Steeltoe Connector, there are several steps to follow:

1. Add a NuGet reference for the backing technology (for example: Redis, MySQL, RabbitMQ, etc)
1. Add Steeltoe NuGet Reference(s)
1. Use Steeltoe to get a connection
1. Optionally provide configuration details (see the page for any specific service type for more information)

## Add NuGet References

Depending on what functionality you wish to use, you may need one or more package references. This table provides a list of Steeltoe Connector packages, a brief description of each:

| Package | Description |
| --- | --- |
| `Steeltoe.Connector.ConnectorBase` | Includes abstractions. Connectors base package. |
| `Steeltoe.Connector.ConnectorCore` | Includes base. Adds ServiceCollection compatibility. |
| `Steeltoe.Connector.ConnectorAutofac` | Includes base. Adds Autofac compatibility. |
| `Steeltoe.Connector.EFCore` | Includes base. Adds compatibility with Entity Framework Core |
| `Steeltoe.Connector.EF6Core` | Includes base. Adds compatibility with Entity Framework 6 |
| `Steeltoe.Connector.EF6Autofac` | Includes base. Adds compatibility with Entity Framework 6 and Autofac |

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
