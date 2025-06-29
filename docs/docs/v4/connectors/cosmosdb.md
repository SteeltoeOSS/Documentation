# CosmosDB

This connector simplifies accessing [Azure CosmosDB](https://azure.microsoft.com/products/cosmos-db/) databases.
It supports the following .NET drivers:

- [Microsoft.Azure.Cosmos](https://www.nuget.org/packages/Microsoft.Azure.Cosmos), which provides a `CosmosClient`

The remainder of this topic assumes that you are familiar with the basic concepts of Steeltoe Connectors. See [Overview](./usage.md) for more information.

## Using the CosmosDB connector

To use this connector:

1. Create a CosmosDB server instance or use the [emulator](https://learn.microsoft.com/azure/cosmos-db/local-emulator).
1. Add NuGet references to your project.
1. Configure your connection string in `appsettings.json`.
1. Initialize the Steeltoe Connector at startup.
1. Use the driver-specific connection/client instance.

### Add NuGet References

To use this connector, add a NuGet reference to `Steeltoe.Connectors`.

Also add a NuGet reference to one of the .NET drivers listed above, as you would if you were not using Steeltoe.

### Configure connection string

The CosmosDB connection string can be obtained as described in the [Microsoft documentation](https://learn.microsoft.com/azure/cosmos-db/nosql/how-to-dotnet-get-started#retrieve-your-account-connection-string).

The following example `appsettings.json` uses the emulator:

```json
{
  "Steeltoe": {
    "Client": {
      "CosmosDb": {
        "Default": {
          "ConnectionString": "AccountEndpoint=https://localhost:8081;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
          "Database": "TestDatabase"
        }
      }
    }
  }
}
```

Notice that this configuration file contains the database name, in addition to the connection string. This value is exposed
as `CosmosDbOptions.Database`.

### Initialize Steeltoe Connector

Update your `Program.cs` to initialize the Connector:

```csharp
using Steeltoe.Connectors.CosmosDb;

var builder = WebApplication.CreateBuilder(args);
builder.AddCosmosDb();
```

### Use CosmosClient

To retrieve data from CosmosDB in your app, use the following steps:

1. Define a class that contains container data:

    ```csharp
    using Newtonsoft.Json;

    public class SampleObject
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        public string? Text { get; set; }
    }
    ```

1. To obtain a `CosmosClient` instance in your application, inject the Steeltoe factory in a controller or view:

    ```csharp
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Azure.Cosmos.Linq;
    using Steeltoe.Connectors;
    using Steeltoe.Connectors.CosmosDb;

    public class HomeController : Controller
    {
        public async Task<IActionResult> Index(
            [FromServices] ConnectorFactory<CosmosDbOptions, CosmosClient> connectorFactory)
        {
            var connector = connectorFactory.Get();
            CosmosClient client = connector.GetConnection();

            Container container = client.GetContainer(connector.Options.Database, "TestContainer");
            List<SampleObject> sampleObjects = new();

            await foreach (SampleObject sampleObject in GetAllAsync(container))
            {
                sampleObjects.Add(sampleObject);
            }

            return View(sampleObjects);
        }

        private async IAsyncEnumerable<SampleObject> GetAllAsync(Container container)
        {
            using FeedIterator<SampleObject> iterator =
                container.GetItemLinqQueryable<SampleObject>().ToFeedIterator();

            while (iterator.HasMoreResults)
            {
                FeedResponse<SampleObject> response = await iterator.ReadNextAsync();

                foreach (SampleObject sampleObject in response)
                {
                    yield return sampleObject;
                }
            }
        }
    }
    ```

A complete sample app that uses `CosmosClient` is provided at https://github.com/SteeltoeOSS/Samples/tree/main/Connectors/src/CosmosDb.
