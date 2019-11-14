### Tracing

The Steeltoe Tracing endpoint provides the ability to view the last several requests made of your application.

When you activate the Tracing endpoint, an [ITraceRepository](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointBase/Trace/ITraceRepository.cs) implementation is configured and created to hold [Trace](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointBase/Trace/Trace.cs) information that can be retrieved using the endpoint.

#### Configure Settings

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

#### Enable HTTP Access

The default path to the Trace endpoint is computed by combining the global `path` prefix setting together with the `id` setting from above. The default path is `/trace`.

The coding steps you take to enable HTTP access to the Trace endpoint differs depending on the type of .NET application your are developing.  The sections which follow describe the steps needed for each of the supported application types.

##### ASP.NET Core App

Refer to the [HTTP Access ASP.NET Core](#http-access-asp-net-core) section below to see the overall steps required to enable HTTP access to endpoints in an ASP.NET Core application.

To add the Trace actuator to the service container, use the `AddTraceActuator()` extension method from [EndpointServiceCollectionExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointCore/Trace/EndpointServiceCollectionExtensions.cs).

To add the Trace actuator middleware to the ASP.NET Core pipeline, use the `UseTraceActuator()` extension method from [EndpointApplicationBuilderExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointCore/Trace/EndpointApplicationBuilderExtensions.cs).

##### ASP.NET 4.x App

Refer to the [HTTP Access ASP.NET 4.x](#http-access-asp-net-4-x) section below to see the overall steps required to enable HTTP access to endpoints in a 4.x application.

To add the Trace actuator endpoint, use the `UseTraceActuator()` method from [ActuatorConfigurator](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointWeb/ActuatorConfigurator.cs).

##### ASP.NET OWIN App

Refer to the [HTTP Access ASP.NET OWIN](#http-access-asp-net-owin) section below to see the overall steps required to enable HTTP access to endpoints in an ASP.NET 4.x OWIN application.

To add the Trace actuator middleware to the ASP.NET OWIN pipeline, use the `UseTraceActuator()` extension method from [TraceEndpointAppBuilderExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointOwin/Trace/TraceEndpointAppBuilderExtensions.cs).
