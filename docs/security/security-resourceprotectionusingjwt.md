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