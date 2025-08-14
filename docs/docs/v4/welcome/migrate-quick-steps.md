# Migrating from Steeltoe 3

This topic provides quick steps to migrate existing applications to Steeltoe 4.
For non-trivial cases, see the related documentation topic and samples for v4.

> [!TIP]
> For detailed information on what has changed, see [What's new in Steeltoe 4](./whats-new.md).

## Bootstrap

For additional information, see the updated [Bootstrap documentation](../bootstrap/index.md).

Project file:

```diff
<Project>
  <ItemGroup>
-    <PackageReference Include="Steeltoe.Bootstrap.Autoconfig" Version="3.*" />
+    <PackageReference Include="Steeltoe.Bootstrap.AutoConfiguration" Version="4.0.0" />
  </ItemGroup>
</Project>
```

Program.cs:

```diff
-using Steeltoe.Bootstrap.Autoconfig;
+using Steeltoe.Bootstrap.AutoConfiguration;

var builder = WebApplication.CreateBuilder(args);
builder.AddSteeltoe();
```

## CircuitBreaker

CircuitBreaker (a .NET port of Netflix Hystrix) has been removed from Steeltoe in v4.
Use [Polly](https://github.com/App-vNext/Polly) instead.

## Common

TODO

## Configuration

For additional information, see the updated [Configuration documentation](../configuration/index.md) and
[Configuration samples](https://github.com/SteeltoeOSS/Samples/tree/main/Configuration).

### Cloud Foundry

Project file:

```diff
<Project>
  <ItemGroup>
-    <PackageReference Include="Steeltoe.Extensions.Configuration.CloudFoundryCore" Version="3.*" />
+    <PackageReference Include="Steeltoe.Configuration.CloudFoundry" Version="4.0.0" />
  </ItemGroup>
</Project>
```

#### Load `VCAP_SERVICES`/`VCAP_APPLICATION` into `IConfiguration`

Program.cs:

```diff
-using Steeltoe.Extensions.Configuration.CloudFoundry;
+using Steeltoe.Configuration.CloudFoundry;

var builder = WebApplication.CreateBuilder(args);
builder.AddCloudFoundryConfiguration();

Console.WriteLine($"Application name: {builder.Configuration["vcap:application:application_name"]}");

foreach (var section in builder.Configuration.GetRequiredSection("vcap:services").GetChildren())
{
    var plans = string.Join(", ", section
        .GetChildren()
        .SelectMany(child => child.GetChildren())
        .Where(child => child.Key == "plan")
        .Select(child => child.Value));
    Console.WriteLine($"Service: {section.Key} with plans: {plans}");
}
```

#### Load `VCAP_SERVICES`/`VCAP_APPLICATION` into `OptionsMonitor`

Program.cs:

```diff
using Microsoft.Extensions.Options;
-using Steeltoe.Extensions.Configuration.CloudFoundry;
+using Steeltoe.Configuration.CloudFoundry;

var builder = WebApplication.CreateBuilder(args);
builder.AddCloudFoundryConfiguration();
-builder.Services.ConfigureCloudFoundryOptions(builder.Configuration);

var app = builder.Build();

var appMonitor = app.Services.GetRequiredService<IOptionsMonitor<CloudFoundryApplicationOptions>>();
Console.WriteLine($"Application name: {appMonitor.CurrentValue.ApplicationName}");

var servicesMonitor = app.Services.GetRequiredService<IOptionsMonitor<CloudFoundryServicesOptions>>();
foreach (var services in servicesMonitor.CurrentValue.Services)
{
    var plans = string.Join(", ", services.Value.Select(service => service.Plan));
    Console.WriteLine($"Service: {services.Key} with plans: {plans}");
}
```

### Config Server

Project file:

```diff
<Project>
  <ItemGroup>
-    <PackageReference Include="Steeltoe.Extensions.Configuration.ConfigServerCore" Version="3.*" />
+    <PackageReference Include="Steeltoe.Configuration.ConfigServer" Version="4.0.0" />
  </ItemGroup>
</Project>
```

Program.cs:

```diff
-using Steeltoe.Extensions.Configuration.ConfigServer;
+using Steeltoe.Configuration.ConfigServer;

var builder = WebApplication.CreateBuilder(args);
builder.AddConfigServer();
```

### Kubernetes

Direct interaction with the Kubernetes API has been removed from Steeltoe in v4.

### Placeholder

Project file:

```diff
<Project>
  <ItemGroup>
-    <PackageReference Include="Steeltoe.Extensions.Configuration.PlaceholderCore" Version="3.*" />
+    <PackageReference Include="Steeltoe.Configuration.Placeholder" Version="4.0.0" />
  </ItemGroup>
</Project>
```

Program.cs:

```diff
-using Steeltoe.Extensions.Configuration.Placeholder;
+using Steeltoe.Configuration.Placeholder;

var builder = WebApplication.CreateBuilder(args);
-builder.AddPlaceholderResolver();
+builder.Configuration.AddPlaceholderResolver();
```

### Random Value

Project file:

```diff
<Project>
  <ItemGroup>
-    <PackageReference Include="Steeltoe.Extensions.Configuration.RandomValueBase" Version="3.*" />
+    <PackageReference Include="Steeltoe.Configuration.RandomValue" Version="4.0.0" />
  </ItemGroup>
</Project>
```

Program.cs:

```diff
-using Steeltoe.Extensions.Configuration.RandomValue;
+using Steeltoe.Configuration.RandomValue;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddRandomValueSource();
```

### Spring Boot

Project file:

```diff
<Project>
  <ItemGroup>
-    <PackageReference Include="Steeltoe.Extensions.Configuration.SpringBootCore" Version="3.*" />
+    <PackageReference Include="Steeltoe.Configuration.SpringBoot" Version="4.0.0" />
  </ItemGroup>
</Project>
```

Program.cs:

```diff
-using Steeltoe.Extensions.Configuration.SpringBoot;
+using Steeltoe.Configuration.SpringBoot;

var builder = WebApplication.CreateBuilder(args);
-builder.AddSpringBootConfiguration();
+builder.Configuration.AddSpringBootFromCommandLine(args);
+builder.Configuration.AddSpringBootFromEnvironmentVariable();
```

## Connectors

For additional information, see the updated [Connectors documentation](../configuration/index.md) and
[Configuration samples](https://github.com/SteeltoeOSS/Samples/tree/main/Connectors).

### MySQL using ADO.NET

Project file:

```diff
<Project>
  <ItemGroup>
    <PackageReference Include="MySql.Data" Version="9.4.0" />
-    <PackageReference Include="Steeltoe.Connector.ConnectorCore" Version="3.*" />
+    <PackageReference Include="Steeltoe.Connectors" Version="4.0.0" />
  </ItemGroup>
</Project>
```

appsettings.json:

```diff
{
-  "$schema": "https://steeltoe.io/schema/v3/schema.json",
+  "$schema": "https://steeltoe.io/schema/v4/schema.json",
-  "MySql:Client:ConnectionString": "Server=localhost;Database=steeltoe;Uid=steeltoe;Pwd=steeltoe"
+  "Steeltoe:Client:MySql:Default:ConnectionString": "Server=localhost;Database=steeltoe;Uid=steeltoe;Pwd=steeltoe"
}
```

Program.cs:

```diff
using MySql.Data.MySqlClient;
-using Steeltoe.Connector.MySql;
+using Steeltoe.Connectors;
+using Steeltoe.Connectors.MySql;

var builder = WebApplication.CreateBuilder(args);
-builder.Services.AddMySqlConnection(builder.Configuration);
+builder.AddMySql();

var app = builder.Build();

-await using var scope = app.Services.CreateAsyncScope();
-await using var connection = scope.ServiceProvider.GetRequiredService<MySqlConnection>();
+var factory = app.Services.GetRequiredService<ConnectorFactory<MySqlOptions, MySqlConnection>>();
+var connector = factory.Get();
+Console.WriteLine($"Using connection string: {connector.Options.ConnectionString}");
+await using var connection = connector.GetConnection();

await connection.OpenAsync();
await using var command = connection.CreateCommand();
command.CommandText = "SELECT 1";
var result = await command.ExecuteScalarAsync();
Console.WriteLine($"Query returned: {result}");
```

### MySQL using Entity Framework Core

Project file:

```diff
<Project>
  <ItemGroup>
    <PackageReference Include="MySql.EntityFrameworkCore" Version="9.0.6" />
-    <PackageReference Include="Steeltoe.Connector.ConnectorCore" Version="3.*" />
-    <PackageReference Include="Steeltoe.Connector.EFCore" Version="3.*" />
+    <PackageReference Include="Steeltoe.Connectors.EntityFrameworkCore" Version="4.0.0" />
  </ItemGroup>
</Project>
```

appsettings.json:

```diff
{
-  "$schema": "https://steeltoe.io/schema/v3/schema.json",
+  "$schema": "https://steeltoe.io/schema/v4/schema.json",
-  "MySql:Client:ConnectionString": "Server=localhost;Database=steeltoe;Uid=steeltoe;Pwd=steeltoe"
+  "Steeltoe:Client:MySql:Default:ConnectionString": "Server=localhost;Database=steeltoe;Uid=steeltoe;Pwd=steeltoe"
}
```

Program.cs:

```diff
using Microsoft.EntityFrameworkCore;
-using Steeltoe.Connector.MySql;
+using Steeltoe.Connectors.MySql;
-using Steeltoe.Connector.MySql.EFCore;
+using Steeltoe.Connectors.EntityFrameworkCore.MySql;

var builder = WebApplication.CreateBuilder(args);
-builder.Services.AddDbContext<AppDbContext>(options => options.UseMySql(builder.Configuration));
-builder.Services.AddMySqlHealthContributor(builder.Configuration);
+builder.AddMySql();
+builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) => options.UseMySql(serviceProvider));

var app = builder.Build();

await using var scope = app.Services.CreateAsyncScope();
await using var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
var rowCount = await dbContext.ExampleEntities.CountAsync();
Console.WriteLine($"Found {rowCount} rows.");
```

### PostgreSQL using ADO.NET

Project file:

```diff
<Project>
  <ItemGroup>
    <PackageReference Include="Npgsql" Version="9.0.3" />
-    <PackageReference Include="Steeltoe.Connector.ConnectorCore" Version="3.*" />
+    <PackageReference Include="Steeltoe.Connectors" Version="4.0.0" />
  </ItemGroup>
</Project>
```

appsettings.json:

```diff
{
-  "$schema": "https://steeltoe.io/schema/v3/schema.json",
+  "$schema": "https://steeltoe.io/schema/v4/schema.json",
-  "Postgres:Client:ConnectionString": "Server=localhost;Database=steeltoe;Uid=steeltoe;Pwd=steeltoe"
+  "Steeltoe:Client:PostgreSQL:Default:ConnectionString": "Server=localhost;Database=steeltoe;Uid=steeltoe;Pwd=steeltoe"
}
```

Program.cs:

```diff
using Npgsql;
-using Steeltoe.Connector.PostgreSql;
+using Steeltoe.Connectors;
+using Steeltoe.Connectors.PostgreSql;

var builder = WebApplication.CreateBuilder(args);
-builder.Services.AddPostgresConnection(builder.Configuration);
+builder.AddPostgreSql();

var app = builder.Build();

-await using var scope = app.Services.CreateAsyncScope();
-await using var connection = scope.ServiceProvider.GetRequiredService<NpgsqlConnection>();
+var factory = app.Services.GetRequiredService<ConnectorFactory<PostgreSqlOptions, NpgsqlConnection>>();
+var connector = factory.Get();
+Console.WriteLine($"Using connection string: {connector.Options.ConnectionString}");
+await using var connection = connector.GetConnection();

await connection.OpenAsync();
await using var command = connection.CreateCommand();
command.CommandText = "SELECT 1";
var result = await command.ExecuteScalarAsync();
Console.WriteLine($"Query returned: {result}");
```

### PostgreSQL using Entity Framework Core

Project file:

```diff
<Project>
  <ItemGroup>
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.4" />
-    <PackageReference Include="Steeltoe.Connector.ConnectorCore" Version="3.*" />
-    <PackageReference Include="Steeltoe.Connector.EFCore" Version="3.*" />
+    <PackageReference Include="Steeltoe.Connectors.EntityFrameworkCore" Version="4.0.0" />
  </ItemGroup>
</Project>
```

appsettings.json:

```diff
{
-  "$schema": "https://steeltoe.io/schema/v3/schema.json",
+  "$schema": "https://steeltoe.io/schema/v4/schema.json",
-  "Postgres:Client:ConnectionString": "Server=localhost;Database=steeltoe;Uid=steeltoe;Pwd=steeltoe"
+  "Steeltoe:Client:PostgreSQL:Default:ConnectionString": "Server=localhost;Database=steeltoe;Uid=steeltoe;Pwd=steeltoe"
}
```

Program.cs:

```diff
using Microsoft.EntityFrameworkCore;
-using Steeltoe.Connector.PostgreSql;
+using Steeltoe.Connectors.PostgreSql;
-using Steeltoe.Connector.PostgreSql.EFCore;
+using Steeltoe.Connectors.EntityFrameworkCore.PostgreSql;

var builder = WebApplication.CreateBuilder(args);
-builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration));
-builder.Services.AddPostgresHealthContributor(builder.Configuration);
+builder.AddPostgreSql();
+builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) => options.UseNpgsql(serviceProvider));

var app = builder.Build();

await using var scope = app.Services.CreateAsyncScope();
await using var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
var rowCount = await dbContext.ExampleEntities.CountAsync();
Console.WriteLine($"Found {rowCount} rows.");
```





### RabbitMQ

Project file:

```diff
<Project>
  <ItemGroup>
    <PackageReference Include="RabbitMQ.Client" Version="7.1.2" />
-    <PackageReference Include="Steeltoe.Connector.ConnectorCore" Version="3.*" />
+    <PackageReference Include="Steeltoe.Connectors" Version="4.0.0" />
  </ItemGroup>
</Project>
```

appsettings.json:

```diff
{
-  "$schema": "https://steeltoe.io/schema/v3/schema.json",
+  "$schema": "https://steeltoe.io/schema/v4/schema.json",
-  "RabbitMQ:Client:ConnectionString": "Server=localhost"
+  "Steeltoe:Client:RabbitMQ:Default:ConnectionString": "amqp://localhost:5672"
}
```

> [!TIP]
> See the RabbitMQ documentation [here](https://www.rabbitmq.com/docs/uri-spec) and [here](https://www.rabbitmq.com/docs/uri-query-parameters) for the `ConnectionString` URI format.

Program.cs:

```diff
using RabbitMQ.Client;
-using Steeltoe.Connector.RabbitMQ;
+using Steeltoe.Connectors;
+using Steeltoe.Connectors.RabbitMQ;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
-builder.Services.AddRabbitMQConnection(builder.Configuration, ServiceLifetime.Singleton);
+builder.AddRabbitMQ();

var app = builder.Build();

-var connectionFactory = app.Services.GetRequiredService<IConnectionFactory>();
-await using var connection = await connectionFactory.CreateConnectionAsync();
+var factory = app.Services.GetRequiredService<ConnectorFactory<RabbitMQOptions, IConnection>>();
+var connector = factory.Get();
+Console.WriteLine($"Using connection string: {connector.Options.ConnectionString}");
+await using var connection = connector.GetConnection();
await using var channel = await connection.CreateChannelAsync();
const string queueName = "example-queue-name";
await channel.QueueDeclareAsync(queueName);

byte[] messageToSend = "example-message"u8.ToArray();
await channel.BasicPublishAsync(exchange: "", queueName, mandatory: true, new BasicProperties(), messageToSend);

var result = await channel.BasicGetAsync(queueName, autoAck: true);
string messageReceived = result == null ? "(none)" : Encoding.UTF8.GetString(result.Body.ToArray());
Console.WriteLine($"Received message: {messageReceived}");
```

---
TODO FROM HERE...
---

### __TEMPLATE__

Project file:

```diff
<Project>
  <ItemGroup>
-    <PackageReference Include="XXXXX" Version="3.*" />
+    <PackageReference Include="XXXXX" Version="4.0.0" />
  </ItemGroup>
</Project>
```

Program.cs:

```diff
-XXXXX
+XXXXX

var builder = WebApplication.CreateBuilder(args);
-XXXXX
+XXXXX
```




## Discovery

### Behavior changes

- Multiple discovery clients (one per type) can be active
- Simplified API: Use `IServiceCollection.Add*DiscoveryClient()` to register; `IHttpClientBuilder.AddServiceDiscovery()` to consume
- Refactored configuration to use ASP.NET Options pattern, responding to changes at runtime nearly everywhere
- Async all-the-way where possible
- More reliable cross-platform detection of local hostname and IP address
- Now supports global service discovery: `services.ConfigureHttpClientDefaults(builder => builder.AddServiceDiscovery())`
- Config Server discovery-first can now query Consul, Eureka, and Configuration-based
- Improved detection of port bindings, including support for new ASP.NET [environment variables](https://learn.microsoft.com/aspnet/core/release-notes/aspnetcore-8.0#http_ports-and-https_ports-config-keys)
- HTTP handlers now depend on a load balancer, which delegates to multiple discovery clients
- Major improvements in documentation, samples cover more use cases
- Eureka: API reduced to what Steeltoe needs for service discovery (too many unanswered questions to provide generic client)
- Eureka: Added support for ASP.NET dynamic port bindings
- Eureka: Optimized communication to Eureka server
- Eureka: Made resilient to concurrent changes, fixing race conditions and lost updates
- Eureka: Made types returned from server immutable; use `EurekaApplicationInfoManager.UpdateInstance()` to change local instance
- Eureka: Server communication via `HttpClientFactory` (allows custom handlers, telemetry, respond to DNS changes, etc)
- Eureka: Client certificate watched in `IConfiguration`, can be shared with other `HttpClient`s
- Eureka: Health handler (to determine local instance status) now runs both contributors and ASP.NET health checks
- Eureka: Prefer secure port when both secure/non-secure are returned from Eureka
- Eureka: Support comma-separated list of multiple names in setting `Eureka:Instance:VipAddress` and `Eureka:Instance:SecureVipAddress`
- Eureka: Improved handling of the various registration methods on Cloud Foundry

### NuGet Package changes

| Source | Change | Replacement | Notes |
| --- | --- | --- | --- |
| Steeltoe.Discovery.Abstractions | Removed | Steeltoe.Common package | No longer needed (except for `IDiscoveryClient`, which moved to Steeltoe.Common package) |
| Steeltoe.Discovery.ClientBase | Removed | Steeltoe.Discovery.Configuration package | Provides configuration-based discovery |
| Steeltoe.Discovery.ClientCore | Removed | Steeltoe.Discovery.Configuration package | Provides configuration-based discovery |
| Steeltoe.Discovery.Configuration | Added | | Provides a configuration-based discovery client |
| Steeltoe.Discovery.HttpClients | Added | | Provides consumption of `IDiscoveryClient`(s) in `HttpClient`/`HttpClientFactory` pipeline |
| Steeltoe.Discovery.Kubernetes | Removed | None | |

### API changes

| Source | Kind | Package | Change | Replacement | Notes |
| --- | --- | --- | --- | --- | --- |
| `Steeltoe.Discovery.Client.ConfigurationUrlHelpers` | Type | Steeltoe.Discovery.Client [Base/Core] | Removed | None | Similar logic moved to internal `ConfigurationExtensions.GetListenAddresses` |
| `Steeltoe.Discovery.Client.DiscoveryApplicationBuilderExtensions.UseDiscoveryClient` | Extension method | Steeltoe.Discovery.Client [Base/Core] | Removed | Call `IServiceCollection.Add\*DiscoveryClient()` extension method | |
| `Steeltoe.Discovery.Client.DiscoveryClientBuilder` | Type | Steeltoe.Discovery.Client [Base/Core] | Removed | Call `IServiceCollection.Add\*DiscoveryClient()` extension method | Refactored to simpler API |
| `Steeltoe.Discovery.Client.DiscoveryClientStartupFilter` | Type | Steeltoe.Discovery.Client [Base/Core] | Removed | None | No longer needed |
| `Steeltoe.Discovery.Client.DiscoveryHostBuilderExtensions.AddDiscoveryClient` | Extension method | Steeltoe.Discovery.Client [Base/Core] | Removed | Call `IServiceCollection.Add\*DiscoveryClient()` extension method | Refactored to simpler API |
| `Steeltoe.Discovery.Client.DiscoveryHostBuilderExtensions.AddServiceDiscovery` | Extension method | Steeltoe.Discovery.Client [Base/Core] | Removed | Call `IHttpClientBuilder.AddServiceDiscovery()` extension method | Refactored to simpler API |
| `Steeltoe.Discovery.Client.DiscoveryServiceCollectionExtensions.AddDiscoveryClient` | Extension method | Steeltoe.Discovery.Client [Base/Core] | Removed | Call `IServiceCollection.Add\*DiscoveryClient()` extension method | Refactored to simpler API |
| `Steeltoe.Discovery.Client.DiscoveryServiceCollectionExtensions.AddServiceDiscovery` | Extension method | Steeltoe.Discovery.Client [Base/Core] | Removed | Call `IHttpClientBuilder.AddServiceDiscovery()` extension method | Refactored to simpler API |
| `Steeltoe.Discovery.Client.DiscoveryServiceCollectionExtensions.ApplicationLifecycle` | Type | Steeltoe.Discovery.Client [Base/Core] | Removed | None | No longer needed |
| `Steeltoe.Discovery.Client.DiscoveryServiceCollectionExtensions.GetNamedDiscoveryServiceInfo` | Extension method | Steeltoe.Discovery.Client [Base/Core] | Removed | Inject `IEnumerable<IDiscoveryClient>` | Refactored to simpler API |
| `Steeltoe.Discovery.Client.DiscoveryServiceCollectionExtensions.GetSingletonDiscoveryServiceInfo` | Extension method | Steeltoe.Discovery.Client [Base/Core] | Removed | Inject `IEnumerable<IDiscoveryClient>` | Refactored to simpler API |
| `Steeltoe.Discovery.Client.DiscoveryWebApplicationBuilderExtensions.AddDiscoveryClient` | Extension method | Steeltoe.Discovery.Client [Base/Core] | Removed | Call `IServiceCollection.Add\*DiscoveryClient()` extension method | Refactored to simpler API |
| `Steeltoe.Discovery.Client.DiscoveryWebApplicationBuilderExtensions.AddServiceDiscovery` | Extension method | Steeltoe.Discovery.Client [Base/Core] | Removed | Call `IHttpClientBuilder.AddServiceDiscovery()` extension method | Refactored to simpler API |
| `Steeltoe.Discovery.Client.DiscoveryWebHostBuilderExtensions.AddDiscoveryClient` | Extension method | Steeltoe.Discovery.Client [Base/Core] | Removed | Call `IServiceCollection.Add\*DiscoveryClient()` extension method | Refactored to simpler API |
| `Steeltoe.Discovery.Client.DiscoveryWebHostBuilderExtensions.AddServiceDiscovery` | Extension method | Steeltoe.Discovery.Client [Base/Core] | Removed | Call `IHttpClientBuilder.AddServiceDiscovery()` extension method | Refactored to simpler API |
| `Steeltoe.Discovery.Client.IDiscoveryClientExtension` | Type | Steeltoe.Discovery.Client [Base/Core] | Removed | Implement `IDiscoveryClient` directly, add it to service container | Refactored to simpler API |
| `Steeltoe.Discovery.Client.SimpleClients.ConfigurationDiscoveryClient` | Type | Steeltoe.Discovery.Client [Base/Core] | Moved | `ConfigurationDiscoveryClient` in Steeltoe.Discovery.Configuration package | |
| `Steeltoe.Discovery.Client.SimpleClients.ConfigurationDiscoveryClientBuilderExtensions.UseConfiguredInstances` | Extension method | Steeltoe.Discovery.Client [Base/Core] | Removed | Call `IServiceCollection.AddConfigurationDiscoveryClient()` extension method | Refactored to simpler API |
| `Steeltoe.Discovery.Client.SimpleClients.ConfigurationDiscoveryClientExtension` | Type | Steeltoe.Discovery.Client [Base/Core] | Removed | Call `IServiceCollection.AddConfigurationDiscoveryClient()` extension method | Refactored to simpler API |
| `Steeltoe.Discovery.DiscoveryClientAssemblyAttribute` | Type | Steeltoe.Discovery.Client [Base/Core] | Removed | None | Dynamically loading custom discovery clients is no longer possible |
| `Steeltoe.Discovery.Configuration.ConfigurationDiscoveryClient` | Type | Steeltoe.Discovery.Configuration | Added | | Reads service instances from configuration |
| `Steeltoe.Discovery.Configuration.ConfigurationDiscoveryOptions` | Type | Steeltoe.Discovery.Configuration | Added | | Options type for service instances in `IConfiguration` |
| `Steeltoe.Discovery.Configuration.ConfigurationServiceCollectionExtensions.AddConfigurationDiscoveryClient` | Extension method | Steeltoe.Discovery.Configuration | Added | | Activates configuration-based discovery client |
| `Steeltoe.Discovery.Configuration.ConfigurationServiceInstance` | Type | Steeltoe.Discovery.Configuration | Added | | Implements `IServiceInstance` for configuration-based discovery |
| `Steeltoe.Discovery.HttpClients.DiscoveryHttpClientBuilderExtensions.AddServiceDiscovery` | Extension method | Steeltoe.Discovery.HttpClients | Added | | Activates service discovery using the randomized load balancer |
| `Steeltoe.Discovery.HttpClients.DiscoveryHttpClientBuilderExtensions.AddServiceDiscovery<T>` | Extension method | Steeltoe.Discovery.HttpClients | Added | | Activates service discovery using a custom ILoadBalancer |
| `Steeltoe.Discovery.HttpClients.DiscoveryHttpClientHandler` | Type | Steeltoe.Discovery.HttpClients | Added | | An `HttpClientHandler` (for `HttpClient`) that load-balances over service instances |
| `Steeltoe.Discovery.HttpClients.DiscoveryHttpDelegatingHandler` | Type | Steeltoe.Discovery.HttpClients | Added | | An `DelegatingHandler` (for `IHttpClientFactory`) that load-balances over service instances |
| `Steeltoe.Discovery.HttpClients.LoadBalancers.ILoadBalancer` | Type | Steeltoe.Discovery.HttpClients | Added | | Chooses a service instance obtained from discovery client(s) |
| `Steeltoe.Discovery.HttpClients.LoadBalancers.RandomLoadBalancer` | Type | Steeltoe.Discovery.HttpClients | Added | | Chooses a random service instance obtained from discovery client(s) |
| `Steeltoe.Discovery.HttpClients.LoadBalancers.RoundRobinLoadBalancer` | Type | Steeltoe.Discovery.HttpClients | Added | | Chooses a service instance obtained from discovery client(s) using round robin |
| `Steeltoe.Discovery.HttpClients.LoadBalancers.ServiceInstancesResolver` | Type | Steeltoe.Discovery.HttpClients | Added | | Used by load balancers to retrieve service instances from discovery clients |
| `Steeltoe.Discovery.Consul.ConsulClientFactory` | Type | Steeltoe.Discovery.Consul | Removed | Use Consul package directly | No longer needed |
| `Steeltoe.Discovery.Consul.ConsulDiscoveryClientBuilderExtensions.UseConsul` | Extension method | Steeltoe.Discovery.Consul | Removed | Call `IServiceCollection.AddConsulDiscoveryClient()` extension method | |
| `Steeltoe.Discovery.Consul.ConsulDiscoveryClientExtension` | Type | Steeltoe.Discovery.Consul | Removed | Call `IServiceCollection.AddConsulDiscoveryClient()` extension method | |
| `Steeltoe.Discovery.Consul.ConsulOptions` | Type | Steeltoe.Discovery.Consul | Moved | `Steeltoe.Discovery.Consul.Configuration.ConsulOptions` | |
| `Steeltoe.Discovery.Consul.ConsulPostConfigurer` | Type | Steeltoe.Discovery.Consul | Removed | None | Moved to internal type `PostConfigureConsulDiscoveryOptions` |
| `Steeltoe.Discovery.Consul.ConsulServiceCollectionExtensions.AddConsulDiscoveryClient` | Extension method | Steeltoe.Discovery.Consul | Added | | Activates the Consul discovery client |
| `Steeltoe.Discovery.Consul.Discovery.ConsulDiscoveryClient.Dispose` | Method | Steeltoe.Discovery.Consul | Removed | Call `ShutdownAsync()` before `IServiceProvider` is disposed | `ShutdownAsync()` is called by internal `DiscoveryClientHostedService` |
| `Steeltoe.Discovery.Consul.Discovery.ConsulDiscoveryClient.EnsureAssemblyIsLoaded` | Method | Steeltoe.Discovery.Consul | Removed | None | No longer needed |
| `Steeltoe.Discovery.Consul.Discovery.ConsulDiscoveryOptions` | Type | Steeltoe.Discovery.Consul | Moved | `Steeltoe.Discovery.Consul.Configuration.ConsulDiscoveryOptions` | |
| `Steeltoe.Discovery.Consul.Discovery.ConsulDiscoveryOptions.ApplyConfigUrls` | Method | Steeltoe.Discovery.Consul | Removed | None | Refactored to internal logic, happens automatically based on settings |
| `Steeltoe.Discovery.Consul.Discovery.ConsulDiscoveryOptions.ApplyNetUtils` | Method | Steeltoe.Discovery.Consul | Removed | None | Refactored to internal logic, happens automatically based on settings |
| `Steeltoe.Discovery.Consul.Discovery.ConsulDiscoveryOptions.CacheTTL` | Property | Steeltoe.Discovery.Consul | Removed | Use caching provided by `ServiceInstancesResolver` | |
| `Steeltoe.Discovery.Consul.Discovery.ConsulDiscoveryOptions.IpAddress` | Property | Steeltoe.Discovery.Consul | Renamed | `ConsulDiscoveryOptions.IPAddress` | |
| `Steeltoe.Discovery.Consul.Discovery.ConsulDiscoveryOptions.IsHeartBeatEnabled` | Property | Steeltoe.Discovery.Consul | Removed | `ConsulDiscoveryOptions.Heartbeat.Enabled` | |
| `Steeltoe.Discovery.Consul.Discovery.ConsulDiscoveryOptions.IsRetryEnabled` | Property | Steeltoe.Discovery.Consul | Removed | `ConsulDiscoveryOptions.Retry.Enabled` | |
| `Steeltoe.Discovery.Consul.Discovery.ConsulDiscoveryOptions.NetUtils` | Property | Steeltoe.Discovery.Consul | Removed | None | OS-based network APIs are no longer pluggable |
| `Steeltoe.Discovery.Consul.Discovery.ConsulDiscoveryOptions.PreferAgentAddress` | Property | Steeltoe.Discovery.Consul | Removed | None | Undocumented and did not work |
| `Steeltoe.Discovery.Consul.Discovery.ConsulDiscoveryOptions.PreferIpAddress` | Property | Steeltoe.Discovery.Consul | Renamed | `ConsulDiscoveryOptions.PreferIPAddress` | |
| `Steeltoe.Discovery.Consul.Discovery.ConsulDiscoveryOptions.TagsAsMetadata` | Property | Steeltoe.Discovery.Consul | Removed | Configure `Tags` and `Metadata` directly | Redundant |
| `Steeltoe.Discovery.Consul.Discovery.ConsulDiscoveryOptions.UseNetUtils` | Property | Steeltoe.Discovery.Consul | Renamed | `ConsulDiscoveryOptions.UseNetworkInterfaces` | |
| `Steeltoe.Discovery.Consul.Discovery.ConsulHealthContributor` | Type | Steeltoe.Discovery.Consul | Removed | None | Made internal, not designed for reuse/extensibility |
| `Steeltoe.Discovery.Consul.Discovery.ConsulHeartbeatOptions` | Type | Steeltoe.Discovery.Consul | Moved | `Steeltoe.Discovery.Consul.Configuration.ConsulHeartbeatOptions` | |
| `Steeltoe.Discovery.Consul.Discovery.ConsulRetryOptions` | Type | Steeltoe.Discovery.Consul | Moved | `Steeltoe.Discovery.Consul.Configuration.ConsulRetryOptions` | |
| `Steeltoe.Discovery.Consul.Discovery.ConsulServiceInstance` | Type | Steeltoe.Discovery.Consul | Removed | None | Made internal |
| `Steeltoe.Discovery.Consul.Discovery.IConsulDiscoveryClient` | Type | Steeltoe.Discovery.Consul | Removed | `Steeltoe.Discovery.Consul.ConsulDiscoveryClient` | |
| `Steeltoe.Discovery.Consul.Discovery.IScheduler` | Type | Steeltoe.Discovery.Consul | Removed | None | Custom implementations are no longer possible |
| `Steeltoe.Discovery.Consul.Discovery.TtlScheduler` | Type | Steeltoe.Discovery.Consul | Removed | None | Made internal |
| `Steeltoe.Discovery.Consul.Registry.ConsulRegistration` | Type | Steeltoe.Discovery.Consul | Removed | None | Made internal |
| `Steeltoe.Discovery.Consul.Registry.ConsulServiceRegistrar` | Type | Steeltoe.Discovery.Consul | Removed | Use Consul package directly | Made internal |
| `Steeltoe.Discovery.Consul.Registry.ConsulServiceRegistry` | Type | Steeltoe.Discovery.Consul | Removed | Use Consul package directly | Made internal |
| `Steeltoe.Discovery.Consul.Registry.IConsulRegistration` | Type | Steeltoe.Discovery.Consul | Removed | None | Custom implementations are no longer possible |
| `Steeltoe.Discovery.Consul.Registry.IConsulServiceRegistrar` | Type | Steeltoe.Discovery.Consul | Removed | None | Custom implementations are no longer possible |
| `Steeltoe.Discovery.Consul.Registry.IConsulServiceRegistry` | Type | Steeltoe.Discovery.Consul | Removed | None | Custom implementations are no longer possible |
| `Steeltoe.Discovery.Consul.Registry.IServiceRegistrar` | Type | Steeltoe.Discovery.Consul | Removed | None | Custom implementations are no longer possible |
| `Steeltoe.Discovery.Consul.Util.ConsulServerUtils` | Type | Steeltoe.Discovery.Consul | Removed | None | Made internal |
| `Steeltoe.Discovery.Consul.Util.DateTimeConversions` | Type | Steeltoe.Discovery.Consul | Removed | None | Made internal |
| `Steeltoe.Discovery.Eureka.AppInfo.Application` | Type | Steeltoe.Discovery.Eureka | Renamed | `Steeltoe.Discovery.Eureka.AppInfo.ApplicationInfo` | |
| `Steeltoe.Discovery.Eureka.AppInfo.Application.Count` | Property | Steeltoe.Discovery.Eureka | Removed | `ApplicationInfo.Instances.Count` | Redundant |
| `Steeltoe.Discovery.Eureka.AppInfo.Application.GetInstance` | Method | Steeltoe.Discovery.Eureka | Removed | Find entry in `ApplicationInfo.Instances` | Made internal |
| `Steeltoe.Discovery.Eureka.AppInfo.Applications` | Type | Steeltoe.Discovery.Eureka | Renamed | `Steeltoe.Discovery.Eureka.AppInfo.ApplicationInfoCollection` | Now implements `IReadOnlyCollection<ApplicationInfo>` |
| `Steeltoe.Discovery.Eureka.AppInfo.Applications.GetInstancesBySecureVirtualHostName` | Method | Steeltoe.Discovery.Eureka | Removed | Enumerate via collection interface | Renamed to internal `GetInstancesBySecureVipAddress` |
| `Steeltoe.Discovery.Eureka.AppInfo.Applications.GetInstancesByVirtualHostName` | Method | Steeltoe.Discovery.Eureka | Removed | Enumerate via collection interface | Renamed to internal `GetInstancesByVipAddress` |
| `Steeltoe.Discovery.Eureka.AppInfo.Applications.GetRegisteredApplication` | Method | Steeltoe.Discovery.Eureka | Removed | Enumerate via collection interface | Made internal |
| `Steeltoe.Discovery.Eureka.AppInfo.Applications.GetRegisteredApplications` | Method | Steeltoe.Discovery.Eureka | Removed | Enumerate via collection interface | Redundant |
| `Steeltoe.Discovery.Eureka.AppInfo.DataCenterInfo` | Type | Steeltoe.Discovery.Eureka | Moved | `Steeltoe.Discovery.Eureka.Configuration.DataCenterInfo` | |
| `Steeltoe.Discovery.Eureka.AppInfo.IDataCenterInfo` | Type | Steeltoe.Discovery.Eureka | Removed | `Steeltoe.Discovery.Eureka.Configuration.DataCenterInfo` | |
| `Steeltoe.Discovery.Eureka.AppInfo.InstanceInfo.Actiontype` | Property | Steeltoe.Discovery.Eureka | Renamed | `InstanceInfo.ActionType` | |
| `Steeltoe.Discovery.Eureka.AppInfo.InstanceInfo.AsgName` | Property | Steeltoe.Discovery.Eureka | Renamed | `InstanceInfo.AutoScalingGroupName` | |
| `Steeltoe.Discovery.Eureka.AppInfo.InstanceInfo.EffectiveStatus` | Property | Steeltoe.Discovery.Eureka | Added | | Calculated based on `Status` and `OverriddenStatus` |
| `Steeltoe.Discovery.Eureka.AppInfo.InstanceInfo.IpAddr` | Property | Steeltoe.Discovery.Eureka | Renamed | `InstanceInfo.IPAddress` | |
| `Steeltoe.Discovery.Eureka.AppInfo.InstanceInfo.IsUnsecurePortEnabled` | Property | Steeltoe.Discovery.Eureka | Renamed | `InstanceInfo.IsNonSecurePortEnabled` | |
| `Steeltoe.Discovery.Eureka.AppInfo.InstanceInfo.LastDirtyTimestamp` | Property | Steeltoe.Discovery.Eureka | Renamed | `InstanceInfo.LastDirtyTimeUtc` | Changed type from `long` to `DateTime` |
| `Steeltoe.Discovery.Eureka.AppInfo.InstanceInfo.LastUpdatedTimestamp` | Property | Steeltoe.Discovery.Eureka | Renamed | `InstanceInfo.LastUpdatedTimeUtc` | Changed type from `long` to `DateTime` |
| `Steeltoe.Discovery.Eureka.AppInfo.InstanceInfo.Port` | Property | Steeltoe.Discovery.Eureka | Renamed | `InstanceInfo.NonSecurePort` | |
| `Steeltoe.Discovery.Eureka.AppInfo.InstanceStatus` | Type | Steeltoe.Discovery.Eureka | Members changed | | Reordered enum members (0 = Unknown) |
| `Steeltoe.Discovery.Eureka.AppInfo.LeaseInfo.DurationInSecs` | Property | Steeltoe.Discovery.Eureka | Renamed | `LeaseInfo.Duration` | Changed type from `int` to `TimeSpan` |
| `Steeltoe.Discovery.Eureka.AppInfo.LeaseInfo.EvictionTimestamp` | Property | Steeltoe.Discovery.Eureka | Renamed | `LeaseInfo.EvictionTimeUtc` | Changed type from `long` to `DateTime` |
| `Steeltoe.Discovery.Eureka.AppInfo.LeaseInfo.LastRenewalTimestamp` | Property | Steeltoe.Discovery.Eureka | Renamed | `LeaseInfo.LastRenewalTimeUtc` | Changed type from `long` to `DateTime` |
| `Steeltoe.Discovery.Eureka.AppInfo.LeaseInfo.LastRenewalTimestampLegacy` | Property | Steeltoe.Discovery.Eureka | Removed | None | Legacy syntax handled internally |
| `Steeltoe.Discovery.Eureka.AppInfo.LeaseInfo.RegistrationTimestamp` | Property | Steeltoe.Discovery.Eureka | Renamed | `LeaseInfo.RegistrationTimeUtc` | Changed type from `long` to `DateTime` |
| `Steeltoe.Discovery.Eureka.AppInfo.LeaseInfo.RenewalIntervalInSecs` | Property | Steeltoe.Discovery.Eureka | Renamed | `LeaseInfo.RenewalInterval` | Changed type from `int` to `TimeSpan` |
| `Steeltoe.Discovery.Eureka.AppInfo.LeaseInfo.ServiceUpTimestamp` | Property | Steeltoe.Discovery.Eureka | Renamed | `LeaseInfo.ServiceUpTimeUtc` | Changed type from `long` to `DateTime` |
| `Steeltoe.Discovery.Eureka.ApplicationInfoManager` | Type | Steeltoe.Discovery.Eureka | Removed | `Steeltoe.Discovery.Eureka.EurekaApplicationInfoManager` | |
| `Steeltoe.Discovery.Eureka.ApplicationsFetchedEventArgs` | Type | Steeltoe.Discovery.Eureka | Added | | List of apps for `EurekaDiscoveryClient.ApplicationsFetched` event |
| `Steeltoe.Discovery.Eureka.DiscoveryClient` | Type | Steeltoe.Discovery.Eureka | Removed | `EurekaDiscoveryClient` | |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.Applications` | Property | Steeltoe.Discovery.Eureka | Removed | Subscribe to `EurekaDiscoveryClient.ApplicationsFetched` event | Made internal to guarantee correctness |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.FetchFullRegistryAsync` | Method | Steeltoe.Discovery.Eureka | Removed | Subscribe to `EurekaDiscoveryClient.ApplicationsFetched` event | Made internal to guarantee correctness |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.FetchRegistryAsync` | Method | Steeltoe.Discovery.Eureka | Removed | Subscribe to `EurekaDiscoveryClient.ApplicationsFetched` event | Made internal to guarantee correctness |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.FetchRegistryDeltaAsync` | Method | Steeltoe.Discovery.Eureka | Removed | Subscribe to `EurekaDiscoveryClient.ApplicationsFetched` event | Made internal to guarantee correctness |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.GetApplication` | Method | Steeltoe.Discovery.Eureka | Removed | Subscribe to `EurekaDiscoveryClient.ApplicationsFetched` event | Made internal to guarantee correctness |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.GetInstanceById` | Method | Steeltoe.Discovery.Eureka | Removed | Subscribe to `EurekaDiscoveryClient.ApplicationsFetched` event | Made internal to guarantee correctness |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.GetInstanceRemoteStatus` | Method | Steeltoe.Discovery.Eureka | Removed | Subscribe to `EurekaDiscoveryClient.ApplicationsFetched` event | Made internal to guarantee correctness |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.GetInstancesByVipAddress` | Method | Steeltoe.Discovery.Eureka | Removed | Subscribe to `EurekaDiscoveryClient.ApplicationsFetched` event | Made internal to guarantee correctness |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.GetInstancesByVipAddressAndAppName` | Method | Steeltoe.Discovery.Eureka | Removed | Subscribe to `EurekaDiscoveryClient.ApplicationsFetched` event | Made internal to guarantee correctness |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.GetNextServerFromEureka` | Method | Steeltoe.Discovery.Eureka | Removed | None | Refactored to internal `EurekaServiceUriStateManager` |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.HealthCheckHandler` | Property | Steeltoe.Discovery.Eureka | Removed | Implement `IHealthCheckHandler`, add it to service container | |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.HttpClient` | Property | Steeltoe.Discovery.Eureka | Removed | None | |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.Initialize` | Method | Steeltoe.Discovery.Eureka | Removed | None | No longer needed |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.InitializeAsync` | Method | Steeltoe.Discovery.Eureka | Removed | None | No longer needed |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.LastGoodDeltaRegistryFetchTimestamp` | Property | Steeltoe.Discovery.Eureka | Removed | None | Made internal to guarantee correctness |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.LastGoodFullRegistryFetchTimestamp` | Property | Steeltoe.Discovery.Eureka | Removed | None | Made internal to guarantee correctness |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.LastGoodHeartbeatTimestamp` | Property | Steeltoe.Discovery.Eureka | Removed | None | Made internal to guarantee correctness |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.LastGoodRegisterTimestamp` | Property | Steeltoe.Discovery.Eureka | Removed | None | Made internal to guarantee correctness |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.LastGoodRegistryFetchTimestamp` | Property | Steeltoe.Discovery.Eureka | Removed | None | Made internal to guarantee correctness |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.LastRemoteInstanceStatus` | Property | Steeltoe.Discovery.Eureka | Removed | None | Made internal to guarantee correctness |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.OnApplicationsChange` | Event | Steeltoe.Discovery.Eureka | Removed | `EurekaDiscoveryClient.ApplicationsFetched` | Was raised after every fetch, even if nothing changed |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.RefreshInstanceInfo` | Method | Steeltoe.Discovery.Eureka | Removed | None | Made internal to guarantee correctness |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.RegisterAsync` | Method | Steeltoe.Discovery.Eureka | Removed | None | Made internal to guarantee correctness |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.RegisterDirtyInstanceInfo` | Method | Steeltoe.Discovery.Eureka | Removed | None | Made internal to guarantee correctness |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.RenewAsync` | Method | Steeltoe.Discovery.Eureka | Removed | None | Made internal to guarantee correctness |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.StartTimer` | Method | Steeltoe.Discovery.Eureka | Removed | None | Made internal to guarantee correctness |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.UnregisterAsync` | Method | Steeltoe.Discovery.Eureka | Removed | None | Made internal to guarantee correctness |
| `Steeltoe.Discovery.Eureka.DiscoveryManager` | Type | Steeltoe.Discovery.Eureka | Removed | `Steeltoe.Discovery.Eureka.EurekaDiscoveryClient` | |
| `Steeltoe.Discovery.Eureka.EurekaApplicationInfoManager.Instance` | Property | Steeltoe.Discovery.Eureka | Added | | Gets immutable snapshot of the local service instance |
| `Steeltoe.Discovery.Eureka.EurekaApplicationInfoManager.InstanceConfig` | Property | Steeltoe.Discovery.Eureka | Removed | `IOptionsMonitor<EurekaInstanceOptions>.CurrentValue` | Refactored to use ASP.NET Options pattern |
| `Steeltoe.Discovery.Eureka.EurekaApplicationInfoManager.UpdateInstance` | Method | Steeltoe.Discovery.Eureka | Added | | Enables to change local instance from code (synced with configuration) |
| `Steeltoe.Discovery.Eureka.EurekaClientConfig` | Type | Steeltoe.Discovery.Eureka | Removed | `Steeltoe.Discovery.Eureka.Configuration.EurekaClientOptions` | |
| `Steeltoe.Discovery.Eureka.EurekaClientConfig.EurekaServerConnectTimeoutSeconds` | Property | Steeltoe.Discovery.Eureka | Removed | `EurekaClientOptions.Server.ConnectTimeoutSeconds` | |
| `Steeltoe.Discovery.Eureka.EurekaClientConfig.EurekaServerRetryCount` | Property | Steeltoe.Discovery.Eureka | Removed | `EurekaClientOptions.Server.RetryCount` | Default changed from 3 attempts to 2 retries (bug fix) |
| `Steeltoe.Discovery.Eureka.EurekaClientConfig.EurekaServerServiceUrls` | Property | Steeltoe.Discovery.Eureka | Removed | `EurekaClientOptions.EurekaServerServiceUrls` | Now supports comma-separated list of URLs |
| `Steeltoe.Discovery.Eureka.EurekaClientConfig.HealthCheckEnabled` | Property | Steeltoe.Discovery.Eureka | Removed | `EurekaClientOptions.Health.CheckEnabled` | |
| `Steeltoe.Discovery.Eureka.EurekaClientConfig.HealthContribEnabled` | Property | Steeltoe.Discovery.Eureka | Removed | `EurekaClientOptions.Health.ContributorEnabled` | |
| `Steeltoe.Discovery.Eureka.EurekaClientConfig.HealthMonitoredApps` | Property | Steeltoe.Discovery.Eureka | Removed | `EurekaClientOptions.Health.MonitoredApps` | |
| `Steeltoe.Discovery.Eureka.EurekaClientConfig.ProxyHost` | Property | Steeltoe.Discovery.Eureka | Removed | `EurekaClientOptions.Server.ProxyHost` | |
| `Steeltoe.Discovery.Eureka.EurekaClientConfig.ProxyPassword` | Property | Steeltoe.Discovery.Eureka | Removed | `EurekaClientOptions.Server.ProxyPassword` | |
| `Steeltoe.Discovery.Eureka.EurekaClientConfig.ProxyPort` | Property | Steeltoe.Discovery.Eureka | Removed | `EurekaClientOptions.Server.ProxyPort` | |
| `Steeltoe.Discovery.Eureka.EurekaClientConfig.ProxyUserName` | Property | Steeltoe.Discovery.Eureka | Removed | `EurekaClientOptions.Server.ProxyUserName` | |
| `Steeltoe.Discovery.Eureka.EurekaClientConfig.ShouldDisableDelta` | Property | Steeltoe.Discovery.Eureka | Moved | `EurekaClientOptions.IsFetchDeltaDisabled` | |
| `Steeltoe.Discovery.Eureka.EurekaClientConfig.ShouldGZipContent` | Property | Steeltoe.Discovery.Eureka | Removed | `EurekaClientOptions.Server.ShouldGZipContent` | |
| `Steeltoe.Discovery.Eureka.EurekaClientConfig.ShouldOnDemandUpdateStatusChange` | Property | Steeltoe.Discovery.Eureka | Removed | None | Always sends batch of local updates to Eureka server immediately |
| `Steeltoe.Discovery.Eureka.EurekaClientOptions` | Type | Steeltoe.Discovery.Eureka | Moved | `Steeltoe.Discovery.Eureka.Configuration.EurekaClientOptions` | |
| `Steeltoe.Discovery.Eureka.EurekaClientOptions.CacheTTL` | Property | Steeltoe.Discovery.Eureka | Removed | Use caching provided by `ServiceInstancesResolver` | |
| `Steeltoe.Discovery.Eureka.EurekaClientOptions.EurekaHealthConfig` | Type | Steeltoe.Discovery.Eureka | Moved | `Steeltoe.Discovery.Eureka.Configuration.EurekaHealthOptions` | |
| `Steeltoe.Discovery.Eureka.EurekaClientOptions.EurekaHealthConfig.Enabled` | Property | Steeltoe.Discovery.Eureka | Renamed | `EurekaHealthOptions.ContributorEnabled` | |
| `Steeltoe.Discovery.Eureka.EurekaClientOptions.EurekaServerConfig` | Type | Steeltoe.Discovery.Eureka | Moved | `Steeltoe.Discovery.Eureka.Configuration.EurekaServerOptions` | |
| `Steeltoe.Discovery.Eureka.EurekaClientOptions.ServiceUrl` | Property | Steeltoe.Discovery.Eureka | Renamed | `EurekaClientOptions.EurekaServerServiceUrls` | |
| `Steeltoe.Discovery.Eureka.EurekaClientOptions.Validate_Certificates` | Property | Steeltoe.Discovery.Eureka | Renamed | `EurekaClientOptions.ValidateCertificates` | |
| `Steeltoe.Discovery.Eureka.EurekaClientService` | Type | Steeltoe.Discovery.Eureka | Removed | `Steeltoe.Discovery.Eureka.EurekaDiscoveryClient` | |
| `Steeltoe.Discovery.Eureka.EurekaDiscoveryClient.ApplicationsFetched` | Event | Steeltoe.Discovery.Eureka | Added | | Resulting set (with delta applied), invoked from ThreadPool thread |
| `Steeltoe.Discovery.Eureka.EurekaDiscoveryClient.ClientConfig` | Property | Steeltoe.Discovery.Eureka | Removed | `IOptionsMonitor<EurekaClientOptions>.CurrentValue` | Refactored to use ASP.NET Options pattern |
| `Steeltoe.Discovery.Eureka.EurekaDiscoveryClient.EnsureAssemblyIsLoaded` | Method | Steeltoe.Discovery.Eureka | Removed | None | No longer needed |
| `Steeltoe.Discovery.Eureka.EurekaDiscoveryClient.GetInstances` | Method | Steeltoe.Discovery.Eureka | Renamed | `EurekaDiscoveryClient.GetInstancesAsync` | |
| `Steeltoe.Discovery.Eureka.EurekaDiscoveryClient.GetServices` | Method | Steeltoe.Discovery.Eureka | Renamed | `EurekaDiscoveryClient.GetServiceIdsAsync` | |
| `Steeltoe.Discovery.Eureka.EurekaDiscoveryClient.Services` | Property | Steeltoe.Discovery.Eureka | Removed | `EurekaDiscoveryClient.GetServiceIdsAsync` | |
| `Steeltoe.Discovery.Eureka.EurekaDiscoveryClientBuilderExtension` | Type | Steeltoe.Discovery.Eureka | Removed | Call `IServiceCollection.AddEurekaDiscoveryClient()` extension method | |
| `Steeltoe.Discovery.Eureka.EurekaDiscoveryClientExtension` | Type | Steeltoe.Discovery.Eureka | Removed | Call `IServiceCollection.AddEurekaDiscoveryClient()` extension method | |
| `Steeltoe.Discovery.Eureka.EurekaDiscoveryManager` | Type | Steeltoe.Discovery.Eureka | Removed | `Steeltoe.Discovery.Eureka.EurekaDiscoveryClient` | |
| `Steeltoe.Discovery.Eureka.EurekaHealthCheckHandler` | Type | Steeltoe.Discovery.Eureka | Removed | Configure `EurekaClientOptions.Health.CheckEnabled` | Made internal |
| `Steeltoe.Discovery.Eureka.EurekaInstanceConfig` | Type | Steeltoe.Discovery.Eureka | Removed | `Steeltoe.Discovery.Eureka.Configuration.EurekaInstanceOptions` | |
| `Steeltoe.Discovery.Eureka.EurekaInstanceConfig.ApplyNetUtils` | Method | Steeltoe.Discovery.Eureka | Removed | None | Refactored to internal logic, happens automatically based on settings |
| `Steeltoe.Discovery.Eureka.EurekaInstanceConfig.ASGName` | Property | Steeltoe.Discovery.Eureka | Removed | `EurekaInstanceOptions.AutoScalingGroupName` | |
| `Steeltoe.Discovery.Eureka.EurekaInstanceConfig.DefaultAddressResolutionOrder` | Property | Steeltoe.Discovery.Eureka | Removed | None | Property was never used |
| `Steeltoe.Discovery.Eureka.EurekaInstanceConfig.NetUtils` | Property | Steeltoe.Discovery.Eureka | Removed | None | OS-based network APIs are no longer pluggable |
| `Steeltoe.Discovery.Eureka.EurekaInstanceConfig.PreferIpAddress` | Property | Steeltoe.Discovery.Eureka | Moved | `EurekaInstanceOptions.PreferIPAddress` | |
| `Steeltoe.Discovery.Eureka.EurekaInstanceConfig.SecurePortEnabled` | Property | Steeltoe.Discovery.Eureka | Moved | `EurekaInstanceOptions.IsSecurePortEnabled` | |
| `Steeltoe.Discovery.Eureka.EurekaInstanceConfig.SecureVirtualHostName` | Property | Steeltoe.Discovery.Eureka | Moved | `EurekaInstanceOptions.SecureVipAddress` | |
| `Steeltoe.Discovery.Eureka.EurekaInstanceConfig.UseNetUtils` | Property | Steeltoe.Discovery.Eureka | Moved | `EurekaInstanceOptions.UseNetworkInterfaces` | |
| `Steeltoe.Discovery.Eureka.EurekaInstanceConfig.VirtualHostName` | Property | Steeltoe.Discovery.Eureka | Moved | `EurekaInstanceOptions.VipAddress` | |
| `Steeltoe.Discovery.Eureka.EurekaInstanceOptions` | Type | Steeltoe.Discovery.Eureka | Moved | `Steeltoe.Discovery.Eureka.Configuration.EurekaInstanceOptions` | |
| `Steeltoe.Discovery.Eureka.EurekaInstanceOptions.AppGroup` | Property | Steeltoe.Discovery.Eureka | Renamed | `EurekaInstanceOptions.AppGroupName` | |
| `Steeltoe.Discovery.Eureka.EurekaInstanceOptions.ApplyConfigUrls` | Method | Steeltoe.Discovery.Eureka | Removed | None | Refactored to internal logic, happens automatically based on settings |
| `Steeltoe.Discovery.Eureka.EurekaInstanceOptions.GetHostName` | Method | Steeltoe.Discovery.Eureka | Removed | None | Refactored to internal handling |
| `Steeltoe.Discovery.Eureka.EurekaInstanceOptions.InstanceEnabledOnInit` | Property | Steeltoe.Discovery.Eureka | Renamed | `EurekaInstanceOptions.IsInstanceEnabledOnInit` | |
| `Steeltoe.Discovery.Eureka.EurekaInstanceOptions.IpAddress` | Property | Steeltoe.Discovery.Eureka | Renamed | `EurekaInstanceOptions.IPAddress` | |
| `Steeltoe.Discovery.Eureka.EurekaInstanceOptions.NonSecurePortEnabled` | Property | Steeltoe.Discovery.Eureka | Renamed | `EurekaInstanceOptions.IsNonSecurePortEnabled` | |
| `Steeltoe.Discovery.Eureka.EurekaInstanceOptions.Port` | Property | Steeltoe.Discovery.Eureka | Renamed | `EurekaInstanceOptions.NonSecurePort` | |
| `Steeltoe.Discovery.Eureka.EurekaPostConfigurer` | Type | Steeltoe.Discovery.Eureka | Removed | None | Refactored to internal type `PostConfigureEurekaInstanceOptions` |
| `Steeltoe.Discovery.Eureka.EurekaServerHealthContributor` | Type | Steeltoe.Discovery.Eureka | Removed | Configure `EurekaClientOptions.Health.ContributorEnabled` | Made internal |
| `Steeltoe.Discovery.Eureka.EurekaServiceCollectionExtensions.AddEurekaDiscoveryClient` | Extension method | Steeltoe.Discovery.Eureka | Added | | Activates the Eureka discovery client |
| `Steeltoe.Discovery.Eureka.EurekaServiceInstance` | Type | Steeltoe.Discovery.Eureka | Removed | `IServiceInstance` | Made internal, implementation detail |
| `Steeltoe.Discovery.Eureka.EurekaServiceUriStateManager` | Type | Steeltoe.Discovery.Eureka | Added | | Load-balances over multiple Eureka servers |
| `Steeltoe.Discovery.Eureka.HealthCheckHandlerProvider` | Type | Steeltoe.Discovery.Eureka | Added | | Run contributors and ASP.NET health checks to determine local instance status |
| `Steeltoe.Discovery.Eureka.IEurekaClient` | Type | Steeltoe.Discovery.Eureka | Removed | `Steeltoe.Discovery.Eureka.EurekaDiscoveryClient` | |
| `Steeltoe.Discovery.Eureka.IEurekaClientConfig` | Type | Steeltoe.Discovery.Eureka | Removed | `Steeltoe.Discovery.Eureka.Configuration.EurekaClientOptions` | |
| `Steeltoe.Discovery.Eureka.IEurekaInstanceConfig` | Type | Steeltoe.Discovery.Eureka | Removed | `Steeltoe.Discovery.Eureka.Configuration.EurekaInstanceOptions` | |
| `Steeltoe.Discovery.Eureka.ILookupService` | Type | Steeltoe.Discovery.Eureka | Removed | `Steeltoe.Discovery.Eureka.EurekaDiscoveryClient` | |
| `Steeltoe.Discovery.Eureka.ScopedEurekaHealthCheckHandler` | Type | Steeltoe.Discovery.Eureka | Removed | `Steeltoe.Discovery.Eureka.IHealthCheckHandler` | Moved to internal type `EurekaHealthCheckHandler` |
| `Steeltoe.Discovery.Eureka.StatusChangedArgs` | Type | Steeltoe.Discovery.Eureka | Removed | `EurekaApplicationInfoManager.UpdateInstance()` | |
| `Steeltoe.Discovery.Eureka.StatusChangedHandler` | Type | Steeltoe.Discovery.Eureka | Removed | `EurekaApplicationInfoManager.UpdateInstance()` | Refactored to internal `EurekaApplicationInfoManager.InstanceChanged` event |
| `Steeltoe.Discovery.Eureka.ThisServiceInstance` | Type | Steeltoe.Discovery.Eureka | Removed | None | No longer needed |
| `Steeltoe.Discovery.Eureka.Transport.EurekaHttpClient` | Type | Steeltoe.Discovery.Eureka | Moved | `Steeltoe.Discovery.Eureka.EurekaClient` | |
| `Steeltoe.Discovery.Eureka.Transport.EurekaHttpClient.DeleteStatusOverrideAsync` | Method | Steeltoe.Discovery.Eureka | Removed | None | Not needed for service discovery |
| `Steeltoe.Discovery.Eureka.Transport.EurekaHttpClient.GetApplicationAsync` | Method | Steeltoe.Discovery.Eureka | Removed | None | Not needed for service discovery |
| `Steeltoe.Discovery.Eureka.Transport.EurekaHttpClient.GetInstanceAsync` | Method | Steeltoe.Discovery.Eureka | Removed | None | Not needed for service discovery |
| `Steeltoe.Discovery.Eureka.Transport.EurekaHttpClient.GetSecureVipAsync` | Method | Steeltoe.Discovery.Eureka | Removed | `EurekaClient.GetByVipAsync` | Identical implementation |
| `Steeltoe.Discovery.Eureka.Transport.EurekaHttpClient.GetVipAsync` | Method | Steeltoe.Discovery.Eureka | Renamed | `EurekaClient.GetByVipAsync` | |
| `Steeltoe.Discovery.Eureka.Transport.EurekaHttpClient.SendHeartBeatAsync` | Method | Steeltoe.Discovery.Eureka | Renamed | `EurekaClient.HeartbeatAsync` | |
| `Steeltoe.Discovery.Eureka.Transport.EurekaHttpClient.Shutdown` | Method | Steeltoe.Discovery.Eureka | Renamed | `EurekaClient.DeregisterAsync` | |
| `Steeltoe.Discovery.Eureka.Transport.EurekaHttpClient.StatusUpdateAsync` | Method | Steeltoe.Discovery.Eureka | Removed | None | Not needed for service discovery |
| `Steeltoe.Discovery.Eureka.Transport.EurekaHttpResponse` | Type | Steeltoe.Discovery.Eureka | Removed | None | No longer needed |
| `Steeltoe.Discovery.Eureka.Transport.EurekaHttpResponse<T>` | Type | Steeltoe.Discovery.Eureka | Removed | None | No longer needed |
| `Steeltoe.Discovery.Eureka.Transport.IEurekaHttpClient` | Type | Steeltoe.Discovery.Eureka | Removed | `Steeltoe.Discovery.Eureka.EurekaClient` | |
| `Steeltoe.Discovery.Eureka.Util.DateTimeConversions` | Type | Steeltoe.Discovery.Eureka | Removed | None | Made internal |

### Notable PRs

- https://github.com/SteeltoeOSS/Steeltoe/pull/1372
- https://github.com/SteeltoeOSS/Steeltoe/pull/1350
- https://github.com/SteeltoeOSS/Steeltoe/pull/1308
- https://github.com/SteeltoeOSS/Steeltoe/pull/1301
- https://github.com/SteeltoeOSS/Steeltoe/pull/1300
- https://github.com/SteeltoeOSS/Steeltoe/pull/1299
- https://github.com/SteeltoeOSS/Steeltoe/pull/1292
- https://github.com/SteeltoeOSS/Steeltoe/pull/1280
- https://github.com/SteeltoeOSS/Steeltoe/pull/1247
- https://github.com/SteeltoeOSS/Steeltoe/pull/1167

### Documentation

For additional information, see the updated [Discovery documentation](../discovery/index.md) and
[Discovery samples](https://github.com/SteeltoeOSS/Samples/tree/main/Discovery).

## Logging

### Behavior changes

- Simplified API: `builder.Logging.AddDynamicSerilog()`
- Monitors and adapts to changes in `IConfiguration` (restores to changed minimum level after reset)
- Optimized for performance: faster updates, uses less memory
- Improved reliability: no more stale reads, race conditions and lost updates
- Fix default minimum log level to be Information instead of None
- Fix category name comparisons to be case-sensitive
- Fix namespace matching in categories: "Ab" is not a descendant of "A"
- Fix broken reset of levels when changed earlier (should revert to configured instead of default)
- Fix mismatches between the levels returned vs. the levels truly active in `ILoggers`
- Fix crash with latest version of Spring Boot Admin
- Fixed: console-specific rules should win over global rules in `appsettings.json`
- Serilog: fix crash when no default category is configured
- Serilog: Configured overrides were ignored when no default level was configured

### NuGet Package changes

| Source | Kind | Package | Change | Replacement | Notes |
| --- | --- | --- | --- | --- | --- |
| Steeltoe.Extensions.Logging.Abstractions | Package | | Renamed | Steeltoe.Logging.Abstractions | |
| Steeltoe.Extensions.Logging.DynamicLogger | Package | | Renamed | Steeltoe.Logging.DynamicConsole | |
| Steeltoe.Extensions.Logging.DynamicSerilogBase | Package | | Renamed | Steeltoe.Logging.DynamicSerilog | |
| Steeltoe.Extensions.Logging.DynamicSerilogCore | Package | | Renamed | Steeltoe.Logging.DynamicSerilog | |

### API changes

| Source | Kind | Package | Change | Replacement | Notes |
| --- | --- | --- | --- | --- | --- |
| `Steeltoe.Extensions.Logging.DynamicLoggerConfiguration` | Type | Steeltoe.Extensions.Logging.Abstractions | Renamed | `Steeltoe.Logging.DynamicLoggerState` | Represents logger state, is unrelated to `IConfiguration` |
| `Steeltoe.Extensions.Logging.DynamicLoggerConfiguration.ConfiguredLevel` | Property | Steeltoe.Extensions.Logging.Abstractions | Renamed | `DynamicLoggerState.BackupMinLevel` | The minimum level before override |
| `Steeltoe.Extensions.Logging.DynamicLoggerConfiguration.EffectiveLevel` | Property | Steeltoe.Extensions.Logging.Abstractions | Renamed | `DynamicLoggerState.EffectiveMinLevel` | The active minimum level, taking overrides into account |
| `Steeltoe.Extensions.Logging.DynamicLoggerConfiguration.Name` | Property | Steeltoe.Extensions.Logging.Abstractions | Renamed | `DynamicLoggerState.CategoryName` | |
| `Steeltoe.Extensions.Logging.DynamicLoggerProviderBase` | Type | Steeltoe.Extensions.Logging.Abstractions | Renamed | `Steeltoe.Logging.DynamicLoggerProvider` | |
| `Steeltoe.Extensions.Logging.DynamicLoggerProviderBase.GetLoggerConfigurations` | Method | Steeltoe.Extensions.Logging.Abstractions | Renamed | `DynamicLoggerProvider.GetLogLevels` | |
| `Steeltoe.Extensions.Logging.IDynamicLoggerProvider.GetLoggerConfigurations` | Method | Steeltoe.Extensions.Logging.Abstractions | Renamed | `IDynamicLoggerProvider.GetLogLevels` | |
| `Steeltoe.Extensions.Logging.ILoggerConfiguration` | Type | Steeltoe.Extensions.Logging.Abstractions | Removed | `Steeltoe.Logging.DynamicLoggerState` | |
| `Steeltoe.Extensions.Logging.InitialLevels` | Type | Steeltoe.Extensions.Logging.Abstractions | Renamed | `Steeltoe.Logging.LogLevelsConfiguration` | |
| `Steeltoe.Extensions.Logging.MessageProcessingLogger.Delegate` | Property | Steeltoe.Extensions.Logging.Abstractions | Renamed | `MessageProcessingLogger.InnerLogger` | |
| `Steeltoe.Extensions.Logging.MessageProcessingLogger.Filter` | Property | Steeltoe.Extensions.Logging.Abstractions | Removed | None | Refactored to handle internally |
| `Steeltoe.Extensions.Logging.MessageProcessingLogger.Name` | Property | Steeltoe.Extensions.Logging.Abstractions | Removed | None | No longer needed |
| `Steeltoe.Extensions.Logging.MessageProcessingLogger.WriteMessage` | Method | Steeltoe.Extensions.Logging.Abstractions | Removed | None | Refactored to handle internally |
| `Steeltoe.Extensions.Logging.StructuredMessageProcessingLogger` | Type | Steeltoe.Extensions.Logging.Abstractions | Removed | `Steeltoe.Logging.MessageProcessingLogger` | No longer needed |
| `Steeltoe.Logging.DynamicLoggerProvider.CreateMessageProcessingLogger` | Method | Steeltoe.Logging.Abstractions | Added | | Provides creation of `MessageProcessingLogger` in derived types |
| `Steeltoe.Logging.DynamicLoggerProvider.GetFilter` | Method | Steeltoe.Logging.Abstractions | Added | | Provides access to filter callback in derived types |
| `Steeltoe.Logging.DynamicLoggerProvider.InnerLoggerProvider` | Property | Steeltoe.Logging.Abstractions | Added | | Provides access to wrapped ILogger in derived types |
| `Steeltoe.Logging.DynamicLoggerProvider.MessageProcessors` | Property | Steeltoe.Logging.Abstractions | Added | | Provides access to processors in derived types |
| `Steeltoe.Logging.DynamicLoggerProvider.RefreshConfiguration` | Method | Steeltoe.Logging.Abstractions | Added | | Applies changes from `IConfiguration` |
| `Steeltoe.Logging.IDynamicLoggerProvider.RefreshConfiguration` | Method | Steeltoe.Logging.Abstractions | Added | | Applies changes from `IConfiguration` |
| `Steeltoe.Logging.LoggerFilter` | Type | Steeltoe.Logging.Abstractions | Added | | Delegate to simplify signatures |
| `Steeltoe.Logging.MessageProcessingLogger.ChangeFilter` | Method | Steeltoe.Logging.Abstractions | Added | | Called by `DynamicLoggerProvider` when effective level changes |
| `Steeltoe.Logging.MessageProcessingLogger.MessageProcessors` | Property | Steeltoe.Logging.Abstractions | Added | | Provides access to processors in derived types |
| `Steeltoe.Extensions.Logging.DynamicConsoleLoggerProvider` | Type | Steeltoe.Extensions.Logging.DynamicLogger | Moved | `Steeltoe.Logging.DynamicConsole.DynamicConsoleLoggerProvider` | |
| `Steeltoe.Extensions.Logging.DynamicLoggerHostBuilderExtensions.AddDynamicLogging` | Extension method | Steeltoe.Extensions.Logging.DynamicLogger | Removed | `LoggingBuilderExtensions.AddDynamicConsole` | |
| `Steeltoe.Extensions.Logging.DynamicLoggingBuilder.AddDynamicConsole` | Extension method | Steeltoe.Extensions.Logging.DynamicLogger | Moved | `LoggingBuilderExtensions.AddDynamicConsole` | |
| `Steeltoe.Extensions.Logging.DynamicSerilog.ISerilogOptions` | Type | Steeltoe.Extensions.Logging.DynamicSerilog [Base/Core] | Removed | `Steeltoe.Logging.DynamicSerilog.SerilogOptions` | Redundant, there's only one Serilog product |
| `Steeltoe.Extensions.Logging.DynamicSerilog.SerilogDynamicLoggerFactory` | Type | Steeltoe.Extensions.Logging.DynamicSerilog [Base/Core] | Removed | `DynamicSerilogLoggerProvider` | No longer needed |
| `Steeltoe.Extensions.Logging.DynamicSerilog.SerilogDynamicProvider` | Type | Steeltoe.Extensions.Logging.DynamicSerilog [Base/Core] | Renamed | `DynamicSerilogLoggerProvider` | |
| `Steeltoe.Extensions.Logging.DynamicSerilog.SerilogHostBuilderExtensions.AddDynamicSerilog` | Extension method | Steeltoe.Extensions.Logging.DynamicSerilog [Base/Core] | Removed | `ILoggingBuilder.AddDynamicSerilog()` | |
| `Steeltoe.Extensions.Logging.DynamicSerilog.SerilogHostBuilderExtensions.UseSerilogDynamicConsole` | Extension method | Steeltoe.Extensions.Logging.DynamicSerilog [Base/Core] | Removed | `ILoggingBuilder.AddDynamicSerilog()` | |
| `Steeltoe.Extensions.Logging.DynamicSerilog.SerilogLoggingBuilderExtensions.AddSerilogDynamicConsole` | Extension method | Steeltoe.Extensions.Logging.DynamicSerilog [Base/Core] | Removed | `ILoggingBuilder.AddDynamicSerilog()` | |
| `Steeltoe.Extensions.Logging.DynamicSerilog.SerilogOptions.ConfigPath` | Property | Steeltoe.Extensions.Logging.DynamicSerilog [Base/Core] | Removed | None | No longer needed |
| `Steeltoe.Extensions.Logging.DynamicSerilog.SerilogOptions.FullnameExclusions` | Property | Steeltoe.Extensions.Logging.DynamicSerilog [Base/Core] | Removed | None | No longer needed |
| `Steeltoe.Extensions.Logging.DynamicSerilog.SerilogOptions.SubloggerConfigKeyExclusions` | Property | Steeltoe.Extensions.Logging.DynamicSerilog [Base/Core] | Removed | None | No longer needed |
| `Steeltoe.Extensions.Logging.DynamicSerilog.SerilogWebApplicationBuilderExtensions.AddDynamicSerilog` | Extension method | Steeltoe.Extensions.Logging.DynamicSerilog [Base/Core] | Removed | `ILoggingBuilder.AddDynamicSerilog()` | |
| `Steeltoe.Extensions.Logging.DynamicSerilog.SerilogWebHostBuilderExtensions.AddDynamicSerilog` | Extension method | Steeltoe.Extensions.Logging.DynamicSerilog [Base/Core] | Removed | `ILoggingBuilder.AddDynamicSerilog()` | |
| `Steeltoe.Extensions.Logging.DynamicSerilog.SerilogWebHostBuilderExtensions.UseSerilogDynamicConsole` | Extension method | Steeltoe.Extensions.Logging.DynamicSerilog [Base/Core] | Removed | `ILoggingBuilder.AddDynamicSerilog()` | |
| `Steeltoe.Logging.DynamicSerilog.SerilogMessageProcessingLogger` | Type | Steeltoe.Logging.DynamicSerilog | Added | | Preserve structured logs with `IDynamicMessageProcessor` in Serilog |

### Notable PRs

- https://github.com/SteeltoeOSS/Steeltoe/pull/1468
- https://github.com/SteeltoeOSS/Steeltoe/pull/1403
- https://github.com/SteeltoeOSS/Steeltoe/pull/1216
- https://github.com/SteeltoeOSS/Steeltoe/pull/1024
- https://github.com/SteeltoeOSS/Steeltoe/pull/1064
- https://github.com/SteeltoeOSS/Steeltoe/pull/1038
- https://github.com/SteeltoeOSS/Steeltoe/pull/991

### Documentation

For additional information, see the updated [Logging documentation](../logging/index.md).

## Management

### Behavior changes

- Unified configuration for `/actuator` and `/cloudfoundryapplication`
- Reduced required code for using an actuator to a single `IServiceCollection` extension method (configures middleware by default)
- Cleaner extensibility model for third-party actuators
- Fail at startup when actuators are used on Cloud Foundry without security
- Improved security and redaction of sensitive data in responses and logs
- Actuators can be turned on/off or bound to different verbs at runtime using configuration
- Simplified content negotiation; updated all actuators to support latest Spring media type
- New actuator `/beans` that lists the contents of the .NET dependency container, including support for keyed services
- Update health checks and actuator to align with latest Spring; hide details by default; contributors can be turned on/off at runtime using configuration
- Support Windows network shares in disk space health contributor
- Update `/mappings` actuator to include endpoints from Minimal APIs, Razor Pages, and Blazor, with richer metadata and improved compatibility with Spring
- Heap dumps are enabled by default in Cloud Foundry on Linux; all dump types supported on Windows/Linux/macOS
- Improved Prometheus exporter that works with latest OpenTelemetry
- Various fixes for interoperability with latest Spring Boot Admin; more flexible configuration, uses smarter defaults
- Unified `/traces` and `/httptraces` actuators to `/httpexchanges`, to align with latest Spring
- WaveFront, Zipkin, and Jaeger support was removed (use OpenTelemetry directly)
- Metrics endpoint was removed (use OpenTelemetry directly)
- Kubernetes actuator was removed

### NuGet Package changes

| Source | Change | Replacement | Notes |
| --- | --- | --- | --- |
| Steeltoe.Management.CloudFoundryCore | Moved | Steeltoe.Management.Endpoint | Contained the Cloud Foundry actuator |
| Steeltoe.Management.Diagnostics | Moved | Steeltoe.Management.Endpoint | Contained code for taking heap dumps |
| Steeltoe.Management.EndpointBase | Renamed | Steeltoe.Management.Endpoint | |
| Steeltoe.Management.EndpointCore | Renamed | Steeltoe.Management.Endpoint | |
| Steeltoe.Management.KubernetesCore | Removed | None | |
| Steeltoe.Management.OpenTelemetryBase | Removed | Steeltoe.Management.Prometheus | WaveFront exporter was removed |
| Steeltoe.Management.Prometheus | Added | | Provides the Prometheus actuator |
| Steeltoe.Management.TaskCore | Renamed | Steeltoe.Management.Tasks | |
| Steeltoe.Management.TracingBase | Renamed | Steeltoe.Management.Tracing | |
| Steeltoe.Management.TracingCore | Renamed | Steeltoe.Management.Tracing | |

### API changes

| Source | Kind | Package | Change | Replacement | Notes |
| --- | --- | --- | --- | --- | --- |
| `Microsoft.Diagnostics.Runtime.Interop.IMAGE_FILE_MACHINE` | Type | Steeltoe.Management.Diagnostics | Removed | None | |
| `Microsoft.Extensions.DependencyInjection.ServiceCollectionExtensions.AddActuatorEndpointMapping<TEndpoint>` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Removed | `Steeltoe.Management.Endpoint.ApplicationBuilderExtensions.UseActuatorEndpoints` | Needed only when setting up middleware manually |
| `Microsoft.Extensions.DependencyInjection.ServiceCollectionExtensions.AddCloudFoundryActuatorServices` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Moved | `builder.Services.AddCloudFoundryActuator()` | |
| `Microsoft.Extensions.DependencyInjection.ServiceCollectionExtensions.AddDbMigrationsActuatorServices` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Moved | `builder.Services.AddDbMigrationsActuator()` | |
| `Microsoft.Extensions.DependencyInjection.ServiceCollectionExtensions.AddEnvActuatorServices` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Moved | `builder.Services.AddEnvironmentActuator()` | |
| `Microsoft.Extensions.DependencyInjection.ServiceCollectionExtensions.AddHealthActuatorServices` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Moved | `builder.Services.AddHealthActuator()` with `.AddHealthContributor()` | |
| `Microsoft.Extensions.DependencyInjection.ServiceCollectionExtensions.AddHeapDumpActuatorServices` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Moved | `builder.Services.AddHeapDumpActuator()` | |
| `Microsoft.Extensions.DependencyInjection.ServiceCollectionExtensions.AddHypermediaActuatorServices` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Moved | `builder.Services.AddHypermediaActuator()` | |
| `Microsoft.Extensions.DependencyInjection.ServiceCollectionExtensions.AddInfoActuatorServices` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Moved | `builder.Services.AddInfoActuator()` with `.AddInfoContributor()` | |
| `Microsoft.Extensions.DependencyInjection.ServiceCollectionExtensions.AddLoggersActuatorServices` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Moved | `builder.Services.AddLoggersActuator()` | |
| `Microsoft.Extensions.DependencyInjection.ServiceCollectionExtensions.AddMappingsActuatorServices` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Moved | `builder.Services.AddRouteMappingsActuator()` | |
| `Microsoft.Extensions.DependencyInjection.ServiceCollectionExtensions.AddMetricsActuatorServices` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Metrics actuator has been removed |
| `Microsoft.Extensions.DependencyInjection.ServiceCollectionExtensions.AddOpenTelemetryMetricsForSteeltoe` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Removed | Use OpenTelemetry packages directly | |
| `Microsoft.Extensions.DependencyInjection.ServiceCollectionExtensions.AddPrometheusActuatorServices` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Moved | `AddPrometheusActuator` in Steeltoe.Management.Prometheus package | |
| `Microsoft.Extensions.DependencyInjection.ServiceCollectionExtensions.AddRefreshActuatorServices` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Moved | `builder.Services.AddRefreshActuator()` | |
| `Microsoft.Extensions.DependencyInjection.ServiceCollectionExtensions.AddSpringBootAdminClient` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Moved | `builder.Services.AddSpringBootAdminClient()` | |
| `Microsoft.Extensions.DependencyInjection.ServiceCollectionExtensions.AddThreadDumpActuatorServices` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Moved | `builder.Services.AddThreadDumpActuator()` | |
| `Microsoft.Extensions.DependencyInjection.ServiceCollectionExtensions.AddTraceActuatorServices` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Moved | `builder.Services.AddHttpExchangesActuator()` | |
| `Microsoft.Extensions.DependencyInjection.ServiceCollectionExtensions.ConfigureSteeltoeMetrics` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Removed | Use OpenTelemetry packages directly | |
| `Steeltoe.Common.Diagnostics.DiagnosticHelpers` | Type | Steeltoe.Management.Abstractions | Removed | None | No longer needed |
| `Steeltoe.Common.Diagnostics.DiagnosticObserver` | Type | Steeltoe.Management.Abstractions | Removed | None | Made internal, moved to Steeltoe.Management.Endpoint package |
| `Steeltoe.Common.Diagnostics.DiagnosticsManager` | Type | Steeltoe.Management.Abstractions | Removed | None | Made internal, moved to Steeltoe.Management.Endpoint package |
| `Steeltoe.Common.Diagnostics.IDiagnosticObserver` | Type | Steeltoe.Management.Abstractions | Removed | None | |
| `Steeltoe.Common.Diagnostics.IDiagnosticsManager` | Type | Steeltoe.Management.Abstractions | Removed | None | |
| `Steeltoe.Common.Diagnostics.IRuntimeDiagnosticSource` | Type | Steeltoe.Management.Abstractions | Removed | None | |
| `Steeltoe.Management.AbstractEndpoint` | Type | Steeltoe.Management.Abstractions | Moved | `IEndpointHandler<,>` in Steeltoe.Management.Endpoint package | |
| `Steeltoe.Management.AbstractEndpoint.Enabled` | Property | Steeltoe.Management.Abstractions | Removed | `EndpointOptions.Enabled` | |
| `Steeltoe.Management.AbstractEndpoint.Id` | Property | Steeltoe.Management.Abstractions | Removed | `EndpointOptions.Id` | |
| `Steeltoe.Management.AbstractEndpoint.Options` | Property | Steeltoe.Management.Abstractions | Removed | `EndpointMiddleware<,>.EndpointOptions` or inject `IOptionsMonitor` | |
| `Steeltoe.Management.AbstractEndpoint.options` | Field | Steeltoe.Management.Abstractions | Removed | `EndpointMiddleware<,>.EndpointOptions` or inject `IOptionsMonitor` | |
| `Steeltoe.Management.AbstractEndpoint.Path` | Property | Steeltoe.Management.Abstractions | Removed | `EndpointOptions.Path` | |
| `Steeltoe.Management.AbstractEndpoint<,>` | Type | Steeltoe.Management.Abstractions | Removed | `IEndpointHandler<,>` in Steeltoe.Management.Endpoint package | |
| `Steeltoe.Management.AbstractEndpoint<,>.Invoke` | Method | Steeltoe.Management.Abstractions | Removed | `IEndpointHandler<,>.InvokeAsync` | |
| `Steeltoe.Management.AbstractEndpoint<>` | Type | Steeltoe.Management.Abstractions | Removed | `IEndpointHandler<,>` in Steeltoe.Management.Endpoint package | |
| `Steeltoe.Management.AbstractEndpoint<>.Invoke` | Method | Steeltoe.Management.Abstractions | Removed | `IEndpointHandler<,>.InvokeAsync` | |
| `Steeltoe.Management.AbstractEndpointOptions` | Type | Steeltoe.Management.Abstractions | Renamed | `Steeltoe.Management.Configuration.EndpointOptions` | |
| `Steeltoe.Management.AbstractEndpointOptions._enabled` | Field | Steeltoe.Management.Abstractions | Removed | `EndpointOptions.Enabled` | |
| `Steeltoe.Management.AbstractEndpointOptions._path` | Field | Steeltoe.Management.Abstractions | Removed | `EndpointOptions.Path` | |
| `Steeltoe.Management.AbstractEndpointOptions._sensitive` | Field | Steeltoe.Management.Abstractions | Removed | None | Was never used |
| `Steeltoe.Management.AbstractEndpointOptions.DefaultEnabled` | Property | Steeltoe.Management.Abstractions | Removed | None, implicitly true | |
| `Steeltoe.Management.AbstractEndpointOptions.ExactMatch` | Property | Steeltoe.Management.Abstractions | Removed | `EndpointOptions.RequiresExactMatch()` | |
| `Steeltoe.Management.AbstractEndpointOptions.Global` | Property | Steeltoe.Management.Abstractions | Removed | Inject `IOptionsMonitor<ManagementOptions>` | |
| `Steeltoe.Management.AbstractEndpointOptions.IsAccessAllowed` | Method | Steeltoe.Management.Abstractions | Removed | `EndpointOptions.RequiredPermissions` | |
| `Steeltoe.Management.CloudFoundry.CloudFoundryActuatorsStartupFilter` | Type | Steeltoe.Management.CloudFoundryCore | Removed | None | |
| `Steeltoe.Management.CloudFoundry.CloudFoundryHostBuilderExtensions.AddCloudFoundryActuators` | Extension method | Steeltoe.Management.CloudFoundryCore | Moved | `builder.Services.AddCloudFoundryActuator()` in Steeltoe.Management.Endpoint package | |
| `Steeltoe.Management.CloudFoundry.CloudFoundryServiceCollectionExtensions.AddCloudFoundryActuators` | Extension method | Steeltoe.Management.CloudFoundryCore | Moved | `builder.Services.AddCloudFoundryActuator()` in Steeltoe.Management.Endpoint package | |
| `Steeltoe.Management.Configuration.EndpointOptions.GetDefaultAllowedVerbs` | Method | Steeltoe.Management.Abstractions | Added | | Override to initialize `AllowedVerbs` default |
| `Steeltoe.Management.Endpoint.ActuatorMediaTypes` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Each actuator uses built-in media type |
| `Steeltoe.Management.Endpoint.ActuatorRouteBuilderExtensions.LookupMiddleware` | Method | Steeltoe.Management.Endpoint [Base/Core] | Removed | Inject `IEnumerable<IEndpointMiddleware>` | |
| `Steeltoe.Management.Endpoint.ActuatorRouteBuilderExtensions.Map` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `MapActuators()` | |
| `Steeltoe.Management.Endpoint.ActuatorRouteBuilderExtensions.MapAllActuators` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `MapActuators()` | |
| `Steeltoe.Management.Endpoint.Actuators.CloudFoundry.ICloudFoundryEndpointHandler` | Type | Steeltoe.Management.Endpoint | Added | | Enables custom implementation to execute actuator |
| `Steeltoe.Management.Endpoint.Actuators.DbMigrations.IDbMigrationsEndpointHandler` | Type | Steeltoe.Management.Endpoint | Added | | Enables custom implementation to execute actuator |
| `Steeltoe.Management.Endpoint.Actuators.Environment.IEnvironmentEndpointHandler` | Type | Steeltoe.Management.Endpoint | Added | | Enables custom implementation to execute actuator |
| `Steeltoe.Management.Endpoint.Actuators.Health.Availability.ApplicationAvailability` | Type | Steeltoe.Management.Endpoint | Added | | Moved from Steeltoe.Common package |
| `Steeltoe.Management.Endpoint.Actuators.Health.Availability.AvailabilityEventArgs` | Type | Steeltoe.Management.Endpoint | Added | | Moved from Steeltoe.Common package |
| `Steeltoe.Management.Endpoint.Actuators.Health.Availability.AvailabilityState` | Type | Steeltoe.Management.Endpoint | Added | | Moved from Steeltoe.Common package |
| `Steeltoe.Management.Endpoint.Actuators.Health.Availability.LivenessState` | Type | Steeltoe.Management.Endpoint | Added | | Moved from Steeltoe.Common package |
| `Steeltoe.Management.Endpoint.Actuators.Health.Availability.LivenessStateContributorOptions.Enabled` | Property | Steeltoe.Management.Endpoint | Added | | Enables turning off via configuration |
| `Steeltoe.Management.Endpoint.Actuators.Health.Availability.ReadinessState` | Type | Steeltoe.Management.Endpoint | Added | | Moved from Steeltoe.Common package |
| `Steeltoe.Management.Endpoint.Actuators.Health.Availability.ReadinessStateContributorOptions.Enabled` | Property | Steeltoe.Management.Endpoint | Added | | Enables turning off via configuration |
| `Steeltoe.Management.Endpoint.Actuators.Health.Contributors.DiskSpaceContributorOptions.Enabled` | Property | Steeltoe.Management.Endpoint | Added | | Enables turning off via configuration |
| `Steeltoe.Management.Endpoint.Actuators.Health.HealthEndpointOptions.ShowComponents` | Property | Steeltoe.Management.Endpoint | Added | | Whether to show/hide contributors in response |
| `Steeltoe.Management.Endpoint.Actuators.Health.HealthEndpointRequest` | Type | Steeltoe.Management.Endpoint | Added | | Data about incoming actuator request |
| `Steeltoe.Management.Endpoint.Actuators.Health.HealthEndpointResponse.Components` | Property | Steeltoe.Management.Endpoint | Added | | List of health contributors in response |
| `Steeltoe.Management.Endpoint.Actuators.Health.HealthEndpointResponse.Exists` | Property | Steeltoe.Management.Endpoint | Added | | Used to indicate the request was invalid |
| `Steeltoe.Management.Endpoint.Actuators.Health.HealthGroupOptions.ShowComponents` | Property | Steeltoe.Management.Endpoint | Added | | Hide/show contributors in the group |
| `Steeltoe.Management.Endpoint.Actuators.Health.HealthGroupOptions.ShowDetails` | Property | Steeltoe.Management.Endpoint | Added | | Hide/show contributor details in the group |
| `Steeltoe.Management.Endpoint.Actuators.Health.IHealthEndpointHandler` | Type | Steeltoe.Management.Endpoint | Added | | Enables custom implementation to execute actuator |
| `Steeltoe.Management.Endpoint.Actuators.HeapDump.IHeapDumpEndpointHandler` | Type | Steeltoe.Management.Endpoint | Added | | Enables custom implementation to execute actuator |
| `Steeltoe.Management.Endpoint.Actuators.HttpExchanges.HttpExchangesEndpointOptions.RequestHeaders` | Property | Steeltoe.Management.Endpoint | Added | | Request header names to exclude from redaction |
| `Steeltoe.Management.Endpoint.Actuators.HttpExchanges.HttpExchangesEndpointOptions.ResponseHeaders` | Property | Steeltoe.Management.Endpoint | Added | | Response header names to exclude from redaction |
| `Steeltoe.Management.Endpoint.Actuators.HttpExchanges.HttpExchangesEndpointOptions.Reverse` | Property | Steeltoe.Management.Endpoint | Added | | Return most recent exchanges at the top |
| `Steeltoe.Management.Endpoint.Actuators.HttpExchanges.IHttpExchangesEndpointHandler` | Type | Steeltoe.Management.Endpoint | Added | | Enables custom implementation to execute actuator |
| `Steeltoe.Management.Endpoint.Actuators.Hypermedia.IHypermediaEndpointHandler` | Type | Steeltoe.Management.Endpoint | Added | | Enables custom implementation to execute actuator |
| `Steeltoe.Management.Endpoint.Actuators.Info.EndpointServiceCollectionExtensions.AddInfoContributor` | Extension method | Steeltoe.Management.Endpoint | Added | | Adds a contributor to info actuator |
| `Steeltoe.Management.Endpoint.Actuators.Info.IInfoContributor` | Type | Steeltoe.Management.Endpoint | Added | | Moved from Steeltoe.Management.Abstractions package |
| `Steeltoe.Management.Endpoint.Actuators.Info.IInfoEndpointHandler` | Type | Steeltoe.Management.Endpoint | Added | | Enables custom implementation to execute actuator |
| `Steeltoe.Management.Endpoint.Actuators.Info.InfoBuilder` | Type | Steeltoe.Management.Endpoint | Added | | Moved from Steeltoe.Management.Abstractions package |
| `Steeltoe.Management.Endpoint.Actuators.Loggers.ILoggersEndpointHandler` | Type | Steeltoe.Management.Endpoint | Added | | Enables custom implementation to execute actuator |
| `Steeltoe.Management.Endpoint.Actuators.Loggers.LoggerGroup` | Type | Steeltoe.Management.Endpoint | Added | | Group in `LoggersResponse` |
| `Steeltoe.Management.Endpoint.Actuators.Loggers.LoggersRequest.Type` | Property | Steeltoe.Management.Endpoint | Added | | Indicates actuator request type (get or change) |
| `Steeltoe.Management.Endpoint.Actuators.Loggers.LoggersRequestType` | Type | Steeltoe.Management.Endpoint | Added | | Indicates actuator request type (get or change) |
| `Steeltoe.Management.Endpoint.Actuators.Loggers.LoggersResponse` | Type | Steeltoe.Management.Endpoint | Added | | Data for outgoing actuator response |
| `Steeltoe.Management.Endpoint.Actuators.Refresh.IRefreshEndpointHandler` | Type | Steeltoe.Management.Endpoint | Added | | Enables custom implementation to execute actuator |
| `Steeltoe.Management.Endpoint.Actuators.RouteMappings.IRouteMappingsEndpointHandler` | Type | Steeltoe.Management.Endpoint | Added | | Enables custom implementation to execute actuator |
| `Steeltoe.Management.Endpoint.Actuators.RouteMappings.ResponseTypes.MediaTypeDescriptor` | Type | Steeltoe.Management.Endpoint | Added | | Used in response structure to mirror Spring |
| `Steeltoe.Management.Endpoint.Actuators.RouteMappings.ResponseTypes.ParameterDescriptor` | Type | Steeltoe.Management.Endpoint | Added | | Used in response structure to mirror Spring |
| `Steeltoe.Management.Endpoint.Actuators.RouteMappings.ResponseTypes.RouteConditionsDescriptor.Headers` | Property | Steeltoe.Management.Endpoint | Added | | Used in response structure to mirror Spring |
| `Steeltoe.Management.Endpoint.Actuators.RouteMappings.ResponseTypes.RouteConditionsDescriptor.Parameters` | Property | Steeltoe.Management.Endpoint | Added | | Used in response structure to mirror Spring |
| `Steeltoe.Management.Endpoint.Actuators.RouteMappings.ResponseTypes.RouteDetailsDescriptor` | Type | Steeltoe.Management.Endpoint | Added | | Used in response structure to mirror Spring |
| `Steeltoe.Management.Endpoint.Actuators.RouteMappings.ResponseTypes.RouteDispatcherServlets` | Type | Steeltoe.Management.Endpoint | Added | | Used in response structure to mirror Spring |
| `Steeltoe.Management.Endpoint.Actuators.RouteMappings.ResponseTypes.RouteHandlerDescriptor` | Type | Steeltoe.Management.Endpoint | Added | | Used in response structure to mirror Spring |
| `Steeltoe.Management.Endpoint.Actuators.RouteMappings.ResponseTypes.RouteMappingContexts` | Type | Steeltoe.Management.Endpoint | Added | | Used in response structure to mirror Spring |
| `Steeltoe.Management.Endpoint.Actuators.RouteMappings.ResponseTypes.RouteMappingsContainer` | Type | Steeltoe.Management.Endpoint | Added | | Used in response structure to mirror Spring |
| `Steeltoe.Management.Endpoint.Actuators.RouteMappings.RouteMappingsEndpointOptions.IncludeActuators` | Property | Steeltoe.Management.Endpoint | Added | | Whether actuator endpoints are included in the response |
| `Steeltoe.Management.Endpoint.Actuators.Services` | Namespace | Steeltoe.Management.Endpoint | Added | | Provides services actuator |
| `Steeltoe.Management.Endpoint.Actuators.Services.EndpointServiceCollectionExtensions.AddServicesActuator` | Extension method | Steeltoe.Management.Endpoint | Added | | Activates the services actuator |
| `Steeltoe.Management.Endpoint.Actuators.Services.IServicesEndpointHandler` | Type | Steeltoe.Management.Endpoint | Added | | Enables custom implementation to execute actuator |
| `Steeltoe.Management.Endpoint.Actuators.Services.ServiceRegistration` | Type | Steeltoe.Management.Endpoint | Added | | Describes an entry in the D/I container |
| `Steeltoe.Management.Endpoint.Actuators.Services.ServicesEndpointOptions` | Type | Steeltoe.Management.Endpoint | Added | | Configuration options for services actuator |
| `Steeltoe.Management.Endpoint.Actuators.ThreadDump.IThreadDumpEndpointHandler` | Type | Steeltoe.Management.Endpoint | Added | | Enables custom implementation to execute actuator |
| `Steeltoe.Management.Endpoint.ActuatorServiceCollectionExtensions.AddAllActuators` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Moved | `Steeltoe.Management.Endpoint.Actuators.All.EndpointServiceCollectionExtensions.AddAllActuators` | For custom CORS, call `services.ConfigureActuatorsCorsPolicy()` |
| `Steeltoe.Management.Endpoint.ActuatorServiceCollectionExtensions.RegisterEndpointOptions` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Redundant |
| `Steeltoe.Management.Endpoint.AllActuatorsStartupFilter` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Moved to internal type `ConfigureActuatorsMiddlewareStartupFilter` |
| `Steeltoe.Management.Endpoint.ApplicationBuilderExtensions.UseActuatorsCorsPolicy` | Extension method | Steeltoe.Management.Endpoint | Added | | Needed only when setting up middleware manually |
| `Steeltoe.Management.Endpoint.ApplicationBuilderExtensions.UseManagementPort` | Extension method | Steeltoe.Management.Endpoint | Added | | Needed only when setting up middleware manually |
| `Steeltoe.Management.Endpoint.CloudFoundry` | Namespace | Steeltoe.Management.Endpoint [Base/Core] | Moved | `Steeltoe.Management.Endpoint.Actuators.CloudFoundry` | |
| `Steeltoe.Management.Endpoint.CloudFoundry.CloudFoundryActuatorStartupFilter` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddCloudFoundryActuator()` | Redundant |
| `Steeltoe.Management.Endpoint.CloudFoundry.CloudFoundryEndpoint` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `ICloudFoundryEndpointHandler` | Moved to internal type `CloudFoundryEndpointHandler` |
| `Steeltoe.Management.Endpoint.CloudFoundry.CloudFoundryEndpointMiddleware` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddCloudFoundryActuator()` | Made internal |
| `Steeltoe.Management.Endpoint.CloudFoundry.CloudFoundryEndpointOptions.CloudFoundryApi` | Property | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `Api` | |
| `Steeltoe.Management.Endpoint.CloudFoundry.CloudFoundryManagementOptions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `CloudFoundryEndpointOptions`, `ManagementOptions` | |
| `Steeltoe.Management.Endpoint.CloudFoundry.CloudFoundrySecurityMiddleware` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `EndpointApplicationBuilderExtensions.UseCloudFoundrySecurity()` | Made internal |
| `Steeltoe.Management.Endpoint.CloudFoundry.CloudFoundrySecurityStartupFilter` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `EndpointApplicationBuilderExtensions.UseCloudFoundrySecurity()` | |
| `Steeltoe.Management.Endpoint.CloudFoundry.ICloudFoundryEndpoint` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `ICloudFoundryEndpointHandler` | |
| `Steeltoe.Management.Endpoint.CloudFoundry.ICloudFoundryOptions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `CloudFoundryEndpointOptions` | |
| `Steeltoe.Management.Endpoint.CloudFoundry.SecurityBase` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Moved to internal type `PermissionsProvider` |
| `Steeltoe.Management.Endpoint.CloudFoundry.SecurityResult` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Made internal |
| `Steeltoe.Management.Endpoint.Configuration.ConfigureEndpointOptions<TOptions>` | Type | Steeltoe.Management.Endpoint | Added | | Derive when implementing custom actuator |
| `Steeltoe.Management.Endpoint.Configuration.IConfigureOptionsWithKey<T>` | Type | Steeltoe.Management.Endpoint | Added | | Contract for loading options in custom actuator |
| `Steeltoe.Management.Endpoint.Configuration.ManagementOptions.SslEnabled` | Property | Steeltoe.Management.Endpoint | Added | | Whether `options.Port` applies to HTTP or HTTPS |
| `Steeltoe.Management.Endpoint.ContentNegotiation.ContentNegotiationExtensions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Redundant |
| `Steeltoe.Management.Endpoint.CoreActuatorServiceCollectionExtensions` | Type | Steeltoe.Management.Endpoint | Added | | Building block to implement custom actuator |
| `Steeltoe.Management.Endpoint.DbMigrations` | Namespace | Steeltoe.Management.Endpoint [Base/Core] | Moved | `Steeltoe.Management.Endpoint.Actuators.DbMigrations` | |
| `Steeltoe.Management.Endpoint.DbMigrations.DbMigrationsEndpoint` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `IDbMigrationsEndpointHandler` | Moved to internal type `DbMigrationsEndpointHandler` |
| `Steeltoe.Management.Endpoint.DbMigrations.DbMigrationsEndpoint.DbMigrationsEndpointHelper` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Redundant |
| `Steeltoe.Management.Endpoint.DbMigrations.DbMigrationsEndpointMiddleware` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddDbMigrationsActuator()` | Made internal |
| `Steeltoe.Management.Endpoint.DbMigrations.DbMigrationsEndpointOptions.KeysToSanitize` | Property | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Was never used |
| `Steeltoe.Management.Endpoint.DbMigrations.DbMigrationsStartupFilter` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddDbMigrationsActuator()` | |
| `Steeltoe.Management.Endpoint.DbMigrations.IDbMigrationsEndpoint` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `IDbMigrationsEndpointHandler` | |
| `Steeltoe.Management.Endpoint.DbMigrations.IDbMigrationsOptions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `DbMigrationsEndpointOptions` | |
| `Steeltoe.Management.Endpoint.Diagnostics.DiagnosticServices` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Moved to internal type `...HttpExchanges.Diagnostics.DiagnosticsService` |
| `Steeltoe.Management.Endpoint.EndpointCollectionConventionBuilder` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.ConfigureActuatorEndpoints()` | |
| `Steeltoe.Management.Endpoint.EndPointExtensions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Moved to internal type `EndpointOptionsExtensions` |
| `Steeltoe.Management.Endpoint.Env` | Namespace | Steeltoe.Management.Endpoint [Base/Core] | Moved | `Steeltoe.Management.Endpoint.Actuators.Environment` | |
| `Steeltoe.Management.Endpoint.Env.EndpointServiceCollectionExtensions.AddEnvActuator` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddEnvironmentActuator()` | |
| `Steeltoe.Management.Endpoint.Env.EnvEndpoint` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `IEnvironmentEndpointHandler` | Moved to internal type `EnvironmentEndpointHandler` |
| `Steeltoe.Management.Endpoint.Env.EnvEndpointMiddleware` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddEnvironmentActuator()` | Made internal |
| `Steeltoe.Management.Endpoint.Env.EnvEndpointOptions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `EnvironmentEndpointOptions` | |
| `Steeltoe.Management.Endpoint.Env.EnvironmentDescriptor` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `EnvironmentResponse` | |
| `Steeltoe.Management.Endpoint.Env.EnvStartupFilter` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddEnvironmentActuator()` | |
| `Steeltoe.Management.Endpoint.Env.GenericHostingEnvironment` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | No longer needed |
| `Steeltoe.Management.Endpoint.Env.IEnvEndpoint` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `IEnvironmentEndpointHandler` | |
| `Steeltoe.Management.Endpoint.Env.IEnvOptions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `EnvironmentEndpointOptions` | |
| `Steeltoe.Management.Endpoint.Env.Sanitizer` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | Use `KeysToSanitize` in configuration | Moved to internal type `Sanitizer` |
| `Steeltoe.Management.Endpoint.Exposure` | Type | Steeltoe.Management.Endpoint [Base/Core] | Moved | `Steeltoe.Management.Endpoint.Configuration.Exposure` | |
| `Steeltoe.Management.Endpoint.Health` | Namespace | Steeltoe.Management.Endpoint [Base/Core] | Moved | `Steeltoe.Management.Endpoint.Actuators.Health` | |
| `Steeltoe.Management.Endpoint.Health.Contributor` | Namespace | Steeltoe.Management.Endpoint [Base/Core] | Moved | `Steeltoe.Management.Endpoint.Actuators.Health.Contributors` | |
| `Steeltoe.Management.Endpoint.Health.Contributor.DiskSpaceContributor` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Made internal |
| `Steeltoe.Management.Endpoint.Health.Contributor.PingHealthContributor` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Made internal |
| `Steeltoe.Management.Endpoint.Health.DefaultHealthAggregator` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Moved to internal type `HealthAggregator` in Steeltoe.Common package |
| `Steeltoe.Management.Endpoint.Health.EndpointServiceCollectionExtensions.AddHealthActuator` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Moved | `builder.Services.AddHealthActuator()` | |
| `Steeltoe.Management.Endpoint.Health.EndpointServiceCollectionExtensions.AddHealthContributors` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Moved | `builder.Services.AddHealthContributor()` | |
| `Steeltoe.Management.Endpoint.Health.HealthCheckExtensions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Moved to internal type `HealthAggregator` in Steeltoe.Common package |
| `Steeltoe.Management.Endpoint.Health.HealthConverter` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | No longer needed |
| `Steeltoe.Management.Endpoint.Health.HealthConverterV3` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | No longer needed |
| `Steeltoe.Management.Endpoint.Health.HealthEndpoint` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `IHealthEndpointHandler` | Moved to internal type `HealthEndpointHandler` |
| `Steeltoe.Management.Endpoint.Health.HealthEndpointCore` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `IHealthEndpointHandler` | Moved to internal type `HealthEndpointHandler` |
| `Steeltoe.Management.Endpoint.Health.HealthEndpointMiddleware` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddHealthActuator()` | Made internal |
| `Steeltoe.Management.Endpoint.Health.HealthRegistrationsAggregator` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | Inject `IHealthAggregator` | Moved to internal type `HealthAggregator` in Steeltoe.Common package |
| `Steeltoe.Management.Endpoint.Health.HealthStartupFilter` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddHealthActuator()` | |
| `Steeltoe.Management.Endpoint.Health.IHealthEndpoint` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `IHealthEndpointHandler` | |
| `Steeltoe.Management.Endpoint.Health.IHealthOptions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `HealthEndpointOptions` | |
| `Steeltoe.Management.Endpoint.Health.IHealthRegistrationsAggregator` | Type | Steeltoe.Management.Endpoint [Base/Core] | Moved | `IHealthAggregator` in Steeltoe.Common package | |
| `Steeltoe.Management.Endpoint.Health.IServiceProviderExtensions.InitializeAvailability` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Made internal |
| `Steeltoe.Management.Endpoint.Health.ShowDetails` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `Steeltoe.Management.Endpoint.Actuators.Health.ShowValues` | Numbers of constants have changed |
| `Steeltoe.Management.Endpoint.HeapDump` | Namespace | Steeltoe.Management.Endpoint [Base/Core] | Moved | `Steeltoe.Management.Endpoint.Actuators.HeapDump` | |
| `Steeltoe.Management.Endpoint.HeapDump.HeapDumpEndpoint` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `IHeapDumpEndpointHandler` | Moved to internal type `HeapDumpEndpointHandler` |
| `Steeltoe.Management.Endpoint.HeapDump.HeapDumpEndpointMiddleware` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddHeapDumpActuator()` | Made internal |
| `Steeltoe.Management.Endpoint.HeapDump.HeapDumper` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Made internal |
| `Steeltoe.Management.Endpoint.HeapDump.HeapDumpStartupFilter` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddHeapDumpActuator()` | |
| `Steeltoe.Management.Endpoint.HeapDump.IHeapDumpEndpoint` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `IHeapDumpEndpointHandler` | |
| `Steeltoe.Management.Endpoint.HeapDump.IHeapDumper` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Made internal |
| `Steeltoe.Management.Endpoint.HeapDump.IHeapDumpOptions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `HeapDumpEndpointOptions` | |
| `Steeltoe.Management.Endpoint.HeapDump.LinuxHeapDumper` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Redundant |
| `Steeltoe.Management.Endpoint.Hypermedia` | Namespace | Steeltoe.Management.Endpoint [Base/Core] | Moved | `Steeltoe.Management.Endpoint.Actuators.Hypermedia` | |
| `Steeltoe.Management.Endpoint.Hypermedia.ActuatorEndpoint` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `IHypermediaEndpointHandler` | Moved to internal type `HypermediaEndpointHandler` |
| `Steeltoe.Management.Endpoint.Hypermedia.ActuatorHypermediaEndpointMiddleware` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddHypermediaActuator()` | Moved to internal type `HypermediaEndpointMiddlewar` |
| `Steeltoe.Management.Endpoint.Hypermedia.ActuatorManagementOptions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | Configure at key `Management:Endpoints:Actuator:Exposure` | |
| `Steeltoe.Management.Endpoint.Hypermedia.EndpointServiceCollectionExtensions.AddActuatorManagementOptions` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Redundant |
| `Steeltoe.Management.Endpoint.Hypermedia.HypermediaService` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Made internal |
| `Steeltoe.Management.Endpoint.Hypermedia.HypermediaStartupFilter` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddHypermediaActuator()` | |
| `Steeltoe.Management.Endpoint.Hypermedia.IActuatorEndpoint` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `IHypermediaEndpointHandler` | |
| `Steeltoe.Management.Endpoint.Hypermedia.IActuatorHypermediaOptions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `HypermediaEndpointOptions` | |
| `Steeltoe.Management.Endpoint.Hypermedia.Links._links` | Property | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `Entries` | |
| `Steeltoe.Management.Endpoint.IEndpointHandler<TRequest, TResponse>` | Type | Steeltoe.Management.Endpoint | Added | | Implement for custom actuator |
| `Steeltoe.Management.Endpoint.Info` | Namespace | Steeltoe.Management.Endpoint [Base/Core] | Moved | `Steeltoe.Management.Endpoint.Actuators.Info` | |
| `Steeltoe.Management.Endpoint.Info.Contributor.AppSettingsInfoContributor` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Made internal |
| `Steeltoe.Management.Endpoint.Info.Contributor.BuildInfoContributor` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Made internal |
| `Steeltoe.Management.Endpoint.Info.Contributor.GitInfoContributor` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Made internal |
| `Steeltoe.Management.Endpoint.Info.IInfoEndpoint` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `IInfoEndpointHandler` | |
| `Steeltoe.Management.Endpoint.Info.IInfoOptions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `InfoEndpointOptions` | |
| `Steeltoe.Management.Endpoint.Info.InfoEndpoint` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `IInfoEndpointHandler` | Moved to internal type `InfoEndpointHandler` |
| `Steeltoe.Management.Endpoint.Info.InfoEndpointMiddleware` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddInfoActuator()` | Made internal |
| `Steeltoe.Management.Endpoint.Info.InfoStartupFilter` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddInfoActuator()` | |
| `Steeltoe.Management.Endpoint.Loggers` | Namespace | Steeltoe.Management.Endpoint [Base/Core] | Moved | `Steeltoe.Management.Endpoint.Actuators.Loggers` | |
| `Steeltoe.Management.Endpoint.Loggers.ILoggersEndpoint` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `ILoggersEndpointHandler` | |
| `Steeltoe.Management.Endpoint.Loggers.ILoggersOptions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `LoggersEndpointOptions` | |
| `Steeltoe.Management.Endpoint.Loggers.LoggerLevels.MapLogLevel` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Made internal |
| `Steeltoe.Management.Endpoint.Loggers.LoggersChangeRequest` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `LoggersRequest` | |
| `Steeltoe.Management.Endpoint.Loggers.LoggersEndpoint` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `ILoggersEndpointHandler` | Moved to internal type `LoggersEndpointHandler` |
| `Steeltoe.Management.Endpoint.Loggers.LoggersEndpointMiddleware` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddLoggersActuator()` | Made internal |
| `Steeltoe.Management.Endpoint.Loggers.LoggersStartupFilter` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddLoggersActuator()` | |
| `Steeltoe.Management.Endpoint.ManagementEndpointOptions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Moved | `Steeltoe.Management.Endpoint.Configuration.ManagementOptions` | |
| `Steeltoe.Management.Endpoint.ManagementEndpointOptions.EndpointOptions` | Property | Steeltoe.Management.Endpoint [Base/Core] | Removed | `EndpointMiddleware<,>.EndpointOptions` or inject `IOptionsMonitor` | |
| `Steeltoe.Management.Endpoint.ManagementEndpointOptions.Sensitive` | Property | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Was never used |
| `Steeltoe.Management.Endpoint.ManagementHostBuilderExtensions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | Use extension methods on `IServiceCollection` | Redundant |
| `Steeltoe.Management.Endpoint.ManagementPort.ManagementPortMiddleware` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Made internal |
| `Steeltoe.Management.Endpoint.ManagementWebApplicationBuilderExtensions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | Use extension methods on `IServiceCollection` | Redundant |
| `Steeltoe.Management.Endpoint.ManagementWebHostBuilderExtensions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | Use extension methods on `IServiceCollection` | Redundant |
| `Steeltoe.Management.Endpoint.Mappings` | Namespace | Steeltoe.Management.Endpoint [Base/Core] | Moved | `Steeltoe.Management.Endpoint.Actuators.RouteMappings` | |
| `Steeltoe.Management.Endpoint.Mappings.ApplicationMappings` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `RouteMappingsResponse` | |
| `Steeltoe.Management.Endpoint.Mappings.AspNetCoreRouteDetails` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `RouteConditionsDescriptor` | |
| `Steeltoe.Management.Endpoint.Mappings.AspNetCoreRouteDetails.HttpMethods` | Property | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `RouteConditionsDescriptor.Methods` | |
| `Steeltoe.Management.Endpoint.Mappings.AspNetCoreRouteDetails.RouteTemplate` | Property | Steeltoe.Management.Endpoint [Base/Core] | Removed | `RouteConditionsDescriptor.Patterns` | |
| `Steeltoe.Management.Endpoint.Mappings.ContextMappings` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `RouteMappingContext` | |
| `Steeltoe.Management.Endpoint.Mappings.EndpointServiceCollectionExtensions.AddMappingsActuator` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `AddRouteMappingsActuator` | |
| `Steeltoe.Management.Endpoint.Mappings.IMappingsOptions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `RouteMappingsEndpointOptions` | |
| `Steeltoe.Management.Endpoint.Mappings.IRouteDetails` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `RouteConditionsDescriptor` | Redundant |
| `Steeltoe.Management.Endpoint.Mappings.IRouteMappings` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Conventional routes are no longer shown in mappings actuator |
| `Steeltoe.Management.Endpoint.Mappings.MappingDescription` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `RouteDescriptor` | |
| `Steeltoe.Management.Endpoint.Mappings.MappingsEndpoint` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `IRouteMappingsEndpointHandler` | Moved to internal type `RouteMappingsEndpointHandler` |
| `Steeltoe.Management.Endpoint.Mappings.MappingsEndpointMiddleware` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddRouteMappingsActuator()` | Moved to internal type `RouteMappingsEndpointMiddleware` |
| `Steeltoe.Management.Endpoint.Mappings.MappingsEndpointOptions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `RouteMappingsEndpointOptions` | |
| `Steeltoe.Management.Endpoint.Mappings.MappingsStartupFilter` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddRouteMappingsActuator()` | |
| `Steeltoe.Management.Endpoint.Mappings.RouteBuilderExtensions.AddRoutesToMappingsActuator` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Conventional routes are no longer shown in mappings actuator |
| `Steeltoe.Management.Endpoint.Mappings.RouteMappings` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Conventional routes are no longer shown in mappings actuator |
| `Steeltoe.Management.Endpoint.MediaTypeVersion` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Each actuator uses built-in media type |
| `Steeltoe.Management.Endpoint.Metrics` | Namespace | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Metrics actuator has been removed |
| `Steeltoe.Management.Endpoint.Metrics.IPrometheusEndpointOptions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `PrometheusEndpointOptions` in Steeltoe.Management.Prometheus package | |
| `Steeltoe.Management.Endpoint.Metrics.PrometheusEndpointOptions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Moved | `PrometheusEndpointOptions` in Steeltoe.Management.Prometheus package | |
| `Steeltoe.Management.Endpoint.Metrics.PrometheusEndpointOptions.ScrapeResponseCacheDurationMilliseconds` | Property | Steeltoe.Management.Endpoint [Base/Core] | Removed | | Was never used |
| `Steeltoe.Management.Endpoint.Middleware.ActuatorMetadataProvider` | Type | Steeltoe.Management.Endpoint | Added | | Default implementation to provide metadata for route mappings actuator |
| `Steeltoe.Management.Endpoint.Middleware.EndpointMiddleware<TRequest, TResponse>.CanInvoke` | Method | Steeltoe.Management.Endpoint | Added | | Verifies enabled/exposed based on request path |
| `Steeltoe.Management.Endpoint.Middleware.EndpointMiddleware<TRequest, TResponse>.GetMetadataProvider` | Method | Steeltoe.Management.Endpoint | Added | | Provides metadata for route mappings actuator |
| `Steeltoe.Management.Endpoint.Middleware.EndpointMiddleware<TRequest, TResponse>.InvokeAsync` | Method | Steeltoe.Management.Endpoint | Added | | `IMiddleware` interface implementation |
| `Steeltoe.Management.Endpoint.Middleware.EndpointMiddleware<TRequest, TResponse>.ParseRequestAsync` | Method | Steeltoe.Management.Endpoint | Added | | Creates `TRequest` instance from `HttpContext` |
| `Steeltoe.Management.Endpoint.Middleware.EndpointMiddleware<TResult, TRequest>` | Type | Steeltoe.Management.Endpoint [Base/Core] | Changed | `EndpointMiddleware<TRequest, TResponse>` | Type parameter order reversed |
| `Steeltoe.Management.Endpoint.Middleware.EndpointMiddleware<TResult>` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `EndpointMiddleware<TRequest, TResponse>` | Pass `object?`/`null` for unused `TRequest` |
| `Steeltoe.Management.Endpoint.Middleware.EndpointMiddleware<TResult>._endpoint` | Field | Steeltoe.Management.Endpoint [Base/Core] | Removed | `EndpointMiddleware<TRequest, TResponse>.EndpointHandler` | |
| `Steeltoe.Management.Endpoint.Middleware.EndpointMiddleware<TResult>._logger` | Field | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | |
| `Steeltoe.Management.Endpoint.Middleware.EndpointMiddleware<TResult>._mgmtOptions` | Field | Steeltoe.Management.Endpoint [Base/Core] | Removed | `EndpointMiddleware<TRequest, TResponse>.ManagementOptionsMonitor` | |
| `Steeltoe.Management.Endpoint.Middleware.EndpointMiddleware<TResult>.Endpoint` | Property | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `EndpointMiddleware<TRequest, TResponse>.EndpointHandler` | |
| `Steeltoe.Management.Endpoint.Middleware.EndpointMiddleware<TResult>.HandleRequest` | Method | Steeltoe.Management.Endpoint [Base/Core] | Removed | `EndpointMiddleware<TRequest, TResponse>.InvokeEndpointHandlerAsync` | |
| `Steeltoe.Management.Endpoint.Middleware.EndpointMiddleware<TResult>.Serialize` | Method | Steeltoe.Management.Endpoint [Base/Core] | Removed | `EndpointMiddleware<TRequest, TResponse>.WriteResponseAsync` | |
| `Steeltoe.Management.Endpoint.Middleware.IEndpointMiddleware` | Type | Steeltoe.Management.Endpoint | Added | | Implement for custom actuator |
| `Steeltoe.Management.Endpoint.Refresh` | Namespace | Steeltoe.Management.Endpoint [Base/Core] | Moved | `Steeltoe.Management.Endpoint.Actuators.Refresh` | |
| `Steeltoe.Management.Endpoint.Refresh.IRefreshEndpoint` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `IRefreshEndpointHandler` | |
| `Steeltoe.Management.Endpoint.Refresh.IRefreshOptions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `RefreshEndpointOptions` | |
| `Steeltoe.Management.Endpoint.Refresh.RefreshEndpoint` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `IRefreshEndpointHandler` | Moved to internal type `RefreshEndpointHandler` |
| `Steeltoe.Management.Endpoint.Refresh.RefreshEndpointMiddleware` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddRefreshActuator()` | Made internal |
| `Steeltoe.Management.Endpoint.Refresh.RefreshStartupFilter` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddRefreshActuator()` | |
| `Steeltoe.Management.Endpoint.Security.EndpointClaim` | Type | Steeltoe.Management.Endpoint [Base/Core] | Moved | `Steeltoe.Management.Endpoint.Actuators.Health.EndpointClaim` | |
| `Steeltoe.Management.Endpoint.Security.ISecurityContext` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Redundant |
| `Steeltoe.Management.Endpoint.SpringBootAdminClient.SpringBootAdminApplicationBuilderExtensions.RegisterWithSpringBootAdmin` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddSpringBootAdminClient()` | |
| `Steeltoe.Management.Endpoint.SpringBootAdminClient.SpringBootAdminClientOptions.ConnectionTimeoutMS` | Property | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `SpringBootAdminClientOptions.ConnectionTimeoutMs` | |
| `Steeltoe.Management.Endpoint.ThreadDump` | Namespace | Steeltoe.Management.Endpoint [Base/Core] | Moved | `Steeltoe.Management.Endpoint.Actuators.ThreadDump` | |
| `Steeltoe.Management.Endpoint.ThreadDump.IThreadDumpEndpoint` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `IThreadDumpEndpointHandler` | |
| `Steeltoe.Management.Endpoint.ThreadDump.IThreadDumpEndpointV2` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `IThreadDumpEndpointHandler` | Redundant |
| `Steeltoe.Management.Endpoint.ThreadDump.IThreadDumper` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Redundant |
| `Steeltoe.Management.Endpoint.ThreadDump.IThreadDumpOptions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `ThreadDumpEndpointOptions` | |
| `Steeltoe.Management.Endpoint.ThreadDump.LockInfo` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Was never used |
| `Steeltoe.Management.Endpoint.ThreadDump.MetaDataImportProvider` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Was never used |
| `Steeltoe.Management.Endpoint.ThreadDump.MonitorInfo` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Was never used |
| `Steeltoe.Management.Endpoint.ThreadDump.ThreadDumpEndpoint` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `IThreadDumpEndpointHandler` | Moved to internal type `ThreadDumpEndpointHandler` |
| `Steeltoe.Management.Endpoint.ThreadDump.ThreadDumpEndpoint_v2` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `IThreadDumpEndpointHandler` | Redundant |
| `Steeltoe.Management.Endpoint.ThreadDump.ThreadDumpEndpointMiddleware` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddThreadDumpActuator()` | Made internal |
| `Steeltoe.Management.Endpoint.ThreadDump.ThreadDumpEndpointMiddleware_v2` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddThreadDumpActuator()` | Redundant |
| `Steeltoe.Management.Endpoint.ThreadDump.ThreadDumperEP` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Renamed to internal type `EventPipeThreadDumper` |
| `Steeltoe.Management.Endpoint.ThreadDump.ThreadDumpResult` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Redundant |
| `Steeltoe.Management.Endpoint.ThreadDump.ThreadDumpStartupFilter` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddThreadDumpActuator()` | |
| `Steeltoe.Management.Endpoint.ThreadDump.TState` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `State` | |
| `Steeltoe.Management.Endpoint.Trace` | Namespace | Steeltoe.Management.Endpoint [Base/Core] | Moved | `Steeltoe.Management.Endpoint.Actuators.HttpExchanges` | |
| `Steeltoe.Management.Endpoint.Trace.EndpointServiceCollectionExtensions.AddTraceActuator` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `AddHttpExchangesActuator` | |
| `Steeltoe.Management.Endpoint.Trace.HttpTrace` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `HttpExchange` | |
| `Steeltoe.Management.Endpoint.Trace.HttpTraceDiagnosticObserver` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Moved to internal type `HttpExchangesDiagnosticObserver` |
| `Steeltoe.Management.Endpoint.Trace.HttpTraceEndpoint` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `IHttpExchangesEndpointHandler` | Moved to internal type `HttpExchangesEndpointHandler` |
| `Steeltoe.Management.Endpoint.Trace.HttpTraceEndpointMiddleware` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddHttpExchangesActuator()` | Moved to internal type `HttpExchangesEndpointMiddleware` |
| `Steeltoe.Management.Endpoint.Trace.HttpTraceEndpointOptions.AddAuthType` | Property | Steeltoe.Management.Endpoint [Base/Core] | Removed | `HttpExchangesEndpointOptions.IncludeRequestHeaders` | Was effectively ignored |
| `Steeltoe.Management.Endpoint.Trace.HttpTraceEndpointOptions.AddParameters` | Property | Steeltoe.Management.Endpoint [Base/Core] | Removed | `HttpExchangesEndpointOptions.IncludeQueryString` | Form data is no longer included |
| `Steeltoe.Management.Endpoint.Trace.HttpTraceEndpointOptions.AddPathInfo` | Property | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `HttpExchangesEndpointOptions.IncludePathInfo` | |
| `Steeltoe.Management.Endpoint.Trace.HttpTraceEndpointOptions.AddQueryString` | Property | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `HttpExchangesEndpointOptions.IncludeQueryString` | |
| `Steeltoe.Management.Endpoint.Trace.HttpTraceEndpointOptions.AddRemoteAddress` | Property | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `HttpExchangesEndpointOptions.IncludeRemoteAddress` | |
| `Steeltoe.Management.Endpoint.Trace.HttpTraceEndpointOptions.AddRequestHeaders` | Property | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `HttpExchangesEndpointOptions.IncludeRequestHeaders` | |
| `Steeltoe.Management.Endpoint.Trace.HttpTraceEndpointOptions.AddResponseHeaders` | Property | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `HttpExchangesEndpointOptions.IncludeResponseHeaders` | |
| `Steeltoe.Management.Endpoint.Trace.HttpTraceEndpointOptions.AddSessionId` | Property | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `HttpExchangesEndpointOptions.IncludeSessionId` | |
| `Steeltoe.Management.Endpoint.Trace.HttpTraceEndpointOptions.AddTimeTaken` | Property | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `HttpExchangesEndpointOptions.IncludeTimeTaken` | |
| `Steeltoe.Management.Endpoint.Trace.HttpTraceEndpointOptions.AddUserPrincipal` | Property | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `HttpExchangesEndpointOptions.IncludeUserPrincipal` | |
| `Steeltoe.Management.Endpoint.Trace.HttpTraceResult` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `HttpExchangesResult` | |
| `Steeltoe.Management.Endpoint.Trace.IHttpTraceEndpoint` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `IHttpExchangesEndpointHandler` | |
| `Steeltoe.Management.Endpoint.Trace.IHttpTraceRepository` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `IHttpExchangesRepository` | |
| `Steeltoe.Management.Endpoint.Trace.IHttpTraceRepository.GetTraces` | Method | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `IHttpExchangesRepository.GetHttpExchanges` | |
| `Steeltoe.Management.Endpoint.Trace.ITraceEndpoint` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `IHttpExchangesEndpointHandler` | |
| `Steeltoe.Management.Endpoint.Trace.ITraceOptions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `HttpExchangesEndpointOptions` | |
| `Steeltoe.Management.Endpoint.Trace.ITraceRepository` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `IHttpExchangesRepository` | |
| `Steeltoe.Management.Endpoint.Trace.ITraceRepository.GetTraces` | Method | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `IHttpExchangesRepository.GetHttpExchanges` | |
| `Steeltoe.Management.Endpoint.Trace.Principal` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `HttpExchangePrincipal` | |
| `Steeltoe.Management.Endpoint.Trace.Request` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `HttpExchangeRequest` | |
| `Steeltoe.Management.Endpoint.Trace.Response` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `HttpExchangeResponse` | |
| `Steeltoe.Management.Endpoint.Trace.Session` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `HttpExchangeSession` | |
| `Steeltoe.Management.Endpoint.Trace.TraceDiagnosticObserver` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Moved to internal type `HttpExchangesDiagnosticObserver` |
| `Steeltoe.Management.Endpoint.Trace.TraceEndpoint` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `IHttpExchangesEndpointHandler` | Moved to internal type `HttpExchangesEndpointHandler` |
| `Steeltoe.Management.Endpoint.Trace.TraceEndpointMiddleware` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddHttpExchangesActuator()` | Moved to internal type `HttpExchangesEndpointMiddleware` |
| `Steeltoe.Management.Endpoint.Trace.TraceEndpointOptions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `HttpExchangesEndpointOptions` | |
| `Steeltoe.Management.Endpoint.Trace.TraceEndpointOptions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `HttpExchangesEndpointOptions` | |
| `Steeltoe.Management.Endpoint.Trace.TraceResult` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `HttpExchange` | |
| `Steeltoe.Management.Endpoint.Trace.TraceStartupFilter` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddHttpExchangesActuator()` | |
| `Steeltoe.Management.Endpoint.Utils` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Redundant |
| `Steeltoe.Management.IEndpoint` | Type | Steeltoe.Management.Abstractions | Removed | `IEndpointHandler<,>` in Steeltoe.Management.Endpoint package | |
| `Steeltoe.Management.IEndpointOptions` | Type | Steeltoe.Management.Abstractions | Removed | `Steeltoe.Management.Configuration.EndpointOptions` | |
| `Steeltoe.Management.IManagementOptions` | Type | Steeltoe.Management.Abstractions | Removed | `ManagementOptions` in Steeltoe.Management.Endpoint package | |
| `Steeltoe.Management.Info.AbstractConfigurationContributor` | Type | Steeltoe.Management.Abstractions | Removed | None | Moved to internal type `ConfigurationContributor` |
| `Steeltoe.Management.Info.IInfoBuilder` | Type | Steeltoe.Management.Abstractions | Removed | `InfoBuilder` in Steeltoe.Management.Endpoint package | Redundant |
| `Steeltoe.Management.Info.IInfoContributor` | Type | Steeltoe.Management.Abstractions | Moved | `IInfoContributor` in Steeltoe.Management.Endpoint package | |
| `Steeltoe.Management.Info.IInfoContributor.Contribute` | Method | Steeltoe.Management.Abstractions | Moved | `ContributeAsync()` | |
| `Steeltoe.Management.Info.InfoBuilder` | Type | Steeltoe.Management.Abstractions | Moved | `InfoBuilder` in Steeltoe.Management.Endpoint package | |
| `Steeltoe.Management.Kubernetes` | Namespace | Steeltoe.Management.KubernetesCore | Removed | None | |
| `Steeltoe.Management.OpenTelemetry` | Namespace | Steeltoe.Management.OpenTelemetryBase | Removed | `builder.Services.AddPrometheusActuator()` from Steeltoe.Management.Prometheus package | WaveFront exporter was removed |
| `Steeltoe.Management.Permissions` | Type | Steeltoe.Management.Abstractions | Renamed | `EndpointPermissions` | |
| `Steeltoe.Management.Permissions.UNDEFINED` | Field | Steeltoe.Management.Abstractions | Removed | None | Default in options changed to Restricted |
| `Steeltoe.Management.Prometheus.PrometheusEndpointOptions` | Type | Steeltoe.Management.Prometheus | Added | | Configuration for Prometheus actuator |
| `Steeltoe.Management.Prometheus.PrometheusExtensions.AddPrometheusActuator` | Extension method | Steeltoe.Management.Prometheus | Added | | Add Prometheus actuator to service collection |
| `Steeltoe.Management.Prometheus.PrometheusExtensions.UsePrometheusActuator` | Extension method | Steeltoe.Management.Prometheus | Added | | Activate Prometheus actuator middleware |
| `Steeltoe.Management.TaskCore` | Namespace | Steeltoe.Management.TaskCore | Moved | `Steeltoe.Management.Tasks` | |
| `Steeltoe.Management.TaskCore.DelegatingTask` | Type | Steeltoe.Management.TaskCore | Removed | `builder.Services.AddTask()` | Made internal |
| `Steeltoe.Management.TaskCore.TaskWebHostExtensions.RunWithTasks` | Extension method | Steeltoe.Management.TaskCore | Renamed | `RunWithTasksAsync` | |
| `Steeltoe.Management.Tasks.TaskHostExtensions.HasApplicationTask` | Extension method | Steeltoe.Management.Tasks | Added | | Check whether a task will run |
| `Steeltoe.Management.Tracing.TracingBaseHostBuilderExtensions.AddDistributedTracing` | Extension method | Steeltoe.Management.Tracing [Base/Core] | Removed | Use OpenTelemetry packages directly | |
| `Steeltoe.Management.Tracing.TracingBaseServiceCollectionExtensions.AddDistributedTracing` | Extension method | Steeltoe.Management.Tracing [Base/Core] | Removed | Use OpenTelemetry packages directly | |
| `Steeltoe.Management.Tracing.TracingCoreServiceCollectionExtensions.AddDistributedTracingAspNetCore` | Extension method | Steeltoe.Management.Tracing [Base/Core] | Removed | Use OpenTelemetry packages directly | |
| `Steeltoe.Management.Tracing.TracingHostBuilderExtensions.AddDistributedTracincAspNetCore` | Extension method | Steeltoe.Management.Tracing [Base/Core] | Removed | Use OpenTelemetry packages directly | |
| `Steeltoe.Management.Tracing.TracingLogProcessor.GetCurrentSpan` | Method | Steeltoe.Management.Tracing [Base/Core] | Removed | None | Redundant |
| `Steeltoe.Management.Tracing.TracingOptions` | Type | Steeltoe.Management.Tracing [Base/Core] | Removed | Use OpenTelemetry packages directly | |
| `Steeltoe.Management.Tracing.TracingServiceCollectionExtensions.AddTracingLogProcessor` | Extension method | Steeltoe.Management.Tracing | Added | | Add trace info from `Activity.Current` to logs |

### Notable PRs

- https://github.com/SteeltoeOSS/Steeltoe/pull/1521
- https://github.com/SteeltoeOSS/Steeltoe/pull/1520
- https://github.com/SteeltoeOSS/Steeltoe/pull/1517
- https://github.com/SteeltoeOSS/Steeltoe/pull/1508
- https://github.com/SteeltoeOSS/Steeltoe/pull/1503
- https://github.com/SteeltoeOSS/Steeltoe/pull/1490
- https://github.com/SteeltoeOSS/Steeltoe/pull/1474
- https://github.com/SteeltoeOSS/Steeltoe/pull/1457
- https://github.com/SteeltoeOSS/Steeltoe/pull/1454
- https://github.com/SteeltoeOSS/Steeltoe/pull/1451
- https://github.com/SteeltoeOSS/Steeltoe/pull/1444
- https://github.com/SteeltoeOSS/Steeltoe/pull/1443
- https://github.com/SteeltoeOSS/Steeltoe/pull/1438
- https://github.com/SteeltoeOSS/Steeltoe/pull/1424
- https://github.com/SteeltoeOSS/Steeltoe/pull/1422
- https://github.com/SteeltoeOSS/Steeltoe/pull/1421
- https://github.com/SteeltoeOSS/Steeltoe/pull/1417
- https://github.com/SteeltoeOSS/Steeltoe/pull/1416
- https://github.com/SteeltoeOSS/Steeltoe/pull/1413
- https://github.com/SteeltoeOSS/Steeltoe/pull/1402
- https://github.com/SteeltoeOSS/Steeltoe/pull/1401
- https://github.com/SteeltoeOSS/Steeltoe/pull/1398
- https://github.com/SteeltoeOSS/Steeltoe/pull/1396
- https://github.com/SteeltoeOSS/Steeltoe/pull/1393
- https://github.com/SteeltoeOSS/Steeltoe/pull/1392
- https://github.com/SteeltoeOSS/Steeltoe/pull/1390
- https://github.com/SteeltoeOSS/Steeltoe/pull/1389
- https://github.com/SteeltoeOSS/Steeltoe/pull/1386
- https://github.com/SteeltoeOSS/Steeltoe/pull/1385
- https://github.com/SteeltoeOSS/Steeltoe/pull/1382
- https://github.com/SteeltoeOSS/Steeltoe/pull/1380
- https://github.com/SteeltoeOSS/Steeltoe/pull/1378
- https://github.com/SteeltoeOSS/Steeltoe/pull/1364
- https://github.com/SteeltoeOSS/Steeltoe/pull/1357
- https://github.com/SteeltoeOSS/Steeltoe/pull/1356
- https://github.com/SteeltoeOSS/Steeltoe/pull/1353
- https://github.com/SteeltoeOSS/Steeltoe/pull/1331
- https://github.com/SteeltoeOSS/Steeltoe/pull/1278
- https://github.com/SteeltoeOSS/Steeltoe/pull/1247
- https://github.com/SteeltoeOSS/Steeltoe/pull/1224
- https://github.com/SteeltoeOSS/Steeltoe/pull/1198
- https://github.com/SteeltoeOSS/Steeltoe/pull/1187
- https://github.com/SteeltoeOSS/Steeltoe/pull/1185
- https://github.com/SteeltoeOSS/Steeltoe/pull/1184
- https://github.com/SteeltoeOSS/Steeltoe/pull/1177
- https://github.com/SteeltoeOSS/Steeltoe/pull/1165
- https://github.com/SteeltoeOSS/Steeltoe/pull/1155
- https://github.com/SteeltoeOSS/Steeltoe/pull/1130
- https://github.com/SteeltoeOSS/Steeltoe/pull/1120
- https://github.com/SteeltoeOSS/Steeltoe/pull/1114
- https://github.com/SteeltoeOSS/Steeltoe/pull/1101
- https://github.com/SteeltoeOSS/Steeltoe/pull/1065
- https://github.com/SteeltoeOSS/Steeltoe/pull/1050

### Documentation

For additional information, see the updated [Management documentation](../management/index.md) and
[Management samples](https://github.com/SteeltoeOSS/Samples/tree/main/Management).

## Security

### Behavior changes

- Drastically simplified implementation, leveraging the built-in ASP.NET option types
- Dropped OAuth support in favor of OpenID Connect
- Removed CredHub client, use [CredHub Service Broker](https://techdocs.broadcom.com/us/en/vmware-tanzu/platform-services/credhub-service-broker/services/credhub-sb/index.html)

### NuGet Package changes

| Source | Change | Replacement | Notes |
| --- | --- | --- | --- |
| Steeltoe.Security.Authentication.CloudFoundryBase | Removed | Steeltoe.Security.Authentication.JwtBearer, Steeltoe.Security.Authentication.OpenIdConnect, Steeltoe.Security.Authorization.Certificate | Replacement packages are split per auth method |
| Steeltoe.Security.Authentication.CloudFoundryCore | Removed | Steeltoe.Security.Authentication.JwtBearer, Steeltoe.Security.Authentication.OpenIdConnect, Steeltoe.Security.Authorization.Certificate | Replacement packages are split per auth method |
| Steeltoe.Security.Authentication.JwtBearer | Added | | JSON Web Tokens (JWT) for Cloud Foundry |
| Steeltoe.Security.Authentication.MtlsCore | Renamed | Steeltoe.Security.Authorization.Certificate | Client certificate auth for Cloud Foundry |
| Steeltoe.Security.Authentication.OpenIdConnect | Added | | OpenID Connect (OIDC) for Cloud Foundry |
| Steeltoe.Security.DataProtection.CredHubBase | Removed | None | Use CredHub Service Broker |
| Steeltoe.Security.DataProtection.CredHubCore | Removed | None | Use CredHub Service Broker |
| Steeltoe.Security.DataProtection.RedisCore | Renamed | Steeltoe.Security.DataProtection.Redis | |

### API changes

| Source | Kind | Package | Change | Replacement | Notes |
| --- | --- | --- | --- | --- | --- |
| `Steeltoe.Security.Authentication.CloudFoundry.ApplicationBuilderExtensions.UseCloudFoundryCertificateAuth` | Extension method | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Removed | `app.UseCertificateAuthorization()` | |
| `Steeltoe.Security.Authentication.CloudFoundry.ApplicationBuilderExtensions.UseCloudFoundryContainerIdentity` | Extension method | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Removed | `app.UseCertificateAuthorization()` | |
| `Steeltoe.Security.Authentication.CloudFoundry.ApplicationClaimTypes` | Type | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Removed | None | Moved to internal type `ApplicationClaimTypes` |
| `Steeltoe.Security.Authentication.CloudFoundry.AuthenticationBuilderExtensions.AddCloudFoundryIdentityCertificate` | Extension method | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Removed | `builder.Configuration.AddAppInstanceIdentityCertificate(); builder.Services.AddAuthentication().AddCertificate();` | |
| `Steeltoe.Security.Authentication.CloudFoundry.AuthenticationBuilderExtensions.AddCloudFoundryJwtBearer` | Extension method | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Removed | `builder.Services.AddAuthentication().AddJwtBearer().ConfigureJwtBearerForCloudFoundry()` | |
| `Steeltoe.Security.Authentication.CloudFoundry.AuthenticationBuilderExtensions.AddCloudFoundryOAuth` | Extension method | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Removed | Use OpenID Connect or JWT instead | OAuth support has been removed |
| `Steeltoe.Security.Authentication.CloudFoundry.AuthenticationBuilderExtensions.AddCloudFoundryOpenIdConnect` | Extension method | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Removed | `builder.Services.AddAuthentication().AddOpenIdConnect().ConfigureOpenIdConnectForCloudFoundry()` | |
| `Steeltoe.Security.Authentication.CloudFoundry.AuthorizationPolicyBuilderExtensions.SameOrg` | Extension method | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Moved | `AuthorizationPolicyBuilder.RequireSameOrg()` in Steeltoe.Security.Authorization.Certificate package | |
| `Steeltoe.Security.Authentication.CloudFoundry.AuthorizationPolicyBuilderExtensions.SameSpace` | Extension method | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Moved | `AuthorizationPolicyBuilder.RequireSameSpace()` in Steeltoe.Security.Authorization.Certificate package | |
| `Steeltoe.Security.Authentication.CloudFoundry.AuthServerOptions` | Type | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Removed | `JwtBearerOptions`, `OpenIdConnectOptions` | Now uses built-in ASP.NET option types |
| `Steeltoe.Security.Authentication.CloudFoundry.CloudFoundryCertificateIdentityAuthorizationHandler` | Type | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Removed | None | Refactored to internal type `CertificateAuthorizationHandler` |
| `Steeltoe.Security.Authentication.CloudFoundry.CloudFoundryClaimActionExtensions` | Type | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Removed | None | Refactored, no longer needed |
| `Steeltoe.Security.Authentication.CloudFoundry.CloudFoundryDefaults` | Type | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Removed | None | Contained constants that are no longer needed |
| `Steeltoe.Security.Authentication.CloudFoundry.CloudFoundryHelper` | Type | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Removed | None | Refactored, no longer needed |
| `Steeltoe.Security.Authentication.CloudFoundry.CloudFoundryJwtBearerConfigurer` | Type | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Removed | None | Refactored to internal type `PostConfigureJwtBearerOptions` |
| `Steeltoe.Security.Authentication.CloudFoundry.CloudFoundryJwtBearerOptions` | Type | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Removed | `Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerOptions` | Refactored, no longer needed |
| `Steeltoe.Security.Authentication.CloudFoundry.CloudFoundryOAuthConfigurer` | Type | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Removed | None | Refactored to internal type `PostConfigureOpenIdConnectOptions` |
| `Steeltoe.Security.Authentication.CloudFoundry.CloudFoundryOAuthHandler` | Type | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Removed | None | Refactored to internal type `TokenKeyResolver` |
| `Steeltoe.Security.Authentication.CloudFoundry.CloudFoundryOAuthOptions` | Type | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Removed | `Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectOptions` | Refactored, no longer needed |
| `Steeltoe.Security.Authentication.CloudFoundry.CloudFoundryOpenIdConnectOptions` | Type | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Removed | `Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectOptions` | Refactored, no longer needed |
| `Steeltoe.Security.Authentication.CloudFoundry.CloudFoundryScopeClaimAction` | Type | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Removed | None | Refactored, no longer needed |
| `Steeltoe.Security.Authentication.CloudFoundry.CloudFoundryTokenKeyResolver` | Type | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Removed | None | Refactored to internal type `TokenKeyResolver` |
| `Steeltoe.Security.Authentication.CloudFoundry.CloudFoundryTokenValidator` | Type | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Removed | None | Refactored, no longer needed |
| `Steeltoe.Security.Authentication.CloudFoundry.ConfigurationBuilderExtensions.AddCloudFoundryContainerIdentity` | Extension method | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Moved | `builder.Configuration.AddAppInstanceIdentityCertificate()` in Steeltoe.Security.Authorization.Certificate package | |
| `Steeltoe.Security.Authentication.CloudFoundry.MutualTlsAuthenticationOptionsPostConfigurer` | Type | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Removed | None | Refactored to internal type `PostConfigureCertificateAuthenticationOptions` |
| `Steeltoe.Security.Authentication.CloudFoundry.OpenIdTokenResponse` | Type | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Removed | None | Refactored, no longer needed |
| `Steeltoe.Security.Authentication.CloudFoundry.SameOrgRequirement` | Type | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Moved | `SameOrgRequirement` in Steeltoe.Security.Authorization.Certificate package | |
| `Steeltoe.Security.Authentication.CloudFoundry.SameSpaceRequirement` | Type | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Moved | `SameSpaceRequirement` in Steeltoe.Security.Authorization.Certificate package | |
| `Steeltoe.Security.Authentication.CloudFoundry.ServiceCollectionExtensions.AddCloudFoundryContainerIdentity` | Extension method | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Removed | `builder.Services.AddAuthorizationBuilder().AddOrgAndSpacePolicies()` in Steeltoe.Security.Authorization.Certificate package | |
| `Steeltoe.Security.Authentication.CloudFoundry.TokenExchanger` | Type | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Removed | None | Refactored, no longer needed |
| `Steeltoe.Security.Authentication.JwtBearer.JwtBearerAuthenticationBuilderExtensions.ConfigureJwtBearerForCloudFoundry` | Extension method | Steeltoe.Security.Authentication.JwtBearer | Added | | Configure JWT for UAA-based systems like Cloud Foundry |
| `Steeltoe.Security.Authentication.Mtls.CertificateApplicationBuilderExtensions.UseCertificateRotation` | Extension method | Steeltoe.Security.Authentication.MtlsCore | Removed | None | Rotating certificates in OS-level certificate store proved to be unreliable |
| `Steeltoe.Security.Authentication.Mtls.CertificateAuthenticationBuilderExtensions.AddMutualTls` | Extension method | Steeltoe.Security.Authentication.MtlsCore | Removed | `app.UseCertificateAuthorization()` | |
| `Steeltoe.Security.Authentication.Mtls.CertificateRotationHostedService` | Type | Steeltoe.Security.Authentication.MtlsCore | Removed | None | Rotating certificates in OS-level certificate store proved to be unreliable |
| `Steeltoe.Security.Authentication.Mtls.CloudFoundryInstanceCertificate` | Type | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Removed | None | Moved to internal type `ApplicationInstanceCertificate` |
| `Steeltoe.Security.Authentication.Mtls.MutualTlsAuthenticationOptions` | Type | Steeltoe.Security.Authentication.MtlsCore | Removed | `Microsoft.AspNetCore.Authentication.Certificate.CertificateAuthenticationOptions` | |
| `Steeltoe.Security.Authentication.Mtls.MutualTlsAuthenticationOptions.IssuerChain` | Property | Steeltoe.Security.Authentication.MtlsCore | Removed | `CertificateOptions.IssuerChain` in Steeltoe.Common.Certificates package | |
| `Steeltoe.Security.Authentication.OpenIdConnect.OpenIdConnectAuthenticationBuilderExtensions.ConfigureOpenIdConnectForCloudFoundry` | Extension method | Steeltoe.Security.Authentication.OpenIdConnect | Added | | Configure OIDC for UAA-based systems like Cloud Foundry |
| `Steeltoe.Security.Authorization.Certificate.CertificateApplicationBuilderExtensions.UseCertificateAuthorization` | Extension method | Steeltoe.Security.Authorization.Certificate | Added | | Activates cert/header forwarding in ASP.NET Core auth middleware |
| `Steeltoe.Security.Authorization.Certificate.CertificateAuthorizationBuilderExtensions.AddOrgAndSpacePolicies` | Extension method | Steeltoe.Security.Authorization.Certificate | Added | | Verify space/org in the incoming client certificate |
| `Steeltoe.Security.Authorization.Certificate.CertificateAuthorizationPolicies.SameOrganization` | Property | Steeltoe.Security.Authorization.Certificate | Added | | Constant for same-org policy |
| `Steeltoe.Security.Authorization.Certificate.CertificateAuthorizationPolicies.SameSpace` | Property | Steeltoe.Security.Authorization.Certificate | Added | | Constant for same-space policy |
| `Steeltoe.Security.Authorization.Certificate.CertificateAuthorizationPolicyBuilderExtensions.RequireSameOrg` | Extension method | Steeltoe.Security.Authorization.Certificate | Added | | Require client certificate to originate from same organization |
| `Steeltoe.Security.Authorization.Certificate.CertificateAuthorizationPolicyBuilderExtensions.RequireSameSpace` | Extension method | Steeltoe.Security.Authorization.Certificate | Added | | Require client certificate to originate from same space |
| `Steeltoe.Security.Authorization.Certificate.CertificateHttpClientBuilderExtensions.AddAppInstanceIdentityCertificate` | Extension method | Steeltoe.Security.Authorization.Certificate | Added | | Send app-identify certificate with outgoing HTTP requests |
| `Steeltoe.Security.Authorization.Certificate.CertificateHttpClientBuilderExtensions.AddClientCertificate` | Extension method | Steeltoe.Security.Authorization.Certificate | Added | | Send custom certificate with outgoing HTTP requests |
| `Steeltoe.Security.Authorization.Certificate.SameOrgRequirement` | Type | Steeltoe.Security.Authorization.Certificate | Added | | Authorization requirement for same-org |
| `Steeltoe.Security.Authorization.Certificate.SameSpaceRequirement` | Type | Steeltoe.Security.Authorization.Certificate | Added | | Authorization requirement for same-space |
| `Steeltoe.Security.DataProtection.Redis.CloudFoundryRedisXmlRepository` | Type | Steeltoe.Security.DataProtection.RedisCore | Removed | | No longer needed |
| `Steeltoe.Security.DataProtection.Redis.RedisDataProtectionBuilderExtensions.PersistKeysToRedis` | Extension method | Steeltoe.Security.DataProtection.Redis | Added | | Takes an optional service binding name |
| `Steeltoe.Security.DataProtection.RedisDataProtectionBuilderExtensions.PersistKeysToRedis` | Extension method | Steeltoe.Security.DataProtection.RedisCore | Moved | `PersistKeysToRedis()` in Steeltoe.Security.DataProtection.Redis package | |

### Notable PRs

- https://github.com/SteeltoeOSS/Steeltoe/pull/1525
- https://github.com/SteeltoeOSS/Steeltoe/pull/1452
- https://github.com/SteeltoeOSS/Steeltoe/pull/1349
- https://github.com/SteeltoeOSS/Steeltoe/pull/1336
- https://github.com/SteeltoeOSS/Steeltoe/pull/1311
- https://github.com/SteeltoeOSS/Steeltoe/pull/1306
- https://github.com/SteeltoeOSS/Steeltoe/pull/1232
- https://github.com/SteeltoeOSS/Steeltoe/pull/1098

### Documentation

For additional information, see the updated [Security documentation](../security/index.md) and
[Discovery samples](https://github.com/SteeltoeOSS/Samples/tree/main/Security).

## Release Notes

Release notes for all releases can be found on the [Steeltoe releases](https://github.com/SteeltoeOSS/Steeltoe/releases) section on GitHub.
