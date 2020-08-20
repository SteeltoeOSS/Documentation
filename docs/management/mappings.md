### Mappings

You can use the Steeltoe mappings endpoint to return the MVC and WebAPI routes and route templates that are used by the application.

#### Configure Settings

The following table describes the settings that you can apply to the endpoint:

|Key|Description|Default|
|---|---|---|
|`Id`|The ID of the mappings endpoint|`mappings`|
|`Enabled`|Whether to enable the mappings management endpoint|`true`|

>NOTE: Each setting above must be prefixed with `Management:Endpoints:Mappings`.

#### Enable HTTP Access

The default path to the mappings endpoint is computed by combining the global `Path` prefix setting together with the `Id` setting described in the preceding section. The default path is <[Context-Path](hypermedia#base-context-path)>`/mappings`.

To add the mappings actuator to the service container, use the `AddMappingsActuator()` extension method from `EndpointServiceCollectionExtensions`.

To add the mappings actuator middleware to the ASP.NET Core pipeline, use the `UseMappingsActuator()` extension method from `EndpointApplicationBuilderExtensions`.
