# Netflix Eureka

The Eureka client implementation lets applications register services with a Eureka server and discover services registered by other applications. This Steeltoe client is an implementation of the 1.0 version of the Netflix Eureka client.

The Eureka client implementation supports the following .NET application types:

* ASP.NET (MVC, WebForm, WebAPI, WCF)
* ASP.NET Core
* Console apps (.NET Framework and .NET Core)

Here are several Steeltoe sample applications when looking for help in understanding how to use this client:

* [AspDotNet4/Fortune-Teller-Service4](https://github.com/SteeltoeOSS/Samples/tree/2.x/Discovery/src/AspDotNet4/Fortune-Teller-Service4): Same as the Quick Start next but built for ASP.NET 4.x and using the Autofac IOC container.
* [AspDotNet4/Fortune-Teller-UI4](https://github.com/SteeltoeOSS/Samples/tree/2.x/Discovery/src/AspDotNet4/Fortune-Teller-UI4): Same as the Quick Start next but built for ASP.NET 4.x and using the Autofac IOC container
* [MusicStore](https://github.com/SteeltoeOSS/Samples/tree/2.x/MusicStore): A sample application showing how to use all of the Steeltoe components together in a ASP.NET Core application. This is a microservices-based application built from the ASP.NET Core MusicStore reference app provided by Microsoft.
* [FreddysBBQ](https://github.com/SteeltoeOSS/Samples/tree/2.x/FreddysBBQ): A polyglot microservices-based sample application showing interoperability between Java and .NET on Cloud Foundry. It is secured with OAuth2 Security Services and using Spring Cloud Services.

## Usage

You should know how the new .NET [Configuration service](https://docs.microsoft.com/aspnet/core/fundamentals/configuration) works before starting to use the client. A basic understanding of the `ConfigurationBuilder` and how to add providers to the builder is necessary in order to configure the client.

You should also know how the ASP.NET Core [Startup](https://docs.microsoft.com/aspnet/core/fundamentals/startup) class is used in configuring the application services and the middleware used in the app. Pay particular attention to the usage of the `Configure()` and `ConfigureServices()` methods.

You should also have a good understanding of the [Spring Cloud Eureka Server](https://projects.spring.io/spring-cloud/).

In order to use the Steeltoe Discovery client, you need to do the following:

* Add appropriate NuGet package reference to your project.
* Configure the settings the Discovery client will use to register services in the service registry.
* Configure the settings the Discovery client will use to discover services in the service registry.
* Add and Use the Discovery client service in the application.
* Use an injected `IDiscoveryClient` to lookup services.

>NOTE: Most of the example code in the following sections is based on using Discovery in a ASP.NET Core application. If you are developing a ASP.NET 4.x application or a console-based app, see the [other samples](https://github.com/SteeltoeOSS/Samples/tree/2.x/Discovery) for example code you can use.

### Eureka Settings

To get the Steeltoe Discovery client to properly communicate with the Eureka server, you need to provide a few configuration settings to the client.

What you provide depends on whether you want your application to register a service and whether it also needs to discover services with which to communicate.

General settings that control the behavior of the client are found under the prefix with a key of `eureka:client`. Settings that affect registering services are found under the `eureka:instance` prefix.

The following table describes the settings that control the overall behavior of the client:

All of these settings should start with `eureka:client:`

|Key|Description|Default|
|---|---|---|
|shouldRegisterWithEureka|Enable or disable registering as a service|true|
|shouldFetchRegistry|Enable or disable discovering services|true|
|shouldGZipContent|Enable or disable GZip usage between client and Eureka server|true|
|serviceUrl|Comma delimited list of Eureka server endpoints|`http://localhost:8761/eureka`|
|validateCertificates|Enable or disable certificate validation|true|
|registryFetchIntervalSeconds|Service fetch interval|30s|
|shouldFilterOnlyUpInstances|Whether to fetch only UP instances|true|
|instanceInfoReplicationIntervalSeconds|How often to replicate instance changes|40s |
|shouldDisableDelta|Whether to disable fetching of delta and, instead, get the full registry|false |
|registryRefreshSingleVipAddress|Whether to be interested in only the registry information for a single VIP|none |
|shouldOnDemandUpdateStatusChange|Whether status updates are trigger on-demand register/update|true|
|accessTokenUri|URI to use to obtain OAuth2 access token|none|
|clientSecret|Secret to use to obtain OAuth2 access token|none|
|clientId|Client ID to use to obtain OAuth2 access token|none|
|cacheTTL|Time in seconds local cache entries are valid|15|
|eurekaServer:proxyHost|Proxy host to Eureka Server|none|
|eurekaServer:proxyPort|Proxy port to Eureka Server|none|
|eurekaServer:proxyUserName|Proxy user name to Eureka Server|none|
|eurekaServer:proxyPassword| Proxy password to Eureka Server|none
|eurekaServer:shouldGZipContent|Whether to compress content|true|
|eurekaServer:connectTimeoutSeconds|Connection timeout|5s|
|eurekaServer:retryCount|Number of times to retry Eureka Server requests|3|
|health:enabled|Enable or disable management health contributor|true|
|health:checkEnabled|Enable or disable Eureka health check handler|true|
|health:monitoredApps|List apps the management health contributor monitors|All apps in registry|

**NOTE**: **Some settings above affect registering as a service as well.**

The following table describes the settings you can use to configure the behavior of the client as it relates to registering services:

|Key|Description|Default|
|---|---|---|
|appName|Name of the application to be registered with Eureka|'spring:application:name' or 'unknown'|
|port|Port on which the instance is registered|80|
|hostName|Address on which the instance is registered|computed|
|instanceId|Unique ID (within the scope of the `appName`) of the instance registered with Eureka|`computed`|
|appGroupName|Name of the application group to be registered with Eureka|none|
|instanceEnabledOnInit|Whether the instance should take traffic as soon as it is registered|false|
|securePort|Secure port on which the instance should receive traffic|443|
|nonSecurePortEnabled|Non-secure port enabled for traffic|true|
|securePortEnabled|Secure port enabled for traffic|false|
|leaseRenewalIntervalInSeconds|How often client needs to send heartbeats|30s|
|leaseExpirationDurationInSeconds|Time the Eureka server waits before removing instance|90s|
|vipAddress|Virtual host name|hostName + port|
|secureVipAddress|Secure virtual host name|hostName + securePort||
|metadataMap|Name/value pairs associated with the instance|none|
|statusPageUrlPath|Relative status page path for this instance|`/Status`|
|statusPageUrl|Absolute status page for this instance|computed|
|homePageUrlPath||`/`|
|homePageUrl|Absolute home page for this instance|computed|
|healthCheckUrlPath||`/healthcheck`|
|healthCheckUrl|Absolute health check page for this instance|computed|
|secureHealthCheckUrl|Secured absolute health check page for this instance|computed|
|ipAddress|IP address to register|computed|
|preferIpAddress|Whether to register by using IpAddress instead of hostname|false|
|registrationMethod|How to register service on Cloud Foundry. Can be `route`, `direct`, or `hostname`|`route`|

All of the settings in the preceding table should start with `eureka:instance:`.

You should register by using the `direct` setting mentioned earlier when you want to use container-to-container networking on Cloud Foundry. You should use the `hostname` setting on Cloud Foundry when you want the registration to use whatever value is configured or computed as `eureka:instance:hostName`.

For a complete understanding of the effects of many of these settings, we recommend that you review the documentation on the [Netflix Eureka Wiki](https://github.com/Netflix/eureka/wiki). In most cases, unless you are confident you understand the effects of changing the values from their defaults, we recommend that you use the defaults.

#### Settings to Fetch Registry

The following example shows the clients settings in JSON that are necessary to cause the client to fetch the service registry from the server at an address of `http://localhost:8761/eureka/`:

```json
{
"spring": {
    "application": {
      "name": "fortuneUI"
    }
  },
  "eureka": {
    "client": {
      "serviceUrl": "http://localhost:8761/eureka/",
      "shouldRegisterWithEureka": false
    }
  }
  ...
}
```

The `eureka:client:shouldRegisterWithEureka` instructs the client to NOT register any services in the registry, as the application does not offer any services (that is, it only wants to discover).

>NOTE: If you use self-signed certificates on Cloud Foundry, you might run into SSL certificate validation issues when pushing apps. A quick way to work around this is to disable certificate validation until a proper solution can be put in place.

#### Settings to Register Services

The following example shows the clients settings in JSON that are necessary to cause the client to register a service named `fortuneService` with a Eureka Server at an address of `http://localhost:8761/eureka/`:

```json
{
 "spring": {
    "application": {
      "name":  "fortuneService"
    }
  },
  "eureka": {
    "client": {
      "serviceUrl": "http://localhost:8761/eureka/",
      "shouldFetchRegistry": false
    },
    "instance": {
      "port": 5000
    }
  }
  ...
}
```

The `eureka:instance:port` setting is the port on which the service is registered. The hostName portion is determined automatically at runtime. The `eureka:client:shouldFetchRegistry` setting instructs the client NOT to fetch the registry as the app does not need to discover services. It only wants to register a service. The default for the `shouldFetchRegistry` setting is true.

The samples and most templates are already set up to read from `appsettings.json`.

### Bind to Cloud Foundry

When you want to use a Eureka Server on Cloud Foundry and you have installed [Spring Cloud Services](https://docs.pivotal.io/spring-cloud-services/), you can create and bind a instance of the server to the application by using the Cloud Foundry CLI, as follows:

```bash
# Create eureka server instance named `myDiscoveryService`
cf create-service p-service-registry standard myDiscoveryService

# Wait for service to become ready
cf services

# Bind the service to `myApp`
cf bind-service myApp myDiscoveryService

# Restage the app to pick up change
cf restage myApp
```

For more information on using the Eureka Server on Cloud Foundry, see the [Spring Cloud Services](https://docs.pivotal.io/spring-cloud-services/) documentation.

Once the service is bound to your application, the connection properties are available in `VCAP_SERVICES`.

### Enable Logging

Sometimes, it is desirable to turn on debug logging in the Eureka client. To do that simply add the following to your `appsettings.json`:

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

### Configuring Health Contributors

The Eureka package provides two different Steeltoe Management Health contributors that can be used to monitor Eureka server health.

The first one, `EurekaServerHealthContributor` is used to determine and report the health of the connection to the Eureka servers. It looks at the status of last good registry fetch and/or the last heartbeat attempt and using that information computes the health of the connection.

If you use the `AddDiscoveryClient()` extension method and you have configured Eureka as your service discovery choice this contributor is automatically added to the container and will automatically picked up an used.

The contributor is enabled by default, but can be disabled by setting `eureka:client:health:enabled=false`.

The second contributor that you can enable is the `EurekaApplicationsHealthContributor`. This contributor is not enabled by default, so you must add it to the service container yourself:

```csharp
    services.AddSingleton<IHealthContributor, EurekaApplicationsHealthContributor>();
```

The `EurekaApplicationsHealthContributor` can be used to report the health of a configurable list of registered services based on the status of the service in the registry. For each service it is configured to monitor it looks at all of the instances of that service and if all of the instances are marked `DOWN`, then the service will be reported in bad health.  You can configure the services that it monitors using the `eureka:client:health:monitoredApps` configuration setting.  Typically you would set this to the list of external service names the app is dependent on and if unavailable would impact the operation of the app.

### Configuring Health Checks

By default, Eureka uses the client heartbeat to determine if a client is up. Unless specified otherwise, the Eureka client does not propagate the current health status of the application as calculated by the health contributors configured for the application. Consequently, after successful registration, Eureka always announces that the application is in 'UP' state. This behavior can be altered by enabling Eureka health checks, which results in propagating application status to Eureka. As a consequence, every other application does not send traffic to applications in states other then 'UP'.

To enable this behavior you need to add the `IHealthCheckHandler` to your service container.  The handler is not added to the container by default.

```csharp
    services.AddSingleton<IHealthCheckHandler, ScopedEurekaHealthCheckHandler>();
```

You can enable or disable the handler by using the following `eureka:client:health:checkEnabled` configuration settings.  It is enabled by default.

If you require more control over the health check, consider implementing your own `IHealthCheckHandler`.

### Configuring Multiple ServiceUrls

You can specify a comma delimited list of Eureka server URLs the client will use when registering or fetching the service registry. Those servers listed should be part of a properly configured Eureka server cluster and should be using peer to peer communications to keep in sync.

The Eureka client will automatically failover to the other nodes in the cluster. When a failed Eureka server node comes back up, the Eureka client will automatically reconnect back to the server at some point.

### Configuring Metadata

It is worth spending a bit of time understanding how the Eureka metadata works so you can use it in a way that makes sense in your application.

There is standard metadata information such as hostname, IP address, port numbers, status page, and health check endpoint that is associated with every service registration. These are published in the service registry and are used by clients to contact the services in a straightforward way.

Additional metadata can be added to instance registrations using the configuration setting `eureka:instance:metadataMap`. The metadata you supply using this configuration is added to the service registration and becomes accessible in remote clients.

In general, additional metadata does not change the behavior of the client, unless the client is made aware of the meaning of the metadata.
