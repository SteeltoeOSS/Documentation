---
uid: guides/service-connectors/mssql
title: Microsoft SQL Database
tags: []
_disableFooter: true
_hideTocVersionToggle: true
---

> [!NOTE]
> This guide applies to Steeltoe v3. Please [open an issue](https://github.com/SteeltoeOSS/Documentation/issues/new/choose) if you'd like to help update the content for Steeltoe v4.

## Using Service Connectors with Microsoft SQL

This tutorial takes you through setting up a .NET Core application with the Microsoft SQL service connector.

> [!NOTE]
> For more detailed examples, please refer to the [SqlServerEFCore](https://github.com/SteeltoeOSS/Samples/tree/3.x/Connectors/src/SqlServerEFCore) project in the [Steeltoe Samples Repository](https://github.com/SteeltoeOSS/Samples/tree/3.x).

First, **start an MsSQL instance** using the [Steeltoe dockerfile](https://github.com/steeltoeoss/dockerfiles).

```powershell
docker run --env ACCEPT_EULA=Y --env SA_PASSWORD=Steeltoe123 --publish 1433:1433 steeltoeoss/mssql
```

Next, **create a .NET Core WebAPI** that interacts with MS SQL

1. Create a new ASP.NET Core WebAPI app with the [Steeltoe Initializr](https://start.steeltoe.io)
1. Name the project "MsSqlConnector"
1. Add the "Microsoft SQL Server" dependency
1. Click **Generate** to download a zip containing the new project
1. Extract the zipped project and open in your IDE of choice
1. Set the instance address in **appsettings.json**

   ```json
   {
     "sqlserver": {
       "credentials": {
         "server": "127.0.0.1",
         "port": "1433",
         "username": "sa",
         "password": "Steeltoe123"
       }
     }
   }
   ```

   > [!TIP]
   > Looking for additional params to use when connecting? Have a look at the [docs](/api/v3/welcome/index.md)

**Run** the application

# [.NET cli](#tab/cli)

```powershell
dotnet run <PATH_TO>\MsSqlConnector.csproj
```

Navigate to the endpoint (you may need to change the port number) [http://localhost:5000/api/values](http://localhost:5000/api/values)

# [Visual Studio](#tab/vs)

1. Choose the top _Debug_ menu, then choose _Start Debugging (F5)_. This should bring up a browser with the app running
1. Navigate to the endpoint (you may need to change the port number) [http://localhost:8080/api/values](http://localhost:8080/api/values)

---

Once the app loads in the browser you will see a list of the default sys tables installed with MS SQL.
"["spt_fallback_db","spt_fallback_dev","spt_fallback_usg","spt_values", "spt_monitor","MSreplication_options"]"
