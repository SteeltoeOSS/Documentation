# Placeholder Provider

The Placeholder resolver enables usage of `${....}` placeholders in your configuration. The provider lets you define configuration values as placeholders in your configuration and have them be resolved to `real` values at runtime during configuration access.

A placeholder takes the form of `${key:subkey1:subkey2?default_value}`, where `key:subkey1:subkey2` represents another key in the configuration. At runtime, when you access the key associated with the placeholder, the resolver is called to resolve the placeholder key to a value that exists in the configuration. If a value for the placeholder key is not found, the key is returned unresolved. If a `default_value` is specified in the placeholder, the `default_value` is returned instead.

Note that placeholder defaults (for example, `default_value`) can be defined to be placeholders as well and those are resolved as well.

## Usage

You should have a good understanding of how the .NET [Configuration services](https://docs.microsoft.com/aspnet/core/fundamentals/configuration) work before starting to use this provider.

To use the Steeltoe Placeholder resolver provider, you need to:

1. Add a NuGet package reference to your project.
1. Add the provider to the Configuration Builder.
1. Optionally, configure Options classes by binding configuration data to the classes.
1. Inject and use the Options classes or access configuration data directly.

### Add NuGet Reference

To use the provider, you need to add a reference to the appropriate Steeltoe NuGet based on the type of the application you are building and what dependency injector you have chosen, if any. The following table describes the available packages:

| Package | Description | .NET Target |
| --- | --- | --- |
| `Steeltoe.Extensions.Configuration.PlaceholderBase` | Base functionality. No dependency injection. | .NET Standard 2.0 |
| `Steeltoe.Extensions.Configuration.PlaceholderCore` | Includes base. Adds ASP.NET Core dependency injection. | ASP.NET Core 3.1+ |

To add this type of NuGet to your project, add a `PackageReference` resembling the following:

```xml
<ItemGroup>
...
    <PackageReference Include="Steeltoe.Extensions.Configuration.PlaceholderCore" Version="3.2.0"/>
...
</ItemGroup>
```

### Add Configuration Provider

In order to have placeholders resolved when accessing your configuration data, you need to add the placeholder resolver provider to the `ConfigurationBuilder`.

There are four different ways to do so:

1. Add the resolver by using `ConfigurationBuilder` extension method `AddPlaceholderResolver()`.
1. Add the resolver to an already built configuration by using `IConfiguration` extension method `AddPlaceholderResolver()`.
1. Add the resolver by using the `IWebHostBuilder` extension method `AddPlaceholderResolver()`.
1. Use the `ConfigurePlaceholderResolver()` in `ConfigureServices()` to add the resolver to the already built `IConfiguration` and to replace it in the container.

The following example shows how to add to the `ConfigurationBuilder`:

```csharp
using Steeltoe.Extensions.Configuration.Placeholder;
...

var builder = new ConfigurationBuilder()
    .SetBasePath(env.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)

    // Add Placeholder resolver
    .AddPlaceholderResolver();
Configuration = builder.Build();
...

```

Extensions are also provided for quick addition to both `IHostBuilder` and `IWebHostBuilder`. Their usage is identical. The following example shows how to add to the `IWebHostBuilder`:

```csharp
public class Program
{
    public static void Main(string[] args)
    {
        BuildWebHost(args).Run();
    }
    public static IWebHost BuildWebHost(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            .UseCloudHosting()
            .AddPlaceholderResolver()
            .UseStartup<Startup>()
            .Build();
}
```

>It is important to understand that the Placeholder resolver works by wrapping and replacing the existing configuration providers already added to the `ConfigurationBuilder`. As a result you typically will want to add it as the last provider.

### Access Configuration Data

Once the configuration has been built, the placeholder resolver is used to resolve any placeholders as you access your configuration data. You can access the configuration data as your normally would, and the resolver tries to resolve any placeholder before returning the value for the key requested.

Consider the following `appsettings.json` file:

```json
{
  "Spring": {
    "Application": {
      "Name": "myName"
    },
    "Cloud": {
      "Config": {
        "Name" : "${Spring:Application:Name?no_name}",
      }
    }
  }
  ...
}
```

When using the normal `IConfiguration` indexer to access the configuration, you see the Placeholder resolver do its thing:

```csharp
var config = builder.Build();

Assert.Equal("myName", config["Spring:Cloud:Config:Name"]);
...
```

### Access Configuration Data as Options

Alternatively, instead of accessing the configuration data directly from the configuration, you can also use the .NET [Options](https://docs.microsoft.com/aspnet/core/fundamentals/configuration) framework together with placeholders.

First, consider the following `appsettings.json` and `appsettings.Development.json` files:

```json
// appsettings.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ResolvedPlaceholderFromEnvVariables": "${PATH?NotFound}",
  "UnresolvedPlaceholder": "${SomKeyNotFound?NotFound}",
  "ResolvedPlaceholderFromJson": "${Logging:LogLevel:System?${Logging:LogLevel:Default?NotFound}}"
}
// appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "System": "Information",
      "Microsoft": "Information"
    }
  }
}
```

Notice that `ResolvedPlaceholderFromEnvVariables` uses a placeholder to reference the `PATH` environment variable, which is added to the configuration by the default Web host builder.
Also notice that `ResolvedPlaceholderFromJson` uses a placeholder to reference keys that come from the `.json` configuration files.

Next, add the placeholder resolver to the `IWebHostBuilder` in `Program.cs` (or in any of the other ways described earlier):

```csharp
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Steeltoe.Extensions.Configuration.Placeholder;
public class Program
{
    public static void Main(string[] args)
    {
        CreateWebHostBuilder(args).Build().Run();
    }

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        WebHost.CreateDefaultBuilder(args)

            // Add Steeltoe Placeholder resolver to apps configuration providers
            .AddPlaceholderResolver()
            .UseStartup<Startup>();
}
```

Then, to use the configuration and the added placeholder resolver together with your Options classes, you can configure the Options as you normally would:

```csharp

// Options class
public class SampleOptions
{
        public string ResolvedPlaceholderFromEnvVariables { get; set; }
        public string UnresolvedPlaceholder { get; set; }
        public string ResolvedPlaceholderFromJson { get; set; }
}

// Startup.cs
public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }
    public void ConfigureServices(IServiceCollection services)
    {
        // Setup Options framework with DI
        services.AddOptions();

        // Configure the SampleOptions class with configuration data
        services.Configure<SampleOptions>(Configuration);
    }
    ....
}
```
