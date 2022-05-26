# Redis Key Storage Provider

By default, ASP.NET Core stores the key ring on the local file system. Local file system usage in a Cloud Foundry environment is unworkable and violates the [twelve-factor guidelines](https://12factor.net/) for developing cloud native applications. By using the Steeltoe Redis Key Storage provider, you can reconfigure the Data Protection service to use Redis on Cloud Foundry for storage.

## Usage

To use this provider:

1. Create a Redis Service instance and bind it to your application.
1. Add the Steeltoe Cloud Foundry config provider to your `ConfigurationBuilder`.
1. Add the Redis `ConnectionMultiplexer` to your ServiceCollection.
1. Add `DataProtection` to your `ServiceCollection` and configure it to `PersistKeysToRedis`.

### Add NuGet Reference

To use the provider, add a reference to the Steeltoe DataProtection Redis NuGet.

The provider can be found in the `Steeltoe.Security.DataProtection.RedisCore` package.

You can add the provider to your project by using the following `PackageReference` in your project file:

```xml
<ItemGroup>
...
    <PackageReference Include="Steeltoe.Security.DataProtection.RedisCore" Version="2.5.2" />
...
</ItemGroup>
```

You also need the Steeltoe Redis connector. Add the `Steeltoe.CloudFoundry.ConnectorCore` package to get the Redis connector and helpers for setting it up.

You can use the NuGet Package Manager tools or directly add the following package reference to your .csproj file:

```xml
<ItemGroup>
...
    <PackageReference Include="Steeltoe.CloudFoundry.ConnectorCore" Version="2.5.2" />
...
</ItemGroup>
```

### Cloud Foundry

To use the Redis Data Protection key ring provider on Cloud Foundry, you have to install a Redis service and create and bind an instance of it to your application by using the Cloud Foundry command line, as shown in the following example:

```bash
# Create Redis service
cf create-service p-redis shared-vm myRedisCache

# Bind service to `myApp`
cf bind-service myApp myRedisCache

# Restage the app to pick up change
cf restage myApp
```

>NOTE: The preceding commands are for the Redis service provided by TAS. If you use a different service, you have to adjust the `create-service` command.

Once the service is bound to your application, the settings are available in `VCAP_SERVICES`.

### Add Redis IConnectionMultiplexer

The next step is to add the StackExchange Redis `IConnectionMultiplexer` to your service container.

You can do so in the `ConfigureServices()` method of the `Startup` class by using the Steeltoe Redis Connector, as follows:

```csharp
using Steeltoe.CloudFoundry.Connector.Redis;

public class Startup {
    ...
    public IConfiguration Configuration { get; private set; }
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }
    public void ConfigureServices(IServiceCollection services)
    {
        // Add StackExchange ConnectionMultiplexer configured from Cloud Foundry
        services.AddRedisConnectionMultiplexer(Configuration);

        // Add framework services.
        services.AddMvc();
        ...
    }
    ...
```

See the documentation on the Steeltoe Redis connector for details on how you can configure additional settings to control its behavior.

### Add PersistKeysToRedis

The last step is to use the provider to configure DataProtection to persist keys to Redis.

You can do so in the `ConfigureServices()` method of the `Startup` class, as shown in the following example:

```csharp
using Steeltoe.CloudFoundry.Connector.Redis;

public class Startup {
    ...
    public IConfiguration Configuration { get; private set; }
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }
    public void ConfigureServices(IServiceCollection services)
    {
        // Add StackExchange ConnectionMultiplexer configured from Cloud Foundry
        services.AddRedisConnectionMultiplexer(Configuration);

        // Add DataProtection and persist keys to Cloud Foundry Redis service
        services.AddDataProtection()
            .PersistKeysToRedis()
            .SetApplicationName("Some Name");

        // Add framework services.
        services.AddMvc();
        ...
    }
    ...
```

### Use Redis Key Store

Once the Redis Key Store has been set up, the keys used by the `DataProtection` framework are stored in the bound Redis Cloud Foundry service. You need not do more.
