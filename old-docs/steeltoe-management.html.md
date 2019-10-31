---
title: Management
order: 70
date: 2016/4/1
tags:
---

Steeltoe includes a number of optional features you can add to your applications to aid in monitoring and managing it while it runs in production. These features are implemented as a number of management endpoints that you can easily add to your application.

The way the endpoints are exposed and used depends on the type of technology you choose in exposing the functionality of the endpoint. Out of the box, Steeltoe provides several easy ways to expose these endpoints over HTTP in .NET applications. Of course, you can build and use whatever you would like to meet your needs.

When you expose the endpoints over HTTP, you can also integrate the endpoints with the [Pivotal Apps Manager](https://docs.pivotal.io/pivotalcf/2-0/console/index.html). The [quick start](#1-1-quick-start), explores this integration in more depth. You should read the [Using Actuators with Apps Manager section](https://docs.pivotal.io/pivotalcf/2-0/console/using-actuators.html) of the Pivotal Cloud Foundry documentation for more details.

>NOTE: Depending on your hosting environment, service instances you create for the purpose of exploring the Quick Starts on this page may have a cost associated.

# 0.0 Initialize Dev Environment

All of the Steeltoe sample applications are in the same repository. If you have not already done so, use git to clone the [Steeltoe Samples](https://github.com/SteeltoeOSS/Samples) repository or download with your browser from GitHub. The following command shows how to use Git to get the samples:

```bash
git clone https://github.com/SteeltoeOSS/Samples.git
```

Make sure your Cloud Foundry CLI tools are logged in and targeting the correct org and space, as shown in the following listing:

```bash
cf login [-a API_URL] [-u USERNAME] [-p PASSWORD] [-o ORG] [-s SPACE] [--skip-ssl-validation]
# or
cf target -o <YourOrg> -s <YourSpace>
```

>NOTE: All Management ASP.NET Core samples have a base path of `Samples/Management/src/AspDotNetCore` and samples that are built on ASP.NET 4.x can be found in `Samples/Management/src/AspDotNet4`.

# 1.0 Management Endpoints

The following table describes all of the currently available Steeltoe management endpoints:

|ID|Description|
|---|---|
|**hypermedia**|Provides hypermedia endpoint for discovery of all available endpoints|
|**cloudfoundry**|Enables management endpoint integration with Pivotal Cloud Foundry|
|**health**|Customizable endpoint that reports application health information|
|**info**|Customizable endpoint that reports arbitrary application information (such as Git Build info and other details)|
|**loggers**|Allows remote access and modification of logging levels in a .NET application|
|**trace**|Reports a configurable set of trace information (such as the last 100 HTTP requests)|
|**refresh**|Triggers the application configuration to be reloaded|
|**env**|Reports the keys and values from the applications configuration|
|**mappings**|Reports the configured ASP.NET routes and route templates|
|**metrics**|Reports the collected metrics for the application|
|**dump**|Generates and reports a snapshot of the applications threads (Windows only)|
|**heapdump**|Generates and downloads a mini-dump of the application (Windows only)|

More detail on each endpoint is provided in upcoming sections.

Note that the Steeltoe Management endpoints themselves support the following .NET application types:

* ASP.NET Core and ASP.NET 4.x
* Console apps (.NET Framework and .NET Core)

Steeltoe currently includes support for exposing the Management endpoints over HTTP with ASP.NET.

In addition to the [Quick Start](#1-1-quick-start), there are other Steeltoe sample applications that you can refer to in order to help you understand how to use these endpoints, including:

* [MusicStore](https://github.com/SteeltoeOSS/Samples/tree/master/MusicStore): A sample application showing how to use all of the Steeltoe components together in a ASP.NET Core application. This is a microservices-based application built from the ASP.NET Core MusicStore reference application provided by Microsoft.

## 1.1 Quick Start

This quick start consists of an ASP.NET Core sample application showing how to use all of the management endpoints on Cloud Foundry and integrating the endpoint information over HTTP with [Pivotal Apps Manager](https://docs.pivotal.io/pivotalcf/2-0/console/index.html).

>NOTE: There are additional [samples](https://github.com/SteeltoeOSS/Samples/tree/dev/Management/src/AspDotNet4) illustrating usage in ASP.NET 4.x applications.

For more information on how to use the Apps Manager with the Management endpoints, read [Using Spring Boot Actuators with Apps Manager](https://docs.pivotal.io/pivotalcf/2-0/console/using-actuators.html).

>NOTE: Steeltoe Management endpoints are not exclusive to Cloud Foundry.

### 1.1.1 Locate Sample

To get started, change directory to where the samples are stored, as follows:

```bash
cd Samples/Management/src/AspDotNetCore/CloudFoundry
```

### 1.1.2 Create Service

To show how the out-of-the-box MySql Health contributor can be used to monitor the health of a connection to a back-end database, you must first create an instance of a MySql service in a org and space, as shown in the following example:

```bash
# Create a MySql service instance on Cloud Foundry
cf create-service p-mysql 100mb myMySqlService

# Make sure the service is ready
cf services
```

### 1.1.3 Publish Sample

See [Publish Sample](#publish-sample) for instructions on how to publish this sample to either Linux or Windows.

### 1.1.4 Push Sample

See [Push Sample](#push-sample) for instructions on how to push this sample to either Linux or Windows on Cloud Foundry.

### 1.1.5 Observe Logs

To see the logs as you startup the application use the `cf` CLI to tail the apps logs. (for example, `cf logs actuator`)

On a Linux cell, you should see something resembling the following during startup:

```bash
2016-06-01T09:14:14.38-0600 [CELL/0]     OUT Creating container
2016-06-01T09:14:15.93-0600 [CELL/0]     OUT Successfully created container
2016-06-01T09:14:17.14-0600 [CELL/0]     OUT Starting health monitoring of container
2016-06-01T09:14:21.04-0600 [APP/0]      OUT Hosting environment: Development
2016-06-01T09:14:21.04-0600 [APP/0]      OUT Content root path: /home/vcap/app
2016-06-01T09:14:21.04-0600 [APP/0]      OUT Now listening on: http://*:8080
2016-06-01T09:14:21.04-0600 [APP/0]      OUT Application started. Press Ctrl+C to shut down.
2016-06-01T09:14:21.41-0600 [CELL/0]     OUT Container became healthy
```

On Windows cells, you should see something slightly different but with the same information.

### 1.1.6 Accessing Endpoints

At this point the app is running.

You can access the management endpoints exposed by Steeltoe by using the [Pivotal Apps Manager](https://docs.pivotal.io/pivotalcf/2-0/console/index.html).

The Steeltoe Management framework exposes Spring Boot Actuator compatible endpoints that can be used within the Pivotal Apps Manager. By using the Apps Manager, you can view the Apps Health, Build Information (such as Git info and other details), and recent Request/Response Traces. You can also manage the applications logging levels. On a Windows cell, you can also get thread snapshots and generate and download mini-dumps of the application.

Check out the Pivotal Apps Manager documentation section, [Using Spring Boot Actuators](https://docs.pivotal.io/pivotalcf/2-0/console/using-actuators.html), for more information on how to use the Apps Manager.

>NOTE: In order too access the endpoints from Apps Manager, you must have the `Space Developer` permission in the space where your application is deployed.

### 1.1.7 Understand Sample

The sample was created using the .NET Core tooling `mvc` template (`dotnet new mvc`)  and then modified to use the Steeltoe frameworks.

To gain an understanding of the Steeltoe related changes to the generated template code,  examine the following files:

* `CloudFoundry.csproj`: Multiple changes as follows:
  * GitInfo `PackageReference` added to gather git build information for the App Info endpoint.
  * _GitProperties `Target` added to build the `git.properties` file used by the Git App Info contributor.
  * Added `Steeltoe.Management.CloudFoundryCore` package reference to bring in all Management endpoints for use on Cloud Foundry.
  * Added various MySQL package references to enable the use of MySQL in the application.
* `Program.cs`: Added code to the `ConfigurationBuilder` in order to pick up Cloud Foundry configuration values when pushed to Cloud Foundry, to use Cloud Foundry hosting and to setup Steeltoe dynamic logging.
* `Startup.cs`: Multiple changes, as follows:
  * Code added to `ConfigureServices()` to use the Steeltoe MySQL connector and the out-of-the-box MySql health contributor.
  * Code added to `ConfigureServices()` to add a custom info endpoint contributor, `ArbitraryInfoContributor`, to the service container.
  * Code added to `ConfigureServices()` to add a custom health endpoint contributor, `CustomHealthContributor`, to the service container.
  * Code added to `ConfigureServices()` to add all of the Steeltoe Management endpoints to the service container.
  * Code added to `Configure()` to add the Steeltoe Management middleware to the pipeline.
* `ArbitraryInfoContributor.cs`: A custom info contributor.
* `CustomHealthContributor.cs`: A custom health contributor.

## 1.2 Usage

Steeltoe provides a base set of endpoint functionality, along with several implementations for exposing the endpoints over HTTP. HTTP implementations are provided with ASP.NET Core middleware, OWIN middleware and HTTP Modules. Should you wish to expose the core endpoint functionality over some protocol other than HTTPS, you are free to provide your own implementation.

Regardless of the endpoint exposure method you select, you should understand how the .NET [Configuration service](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration) works before starting to use the management endpoints. You need at least a basic understanding of the `ConfigurationBuilder` and how to add providers to the builder to configure the endpoints.

When developing ASP.NET Core applications, you should also understand how the ASP.NET Core [Startup](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/startup) class is used in configuring the application services for the app. Pay particular attention to the usage of the `ConfigureServices()` and `Configure()` methods.

When adding Steeltoe Management endpoints to your ASP.NET 4.x applications, you can choose between using HTTP modules and OWIN middleware. If you select HTTP modules, you should be familiar with `Global.asax.cs` and how it is used in initializing and configuring your application. If you select the OWIN middleware approach, you should be familiar with how the Startup class is used in configuring application middleware. The rest of this document will refer to the HTTP Module implementation simply as ASP.NET 4.x, and the OWIN implementation as ASP.NET OWIN.

>NOTE: You may wish to select the OWIN implementation for your ASP.NET 4.x application when you don't want to depend on `System.Web` or you also plan to use [Steeltoe security providers](../steeltoe-security) for authentication/authorization on Cloud Foundry.

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

>NOTE: When you want to integrate with the [Pivotal Apps Manager](https://docs.pivotal.io/pivotalcf/2-0/console/index.html), you need to configure the global management path prefix, as described in the [Endpoint Settings](#1-2-2-settings) section, to be `/cloudfoundryapplication`. To do so, add `management:endpoints:path=/cloudfoundryapplication` to your configuration.

### 1.2.1 Add NuGet References

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

### 1.2.2 Configure Global Settings

Endpoints can be configured by using the normal .NET [Configuration service](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration). You can globally configure settings that apply to all endpoints as well as configure settings that are specific to a particular endpoint.

All management endpoint settings should be placed under the prefix with the key `management:endpoints`. Any settings found under this prefix apply to all endpoints globally.

Settings that you want to apply to specific endpoints should be placed under the prefix with the key `management:endpoints:` + ID (for example, `management:endpoints:health`). Any settings you apply to a specific endpoint override any settings applied globally.

The following table describes the settings that you can apply globally:

|Key|Description|Default|
|---|---|---|
|enabled|Whether to enable all management endpoints|true|
|path|The path prefix applied to all endpoints when exposed over HTTP|`/`|

When you want to integrate with the [Pivotal Apps Manager](https://docs.pivotal.io/pivotalcf/2-0/console/index.html), you need to configure the global management path prefix to be `/cloudfoundryapplication`.

#### 1.2.3 Exposing Endpoints

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

### 1.2.3 Hypermedia

The purpose of this endpoint is to provide hypermedia for all the management endpoints configured in your application.
It also creates a base context path from which the endpoints can be accessed. The hypermedia actuator enables the following functionality:  
  
* Exposes an endpoint that can be queried to return the IDs of and links to all of the enabled management endpoints in the application.
* Adds extension methods that simplify adding all of the Steeltoe management endpoints with HTTP access to the application.

>NOTE: Adding Cloud Foundry and Hypermedia endpoint together will allow Pivotal Apps Manager integration along with the ability to access these endpoints on another route (by default /actuator). Using Cloud Foundry endpoint without Hypermedia endpoint allows Apps Manager integration, however  external clients cannot access the endpoints.  When Apps Manager integration is not needed the Hypermedia endpoint can be used by itself.

#### 1.2.3.1 Configure Settings

The following table describes the additional settings that you could apply to the Hypermedia endpoint:

|Key|Description|Default|
|---|---|---|
|id|The ID of the Hypermedia endpoint|""|
|enabled|Whether to enable the Hypermedia endpoint|true|

#### 1.2.3.2 Enable HTTP Access

The default path to the Hypermedia endpoint is computed by combining the global `path` prefix setting together with the `id` setting from above. The default path is `/actuator`.

The coding steps you take to enable HTTP access to the endpoint differs depending on the type of .NET application your are developing.  The sections which follow describe the steps needed for each of the supported application types.

##### 1.2.3.2.1 ASP.NET Core App

Refer to the [HTTP Access ASP.NET Core](#http-access-asp-net-core) section below to see the overall steps required to enable HTTP access to endpoints in an ASP.NET Core application.

To add the Cloud Foundry actuator to the service container, you can use the `AddHypermediaActuator()` extension method from [EndpointServiceCollectionExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointCore/Hypermedia/EndpointServiceCollectionExtensions.cs).

To add the Cloud Foundry actuator and security middleware to the ASP.NET Core pipeline, use the `UseHypermediaActuator()`  extension methods from [EndpointApplicationBuilderExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointCore/Hypermedia/EndpointApplicationBuilderExtensions.cs).

##### 1.2.3.2.2 ASP.NET 4.x App

Refer to the [HTTP Access ASP.NET 4.x](#http-access-asp-net-4-x) section below to see the overall steps required to enable HTTP access to endpoints in a 4.x application.

To add the Hypermedia actuator endpoint, use the `UseHypermediaActuator()` methods from [ActuatorConfigurator](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointWeb/ActuatorConfigurator.cs).

##### 1.2.3.2.3 ASP.NET OWIN App

Refer to the [HTTP Access ASP.NET OWIN](#http-access-asp-net-owin) section below to see the overall steps required to enable HTTP access to endpoints in an ASP.NET 4.x OWIN application.

To add the Hypermedia actuator to the ASP.NET OWIN pipeline, use the `UseHypermediaActuator()` from [HypermediaEndpointAppBuilderExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointOwin/Hypermedia/HypermediaEndpointAppBuilderExtensions.cs).

### 1.2.4 Cloud Foundry

The primary purpose of this endpoint is to enable integration with the Pivotal Apps Manager. This endpoint is similar to Hypermedia Actuator but is preconfigured for Apps Manager integration. When used, the Steeltoe Cloud Foundry management endpoint enables the following additional functionality on Cloud Foundry:

* Provides an alternate, secured route to the endpoints expected by Apps Manager and configured in your application
* Exposes an endpoint that can be queried to return the IDs of and links to the enabled management endpoints in the application.
* Adds Cloud Foundry security middleware to the request pipeline, to secure access to the management endpoints by using security tokens acquired from the UAA.
* Adds extension methods that simplify adding the Steeltoe management endpoints necessary for Apps Manager integration with HTTP access to the application.

When adding this management endpoint to your application, the [Cloud Foundry security middleware](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointCore/CloudFoundry/CloudFoundrySecurityMiddleware.cs) is added to the request processing pipeline of your application to enforce that when a request is made of any of the management endpoints, a valid UAA access token is provided as part of that request. Additionally, the security middleware uses the token to determine whether the authenticated user has permissions to access the management endpoint.

>NOTE: The Cloud Foundry security middleware is automatically disabled when your application is not running on Cloud Foundry (for example, running locally on your desktop).

#### 1.2.4.1 Configure Settings

Typically, you need not do any additional configuration. However, the following table describes the additional settings that you could apply to the Cloud Foundry endpoint:

|Key|Description|Default|
|---|---|---|
|id|The ID of the Cloud Foundry endpoint|""|
|enabled|Whether to enable Cloud Foundry management endpoint|true|
|validateCertificates|Whether to validate server certificates|true|
|applicationId|The ID of the application used in permissions check|VCAP settings|
|cloudFoundryApi|The URL of the Cloud Foundry API|VCAP settings|

**Note**: **Each setting above must be prefixed with `management:endpoints:cloudfoundry`**.

#### 1.2.4.2 Enable HTTP Access

The default path to the Cloud Foundry endpoint is computed by combining the global `path` prefix setting together with the `id` setting from above. The default path is `/cloudfoundryapplication`.

The coding steps you take to enable HTTP access to the endpoint differs depending on the type of .NET application your are developing.  The sections which follow describe the steps needed for each of the supported application types.

##### 1.2.4.2.1 ASP.NET Core App

Refer to the [HTTP Access ASP.NET Core](#http-access-asp-net-core) section below to see the overall steps required to enable HTTP access to endpoints in an ASP.NET Core application.

To add the Cloud Foundry actuator to the service container, you can use the `AddCloudFoundryActuator()` extension method from [EndpointServiceCollectionExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointCore/CloudFoundry/EndpointServiceCollectionExtensions.cs).

To add the Cloud Foundry actuator and security middleware to the ASP.NET Core pipeline, use the `UseCloudFoundryActuator()` and `UseCloudFoundrySecurity()` extension methods from [EndpointApplicationBuilderExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointCore/CloudFoundry/EndpointApplicationBuilderExtensions.cs).

##### 1.2.4.2.2 ASP.NET 4.x App

Refer to the [HTTP Access ASP.NET 4.x](#http-access-asp-net-4-x) section below to see the overall steps required to enable HTTP access to endpoints in a 4.x application.

To add the Cloud Foundry actuator endpoint, use the `UseCloudFoundrySecurity()` and `UseCloudFoundryActuator()` methods from [ActuatorConfigurator](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointWeb/ActuatorConfigurator.cs).

##### 1.2.4.2.3 ASP.NET OWIN App

Refer to the [HTTP Access ASP.NET OWIN](#http-access-asp-net-owin) section below to see the overall steps required to enable HTTP access to endpoints in an ASP.NET 4.x OWIN application.

To add the Cloud Foundry actuator and security middleware to the ASP.NET OWIN pipeline, use the `UseCloudFoundryActuator()` from [CloudFoundryEndpointAppBuilderExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointOwin/CloudFoundry/CloudFoundryEndpointAppBuilderExtensions.cs).
and `UseCloudFoundrySecurityMiddleware()` from [CloudFoundrySecurityAppBuilderExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointOwin/CloudFoundry/CloudFoundrySecurityAppBuilderExtensions.cs).

### 1.2.5 Health

The Steeltoe Health management endpoint can be used to check and return the status of your running application. It can often be used by monitoring software to alert someone if a production system goes down. The information exposed by the `health` endpoint depends on the `management:endpoints:health:showdetails` property which can be configured with one of the following values:

|Name|Description|
|---|---|
|`never`|Details are never shown.|
|`whenauthorized`|Details are only shown to authorized users. |  
|`always`|Details are always shown.|

The default value is `always`. Authorized roles can be configured using `management:endpoints:health:claim or management:endpoints:health:role`. A user is considered to be authorized when they are in the given role or have the specified claim. For example:

```json
"management": {
    "endpoints": {
        "health": {
            "showdetails": "whenauthorized",
            "claim": {
                "type": "health_actuator",
                "value": "see_details"
            }
        }
    }
}
```

Health information is collected from all [IHealthContributor](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointBase/Health/IHealthContributor.cs) implementations provided to the [HealthEndpoint](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointBase/Health/HealthEndpoint.cs). Steeltoe includes several `IHealthContributor` implementations out of the box that you can use, and, more importantly, you can write your own.

By default, the final application health state is computed by the [IHealthAggregator](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointBase/Health/IHealthAggregator.cs) that is provided to the `HealthEndpoint`. The `IHealthAggregator` is responsible for sorting out all of the returned statuses from each `IHealthContributor` and deriving an overall application health state. The [DefaultHealthAggregator](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointBase/Health/DefaultHealthAggregator.cs) returns the `worst` status returned from all of the `IHealthContributors`.

#### 1.2.5.1 Health Contributors

At present, Steeltoe provides the following `IHealthContributor` implementations you can choose from:

|Name|Description|
|---|---|
|[DiskSpaceContributor](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointBase/Health/Contributor/DiskSpaceContributor.cs)|checks for low disk space, configure using [DiskSpaceContributorOptions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointBase/Health/Contributor/DiskSpaceContributorOptions.cs) |
|[RabbitMQHealthContributor](https://github.com/SteeltoeOSS/Connectors/blob/master/src/Steeltoe.CloudFoundry.ConnectorBase/Queue/RabbitMQHealthContributor.cs)|checks RabbitMQ connection health|
|[RedisHealthContributor](https://github.com/SteeltoeOSS/Connectors/blob/master/src/Steeltoe.CloudFoundry.ConnectorBase/Cache/RedisHealthContributor.cs)|checks Redis cache connection health|
|[RelationalHealthContributor](https://github.com/SteeltoeOSS/Connectors/blob/master/src/Steeltoe.CloudFoundry.ConnectorBase/Relational/RelationalHealthContributor.cs)|checks relational database connection health (MySql, Postgres, SqlServer)|

Many of the above health contributors are located in the [Steeltoe Connectors](https://github.com/SteeltoeOSS/Connectors) package and are made available to your application when you reference the Connectors package.

If you wish to provide custom health information for your application, create a class that implements the `IHealthContributor` interface and then add that to the `HealthEndpoint`. Details on how to add a contributor to the endpoint is provided below.

The following is an example `IHealthContributor` that always returns a `HealthStatus` of `UP`.

```csharp
public class CustomHealthContributor : IHealthContributor
{
    public string Id => "CustomHealthContributor";

    public HealthCheckResult Health()
    {
        var result = new HealthCheckResult {
            // this is used as part of the aggregate, it is not directly part of the middleware response
            Status = HealthStatus.UP,
            Description = "This health check does not check anything"
        };
        result.Details.Add("status", HealthStatus.UP.ToString());
        return result;
    }
}
```

#### 1.2.5.2 Configure Settings

The following table describes the settings that you can apply to the endpoint.

|Key|Description|Default|
|---|---|---|
|id|The ID of the health endpoint|`health`|
|enabled|Whether to enable the health management endpoint|true|
|sensitive|Currently not used|false|
|requiredPermissions|The user permissions required on Cloud Foundry to access endpoint|RESTRICTED|

**Note**: **Each setting above must be prefixed with `management:endpoints:health`**.

#### 1.2.5.3 Enable HTTP Access

The default path to the Health endpoint is computed by combining the global `path` prefix setting together with the `id` setting from above. The default path is `/health`.

The coding steps you take to enable HTTP access to the Health endpoint together with how to use custom Health contributors differs depending on the type of .NET application your are developing.  The sections which follow describe the steps needed for each of the supported application types.

##### 1.2.5.3.1 ASP.NET Core App

Refer to the [HTTP Access ASP.NET Core](#http-access-asp-net-core) section below to see the overall steps required to enable HTTP access to endpoints in an ASP.NET Core application.

To add the Health actuator to the service container, use any one of the `AddHealthActuator()` extension methods from [EndpointServiceCollectionExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointCore/Health/EndpointServiceCollectionExtensions.cs).

To add the Health actuator middleware to the ASP.NET Core pipeline, use the `UseHealthActuator()` extension method from [EndpointApplicationBuilderExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointCore/Health/EndpointApplicationBuilderExtensions.cs).

The following example shows how enable the Health endpoint and to add a custom `IHealthContributor` to the service container by adding `CustomHealthContributor` as a singleton. Once that's done the Health endpoint will discover and use it during health checks.

```csharp
public class Startup
{
    ...
    public void ConfigureServices(IServiceCollection services)
    {
        // Add your own IHealthContributor, registered with the interface
        services.AddSingleton<IHealthContributor, CustomHealthContributor>();

        // Add health actuator
        services.AddHealthActuator(Configuration);

        // Add framework services.
        services.AddMvc();
    }
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
        app.UseStaticFiles();

        // Add management endpoint into pipeline
        app.UseHealthActuator();
    }
}
```

>NOTE: When you use any of the Steeltoe Connectors in your application we automatically add the corresponding health contributors to the service container.

##### 1.2.5.3.2 ASP.NET 4.x App

Refer to the [HTTP Access ASP.NET 4.x](#http-access-asp-net-4-x) section below to see the overall steps required to enable HTTP access to endpoints in an ASP.NET 4.x application.

To add the Health actuator endpoint, use the `UseHealthActuator()` method from [ActuatorConfigurator](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointWeb/ActuatorConfigurator.cs). Optionally you can provide a custom `IIHealthAggregator` and a list of `IHealthContributor`s should you want to customize the actuator endpoint.  If none are provided, defaults will be provided.

The following example shows how enable the Health endpoint and use the default `IIHealthAggregator` together with two `IHealthContributor`s.

```csharp
public class ManagementConfig
{
    public static void ConfigureManagementActuators(IConfiguration configuration, ILoggerFactory loggerFactory = null)
    {
        ...
        ActuatorConfigurator.UseHealthActuator(
            configuration,
            new DefaultHealthAggregator(),
            GetHealthContributors(configuration),
            loggerFactory);
        ...
    }
    private static IEnumerable<IHealthContributor> GetHealthContributors(IConfiguration configuration)
    {
        var healthContributors = new List<IHealthContributor>
        {
            new DiskSpaceContributor(),
            RelationalHealthContributor.GetMySqlContributor(configuration)
        };
        return healthContributors;
    }
```

##### 1.2.5.3.3 ASP.NET OWIN App

Refer to the [HTTP Access ASP.NET OWIN](#http-access-asp-net-owin) section below to see the overall steps required to enable HTTP access to endpoints in an ASP.NET 4.x OWIN application.

To add the Health actuator middleware to the ASP.NET OWIN pipeline, use the `UseHealthActuator()` extension method from [HealthEndpointAppBuilderExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointOwin/Health/HealthEndpointAppBuilderExtensions.cs).

The following example shows how enable the Health endpoint and use the default `IIHealthAggregator` together with two `IHealthContributor`s.

```csharp
public class Startup
{
    ...
    public void Configuration(IAppBuilder app)
    {
        ...
        app.UseHealthActuator(
            new HealthOptions(ApplicationConfig.Configuration),
            new DefaultHealthAggregator(),
            GetHealthContributors(ApplicationConfig.Configuration),
            LoggingConfig.LoggerFactory);
        ...
    }
    private static IEnumerable<IHealthContributor> GetHealthContributors(IConfiguration configuration)
    {
        var healthContributors = new List<IHealthContributor>
        {
            new DiskSpaceContributor(),
            RelationalHealthContributor.GetMySqlContributor(configuration)
        };
        return healthContributors;
    }
}
```

### 1.2.6 Info

The Steeltoe `Info` management endpoint exposes various application information collected from all [IInfoContributor's](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointBase/Info/IInfoContributor.cs) provided to the [InfoEndpoint](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointBase/Info/InfoEndpoint.cs).

Steeltoe includes a couple `IInfoContributor`s out of the box that you can use, but most importantly you can also write your own.

#### 1.2.6.1 Info Contributors

The following table describes the `IInfoContributor` implementations provided by Steeltoe:

|Name|Description|
|---|---|
| [AppSettingsInfoContributor](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointBase/Info/Contributor/AppSettingsInfoContributor.cs)|Exposes any values under the key `info` (for example, `info:foo:bar=foobar`) that is in your apps configuration (for example, `appsettings.json`)|
| [GitInfoContributor](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointBase/Info/Contributor/GitInfoContributor.cs)|Exposes git information (if a git.properties file is available)|

For an example of how to use the above `GitInfoContributor` within MSBuild using [GitInfo](https://github.com/kzu/GitInfo), see the [Steeltoe management sample](https://github.com/SteeltoeOSS/Samples/tree/master/Management/src/AspDotNetCore/CloudFoundry) and the [CloudFoundry.csproj](https://github.com/SteeltoeOSS/Samples/blob/master/Management/src/AspDotNetCore/CloudFoundry/CloudFoundry.csproj) file.

If you wish to provide custom information for your application, create a class that implements the `IInfoContributor` interface and then add that to the `InfoEndpoint`. Details on how to add a contributor to the endpoint is provided below.

The following example `IInfoContributor` adds `someProperty=someValue` to the application's information.

```csharp
public class ArbitraryInfoContributor : IInfoContributor
{
    public void Contribute(IInfoBuilder builder)
    {
        // pass in the info
        builder.WithInfo("arbitraryInfo", new { someProperty = "someValue" });
    }
}
```

>NOTE: Custom `IInfoContributor` implementations must be retrievable from the DI container by interface in order for Steeltoe to find them.

#### 1.2.6.2 Configure Settings

The following table describes the settings that you can apply to the endpoint.

|Key|Description|Default|
|---|---|---|
|id|The ID of the info endpoint|`info`|
|enabled|Whether to enable info management endpoint|true|
|sensitive|Currently not used|false|
|requiredPermissions|User permissions required on Cloud Foundry to access endpoint|RESTRICTED|

**Note**: **Each setting above must be prefixed with `management:endpoints:info`**.

#### 1.2.6.3 Enable HTTP Access

The default path to the Info endpoint is computed by combining the global `path` prefix setting together with the `id` setting from above. The default path is `/info`.

The coding steps you take to enable HTTP access to the Info endpoint together with how to use custom Info contributors differs depending on the type of .NET application your are developing.  The sections which follow describe the steps needed for each of the supported application types.

>NOTE: If you are using dependency injection, all `IInfoContributor` implementations that are retrievable from the DI container by interface will be returned in the Info response.

##### 1.2.6.3.1 ASP.NET Core App

Refer to the [HTTP Access ASP.NET Core](#http-access-asp-net-core) section below to see the overall steps required to enable HTTP access to endpoints in an ASP.NET Core application.

To add the Info actuator to the service container, you can use any of the `AddInfoActuator()` extension methods from [EndpointServiceCollectionExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointCore/Info/EndpointServiceCollectionExtensions.cs).

To add the Info actuator middleware to the ASP.NET Core pipeline, use the `UseInfoActuator()` extension method from [EndpointApplicationBuilderExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointCore/Info/EndpointApplicationBuilderExtensions.cs).

The following example shows how enable the Info endpoint and to add a custom `IInfoContributor` to the service container by adding `ArbitraryInfoContributor` as a singleton. Once that's done the Info endpoint will discover and use it during info requests.

```csharp
public class Startup
{
    ...
    public void ConfigureServices(IServiceCollection services)
    {
        // Add custom info contributor, specifying the interface type
        services.AddSingleton<IInfoContributor, ArbitraryInfoContributor>();

        // Add Info actuator
        services.AddInfoActuator(Configuration);

        // Add framework services.
        services.AddMvc();
    }
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
        app.UseStaticFiles();

        // Add management endpoint into pipeline
        app.UseInfoActuator();
    }
}
```

##### 1.2.6.3.2 ASP.NET 4.x App

Refer to the [HTTP Access ASP.NET 4.x](#http-access-asp-net-4-x) section below to see the overall steps required to enable HTTP access to endpoints in a 4.x application.

To add the Info actuator endpoint, use the `UseInfoActuator()` method from [ActuatorConfigurator](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointWeb/ActuatorConfigurator.cs). Optionally you can provide a list of `IInfoContributor`s should you want to customize the actuator endpoint.  If none are provided, defaults will be provided.

The following example shows how enable the Info endpoint and use the `GitInfoContributor` and `AppSettingsInfoContributor` as `IInfoContributor`s.

```csharp
public class ManagementConfig
{
    public static void ConfigureManagementActuators(IConfiguration configuration, ILoggerFactory loggerFactory)
    {
        ...
        ActuatorConfigurator.UseInfoActuator(configuration, GetInfoContributors(configuration), loggerFactory);
        ...
    }
    private static IEnumerable<IInfoContributor> GetInfoContributors(IConfiguration configuration)
    {
        var contributors = new List<IInfoContributor>() { new GitInfoContributor(), new AppSettingsInfoContributor(configuration) }
        return contributors;
    }
```

##### 1.2.6.3.3 ASP.NET OWIN App

Refer to the [HTTP Access ASP.NET OWIN](#http-access-asp-net-owin) section below to see the overall steps required to enable HTTP access to endpoints in an ASP.NET 4.x OWIN application.

To add the Info actuator middleware to the ASP.NET OWIN pipeline, use the `UseInfoActuator()` extension method from [InfoEndpointAppBuilderExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointOwin/Info/InfoEndpointAppBuilderExtensions.cs).

The following example shows how to enable the Info endpoint and use the `GitInfoContributor` and `AppSettingsInfoContributor` as `IInfoContributor`s.

```csharp
public class Startup
{
    ...
    public void Configuration(IAppBuilder app)
    {
        ...
        app.UseInfoActuator(
            ApplicationConfig.Configuration,
            GetInfoContributors(ApplicationConfig.Configuration),
            LoggingConfig.LoggerFactory);
        ...
    }
    private static IEnumerable<IInfoContributor> GetInfoContributors(IConfiguration configuration)
    {
        var contributors = new List<IInfoContributor>() { new GitInfoContributor(), new AppSettingsInfoContributor(configuration) }
        return contributors;
    }
}
```

### 1.2.7 Loggers

The Steeltoe Loggers management endpoint includes the ability to view and configure the logging levels of your application at runtime when using the [Steeltoe Logging provider](https://github.com/SteeltoeOSS/Logging).

You can view a list of all active loggers in an application and their current configuration. The configuration information is made up of both the explicitly configured logging levels as well as the effective level given to it by the logging framework.

#### 1.2.7.1 Configure Settings

The following table describes the settings that you can apply to the endpoint.

|Key|Description|Default|
|---|---|---|
|id|The ID of the loggers endpoint|`loggers`|
|enabled|Enable or disable loggers management endpoint|true|
|sensitive|Currently not used|false|
|requiredPermissions|User permissions required on Cloud Foundry to access endpoint|RESTRICTED|

**Note**: **Each setting above must be prefixed with `management:endpoints:loggers`**.

#### 1.2.7.2 Enable HTTP Access

The default path to the Loggers endpoint is computed by combining the global `path` prefix setting together with the `id` setting from above. The default path is `/loggers`.

The coding steps you take to enable HTTP access to the Loggers endpoint together with how to use the [Steeltoe Logging provider](https://github.com/SteeltoeOSS/Logging) differs depending on the type of .NET application your are developing.  The sections which follow describe the steps needed for each of the supported application types.

>NOTE: The Steeltoe logging provider is a wrapper around the [Microsoft Console Logging](https://github.com/aspnet/Logging) provider from Microsoft. This wrapper allows querying defined loggers and modifying the levels dynamically at runtime. For more information, see the [Steeltoe Logging documentation](/docs/steeltoe-logging).

##### 1.2.7.2.1 ASP.NET Core App

Refer to the [HTTP Access ASP.NET Core](#http-access-asp-net-core) section below to see the overall steps required to enable HTTP access to endpoints in an ASP.NET Core application.

To add the Loggers actuator to the service container, use the `AddLoggersActuator()` extension method from [EndpointServiceCollectionExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointCore/Loggers/EndpointServiceCollectionExtensions.cs).

To add the Loggers actuator middleware to the ASP.NET Core pipeline, use the `UseLoggersActuator()` extension method from [EndpointApplicationBuilderExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointCore/Loggers/EndpointApplicationBuilderExtensions.cs).

To add the [Steeltoe Logging provider](https://github.com/SteeltoeOSS/Logging) to the `ILoggerFactory`, use the `AddDynamicConsole()` extension method and update the `Program.cs` class as shown below:

```csharp
using Steeltoe.Extensions.Logging;
public class Program
{
    public static void Main(string[] args)
    {
        var host = new WebHostBuilder()
            .UseKestrel()
            .UseContentRoot(Directory.GetCurrentDirectory())
            .UseStartup<Startup>()
            .ConfigureAppConfiguration((builderContext, config) =>
            {
                config.SetBasePath(builderContext.HostingEnvironment.ContentRootPath)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{builderContext.HostingEnvironment.EnvironmentName}.json", optional: true)
                    .AddEnvironmentVariables();
            })
            .ConfigureLogging((builderContext, loggingBuilder) =>
            {
                loggingBuilder.AddConfiguration(builderContext.Configuration.GetSection("Logging"));

                // Add Steeltoe dynamic console logger
                loggingBuilder.AddDynamicConsole();
            })
            .Build();

        host.Run();
    }
}
```

##### 1.2.7.2.2 ASP.NET 4.x App

Refer to the [HTTP Access ASP.NET 4.x](#http-access-asp-net-4-x) section below to see the overall steps required to enable HTTP access to endpoints in a 4.x application.

To add the Loggers actuator endpoint, use the `UseLoggerActuator()` method from [ActuatorConfigurator](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointWeb/ActuatorConfigurator.cs).

The following example shows how enable the Loggers endpoint and configure it with the [Steeltoe Logging provider](https://github.com/SteeltoeOSS/Logging).

```csharp
public class ManagementConfig
{
    public static void ConfigureManagementActuators(IConfiguration configuration)
    {
        ...
        ActuatorConfigurator.UseLoggerActuator(configuration, LoggingConfig.LoggerProvider, LoggingConfig.LoggerProvider);
        ...
    }
```

Below is an example of how you can create a [Steeltoe Logging provider](https://github.com/SteeltoeOSS/Logging) in an 4.x application.

```csharp
public static class LoggingConfig
{
    public static ILoggerFactory LoggerFactory { get; set; }
    public static ILoggerProvider LoggerProvider { get; set; }

    public static void Configure(IConfiguration configuration)
    {
        LoggerProvider = new DynamicLoggerProvider(new ConsoleLoggerSettings().FromConfiguration(configuration));
        LoggerFactory = new LoggerFactory();
        LoggerFactory.AddProvider(LoggerProvider);
    }
}
```

##### 1.2.7.2.3 ASP.NET OWIN App

Refer to the [HTTP Access ASP.NET OWIN](#http-access-asp-net-owin) section below to see the overall steps required to enable HTTP access to endpoints in an ASP.NET 4.x OWIN application.

To add the Loggers actuator middleware to the ASP.NET OWIN pipeline, use the `UseLoggersActuator()` extension method from [LoggersEndpointAppBuilderExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointOwin/Loggers/LoggersEndpointAppBuilderExtensions.cs).

The following example shows how enable the Loggers endpoint and configure it with the [Steeltoe Logging provider](https://github.com/SteeltoeOSS/Logging).

```csharp
public class Startup
{
    ...
    public void Configuration(IAppBuilder app)
    {
        ...
        app.UseLoggersActuator(
            ApplicationConfig.Configuration,
            LoggingConfig.LoggerProvider,
            LoggingConfig.LoggerFactory);
        ...
    }
}
```

Below is an example of how you can create a [Steeltoe Logging provider](https://github.com/SteeltoeOSS/Logging) in an 4.x application.

```csharp
public static class LoggingConfig
{
    public static ILoggerFactory LoggerFactory { get; set; }
    public static ILoggerProvider LoggerProvider { get; set; }

    public static void Configure(IConfiguration configuration)
    {
        LoggerProvider = new DynamicLoggerProvider(new ConsoleLoggerSettings().FromConfiguration(configuration));
        LoggerFactory = new LoggerFactory();
        LoggerFactory.AddProvider(LoggerProvider);
    }
}
```

#### 1.2.7.3 Interacting with the Loggers Actuator

To retrieve the loggers that can be configured and the log levels that are allowed, send an HTTP GET request to `/{LoggersActuatorPath}`.

Log levels can be changed at namespace or class levels with an HTTP POST request to `/{LoggersActuatorPath}/{NamespaceOrClassName}` and a JSON request body that defines the minimum level you wish to log:

```json
{
  "configuredLevel":"INFO"
}
```

> NOTE: The Pivotal Apps Manager integration involves sending the fully-qualified logger name over HTTP. Avoid using colons in the name of a logger to prevent invalid HTTP Requests.

### 1.2.8 Tracing

The Steeltoe Tracing endpoint provides the ability to view the last several requests made of your application.

When you activate the Tracing endpoint, an [ITraceRepository](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointBase/Trace/ITraceRepository.cs) implementation is configured and created to hold [Trace](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointBase/Trace/Trace.cs) information that can be retrieved using the endpoint.

#### 1.2.8.1 Configure Settings

The following table describes the settings that you can apply to the endpoint:

|Key|Description|Default|
|---|---|---|
|id|The ID of the trace endpoint|`trace`|
|enabled|Enable or disable trace management endpoint|true|
|sensitive|Currently not used|false|
|requiredPermissions|User permissions required on Cloud Foundry to access endpoint|RESTRICTED|
|capacity|Size of the circular buffer of traces|100|
|addRequestHeaders|Add request headers|true|
|addResponseHeaders|Add response headers|true|
|addPathInfo|Add path information|false|
|addUserPrincipal|Add user principal|false|
|addParameters|Add request parameters|false|
|addQueryString|Add query string|false|
|addAuthType|Add authentication type|false|
|addRemoteAddress|Add remote address of user|false|
|addSessionId|Add session id|false|
|addTimeTaken|Add time take|true|

**Note**: **Each setting above must be prefixed with `management:endpoints:trace`**.

#### 1.2.8.2 Enable HTTP Access

The default path to the Trace endpoint is computed by combining the global `path` prefix setting together with the `id` setting from above. The default path is `/trace`.

The coding steps you take to enable HTTP access to the Trace endpoint differs depending on the type of .NET application your are developing.  The sections which follow describe the steps needed for each of the supported application types.

##### 1.2.8.2.1 ASP.NET Core App

Refer to the [HTTP Access ASP.NET Core](#http-access-asp-net-core) section below to see the overall steps required to enable HTTP access to endpoints in an ASP.NET Core application.

To add the Trace actuator to the service container, use the `AddTraceActuator()` extension method from [EndpointServiceCollectionExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointCore/Trace/EndpointServiceCollectionExtensions.cs).

To add the Trace actuator middleware to the ASP.NET Core pipeline, use the `UseTraceActuator()` extension method from [EndpointApplicationBuilderExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointCore/Trace/EndpointApplicationBuilderExtensions.cs).

##### 1.2.8.2.2 ASP.NET 4.x App

Refer to the [HTTP Access ASP.NET 4.x](#http-access-asp-net-4-x) section below to see the overall steps required to enable HTTP access to endpoints in a 4.x application.

To add the Trace actuator endpoint, use the `UseTraceActuator()` method from [ActuatorConfigurator](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointWeb/ActuatorConfigurator.cs).

##### 1.2.8.2.3 ASP.NET OWIN App

Refer to the [HTTP Access ASP.NET OWIN](#http-access-asp-net-owin) section below to see the overall steps required to enable HTTP access to endpoints in an ASP.NET 4.x OWIN application.

To add the Trace actuator middleware to the ASP.NET OWIN pipeline, use the `UseTraceActuator()` extension method from [TraceEndpointAppBuilderExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointOwin/Trace/TraceEndpointAppBuilderExtensions.cs).

### 1.2.9 Thread Dump

The Steeltoe Thread dump endpoint can be used to generate a snapshot of information about all the threads in your application. That snapshot includes several bits of information for each thread, including the thread's state, a stack trace, any monitor locks held by the thread, any monitor locks the thread is waiting on, and other details.

>NOTE: At this time, thread dumps are only possible on the Windows operating system. When integrating with the [Pivotal Apps Manager](https://docs.pivotal.io/pivotalcf/2-0/console/index.html), you will not have the ability to obtain thread dumps from apps running on Linux cells.

#### 1.2.9.1 Configure Settings

The following table describes the settings that you can apply to the endpoint:

|Key|Description|Default|
|---|---|---|
|id|The ID of the thread dump endpoint|`dump`|
|enabled|Whether to enable the thread dump management endpoint|true|
|sensitive|Currently not used|false|

**Note**: **Each setting above must be prefixed with `management:endpoints:dump`**.

#### 1.2.9.2 Enable HTTP Access

The default path to the Thread Dump endpoint is computed by combining the global `path` prefix setting together with the `id` setting from above. The default path is `/dump`.

The coding steps you take to enable HTTP access to the Thread Dump endpoint differs depending on the type of .NET application your are developing.  The sections which follow describe the steps needed for each of the supported application types.

##### 1.2.9.2.1 ASP.NET Core App

Refer to the [HTTP Access ASP.NET Core](#http-access-asp-net-core) section below to see the overall steps required to enable HTTP access to endpoints in an ASP.NET Core application.

To add the Thread dump actuator to the service container, use the `AddThreadDumpActuator()` extension method from [EndpointServiceCollectionExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointCore/ThreadDump/EndpointServiceCollectionExtensions.cs).

To add the Thread dump actuator middleware to the ASP.NET Core pipeline, use the `UseThreadDumpActuator()` extension method from [EndpointApplicationBuilderExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointCore/ThreadDump/EndpointApplicationBuilderExtensions.cs).

##### 1.2.9.2.2 ASP.NET 4.x App

Refer to the [HTTP Access ASP.NET 4.x](#http-access-asp-net-4-x) section below to see the overall steps required to enable HTTP access to endpoints in a 4.x application.

To add the Thread Dump actuator endpoint, use the `UseThreadDumpActuator()` method from [ActuatorConfigurator](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointWeb/ActuatorConfigurator.cs).

##### 1.2.9.2.3 ASP.NET OWIN App

Refer to the [HTTP Access ASP.NET OWIN](#http-access-asp-net-owin) section below to see the overall steps required to enable HTTP access to endpoints in an ASP.NET 4.x OWIN application.

To add the Thread Dump actuator middleware to the ASP.NET OWIN pipeline, use the `UseThreadDumpActuator()` extension method from [ThreadDumpEndpointAppBuilderExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointOwin/ThreadDump/ThreadDumpEndpointAppBuilderExtensions.cs).

### 1.2.10 Heap Dump

The Steeltoe Heap dump endpoint can be used to generate and download a mini-dump of your application. The mini-dump can then be read into Visual Studio for analysis.

>NOTE: At this time, dumps are only possible on the Windows operating system. When integrating with the [Pivotal Apps Manager](https://docs.pivotal.io/pivotalcf/2-0/console/index.html), you will not have the ability to obtain dumps from apps running on Linux cells. Also, the heap dump filename used by the Pivotal Apps Manager ends with the `.hprof` extension instead of the usual `.dmp` extension. This may cause problems when opening the dump with Visual Studio or some other diagnostic tool. As a workaround, you can rename the file to use the `.dmp` extension.

#### 1.2.10.1 Configure Settings

The following table describes the settings that you can apply to the endpoint:

|Key|Description|Default|
|---|---|---|
|id|The ID of the heap dump endpoint|`heapdump`|
|enabled|Whether to enable the heap dump management endpoint|true|
|sensitive|Currently not used|false|

**Note**: **Each setting above must be prefixed with `management:endpoints:heapdump`**.

#### 1.2.10.2 Enable HTTP Access

The default path to the Heap Dump endpoint is computed by combining the global `path` prefix setting together with the `id` setting from above. The default path is `/heapdump`.

The coding steps you take to enable HTTP access to the Heap Dump endpoint differs depending on the type of .NET application your are developing.  The sections which follow describe the steps needed for each of the supported application types.

##### 1.2.10.2.1 ASP.NET Core App

Refer to the [HTTP Access ASP.NET Core](#http-access-asp-net-core) section below to see the overall steps required to enable HTTP access to endpoints in an ASP.NET Core application.

To add the Heap dump actuator to the service container, use the `AddHeapDumpActuator()` extension method from [EndpointServiceCollectionExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointCore/HeapDump/EndpointServiceCollectionExtensions.cs).

To add the Heap dump actuator middleware to the ASP.NET Core pipeline, use the `UseHeapDumpActuator()` extension method from [EndpointApplicationBuilderExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointCore/HeapDump/EndpointApplicationBuilderExtensions.cs).

##### 1.2.10.2.2 ASP.NET 4.x App

Refer to the [HTTP Access ASP.NET 4.x](#http-access-asp-net-4-x) section below to see the overall steps required to enable HTTP access to endpoints in a 4.x application.

To add the Heap Dump actuator endpoint, use the `UseHeapDumpActuator()` method from [ActuatorConfigurator](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointWeb/ActuatorConfigurator.cs).

##### 1.2.10.2.3 ASP.NET OWIN App

Refer to the [HTTP Access ASP.NET OWIN](#http-access-asp-net-owin) section below to see the overall steps required to enable HTTP access to endpoints in an ASP.NET 4.x OWIN application.

To add the Heap Dump actuator middleware to the ASP.NET OWIN pipeline, use the `UseHeapDumpActuator()` extension method from [HeapDumpEndpointAppBuilderExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointOwin/HeapDump/HeapDumpEndpointAppBuilderExtensions.cs).

### 1.2.11 Env

The Steeltoe Env endpoint can be used to query and return the configuration values and keys currently in use in your application. The endpoint returns the keys and values from the applications `IConfiguration`.

#### 1.2.11.1 Configure Settings

The following table describes the settings that you can apply to the endpoint:

|Key|Description|Default|
|---|---|---|
|id|The ID of the env endpoint|`env`|
|enabled|Whether to enable the env management endpoint|true|
|keysToSanitize|Keys that should be sanitized. Keys can be simple strings that the property ends with or regex expressions|```["password", "secret", "key", "token", ".*credentials.*", "vcap_services"]```|

**Note**: **Each setting above must be prefixed with `management:endpoints:env`**.

#### 1.2.11.2 Enable HTTP Access

The default path to the Env endpoint is computed by combining the global `path` prefix setting together with the `id` setting from above. The default path is `/env`.

The coding steps you take to enable HTTP access to the Env endpoint differs depending on the type of .NET application your are developing.  The sections which follow describe the steps needed for each of the supported application types.

##### 1.2.11.2.1 ASP.NET Core App

Refer to the [HTTP Access ASP.NET Core](#http-access-asp-net-core) section below to see the overall steps required to enable HTTP access to endpoints in an ASP.NET Core application.

To add the Env actuator to the service container, use the `AddEnvActuator()` extension method from [EndpointServiceCollectionExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointCore/Env/EndpointServiceCollectionExtensions.cs).

To add the Env actuator middleware to the ASP.NET Core pipeline, use the `UseEnvActuator()` extension method from [EndpointApplicationBuilderExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointCore/Env/EndpointApplicationBuilderExtensions.cs).

##### 1.2.11.2.2 ASP.NET 4.x App

Refer to the [HTTP Access ASP.NET 4.x](#http-access-asp-net-4-x) section below to see the overall steps required to enable HTTP access to endpoints in a 4.x application.

To add the Env actuator endpoint, use the `UseEnvActuator()` method from [ActuatorConfigurator](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointWeb/ActuatorConfigurator.cs).

##### 1.2.11.2.3 ASP.NET OWIN App

Refer to the [HTTP Access ASP.NET OWIN](#http-access-asp-net-owin) section below to see the overall steps required to enable HTTP access to endpoints in an ASP.NET 4.x OWIN application.

To add the Env actuator middleware to the ASP.NET OWIN pipeline, use the `UseEnvActuator()` extension method from [EnvEndpointAppBuilderExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointOwin/Env/EnvEndpointAppBuilderExtensions.cs).

### 1.2.12 Refresh

The Steeltoe Refresh endpoint can be used to cause the applications configuration to be reloaded and return the new values and keys currently in use in your application. The endpoint reloads the configuration using the applications `IConfigurationRoot`.

#### 1.2.12.1 Configure Settings

The following table describes the settings that you can apply to the endpoint:

|Key|Description|Default|
|---|---|---|
|id|The ID of the refresh endpoint|`refresh`|
|enabled|Whether to enable the refresh management endpoint|true|

**Note**: **Each setting above must be prefixed with `management:endpoints:refresh`**.

#### 1.2.12.2 Enable HTTP Access

The default path to the Refresh endpoint is computed by combining the global `path` prefix setting together with the `id` setting from above. The default path is `/refresh`.

The coding steps you take to enable HTTP access to the Refresh endpoint differs depending on the type of .NET application your are developing.  The sections which follow describe the steps needed for each of the supported application types.

##### 1.2.12.2.1 ASP.NET Core App

Refer to the [HTTP Access ASP.NET Core](#http-access-asp-net-core) section below to see the overall steps required to enable HTTP access to endpoints in an ASP.NET Core application.

To add the Refresh actuator to the service container, use the `AddRefreshActuator()` extension method from [EndpointServiceCollectionExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointCore/Refresh/EndpointServiceCollectionExtensions.cs).

To add the Refresh actuator middleware to the ASP.NET Core pipeline, use the `UseRefreshActuator()` extension method from [EndpointApplicationBuilderExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointCore/Refresh/EndpointApplicationBuilderExtensions.cs).

##### 1.2.12.2.2 ASP.NET 4.x App

Refer to the [HTTP Access ASP.NET 4.x](#http-access-asp-net-4-x) section below to see the overall steps required to enable HTTP access to endpoints in a 4.x application.

To add the Refresh actuator endpoint, use the `UseRefreshActuator()` method from [ActuatorConfigurator](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointWeb/ActuatorConfigurator.cs).

##### 1.2.12.2.3 ASP.NET OWIN App

Refer to the [HTTP Access ASP.NET OWIN](#http-access-asp-net-owin) section below to see the overall steps required to enable HTTP access to endpoints in an ASP.NET 4.x OWIN application.

To add the Refresh actuator middleware to the ASP.NET OWIN pipeline, use the `UseRefreshActuator()` extension method from [RefreshEndpointAppBuilderExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointOwin/Refresh/RefreshEndpointAppBuilderExtensions.cs).

### 1.2.13 Mappings

The Steeltoe Mappings endpoint can be used to return the MVC and WebAPI Routes and Route templates used by the application.

#### 1.2.13.1 Configure Settings

The following table describes the settings that you can apply to the endpoint:

|Key|Description|Default|
|---|---|---|
|id|The ID of the mappings endpoint|`mappings`|
|enabled|Whether to enable the mappings management endpoint|true|

**Note**: **Each setting above must be prefixed with `management:endpoints:mappings`**.

#### 1.2.13.2 Enable HTTP Access

The default path to the Mappings endpoint is computed by combining the global `path` prefix setting together with the `id` setting from above. The default path is `/mappings`.

The coding steps you take to enable HTTP access to the Mappings endpoint differs depending on the type of .NET application your are developing.  The sections which follow describe the steps needed for each of the supported application types.

##### 1.2.13.2.1 ASP.NET Core App

Refer to the [HTTP Access ASP.NET Core](#http-access-asp-net-core) section below to see the overall steps required to enable HTTP access to endpoints in an ASP.NET Core application.

To add the Mappings actuator to the service container, use the `AddMappingsActuator()` extension method from [EndpointServiceCollectionExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointCore/Mappings/EndpointServiceCollectionExtensions.cs).

To add the Mappings actuator middleware to the ASP.NET Core pipeline, use the `UseMappingsActuator()` extension method from [EndpointApplicationBuilderExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointCore/Mappings/EndpointApplicationBuilderExtensions.cs).

##### 1.2.13.2.2 ASP.NET 4.x App

Refer to the [HTTP Access ASP.NET 4.x](#http-access-asp-net-4-x) section below to see the overall steps required to enable HTTP access to endpoints in a 4.x application.

To add the Mappings actuator endpoint, use the `UseMappingsActuator()` method from [ActuatorConfigurator](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointWeb/ActuatorConfigurator.cs).

By default, the endpoint will return the Routes and Route templates from the apps global `RouteTable`.  If you wish to expose WebAPI routes, in addition to those from the `RouteTable`, provide a reference to the `IApiExplorer` obtained from `GlobalConfiguration.Configuration.Services.GetApiExplorer()`.

##### 1.2.13.2.3 ASP.NET OWIN App

Refer to the [HTTP Access ASP.NET OWIN](#http-access-asp-net-owin) section below to see the overall steps required to enable HTTP access to endpoints in an ASP.NET 4.x OWIN application.

To add the Mappings actuator middleware to the ASP.NET OWIN pipeline, use the `UseMappingsActuator()` extension method from [MappingsEndpointAppBuilderExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointOwin/Mappings/MappingsEndpointAppBuilderExtensions.cs).

You must provide a reference to the `IApiExplorer` obtained from `GlobalConfiguration.Configuration.Services.GetApiExplorer()` when using this endpoint in a OWIN based app.

### 1.2.14 Metrics

The Steeltoe Metrics endpoint configures application metrics collection using the open source [OpenCensus](https://opencensus.io/) project. It automatically configures built-in instrumentation of various aspects of the application and exposes the collected metrics via the endpoint.

The following instrumentation is automatically configured:

* CLR Metrics
  * Heap memory, Garbage collections, Thread utilization
* HTTP Client Metrics
  * Request timings & counts
* HTTP Server Metrics
  * Request timings & counts

All of the above metrics are tagged with values specific to the requests being processed; thereby giving multi-dimensional views of the collected metrics.

>NOTE: The OpenCensus implementation used in Steeltoe (for example, `Steeltoe.Management.OpenCensus`) has been contributed to the OpenCensus community. At some point in the near future the metrics collection functionality will move to using it, instead of the Steeltoe version.

#### 1.2.14.1 Configure Settings

The following table describes the settings that you can apply to the endpoint:

|Key|Description|Default|
|---|---|---|
|id|The ID of the metrics endpoint|`metrics`|
|enabled|Whether to enable the metrics management endpoint|true|
|ingressIgnorePattern|Regex pattern describing what incoming requests to ignore|See [MetricsOptions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointBase/Metrics/MetricsOptions.cs)|
|egressIgnorePattern|Regex pattern describing what outgoing requests to ignore|See [MetricsOptions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointBase/Metrics/MetricsOptions.cs)|

**Note**: **Each setting above must be prefixed with `management:endpoints:metrics`**.

#### 1.2.14.2 Enable HTTP Access

The default path to the Metrics endpoint is computed by combining the global `path` prefix setting together with the `id` setting from above. The default path is `/metrics`.

The coding steps you take to enable HTTP access to the Metrics endpoint differs depending on the type of .NET application your are developing.  The sections which follow describe the steps needed for each of the supported application types.

##### 1.2.14.2.1 ASP.NET Core App

Refer to the [HTTP Access ASP.NET Core](#http-access-asp-net-core) section below to see the overall steps required to enable HTTP access to endpoints in an ASP.NET Core application.

To add the Metrics actuator to the service container, use the `AddMetricsActuator()` extension method from [EndpointServiceCollectionExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointCore/Metrics/EndpointServiceCollectionExtensions.cs).

To add the Mappings actuator middleware to the ASP.NET Core pipeline, use the `UseMetricsActuator()` extension method from [EndpointApplicationBuilderExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointCore/Metrics/EndpointApplicationBuilderExtensions.cs).

##### 1.2.14.2.2 ASP.NET 4.x App

Refer to the [HTTP Access ASP.NET 4.x](#http-access-asp-net-4-x) section below to see the overall steps required to enable HTTP access to endpoints in a 4.x application.

To add the Metrics actuator endpoint, use the `UseMetricsActuator()` method from [ActuatorConfigurator](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointWeb/ActuatorConfigurator.cs).

##### 1.2.14.2.3 ASP.NET OWIN App

Refer to the [HTTP Access ASP.NET OWIN](#http-access-asp-net-owin) section below to see the overall steps required to enable HTTP access to endpoints in an ASP.NET 4.x OWIN application.

To add the Metrics actuator middleware to the ASP.NET OWIN pipeline, use the `UseMetricsActuator()` extension method from [MetricsEndpointAppBuilderExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointOwin/Metrics/MappingsEndpointAppBuilderExtensions.cs).

#### 1.2.14.3 Exporting

By default when you enable metrics collection in your application you do *NOT* automatically enable exporting of those metrics to a backend system.

The coding steps you take to enable metrics exporting differs depending on what backend system you are targeting and the type of .NET application your are developing.  The sections which follow describe the steps needed for each of the backend systems and supported application types.

##### 1.2.14.3.1 Add NuGet References

To use the metrics exporters, you need to add a reference to the appropriate Steeltoe NuGet based on the type of the application you are building and what Dependency Injector you have chosen, if any.

The following table describes the available packages:

|App Type|Package|Description|
|---|---|---|
|All|`Steeltoe.Management.ExporterBase`|Base functionality, no dependency injection|
|ASP.NET Core|`Steeltoe.Management.ExporterCore`|Includes `ExporterBase`, adds ASP.NET Core DI|

To add this type of NuGet to your project, add a `PackageReference` resembling the following:

```xml
<ItemGroup>
...
    <PackageReference Include="Steeltoe.Management.ExporterCore" Version= "2.1.0"/>
...
</ItemGroup>
```

or

```powershell
PM>Install-Package  Steeltoe.Management.ExporterCore -Version 2.1.0
```

##### 1.2.14.3.2 Cloud Foundry Forwarder

The [Metrics Forwarder for Pivotal Cloud Foundry (PCF)](https://docs.pivotal.io/metrics-forwarder/) is a service that allows apps to emit metrics to the [Loggregator](https://docs.pivotal.io/pivotalcf/2-2/loggregator/architecture.html) system and consume those metrics from the [Loggregator Firehose](https://docs.pivotal.io/pivotalcf/2-2/loggregator/architecture.html#firehose).

You can interact with the service through the Cloud Foundry Command Line Interface (cf CLI), [Pivotal Apps Manager](https://docs.pivotal.io/pivotalcf/2-0/console/manage-apps.html), and an [HTTP API](https://docs.pivotal.io/metrics-forwarder/api/). See the [documentation](https://docs.pivotal.io/metrics-forwarder/using.html) for details on how to use the service in your application.

[Metrics Forwarder for Pivotal Cloud Foundry (PCF)](https://docs.pivotal.io/metrics-forwarder/)  enables users to do the following:

* Configure apps to emit custom metrics to [Loggregator](https://docs.pivotal.io/pivotalcf/2-2/loggregator/architecture.html) system.
* Read custom metrics from the [Loggregator Firehose](https://docs.pivotal.io/pivotalcf/2-2/loggregator/architecture.html#firehose) using a Firehose consumer of their choice, including [community](https://github.com/cloudfoundry/loggregator-release/blob/develop/docs/community-nozzles.md) and third-party nozzles.

There are many third-party products you can choose from, including [PCF Metrics](https://docs.pivotal.io/pcf-metrics/1-4/).

##### 1.2.14.3.2.1 Configure Settings

The following table describes the settings that you can apply to the exporter:

|Key|Description|Default|
|---|---|---|
|endpoint|the uri used to POST metrics|null|
|accessToken|the authentication token needed to access the endpoint|null|
|rateMilli|delay in milliseconds between metrics POSTs|60000|
|validateCertificates|validate SSL certificates received from exporter service|true|
|timeoutSeconds|timeout used in seconds for each POST request|3|
|applicationId|cloud foundry application ID the POST applies to|null|
|instanceId|cloud foundry application instance ID the POST applies to|null|
|instanceIndex|cloud foundry application instance index the POST applies to|null|
|micrometerMetricWriter|emit metrics using Spring Boot 2.x format |false|

**Note**: **The `endpoint`, `accessToken`,`applicationId`, `instanceId` and `instanceIndex` settings above will be automatically picked up from the Metrics Forwarder service binding found for your application.**

##### 1.2.14.3.2.2 ASP.NET Core App

There are three steps needed to use the Metrics Forwarder for Pivotal Cloud Foundry (PCF) service:

1. Create and bind a forwarder service to your application. Follow the steps in the Metrics Forwarder for PCF [documentation](https://docs.pivotal.io/metrics-forwarder/using.html).
1. Add the exporter to the service container. Use the `AddMetricsForwarderExporter()` extension method from [EndpointServiceCollectionExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.ExporterCore/Metrics/CloudFoundryForwarder/EndpointServiceCollectionExtensions.cs).
1. Start the exporter background thread. Use the `UseMetricsExporter()` extension method from [EndpointApplicationBuilderExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.ExporterCore/Metrics/CloudFoundryForwarder/EndpointApplicationBuilderExtensions.cs).

```csharp
public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }
    public void ConfigureServices(IServiceCollection services)
    {
        // Add Metrics collection
        services.AddMetricsActuator(Configuration);

        // Export metrics to Cloud Foundry forwarder
        services.AddMetricsForwarderExporter(Configuration);
        ...
    }
    public void Configure(IApplicationBuilder app)
    {
        app.UseStaticFiles();

        // Expose Metrics endpoint
        app.UseMetricsActuator();

        app.UseMvc();

        // Start up metrics exporter
        app.UseMetricsExporter();
    }
}
```

##### 1.2.14.3.2.3 ASP.NET 4.x App

There are two steps needed to use the Metrics Forwarder for Pivotal Cloud Foundry (PCF) service:

1. Create and bind a forwarder service to your application. Follow the steps in the Metrics Forwarder for PCF [documentation](https://docs.pivotal.io/metrics-forwarder/using.html).
1. Configure and start the exporter background thread.

```csharp
public class ManagementConfig
{
    public static IMetricsExporter MetricsExporter { get; set; }

    public static void UseCloudFoundryMetricsExporter(IConfiguration configuration, ILoggerFactory loggerFactory = null)
    {
        var options = new CloudFoundryForwarderOptions(configuration);
        MetricsExporter = new CloudFoundryForwarderExporter(
            options,
            OpenCensusStats.Instance,
            loggerFactory != null ? loggerFactory.CreateLogger<CloudFoundryForwarderExporter>() : null);
    }
    public static void Start()
    {
        DiagnosticsManager.Instance.Start();
        if (MetricsExporter != null)
        {
            MetricsExporter.Start();
        }
    }
    public static void Stop()
    {
        DiagnosticsManager.Instance.Stop();
        if (MetricsExporter != null)
        {
            MetricsExporter.Stop();
        }
    }
}
```

# 2.0 Distributed Tracing

Steeltoe distributed tracing implements a solution for .NET applications based on the open source [OpenCensus](https://opencensus.io/) project. For most users implementing and using distributed tracing should be invisible, and many of the interactions with external systems should be instrumented automatically. You can capture trace data in logs, or by sending it to a remote collector service.

>NOTE: The OpenCensus implementation used in Steeltoe (for example, `Steeltoe.Management.OpenCensus`) has been contributed to the OpenCensus community. At some point in the near future the distributed tracing functionality will move to using it, instead of the Steeltoe version.

A Span is the basic unit of work. For example, sending an RPC is a new span, as is sending a response to an RPC. Span’s are identified by a unique 64-bit ID for the span and another 64-bit ID for the trace the span is a part of. Spans also have other data, such as descriptions, key-value annotations, the ID of the span that caused them, and process ID’s (normally IP address). Spans are started and stopped, and they keep track of their timing information. Once you create a span, you must stop it at some point in the future. A set of spans forming a tree-like structure called a Trace. For example, if you are running a distributed big-data store, a trace might be formed by a put request.

Features:

* Adds trace and span ids to the application log messages, so you can extract all the logs from a given trace or span in a log aggregator.
* Using the  [OpenCensus](https://opencensus.io/) APIs we provide an abstraction over common distributed tracing data models: traces, spans (forming a DAG), annotations, key-value annotations.
* Automatically instruments common ingress and egress points from .NET applications (e.g MVC Controllers, Views, Http clients).
* Optionally generate, collect and export Zipkin-compatible traces via HTTP.

>NOTE: Currently, distributed tracing is only supported in ASP.NET Core applications.

## 2.1 Quick Start

This quick start uses multiple ASP.NET Core applications to show how to use the Steeltoe distributed tracing features to capture trace information and send it to a Zipkin server running locally on your development machine. It also shows how to take that same set of applications and push them to Cloud Foundry and use a Zipkin server operating there.

The application consists of two components: a Fortune-Teller-Service that registers a FortuneService, and a Fortune-Teller-UI that discovers the service and fetches fortunes from it. Both use Steeltoe distributed tracing and the auto-instrumentation features to capture trace information and send it to a Zipkin server.

### 2.1.1 Running Locally

To run the fortune teller service and the fortune teller UI on your local machine and observe the results, work through the following sections:

* [Start Eureka Server](#2-1-1-1-start-eureka-server)
* [Start Zipkin Server](#2-1-1-2-start-zipkin-server)
* [Locate Sample](#2-1-1-3-locate-sample)
* [Run Fortune Teller](#2-1-1-4-run-fortune-teller)
* [Observe Logs](#2-1-1-5-observe-logs)
* [View Traces](#2-1-1-6-view-traces)

#### 2.1.1.1 Start Eureka Server

In this step, we fetch a GitHub repository from which we can start up a Netflix Eureka Server locally on the desktop. This server has been pre-configured to listen for service registrations and discovery requests at <http://localhost:8761/eureka>. The following script shows how to get the sample from GitHub and start the service:

```bash
git clone https://github.com/spring-cloud-samples/eureka.git
cd eureka
mvnw spring-boot:run
```

Alternately, if you have a running docker environment installed on your system, then you should be able to:

```bash
docker run -d -p:8761:8761 steeltoeoss/eurekaserver
```

#### 2.1.1.2 Start Zipkin Server

Follow the instructions in the [Zipkin Quickstart](https://zipkin.io/pages/quickstart) guide. You will need Java installed to do this.

Alternately, if you have a running docker environment installed on your system, then you should be able to:

```bash
docker run -d -p 9411:9411 openzipkin/zipkin
```

#### 2.1.1.3 Locate Sample

Now that you have the service running, you need to change directory to where the sample is:

```bash
cd Samples/Management/src/AspDotNetCore/Tracing
```

#### 2.1.1.4 Run Fortune Teller

We recommend running this application with the dotnet CLI. Scripts are provided to start both the service and the UI with a single command, as follows:

```bash
# Use the helper scripts, passing in net461, netcoreapp2.0 or netcoreapp2.1
.\RunFortuneTeller net461
```

You can also run the commands directly yourself, as follows:

```bash
# Run the service in one window:
cd Samples/Management/src/AspDotNetCore/Tracing/Fortune-Teller-Service
dotnet run -f netcoreapp2.1 --force

# And the UI in another:
cd Samples/Management/src/AspDotNetCore/Tracing/Fortune-Teller-UI
dotnet run -f netcoreapp2.1 --force
```

#### 2.1.1.5 Observe Logs

The `dotnet run` command in each sample should produce output similar to the following:

```bash
info: Microsoft.Data.Entity.Storage.Internal.InMemoryStore[1]
      Saved 50 entities to in-memory store.
Hosting environment: Production
Now listening on: http://*:5000
Application started. Press Ctrl+C to shut down.
```

Once you see `Application started...` for both applications, the Fortune Teller sample is ready for use.

#### 2.1.1.6 View Traces

Start a browser and visit Fortune-Teller-UI: <http://localhost:5555>. You should see your fortune displayed. Refresh the browser to see a new fortune.

To view traces generated by the sample:

1. Open a browser to the Zipkin server UI: <http://localhost:9411/zipkin/>.
1. Go back to the Fortune-Teller-UI and hit refresh a few times to get new fortunes.
1. Go back to the Zipkin server UI.
1. In the `Sort` field select `Newest First`.
1. Then to fetch some traces hit `Find Traces`.

At that point you should see several traces that you can select and view.

### 2.1.2 Running on Cloud Foundry

To run the fortune teller service and the fortune teller UI on Cloud Foundry and observe the results, work through the following sections:

* [Start Eureka Server](#2-1-2-1-start-eureka-server)
* [Start Zipkin Server](#2-1-2-2-start-zipkin-server)
* [Configure Settings](#2-1-2-3-configure-settings)
* [Publish Both Applications](#2-1-2-4-publish-both-applications)
* [Push Both Applications](#2-1-2-5-push-both-applications)
* [Observe Logs](#2-1-2-6-observe-logs)
* [View Traces](#2-1-2-7-view-traces)

#### 2.1.2.1 Start Eureka Server

Use the Cloud Foundry CLI to create a service instance of the Spring Cloud Eureka Server on Cloud Foundry, as follows:

```bash
# Create a Eureka Server instance on Cloud Foundry
cf create-service p-service-registry standard myDiscoveryService

# Wait for the service to be ready
cf services
```

#### 2.1.2.2 Start Zipkin Server

Download the latest Zipkin server jar from [here](https://dl.bintray.com/openzipkin/maven/io/zipkin/java/zipkin-server/) (for example, zipkin-server-2.8.4-exec.jar).

Use the Cloud Foundry CLI to start an instance of the Zipkin server on Cloud Foundry as follows:

```bash
# Start a Zipkin server instance on Cloud Foundry
cf push zipkin-server -p ./zipkin-server-2.8.4-exec.jar
```

Verify the server is up and running by opening a browser to the Zipkin server UI: (for example, <https://zipkin-server.cfapps.io/>).

#### 2.1.2.3 Configure Settings

Open `appsettings.json` and modify the `management:tracing:exporter:zipkin:endpoint` configuration setting to match the endpoint of the Zipkin server deployed to Cloud Foundry above. (for example, <https://zipkin-server.cfapps.io/api/v2/spans>).

You need to make this modification in both Fortune-Teller-Service and Fortune-Teller-UI.

#### 2.1.2.4 Publish Both Applications

.NET Applications should be published before pushing to Cloud Foundry. You need to publish both Fortune-Teller-Service and Fortune-Teller-UI.

See [Publish Sample](#publish-sample) for instructions on how to publish this sample for either Linux or Windows.

#### 2.1.2.5 Push Both Applications

For the Fortune Teller to work on Cloud Foundry, you need to push both Fortune-Teller-Service and Fortune-Teller-UI.

See [Push Sample](#push-sample) for instructions on how to push this sample to either Linux or Windows on Cloud Foundry.

#### 2.1.2.6 Observe Logs

To see the logs as you startup the application, use `cf logs fortuneService` or `cf logs fortuneui`.

On a Linux cell, you should see output resembling the following during startup.

```bash
2016-06-01T09:14:14.38-0600 [CELL/0]     OUT Creating container
2016-06-01T09:14:15.93-0600 [CELL/0]     OUT Successfully created container
2016-06-01T09:14:17.14-0600 [CELL/0]     OUT Starting health monitoring of container
2016-06-01T09:14:21.04-0600 [APP/0]      OUT Hosting environment: Development
2016-06-01T09:14:21.04-0600 [APP/0]      OUT Content root path: /home/vcap/app
2016-06-01T09:14:21.04-0600 [APP/0]      OUT Now listening on: http://*:8080
2016-06-01T09:14:21.04-0600 [APP/0]      OUT Application started. Press Ctrl+C to shut down.
2016-06-01T09:14:21.41-0600 [CELL/0]     OUT Container became healthy
```

On Windows cells, you should see something slightly different but with the same information.

#### 2.1.2.7 View Traces

Start a browser and visit <https://fortuneui.x.y.z/> where `x.y.z` corresponds to the Cloud Foundry application domain that you are operating under.

You should see your fortune. Refresh the browser to see a new fortune.

Follow the same steps [above](#2-1-1-6-view-traces) to see traces generated by the sample.

### 2.1.3 Understanding the Sample

Fortune-Teller-Service was created with the .NET Core tooling `webapi` template (`dotnet new webapi`), and then modifications were made to add the Steeltoe frameworks.

To understand the Steeltoe related changes to the generated template code, examine the following files.

* `Fortune-Teller-Service.csproj`: Contains `PackageReference`s for OpenCensus and Steeltoe NuGet packages.
* `Program.cs`: Code was added to the `WebHostBuilder` to pick up Cloud Foundry configuration values when pushed to Cloud Foundry, to use Cloud Foundry hosting and to use the Steeltoe Dynamic Logger.
* `appsettings.json`: Contains the configuration data needed to cause the Steeltoe Discovery client to register the FortuneService with the Eureka server and to NOT fetch service information from the Eureka server. It also contains changes for configuring the tracing features.
* `Startup.cs`: Code was added to the `ConfigureServices()` method to add the Discovery Client as a singleton, to add the Distributed Tracing service and the Zipkin exporter to the service container. Additionally, code was added to the `Configure()` method to cause the Discovery Client to start communicating with the Eureka Server and to startup the Tracing exporter.

Various other files that pertain to the application were added to the project, but they do not directly interact with the Steeltoe Discovery client.

Fortune-Teller-UI was created with the .NET Core tooling `mvc` template (`dotnet new mvc`), and then modifications were made to add the Steeltoe frameworks.

To understand the Steeltoe related changes to the generated template code, examine the following files:

* `Fortune-Teller-UI.csproj`: Contains `PackageReference`s for OpenCensus and Steeltoe NuGet packages.
* `Program.cs`: Code was added to the `WebHostBuilder` to pick up Cloud Foundry configuration values when pushed to Cloud Foundry, to use Cloud Foundry hosting and to use the Steeltoe Dynamic Logger.
* `appsettings.json`: Contains the configuration data needed to cause the Steeltoe Discovery client to NOT register as a service, yet it still fetches service information from the Eureka server. It also contains changes for configuring the tracing features.
* `Startup.cs`: Code was added to the `ConfigureServices()` method to add the Discovery Client as a singleton, to add the Distributed Tracing service and the Zipkin exporter to the service container. Additionally, code was added to the `Configure()` method to cause the Discovery Client to start communicating with the Eureka Server and to startup the Tracing exporter.
* `FortuneService.cs`: Contains code used to fetch the fortune from the FortuneService. Uses an injected `IDiscoveryClient`, together with the `DiscoveryHttpClientHandler`, to do the service lookup and to issue the HTTP GET request to the Fortune-Teller-Service.

## 2.2 Usage

You should understand how the .NET [Configuration service](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration) works before starting to use the management endpoints. You need at least a basic understanding of the `ConfigurationBuilder` and how to add providers to the builder to configure the endpoints.

When developing ASP.NET Core applications, you should also understand how the ASP.NET Core [Startup](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/startup) class is used in configuring the application services for the app. Pay particular attention to the usage of the `ConfigureServices()` and `Configure()` methods.

Steeltoe distributed tracing automatically applies instrumentation at key ingress and egress points in your ASP.NET Core application so that you are able to get meaningful traces without having to do any instrumentation yourself. These points include:

* HTTP Server
  * Request Start & Finish
  * Unhandled and Handled exceptions
  * MVC Action Start & Finish
  * MVC View Start & Finish
* HTTP Client (Desktop and Core)
  * Outgoing Request Start & Finish
  * Unhandled and Handled exceptions

### 2.2.1 Add NuGet References

To use the distributed tracing exporters, you need to add a reference to the appropriate Steeltoe NuGet based on the type of the application you are building and what Dependency Injector you have chosen, if any.

The following table describes the available packages:

|App Type|Package|Description|
|---|---|---|
|All|`Steeltoe.Management.TracingBase`|Base functionality, no dependency injection|
|ASP.NET Core|`Steeltoe.Management.TracingCore`|Includes `TracingBase`, adds ASP.NET Core DI|

To add this type of NuGet to your project, add a `PackageReference` resembling the following:

```xml
<ItemGroup>
...
    <PackageReference Include="Steeltoe.Management.TracingCore" Version= "2.1.0"/>
...
</ItemGroup>
```

or

```powershell
PM>Install-Package  Steeltoe.Management.TracingCore -Version 2.1.0
```

### 2.2.2 Configure Settings

Distributed tracing can be configured by using the normal .NET [Configuration service](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration).

All settings should be placed under the prefix with the key `management:tracing:`.

|Key|Description|Default|
|---|---|---|
|name|the name of the application|spring:application:name, Cloud Foundry name, or "Unknown"|
|ingressIgnorePattern|Regex pattern describing what incoming requests to ignore|See [TracingOptions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.TracingBase/TracingOptions.cs)|
|egressIgnorePattern|Regex pattern describing what outgoing requests to ignore|See [TracingOptions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.TracingBase/TracingOptions.cs)|
|maxNumberOfAttributes|max attributes attachable to OpenCensus span|32|
|maxNumberOfAnnotations|max annotations attachable to OpenCensus span|32|
|maxNumberOfMessageEvents|max events attachable to OpenCensus span|128|
|maxNumberOfLinks|max links attachable to OpenCensus span|128|
|alwaysSample|enable the OpenCensus AlwaysSampler|OpenCensus ProbabilitySampler|
|neverSample|enable the OpenCensus NeverSampler|OpenCensus ProbabilitySampler|
|useShortTraceIds|truncate the ids to 8 bytes instead of 16, use for backwards compatibility with Spring Sleuth, PCF Metrics, etc.|true|

### 2.2.3 Enabling Log Correlation

If you want to use distributed tracing together with log correlation, then you must utilize the [Steeltoe Logging provider](https://github.com/SteeltoeOSS/Logging) in your application.

Follow these [instructions](https://steeltoe.io/docs/steeltoe-logging/#1-0-dynamic-logging-provider) for how to enable the provider in your application.

Once that is done, then whenever your application issues any log statements, the Steeltoe logger will add additional trace information to each log message if there is an active trace context. The format of that information is of the form:

* `[app name, trace id, span id, trace flags]`  (for example, `[service1,2485ec27856c56f4,2485ec27856c56f4,true]`)

### 2.2.4 Propagating Trace Context

When working with distributed tracing systems you will find that a trace context (for example, trace state information) must get propagated to all child processes to ensure that child spans originating from a root trace get collected and correlated into a single trace in the end.  The current trace and span IDs are just one piece of the required information that must get propagated.

Steeltoe distributed tracing handles this for you by default when using the .NET HttpClient. When a downstream HTTP call is made, the current trace context is encoded as request headers and sent along with the request automatically.  Currently, Steeltoe encodes the context using [Zipkin B3 Propagation](https://github.com/openzipkin/b3-propagation) encodings. As a result, you will find that Steeltoe tracing is interoperable with several other instrumentation libraries such as [Spring Cloud Sleuth](https://cloud.spring.io/spring-cloud-sleuth/2.0.x/single/spring-cloud-sleuth.html).

### 2.2.5 Add Distributed Tracing

To enable distributed tracing all you need to to do is add the service to the container. To do this use the `AddDistributedTracing()` extension method from [TracingServiceCollectionExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.TracingCore/TracingServiceCollectionExtensions.cs).

```csharp
public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; set; }

    public void ConfigureServices(IServiceCollection services)
    {
        ...
        // Add Distributed tracing
        services.AddDistributedTracing(Configuration);

        // Add framework services.
        services.AddMvc();
    }
    public void Configure(IApplicationBuilder app)
    {
        app.UseStaticFiles();

        app.UseMvc();
    }
}
```

## 2.3 Exporting

By default when you enable distributed tracing in your application you do *NOT* automatically enable exporting of those traces to a backend system. Currently, Steeltoe supports exporting traces to a backend Zipkin server.

To enable exporting you will need to do the following:

* Add appropriate NuGet package reference to your project.
* Configure the settings the exporter will use during export.
* Add and Use the exporter service in the application

### 2.3.1 Add NuGet References

All of the exporters can be found in the `Steeltoe.Management.ExporterBase` and in `Steeltoe.Management.OpenCensus`.

To use an exporter in a ASP.NET Core application, then add the following `PackageReference` to your `.csproj` file.

```xml
<ItemGroup>
...
    <PackageReference Include="Steeltoe.Management.ExporterCore" Version= "2.1.0"/>
...
</ItemGroup>
```

or

```powershell
PM>Install-Package  Steeltoe.Management.ExporterCore -Version 2.1.0
```

### 2.3.2 Zipkin Server

Zipkin is a popular distributed tracing system which has been around for several years. It is composed of client libraries for instrumenting application code and a backend server for collecting and viewing the collected data. For more information on Zipkin we encourage you to review the [documentation](https://zipkin.io/).  Check out the [Quickstart](https://zipkin.io/pages/quickstart) guide for details on how to set up a server.

Steeltoe provides an exporter that will send all captured traces to a Zipkin server.  The following sections outline how to enable the exporter in your application.

#### 2.3.2.1 Configure Settings

The following table describes the settings that you can apply to the exporter:

|Key|Description|Default|
|---|---|---|
|endpoint|the uri used to POST traces|`http://localhost:9411/api/v2/spans`|
|validateCertificates|validate SSL certificates received from exporter service|true|
|timeoutSeconds|timeout used in seconds for each POST request|3|
|serviceName|app name used in log messages|null|
|useShortTraceIds|truncate the ids to 8 bytes instead of 16, use for backwards compatibility with Spring Sleuth, PCF Metrics, etc.|true|

#### 2.3.2.2 Add and Use Zipkin Exporter

There are two steps needed to use the Zipkin exporter:

1. Add the exporter to the service container. Use the `AddZipkinExporter()` extension method from [ZipkinExporterServiceCollectionExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.ExporterCore/Tracing/Zipkin/ZipkinExporterServiceCollectionExtensions.cs).
1. Start the exporter background thread. Use the `UseTracingExporter()` extension method from [ZipkinExporterApplicationBuilderExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.ExporterCore/Tracing/Zipkin/ZipkinExporterApplicationBuilderExtensions.cs).

```csharp
public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }
    public void ConfigureServices(IServiceCollection services)
    {
        // Add Distributed tracing
        services.AddDistributedTracing(Configuration);

        // Export traces to Zipkin
        services.AddZipkinExporter(Configuration);
        ...
    }
    public void Configure(IApplicationBuilder app)
    {
        app.UseStaticFiles();
        app.UseMvc();

        // Start up trace exporter
        app.UseTracingExporter();
    }
}
```

# Common Steps

## Publish Sample

### ASP.NET Core

Use the `dotnet` CLI to [build and locally publish](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-publish) the application for the framework and runtime you will deploy the application to:

* Linux with .NET Core: `dotnet publish -f netcoreapp2.1 -r ubuntu.14.04-x64`
* Windows with .NET Core: `dotnet publish -f netcoreapp2.1 -r win10-x64`
* Windows with .NET Platform: `dotnet publish -f net461 -r win10-x64`

>NOTE: Starting with .NET Core 2.0, the `dotnet publish` command will automatically restore dependencies for you. Running `dotnet restore` explicitly is not generally required.

### ASP.NET 4.x

1. Open the solution for the sample in Visual Studio
1. Right click on the project, select "Publish"
1. Use the included `FolderProfile` to publish to `bin/Debug/net461/win10-x64/publish`

## Push Sample

Use the Cloud Foundry CLI to push the published application to Cloud Foundry by using the parameters that match what you selected for framework and runtime, as follows:

```bash
# Push to Linux cell
cf push -f manifest.yml -p bin/Debug/netcoreapp2.1/ubuntu.14.04-x64/publish

 # Push to Windows cell, .NET Core
cf push -f manifest-windows.yml -p bin/Debug/netcoreapp2.1/win10-x64/publish

 # Push to Windows cell, .NET Framework
cf push -f manifest-windows.yml -p bin/Debug/net461/win10-x64/publish
```

## HTTP Access ASP.NET Core

To expose any of the management endpoints over HTTP in an ASP.NET Core application:

1. Add a reference to `Steeltoe.Management.EndpointCore` or `Steeltoe.Management.CloudFoundryCore`.
1. Configure endpoint settings, as needed (for example, `appsettings.json`).
1. Add any additional "contributors" to the service container. (for example, `AddSingleton<IHealthContributor, CustomHealthContributor>()`)
1. `Add` the actuator endpoint to the service container (for example, `AddHealthActuator()`).
1. `Use` the actuator middleware to provide HTTP access (for example, `UseInfoActuator()`).

>NOTE: Each endpoint uses the same host and port as the application. The default path to each endpoint is specified in its section on this page, along with specific `Add` and `Use` method names.

If you wish to use all of the Steeltoe endpoints which integrate with the Pivotal Apps Manager, use `AddCloudFoundryActuators()` and `UseCloudFoundryActuators()` to add them all at once instead of including each individually, as shown in the following example:

```csharp
public class Startup
{
    public IConfiguration Configuration { get; }
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }
    public void ConfigureServices(IServiceCollection services)
    {
        ...
        // Add all management endpoint services
        services.AddCloudFoundryActuators(Configuration);
        ...
    }
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
        ...
        // Add all management endpoints into pipeline
        app.UseCloudFoundryActuators();

        // Add ASP.NET Core MVC middleware to pipeline
        app.UseMvc();
        ...
    }
}
```

>NOTE: The order in which you add middleware to the [ASP.NET Core pipeline](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-2.1&tabs=aspnetcore2x) is important. We recommend that you add the Steeltoe management endpoints before others to ensure proper operation.

## HTTP Access ASP.NET 4.x

To expose any of the management endpoints over HTTP in an ASP.NET 4.x application:

1. Add a reference to `Steeltoe.Management.EndpointWeb`.
1. Configure endpoint settings, as needed (for example, `appsettings.json`).
1. `Use` the middleware to provide HTTP access (for example,, `UseInfoActuator()`).
1. If using Metrics, start/stop Diagnostics and MetricsExporting (for example, `DiagnosticsManager.Instance.Start()`)
1. Update web.config to allow extensionless requests to reach the actuators

```xml
<system.webServer>
    <handlers>
        <!--  This example is overly broad, it does not need to be used exactly as-is  -->
        <!-- Allow at least GET, POST and OPTIONS requests to go past IIS to actuators -->
        <add name="ExtensionlessUrlHandler-Integrated-4.0"
                path="*."
                verb="*"
                type="System.Web.Handlers.TransferRequestHandler"
                preCondition="integratedMode,runtimeVersionv4.0" />
    </handlers>
</system.webServer>
```

>NOTE: Each endpoint uses the same host and port as the application. The default path to each endpoint is specified in its section on this page, along with specific `Use` method name.

If you wish to use all of the Steeltoe endpoints which integrate with the Pivotal Apps Manager, call `UseCloudFoundryActuators()` to configure them all at once instead of including each individually, as shown in the following example:

```csharp
public class ManagementConfig
{
    public static IMetricsExporter MetricsExporter { get; set; }

    public static void ConfigureActuators(
        IConfiguration config,
        ILoggerProvider logger,
         IEnumerable<IHealthContributor> contrib,
         IApiExplorer api,
         ILoggerFactory factory = null)
    {
        ActuatorConfigurator.UseCloudFoundryActuators(config, logger, contrib, api, factory);
    }

    public static void Start()
    {
        DiagnosticsManager.Instance.Start();
        if (MetricsExporter != null)
        {
            MetricsExporter.Start();
        }
    }

    public static void Stop()
    {
        DiagnosticsManager.Instance.Stop();
        if (MetricsExporter != null)
        {
            MetricsExporter.Stop();
        }
    }
}
```

The above static methods should be called in `Global.asax.cs`.  In the `Application_Start()` method call `ConfigureActuators()`and `Start()` and in `Application_Stop()` call `Stop()`.  See the [Steeltoe Samples repository](https://github.com/SteeltoeOSS/Samples/tree/dev/Management/src/AspDotNet4) for more details.

## HTTP Access ASP.NET OWIN

To expose any of the management endpoints over HTTP in an ASP.NET 4.x application:

1. Add a reference to `Steeltoe.Management.EndpointOwin`.
1. Configure endpoint settings, as needed (for example, `appsettings.json`).
1. `Use` the middleware to provide HTTP access (for example, `UseInfoActuator()`).
1. If using Metrics, start/stop Diagnostics and MetricsExporting (for example, `DiagnosticsManager.Instance.Start()`)
1. If not self-hosting, add/update web.config entries to ensure OWIN startup and allow requests to reach the actuators

```xml
    <appSettings>
        <add key="owin:AutomaticAppStartup" value="true" />
    </appSettings>
    <system.webServer>
        <handlers>
            <!-- Allow GET, POST and OPTIONS requests to go past IIS to actuators -->
            <!-- Adjust the path value if you are not using Pivotal Apps Manager -->
            <add name="ApiURIs-ISAPI-Integrated-4.0"
                    path="cloudfoundryapplication/*"
                    verb="GET,POST,OPTIONS"
                    type="System.Web.Handlers.TransferRequestHandler"
                    preCondition="integratedMode,runtimeVersionv4.0" />
        </handlers>
    </system.webServer>
```

>NOTE: Each endpoint uses the same host and port as the application. The default path to each endpoint is specified in its section on this page, along with specific `Use` method name.

If you wish to use all of the Steeltoe endpoints which integrate with the Pivotal Apps Manager, use `UseCloudFoundryActuators()` to configure them all at once instead of including each individually, as shown in the following example:

```csharp
public class Startup
{
    private IMetricsExporter MetricsExporter { get; set; }

    public void Configuration(IAppBuilder app)
    {
        var config = GlobalConfiguration.Configuration;

        app.UseCloudFoundryActuators(
            ApplicationConfig.Configuration,
            GetHealthContributors(ApplicationConfig.Configuration),
            config.Services.GetApiExplorer(),
            LoggingConfig.LoggerProvider,
            LoggingConfig.LoggerFactory);

        Start();
    }

    private void Start()
    {
        DiagnosticsManager.Instance.Start();
        if (MetricsExporter != null)
        {
            MetricsExporter.Start();
        }
    }

    public void Stop()
    {
        DiagnosticsManager.Instance.Stop();
        if (MetricsExporter != null)
        {
            MetricsExporter.Stop();
        }
    }
}
```

See the [Steeltoe Samples repository](https://github.com/SteeltoeOSS/Samples/tree/dev/Management/src/AspDotNet4) for more details.
