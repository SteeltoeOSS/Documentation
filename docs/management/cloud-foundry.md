### Cloud Foundry

The primary purpose of this endpoint is to enable integration with the Pivotal Apps Manager. This endpoint is similar to Hypermedia Actuator but is preconfigured for Apps Manager integration. When used, the Steeltoe Cloud Foundry management endpoint enables the following additional functionality on Cloud Foundry:

* Provides an alternate, secured route to the endpoints expected by Apps Manager and configured in your application.
* Exposes an endpoint that can be queried to return the IDs of and links to the enabled management endpoints in the application.
* Adds Cloud Foundry security middleware to the request pipeline, to secure access to the management endpoints by using security tokens acquired from the UAA.
* Adds extension methods that simplify adding the Steeltoe management endpoints necessary for Apps Manager integration with HTTP access to the application.

When adding this management endpoint to your application, the Cloud Foundry security middleware is added to the request processing pipeline of your application to enforce that, when a request is made of any of the management endpoints, a valid UAA access token is provided as part of that request. Additionally, the security middleware uses the token to determine whether the authenticated user has permission to access the management endpoint.

>NOTE: The Cloud Foundry security middleware is automatically disabled when your application is not running on Cloud Foundry (for example, running locally on your desktop).

#### Configure Settings

Typically, you need not do any additional configuration. However, the following table describes the additional settings that you could apply to the Cloud Foundry endpoint:

|Key|Description|Default|
|---|---|---|
|`Id`|The ID of the Cloud Foundry endpoint|""|
|`Enabled`|Whether to enable Cloud Foundry management endpoint|`true`|
|`ValidateCertificates`|Whether to validate server certificates|`true`|
|`ApplicationId`|The ID of the application used in permissions check|VCAP settings|
|`CloudFoundryApi`|The URL of the Cloud Foundry API|VCAP settings|

>NOTE: Each setting in the preceding table must be prefixed with `Management:Endpoints:cloudfoundry`.

#### Enable HTTP Access

The default path to the Cloud Foundry endpoint is computed by combining the global `Path` prefix setting together with the `Id` setting described in the previous section. The default path is `/cloudfoundryapplication`.

To add the Cloud Foundry actuator to the service container, you can use the `AddCloudFoundryActuator()` extension method from `EndpointServiceCollectionExtensions`.

To add the Cloud Foundry actuator and security middleware to the ASP.NET Core pipeline, use the `UseCloudFoundryActuator()` and `UseCloudFoundrySecurity()` extension methods from `EndpointApplicationBuilderExtensions`.
