---
title: Network File Share
order: 120
date: 2019/3/12
tags: smb, windows
---

Network shares are not the most cloud-native way to deal with files. For new development consider exploring message queues, caches, blob stores, and/or NoSQL stores. The alternatives offer greater resiliency and decoupling from backing services. With that said, sometimes the alternatives aren't viable. We're not here to judge you. For .NET applications deployed to Microsoft Windows Servers, Steeltoe provides a stepping stone towards cloud-native in the form of the `WindowsNetworkFileShare`. For applications deployed to Linux hosts on Pivotal Cloud Foundry, [volume services](https://docs.pivotal.io/pivotalcf/opsguide/enable-vol-services.html) is available.

# Windows Network File Shares

Steeltoe's `WindowsNetworkFileShare` provides a simplified experience for interacting with SMB file shares by making [P/Invoke calls](https://docs.microsoft.com/en-us/cpp/dotnet/how-to-call-native-dlls-from-managed-code-using-pinvoke) to underlying Windows APIs, specifically to `mpr.dll`. For more background on SMB, see [Microsoft's SMB documentation](https://docs.microsoft.com/en-us/windows/desktop/fileio/microsoft-smb-protocol-and-cifs-protocol-overview)

## 0.0 Initialize Dev Environment

All of the Steeltoe sample applications are in the same repository. If you have not already done so, use git to clone the [Steeltoe Samples](https://github.com/SteeltoeOSS/Samples) repository or download it with your browser from GitHub. The following git command shows how to clone the repository from the command line:

```bash
git clone https://github.com/SteeltoeOSS/Samples.git
```

>NOTE: All File Share samples in the Samples repository have a base path of `Samples/FileShares/src/`.

Make sure your Cloud Foundry CLI tools are logged in and targeting the correct org and space, as follows:

```bash
cf login [-a API_URL] [-u USERNAME] [-p PASSWORD] [-o ORG] [-s SPACE] [--skip-ssl-validation]
```

or

```bash
cf target -o <YourOrg> -s <YourSpace>
```

### 0.0.1 Setup File Share

If you do not already have a network file share available, you may use `create-user-and-share.ps1`, provided in the scripts folder, but please read this entire section before executing it. To run the script, execute it from a prompt with adminstrator-level permissions:

```powershell
.\Samples\FileShares\scripts\create-user-and-share.ps1
```

By default, this script will create a user named `shareWriteUser` with password `thisIs1Pass!` and a folder at `c:\steeltoe_network_share` that is shared as `steeltoe_network_share` with the user created in the script. All four of those values are configured as parameters for the script and can be set manually when executing the script. This is how they are defined:

```powershell
Param(
    [string]$shareName = "steeltoe_network_share",
    [string]$folderPath = "c:\steeltoe_network_share",
    [string]$username = "shareWriteUser",
    [string]$password = "thisIs1Pass!"
)
```

## 1.1 Quick Start

This quick start uses an ASP.NET Core application to show how to use the Steeltoe `WindowsNetworkFileShare` to interact with an SMB file share.

### 1.1.1 Locate Sample

First, you must navigate to the correct directory. If you plan to use Visual Studio, you will find the `.sln` file in `Samples/FileShares/src/AspNetCore`.

If you plan to run the sample from the command line or push it to Cloud Foundry later, enter the directory like this:

```bash
cd Samples/FileShares/src/AspNetCore/SMBFileShares
```

### 1.1.2 Set Credentials

If you are using an existing network share, or specified credentials other than the defaults, adjust these settings in `appsettings.Development.json` accordingly:

```json
"vcap": {
  "services": {
    "credhub": [{
        "credentials": {
            "location": "\\\\localhost\\steeltoe_network_share",
            "username": "shareWriteUser",
            "password": "thisIs1Pass!"
        }
    }]
  }
}
```

### 1.1.3 Run Sample

To start the sample from Visual Studio, start the sample with the F5 key, or by right-clicking the project name and selecting `Debug -> Start new instance`.

To start the sample from the commandline, execute `dotnet run`.

### 1.1.4 Publish and Push Sample

In order to run this sample on Cloud Foundry, you will need a fileshare that is [network-accessible](https://en.wikipedia.org/wiki/Server_Message_Block#Features) from your Cloud Foundry applications.

This sample was built with support for the [CredHub service broker](https://docs.pivotal.io/credhub-service-broker/). To add your credentials to a CredHub Service instance, you may use the included script `cf-create-service.ps1`. Again, you may override the location, username and password by setting parameters when executing the script:

```powershell
.\Samples\FileShares\scripts\cf-create-service.ps1 -networkAddress "\\someHost\\some_network_share" -username "alternativeUser" -password "alternativePassword"
```

If you do not wish to use the CredHub service broker, or it is not available for you, remove the services section from `manifest-windows.yml` and directly add the appropriate values to `appsettings.json` or any other included configuration source.

See [Publish Sample](#publish-sample) and the sections that follow for instructions on how to publish and push this sample to either Linux or Windows and subsequently view the logs.

### 1.1.5 Understand Sample

Whether you run the sample on your local machine or on Cloud Foundry, when you load up the application you should be presented with 3 links. Each of those links interacts with the file share by copying a test file, listing the contents of the directory, or deleting the test file.

This sample was created by using the `mvc` template from the .NET Core tooling and making several modifications:

* `SMBFileShares.csproj`: added `PackageReference`s for `Steeltoe.Common.Net` and `Steeltoe.Extensions.Configuration.CloudFoundryCore`
* `Program.cs`: `AddCloudFoundry()` adds the CloudFoundry config provider, `UseCloudFoundryHosting()` lets the app use the port assigned by Cloud Foundry
* `Startup.cs`: `services.ConfigureCloudFoundryOptions(Configuration);` adds `CloudFoundryServicesOptions` for retrieving service bindings
* `FilesController.cs`: reads fileshare information out of `CloudFoundryServicesOptions` and includes methods for creating `WindowsNetworkFileShare` and interacting with the file share.

## 1.2 Usage

Before starting with Steeltoe's Windows Network File Share library, you should already have a plan for interacting with the file system. Steeltoe will only be managing the connection to the file share.

### 1.2.1 Add NuGet References

Use the NuGet Package Manager or a `PackageReference` to add a reference to `Steeltoe.Common.Net`

### 1.2.2 Managing the Connection

The state of the connection to the file share is managed through the lifecycle of `WindowsNetworkFileShare`. In order to open the connection, instantiate a `WindowsFileShare`:

```csharp
var fileShare = new WindowsNetworkFileShare(@"\\server\path", new System.Net.NetworkCredential("username", "password"));
```

The constructor opens the connection with the equivalent of the `net use` command by calling [WNetAddConnection2](https://docs.microsoft.com/en-us/windows/desktop/api/winnetwk/nf-winnetwk-wnetaddconnection2a) with the information provided in the constructor.

In order to close the connection, call dispose on the `WindowsNetworkFileShare`.

```csharp
fileShare.Dispose();
```

Because `WindowsNetworkFileShare` implements `IDisposable`, you also have the ability to manage the `WindowsNetworkFileShare` with a using statement:

```csharp
using (new WindowsNetworkFileShare(@"\\server\path", new System.Net.NetworkCredential("username", "password")))
{
    // Interact with an attached network share here
}
```

>WARNING: Levels of support for accessing SMB shares on Pivotal Application Service for Windows (PASW) may vary by installed version. Support for IP-based SMB shares was included with the initial 2.4 release and in patch releases for lower versions. Support for FQDN-based SMB shares will be included in PASW 2.5 and in patch releases for lower versions. See the [PASW Release notes](https://docs.pivotal.io/pivotalcf/2-4/pcf-release-notes/windows-rn.html) to confirm the relevant patch version required.

### 1.2.3 Managing Credentials

Credentials are generally required for interacting with SMB shares. `WindowsNetworkFileShare` does not have an opinion on where those credentials are stored, but as a general guideline, storing credentials with your application's code or standard configuration is not recommended. Consider using the [CredHub Service Broker](https://docs.pivotal.io/credhub-service-broker/) for storing and retrieving your credentials.

When used in conjuction with the [Cloud Foundry Configuration Provider](../steeltoe-configuration/#1-0-cloud-foundry-provider), you can access the values in a relatively straightforward way:

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

# Common Steps

## Publish Sample

### ASP.NET Core

Use the `dotnet` CLI to [build and locally publish](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-publish) the application for the framework and runtime you will deploy the application to:

* Windows with .NET Core: `dotnet publish -f netcoreapp2.1 -r win10-x64`
* Windows with .NET Platform: `dotnet publish -f net461 -r win10-x64`

>NOTE: Starting with .NET Core 2.0, the `dotnet publish` command will automatically restore dependencies for you. Running `dotnet restore` explicitly is not generally required.

### ASP.NET 4.x

1. Open the solution for the sample in Visual Studio
1. Right click on the project, select "Publish"
1. Use the included `FolderProfile` to publish to `bin/Debug/net461/win10-x64/publish`

## Push Sample

Use the Cloud Foundry CLI to push the published application to Cloud Foundry using the parameters that match what you selected for framework and runtime, as follows:

```bash
 # Push to Windows cell, .NET Core
cf push -f manifest-windows.yml -p bin/Debug/netcoreapp2.1/win10-x64/publish

 # Push to Windows cell, .NET Framework
cf push -f manifest-windows.yml -p bin/Debug/net461/win10-x64/publish
```

>NOTE: All sample manifests have been defined to bind their application to service(s) as created above.
