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
    <PackageReference Include="Steeltoe.Security.DataProtection.CredHubCore" Version="2.5.2" />
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

The samples and most templates are already set up to read from `appsettings.json`.

### On Cloud Foundry

TAS (in versions 2.0 and up) ships with a version of CredHub Server with which this client works. To use UAA authentication, you will need a user with `credhub.read` and/or `credhub.write` claims.

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

All `CredHubClient` Read operations operate asynchronously and do not change the credentials or permissions stored in CredHub. Refer to the [CredHub documentation](https://credhub-api.cfapps.io/) for more detail. For brevity, the samples shown later in this guide use `_credHubClient` to reference an instance of `CredHubClient` that has been created previously.

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

One of the more powerful features of CredHub is the `Interpolate` endpoint. With one request, you may retrieve N number of credentials that have been stored in CredHub. To use it from .NET, call `await _credHub.InterpolateServiceDataAsync(serviceData)`, where `serviceData` is the string representation of `VCAP_SERVICES`. `CredHubClient` returns the interpolated `VCAP_SERVICES` data as a string.

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

All `CredHubClient` Change operations operate asynchronously and affect stored credentials. Refer to the [CredHub documentation](https://credhub-api.cfapps.io/) for more detail. For brevity, the samples shown later in this guide use `_credHubClient` to reference an instance of `CredHubClient` that has been created previously.

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
