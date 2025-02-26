# RabbitMQ

This connector simplifies accessing [RabbitMQ](https://www.rabbitmq.com/) message brokers.
It supports the following .NET drivers:
- [RabbitMQ.Client](https://www.nuget.org/packages/RabbitMQ.Client), which provides an `IConnection`.

The remainder of this page assumes you're familiar with the [basic concepts of Steeltoe Connectors](./usage.md).

## Usage

To use this connector:

1. Create a RabbitMQ server instance or use a [docker container](https://github.com/SteeltoeOSS/Samples/blob/main/CommonTasks.md#rabbitmq).
1. Add NuGet references to your project.
1. Configure your connection string in `appsettings.json` (optional).
1. Initialize the Steeltoe Connector at startup.
1. Use the driver-specific connection/client instance.

### Add NuGet References

To use this connector, add a NuGet reference to `Steeltoe.Connectors`.

Also add a NuGet reference to one of the .NET drivers listed above, as you would if you were not using Steeltoe.

### Configure connection string

The available connection string parameters for RabbitMQ are documented [here](https://www.rabbitmq.com/uri-spec.html).

The following example `appsettings.json` uses the docker container from above:

```json
{
  "Steeltoe": {
    "Client": {
      "RabbitMQ": {
        "Default": {
          "ConnectionString": "amqp://localhost"
        }
      }
    }
  }
}
```

### Initialize Steeltoe Connector

Update your `Program.cs` as below to initialize the Connector:

```csharp
using Steeltoe.Connectors.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);
builder.AddRabbitMQ();
```

### Use IConnection

To obtain an `IConnection` instance in your application, inject the Steeltoe factory in a controller or view:

```csharp
using System.Text;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using Steeltoe.Connectors;
using Steeltoe.Connectors.RabbitMQ;

public class HomeController : Controller
{
    public IActionResult Index(
        [FromServices] ConnectorFactory<RabbitMQOptions, IConnection> connectorFactory)
    {
        var connector = connectorFactory.Get();
        IConnection connection = connector.GetConnection();

        using IModel channel = connection.CreateModel();
        BasicGetResult? result = channel.BasicGet("ExampleQueue", true);
        string? message = result != null ? Encoding.UTF8.GetString(result.Body.ToArray()) : null;

        ViewData["Result"] = message;
        return View();
    }
}
```

A complete sample app that uses `IConnection` is provided at https://github.com/SteeltoeOSS/Samples/tree/main/Connectors/src/RabbitMQ.

## Cloud Foundry

This Connector supports the following service brokers:
- [VMware RabbitMQ for Tanzu Application Service](https://docs.vmware.com/en/VMware-RabbitMQ-for-Tanzu-Application-Service/2.2/tanzu-rmq/GUID-index.html)

You can create and bind an instance to your application by using the Cloud Foundry CLI:

```bash
# Create RabbitMQ service
cf create-service p.rabbitmq single-node myRabbitMQService

# Bind service to your app
cf bind-service myApp myRabbitMQService

# Restage the app to pick up change
cf restage myApp
```

## Kubernetes

This Connector supports the [Service Binding Specification for Kubernetes](https://github.com/servicebinding/spec).
It can be used through the Bitnami [Services Toolkit](https://docs.vmware.com/en/VMware-Tanzu-Application-Platform/1.5/tap/services-toolkit-install-services-toolkit.html).

For details on how to use this, see the instructions at https://github.com/SteeltoeOSS/Samples/tree/main/Connectors/src/RabbitMQ#running-on-tanzu-application-platform-tap.
