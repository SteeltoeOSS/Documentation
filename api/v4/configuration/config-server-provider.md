# Config Server Provider

This provider enables the Spring Cloud Config Server to be used as a source of configuration data for a .NET application.

The Spring Cloud Config Server is an application configuration service that gives you a central place to manage an application's configuration values externally across all environments. As an application moves through the deployment pipeline from development to test and into production, you can use the config server to manage the configuration between environments and be certain that the application has everything it needs to run when you migrate it. The config server easily supports labeled versions of environment-specific configurations and is accessible to a wide range of tooling for managing its content.

To gain a better understanding of the Spring Cloud Config Server, you should read the [Spring Cloud Config documentation](https://spring.io/projects/spring-cloud-config).

In addition to the Quick Start provided later, you can refer to the [Steeltoe ConfigurationProviders](https://github.com/SteeltoeOSS/Samples/tree/main/Configuration/src/ConfigurationProviders) sample application when you need to understand how to use this provider.

## Usage

You should know how the [.NET Configuration System](https://learn.microsoft.com/aspnet/core/fundamentals/configuration) works before starting to use this provider. A basic understanding of the `ConfigurationBuilder` and how to add providers to the builder is necessary.

You should also have a good understanding of the [Spring Cloud Config Server](https://cloud.spring.io/spring-cloud-config/).

To use the Steeltoe provider, you need to do the following:

1. Add the appropriate NuGet package reference to your project.
1. Configure the settings that the Steeltoe provider uses to access the Spring Cloud Config Server.
1. Add the provider to the host builder or configuration builder.
1. Optionally, bind the configuration data to a class using the Options Pattern.
1. Inject and use `IConfiguration` or `IOptions<>` to access configuration data.

### Add NuGet Reference

To use the provider, you need to add a reference to the `Steeltoe.Configuration.ConfigServer` NuGet package.

### Configure Settings

The most convenient way to configure settings for the provider is to put them in a file and then use one of the other file-based configuration providers to read them.

The following example shows some provider settings that have been put in a JSON file. Only two settings are really necessary. `Spring:Application:Name` configures the "application name" to be `sample`, and `Spring:Cloud:Config:Uri` configures the address of the config server.

> [!TIP]
> The `Spring:Application:Name` key is also used by various other Steeltoe components.

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
}
```

The following table describes all the settings that can be used to configure the behavior of the provider:

| Key | Description | Default |
| --- | --- | --- |
| `Name` | Application name for which to request configuration. | |
| `Enabled` | Enable or disable the config server client. | `true` |
| `Uri` | Comma-separated list of config server endpoints. | `http://localhost:8888` |
| `Env` | Environment or profile used in the server request. | `Production` |
| `ValidateCertificates` | Enable or disable server certificate validation. | `true` |
| `Label` | Comma-separated list of labels to request. | |
| `Timeout` | Time to wait for response from server, in milliseconds. | `60_000` (1 min) |
| `PollingInterval` | How often to check for changes in Config Server. | |
| `Username` | Username for basic authentication. | |
| `Password` | Password for basic authentication. | |
| `FailFast` | Enable or disable failure at startup. | `false` |
| `Headers` | Extra HTTP headers which are added to config server requests. | |
| `Token` | HashiCorp Vault authentication token. | |
| `TokenTtl` | HashiCorp Vault token renewal TTL, in milliseconds. Valid on Cloud Foundry only. | `300_000` (5 min) |
| `TokenRenewRate` | HashiCorp Vault token renewal rate, in milliseconds. Valid on Cloud Foundry only. | `60_000` (1 min) |
| `DisableTokenRenewal` | Enable or disable HashiCorp Vault token renewal. Valid on Cloud Foundry only. | `false` |
| `Retry:Enabled` | Enable or disable retry logic. | `false` |
| `Retry:MaxAttempts` | Max retries if retry enabled. | `6` |
| `Retry:InitialInterval` | Starting interval, in milliseconds. | `1000` |
| `Retry:MaxInterval` | Maximum retry interval, in milliseconds. | `2000` |
| `Retry:Multiplier` | Retry interval multiplier. | `1.1` |
| `ClientId` | OAuth2 client ID when using OAuth security. | |
| `ClientSecret` | OAuth2 client secret when using OAuth security. | |
| `AccessTokenUri` | URI to use to obtain OAuth access token. | |
| `Discovery:Enabled` | Enable or disable the Discovery First feature. | `false` |
| `Discovery:ServiceId` | Config Server service ID to use in Discovery First feature. | `configserver` |
| `Health:Enabled` | Enable or disable config server health check contributor. | `true` |
| `Health:TimeToLive` | Health check contributor cache time to live, in milliseconds. | `300_000` (5 min) |

As mentioned earlier, all settings above should start with `Spring:Cloud:Config:`

> [!TIP]
> If you use self-signed certificates on Cloud Foundry, you might run into certificate validation issues when pushing an application.
> A quick way to work around this is to disable certificate validation until a proper solution can be put in place.

### Add Configuration Provider

Once the provider's settings have been defined and put in a file (such as a JSON file), the next step is to read them and make them available to the provider.

In the next C# example, the provider's configuration settings from the preceding example are put in the `appsettings.json` file included with the application. Then, by using the .NET JSON configuration provider, we can read the settings by adding the JSON provider to the configuration builder (`AddJsonFile("appsettings.json")`).

Then, after the JSON provider has been added, you can add the config server provider to the builder. Steeltoe provides an extension method, `AddConfigServer()`, that you can use to do so.

Because the JSON provider that reads `appsettings.json` has been added *before* the config server provider, the JSON-based settings become available to the Steeltoe provider. Note that you do not have to use JSON for the Steeltoe settings. You can use any of the other off-the-shelf configuration providers for the settings (such as INI files, environment variables, and so on).

> [!CAUTION]
> You need to use the `Add*()` methods to add the source of the config server clients settings (`AddJsonFile(..)`) *before* you use `AddConfigServer(..)`. Otherwise, the settings are not picked up and used.

The following sample shows how to add a configuration provider:

```csharp
using Steeltoe.Configuration.ConfigServer;

var configurationBuilder = new ConfigurationBuilder()
    .SetBasePath(hostEnvironment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{hostEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddConfigServer();

var configuration = configurationBuilder.Build();
```

When developing a .NET Core application, you can accomplish the same thing by using the `AddConfigServer()` host builder extension method, as follows:

```csharp
using Steeltoe.Configuration.ConfigServer;

var builder = WebApplication.CreateBuilder(args);
builder.AddConfigServer();
```

### Bind to Cloud Foundry

When you want to use a Config Server on Cloud Foundry and you have installed [Spring Cloud Services](https://github.com/SteeltoeOSS/Samples/blob/main/CommonTasks.md#provision-sccs-on-cloud-foundry), you can create and bind an instance of it to your application by using the Cloud Foundry CLI, as follows:

```bash
# Create a Config Server instance named `myConfigServer`
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

### Access Configuration Data

When the `ConfigurationBuilder` builds the configuration, the Config Server client makes the appropriate REST calls to the Config Server and retrieves the configuration values based on the settings that have been provided.

If there are any errors or problems accessing the server, the application continues to initialize, but the values from the server are not retrieved. If this is not the behavior you want, you should set `Spring:Cloud:Config:FailFast` to `true`. Once that is done, the application fails to start if problems occur during the retrieval.

> [!TIP]
> To diagnose startup errors, activate bootstrap logging as described [here](../bootstrap/index.md#logging-inside-configuration-providers).

After the configuration has been built, you can access the retrieved data directly by using `IConfiguration`. The following example shows how to do so:

```csharp
var configuration = configurationBuilder.Build();
string? property1 = configuration["example:property1"];
string? property2 = configuration["example:property2"];
```

Alternatively, you can create a class to hold your configuration data and then use the [Options Pattern](https://learn.microsoft.com/aspnet/core/fundamentals/configuration/options) together with [Dependency Injection](https://learn.microsoft.com/aspnet/core/fundamentals/dependency-injection) to obtain an instance of your options class into your controllers and views.

To do so, first create a class representing the configuration data you expect to retrieve from the server, as follows:

```csharp
public class ExampleOptions
{
    public string? Property1 { get; set; }
    public string? Property2 { get; set; }
}
```

Next, use the code below to bind the `example:*` values to an instance of the `ExampleOptions` class.

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOptions<ExampleOptions>().BindConfiguration("example");
```

After this has been done, you can gain access to the data in your `Controller` or `View` through dependency injection. The following example shows how to do so:

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

public class HomeController(IOptionsSnapshot<ExampleOptions> optionsSnapshot)
    : Controller
{
    public IActionResult Index()
    {
        ExampleOptions options = optionsSnapshot.Value;

        ViewData["property1"] = options.Property1;
        ViewData["property2"] = options.Property2;
        ViewData["property2"] = options.Property2;
        return View();
    }
}
```

### Configuring Discovery First

The default behavior for the Steeltoe Config Client is to access the Config Server through the `Spring:Cloud:Config:Uri` configuration setting. This of course requires that the application contains an `appsettings.json` or an environment variable with the Config Server's address set in `Spring:Cloud:Config:Uri`.  This mode of operation, the default, is called "Config First".

Alternatively, you can set up your Config Server to register with a discovery service such as Netflix Eureka. This enables your application to look up the address of the Config Server by using a discovery service instead of configuring it in `appsettings.json`. Note that you have to specifically configure your Config Server deployment to register with a discovery service, as this does not happen automatically. See the Spring Cloud Config Server documentation for how to do so.

However, with the default "Config First" mode of the Steeltoe client, you are not able to take advantage of the Config Server registration unless you change the clients' mode of operation to "Discovery First". To do so:

1. If your application does not use a service discovery service, you need to configure your application to do so. See the [Steeltoe Discovery documentation](../discovery/initialize-discovery-client.md) for the details on how. At a minimum, you need to configure the Eureka Server address.
1. Change the Steeltoe Config Server client setting `Spring:Cloud:Config:Discovery:Enabled` to `true` (the default is `false`).
1. If your Config Server is registered with Eureka using a name other than "configserver", use `Spring:Cloud:Config:Discovery:ServiceId` to specify the name used by the client for lookup.

Note that the cost for using this mode of operation is an extra network roundtrip on startup to locate the Config Server service registration. The benefit is that, as long as the discovery service is at a fixed point, the Config Server can change its address and no changes to applications are needed.

### Configuring Health Contributor

The Config Server package provides a Steeltoe management health contributor that attempts to load configuration from the Config Server and contributes health information to the results of the health endpoint.

If you use the `AddConfigServer()` host builder extension method, the contributor is automatically added to the container and is automatically picked up and used. Otherwise, you can manually add the contributor to the container by using the `AddConfigServerHealthContributor()` extension method.

The contributor is enabled by default, but can be disabled by setting `Spring:Cloud:Config:Health:Enabled` to `false`.

The response from the Config Server is cached for performance reasons. The default cache time-to-live is five minutes. To change that value, set the `Spring:Cloud:Config:Health:TimeToLive` to the desired milliseconds.

### Configuring Fail Fast

In some cases, you may want to fail the startup of your application if it cannot connect to the Config Server. If this is the desired behavior, set the configuration setting `Spring:Cloud:Config:FailFast` to `true` to make the client halt with an `Exception`.

### Configuring Retries

If you expect that the Config Server may occasionally be unavailable when your application starts, you can make it keep trying after a failure.

First, you need to set `Spring:Cloud:Config:FailFast` to `true`. Then you need to enable retry by setting `Spring:Cloud:Config:Retry:Enabled` to `true`.

The default behavior is to retry six times with an initial back-off interval of 1000ms and an exponential multiplier of 1.1 for subsequent back-offs. You can configure these settings (and others) by setting the `Spring:Cloud:Config:Retry:*` configuration settings described earlier.

### Configuring Multiple URLs

To ensure high availability when you have multiple instances of Config Server deployed and expect one or more instances to be unavailable from time to time, you can either specify multiple URLs as a comma-separated list for `Spring:Cloud:Config:Uri` or have all your instances register in a Service Registry such as Eureka (if you use "Discovery First" mode).

Note that doing so ensures high availability only when the Config Server is not running or responding (for example, when the server has exited or when a connection timeout has occurred). For example, if the Config Server returns a 500 (Internal Server Error) response or the Steeltoe client receives a 401 from the Config Server (due to bad credentials or other causes), the client does not try to fetch properties from other URLs. An error of that kind indicates a user issue rather than an availability problem.

If you use HTTP basic auth security on your Config Server, it is possible to use per-Config Server auth credentials by embedding the credentials in each URL you specify for the `Spring:Cloud:Config:Uri` setting. If you use any other kind of security mechanism, you cannot currently use per-Config Server authentication and authorization.

### Configuring Mutual TLS

When Spring Cloud Config Server is configured to require Mutual TLS authentication, Steeltoe needs to be provided with a valid client certificate. A client certificate can be configured in `appsettings.json`:

```json
{
  "Certificates": {
    "ConfigServer": {
      "CertificateFilePath": "/path/to/instance.crt",
      "PrivateKeyFilePath": "/path/to/instance.key"
    }
  }
}
```

Aside from PEM files, Steeltoe supports a single file in PKCS#12 format:

```json
{
  "Certificates": {
    "ConfigServer": {
      "CertificateFilePath": "/path/to/instance.p12"
    }
  }
}
```

> [!TIP]
> A single certificate can be shared with both Config Server and Eureka, by using the key "Certificates" instead of "Certificates:ConfigServer".
