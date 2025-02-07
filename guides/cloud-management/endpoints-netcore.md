---
uid: guides/cloud-management/endpoints-netcore
title: Management Endpoints (.NET Core)
tags: []
_disableFooter: true
_hideTocVersionToggle: true
---

> [!NOTE]
> This guide applies to Steeltoe v3. Please [open an issue](https://github.com/SteeltoeOSS/Documentation/issues/new/choose) if you'd like to help update the content for Steeltoe v4.

> [!TIP]
> Looking for a .NET Framework example? [Have a look](endpoints-framework.md).

## Using Management Endpoints

This tutorial takes you through setting up a .NET Core application with cloud management endpoints automatically added in.

> [!NOTE]
> For more detailed examples, please refer to the [Management](https://github.com/SteeltoeOSS/Samples/tree/3.x/Management/src) projects in the [Steeltoe Samples Repository](https://github.com/SteeltoeOSS/Samples/tree/3.x).

**Create a .NET Core WebAPI** with Actuators enabled

1. Create a new ASP.NET Core WebAPI app with the [Steeltoe Initializr](https://start.steeltoe.io)
1. Name the project "ManagementEndpointsNetCoreExample"
1. Add the "Actuators" dependency
1. Click **Generate Project** to download a zip containing the new project
1. Extract the zipped project and open in your IDE of choice
1. Expose all endpoints in **appsettings.json**

   ```json
   {
     "management": {
       "endpoints": {
         "actuator": {
           "exposure": {
             "include": ["*"]
           }
         }
       }
     }
   }
   ```

> [!NOTE]
> Exposing all endpoints is not an ideal setting in production. This is for example only.

**Run** the application

# [.NET cli](#tab/cli)

```powershell
dotnet run<PATH_TO>\ManagementEndpointsNetCoreExample.csproj
```

Navigate to the management endpoints summary page (you may need to change the port number) [http://localhost:5000/actuator](http://localhost:5000/actuator)

# [Visual Studio](#tab/vs)

1. Choose the top _Debug_ menu, then choose _Start Debugging (F5)_. This should bring up a browser with the app running
1. Navigate to the management endpoints summary page (you may need to change the port number) [http://localhost:8080/actuator](http://localhost:8080/actuator)

---

Once the summary page loads, you will see a list of all available management endpoints that have been automatically created. Click each link to see what information is offered.
