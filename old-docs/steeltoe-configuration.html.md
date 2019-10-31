---
title: Configuration
order: 20
date: 2016/5/1
tags:
---

Steeltoe Configuration builds on the new .NET configuration API, which enables developers to configure an application with values from a variety of sources by using Configuration Providers. Each provider supports reading a set of name-value pairs from a given source location and adding them into a combined multi-level configuration dictionary.

Each value contained in the configuration is tied to a string-typed key or name. The values are organized by key into a hierarchical list of name-value pairs in which the components of the keys are separated by a colon (for example, `spring:application:key = value`).

.NET supports the following providers/sources:

* Command-line arguments
* File sources (such as JSON, XML, and INI)
* Environment variables
* Custom providers

To better understand .NET configuration services, you should read the [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration) documentation. Note that, while the documentation link suggests this service is tied to ASP.NET Core, it is not. It can be used in many different application types, including Console, ASP.NET 4.x., UWP, and others.

Steeltoe adds four additional configuration providers to the preceding list:

* Cloud Foundry
* Spring Cloud Config Server
* Placeholder resolvers
* RandomValue generator

The following sections provide more more detail on each of these new providers.

>NOTE: Depending on your hosting environment, service instances you create for the purpose of exploring the Quick Starts on this page may have a cost associated.

# 0.0 Initialize Dev Environment

