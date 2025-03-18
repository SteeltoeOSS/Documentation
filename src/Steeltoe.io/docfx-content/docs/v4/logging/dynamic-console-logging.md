# Dynamic Console Logging

This logging provider is a wrapper around the [Microsoft Console Logger](https://learn.microsoft.com/dotnet/core/extensions/logging-providers#console). It allows for querying the active loggers and their minimum levels, as well as then modifying the levels dynamically at runtime via the [loggers actuator](../management/loggers.md).

> [!CAUTION]
> External tool integration involves sending the fully-qualified logger name over HTTP. Avoid using colons in the name of a logger to prevent invalid HTTP requests.

## Usage

Before starting to use Steeltoe provider, you should know how [Logging in .NET](https://learn.microsoft.com/aspnet/core/fundamentals/logging) works, as Steeltoe provides nothing more than a wrapper around the existing Microsoft console logger.

To use the Steeltoe logging provider, you need to:

1. Add the appropriate NuGet package reference to your project.
1. Configure logging settings.
1. Add the dynamic logging provider to the logging builder.

### Add NuGet References

To use the logging provider, you need to add a reference to the `Steeltoe.Logging.DynamicConsole` NuGet package.

### Configure Settings

As mentioned earlier, the Steeltoe Logging provider is a wrapper around the Microsoft Console logging provider. Consequently, you can configure it the same way you would the Microsoft provider. For more details on how this is done, see [Configure logging](https://learn.microsoft.com/aspnet/core/fundamentals/logging#configure-logging).

### Add the Logging Provider

To use the provider, you need to add it to the logging builder by using the `AddDynamicConsole()` extension method:

```csharp
using Steeltoe.Logging.DynamicConsole;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddDynamicConsole();
```
