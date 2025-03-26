# Resource Protection using JWT in ASP.NET WebAPI

This provider lets you control access to REST resources by using JWT tokens issued by Cloud Foundry Security services (such as [UAA Server](https://github.com/cloudfoundry/uaa) or [TAS Single-Sign-on](https://docs.pivotal.io/p-identity)) in ASP.NET Core, ASP.NET WebAPI and WCF.

Other Steeltoe sample applications can help you understand how to use this tool, including:

* `FreddysBBQ`: A polyglot microservices-based sample showing interoperability between Java and .NET on Cloud Foundry, secured with OAuth2 Security Services, and using Spring Cloud Services.

## Usage

This package is an extension of the Microsoft OWIN JWT bearer token middleware. You should take some time to understand both JWT and OWIN middlewares before proceeding to use this provider.

Many resources are available for understanding JWT (for example, see [JWT IO](https://jwt.io/) or [JSON Web Token](https://en.wikipedia.org/wiki/JSON_Web_Token)).

To learn more about OWIN, start with the [Overview of Project Katana](https://docs.microsoft.com/aspnet/aspnet/overview/owin-and-katana/an-overview-of-project-katana).

Additionally, you should know how the .NET [Configuration services](https://docs.asp.net/en/latest/fundamentals/configuration.html) the `ConfigurationBuilder` work and how to add providers to the builder.

With regard to Cloud Foundry, you should have a good understanding of Cloud Foundry OAuth2 security services (such as [UAA Server](https://github.com/cloudfoundry/uaa) or [TAS Single Signon](https://docs.pivotal.io/p-identity/)) along with an understanding how they use and issue JWT.

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

The Steeltoe sample is set up to read from `appsettings.json`.

>NOTE: The setting `SkipAuthIfNoBoundSSOService` was added in Steeltoe 2.2.0, and has a default value of `true` for backwards compatibility with previous versions. This setting was added to control functionality that was previously always-on. A future release is likely to change the default to `false` or may remove the functionality entirely.

### Cloud Foundry

As mentioned earlier, there are two auth services (UAA Server and TAS SSO) on Cloud Foundry. Rather than explaining how to create and bind those services to your app here, we recommend that you read the documentation provided by each of the service providers.

Regardless of which provider you choose, once the service is bound to your application, the settings are available in `VCAP_SERVICES`.

#### Add Cloud Foundry JwtAuthentication

In order to configure the Cloud Foundry OWIN JWT provider in your application, you will need an [OWIN Startup class](https://docs.microsoft.com/aspnet/aspnet/overview/owin-and-katana/owin-startup-class-detection) if you do not already have one, along with an `IConfigurationRoot` that includes a service binding for UAA or TAS SSO.

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
