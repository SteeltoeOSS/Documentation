# Redis Key Storage Provider

By default, the [data protection system in ASP.NET Core](https://learn.microsoft.com/aspnet/core/security/data-protection/introduction) stores cryptographic keys on the local file system.
Even when not used by the application directly, these cryptographic keys are used for systems like session state storage.
Local file system usage in a cloud environment, where application instances come and go at unpredictable intervals, is unworkable and violates the [twelve-factor guidelines](https://12factor.net/) for developing cloud-native applications.
By using the Steeltoe Redis key storage provider, you can easily reconfigure the data protection service to use Redis instances that are accessible through the [Steeltoe Redis Connector](../connectors/redis.md).

## Usage

To use this provider:

1. Add NuGet reference(s).
1. (Optional) Add the Cloud Foundry configuration provider.
1. Initialize the Steeltoe Connector at startup.
1. Add `DataProtection` to your `ServiceCollection` and configure it to `PersistKeysToRedis`.
1. Create a Redis service instance and bind it to your application.

### Add NuGet References

To use the provider, you need to add a reference to the `Steeltoe.Security.DataProtection.Redis` NuGet package.

If you are using Cloud Foundry service bindings, you will also need to add a reference to `Steeltoe.Configuration.CloudFoundry`.

### Add Cloud Foundry Configuration

The Steeltoe package `Steeltoe.Configuration.CloudFoundry` reads Redis credentials from Cloud Foundry service bindings (`VCAP_SERVICES`) and maps them into the application's configuration in format that the Redis connector can read.
Add the configuration provider to your application with this code:

```csharp
using Steeltoe.Configuration.CloudFoundry;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.AddCloudFoundryConfiguration();
```

### Initialize Steeltoe Connector

Update your `Program.cs` as below to initialize the Connector:

```csharp
using Steeltoe.Connectors.Redis;

builder.AddRedis();
```

### Persist Keys to Redis

There are several steps required to configure key storage in Redis:

- Add data protection (while the services are added automatically, this step is required in order to access the builder).
- Set the key persistence location to Redis.
- Set an application name so all instances of the application can see the same data.

These steps can be performed by chaining the method calls together:

```csharp
using Steeltoe.Security.DataProtection.Redis;

builder.Services.AddDataProtection().PersistKeysToRedis().SetApplicationName("redis-data-protection-sample");
```

> [!NOTE]
> At this point, the keys used by the `DataProtection` framework are stored in the bound Redis service.
> No additional steps are _required_, but you can also [use data protection in your application](https://learn.microsoft.com/aspnet/core/security/data-protection/consumer-apis/overview).

### Cloud Foundry

To use the Redis data protection key ring provider on Cloud Foundry, [use a supported Redis service](../connectors/redis.md#cloud-foundry) to create and bind an instance of Redis to your application.

You can complete these steps using the Cloud Foundry command line, as follows:

```bash
# Create Redis service
cf create-service p-redis shared-vm myRedisService

# Bind service to your app
cf bind-service myApp myRedisService

# Restage the app to pick up change
cf restage myApp
```

> [!NOTE]
> The preceding commands are for the Redis service provided by Tanzu Platform for Cloud Foundry.
> If you use a different service, you have to adjust the `create-service` command.

Once the service is bound to your application, the configuration settings are available in `VCAP_SERVICES`.
