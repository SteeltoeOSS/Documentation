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

## Configuration

For additional information, see the updated [Configuration documentation](../configuration/index.md) and
[Configuration samples](https://github.com/SteeltoeOSS/Samples/tree/4.x/Configuration).

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
[Configuration samples](https://github.com/SteeltoeOSS/Samples/tree/4.x/Connectors).

> [!IMPORTANT]
> The configuration structure for Connectors has changed in Steeltoe 4. Always use the `ConnectionString` property instead of `Host`, `Port`, `Username`, `Password`, etc.
> Replace the key `Default` with the name of the service binding if you have multiple.

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
-  "MySql": {
-    "Client": {
-      "ConnectionString": "Server=localhost;Database=steeltoe;Uid=steeltoe;Pwd=steeltoe"
-    }
-  }
+  "Steeltoe": {
+    "Client": {
+      "MySql": {
+        "Default": {
+          "ConnectionString": "Server=localhost;Database=steeltoe;Uid=steeltoe;Pwd=steeltoe"
+        }
+      }
+    }
+  }
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
-  "MySql": {
-    "Client": {
-      "ConnectionString": "Server=localhost;Database=steeltoe;Uid=steeltoe;Pwd=steeltoe"
-    }
-  }
+  "Steeltoe": {
+    "Client": {
+      "MySql": {
+        "Default": {
+          "ConnectionString": "Server=localhost;Database=steeltoe;Uid=steeltoe;Pwd=steeltoe"
+        }
+      }
+    }
+  }
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
-  "Postgres": {
-    "Client": {
-      "ConnectionString": "Server=localhost;Database=steeltoe;Uid=steeltoe;Pwd=steeltoe"
-    }
-  }
+  "Steeltoe": {
+    "Client": {
+      "PostgreSql": {
+        "Default": {
+          "ConnectionString": "Server=localhost;Database=steeltoe;Uid=steeltoe;Pwd=steeltoe"
+        }
+      }
+    }
+  }
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
-  "Postgres": {
-    "Client": {
-      "ConnectionString": "Server=localhost;Database=steeltoe;Uid=steeltoe;Pwd=steeltoe"
-    }
-  }
+  "Steeltoe": {
+    "Client": {
+      "PostgreSql": {
+        "Default": {
+          "ConnectionString": "Server=localhost;Database=steeltoe;Uid=steeltoe;Pwd=steeltoe"
+        }
+      }
+    }
+  }
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
-  "Rabbitmq": {
-    "Client": {
-      "Uri": "amqp://guest:guest@127.0.0.1/"
-    }
-  }
+  "Steeltoe": {
+    "Client": {
+      "RabbitMQ": {
+        "Default": {
+          "ConnectionString": "amqp://localhost:5672"
+        }
+      }
+    }
+  }
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
-var connection = await connectionFactory.CreateConnectionAsync(); // long-lived, do not dispose
+var factory = app.Services.GetRequiredService<ConnectorFactory<RabbitMQOptions, IConnection>>();
+var connector = factory.Get();
+Console.WriteLine($"Using connection string: {connector.Options.ConnectionString}");
+var connection = connector.GetConnection(); // long-lived, do not dispose
await using var channel = await connection.CreateChannelAsync();
const string queueName = "example-queue-name";
await channel.QueueDeclareAsync(queueName);

byte[] messageToSend = "example-message"u8.ToArray();
await channel.BasicPublishAsync(exchange: "", queueName, mandatory: true, new BasicProperties(), messageToSend);

var result = await channel.BasicGetAsync(queueName, autoAck: true);
string messageReceived = result == null ? "(none)" : Encoding.UTF8.GetString(result.Body.ToArray());
Console.WriteLine($"Received message: {messageReceived}");
```

### Redis/Valkey

Project file:

```diff
<Project>
  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="9.0.8" />
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
-  "Redis": {
-    "Client": {
-      "ConnectionString": "localhost:6379"
-    }
-  }
+  "Steeltoe": {
+    "Client": {
+      "Redis": {
+        "Default": {
+          "ConnectionString": "localhost"
+        }
+      }
+    }
+  }
}
```

Program.cs:

```diff
using Microsoft.Extensions.Caching.Distributed;
-using Steeltoe.Connector.Redis;
+using Steeltoe.Connectors;
+using Steeltoe.Connectors.Redis;

var builder = WebApplication.CreateBuilder(args);
-builder.Services.AddDistributedRedisCache(builder.Configuration);
+builder.AddRedis();

var app = builder.Build();

-var cache = app.Services.GetRequiredService<IDistributedCache>();
+var factory = app.Services.GetRequiredService<ConnectorFactory<RedisOptions, IDistributedCache>>();
+var connector = factory.Get();
+Console.WriteLine($"Using connection string: {connector.Options.ConnectionString}");
+var cache = connector.GetConnection();
await cache.SetAsync("example-key", "example-value"u8.ToArray());
var value = await cache.GetStringAsync("example-key");
Console.WriteLine($"Received value: {value}");
```

## Discovery

For additional information, see the updated [Discovery documentation](../discovery/index.md) and
[Discovery samples](https://github.com/SteeltoeOSS/Samples/tree/4.x/Discovery).

### Eureka

#### Register your service

Project file:

```diff
<Project>
  <ItemGroup>
-    <PackageReference Include="Steeltoe.Discovery.Eureka" Version="3.*" />
+    <PackageReference Include="Steeltoe.Discovery.Eureka" Version="4.0.0" />
  </ItemGroup>
</Project>
```

appsettings.json:

```diff
{
-  "$schema": "https://steeltoe.io/schema/v3/schema.json",
+  "$schema": "https://steeltoe.io/schema/v4/schema.json",
  "Spring": {
    "Application": {
      "Name": "example-service"
    }
  },
  "Eureka": {
    "Client": {
      "ShouldRegisterWithEureka": true,
      "ShouldFetchRegistry": false
    }
  }
}
```

launchSettings.json:

```diff
{
  "profiles": {
    "http": {
      "commandName": "Project",
      "applicationUrl": "http://+:5005" // bind to all host names and IP addresses
    }
  }
}
```

Program.cs:

```diff
-using Steeltoe.Discovery.Client;
+using Steeltoe.Discovery.Eureka;

var builder = WebApplication.CreateBuilder(args);
-builder.Services.AddDiscoveryClient();
+builder.Services.AddEurekaDiscoveryClient();

var app = builder.Build();

app.MapGet("/ping", async httpContext =>
{
    httpContext.Response.StatusCode = 200;
    httpContext.Response.ContentType = "text/plain";
    await httpContext.Response.WriteAsync("pong");
});
```

#### Lookup other services

Project file:

```diff
<Project>
  <ItemGroup>
-    <PackageReference Include="Steeltoe.Discovery.Eureka" Version="3.*" />
+    <PackageReference Include="Steeltoe.Discovery.Eureka" Version="4.0.0" />
+    <PackageReference Include="Steeltoe.Discovery.HttpClients" Version="4.0.0" />
  </ItemGroup>
</Project>
```

appsettings.json:

```diff
{
-  "$schema": "https://steeltoe.io/schema/v3/schema.json",
+  "$schema": "https://steeltoe.io/schema/v4/schema.json",
  "Spring": {
    "Application": {
      "Name": "example-service"
    }
  },
  "Eureka": {
    "Client": {
      "ShouldRegisterWithEureka": false,
      "ShouldFetchRegistry": true
    }
  }
}
```

Program.cs:

```diff
-using Steeltoe.Common.Http.Discovery;
-using Steeltoe.Discovery.Client;
+using Steeltoe.Discovery.Eureka;
+using Steeltoe.Discovery.HttpClients;

var builder = WebApplication.CreateBuilder(args);
-builder.Services.AddDiscoveryClient();
+builder.Services.AddEurekaDiscoveryClient();
builder.Services
    .AddHttpClient<PingClient>(httpClient => httpClient.BaseAddress = new Uri("http://example-service/"))
    .AddServiceDiscovery();

var app = builder.Build();

var pingClient = app.Services.GetRequiredService<PingClient>();
string response = await pingClient.GetPingAsync();
Console.WriteLine($"Response: {response}");

public class PingClient(HttpClient httpClient)
{
    public async Task<string> GetPingAsync()
    {
        return await httpClient.GetStringAsync("ping");
    }
}
```

### Consul

#### Register your service

Project file:

```diff
<Project>
  <ItemGroup>
-    <PackageReference Include="Steeltoe.Discovery.Consul" Version="3.*" />
+    <PackageReference Include="Steeltoe.Discovery.Consul" Version="4.0.0" />
  </ItemGroup>
</Project>
```

appsettings.json:

```diff
{
-  "$schema": "https://steeltoe.io/schema/v3/schema.json",
+  "$schema": "https://steeltoe.io/schema/v4/schema.json",
  "Spring": {
    "Application": {
      "Name": "example-service"
    }
  },
  "Consul": {
    "Discovery": {
      "Register": true
    }
  }
}
```

launchSettings.json:

```diff
{
  "profiles": {
    "http": {
      "commandName": "Project",
      "applicationUrl": "http://+:5005" // bind to all host names and IP addresses
    }
  }
}
```

Program.cs:

```diff
-using Steeltoe.Discovery.Client;
+using Steeltoe.Discovery.Consul;

var builder = WebApplication.CreateBuilder(args);
-builder.Services.AddDiscoveryClient();
builder.Services.AddConsulDiscoveryClient();

var app = builder.Build();

app.MapGet("/ping", async httpContext =>
{
    httpContext.Response.StatusCode = 200;
    httpContext.Response.ContentType = "text/plain";
    await httpContext.Response.WriteAsync("pong");
});
```

#### Lookup other services

Project file:

```diff
<Project>
  <ItemGroup>
-    <PackageReference Include="Steeltoe.Discovery.Consul" Version="3.*" />
+    <PackageReference Include="Steeltoe.Discovery.Consul" Version="4.0.0" />
+    <PackageReference Include="Steeltoe.Discovery.HttpClients" Version="4.0.0" />
  </ItemGroup>
</Project>
```

appsettings.json:

```diff
{
-  "$schema": "https://steeltoe.io/schema/v3/schema.json",
+  "$schema": "https://steeltoe.io/schema/v4/schema.json",
  "Consul": {
    "Discovery": {
      "Register": false
    }
  }
}
```

Program.cs:

```diff
-using Steeltoe.Common.Http.Discovery;
-using Steeltoe.Discovery.Client;
+using Steeltoe.Discovery.Consul;
+using Steeltoe.Discovery.HttpClients;

var builder = WebApplication.CreateBuilder(args);
-builder.Services.AddDiscoveryClient();
+builder.Services.AddConsulDiscoveryClient();
builder.Services
    .AddHttpClient<PingClient>(httpClient => httpClient.BaseAddress = new Uri("http://example-service/"))
    .AddServiceDiscovery();

var app = builder.Build();

var pingClient = app.Services.GetRequiredService<PingClient>();
string response = await pingClient.GetPingAsync();
Console.WriteLine($"Response: {response}");

public class PingClient(HttpClient httpClient)
{
    public async Task<string> GetPingAsync()
    {
        return await httpClient.GetStringAsync("ping");
    }
}
```

## Integration

Integration (lightweight messaging for Spring-based applications) has been removed from Steeltoe in v4.

## Logging

For additional information, see the updated [Logging documentation](../logging/index.md).

### Dynamic Console

Project file:

```diff
<Project>
  <ItemGroup>
-    <PackageReference Include="Steeltoe.Extensions.Logging.DynamicLogger" Version="3.*" />
+    <PackageReference Include="Steeltoe.logging.DynamicConsole" Version="4.0.0" />
  </ItemGroup>
</Project>
```

Program.cs:

```diff
-using Steeltoe.Extensions.Logging;
+using Steeltoe.Logging;
+using Steeltoe.Logging.DynamicConsole;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.SetMinimumLevel(LogLevel.Debug);
builder.Configuration["Logging:LogLevel:Default"] = "Warning";
builder.Logging.AddDynamicConsole();

var app = builder.Build();

var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
var exampleLogger = loggerFactory.CreateLogger("Example.Sub.Namespace");

exampleLogger.LogDebug("Example debug message (1) - hidden");

var dynamicLoggerProvider = app.Services.GetRequiredService<IDynamicLoggerProvider>();
dynamicLoggerProvider.SetLogLevel("Example", LogLevel.Debug);

exampleLogger.LogDebug("Example debug message (2)");

await Task.Delay(TimeSpan.FromMilliseconds(250)); // wait for logs to flush
```

## Management

For additional information, see the updated [Management documentation](../management/index.md) and
[Management samples](https://github.com/SteeltoeOSS/Samples/tree/4.x/Management).

### Endpoints

Project file:

```diff
<Project>
  <ItemGroup>
-    <PackageReference Include="Steeltoe.Management.EndpointCore" Version="3.*" />
+    <PackageReference Include="Steeltoe.Management.Endpoint" Version="4.0.0" />
  </ItemGroup>
</Project>
```

#### All actuators

appsettings.json:

```diff
{
-  "$schema": "https://steeltoe.io/schema/v3/schema.json",
+  "$schema": "https://steeltoe.io/schema/v4/schema.json",
  "Management": {
    "Endpoints": {
      "Actuator": {
        "Exposure": {
          "Include": [ "*" ]
        }
+      },
+      "Health": {
+        "ShowComponents": "Always",
+        "ShowDetails": "Always",
+        "Readiness": {
+          "Enabled": true
+        },
+        "Liveness": {
+          "Enabled": true
+        }
      }
    }
  }
}
```

Program.cs:

```diff
-using Steeltoe.Management.Endpoint;
+using Steeltoe.Management.Endpoint.Actuators.All;

var builder = WebApplication.CreateBuilder(args);
-builder.AddAllActuators();
+builder.Services.AddAllActuators();
```

#### Custom health contributor

appsettings.json:

```diff
{
-  "$schema": "https://steeltoe.io/schema/v3/schema.json",
+  "$schema": "https://steeltoe.io/schema/v4/schema.json",
+  "Management": {
+    "Endpoints": {
+      "Health": {
+        "ShowComponents": "Always",
+        "ShowDetails": "Always",
+        "Readiness": {
+          "Enabled": true
+        },
+        "Liveness": {
+          "Enabled": true
+        }
+      }
+    }
+  }
}
```

Program.cs:

```diff
using Steeltoe.Common.HealthChecks;
-using Steeltoe.Management.Endpoint;
+using Steeltoe.Management.Endpoint.Actuators.Health;

var builder = WebApplication.CreateBuilder(args);

-builder.AddHealthActuator();
+builder.Services.AddHealthActuator();
-builder.Services.AddSingleton<IHealthContributor, WarningHealthContributor>();
+builder.Services.AddHealthContributor<WarningHealthContributor>();
-builder.Services.AddControllers();

var app = builder.Build();
-app.MapControllers();
app.Run();

public class WarningHealthContributor : IHealthContributor
{
    public string Id => "exampleContributor";

-    public HealthCheckResult Health()
+    public async Task<HealthCheckResult?> CheckHealthAsync(CancellationToken cancellationToken)
    {
+        await Task.Yield();
        return new HealthCheckResult
        {
-            Status = HealthStatus.WARNING,
+            Status = HealthStatus.Warning,
+            Description = "Example health contributor reports warning.",
            Details =
            {
-                ["status"] = HealthStatus.WARNING,
-                ["description"] = "Example health contributor reports warning.",
                ["currentTime"] = DateTime.UtcNow.ToString("O")
            }
        };
    }
}
```

#### Custom info contributor

Program.cs:

```diff
-using Steeltoe.Management.Endpoint;
-using Steeltoe.Management.Info;
+using Steeltoe.Management.Endpoint.Actuators.Info;

var builder = WebApplication.CreateBuilder(args);
-builder.AddInfoActuator();
+builder.Services.AddInfoActuator();
-builder.Services.AddSingleton<IInfoContributor, ExampleInfoContributor>();
+builder.Services.AddInfoContributor<ExampleInfoContributor>();
-builder.Services.AddControllers();

var app = builder.Build();
-app.MapControllers();
app.Run();

public class ExampleInfoContributor : IInfoContributor
{
-    public void Contribute(IInfoBuilder builder)
+    public async Task ContributeAsync(InfoBuilder builder, CancellationToken cancellationToken)
    {
+        await Task.Yield();
        builder.WithInfo(".NET version", Environment.Version);
    }
}
```

#### Cloud hosting

The `UseCloudHosting` extension method has been removed from Steeltoe in v4. Use one of the methods described at
[8 ways to set the URLs for an ASP.NET Core app](https://andrewlock.net/8-ways-to-set-the-urls-for-an-aspnetcore-app/)
to configure the port number(s) to listen on.

Program.cs:

```diff
-using Steeltoe.Common.Hosting;
-using Steeltoe.Management.Endpoint;
using Steeltoe.Management.Endpoint.Actuators.All;

var builder = WebApplication.CreateBuilder(args);
-builder.UseCloudHosting(runLocalHttpPort: 8080, runLocalHttpsPort: 9090);
+builder.WebHost.UseUrls("http://+:8080", "https://+:9090");
-builder.AddAllActuators();
builder.Services.AddAllActuators();
```

For deployment to Cloud Foundry, the `builder.WebHost.UseUrls` line should be omitted.

- When using the dotnet_core_buildpack, the `PORT` environment variable is picked up automatically.
- When using the binary_buildpack, use the `PORT` environment variable in the `manifest.yml` file:

   ```yaml
   ---
   applications:
   - name: example-app
     stack: windows
     buildpacks:
      - binary_buildpack
     command: cmd /c ./example-app --urls=http://0.0.0.0:%PORT%
   ```

#### Spring Boot Admin

appsettings.json:

```diff
{
-  "$schema": "https://steeltoe.io/schema/v3/schema.json",
+  "$schema": "https://steeltoe.io/schema/v4/schema.json",
  "Spring": {
    "Application": {
      "Name": "example-service"
    },
    "Boot": {
      "Admin": {
        "Client": {
          "Url": "http://localhost:9099",
-          "BasePath": "http://host.docker.internal:5050"
+          "BaseHost": "host.docker.internal"
        }
      }
    }
  },
  "Management": {
    "Endpoints": {
      "Actuator": {
        "Exposure": {
          "Include": [ "*" ]
        }
      }
    }
  }
}
```

Program.cs:

```diff
-using Steeltoe.Management.Endpoint;
+using Steeltoe.Management.Endpoint.Actuators.All;
+using Steeltoe.Management.Endpoint.SpringBootAdminClient;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://host.docker.internal:5050");
-builder.AddAllActuators();
+builder.Services.AddAllActuators();
builder.Services.AddSpringBootAdminClient();
```

### OpenTelemetry

Using OpenTelemetry for collecting logs, metrics and distributed traces now works out of the box without requiring a Steeltoe NuGet package.
See the instructions [here](../tracing/index.md) to configure OpenTelemetry in your application.
See [here](../management/prometheus.md) to export metrics to Prometheus using Steeltoe v4.

The sample [here](https://github.com/SteeltoeOSS/Samples/blob/4.x/Management/src/ActuatorWeb/README.md#viewing-metric-dashboards) demonstrates exporting to Prometheus and Grafana.

### Kubernetes

Direct interaction with the Kubernetes API has been removed from Steeltoe in v4.

### Application tasks

Project file:

```diff
<Project>
  <ItemGroup>
-    <PackageReference Include="Steeltoe.Management.TaskCore" Version="3.*" />
+    <PackageReference Include="Steeltoe.Management.Tasks" Version="4.0.0" />
  </ItemGroup>
</Project>
```

After the following steps, run your app:

```shell
dotnet run runtask=example-task
```

#### Using inline code

Program.cs:

```diff
-using Steeltoe.Management.TaskCore;
+using Steeltoe.Management.Tasks;

var builder = WebApplication.CreateBuilder(args);
-builder.Services.AddTask("example-task", serviceProvider =>
+builder.Services.AddTask("example-task", async (serviceProvider, cancellationToken) =>
{
+    await Task.Yield();
    var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger("ExampleTaskLogger");
    logger.LogInformation("Example task executed.");
});

var app = builder.Build();
-app.RunWithTasks();
+await app.RunWithTasksAsync(CancellationToken.None);
```

#### Implementing `IApplicationTask`

Program.cs:

```diff
using Steeltoe.Common;
-using Steeltoe.Management.TaskCore;
+using Steeltoe.Management.Tasks;

var builder = WebApplication.CreateBuilder(args);
-builder.Services.AddTask<ExampleTask>();
+builder.Services.AddTask<ExampleTask>("example-task");

var app = builder.Build();
-app.RunWithTasks();
+await app.RunWithTasksAsync(CancellationToken.None);

public class ExampleTask(ILogger<ExampleTask> logger) : IApplicationTask
{
-    public string Name => "example-task";

-    public void Run()
+    public Task RunAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Example task executed.");
+        return Task.CompletedTask;
    }
}
```

## Messaging

Template-based support for Spring messaging systems has been removed from Steeltoe in v4.

## Stream

Spring Cloud Stream support has been removed from Steeltoe in v4.

## Security

For additional information, see the updated [Security documentation](../security/index.md) and
[Discovery samples](https://github.com/SteeltoeOSS/Samples/tree/4.x/Security).

### CredHub client

The CredHub client has been removed from Steeltoe in v4.
Use [CredHub Service Broker](https://techdocs.broadcom.com/us/en/vmware-tanzu/platform-services/credhub-service-broker/services/credhub-sb/index.html) instead.

### OAuth

OAuth support has been removed from Steeltoe in v4. Use OpenID Connect instead.

Before migrating to Steeltoe v4, apply the following changes to migrate from OAuth to OpenID Connect using Steeltoe v3.

appsettings.json:

```diff
{
  "$schema": "https://steeltoe.io/schema/v3/schema.json",
  "Security": {
    "Oauth2": {
      "Client": {
-        "AuthDomain": "http://localhost:8080",
+        "Authority": "http://localhost:8080",
+        "MetadataAddress": "http://localhost:8080/.well-known/openid-configuration",
+        "RequireHttpsMetadata": false,
+        "AdditionalScopes": "sampleapi.read",
        "CallbackPath": "/signin-oidc",
        "ClientId": "steeltoesamplesclient",
        "ClientSecret": "client_secret"
      }
    }
  }
}
```

> [!NOTE]
> Depending on your application's needs, you may need to add scopes to the application's configuration that did not previously need to be specified.

program.cs:

```diff
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using Steeltoe.Security.Authentication.CloudFoundry;

var builder = WebApplication.CreateBuilder(args);
builder.AddCloudFoundryConfiguration();
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = CloudFoundryDefaults.AuthenticationScheme;
    })
    .AddCookie(options => options.AccessDeniedPath = new PathString("/Home/AccessDenied"))
