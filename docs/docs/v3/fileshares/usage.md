# Usage

Before starting with Steeltoe's Windows Network File Share library, you should already have a plan for interacting with the file system. Steeltoe only manages the connection to the file share.

## Add NuGet References

Use the NuGet Package Manager or a `PackageReference` to add a reference to `Steeltoe.Common.Net`

## Managing the Connection

The state of the connection to the file share is managed through the lifecycle of `WindowsNetworkFileShare`. To open the connection, instantiate a `WindowsFileShare`:

```csharp
var fileShare = new WindowsNetworkFileShare(@"\\server\path", new System.Net.NetworkCredential("username", "password"));
```

The constructor opens the connection with the equivalent of the `net use` command by calling [`WNetAddConnection2`](https://docs.microsoft.com/windows/desktop/api/winnetwk/nf-winnetwk-wnetaddconnection2a) with the information provided in the constructor.

To close the connection, call dispose on the `WindowsNetworkFileShare`:

```csharp
fileShare.Dispose();
```

Because `WindowsNetworkFileShare` implements `IDisposable`, you can also manage the `WindowsNetworkFileShare` with a `using` statement:

```csharp
using (new WindowsNetworkFileShare(@"\\server\path", new System.Net.NetworkCredential("username", "password")))
{
    // Interact with an attached network share here
};
```

>WARNING: Levels of support for accessing SMB shares on TAS for Windows may vary by installed version. Support for IP-based SMB shares was included with the initial 2.4 release and in patch releases for lower versions. Support for FQDN-based SMB shares will be included in PASW 2.5 and in patch releases for lower versions. See the [TAS Release notes](https://docs.pivotal.io/pivotalcf/2-4/pcf-release-notes/windows-rn.html) to confirm the relevant patch version required.

### Managing Credentials

Credentials are generally required for interacting with SMB shares. `WindowsNetworkFileShare` does not have an opinion on where those credentials are stored. However, as a general guideline, storing credentials with your application's code or standard configuration is not recommended. Consider using the [CredHub Service Broker](https://docs.pivotal.io/credhub-service-broker/) for storing and retrieving your credentials.

When used in conjunction with the Cloud Foundry Configuration Provider, you can access the values as follows:

```csharp
public SomeClassConstructor(IOptions<CloudFoundryServicesOptions> serviceOptions)
{
    var userName = serviceOptions.Services["credhub"]
       .First(q => q.Name.Equals("my-network-share"))
       .Credentials["share-username"].Value;
    var password = serviceOptions.Services["credhub"]
       .First(q => q.Name.Equals("my-network-share"))
       .Credentials["share-password"].Value;

    // create credential object to pass to WindowsNetworkFileShare
    var _shareCredential = new NetworkCredential(userName, password);
}
```
