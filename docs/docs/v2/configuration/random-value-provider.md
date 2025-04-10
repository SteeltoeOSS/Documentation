# RandomValue Provider

Sometimes you might find the need to generate random values as part of your applications configuration values.

The Steeltoe RandomValue generator is a configuration provider which you can use to do just that. It can produce integers, longs, uuids or strings as shown in the following examples:

```csharp
var my_secret = config["random:value"];
var my_number = config["random:int"];
var my_big_number = config["random:long"];
var my_uuid = config["random:uuid"];
var my_number_less_than_ten = config["random:int(10)"];
var my_number_in_range = config["random:int[1024,65536]"];

```

You can also use the generator together with property placeholders. For example, consider the following `appsettings.json`

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

The RandomValue provider supports the following .NET application types:

* ASP.NET (MVC, WebForms, WebAPI, WCF)
* ASP.NET Core
* Console apps (.NET Framework and .NET Core)

 The source code for this provider can be found [here](https://github.com/SteeltoeOSS/Configuration).

## Usage

You should have a good understanding of how the .NET [Configuration services](https://docs.microsoft.com/aspnet/core/fundamentals/configuration) work before starting to use this provider.

In order to use the Steeltoe RandomValue provider you need to do the following:

1. Add a NuGet package reference to your project.
1. Add the provider to the Configuration Builder.
1. Access random values from the `IConfiguration`.

>NOTE: Most of the example code in the following sections is based on using Steeltoe in an ASP.NET Core application. If you are developing an ASP.NET 4.x application or a Console based app, see the [other samples](https://github.com/SteeltoeOSS/Samples/tree/2.x/Configuration) for example code you can use.

### Add NuGet Reference

To use the provider, you need to add a reference to the appropriate Steeltoe NuGet.

To do this add a `PackageReference` resembling the following:

```xml
<ItemGroup>
...
    <PackageReference Include="Steeltoe.Extensions.Configuration.RandomValueBase" Version="2.5.2" />
...
</ItemGroup>
```

### Add Configuration Provider

In order to have the ability to generate random values from the configuration, you need to add the RandomValue generatore provider to the `ConfigurationBuilder`.

The following example shows how to add to this:

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

>NOTE: It if you wish to generate random values as part of using placeholders, then it's important to add the RandomValue provider to the builder before you add the Placeholder resolver.

### Access Random Value Data

Once the configuration has been built, the RandomValue generator can be used to generate values.  Simply access the configuration data using the appropriate `random` keys.

Consider the following `HomeController`:

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