-    .AddCloudFoundryOAuth(builder.Configuration);
+    .AddCloudFoundryOpenIdConnect(builder.Configuration);
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("read", policy => policy.RequireClaim("scope", "sampleapi.read"));

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto
});

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/test-auth", async httpContext =>
    {
        httpContext.Response.StatusCode = 200;
        httpContext.Response.ContentType = "text/plain";
        await httpContext.Response.WriteAsync("You are logged in and carry the required claim.");
    }).RequireAuthorization("read");

app.Run();
```

### OpenID Connect

Project file:

```diff
<Project>
  <ItemGroup>
-    <PackageReference Include="Steeltoe.Extensions.Configuration.CloudFoundryCore" Version="3.*" />
+    <PackageReference Include="Steeltoe.Configuration.CloudFoundry" Version="4.0.0" />
-    <PackageReference Include="Steeltoe.Security.Authentication.CloudFoundryCore" Version="3.*" />
+    <PackageReference Include="Steeltoe.Security.Authentication.OpenIdConnect" Version="4.0.0" />
  </ItemGroup>
</Project>
```

appsettings.json:

```diff
{
-  "$schema": "https://steeltoe.io/schema/v3/schema.json",
+  "$schema": "https://steeltoe.io/schema/v4/schema.json",
-  "Security": {
-    "Oauth2": {
-      "Client": {
-        "Authority": "http://localhost:8080",
-        "MetadataAddress": "http://localhost:8080/.well-known/openid-configuration",
-        "RequireHttpsMetadata": false,
-        "AdditionalScopes": "sampleapi.read",
-        "CallbackPath": "/signin-oidc",
-        "ClientId": "steeltoesamplesclient",
-        "ClientSecret": "client_secret"
-      }
-    }
-  }
+  "Authentication": {
+    "Schemes": {
+      "OpenIdConnect": {
+        "Authority": "http://localhost:8080",
+        "MetadataAddress": "http://localhost:8080/.well-known/openid-configuration",
+        "RequireHttpsMetadata": false,
+        "Scope": [ "openid", "sampleapi.read" ],
+        "CallbackPath": "/signin-oidc",
+        "ClientId": "steeltoesamplesclient",
+        "ClientSecret": "client_secret"
+      }
+    }
+  }
}
```

> [!NOTE]
> This is not a complete listing of appsettings. As of version 4, Steeltoe configures Microsoft's option class rather than maintaining separate options.
> Refer to [the OpenIdConnectOptions class documentation](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.authentication.openidconnect.openidconnectoptions) for the new options.

Program.cs:

```diff
using Microsoft.AspNetCore.Authentication.Cookies;
-using Microsoft.AspNetCore.HttpOverrides;
-using Steeltoe.Extensions.Configuration.CloudFoundry;
+using Steeltoe.Configuration.CloudFoundry;
+using Steeltoe.Configuration.CloudFoundry.ServiceBindings;
-using Steeltoe.Security.Authentication.CloudFoundry;
+using Microsoft.AspNetCore.Authentication.OpenIdConnect;
+using Steeltoe.Security.Authentication.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);
builder.AddCloudFoundryConfiguration();
+builder.Configuration.AddCloudFoundryServiceBindings();
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
-        options.DefaultChallengeScheme = CloudFoundryDefaults.AuthenticationScheme;
+        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(options => options.AccessDeniedPath = new PathString("/Home/AccessDenied"))
-    .AddCloudFoundryOpenIdConnect(builder.Configuration);
+    .AddOpenIdConnect().ConfigureOpenIdConnectForCloudFoundry();
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("read", policy => policy.RequireClaim("scope", "sampleapi.read"));

