# Serilog Dynamic Logger

This logging provider extends the dynamic logging provider with [Serilog](https://serilog.net/). This allows logger levels configured via Serilog to be queried and modified at runtime via the loggers endpoint.

The source code for the Serilog Dynamic Logger can be found [here](https://github.com/SteeltoeOSS/steeltoe/tree/master/src/Logging/src/DynamicSerilogCore).

A sample working project can be found [here](https://github.com/SteeltoeOSS/Samples/tree/master/Management/src/CloudFoundry).

## Usage

In order to use the Serilog Dynamic Logger, you need to do the following:

1. Add the Logging NuGet package references to your project.
1. Configure Logging settings.
1. Add the Serilog Dynamic Logger to the logging builder.

### Add NuGet References

To use the logging provider, you need to add a reference to the Steeltoe Dynamic Logging NuGet.

The provider is found in the `Steeltoe.Extensions.Logging.DynamicSerilogBase` package.

You can add the provider to your project by using the following `PackageReference`:

```xml
<ItemGroup>
...
    <PackageReference Include="Steeltoe.Extensions.Logging.DynamicSerilogBase" Version= "3.0.1"/>
...
</ItemGroup>
```

### Configure Settings

As mentioned earlier, the Serilog Dynamic Logger provider extends Serilog. Consequently, you can configure it the same way you would Serilog. For more details on how this is done, see the section on [Serilog-Settings-Configuration](https://github.com/serilog/serilog-settings-configuration).

```json
...
"Serilog": {
    "IncludeScopes": false,
    "MinimumLevel": {
        "Default": "Warning",
        "Override": {
            "Microsoft": "Warning",
            "Steeltoe": "Information",
            "CloudFoundry.Controllers": "Verbose"
        }
    },
    "WriteTo": [{
        "Name": "Console",
        "Args": {
            "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Properties} {NewLine} {EventId} {Message:lj}{NewLine}{Exception}"
        }
    }],
    "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"]
},
...
```

### Add Serilog Dynamic Logger

In order to use the provider, you need to add it to the logging builder by using the `AddSerilogDynamicConsole()` extension method, as shown in the following example:

```csharp
using Steeltoe.Extensions.Logging;
public class Program
{
    public static void Main(string[] args)
    {
        var host = new WebHostBuilder()
            .UseKestrel()
            .UseCloudHosting()
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

                // Add Serilog Dynamic Logger
                loggingBuilder.AddSerilogDynamicConsole();
            })
            .Build();

        host.Run();
    }
}
```
