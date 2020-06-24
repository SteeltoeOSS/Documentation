### Refresh

You can use the Steeltoe refresh endpoint to cause the application's configuration to be reloaded and return the new values and keys currently in use in your application. The endpoint reloads the configuration by using the application's `IConfigurationRoot`.

#### Configure Settings

The following table describes the settings that you can apply to the endpoint:

|Key|Description|Default|
|---|---|---|
|`id`|The ID of the refresh endpoint|`refresh`|
|`enabled`|Whether to enable the refresh management endpoint|`true`|

>NOTE: Each setting must be prefixed with `management:endpoints:refresh`.

#### Enable HTTP Access

The default path to the refresh endpoint is computed by combining the global `path` prefix setting together with the `id` setting described in the preceding section. The default path is `/refresh`.

The coding steps you take to enable HTTP access to the refresh endpoint differ, depending on the type of .NET application you are developing. The sections that follow describe the steps needed for each of the supported application types.

##### ASP.NET Core App

To add the refresh actuator to the service container, use the `AddRefreshActuator()` extension method from `EndpointServiceCollectionExtensions`.

To add the refresh actuator middleware to the ASP.NET Core pipeline, use the `UseRefreshActuator()` extension method from `EndpointApplicationBuilderExtensions`.

##### ASP.NET 4.x App

To add the refresh actuator endpoint, use the `UseRefreshActuator()` method from `ActuatorConfigurator`.

##### ASP.NET OWIN App

To add the refresh actuator middleware to the ASP.NET OWIN pipeline, use the `UseRefreshActuator()` extension method from `RefreshEndpointAppBuilderExtensions`.
