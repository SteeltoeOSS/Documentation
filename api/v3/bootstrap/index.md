# Application Bootstrapping

In order to improve the Steeltoe developer experience, Steeltoe 3.1.0 added this new feature that allows the configuration of most Steeltoe components with a single line of code in your application. The package is named [`Steeltoe.Bootstrap.Autoconfig`](https://github.com/SteeltoeOSS/Steeltoe/tree/release/3.2/src/Bootstrap/src/Autoconfig), and it works by applying the same extensions that are already included in Steeltoe packages to automatically wire up each of those components.

Applications running on .NET Core 3.1+ and .NET 5.0+ are supported. Get started by adding a reference to the Autoconfig package (you may want to add other Steeltoe references at this point too, see [the table below](#supported-steeltoe-packages) for the full list of what's supported now):

```xml
<ItemGroup>
    <PackageReference Include="Steeltoe.Bootstrap.Autoconfig" Version="3.2.0" />
</ItemGroup>
```

 After adding the NuGet reference(s), simply include `.AddSteeltoe()` like you see in this example and you're all set with the basic implementation:

```csharp
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Steeltoe.Bootstrap.Autoconfig;

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

`Steeltoe.Bootstrap.Autoconfig` is not a meta-package. In order for a Steeltoe feature to be automatically bootstrapped in the application, the appropriate package must also be added as a reference. The following table describes the Steeltoe package that is required to light up a feature and any additional packages that may also be required:

|  Feature Description | Steeltoe Package |Additional Package Required |
| --- | --- | --- |
| [Config Server Configuration](../configuration/config-server-provider.md) | `Steeltoe.Extensions.Configuration.ConfigServerBase` or `ConfigServerCore` | N/A |
| [Cloud Foundry Configuration](../configuration/cloud-foundry-provider.md) |`Steeltoe.Extensions.Configuration.CloudFoundryBase` or `CloudFoundryCore` |  N/A |
| [Kubernetes Configuration](../configuration/kubernetes-providers.md) |`Steeltoe.Extensions.Configuration.KubernetesBase` or `KubernetesCore` |  N/A |
| [Random Value Provider](../configuration/random-value-provider.md) |`Steeltoe.Extensions.Configuration.RandomValueBase` |  N/A |
| [Placeholder Resolver](../configuration/placeholder-provider.md) |`Steeltoe.Extensions.Configuration.PlaceholderBase` or `PlaceholderCore` |  N/A |
| [Connectors*](../connectors/index.md) |`Steeltoe.Connector.ConnectorCore` |  Supported driver (MySQL, PostgreSQL, RabbitMQ, SQL Server, etc) |
| [Dynamic Serilog](../logging/serilog-logger.md) | `Steeltoe.Extensions.Logging.DynamicSerilogCore` | N/A |
| [Service Discovery](../discovery/index.md) |`Steeltoe.Discovery.ClientBase` or `ClientCore` | Desired client (Eureka, Consul, Kubernetes)
| [Actuators](../management/index.md) | `Steeltoe.Management.EndpointCore` | N/A |
| [Actuators with Cloud Foundry support**](../management/cloud-foundry.md) |`Steeltoe.Management.CloudFoundryCore` |  N/A |
| [Actuators with Kubernetes support](../management/index.md) |`Steeltoe.Management.KubernetesCore` |  N/A |
| [Distributed Tracing](../tracing/index.md) | `Steeltoe.Management.TracingBase` or `TracingCore` | OpenTelemetry Exporter (Zipkin, Jaeger, OTLP) |
| [Cloud Foundry Container Identity](../security/mtls.md#configure-settings) | `Steeltoe.Security.Authentication.CloudFoundryCore` | N/A |

>\* The [Connection String Configuration provider](../connectors/usage.md#connectionstring-configuration-provider) is always added when `ConnectorCore` is referenced. Individual connector clients will only be configured if a corresponding supported driver NuGet package reference is also included.

>\*\* Cloud Foundry support is now included in `Steeltoe.Management.EndpointCore`. The Cloud Foundry Actuator package is not required and has been marked obsolete in 3.1.0.

## Excluding Components

If you wish to exclude a component from the automatic bootstrap process, you may add the feature's assembly name to the exclusions list. One example where this feature would be desired is if you want to control the order configuration providers are added. This example shows how to provide the exclusions list:

```csharp
public static IHostBuilder CreateHostBuilder(string[] args)
{
    List<string> myExclusions = new () { SteeltoeAssemblies.Steeltoe_Extensions_Configuration_ConfigServerCore };
    return Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        })
        .AddSteeltoe(myExclusions);
}
```

>The static class `SteeltoeAssemblies` is `public` so that you can easily find the name of any specific assembly to exclude.

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
* Features that require a custom type (such as a DbContext) for setup.

## Feedback

Love it? Hate it? Want to know more or make a suggestion? Let us know by [filing an issue](https://github.com/SteeltoeOSS/Steeltoe/issues/new/choose), [joining us on slack](https://slack.steeltoe.io/) or [Tweeting at us](https://twitter.com/steeltoeoss)