All of the Steeltoe sample applications are in the same repository. If you have not already done so, use git to clone the [Steeltoe Samples](https://github.com/SteeltoeOSS/Samples) repository or download it with your browser from GitHub. The following git command shows how to clone the repository from the command line:

```bash
git clone https://github.com/SteeltoeOSS/Samples.git
```

>NOTE: All Configuration samples in the Samples repository have a base path of `Samples/Configuration/src/`.

Make sure your Cloud Foundry CLI tools are logged in and targeting the correct org and space, as follows:

```bash
cf login [-a API_URL] [-u USERNAME] [-p PASSWORD] [-o ORG] [-s SPACE] [--skip-ssl-validation]
```

or

```bash
cf target -o <YourOrg> -s <YourSpace>
```

The Configuration sample requires a Config server. If you intend to run the samples locally, install the Java 8 JDK and Maven 3.x now.

# 1.0 Cloud Foundry Provider

<span style="display:inline-block;margin:0 20px;">For use with </span><span style="display:inline-block;vertical-align:top;width:40%"> ![alt text](/images/CFF_Logo_rgb.png "Cloud Foundry")</span>

The Cloud Foundry provider enables the standard Cloud Foundry environment variables (`VCAP_APPLICATION`,  `VCAP_SERVICES`, and `CF_*`) to be parsed and accessed as configuration data within a .NET application.

Cloud Foundry creates and uses these environment variables to communicate an application's environment and configuration to the application code running inside a container. More specifically, the values found in `VCAP_APPLICATION` provide information about the application's resource limits, routes (URIs), and version number, among other things. The `VCAP_SERVICES` environment variable provides information about the external services (Databases, Caches, and so on) to which the application is bound, along with details on how to contact those services.

You can read more information on the Cloud Foundry environment variables at the [Cloud Foundry docs](https://docs.cloudfoundry.org/devguide/deploy-apps/environment-variable.html) website.

The Steeltoe Cloud Foundry provider supports the following .NET application types:

* ASP.NET (MVC, WebForms, WebAPI, WCF)
* ASP.NET Core
* Console apps (.NET Framework and .NET Core)

 The source code for this provider can be found [here](https://github.com/SteeltoeOSS/Configuration).

## 1.1 Quick Start

This quick start shows how to use the Cloud Foundry configuration provider in an ASP.NET Core MVC application on Cloud Foundry.

You need access to a Cloud Foundry runtime environment in order to complete the quick start.

### 1.1.1 Locate Sample

First, you must navigate to the correct directory, as follows:

```bash
cd Samples/Configuration/src/AspDotNetCore/CloudFoundry
```

### 1.1.2 Publish Sample

See [Publish Sample](#publish-sample) for how to publish this sample to either Linux or Windows.

### 1.1.3 Push Sample

See [Push Sample](#push-sample) for how to push this sample to either Linux or Windows on Cloud Foundry.

### 1.1.4 Observe Logs

To monitor the logs as you start the application, use the following command: `cf logs cloud`.

On a Linux cell, you should see resembling the following during startup:

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

On a Windows cell, you should see something slightly different but with the same information.

### 1.1.5 What to Expect

Use the menu at the top of the sample application to see the various outputs produced by the provider. Specifically, click on the `CloudFoundry Settings` menu item. You should see `VCAP_APPLICATION` and `VCAP_SERVICES` configuration data for the app.

Because you have not bound any Cloud Foundry services to the app, there is no `VCAP_SERVICES` information.

To see service binding information, bind any service to the application and then restart it. To do so, follow the instructions on the [Cloud Foundry documentation](https://docs.cloudfoundry.org/devguide/services/application-binding.html) site.

### 1.1.6 Understand Sample

The `CloudFoundry` quick start sample was created by using the .NET Core tooling `mvc` template (`dotnet new mvc`) and then modified to include the Steeltoe framework.

To gain an understanding of the Steeltoe related changes to the generated template code, examine the following files:

* `CloudFoundry.csproj`: Contains the `PackageReference` for Steeltoe NuGet `Steeltoe.Extensions.Configuration.CloudFoundryCore`
* `Program.cs`:  Code was added to the `ConfigurationBuilder` to pick up Cloud Foundry configuration values when pushed to Cloud Foundry and to use Cloud Foundry hosting.
* `Startup.cs`: Code was added to the `ConfigureCloudFoundryOptions`.
* `HomeController.cs`: Code was added for Options injection into the Controller. Code was also added to display the Cloud Foundry configuration data.
* `CloudFoundryViewModel.cs`: Used to communicate config values to `CloudFoundry.cshtml`.
* `CloudFoundry.cshtml`: The view used to display Cloud Foundry configuration values.

## 1.2 Usage

The following sections describe how to use the Cloud Foundry configuration provider:

* [Add NuGet Reference](#1-2-1-add-nuget-reference)
* [Add Configuration Provider](#1-2-2-add-configuration-provider)
* [Access Configuration Data](#1-2-3-access-configuration-data)
* [Access Configuration Data as Options](#1-2-4-access-configuration-data-as-options)

You should have a good understanding of how the .NET [Configuration services](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration) work before starting to use this provider.

In order to use the Steeltoe Cloud Foundry provider you need to do the following:

1. Add a NuGet package reference to your project.
1. Add the provider to the Configuration Builder.
1. Configure Cloud Foundry options classes by binding configuration data to the classes.
1. Inject and use the Cloud Foundry Options to access Cloud Foundry configuration data.

>NOTE: Most of the example code in the following sections is based on using Steeltoe in an ASP.NET Core application. If you are developing an ASP.NET 4.x application or a Console based app, see the [other samples](https://github.com/SteeltoeOSS/Samples/tree/master/Configuration) for example code you can use.

### 1.2.1 Add NuGet Reference

To use the provider, you need to add a reference to the appropriate Steeltoe Cloud Foundry NuGet based on the type of the application you are building and what Dependency Injector you have chosen, if any. The following table describes the available packages:

|App Type|Package|Description|
|---|---|---|
|Console/ASP.NET 4.x|`Steeltoe.Extensions.Configuration.CloudFoundryBase`|Base functionality. No dependency injection.|
|ASP.NET Core|`Steeltoe.Extensions.Configuration.CloudFoundryCore`|Includes base. Adds ASP.NET Core dependency injection.|
|ASP.NET 4.x with Autofac|`Steeltoe.Extensions.Configuration.CloudFoundryAutofac`|Includes base. Adds Autofac dependency injection.|

To add this type of NuGet to your project, add a `PackageReference` resembling the following:

```xml
<ItemGroup>
...
    <PackageReference Include="Steeltoe.Extensions.Configuration.CloudFoundryCore" Version= "2.1.0"/>
...
</ItemGroup>
```

### 1.2.2 Add Configuration Provider

In order to parse the Cloud Foundry environment variables and make them available in the application's configuration, you need to add the Cloud Foundry configuration provider to the `ConfigurationBuilder`.

The following example shows how to do so:

```csharp
using Steeltoe.Extensions.Configuration;
...

var builder = new ConfigurationBuilder()
    .SetBasePath(env.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)

    // Add VCAP_* configuration data
    .AddCloudFoundry();
Configuration = builder.Build();
...

```

When developing an ASP.NET Core application, you can do the same thing by using the `AddCloudFoundry()` extension method on the `IWebHostBuilder`. The following example shows how to do so:

```csharp
public class Program
{
    public static void Main(string[] args)
    {
        BuildWebHost(args).Run();
    }
    public static IWebHost BuildWebHost(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            .UseCloudFoundryHosting()

            // Add VCAP_* configuration data
            .AddCloudFoundry()
            .UseStartup<Startup>()
            .Build();
}
```

### 1.2.3 Access Configuration Data

Once the configuration has been built, the values from the `VCAP_APPLICATION` and `VCAP_SERVICES` environment variables have been added to the application's configuration data and become available under keys prefixed with `vcap:application` and `vcap:services` respectively.

You can access the values from the `VCAP_APPLICATION` environment variable settings directly from the configuration as follows:

```csharp
var config = builder.Build();
var appName = config["vcap:application:application_name"]
var instanceId = config["vcap:application:instance_id"]
...
```

A list of all `VCAP_APPLICATION` keys is available in the [VCAP_APPLICATION](https://docs.CloudFoundry.org/devguide/deploy-apps/environment-variable.html#VCAP-APPLICATION) topic of the Cloud Foundry documentation.

You can also directly access the values from the `VCAP_SERVICES` environment variable. For example, to access the information about the first instance of a bound Cloud Foundry service with a name of `service-name`, you could code the following:

```csharp
var config = builder.Build();
var name = config["vcap:services:service-name:0:name"]
var uri = config["vcap:services:service-name:0:credentials:uri"]
...
```

A list of all `VCAP_SERVICES` keys is available in the [VCAP_SERVICES](https://docs.CloudFoundry.org/devguide/deploy-apps/environment-variable.html#VCAP-SERVICES) topic of the Cloud Foundry documentation.

>NOTE: This provider uses the built-in .NET [JSON Configuration Parser](https://github.com/aspnet/Configuration/tree/dev/src/Microsoft.Extensions.Configuration.Json) when parsing the JSON provided in the `VCAP_*` environment variables. As a result, you can expect the exact same key names and behavior as you see when parsing JSON configuration files (such as `appsettings.json`) in your application.

### 1.2.4 Access Configuration Data as Options

#### 1.2.4.1 ConfigureCloudFoundryOptions()

Alternatively, instead of accessing the Cloud Foundry configuration data directly from the configuration, you can use the .NET [Options](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration) framework together with [Dependency Injection](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection).

The Cloud Foundry provider includes two additional classes, `CloudFoundryApplicationOptions` and `CloudFoundryServicesOptions`. Both can be configured through the Options framework to hold the parsed `VCAP_*` data by using the Options `Configure()` feature.

To use it in an ASP.NET Core application, add the the following to the `ConfigureServices()` method in the `Startup` class:

```csharp
using Steeltoe.Extensions.Configuration.CloudFoundry;

public void ConfigureServices(IServiceCollection services)
{
    // Setup Options framework with DI
    services.AddOptions();

    // Add Steeltoe Cloud Foundry Options to service container
    services.ConfigureCloudFoundryOptions(Configuration);
}
```

The `ConfigureCloudFoundryOptions(Configuration)` method call uses the Options framework to bind the `vcap:application` configuration values to an instance of `CloudFoundryApplicationOptions` and binds the `vcap:services` values to an instance of `CloudFoundryServicesOptions`.

Both of these method calls also add these objects to the service container as `IOptions`.

Once this is done, you can access these configuration objects in the Controllers or Views of an application by using normal Dependency Injection.

The following example controller shows how to do so:

```csharp
using Steeltoe.Extensions.Configuration.CloudFoundry;

public class HomeController : Controller
{
    public HomeController(IOptions<CloudFoundryApplicationOptions> appOptions,
                            IOptions<CloudFoundryServicesOptions> serviceOptions)
    {
        AppOptions = appOptions.Value;
        ServiceOptions = serviceOptions.Value;
    }

    CloudFoundryApplicationOptions AppOptions { get; private set; }
    CloudFoundryServicesOptions ServiceOptions { get; private set; }

    // GET: /<controller>/
    public IActionResult Index()
    {
        ViewData["AppName"] = AppOptions.ApplicationName;
        ViewData["AppId"] = AppOptions.ApplicationId;
        ViewData["URI-0"] = AppOptions.ApplicationUris[0];

        ViewData[ServiceOptions.ServicesList[0].Label] = ServiceOptions.ServicesList[0].Name;
        ViewData["client_id"]= ServiceOptions.ServicesList[0].Credentials["client_id"].Value;
        ViewData["client_secret"]= ServiceOptions.ServicesList[0].Credentials["client_secret"].Value;
        ViewData["uri"]= ServiceOptions.ServicesList[0].Credentials["uri"].Value;
        return View();
    }
}
```

#### 1.2.4.2 ConfigureCloudFoundryService

As an alternative to using `CloudFoundryServicesOptions` to access Cloud Foundry service data you can also use `ConfigureCloudFoundryService<TOption>()` or `ConfigureCloudFoundryServices<TOption>()` to easily gain access to service data.  

These methods allow you to define an Option class which represents a particular type of Cloud Foundry service binding and then use either method to select that data from `VCAP_SERVICES` and bind the data to it.

To do this, you first need to create a Options class that derives from `AbstractServiceOptions`. That class must match the data provided in `VCAP_SERVICES`.  

Here is an example that illustrates how to do this for a MySql service binding on PCF:

```csharp
using Steeltoe.Extensions.Configuration.CloudFoundry;

public class MySqlServiceOption : AbstractServiceOptions
{
    public MySqlServiceOption() { }
    public MySqlCredentials Credentials { get; set; }
}

public class MySqlCredentials
{
    public string Hostname { get; set; }
    public int Port { get; set; }
    public string Name { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string Uri { get; set; }
    public string JdbcUrl { get; set; }
}
```

Next in your `Startup` class you can use either `ConfigureCloudFoundryService<TOption>()` or `ConfigureCloudFoundryServices<TOption>()` to bind the service data from `VCAP_SERVICES` to your `TOption`.  There are multiple ways to do this depending on your needs.

You can use `ConfigureCloudFoundryService<TOption>()` method to select a specific Cloud Foundry service binding from `VCAP_SERVICES` by specifying a service name. Or you can use `ConfigureCloudFoundryServices<TOption>()` to bind to all services of a particular type by specifying a Cloud Foundry service label.  

Here are some examples:

```csharp
using Steeltoe.Extensions.Configuration.CloudFoundry;

public void ConfigureServices(IServiceCollection services)
{
    // Setup Options framework with DI
    services.AddOptions();

    // Bind VCAP_SERVICES data for mySql2 service instance to MySqlServiceOption
    services.ConfigureCloudFoundryService<MySqlServiceOption>(Configuration, "mySql2");

    // Bind VCAP_SERVICES data for all p-mysql service instances to MySqlServiceOption
    services.ConfigureCloudFoundryServices<MySqlServiceOption>(Configuration, "p-mysql");
}
```

As you can see all of this is built using the Microsoft provided Options framework.  As a result we are able to leverage the `named` Options feature Microsoft has implemented in options binding, and configure each `TOption` with a name equal to the Cloud Foundry service name found in `VCAP_SERVICES`.

What this means is within a controller you can inject the `IOptionsSnapshot<MySqlServiceOption>` or `IOptionsMonitor<MySqlServiceOption>` as you normally would and then access the Option by name. (for example: specific Cloud Foundry service binding instance).

Here is an example:

```csharp
    public class HomeController : Controller
    {
        private IOptionsSnapshot<MySqlServiceOption> _mySqlOptions;
        private  MySqlServiceOption MySqlOptions
        {
            get
            {
                return _mySqlOptions.Get("mySql2");
            }
        }

        public HomeController(IOptionsSnapshot<MySqlServiceOption> mySqlOptions)
        {
            _mySqlOptions = mySqlOptions;
        }
```

# 2.0 Config Server Provider

This provider enables the Spring Cloud Config Server to be used as a source of configuration data for a .NET application.

The Spring Cloud Config Server is an application configuration service that gives you a central place to manage an application's configuration values externally across all environments. As an application moves through the deployment pipeline from development to test and into production, you can use the config server to manage the configuration between environments and be certain that the application has everything it needs to run when you migrate it. The config server easily supports labelled versions of environment-specific configurations and is accessible to a wide range of tooling for managing its content.

To gain a better understanding of the Spring Cloud Config Server, you should read the [Spring Cloud](https://projects.spring.io/spring-cloud/) documentation.

The Steeltoe Config Server provider supports the following .NET application types:

* ASP.NET (MVC, WebForm, WebAPI, WCF)
* ASP.NET Core
* Console apps (.NET Framework and .NET Core)

In addition to the Quick Start below, there are several other Steeltoe sample applications that you can refer to when needing help in understanding how to use this provider:

* [AspDotNetCore/Simple](https://github.com/SteeltoeOSS/Samples/tree/master/Configuration/src/AspDotNetCore/Simple): ASP.NET Core sample app showing how to use the open source Config Server.
* [AspDotNet4/Simple](https://github.com/SteeltoeOSS/Samples/tree/master/Configuration/src/AspDotNet4/Simple): Same as AspDotNetCore/Simple but built for ASP.NET 4.x
* [AspDotNet4/SimpleCloudFoundry](https://github.com/SteeltoeOSS/Samples/tree/master/Configuration/src/AspDotNet4/SimpleCloudFoundry): Same as the Quick Start sample mentioned later but built for ASP.NET 4.x.
* [AspDotNet4/AutofacCloudFoundry](https://github.com/SteeltoeOSS/Samples/tree/master/Configuration/src/AspDotNet4/AutofacCloudFoundry): Same as AspDotNet4/SimpleCloudFoundry but built using the Autofac IOC container.
* [MusicStore](https://github.com/SteeltoeOSS/Samples/tree/master/MusicStore): A sample application showing how to use all of the Steeltoe components together in a ASP.NET Core application. This is a micro-services based application built from the ASP.NET Core MusicStore reference app provided by Microsoft.
* [FreddysBBQ](https://github.com/SteeltoeOSS/Samples/tree/master/FreddysBBQ): A polyglot microservices-based sample app showing inter-operability between Java and .NET on Cloud Foundry. It is secured with OAuth2 Security Services and using Spring Cloud Services.

The source code for this provider can be found [here](https://github.com/SteeltoeOSS/Configuration).

>IMPORTANT: The `Pivotal.Extensions.Configuration.ConfigServer*` packages have been deprecated in Steeltoe 2.2 and will be removed in a future release.  All functionality provided in those packages has been pushed into the corresponding `Steeltoe.Extensions.Configuration.ConfigServer*` packages.

## 2.1 Quick Start

This quick start uses an ASP.NET Core application to show how to use the Steeltoe Config Server provider to fetch configuration data from a Config Server running locally on your development machine and also how to take that same application and push it to Cloud Foundry and use a config server operating there.

### 2.1.1  Running Locally

The following sections describe how to use the Spring Cloud Config Server configuration provider when running locally:

* [Start Config Server](#2-1-1-1-start-config-server)
* [Locate Sample](#2-1-1-2-locate-sample)
* [Run Sample](#2-1-1-3-run-sample)
* [Observe Logs](#2-1-1-4-observe-logs)
* [View Results](#2-1-1-5-view-results)

#### 2.1.1.1 Start Config Server

In this step, we fetch a GitHub repository from which we can start up a Spring Cloud Config Server locally on the desktop. This particular server has been pre-configured to fetch its configuration data from <https://github.com/steeltoeoss/config-repo>.

You can use this same GitHub repository for your own future development work. If you do so, at some point, you will want to change the location from which the server fetches its configuration data. To do so, you must modify `configserver/src/main/resources/application.yml` to point to a new GitHub repository. Once that is done, you need to run `mvnw clean` followed by `mvnw spring-boot:run` to make sure your server picks up the changes, as shown in the following example:

```bash
git clone https://github.com/SteeltoeOSS/configserver.git
cd configserver
mvnw spring-boot:run
```

#### 2.1.1.2 Locate Sample

To find the sample, change the current directory as follows:

```bash
cd Samples/Configuration/src/AspDotNetCore/SimpleCloudFoundry
```

#### 2.1.1.3 Run Sample

To run the application on .NET Core on Windows, Linux or OSX, use the following command:

```bash
dotnet run -f netcoreapp2.1
```

To run the application on .NET Framework on Windows, use the following command:

```bash
dotnet run -f net461
```

#### 2.1.1.4 Observe Logs

The `dotnet run` command should produce output similar to the following:

```bash
Hosting environment: Production
Now listening on: http://localhost:5000
Application started. Press Ctrl+C to shut down.
```

#### 2.1.1.5 View Results

Start a browser and visit <http://localhost:5000>. Use the menu presented by the app to see various output, as follows:

* `Config Server Settings`: Shows the settings used by the Steeltoe client when communicating to the config server. These come from settings in `appsettings.json`.
* `Config Server Data`: Shows the configuration data returned from the config server's github repository. It includes some of the data from `sample.properties`, `sample-Production.properties`, and `application.yml` if they are found in the GitHub repository: <https://github.com/steeltoeoss/config-repo>.
* `Reload`: Triggers a reload of the configuration data from the server.

Change the Hosting environment variable setting to `development` (by using `export ASPNETCORE_ENVIRONMENT=development` or `SET ASPNETCORE_ENVIRONMENT=development`) and then restart the application.

You should see different configuration data returned for that profile/hosting environment. This time, it contains some of the data from `sample.properties`, `sample-development.properties`, and `application.yml`, if they are found in the GitHub repository: <https://github.com/steeltoeoss/config-repo>.

### 2.1.2 Running on Cloud Foundry

The following sections describe how to use the Spring Cloud Config Server configuration provider on Cloud Foundry:

* [Start Config Server](#2-1-2-1-start-config-server)
* [Publish Sample](#2-1-2-2-publish-sample)
* [Push Sample](#2-1-2-3-push-sample)
* [Observe Logs](#2-1-2-4-observe-logs)
* [View Results](#2-1-2-5-view-results)

#### 2.1.2.1 Start Config Server

In this step, we use the Cloud Foundry CLI to create a service instance of the Spring Cloud Config Server on Cloud Foundry. In the `config-server.json` file, you can see that we have set the config server's github repository to `https://github.com/spring-cloud-samples/config-repo`. To do so, run the following commands:

```bash
# Make sure you are in the samples directory
cd Samples/Configuration/src/AspDotNetCore/SimpleCloudFoundry

# Create a Config Server instance on Cloud Foundry, using config-server.json settings
cf create-service p-config-server standard myConfigServer -c ./config-server.json

# Wait for the service to become ready
cf services
```

These commands create a Spring Cloud Config Server instance on Cloud Foundry named `myConfigServer` configured from the contents of the file `config-server.json`.

#### 2.1.2.2 Publish Sample

See [Publish Sample](#publish-sample) for instructions on how to publish this sample to either Linux or Windows.

#### 2.1.2.3 Push Sample

See [Push Sample](#push-sample) for instructions on how to push this sample to either Linux or Windows on Cloud Foundry.

#### 2.1.2.4 Observe Logs

To see the logs as you start the application, use `cf logs foo`.

On a Linux cell, you should see something resembling the following during startup:

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

On Windows cells, you should see something slightly different but with the same content.

#### 2.1.2.5 View Results

The `cf push` command creates an application by the name of `foo` and binds the `myConfigServer` service instance to the application. You can see the application at `https://sample.x.y.z/`.

Use the menu provided by the app to see various output related to Cloud Foundry and the config server:

* `CloudFoundry Settings`: Should show `VCAP_APPLICATION` and `VCAP_SERVICES` settings read as configuration data.
* `Config Server Settings`: Should show the settings used by the Steeltoe client when communicating to the Config Server. These have been picked up from the service binding.
* `Config Server Data`: This is the configuration data returned from the Config Server's configured GitHub repository. It includes some of the data from `foo.properties`, `foo-development.properties` and `application.yml` found in the GitHub repository: (<https://github.com/spring-cloud-samples/config-repo>).
* `Reload`: Triggers a reload of the configuration data from the Config Server.

Change the Hosting environment setting to `production` (by using `export ASPNETCORE_ENVIRONMENT=production` or  `SET ASPNETCORE_ENVIRONMENT=production`) and then re-push the application. You should see different configuration data returned for that profile/hosting environment.

### 2.1.3 Understand Sample

The `SimpleCloudFoundry` sample was created from the .NET Core tooling `mvc` template (for example, `dotnet new mvc`) and then modified to add the Steeltoe framework.

To gain an understanding of the Steeltoe related changes to generated template code, examine the following files:

* `SimpleCloudFoundry.csproj` - Contains `PackageReference` for Steeltoe NuGet `Pivotal.Extensions.Configuration.ConfigServer`
* `Program.cs` - Code was added to the `ConfigurationBuilder` in order to add Config Server configuration values to the configuration and to use Cloud Foundry hosting.
* `appsettings.json` - Contains configuration data needed for the Steeltoe Config Server provider.
* `ConfigServerData.cs` - Object used to hold the data retrieved from the config server
* `Startup.cs` - Code added to configure the `ConfigServerData` Options added to the service container.
* `HomeController.cs` - Code added for `ConfigServerData` Options injected into the controller and ultimately used to display the data returned from config server.
* `ConfigServer.cshtml` - The view used to display the data returned from the config server.

## 2.2 Usage

The following sections describe how to use the config server configuration provider.

* [Add NuGet Reference](#2-2-1-add-nuget-reference)
* [Configure Settings](#2-2-2-configure-settings)
* [Add Configuration Provider](#2-2-3-add-configuration-provider)
* [Bind to Cloud Foundry](#2-2-4-bind-to-cloud-foundry)
* [Access Configuration Data](#2-2-5-access-configuration-data)
* [Enable Logging](#2-2-6-enable-logging)
* [Configuring Discovery First](#2-2-7-configuring-discovery-first)
* [Configuring Health Contributor](#2-2-8-configuring-health-contributor)
* [Configuring Fail Fast](#2-2-9-configuring-fail-fast)
* [Configuring Retry](#2-2-10-configuring-retry)
* [Configuring Multiple Urls](#2-2-11-configuring-multiple-urls)

You should know how the new .NET [Configuration services](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration) work before starting to use this provider. A basic understanding of the `ConfigurationBuilder` and how to add providers to the builder is necessary.

You should also have a good understanding of the [Spring Cloud Config Server](https://cloud.spring.io/spring-cloud-config/).

To use the Steeltoe provider, you need to do the following:

1. Add the appropriate NuGet package reference to your project.
1. Configure the settings that the Steeltoe provider uses to access the Spring Cloud Config Server.
1. Add the provider to the configuration builder.
1. Optionally, configure the returned config server config data as Options.
1. Inject and use Options or ConfigurationRoot to access configuration data.

### 2.2.1 Add NuGet Reference

You can choose from two Config Server client NuGets, depending on your needs.

If you plan on only connecting to the open source version of [Spring Cloud Config Server](https://projects.spring.io/spring-cloud/), then you should use one of the packages described by the following table, depending on your application type and needs:

|App Type|Package|Description|
|---|---|---|
|Console/ASP.NET 4.x|`Steeltoe.Extensions.Configuration.ConfigServerBase`|Base functionality. No dependency injection.|
|ASP.NET Core|`Steeltoe.Extensions.Configuration.ConfigServerCore`|Includes base. Adds ASP.NET Core dependency injection.|
|ASP.NET 4.x with Autofac|`Steeltoe.Extensions.Configuration.ConfigServerAutofac`|Includes base. Adds Autofac dependency injection.|

To add this type of NuGet to your project, add a `PackageReference` that resembles the following:

```xml
<ItemGroup>
...
    <PackageReference Include="Steeltoe.Extensions.Configuration.ConfigServerCore" Version= "2.1.0"/>
...
</ItemGroup>
```

If you plan to connect to the open source version of [Spring Cloud Config Server](https://projects.spring.io/spring-cloud/) and you plan to push your application to Cloud Foundry to use [Spring Cloud Services](https://docs.pivotal.io/spring-cloud-services/1-5/common/index.html), you should use one of the packages described in the following table, depending on your application type and needs:

|App Type|Package|Description|
|---|---|---|
|Console/ASP.NET 4.x|`Pivotal.Extensions.Configuration.ConfigServerBase`|Base functionality. No dependency injection.|
|ASP.NET Core|`Pivotal.Extensions.Configuration.ConfigServerCore`|Includes base. Adds ASP.NET Core dependency injection.|
|ASP.NET 4.x with Autofac|`Pivotal.Extensions.Configuration.ConfigServerAutofac`|Includes base. Adds Autofac dependency injection.|

To add this type of NuGet to your project add a `PackageReference` similar to the following:

```xml
<ItemGroup>
...
    <PackageReference Include="Pivotal.Extensions.Configuration.ConfigServerCore" Version= "2.1.0"/>
...
</ItemGroup>
```

>IMPORTANT: The `Pivotal.Extensions.Configuration.ConfigServer*` packages have been deprecated in Steeltoe 2.2 and will be removed in a future release.  All functionality provided in those packages has been pushed into the corresponding `Steeltoe.Extensions.Configuration.ConfigServer*` packages.

### 2.2.2 Configure Settings

The most convenient way to configure settings for the provider is to put them in a file and then use one of the other file-based configuration providers to read them.

The following example shows some provider settings put in a JSON file. Only two settings are really necessary. `spring:application:name` configures the "application name" to be `sample`, and `spring:cloud:config:uri` the address of the config server.

>NOTE: The `spring:application:name` is also used by other Steeltoe libraries in addition to the config server.

```json
{
  "spring": {
    "application": {
      "name": "sample"
    },
    "cloud": {
      "config": {
        "uri": "http://localhost:8888"
      }
    }
  }
  ...
}
```

The following table describes all the settings that can be used to configure the behavior of the provider:

|Key|Description|Default|
|---|---|---|
|name|App name for which to request config|`IHostingEnvironment.ApplicationName`|
|enabled|Enable or disable config server client|true|
|uri|Comma-separated list of config server endpoints|`http://localhost:8888`|
|env|Environment or profile used in the server request|`IHostingEnvironment.EnvironmentName`|
|validateCertificates|Enable or disable certificate validation|true|
|label|Comma-separated list of labels to request|master|
|timeout|Time to wait for response from server, in milliseconds|6000|
|username|Username for basic authentication|none|
|password|Password for basic authentication|none|
|failFast|Enable or disable failure at startup|false|
|token|Hashicorp Vault authentication token|none|
|tokenTtl|Hashicorp Vault token renewal TTL. Valid on Cloud Foundry only|300000ms|
|tokenRenewRate|Hashicorp Vault token renewal rate. Valid on Cloud Foundry only|60000ms|
|retry:enabled|Enable or disable retry logic|false|
|retry:maxAttempts|Max retries if retry enabled|6|
|retry:initialInterval|Starting interval|1000ms|
|retry:maxInterval|Maximum retry interval|2000ms|
|retry:multiplier|Retry interval multiplier|1.1|
|clientId|OAuth2 client id when using OAuth security|none|
|clientSecret|OAuth2 client secret when using OAuth security|none|
|accessTokenUri|Uri to use to obtain OAuth access token|none|
|discovery:enabled|Enable or disable discovery first feature|false|
|discovery:serviceId|Config Server service id to use in discovery first feature|configserver|
|health:enabled|Enable or disable config server health check contributor|true|
|health:timeToLive|Health check contributor cache time to live in ms|60*5ms|

As mentioned earlier, all settings should start with `spring:cloud:config:`

>NOTE: If you use self-signed certificates on Cloud Foundry, you might run into certificate validation issues when pushing an application. A quick way to work around this is to disable certificate validation until a proper solution can be put in place.

### 2.2.3 Add Configuration Provider

Once the provider's settings have been defined and put in a file (such as a JSON file), the next step is to read them and make them available to the provider.

In the next C# example, the provider's configuration settings from the preceding example are put in the `appsettings.json` file included with the application. Then, by using the .NET JSON configuration provider, we can read the settings by adding the JSON provider to the configuration builder (`AddJsonFile("appsettings.json")`.

Then, after the JSON provider has been added, you can add the config server provider to the builder. We include an extension method, `AddConfigServer()`, that you can use to do so.

Because the JSON provider that reads `appsettings.json` has been added `before` the config server provider, the JSON-based settings become available to the Steeltoe provider. Note that you do not have to use JSON for the Steeltoe settings. You can use any of the other off-the-shelf configuration providers for the settings (such as INI files, environment variables, and so on).

You need to `Add*()` the source of the config server clients settings (`AddJsonFile(..)`) *before* you `AddConfigServer(..)`. Otherwise, the settings are not picked up and used.

The following sample shows how to add a configuration provider:

```csharp
using Steeltoe.Extensions.Configuration;
...

var builder = new ConfigurationBuilder()
    .SetBasePath(env.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddConfigServer(env)
    .AddEnvironmentVariables();

var config = builder.Build();
...
```

When developing an ASP.NET Core application, you can accomplish the same thing by using the `AddConfigServer()` extension method on the `IWebHostBuilder`. The following example shows how to do so:

```csharp
public class Program
{
    public static void Main(string[] args)
    {
        BuildWebHost(args).Run();
    }
    public static IWebHost BuildWebHost(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            .UseCloudFoundryHosting()

            // Use Config Server for configuration data
            .AddConfigServer()
            .UseStartup<Startup>()
            .Build();
}
```

### 2.2.4 Bind to Cloud Foundry

When you want to use a Config Server on Cloud Foundry and you have installed [Spring Cloud Services](https://docs.pivotal.io/spring-cloud-services/1-5/common/index.html), you can create and bind an instance of it to your application by using the Cloud Foundry CLI, as follows:

```bash
# Create config server instance named `myConfigServer`
cf create-service p-config-server standard myConfigServer

# Wait for service to become ready
cf services

# Bind the service to `myApp`
cf bind-service myApp myConfigServer

# Restage the app to pick up change
cf restage myApp
```

Once the service is bound to the application, the config server settings are available and can be setup in `VCAP_SERVICES`.

Then, when you push the application, the Steeltoe provider takes the settings from the service binding and merges those settings with the settings that you have provided through other configuration mechanisms (such as `appsettings.json`).

If there are any merge conflicts, the last provider added to the Configuration takes precedence and overrides all others.

### 2.2.5 Access Configuration Data

When the `ConfigurationBuilder` builds the configuration, the Config Server client makes the appropriate REST calls to the Config Server and retrieves the configuration values based on the settings that have been provided.

If there are any errors or problems accessing the server, the application continues to initialize, but the values from the server are not retrieved. If this is not the behavior you want, you should set the `spring:cloud:config:failFast` to `true`. Once that's done, the application fails to start if problems occur during the build.

After the configuration has been built, you can access the retrieved data directly by using `IConfiguration`. The following example shows how to do so:

```csharp
...
var config = builder.Build();
var property1 = config["myconfiguration:property1"]
var property2 = config["myconfiguration:property2"]
...
```

Alternatively, you can create a class to hold your configuration data and then use the [Options](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration) framework together with [Dependency Injection](https://docs.asp.net/en/latest/fundamentals/dependency-injection.html) to inject an instance of the class into your controllers and view.

To do so, first create a class representing the configuration data you expect to retrieve from the server, as shown in the following example:

```csharp
public class MyConfiguration {
    public string Property1 { get; set; }
    public string Property2 { get; set; }
}
```

Next, use the `Configure<>()` method to tell the Options framework to create an instance of that class with the returned data. For the preceding `MyConfiguration` class, you could add the following code to the `ConfigureServices()` method in the `Startup` class in an ASP.NET Core application, as shown in the following example:

```csharp
public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; set; }
    public void ConfigureServices(IServiceCollection services)
    {
        // Setup Options framework with DI
        services.AddOptions();

        // Configure IOptions<MyConfiguration>
        services.Configure<MyConfiguration>(Configuration.GetSection("myconfiguration"));
        ...
    }
}
```

The preceding `Configure<MyConfiguration>(Configuration.GetSection("myconfiguration"))` method call instructs the Options framework to bind the `myconfiguration:...` values to an instance of the `MyConfiguration` class.

After this has been done, you can gain access to the data in your `Controller` or `View` through dependency injection. The following example shows how to do so:

```csharp

public class HomeController : Controller
{
    public HomeController(IOptions<MyConfiguration> myOptions)
    {
        MyOptions = myOptions.Value;
    }

    MyConfiguration MyOptions { get; private set; }

    // GET: /<controller>/
    public IActionResult Index()
    {
        ViewData["property1"] = MyOptions.Property1;
        ViewData["property2"] = MyOptions.Property2;
        return View();
    }
}
```

### 2.2.6 Enable Logging

Sometimes, it is desirable to turn on debug logging in the provider.

To do so, you need to inject the `ILoggerFactory` into the `Startup` class constructor by adding it as an argument to the constructor. Once you have access to it, you can add a console logger to the factory and also set its minimum logging level set to Debug.

Once that is done, pass the `ILoggerFactory` to the Steeltoe configuration provider. The provider then uses it to establish a logger with the debug level logging turned on.

The following example shows how to enable Debug-level logging:

```csharp
using Steeltoe.Extensions.Configuration;

    LoggerFactory logFactory = new LoggerFactory();
    logFactory.AddConsole(minLevel: LogLevel.Debug);

    // Set up configuration sources.
    var builder = new ConfigurationBuilder()
        .SetBasePath(env.ContentRootPath)
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
        .AddEnvironmentVariables()
        .AddConfigServer(env, logFactory);
...
```

### 2.2.7 Configuring Discovery First

The default behavior for the Steeltoe Config Client is to access the Config Server through the `spring:cloud:config:uri` configuration setting. This of course requires that the application needs a `appsettings.json` or an environment variable with the Config Servers address set in `spring:cloud:config:uri`.  This mode of operation, the default, is called `Config First`.

Alternatively, you can setup your Config Server to register with a Discovery service such as Netflix Eureka. This enables you application to lookup the address of the Config Server using a Discovery Service instead of configuring it in `appsettings.json`.  Note that you have to specifically configure your Config Server deployment to register with an Discovery service as this doesn't happen automatically.  See the Spring Cloud Config Server documentation on how to do this.

However, with the default `Config First` mode of the Steeltoe client you are not able to take advantage of the Config Server registration unless you change the clients mode of operation to `Discovery First`. To do this follow these steps:

1. If your application is not using a Service Discovery service you need to configure your application to do so.  See the Steeltoe Discovery documentation for details on how to do this. Note that currently we only support Netflix Eureka. You will at a minimum need to configure the Eureka Server address.
1. Change the Steeltoe Config Server client setting `spring:cloud:config:discovery:enabled`; set it to be `true`; the default is `false`.
1. Optionally, if you change the service name registered by the Config Server with Eureka, you can use `spring:cloud:config:discovery:serviceId=YourNewName` to change the name used by the client for lookup.

Note that the price for using this mode of operation is an extra network round trip on startup to locate the Config Server service registration. The benefit is, as long as the Discovery Service is at fixed point, the Config Server can change its address and no changes are need to applications.

### 2.2.8 Configuring Health Contributor

The Config Server package provides a Steeltoe Management Health contributor that attempts to load configuration from the Config Server and contributes health information to the results of the Health endpoint.

If you use the `AddConfigServer()` extension method of the `IWebHostBuilder` the contributor is automatically added to the container and will automatically picked up an used. Otherwise you can manually add the contributor to the container using the `AddConfigServerHealthContributor()` extension method.

The contributor is enabled by default, but can be disabled by setting `spring:cloud:config:health:enabled=false`.

The response from the Config Server is cached for performance reasons. The default cache time to live is 5 minutes. To change that value, set the `spring:cloud:config:health:timeToLive=xxxx` setting (in milliseconds).

### 2.2.9 Configuring Fail Fast

In some cases, you may want to fail the startup of your application if it cannot connect to the Config Server. If this is the desired behavior, set the configuration setting `spring:cloud:config:failFast=true` to make the client halt with an Exception.

### 2.2.10 Configuring Retry

If you expect that the Config Server may occasionally be unavailable when your application starts, you can make it keep trying after a failure.

First, you need to set `spring:cloud:config:failFast=true`. Then you need to enable retry by adding the setting `spring:cloud:config:retry:enabled=true`.

The default behavior is to retry six times with an initial backoff interval of 1000ms and an exponential multiplier of 1.1 for subsequent backoffs. You can configure these settings (and others) by setting the `spring:cloud:config:retry:*` configuration settings described earlier.

### 2.2.11 Configuring Multiple Urls

To ensure high availability when you have multiple instances of Config Server deployed and expect one or more instances to be unavailable from time to time, you can either specify multiple URLs as a comma-separated list for `spring:cloud:config:uri` or have all your instances register in a Service Registry like Eureka if using `Discovery First` mode.

Note that doing so ensures high availability only when the Config Server is not running or responding (for example, when the server has exited or when a connection timeout has occurred). For example, if the Config Server returns a 500 (Internal Server Error) response or the Steeltoe client receives a 401 from the Config Server (due to bad credentials or other causes), the client does not try to fetch properties from other URLs. An error of that kind indicates a user issue rather than an availability problem.

If you use HTTP basic auth security on your Config Server, it is currently only possible to support per-Config Server auth credentials if you embed the credentials in each URL you specify for the `spring:cloud:config:uri` setting. If you use any other kind of security mechanism, you cannot currently support per-Config Server authentication and authorization.

# 3.0 Placeholder Provider

The Placeholder resolver enables usage of `${....}` placeholders in your configuration. The provider enables you to define configuration values as placeholders in your configuration and have them resolved to `real` values at runtime during configuration access.

A placeholders takes the form of `${key:subkey1:subkey2?default_value}` where `key:subkey1:subkey2` represents another key in the configuration. At runtime when you access the key associated with the placeholder the resolver is called to resolve the placeholder key to a value that exists in the configuration.  If a value for the placeholder key is not found, the key will be returned unresolved. If a `default_value` is specified in the placeholder, then the `default_value` will returned instead.

Note that placeholder defaults (for example, `default_value`) can be defined to be placeholders as well and those will be resolved as well.

The Placeholder resolver provider supports the following .NET application types:

* ASP.NET (MVC, WebForms, WebAPI, WCF)
* ASP.NET Core
* Console apps (.NET Framework and .NET Core)

 The source code for this provider can be found [here](https://github.com/SteeltoeOSS/Configuration).

## 3.1 Quick Start

This quick start shows how to use the Placeholder configuration provider in an ASP.NET Core MVC application.

### 3.1.1 Locate Sample

First, you must navigate to the correct directory, as follows:

```bash
cd Samples/Configuration/src/AspDotNetCore/Placeholder
```

#### 3.1.2 Run Sample

To run the application on .NET Core on Windows, Linux or OSX, use the following command:

```bash
dotnet run -f netcoreapp2.2
```

#### 3.1.3 Observe Logs

The `dotnet run` command should produce output similar to the following:

```bash
Hosting environment: Production
Now listening on: http://localhost:5000
Application started. Press Ctrl+C to shut down.
```

### 3.1.4 What to Expect

Access the main page of the application at `http://localhost:5000/` and you should see three values from the apps configuration that have been tied to placeholders

* `ResolvedPlaceholderFromEnvVariables` - placeholder that references `PATH` environment variable
* `UnresolvedPlaceholder` - placeholder that can't be resolved
* `ResolvedPlaceholderFromJson` - placeholder that is resolved from the two `appsettings.json` files.

### 3.1.5 Understand Sample

The `Placeholder` quick start sample was created by using the .NET Core tooling `mvc` template (`dotnet new mvc`) and then modified to include the Steeltoe framework.

To gain an understanding of the Steeltoe related changes to the generated template code, examine the following files:

* `Placeholder.csproj`: Contains the `PackageReference` for Steeltoe NuGet `Steeltoe.Extensions.Configuration.PlaceholderCore`
* `Program.cs`:  Code was added to the `IWebHostBuilder` to add the Placeholder resolver to the apps configuration.
* `Startup.cs`: Code was added to configure `SampleOptions`.
* `HomeController.cs`: Code was added for Options injection into the Controller. Code was also added to display the resolved configuration data.

## 3.2 Usage

The following sections describe how to use the Placeholder resolver configuration provider:

* [Add NuGet Reference](#3-2-1-add-nuget-reference)
* [Add Configuration Provider](#3-2-2-add-configuration-provider)
* [Access Configuration Data](#3-2-3-access-configuration-data)
* [Access Configuration Data as Options](#3-2-4-access-configuration-data-as-options)

You should have a good understanding of how the .NET [Configuration services](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration) work before starting to use this provider.

In order to use the Steeltoe Placeholder resolver provider you need to do the following:

1. Add a NuGet package reference to your project.
1. Add the provider to the Configuration Builder.
1. Optionally, configure Options classes by binding configuration data to the classes.
1. Inject and use the Options classes or access configuration data directly.

>NOTE: Most of the example code in the following sections is based on using Steeltoe in an ASP.NET Core application. If you are developing an ASP.NET 4.x application or a Console based app, see the [other samples](https://github.com/SteeltoeOSS/Samples/tree/master/Configuration) for example code you can use.

### 3.2.1 Add NuGet Reference

To use the provider, you need to add a reference to the appropriate Steeltoe NuGet based on the type of the application you are building and what Dependency Injector you have chosen, if any. The following table describes the available packages:

|App Type|Package|Description|
|---|---|---|
|Console/ASP.NET 4.x|`Steeltoe.Extensions.Configuration.PlaceholderBase`|Base functionality. No dependency injection.|
|ASP.NET Core|`Steeltoe.Extensions.Configuration.PlaceholderCore`|Includes base. Adds ASP.NET Core dependency injection.|

To add this type of NuGet to your project, add a `PackageReference` resembling the following:

```xml
<ItemGroup>
...
    <PackageReference Include="Steeltoe.Extensions.Configuration.PlaceholderCore" Version= "2.2.0"/>
...
</ItemGroup>
```

### 3.2.2 Add Configuration Provider

In order to have placeholders resolved when accessing your configuration data, you need to add the Placeholder resolver provider to the `ConfigurationBuilder`.  

There are four different ways in which you can do this.

1. Add the resolver using `ConfigurationBuilder` extension method `AddPlaceholderResolver()`.
1. Add the resolver to an already built configuration using `IConfiguration` extension method `AddPlaceholderResolver()`.
1. Add the resolver using `IWebHostBuilder` extension method `AddPlaceholderResolver()`.
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

The following example shows how to add to the `IWebHostBuilder`:

```csharp
public class Program
{
    public static void Main(string[] args)
    {
        BuildWebHost(args).Run();
    }
    public static IWebHost BuildWebHost(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            .UseCloudFoundryHosting()

            .AddPlaceholderResolver()
            .UseStartup<Startup>()
            .Build();
}
```

>NOTE: It is important to understand that the Placeholder resolver works by wrapping and replacing the existing configuration providers already added to the `ConfigurationBuilder`. As a result you typically will want to add it as the last provider.

### 3.2.3 Access Configuration Data

Once the configuration has been built, the Placeholder resolver will be used to resolve any placeholders as you access your configuration data.  Simply access the configuration data as your normally would and the resolver will attempt to resolve and placeholder before returning the value for the key requested.

Consider the following `appsettings.json` file:

```json
{
    "spring": {
        "bar": {
            "name": "myName"
    },
      "cloud": {
        "config": {
            "name" : "${spring:bar:name?no_name}",
        }
      }
    }
  ...
}
```

When using the normal `IConfiguration` indexer to access the configuration you will see the Placeholder resolver do its thing:

```csharp
var config = builder.Build();

Assert.Equal("myName", config["spring:cloud:config:name"]);
...
```

### 3.2.4 Access Configuration Data as Options

Alternatively, instead of accessing the configuration data directly from the configuration, you can also use the .NET [Options](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration) framework together with placeholders.

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

Notice `ResolvedPlaceholderFromEnvVariables` uses a placeholder that references the `PATH` environment variable which is added to the configuration by the default Web host builder.
Also notice `ResolvedPlaceholderFromJson` uses a placeholder that references keys that come from the `.json` configuration files.

Next, add the Placeholder resolver to the `IWebHostBuilder` in `Program.cs` or in any of the other ways described above:

```csharp
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Steeltoe.Extensions.Configuration.PlaceholderCore;
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

Then to use the configuration and the added Placeholder resolver together with your Options classes simply configure the Options as you normally would.

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

# 4.0 RandomValue Provider

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
    "my" : {
        "secret" = "${random:value}",
        "number" = "${random:int}",
        "big_number" = "${random:long}",
        "uuid" = "${random:uuid}",
        "number_less_than_ten" = "${random:int(10)}",
        "number_in_range" = "${random:int[1024,65536]}"
    }
}
```

The RandomValue provider supports the following .NET application types:

* ASP.NET (MVC, WebForms, WebAPI, WCF)
* ASP.NET Core
* Console apps (.NET Framework and .NET Core)

 The source code for this provider can be found [here](https://github.com/SteeltoeOSS/Configuration).

## 4.1 Quick Start

This quick start shows how to use the RandomValue configuration provider in an ASP.NET Core MVC application.

### 4.1.1 Locate Sample

First, you must navigate to the correct directory, as follows:

```bash
cd Samples/Configuration/src/AspDotNetCore/RandomValue
```

#### 4.1.2 Run Sample

To run the application on .NET Core on Windows, Linux or OSX, use the following command:

```bash
dotnet run -f netcoreapp2.2
```

#### 4.1.3 Observe Logs

The `dotnet run` command should produce output similar to the following:

```bash
Hosting environment: Production
Now listening on: http://localhost:5000
Application started. Press Ctrl+C to shut down.
```

### 4.1.4 What to Expect

Access the main page of the application at `http://localhost:5000/` and you should see multiple random values generated from an `IConfiguration`.

### 4.1.5 Understand Sample

The `RandomValue` quick start sample was created by using the .NET Core tooling `mvc` template (`dotnet new mvc`) and then modified to include the Steeltoe framework.

To gain an understanding of the Steeltoe related changes to the generated template code, examine the following files:

* `RandomValue.csproj`: Contains the `PackageReference` for Steeltoe NuGet `Steeltoe.Extensions.Configuration.RandomValueBase`
* `Program.cs`:  Code was added to the `IWebHostBuilder` to add the RandomValue generator to the apps configuration.
* `HomeController.cs`: Code was added for `IConfiguration` injection into the Controller. Code was also added to display the random values.

## 4.2 Usage

The following sections describe how to use the Placeholder resolver configuration provider:

* [Add NuGet Reference](#4-2-1-add-nuget-reference)
* [Add Configuration Provider](#4-2-2-add-configuration-provider)
* [Access Random Value Data](#4-2-3-access-random-value-data)

You should have a good understanding of how the .NET [Configuration services](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration) work before starting to use this provider.

In order to use the Steeltoe RandomValue provider you need to do the following:

1. Add a NuGet package reference to your project.
1. Add the provider to the Configuration Builder.
1. Access random values from the `IConfiguration`.

>NOTE: Most of the example code in the following sections is based on using Steeltoe in an ASP.NET Core application. If you are developing an ASP.NET 4.x application or a Console based app, see the [other samples](https://github.com/SteeltoeOSS/Samples/tree/master/Configuration) for example code you can use.

### 4.2.1 Add NuGet Reference

To use the provider, you need to add a reference to the appropriate Steeltoe NuGet.

To do this add a `PackageReference` resembling the following:

```xml
<ItemGroup>
...
    <PackageReference Include="Steeltoe.Extensions.Configuration.RandomValueBase" Version= "2.2.0"/>
...
</ItemGroup>
```

### 4.2.2 Add Configuration Provider

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

### 4.2.3 Access Random Value Data

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

# 5.0 Hosting Extensions

Many cloud hosting providers, including Pivotal Cloud Foundry, dynamically provide port numbers at runtime. For ASP.NET Core applications, Steeltoe provides an extension method for `IWebHostBuilder` named `UseCloudFoundryHosting` that will automatically use the environment variable `PORT` (when present) to set the address the application is listening on. This sample illustrates basic usage:

```csharp
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                ...
                .UseCloudFoundryHosting()
                ...
```

The extension includes an optional parameter to explicitly set the port, which is particularly useful when you are running multiple services at once on your workstation that will later be deployed to a cloud platform.

```csharp
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                ...
                .UseCloudFoundryHosting(5001)
                ...
```

>NOTE: As this extension is intended for use on Cloud Foundry, if the 'PORT' environment variable is present, it will always override the parameter.

# Common Steps

This section describes tasks that are common to many of the processes described in this guide.

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

>NOTE: Manifest file names may vary. Some samples use a different manifest for .NET 4 vs .NET Core.

<!-- -->
>NOTE: All sample manifests have been defined to bind their application to their service(s).
