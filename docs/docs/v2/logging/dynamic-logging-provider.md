# Dynamic Logging Provider

This logging provider is a wrapper around the [Microsoft Console Logging](https://github.com/aspnet/Logging) provider. This wrapper allows for querying the currently defined loggers and their levels as well as then modifying the levels dynamically at runtime.

For more information on how to use [Apps Manager](https://techdocs.broadcom.com/us/en/vmware-tanzu/platform/elastic-application-runtime/10-3/eart/dev-console.html) on Cloud Foundry for viewing and modifying logging levels, see the Using Actuators with Apps Manager section of the Tanzu Platform documentation.

> NOTE: The Apps Manager integration involves sending the fully-qualified logger name over HTTP. Avoid using colons in the name of a logger to prevent invalid HTTP Requests.

## Usage

Before starting to use Steeltoe provider, you should have a good understanding of how the .NET [Logging service](https://learn.microsoft.com/aspnet/core/fundamentals/logging/) works, as it is nothing more than a wrapper around the existing Microsoft Console logger.

In order to use the Steeltoe logging provider, you need to do the following:

1. Add the Logging NuGet package references to your project.
1. Configure Logging settings.
1. Add the Dynamic logging provider to the logging builder.

### Add NuGet References

To use the logging provider, you need to add a reference to the Steeltoe Dynamic Logging NuGet.

The provider is found in the `Steeltoe.Extensions.Logging.DynamicLogger` package.

You can add the provider to your project by using the following `PackageReference`:

```xml
<ItemGroup>
...
    <PackageReference Include="Steeltoe.Extensions.Logging.DynamicLogger" Version="2.*" />
...
</ItemGroup>
```

### Configure Settings

As mentioned earlier, the Steeltoe Logging provider is a wrapper around the Microsoft Console logging provider. Consequently, you can configure it the same way you would the Microsoft provider. For more details on how this is done, see the section on [Apply log filter rules in code](https://learn.microsoft.com/aspnet/core/fundamentals/logging/#apply-log-filter-rules-in-code).

### Add Logging Provider

In order to use the provider, you need to add it to the logging builder by using the `AddDynamicConsole()` extension method, as shown in the following example:

```csharp
using Steeltoe.Extensions.Logging;
public class Program
{
    public static void Main(string[] args)
    {
        var host = WebHost.CreateDefaultBuilder(args)
            .UseStartup<Startup>()
            .ConfigureLogging((builderContext, loggingBuilder) =>
            {
                // Add Steeltoe Dynamic Logging provider
                loggingBuilder.AddDynamicConsole();
            })
            .Build();

        host.Run();
    }
}
```