var app = builder.Build();

-app.UseForwardedHeaders(new ForwardedHeadersOptions
-{
-    ForwardedHeaders = ForwardedHeaders.XForwardedProto
-});

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/test-auth", async httpContext =>
    {
        httpContext.Response.StatusCode = 200;
        httpContext.Response.ContentType = "text/plain";
        await httpContext.Response.WriteAsync("You are logged in and carry the required claim.");
    }).RequireAuthorization("read");

app.Run();    
```

### JWT Bearer

Project file:

```diff
<Project>
  <ItemGroup>
-    <PackageReference Include="Steeltoe.Extensions.Configuration.CloudFoundryCore" Version="3.*" />
+    <PackageReference Include="Steeltoe.Configuration.CloudFoundry" Version="4.0.0" />
-    <PackageReference Include="Steeltoe.Security.Authentication.CloudFoundryCore" Version="3.*" />
+    <PackageReference Include="Steeltoe.Security.Authentication.JwtBearer" Version="4.0.0" />
  </ItemGroup>
</Project>
```

appsettings.json:

```diff
{
-  "$schema": "https://steeltoe.io/schema/v3/schema.json",
+  "$schema": "https://steeltoe.io/schema/v4/schema.json",
-  "Security": {
-    "Oauth2": {
-      "Client": {
-        "AuthDomain": "http://localhost:8080",
-        "MetadataAddress": "http://localhost:8080/.well-known/openid-configuration",
-        "RequireHttpsMetadata": false,
-        "ClientId": "steeltoesamplesserver",
-        "ClientSecret": "server_secret"
-      }
-    }
-  }
+  "Authentication": {
+    "Schemes": {
+      "Bearer": {
+        "Authority": "http://localhost:8080",
+        "MetadataAddress": "http://localhost:8080/.well-known/openid-configuration",
+        "RequireHttpsMetadata": false,
+        "ClientId": "steeltoesamplesserver",
+        "ClientSecret": "server_secret",
+        "ValidAudiences": [ "sampleapi" ]
+      }
+    }
+  }
}
```

> [!NOTE]
> This is not a complete listing of appsettings. As of version 4, Steeltoe configures Microsoft's option class rather than maintaining separate options.
> Refer to [the JwtBearerOptions class documentation](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.authentication.jwtbearer.jwtbeareroptions) for the new options.

Program.cs:

```diff
-using Microsoft.AspNetCore.HttpOverrides;
-using Steeltoe.Extensions.Configuration.CloudFoundry;
+using Steeltoe.Configuration.CloudFoundry;
+using Steeltoe.Configuration.CloudFoundry.ServiceBindings;
-using Steeltoe.Security.Authentication.CloudFoundry;
+using Steeltoe.Security.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

