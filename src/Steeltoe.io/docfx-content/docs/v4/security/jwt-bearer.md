# Resource Protection using JWT in ASP.NET Core

This library is a supplement to ASP.NET Core Security, adding functionality that helps you use Cloud Foundry Security services such as [Single Sign-On for VMware Tanzu](https://techdocs.broadcom.com/us/en/vmware-tanzu/platform-services/single-sign-on-for-tanzu/1-16/sso-tanzu/index.html) or a [UAA Server](https://github.com/cloudfoundry/uaa) for authentication and authorization using JSON Web Tokens (JWT) in ASP.NET Core web applications.

The [Steeltoe Security samples](https://github.com/SteeltoeOSS/Samples/blob/main/Security/src/AuthWeb/README.md) can help you understand how to use this tool.

General guidance on JWT is beyond the scope of this document and can be found in many other sources (for example, see [Wikipedia](https://en.wikipedia.org/wiki/JSON_Web_Token) or [JWT IO](https://jwt.io/)).

For the documentation of the underlying Microsoft Jwt Bearer Authentication library, visit the [official documentation](https://learn.microsoft.com/aspnet/core/security/authentication/configure-jwt-bearer-authentication).

## Usage

Steps involved in using this library:

1. Add NuGet references.
1. Configure settings for the security provider.
1. Add and use the security provider in the application.
1. Secure your endpoints.
1. Create an instance of a Cloud Foundry Single Sign-On service and bind it to your application.

### Add NuGet References

To use the provider, you need to add a reference to the `Steeltoe.Security.Authentication.JwtBearer` NuGet package.

Also add a reference to `Steeltoe.Configuration.CloudFoundry`, so that Cloud Foundry service bindings can be read by Steeltoe.

### Configure Settings

Since Steeltoe's JWT Bearer library configures Microsoft's JWT Bearer implementation, all available settings can be found in [`Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerOptions`](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.authentication.jwtbearer.jwtbeareroptions).

`JwtBearerOptions` is bound to configuration values found under `Authentication:Schemes:Bearer`. The following example `appsettings.json` shows how to declare the audience for which tokens should be considered valid (such as when a token is issued to a specific web application and then passed to backend services to perform actions on behalf of a user):

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

The Steeltoe package `Steeltoe.Configuration.CloudFoundry` reads Single Sign-On credentials from Cloud Foundry service bindings (`VCAP_SERVICES`) and re-maps them for Microsoft's JWT Bearer library to read. Add the configuration provider to your application with this code:

```csharp
using Steeltoe.Configuration.CloudFoundry;
using Steeltoe.Configuration.CloudFoundry.ServiceBindings;

var builder = WebApplication.CreateBuilder(args);

// Steeltoe: Add Cloud Foundry application and service info to configuration.
builder.AddCloudFoundryConfiguration();
builder.Configuration.AddCloudFoundryServiceBindings();
```

#### Local UAA

A UAA server (such as [UAA Server for Steeltoe samples](https://github.com/SteeltoeOSS/Dockerfiles/tree/main/uaa-server)), can be used for local development of applications that will be deployed to Cloud Foundry. Configuration of UAA itself is beyond the scope of this documentation, but configuring your `appsettings.Development.json` to work with a local UAA server is possible with the addition of settings like these:

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

> [!IMPORTANT]
> If you wish to use the Steeltoe UAA server without modification, some application configuration options will be limited.
> The Steeltoe UAA Server configuration can be found in [uaa.yml](https://github.com/SteeltoeOSS/Dockerfiles/blob/main/uaa-server/uaa.yml#L111).

### Add and use JWT Bearer Authentication

Since the majority of the JWT Bearer functionality is provided by Microsoft's libraries, the only difference when using Steeltoe will be the addition of calling `ConfigureJwtBearerForCloudFoundry` on the `AuthenticationBuilder`, as shown in the following example:

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Steeltoe.Security.Authentication.JwtBearer;

// Add Microsoft Authentication services
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer()
    // Steeltoe: configure JWT to work with UAA/Cloud Foundry
    .ConfigureJwtBearerForCloudFoundry();

// Register Microsoft Authorization services
builder.Services.AddAuthorizationBuilder()
    // Create a named authorization policy that requires the user to have a scope with the same value
    .AddPolicy("sampleapi.read", policy => policy.RequireClaim("scope", "sampleapi.read"));
```

Activate authentication and authorization services _after_ routing services, but _before_ controller route registrations, with the following code:

```csharp
var app = builder.Build();

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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class SampleController : ControllerBase
{

    [HttpGet]
    [Authorize(Policy = "sampleapi.read")]
    public string Get()
    {
        return "You have permission to read from the sample API.";
    }
}
```

In the preceding example, if an incoming GET request is made to the `/api/sample` endpoint and the request does not contain a valid JWT bearer token for a user with a `scope` claim equal to `sampleapi.read`, the request is rejected.

Review the [Steeltoe Security samples](https://github.com/SteeltoeOSS/Samples/blob/main/Security/src/AuthWeb/README.md) for a demonstration of using a user's access token to interact with downstream APIs, focusing on these locations:

- [Configure ASP.NET Core to save the user's token](https://github.com/SteeltoeOSS/Samples/blob/main/Security/src/AuthWeb/appsettings.json#L15)
- [Get the user's token](https://github.com/SteeltoeOSS/Samples/blob/main/Security/src/AuthWeb/Controllers/HomeController.cs#L60)
- [Include the token in a downstream request](https://github.com/SteeltoeOSS/Samples/blob/main/Security/src/AuthWeb/ApiClients/JwtAuthorizationApiClient.cs#L24)

### Single Sign-On for VMware Tanzu

When using Single Sign-On for VMware Tanzu, you will need to identify the service plan to be used before creating a service instance of that plan.
If you do not have an existing service plan, a platform operator may need to create a new plan for you.
The operator should refer to the [Single Sign-On for Tanzu operator guide](https://techdocs.broadcom.com/us/en/vmware-tanzu/platform-services/single-sign-on-for-tanzu/1-16/sso-tanzu/operator-index.html) for information on how to configure plans for developer use.

Once you have identified the service plan that will be used, create a service instance:

```shell
cf create-service p-identity SERVICE_PLAN_NAME MY_SERVICE_INSTANCE
```

#### Bind and configure with app manifest

Using a manifest file when you deploy to Cloud Foundry is recommended, and can save some work with the SSO configuration. Review the Single Sign-On documentation for [configuring an app manifest](https://techdocs.broadcom.com/us/en/vmware-tanzu/platform-services/single-sign-on-for-tanzu/1-16/sso-tanzu/config-apps.html#configure-app-manifest).

Consider this example manifest that names the application, buildpack and configures several properties for the SSO binding:

```yml
applications:
- name: steeltoesamplesserver
  buildpacks:
  - dotnet_core_buildpack
  env:
    GRANT_TYPE: client_credentials
    SSO_AUTHORITIES: uaa.resource, sampleapi.read
    SSO_RESOURCES: sampleapi.read
    SSO_SHOW_ON_HOME_PAGE: "false"
  services:
  - sampleSSOService
```

#### Bind and configure manually

Alternatively, you can manually bind the instance, restage the app with the Cloud Foundry CLI and later configure the SSO binding yourself with the web interface:

```shell
# Bind service to your app
cf bind-service MY_APPLICATION MY_SERVICE_INSTANCE

# Restage the app to pick up change
cf restage MY_APPLICATION
```

For further information, refer to the [Single Sign-On for Tanzu developer guide](https://techdocs.broadcom.com/us/en/vmware-tanzu/platform-services/single-sign-on-for-tanzu/1-16/sso-tanzu/developer-index.html) or follow the instructions included in the [Steeltoe Security samples](https://github.com/SteeltoeOSS/Samples/blob/main/Security/src/AuthWeb/README.md).

### UAA Server

If Single Sign-On for Tanzu is not available or desired for your application, you can use UAA as an alternative.

There is no service broker available to manage service instances or bindings for UAA, so a [user provided service instance](https://docs.cloudfoundry.org/devguide/services/user-provided.html) must be used to hold the credentials.

The following command is an example of how the binding could be created:

```shell
cf cups MY_SERVICE_INSTANCE -p '{"auth_domain": "https://uaa.login.sys.cf-app.com","grant_types": [ "authorization_code", "client_credentials" ],"client_secret": "SOME_CLIENT_SECRET","client_id": "SOME_CLIENT_ID"}'
```

And the command below is an example of how to bind the service instance to the app:

```shell
cf bind-service MY_APPLICATION MY_SERVICE_INSTANCE
```

For additional information, refer to the [UAA documentation](https://docs.cloudfoundry.org/concepts/architecture/uaa.html).
