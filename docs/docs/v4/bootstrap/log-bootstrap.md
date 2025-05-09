# Logger Bootstrapping

For some Steeltoe components, primarily configuration providers, an `ILoggerFactory` must be provided to retrieve logs for debugging.

## Using BootstrapLoggerFactory

`BootstrapLoggerFactory`  logs to the console until the service container has been built.
After the service container has become available, it automatically upgrades existing loggers to use
the application configuration and adapt to configuration changes at runtime.

For example, the following code uses `BootstrapLoggerFactory` to write startup logs from the Config Server configuration provider to the console:

```csharp
using Steeltoe.Common.Logging;
using Steeltoe.Configuration.ConfigServer;

var builder = WebApplication.CreateBuilder(args);
var loggerFactory = BootstrapLoggerFactory.CreateConsole();

builder.Configuration.AddConfigServer(loggerFactory);
builder.Services.UpgradeBootstrapLoggerFactory(loggerFactory);
```

All `BootstrapLoggerFactory` creation methods provide an `ILoggingBuilder` action parameter that enables further configuration of the startup logger.

For example, the following code adds the Debug logger and sets the minimum level to `Warning`:

```csharp
var loggerFactory = BootstrapLoggerFactory.CreateConsole(loggingBuilder =>
{
    loggingBuilder.AddDebug();
    loggingBuilder.SetMinimumLevel(LogLevel.Warning);
});
```

> [!TIP]
> To skip usage of the console logger, use `BootstrapLoggerFactory.CreateEmpty`
> with the `ILoggingBuilder` action parameter to configure from scratch.

## Using LoggerFactory.Create

An `ILoggerFactory` can be created directly using `LoggerFactory.Create`.
Beware that this doesn't provide the advantages of `BootstrapLoggerFactory`.

The following code writes all logs from the Config Server configuration provider to the console:

```csharp
using Steeltoe.Configuration.ConfigServer;

var builder = WebApplication.CreateBuilder(args);
var loggerFactory = LoggerFactory.Create(loggingBuilder =>
{
    loggingBuilder.AddConsole();
    loggingBuilder.SetMinimumLevel(LogLevel.Trace);
});

builder.Configuration.AddConfigServer(loggerFactory);
```
