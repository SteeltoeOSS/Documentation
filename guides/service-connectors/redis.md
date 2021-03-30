---
uid: guides/service-connectors/redis
title: Redis Cache
tags: []
_disableFooter: true
---

## Using Service Connectors with Redis Cache

This tutorial takes you through setting up a .NET Core application with the Redis service connector.

> [!NOTE]
> For more detailed examples, please refer to the [Redis](https://github.com/SteeltoeOSS/Samples/tree/main/Connectors/src/Redis) project in the [Steeltoe Samples Repository](https://github.com/SteeltoeOSS/Samples).

First, **start a Redis instance** using the [Steeltoe dockerfile](https://github.com/steeltoeoss/dockerfiles), start a local instance of Redis.

```powershell
docker run --publish 6379:6379 steeltoeoss/redis
```

Next, **create a .NET Core WebAPI** that interacts with Redis

1. Create a new ASP.NET Core WebAPI app with the [Steeltoe Initializr](https://start.steeltoe.io)
   <img src="~/guides/images/initializr/redis-connector-dependency.png" alt="Steeltoe Initialzr - Redis Connector" width="100%">
1. Name the project "RedisConnector"
1. Add the "Redis" dependency
1. Click **Generate** to download a zip containing the new project
1. Extract the zipped project and open in your IDE of choice
1. Set the instance address in **appsettings.json**

   ```json
   {
    "redis": {
      "client": {
        "connectRetry": 3
      }
    },
   ```

   > [!TIP]
   > Looking for additional params to use when connecting? Have a look at the [docs](~/api/v3/welcome/index.md)

**Run** the application

# [.NET cli](#tab/cli)

```powershell
dotnet run <PATH_TO>\RedisConnector.csproj
```

Navigate to the endpoint (you may need to change the port number) [http://localhost:5000/api/values](http://localhost:5000/api/values)

# [Visual Studio](#tab/vs)

1. Choose the top _Debug_ menu, then choose _Start Debugging (F5)_. This should bring up a browser with the app running
1. Navigate to the endpoint (you may need to change the port number) [http://localhost:8080/api/values](http://localhost:8080/api/values)

---

Once the app loads in the browser you will see the 2 values that were written and retrieved from Redis.
"[123,456]"
