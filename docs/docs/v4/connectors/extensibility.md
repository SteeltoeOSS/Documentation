# Extensibility

Steeltoe Connectors cover a fixed set of supported data stores and messaging systems (PostgreSQL, MySQL, SQL Server, MongoDB, Cosmos DB, RabbitMQ, and Redis/Valkey).
They are not open-ended plug-ins; extensibility means shaping platform credentials into the connection strings external drivers for those built-in connectors already understand.

Connectors map credentials from [Cloud Foundry service bindings](https://techdocs.broadcom.com/us/en/vmware-tanzu/platform/elastic-application-runtime/10-3/eart/binding-credentials.html) and
[Service Binding Spec for Kubernetes](https://github.com/servicebinding/spec#well-known-secret-entries) into configuration keys starting with `steeltoe:service-bindings` and merges them with local settings from `Steeltoe:Client`.
Each connector runs the binding logic for its own service type.

To use a third-party `VCAP_SERVICES` structure, populate the `steeltoe:service-bindings` keys yourself.
It is recommended to turn off the built-in binding logic to prevent conflicts by setting `SkipDefaultServiceBindings` to `true`.
Doing so will still merge with local settings from `Steeltoe:Client`.

> [!TIP]
> See the [Cloud Foundry configuration provider](../configuration/cloud-foundry-provider.md) for `vcap:*` keys and [`VCAP_SERVICES`](https://docs.cloudfoundry.org/devguide/deploy-apps/environment-variable.html#VCAP-SERVICES) in general.

For example, to use a third-party Cloud Foundry service broker for PostgreSQL that sets the `VCAP_SERVICES` environment variable to:

```json
{
  "custom-postgres-broker": [
    {
      "name": "products-db",
      "credentials": {
        "custom-hostname-key": "example.cloud.com",
        "custom-port-key": 2345,
        "custom-username-key": "products-user",
        "custom-password-key": "products-secret",
        "custom-database-name-key": "product-database"
      }
    },
    {
      "name": "orders-db",
      "credentials": {
        "custom-hostname-key": "example.cloud.com",
        "custom-port-key": 2345,
        "custom-username-key": "orders-user",
        "custom-password-key": "orders-secret",
        "custom-database-name-key": "order-database"
      }
    }
  ]
}
```

The following code can be used to map the PostgreSQL credentials to the format that [`NpgsqlConnectionStringBuilder`](https://www.npgsql.org/doc/api/Npgsql.NpgsqlConnectionStringBuilder.html) expects:

```c#
using Npgsql;
using Steeltoe.Configuration.CloudFoundry;
using Steeltoe.Connectors;
using Steeltoe.Connectors.PostgreSql;

var builder = WebApplication.CreateBuilder();
builder.AddCloudFoundryConfiguration();
MapCustomServiceBindings("custom-postgres-broker");
builder.AddPostgreSql(configure => configure.SkipDefaultServiceBindings = true, null);
var app = builder.Build();

var factory = app.Services.GetRequiredService<ConnectorFactory<PostgreSqlOptions, NpgsqlConnection>>();

PostgreSqlOptions productsDbOptions = factory.Get("products-db").Options;
Console.WriteLine(productsDbOptions.ConnectionString);
// Database=product-database;Host=example.cloud.com;Password=products-secret;Port=2345;Username=products-user

PostgreSqlOptions ordersDbOptions = factory.Get("orders-db").Options;
Console.WriteLine(ordersDbOptions.ConnectionString);
// Database=order-database;Host=example.cloud.com;Password=orders-secret;Port=2345;Username=orders-user

void MapCustomServiceBindings(string brokerName)
{
    var options = builder.Configuration.GetSection("vcap").Get<CloudFoundryServicesOptions>();

    foreach (CloudFoundryService service in options?.Services
        .Where(pair => pair.Key == brokerName)
        .SelectMany(pair => pair.Value) ?? [])
    {
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            // Map credentials into the property names expected by NpgsqlConnectionStringBuilder.
            [$"steeltoe:service-bindings:postgresql:{service.Name}:host"] = service.Credentials["custom-hostname-key"].Value,
            [$"steeltoe:service-bindings:postgresql:{service.Name}:port"] = service.Credentials["custom-port-key"].Value,
            [$"steeltoe:service-bindings:postgresql:{service.Name}:username"] = service.Credentials["custom-username-key"].Value,
            [$"steeltoe:service-bindings:postgresql:{service.Name}:password"] = service.Credentials["custom-password-key"].Value,
            [$"steeltoe:service-bindings:postgresql:{service.Name}:database"] = service.Credentials["custom-database-name-key"].Value
        });
    }
}
```

> [!TIP]
> See [Advanced settings](usage.md#advanced-settings) to customize the built-in Connectors.