builder.AddCloudFoundryConfiguration();
+builder.Configuration.AddCloudFoundryServiceBindings();
builder.Services.AddAuthentication()
-    .AddCloudFoundryJwtBearer(builder.Configuration);
+    .AddJwtBearer().ConfigureJwtBearerForCloudFoundry();
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("sampleapi.read", policy => policy.RequireClaim("scope", "sampleapi.read"));

var app = builder.Build();

-app.UseForwardedHeaders(new ForwardedHeadersOptions
-{
-    ForwardedHeaders = ForwardedHeaders.XForwardedProto
-});

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/test-jwt", async httpContext =>
    {
        httpContext.Response.StatusCode = 200;
        httpContext.Response.ContentType = "text/plain";
        await httpContext.Response.WriteAsync("JWT is valid and contains the required claim.");
    }).RequireAuthorization("sampleapi.read");

app.Run();
```

### Client Certificates (Mutual TLS)

Project file:

```diff
<Project>
  <ItemGroup>
-    <PackageReference Include="Steeltoe.Security.Authentication.CloudFoundryCore" Version="3.*" />
+    <PackageReference Include="Steeltoe.Security.Authorization.Certificate" Version="4.0.0" />
  </ItemGroup>
</Project>
```

launchsettings.json (server-side):

```diff
{
  "profiles": {
    "http": {
      "commandName": "Project",
      "applicationUrl": "https://localhost:7107"
    }
  }
}
```

Program.cs (server-side):

```diff
+using Steeltoe.Common.Certificates;
-using Steeltoe.Security.Authentication.CloudFoundry;
+using Steeltoe.Security.Authorization.Certificate;

