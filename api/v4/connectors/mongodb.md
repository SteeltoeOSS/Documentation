# MongoDB

This connector simplifies accessing [MongoDB](https://www.mongodb.com/) databases.
It supports the following .NET drivers:
- [MongoDB.Driver](https://www.nuget.org/packages/MongoDB.Driver), which provides an `IMongoClient`.

The remainder of this page assumes you're familiar with the [basic concepts of Steeltoe Connectors](./usage.md).

## Usage

To use this connector:

1. Create a MongoDB server instance or use a [docker container](https://github.com/SteeltoeOSS/Samples/blob/main/CommonTasks.md#mongodb).
1. Add NuGet references to your project.
1. Configure your connection string in `appsettings.json`.
1. Initialize the Steeltoe Connector at startup.
1. Use the driver-specific connection/client instance.

### Add NuGet References

To use this connector, add a NuGet reference to `Steeltoe.Connectors`.

Also add a NuGet reference to one of the .NET drivers listed above, as you would if you were not using Steeltoe.

### Configure connection string

The available connection string parameters for MongoDB are documented [here](https://www.mongodb.com/docs/manual/reference/connection-string/).

The following example `appsettings.json` uses the docker container from above:

```json
{
  "Steeltoe": {
    "Client": {
      "MongoDb": {
        "Default": {
          "ConnectionString": "mongodb://localhost:27017",
          "Database": "TestCollection"
        }
      }
    }
  }
}
```

Notice this configuration file contains the database name, in addition to the connection string. This value is exposed
as `MongoDbOptions.Database`.

### Initialize Steeltoe Connector

Update your `Program.cs` as below to initialize the Connector:

```csharp
using Steeltoe.Connectors.MongoDb;

var builder = WebApplication.CreateBuilder(args);
builder.AddMongoDb();
```

### Use IMongoClient

Start by defining a class that contains collection data:
```csharp
using MongoDB.Bson;

public class SampleObject
{
    public ObjectId Id { get; set; }
    public string? Text { get; set; }
}
```

To obtain an `IMongoClient` instance in your application, inject the Steeltoe factory in a controller or view:

```csharp
using Microsoft.AspNetCore.Mvc;
using MongoDb.Data;
using MongoDB.Driver;
using Steeltoe.Connectors;
using Steeltoe.Connectors.MongoDb;

public class HomeController : Controller
{
    public async Task<IActionResult> Index(
        [FromServices] ConnectorFactory<MongoDbOptions, IMongoClient> connectorFactory)
    {
        var connector = connectorFactory.Get();
        IMongoClient client = connector.GetConnection();

        IMongoDatabase database = client.GetDatabase(connector.Options.Database);
        IMongoCollection<SampleObject> collection = database.GetCollection<SampleObject>("SampleObjects");
        List<SampleObject> sampleObjects = await collection.Find(obj => true).ToListAsync();

        return View(sampleObjects);
    }
}
```

A complete sample app that uses `IMongoClient` is provided at https://github.com/SteeltoeOSS/Samples/tree/main/Connectors/src/MongoDb.

## Cloud Foundry

This Connector supports the following service brokers:
- [VMware Tanzu Cloud Service Broker for Azure](https://docs.vmware.com/en/Tanzu-Cloud-Service-Broker-for-Azure/1.4/csb-azure/GUID-index.html)

You can create and bind an instance to your application by using the Cloud Foundry CLI:

```bash
# Create MongoDB service
cf create-service csb-azure-mongodb small myMongoDbService

# Bind service to your app
cf bind-service myApp myMongoDbService

# Restage the app to pick up change
cf restage myApp
```

## Kubernetes

This Connector supports the [Service Binding Specification for Kubernetes](https://github.com/servicebinding/spec).
It can be used through the Bitnami [Services Toolkit](https://docs.vmware.com/en/VMware-Tanzu-Application-Platform/1.5/tap/services-toolkit-install-services-toolkit.html).

For details on how to use this, see the instructions at https://github.com/SteeltoeOSS/Samples/tree/main/Connectors/src/MongoDb#running-on-tanzu-application-platform-tap.
