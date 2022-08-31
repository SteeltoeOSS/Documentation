# Cloud Foundry Provider

The Cloud Foundry provider enables the standard Cloud Foundry environment variables (`VCAP_APPLICATION`,  `VCAP_SERVICES`, and `CF_*`) to be parsed and accessed as configuration data within a .NET application.

Cloud Foundry creates and uses these environment variables to communicate an application's environment and configuration to the application code running inside a container. More specifically, the values found in `VCAP_APPLICATION` provide information about the application's resource limits, routes (URIs), and version number, among other things. The `VCAP_SERVICES` environment variable provides information about the external services (databases, caches, and so on) to which the application is bound, along with details on how to contact those services.

You can read more information on the Cloud Foundry environment variables at the [Cloud Foundry docs](https://docs.cloudfoundry.org/devguide/deploy-apps/environment-variable.html) website.

## Usage

You should have a good understanding of how the .NET [Configuration services](https://docs.microsoft.com/aspnet/core/fundamentals/configuration) work before starting to use this provider.

In order to use the Steeltoe Cloud Foundry provider, you need to do the following:

1. Add a NuGet package reference to your project.
1. Add the provider to the Configuration Builder.
1. Configure Cloud Foundry options classes by binding configuration data to the classes.
1. Inject and use the Cloud Foundry Options to access Cloud Foundry configuration data.

### Add NuGet Reference

To use the provider, you need to add a reference to the appropriate Steeltoe Cloud Foundry NuGet based on the type of the application you are building and what Dependency Injector you have chosen, if any. The following table describes the available packages:

| Package | Description | .NET Target |
| --- | --- | --- |
| `Steeltoe.Extensions.Configuration.CloudFoundryBase` | Base functionality. No dependency injection. | .NET Standard 2.0 |
| `Steeltoe.Extensions.Configuration.CloudFoundryCore` | Includes base. Adds ASP.NET Core dependency injection. | ASP.NET Core 3.1+ |

To add this type of NuGet to your project, add a `PackageReference` resembling the following:

```xml
<ItemGroup>
...
    <PackageReference Include="Steeltoe.Extensions.Configuration.CloudFoundryCore" Version="3.2.0" />
...
</ItemGroup>
```

### Add Configuration Provider

To parse the Cloud Foundry environment variables and make them available in the application's configuration, you need to add the Cloud Foundry configuration provider to the `ConfigurationBuilder`, as follows:

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

When developing a .NET Core application, you can do the same thing by using the `AddCloudFoundryConfiguration()` extension method for either the `IWebHostBuilder` or Generic `IHostBuilder`. The following example shows how to do so:

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

            // Add VCAP_* configuration data
            .AddCloudFoundryConfiguration()
            .UseStartup<Startup>()
            .Build();
}
```

### Access Configuration Data

Once the configuration has been built, the values from the `VCAP_APPLICATION` and `VCAP_SERVICES` environment variables have been added to the application's configuration data and become available under keys prefixed with `vcap:application` and `vcap:services` respectively.

You can access the values from the `VCAP_APPLICATION` environment variable settings directly from the configuration as follows:

```csharp
var config = builder.Build();
var appName = config["vcap:application:application_name"];
var instanceId = config["vcap:application:instance_id"];
...
```

A list of all `VCAP_APPLICATION` keys is available in the [VCAP_APPLICATION](https://docs.CloudFoundry.org/devguide/deploy-apps/environment-variable.html#VCAP-APPLICATION) topic of the Cloud Foundry documentation.

You can also directly access the values from the `VCAP_SERVICES` environment variable. For example, to access the information about the first instance of a bound Cloud Foundry service with a name of `service-name`, you could code the following:

```csharp
var config = builder.Build();
var name = config["vcap:services:service-name:0:name"];
var uri = config["vcap:services:service-name:0:credentials:uri"];
...
```

A list of all `VCAP_SERVICES` keys is available in the [VCAP_SERVICES](https://docs.CloudFoundry.org/devguide/deploy-apps/environment-variable.html#VCAP-SERVICES) topic of the Cloud Foundry documentation.

>This provider uses the built-in .NET [JSON Configuration Parser](https://github.com/aspnet/Configuration/tree/dev/src/Microsoft.Extensions.Configuration.Json) when parsing the JSON provided in the `VCAP_*` environment variables. As a result, you can expect the exact same key names and behavior as you see when parsing JSON configuration files (such as `appsettings.json`) in your application.

### Access Configuration Data as Options

#### Using the ConfigureCloudFoundryOptions() Method

Alternatively, instead of accessing the Cloud Foundry configuration data directly from the configuration, you can use the .NET [Options](https://docs.microsoft.com/aspnet/core/fundamentals/configuration) framework together with [Dependency Injection](https://docs.microsoft.com/aspnet/core/fundamentals/dependency-injection).

The Cloud Foundry provider includes two additional classes, `CloudFoundryApplicationOptions` and `CloudFoundryServicesOptions`. You can configured both through the options framework to hold the parsed `VCAP_*` data by using the options `Configure()` feature.

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

Once this is done, you can access these configuration objects in the controllers or views of an application by using normal Dependency Injection, as follows:

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

#### ConfigureCloudFoundryService()

As an alternative to using `CloudFoundryServicesOptions` to access Cloud Foundry service data, you can also use `ConfigureCloudFoundryService<TOption>()` or `ConfigureCloudFoundryServices<TOption>()` to easily gain access to service data.

These methods let you define an `Options` class that represents a particular type of Cloud Foundry service binding and then use either method to select that data from `VCAP_SERVICES` and bind the data to it.

To do this, you first need to create an `Options` class that derives from `CloudFoundryServicesOptions`. That class must match the data provided in `VCAP_SERVICES`.

The following example shows how to do this for a MySql service binding on PCF:

```csharp
using Steeltoe.Extensions.Configuration.CloudFoundry;

public class MySqlServiceOption : CloudFoundryServicesOptions
{
    public MySqlServiceOption() { }
    public MySqlServiceOption(IConfiguration config) : base(config) { }
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

Next in your `Startup` class you can use either `ConfigureCloudFoundryService<TOption>()` or `ConfigureCloudFoundryServices<TOption>()` to bind the service data from `VCAP_SERVICES` to your `TOption`. There are multiple ways to do this depending on your needs.

You can use the `ConfigureCloudFoundryService<TOption>()` method to select a specific Cloud Foundry service binding from `VCAP_SERVICES` by specifying a service name. Alternatively, you can use `ConfigureCloudFoundryServices<TOption>()` to bind to all services of a particular type by specifying a Cloud Foundry service label.

The following listing shows some examples:

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

All of this is built by using the Microsoft provided `Options` framework.  As a result we can leverage the `Named` `Options` feature Microsoft has implemented in options binding and configure each `TOption` with a name equal to the Cloud Foundry service name found in `VCAP_SERVICES`.

What this means is that, within a controller, you can inject the `IOptionsSnapshot<MySqlServiceOption>` or `IOptionsMonitor<MySqlServiceOption>` as you normally would and then access the option by name (for example: specific Cloud Foundry service binding instance).

The following listing shows how to do so:

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
    }
```
