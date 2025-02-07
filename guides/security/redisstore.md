---
uid: guides/security-providers/redisstore
title: Key Ring with Redis
tags: []
_disableFooter: true
_hideTocVersionToggle: true
---

> [!NOTE]
> This guide applies to Steeltoe v3. Please [open an issue](https://github.com/SteeltoeOSS/Documentation/issues/new/choose) if you'd like to help update the content for Steeltoe v4.

## Using Cloud Security with a Redis Cache for key ring store

This tutorial takes you through setting up a .NET Core application that stores its master keys used to protect payloads in an external Redis cache. Learn more about ASP.NET data protection [here](https://docs.microsoft.com/en-us/aspnet/core/security/data-protection).

> [!NOTE]
> For more detailed examples, please refer to the [RedisDataProtectionKeyStore](https://github.com/SteeltoeOSS/Samples/tree/3.x/Security/src/RedisDataProtectionKeyStore) project in the [Steeltoe Samples Repository](https://github.com/SteeltoeOSS/Samples/tree/3.x).

First, **start a Redis instance**. Using the [Steeltoe dockerfile](https://github.com/steeltoeoss/dockerfiles), start a local instance of RedisStore.

```powershell
docker run --publish 6379:6379 steeltoeoss/redis
```

Next, **create a .NET Core WebAPI** using redis for key storage

1. Create a new ASP.NET Core WebAPI app with the [Steeltoe Initializr](https://start.steeltoe.io)
1. Name the project "RedisKeyRingExample"
1. Add the "Redis" dependency
1. Click **Generate** to download a zip containing the new project
1. Extract the zipped project and open in your IDE of choice

1. Set the Redis multiplexer and DataProtection in **Startup.cs**

   ```csharp
   using Steeltoe.CloudFoundry.Connector.Redis;

   public class Startup {
     public IConfiguration Configuration { get; private set; }
     public Startup(IConfiguration configuration) {
       Configuration = configuration;
     }

     public void ConfigureServices(IServiceCollection services) {
       // Add StackExchange ConnectionMultiplexer configured from Cloud Foundry
       services.AddRedisConnectionMultiplexer(Configuration);

       // Add DataProtection and persist keys to Redis service
       services.AddDataProtection()
         .PersistKeysToRedis()
         .SetApplicationName("Some Name");
       // Add framework services.

       services.AddMvc();
     }
   }
   ```

**Run** the application

# [.NET cli](#tab/cli)

```powershell
dotnet run<PATH_TO>\RedisKeyRingExample.csproj
```

Navigate to the endpoint (you may need to change the port number) [http://localhost:5000/api/values](http://localhost:5000/api/values)

# [Visual Studio](#tab/vs)

1. Choose the top _Debug_ menu, then choose _Start Debugging (F5)_. This should bring up a browser with the app running
1. Navigate to the endpoint (you may need to change the port number) [http://localhost:8080/api/values](http://localhost:8080/api/values)

---

Thats it! Now you can run multiple instances of your application and they will all share the same master key for encrypting its payloads.
