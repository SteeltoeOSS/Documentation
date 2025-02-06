# Single Sign-on with OpenID Connect

Single Sign-on with OpenID Connect enables you to leverage existing credentials configured in a [UAA Server](https://github.com/cloudfoundry/uaa) or [TAS Single-Sign-on service](https://docs.pivotal.io/p-identity) for authentication and authorization in ASP.NET 4.x (via OWIN middleware) and ASP.NET Core applications.

## Usage

### Usage in ASP NET Core

Steeltoe builds on top of `Microsoft.AspNetCore.Authentication.OpenIdConnect`. You may benefit from reading more about using [OpenID Connect in ASP.NET Core](https://andrewlock.net/an-introduction-to-openid-connect-in-asp-net-core/).

Usage of Steeltoe's OpenID Connect provider is effectively identical to that of the OAuth2 provider, although the behind-the-scenes story is a little different. The OpenID Connect provider uses Microsoft's OpenId Connect implementation, and settings are based on `Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectOptions`, with these additional properties:

|Name|Description|Default|
|---|---|---|
|additionalScopes|Scopes to request for tokens in addition to `openid`|`string.Empty`|
|validateCertificates|Validate Auth server certificate|`true`|

**Note**: **Each setting above must be prefixed with `security:oauth2:client`**.

Aside from the different base class for options, the only usage change is to call `.AddCloudFoundryOpenId` instead of `.AddCloudFoundryOAuth`.

### Usage in ASP NET 4

This package is built on OpenID Connect and OWIN Middleware. You should take some time to understand both before proceeding to use this provider.

Resources are available elsewhere for understanding OpenID Connect. For example, see [Understanding OAuth 2.0 and OpenID Connect](https://blog.runscope.com/posts/understanding-oauth-2-and-openid-connect).

To learn more about OWIN, start with the [Overview of Project Katana](https://docs.microsoft.com/aspnet/aspnet/overview/owin-and-katana/an-overview-of-project-katana).

Additionally, you should know how the .NET [Configuration service](https://docs.asp.net/en/latest/fundamentals/configuration.html) and the `ConfigurationBuilder` work and how to add providers to the builder.

With regard to Cloud Foundry, you should know how Cloud Foundry OAuth security services (for example, [UAA Server](https://github.com/cloudfoundry/uaa) or [TAS Single Signon](https://docs.pivotal.io/p-identity/)) work.

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

The samples and most templates are already set up to read from `appsettings.json`.

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

As mentioned earlier, there are two ways to use OAuth2 services on Cloud Foundry. We recommend you read the offical documentation ([UAA Server](https://github.com/cloudfoundry/uaa) and [TAS SSO](https://docs.pivotal.io/p-identity/1-5/getting-started.html)) or follow the instructions included in the samples for [UAA Server](https://github.com/SteeltoeOSS/Samples/blob/2.x/Security/src/AspDotNet4/CloudFoundrySingleSignon/README.md) and [TAS SSO](https://github.com/SteeltoeOSS/Samples/blob/2.x/Security/src/AspDotNet4/CloudFoundrySingleSignon/README-SSO.md) to quickly learn how to create and bind OAuth2 services.

Regardless of which provider you choose, once the service is bound to your application, the settings are available in `VCAP_SERVICES`.

#### Configure OWIN Startup

In order to configure the Cloud Foundry OWIN OAuth provider in your application, you will need an [OWIN Startup class](https://docs.microsoft.com/aspnet/aspnet/overview/owin-and-katana/owin-startup-class-detection) if you do not already have one.

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

The `app.UseCloudFoundryOpenIdConnect` method call adds an authentication middleware that has been configured to work with UAA or TAS SSO to the OWIN Request pipeline.

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
