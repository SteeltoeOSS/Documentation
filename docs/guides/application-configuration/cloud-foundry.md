---
uid: guides/application-configuration/cloud-foundry
title: Cloud Foundry Provider
tags: []
_disableFooter: true
_hideTocVersionToggle: true
---

> [!NOTE]
> This guide applies to Steeltoe v3. Please [open an issue](https://github.com/SteeltoeOSS/Documentation/issues/new/choose) if you'd like to help update the content for Steeltoe v4.

## App Configuration with Cloud Foundry

This tutorial takes you through setting up a .NET Core application that retrieves environment variable values from Cloud Foundry.

> [!NOTE]
> For more detailed examples, please refer to the [SimpleCloundFoundry](https://github.com/SteeltoeOSS/Samples/tree/3.x/Configuration/src/SimpleCloudFoundry) project in the [Steeltoe Samples Repository](https://github.com/SteeltoeOSS/Samples/tree/3.x).

First, **create a .NET Core WebAPI** that retrieves (configuration) environment variables from Cloud Foundry.

1. Create a new ASP.NET Core WebAPI app with the [Steeltoe Initializr](https://start.steeltoe.io)
1. Name the project "CloudFoundryExample"
1. Add the "Cloud Foundry" dependency
1. Click **Generate Project** to download a zip containing the new project

**Run** the application.

1. Publish the application locally using the .NET cli. The following command will create a publish folder automatically

   ```powershell
   dotnet publish -o .\publish <PATH_TO>\CloudFoundryExample.csproj
   ```

1. Create **manifest.yml** in the same folder as CloudFoundryExample.csproj

   ```yaml
   ---
   applications:
     - name: Cloud_Foundry_Example
   buildpacks:
     - dotnet_core_buildpack
   stack: cflinuxfs3
   ```

   > [!NOTE]
   > With yaml files indention and line endings matter. Use an IDE like VS Code to confirm spacing and that line endings are set to `LF` (not the Windows default `CR LF`)

1. Push the app to Cloud Foundry

   ```powershell
   cf push -f <PATH_TO>\manifest.yml -p .\publish
   ```

1. Navigate to the application endpoint `https://<APP_ROUTE>/api/values`
1. If all things go well you should see an output of configuration values provided by the Cloud Foundry platform
