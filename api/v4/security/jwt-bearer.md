# Resource Protection using JWT in ASP.NET Core

This library is a supplement to ASP.NET Security, adding functionality that helps you use Cloud Foundry Security services such as [Single Sign-On for VMware Tanzu](https://techdocs.broadcom.com/us/en/vmware-tanzu/platform-services/single-sign-on-for-tanzu/1-16/sso-tanzu/index.html) or a [UAA Server](https://github.com/cloudfoundry/uaa) for authentication and authorization using JSON Web Tokens (JWT) in ASP.NET Core web applications.

The [Steeltoe Security samples](https://github.com/SteeltoeOSS/Samples/blob/latest/Security/src/AuthClient/README.md) can help you understand how to use this tool.

General guidance on JWT is beyond the scope of this document and can be found in many other sources (for example, see [Wikipedia](https://en.wikipedia.org/wiki/JSON_Web_Token) or [JWT IO](https://jwt.io/)).

For the documentation of the underlying Microsoft OpenID Connect library, visit [ASP.NET Core Security](https://learn.microsoft.com/aspnet/core/security).

## Usage

Steps involved in using this library:

1. Add NuGet reference
1. Configure settings for the security provider
1. Add and use the security provider in the application
1. Secure your endpoints
1. Create an instance of a Cloud Foundry Single Sign-On service and bind it to your application

### Add NuGet Reference

To use the provider, you need to add a reference to the `Steeltoe.Security.Authentication.JwtBearer` NuGet package.

If you are using Cloud Foundry service bindings, you will also need to add a reference to `Steeltoe.Configuration.CloudFoundry`.

### Configure Settings

Since Steeltoe's Jwt Bearer library configures Microsoft's JWT Bearer implementation, all available settings can be found in [`Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerOptions`](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.authentication.jwtbearer.jwtbeareroptions)

`JwtBearerOptions` is bound to configuration values found under `Authentication:Schemes:Bearer`. The following example shows how to declare the audience for which tokens should be considered valid (such as when a token is issued to a specific web application and then passed to backend services to perform actions on behalf of a user):

```json
{
  "Authentication": {
    "Schemes": {
      "Bearer": {
        "ValidAudience": "sampleapi"
      }
    }
  }
}
```

#### Cloud Foundry Service Bindings

The Steeltoe package `Steeltoe.Configuration.CloudFoundry` reads Single Sign-On credentials from Cloud Foundry service bindings (`VCAP_SERVICES`) and re-maps them for Microsoft's JwtBearer library to read. Add the configuration provider to your application with this code:

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

### Add and use JWT Bearer Authentication

Since the majority of the JWT Bearer functionality is provided by Microsoft's libraries, the only difference when using Steeltoe will be the addition of calling `ConfigureJwtBearerForCloudFoundry` on the `AuthenticationBuilder`, as shown in the following example:

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

Activate authentication and authorization services _after_ routing services, but _before_ controller route registrations, with the following code:

```csharp
WebApplication app = builder.Build();

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

### Single Sign-On for VMware Tanzu

When using Single Sign-On for VMware Tanzu, you will need to identify the service plan to be used before creating a service instance of that plan.
If you do not have an existing service plan, a platform operator may need to create a new plan for you.
The operator should refer to the [Single Sign-On for Tanzu operator guide](https://techdocs.broadcom.com/us/en/vmware-tanzu/platform-services/single-sign-on-for-tanzu/1-16/sso-tanzu/operator-index.html) for information on how to configure plans for developer use.

Once you have identified the service plan that will be used, create a service instance:

```shell
cf create-service p-identity SERVICE_PLAN_NAME MY_SERVICE_INSTANCE
```

If using a manifest file, [add a service binding reference](https://docs.cloudfoundry.org/devguide/deploy-apps/manifest-attributes.html#services-block), otherwise bind the instance and restage the app with the cf cli:

```shell
# Bind service to your app
cf bind-service MY_APPLICATION MY_SERVICE_INSTANCE

# Restage the app to pick up change
cf restage MY_APPLICATION
```

For further information, refer to the [Single Sign-On for Tanzu developer guide](https://techdocs.broadcom.com/us/en/vmware-tanzu/platform-services/single-sign-on-for-tanzu/1-16/sso-tanzu/developer-index.html) or follow the instructions included in the [Steeltoe Security samples](https://github.com/SteeltoeOSS/Samples/blob/latest/Security/src/AuthWeb/README.md).

### UAA Server

If Single Sign-On for Tanzu is not available or desired for your application, you can use UAA as an alternative.

There is no service broker available to manage service instances or bindings for UAA, so a [user provided service instance](https://docs.cloudfoundry.org/devguide/services/user-provided.html) should be used to hold the credentials.

This command is an example of how the binding could be created:

```shell
cf cups MY_SERVICE_INSTANCE -p '{"auth_domain": "https://uaa.login.sys.cf-app.com","grant_types": [ "authorization_code", "client_credentials" ],"client_secret": "SOME_CLIENT_SECRET","client_id": "SOME_CLIENT_ID"}'
```

And this command is an example of how to bind the service instance to the app:

```shell
cf bind-service MY_APPLICATION MY_SERVICE_INSTANCE
```

For additional information, refer to the [UAA documentation](https://docs.cloudfoundry.org/concepts/architecture/uaa.html).
