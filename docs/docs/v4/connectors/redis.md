# Redis/Valkey

This connector simplifies accessing [Redis](https://redis.io/) and [Valkey](https://valkey.io/) databases.

It supports the following .NET drivers:

- [StackExchange.Redis](https://www.nuget.org/packages/StackExchange.Redis), which provides an `IConnectionMultiplexer`
- [Microsoft.Extensions.Caching.StackExchangeRedis](https://www.nuget.org/packages/Microsoft.Extensions.Caching.StackExchangeRedis), which provides an `IDistributedCache`

The remainder of this topic assumes that you are familiar with the basic concepts of Steeltoe Connectors. See [Overview](./usage.md) for more information.

## Using the Redis Connector

To use this connector:

1. Create a Redis/Valkey server instance or use a [Redis docker container](https://github.com/SteeltoeOSS/Samples/blob/4.x/CommonTasks.md#redis) or [Valkey docker container](https://github.com/SteeltoeOSS/Samples/blob/4.x/CommonTasks.md#valkey).
1. Add NuGet references to your project.
1. Configure your connection string in `appsettings.json`.
1. Initialize the Steeltoe Connector at startup.
1. Use the driver-specific connection/client instance.

### Add NuGet References

To use this connector, add a NuGet reference to `Steeltoe.Connectors`.

Also add a NuGet reference to one of the .NET drivers listed above, as you would if you were not using Steeltoe.

### Configure connection string

The available connection string parameters for Redis are described in [StackExchange.Redis](https://stackexchange.github.io/StackExchange.Redis/Configuration.html).

The following example `appsettings.json` works with either docker container referred to earlier:

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

### Use IConnectionMultiplexer

To obtain an `IConnectionMultiplexer` instance in your application, inject the Steeltoe factory in a controller or view:

```csharp
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using Steeltoe.Connectors;
using Steeltoe.Connectors.Redis;

public class HomeController : Controller
{
    public async Task<IActionResult> Index(
        [FromServices] ConnectorFactory<RedisOptions, IConnectionMultiplexer> connectorFactory)
    {
        var connector = connectorFactory.Get();
        IConnectionMultiplexer client = connector.GetConnection();

        IDatabase database = client.GetDatabase();
        database.StringSet("myKey", DateTime.UtcNow.ToString());

        ViewData["Result"] = database.StringGet("myKey");
        return View();
    }
}
```

For a complete sample app that uses `IConnectionMultiplexer`, see https://github.com/SteeltoeOSS/Samples/tree/4.x/Connectors/src/Redis.

### Use IDistributedCache

To obtain an `IDistributedCache` instance in your application, inject the Steeltoe factory in a controller or view:

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Steeltoe.Connectors;
using Steeltoe.Connectors.Redis;

public class HomeController : Controller
{
    public async Task<IActionResult> Index(
        [FromServices] ConnectorFactory<RedisOptions, IDistributedCache> connectorFactory)
    {
        var connector = connectorFactory.Get();
        IDistributedCache client = connector.GetConnection();

        await client.SetStringAsync("myKey", DateTime.UtcNow.ToString());

        ViewData["Result"] = await client.GetStringAsync("myKey");
        return View();
    }
}
```

For a complete sample app that uses `IDistributedCache`, see https://github.com/SteeltoeOSS/Samples/tree/4.x/Connectors/src/Redis.

## Cloud Foundry

This Connector supports the following service brokers:

- [Redis for Tanzu Application Service](https://techdocs.broadcom.com/us/en/vmware-tanzu/data-solutions/redis-for-tanzu-application-service/3-5/redis-for-tas/index.html)
- [Tanzu for Valkey on Cloud Foundry](https://techdocs.broadcom.com/us/en/vmware-tanzu/data-solutions/tanzu-for-valkey-on-cloud-foundry/4-0/valkey-on-cf/index.html)
- [Tanzu Cloud Service Broker for Microsoft Azure](https://techdocs.broadcom.com/us/en/vmware-tanzu/platform-services/tanzu-cloud-service-broker-for-microsoft-azure/1-13/csb-azure/reference-azure-redis.html)
- [Tanzu Cloud Service Broker for AWS](https://techdocs.broadcom.com/us/en/vmware-tanzu/platform-services/tanzu-cloud-service-broker-for-aws/1-14/csb-aws/reference-aws-redis.html)

You can create and bind an instance to your application by using the Cloud Foundry CLI.

1. Create Redis service:

   ```shell
   cf create-service p.redis your-plan sampleRedisService
   ```

1. Bind service to your app:

   ```shell
   cf bind-service sampleApp sampleRedisService
   ```

1. Restage the app to pick up change:

   ```shell
   cf restage sampleApp
   ```

## Kubernetes

This Connector supports the [Service Binding Specification for Kubernetes](https://github.com/servicebinding/spec).
It can be used through the [Services Toolkit](https://techdocs.broadcom.com/us/en/vmware-tanzu/standalone-components/tanzu-application-platform/1-12/tap/services-toolkit-install-services-toolkit.html).

For details on how to use this, see the instructions at https://github.com/SteeltoeOSS/Samples/tree/4.x/Connectors/src/Redis#running-on-tanzu-platform-for-kubernetes.
