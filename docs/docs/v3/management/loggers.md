# Loggers

The Steeltoe loggers management endpoint includes the ability to view and configure the logging levels of your application at runtime when using the Steeltoe logging provider.

You can view a list of all active loggers in an application and their current configuration. The configuration information is made up of both the explicitly configured logging levels as well as the effective level given to it by the logging framework.

## Configure Settings

The following table describes the settings that you can apply to the endpoint:

| Key | Description | Default |
| --- | --- | --- |
| `Id` | The ID of the loggers endpoint. | `loggers` |
| `Enabled` | Enable or disable loggers management endpoint. | `true` |
| `Sensitive` | Currently not used. | `false` |
| `RequiredPermissions` | User permissions required on Cloud Foundry to access endpoint. | `RESTRICTED` |

>Each setting above must be prefixed with `Management:Endpoints:Loggers`.

## Enable HTTP Access

The default path to the Loggers endpoint is computed by combining the global `Path` prefix setting together with the `Id` setting described in the preceding section. The default path is `/actuator/loggers`.

See the [HTTP Access](./using-endpoints.md#http-access) section to see the overall steps required to enable HTTP access to endpoints in an ASP.NET Core application.

To add the actuator to the service container and map its route, use the `AddLoggersActuator` extension methods from `ManagementHostBuilderExtensions`.

Alternatively, first, add the Loggers actuator to the service container, using the `AddLoggersActuator()` extension method from `EndpointServiceCollectionExtensions`.

Then, add the Loggers actuator middleware to the ASP.NET Core pipeline, using the `Map<LoggersEndpoint>()` extension method from `ActuatorRouteBuilderExtensions`.

To add the Steeltoe Logging provider to the `ILoggerFactory`, use the `AddDynamicConsole()` extension method and update the `Program.cs` class, as follows:

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

## Modifying Log Levels

To retrieve the loggers that can be configured and the log levels that are allowed, send an HTTP GET request to `/{LoggersActuatorPath}`.

Log levels can be changed at namespace or class levels with an HTTP POST request to `/{LoggersActuatorPath}/{NamespaceOrClassName}` and a JSON request body that defines the minimum level you wish to log:

```json
{
  "configuredLevel":"INFO"
}
```

## Apps Manager

Apps Manager integration involves sending the fully-qualified logger name over HTTP. Avoid using colons in the name of a logger to prevent invalid HTTP Requests.
