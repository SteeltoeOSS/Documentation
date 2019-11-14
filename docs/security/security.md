Steeltoe provides a number of Security related services that simplify using Cloud Foundry based security services in ASP.NET applications.

These providers enable the use of Cloud Foundry security services (such as [UAA Server](https://github.com/cloudfoundry/uaa) and/or [Pivotal Single Sign-on](https://docs.pivotal.io/p-identity/)) for Authentication and Authorization.

You can choose from several providers when adding Cloud Foundry security integration:

* [OAuth2 Single Sign-on with Cloud Foundry Security services](#1-0-oauth2-single-sign-on) - ASP.NET (MVC, WebAPI), ASP.NET Core.
* [Using JWT tokens issued by Cloud Foundry for securing resources/endpoints](#2-0-resource-protection-using-jwt) - ASP.NET (MVC, WebAPI, WCF), ASP.NET Core.

In addition to Authentication and Authorization providers, Steeltoe Security offers:

* A security provider for [using Redis on Cloud Foundry with ASP.NET Core Data Protection Key Ring storage](#3-0-redis-key-storage-provider).
* A [CredHub API Client for .NET applications](#4-0-credhub-api-client) to perform credential storage, retrieval and generation.

>NOTE: Depending on your hosting environment, service instances you create for the purpose of exploring the Quick Starts on this page may have a cost associated.

# Single Sign-on with OAuth2

Single Sign-on with OAuth 2.0 enables you to leverage existing credentials configured in a [UAA Server](https://github.com/cloudfoundry/uaa) or [Pivotal Single Sign-on service](https://docs.pivotal.io/p-identity) for authentication and authorization in ASP.NET Core applications. Single signon functionality for ASP.NET 4.x applications is available with the [OpenID Connect provider](#2-0.single-sign-on-with-openid-connect).

In addition to the Quick Start, you can use other Steeltoe sample applications to help you understand how to use this provider, including:

* [FreddysBBQ](https://github.com/SteeltoeOSS/Samples/tree/master/FreddysBBQ): A polyglot microservices-based sample application showing interoperability between Java and .NET on Cloud Foundry, secured with OAuth2 Security Services, and using Spring Cloud Services.

The source code for this provider can be found [here](https://github.com/SteeltoeOSS/Security).

## Usage

This package is built on the OAuth 2 authentication flow and the services provided by ASP.NET Core Security. You should take some time to understand both before proceeding to use this provider.

Many resources are available for understanding OAuth 2. For example, see [Introduction to OAuth 2](https://www.digitalocean.com/community/tutorials/an-introduction-to-oauth-2) or [Understanding OAuth 2](https://www.bubblecode.net/en/2016/01/22/understanding-oauth2/).

To get a good understanding of ASP.NET Core Security, review the [documentation](https://docs.microsoft.com/en-us/aspnet/core/security) provided by Microsoft. If you are upgrading an application from ASP.NET Core 1.x, you may also want to review [Migrating Auth and Identity to ASP.NET Core 2.0](https://docs.microsoft.com/en-us/aspnet/core/migration/1x-to-2x/identity-2x).

Additionally, you should know how the [.NET Configuration service](https://docs.asp.net/en/latest/fundamentals/configuration.html) and the `ConfigurationBuilder` work and how to add providers to the builder.

You should also know how the ASP.NET Core [Startup](https://docs.asp.net/en/latest/fundamentals/startup.html) class is used in configuring the application services and how the middleware used in the application. Pay particular attention to the usage of the `Configure()` and `ConfigureService())` methods.

With regard to Cloud Foundry, you should know how Cloud Foundry OAuth2 security services (for example, [UAA Server](https://github.com/cloudfoundry/uaa) or [Pivotal Single Signon](https://docs.pivotal.io/p-identity/)) work.

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
    <PackageReference Include="Steeltoe.Security.Authentication.CloudFoundryCore" Version= "2.1.0"/>
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

The samples and most templates are already set up to read from `appsettings.json`. See [Reading Configuration Values](#reading-configuration-values).

The Steeltoe OAuth2 security provider options are based on [`Microsoft.AspNetCore.Authentication.OAuth.OAuthOptions`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authentication.oauth.oauthoptions), with these additional properties:

|Name|Description|Default|
|---|---|---|
|additionalScopes|Scopes to request for tokens in addition to `openid`|`string.Empty`|
|validateCertificates|Validate Auth server certificate|`true`|

**Note**: **Each setting above must be prefixed with `security:oauth2:client`**.

### Cloud Foundry

As mentioned earlier, there are two OAuth-compatible services available on Cloud Foundry. We recommend you read the offical documentation ([UAA Server](https://github.com/cloudfoundry/uaa) and [Pivotal SSO](https://docs.pivotal.io/p-identity/1-5/getting-started.html)) or follow the instructions included in the samples for [UAA Server](https://github.com/SteeltoeOSS/Samples/blob/master/Security/src/AspDotNetCore/CloudFoundrySingleSignon/README.md) and [Pivotal SSO](https://github.com/SteeltoeOSS/Samples/blob/master/Security/src/AspDotNetCore/CloudFoundrySingleSignon/README-SSO.md) to quickly learn how to create and bind OAuth2 services.

Regardless of which provider you choose, once the service is bound to your application, the settings are available in `VCAP_SERVICES`. See [Reading Configuration Values](#reading-configuration-values).

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

>TIP: See the Microsoft documentation on [ASP.NET Core Authorization](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/introduction).

# Single Sign-on with OpenID Connect

Single Sign-on with OpenID Connect enables you to leverage existing credentials configured in a [UAA Server](https://github.com/cloudfoundry/uaa) or [Pivotal Single Sign-on service](https://docs.pivotal.io/p-identity) for authentication and authorization in ASP.NET 4.x (via OWIN middleware) and ASP.NET Core applications.

The source code for this provider can be found [here](https://github.com/SteeltoeOSS/Security).

## Usage

### Usage in ASP.NET Core

Steeltoe builds on top of `Microsoft.AspNetCore.Authentication.OpenIdConnect`. You may benefit from reading more about using [OpenID Connect in ASP.NET Core](https://andrewlock.net/an-introduction-to-openid-connect-in-asp-net-core/).

Usage of Steeltoe's OpenID Connect provider is effectively identical to that of the [OAuth2 provider](#1-2-usage), although the behind-the-scenes story is a little different. The OpenID Connect provider uses Microsoft's OpenId Connect implementation, and settings are based on `Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectOptions`, with these additional properties:

|Name|Description|Default|
|---|---|---|
|additionalScopes|Scopes to request for tokens in addition to `openid`|`string.Empty`|
|validateCertificates|Validate Auth server certificate|`true`|

**Note**: **Each setting above must be prefixed with `security:oauth2:client`**.

Aside from the different base class for options, the only usage change is to call `.AddCloudFoundryOpenId` instead of `.AddCloudFoundryOAuth`.

### Usage in ASP.NET 4.x

This package is built on OpenID Connect and OWIN Middleware. You should take some time to understand both before proceeding to use this provider.

Resources are available elsewhere for understanding OpenID Connect. For example, see [Understanding OAuth 2.0 and OpenID Connect](https://blog.runscope.com/posts/understanding-oauth-2-and-openid-connect).

To learn more about OWIN, start with the [Overview of Project Katana](https://docs.microsoft.com/en-us/aspnet/aspnet/overview/owin-and-katana/an-overview-of-project-katana).

Additionally, you should know how the .NET [Configuration service](https://docs.asp.net/en/latest/fundamentals/configuration.html) and the `ConfigurationBuilder` work and how to add providers to the builder.

With regard to Cloud Foundry, you should know how Cloud Foundry OAuth security services (for example, [UAA Server](https://github.com/cloudfoundry/uaa) or [Pivotal Single Signon](https://docs.pivotal.io/p-identity/)) work.

In order to use the Security provider:

1. Create an instance of a Cloud Foundry OAuth service and bind it to your application.
1. (Optional) Configure any additional settings the Security provider needs.
1. Add the Cloud Foundry configuration provider to the `ConfigurationBuilder`.
1. Add the security provider to the OWIN pipeline in the application.
1. Secure your endpoints.

#### Add NuGet Reference

To use the provider, use the NuGet package manager to add a reference to the `Steeltoe.Security.Authentication.CloudFoundryOwin` package.

#### Configure Settings

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

The samples and most templates are already set up to read from `appsettings.json`. See [Reading Configuration Values](#reading-configuration-values).

This full list of settings can also be configured, though `AuthDomain`, `ClientId` and `ClientSecret` will be overridden by service bindings (if present).

|Name|Description|Default|
|---|---|---|
|additionalScopes|Scopes to request for tokens in addition to `openid`|`string.Empty`|
|authDomain|Location of the OAuth2 server|`https://Default_OAuthServiceUrl`|
|authenticationType|Corresponds to the IIdentity AuthenticationType|`CloudFoundry`|
|callbackPath|Path the user is redirected back to after authentication|`/signin-cloudfoundry`|
|clientId|App credentials with auth server|`Default_ClientId`|
|clientSecret|App credentials with auth server|`Default_ClientSecret`|
|validateCertificates|Validate OAuth2 server certificate|`true`|

**Note**: **Each setting above must be prefixed with `security:oauth2:client`**.

>NOTE: Prior to Steeltoe 2.2, the default value for CallbackPath was `/signin-oidc`

#### Cloud Foundry

As mentioned earlier, there are two ways to use OAuth2 services on Cloud Foundry. We recommend you read the offical documentation ([UAA Server](https://github.com/cloudfoundry/uaa) and [Pivotal SSO](https://docs.pivotal.io/p-identity/1-5/getting-started.html)) or follow the instructions included in the samples for [UAA Server](https://github.com/SteeltoeOSS/Samples/blob/master/Security/src/AspDotNet4/CloudFoundrySingleSignon/README.md) and [Pivotal SSO](https://github.com/SteeltoeOSS/Samples/blob/master/Security/src/AspDotNet4/CloudFoundrySingleSignon/README-SSO.md) to quickly learn how to create and bind OAuth2 services.

Regardless of which provider you choose, once the service is bound to your application, the settings are available in `VCAP_SERVICES`. See [Reading Configuration Values](#reading-configuration-values).

#### Configure OWIN Startup

In order to configure the Cloud Foundry OWIN OAuth provider in your application, you will need an [OWIN Startup class](https://docs.microsoft.com/en-us/aspnet/aspnet/overview/owin-and-katana/owin-startup-class-detection) if you do not already have one.

```csharp
using Steeltoe.Security.Authentication.CloudFoundry.Owin;
using System;
using System.IdentityModel.Claims;
using System.Linq;
using System.Web.Helpers;

[assembly: OwinStartup(typeof(CloudFoundrySingleSignon.Startup))]

namespace CloudFoundrySingleSignon
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType("ExternalCookie");
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "ExternalCookie",
                AuthenticationMode = AuthenticationMode.Active,
                CookieName = ".AspNet.ExternalCookie",
                LoginPath = new PathString("/Account/AuthorizeSSO"),
                ExpireTimeSpan = TimeSpan.FromMinutes(5)
            });

            app.UseCloudFoundryOpenIdConnect(ApplicationConfig.Configuration);

            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;
        }
    }
}
```

The `app.UseCloudFoundryOpenIdConnect` method call adds an authentication middleware that has been configured to work with UAA or Pivotal SSO to the OWIN Request pipeline.

>TIP: This code is commonly refactored into a separate class (for example `Startup.Auth.cs`), particularly when there is additional configuration on the OWIN pipeline.

#### Redirecting to OAuth Server

To redirect the user to the OAuth server for authentication, invoke `IAuthenticationManager.Challenge`, specifying a redirect uri (for the user to land on after authentication) and the string value configured as the authentication type.

In Steeltoe 2.2 and up, the default value for AuthenticationType is `CloudFoundryDefaults.DisplayName` or "CloudFoundry". Prior to Steeltoe 2.2, this value was `PivotalSSO` and was not configureable. The now-deprecated extension `.UseOpenIDConnect()` still uses that as the default, but can be overridden in `OpenIDConnectOptions`.

```csharp
public void AuthorizeSSO(string returnUrl)
{
    var properties = new AuthenticationProperties { RedirectUri = returnUrl ?? Url.Action("Secure", "Home") };
    HttpContext.GetOwinContext().Authentication.Challenge(properties, CloudFoundryDefaults.DisplayName);
}
```

>NOTE: The second parameter passed to `Authentication.Challenge` _must_ match the authentication type used to configure the provider; this is the piece that hands the flow over to Steeltoe.

If you wish to allow your controller action to return `ActionResult` instead of void, refactor the call to `Challenge` into a class that inherits `HttpUnauthorizedResult` such as this

```csharp
internal class ChallengeResult : HttpUnauthorizedResult
{
    public ChallengeResult(string authType, string redirectUri)
    {
        AuthenticationType = authType;
        RedirectUri = redirectUri;
    }

    public string AuthenticationType { get; set; }

    public string RedirectUri { get; set; }

    public override void ExecuteResult(ControllerContext context)
    {
        var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
        context.HttpContext.GetOwinContext().Authentication.Challenge(properties, AuthenticationType);
    }
}
```

The updated controller code would then look like this:

```csharp
public ActionResult AuthorizeSSO(string returnUrl)
{
    return new ChallengeResult(CloudFoundryDefaults.DisplayName, returnUrl ?? Url.Action("Secure", "Home"));
}
```

#### Securing Endpoints

Once the `Startup` class is in place and the middleware is configured, you can use the standard ASP.NET `Authorize` attribute to require authentication, as shown in the following example:

```csharp
using System.Web.Mvc;
...
public class HomeController : Controller
{
    public ActionResult Index()
    {
        return View();
    }

    [Authorize]
    public ActionResult Secure()
    {
        ViewData["Message"] = "This page requires authentication";
        return View();
    }
    ...
}
```

Requiring claims is not built into the framework, so it is not quite as simple as in ASP.NET Core, but is still possible in a variety of ways. The Steeltoe SSO sample demonstrates extending the [Thinktecture ClaimsAuthorizeAttribute](https://github.com/IdentityModel/Thinktecture.IdentityModel.45/blob/master/IdentityModel/Thinktecture.IdentityModel/Authorization/MVC/ClaimsAuthorizeAttribute.cs) with a [CustomClaimsAuthorizeAttribute](https://github.com/SteeltoeOSS/Samples/blob/dev/Security/src/AspDotNet4/CloudFoundrySingleSignon/CustomClaimsAuthorizeAttribute.cs) to redirect to an &quot;Access Denied&quot; page when a user is authenticated but lacks the required claim. Using either of those attributes is straightforward, as shown in this example:

```csharp
// When using this attribute, an authenticated user who does not have the claim will be redirected to the login page
[ClaimsAuthorize("testgroup")]
public ActionResult TestGroupV1()
{
    ViewBag.Message = "Congratulations, you have access to 'testgroup'";
    return View("Index");
}

[CustomClaimsAuthorize("testgroup")]
public ActionResult TestGroupV2()
{
    ViewBag.Message = "Congratulations, you have access to 'testgroup'";
    return View("Index");
}
```

# Resource Protection using JWT

This provider lets you control access to REST resources by using JWT tokens issued by Cloud Foundry Security services (such as [UAA Server](https://github.com/cloudfoundry/uaa) or [Pivotal Single Sign-on](https://docs.pivotal.io/p-identity)) in ASP.NET Core, ASP.NET WebAPI and WCF.

In addition to the [Quick Start](#2-1-quick-start), other Steeltoe sample applications can help you understand how to use this tool, including:

* [FreddysBBQ](https://github.com/SteeltoeOSS/Samples/tree/master/FreddysBBQ): A polyglot microservices-based sample showing interoperability between Java and .NET on Cloud Foundry, secured with OAuth2 Security Services, and using Spring Cloud Services.

## Usage in ASP.NET Core

This package uses JSON Web Tokens (JWT) and builds on JWT Security services provided by ASP.NET Core Security. You should take some time to understand both before proceeding to use this provider.

Many resources are available for understanding JWT (for example, see [JWT IO](https://jwt.io/) or [JSON Web Token](https://en.wikipedia.org/wiki/JSON_Web_Token)).

To get a good understanding of ASP.NET Core Security, review the [documentation](https://docs.microsoft.com/en-us/aspnet/core/) provided by Microsoft.

Additionally, you should know how the .NET [Configuration services](https://docs.asp.net/en/latest/fundamentals/configuration.html) the `ConfigurationBuilder` work and how to add providers to the builder.

You should also know how the ASP.NET Core [Startup](https://docs.asp.net/en/latest/fundamentals/startup.html) class is used in configuring the application services and how the middleware is used by the app. Pay particular attention to the usage of the `Configure()` and `ConfigureServices()` methods.

With regard to Cloud Foundry, you should have a good understanding of Cloud Foundry OAuth2 security services (such as [UAA Server](https://github.com/cloudfoundry/uaa) or [Pivotal Single Signon](https://docs.pivotal.io/p-identity/)) along with an understanding how they use and issue JWT.

To use the JWT Security provider:

1. Create and bind an instance of a Cloud Foundry OAuth2 service to your application.
1. (Optional) Configure any additional settings the Security provider will need.
1. Add the Cloud Foundry configuration provider to the ConfigurationBuilder.
1. Add and Use the security provider in the application.
1. Secure your endpoints

### Add NuGet Reference

To use the provider, add a reference to the Steeltoe Cloud Foundry Security NuGet package, `Steeltoe.Security.Authentication.CloudFoundryCore`, with the NuGet package manager or directly to your project file by using the following `PackageReference`:

```xml
<ItemGroup>
...
    <PackageReference Include="Steeltoe.Security.Authentication.CloudFoundryCore" Version= "2.1.0"/>
...
</ItemGroup>
```

### Configure Settings

Configuring additional settings for the provider is not typically required, but, when Cloud Foundry uses self-signed certificates, you might need to disable certificate validation, as shown in the following example:

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

The JWT provider uses Microsoft's JWT implementation, and settings are based on `Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerOptions`, with these additional properties:

|Name|Description|Default|
|---|---|---|
|validateCertificates|Validate Auth server certificate|`true`|

**Note**: **Each setting above must be prefixed with `security:oauth2:client`**.

The samples and most templates are already set up to read from `appsettings.json`. See [Reading Configuration Values](#reading-configuration-values).

### Cloud Foundry

As mentioned earlier. you can use a couple of OAuth2 services (such as UAA Server or Pivotal SSO) on Cloud Foundry. Rather than explaining how to create and bind OAuth2 services to your app here, we recommend that you read the documentation provided by each of the service providers.

Regardless of which provider you choose, once the service is bound to your application, the settings are available in `VCAP_SERVICES`. See [Reading Configuration Values](#reading-configuration-values).

### Add Cloud Foundry JwtAuthentication

To use the provider in your application, add it to your service collection in the `ConfigureServices()` method of the `Startup` class, as shown in the following example:

```csharp
using Steeltoe.Security.Authentication.CloudFoundryCore;

public class Startup {
    ...
    public IConfiguration Configuration { get; private set; }
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }
    public void ConfigureServices(IServiceCollection services)
    {
        // Add Cloud Foundry JWT Authentication service as the default
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddCloudFoundryJwtBearer(Configuration);
        // Add authorization policies
        services.AddAuthorization(options =>
        {
            options.AddPolicy("Orders", policy => policy.RequireClaim("scope", "order.me"));
            options.AddPolicy("AdminOrders", policy => policy.RequireClaim("scope", "order.admin"));
        });
        ...
    }
    public void Configure(IApplicationBuilder app, ...)
    {
        ...
        // Add the authentication middleware to the pipeline
        app.UseAuthentication();
    }
    ...
```

The `AddCloudFoundryJwtBearer(Configuration)` method call configures and adds the Cloud Foundry JWT authentication service to the service container. Once in place, the authentication middleware can use it during request processing.

### Securing Endpoints

Once you have the work done in your `Startup` class, you can then you can start to secure endpoints by using the standard ASP.NET Core `Authorize` attribute.

See the Microsoft documentation on [ASP.NET Core Security](https://docs.asp.net/en/latest/security/) for a better understanding of how to use these attributes.

The following example shows a controller using the security attributes:

```csharp
using Microsoft.AspNetCore.Authentication;
...

[Route("api/[controller]")]
public class ValuesController : Controller
{
    // GET api/values
    [HttpGet]
    [Authorize(Policy = "testgroup")]
    public IEnumerable<string> Get()
    {
        return new string[] { "value1", "value2" };
    }
}
```

In the preceding example, if an incoming REST request is made to the `api/values` endpoint and the request does not contain a valid JWT bearer token with a `scope` claim equal to `testgroup`, the request is rejected.

## Usage in ASP.NET WebAPI

This package is an extension of the Microsoft OWIN JWT bearer token middleware. You should take some time to understand both JWT and OWIN middlewares before proceeding to use this provider.

Many resources are available for understanding JWT (for example, see [JWT IO](https://jwt.io/) or [JSON Web Token](https://en.wikipedia.org/wiki/JSON_Web_Token)).

To learn more about OWIN, start with the [Overview of Project Katana](https://docs.microsoft.com/en-us/aspnet/aspnet/overview/owin-and-katana/an-overview-of-project-katana).

Additionally, you should know how the .NET [Configuration services](https://docs.asp.net/en/latest/fundamentals/configuration.html) the `ConfigurationBuilder` work and how to add providers to the builder.

With regard to Cloud Foundry, you should have a good understanding of Cloud Foundry OAuth2 security services (such as [UAA Server](https://github.com/cloudfoundry/uaa) or [Pivotal Single Signon](https://docs.pivotal.io/p-identity/)) along with an understanding how they use and issue JWT.

To use the JWT Security provider:

1. Create and bind an instance of a Cloud Foundry OAuth2 service to your application.
1. (Optional) Configure any additional settings the Security provider will need.
1. Add the Cloud Foundry configuration provider to the ConfigurationBuilder.
1. Add the security provider to the OWIN pipeline in the application.
1. Secure your endpoints

### Add NuGet Reference

To use the provider, use the NuGet package manager to add a reference to the `Steeltoe.Security.Authentication.CloudFoundryOwin` package.

### Configure Settings

Configuring additional settings for the provider is not typically required, but, when Cloud Foundry uses self-signed certificates, you might need to disable certificate validation, as shown in the following example:

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

The JWT provider uses Microsoft's JWT implementation, and settings are based on `Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerOptions`, with these additional properties:

|Name|Description|Default|
|---|---|---|
|skipAuthIfNoBoundSSOService|JWT Middleware will not be added if SSO binding is not found|`true`|
|validateCertificates|Validate Auth server certificate|`true`|

**Note**: **Each setting above must be prefixed with `security:oauth2:client`**.

The Steeltoe sample is set up to read from `appsettings.json`. If you require additional information, see [Reading Configuration Values](#reading-configuration-values).

>NOTE: The setting `SkipAuthIfNoBoundSSOService` was added in Steeltoe 2.2.0, and has a default value of `true` for backwards compatibility with previous versions. This setting was added to control functionality that was previously always-on. A future release is likely to change the default to `false` or may remove the functionality entirely.

### Cloud Foundry

As mentioned earlier, there are two auth services (UAA Server and Pivotal SSO) on Cloud Foundry. Rather than explaining how to create and bind those services to your app here, we recommend that you read the documentation provided by each of the service providers.

Regardless of which provider you choose, once the service is bound to your application, the settings are available in `VCAP_SERVICES`. See [Reading Configuration Values](#reading-configuration-values).

### Add Cloud Foundry JwtAuthentication

In order to configure the Cloud Foundry OWIN JWT provider in your application, you will need an [OWIN Startup class](https://docs.microsoft.com/en-us/aspnet/aspnet/overview/owin-and-katana/owin-startup-class-detection) if you do not already have one, along with an `IConfigurationRoot` that includes a service binding for UAA or Pivotal SSO.

```csharp
using Owin;
using Steeltoe.Security.Authentication.CloudFoundry;

namespace CloudFoundryJwtAuthentication
{
    public partial class Startup
    {
        public void ConfigureAuth(IAppBuilder app)
        {
            app.UseCloudFoundryJwtBearerAuthentication(ApplicationConfig.Configuration);
        }
   }
}
```

The `UseCloudFoundryJwtBearerAuthentication(Configuration)` method call configures and adds the Microsoft OWIN JWT authentication middleware to the OWIN pipeline with configuration for Cloud Foundry. Once in place, the authentication middleware can use it during request processing.

### Securing Endpoints

Once the `Startup` class is in place and the middleware is configured, you can use the standard ASP.NET `Authorize` attribute to require authentication.

The `CloudFoundryJwtAuthentication` sample demonstrates extending the AuthorizeAttribute with a [CustomClaimsAuthorizeAttribute](https://github.com/SteeltoeOSS/Samples/blob/dev/Security/src/AspDotNet4/CloudFoundryJwtAuthentication/CustomClaimsAuthorizeAttribute.cs) to require a given claim on an endpoint in a straightforward way. The following example shows a controller using the `CustomClaimsAuthorizeAttribute`:

```csharp
using System;
using System.Collections.Generic;
using System.Web.Http;

public class ValuesController : ApiController
{
    // GET: api/Values
    [CustomClaimsAuthorize("testgroup")]
    public IEnumerable<string> Get()
    {
        Console.WriteLine("Received GET Request");
        return new string[] { "value1", "value2" };
    }
}
```

In the preceding example, if an incoming REST request is made to the `api/values` endpoint and the request does not contain a valid JWT bearer token with a `scope` claim equal to `testgroup`, the request is rejected.

## Usage in ASP.NET WCF

This package is a custom authorization provider for using JWT Bearer tokens in Windows Communication Foundation (WCF) applications. The provider is built on `System.IdentityModel.Claims`. You should take some time to understand both JWT and WCF security before proceeding to use this provider.

Many resources are available for understanding JWT (for example, see [JWT IO](https://jwt.io/) or [JSON Web Token](https://en.wikipedia.org/wiki/JSON_Web_Token)).

Additionally, you should know how the .NET [Configuration services](https://docs.asp.net/en/latest/fundamentals/configuration.html) the `ConfigurationBuilder` work and how to add providers to the builder.

With regard to Cloud Foundry, you should have a good understanding of Cloud Foundry OAuth2 security services (such as [UAA Server](https://github.com/cloudfoundry/uaa) or [Pivotal Single Signon](https://docs.pivotal.io/p-identity/)) along with an understanding how they use and issue JWT.

To use the JWT Security provider:

1. Create and bind an instance of a Cloud Foundry OAuth2 service to your application.
1. (Optional) Configure any additional settings the Security provider will need.
1. Add the Cloud Foundry configuration provider to the ConfigurationBuilder.
1. Configure JwtAuthorizationManager as the ServiceAuthorizationManger for the server application.
1. Secure your endpoints
1. Update your WCF client(s) to include JWTs in the request

### Add NuGet Reference

To use the provider, use the NuGet package manager to add a reference to the `Steeltoe.Security.Authentication.CloudFoundryWcf` package in both your client and server applications.

### Configure Settings

Configuring additional settings for the provider is not typically required, but, when Cloud Foundry uses self-signed certificates, you might need to disable certificate validation, as shown in the following example:

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

The Steeltoe sample is set up to read from `appsettings.json`, if you require additional information, see [Reading Configuration Values](#reading-configuration-values).

|Name|Description|Default|
|---|---|---|
|forwardUserCredentials|Whether to use app credentials or forward users's credentials|`false`|
|validateAudience|Whether or not a token's audience should be validated|`true`|
|validateIssuer|Whether or not a token's issuer should be validated|`true`|
|validateLifeTime|Whether or not a token's lifetime should be validated|`true`|
|validateCertificates|Validate Auth server certificate|`true`|

### Cloud Foundry

As mentioned earlier. you can use a couple of OAuth2 services (such as UAA Server or Pivotal SSO) on Cloud Foundry. Rather than explaining how to create and bind OAuth2 services to your app here, we recommend that you read the documentation provided by each of the service providers.

Regardless of which provider you choose, once the service is bound to your application, the settings are available in `VCAP_SERVICES`. See [Reading Configuration Values](#reading-configuration-values).

### Set ServiceAuthorizationManager

To configure the Cloud Foundry JWT provider for your WCF service, provide your `IConfiguration` to the `AddJwtAuthorization` extension, as shown in the following example:

```csharp
using Steeltoe.Security.Authentication.CloudFoundry.Wcf;
using System;
using System.ServiceModel;

public class Global : System.Web.HttpApplication
{
    protected void Application_Start(object sender, EventArgs e)
    {
        ApplicationConfig.RegisterConfig("development");
        var serviceHost = new ServiceHost(typeof(ValueService));
        serviceHost.AddJwtAuthorization(ApplicationConfig.Configuration);
    }
}
```

>NOTE: The above is not the only way to configure the JwtAuthorizationManager. WCF can be configured many ways, which are beyond the scope of this documentation.

### Securing Endpoints

Inside your WCF service, apply claims rules to endpoints with the `ScopePermission` attribute, as seen in this example:

```csharp
using Steeltoe.Security.Authentication.CloudFoundry.Wcf;
using System.Security.Permissions;

namespace CloudFoundryWcf
{
    public class ValueService : IValueService
    {
        [ScopePermission(SecurityAction.Demand, Scope = "testgroup")]
        public string GetData()
        {
            return "Hello from the WCF Sample!";
        }
    }
}
```

### Updating Client to Send JWT

In order to include a JWT in the request to a WCF service, you will need to apply an EndpointBehavior. The `JwtHeaderEndpointBehavior` is provided for you in the CloudFoundryWcf package.
The `JwtHeaderEndpointBehavior` can be configured to pass a user's credentials forward to backing services, or to get a token on behalf of the application (Client Credentials flow).

The default behavior of the library is the Client Credentials flow. The [CloudFoundryOptions](https://github.com/SteeltoeOSS/Security/blob/dev/src/Steeltoe.Security.Authentication.CloudFoundryWcf/CloudFoundryOptions.cs) required by the `JwtHeaderEndpointBehavior` can be configured several ways:

1. From an `IConfiguration`
   * `new CloudFoundryOptions(configuration)`
   * Settings bound from the subsection defined as `security:oauth2:client`
1. Inline
   * `new CloudFoundryOptions {  ClientId = "client-id", ClientSecret = "client-secret" }`)
1. From environment variables
   * `new CloudFoundryOptions()`
      * Auth domain is set with `sso_auth_domain`
      * Client Id is set with `sso_client_id`
      * Client Secret is set with `sso_client_secret`

Regardless of the method chosen for instantiating the `CloudFoundryOptions`, the code to apply the behavior should look something like this:

```csharp
    // create an instance of the WCF client
    var sRef = new ValueService.ValueServiceClient();

    // apply the behavior, expecting it to manage and pass the token for the application
    sRef.Endpoint.EndpointBehaviors.Add(new JwtHeaderEndpointBehavior(new CloudFoundryOptions(configuration)));
    string serviceResponse = await sRef.GetDataAsync();
```

To pass a user's token (instead of the application's) to the backing service, first set `security:oauth2:client:forwardUserCredentials` to `true` in your configuration. You will also need to retrieve the user's token and pass that into the `JwtHeaderEndpointBehavior` when making requests, as seen in this example:

```csharp
    // retrieve the user's token
    var token = Request.GetOwinContext().Authentication.User.Claims.First(c => c.Type == ClaimTypes.Authentication)?.Value;

    // create an instance of the WCF client
    var sRef = new ValueService.ValueServiceClient(binding, address);

    // apply the behavior, including the user's token
    sRef.Endpoint.EndpointBehaviors.Add(new JwtHeaderEndpointBehavior(new CloudFoundryOptions(ApplicationConfig.Configuration), token));
    string serviceResponse = await sRef.GetDataAsync();
```

# Redis Key Storage Provider

<span style="display:inline-block;margin:0 20px;">For use with </span><span style="display:inline-block;vertical-align:top;width:40%"> ![alt text](/images/CFF_Logo_rgb.png "Cloud Foundry")</span>

By default, ASP.NET Core stores the key ring on the local file system. Local file system usage in a Cloud Foundry environment is unworkable and violates the [twelve-factor guidelines](https://12factor.net/) for developing cloud native applications. By using the Steeltoe Redis Key Storage provider, you can reconfigure the Data Protection service to use Redis on Cloud Foundry for storage.

## Usage

To use this provider:

1. Create a Redis Service instance and bind it to your application.
1. Add the Steeltoe Cloud Foundry config provider to your `ConfigurationBuilder`.
1. Add the Redis `ConnectionMultiplexer` to your ServiceCollection.
1. Add `DataProtection` to your `ServiceCollection` and configure it to `PersistKeysToRedis`.

### Add NuGet Reference

To use the provider, add a reference to the Steeltoe DataProtection Redis NuGet.

The provider can be found in the `Steeltoe.Security.DataProtection.RedisCore` package.

You can add the provider to your project by using the following `PackageReference` in your project file:

```xml
<ItemGroup>
...
    <PackageReference Include="Steeltoe.Security.DataProtection.RedisCore" Version= "2.1.0"/>
...
</ItemGroup>
```

You also need the Steeltoe Redis connector. Add the `Steeltoe.CloudFoundry.ConnectorCore` package to get the Redis connector and helpers for setting it up.

You can use the NuGet Package Manager tools or directly add the following package reference to your .csproj file:

```xml
<ItemGroup>
...
    <PackageReference Include="Steeltoe.CloudFoundry.ConnectorCore" Version= "2.1.0"/>
...
</ItemGroup>
```

### Cloud Foundry

To use the Redis Data Protection key ring provider on Cloud Foundry, you have to install a Redis service and create and bind an instance of it to your application by using the Cloud Foundry command line, as shown in the following example:

```bash
# Create Redis service
cf create-service p-redis shared-vm myRedisCache

# Bind service to `myApp`
cf bind-service myApp myRedisCache

# Restage the app to pick up change
cf restage myApp
```

>NOTE: The preceding commands are for the Redis service provided by Pivotal on Cloud Foundry. If you use a different service, you have to adjust the `create-service` command.

Once the service is bound to your application, the settings are available in `VCAP_SERVICES`. See [Reading Configuration Values](#reading-configuration-values).

### Add Redis IConnectionMultiplexer

The next step is to add the StackExchange Redis `IConnectionMultiplexer` to your service container.

You can do so in the `ConfigureServices()` method of the `Startup` class by using the Steeltoe Redis Connector, as follows:

```csharp
using Steeltoe.CloudFoundry.Connector.Redis;

public class Startup {
    ...
    public IConfiguration Configuration { get; private set; }
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }
    public void ConfigureServices(IServiceCollection services)
    {
        // Add StackExchange ConnectionMultiplexer configured from Cloud Foundry
        services.AddRedisConnectionMultiplexer(Configuration);

        // Add framework services.
        services.AddMvc();
        ...
    }
    ...
```

See the documentation on the [Steeltoe Redis connector](../steeltoe-connectors/#5-2-2-configure-settings) for details on how you can configure additional settings to control its behavior.

### Add PersistKeysToRedis

The last step is to use the provider to configure DataProtection to persist keys to Redis.

You can do so in the `ConfigureServices()` method of the `Startup` class, as shown in the following example:

```csharp
using Steeltoe.CloudFoundry.Connector.Redis;

public class Startup {
    ...
    public IConfiguration Configuration { get; private set; }
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }
    public void ConfigureServices(IServiceCollection services)
    {
        // Add StackExchange ConnectionMultiplexer configured from Cloud Foundry
        services.AddRedisConnectionMultiplexer(Configuration);

        // Add DataProtection and persist keys to Cloud Foundry Redis service
        services.AddDataProtection()
            .PersistKeysToRedis()
            .SetApplicationName("Some Name");

        // Add framework services.
        services.AddMvc();
        ...
    }
    ...
```

### Use Redis Key Store

Once the Redis Key Store has been set up, the keys used by the `DataProtection` framework are stored in the bound Redis Cloud Foundry service. You need not do more.

# CredHub API Client

[CredHub](https://github.com/cloudfoundry-incubator/credhub) manages credentials such as  passwords, certificates, certificate authorities, ssh keys, rsa keys, and other arbitrary values. Steeltoe provides the `CredHubBase` library for interacting with the [CredHub API](https://credhub-api.cfapps.io/) and provides the `CredHubCore` library for making that client library simpler to use in ASP.NET Core applications. Cloud Foundry is not required for the CredHub server or client but is used in this documentation as the hosting environment. You may wish to review the documentation for [CredHub on PCF](https://docs.pivotal.io/pivotalcf/2-0/credhub/). If you do not already have a UAA user to use for this test, you will need to use the UAA command line tool to establish some security credentials for the sample app. Choose one of the provided `credhub-setup` scripts in the folder `samples/Security/scripts` to target your Cloud Foundry environment and create a UAA client with permissions to read and write in CredHub.

>NOTE: If you choose to change the values for `UAA_CLIENT_ID` or `UAA_CLIENT_SECRET`, be sure to update the credentials in appsettings.json

<!--  -->
>WARNING: As of this writing, CredHub is not approved for general use in all applications. We encourage you to check whether your use case is currently supported by CredHub before getting too involved.

## Usage

To use the Steeltoe CredHub Client, you must:

* Have access to a CredHub Server (the client was built against version 1.6.5).
* Have UAA credentials for accessing the server with sufficient permissions for your use case
* Use the provided methods and constructors to create, inject, or utilize the client.

### Add NuGet Reference

To use this library with ASP.NET Core, add a NuGet reference to `Steeltoe.Security.DataProtection.CredHubCore`. For other application types, use `Steeltoe.Security.DataProtection.CredHubBase`. Most of the functionality resides in `CredHubBase`. The purpose of `CredHubCore` is to provide additional methods for a simpler experience when using ASP.NET Core.

Use the NuGet package manager tools or directly add the appropriate package to your project using the a `PackageReference`, as follows:

```xml
<ItemGroup>
...
    <PackageReference Include="Steeltoe.Security.DataProtection.CredHubCore" Version= "2.1.0-rc1"/>
...
</ItemGroup>
```

### Configure Settings

Settings for this library are expected to have a prefix of `CredHubClient`. The following example shows what that looks like in JSON:

```json
{
  ...
  "credHubClient": {
    "validateCertificates": "false"
  }
  ...
}
```

The `CredHubClient` supports four settings:

|Setting Name|Description|Default|
|---|---|---|
|CredHubUrl|The address of the CredHub API|<https://credhub.service.cf.internal:8844/api>|
|CredHubUser|The username for UAA auth|`null`|
|CredHubPassword|The password for UAA auth|`null`|
|ValidateCertificates|Whether to validate certificates for UAA and/or CredHub servers|`true`|

The samples and most templates are already set up to read from `appsettings.json`. See [Reading Configuration Values](#reading-configuration-values).

### On Cloud Foundry

Pivotal Cloud Foundry (in versions 2.0 and up) ships with a version of CredHub Server with which this client works. To use UAA authentication, you will need a user with `credhub.read` and/or `credhub.write` claims.

### Getting a Client

There are several ways to create a `CredHubClient`, depending on whether you want to use Microsoft's dependency injection. Regardless of the creation method selected, once the client has been created, all functionality is the same.

#### Create and Inject Client

If you use Microsoft's Dependency injection framework, you can use `IServiceCollection.AddCredHubClient()` to create, configure, and inject `CredHubClient`, as shown in the folloowing example:

```csharp
using Steeltoe.Security.DataProtection.CredHubCore;
...
public class Startup
{
    ILoggerFactory logFactory;
    public Startup(IConfiguration configuration, ILoggerFactory logFactory)
    {
        Configuration = configuration;
        this.logFactory = logFactory;
    }
    public IConfiguration Configuration { get; }
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMvc();
        services.AddCredHubClient(Configuration, logFactory);
    }
    ...
}
...
```

#### Create UAA Client

You can use `CredHubClient.CreateUAAClientAsync()` to directly create a CredHub client that authenticates with a username and password that is valid on the UAA server CredHub is configured to trust. This client calls `/info` on the CredHub server to discover the UAA server's address and appends `/oauth/token` when requesting a token. The following listing shows how to do it:

```csharp
var credHubClient = await CredHubClient.CreateUAAClientAsync(new CredHubOptions());
```

>NOTE: If you need to override the UAA server address, use the `UAA_Server_Override` environment variable, making sure to include the path to the token endpoint.

#### Interpolation-Only

If you wish to use CredHub to interpolate entries in `VCAP_SERVICES`, you can use `WebHostBuilder.UseCredHubInterpolation()`. This method looks for `credhub-ref` in `VCAP_SERVICES` and uses a `CredHubClient` to replace the credential references with credentials stored in CredHub but does not return the `CredHubClient`. The following example shows how to do it:

```csharp
    var host = new WebHostBuilder()
        .UseKestrel()
        .UseCloudFoundryHosting()
        .UseContentRoot(Directory.GetCurrentDirectory())
        .UseIISIntegration()
        .UseStartup<Startup>()
        .ConfigureAppConfiguration((builderContext, config) =>
        {
            config.SetBasePath(builderContext.HostingEnvironment.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{builderContext.HostingEnvironment.EnvironmentName}.json", optional: true)
                .AddCloudFoundry()
                .AddEnvironmentVariables();
        })
        .ConfigureLogging((builderContext, loggingBuilder) =>
        {
            loggingBuilder.AddConfiguration(builderContext.Configuration.GetSection("Logging"));
            loggingBuilder.AddDynamicConsole();
        })
        .UseCredHubInterpolation(new LoggerFactory().AddConsole())
        .Build();
```

### Credential Types

These are the .NET representations of credentials that can be stored in CredHub. Refer to the [CredHub documentation](https://credhub-api.cfapps.io/) for more detail.

#### ValueCredential

Any string can be used for a `ValueCredential`. CredHub allows Get, Set, Delete, and Find operations with `ValueCredential`

#### PasswordCredential

Any string can be used for a `PasswordCredential`. CredHub allows Get, Set, Delete, Find, Generate, and Regenerate operations with `PasswordCredential`

#### UserCredential

A `UserCredential` has `string` properties for `Username` and `Password`. CredHub allows Get, Set, Delete, Find, Generate, and Regenerate operations with `UserCredential`. Regenerate operations do not regenerate the username.

#### JsonCredential

Any JSON object can be used for a `JsonCredential`. CredHub allows Get, Set, Delete, and Find operations with `JsonCredential`.

#### CertificateCredential

A `CertificateCredential` represents a security certificate. CredHub allows Get, Set, Delete, Find, Generate, Regenerate, and Bulk Regenerate operations with `CertificateCredential`. The following table describes specific properties:

|Property|Description|
|---|---|
|CertificateAuthority|The certificate of the Certificate Authority|
|CertificateAuthorityName|The name of the CA credential in credhub that has signed this certificate|
|Certificate|The string representation of the certificate|
|PrivateKey|The private key for the certificate|

#### RsaCredential

The `RsaCredential` has string properties for `PublicKey` and `PrivateKey`. CredHub allows Get, Set, Delete, Find, Generate, and Regenerate operations with `RsaCredential`.

#### SshCredential

The `SshCredential` has string properties for `PublicKey`, `PrivateKey`, and `PublicKeyFingerprint`. CredHub allows Get, Set, Delete, Find, Generate, and Regenerate operations with `SshCredential`.

### CredHub Read Operations

All `CredHubClient` Read operations operate asynchronously and do not change the credentials or permissions stored in CredHub. Refer to the [CredHub documentation](https://credhub-api.cfapps.io/) for more detail. For brevity, the samples shown later in this guide use `_credHubClient` to reference an instance of `CredHubClient` that has been created previously. See [Getting a Client](#4-2-4-getting-a-client) for instructions on how to create a `CredHubClient`.

#### Get by ID

You can use `await _credHubClient.GetByIdAsync<CredentialType>(credentialId)` to retrieve a credential by its `Guid`. Only the current credential value is returned.

#### Get by Name

You can use `await _credHubClient.GetByNameAsync<CredentialType>(credentialName)` to retrieve a credential by its `Name`. Only the current credential value is returned.

#### Get by Name with History

You can use `await _credHubClient.GetByNameWithHistoryAsync<CredentialType>(credentialName, numEntries)` to retrieve a credential by name with the most recent `numEntries` holding the number of entries.

#### Find by Name

You can use `await _credHubClient.FindByNameAsync(credentialName)` to retrieve a list of `FoundCredential` objects that are either a full or partial match for the name searched. The `FoundCredential` type includes only the `Name` and `VersionCreatedAt` properties, so follow-up requests are expected to retrieve credential details.

#### Find by Path

You can use `await _credHubClient.FindByPathAsync(path)` to retrieve a list of `FoundCredential` objects that are either a full or partial match for the name searched. The `FoundCredential` type includes only the `Name` and `VersionCreatedAt` properties, so follow-up requests are expected to retrieve credential details. Use the `/` path value to return all accessible credentials.

#### Find All Paths

You can use `await _credHub.FindAllPathsAsync()` to retrieve a list of all known credential paths.

#### Interpolate

One of the more powerful features of CredHub is the `Interpolate` endpoint. With one request, you may retrieve N number of credentials that have been stored in CredHub. To use it from .NET, call `await _credHub.InterpolateServiceDataAsync(serviceData)`, where `serviceData` is the string representation of `VCAP_SERVICES`. `CredHubClient` returns the interpolated `VCAP_SERVICES` data as a string. If you wish to have the interpolated data applied to your application configuration, see [the .UseCredHubInterpolation() documentation](#4-2-4-4-interpolation-only)

The following example shows a typical request object for the `Interpolate` endpoint:

```json
{
  "p-demo-resource": [
    {
      "credentials": {
        "credhub-ref": "((/config-server/credentials))"
      },
      "label": "p-config-server",
      "name": "config-server",
      "plan": "standard",
      "provider": null,
      "syslog_drain_url": null,
      "tags": [
        "configuration",
        "spring-cloud"
      ],
      "volume_mounts": []
    }
  ]
}
```

The following example shows a typical response object from the `Interpolate` endpoint:

```json
{
  "p-demo-resource": [
    {
      "credentials": {
        "key": 123,
        "key_list": [
          "val1",
          "val2"
        ],
        "is_true": true
      },
      "label": "p-config-server",
      "name": "config-server",
      "plan": "standard",
      "provider": null,
      "syslog_drain_url": null,
      "tags": [
        "configuration",
        "spring-cloud"
      ],
      "volume_mounts": []
    }
  ]
}
```

>NOTE: At this time, only credential references at `credentials.credhub-ref` are interpolated. The `credhub-ref` key is removed and the referenced credential object is set as the value of the credentials.

### CredHub Change Operations

All `CredHubClient` Change operations operate asynchronously and affect stored credentials. Refer to the [CredHub documentation](https://credhub-api.cfapps.io/) for more detail. For brevity, the samples shown later in this guide use `_credHubClient` to reference an instance of `CredHubClient` that has been created previously. See [Getting a Client](#4-2-4-getting-a-client) for instructions on how to create a `CredHubClient`.

#### Write

If you already have a credential that you want to store in CredHub, use a `Write` request. `CredHubClient.WriteAsync<T>()` expects a request object that descends from `CredentialSetRequest`. There is a `[Type]SetRequest` class for each credential type (`ValueSetRequest`, `PasswordSetRequest`, and so on). The SetRequest family of classes includes optional parameters for overwriting existing values and setting permissions, in addition to value properties for the credential. Include the type of credential you want to write to CredHub in the T parameter so the compiler knows the return type. The following example shows a typical `Write` request:

```csharp
var setValueRequest = new ValueSetRequest("sampleValueCredential", "someValue");
// set the value of credential "/sampleValueCredential" to "someValue" if there is NOT an existing value
var setValue1 = await _credHub.WriteAsync<ValueCredential>(setValueRequest);

// set the value of credential "/sampleValueCredential" to "someValue" even if there is an existing value
setValueRequest.Overwrite = true;
var setValue2 = await _credHub.WriteAsync<ValueCredential>(setValueRequest);
```

>NOTE: The default behavior on `Write` requests is to leave existing values alone. If you wish to overwrite a credential, be sure to pass either `OverwriteMode.converge` or `OverwriteMode.overwrite` for the `overwriteMode` parameter on your request object. See [Overwriting Credential Values](https://credhub-api.cfapps.io/#overwriting-credential-values).

Write requests allow the setting of permissions on a credential during generation, as shown in the following example:

```csharp
var desiredOperations = new List<OperationPermissions>
                        {
                            OperationPermissions.read,
                            OperationPermissions.write,
                            OperationPermissions.delete
                        };
var newPerms = new CredentialPermission { Actor = "uaa-user:credhub_client", Operations = desiredOperations };
var setRequest = new ValueSetRequest("sampleValueCredential", "someValue", new List<CredentialPermission> { newPerms });
var setValue = await _credHub.WriteAsync<ValueCredential>(setRequest);
```

#### Generate

You can generate a new credential or overwrite an existing credential with a new generated value. CredHub is able to generate values for the following types: `CertificateCredential`, `PasswordCredential`, `RsaCredential`, `SshCredential`, and `UserCredential`. `CredHubClient.GenerateAsync<T>()` expects a request object that descends from `CredHubGenerateRequest`. There is a `[Type]GenerateRequest` class for each supported credential type (`CertificateGenerationRequest`, `PasswordGenerationRequest`, `RsaGenerationRequest`, and so on). Most of the `GenerateRequest` family of classes include a `[Type]GenerationParameters` parameter for specifying criteria for generating the credential. The following example shows how to generate a credential:

```csharp
var genParameters = new PasswordGenerationParameters { Length = 20 };
var genRequest = new PasswordGenerationRequest("generatedPassword", genParameters);
CredHubCredential<PasswordCredential> genPassword = await _credHub.GenerateAsync<PasswordCredential>(genRequest);
```

The following example demonstrates that `Generate` requests allow the setting of permissions on a credential during generation:

```csharp
var genParams = new PasswordGenerationParameters { Length = 20 };
var desiredOperations = new List<OperationPermissions>
                        {
                            OperationPermissions.read,
                            OperationPermissions.write,
                            OperationPermissions.delete
                        };
var newPerms = new CredentialPermission { Actor = "uaa-user:credhub_client", Operations = desiredOperations };
var genRequest = new PasswordGenerationRequest("generatedPW", genParams, new List<CredentialPermission> { newPerms });
CredHubCredential<PasswordCredential> genPassword = await _credHub.GenerateAsync<PasswordCredential>(genRequest);
```

>NOTE: The default behavior on `Generate` requests is to leave existing values alone. If you wish to overwrite a credential, be sure to pass either `OverwriteMode.converge` or `OverwriteMode.overwrite` for the `overwriteMode` parameter on your request object. See [Overwriting Credential Values](https://credhub-api.cfapps.io/#overwriting-credential-values).

#### Regenerate

You can regenerate a credential in CredHub. CredHub can generate values for the following types: `CertificateCredential`, `PasswordCredential`, `RsaCredential`, `SshCredential`, and `UserCredential`.

>NOTE: Only credentials that were previously generated by CredHub can be regenerated.

The following example shows one way to regenerate a credential:

```csharp
var regeneratedCert = await _credHub.RegenerateAsync<CertificateCredential>("/MyGeneratedCert");
```

#### Bulk Regenerate

You can regenerate all certificates that were previously generated by CredHub with a given certificate authority. The following example returns a list of `RegeneratedCertificates` which contains the credential names as a `List<string>` property named RegeneratedCredentials:

```csharp
RegeneratedCertificates bulkRegenerate = await _credHub.BulkRegenerateAsync("NameThatCA");
```

#### Delete by Name

You can delete a credential by its full name. The following example returns a boolean indicating success or failure:

```csharp
bool deleteCertificate = await _credHub.DeleteByNameAsync("/MyPreviouslyGeneratedCertificate");
```

### Permission Operations

CredHub supports permissions management on credential access for UAA users. See the [offical CredHub Permissions documentation](https://credhub-api.cfapps.io/#permissions).

#### Get Permissions

You can get the permissions associated with a credential, as shown in the following example:

```csharp
List<CredentialPermission> response = await _credHub.GetPermissionsAsync("/example-password");
```

#### Add Permissions

You can add permissions to an existing credential. The following example eturns the updated list of permissions for the specified credential:

```csharp
var desiredOps = new List<OperationPermissions> { OperationPermissions.read, OperationPermissions.write };
var newActorPermissions = new CredentialPermission { Actor = "uaa-user:credhub_client", Operations = desiredOps };
var newPerms = new List<CredentialPermission> { newActorPermissions };
List<CredentialPermission> response = await _credHub.AddPermissionsAsync("/example-password", newPerms);
```

#### Delete Permissions

You can delete a permission associated with a credential. The following example returns a boolean indicating success or failure:

```csharp
bool response = await _credHub.DeletePermissionAsync("/example-password", "uaa-user:credhub_client");
```