---
uid: labs/application-configuration/placeholder
title: Placeholder Provider
tags: []
_disableFooter: true
---
## Application Configuration Placeholders
This tutorial takes you through setting up a .NET Core application that uses placeholders for config values.

First, **create a .NET Core WebAPI** that has a placeholder implemented

1. Create a new ASP.NET Core WebAPI app with the [Steeltoe Initializr](https://start.steeltoe.io)
    ![Steeltoe Initialzr](~/labs/images/initializr/placeholder.png)
1. Name the project "Placeholder-Example"
1. Add the "Placeholder" dependency
1. Click **Generate Project** to download a zip containing the new project
1. Extract the zipped project and open in your IDE of choice (we use Visual Studio)

**Run** the application

  # [.NET cli](#tab/cli)

  ```powershell
  dotnet run <PATH_TO>\Placeholder_Example.csproj
  ```

  Navigate to the endpoint (you may need to change the port number) [http://localhost:5000/api/values](http://localhost:5000/api/values)

  # [Visual Studio](#tab/vs)

  1. Choose the top *Debug* menu, then choose *Start Debugging (F5)*. This should bring up a browser with the app running.
  1. Navigate to the endpoint (you may need to change the port number) [http://localhost:8080/api/values](http://localhost:8080/api/values)

  ***

Once the app loads in the browser you will see the three values output.

  `["<PATH_ENV_VAR>","NotFound","Information"]`
