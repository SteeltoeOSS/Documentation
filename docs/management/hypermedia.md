### Hypermedia

The purpose of this endpoint is to provide hypermedia for all the management endpoints configured in your application.

#### Base Context Path

This actuator also creates a base context path from which the endpoints can be accessed. The hypermedia actuator enables the following functionality:

* Exposes an endpoint that can be queried to return the IDs of and links to all of the enabled management endpoints in the application.
* Adds extension methods that simplify adding all of the Steeltoe management endpoints with HTTP access to the application.


#### Configure Settings

The following table describes the additional settings that you could apply to the Hypermedia endpoint:

|Key|Description|Default|
|---|---|---|
|`Id`|The ID of the Hypermedia endpoint|""|
|`Enabled`|Whether to enable the Hypermedia endpoint|`true`|

#### Enable HTTP Access

The default path to the Hypermedia endpoint is computed by combining the global `Path` prefix setting together with the `Id` setting described in the preceding section. The default path is `/actuator`.

To add the Cloud Foundry actuator to the service container, use the `AddHypermediaActuator()` extension method from `EndpointServiceCollectionExtensions`

To add the Cloud Foundry actuator and security middleware to the ASP.NET Core pipeline, use the `UseHypermediaActuator()`  extension methods from `EndpointApplicationBuilderExtensions`.
