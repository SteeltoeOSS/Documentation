# Application Bootstrapping

In order to improve the Steeltoe developer experience, this feature allows the configuration of most Steeltoe components with a single line of code in your application. The package is named [`Steeltoe.Bootstrap.AutoConfiguration`](https://github.com/SteeltoeOSS/Steeltoe/tree/main/src/Bootstrap/src/AutoConfiguration), and it works by applying the same extensions that are already included in Steeltoe packages to automatically wire up each of those components.

Get started by adding a reference to the AutoConfiguration package (you may want to add other Steeltoe references at this point too, see [the table below](#supported-steeltoe-packages) for the full list of what's supported now):

```xml
<ItemGroup>
    <PackageReference Include="Steeltoe.Bootstrap.AutoConfiguration" Version="4.0.0" />
</ItemGroup>
```

 After adding the NuGet reference(s), simply include `.AddSteeltoe()` like you see in this example and you're all set with the basic implementation:

```csharp
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Steeltoe.Bootstrap.AutoConfiguration;

namespace WebApplication1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .AddSteeltoe();
    }
}

```

## Supported Steeltoe Packages

`Steeltoe.Bootstrap.AutoConfiguration` is not a meta-package. In order for a Steeltoe feature to be automatically bootstrapped in the application, the appropriate NuGet package must also referenced. The following table describes the Steeltoe package that is required to light up a feature and any additional packages that may also be required:

|  Feature Description | Steeltoe Package |Additional Package Required |
| --- | --- | --- |
| [Config Server Configuration](../configuration/config-server-provider.md) | `Steeltoe.Configuration.ConfigServer` | N/A |
| [Cloud Foundry Configuration](../configuration/cloud-foundry-provider.md) |`Steeltoe.Configuration.CloudFoundry` |  N/A |
| [Kubernetes Configuration](../configuration/kubernetes-providers.md) |`Steeltoe.Configuration.Kubernetes` |  N/A |
| [Random Value Provider](../configuration/random-value-provider.md) |`Steeltoe.Configuration.RandomValue` |  N/A |
| [Placeholder Resolver](../configuration/placeholder-provider.md) |`Steeltoe.Configuration.Placeholder` |  N/A |
| [Connectors*](../connectors/index.md) |`Steeltoe.Connectors` |  Supported driver (MySQL, PostgreSQL, RabbitMQ, SQL Server, etc) |
| [Dynamic Serilog](../logging/serilog-logger.md) | `Steeltoe.Logging.DynamicSerilog` | N/A |
| [Service Discovery](../discovery/index.md) |`Steeltoe.Discovery.Client` | Desired client (Eureka, Consul, Kubernetes)
| [Actuators](../management/index.md) | `Steeltoe.Management.Endpoint` | N/A |
| [Actuators with Cloud Foundry support**](../management/cloud-foundry.md) |`Steeltoe.Management.CloudFoundry` |  N/A |
| [Actuators with Kubernetes support](../management/index.md) |`Steeltoe.Management.Kubernetes` |  N/A |
| [Distributed Tracing](../tracing/index.md) | `Steeltoe.Management.Tracing` | OpenTelemetry Exporter (Zipkin, Jaeger, OTLP) |
| [Cloud Foundry Container Identity](../security/mtls.md#configure-settings) | `Steeltoe.Security.Authentication.CloudFoundry` | N/A |

>\* Individual connector clients will only be configured if a corresponding supported driver NuGet package reference is also included.

>\*\* Cloud Foundry support is now included in `Steeltoe.Management.Endpoint`. The Cloud Foundry Actuator package is not required and has been marked obsolete in 3.1.0.

## Excluding Components

If you wish to exclude a component from the automatic bootstrap process, you may add the feature's assembly name to the exclusions list. One example where this feature would be desired is if you want to control the order configuration providers are added. This example shows how to provide the exclusions list:

```csharp
public static IHostBuilder CreateHostBuilder(string[] args)
{
    List<string> myExclusions = new () { SteeltoeAssemblyNames.SteeltoeConfigurationConfigServer };
    return Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        })
        .AddSteeltoe(myExclusions);
}
```

>The static class `SteeltoeAssemblyNames` is `public` so that you can easily find the name of any specific assembly to exclude.

## Logging Inside Config Providers

For some Steeltoe components, primarily configuration providers, providing a `LoggerFactory` is required to retrieve logs for debugging. Use the optional parameter to provide one as needed:

```csharp
public static IHostBuilder CreateHostBuilder(string[] args)
{
    LoggerFactory loggerFactory = new (new List<ILoggerProvider> { new DebugLoggerProvider() });
    return Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        })
        .AddSteeltoe(loggerFactory: loggerFactory);
}
```

## Limitations

At this time there is no support for:

* Features that need to be configured directly in `IApplicationBuilder`, such as Cloud Foundry SSO and JWT.
* Features that require a custom type (such as a `DbContext`) for setup.

## Feedback

Love it? Hate it? Want to know more or make a suggestion? Let us know by [filing an issue](https://github.com/SteeltoeOSS/Steeltoe/issues/new/choose), [joining us on slack](https://slack.steeltoe.io/) or [Tweeting at us](https://twitter.com/steeltoeoss)
