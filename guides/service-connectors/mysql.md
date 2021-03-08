---
uid: labs/service-connectors/mysql
title: MySQL Database
tags: []
_disableFooter: true
---

## Using Service Connectors with MySQL

This tutorial takes you through setting up a .NET Core application with the MySQL service connector.

First, **start a MySQL instance** using the [Steeltoe dockerfile](https://github.com/steeltoeoss/dockerfiles) start a local instance of MySQL.

  ```powershell
  docker run --env MYSQL_ROOT_PASSWORD=Steeltoe456 --publish 3306:3306 steeltoeoss/mysql
  ```

Next, **create a .NET Core WebAPI** that interacts with MySQL

1. Create a new ASP.NET Core WebAPI app with the [Steeltoe Initializr](https://start.steeltoe.io)
    ![Steeltoe Initialzr](~/labs/images/initializr/mysql.png)
1. Name the project "MySql_Connector"
1. Add the "MySQL" dependency
1. Click **Generate** to download a zip containing the new project
1. Extract the zipped project and open in your IDE of choice
1. Set the instance address in **appsettings.json**

    ```json
    {
      "mysql": {
        "client": {
          "server": "127.0.0.1",
          "port": "3306",
          "username": "root",
          "password": "Steeltoe456"
        }
      }
    }
    ```

    > [!TIP]
    >Looking for additional params to use when connecting? Have a look at the [docs](~/api/v3/welcome/index.md)

**Run** the application

  # [.NET cli](#tab/cli)

  ```powershell
  dotnet run <PATH_TO>\MySql_Connector.csproj
  ```

  Navigate to the endpoint (you may need to change the port number) [http://localhost:5000/api/values](http://localhost:5000/api/values)

  # [Visual Studio](#tab/vs)

  1. Choose the top *Debug* menu, then choose *Start Debugging (F5)*. This should bring up a browser with the app running
  1. Navigate to the endpoint (you may need to change the port number) [http://localhost:8080/api/values](http://localhost:8080/api/values)
  
  ***

Once the app loads in the browser you will see a list of the default schema info installed with MySQL.
"["CHARACTER_SETS","COLLATIONS","COLLATION_CHARACTER_SET_APPLICABILITY", "COLUMNS","COLUMN_PRIVILEGES" ..."
*In cloud foundry this information is cleared. The app will return an empty collection "[]".
