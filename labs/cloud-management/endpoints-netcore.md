---
uid: labs/cloud-management/endpoints-netcore
title: Management Endpoints (.NET Core)
tags: []
_disableFooter: true
---

> [!TIP]
> Looking for a .NET Framework example? [Have a look](endpoints-framework.md).

## Using Management Endpoints

This tutorial takes you through setting up a .NET Core application with cloud management endpoints automatically added in.

**Create a .NET Core WebAPI** with Actuators enabled

1. Create a new ASP.NET Core WebAPI app with the [Steeltoe Initializr](https://start.steeltoe.io)
    ![Steeltoe Initialzr](~/labs/images/initializr/actuators.png)
1. Name the project "Management_Endpoints_Netcore_Example"
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
  dotnet run<PATH_TO>\Management_Endpoints_Netcore_Example.csproj
  ```

  Navigate to the management endpoints summary page (you may need to change the port number) [http://localhost:5000/actuator](http://localhost:5000/actuator)

  # [Visual Studio](#tab/vs)

  1. Choose the top *Debug* menu, then choose *Start Debugging (F5)*. This should bring up a browser with the app running
  1. Navigate to the management endpoints summary page (you may need to change the port number) [http://localhost:8080/actuator](http://localhost:8080/actuator)
  
  ***

Once the summary page loads, you will see a list of all available management endpoints that have been automatically created. Click each link to see what information is offered.