const string orgId = "a8fef16f-94c0-49e3-aa0b-ced7c3da6229";
const string spaceId = "122b942a-d7b9-4839-b26e-836654b9785f";

var builder = WebApplication.CreateBuilder(args);
-builder.Configuration.AddCloudFoundryContainerIdentity(orgId, spaceId);
+builder.Configuration.AddAppInstanceIdentityCertificate(new Guid(orgId), new Guid(spaceId));
-builder.Services.AddCloudFoundryCertificateAuth(options => options.CertificateHeader = "X-Forwarded-Client-Cert");
+builder.Services.AddAuthentication().AddCertificate();
+builder.Services.AddAuthorizationBuilder().AddOrgAndSpacePolicies("X-Forwarded-Client-Cert");

var app = builder.Build();

-app.UseCloudFoundryCertificateAuth();
+app.UseCertificateAuthorization();

app.MapGet("/test-same-org", async httpContext =>
    {
        httpContext.Response.StatusCode = 200;
        httpContext.Response.ContentType = "text/plain";
        await httpContext.Response.WriteAsync("Client and server identity certificates have matching Org values.");
    })
-    .RequireAuthorization(CloudFoundryDefaults.SameOrganizationAuthorizationPolicy);
+    .RequireAuthorization(CertificateAuthorizationPolicies.SameOrg);
app.MapGet("/test-same-space", async httpContext =>
    {
        httpContext.Response.StatusCode = 200;
        httpContext.Response.ContentType = "text/plain";
        await httpContext.Response.WriteAsync("Client and server identity certificates have matching Space values.");
    })
