# MongoDB

This connector simplifies using MongoDB with the [.NET MongoDB Driver](https://docs.mongodb.com/ecosystem/drivers/csharp/).

## Usage

To use this connector:

1. Create a MongoDB service instance and bind it to your application.
1. Optionally, configure any MongoDB client settings.
1. Optionally, add the Steeltoe Cloud Foundry config provider to your `ConfigurationBuilder`.
1. Add MongoDB classes to your DI container.

### Add NuGet Reference

To use the MongoDB connector, add the official [MongoDB.Driver NuGet package](https://www.nuget.org/packages/MongoDB.Driver/) as you would if you were not using Steeltoe. Then [add a reference to the appropriate Steeltoe Connector NuGet package](usage.md#add-nuget-references).

### Configure Settings

This connector supports several settings for local interaction with MongoDB that are overridden by service bindings on deployment:

```json
{
  "MongoDb": {
    "Client": {
      "Server": "localhost",
      "Port": 27017,
      "Database": "SampleDb",
      "Options": {
        "ReplicaSet": "rs0"
      }
    }
  }
}
```

The following table table describes all possible settings for the connector

| Key | Description | Default |
| --- | --- | --- |
| `Server` | Hostname or IP Address of the server. | `localhost` |
| `Port` | Port number of the server. | 27017 |
| `Username` | Username for authentication. | not set |
| `Password` | Password for authentication. | not set |
| `Database` | Name of the database to use. | not set |
| `Options` | Any additional [options](https://mongodb.github.io/mongo-csharp-driver/2.7/apidocs/html/T_MongoDB_Driver_MongoClientSettings.htm), passed through as provided. | not set |
| `ConnectionString` | Full connection string. | Built from settings |

>IMPORTANT: All of these settings should be prefixed with `MongoDb:Client:`.

The samples and most templates are already set up to read from `appsettings.json`.

>If a ConnectionString is provided and VCAP_SERVICES are not detected (a typical scenario for local app development), the ConnectionString will be used exactly as provided.

### Cloud Foundry

To use MongoDB on Cloud Foundry, create and bind an instance to your application by using the Cloud Foundry CLI, as shown in the following example:

```bash
# Create MongoDB service
cf create-service mongodb-odb standalone_small myMongoDb

# Bind service to `myApp`
cf bind-service myApp myMongoDb

# Restage the app to pick up change
cf restage myApp
```

>The preceding commands assume you use the MongoDB Enterprise Service for PCF. If you use a different service, you may have to adjust the `create-service` command to fit your environment.

### Add Mongo Client

To use `MongoClient` and `MongoUrl` in your application, use the extension provided for Microsoft DI:

```csharp
using Steeltoe.Connector.MongoDb;
public class Startup
{
  ...
  public IServiceProvider ConfigureServices(IServiceCollection services)
  {
      services.AddMongoClient(Configuration);
  }
  ...
}
```

### Use Mongo Client

The following example shows how to inject and use an `IMongoClient` and a `MongoUrl` in order to get an `IMongoDatabase` object:

```csharp
public class SomeClass
{
  private readonly IMongoDatabase _database = null;
  public SomeClass(IMongoClient mongoClient, MongoUrl mongoUrl)
  {
    _database = mongoClient.GetDatabase(mongoUrl.DatabaseName);
  }
  public IMongoCollection<SomeObject> MyObjects
  {
      get { return _database.GetCollection<SomeObject>("MyObjects"); }
  }
}
```
