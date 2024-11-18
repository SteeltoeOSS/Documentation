# Resource Protection using Mutual TLS in ASP.NET Core

This component builds on top of [ASP.Net Core's Certificate Authentication](https://docs.microsoft.com/aspnet/core/security/authentication/certauth), with the addition of automatic configuration for [Cloud Foundry Instance Identity certificates](https://docs.cloudfoundry.org/devguide/deploy-apps/instance-identity.html) and authorization policies based on certificate data. Additionally, resources are included for automatically generating certificates for local development that resemble what is found on the platform.

## Usage

In order to use this provider, the following steps are required:

1. Add NuGet package reference
1. Add identity certificates to configuration
1. Configure authentication services
1. Include services in ASP.NET Core pipeline
1. Secure Endpoints
1. Attach certificate to requests to secured endpoints

### Add NuGet Reference

To use the provider, add a reference to the Steeltoe Cloud Foundry Security NuGet package, `Steeltoe.Security.Authentication.CloudFoundryCore`, with the NuGet package manager or directly to your project file by using the following `PackageReference`:

```xml
<ItemGroup>
...
    <PackageReference Include="Steeltoe.Security.Authentication.CloudFoundryCore" Version="3.2.0"/>
...
</ItemGroup>
```

>This step is required on services that are sending or receiving mTLS-secured requests

### Configure Settings

In a Cloud Foundry setting, instance identity certificates are automatically provisioned (and rotated on a regular basis) for each application instance. Steeltoe provides the `AddCloudFoundryContainerIdentity` extension to find the location of the certificate files from the environment variables `CF_INSTANCE_CERT` and `CF_INSTANCE_KEY`. When running outside of Cloud Foundry, this extension will automatically generate similar certificates. Use the optional parameters specify a space and/or org id to facilitate communication between services that are using this form of security.

This sample code adds the certificate paths (and creates the certificates when running off-platform) to configuration for use later:

```csharp
public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration(cfg => cfg.AddCloudFoundryContainerIdentity("a8fef16f-94c0-49e3-aa0b-ced7c3da6229", "122b942a-d7b9-4839-b26e-836654b9785f"));
```

The above example will create self-signed certificates with an OrgId of `a8fef16f-94c0-49e3-aa0b-ced7c3da6229` and a SpaceId of `122b942a-d7b9-4839-b26e-836654b9785f` when running locally.

>This step is required on services that are sending or receiving mTLS-secured requests

### Securing Endpoints

In order to use identity certificates for authorization in a service application, services need to be configured and activated and policies need to be applied.

#### Adding and using services

Several steps need to happen to add all of the required services to the DI container: the configuration values need to be read into `CertificateOptions`, a monitoring service for certificate rotation needs to be configured to ensure the options stay up to date, certificate forwarding needs to be configured, and authorization needs to be added and authorization policies need to be configured.

Fortunately, all of that can be done by adding this one line to `ConfigureServices` inside `startup.cs`:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    ...
    services.AddCloudFoundryCertificateAuth(Configuration);
    ...
}
```

To enable the application of certificate-based authorization to the authentication services in the request pipeline, add this line to `startup.cs` after the call to `AddRouting` in the `Configure` method:

```csharp
public void Configure(IApplicationBuilder app, ...)
{
    ...
    // use the auth middleware in the pipeline
    app.UseCloudFoundryCertificateAuth();
    ...
}
```

>These steps are only required on services that are receiving mTLS-secured requests

#### Applying Authorization Policies

Steeltoe includes policies for validating that a request came from an application in the same org or the space. Once you have done the work in the `Startup` class, you can secure endpoints by using the standard ASP.NET Core `Authorize` attribute with one of these security policies.

See the Microsoft documentation on [ASP.NET Core Security](https://docs.asp.net/en/latest/security/) for a better understanding of how to use these attributes.

The following example shows a controller using the security attributes with the included policies:

```csharp
[Route("api")]
public class HomeController : ControllerBase
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    [Authorize(CloudFoundryDefaults.SameOrganizationAuthorizationPolicy)]
    [HttpGet("[action]")]
    public string SameOrgCheck()
    {
        _logger.LogDebug("Received a request with a client certificate from the same org");
        return "Certificate is valid and both client and server are in the same org";
    }

    [Authorize(CloudFoundryDefaults.SameSpaceAuthorizationPolicy)]
    [HttpGet("[action]")]
    public string SameSpaceCheck()
    {
        _logger.LogDebug("Received a request with a client certificate from the same space");
        return "Certificate is valid and both client and server are in the same space";
    }
}
```

In the preceding example, when an incoming request is made to the `SameOrgCheck` endpoint, the request is evaluated for the presence of a certificate. If a certificate is not present, the request is rejected. If a certificate is present, the subject is evaluated for the presence of an `org` value, which is then compared with the `org` value in the certificate found on disk where the service is deployed. If the values do not match, the request is rejected. The same process is applied for `SameSpaceCheck`, with the only difference being a check for the `space` value instead of the `org` value.

>This step is only required on services that are receiving mTLS-secured requests

### Communicating with Secured Services

In order to use identity certificates in a client application, services need to be configured, but don't necessarily need to be activated in the ASP.NET Core request pipeline.

#### Adding services

For applications that are only using identity certificates to make requests, configure the `CertificateOptions` and setup the certificate rotation service by adding this line to `ConfigureServices` in `startup.cs`:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    ...
    services.AddCloudFoundryContainerIdentity(Configuration);
    ...
}
```

>This step is for use with services that are only sending mTLS-secured requests. For applications that both send and receive mTLS requests, use [the instructions above](#adding-and-using-services)

#### Sending Requests to Secured Endpoints

Endpoints that are secured with Steeltoe's mTLS component expect the certificate to be provided as a Base64-encoded string in a header named `X-Forwarded-Client-Cert`.

Attach the certificate to outgoing requests from an `HttpClient` by configuring it with `HttpClientFactory` as seen in this example:

```csharp
services.AddHttpClient("default", (services, client) =>
{
    var options = services.GetService<IOptions<CertificateOptions>>();
    var b64 = Convert.ToBase64String(options.Value.Certificate.Export(X509ContentType.Cert));
    client.DefaultRequestHeaders.Add("X-Forwarded-Client-Cert", b64);
});
```

>This step is only required on services that are sending mTLS-secured requests

Should you need to customize or disable the certificate validation on clients making requests to secured endpoints (such as with certificates that are self-signed or use an un-trusted root), it is possible to configure the primary http message handler used in the `HttpClientFactory` with code like this:

```csharp
.ConfigurePrimaryHttpMessageHandler((isp) => new HttpClientHandler { ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true })
```
