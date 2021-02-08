---
uid: labs/service-connectors/postgresql
title: PostgreSQL Database
tags: []
_disableFooter: true
---

## Using Service Connectors with PostgreSQL Database

This tutorial takes you through setting up a .NET Core application with the PostgreSQL service connector.

First, **start a PostgreSQL instance** using the [Steeltoe dockerfile](https://github.com/steeltoeoss/dockerfiles), start a local instance of PostgreSQL.

 ```powershell
 docker run --env POSTGRES_PASSWORD=Steeltoe789 --publish 5432:5432 steeltoeoss/postgresql
 ```

Next, **create a .NET Core WebAPI** that interacts with PostgreSQL

1. Create a new ASP.NET Core WebAPI app with the [Steeltoe Initializr](https://start.steeltoe.io)
    ![Steeltoe Initialzr](~/labs/images/initializr/mongo-connector.png)
1. Name the project "Postgre_Connector"
1. Add the "PostgreSQL" dependency
1. Click **Generate** to download a zip containing the new project
1. Extract the zipped project and open in your IDE of choice
1. Set the instance address in **appsettings.json**

    ```json
    {
      "postgres": {
        "client": {
          "server": "127.0.0.1",
          "port": "5432",
          "username": "postgres",
          "password": "Steeltoe789"
        }
      }
    }
    ```

    > [!TIP]
    >Looking for additional params to use when connecting? Have a look at the [docs](~/api/v3/welcome/index.md)

**Run** the application

  # [.NET cli](#tab/cli)

  ```powershell
  dotnet run <PATH_TO>\Mongo_Connector.csproj
  ```

  Navigate to the endpoint (you may need to change the port number) [http://localhost:5000/api/values](http://localhost:5000/api/values)

  # [Visual Studio](#tab/vs)

  1. Choose the top *Debug* menu, then choose *Start Debugging (F5)*. This should bring up a browser with the app running
  1. Navigate to the endpoint (you may need to change the port number) [http://localhost:8080/api/values](http://localhost:8080/api/values)
  
  ***

Once the app loads in the browser you will see a list of the default database schemas installed with PostgreSQL.
"["UTF8","UTF8","UTF8"]"
