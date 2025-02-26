# Prerequisites

While not required, we recommend installing one of the development tools ([Visual Studio](https://www.visualstudio.com/) or [Visual Studio Code](https://code.visualstudio.com/)) provided by Microsoft.

If you plan to develop with [.NET](https://learn.microsoft.com/dotnet/fundamentals) or [ASP.NET Core](https://learn.microsoft.com/aspnet/core), you will need to download and install the latest [.NET SDK](https://dotnet.microsoft.com/download). Additionally, if you do not already know the language and framework well, we recommend you first spend time working through some of the following tutorials from Microsoft:

* [Getting Started with C#](https://learn.microsoft.com/dotnet/csharp)
* [Getting Started with ASP.NET Core](https://learn.microsoft.com/aspnet/core/getting-started)

## NuGet Feeds

When developing applications with Steeltoe, you need to pull the Steeltoe NuGet packages into your project.

To use the latest *unstable* packages from the developer feed, create a `Nuget.config` file with the contents below.

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <!-- Stable release and release candidates -->
    <add key="NuGet" value="https://api.nuget.org/v3/index.json" />
    <!-- Latest unstable packages from CI builds -->
    <add key="SteeltoeDev" value="https://pkgs.dev.azure.com/dotnet/Steeltoe/_packaging/dev/nuget/v3/index.json" />
  </packageSources>
</configuration>
```