-    .RequireAuthorization(CloudFoundryDefaults.SameSpaceAuthorizationPolicy);
+    .RequireAuthorization(CertificateAuthorizationPolicies.SameSpace);

app.Run();
```

> [!NOTE]
> Prior to Steeltoe 3.3.0, Steeltoe Certificate Auth used the header `X-Forwarded-Client-Cert`, which was not configurable.
> The code shown above is provided for compatibility between the versions. The preferred header name is `X-Client-Cert`.
> In Steeltoe 4.0, the default header is `X-Client-Cert`, so the parameter can be omitted if cross-compatibility is not required.

Program.cs (client-side):

```diff
-using System.Security.Cryptography.X509Certificates;
-using Microsoft.Extensions.Options;
-using Steeltoe.Common.Options;
+using Steeltoe.Common.Certificates;
-using Steeltoe.Security.Authentication.CloudFoundry;
+using Steeltoe.Security.Authorization.Certificate;

const string orgId = "a8fef16f-94c0-49e3-aa0b-ced7c3da6229";
const string spaceId = "122b942a-d7b9-4839-b26e-836654b9785f";

var builder = WebApplication.CreateBuilder(args);

-builder.Configuration.AddCloudFoundryContainerIdentity(orgId, spaceId);
+builder.Configuration.AddAppInstanceIdentityCertificate(new Guid(orgId), new Guid(spaceId));
-builder.Services.AddCloudFoundryContainerIdentity();
builder.Services
-    .AddHttpClient<TestClient>((services, client) =>
-    {
-        client.BaseAddress = new Uri("https://localhost:7107");
-        var options = services.GetRequiredService<IOptions<CertificateOptions>>();
-        var b64 = Convert.ToBase64String(options.Value.Certificate.Export(X509ContentType.Cert));
-        client.DefaultRequestHeaders.Add("X-Forwarded-Client-Cert", b64);
-    });
+    .AddHttpClient<TestClient>(httpClient => httpClient.BaseAddress = new Uri("https://localhost:7107"))
+        .AddAppInstanceIdentityCertificate("X-Forwarded-Client-Cert");

