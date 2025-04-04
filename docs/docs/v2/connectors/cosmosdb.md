# CosmosDB

This connector simplifies using Azure Cosmos DB in an application running on Cloud Foundry. The connector is built to work with Azure Cosmos DB service instances that have been provisioned using the [Microsoft Azure Service Broker](https://docs.pivotal.io/partners/azure-sb/index.html), where  from either `Microsoft.Azure.Cosmos` or the newer package `Azure.Cosmos`.

## Usage

To use this connector:

1. Create a Cosmos DB Service instance and bind it to your application.
1. Optionally, configure any CosmosDB client settings.
1. Add the Steeltoe Cloud Foundry configuration provider to your `ConfigurationBuilder`.

### Add NuGet References

To use the CosmosDB connector, add either [Microsoft.Azure.Cosmos](https://www.nuget.org/packages/Microsoft.Azure.Cosmos) or [Azure.Cosmos](https://www.nuget.org/packages/Azure.Cosmos/) (pre-release only as of this writing) as you would if you weren't using Steeltoe. Then, [add a reference to the appropriate Steeltoe Connector NuGet package](usage.md#add-nuget-references)

### Configure Settings

This connector supports several settings for local interaction with CosmosDB that will be overridden by service bindings on deployment:

```json
{
  "cosmosdb": {
    "client": {
      "host": "https://localhost:8081",
      "masterKey": "<yourMasterKeyHere>"
    }
  }
}
```

The following table table describes all possible settings for the connector

|Key|Description|Default|
|---|---|---|
|host|Protocol, hostname or IP Address and port of the server|not set|
|masterKey|Authentication for read/write access|not set|
|readOnlyKey|Authentication for read-only access|not set|
|databaseId|Name of the database to use|not set|
|useReadOnlyCredentials|Designate that the read-only key should be used|false|
|connectionString|Full connection string|built from settings|
|urlEncodedCredentials|Set to `true` if your service broker provides URL-encoded credentials|false|

>IMPORTANT: All of these settings should be prefixed with `cosmosdb:client:`.

The samples and most templates are already set up to read from `appsettings.json`.

>NOTE: If a ConnectionString is provided and VCAP_SERVICES are not detected (a typical scenario for local app development), the ConnectionString will be used exactly as provided.

### Cloud Foundry

To use CosmosDB on Cloud Foundry, create and bind an instance to your application by using the Cloud Foundry CLI, as shown in the following example:

```bash
# Create CosmosDB service
cf create-service azure-cosmosdb standard myCosmosDb

# Bind service to `myApp`
cf bind-service myApp myCosmosDb

# Restage the app to pick up change
cf restage myApp
```

### Use CosmosClient

Use Steeltoe's `ConnectionStringManager` to access connection information built by combining your `cosmosdb:client` settings with credentials from service bindings (when present) and create a new `CosmosClient`:

```csharp
// read settings from "cosmosdb:client" and VCAP:services or services:
var configMgr = new ConnectionStringManager(configuration);
var cosmosInfo = configMgr.Get<CosmosDbConnectionInfo>();

// these are mapped into the properties dictionary
var databaseName = cosmosInfo.Properties["DatabaseId"];
var databaseLink = cosmosInfo.Properties["DatabaseLink"];

// container is not provided by a service binding, use your own config value to store it:
var containerName = configuration.GetValue<string>("CosmosDb:Container");
var cosmosClient = new CosmosClient(cosmosInfo.ConnectionString);
```
