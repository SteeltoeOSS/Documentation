---
title: Logging
order: 80
date: 2016/4/1
tags:
---

Steeltoe adds a Logging provider to the set of available logging packages in order to support the Steeltoe Management Logger endpoint.

When used with the Steeltoe Logger Management endpoint, this package enables dynamically changing the logging levels for running applications with the [Pivotal Apps Manager](https://docs.pivotal.io/pivotalcf/2-0/console/index.html) on Cloud Foundry.

# 1.0 Dynamic Logging Provider

This logging provider is a wrapper around the [Microsoft Console Logging](https://github.com/aspnet/Logging) provider. This wrapper allows for querying the currently defined loggers and their levels as well as then modifying the levels dynamically at runtime.

For more information on how to use [Pivotal Apps Manager](https://docs.pivotal.io/pivotalcf/2-0/console/index.html) on Cloud Foundry for viewing and modifying logging levels, see the [Using Actuators with Apps Manager section](https://docs.pivotal.io/pivotalcf/2-0/console/using-actuators.html) of the Pivotal Cloud Foundry documentation.

> NOTE: The Pivotal Apps Manager integration involves sending the fully-qualified logger name over HTTP. Avoid using colons in the name of a logger to prevent invalid HTTP Requests.

The source code for the Logging provider can be found [here](https://github.com/SteeltoeOSS/Logging).

## 1.1 Usage

Before starting to use Steeltoe provider, you should have a good understanding of how the .NET [Logging service](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?tabs=aspnetcore2x) works, as it is nothing more than a wrapper around the existing Microsoft Console logger.

In order to use the Steeltoe logging provider, you need to do the following:

1. Add the Logging NuGet package references to your project.
1. Configure Logging settings.
1. Add the Dynamic logging provider to the logging builder.

### 1.1.1 Add NuGet References

To use the logging provider, you need to add a reference to the Steeltoe Dynamic Logging NuGet.

The provider is found in the `Steeltoe.Extensions.Logging.DynamicLogger` package.

You can add the provider to your project by using the following `PackageReference`:

```xml
<ItemGroup>
...
    <PackageReference Include="Steeltoe.Extensions.Logging.DynamicLogger" Version= "2.1.0"/>
...
</ItemGroup>
```

### 1.1.2 Configure Settings

As mentioned earlier, the Steeltoe Logging provider is a wrapper around the Microsoft Console logging provider. Consequently, you can configure it the same way you would the Microsoft provider. For more details on how this is done, see the section on [Log Filtering](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?tabs=aspnetcore2x#log-filtering).

### 1.1.3 Add Logging Provider

In order to use the provider, you need to add it to the logging builder by using the `AddDynamicConsole()` extension method, as shown in the following example:

```csharp
using Steeltoe.Extensions.Logging;
public class Program
{
    public static void Main(string[] args)
    {
        var host = new WebHostBuilder()
            .UseKestrel()
            .UseCloudFoundryHosting()
            .UseContentRoot(Directory.GetCurrentDirectory())
            .UseIISIntegration()
            .UseStartup<Startup>()
            .UseApplicationInsights()
            .ConfigureAppConfiguration((builderContext, config) =>
            {
                config.SetBasePath(builderContext.HostingEnvironment.ContentRootPath)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{builderContext.HostingEnvironment.EnvironmentName}.json", optional: true)
                    .AddCloudFoundry()
                    .AddEnvironmentVariables();
            })
            .ConfigureLogging((builderContext, loggingBuilder) =>
            {
                loggingBuilder.AddConfiguration(builderContext.Configuration.GetSection("Logging"));

                // Add Steeltoe Dynamic Logging provider
                loggingBuilder.AddDynamicConsole();
            })
            .Build();

        host.Run();
    }
}
```
