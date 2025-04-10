# Redis

This connector simplifies using a Microsoft [`RedisCache`](https://docs.microsoft.com/aspnet/core/performance/caching/distributed#using-a-redis-distributed-cache) or a StackExchange [`IConnectionMultiplexer`](https://stackexchange.github.io/StackExchange.Redis/).

The following Steeltoe sample applications are available to help you understand how to use this connector:

* [DataProtection](https://github.com/SteeltoeOSS/Samples/tree/3.x/Security/src/RedisDataProtectionKeyStore): A sample application showing how to use the Steeltoe DataProtection Key Storage Provider for Redis.
* [MusicStore](https://github.com/SteeltoeOSS/Samples/tree/3.x/MusicStore): A sample application showing how to use all of the Steeltoe components together in an ASP.NET Core application. This is a microservices-based application built from the  MusicStore ASP.NET Core reference application provided by Microsoft.

This connector provides an `IHealthContributor`, which you can use in conjunction with the [Steeltoe Management Health](../management/health.md) check endpoint.

## Usage

You should know how the .NET [configuration service](https://docs.microsoft.com/aspnet/core/fundamentals/configuration) works before starting to use the connector. To configure the connector, you need a basic understanding of the `ConfigurationBuilder` and how to add providers to the builder.

You should also know how the ASP.NET Core [`Startup`](https://docs.microsoft.com/aspnet/core/fundamentals/startup) class is used in configuring the application services for the application. Pay particular attention to the usage of the `ConfigureServices()` method.

You probably want some understanding of how to use the [`RedisCache`](https://docs.microsoft.com/aspnet/core/performance/caching/distributed#using-a-redis-distributed-cache) or [`IConnectionMultiplexer`](https://stackexchange.github.io/StackExchange.Redis/) before starting to use this connector.

To use this connector:

1. Create a Redis Service instance and bind it to your application.
1. Optionally, configure any Redis client settings (for example, in `appsettings.json`).
1. Optionally, add the Steeltoe Cloud Foundry config provider to your `ConfigurationBuilder`.
1. Add `DistributedRedisCache` or `ConnectionMultiplexer` to your `ServiceCollection`.

>The Microsoft wrapper for the Stack Exchange Redis client depends on Lua commands `EVAL` and/or `EVALSHA`. Lua scripting is disabled by default in many Redis tile installations on the TAS Platform. If you encounter a message similar to `StackExchange.Redis.RedisServerException: ERR unknown command EVALSHA`, you need to either have Lua scripting enabled by a platform operator or try using the `ConnectionMultiplexer` instead of the `IDistributedCache` or `RedisCache` interfaces provided by Microsoft.

### Add NuGet Reference

To use the Redis connector, you need to [add a reference to the appropriate Steeltoe Connector NuGet package](usage.md#add-nuget-references) and a reference to `Microsoft.Extensions.Caching.Redis`, `Microsoft.Extensions.Caching.StackExchangeRedis`, `StackExchange.Redis`, or `StackExchange.Redis.StrongName`.

>Because `Microsoft.Extensions.Caching.Redis` depends on `StackExchange.Redis.StrongName`, adding a reference to the Microsoft library also enables access to the StackExchange classes, as seen in the sample application.

### Configure Settings

The connector supports several settings for the Redis connection that can be useful when you are developing and testing an application locally and you need to have the connector configure the connection for non-default settings.

The following example of the connector's configuration in JSON shows how to set up a connection to a Redis server at `https://foo.bar:1111`:

```json
{
  ...
  "Redis": {
    "Client": {
      "Host": "https://foo.bar",
      "Port": 1111
    }
  }
  ...
}
```

The following table table describes all possible settings for the connector

| Key | Description | Default |
| --- | --- | --- |
| `Host` | Hostname or IP Address of the server. | `localhost` |
| `Port` | Port number of the server. |6379|
| `EndPoints` |Comma-separated list of host:port pairs. | not set |
| `ClientName` | Identification for the connection within Redis. | not set |
| `ConnectRetry` | Times to repeat initial connect attempts. | 3 |
| `ConnectTimeout` | Timeout (ms) for connect operations. | 5000 |
| `AbortOnConnectFail` | Does not create a connection while no servers are available. | `true` |
| `KeepAlive` | Time (seconds) at which to send a message to help keep sockets alive. | -1 |
| `ResolveDns` | Whether DNS resolution should be explicit and eager, rather than implicit. | `false` |
| `Ssl` | Whether SSL encryption should be used. | `false` |
| `SslHost` | Enforces a particular SSL host identity on the server's certificate. | not set |
| `WriteBuffer` | Size of the output buffer. | 4096 |
| `ConnectionString` | Full connection string. | Built from settings |
| `InstanceId` | Cache ID. Used only with `IDistributedCache`. | not set |

>IMPORTANT: All of these settings should be prefixed with `Redis:Client:`.

The samples and most templates are already set up to read from `appsettings.json`.

>If a `ConnectionString` is provided and `VCAP_SERVICES` are not detected (a typical scenario for local application development), the `ConnectionString` is used exactly as provided.

### Cloud Foundry

To use Redis on Cloud Foundry, create and bind an instance to your application by using the Cloud Foundry CLI:

```bash
# Create Redis service
cf create-service p-redis shared-vm myRedisCache

# Bind service to `myApp`
cf bind-service myApp myRedisCache

# Restage the app to pick up change
cf restage myApp
```

>The preceding commands assume you use the Redis service provided by TAS. If you use a different service, you have to adjust the `create-service` command to fit your environment.

This connector also works with the [Cloud Service Broker](https://docs.vmware.com/en/Tanzu-Cloud-Service-Broker-for-Azure/1.2/csb-azure/GUID-index.html).

Once the service is bound to your application, the connector's settings are available in `VCAP_SERVICES`.

### Add IDistributedCache

To use Microsoft's `IDistributedCache` in your application, add it to the service container:

```csharp
using Steeltoe.Connector.Redis;
public class Startup {
    public IConfiguration Configuration { get; private set; }
    public Startup()
    {
    }
    public void ConfigureServices(IServiceCollection services)
    {
        // Add Microsoft Redis Cache (IDistributedCache)
        services.AddDistributedRedisCache(Configuration);

        // Add framework services
        services.AddMvc();
    }
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

To use a StackExchange `IConnectionMultiplexer` in your application directly, add it to the service container in the `ConfigureServices()` method of the `Startup` class:

 ```csharp
using Steeltoe.Connector.Redis;

public class Startup {
    ...
    public IConfiguration Configuration { get; private set; }
    public Startup(...)
    {
      ...
    }
    public void ConfigureServices(IServiceCollection services)
    {
        // Add StackExchange IConnectionMultiplexer
        services.AddRedisConnectionMultiplexer(Configuration);

        // Add framework services
        services.AddMvc();
        ...
    }
    ...
}
```

The `AddRedisConnectionMultiplexer(Configuration)` method call configures the `IConnectionMultiplexer` by using the configuration built by the application and adds the connection to the service container.

>If necessary, you can use both `IDistributedCache` and `IConnectionMultiplexer` in your application.

### Use IConnectionMultiplexer

Once you have configured and added the `IConnectionMultiplexer` to the service container, you can inject it and use it in a controller or a view:

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
