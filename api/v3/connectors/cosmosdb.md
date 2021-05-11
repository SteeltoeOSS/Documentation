# CosmosDB

This connector simplifies using Azure Cosmos DB. The connector is built to work with Azure Cosmos DB from either `Microsoft.Azure.Cosmos` or the newer package `Azure.Cosmos`.

## Usage

To use this connector:

1. Create a Cosmos DB Service instance and bind it to your application.
1. Optionally, configure any CosmosDB client settings.
1. Optionally, add the Steeltoe Cloud Foundry configuration provider to your `ConfigurationBuilder`.

### Add NuGet References

To use the CosmosDB connector, add either [Microsoft.Azure.Cosmos](https://www.nuget.org/packages/Microsoft.Azure.Cosmos) or [Azure.Cosmos](https://www.nuget.org/packages/Azure.Cosmos/) (pre-release only as of this writing) as you would if you were not using Steeltoe. Then [add a reference to the appropriate Steeltoe Connector NuGet package](usage.md#add-nuget-references).

### Configure Settings

This connector supports several settings for local interaction with CosmosDB that are overridden by service bindings on deployment:

```json
{
  "Cosmosdb": {
    "Client": {
      "Host": "https://localhost:8081",
      "MasterKey": "<yourMasterKeyHere>"
    }
  }
}
```

The following table table describes all possible settings for the connector

| Key | Description | Default |
| --- | --- | --- |
| `Host` | Protocol, hostname or IP Address and port of the server. | not set |
| `MasterKey` | Authentication for read/write access. | not set |
| `ReadOnlyKey` | Authentication for read-only access. | not set |
| `DatabaseId` | Name of the database to use. | not set |
| `UseReadOnlyCredentials` | Designate that the read-only key should be used. | `false` |
| `ConnectionString` | Full connection string. | Built from settings |

>IMPORTANT: All of these settings should be prefixed with `CosmosDb:Client:`.

The samples and most templates are already set up to read from `appsettings.json`.

>If a `ConnectionString` is provided and `VCAP_SERVICES` are not detected (a typical scenario for local application development), the `ConnectionString` is used exactly as provided.

### Cloud Foundry

To use CosmosDB on Cloud Foundry, create and bind an instance to your application by using the Cloud Foundry CLI:

```bash
# Create CosmosDB service
cf create-service azure-cosmosdb standard myCosmosDb

# Bind service to `myApp`
cf bind-service myApp myCosmosDb

# Restage the app to pick up change
cf restage myApp
```

>The connector is built to work with Azure Cosmos DB service instances that have been provisioned with the [Microsoft Azure Service Broker](https://docs.pivotal.io/partners/azure-sb/index.html).

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