var app = builder.Build();

var testClient = app.Services.GetRequiredService<TestClient>();
string orgResponse = await testClient.GetAsync("/test-same-org");
Console.WriteLine($"Org response: {orgResponse}");
string spaceResponse = await testClient.GetAsync("/test-same-space");
Console.WriteLine($"Space response: {spaceResponse}");

public class TestClient(HttpClient httpClient)
{
    public async Task<string> GetAsync(string requestPath)
    {
        return await httpClient.GetStringAsync(requestPath);
    }
}
```

> [!NOTE]
> Prior to Steeltoe 3.3.0, Steeltoe Certificate Auth used the header `X-Forwarded-Client-Cert`, which was not configurable.
> The code shown above is provided for compatibility between the versions. The preferred header name is `X-Client-Cert`.
> In Steeltoe 4.0, the default header is `X-Client-Cert`, so the parameter can be omitted if cross-compatibility is not required.

### DataProtection Key Store using Redis/Valkey

```diff
<Project>
  <ItemGroup>
-    <PackageReference Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="9.0.8" />
-    <PackageReference Include="Steeltoe.Connector.ConnectorCore" Version="3.*" />
-    <PackageReference Include="Steeltoe.Security.DataProtection.RedisCore" Version="3.*" />
+    <PackageReference Include="Steeltoe.Security.DataProtection.Redis" Version="4.0.0" />
  </ItemGroup>
