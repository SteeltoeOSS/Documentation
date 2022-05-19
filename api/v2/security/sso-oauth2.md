# Single Sign-on with OAuth2

Single Sign-on with OAuth 2.0 enables you to leverage existing credentials configured in a [UAA Server](https://github.com/cloudfoundry/uaa) or [TAS Single-Sign-on service](https://docs.pivotal.io/p-identity) for authentication and authorization in ASP.NET Core applications. Single signon functionality for ASP.NET 4.x applications is available with the OpenID Connect provider.

In addition to the Quick Start, you can use other Steeltoe sample applications to help you understand how to use this provider, including:

* `FreddysBBQ`: A polyglot microservices-based sample application showing interoperability between Java and .NET on Cloud Foundry, secured with OAuth2 Security Services, and using Spring Cloud Services.

## Usage

This package is built on the OAuth 2 authentication flow and the services provided by ASP.NET Core Security. You should take some time to understand both before proceeding to use this provider.

Many resources are available for understanding OAuth 2. For example, see [Introduction to OAuth 2](https://www.digitalocean.com/community/tutorials/an-introduction-to-oauth-2) or [Understanding OAuth 2](https://www.bubblecode.net/en/2016/01/22/understanding-oauth2/).

To get a good understanding of ASP.NET Core Security, review the [documentation](https://docs.microsoft.com/aspnet/core/security) provided by Microsoft. If you are upgrading an application from ASP.NET Core 1.x, you may also want to review [Migrating Auth and Identity to ASP.NET Core 2.0](https://docs.microsoft.com/aspnet/core/migration/1x-to-2x/identity-2x).

Additionally, you should know how the [.NET Configuration service](https://docs.asp.net/en/latest/fundamentals/configuration.html) and the `ConfigurationBuilder` work and how to add providers to the builder.

You should also know how the ASP.NET Core [Startup](https://docs.asp.net/en/latest/fundamentals/startup.html) class is used in configuring the application services and how the middleware used in the application. Pay particular attention to the usage of the `Configure()` and `ConfigureService())` methods.

With regard to Cloud Foundry, you should know how Cloud Foundry OAuth2 security services (for example, [UAA Server](https://github.com/cloudfoundry/uaa) or [TAS Single Signon](https://docs.pivotal.io/p-identity/)) work.

In order to use the security provider:

1. Create an instance of a Cloud Foundry OAuth2 service and bind it to your application.
1. (Optional) Configure any additional settings the security provider needs.
1. Add the Cloud Foundry configuration provider to the `ConfigurationBuilder`.
1. Add and use the security provider in the application.
1. Secure your endpoints.

### Add NuGet Reference

To use the provider, add a reference to the Steeltoe Cloud Foundry Security NuGet.

The provider can be found in the `Steeltoe.Security.Authentication.CloudFoundryCore` package.

You can add the provider to your project by using the following `PackageReference`:

```xml
<ItemGroup>
...
    <PackageReference Include="Steeltoe.Security.Authentication.CloudFoundryCore" Version="2.5.2" />
...
</ItemGroup>
```

### Configure Settings

Configuring settings for the provider beyond what is provided in a service binding is not typically required, but when Cloud Foundry is using self-signed certificates, you might need to disable certificate validation, as shown in the following example:

```json
{
  "security": {
    "oauth2": {
      "client": {
        "validateCertificates": false
      }
    }
  }
}
```

The samples and most templates are already set up to read from `appsettings.json`.

The Steeltoe OAuth2 security provider options are based on [`Microsoft.AspNetCore.Authentication.OAuth.OAuthOptions`](https://docs.microsoft.com/dotnet/api/microsoft.aspnetcore.authentication.oauth.oauthoptions), with these additional properties:

|Name|Description|Default|
|---|---|---|
|additionalScopes|Scopes to request for tokens in addition to `openid`|`string.Empty`|
|validateCertificates|Validate Auth server certificate|`true`|

**Note**: **Each setting above must be prefixed with `security:oauth2:client`**.

### Cloud Foundry

As mentioned earlier, there are two OAuth-compatible services available on Cloud Foundry. We recommend you read the offical documentation ([UAA Server](https://github.com/cloudfoundry/uaa) and [TAS SSO](https://docs.pivotal.io/p-identity/1-5/getting-started.html)) or follow the instructions included in the samples for [UAA Server](https://github.com/SteeltoeOSS/Samples/blob/master/Security/src/AspDotNetCore/CloudFoundrySingleSignon/README.md) and [TAS SSO](https://github.com/SteeltoeOSS/Samples/blob/master/Security/src/AspDotNetCore/CloudFoundrySingleSignon/README-SSO.md) to quickly learn how to create and bind OAuth2 services.

Regardless of which provider you choose, once the service is bound to your application, the settings are available in `VCAP_SERVICES`.

### Add Cloud Foundry OAuth

As with other ASP.NET Core middleware, in order to configure the Cloud Foundry OAuth provider in your application,
first add and configure it in the `ConfigureServices()` method of the `Startup` class, then use it in the `Configure()`
method of the `Startup` class. The Cloud Foundry OAuth provider is built on top of ASP.NET Core Authentication services
and is configured with an extension method on the `AuthenticationBuilder`, as seen in the following example:

```csharp
using Steeltoe.Security.Authentication.CloudFoundry;

public class Startup {
    ...
    public IConfiguration Configuration { get; private set; }
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }
    public void ConfigureServices(IServiceCollection services)
    {
        ...
        services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CloudFoundryDefaults.AuthenticationScheme;
            })
            .AddCookie((options) =>
            {
                // set values like login url, access denied path, etc here
                options.AccessDeniedPath = new PathString("/Home/AccessDenied");
            })
            .AddCloudFoundryOAuth(Configuration); // Add Cloud Foundry authentication service
        ...
    }
    public void Configure(IApplicationBuilder app, ...)
    {
        ...
        // Use the protocol from the original request when generating redirect uris
        // (eg: when TLS termination is handled by an appliance in front of the app)
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedProto
        });

        // Add authentication middleware to pipeline
        app.UseAuthentication();
    }
    ...
```

The `AddCloudFoundryOAuth(Configuration)` method call configures and adds the Cloud Foundry OAuth authentication service to the service container. Once in place, it can be used by the authentication middleware during request processing.

>NOTE: When running behind a reverse-proxy (like Gorouter or HAProxy) that handles TLS termination for your app, use `app.UseForwardedHeaders` to generate the correct redirect URI so that the user is not sent back over HTTP instead of HTTPS after authenticating.

### Securing Endpoints

Once the `Startup` class has been updated, you can secure endpoints with the standard ASP.NET Core `Authorize` attribute, as shown in the following example:

```csharp
using Microsoft.AspNetCore.Authentication;
...
public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    [Authorize]
    public IActionResult About()
    {
        ViewData["Message"] = "Your About page.";
        return View();
    }

    [Authorize(Policy = "testgroup1")]
    public IActionResult Contact()
    {
        ViewData["Message"] = "Your contact page.";

        return View();
    }
    ...
}
```

The preceding example code establishes the following security rules:

* If a user attempts to access the `About` action and the user is not authenticated, then the user is redirected to the OAuth2 server (such as a UAA Server) to login.
* If an authenticated user attempts to access the `Contact` action but does not meet the restrictions established by the policy `testgroup1`, the user is denied access.

>TIP: See the Microsoft documentation on [ASP.NET Core Authorization](https://docs.microsoft.com/aspnet/core/security/authorization/introduction).
