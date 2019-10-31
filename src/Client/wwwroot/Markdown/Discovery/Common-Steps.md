# Common Steps

This section contains snippets of commands that you are likely to use repeatedly for common tasks.

## Publish Sample

### ASP.NET Core

Use the `dotnet` CLI to [build and locally publish](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-publish) the application for the framework and runtime you will deploy the application to:

* Linux with .NET Core: `dotnet publish -f netcoreapp2.1 -r ubuntu.14.04-x64`
* Windows with .NET Core: `dotnet publish -f netcoreapp2.1 -r win10-x64`
* Windows with .NET Platform: `dotnet publish -f net461 -r win10-x64`

>NOTE: Starting with .NET Core 2.0, the `dotnet publish` command will automatically restore dependencies for you. Running `dotnet restore` explicitly is not generally required.

### ASP.NET 4.x

1. Open the solution for the sample in Visual Studio
1. Right click on the project, select "Publish"
1. Use the included `FolderProfile` to publish to `bin/Debug/net461/win10-x64/publish`

## Push Sample

Use the Cloud Foundry CLI to push the published application to Cloud Foundry using the parameters that match what you selected for framework and runtime:

```bash
# Push to Linux cell
cf push -f manifest.yml -p bin/Debug/netcoreapp2.1/ubuntu.14.04-x64/publish

# Push to Windows cell, .NET Core
cf push -f manifest-windows.yml -p bin/Debug/netcoreapp2.1/win10-x64/publish

# Push to Windows cell, .NET Framework
cf push -f manifest-windows.yml -p bin/Debug/net461/win10-x64/publish
```

Manifest file names may vary, some samples use a different manifest for .NET 4 vs .NET Core.

>NOTE: All sample manifests have been defined to bind their application to their services, as created earlier.

## Reading Configuration Values

Once settings have been defined, the next step is to read them so that they can be made available.

The following code reads settings from the `appsettings.json` file with the .NET JSON configuration provider (`AddJsonFile("appsettings.json"))` and from `VCAP_SERVICES` with `AddCloudFoundry()`:

```csharp
public class Program {
    ...
    public static IWebHost BuildWebHost(string[] args)
    {
        return new WebHostBuilder()
            ...
            .UseCloudFoundryHosting()
            ...
            .ConfigureAppConfiguration((builderContext, configBuilder) =>
            {
                var env = builderContext.HostingEnvironment;
                configBuilder.SetBasePath(env.ContentRootPath)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                    .AddEnvironmentVariables()
                    // Add to configuration the Cloudfoundry VCAP settings
                    .AddCloudFoundry();
            })
            .Build();
    }
    ...
```

Both sources are then added to the configuration builder.

When pushing the application to Cloud Foundry, the settings from service bindings merge with the settings from other configuration mechanisms (such as `appsettings.json`).

If there are merge conflicts, the last provider added to the Configuration take precedence and overrides all others.

To manage application settings centrally instead of with individual files, use [Steeltoe Configuration](/docs/steeltoe-configuration) and a tool such as [Spring Cloud Config Server](https://github.com/spring-cloud/spring-cloud-config).

>NOTE: If you use the Spring Cloud Config Server, `AddConfigServer()` automatically calls `AddCloudFoundry()` for you.