# Dynamic Serilog Logging

This logging provider integrates with [Serilog](https://serilog.net/). It enables logger minimum levels configured via Serilog to be queried and modified at runtime via the [loggers actuator](../management/loggers.md).

## Usage

To use the Serilog Dynamic Logger, you need to do the following:

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
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Properties}{NewLine}  {Message:lj}{NewLine}{Exception}"
        }
      }
    ],
    "Enrich": [ "FromLogContext", "WithMachineName", "WithThreadId" ]
  }
}
```

Alternatively, configuration can be built from code using the Serilog API:

```csharp
using Serilog;
using Serilog.Events;

var serilogConfiguration = new LoggerConfiguration()
    .MinimumLevel.Warning()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Steeltoe", LogEventLevel.Information)
    .MinimumLevel.Override("MyApp", LogEventLevel.Verbose)
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Properties}{NewLine}  {Message:lj}{NewLine}{Exception}")
    .Enrich.FromLogContext();
```

### Add Serilog Dynamic Logger

To use this logging provider, you need to add it to the logging builder by using the `AddDynamicSerilog()` extension method:

```csharp
using Steeltoe.Logging.DynamicSerilog;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddDynamicSerilog();
```

If you built the Serilog configuration from code, use the appropriate overload instead:
```csharp
builder.Logging.AddDynamicSerilog(serilogConfiguration);
```

### Serilog API usage

Because dynamic logging is built on the `Microsoft.Extensions.Logging` abstractions, changing log levels dynamically **won't work** if you're using Serilog's static `Log` class directly.

```csharp
using Serilog;

// Use ILogger instead.
Log.Warning("DO NOT USE!");
```

Instead, use the injectable `ILogger` interface to log messages, and the `ILoggerFactory` interface to create loggers.

```csharp
using Steeltoe.Logging.DynamicSerilog;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddDynamicSerilog();

var app = builder.Build();

var programLogger = app.Services.GetRequiredService<ILogger<Program>>();
programLogger.LogInformation("Hello from Program.");

var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
var exampleLogger = loggerFactory.CreateLogger("Example");
exampleLogger.LogInformation("Hello from Example.");
```

The Serilog dynamic logger supports the use of logger scopes, as well as Serilog's enrichers and destructuring.

```csharp
using Serilog.Context;
using Steeltoe.Logging.DynamicSerilog;

var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
var logger = loggerFactory.CreateLogger("Example");

// ILogger scopes.
using (logger.BeginScope("ExampleScope"))
{
    logger.LogInformation("Hello from Example.");
}

// Serilog enrichers.
using (LogContext.PushProperty("SerilogExampleScope", 123))
{
    logger.LogInformation("Hello from Example.");
}

// Serilog destructuring.
logger.LogInformation("App started with {ArgCount} arguments.", args.Length);
```
