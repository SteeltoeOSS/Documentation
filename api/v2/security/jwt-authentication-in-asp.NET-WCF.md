# Resource Protection using JWT in ASP.NET Core

This provider lets you control access to REST resources by using JWT tokens issued by Cloud Foundry Security services (such as [UAA Server](https://github.com/cloudfoundry/uaa) or [TAS Single-Sign-on](https://docs.pivotal.io/p-identity)) in ASP.NET Core, ASP.NET WebAPI and WCF.

Other Steeltoe sample applications can help you understand how to use this tool, including:

* `FreddysBBQ`: A polyglot microservices-based sample showing interoperability between Java and .NET on Cloud Foundry, secured with OAuth2 Security Services, and using Spring Cloud Services.

## Usage

This package is a custom authorization provider for using JWT Bearer tokens in Windows Communication Foundation (WCF) applications. The provider is built on `System.IdentityModel.Claims`. You should take some time to understand both JWT and WCF security before proceeding to use this provider.

Many resources are available for understanding JWT (for example, see [JWT IO](https://jwt.io/) or [JSON Web Token](https://en.wikipedia.org/wiki/JSON_Web_Token)).

Additionally, you should know how the .NET [Configuration services](https://docs.asp.net/en/latest/fundamentals/configuration.html) the `ConfigurationBuilder` work and how to add providers to the builder.

With regard to Cloud Foundry, you should have a good understanding of Cloud Foundry OAuth2 security services (such as [UAA Server](https://github.com/cloudfoundry/uaa) or [TAS Single Signon](https://docs.pivotal.io/p-identity/)) along with an understanding how they use and issue JWT.

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

The Steeltoe sample is set up to read from `appsettings.json`.

|Name|Description|Default|
|---|---|---|
|forwardUserCredentials|Whether to use app credentials or forward users's credentials|`false`|
|validateAudience|Whether or not a token's audience should be validated|`true`|
|validateIssuer|Whether or not a token's issuer should be validated|`true`|
|validateLifeTime|Whether or not a token's lifetime should be validated|`true`|
|validateCertificates|Validate Auth server certificate|`true`|

### Cloud Foundry

As mentioned earlier. you can use a couple of OAuth2 services (such as UAA Server or TAS SSO) on Cloud Foundry. Rather than explaining how to create and bind OAuth2 services to your app here, we recommend that you read the documentation provided by each of the service providers.

Regardless of which provider you choose, once the service is bound to your application, the settings are available in `VCAP_SERVICES`.

#### Set ServiceAuthorizationManager

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
