# Single Sign-On with OpenID Connect

OpenID Connect is commonly used when users should be able to interact with a collection of applications using a single set of credentials for authentication and authorization.

This library is a supplement to ASP.NET Core Security's OpenID Connect library (`Microsoft.AspNetCore.Authentication.OpenIdConnect`), adding functionality that helps you use Cloud Foundry Security services such as [Single Sign-On for VMware Tanzu](https://techdocs.broadcom.com/us/en/vmware-tanzu/platform-services/single-sign-on-for-tanzu/1-16/sso-tanzu/index.html) or [User Account and Authentication (UAA) Server](https://github.com/cloudfoundry/uaa).

General guidance on OpenID Connect is beyond the scope of this document and can be found in many other sources (for example, see [OpenID](https://openid.net/developers/how-connect-works/)).
For the documentation of the underlying Microsoft OpenID Connect library, visit the [official documentation](https://learn.microsoft.com/aspnet/core/security/authentication/configure-oidc-web-authentication).

The [Steeltoe Security samples](https://github.com/SteeltoeOSS/Samples/blob/main/Security/src/AuthWeb/README.md) can help you understand how to use this library.

## Usage

Steps involved in using this library:

1. Add NuGet references.
1. Configure settings for the security provider.
1. Add and use the security provider in the application.
1. Secure your endpoints.
1. Create an instance of a Cloud Foundry Single Sign-On service and bind it to your application.

### Add NuGet References

To use this package, you will need to add a reference to the NuGet package `Steeltoe.Security.Authentication.OpenIdConnect`.

Also add a reference to `Steeltoe.Configuration.CloudFoundry`, so that Cloud Foundry service bindings can be read by Steeltoe.

### Configure Settings

Since Steeltoe's OpenID Connect library configures Microsoft's OpenID Connect implementation, all available settings can be found in [`Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectOptions`](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.authentication.openidconnect.openidconnectoptions)

`OpenIdConnectOptions` is bound to configuration values found under `Authentication:Schemes:OpenIdConnect`. The following example `appsettings.json` shows how to declare a list of permissions that should be requested for users:

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
using Steeltoe.Configuration.CloudFoundry;
using Steeltoe.Configuration.CloudFoundry.ServiceBindings;

var builder = WebApplication.CreateBuilder(args);

// Steeltoe: Add Cloud Foundry application and service info to configuration.
builder.AddCloudFoundryConfiguration();
builder.Configuration.AddCloudFoundryServiceBindings();
```

#### Local UAA

A UAA server (such as [UAA Server for Steeltoe samples](https://github.com/SteeltoeOSS/Dockerfiles/tree/main/uaa-server)) can be used for local development of applications that will be deployed to Cloud Foundry. Configuration of UAA itself is beyond the scope of this documentation, but configuring your `appsettings.Development.json` to work with a local UAA server is possible with the addition of settings like these:

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

> [!IMPORTANT]
> If you wish to use the Steeltoe UAA server without modification, some application configuration options will be very limited.
> Because the OpenID Connect authentication flow requires user redirection to known locations, the client `steeltoesamplesclient` is expected to listen at <https://localhost:7072>, so you will need to update launchSettings.json accordingly.
> The Steeltoe UAA Server configuration can be found in [uaa.yml](https://github.com/SteeltoeOSS/Dockerfiles/blob/main/uaa-server/uaa.yml#L116).

### Add and use OpenID Connect

Since the majority of the OpenID Connect functionality is provided by Microsoft's libraries, the only difference when using Steeltoe will be the addition of calling `ConfigureOpenIdConnectForCloudFoundry` on the `AuthenticationBuilder`, as shown in the following example:

```csharp
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Steeltoe.Security.Authentication.OpenIdConnect;

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
    // which represents the user's permission to read from the sample API
    .AddPolicy("sampleapi.read", policy => policy.RequireClaim("scope", "sampleapi.read"));
```

Activate authentication and authorization services _after_ routing services, but _before_ controller route registrations, with the following code:

```csharp
using Microsoft.AspNetCore.HttpOverrides;

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

public class HomeController : Controller
{
    [Authorize]
    public IActionResult Privacy()
    {
        return View();
    }

    [Authorize(Policy = "sampleapi.read")]
    public string TestGroup()
    {
        return "You have the 'sampleapi.read' permission.";
    }
}
```

The preceding example establishes the following security rules:

* If a user attempts to access the `Privacy` action and the user is not authenticated, the user is redirected to the OAuth server (such as a UAA Server) to log in.
* If an authenticated user attempts to access the `TestGroup` action but does not meet the restrictions established by the referenced policy, the user is denied access.

> [!TIP]
> The Steeltoe UAA server has several pre-provisioned user accounts. Sign in with `testuser` and password `password` to access resources secured with `sampleapi.read`
> To test with a user that does not have the permission `sampleapi.read`, sign in with the user `customer` and password `password`.
>
> You may want to explore the [Steeltoe Security samples](https://github.com/SteeltoeOSS/Samples/blob/main/Security/src/AuthWeb/README.md) for examples of additional basic functionality such as signing out of the application.

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
- name: steeltoesamplesclient
  buildpacks:
  - dotnet_core_buildpack
  env:
    GRANT_TYPE: authorization_code, client_credentials
    SSO_AUTHORITIES: uaa.resource, sampleapi.read
    SSO_IDENTITY_PROVIDERS: steeltoe-uaa
    SSO_SCOPES: openid, profile, sampleapi.read
    SSO_SHOW_ON_HOME_PAGE: "true"
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

For further information, such as instructions on using the web interface, refer to the [Single Sign-On for Tanzu developer guide](https://techdocs.broadcom.com/us/en/vmware-tanzu/platform-services/single-sign-on-for-tanzu/1-16/sso-tanzu/developer-index.html) or follow the instructions included in the [Steeltoe Security samples](https://github.com/SteeltoeOSS/Samples/blob/main/Security/src/AuthWeb/README.md).

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
