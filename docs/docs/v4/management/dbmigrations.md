# Database Migrations

The Steeltoe Database Migrations endpoint exposes information about database migrations that are available to an application's data source that has been built with Entity Framework Core (EF Core). EF Core migrations give developers the ability to update an application's database schema while staying consistent with the application's data model, and without removing any existing data.

> [!TIP]
> For more detailed information, see [EF Core Migrations Overview](https://learn.microsoft.com/ef/core/managing-schemas/migrations) in the Microsoft documentation.

## Configure Settings

The following table describes the configuration settings that you can apply to the endpoint.
Each key must be prefixed with `Management:Endpoints:DbMigrations:`.

| Key | Description | Default |
| --- | --- | --- |
| `Enabled` | Whether the endpoint is enabled | `true` |
| `ID` | The unique ID of the endpoint | `dbmigrations` |
| `Path` | The relative path at which the endpoint is exposed | same as `ID` |
| `RequiredPermissions` | Permissions required to access the endpoint when running on Cloud Foundry | `Restricted` |
| `AllowedVerbs` | An array of HTTP verbs at which the endpoint is exposed | `GET` |

## Enable HTTP Access

The URL path to the endpoint is computed by combining the global `Management:Endpoints:Path` setting with the `Path` setting described in the preceding section.
The default path is `/actuator/dbmigrations`.

See the [Exposing Endpoints](./using-endpoints.md#exposing-endpoints) and [HTTP Access](./using-endpoints.md#http-access) sections for the steps required to enable HTTP access to endpoints in an ASP.NET Core application.

To add the actuator to the service container and map its route, use the `AddDbMigrationsActuator` extension method.

Add the following code to `Program.cs` to use the actuator endpoint:

```csharp
using Steeltoe.Management.Endpoint.Actuators.DbMigrations;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbMigrationsActuator();
```

> [!TIP]
> It is recommended that you use `AddAllActuators()` instead of adding individual actuators;
> this enables individually turning them on/off at runtime via configuration.

## Sample Output

This endpoint returns a list of objects representing each registered `DbContext`, along with its migrations, grouped by status (pending or applied).

The response is always returned as JSON:

```json
{
  "AppDbContext": {
    "pendingMigrations": [
      "20241028091643_AddTable"
    ],
    "appliedMigrations": [
      "20241028091056_InitialCreate",
      "20241028091550_AddDbColumn"
    ]
  }
}
```
