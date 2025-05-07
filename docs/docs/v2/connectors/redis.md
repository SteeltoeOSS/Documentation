# Redis

 This connector simplifies using a Microsoft [RedisCache](https://docs.microsoft.com/aspnet/core/performance/caching/distributed#using-a-redis-distributed-cache) or a StackExchange [IConnectionMultiplexer](https://stackexchange.github.io/StackExchange.Redis/) in an application running on Cloud Foundry.

Here are some Steeltoe sample applications are available to help you understand how to use this connector:

* [AspDotNet4/Redis4](https://github.com/SteeltoeOSS/Samples/tree/dev/Connectors/src/AspDotNet4/Redis4): Same as the next Quick Start but built for ASP.NET 4.x.
* [DataProtection](https://github.com/SteeltoeOSS/Samples/tree/2.x/Security/src/RedisDataProtectionKeyStore): A sample application showing how to use the Steeltoe DataProtection Key Storage Provider for Redis.
* [MusicStore](https://github.com/SteeltoeOSS/Samples/tree/2.x/MusicStore): A sample application showing how to use all of the Steeltoe components together in an ASP.NET Core application. This is a micro-services based application built from the ASP.NET Core reference app MusicStore provided by Microsoft.

This connector provides a `IHealthContributor` which you can use in conjunction with the [Steeltoe Management Health](../management/health.md) check endpoint.

## Usage

You should know how the .NET [Configuration service](https://docs.microsoft.com/aspnet/core/fundamentals/configuration) works before starting to use the connector. To configure the connector, you need a basic understanding of the `ConfigurationBuilder` and how to add providers to the builder.

You should also know how the ASP.NET Core [Startup](https://docs.microsoft.com/aspnet/core/fundamentals/startup) class is used in configuring the application services for the app. Pay particular attention to the usage of the `ConfigureServices()` method.

You probably want some understanding of how to use the [RedisCache](https://docs.microsoft.com/aspnet/core/performance/caching/distributed#using-a-redis-distributed-cache) or [IConnectionMultiplexer](https://stackexchange.github.io/StackExchange.Redis/) before starting to use this connector.

To use this connector:

1. Create a Redis Service instance and bind it to your application.
1. Optionally, configure any Redis client settings (for example, in `appsettings.json`).
1. Add the Steeltoe Cloud Foundry config provider to your ConfigurationBuilder.
1. Add DistributedRedisCache or ConnectionMultiplexer to your ServiceCollection.

>NOTE: The Microsoft wrapper for the Stack Exchange Redis client depends on Lua commands `EVAL` and/or `EVALSHA`. Lua scripting is disabled by default in many Redis tile installations on the TAS Platform. If you encounter a message similar to `StackExchange.Redis.RedisServerException: ERR unknown command EVALSHA`, you need to either have Lua scripting enabled by a platform operator or try using the `ConnectionMultiplexer` instead of the `IDistributedCache` or `RedisCache` interfaces provided by Microsoft.

### Add NuGet Reference

To use the Redis connector, you need to [add a reference to the appropriate Steeltoe Connector NuGet package](usage.md#add-nuget-references) and a reference to `Microsoft.Extensions.Caching.Redis`, `StackExchange.Redis`, or `StackExchange.Redis.StrongName`.

>NOTE: The requirement to add a direct Redis package reference is new as of version 2.0.0.

<!-- -->
>NOTE: Because `Microsoft.Extensions.Caching.Redis` depends on `StackExchange.Redis.StrongName`, adding a reference to the Microsoft library also enables access to the StackExchange classes, as seen in the sample application.

### Configure Settings

The connector supports several settings for the Redis connection that can be useful when you are developing and testing an application locally and you need to have the connector configure the connection for non-default settings.

The following example of the connector's configuration in JSON that shows how to set up a connection to a Redis server at `https://foo.bar:1111`:

```json
{
  ...
  "redis": {
    "client": {
      "host": "https://foo.bar",
      "port": 1111
    }
  }
  ...
}
```

The following table table describes all possible settings for the connector

|Key|Description|Default|
|---|---|---|
|host|Hostname or IP Address of the server|localhost|
|port|Port number of the server|6379|
|endPoints|Comma-separated list of host:port pairs|not set|
|clientName|Identification for the connection within redis|not set|
|connectRetry|Times to repeat initial connect attempts|3|
|connectTimeout|Timeout (ms) for connect operations|5000|
|abortOnConnectFail|Will not create a connection while no servers are available|true|
|keepAlive|Time (seconds) at which to send a message to help keep sockets alive|-1|
|resolveDns|Whether DNS resolution should be explicit and eager, rather than implicit|false|
|ssl|Whether SSL encryption should be used|false|
|sslHost|Enforces a particular SSL host identity on the server's certificate|not set|
|writeBuffer|Size of the output buffer|4096|
|connectionString|Full connection string|built from settings|
|instanceId|Cache ID. Used only with `IDistributedCache`|not set|
|urlEncodedCredentials|Set to `true` if your service broker provides URL-encoded credentials|false|

>IMPORTANT: All of these settings should be prefixed with `redis:client:`.

The samples and most templates are already set up to read from `appsettings.json`.

>NOTE: If a ConnectionString is provided and VCAP_SERVICES are not detected (a typical scenario for local app development), the ConnectionString will be used exactly as provided.

### Cloud Foundry

To use Redis on Cloud Foundry, create and bind an instance to your application by using the Cloud Foundry CLI, as shown in the following example:

```bash
# Create Redis service
cf create-service p-redis shared-vm myRedisCache

# Bind service to `myApp`
cf bind-service myApp myRedisCache

# Restage the app to pick up change
cf restage myApp
```

>NOTE: The preceding commands assume you use the Redis service provided by TAS. If you use a different service, you have to adjust the `create-service` command to fit your environment.

Version 2.1.1+ of this connector works with the [Azure Open Service Broker for PCF](https://docs.pivotal.io/partners/azure-open-service-broker-pcf/index.html). Be sure to set `redis:client:urlEncodedCredentials` to `true` as this broker may provide credentials that have been URL Encoded.

Once the service is bound to your application, the connector's settings are available in `VCAP_SERVICES`.

### Add IDistributedCache

To use Microsoft's `IDistributedCache` in your application, add it to the service container, as shown in the following example:

 ```csharp
using Steeltoe.CloudFoundry.Connector.Redis;
public class Startup {
    public IConfiguration Configuration { get; private set; }
    public Startup()
    {
    }
    public void ConfigureServices(IServiceCollection services)
    {
        // Add Microsoft Redis Cache (IDistributedCache) configured from Cloud Foundry
        services.AddDistributedRedisCache(Configuration);

        // Add framework services
        services.AddMvc();
    }
```

The `AddDistributedRedisCache(Configuration)` method call configures the `IDistributedCache` by using the configuration built by the application earlier and adds the connection to the service container.

### Use IDistributedCache

The following example shows how to inject and use the `IDistributedCache` in a controller once it has been added to the service container:

 ```csharp
 using Microsoft.Extensions.Caching.Distributed;
 ...
 public class HomeController : Controller
 {
     private IDistributedCache _cache;
     public HomeController(IDistributedCache cache)
     {
         _cache = cache;
     }
     ...
     public IActionResult CacheData()
     {
         string key1 = Encoding.Default.GetString(_cache.Get("Key1"));
         string key2 = Encoding.Default.GetString(_cache.Get("Key2"));

         ViewData["Key1"] = key1;
         ViewData["Key2"] = key2;

         return View();
     }
 }
 ```

### Add IConnectionMultiplexer

To use a StackExchange `IConnectionMultiplexer` in your application directly, add it to the service container in the `ConfigureServices()` method of the `Startup` class, as shown in the following example:

 ```csharp
using Steeltoe.CloudFoundry.Connector.Redis;

public class Startup {
    ...
    public IConfiguration Configuration { get; private set; }
    public Startup(...)
    {
      ...
    }
    public void ConfigureServices(IServiceCollection services)
    {

        // Add StackExchange IConnectionMultiplexer configured from Cloud Foundry
        services.AddRedisConnectionMultiplexer(Configuration);

        // Add framework services
        services.AddMvc();
        ...
    }
    ...
```

The `AddRedisConnectionMultiplexer(Configuration)` method call configures the `IConnectionMultiplexer` by using the configuration built by the application and adds the connection to the service container.

>NOTE: If necessary, you can use both `IDistributedCache` and `IConnectionMultiplexer` in your application.

### Use IConnectionMultiplexer

Once you have configured and added the `IConnectionMultiplexer` to the service container, you can inject it and use it in a controller or a view, as shown in the following example:

 ```csharp
 using Microsoft.Extensions.Caching.Distributed;
 ...
 public class HomeController : Controller
 {
     private IConnectionMultiplexer _conn;
     public HomeController(IConnectionMultiplexer conn)
     {
         _conn = conn;
     }
     ...
      public IActionResult ConnData()
    {
        IDatabase db = _conn.GetDatabase();

        string key1 = db.StringGet("ConnectionMultiplexerKey1");
        string key2 = db.StringGet("ConnectionMultiplexerKey2");

        ViewData["ConnectionMultiplexerKey1"] = key1;
        ViewData["ConnectionMultiplexerKey2"] = key2;

        return View();
    }
 }
 ```
