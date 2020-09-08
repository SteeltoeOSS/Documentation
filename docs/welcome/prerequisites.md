# Prerequisites

While not required, we recommend installing one of the development tools ([Visual Studio](https://www.visualstudio.com/) or [Visual Studio Code](https://code.visualstudio.com/)) provided by Microsoft.

If you plan to develop with [.NET Core](https://docs.microsoft.com/en-us/dotnet/articles/core/) or [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/), you need to download and install the latest [.NET Core SDK](https://www.microsoft.com/net/download/core). Additionally, if you do not already know the language and framework well, we recommend you first spend time working through some of the following tutorials from Microsoft:

* [Getting Started with ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/getting-started)
* [Getting Started with C#](https://www.microsoft.com/net/tutorials/csharp/getting-started)

## NuGet Feeds

When developing applications with Steeltoe you need to pull the Steeltoe NuGet packages into your application.

To use the latest releases of Steeltoe, you can subscribe to any one of the following feeds, depending on your needs:

* [Release or Release Candidates](https://www.nuget.org/profiles/steeltoe)
* [Pre-release - Stable](https://www.myget.org/gallery/steeltoemaster)
* [Development - Less Stable](https://www.myget.org/gallery/steeltoedev)

The following example shows a NuGet.config file that you can edit and use when developing applications with Steeltoe:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="SteeltoeMaster" value="https://www.myget.org/F/steeltoemaster/api/v3/index.json" />
    <add key="SteeltoeDev" value="https://www.myget.org/F/steeltoedev/api/v3/index.json" />
    <add key="NuGet" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
```

>If you want to use only release or pre-release (RC) versions of Steeltoe, you need not make any changes to your NuGet.config file, as those are served from `nuget.org`.

## Quick Starts

For many of the Steeltoe components, we provide Quick Start samples and a guide that describes how to quickly get a sample application up and running by using a particular Steeltoe component. A detailed breakdown of the sample code, describing how Steeltoe has been integrated into the application, is provided.

In many cases, these guides provide two ways to exercise the applications: one that describes how to create and run the application locally on your development machine and a second that describes running the application on Cloud Foundry.

For the Quick Starts in which we run the application locally, Java is required to run instances of some of the servers (such as Spring Cloud Config Server, Netflix Eureka Server, and others) on your machine. If you do not have Java available on your machine, you may want to install that now.

For the Quick Starts that run on Cloud Foundry, you need access to a Cloud Foundry environment that has the appropriate services (such as Spring Cloud Config Server, Netflix Eureka Server, and others) installed. One option is to run [PCF Dev](https://docs.pivotal.io/pcf-dev/), the local developer version of Pivotal Cloud Foundry on your development machine. PCF Dev depends on Virtual Box, so, depending on your desktop operating system and configuration, you may not be able to use it.

To work with Cloud Foundry, you need to install the [Cloud Foundry Command Line Interface (CLI)](https://github.com/cloudfoundry/cli/releases).

Finally, for all of the Quick Starts, you need to install the [GIT command line tools](https://git-scm.com/book/en/v2/Getting-Started-Installing-Git) so that you can fetch the Quick Start sample code and work with it on your computer.
