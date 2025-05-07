# Mappings

The Steeltoe mappings endpoint can be used to return the MVC and WebAPI Routes and Route templates used by the application.

## Configure Settings

The following table describes the settings that you can apply to the endpoint:

|Key|Description|Default|
|---|---|---|
|id|The ID of the mappings endpoint|`mappings`|
|enabled|Whether to enable the mappings management endpoint|true|

**Note**: **Each setting above must be prefixed with `management:endpoints:mappings`**.

## Enable HTTP Access

The default path to the Mappings endpoint is computed by combining the global `path` prefix setting together with the `id` setting from above. The default path is  `/actuator/mappings`.

The coding steps you take to enable HTTP access to the Mappings endpoint differs depending on the type of .NET application your are developing.  The sections which follow describe the steps needed for each of the supported application types.

### ASP.NET Core App

To add the Mappings actuator to the service container, use the `AddMappingsActuator()` extension method from `EndpointServiceCollectionExtensions`.

To add the Mappings actuator middleware to the ASP.NET Core pipeline, use the `UseMappingsActuator()` extension method from `EndpointApplicationBuilderExtensions`.

### ASP.NET 4.x App

To add the Mappings actuator endpoint, use the `UseMappingsActuator()` method from `ActuatorConfigurator`.

By default, the endpoint will return the Routes and Route templates from the apps global `RouteTable`.  If you wish to expose WebAPI routes, in addition to those from the `RouteTable`, provide a reference to the `IApiExplorer` obtained from `GlobalConfiguration.Configuration.Services.GetApiExplorer()`.

### ASP.NET OWIN App

To add the Mappings actuator middleware to the ASP.NET OWIN pipeline, use the `UseMappingsActuator()` extension method from `MappingsEndpointAppBuilderExtensions`.

You must provide a reference to the `IApiExplorer` obtained from `GlobalConfiguration.Configuration.Services.GetApiExplorer()` when using this endpoint in a OWIN based app.

