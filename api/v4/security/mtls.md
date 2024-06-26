# Resource Protection using Mutual TLS in ASP.NET Core

This component builds on top of [ASP.NET Core's Certificate Authentication](https://docs.microsoft.com/aspnet/core/security/authentication/certauth), with the addition of automatic configuration for [Cloud Foundry Instance Identity certificates](https://docs.cloudfoundry.org/devguide/deploy-apps/instance-identity.html) and authorization policies based on certificate data. Additionally, resources are included for automatically generating certificates for local development that resemble what is found on the platform.

## Usage

In order to use this provider, the following steps are required:

1. Add NuGet package reference
1. Add identity certificates to configuration
1. Configure authentication and authorization services
1. Include services in ASP.NET Core pipeline
1. Secure endpoints
1. Attach certificate to requests to secured endpoints

### Add NuGet Reference

To use Certificate Authorization, you need to add a reference to the `Steeltoe.Security.Authorization.Certificate` NuGet package.

> [!NOTE]
> This step is required on all applications that are sending or receiving certificate-authorized requests.

### Configure Settings

In a Cloud Foundry environment, instance identity certificates are automatically provisioned (and rotated on a regular basis) for each application instance. Steeltoe provides the `AddAppInstanceIdentityCertificate` extension method to find the location of the certificate files from the environment variables `CF_INSTANCE_CERT` and `CF_INSTANCE_KEY`. When running outside of Cloud Foundry, this method will automatically generate similar certificates. Use the optional parameters to coordinate `organizationId` and/or `spaceId` between your applications to facilitate communication when running outside of Cloud Foundry.

This code adds the certificate paths to configuration for use later (and generates the instance identity certificate when running outside Cloud Foundry):

```csharp
using Steeltoe.Common.Certificates;

const string organizationId = "a8fef16f-94c0-49e3-aa0b-ced7c3da6229";
const string spaceId = "122b942a-d7b9-4839-b26e-836654b9785f";

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddAppInstanceIdentityCertificate(new Guid(organizationId), new Guid(spaceId));
```

When running locally, the code shown above will create a chain of self-signed certificates and the application instance identity certificate will have a subject containing an OrgId of `a8fef16f-94c0-49e3-aa0b-ced7c3da6229` and a SpaceId of `122b942a-d7b9-4839-b26e-836654b9785f`. A root certificate and intermediate certificate are created on disk one level above the current project in a directory named `GeneratedCertificates`. The root and intermediate certificates will automatically be shared between applications housed within the same solution, so that the applications will be able to trust each other.

> [!NOTE]
> This step is required on all applications that are sending or receiving certificate-authorized requests.

### Securing Endpoints

In order to authorize incoming requests using an identity certificate, services need to be configured and activated, and polices need to be applied.

#### Adding and using services

Several steps need to happen before certificate authorization policies can be used to secure resources:

1. Configuration values need to be bound into named `CertificateOptions`
1. Certificate files need to be monitored for changes (to stay up to date when certificates are rotated)
1. Certificate forwarding needs to be configured (so that ASP.NET reads the certificate out of an HTTP Header)
1. Authentication services need to be added
1. Authorization services and policies need to be added
1. Middleware need to be activated

Fortunately, all of the requirements can be satisfied with a handful of extension methods:

```csharp
using Steeltoe.Security.Authorization.Certificate;

// Register Microsoft's Certificate Authentication library
builder.Services
    .AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme)
    .AddCertificate();

// Register Microsoft authorization services
builder.Services.AddAuthorizationBuilder()
    // Register Steeltoe components and policies requiring space or org to match between client and server certificates
    .AddOrgAndSpacePolicies();
```

> [!TIP]
> Steeltoe configures the certificate forwarding middleware to look for a certificate in the `X-Client-Cert` HTTP header.

To activate certificate-based authorization in the request pipeline, use the `IApplicationBuilder` extension method `UseCertificateAuthorization`:

```csharp
WebApplication app = builder.Build();

// Steeltoe: Use certificate and header forwarding along with ASP.NET Core Authentication and Authorization middleware
app.UseCertificateAuthorization();
```

Steeltoe exposes some of the policy-related components directly if more customized scenarios are required:

```csharp
// AuthorizationPolicyBuilder extensions
builder.Services.AddAuthorizationBuilder().AddOrgAndSpacePolicies()
        .AddDefaultPolicy("sameOrgAndSpace", authorizationPolicyBuilder => authorizationPolicyBuilder.RequireSameOrg().RequireSameSpace());

// Requirement classes are public
builder.Services.AddAuthorizationBuilder().AddOrgAndSpacePolicies()
        .AddPolicy("sameOrgAndSpace",
            authorizationPolicyBuilder => authorizationPolicyBuilder.AddRequirements([
                new SameOrgRequirement(),
                new SameSpaceRequirement()
            ]));
```

> [!NOTE]
> These steps are only required on applications that are receiving certificate-authorized requests.

#### Applying Authorization Polices

As implied by the name of the extension method `AddOrgAndSpacePolicies` from the previous section on this page, Steeltoe provides policies for validating that a request came from an application in the same org or the same space. You can secure endpoints by using the standard ASP.NET Core `Authorize` attribute with one of these security policies.

> [!TIP]
> If needed, see the Microsoft documentation on [authorization in ASP.NET Core](https://learn.microsoft.com/aspnet/core/security/authorization/introduction) for a better understanding of how to use these attributes.

The following example shows a controller using the security attributes with the included policies:

```csharp
[Route("api")]
public class HomeController : ControllerBase
{
    [Authorize(CertificateAuthorizationPolicies.SameOrganization)]
    [HttpGet("[action]")]
    public string SameOrgCheck()
    {
        return "Certificate is valid and both client and server are in the same org";
    }

    [Authorize(CertificateAuthorizationPolicies.SameSpace)]
    [HttpGet("[action]")]
    public string SameSpaceCheck()
    {
        return "Certificate is valid and both client and server are in the same space";
    }

    [Authorize("sameOrgAndSpace")]
    [HttpGet("[action]")]
    public string SameOrgAndSpaceCheck()
    {
        return "Certificate is valid and both client and server are in the same org and space";
    }
}
```

In the preceding example, when an incoming request is made to the `SameOrgCheck` endpoint, the request is evaluated for the presence of a certificate. If a certificate is not present, the user is denied access. If a certificate is present, its subject is evaluated for the presence of an `org` value, which is then compared with the `org` value in the certificate found on disk where the service is deployed. If the values do not match, the user is denied access. The same process is applied for `SameSpaceCheck`, with the only difference being a check for the `space` value instead of the `org` value.

> [!NOTE]
> This step is only required on applications that are receiving certificate-authorized requests.

### Communicating with Secured Services

In order to use app instance identity certificates in a client application, services need to be configured, but nothing needs to be activated in the ASP.NET Core request pipeline.

#### IHttpClientFactory integration

For applications that need to send identity certificates in outgoing requests, Steeltoe provides a smooth experience through an `IHttpClientBuilder` extension method named `AddAppInstanceIdentityCertificate`. This method invokes code that handles loading certificates from paths defined in the application's configuration, monitors those file paths and their content for changes, and places the certificate in an HTTP header named `X-Client-Cert` on all outbound requests.

> [!TIP]
> If needed, see the Microsoft documentation on [IHttpClientFactory documentation](https://learn.microsoft.com/aspnet/core/fundamentals/http-requests)

```csharp
builder.Services.AddHttpClient("AppInstanceIdentity").AddAppInstanceIdentityCertificate();
```

> [!NOTE]
> This step is only required on applications that are sending certificate-authorized requests.