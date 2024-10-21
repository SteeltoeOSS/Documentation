# Serilog Dynamic Logger

This logging provider extends the dynamic logging provider with [Serilog](https://serilog.net/). This allows logger minimum levels configured via Serilog to be queried and modified at runtime via the loggers management endpoint.

## Usage

In order to use the Serilog Dynamic Logger, you need to do the following:

1. Add the appropriate NuGet package reference to your project.
1. Configure Logging settings.
1. Add the Serilog Dynamic Logger to the logging builder.

### Add NuGet References

To use the logging provider, you need to add a reference to the `Steeltoe.Logging.DynamicSerilog` NuGet package.

### Configure Settings

As mentioned earlier, the Serilog Dynamic Logger provider extends Serilog. Consequently, you can configure it the same way you would Serilog. For more details on how this is done, see [Serilog-Settings-Configuration](https://github.com/serilog/serilog-settings-configuration).

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Warning",
      "Override": {
        "Microsoft": "Warning",
        "Steeltoe": "Information",
        "MyApp": "Verbose"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Properties} {NewLine} {EventId} {Message:lj}{NewLine}{Exception}"
        }
      }
    ],
    "Enrich": [ "FromLogContext", "WithMachineName", "WithThreadId" ]
  }
}
```

### Add Serilog Dynamic Logger

In order to use this logging provider, you need to add it to the logging builder. Steeltoe provides methods to add directly to `ILoggingBuilder`, along with convenience methods for adding to host builders.

#### Host builder

The following example shows usage of a host builder extension:

```csharp
using Steeltoe.Logging.DynamicSerilog;

var builder = WebApplication.CreateBuilder(args);
builder.AddDynamicSerilog();
```

#### LoggingBuilder

The following example shows usage of the `ILoggingBuilder` extension:

```csharp
using Steeltoe.Logging.DynamicSerilog;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddDynamicSerilog();
```
