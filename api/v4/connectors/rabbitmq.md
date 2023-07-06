# RabbitMQ

This connector simplifies using the [RabbitMQ Client](https://www.rabbitmq.com/tutorials/tutorial-one-dotnet.html) in an application running on Cloud Foundry. We recommend following that tutorial, because you need to know how to use it before proceeding to use the connector.

This connector provides an `IHealthContributor`, which you can use in conjunction with the [Steeltoe Management Health](../management/health.md) check endpoint.

## Usage

You should know how the .NET [configuration service](https://docs.microsoft.com/aspnet/core/fundamentals/configuration) works before starting to use the connector. To configure the connector, you need a basic understanding of `ConfigurationBuilder` and how to add providers to the builder.

You should also know how the ASP.NET Core [Startup](https://docs.microsoft.com/aspnet/core/fundamentals/startup) class is used in configuring the application services for the application. Pay particular attention to the usage of the `ConfigureServices()` method.

You also need some understanding of how to use the [RabbitMQ Client](https://www.rabbitmq.com/tutorials/tutorial-one-dotnet.html) before starting to use this connector.

To use this Connector:

1. Create and bind a RabbitMQ service instance to your application.
1. (Optionally) Configure any RabbitMQ client settings (such as in `appsettings.json`).
1. Add the Steeltoe Cloud Foundry config provider to your `ConfigurationBuilder`.
1. Add the RabbitMQ `ConnectionFactory` to your `ServiceCollection`.

### Add NuGet Reference

To use the RabbitMQ connector, you need to [add a reference to the appropriate Steeltoe Connector NuGet package](usage.md#add-nuget-references) and `RabbitMQ.Client`.

### Configure Settings

The connector supports several settings for the RabbitMQ `ConnectionFactory` that can be useful when you are developing and testing an application locally and you need to have the connector configure the connection for non-default settings.

The following example of the connectors configuration in JSON shows how to setup a connection to a RabbitMQ server at `amqp://guest:guest@127.0.0.1/`:

```json
{
  ...
  "RabbitMq": {
    "Client": {
      "Uri": "amqp://guest:guest@127.0.0.1/"
    }
  }
  ...
}
```

The following table describes all the possible settings for the connector:

| Key | Description | Default |
| --- | --- | --- |
| `Server` | Hostname or IP Address of the server. | 127.0.0.1 |
| `Port` | Port number of the server. | 5672 |
| `Username` | Username for authentication. | not set |
| `Password` | Password for authentication. | not set |
| `VirtualHost` | Virtual host to which to connect. | not set |
| `SslEnabled` | Should SSL be enabled. | `false` |
| `SslPort` | SSL Port number of server. | 5671 |
| `Uri` | Full connection string. | Built from settings |

>IMPORTANT: All of these settings should be prefixed with `RabbitMq:Client:`.

The samples and most templates are already set up to read from `appsettings.json`.

### Cloud Foundry

To use RabbitMQ on Cloud Foundry, you can create and bind an instance to your application using the Cloud Foundry CLI, as follows:

```bash
# Create RabbitMQ service
cf create-service p-rabbitmq standard myRabbitMQService

# Bind the service to `myApp`
cf bind-service myApp myRabbitMQService

# Restage the app to pick up changes
cf restage myApp
```

>The preceding commands assume you use the RabbitMQ service provided by TAS. If you use a different service, adjust the `create-service` command to fit your environment.

Once the service is bound to your application, the connector's settings are available in `VCAP_SERVICES`.

### Add RabbitMQ ConnectionFactory

To use a RabbitMQ `ConnectionFactory` in your application, add it to the service container in the `ConfigureServices()` method of the `Startup` class:

```csharp
using Steeltoe.Connector.RabbitMQ;

public class Startup {
    ...
    public IConfiguration Configuration { get; private set; }
    public Startup(...)
    {
      ...
    }
    public void ConfigureServices(IServiceCollection services)
    {
        // Add RabbitMQ ConnectionFactory
        services.AddRabbitMQConnection(Configuration);

        // Add framework services.
        services.AddMvc();
        ...
    }
    ...
}
```

### Use RabbitMQ ConnectionFactory

Once you have configured and added the RabbitMQ `ConnectionFactory` to the service container, you can inject it and use it in a controller or a view:

```csharp
using RabbitMQ.Client;
...
public class HomeController : Controller
{
    ...
    public IActionResult RabbitMQData([FromServices] ConnectionFactory factory)
    {

        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            CreateQueue(channel);
            var body = Encoding.UTF8.GetBytes("a message");
            channel.BasicPublish(exchange: "",
                                  routingKey: "a-topic",
                                  basicProperties: null,
                                  body: body);

        }
        return View();
    }

}
```
