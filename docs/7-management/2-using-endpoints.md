## Usage

Steeltoe provides a base set of endpoint functionality, along with several implementations for exposing the endpoints over HTTP. HTTP implementations are provided with ASP.NET Core middleware, OWIN middleware and HTTP Modules. Should you wish to expose the core endpoint functionality over some protocol other than HTTPS, you are free to provide your own implementation.

Regardless of the endpoint exposure method you select, you should understand how the .NET [Configuration service](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration) works before starting to use the management endpoints. You need at least a basic understanding of the `ConfigurationBuilder` and how to add providers to the builder to configure the endpoints.

When developing ASP.NET Core applications, you should also understand how the ASP.NET Core [Startup](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/startup) class is used in configuring the application services for the app. Pay particular attention to the usage of the `ConfigureServices()` and `Configure()` methods.

When adding Steeltoe Management endpoints to your ASP.NET 4.x applications, you can choose between using HTTP modules and OWIN middleware. If you select HTTP modules, you should be familiar with `Global.asax.cs` and how it is used in initializing and configuring your application. If you select the OWIN middleware approach, you should be familiar with how the Startup class is used in configuring application middleware. The rest of this document will refer to the HTTP Module implementation simply as ASP.NET 4.x, and the OWIN implementation as ASP.NET OWIN.

>NOTE: You may wish to select the OWIN implementation for your ASP.NET 4.x application when you don't want to depend on `System.Web` or you also plan to use Steeltoe security providers for authentication/authorization on Cloud Foundry.

The following table describes the available Steeltoe management endpoints that can be used in an application:

|ID|Description|
|---|---|
|**hypermedia**|Provides hypermedia endpoint for discovery of all available endpoints|
|**cloudfoundry**|Enables management endpoint integration with Cloud Foundry|
|**health**|Customizable endpoint that gathers application health information|
|**info**|Customizable endpoint that gathers arbitrary application information (such as Git Build info)|
|**loggers**|Gathers existing loggers and allows modification of logging levels|
|**trace**|Gathers a configurable set of trace information (such as the last 100 HTTP requests)|
|**refresh**|Triggers the application configuration to be reloaded|
|**env**|Reports the keys and values from the applications configuration|
|**mappings**|Reports the configured ASP.NET routes and route templates|
|**metrics**|Reports the collected metrics for the application|
|**dump**|Generates and reports a snapshot of the application's threads (Windows only)|
|**heapdump**|Generates and downloads a mini-dump of the application (Windows only)|

Each endpoint has an associated ID. When you want to expose that endpoint over HTTP, that ID is used in the mapped URL that exposes the endpoint. For example, the `health` endpoint below is mapped to `/health`.

>NOTE: When you want to integrate with the [Pivotal Apps Manager](https://docs.pivotal.io/pivotalcf/2-0/console/index.html), you need to configure the global management path prefix to be `/cloudfoundryapplication`. To do so, add `management:endpoints:path=/cloudfoundryapplication` to your configuration.

### Add NuGet References

To use the management endpoints, you need to add a reference to the appropriate Steeltoe NuGet based on the type of the application you are building and what Dependency Injector you have chosen, if any.

The following table describes the available packages:

|App Type|Package|Description|
|---|---|---|
|All|`Steeltoe.Management.EndpointBase`|Base functionality, no dependency injection, no HTTP middleware.|
|ASP.NET Core|`Steeltoe.Management.EndpointCore`|Includes `EndpointBase`, adds ASP.NET Core DI, includes HTTP middleware,  no Pivotal Apps Manager integration. |
|ASP.NET Core|`Steeltoe.Management.CloudFoundryCore`|Includes `EndpointCore`, enables Pivotal Apps Manager integration. |
|ASP.NET 4.x|`Steeltoe.Management.EndpointWeb`|Includes `EndpointBase`, enables Pivotal Apps Manager integration.|
|ASP.NET 4.x OWIN|`Steeltoe.Management.EndpointOwin`|Includes `EndpointBase`, enables Pivotal Apps Manager integration.|
|ASP.NET 4.x OWIN with Autofac|`Steeltoe.Management.EndpointOwin.Autofac`|Includes `EndpointOwin`, adds Autofac DI, enables Pivotal Apps Manager integration.|

To add this type of NuGet to your project, add a `PackageReference` resembling the following:

```xml
<ItemGroup>
...
    <PackageReference Include="Steeltoe.Management.EndpointCore" Version= "2.2.0"/>
...
</ItemGroup>
```

or

```powershell
PM>Install-Package  Steeltoe.Management.EndpointWeb -Version 2.2.0
```

### Configure Global Settings

Endpoints can be configured by using the normal .NET [Configuration service](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration). You can globally configure settings that apply to all endpoints as well as configure settings that are specific to a particular endpoint.

All management endpoint settings should be placed under the prefix with the key `management:endpoints`. Any settings found under this prefix apply to all endpoints globally.

Settings that you want to apply to specific endpoints should be placed under the prefix with the key `management:endpoints:` + ID (for example, `management:endpoints:health`). Any settings you apply to a specific endpoint override any settings applied globally.

The following table describes the settings that you can apply globally:

|Key|Description|Default|
|---|---|---|
|enabled|Whether to enable all management endpoints|true|
|path|The path prefix applied to all endpoints when exposed over HTTP|`/`|

When you want to integrate with the [Pivotal Apps Manager](https://docs.pivotal.io/pivotalcf/2-0/console/index.html), you need to configure the global management path prefix to be `/cloudfoundryapplication`.

#### Exposing Endpoints

Since endpoints may contain sensitive information, only Health and Info are exposed by default. To change which endpoints are exposed, use the `include` and `exclude` properties:

|Property|Default|
|---|---|
|exposure:include | [`info`, `health`]|
|exposure:exclude | |

**Note**: **Each setting above must be prefixed with `management:endpoints:actuator`**. To select all endpoints,
`*`  can be used. For example, to expose everything except `env` and `refresh`, use the following property:

```json
"management": {
    "endpoints": {
        "actuator":{
            "exposure": {
                "include": [ "*" ],
                "exclude": [ "env", "refresh"]
            }
        }
    }
}
```

>NOTE: The exposure settings do not apply to endpoint routes mapped to the /cloudfoundryapplication context. If you add the Cloud Foundry endpoint, it will provide a route to access all endpoints without respecting the exposure settings through either the global path specified or its default of "/actuator". On the contrary, if you do not add either the Cloud Foundry or Hypermedia actuators, the default settings still apply. Adding endpoints other than health and info will require you to explicitly set the exposure setting. 

The upcoming sections show the settings that you can apply to specific endpoints.

