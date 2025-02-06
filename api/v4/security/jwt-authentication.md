# Resource Protection using JWT in ASP.NET Core

This provider lets you control access to REST resources by using JSON Web Tokens (JWT) issued by Cloud Foundry Security services (such as [UAA Server](https://github.com/cloudfoundry/uaa) or [Single-Sign-on for VMware Tanzu](https://docs.vmware.com/en/Single-Sign-On-for-VMware-Tanzu-Application-Service/index.html)) in ASP.NET Core.

The [Steeltoe Security samples](https://github.com/SteeltoeOSS/Samples/blob/latest/Security/src/AuthClient/README.md) can help you understand how to use this tool.

Many resources are available for understanding JWT (for example, see [JWT IO](https://jwt.io/) or [JSON Web Token](https://en.wikipedia.org/wiki/JSON_Web_Token)).

## Usage

This library supplements ASP.NET Security. For the documentation from Microsoft, visit [ASP.NET Core Security](https://learn.microsoft.com/aspnet/core/security).

This package uses JSON Web Tokens (JWT) and builds on JWT Security services provided by ASP.NET Core Security. You should take some time to understand both before proceeding to use this provider.

Steps involved in using this library:

1. Add NuGet reference(s)
1. Configure settings for the security provider
1. Add and use the security provider in the application
1. Secure your endpoints
1. Create an instance of a Cloud Foundry Single SignOn service and bind it to your application

### Add NuGet Reference

To use the provider, you need to add a reference to the `Steeltoe.Security.Authentication.JwtBearer` NuGet package.

### Configure Settings

Since Steeltoe's OpenID Connect library configures Microsoft's JWT Bearer implementation, all available settings can be found in [`Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerOptions`](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.authentication.jwtbearer.jwtbeareroptions)

`JwtBearerOptions` is bound to configuration values found under `Authentication:Schemes:Bearer`. The following example shows how to declare the audience for which tokens should be considered valid (such as when a token is issued to a specific web application and then passed to backend services to perform actions on behalf of a user):

```json
{
  "Authentication": {
    "Schemes": {
      "Bearer": {
        "Audience": "steeltoesamplesclient"
      }
    }
  }
}
```

#### Cloud Foundry Service Bindings

The Steeltoe package `Steeltoe.Configuration.CloudFoundry.ServiceBinding` reads Single SignOn credentials from Cloud Foundry service bindings (`VCAP_SERVICES`) and re-maps them for Microsoft's JwtBearer to read. Add the configuration provider to your application with this code:

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
      "Bearer": {
        "Authority": "http://localhost:8080/uaa",
        "ClientId": "steeltoesamplesserver",
        "ClientSecret": "server_secret",
        "MetadataAddress": "http://localhost:8080/.well-known/openid-configuration",
        "RequireHttpsMetadata": false
      }
    }
  }
}
```

#### Add and use JWT Bearer Authentication

Since the majority of the JWT Bearer functionality is provided by Microsoft's libraries, the only difference when using Steeltoe will be the addition of calling `ConfigureOpenIdConnectForCloudFoundry` on the `AuthenticationBuilder`, as shown in the following example:

```csharp
using Steeltoe.Security.Authentication.CloudFoundry;

// Add Microsoft Authentication services
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer()
    // Steeltoe: configure JWT to work with UAA/Cloud Foundry
    .ConfigureJwtBearerForCloudFoundry();

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
...

[Route("api/[controller]")]
public class ValuesController : Controller
{
    // GET api/values
    [HttpGet]
    [Authorize(Policy = Globals.RequiredJwtScope)]
    public IEnumerable<string> Get()
    {
        return new string[] { "value1", "value2" };
    }
}
```

In the preceding example, if an incoming REST request is made to the `api/values` endpoint and the request does not contain a valid JWT bearer token with a `scope` claim equal to `sampleapi.read`, the request is rejected.

### Cloud Foundry Single SignOn Service

There are two services available on Cloud Foundry. We recommend you read the official documentation ([UAA Server](https://github.com/cloudfoundry/uaa) and [Single-Sign-on for VMware Tanzu](https://docs.vmware.com/en/Single-Sign-On-for-VMware-Tanzu-Application-Service/index.html)) or follow the instructions included in the [Steeltoe Security samples](https://github.com/SteeltoeOSS/Samples/blob/latest/Security/src/AuthClient/README.md) for more information.
