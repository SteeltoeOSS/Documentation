# Usage

Before starting with Steeltoe's Windows Network File Share library, you should already have a plan for interacting with the file system. Steeltoe only manages the connection to the file share.

## Add NuGet References

To use the `WindowsNetworkFileShare` class, you need to add a reference to the `Steeltoe.Common.Net` NuGet package.

## Managing the Connection

The state of the connection to the file share is managed through the lifecycle of `WindowsNetworkFileShare`. To open the connection, instantiate a `WindowsFileShare`:

```csharp
using System.Net;
using Steeltoe.Common.Net;

var fileShare = new WindowsNetworkFileShare(@"\\server\path", new NetworkCredential("username", "password"));
```

The constructor opens the connection with the equivalent of the `net use` command.

To close the connection, call dispose on the `WindowsNetworkFileShare`:

```csharp
fileShare.Dispose();
```

Because `WindowsNetworkFileShare` implements `IDisposable`, you can also manage the `WindowsNetworkFileShare` with a `using` statement:

```csharp
using (new WindowsNetworkFileShare(@"\\server\path", new NetworkCredential("username", "password")))
{
    // Interact with an attached network share here.
}
```

### Managing Credentials

Credentials are generally required for interacting with SMB shares. `WindowsNetworkFileShare` does not have an opinion on where those credentials are stored. However, as a general guideline, storing credentials with your application's code or standard configuration is not recommended. Consider using the [CredHub Service Broker](https://docs.vmware.com/en/VMware-Tanzu-Application-Service/6.0/tas-for-vms/credhub-index.html) for storing and retrieving your credentials.

When used in conjunction with the Cloud Foundry Configuration Provider, you can access the values as follows:

```shell
dotnet add package Steeltoe.Configuration.CloudFoundry
dotnet add package Steeltoe.Common.Net
```

```csharp
using System.Net;
using Microsoft.Extensions.Options;
using Steeltoe.Common.Net;
using Steeltoe.Configuration.CloudFoundry;

var builder = WebApplication.CreateBuilder(args);
builder.AddCloudFoundryConfiguration();
builder.Services.AddSingleton<CredentialsReader>();

var app = builder.Build();

var reader = app.Services.GetRequiredService<CredentialsReader>();
var credentials = reader.ReadCredentials();

using (new WindowsNetworkFileShare(@"\\server\path", credentials))
{
    // Interact with an attached network share here.
}

public sealed class CredentialsReader(IOptionsMonitor<CloudFoundryServicesOptions> optionsMonitor)
{
    public NetworkCredential ReadCredentials()
    {
        foreach (CloudFoundryService service in optionsMonitor.CurrentValue.GetServicesOfType("credhub"))
        {
            if (service.Name == "my-network-share")
            {
                string? username = service.Credentials["share-username"].Value;
                string? password = service.Credentials["share-password"].Value;

                return new NetworkCredential(username, password);
            }
        }

        throw new InvalidOperationException("No credentials found for 'my-network-share'");
    }
}
```
