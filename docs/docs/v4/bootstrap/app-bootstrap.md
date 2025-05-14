# Application Bootstrapping

To improve the Steeltoe developer experience, this feature allows the configuration of most Steeltoe components with a single line of code in your application.
The package is named [`Steeltoe.Bootstrap.AutoConfiguration`](https://www.nuget.org/packages/Steeltoe.Bootstrap.AutoConfiguration).
It wires up each component automatically by calling the same extension methods already included in Steeltoe packages.

> [!IMPORTANT]
> `Steeltoe.Bootstrap.AutoConfiguration` is not a meta-package. For a Steeltoe feature to be automatically bootstrapped in the application,
> the appropriate NuGet package must also be referenced.

## Get started

To get started, use the following steps:

1. Add a NuGet reference to the AutoConfiguration package:

   ```shell
   dotnet add package Steeltoe.Bootstrap.AutoConfiguration
   ```

   You may want to add additional Steeltoe packages at this point. See the [Supported Steeltoe Packages](#supported-steeltoe-packages) section for the complete list of what's supported.

1. Use the `AddSteeltoe` extension method in your `Program.cs` file:

   ```csharp
   using Steeltoe.Bootstrap.AutoConfiguration;

   var builder = WebApplication.CreateBuilder(args);
   builder.AddSteeltoe();
   ```

1. Add any other Steeltoe packages you want to use, as described in the following section.

## Supported Steeltoe Packages

The following table describes the Steeltoe package that is required to light up a feature, and any additional packages that may also be installed:

| Feature Description | Steeltoe Package | Additional Packages |
| --- | --- | --- |
| [Config Server Configuration Provider](../configuration/config-server-provider.md) | `Steeltoe.Configuration.ConfigServer` | Optional: `Steeltoe.Discovery.Eureka` to use discovery-first |
| [Cloud Foundry Configuration Provider](../configuration/cloud-foundry-provider.md) | `Steeltoe.Configuration.CloudFoundry` | N/A |
| [Random Value Configuration Provider](../configuration/random-value-provider.md) | `Steeltoe.Configuration.RandomValue` | N/A |
| [Placeholder Configuration Provider](../configuration/placeholder-provider.md) | `Steeltoe.Configuration.Placeholder` | N/A |
| [Encrypted Configuration Provider](../configuration/decryption-provider.md) | `Steeltoe.Configuration.Encryption` | N/A |
| [Spring Boot Configuration Provider](../configuration/spring-boot-provider.md) | `Steeltoe.Configuration.SpringBoot` | N/A |
| [Connectors](../connectors/index.md) | `Steeltoe.Connectors` | Required: Supported driver[^1] (MySQL, PostgreSQL, SQL Server, MongoDB, CosmosDB, Redis/Valkey, RabbitMQ) |
| [Eureka Service Discovery](../discovery/netflix-eureka.md) | `Steeltoe.Discovery.Eureka` | Optional: `Steeltoe.Management.Endpoint` for health checks |
| [Consul Service Discovery](../discovery/hashicorp-consul.md) | `Steeltoe.Discovery.Consul` | N/A |
| [Configuration-based Service Discovery](../discovery/configuration-based.md) | `Steeltoe.Discovery.Configuration` | N/A |
| [Dynamic Console Logging](../logging/dynamic-console-logging.md) | `Steeltoe.Logging.DynamicConsole` | N/A |
| [Dynamic Serilog Logging](../logging/dynamic-serilog-logging.md) | `Steeltoe.Logging.DynamicSerilog` | N/A |
| [Actuators](../management/index.md) | `Steeltoe.Management.Endpoint` | N/A |
| [Distributed Tracing](../tracing/index.md) | `Steeltoe.Management.Tracing` | N/A |

[^1]: Individual connector clients are only configured if a corresponding supported driver NuGet package reference is also included.

## Excluding Components

You may add the feature's assembly name to the exclusions list to exclude a component from the automatic bootstrap process.
One example where this feature would be desired is if you want to control the order in which configuration providers are added.
This example shows how to provide exclusions:

```csharp
using Steeltoe.Bootstrap.AutoConfiguration;

HashSet<string> assemblyNamesToExclude = [SteeltoeAssemblyNames.ConfigurationConfigServer];

var builder = WebApplication.CreateBuilder(args);
builder.AddSteeltoe(assemblyNamesToExclude);
```

> [!TIP]
> The static class `SteeltoeAssemblyNames` lets you easily find the name of any specific assembly to exclude.

## Logger Bootstrapping

The `AddSteeltoe` extension method automatically activates [`BootstrapLoggerFactory`](log-bootstrap.md#using-bootstraploggerfactory) with console logging.
To turn this off, pass `NullLoggerFactory.Instance`. For example:

```csharp
using Microsoft.Extensions.Logging.Abstractions;
using Steeltoe.Bootstrap.AutoConfiguration;

var builder = WebApplication.CreateBuilder(args);
builder.AddSteeltoe(NullLoggerFactory.Instance);
```

To customize the Bootstrap logger, pass your own instance. For example:

```csharp
using Steeltoe.Bootstrap.AutoConfiguration;
using Steeltoe.Common.Logging;

var loggerFactory = BootstrapLoggerFactory.CreateEmpty(loggingBuilder =>
{
    loggingBuilder.AddDebug();
    loggingBuilder.SetMinimumLevel(LogLevel.Warning);
});

var builder = WebApplication.CreateBuilder(args);
builder.AddSteeltoe(loggerFactory);
```

## Limitations

Currently unsupported:

* Features that need to be configured directly in `IApplicationBuilder`, such as Cloud Foundry SSO and JWT.
* Features that require a custom type (such as a `DbContext`) for setup.

## Feedback

Love it? Hate it? Want to know more or make a suggestion? Let us know by [filing an issue](https://github.com/SteeltoeOSS/Steeltoe/issues/new/choose), [joining us on Slack](https://slack.steeltoe.io/) or [tagging us on Twitter/X](https://x.com/steeltoeoss).
