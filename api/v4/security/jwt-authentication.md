# Resource Protection using JWT in ASP.NET Core

This provider lets you control access to REST resources by using JSON Web Tokens (JWT) issued by Cloud Foundry Security services (such as [UAA Server](https://github.com/cloudfoundry/uaa) or [TAS Single-Sign-on](https://docs.pivotal.io/p-identity)) in ASP.NET Core.

Other Steeltoe sample applications can help you understand how to use this tool, including `FreddysBBQ` (a polyglot microservices-based sample showing interoperability between Java and .NET on Cloud Foundry, secured with OAuth2 Security Services, and using Spring Cloud Services).

## Usage

This package uses JSON Web Tokens (JWT) and builds on JWT Security services provided by ASP.NET Core Security. You should take some time to understand both before proceeding to use this provider.

Many resources are available for understanding JWT (for example, see [JWT IO](https://jwt.io/) or [JSON Web Token](https://en.wikipedia.org/wiki/JSON_Web_Token)).

To get a good understanding of ASP.NET Core Security, see the [documentation](https://docs.microsoft.com/aspnet/core/) provided by Microsoft.

Additionally, you should know how the .NET [configuration services](https://docs.asp.net/en/latest/fundamentals/configuration.html) and the `ConfigurationBuilder` work and how to add providers to the builder.

You should also know how the ASP.NET Core [`Startup`](https://docs.asp.net/en/latest/fundamentals/startup.html) class is used in configuring the application services and how the middleware is used by the app. Pay particular attention to the usage of the `Configure()` and `ConfigureServices()` methods.

With regard to Cloud Foundry, you should have a good understanding of Cloud Foundry OAuth2 security services (such as [UAA Server](https://github.com/cloudfoundry/uaa) or [TAS Single Signon](https://docs.pivotal.io/p-identity/)) along with an understanding how they use and issue JWT.

To use the JWT Security provider:

1. Add NuGet package reference
1. Create and bind an instance of a Cloud Foundry OAuth2 service to your application.
1. (Optional) Configure any additional settings the security provider needs.
1. Add the Cloud Foundry configuration provider to the `ConfigurationBuilder`.
1. Add and use the security provider in the application.
1. Secure your endpoints.

### Add NuGet Reference

To use the provider, add a reference to the Steeltoe Cloud Foundry Security NuGet package, `Steeltoe.Security.Authentication.CloudFoundryCore`, with the NuGet package manager or directly to your project file by using the following `PackageReference`:

```xml
<ItemGroup>
...
    <PackageReference Include="Steeltoe.Security.Authentication.CloudFoundryCore" Version="3.2.0"/>
...
</ItemGroup>
```

### Configure Settings

Configuring additional settings for the provider is not typically required. However, when Cloud Foundry uses self-signed certificates, you might need to disable certificate validation, as follows:

```json
{
  "Security": {
    "Oauth2": {
      "Client": {
        "ValidateCertificates": false
      }
    }
  }
}
```

The JWT provider uses Microsoft's JWT implementation, and settings are based on `Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerOptions`, with these additional properties:

| Key | Description | Default |
| --- | --- | --- |
| `Timeout` | The timeout (in milliseconds) for calls to the auth server. | 100000 |
| `ValidateCertificates` | Validate Auth server certificate. | `true` |

>Each setting above must be prefixed with `Security:Oauth2:Client`.

The samples and most templates are already set up to read from `appsettings.json`.

### Cloud Foundry

As mentioned earlier. you can use a couple of OAuth2 services (such as UAA Server or TAS SSO) on Cloud Foundry. Rather than explaining how to create and bind OAuth2 services to your application here, we recommend that you read the documentation provided by each of the service providers.

Regardless of which provider you choose, once the service is bound to your application, the settings are available in `VCAP_SERVICES`.

#### Add Cloud Foundry JwtAuthentication

To use the provider in your application, add it to your service collection in the `ConfigureServices()` method of the `Startup` class, as follows:

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
}
```

The `AddCloudFoundryJwtBearer(Configuration)` method call configures and adds the Cloud Foundry JWT authentication service to the service container. Once in place, the authentication middleware can use it during request processing.

### Securing Endpoints

Once you have the work done in your `Startup` class, you can then start to secure endpoints by using the standard ASP.NET Core `Authorize` attribute.

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
