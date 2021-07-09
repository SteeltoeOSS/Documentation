# Config Server Provider

This provider enables the Spring Cloud Config Server to be used as a source of configuration data for a .NET application.

The Spring Cloud Config Server is an application configuration service that gives you a central place to manage an application's configuration values externally across all environments. As an application moves through the deployment pipeline from development to test and into production, you can use the config server to manage the configuration between environments and be certain that the application has everything it needs to run when you migrate it. The config server easily supports labelled versions of environment-specific configurations and is accessible to a wide range of tooling for managing its content.

To gain a better understanding of the Spring Cloud Config Server, you should read the [Spring Cloud](https://projects.spring.io/spring-cloud/) documentation.

In addition to the Quick Start provided later, you can refer to several other Steeltoe sample applications when you need to understand how to use this provider:

* [AspDotNetCore/Simple](https://github.com/SteeltoeOSS/Samples/tree/master/Configuration/src/Simple): ASP.NET Core sample application showing how to use the open source Config Server.
* [MusicStore](https://github.com/SteeltoeOSS/Samples/tree/master/MusicStore): A sample application showing how to use all of the Steeltoe components together in a ASP.NET Core application. This is a microservices based application built from the ASP.NET Core MusicStore reference app provided by Microsoft.
* [FreddysBBQ](https://github.com/SteeltoeOSS/Samples/tree/master/FreddysBBQ): A polyglot microservices-based sample application showing inter-operability between Java and .NET on Cloud Foundry. It is secured with OAuth2 Security Services and uses Spring Cloud Services.

## Usage

You should know how the .NET [Configuration services](https://docs.microsoft.com/aspnet/core/fundamentals/configuration) work before starting to use this provider. A basic understanding of the `ConfigurationBuilder` and how to add providers to the builder is necessary.

You should also have a good understanding of the [Spring Cloud Config Server](https://cloud.spring.io/spring-cloud-config/).

To use the Steeltoe provider, you need to do the following:

1. Add the appropriate NuGet package reference to your project.
1. Configure the settings that the Steeltoe provider uses to access the Spring Cloud Config Server.
1. Add the provider to the configuration builder.
1. Optionally, configure the returned config server config data as Options.
1. Inject and use `Options` or `ConfigurationRoot` to access configuration data.

### Add NuGet Reference

You can choose one of two Config Server client NuGets, depending on your needs.

If you plan on only connecting to the open source version of [Spring Cloud Config Server](https://projects.spring.io/spring-cloud/), you should use one of the packages described by the following table, depending on your application type and needs:

| Package | Description | .NET Target |
| --- | --- | --- |
| `Steeltoe.Extensions.Configuration.ConfigServerBase` | Base functionality. No dependency injection. | .NET Standard 2.0 |
| `Steeltoe.Extensions.Configuration.ConfigServerCore` | Includes base. Adds ASP.NET Core dependency injection. | ASP.NET Core 3.1+ |

To add this type of NuGet to your project, add a `PackageReference` that resembles the following:

```xml
<ItemGroup>
...
    <PackageReference Include="Steeltoe.Extensions.Configuration.ConfigServerCore" Version="3.1.0"/>
...
</ItemGroup>
```

### Configure Settings

The most convenient way to configure settings for the provider is to put them in a file and then use one of the other file-based configuration providers to read them.

The following example shows some provider settings that have been put in a JSON file. Only two settings are really necessary. `Spring:Application:Name` configures the "application name" to be `sample`, and `Spring:Cloud:Config:Uri` configures the address of the config server.

>The `Spring:Application:Name` is also used by other Steeltoe libraries in addition to the config server.

```json
{
  "Spring": {
    "Application":{
      "Name": "sample"
    },
    "Cloud": {
      "Config": {
        "Uri": "http://localhost:8888"
      }
    }
  }
  ...
}
```

The following table describes all the settings that can be used to configure the behavior of the provider:

| Key | Description | Default |
| --- | --- | --- |
| `Name` | Application name for which to request config. | `IHostingEnvironment.ApplicationName` |
| `Enabled` | Enable or disable config server client. | `true` |
| `Uri` | Comma-separated list of config server endpoints. | `http://localhost:8888` |
| `Env` | Environment or profile used in the server request. | `IHostingEnvironment.EnvironmentName` |
| `ValidateCertificates` | Enable or disable certificate validation. | `true` |
| `Label` | Comma-separated list of labels to request. | master |
| `Timeout` | Time to wait for response from server, in milliseconds. | 6000 |
| `Username` | Username for basic authentication. | none |
| `Password` | Password for basic authentication. | none |
| `FailFast` | Enable or disable failure at startup. | `false` |
| `Token` | Hashicorp Vault authentication token. | none |
| `TokenTtl` | Hashicorp Vault token renewal TTL. Valid on Cloud Foundry only. | 300000ms |
| `TokenRenewRate` | Hashicorp Vault token renewal rate. Valid on Cloud Foundry only. | 60000ms |
| `Retry:Enabled` | Enable or disable retry logic. | false |
| `Retry:MaxAttempts` | Max retries if retry enabled. | 6 |
| `Retry:InitialInterval` | Starting interval. | 1000ms |
| `Retry:MaxInterval` | Maximum retry interval. | 2000ms |
| `Retry:Multiplier` | Retry interval multiplier. | 1.1 |
| `ClientId` | OAuth2 client ID when using OAuth security. | none |
| `ClientSecret` | OAuth2 client secret when using OAuth security. | none |
| `AccessTokenUri` | URI to use to obtain OAuth access token. | none |
| `Discovery:Enabled` | Enable or disable discovery first feature. | `false` |
| `Discovery:ServiceId` | Config Server service ID to use in discovery first feature. | `configserver` |
| `Health:Enabled` | Enable or disable config server health check contributor. | `true` |
| `Health:TimeToLive` | Health check contributor cache time to live in ms. | 60*5ms |

As mentioned earlier, all settings should start with `Spring:Cloud:Config:`

>If you use self-signed certificates on Cloud Foundry, you might run into certificate validation issues when pushing an application. A quick way to work around this is to disable certificate validation until a proper solution can be put in place.

### Add Configuration Provider

Once the provider's settings have been defined and put in a file (such as a JSON file), the next step is to read them and make them available to the provider.

In the next C# example, the provider's configuration settings from the preceding example are put in the `appsettings.json` file included with the application. Then, by using the .NET JSON configuration provider, we can read the settings by adding the JSON provider to the configuration builder (`AddJsonFile("appsettings.json")`.

Then, after the JSON provider has been added, you can add the config server provider to the builder. We include an extension method, `AddConfigServer()`, that you can use to do so.

Because the JSON provider that reads `appsettings.json` has been added `before` the config server provider, the JSON-based settings become available to the Steeltoe provider. Note that you do not have to use JSON for the Steeltoe settings. You can use any of the other off-the-shelf configuration providers for the settings (such as INI files, environment variables, and so on).

You need to use the `Add*()` methods to add the source of the config server clients settings (`AddJsonFile(..)`) *before* you use `AddConfigServer(..)`. Otherwise, the settings are not picked up and used.

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

When developing a .NET Core application, you can accomplish the same thing by using the `AddConfigServer()` extension method for either the `IWebHostBuilder` or Generic `IHostBuilder`, as follows:

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

            // Use Config Server for configuration data
            .AddConfigServer()
            .UseStartup<Startup>()
            .Build();
}
```

### Bind to Cloud Foundry

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

Once the service is bound to the application, the config server settings are available and can be set up in `VCAP_SERVICES`.

Then, when you push the application, the Steeltoe provider takes the settings from the service binding and merges those settings with the settings that you have provided through other configuration mechanisms (such as `appsettings.json`).

If there are any merge conflicts, the last provider added to the configuration takes precedence and overrides all others.

>As of Steeltoe 3.0.0, an additional NuGet reference for `Steeltoe.Connector.CloudFoundry` is required to read in service bindings. Just adding the reference will be enough for service bindings to be discoverable.

### Access Configuration Data

When the `ConfigurationBuilder` builds the configuration, the Config Server client makes the appropriate REST calls to the Config Server and retrieves the configuration values based on the settings that have been provided.

If there are any errors or problems accessing the server, the application continues to initialize, but the values from the server are not retrieved. If this is not the behavior you want, you should set the `Spring:Cloud:Config:FailFast` to `true`. Once that is done, the application fails to start if problems occur during the build.

After the configuration has been built, you can access the retrieved data directly by using `IConfiguration`. The following example shows how to do so:

```csharp
...
var config = builder.Build();
var property1 = config["myconfiguration:property1"];
var property2 = config["myconfiguration:property2"];
...
```

Alternatively, you can create a class to hold your configuration data and then use the [Options](https://docs.microsoft.com/aspnet/core/fundamentals/configuration) framework together with [Dependency Injection](https://docs.asp.net/en/latest/fundamentals/dependency-injection.html) to inject an instance of the class into your controllers and view.

To do so, first create a class representing the configuration data you expect to retrieve from the server, as follows:

```csharp
public class MyConfiguration {
    public string Property1 { get; set; }
    public string Property2 { get; set; }
}
```

Next, use the `Configure<>()` method to tell the `Options` framework to create an instance of that class with the returned data. For the preceding `MyConfiguration` class, you could add the following code to the `ConfigureServices()` method in the `Startup` class in an ASP.NET Core application, as follows:

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

### Enable Logging

Sometimes, it is desirable to turn on debug logging in the provider.

To do so, you need to inject the `ILoggerFactory` into the `Startup` class constructor by adding it as an argument to the constructor. Once you have access to it, you can add a console logger to the factory and also set its minimum logging level set to `Debug`.

Once that is done, pass the `ILoggerFactory` to the Steeltoe configuration provider. The provider then uses it to establish a logger with the debug-level logging turned on.

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

### Configuring Discovery First

The default behavior for the Steeltoe Config Client is to access the Config Server through the `Spring:Cloud:Config:Uri` configuration setting. This of course requires that the application needs a `appsettings.json` or an environment variable with the Config Servers address set in `Spring:Cloud:Config:Uri`.  This mode of operation, the default, is called `Config First`.

Alternatively, you can set up your Config Server to register with a discovery service such as Netflix Eureka. This enables you application to lookup the address of the Config Server by using a discovery Service instead of configuring it in `appsettings.json`. Note that you have to specifically configure your Config Server deployment to register with a discovery service as this does not happen automatically. See the Spring Cloud Config Server documentation for how to do so.

However, with the default `Config First` mode of the Steeltoe client you are not able to take advantage of the Config Server registration unless you change the clients mode of operation to `Discovery First`. To do so:

1. If your application does not use a service discovery service, you need to configure your application to do so. See the Steeltoe Discovery documentation for the details of how to do so. Note that, currently, we support only Netflix Eureka. At a minimum, you need to configure the Eureka Server address.
1. Change the Steeltoe Config Server client setting `Spring:Cloud:Config:Discovery:Enabled` to `true` (the default is `false`).
1. Optionally, if you change the service name registered by the Config Server with Eureka, you can use `Spring:Cloud:Config:Discovery:ServiceId=YourNewName` to change the name used by the client for lookup.

Note that the price for using this mode of operation is an extra network round trip on startup to locate the Config Server service registration. The benefit is, as long as the discovery service is at a fixed point, the Config Server can change its address and no changes to applications are needed.

### Configuring Health Contributor

The Config Server package provides a Steeltoe management health contributor that attempts to load configuration from the Config Server and contributes health information to the results of the health endpoint.

If you use the `AddConfigServer()` extension method of the `IWebHostBuilder`, the contributor is automatically added to the container and is automatically picked up and used. Otherwise, you can manually add the contributor to the container by using the `AddConfigServerHealthContributor()` extension method.

The contributor is enabled by default but can be disabled by setting `Spring:Cloud:Config:Health:Enabled=false`.

The response from the Config Server is cached for performance reasons. The default cache time to live is five minutes. To change that value, set the `Spring:Cloud:Config:Health:TimeToLive=xxxx` setting (in milliseconds).

### Configuring Fail Fast

In some cases, you may want to fail the startup of your application if it cannot connect to the Config Server. If this is the desired behavior, set the configuration setting `Spring:Cloud:Config:FailFast=true` to make the client halt with an `Exception`.

### Configuring Retry

If you expect that the Config Server may occasionally be unavailable when your application starts, you can make it keep trying after a failure.

First, you need to set `Spring:Cloud:Config:FailFast=true`. Then you need to enable retry by setting `Spring:Cloud:Config:Retry:Enabled=true`.

The default behavior is to retry six times with an initial back-off interval of 1000ms and an exponential multiplier of 1.1 for subsequent back-offs. You can configure these settings (and others) by setting the `Spring:Cloud:Config:Retry:*` configuration settings described earlier.

### Configuring Multiple URLs

To ensure high availability when you have multiple instances of Config Server deployed and expect one or more instances to be unavailable from time to time, you can either specify multiple URLs as a comma-separated list for `Spring:Cloud:Config:Uri` or have all your instances register in a Service Registry such as Eureka (if you use `Discovery First` mode).

Note that doing so ensures high availability only when the Config Server is not running or responding (for example, when the server has exited or when a connection timeout has occurred). For example, if the Config Server returns a 500 (Internal Server Error) response or the Steeltoe client receives a 401 from the Config Server (due to bad credentials or other causes), the client does not try to fetch properties from other URLs. An error of that kind indicates a user issue rather than an availability problem.

If you use HTTP basic auth security on your Config Server, it is currently possible to support only per-Config Server auth credentials if you embed the credentials in each URL you specify for the `Spring:Cloud:Config:Uri` setting. If you use any other kind of security mechanism, you cannot currently support per-Config Server authentication and authorization.

### Configuring Mutual TLS

When Spring Cloud Config Server is configured to require Mutual TLS authentication, Steeltoe needs to be provided with a valid client certificate. A client certificate can be provided by an implementation of `Steeltoe.Common.ICertificateSource` that has been included in the configuration or directly as a property on the settings object.

Steeltoe includes `ICertificateSource` implementations that work with PEM files or a single file (such as PKCS#12), as follows:

```csharp
var configurationBuilder = new ConfigurationBuilder()
        .AddPemFiles("instance.crt", "instance.key") /* OR */ .AddCertificateFile("instance.p12")
        .AddConfigServer();
```

If that methodology does not work for your use case, you can supply an x509Certificate2 on the `ConfigServerClientSettings` object, as follows:

```csharp
var clientCertificate = new X509Certificate2(); // construct or retrieve your certificate as needed
var settings = new ConfigServerClientSettings { ClientCertificate = clientCertificate };
var configurationBuilder = new ConfigurationBuilder()
        .AddConfigServer(settings);
```

>A single `ICertificateSource` can be used for both Config Server and [Eureka mTLS connections](../discovery/netflix-eureka.md#configuring-mutual-tls).
