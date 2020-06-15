### Mappings

You can use the Steeltoe mappings endpoint to return the MVC and WebAPI routes and route templates that are used by the application.

#### Configure Settings

The following table describes the settings that you can apply to the endpoint:

|Key|Description|Default|
|---|---|---|
|`id`|The ID of the mappings endpoint|`mappings`|
|`enabled`|Whether to enable the mappings management endpoint|`true`|

>NOTE: Each setting above must be prefixed with `management:endpoints:mappings`.

#### Enable HTTP Access

The default path to the mappings endpoint is computed by combining the global `path` prefix setting together with the `id` setting described in the preceding section. The default path is `/mappings`.

The coding steps you take to enable HTTP access to the mappings endpoint differ, depending on the type of .NET application your are developing. The sections that follow describe the steps needed for each of the supported application types.

##### ASP.NET Core App

To add the mappings actuator to the service container, use the `AddMappingsActuator()` extension method from `EndpointServiceCollectionExtensions`.

To add the mappings actuator middleware to the ASP.NET Core pipeline, use the `UseMappingsActuator()` extension method from `EndpointApplicationBuilderExtensions`.

##### ASP.NET 4.x App

To add the mappings actuator endpoint, use the `UseMappingsActuator()` method from `ActuatorConfigurator`.

By default, the endpoint returns the routes and route templates from the application's global `RouteTable`. To expose WebAPI routes, in addition to those from the `RouteTable`, provide a reference to the `IApiExplorer` obtained from `GlobalConfiguration.Configuration.Services.GetApiExplorer()`.

##### ASP.NET OWIN App

To add the mappings actuator middleware to the ASP.NET OWIN pipeline, use the `UseMappingsActuator()` extension method from `MappingsEndpointAppBuilderExtensions`.

You must provide a reference to the `IApiExplorer` obtained from `GlobalConfiguration.Configuration.Services.GetApiExplorer()` when using this endpoint in an OWIN based app.
