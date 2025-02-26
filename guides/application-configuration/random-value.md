---
uid: guides/application-configuration/random-value
title: Random Value Provider
tags: []
_disableFooter: true
_hideTocVersionToggle: true
---

> [!NOTE]
> This guide applies to Steeltoe v3. Please [open an issue](https://github.com/SteeltoeOSS/Documentation/issues/new/choose) if you'd like to help update the content for Steeltoe v4.

## Application Configuration Random Values

This tutorial takes you through setting up a .NET Core application that gets a random value for a config setting.

> [!NOTE]
> For more detailed examples, please refer to the [RandomValue](https://github.com/SteeltoeOSS/Samples/tree/3.x/Configuration/src/RandomValue) project in the [Steeltoe Samples Repository](https://github.com/SteeltoeOSS/Samples/tree/3.x).

First, **create a .NET Core WebAPI** that has a placeholder implemented.

1. Create a new ASP.NET Core WebAPI app with the [Steeltoe Initializr](https://start.steeltoe.io)
1. Name the project "RandomValueExample"
1. Add the "Random value" dependency
1. Click **Generate Project** to download a zip containing the new project
1. Extract the zipped project and open in your IDE of choice (we use Visual Studio)

**Run** the application

# [.NET cli](#tab/cli)

```powershell
dotnet run <PATH_TO>\RandomValueExample.csproj
```

Navigate to the endpoint (you may need to change the port number) [http://localhost:5000/api/values](http://localhost:5000/api/values)

# [Visual Studio](#tab/vs)

1. Choose the top _Debug_ menu, then choose _Start Debugging (F5)_. This should bring up a browser with the app running.
1. Navigate to the endpoint (you may need to change the port number) [http://localhost:8080/api/values](http://localhost:8080/api/values)

---

Once the app loads in the browser you will see three random values output.

`["<INTEGER>","<UUID>","<STRING>"]`
