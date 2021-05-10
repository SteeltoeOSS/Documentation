# Prerequisites

While not required, we recommend installing one of the development tools ([Visual Studio](https://www.visualstudio.com/) or [Visual Studio Code](https://code.visualstudio.com/)) provided by Microsoft.

If you plan to develop with [.NET Core](https://docs.microsoft.com/dotnet/fundamentals/) or [ASP.NET Core](https://docs.microsoft.com/aspnet/core/), you will need to download and install the latest [.NET SDK](https://dotnet.microsoft.com/download). Additionally, if you do not already know the language and framework well, we recommend you first spend time working through some of the following tutorials from Microsoft:

* [Getting Started with ASP.NET Core](https://docs.microsoft.com/aspnet/core/getting-started)
* [Getting Started with C#](https://www.microsoft.com/net/tutorials/csharp/getting-started)

>NOTE: Many of the Steeltoe packages can also be used with .NET Framework and ASP.NET 4 based applications. You are not required to target .NET/ASP.NET Core when using Steeltoe.

## NuGet Feeds

When developing applications with Steeltoe, whether on .NET Core or .NET Framework, you need to pull the Steeltoe NuGet packages into your application.

The following example shows a NuGet.config file that you can edit and use when developing applications with Steeltoe:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <!-- Release or Release Candidates -->
    <add key="NuGet" value="https://api.nuget.org/v3/index.json" />
    <!-- The Development feed provides the latest packages (all CI builds) -->
    <add key="SteeltoeDev" value="https://pkgs.dev.azure.com/dotnet/Steeltoe/_packaging/dev/nuget/v3/index.json" />
  </packageSources>
</configuration>
```

>NOTE: If you only want to use release or pre-release (RC) versions of Steeltoe, you need not make any changes to your NuGet.config file, as those are served from `nuget.org`.
