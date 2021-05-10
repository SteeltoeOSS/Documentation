# MongoDB

This connector simplifies using MongoDB in an application running on Cloud Foundry with the [.NET MongoDB Driver](https://docs.mongodb.com/ecosystem/drivers/csharp/).

>NOTE: There are currently no dedicated samples for the MongoDB connector. You can see it in action in the Steeltoe fork of [eShopOnContainers](https://github.com/SteeltoeOSS/eShopOnContainers), in the Locations API and the Marketing API.

## Usage

To use this connector:

1. Create a MongoDB service instance and bind it to your application.
1. Optionally, configure any MongoDB client settings.
1. Add the Steeltoe Cloud Foundry config provider to you `ConfigurationBuilder`.
1. Add MongoDB classes to your DI container.

### Add NuGet Reference

To use the MongoDB connector, add the official [MongoDB.Driver NuGet package](https://www.nuget.org/packages/MongoDB.Driver/) as you would if you weren't using Steeltoe. Then, [add a reference to the appropriate Steeltoe Connector NuGet package](usage.md#add-nuget-references).

### Configure Settings

This connector supports several settings for local interaction with MongoDB that will be overridden by service bindings on deployment:

```json
{
  "mongodb": {
    "client": {
      "server": "localhost",
      "port": 27017,
      "options": {
        "replicaSet": "rs0"
      }
    }
  }
}
```

The following table table describes all possible settings for the connector

|Key|Description|Default|
|---|---|---|
|server|Hostname or IP Address of the server|localhost|
|port|Port number of the server|27017|
|username|Username for authentication|not set|
|password|Password for authentication|not set|
|database|Name of the database to use|not set|
|options|any additional [options](https://mongodb.github.io/mongo-csharp-driver/2.7/apidocs/html/T_MongoDB_Driver_MongoClientSettings.htm), passed through as provided|not set|
|connectionString|Full connection string|built from settings|
|urlEncodedCredentials|Set to `true` if your service broker provides URL-encoded credentials|false|

>IMPORTANT: All of these settings should be prefixed with `mongodb:client:`.

The samples and most templates are already set up to read from `appsettings.json`.

>NOTE: If a ConnectionString is provided and VCAP_SERVICES are not detected (a typical scenario for local app development), the ConnectionString will be used exactly as provided.

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

>NOTE: The preceding commands assume you use the MongoDB Enterprise Service for PCF. If you use a different service, you may have to adjust the `create-service` command to fit your environment.

### Add Mongo Client

To use `MongoClient` and `MongoUrl` in your application, use the extension provided for Microsoft DI:

```csharp
using Steeltoe.CloudFoundry.Connector.MongoDb;
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

Or the extension provided for Autofac:

```csharp
using Steeltoe.CloudFoundry.ConnectorAutofac;
...
  ContainerBuilder container = new ContainerBuilder();
  var regBuilder = container.RegisterMongoDbConnection(configuration);
...
```

### Use Mongo Client

The following example shows how to inject and use an `IMongoClient` and `MongoUrl` in order to get an `IMongoDatabase` to interact with:

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
