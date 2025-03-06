# Dynamic Console Logging

This logging provider is a wrapper around the [Microsoft Console Logger](https://learn.microsoft.com/dotnet/core/extensions/logging-providers#console). It allows you to:

* Query the active loggers and their minimum levels
* Modify the levels dynamically at runtime using the [loggers actuator](../management/loggers.md)

> [!CAUTION]
> External tool integration involves sending the fully-qualified logger name over HTTP. Avoid using colons in the name of a logger to prevent invalid HTTP requests.

## Usage

Before starting to use Steeltoe provider, you should know how [Logging in .NET](https://learn.microsoft.com/aspnet/core/fundamentals/logging) works; Steeltoe provides nothing more than a wrapper around the existing Microsoft console logger.

To use the Steeltoe logging provider:

1. Add the appropriate NuGet package reference to your project.
2. Configure logging settings.
3. Add the dynamic logging provider to the logging builder.

### Add NuGet References

To use the logging provider, add a reference to the `Steeltoe.Logging.DynamicConsole` NuGet package.

### Configure Settings

The Steeltoe Logging provider is a wrapper around the Microsoft Console logging provider, so you can configure it the same way you would the Microsoft provider. For more details about how to do this, see [Configure logging](https://learn.microsoft.com/aspnet/core/fundamentals/logging#configure-logging).

### Add the Logging Provider

To use the provider, add it to the logging builder by using the `AddDynamicConsole()` extension method:

```csharp
using Steeltoe.Logging.DynamicConsole;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddDynamicConsole();
```
