# Common Steps

This section outlines how to work with two sample applications:

* <a href="#steeltoe-common-steps-publish-sample">Publish Sample</a>
* <a href="#steeltoe-common-steps-cloud-foundry-push-sample">Cloud Foundry Push Sample</a>

<a name="steeltoe-common-steps-publish-sample"></a>
## Publish Sample

This section describes how to deploy the publish sample on either Linux or Windows.

### ASP.NET Core

You can use the `dotnet` CLI to [build and locally publish](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-publish) the application for the framework and runtime to which you want to deploy the application:

* Linux with .NET Core: `dotnet publish -f netcoreapp3.1 -r ubuntu.14.04-x64`
* Windows with .NET Core: `dotnet publish -f netcoreapp3.1 -r win10-x64`

>NOTE: Starting with .NET Core 2.0, the `dotnet publish` command automatically restores dependencies for you. Running `dotnet restore` explicitly is not generally required.

<a name="steeltoe-common-steps-cloud-foundry-push-sample"></a>
## Cloud Foundry Push Sample

This section describes how to use the Cloud Foundry CLI to push the published application to Cloud Foundry by using the parameters that match what you selected for framework and runtime:

```bash
# Push to Linux cell
cf push -f manifest.yml -p bin/Debug/netcoreapp3.1/ubuntu.14.04-x64/publish

# Push to Windows cell, .NET Core
cf push -f manifest-windows.yml -p bin/Debug/netcoreapp3.1/win10-x64/publish
```

>NOTE: All sample manifests have been defined to bind their application to the services as created earlier.

### Observe the Logs

To see the logs as you startup the application, use `cf logs oauth`.

On a Linux cell, you should see output that resembles the following during startup:

```bash
2016-06-01T09:14:14.38-0600 [CELL/0]     OUT Creating container
2016-06-01T09:14:15.93-0600 [CELL/0]     OUT Successfully created container
2016-06-01T09:14:17.14-0600 [CELL/0]     OUT Starting health monitoring of container
2016-06-01T09:14:21.04-0600 [APP/0]      OUT Hosting environment: Development
2016-06-01T09:14:21.04-0600 [APP/0]      OUT Content root path: /home/vcap/app
2016-06-01T09:14:21.04-0600 [APP/0]      OUT Now listening on: http://*:8080
2016-06-01T09:14:21.04-0600 [APP/0]      OUT Application started. Press Ctrl+C to shut down.
2016-06-01T09:14:21.41-0600 [CELL/0]     OUT Container became healthy
```

On Windows cells, you should see something slightly different output with the same information.

### Reading Configuration Values

Once the connector's settings have been defined, the next step is to read them so that they can be made available to the connector.

The code in the next example reads connector settings from the `appsettings.json` file with the .NET JSON configuration provider (`AddJsonFile("appsettings.json"))` and from `VCAP_SERVICES` with `AddCloudFoundry()`. Both sources are then added to the configuration builder. The following code shows how to read from both sources:

```csharp
public class Program {
    ...
    public static IWebHost BuildWebHost(string[] args)
    {
        return new WebHostBuilder()
            ...
            .UseCloudHosting()
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
}
```

When pushing the application to Cloud Foundry, the settings from service bindings merge with the settings from other configuration mechanisms (such as `appsettings.json`).

If there are merge conflicts, the last provider added to the configuration takes precedence and overrides all others.

To manage application settings centrally instead of with individual files, you can use [Steeltoe Configuration](/docs/configuration) and a tool such as [Spring Cloud Config Server](https://github.com/spring-cloud/spring-cloud-config)

>NOTE: If you use the Spring Cloud Config Server, `AddConfigServer()` automatically calls `AddCloudFoundry()` for you.
