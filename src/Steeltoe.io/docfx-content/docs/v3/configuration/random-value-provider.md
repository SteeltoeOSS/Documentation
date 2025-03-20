# RandomValue Provider

Sometimes, you might need to generate random values as part of your application's configuration values.

The Steeltoe `RandomValue` generator is a configuration provider that you can use to do just that. It can produce integers, longs, uuids, or strings, as the following examples show:

```csharp
var my_secret = config["random:value"];
var my_number = config["random:int"];
var my_big_number = config["random:long"];
var my_uuid = config["random:uuid"];
var my_number_less_than_ten = config["random:int(10)"];
var my_number_in_range = config["random:int[1024,65536]"];

```

You can also use the generator together with property placeholders. For example, consider the following `appsettings.json`:

```json
{
    "my": {
        "secret": "${random:value}",
        "number": "${random:int}",
        "big_number": "${random:long}",
        "uuid": "${random:uuid}",
        "number_less_than_ten": "${random:int(10)}",
        "number_in_range": "${random:int[1024,65536]}"
    }
}
```

## Usage

You should have a good understanding of how the .NET [Configuration services](https://docs.microsoft.com/aspnet/core/fundamentals/configuration) work before starting to use this provider.

In order to use the Steeltoe RandomValue provider, you need to:

1. Add a NuGet package reference to your project.
1. Add the provider to the Configuration Builder.
1. Access random values from the `IConfiguration`.

### Add NuGet Reference

To use the provider, you need to add a reference to the appropriate Steeltoe NuGet.

To do so, add a `PackageReference` resembling the following:

```xml
<ItemGroup>
...
    <PackageReference Include="Steeltoe.Extensions.Configuration.RandomValueBase" Version="3.2.0"/>
...
</ItemGroup>
```

### Add Configuration Provider

To have the ability to generate random values from the configuration, you need to add the `RandomValue` generator provider to the `ConfigurationBuilder`.

The following example shows how to do so:

```csharp
using Steeltoe.Extensions.Configuration.RandomValue;
...

var builder = new ConfigurationBuilder()
    .SetBasePath(env.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)

    // Add RandomValue generator
    .AddRandomValueSource();
Configuration = builder.Build();
...

```

>If you wish to generate random values as part of using placeholders, you need to add the `RandomValue` provider to the builder before you add the placeholder resolver.

### Access Random Value Data

Once the configuration has been built, the `RandomValue` generator can be used to generate values. You can access the configuration data by using the appropriate `random` keys.

Consider the following `HomeController` example:

```csharp
public class HomeController : Controller
{
    private IConfiguration _config;
    public HomeController(IConfiguration config)
    {
        _config = config;
    }
    public IActionResult Index()
    {
        ViewData["random:int"] = _config["random:int"];
        ViewData["random:long"] = _config["random:long"];
        ViewData["random:int(10)"] = _config["random:int(10)"];
        ViewData["random:long(100)"] = _config["random:long(100)"];
        ViewData["random:int(10,20)"] = _config["random:int(10,20)"];
        ViewData["random:long(100,200)"] = _config["random:long(100,200)"];
        ViewData["random:uuid"] = _config["random:uuid"];
        ViewData["random:string"] = _config["random:string"];
        return View();
    }
    ...
}
```
