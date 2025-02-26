# Random Value Provider

Sometimes, you might need to generate random values as part of your application's configuration values.

The Steeltoe random value generator is a configuration provider that you can use to do just that. It can produce integers, longs, GUIDs, or strings, as the following examples show:

```csharp
string? randomValue = builder.Configuration["random:value"];
int randomNumber = builder.Configuration.GetValue<int>("random:int");
long randomLargeNumber = builder.Configuration.GetValue<long>("random:long");
Guid randomGuid = builder.Configuration.GetValue<Guid>("random:uuid");
int randomNumberLessThanTen = builder.Configuration.GetValue<int>("random:int(10)");
int randomNumberInRange = builder.Configuration.GetValue<int>("random:int[1024,65536]");
```

You can also use the generator together with property placeholders. For example, consider the following `appsettings.json`:

```json
{
    "Example": {
        "RandomValue": "${random:value}",
        "RandomNumber": "${random:int}",
        "RandomLargeNumber": "${random:long}",
        "RandomGuid": "${random:uuid}",
        "RandomNumberLessThanTen": "${random:int(10)}",
        "RandomNumberInRange": "${random:int[1024,65536]}"
    }
}
```

## Usage

You should have a good understanding of how the [.NET Configuration System](https://learn.microsoft.com/aspnet/core/fundamentals/configuration) works before starting to use this provider.

To use the Steeltoe random value provider, you need to:

1. Add the appropriate NuGet package reference to your project.
1. Add the provider to the Configuration Builder.
1. Access random values from the `IConfiguration`.

### Add NuGet Reference

To use the provider, you need to add a reference to the `Steeltoe.Configuration.RandomValue` NuGet package.

### Add Configuration Provider

To have the ability to generate random values from the configuration, you need to add the random value provider to the `ConfigurationBuilder`.

The following example shows how to do so:

```csharp
using Steeltoe.Configuration.RandomValue;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddRandomValueSource();
```

> [!TIP]
> If you wish to generate random values as part of using placeholders, you need to add the random value provider to the builder *before* you add the placeholder resolver.

### Access Random Value Data

Once the configuration has been built, the random value provider can be used to generate values. You can access the configuration data by using the appropriate `random` keys.

Consider the following `HomeController` example:

```csharp
public class HomeController(IConfiguration configuration) : Controller
{
    public IActionResult Index()
    {
        ViewData["random:int"] = configuration["random:int"];
        ViewData["random:long"] = configuration["random:long"];
        ViewData["random:int(10)"] = configuration["random:int(10)"];
        ViewData["random:long(100)"] = configuration["random:long(100)"];
        ViewData["random:int(10,20)"] = configuration["random:int(10,20)"];
        ViewData["random:long(100,200)"] = configuration["random:long(100,200)"];
        ViewData["random:uuid"] = configuration["random:uuid"];
        ViewData["random:string"] = configuration["random:string"];
        return View();
    }
}
```
