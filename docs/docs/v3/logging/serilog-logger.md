# Serilog Dynamic Logger

This logging provider extends the dynamic logging provider with [Serilog](https://serilog.net/). This allows logger levels configured via Serilog to be queried and modified at runtime via the loggers endpoint.

The source code for the Serilog Dynamic Logger can be found [here](https://github.com/SteeltoeOSS/steeltoe/tree/release/3.2/src/Logging/src/).

A sample working project can be found [here](https://github.com/SteeltoeOSS/Samples/tree/3.x/Management/src/CloudFoundry).

## Usage

In order to use the Serilog Dynamic Logger, you need to do the following:

1. Add the Logging NuGet package references to your project.
1. Configure Logging settings.
1. Add the Serilog Dynamic Logger to the logging builder.

### Add NuGet References

To use the logging provider, you need to add a package reference to a Steeltoe Dynamic Logging package.

The provider is found in the `Steeltoe.Extensions.Logging.DynamicSerilogBase` package. If you wish to use the [WebHostBuilder extension](#webhostbuilder), use the package `Steeltoe.Extensions.Logging.DynamicSerilogCore`.

You can add the provider to your project by using the following `PackageReference`:

```xml
<ItemGroup>
...
    <PackageReference Include="Steeltoe.Extensions.Logging.DynamicSerilogBase" Version="3.2.0"/>
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

In order to use this logging provider, you need to add it to the logging builder. Steeltoe provides methods to add directly to `ILoggingBuilder`, along with convenience methods for adding to `HostBuilder` and `WebHostBuilder`.

#### HostBuilder

The following example shows usage of the `HostBuilder` extension:

```csharp
using Steeltoe.Extensions.Logging.DynamicSerilog;
public class Program
{
    public static void Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder()
            .UseStartup<Startup>()
            .AddDynamicSerilog()
            .Build();

        host.Run();
    }
}
```

#### LoggingBuilder

The following example shows usage of the `ILoggingBuilder` extension:

```csharp
using Steeltoe.Extensions.Logging.DynamicSerilog;
public class Program
{
    public static void Main(string[] args)
    {
        var host = WebHost.CreateDefaultBuilder()
            .UseStartup<Startup>()
            .ConfigureLogging((builderContext, loggingBuilder) =>
            {
                // Add Serilog Dynamic Logger
                loggingBuilder.AddDynamicSerilog();
            })
            .Build();

        host.Run();
    }
}
```

#### WebHostBuilder

The following example shows usage of the `WebHostBuilder` extension:

```csharp
using Steeltoe.Extensions.Logging.DynamicSerilog;
public class Program
{
    public static void Main(string[] args)
    {
        var host = WebHost.CreateDefaultBuilder()
            .UseStartup<Startup>()
            .AddDynamicSerilog()
            .Build();

        host.Run();
    }
}
```

> Please be aware this extension is provided in Steeltoe.Extensions.Logging.DynamicSerilogCore
