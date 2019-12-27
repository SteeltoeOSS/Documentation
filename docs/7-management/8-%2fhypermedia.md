### Hypermedia

The purpose of this endpoint is to provide hypermedia for all the management endpoints configured in your application.
It also creates a base context path from which the endpoints can be accessed. The hypermedia actuator enables the following functionality:  
  
* Exposes an endpoint that can be queried to return the IDs of and links to all of the enabled management endpoints in the application.
* Adds extension methods that simplify adding all of the Steeltoe management endpoints with HTTP access to the application.

>NOTE: Adding Cloud Foundry and Hypermedia endpoint together will allow Pivotal Apps Manager integration along with the ability to access these endpoints on another route (by default /actuator). Using Cloud Foundry endpoint without Hypermedia endpoint allows Apps Manager integration, however  external clients cannot access the endpoints.  When Apps Manager integration is not needed the Hypermedia endpoint can be used by itself.

#### Configure Settings

The following table describes the additional settings that you could apply to the Hypermedia endpoint:

|Key|Description|Default|
|---|---|---|
|id|The ID of the Hypermedia endpoint|""|
|enabled|Whether to enable the Hypermedia endpoint|true|

#### Enable HTTP Access

The default path to the Hypermedia endpoint is computed by combining the global `path` prefix setting together with the `id` setting from above. The default path is `/actuator`.

The coding steps you take to enable HTTP access to the endpoint differs depending on the type of .NET application your are developing.  The sections which follow describe the steps needed for each of the supported application types.

##### ASP.NET Core App

To add the Cloud Foundry actuator to the service container, you can use the `AddHypermediaActuator()` extension method from `EndpointServiceCollectionExtensions`

To add the Cloud Foundry actuator and security middleware to the ASP.NET Core pipeline, use the `UseHypermediaActuator()`  extension methods from `EndpointApplicationBuilderExtensions`.

##### ASP.NET 4.x App

To add the Hypermedia actuator endpoint, use the `UseHypermediaActuator()` methods from `ActuatorConfigurator`.

##### ASP.NET OWIN App

To add the Hypermedia actuator to the ASP.NET OWIN pipeline, use the `UseHypermediaActuator()` from `HypermediaEndpointAppBuilderExtensions`.

