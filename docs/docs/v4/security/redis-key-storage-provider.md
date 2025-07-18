# Redis Key Storage Provider

The Redis Key Storage Provider is commonly used when secured data needs to be shared between two or more instances of the same application.

> [!NOTE]
> The Key Storage Provider can be used with [Redis](https://redis.io/) and [Valkey](https://valkey.io/) databases.

By default, the [data protection system in ASP.NET Core](https://learn.microsoft.com/aspnet/core/security/data-protection/introduction) stores cryptographic keys on the local file system.
Even when not used by the application directly, these cryptographic keys are used for systems like
[session state](https://learn.microsoft.com/aspnet/core/fundamentals/app-state#session-state) storage and
[antiforgery tokens](https://learn.microsoft.com/aspnet/core/security/anti-request-forgery#antiforgery-in-aspnet-core).

By using the Steeltoe Redis Key Storage Provider, you can easily reconfigure the data protection service to store these keys in Redis/Valkey instances that are accessible through the [Steeltoe Redis Connector](../connectors/redis.md).

For more information, see the [Steeltoe Security samples](https://github.com/SteeltoeOSS/Samples/blob/main/Security/src/RedisDataProtection/README.md).

## Using the Redis Key Storage Provider

To use this provider:

1. Add NuGet reference(s).
1. Configure your connection string.
1. Initialize the Steeltoe Connector at startup.
1. Configure the data protection system to persist keys in the Redis database.
1. Add the Cloud Foundry configuration provider.
1. Create a Redis/Valkey service instance and bind it to your application.

### Add NuGet References

To use the provider, add a reference to the `Steeltoe.Security.DataProtection.Redis` NuGet package.

If you are using Cloud Foundry service bindings, you must also add a reference to `Steeltoe.Configuration.CloudFoundry`.

### Configure connection string

You must configure a connection string to use Redis/Valkey.
The following example `appsettings.Development.json` uses a local Redis server listening on the default Redis port:

```json
{
  "Steeltoe": {
    "Client": {
      "Redis": {
        "Default": {
          "ConnectionString": "localhost"
        }
      }
    }
  }
}
```

### Initialize Steeltoe Connector

Update your `Program.cs` to initialize the Connector:

```csharp
using Steeltoe.Connectors.Redis;

var builder = WebApplication.CreateBuilder(args);
builder.AddRedis();
```

### Persist Keys to Redis

There are several steps required to configure key storage in Redis:

- Add data protection (while the services are added automatically, this step is required for accessing the builder).
- Set the key persistence location to Redis.
- Set an application name so all instances of the application can see the same data.

These steps can be performed by chaining the method calls:

```csharp
using Microsoft.AspNetCore.DataProtection;
using Steeltoe.Security.DataProtection.Redis;

builder.Services.AddDataProtection().PersistKeysToRedis().SetApplicationName("redis-data-protection-sample");
```

> [!NOTE]
> At this point, the keys used by the `DataProtection` framework are stored in the bound Redis service.
> No additional steps are _required_, but you can also use [data protection](https://learn.microsoft.com/aspnet/core/security/data-protection/consumer-apis/overview) in your application.

### Add Cloud Foundry Configuration

The Steeltoe package `Steeltoe.Configuration.CloudFoundry` reads Redis credentials from Cloud Foundry service bindings (`VCAP_SERVICES`) and maps them into the application's configuration in a format that the Redis connector can read.
Add the configuration provider to your application with this code:

```csharp
using Steeltoe.Configuration.CloudFoundry;

var builder = WebApplication.CreateBuilder(args);
builder.AddCloudFoundryConfiguration();
```

### Cloud Foundry

To store data protection keys in a Redis/Valkey cache on Cloud Foundry, use a supported [Redis service](../connectors/redis.md#cloud-foundry) to create and bind an instance of Redis/Valkey to your application.

You can complete these steps using the Cloud Foundry command line.

1. Push your app:

   ```shell
   cf push sampleApp --buildpack dotnet_core_buildpack
   ```

1. Create Redis/Valkey service:

   ```shell
   cf create-service p.redis your-plan sampleRedisDataProtectionService
   ```

1. Bind service to your app:

   ```shell
   cf bind-service sampleApp sampleRedisDataProtectionService
   ```

1. Restage the app to pick up change:

   ```shell
   cf restage sampleApp
   ```

After the service is bound to your application, the configuration settings are available in `VCAP_SERVICES`.

> [!NOTE]
> The preceding commands are for the Redis service provided by Tanzu Platform for Cloud Foundry.
> If you use a different service, adjust the `create-service` command accordingly.

> [!TIP]
> For more information, see the [Steeltoe sample application](https://github.com/SteeltoeOSS/Samples/blob/main/Security/src/RedisDataProtection/README.md).
