### Env

The Steeltoe Env endpoint can be used to query and return the configuration values and keys currently in use in your application. The endpoint returns the keys and values from the applications `IConfiguration`.

#### Configure Settings

The following table describes the settings that you can apply to the endpoint:

|Key|Description|Default|
|---|---|---|
|id|The ID of the env endpoint|`env`|
|enabled|Whether to enable the env management endpoint|true|
|keysToSanitize|Keys that should be sanitized. Keys can be simple strings that the property ends with or regex expressions|```["password", "secret", "key", "token", ".*credentials.*", "vcap_services"]```|

**Note**: **Each setting above must be prefixed with `management:endpoints:env`**.

#### Enable HTTP Access

The default path to the Env endpoint is computed by combining the global `path` prefix setting together with the `id` setting from above. The default path is `/env`.

The coding steps you take to enable HTTP access to the Env endpoint differs depending on the type of .NET application your are developing.  The sections which follow describe the steps needed for each of the supported application types.

##### ASP.NET Core App

Refer to the [HTTP Access ASP.NET Core](#http-access-asp-net-core) section below to see the overall steps required to enable HTTP access to endpoints in an ASP.NET Core application.

To add the Env actuator to the service container, use the `AddEnvActuator()` extension method from [EndpointServiceCollectionExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointCore/Env/EndpointServiceCollectionExtensions.cs).

To add the Env actuator middleware to the ASP.NET Core pipeline, use the `UseEnvActuator()` extension method from [EndpointApplicationBuilderExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointCore/Env/EndpointApplicationBuilderExtensions.cs).

##### ASP.NET 4.x App

Refer to the [HTTP Access ASP.NET 4.x](#http-access-asp-net-4-x) section below to see the overall steps required to enable HTTP access to endpoints in a 4.x application.

To add the Env actuator endpoint, use the `UseEnvActuator()` method from [ActuatorConfigurator](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointWeb/ActuatorConfigurator.cs).

##### ASP.NET OWIN App

Refer to the [HTTP Access ASP.NET OWIN](#http-access-asp-net-owin) section below to see the overall steps required to enable HTTP access to endpoints in an ASP.NET 4.x OWIN application.

To add the Env actuator middleware to the ASP.NET OWIN pipeline, use the `UseEnvActuator()` extension method from [EnvEndpointAppBuilderExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointOwin/Env/EnvEndpointAppBuilderExtensions.cs).

