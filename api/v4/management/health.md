# Health

You can use the Steeltoe Health endpoint to query the status of your running application.
It is often used to monitor software and alert someone if a production system goes down.

Health information is collected from all `IHealthContributor` implementations registered in the application,
as well as from [ASP.NET Core Health Checks](https://learn.microsoft.com/aspnet/core/host-and-deploy/health-checks).
Steeltoe includes several contributors, out-of-the-box, that you can use.
Other Steeltoe components typically add their own contributors where applicable.
In addition, you can write your own.

By default, the final application health state is computed by the registered `IHealthAggregator` implementation.
It is responsible for sorting out all of the returned statuses from each `IHealthContributor` and [`IHealthCheck`](https://learn.microsoft.com/dotnet/api/microsoft.extensions.diagnostics.healthchecks.ihealthcheck) and deriving an overall application health state.
The built-in aggregator returns the "worst" status returned from the contributors and checks.

## Configure Settings

The following table describes the configuration settings that you can apply to the endpoint.
Each key must be prefixed with `Management:Endpoints:Health:`.

| Key | Description | Default |
| --- | ----------- | ------- |
| `Enabled` | Whether the endpoint is enabled | `true` |
| `ID`      | The unique ID of the endpoint | `health` |
| `Path`    | The relative path at which the endpoint is exposed | same as `ID` |
| `RequiredPermissions` | Permissions required to access the endpoint when running on Cloud Foundry | `Restricted` |
| `AllowedVerbs` | An array of HTTP verbs in which the endpoint is exposed | `GET` |
| `ShowComponents` | Whether health check components should be included in the response | `Never` |
| `ShowDetails` | Whether details of health check components should be included in the response | `Never` |
| `Claim` | The claim required in `HttpContext.User` when `ShowComponents` and/or `ShowDetails` is set to `WhenAuthorized` | |
| `Role`  | The role required in `HttpContext.User` when `ShowComponents` and/or `ShowDetails` is set to `WhenAuthorized`  | |

The depth of information exposed by the health endpoint depends on the `ShowComponents` and `ShowDetails` properties; both can be configured with one of the following values:

| Name    | Description |
| ------- | ----------- |
| `Never` | Never shown |
| `WhenAuthorized` | Shown only to authorized users |
| `Always` | Always shown |

`ShowDetails` has an effect only when `ShowComponents` is set to:

* `Always`
* `WhenAuthorized` if the request is authorized

Authorized users can be configured by setting `Claim` or `Role`.
A user is considered to be authorized when they are in the given role or have the specified claim.
The following example uses `Management:Endpoints:Health:Claim`:

```json
{
  "Management": {
    "Endpoints": {
      "Health": {
        "ShowComponents": "WhenAuthorized",
        "ShowDetails": "WhenAuthorized",
        "Claim": {
          "Type": "health_actuator",
          "Value": "see_all"
        }
      }
    }
  }
}
```

## Enable HTTP Access

The URL path to the endpoint is computed by combining the global `Management:Endpoints:Path` setting with the `Path` setting described in the preceding section.
The default path is `/actuator/health`.

See the [Exposing Endpoints](./using-endpoints.md#exposing-endpoints) and [HTTP Access](./using-endpoints.md#http-access) sections for the steps required to enable HTTP access to endpoints in an ASP.NET Core application.

To add the actuator to the service container and map its route, use the `AddHealthActuator` extension method.

Add the following code to `Program.cs` to use the actuator endpoint:

```csharp
using Steeltoe.Management.Endpoint.Actuators.Health;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHealthActuator();
```

> [!TIP]
> It is recommended that you use `AddAllActuators()` instead of adding individual actuators;
> this enables individually turning them on/off at runtime via configuration.

By default, the health status is reflected in the HTTP response status code.
For example, when a health check fails, the response status code is `503 Service Unavailable`.
The configuration key `Management:Endpoints:UseStatusCodeFromResponse` can be set to `false`, which makes the health check always respond with status code `200 OK`.
Clients can overrule this per request by sending an `X-Use-Status-Code-From-Response` HTTP header with the value `true` or `false`.

> [!TIP]
> By default, health contributors for disk space and ping are activated. They can be turned off through configuration:
>
> ```json
> {
>   "Management": {
>     "Endpoints": {
>       "Health": {
>         "DiskSpace": {
>           "Enabled": "false"
>         },
>         "Ping": {
>           "Enabled": "false"
>         }
>       }
>     }
>   }
> }
> ```

## Sample Output

This endpoint returns the top-level status, along with the details of the contributors and checks.

The response is always returned as JSON, and this is the default value:

```json
{
  "status": "UP"
}
```

When `ShowComponents` and `ShowDetails` are set to `Always`, or when set to `WhenAuthorized` and the request is authorized, the response is more detailed:

```json
{
  "status": "UP",
  "components": {
    "ping": {
      "status": "UP"
    },
    "diskSpace": {
      "status": "UP",
      "details": {
        "total": 1999599824896,
        "free": 1330717282304,
        "threshold": 10485760,
        "path": "C:\\source\\Repository\\src\\Project",
        "exists": true
      }
    }
  }
}
```

> [!NOTE]
> When using Steeltoe Connectors, Service Discovery, or Config Server in your application, the corresponding health contributors are automatically added to the service container.
> See their corresponding documentation for how to turn them off.

## Health Groups

If you want to check application health based on a subset of health contributors, you can specify the name of the grouping and a comma-separated list of contributors to include:

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

While group names are case-sensitive, the entries in `Include` are case-insensitive and activate only health contributors with a matching `Id`, and/or ASP.NET health check registrations with a matching name.

For any group that has been defined, you can access health information from the group by appending the group name to the HTTP request URL. For example: `/actuator/health/example-group`.

`ShowComponents` and `ShowDetails` can also be set at the group level, overriding the settings found at the endpoint level.

```json
{
  "Management": {
    "Endpoints": {
      "Health": {
        "Claim": {
          "Type": "health_actuator",
          "Value": "see_all"
        },
        "Groups": {
          "example-group": {
            "Include": "Redis,RabbitMQ",
            "ShowComponents": "Always",
            "ShowDetails": "WhenAuthorized"
          }
        }
      }
    }
  }
}
```

### Kubernetes Health Groups

Applications deployed on Kubernetes can provide information about their internal state with [Container Probes](https://kubernetes.io/docs/concepts/workloads/pods/pod-lifecycle/#container-probes).
Depending on your [Kubernetes configuration](https://kubernetes.io/docs/tasks/configure-pod-container/configure-liveness-readiness-startup-probes/), the kubelet calls those probes and reacts to the result.

Steeltoe provides an [`ApplicationAvailability`](https://github.com/SteeltoeOSS/Steeltoe/blob/main/src/Management/src/Endpoint/Actuators/Health/Availability/ApplicationAvailability.cs) class for managing various types of application state.
Support is provided, out of the box, for Liveness and Readiness, where each is exposed in a corresponding `IHealthContributor` and health group.
While these health contributors are included, they are disabled by default and must be enabled in the configuration (as shown in the example below).

To change the health contributors that are included in either of the two built-in groups, use the same style of configuration described above.
Note that this _replaces_ these groupings, so if you want to _add_ an `IHealthContributor`, you must include the original entry.
These entries demonstrate enabling the probes and their groups, and specifies including disk space in both groups:

```json
{
  "Management": {
    "Endpoints": {
      "Health": {
        "Liveness": {
          "Enabled": "true"
        },
        "Readiness": {
          "Enabled": "true"
        },
        "Groups": {
          "liveness": {
            "Include": "diskSpace,livenessState"
          },
          "readiness": {
            "Include": "diskSpace,readinessState"
          }
        }
      }
    }
  }
}
```

#### Liveness

The Liveness state of an application tells whether its internal state is valid. If Liveness is broken, this means that the application itself is in a failed state and cannot recover from it. In this case, the best course of action is to restart the application instance.

Out of the box, any of Steeltoe's extension methods that set up the health actuator initialize the liveness state with `LivenessState.Correct`. The only other defined state for liveness is `LivenessState.Broken`, though Steeltoe code does not currently cover any conditions that set this state.

> [!CAUTION]
> In general, the Liveness state should not depend on external system checks such as a database, queue, or cache server. Including checks on external systems could trigger massive restarts and cascading failures across the platform.

#### Readiness

The Readiness state tells whether the application is ready to accept client requests. If the Readiness state is unready, the platform should not route traffic to this instance. If an application is too busy processing a task queue, then it could declare itself as busy until its load is manageable again.

Out of the box, any of Steeltoe's extension methods that set up the health actuator also:

* Register a callback on [`ApplicationStarted`](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.hosting.iapplicationlifetime.applicationstarted) to initialize the readiness state to `AcceptingTraffic` when the application has started
* Register a callback on [`ApplicationStopping`](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.hosting.iapplicationlifetime.applicationstopping) to change the state to `RefusingTraffic` when the application begins to shut down

These are the only defined states for this availability type.

#### Managing Application Availability State

Application components can retrieve the current availability state at any time by requesting `ApplicationAvailability` from the dependency injection container and calling methods on it:

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

If you want to provide custom health information for your application:

1. Create a class that implements the `IHealthContributor` interface.
1. Add the new class to the service container.

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
  "status":"WARNING"
}
```

When `ShowComponents` and `ShowDetails` are set to `Always`, or when set to `WhenAuthorized` and the request is authorized, the response is more detailed:

```json
{
  "status": "WARNING",
  "components": {
    "ExampleHealthContributor": {
      "status": "WARNING",
      "description": "This health check does not check anything"
    },
    "ping": {
      "status": "UP"
    },
    "diskSpace": {
      "status": "UP",
      "details": {
        "total": 1999599824896,
        "free": 1330717282304,
        "threshold": 10485760,
        "path": "C:\\source\\Repository\\src\\Project",
        "exists": true
      }
    }
  }
}
```

## ASP NET Core Health Checks

ASP.NET Core offers [middleware and libraries](https://learn.microsoft.com/aspnet/core/host-and-deploy/health-checks) and abstractions for reporting health. There is wide community support for these abstractions from libraries such as [AspNetCore.Diagnostics.HealthChecks](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks). You can use these community-provided health checks and make them available using the health actuator (for integration with Cloud Foundry or any other infrastructure that depends on this format). In addition, Steeltoe Connectors expose functionality to get the connection string, which is needed to set up these community health checks.

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
