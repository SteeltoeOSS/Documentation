# Health

You can use the Steeltoe Health endpoint to query the status of your running application.
It is often used to monitor software and alert someone if a production system goes down.

Health information is collected from all `IHealthContributor` implementations registered in the application,
as well as from [ASP.NET Core Health Checks](https://learn.microsoft.com/aspnet/core/host-and-deploy/health-checks).
Steeltoe includes several contributors out of the box that you can use.
Other Steeltoe components typically add their own contributors where applicable.
Also, and perhaps more importantly, you can write your own.

By default, the final application health state is computed by the registered `IHealthAggregator` implementation.
It is responsible for sorting out all of the returned statuses from each `IHealthContributor` and [`IHealthCheck`](https://learn.microsoft.com/dotnet/api/microsoft.extensions.diagnostics.healthchecks.ihealthcheck) and deriving an overall application health state.
The built-in aggregator returns the "worst" status returned from the contributors and checks.

The following table describes the configuration settings that you can apply to the endpoint.
Each key must be prefixed with `Management:Endpoints:Health:`.

| Key | Description | Default |
| --- | --- | --- |
| `Enabled` | Whether the endpoint is enabled. | `true` |
| `ID` | The unique ID of the endpoint. | `health` |
| `Path` | The relative path at which the endpoint is exposed. | same as `ID` |
| `RequiredPermissions` | Permissions required to access the endpoint, when running on Cloud Foundry. | `Restricted` |
| `AllowedVerbs` | An array of HTTP verbs the endpoint is exposed at. | `GET` |
| `ShowDetails` | The level of detail included in the response. | `Always` |
| `Claim` | The claim required in `HttpContext.User` when `ShowDetails` is set to `WhenAuthorized`. | |
| `Role` | The role required in `HttpContext.User` when `ShowDetails` is set to `WhenAuthorized`. | |
| `Groups` | Specify logical groupings of health contributors. | |

The information exposed by the health endpoint depends on the `ShowDetails` property, which can be configured with one of the following values:

| Name | Description |
| --- | --- |
| `Never` | Details are never shown. |
| `WhenAuthorized` | Details are shown only to authorized users. |
| `Always` | Details are always shown. |

Authorized users can be configured by setting `Claim` or `Role`.
A user is considered to be authorized when they are in the given role or have the specified claim.
The following example uses `Management:Endpoints:Health:Claim`:

```json
{
  "Management": {
    "Endpoints": {
      "Health": {
        "ShowDetails": "WhenAuthorized",
        "Claim": {
          "Type": "health_actuator",
          "Value": "see_details"
        }
      }
    }
  }
}
```

## Enable HTTP Access

The URL path to the endpoint is computed by combining the global `Management:Endpoints:Path` setting together with the `Path` setting described in the preceding section.
The default path is `/actuator/health`.

See the [Exposing Endpoints](./using-endpoints.md#exposing-endpoints) and [HTTP Access](./using-endpoints.md#http-access) sections for the overall steps required to enable HTTP access to endpoints in an ASP.NET Core application.

To add the actuator to the service container and map its route, use the `AddHealthActuator` extension method.

Add the following code to `Program.cs` to use the actuator endpoint:

```csharp
using Steeltoe.Management.Endpoint.Actuators.Health;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHealthActuator();
```

> [!TIP]
> It's recommended to use `AddAllActuators()` instead of adding individual actuators,
> which enables individually turning them on/off at runtime via configuration.

> [!TIP]
> By default, health contributors for disk space, liveness, and readiness are activated. They can be turned off through configuration:
> ```json
> {
>   "Management": {
>     "Endpoints": {
>       "Health": {
>         "DiskSpace": {
>           "Enabled": "false"
>         },
>         "Liveness": {
>           "Enabled": "false"
>         },
>         "Readiness": {
>           "Enabled": "false"
>         }
>       }
>     }
>   }
> }
> ```

## Sample Output

This endpoint returns the top-level status, along with the details of the contributors and checks.

The response will always be returned as JSON, like this:

```json
{
  "status": "UP",
  "details": {
    "diskSpace": {
      "total": 1020959649792,
      "free": 813492727808,
      "threshold": 10485760,
      "status": "UP"
    },
    "readiness": {
      "ReadinessState": "ACCEPTING_TRAFFIC"
    },
    "liveness": {
      "LivenessState": "CORRECT"
    }
  }
}
```

> [!NOTE]
> When using Steeltoe Connectors, Service Discovery, or Config Server in your application, the corresponding health contributors are automatically added to the service container.
> See their corresponding documentation for how to turn them off.

## Health Groups

Should you need to check application health based on a subset of health contributors, you may specify the name of the grouping and a comma-separated list of contributors to include like this:

```json
{
  "Management": {
    "Endpoints": {
      "Health": {
        "Groups": {
          "example-group": {
            "Include": "Redis,RabbitMQ"
          }
        }
      }
    }
  }
}
```

While group names are case-sensitive, the entries in `Include` are case-insensitive and will only activate health contributors with a matching `Id`.

> [!NOTE]
> Adding your own health groups implicitly turns off the built-in groups for liveness and readiness. If you want to keep them, you need to include them explictly.
>
> To turn off the default liveness/readiness health groups without adding any of your own, configure a single-element empty group like this:
> ```json
> {
>   "Management": {
>     "Endpoints": {
>       "Health": {
>         "Groups": {
>           "": {
>             "Include": ""
>           }
>         }
>       }
>     }
>   }
> }
> ```
> This clumsy format is used because there's no syntax in .NET Configuration to express an empty collection.
> See https://github.com/dotnet/extensions/issues/1341#issuecomment-598310364 for details.

For any group that has been defined, you may access health information from the group by appending the group name to the HTTP request URL. For example: `/actuator/health/example-group`.


### Kubernetes Health Groups

Applications deployed on Kubernetes can provide information about their internal state with [Container Probes](https://kubernetes.io/docs/concepts/workloads/pods/pod-lifecycle/#container-probes). Depending on your [Kubernetes configuration](https://kubernetes.io/docs/tasks/configure-pod-container/configure-liveness-readiness-startup-probes/), the kubelet will call those probes and react to the result.

Steeltoe provides an [`ApplicationAvailability`](https://github.com/SteeltoeOSS/Steeltoe/blob/main/src/Management/src/Endpoint/Actuators/Health/Availability/ApplicationAvailability.cs) class for managing various types of application state. Out of the box, support is provided for Liveness and Readiness, where each is exposed in a corresponding `IHealthContributor` and health group.

To change the health contributors that are included in either of the two default groups, use the same style of configuration described above. Please note that this will _replace_ the default groupings, so if you would like to _add_ an `IHealthContributor` you will need to include the original entry. These entries demonstrate including disk space in both groups:

```json
{
  "Management": {
    "Endpoints": {
      "Health": {
        "Groups": {
          "liveness": {
            "Include": "diskSPACE,liveness"
          },
          "readiness": {
            "Include": "diskspace,readiness"
          }
        }
      }
    }
  }
}
```

#### Liveness

The "Liveness" state of an application instance tells whether its internal state allows it to work correctly, or recover by itself if it's currently failing. A broken "Liveness" state means that the application is in a state that it cannot recover from, and the infrastructure should restart the application.

Out of the box, any of Steeltoe's extension methods that set up the health actuator will initialize the liveness state `LivenessState.Correct`. The only other defined state for liveness is `LivenessState.Broken`, though Steeltoe code does not currently cover any conditions that set this state.

> [!NOTE]
> In general, the "Liveness" state should not depend on external system checks such as a database, queue, or cache server. Including checks on external systems could trigger massive restarts and cascading failures across the platform.

#### Readiness

The "Readiness" state of an application instance describes whether the application is ready to handle traffic. A failing "Readiness" state tells the platform that it should not route traffic to the application instance.

Out of the box, any of Steeltoe's extension methods that set up the health actuator will also register a callback on [`ApplicationStarted`](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.hosting.iapplicationlifetime.applicationstarted) to initialize the readiness state to `AcceptingTraffic` when the application has started, and register a callback on [`ApplicationStopping`](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.hosting.iapplicationlifetime.applicationstopping) to change the state to `RefusingTraffic` when the application begins to shut down. These are the only defined states for this availability type.

#### Managing Application Availability State

Application components can retrieve the current availability state at any time, by requesting `ApplicationAvailability` from the dependency injection container and calling methods on it:

```csharp
[ApiController]
[Route("[controller]")]
public class AvailabilityController(ApplicationAvailability applicationAvailability) : ControllerBase
{
    [HttpGet]
    public string Get()
    {
        var readinessState = applicationAvailability.GetReadinessState();
        var livenessState = applicationAvailability.GetLivenessState();

        return $"Readiness state = {readinessState}, Liveness state = {livenessState}";
    }
}
```

Additionally, it is possible to subscribe to changes in liveness and readiness by attaching an event handler:

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHealthActuator();

var app = builder.Build();

var availability = app.Services.GetRequiredService<ApplicationAvailability>();
availability.LivenessChanged += (_, args) => Console.WriteLine($"Liveness state changed to {args.NewState}");
availability.ReadinessChanged += (_, args) => Console.WriteLine($"Readiness state changed to {args.NewState}");

app.Run();
```

## Creating a Custom Health Contributor

If you wish to provide custom health information for your application, create a class that implements the `IHealthContributor` interface and then add it to the service container.

The following example contributor always returns a `HealthStatus` of `WARNING`:

```csharp
using Steeltoe.Common.HealthChecks;
using Steeltoe.Management.Endpoint.Actuators.Health;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHealthActuator();
builder.Services.AddHealthContributor<ExampleHealthContributor>();

public class ExampleHealthContributor : IHealthContributor
{
    public string Id => nameof(ExampleHealthContributor);

    public Task<HealthCheckResult?> CheckHealthAsync(CancellationToken cancellationToken)
    {
        HealthCheckResult? status = GetStatus();
        return Task.FromResult(status);
    }

    private static HealthCheckResult? GetStatus()
    {
        return new HealthCheckResult
        {
            Status = HealthStatus.Warning,
            Description = "This health check does not check anything"
        };
    }
}
```

Sending a GET request to `/actuator/health` returns the following response:

```json
{
  "status": "WARNING",
  "details": {
    "ExampleHealthContributor": {
      "status": "WARNING",
      "description": "This health check does not check anything"
    }
  }
}
```

## ASP NET Core Health Checks

ASP.NET Core also offers [middleware and libraries](https://learn.microsoft.com/aspnet/core/host-and-deploy/health-checks) and abstractions for reporting health. There is wide community support for these abstractions from libraries such as [AspNetCore.Diagnostics.HealthChecks](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks). You can use these community-provided health checks and make them available over the health actuator endpoint (for integration with Cloud Foundry or any other infrastructure that depends on this format). In addition, Steeltoe Connectors expose functionality to get the connection string, which is needed to set up these community health checks.

For example, to use the Steeltoe MySQL connector, but replace its health contributor with the ASP.NET Core community health check,
use the following code in `Program.cs`:

```csharp
using Microsoft.Extensions.Options;
using Steeltoe.Connectors.MySql;
using Steeltoe.Management.Endpoint.Actuators.Health;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHealthActuator();

// Add the Steeltoe Connector for MySQL, but turn off its health contributor.
builder.AddMySql(null, options => options.EnableHealthChecks = false);

// Add the community-based ASP.NET health check, obtaining the connection string from Steeltoe.
builder.Services.AddHealthChecks().AddMySql(serviceProvider =>
{
    var options = serviceProvider.GetRequiredService<IOptions<MySqlOptions>>();
    return options.Value.ConnectionString!;
});
```

The code above assumes the following `appsettings.json` configuration, combined with the [Steeltoe docker container for MySQL](https://github.com/SteeltoeOSS/Samples/blob/main/CommonTasks.md#mysql):

```json
{
  "Steeltoe": {
    "Client": {
      "MySql": {
        "Default": {
          "ConnectionString": "SERVER=localhost;Database=steeltoe;UID=steeltoe;PWD=steeltoe"
        }        
      }
    }
  }
}
```
