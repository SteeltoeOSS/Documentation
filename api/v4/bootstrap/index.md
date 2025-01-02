# Application Bootstrapping

To improve the Steeltoe developer experience, this feature allows the configuration of most Steeltoe components with a single line of code in your application. The package is named [`Steeltoe.Bootstrap.AutoConfiguration`](https://github.com/SteeltoeOSS/Steeltoe/tree/main/src/Bootstrap/src/AutoConfiguration), and it works by calling the same extension methods that are already included in Steeltoe packages to automatically wire up each of those components.

Get started by adding a reference to the AutoConfiguration package (you may want to add other Steeltoe references at this point too, see [the table below](#supported-steeltoe-packages) for the full list of what's supported):

```shell
dotnet add package Steeltoe.Bootstrap.AutoConfiguration
```

After adding the NuGet reference(s), simply include `.AddSteeltoe()` like you see in the code below and you're all set with the basic implementation.

```csharp
using Steeltoe.Bootstrap.AutoConfiguration;

var builder = WebApplication.CreateBuilder(args);
builder.AddSteeltoe();
```

## Supported Steeltoe Packages

`Steeltoe.Bootstrap.AutoConfiguration` is not a meta-package. In order for a Steeltoe feature to be automatically bootstrapped in the application, the appropriate NuGet package must also be referenced. The following table describes the Steeltoe package that is required to light up a feature and any additional packages that may also be installed:

|  Feature Description | Steeltoe Package | Additional Packages |
| --- | --- | --- |
| [Config Server Configuration Provider](../configuration/config-server-provider.md) | `Steeltoe.Configuration.ConfigServer` | Optional: `Steeltoe.Discovery.Eureka` to use discovery-first |
| [Cloud Foundry Configuration Provider](../configuration/cloud-foundry-provider.md) |`Steeltoe.Configuration.CloudFoundry` |  N/A |
| [Random Value Configuration Provider](../configuration/random-value-provider.md) |`Steeltoe.Configuration.RandomValue` |  N/A |
| [Placeholder Configuration Provider](../configuration/placeholder-provider.md) |`Steeltoe.Configuration.Placeholder` |  N/A |
| [Encrypted Configuration Provider](../configuration/decryption-provider.md) | `Steeltoe.Configuration.Encryption` | N/A |
| [Spring Boot Configuration Provider](../configuration/spring-boot-provider.md) | `Steeltoe.Configuration.SpringBoot` | N/A |
| [Connectors](../connectors/index.md) |`Steeltoe.Connectors` |  Required: Supported driver [^1] (MySQL, PostgreSQL, SQL Server, MongoDB, CosmosDB, Redis, RabbitMQ) |
| [Eureka Service Discovery](../discovery/netflix-eureka.md) |`Steeltoe.Discovery.Eureka` | Optional: `Steeltoe.Management.Endpoint` for health checks |
| [Consul Service Discovery](../discovery/hashicorp-consul.md) |`Steeltoe.Discovery.Consul` | N/A |
| [Configuration-based Service Discovery](../discovery/configuration-based.md) |`Steeltoe.Discovery.Configuration` | N/A |
| [Dynamic Console Logging](../logging/dynamic-console-logging.md) | `Steeltoe.Logging.DynamicConsole` | N/A |
| [Dynamic Serilog Logging](../logging/dynamic-serilog-logging.md) | `Steeltoe.Logging.DynamicSerilog` | N/A |
| [Actuators](../management/index.md) | `Steeltoe.Management.Endpoint` | N/A |
| [Distributed Tracing](../tracing/index.md) | `Steeltoe.Management.Tracing` | N/A |

[^1]: Individual connector clients will only be configured if a corresponding supported driver NuGet package reference is also included.

## Excluding Components

If you wish to exclude a component from the automatic bootstrap process, you may add the feature's assembly name to the exclusions list. One example where this feature would be desired is if you want to control the order in which configuration providers are added. This example shows how to provide exclusions:

```csharp
using Steeltoe.Bootstrap.AutoConfiguration;

HashSet<string> assemblyNamesToExclude = [SteeltoeAssemblyNames.ConfigurationConfigServer];
builder.AddSteeltoe(assemblyNamesToExclude);
```

> [!TIP]
> The static class `SteeltoeAssemblyNames` enables to easily find the name of any specific assembly to exclude.

## Logging inside Configuration Providers

For some Steeltoe components, primarily configuration providers, providing a `LoggerFactory` is required to retrieve logs for debugging. Use the optional parameter to provide one as needed:

```csharp
using Microsoft.Extensions.Logging.Debug;
using Steeltoe.Bootstrap.AutoConfiguration;

var loggerFactory = LoggerFactory.Create(loggingBuilder =>
{
    loggingBuilder.AddDebug(); // or: loggingBuilder.AddConsole();
    loggingBuilder.SetMinimumLevel(LogLevel.Debug);
});

builder.AddSteeltoe(loggerFactory);
```

Alternatively, you can use `BootstrapLoggerFactory`. It logs to the console until the service container has been built.
Once the service container has become available, it automatically upgrades existing loggers to use the application configuration.

```csharp
using Steeltoe.Bootstrap.AutoConfiguration;
using Steeltoe.Common.Logging;

var loggerFactory = BootstrapLoggerFactory.CreateConsole();
builder.AddSteeltoe(loggerFactory);
```

## Limitations

At this time there is no support for:

* Features that need to be configured directly in `IApplicationBuilder`, such as Cloud Foundry SSO and JWT.
* Features that require a custom type (such as a `DbContext`) for setup.

## Feedback

Love it? Hate it? Want to know more or make a suggestion? Let us know by [filing an issue](https://github.com/SteeltoeOSS/Steeltoe/issues/new/choose), [joining us on slack](https://slack.steeltoe.io/) or [Tweeting at us](https://twitter.com/steeltoeoss)
