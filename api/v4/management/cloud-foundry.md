# Cloud Foundry Integration

Integration with Apps Manager is accomplished by adding the Cloud Foundry management endpoint to your application. When used, this endpoint enables the following additional functionality on Cloud Foundry:

* Provides an alternate, secured route to the endpoints expected by Apps Manager and configured in your application.
* Exposes an endpoint that can be queried to return the IDs of and links to the enabled management endpoints in the application.
* Adds Cloud Foundry security middleware to the request pipeline, to secure access to the management endpoints by using security tokens acquired from the UAA.

> [!NOTE]
> The Cloud Foundry integration will not work unless the [Cloud Foundry Configuration Provider](../configuration/cloud-foundry-provider.md) has also been configured.

## Security

When adding this management endpoint to your application, the Cloud Foundry security middleware is added to the request processing pipeline of your application to enforce that, when a request is made to any of the management endpoints, a valid UAA access token is provided as part of that request. Additionally, the security middleware uses the token to determine whether the authenticated user has permission to access the management endpoint.

> [!NOTE]
> The Cloud Foundry security middleware is only active when your application is running on Cloud Foundry.

## External access

When running in Cloud Foundry, it is possible to access the endpoints via the [hypermedia](./hypermedia.md) URL, which defaults to `/actuator`. In other words, you can also access all your endpoints from this URL prefix. For example, the [info](./info.md) endpoint would be accessible at `/actuator/info`.

While the endpoints provided on the `/cloudfoundryapplication` path are secured as described above, the endpoints provided on the `/actuator` path are not. For this reason, only health and info are exposed by default and others must be exposed explicitly. In addition, the endpoints may be secured by whatever security mechanism the application itself uses. For more details, see [securing actuators](./using-endpoints.md#securing-endpoints).

## Configure Settings

Typically, no additional configuration is needed. However, the following table describes the configuration settings that you can apply to the Cloud Foundry endpoint.
Each key must be prefixed with `Management:Endpoints:CloudFoundry`.

| Key | Description | Default |
| --- | --- | --- |
| `Enabled` | Whether the endpoint is enabled. | `true` |
| `ID` | The unique ID of the endpoint. | `""` |
| `Path` | The relative path at which the endpoint is exposed. | same as `ID` |
| `RequiredPermissions` | Permissions required to access the endpoint, when running on Cloud Foundry. | `Restricted` |
| `AllowedVerbs` | An array of HTTP verbs the endpoint is exposed at. | `GET` |
| `ValidateCertificates` | Whether to validate server certificates. | `true` |
| `ApplicationId` | The ID of the application used in permission checks. | |
| `CloudFoundryApi` | The URL of the Cloud Foundry API. | |

## Enable HTTP Access

The URL path to the endpoint is computed by combining the global `Management:Endpoints:Path` setting together with the `Path` setting described in the preceding section.
The default path is `/cloudfoundryapplication`.

See the [Exposing Endpoints](./using-endpoints.md#exposing-endpoints) and [HTTP Access](./using-endpoints.md#http-access) sections for the overall steps required to enable HTTP access to endpoints in an ASP.NET Core application.

To add the actuator to the service container, add a CORS policy, register security middleware and map its route, use the `AddCloudFoundryActuator` extension method.

Add the following code to `Program.cs` to use the actuator endpoint:

```csharp
using Steeltoe.Configuration.CloudFoundry;
using Steeltoe.Management.Endpoint.Actuators.CloudFoundry;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddCloudFoundry();
builder.Services.AddCloudFoundryActuator();
```

> [!TIP]
> It's recommended to use `AddAllActuators()` instead of adding individual actuators,
> which enables individually turning them on/off at runtime via configuration.
