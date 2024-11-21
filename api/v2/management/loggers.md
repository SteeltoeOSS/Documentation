# Loggers

The Steeltoe loggers management endpoint includes the ability to view and configure the logging levels of your application at runtime when using the Steeltoe Logging provider.

You can view a list of all active loggers in an application and their current configuration. The configuration information is made up of both the explicitly configured logging levels as well as the effective level given to it by the logging framework.

## Configure Settings

The following table describes the settings that you can apply to the endpoint.

|Key|Description|Default|
|---|---|---|
|id|The ID of the loggers endpoint|`loggers`|
|enabled|Enable or disable loggers management endpoint|true|
|sensitive|Currently not used|false|
|requiredPermissions|User permissions required on Cloud Foundry to access endpoint|RESTRICTED|

**Note**: **Each setting above must be prefixed with `management:endpoints:loggers`**.

## Enable HTTP Access

The default path to the Loggers endpoint is computed by combining the global `path` prefix setting together with the `id` setting from above. The default path is  `/actuator/loggers`.

The coding steps you take to enable HTTP access to the Loggers endpoint together with how to use the Steeltoe Logging provider, differs depending on the type of .NET application your are developing.  The sections which follow describe the steps needed for each of the supported application types.

>NOTE: The Steeltoe logging provider is a wrapper around the [Microsoft Console Logging](https://github.com/aspnet/Logging) provider from Microsoft. This wrapper allows querying defined loggers and modifying the levels dynamically at runtime.

### ASP.NET Core App

To add the Loggers actuator to the service container, use the `AddLoggersActuator()` extension method from `EndpointServiceCollectionExtensions`.

To add the Loggers actuator middleware to the ASP.NET Core pipeline, use the `UseLoggersActuator()` extension method from `EndpointApplicationBuilderExtensions`.

To add the Steeltoe Logging provider to the `ILoggerFactory`, use the `AddDynamicConsole()` extension method and update the `Program.cs` class as shown below:

```csharp
using Steeltoe.Extensions.Logging;
public class Program
{
    public static void Main(string[] args)
    {
        var host = new WebHostBuilder()
            .UseKestrel()
            .UseContentRoot(Directory.GetCurrentDirectory())
            .UseStartup<Startup>()
            .ConfigureAppConfiguration((builderContext, config) =>
            {
                config.SetBasePath(builderContext.HostingEnvironment.ContentRootPath)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{builderContext.HostingEnvironment.EnvironmentName}.json", optional: true)
                    .AddEnvironmentVariables();
            })
            .ConfigureLogging((builderContext, loggingBuilder) =>
            {
                loggingBuilder.AddConfiguration(builderContext.Configuration.GetSection("Logging"));

                // Add Steeltoe dynamic console logger
                loggingBuilder.AddDynamicConsole();
            })
            .Build();

        host.Run();
    }
}
```

### ASP.NET 4.x App

To add the Loggers actuator endpoint, use the `UseLoggerActuator()` method from `ActuatorConfigurator`.

The following example shows how enable the Loggers endpoint and configure it with the Steeltoe Logging provider.

```csharp
public class ManagementConfig
{
    public static void ConfigureManagementActuators(IConfiguration configuration)
    {
        ...
        ActuatorConfigurator.UseLoggerActuator(configuration, LoggingConfig.LoggerProvider, LoggingConfig.LoggerProvider);
        ...
    }
```

Below is an example of how you can create a Steeltoe Logging provider in an 4.x application.

```csharp
public static class LoggingConfig
{
    public static ILoggerFactory LoggerFactory { get; set; }
    public static ILoggerProvider LoggerProvider { get; set; }

    public static void Configure(IConfiguration configuration)
    {
        LoggerProvider = new DynamicLoggerProvider(new ConsoleLoggerSettings().FromConfiguration(configuration));
        LoggerFactory = new LoggerFactory();
        LoggerFactory.AddProvider(LoggerProvider);
    }
}
```

### ASP.NET OWIN App

To add the Loggers actuator middleware to the ASP.NET OWIN pipeline, use the `UseLoggersActuator()` extension method from `LoggersEndpointAppBuilderExtensions`.

The following example shows how enable the Loggers endpoint and configure it with the Steeltoe Logging provider.

```csharp
public class Startup
{
    ...
    public void Configuration(IAppBuilder app)
    {
        ...
        app.UseLoggersActuator(
            ApplicationConfig.Configuration,
            LoggingConfig.LoggerProvider,
            LoggingConfig.LoggerFactory);
        ...
    }
}
```

Below is an example of how you can create a Steeltoe Logging provider in an 4.x application.

```csharp
public static class LoggingConfig
{
    public static ILoggerFactory LoggerFactory { get; set; }
    public static ILoggerProvider LoggerProvider { get; set; }

    public static void Configure(IConfiguration configuration)
    {
        LoggerProvider = new DynamicLoggerProvider(new ConsoleLoggerSettings().FromConfiguration(configuration));
        LoggerFactory = new LoggerFactory();
        LoggerFactory.AddProvider(LoggerProvider);
    }
}
```

## Interacting with the Loggers Actuator

To retrieve the loggers that can be configured and the log levels that are allowed, send an HTTP GET request to `/{LoggersActuatorPath}`.

Log levels can be changed at namespace or class levels with an HTTP POST request to `/{LoggersActuatorPath}/{NamespaceOrClassName}` and a JSON request body that defines the minimum level you wish to log:

```json
{
  "configuredLevel":"INFO"
}
```

> NOTE: The TAS Apps Manager integration involves sending the fully-qualified logger name over HTTP. Avoid using colons in the name of a logger to prevent invalid HTTP Requests.

