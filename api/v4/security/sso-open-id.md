# Single Sign-On with OpenID Connect

This provider helps you use a [UAA Server](https://github.com/cloudfoundry/uaa) or [Single Sign-On for VMware Tanzu](https://techdocs.broadcom.com/us/en/vmware-tanzu/platform-services/single-sign-on-for-tanzu/1-16/sso-tanzu/index.html) for authentication and authorization using OpenID Connect in ASP.NET Core applications.

The [Steeltoe Security samples](https://github.com/SteeltoeOSS/Samples/blob/latest/Security/src/AuthClient/README.md) can help you understand how to use this library. Another reference application is the [FreddysBBQ](https://github.com/SteeltoeOSS/Samples/blob/latest/FreddysBBQ) sample, which is a polyglot microservices-based sample showing interoperability between Java and .NET on Cloud Foundry, secured with OAuth2 Security Services, and using Spring Cloud Services.

General understanding of OpenID Connect is beyond the scope of this document, many other sources exist online (for example, see [OpenID](https://openid.net/developers/how-connect-works/)).

## Usage

This library supplements ASP.NET Security. For the documentation from Microsoft, visit [ASP.NET Core Security](https://learn.microsoft.com/aspnet/core/security).

Steps involved in using this library:

1. Add NuGet reference
1. Configure settings for the security provider
1. Add and use the security provider in the application
1. Secure your endpoints
1. Create an instance of a Cloud Foundry Single Sign-On service and bind it to your application

### Add NuGet Reference

To use this package, you will need to add a reference to the NuGet package `Steeltoe.Security.Authentication.OpenIdConnect`.

If you are using Cloud Foundry service bindings, you will also need to add a reference to `Steeltoe.Configuration.CloudFoundry`.

### Configure Settings

Since Steeltoe's OpenID Connect library configures Microsoft's OpenID Connect implementation, all available settings can be found in [`Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectOptions`](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.authentication.openidconnect.openidconnectoptions)

`OpenIdConnectOptions` is bound to configuration values found under `Authentication:Schemes:OpenIdConnect`. The following example shows how to declare a list of permissions that should be requested for users:

```json
{
  // Configure OpenID Connect to request specific scopes (permissions)
  "Authentication": {
    "Schemes": {
      "OpenIdConnect": {
        "Scope": [ "openid", "sampleapi.read" ]
      }
    }
  }
}
```

#### Cloud Foundry Service Bindings

The Steeltoe package `Steeltoe.Configuration.CloudFoundry` reads Single Sign-On credentials from Cloud Foundry service bindings (`VCAP_SERVICES`) and re-maps them for Microsoft's OpenID Connect to read. Add the configuration provider to your application with this code:

```csharp
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Steeltoe: Add Cloud Foundry service info to configuration
builder.Configuration.AddCloudFoundryServiceBindings();
```

#### Local UAA

A UAA server (such as [UAA Server for Steeltoe samples](https://github.com/SteeltoeOSS/Dockerfiles/tree/main/uaa-server)), can be used for local development of applications that will be deployed to Cloud Foundry. Configuration of UAA itself is beyond the scope of this documentation, but configuring your application to work with a local UAA server is possible with settings like these:

```json
{
  "Authentication": {
    "Schemes": {
      "OpenIdConnect": {
        "Authority": "http://localhost:8080/uaa",
        "ClientId": "steeltoesamplesclient",
        "ClientSecret": "client_secret",
        "MetadataAddress": "http://localhost:8080/.well-known/openid-configuration",
        "RequireHttpsMetadata": false
      }
    }
  }
}
```

### Add and use OpenID Connect

Since the majority of the OpenID Connect functionality is provided by Microsoft's libraries, the only difference when using Steeltoe will be the addition of calling `ConfigureOpenIdConnectForCloudFoundry` on the `AuthenticationBuilder`, as shown in the following example:

```csharp
// Add Microsoft Authentication services
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.AccessDeniedPath = new PathString("/Home/AccessDenied");
    })
    .AddOpenIdConnect()
    // Steeltoe: configure OpenID Connect to work with UAA/Cloud Foundry
    .ConfigureOpenIdConnectForCloudFoundry();

// Register Microsoft Authorization services
builder.Services.AddAuthorizationBuilder()
    // Create a named authorization policy that requires the user to have a scope with the same value
    // In the Steeltoe sample application, Globals.RequiredJwtScope = "sampleapi.read",
    // which represents the user's permission to read from the sample API
    .AddPolicy(Globals.RequiredJwtScope, policy => policy.RequireClaim("scope", Globals.RequiredJwtScope))
```

Activate authentication and authorization services after routing services, but before controller route registrations, with the following code:

```csharp
WebApplication app = builder.Build();

// Use forwarded headers so that links generate correctly behind a reverse proxy (eg: when in Cloud Foundry)
app.UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedProto });

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultControllerRoute();

app.Run();
```

> [!TIP]
> In the sample code above, `app.UseForwardedHeaders` is used so that any links generated within the application will be compatible with reverse-proxy scenarios, such as when running in Cloud Foundry.

### Securing Endpoints

Once the services and middleware have been configured, you can secure endpoints with the standard ASP.NET Core `Authorize` attribute, as follows:

```csharp
using Microsoft.AspNetCore.Authentication;

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

    [Authorize(Policy = Globals.RequiredJwtScope)]
    public IActionResult TestGroup()
    {
        ViewData["Message"] = $"You have the '{Globals.RequiredJwtScope}' permission.";
        return View();
    }
}
```

The preceding example establishes the following security rules:

* If a user attempts to access the `About` action and the user is not authenticated, the user is redirected to the OAuth server (such as a UAA Server) to log in.
* If an authenticated user attempts to access the `TestGroup` action but does not meet the restrictions established by the referenced policy, the user is denied access.

### Cloud Foundry Single Sign-On Service

There are two services available on Cloud Foundry. We recommend you read the official documentation ([UAA Server](https://github.com/cloudfoundry/uaa) and [Single Sign-On for VMware Tanzu](https://techdocs.broadcom.com/us/en/vmware-tanzu/platform-services/single-sign-on-for-tanzu/1-16/sso-tanzu/index.html)) or follow the instructions included in the [Steeltoe Security samples](https://github.com/SteeltoeOSS/Samples/blob/latest/Security/src/AuthClient/README.md) for more information.