</Project>
```

appsettings.json:

```diff
{
-  "$schema": "https://steeltoe.io/schema/v3/schema.json",
+  "$schema": "https://steeltoe.io/schema/v4/schema.json",
-  "Redis": {
-    "Client": {
-      "ConnectionString": "localhost:6379"
-    }
-  }
+  "Steeltoe": {
+    "Client": {
+      "Redis": {
+        "Default": {
+          "ConnectionString": "localhost"
+        }
+      }
+    }
+  }
}
```

Program.cs:

```diff
using Microsoft.AspNetCore.DataProtection;
-using Steeltoe.Connector.Redis;
+using Steeltoe.Connectors.Redis;
-using Steeltoe.Security.DataProtection;
+using Steeltoe.Security.DataProtection.Redis;

var builder = WebApplication.CreateBuilder(args);
-builder.Services.AddRedisConnectionMultiplexer(builder.Configuration);
-builder.Services.AddDistributedRedisCache(builder.Configuration);
+builder.AddRedis();
builder.Services.AddDataProtection()
    .PersistKeysToRedis()
    .SetApplicationName("example-app");
builder.Services.AddSession();

var app = builder.Build();
app.UseSession();

app.MapPost("set-session", httpContext =>
{
    httpContext.Session.SetString("example-key", $"example-value-{Guid.NewGuid()}");
    httpContext.Response.StatusCode = 204;
    return Task.CompletedTask;
});

app.MapGet("get-session", async httpContext =>
{
    var sessionValue = httpContext.Session.GetString("example-key");
    httpContext.Response.StatusCode = 200;
    httpContext.Response.ContentType = "text/plain";
    await httpContext.Response.WriteAsync($"Session value: {sessionValue ?? "(none)"}");
});

app.Run();
```
