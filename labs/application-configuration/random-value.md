---
uid: labs/application-configuration/random-value
title: Random Value Provider
tags: []
_disableFooter: true
---

## Application Configuration Random Values
This tutorial takes you through setting up a .NET Core application that gets a random value for a config setting.

First, **create a .NET Core WebAPI** that has a placeholder implemented.

1. Create a new ASP.NET Core WebAPI app with the [Steeltoe Initializr](https://start.steeltoe.io)
  ![Steeltoe Initialzr](~/labs/images/initializr/randomvalue.png)
1. Name the project "Random-Value-Example"
1. Add the "Random value" dependency
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

Once the app loads in the browser you will see three random values output.

  `["<INTEGER>","<UUID>","<STRING>"]`
