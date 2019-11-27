### Refresh

The Steeltoe Refresh endpoint can be used to cause the applications configuration to be reloaded and return the new values and keys currently in use in your application. The endpoint reloads the configuration using the applications `IConfigurationRoot`.

#### Configure Settings

The following table describes the settings that you can apply to the endpoint:

|Key|Description|Default|
|---|---|---|
|id|The ID of the refresh endpoint|`refresh`|
|enabled|Whether to enable the refresh management endpoint|true|

**Note**: **Each setting above must be prefixed with `management:endpoints:refresh`**.

#### Enable HTTP Access

The default path to the Refresh endpoint is computed by combining the global `path` prefix setting together with the `id` setting from above. The default path is `/refresh`.

The coding steps you take to enable HTTP access to the Refresh endpoint differs depending on the type of .NET application your are developing.  The sections which follow describe the steps needed for each of the supported application types.

##### ASP.NET Core App

Refer to the [HTTP Access ASP.NET Core](#http-access-asp-net-core) section below to see the overall steps required to enable HTTP access to endpoints in an ASP.NET Core application.

To add the Refresh actuator to the service container, use the `AddRefreshActuator()` extension method from [EndpointServiceCollectionExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointCore/Refresh/EndpointServiceCollectionExtensions.cs).

To add the Refresh actuator middleware to the ASP.NET Core pipeline, use the `UseRefreshActuator()` extension method from [EndpointApplicationBuilderExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointCore/Refresh/EndpointApplicationBuilderExtensions.cs).

##### ASP.NET 4.x App

Refer to the [HTTP Access ASP.NET 4.x](#http-access-asp-net-4-x) section below to see the overall steps required to enable HTTP access to endpoints in a 4.x application.

To add the Refresh actuator endpoint, use the `UseRefreshActuator()` method from [ActuatorConfigurator](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointWeb/ActuatorConfigurator.cs).

##### ASP.NET OWIN App

Refer to the [HTTP Access ASP.NET OWIN](#http-access-asp-net-owin) section below to see the overall steps required to enable HTTP access to endpoints in an ASP.NET 4.x OWIN application.

To add the Refresh actuator middleware to the ASP.NET OWIN pipeline, use the `UseRefreshActuator()` extension method from [RefreshEndpointAppBuilderExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointOwin/Refresh/RefreshEndpointAppBuilderExtensions.cs).

