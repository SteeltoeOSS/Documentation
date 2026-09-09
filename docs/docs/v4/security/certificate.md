# Resource Protection using Mutual TLS in ASP.NET Core

Certificate Authentication, also known as Mutual TLS, is a way for a client and server to validate each other's identity. This method is commonly used to secure service-to-service communications.

This library is a supplement to [ASP.NET Core Certificate Authentication](https://learn.microsoft.com/aspnet/core/security/authentication/certauth), adding functionality that helps you use [Cloud Foundry Instance Identity certificates](https://docs.cloudfoundry.org/devguide/deploy-apps/instance-identity.html) and authorization policies based on certificate data.
Additionally, resources are included for automatically generating certificates for local development that resemble what is found on the platform.

## Using Mutual TLS in ASP.NET Core

To use this provider, the following steps are required:

1. Add NuGet package reference.
1. Add identity certificates to the configuration.
1. Add and use the security provider in the application.
1. Secure your endpoints.
1. Attach certificate to requests to secured endpoints.
1. (Optional) Add support for additional intermediate certificate authorities.

### Add NuGet Reference

> [!NOTE]
> This step is required for all applications that are sending or receiving certificate-authorized requests.

To use Certificate Authorization, add a reference to the `Steeltoe.Security.Authorization.Certificate` NuGet package.

### Add Identity Certificates to Configuration

> [!NOTE]
> This step is required for all applications that are sending or receiving certificate-authorized requests.

In a Cloud Foundry environment, instance identity certificates are automatically provisioned (and rotated on a regular basis) for each application instance.
Steeltoe provides the extension method `AddAppInstanceIdentityCertificate` to find the location of the certificate files from the environment variables `CF_INSTANCE_CERT` and `CF_INSTANCE_KEY`.
When running outside of Cloud Foundry, this method automatically generates similar certificates.
Use the optional parameters to coordinate `orgId` and/or `spaceId` between your applications to facilitate communication when running outside of Cloud Foundry.

This code adds the certificate paths to the configuration for use later (and generates the instance identity certificate when running outside Cloud Foundry):

```csharp
using Steeltoe.Common.Certificates;

const string orgId = "a8fef16f-94c0-49e3-aa0b-ced7c3da6229";
const string spaceId = "122b942a-d7b9-4839-b26e-836654b9785f";

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddAppInstanceIdentityCertificate(new Guid(orgId), new Guid(spaceId));
```

When running locally, the preceding code sample creates a chain of self-signed certificates. The application instance identity certificate is created with a subject containing an OrgId of `a8fef16f-94c0-49e3-aa0b-ced7c3da6229` and a SpaceId of `122b942a-d7b9-4839-b26e-836654b9785f`.
A root certificate and intermediate certificate are created on disk one level above the current project in a directory named `GeneratedCertificates`.
The root and intermediate certificates are automatically shared between applications housed within the same solution, so that the applications can trust each other.

### Add and use Certificate Authentication

> [!NOTE]
> This section is required only for applications that are receiving certificate-authorized requests.

Several steps need to happen before certificate authorization policies can be used to secure resources:

1. Bind configuration values into named `CertificateOptions`.
1. Monitor certificate files for changes (to stay up to date when certificates are rotated).
1. Configure certificate forwarding (so that ASP.NET reads the certificate forwarded by the Cloud Foundry Gorouter).
1. Add authentication services.
1. Add authorization services and policies.
1. Activate middleware.

Fortunately, all of these requirements can be satisfied with a handful of extension methods:

```csharp
using Steeltoe.Security.Authorization.Certificate;

// Register Microsoft's Certificate Authentication library
builder.Services
    .AddAuthentication()
    .AddCertificate();

// Register Microsoft authorization services
builder.Services.AddAuthorizationBuilder()
    // Register Steeltoe components and policies requiring org and/or space to match between client and server certificates
    .AddOrgAndSpacePoliciesForMutualTls();
```

> [!TIP]
> `AddOrgAndSpacePoliciesForMutualTls` configures the certificate forwarding middleware to trust only the `X-Forwarded-Client-Cert` HTTP header.
> This requires the Cloud Foundry Gorouter to be configured as the point of TLS termination. When configured correctly, Gorouter terminates the mutual TLS handshake itself and forwards the verified client certificate in the `X-Forwarded-Client-Cert` header, while stripping any instance of that header sent by the original client. This means a request cannot spoof its identity by setting the header directly, because Gorouter overwrites or removes it before the request reaches the app. If TLS termination is not enabled at the router, this header is never set and the configured policies will reject every request.

Steeltoe exposes some of the policy-related components directly if more customized scenarios are required:

```csharp
// AuthorizationPolicyBuilder setup
builder.Services.AddAuthorizationBuilder()
    .AddOrgAndSpacePoliciesForMutualTls()
    .AddDefaultPolicy("sameOrgAndSpace", policy => policy.RequireSameOrg().RequireSameSpace());

// Or the equivalent using different syntax
builder.Services.AddAuthorizationBuilder()
    .AddOrgAndSpacePoliciesForMutualTls()
    .AddPolicy("sameOrgAndSpace", policy => policy.AddRequirements(new SameOrgRequirement(), new SameSpaceRequirement()));
```

> [!NOTE]
> The original `AddOrgAndSpacePolicies` method (and its overload accepting a custom header name) is marked obsolete since Steeltoe 4.3.0: it trusts whatever HTTP header is configured (`X-Client-Cert` by default) without verifying that a reverse proxy set it after a successful mTLS handshake, so any client can set that header itself and spoof its identity. It remains available to support scenarios where client and server applications are upgraded one side at a time.

To activate certificate-based authorization in the request pipeline, use the `UseCertificateAuthorization` extension method on `IApplicationBuilder`:

```csharp
var app = builder.Build();

// Steeltoe: Use certificate forwarding along with ASP.NET Core Authentication and Authorization middleware
app.UseCertificateAuthorization();
```

> [!NOTE]
> This feature requires the application to be compatible with reverse-proxy scenarios, such as when running on Cloud Foundry.
> [Reverse-proxy support is automatically configured by the configuration provider for Cloud Foundry](../configuration/cloud-foundry-provider.md#reverseproxy-and-forwarded-headers-support).

### Securing Endpoints

> [!NOTE]
> This step is required only for applications that are receiving certificate-authorized requests.

As implied by the name of the extension method `AddOrgAndSpacePoliciesForMutualTls` (from the previous section in this topic), Steeltoe provides policies for validating that a request came from an application in the same org and/or the same space. You can secure endpoints using the standard ASP.NET Core `Authorize` attribute with these security policies.

> [!TIP]
> For more information about authorization in ASP.NET Core, see the [Microsoft documentation](https://learn.microsoft.com/aspnet/core/security/authorization/introduction).

The following example shows a controller using the security attributes with the included policies:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Steeltoe.Security.Authorization.Certificate;

[Route("api")]
public class HomeController : ControllerBase
{
    [Authorize(CertificateAuthorizationPolicies.SameOrg)]
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

    [Authorize(CertificateAuthorizationPolicies.SameOrg)]
    [Authorize(CertificateAuthorizationPolicies.SameSpace)]
    [HttpGet("[action]")]
    public string SameOrgAndSpaceCheck()
    {
        return "Certificate is valid and both client and server are in the same org and space";
    }
}
```

In the preceding example, when an incoming request is made to the `SameOrgCheck` endpoint, the request is evaluated for the presence of a certificate. If a certificate is not present, the user is denied access. If a certificate is present, its subject is evaluated for the presence of an `org` value, which is then compared with the `org` value in the certificate found on disk where the service is deployed. If the values do not match, the user is denied access. The same process is applied for `SameSpaceCheck`, with the only difference being a check for the `space` value instead of the `org` value.

### Communicating with Secured Services

To use app instance identity certificates in a client application, services must be configured, but nothing needs to be activated in the ASP.NET Core request pipeline.

#### IHttpClientFactory integration

> [!NOTE]
> This step is required only for applications that are sending certificate-authorized requests.

For applications that need to send identity certificates in outgoing requests, Steeltoe provides a smooth experience through an extension method on `IHttpClientBuilder` called `AddAppInstanceIdentityCertificateForMutualTls`.
This method invokes code that handles loading certificates from paths defined in the application's configuration, monitors those file paths and their content for changes, and attaches the certificate to outbound requests using a real mutual TLS handshake.

> [!TIP]
> For more information about `IHttpClientFactory`, see the [Microsoft documentation](https://learn.microsoft.com/aspnet/core/fundamentals/http-requests).

```csharp
using Steeltoe.Security.Authorization.Certificate;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient<ExampleApiClient>().AddAppInstanceIdentityCertificateForMutualTls();
```

> [!NOTE]
> mTLS requires `SocketsHttpHandler` as the primary handler for the `HttpClient`. If none was explicitly configured, one is created automatically. If an incompatible primary handler was explicitly configured, an `InvalidOperationException` is thrown on .NET 9 and later; on .NET 8, the handler is silently replaced (logged at Debug level).

To send a certificate to a service that has not yet been upgraded to trust mTLS, use `AddAppInstanceIdentityCertificate()` (or the `certificateName`-based `AddClientCertificate` overloads) instead. These methods inject the certificate into an HTTP header (`X-Client-Cert` by default, or a caller-supplied name) and are obsolete since Steeltoe 4.3.0 for the same reason described above, but remain available to support one-sided upgrade scenarios during a transition. For example: `.AddAppInstanceIdentityCertificate("X-Custom-Certificate-Header")`.

### Customizing CertificateAuthenticationOptions

In some scenarios (particularly when running applications across Linux and Windows cells in Cloud Foundry), you may encounter issues where instance identity certificates are not trusted, even though they are properly issued.
This usually happens because the identity certificates are signed by different intermediate certificates, depending on the operating system.

If the intermediate certificate from one environment is not included in the trust store of the other, authentication can fail with errors, such as:

```text
Certificate validation failed... NotSignatureValid The signature of the certificate cannot be verified.
```

To resolve this, you can manually extract the intermediate certificate from a trusted identity certificate, export it to a .crt file, and then configure `CertificateAuthenticationOptions` to include it.

1. Extract the identity certificate:

    ```shell
    cf ssh your-app-name
    cat /etc/cf-instance-credentials/instance.crt
    ```

1. Copy everything from the second `-----BEGIN CERTIFICATE-----` to the ending `-----END CERTIFICATE-----` and save it all in a separate file (such as `intermediate.crt`)

1. Now that you have the intermediate certificate as its own .crt file, configure CertificateAuthenticationOptions to include it during the certificate chain validation:

    ```csharp
    using System.Security.Cryptography.X509Certificates;
    using Microsoft.AspNetCore.Authentication;

    builder.Services
        .AddAuthentication()
        .AddCertificate(options =>
        {
            X509Certificate2 certificate = new X509Certificate2("intermediate.crt");
            options.AdditionalChainCertificates.Add(certificate);
        });
    ```
