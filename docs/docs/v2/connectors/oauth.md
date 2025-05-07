# OAuth

This connector simplifies using Cloud Foundry OAuth2 security services (for example, [UAA Server](https://github.com/cloudfoundry/uaa) or [TAS Single Sign-on](https://docs.pivotal.io/p-identity/)) by exposing the Cloud Foundry OAuth service configuration data as injectable `IOption<OAuthServiceOptions>`. It is used by the [Cloud Foundry External Security Providers](../security/index.md) but can be used separately.

## Usage

You should know how the new .NET [Configuration service](https://docs.microsoft.com/aspnet/core/fundamentals/configuration) works before starting to use the connector. To configure the connector, you need a basic understanding of the `ConfigurationBuilder` and how to add providers to the builder.

You should also know how the ASP.NET Core [Startup](https://docs.microsoft.com/aspnet/core/fundamentals/startup) class is used in configuring the application services for the application. Pay particular attention to the usage of the `ConfigureServices()` method.

You probably want some understanding of Cloud Foundry OAuth2 security services (such as [UAA Server](https://github.com/cloudfoundry/uaa) or [TAS Single Sign-on](https://docs.pivotal.io/p-identity/)) before starting to use this connector.

To use this Connector:

1. Create an OAuth service instance and bind it to your application.
1. (Optional) Configure any additional settings the OAuth connector needs.
1. Add the Steeltoe Cloud Foundry configuration provider to your ConfigurationBuilder.
1. Add the OAuth connector to your ServiceCollection.
1. Access the OAuth service options.

### Add NuGet Reference

To use the OAuth connector, you need to [add a reference to the appropriate Steeltoe Connector NuGet package](usage.md#add-nuget-references).

### Configure Settings

Configuring additional settings for the connector is not typically required, but, when Cloud Foundry uses self-signed certificates, you might need to disable certificate validation, as shown in the following example:

```json
{
  ...
  "security": {
    "oauth2": {
      "client": {
        "validateCertificates": false
      }
    }
  }
  ...
}
```

>CAUTION: Self-signed certificates are inherently insecure. Never use them for a production environment.

The samples and most templates are already set up to read from `appsettings.json`.

### Cloud Foundry

There are multiple ways to set up OAuth services on Cloud Foundry.

There is a user-provided service to define a direct binding to the Cloud Foundry UAA server. Alternatively, you can use the [TAS Single Sign-on](https://docs.pivotal.io/p-identity/)) product to provision an OAuth service binding. The process to create service binding varies for each of the approaches.

Regardless of which you choose, once the service is bound to your application, the connector's settings are available in `VCAP_SERVICES`.

### Add OAuthServiceOptions

Once the OAuth service has been bound to the application, add the OAuth connector to your service collection in the `ConfigureServices()` method of the `Startup` class, as shown in the following example:

```csharp
using Steeltoe.CloudFoundry.Connector.OAuth;

public class Startup {
    ...
    public IConfiguration Configuration { get; private set; }
    public Startup(...)
    {
      ...
    }
    public void ConfigureServices(IServiceCollection services)
    {
        // Configure and Add IOptions<OAuthServiceOptions> to the container
        services.AddOAuthServiceOptions(Configuration);

        // Add framework services.
        services.AddMvc();
        ...
    }
    ...
```

The `AddOAuthServiceOptions(Configuration)` method call configures a `OAuthServiceOptions` instance by using the configuration built by the application and adds it to the service container.

### Use OAuthServiceOptions

Finally, you can inject and use the configured `OAuthServiceOptions` into a controller, as shown in the following example:

 ```csharp
 using Steeltoe.CloudFoundry.Connector.OAuth;
 ...
 public class HomeController : Controller
 {
     OAuthServiceOptions _options;

     public HomeController(IOptions<OAuthServiceOptions> oauthOptions)
     {
         _options = oauthOptions.Value;
     }
     ...
     public IActionResult OAuthOptions()
     {
         ViewData["ClientId"] = _options.ClientId;
         ViewData["ClientSecret"] = _options.ClientSecret;
         ViewData["UserAuthorizationUrl"] = _options.UserAuthorizationUrl;
         ViewData["AccessTokenUrl"] = _options.AccessTokenUrl;
         ViewData["UserInfoUrl"] = _options.UserInfoUrl;
         ViewData["TokenInfoUrl"] = _options.TokenInfoUrl;
         ViewData["JwtKeyUrl"] = _options.JwtKeyUrl;
         ViewData["ValidateCertificates"] = _options.ValidateCertificates;
         ViewData["Scopes"] = CommaDelimit(_options.Scope);

         return View();
     }
 }
 ```
