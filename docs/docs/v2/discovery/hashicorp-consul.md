# HashiCorp Consul

The Consul client implementation lets applications register services with a Consul server and discover services registered by other applications. This Steeltoe client utilizes the Consul .NET package provided by the open source project [consuldotnet](https://github.com/PlayFab/consuldotnet).

The Consul client implementation supports the following .NET application types:

* ASP.NET (MVC, WebForm, WebAPI, WCF)
* ASP.NET Core
* Console apps (.NET Framework and .NET Core)

## Usage

You should know how the new .NET [Configuration service](https://docs.microsoft.com/aspnet/core/fundamentals/configuration) works before starting to use the client. A basic understanding of the `ConfigurationBuilder` and how to add providers to the builder is necessary in order to configure the client.

You should also know how the ASP.NET Core [Startup](https://docs.microsoft.com/aspnet/core/fundamentals/startup) class is used in configuring the application services and the middleware used in the app. Pay particular attention to the usage of the `Configure()` and `ConfigureServices()` methods.

It might be helpful to have an understanding of the [Spring Cloud Consul](https://spring.io/projects/spring-cloud-consul) project as we have based our work on this project.

In order to use the Steeltoe Discovery client, you need to do the following:

* Add appropriate NuGet package reference to your project.
* Configure the settings the Discovery client will use to register services in the service registry.
* Configure the settings the Discovery client will use to discover services in the service registry.
* Add and Use the Discovery client service in the application.
* Use an injected `IDiscoveryClient` to lookup services.

>NOTE: Most of the example code in the following sections is based on using Discovery in a ASP.NET Core application. If you are developing a ASP.NET 4.x application or a console-based app, see the [other samples](https://github.com/SteeltoeOSS/Samples/tree/2.x/Discovery) for example code you can use.

### Consul Settings

To get the Steeltoe Discovery client to properly communicate with the Consul server, you need to provide a few configuration settings to the client. There are two sections you may need to configure.

The first pertains to configuring the information needed to connect to the Consul server. All of these settings should start with `consul:`

|Key|Description|Default|
|---|---|---|
|host|Address of the Consul server|localhost|
|port|Port number the Consul server is listening on|8500|
|scheme|Scheme to use with the Consul server (http or https)|http|
|datacenter|The datacenter name passed in each request to the server|none|
|token|The auth token passed in each request to the server|true|
|waitTime|The time a Watch request blocks or waits|none|
|username|Username for HTTP authentication|none|
|password|Password for HTTP authentication|none|

The second set of settings you may need to specify pertain to service registration and service discovery. All of these settings should start with `consul:discovery`

|Key|Description|Default|
|---|---|---|
|enabled|Enable to disable the Consul client|true|
|register|Whether to register as a service|true|
|cacheTTL|Time in seconds local cache entries are valid|15|
|deregister|Whether to de-register on shutdown|true|
|serviceName|The service name to register|computed|
|scheme|Scheme to register for service|http|
|hostname|Hostname to use when registering server|computed|
|ipAddress|IP address to register|computed|
|port|Port number to register|none|
|preferIpAddress|Register IP address instead of hostname|false|
|useAspNetCoreUrls|Register with the address ASP.NET Core is listening on|true|
|instanceId|The instance id registered for service|computed|
|tags|The list of tags used when registering a service|none|
|defaultQueryTag|Tag to query for in service list if one is not listed in serverListQueryTags|none|
|queryPassing|Enable or disable whether to add the 'passing' parameter to health requests. This pushes health check passing to the server.|false|
|registerHealthCheck|Enable or disable health check registration|true|
|healthCheckUrl|The health check URL override|none|
|healthCheckPath|Alternate server health check path|'/actuator/health'|
|healthCheckInterval|How often to perform the health check|10s|
|healthCheckTimeout|Timeout for health check|10s|
|healthCheckCriticalTimeout|Timeout to de-register services critical for longer than this value|30m|
|healthCheckTlsSkipVerify|Health check verifies TLS|true|
|instanceZone|Instance zone to use during registration|none|
|instanceGroup|Instance group to use during registration|none|
|defaultZoneMetadataName|Metadata tag name of the zone|'zone'|
|failFast|Throw exception if registration fails|true|
|retry:enabled|Enable or disable retry logic|false|
|retry:maxAttempts|Max retries if retry enabled|6|
|retry:initialInterval|Starting interval|1000ms|
|retry:maxInterval|Maximum retry interval|2000ms|
|retry:multiplier|Retry interval multiplier|1.1|
|heartbeat:enabled|Enable or disable heartbeat logic|false|
|heartbeat:ttlValue|Time to live heartbeat time|30|
|heartbeat:ttlUnit|Time to live heartbeat unit|s|
|heartbeat:intervalRation|The interval ration|2.0/3.0|

### Enable Logging

Sometimes, it is desirable to turn on debug logging. To do that simply add the following to your `appsettings.json`:

```json
{
  "Logging": {
    "IncludeScopes": false,
    "LogLevel": {
      "Default": "Information",
      "Steeltoe":  "Debug"
    }
  }
}
```

### Health Contributor

The Consul package provides a Steeltoe Management Health contributor (`ConsulHealthContributor`) that can be used to monitor Consul server health.

If you use the `AddDiscoveryClient()` extension method and you have configured Consul as your service discovery choice this contributor is automatically added to the container and will automatically picked up an used.

### Configuring Health Check

The health check for a Consul service instance defaults to `/actuator/health`, which is a good default when you have enabled the Steeltoe Management features in your application. You can change this path and provide your own implementation using the `consul:discovery:healthCheckPath` setting. Additionally, the interval that Consul uses to check the health endpoint may also be configured. You can change this setting using the `consul:discovery:healthCheckInterval`. You should use settings such as "10s" and "1m" to represent 10 seconds and 1 minute respectively.

### Configuring Metadata

Consul does not yet support including metadata with service instance registrations, but the Steeltoe `IServiceInstance` has an `IDictionary<string, string> Metadata` property that is used to obtain metadata settings for an instance.

The Steeltoe Consul client uses the Consul tags feature to approximate metadata registration until Consul officially supports associating metadata with instances.

Tags with the form `key=value` will be split and used as `IDictionary` keys and values respectively. Tags without the equal sign will be used as both the key and value. You can add metadata with the `consul:discovery:tags` string array:

```json
{
  "consul": {
    "discovery": {
      "tags": [
        "somekey=somevalue",
        "someothervalue"
      ]
    }
  }
}
```

The above tag list results in metadata that looks like this:

```json
{
  "somekey": "somevalue",
  "someothervalue": "someothervalue"
}
```

### Configuring InstanceId

By default, if no other values are configured, a Consul service instance is registered with an ID that is equal to the applications name concatenated with a random value.

You can change that by configuring the setting `spring:application:instance_id` or `vcap:application:instance_id` to some value and then the ID will be equal to the applications name concatenated with that value.  Note that on Cloud Foundry, `vcap:application:instance_id` will automatically be set for you if you use the Steeltoe Cloud Foundry configuration provider.

You can completely override all of the above by setting `consul:discovery:instanceId` to some value instead.
