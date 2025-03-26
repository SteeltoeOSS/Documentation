---
uid: guides/service-connectors/mongo
title: Mongo Database
tags: []
_disableFooter: true
_hideTocVersionToggle: true
---

> [!NOTE]
> This guide applies to Steeltoe v3. Please [open an issue](https://github.com/SteeltoeOSS/Documentation/issues/new/choose) if you'd like to help update the content for Steeltoe v4.

## Using Service Connectors with Mongo DB

This tutorial takes you through setting up a .NET Core application with the Mongo DB service connector.

> [!NOTE]
> For more detailed examples, please refer to the [MongoDb](https://github.com/SteeltoeOSS/Samples/tree/3.x/Connectors/src/MongoDb) project in the [Steeltoe Samples Repository](https://github.com/SteeltoeOSS/Samples/tree/3.x).

First, **start a Mongo DB instance**. Depending on your hosting platform this is done in several ways.

1. Using the [Steeltoe dockerfile](https://github.com/steeltoeoss/dockerfiles), start a local instance of Mongo.

   ```powershell
   docker run --env MONGO_INITDB_ROOT_USERNAME=steeltoe --env MONGO_INITDB_ROOT_PASSWORD=Steeltoe234 --publish 27017:27017 mongo
   ```

Next, **create a .NET Core WebAPI** that interacts with Mongo DB

1. Create a new ASP.NET Core WebAPI app with the [Steeltoe Initializr](https://start.steeltoe.io)
1. Name the project "MongoConnector"
1. Add the "MongoDB" dependency
1. Click **Generate** to download a zip containing the new project
1. Extract the zipped project and open in your IDE of choice
1. Set the instance address in **appsettings.json**

   ```json
   {
     "mongodb": {
       "client": {
         "server": "127.0.0.1",
         "port": "27017",
         "username": "steeltoe",
         "password": "Steeltoe234"
       }
     }
   }
   ```

   > [!TIP]
   > Looking for additional params to use when connecting? Have a look at the [docs](/api/v3/welcome/index.md)

**Run** the application

# [.NET cli](#tab/cli)

```powershell
dotnet run <PATH_TO>\MongoConnector.csproj
```

Navigate to the endpoint (you may need to change the port number) [http://localhost:5000/api/values](http://localhost:5000/api/values)

# [Visual Studio](#tab/vs)

1. Choose the top _Debug_ menu, then choose _Start Debugging (F5)_. This should bring up a browser with the app running
1. Navigate to the endpoint (you may need to change the port number) [http://localhost:8080/api/values](http://localhost:8080/api/values)

---

Once the app loads in the browser you will see a list of the default databases installed with Mongo.
"["admin","config","local"]"
