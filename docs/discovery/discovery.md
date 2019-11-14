A service registry provides a database that applications can use to implement the Service Discovery pattern, one of the key tenets of a microservices-based architecture. Trying to hand-configure each client of a service or adopt some form of access convention can be difficult and prove to be brittle in production. Instead, applications can use a service registry to dynamically discover and call registered services.

There are several options to choose from when implementing the Service Discovery pattern. Steeltoe has initially chosen to support one based on Eureka and using Netflix's Service Discovery server and client. For more information about Eureka, see the [Netflix/Eureka Wiki](https://github.com/Netflix/eureka/wiki) and the [Spring Cloud Netflix](https://projects.spring.io/spring-cloud/) documentation.

>NOTE: Depending on your hosting environment, service instances you create for the purpose of exploring the Quick Starts on this page may have a cost associated.

# Steeltoe Discovery

Steeltoe provides a set of generalized interfaces for interacting with multiple service discovery back ends. This section will cover the general components first. If you are looking for something specific to the registry server you are using, feel free to skip ahead to the section for [Netflix Eureka](#2-0-netflix-eureka) or [HashiCorp Consul](#3-0-hashicorp-consul).

In order to use any Steeltoe Discovery client, you need to do the following:

* Add appropriate NuGet package reference to your project.
* Configure the settings the Discovery client will use to register services in the service registry.
* Configure the settings the Discovery client will use to discover services in the service registry.
* Add and Use the Discovery client service in the application.
* Use an injected `IDiscoveryClient` to lookup services.

>NOTE: The Steeltoe Discovery implementation (for example: the decision between Eureka and Consul) is automatically setup within the application based on the application configuration provided.

>IMPORTANT: The `Pivotal.Discovery.*` packages have been deprecated in Steeltoe 2.2 and will be removed in a future release.  All functionality provided in those packages has been pushed into the corresponding `Steeltoe.Discovery.*` packages.

## Add NuGet References

<!-- TODO: review this section, its not completely correct -->
The simplest way to get started with Steeltoe Discovery is to add a reference to a package built for either Microsoft's dependency injection or Autofac. Either package will also include all relevant dependencies. If you are using another DI tool, please file an issue to let us know, and in the mean time use the relevant base package:

|App Type|Package|Description|
|---|---|---|
|ASP.NET Core|`Steeltoe.Discovery.ClientCore`|Includes base. Adds ASP.NET Core dependency injection.|
|ASP.NET 4.x with Autofac|`Steeltoe.Discovery.ClientAutofac`|Includes base. Adds Autofac dependency injection.|
|Console/ASP.NET 4.x|`Steeltoe.Discovery.EurekaBase`|Base Eureka functionality. No dependency injection.|
|Console/ASP.NET 4.x|`Steeltoe.Discovery.ConsulBase`|Base Consul functionality. No dependency injection.|

To add this type of NuGet to your project, add an element resembling the following `PackageReference`:

```xml
<ItemGroup>
...
    <PackageReference Include="Steeltoe.Discovery.ClientCore" Version= "2.1.0"/>
...
</ItemGroup>
```

## Initialize Discovery Client

### ASP.NET Core

<!-- TODO: rewrite this section to account for Pivotal packages going away, mention Autofac -->
The next step is to add the Steeltoe Discovery client to the service container and use it to cause the client to start communicating with the server.

You do these two things in the `ConfigureServices()` and `Configure()` methods of the `Startup` class, as shown in the following example:

```csharp
using Pivotal.Discovery.Client;
// or
using Steeltoe.Discovery.Client;

public class Startup {
    ...
    public IConfiguration Configuration { get; private set; }
    public Startup(...)
    {
      ...
    }
    public void ConfigureServices(IServiceCollection services)
    {
        // Add Steeltoe Discovery Client service
        services.AddDiscoveryClient(Configuration);

        // Add framework services.
        services.AddMvc();
        ...
    }
    public void Configure(IApplicationBuilder app, ...)
    {
        ...
        app.UseStaticFiles();
        app.UseMvc();

        // Use the Steeltoe Discovery Client service
        app.UseDiscoveryClient();
    }
    ...
```

>NOTE: If you use the `Pivotal.Discovery.ClientCore` package, you need to add a `using Pivotal.Discovery.Client;`.  If you use the `Steeltoe.Discovery.ClientCore`, you need to add a `Steeltoe.Discovery.Client;`. Doing so is required to gain access to the extension methods described later.

### ASP.NET

### Registering Services

If you configured the clients settings to register services, the service is automatically registered when the `UseDiscoveryClient()` method is called in the `Configure()` method. You do not need to do anything else to cause service registration.

See the [Eureka Client Settings](#2-2-2-eureka-client-settings) or [Consul Client Settings](#3-0-hashicorp-consul)

## Discovering Services

Depending on which Discovery service technology (e.g. Eureka or Consul) you are using the behavior of the client differs.

With Eureka, once the app starts, the client begins to operate in the background, both registering and renewing service registrations and also periodically fetching the service registry from the server.

With Consul, once the app starts, the client registers any services if required and if configured starts a health thread to keep updating the health of the service registration.  No service registrations are fetched by the Consul client until you ask to lookup a service. At that point a request is made of the Consul server.   As a result, you will probably want to use the Steeltoe caching load balancer with the Consul service discovery.

### DiscoveryHttpClientHandler

A simple way to use the registry to lookup services is to use the Steeltoe `DiscoveryHttpClientHandler` with `HttpClient`.

This `FortuneService` class retrieves fortunes from the Fortune microservice, which is registered under a name of `fortuneService`:

```csharp
using Pivotal.Discovery.Client;
// or
// using Steeltoe.Discovery.Client;

...
public class FortuneService : IFortuneService
{
    DiscoveryHttpClientHandler _handler;
    private const string RANDOM_FORTUNE_URL = "https://fortuneService/api/fortunes/random";
    public FortuneService(IDiscoveryClient client)
    {
        _handler = new DiscoveryHttpClientHandler(client);
    }
    public async Task<string> RandomFortuneAsync()
    {
        var client = GetClient();
        return await client.GetStringAsync(RANDOM_FORTUNE_URL);
    }
    private HttpClient GetClient()
    {
        // WARNING: do NOT create a new HttpClient for every request in your code
        // -- you may experience socket exhaustion if you do!
        var client = new HttpClient(_handler, false);
        return client;
    }
}
```

First, notice that the `FortuneService` constructor takes an `IDiscoveryClient` as a parameter. This is Steeltoe's interface for finding services in the service registry. The `IDiscoveryClient` implementation is registered with the service container for use in any controller, view, or service [during initialization](#1-2-initialize-discovery-client). The constructor code for this class uses the client to create an instance of Steeltoe's `DiscoveryHttpClientHandler`.

Next, notice that when the `RandomFortuneAsync()` method is called, the `HttpClient` is created with the Steeltoe handler. The handler's role is to intercept any requests made with the `HttpClient` and to evaluate the URL to see if the host portion of the URL can be resolved from the service registry. In this example, the `fortuneService` name should be resolved into an actual `host:port` before letting the request continue.

If the name cannot be resolved, the handler ignores the request URL and lets the request continue unchanged.

>NOTE: `DiscoveryHttpClientHandler` performs random load balancing by default. That is, if there are multiple instances registered under a particular service name, the handler randomly selects one of those instances each time the handler is invoked. For more information, see the section on [load balancing](#1-4-load-balancing)

### Using HttpClientFactory

In addition to the `DiscoveryHttpClientHandler` mentioned above, you also have the option to use the new [HttpClientFactory](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests) together with the Steeltoe provided `DiscoveryHttpMessageHandler` for service lookup.

`DiscoveryHttpMessageHandler` is a `DelegatingHandler` that can be used, much like the `DiscoveryHttpClientHandler`, to intercept requests and to evaluate the URL to see if the host portion of the URL can be resolved from the current service registry.  The handler will do this for any `HttpClient` created by the factory.

After [initializing the discovery client](#1-2-initialize-discovery-client), you can easily configure `HttpClient`:

```csharp
public class Startup
{
    ...
    public void ConfigureServices(IServiceCollection services)
    {
      ...
      // Add Steeltoe handler to container (this line can be omitted when using Steeltoe versions >= 2.2.0)
      services.AddTransient<DiscoveryHttpMessageHandler>();

      // Configure HttpClient
      services.AddHttpClient("fortunes", c =>
      {
        c.BaseAddress = new Uri("https://fortuneService/api/fortunes/");
      })
      .AddHttpMessageHandler<DiscoveryHttpMessageHandler>()
      .AddTypedClient<IFortuneService, FortuneService>();
      ...
    }
    ...
}
```

The updated version of `FortuneService` is a bit simpler:

```csharp
public class FortuneService : IFortuneService
{
    private const string RANDOM_FORTUNE_URL = "https://fortuneService/api/fortunes/random";
    private HttpClient _client;
    public FortuneService(HttpClient client)
    {
        _client = client;
    }
    public async Task<string> RandomFortuneAsync()
    {
        return await _client.GetStringAsync(RANDOM_FORTUNE_URL);
    }
}
```

Check out the Microsoft documentation on [HttpClientFactory](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests) to see all the various ways you can make use of message handlers.

>NOTE: `DiscoveryHttpMessageHandler` has an optional `ILoadBalancer` parameter. If no `ILoadBalancer` is provided via dependency injection, a `RandomLoadBalancer` is used. To change this behavior, add an `ILoadBalancer` to the DI container or use a load-balancer first configuration as described within section 1.4 on this page.

### Using IDiscoveryClient

In the event the handler options don't serve your needs, you can always make lookup requests directly on the `IDiscoveryClient` interface.

These methods available on an `IDiscoveryClient` provide access to services and service instances available in the registry:

```csharp
/// <summary>
/// Gets all known service Ids
/// </summary>
IList<string> Services { get; }

/// <summary>
/// Get all ServiceInstances associated with a particular serviceId
/// </summary>
/// <param name="serviceId">the serviceId to lookup</param>
/// <returns>List of service instances</returns>
IList<IServiceInstance> GetInstances(string serviceId);
```

## Load Balancing

Any time a client needs to select a service instance to send a request to, some mechanism is required for selecting the instance to call. In all mechanisms provided for service discovery in Steeltoe versions before 2.2.0, service instances were selected randomly. `Steeltoe.Common` 2.2.0 added a new abstraction named `ILoadBalancer`, which provides configurable load balancing.

### ILoadBalancer

The `ILoadBalancer` interface defines two methods:

```csharp
  public interface ILoadBalancer
  {
      /// <summary>
      /// Evaluates a Uri for a host name that can be resolved into a service instance
      /// </summary>
      /// <param name="request">A Uri containing a service name that can be resolved into one or more service instances</param>
      /// <returns>The original Uri, with serviceName replaced by the host:port of a service instance</returns>
      Task<Uri> ResolveServiceInstanceAsync(Uri request);

      /// <summary>
      /// A mechanism for tracking statistics for service instances
      /// </summary>
      /// <param name="originalUri">The original request Uri</param>
      /// <param name="resolvedUri">The Uri resolved by the load balancer</param>
      /// <param name="responseTime">The amount of time taken for a remote call to complete</param>
      /// <param name="exception">Any exception called during calls to a resolved service instance</param>
      /// <returns>A task</returns>
      Task UpdateStatsAsync(Uri originalUri, Uri resolvedUri, TimeSpan responseTime, Exception exception);
  }
```

Any implementation of `ILoadBalancer` is expected to know how to interact with some form of service discovery mechanism. The included load balancers expect an `IServiceInstanceProvider` to be available in the DI service container, so they still require configuration of Eureka, Consul or some other mechanism for providing service instances.

### Random Load Balancer

The `RandomLoadBalancer`, as the name implies, randomly selects a service instance from all instances that are resolved from a given service name. The `ILoadBalancer` implementation adds the (optional) ability to cache service instance data, which is useful for `IServiceInstanceProvider` or `IDiscoveryClient` implementations that do not provide their own caching (such as the Consul provider). Service instance data caching happens automatically if an `IDistributedCache` instance is provided via constructor injection.

>NOTE: `RandomLoadBalancer` does not track stats or exceptions. `UpdateStatsAsync` simply returns `Task.CompletedTask`

#### Using HttpClientFactory

To add a service registry-backed random load balancer to an `HttpClient` constructed using `HttpClientFactory`, you may use the `AddRandomLoadBalancer()` extension:

```csharp
  services.AddHttpClient("fortunes")
      .AddRandomLoadBalancer()
```

>NOTE: This is functionally equivalent to using the default behavior of the `DiscoveryHttpMessageHandler`, as described [above](#1-3-2-using-httpclientfactory)

#### Using an HttpClientHandler

The random load balancer can be used with the included `HttpClientHandler` that works with any `ILoadBalancer`:

```csharp
  private HttpClient _httpClient;
  public FortuneService(IDiscoveryClient discoveryClient)
  {
      var loadBalancer = new RandomLoadBalancer(discoveryClient);
      var handler = new LoadBalancerHttpClientHandler(loadBalancer);
      _httpClient = new HttpClient(handler);
  }
```

### Round Robin Load Balancer

The provided round robin load balancer sends traffic to service instances in sequential order, as they are provided by the `IServiceInstanceProvider`. Like the `RandomLoadBalancer`, the `RoundRobinLoadBalancer` also includes the (optional) ability to cache service instances if an `IDistributedCache` instance is provided via constructor injection. Additionally, when a provided `IDistributedCache` instance is shared amongst clients (for example: using a shared Redis cache for multiple front-end application instances) the round robin sequence tracking will be shared across clients, ensuring an even load distribution.

>NOTE: `RoundRobinLoadBalancer` does not track stats or exceptions. `UpdateStatsAsync` simply returns `Task.CompletedTask`

#### Using with HttpClientFactory

To add a service registry-backed round robin load balancer to an `HttpClient`, you may use the `AddRoundRobinLoadBalancer()` extension. This example also adds a Redis cache so that regardless of which client service instance makes the call, backend service instances will be called in round robin order:

```csharp
  services.AddDistributedRedisCache(Configuration);
  services.AddHttpClient("fortunes")
      .AddRoundRobinLoadBalancer()
```

#### Using an HttpClientHandler

The round robin load balancer can be used with the included `HttpClientHandler` that works with any `ILoadBalancer`:

```csharp
  private HttpClient _httpClient;
  public FortuneService(IDiscoveryClient discoveryClient)
  {
      var loadBalancer = new RoundRobinLoadBalancer(discoveryClient);
      var handler = new LoadBalancerHttpClientHandler(loadBalancer);
      _httpClient = new HttpClient(handler);
  }
```

### Custom ILoadBalancer

If the provided load balancer implementations don't suit your needs, you are free to create your own implementation of `ILoadBalancer`.

This example shows a load balancer that would always return the first listed instance, no matter what:

```csharp
  private readonly IServiceInstanceProvider _serviceInstanceProvider;

  public FirstInstanceLoadBalancer(IServiceInstanceProvider serviceInstanceProvider)
  {
      _serviceInstanceProvider = serviceInstanceProvider;
  }

  public Task<Uri> ResolveServiceInstanceAsync(Uri request)
  {
      var availableServiceInstances = _serviceInstanceProvider.GetInstances(request.Host);
      return Task.FromResult(new Uri(availableServiceInstances[0].Uri, request.PathAndQuery));
  }

  public Task UpdateStatsAsync(Uri originalUri, Uri resolvedUri, TimeSpan responseTime, Exception exception)
  {
      return Task.CompletedTask;
  }
```

#### Usage with HttpClientFactory

Custom load balancers can be added to the HttpClient pipeline with an included generic extension:

```csharp
    services.AddHttpClient("fortunes")
        .AddLoadBalancer<RandomLoadBalancer>()
```

With this model, a `LoadBalancerDelegatingHandler` will expect an `ILoadBalancer` to be provided via dependency injection, so be sure to add yours to the DI container.

#### Using an HttpClientHandler

Additionally, your custom load balancer can also be used with the included `HttpClientHandler`. Create an instance of your load balancer, pass it to a `LoadBalancerHttpClientHandler` and create an `HttpClient` that uses that handler:

```csharp
  private HttpClient _httpClient;
  public FortuneService(IDiscoveryClient discoveryClient)
  {
      var loadBalancer = new FirstInstanceLoadBalancer(discoveryClient);
      var handler = new LoadBalancerHttpClientHandler(loadBalancer);
      _httpClient = new HttpClient(handler);
  }
```

## Enable Logging

Sometimes, it is desirable to turn on debug logging in the Discovery client. To do so, you can modify the `appsettings.json` file and turn on Debug level logging for the Steeltoe/Pivotal components, as shown in the following example:

Here is an example `appsettings.json` file:

```json
{
  "Logging": {
    "IncludeScopes": false,
    "LogLevel": {
      "Default": "Warning",
      "Pivotal": "Debug",
      "Steeltoe": "Debug"
    }
  },
  ...
}
```

# Netflix Eureka

The Eureka client implementation lets applications register services with a Eureka server and discover services registered by other applications. This Steeltoe client is an implementation of the 1.0 version of the Netflix Eureka client.

The Eureka client implementation supports the following .NET application types:

* ASP.NET (MVC, WebForm, WebAPI, WCF)
* ASP.NET Core
* Console apps (.NET Framework and .NET Core)

 In addition to the [quick start](#2-1-quick-start), you can choose from several other Steeltoe sample applications when looking for help in understanding how to use this client:

* [AspDotNet4/Fortune-Teller-Service4](https://github.com/SteeltoeOSS/Samples/tree/master/Discovery/src/AspDotNet4/Fortune-Teller-Service4): Same as the Quick Start next but built for ASP.NET 4.x and using the Autofac IOC container.
* [AspDotNet4/Fortune-Teller-UI4](https://github.com/SteeltoeOSS/Samples/tree/master/Discovery/src/AspDotNet4/Fortune-Teller-UI4): Same as the Quick Start next but built for ASP.NET 4.x and using the Autofac IOC container
* [MusicStore](https://github.com/SteeltoeOSS/Samples/tree/master/MusicStore): A sample application showing how to use all of the Steeltoe components together in a ASP.NET Core application. This is a microservices-based application built from the ASP.NET Core MusicStore reference app provided by Microsoft.
* [FreddysBBQ](https://github.com/SteeltoeOSS/Samples/tree/master/FreddysBBQ): A polyglot microservices-based sample application showing interoperability between Java and .NET on Cloud Foundry. It is secured with OAuth2 Security Services and using Spring Cloud Services.

The source code for discovery can be found [here](https://github.com/SteeltoeOSS/Discovery).

## Usage

The following sections describe how to use the Eureka client.

* [Eureka Settings](#2-2-1-eureka-settings)
* [Bind to Cloud Foundry](#2-2-2-bind-to-cloud-foundry)
* [Enable Logging](#2-2-3-enable-logging)
* [Configuring Health Contributors](#2-2-4-configuring-health-contributors)
* [Configuring Health Checks](#2-2-5-configuring-health-checks)
* [Configuring Multiple ServiceUrls](#2-2-6-configuring-multiple-serviceurls)
* [Configuring Metadata](#2-2-7-configuring-metadata)

You should know how the new .NET [Configuration service](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration) works before starting to use the client. A basic understanding of the `ConfigurationBuilder` and how to add providers to the builder is necessary in order to configure the client.

You should also know how the ASP.NET Core [Startup](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/startup) class is used in configuring the application services and the middleware used in the app. Pay particular attention to the usage of the `Configure()` and `ConfigureServices()` methods.

You should also have a good understanding of the [Spring Cloud Eureka Server](https://projects.spring.io/spring-cloud/).

In order to use the Steeltoe Discovery client, you need to do the following:

* Add appropriate NuGet package reference to your project.
* Configure the settings the Discovery client will use to register services in the service registry.
* Configure the settings the Discovery client will use to discover services in the service registry.
* Add and Use the Discovery client service in the application.
* Use an injected `IDiscoveryClient` to lookup services.

>NOTE: Most of the example code in the following sections is based on using Discovery in a ASP.NET Core application. If you are developing a ASP.NET 4.x application or a console-based app, see the [other samples](https://github.com/SteeltoeOSS/Samples/tree/master/Discovery) for example code you can use.

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

The samples and most templates are already set up to read from `appsettings.json`. See [Reading Configuration Values](#reading-configuration-values) for more information about reading configuration values.

### Bind to Cloud Foundry

When you want to use a Eureka Server on Cloud Foundry and you have installed [Spring Cloud Services](https://docs.pivotal.io/spring-cloud-services/1-5/common/index.html), you can create and bind a instance of the server to the application by using the Cloud Foundry CLI, as follows:

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

For more information on using the Eureka Server on Cloud Foundry, see the [Spring Cloud Services](https://docs.pivotal.io/spring-cloud-services/1-5/common/index.html) documentation.

Once the service is bound to your application, the connection properties are available in `VCAP_SERVICES`. See [Reading Configuration Values](#reading-configuration-values) for more information on reading `VCAP_SERVICES`.

### Enable Logging

Sometimes, it is desirable to turn on debug logging in the Eureka client. To do that simply add the following to your `appsettings.json`:

```json
{
  "Logging": {
    "IncludeScopes": false,
    "LogLevel": {
      "Default": "Information",
      "Pivotal": "Debug",
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

# HashiCorp Consul

The Consul client implementation lets applications register services with a Consul server and discover services registered by other applications. This Steeltoe client utilizes the Consul .NET package provided by the open source project [consuldotnet](https://github.com/PlayFab/consuldotnet).

The Consul client implementation supports the following .NET application types:

* ASP.NET (MVC, WebForm, WebAPI, WCF)
* ASP.NET Core
* Console apps (.NET Framework and .NET Core)

The source code for discovery can be found [here](https://github.com/SteeltoeOSS/Discovery).

## Usage

The following sections describe how to use the Consul client.

* [Consul Settings](#3-2-1-consul-settings)
* [Enable Logging](#3-2-2-enable-logging)
* [Health Contributor](#3-2-3-health-contributors)
* [Configuring Health Check](#3-2-4-configuring-health-check)
* [Configuring Metadata](#3-2-5-configuring-metadata)
* [Configuring InstanceId](#3-2-6-configuring-instanceid)

You should know how the new .NET [Configuration service](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration) works before starting to use the client. A basic understanding of the `ConfigurationBuilder` and how to add providers to the builder is necessary in order to configure the client.

You should also know how the ASP.NET Core [Startup](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/startup) class is used in configuring the application services and the middleware used in the app. Pay particular attention to the usage of the `Configure()` and `ConfigureServices()` methods.

It might be helpful to have an understanding of the [Spring Cloud Consul](https://spring.io/projects/spring-cloud-consul) project as we have based our work on this project.

In order to use the Steeltoe Discovery client, you need to do the following:

* Add appropriate NuGet package reference to your project.
* Configure the settings the Discovery client will use to register services in the service registry.
* Configure the settings the Discovery client will use to discover services in the service registry.
* Add and Use the Discovery client service in the application.
* Use an injected `IDiscoveryClient` to lookup services.

>NOTE: Most of the example code in the following sections is based on using Discovery in a ASP.NET Core application. If you are developing a ASP.NET 4.x application or a console-based app, see the [other samples](https://github.com/SteeltoeOSS/Samples/tree/master/Discovery) for example code you can use.

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
|deregister|Whether to de-register on shutdown|true|
|serviceName|The service name to register|computed|
|scheme|Scheme to register for service|http|
|hostname|Hostname to use when registering server|computed|
|ipAddress|IP address to register|computed|
|port|Port number to register|none|
|preferIpAddress|Register IP address instead of hostname|false|
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
      "Pivotal": "Debug",
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
