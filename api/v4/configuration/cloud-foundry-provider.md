# Cloud Foundry Provider

The Cloud Foundry provider enables the standard Cloud Foundry environment variables (`VCAP_APPLICATION`,  `VCAP_SERVICES`, and `CF_*`) to be parsed and accessed as configuration data within a .NET application.

Cloud Foundry creates and uses these environment variables to communicate an application's environment and configuration to the application code running inside a container. More specifically, the values found in `VCAP_APPLICATION` provide information about the application's resource limits, routes (URIs), and version numbers, among other things. The `VCAP_SERVICES` environment variable provides information about the external services (databases, caches, and so on) to which the application is bound, along with details for contacting those services.

For more information about the Cloud Foundry environment variables, see the [Cloud Foundry documentation](https://docs.cloudfoundry.org/devguide/deploy-apps/environment-variable.html).

## Usage

You should have a good understanding of how the [.NET Configuration System](https://learn.microsoft.com/aspnet/core/fundamentals/configuration) works before starting to use this provider.

To use the Steeltoe Cloud Foundry provider:

1. Add the appropriate NuGet package reference to your project.
1. Add the provider to the host builder or configuration builder.
1. Configure Cloud Foundry option classes by binding configuration data to them.
1. Inject and use `IConfiguration` or `IOptions<>` to access configuration data.

### Add NuGet Reference

To use the provider, add a reference to the `Steeltoe.Configuration.CloudFoundry` NuGet package.

### Add Configuration Provider with Options

The `AddCloudFoundryConfiguration()` host builder extension method is the easiest way to set things up. It performs
both of the individual steps described below: adding the configuration provider and configuring the options.
See the following example:

```csharp
using Steeltoe.Configuration.CloudFoundry;

var builder = WebApplication.CreateBuilder(args);
builder.AddCloudFoundryConfiguration();
```

#### Add Configuration Provider without using the host builder extension method

If you don't want to use the host builder extension method, the following code shows how to add the Cloud Foundry configuration provider to the `ConfigurationBuilder`:

```csharp
using Steeltoe.Configuration.CloudFoundry;

var configurationBuilder = new ConfigurationBuilder();
configurationBuilder.AddCloudFoundry();
var configuration = configurationBuilder.Build();
```

### Access Configuration Data

After the configuration has been built, the values from the `VCAP_APPLICATION` and `VCAP_SERVICES` environment variables have been added to the application's configuration data and become available under keys prefixed with `vcap:application` and `vcap:services`, respectively.

You can access the values from the `VCAP_APPLICATION` environment variable settings directly from the `IConfiguration` as follows:

```csharp
var appName = configuration["vcap:application:application_name"];
var instanceId = configuration["vcap:application:instance_id"];
```

A list of all `VCAP_APPLICATION` keys is available in [VCAP_APPLICATION](https://docs.CloudFoundry.org/devguide/deploy-apps/environment-variable.html#VCAP-APPLICATION), in the Cloud Foundry documentation.

You can also access the values directly from the `VCAP_SERVICES` environment variable. For example, to access the information about the first instance of a bound Cloud Foundry service with the name `service-name`, you could code the following:

```csharp
var name = configuration["vcap:services:service-name:0:name"];
var uri = configuration["vcap:services:service-name:0:credentials:uri"];
```

A list of all `VCAP_SERVICES` keys is available in the [VCAP_SERVICES](https://docs.CloudFoundry.org/devguide/deploy-apps/environment-variable.html#VCAP-SERVICES) topic of the Cloud Foundry documentation.

> [!NOTE]
> This provider uses the built-in .NET [JSON configuration provider](https://learn.microsoft.com/dotnet/core/extensions/configuration-providers#json-configuration-provider) when parsing the JSON provided in the `VCAP_*` environment variables. As a result, you can expect exactly the same key names and behavior as you see when parsing JSON configuration files (such as `appsettings.json`) in your application.

### Access Configuration Data as Options

Alternatively, instead of accessing the Cloud Foundry configuration data directly from the configuration, you can use the [Options Pattern](https://learn.microsoft.com/aspnet/core/fundamentals/configuration/options) with [Dependency Injection](https://learn.microsoft.com/aspnet/core/fundamentals/dependency-injection) to obtain an instance of the option classes into your controllers and views.

The Cloud Foundry provider includes two additional classes, `CloudFoundryApplicationOptions` and `CloudFoundryServicesOptions`. You can configure both to bind to the parsed `VCAP_*` data.

If you're not using the host builder extension method described earlier, you can use the `AddCloudFoundryOptions()` method to configure the Cloud Foundry option classes. This method binds the parsed `VCAP_*` data to the `CloudFoundryApplicationOptions` and `CloudFoundryServicesOptions` classes. See the following example:

```csharp
using Steeltoe.Configuration.CloudFoundry;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddCloudFoundry();
builder.Services.AddCloudFoundryOptions();
```

After this is done, you can access these configuration objects in the controllers or views of an application by using normal Dependency Injection, as follows:

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Steeltoe.Configuration.CloudFoundry;

public class HomeController(
    IOptionsSnapshot<CloudFoundryApplicationOptions> applicationOptionsSnapshot,
    IOptionsSnapshot<CloudFoundryServicesOptions> servicesOptionsSnapshot)
    : Controller
{
    public IActionResult Index()
    {
        CloudFoundryApplicationOptions applicationOptions = applicationOptionsSnapshot.Value;

        ViewData["AppName"] = applicationOptions.ApplicationName;
        ViewData["AppId"] = applicationOptions.ApplicationId;
        ViewData["URI-0"] = applicationOptions.Uris[0];

        CloudFoundryServicesOptions servicesOptions = servicesOptionsSnapshot.Value;
        CloudFoundryService firstService = servicesOptions.GetAllServices().First();

        ViewData["name"] = firstService.Name;
        ViewData["client_id"] = firstService.Credentials["client_id"].Value;
        ViewData["client_secret"] = firstService.Credentials["client_secret"].Value;
        ViewData["uri"] = firstService.Credentials["uri"].Value;
        return View();
    }
}
```
