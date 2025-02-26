# Common Steps

This section outlines how to work with the Steeltoe sample applications:

* [Publish Sample](#publish-sample)
* [Push Sample to Cloud Foundry](#cloud-foundry-push-sample)

## Publish Sample

This section describes how to publish a sample on Windows, Linux, or macOS.

### ASP.NET Core

You can use the `dotnet` CLI to [build and locally publish](https://learn.microsoft.com/dotnet/core/tools/dotnet-publish) the application for the target framework and runtime to which you want to deploy the application:

* Windows: `dotnet publish --framework net8.0 --runtime win-x64`
* Linux: `dotnet publish --framework net8.0 --runtime linux-x64`
* macOS: `dotnet publish --framework net8.0 --runtime osx-x64`

> [!TIP]
> Since .NET Core 2.0, the `dotnet publish` command automatically runs NuGet package restore for you. Running `dotnet restore` explicitly is no longer required.

## Cloud Foundry Push Sample

This section describes how to use the [Cloud Foundry CLI](https://docs.cloudfoundry.org/cf-cli/install-go-cli.html) to push the published application to Cloud Foundry by using the parameters that match what you selected for framework and runtime:

```bash
# Push to Linux cell
cf push -f manifest.yml -p bin/Debug/net8.0/linux-x64/publish

# Push to Windows cell, .NET Core
cf push -f manifest-windows.yml -p bin/Debug/net8.0/win-x64/publish
```

> [!NOTE]
> All sample manifests have been defined to bind their application to the services as created earlier.

### Observe the Logs

To see the logs as you start the application, use `cf logs your-app-name`.

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

On Windows cells, you should see something slightly different with similar information.

### Reading Configuration Values

When pushing the application to Cloud Foundry, the settings from service bindings merge with the configuration settings from other configuration mechanisms (such as `appsettings.json`).

If there are merge conflicts, the last provider added to the configuration takes precedence and overrides all others.

To manage application settings centrally instead of with individual files, you can use [Steeltoe Configuration](../configuration/index.md) and a tool such as [Spring Cloud Config Server](https://github.com/spring-cloud/spring-cloud-config).
