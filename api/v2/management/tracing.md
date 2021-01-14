# Trace

The Steeltoe HTTP trace endpoint provides the ability to view the last several requests made of your application.

When you activate the tracing endpoint, an `ITraceRepository` implementation is configured and created to hold `Trace` information that can be retrieved using the endpoint.

## Configure Settings

The following table describes the settings that you can apply to the endpoint:

|Key|Description|Default|
|---|---|---|
|id|The ID of the trace endpoint|`trace`|
|enabled|Enable or disable trace management endpoint|true|
|sensitive|Currently not used|false|
|requiredPermissions|User permissions required on Cloud Foundry to access endpoint|RESTRICTED|
|capacity|Size of the circular buffer of traces|100|
|addRequestHeaders|Add request headers|true|
|addResponseHeaders|Add response headers|true|
|addPathInfo|Add path information|false|
|addUserPrincipal|Add user principal|false|
|addParameters|Add request parameters|false|
|addQueryString|Add query string|false|
|addAuthType|Add authentication type|false|
|addRemoteAddress|Add remote address of user|false|
|addSessionId|Add session id|false|
|addTimeTaken|Add time take|true|

**Note**: **Each setting above must be prefixed with `management:endpoints:trace`**.

## Enable HTTP Access

The default path to the Trace endpoint is computed by combining the global `path` prefix setting together with the `id` setting from above. The default path is  `/actuator/trace`.

The coding steps you take to enable HTTP access to the Trace endpoint differs depending on the type of .NET application your are developing.  The sections which follow describe the steps needed for each of the supported application types.

### ASP.NET Core App

Refer to the [HTTP Access ASP.NET Core](./using-endpoints.md#http-access-aspnet-core) section below to see the overall steps required to enable HTTP access to endpoints in an ASP.NET Core application.

To add the Trace actuator to the service container, use the `AddTraceActuator()` extension method from `EndpointServiceCollectionExtensions`.

To add the Trace actuator middleware to the ASP.NET Core pipeline, use the `UseTraceActuator()` extension method from `EndpointApplicationBuilderExtensions`.

### ASP.NET 4.x App

To add the Trace actuator endpoint, use the `UseTraceActuator()` method from `ActuatorConfigurator`.

### ASP.NET OWIN App

To add the Trace actuator middleware to the ASP.NET OWIN pipeline, use the `UseTraceActuator()` extension method from `TraceEndpointAppBuilderExtensions`.
