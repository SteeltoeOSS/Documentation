### Refresh

You can use the Steeltoe refresh endpoint to cause the application's configuration to be reloaded and return the new values and keys currently in use in your application. The endpoint reloads the configuration by using the application's `IConfigurationRoot`.

#### Configure Settings

The following table describes the settings that you can apply to the endpoint:

|Key|Description|Default|
|---|---|---|
|`Id`|The ID of the refresh endpoint|`refresh`|
|`Enabled`|Whether to enable the refresh management endpoint|`true`|

>NOTE: Each setting must be prefixed with `Management:Endpoints:Refresh`.

#### Enable HTTP Access

The default path to the refresh endpoint is computed by combining the global `Path` prefix setting together with the `Id` setting described in the preceding section. The default path is <[Context-Path](hypermedia#base-context-path)>`/refresh`.

To add the refresh actuator to the service container, use the `AddRefreshActuator()` extension method from `EndpointServiceCollectionExtensions`.

To add the refresh actuator middleware to the ASP.NET Core pipeline, use the `UseRefreshActuator()` extension method from `EndpointApplicationBuilderExtensions`.
