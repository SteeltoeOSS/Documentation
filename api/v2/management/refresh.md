# Refresh

The Steeltoe Refresh endpoint can be used to cause the applications configuration to be reloaded and return the new values and keys currently in use in your application. The endpoint reloads the configuration using the applications `IConfigurationRoot`.

## Configure Settings

The following table describes the settings that you can apply to the endpoint:

|Key|Description|Default|
|---|---|---|
|id|The ID of the refresh endpoint|`refresh`|
|enabled|Whether to enable the refresh management endpoint|true|

**Note**: **Each setting above must be prefixed with `management:endpoints:refresh`**.

## Enable HTTP Access

The default path to the Refresh endpoint is computed by combining the global `path` prefix setting together with the `id` setting from above. The default path is  `/actuator/refresh`.

The coding steps you take to enable HTTP access to the Refresh endpoint differs depending on the type of .NET application your are developing.  The sections which follow describe the steps needed for each of the supported application types.

### ASP.NET Core App

To add the Refresh actuator to the service container, use the `AddRefreshActuator()` extension method from `EndpointServiceCollectionExtensions`.

To add the Refresh actuator middleware to the ASP.NET Core pipeline, use the `UseRefreshActuator()` extension method from `EndpointApplicationBuilderExtensions`.

### ASP.NET 4.x App

To add the Refresh actuator endpoint, use the `UseRefreshActuator()` method from `ActuatorConfigurator`.

### ASP.NET OWIN App

To add the Refresh actuator middleware to the ASP.NET OWIN pipeline, use the `UseRefreshActuator()` extension method from `RefreshEndpointAppBuilderExtensions`.

