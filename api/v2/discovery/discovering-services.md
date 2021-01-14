# Discovering Services

Depending on which Discovery service technology (e.g. Eureka or Consul) you are using the behavior of the client differs.

With Eureka, once the app starts, the client begins to operate in the background, both registering and renewing service registrations and also periodically fetching the service registry from the server.

With Consul, once the app starts, the client registers any services if required and if configured starts a health thread to keep updating the health of the service registration.  No service registrations are fetched by the Consul client until you ask to lookup a service. At that point a request is made of the Consul server.   As a result, you will probably want to use the Steeltoe caching load balancer with the Consul service discovery.

## DiscoveryHttpClientHandler

A simple way to use the registry to lookup services is to use the Steeltoe `DiscoveryHttpClientHandler` with `HttpClient`.

This `FortuneService` class retrieves fortunes from the Fortune microservice, which is registered under a name of `fortuneService`:

```csharp
using Steeltoe.Discovery.Client;

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

First, notice that the `FortuneService` constructor takes an `IDiscoveryClient` as a parameter. This is Steeltoe's interface for finding services in the service registry. The `IDiscoveryClient` implementation is registered with the service container for use in any controller, view, or service during initialization. The constructor code for this class uses the client to create an instance of Steeltoe's `DiscoveryHttpClientHandler`.

Next, notice that when the `RandomFortuneAsync()` method is called, the `HttpClient` is created with the Steeltoe handler. The handler's role is to intercept any requests made with the `HttpClient` and to evaluate the URL to see if the host portion of the URL can be resolved from the service registry. In this example, the `fortuneService` name should be resolved into an actual `host:port` before letting the request continue.

If the name cannot be resolved, the handler ignores the request URL and lets the request continue unchanged.

>NOTE: `DiscoveryHttpClientHandler` performs random load balancing by default. That is, if there are multiple instances registered under a particular service name, the handler randomly selects one of those instances each time the handler is invoked.

## Using HttpClientFactory

In addition to the `DiscoveryHttpClientHandler` mentioned above, you also have the option to use the new [HttpClientFactory](https://docs.microsoft.com/aspnet/core/fundamentals/http-requests) together with the Steeltoe provided `DiscoveryHttpMessageHandler` for service lookup.

`DiscoveryHttpMessageHandler` is a `DelegatingHandler` that can be used, much like the `DiscoveryHttpClientHandler`, to intercept requests and to evaluate the URL to see if the host portion of the URL can be resolved from the current service registry.  The handler will do this for any `HttpClient` created by the factory.

After initializing the discovery client, you can easily configure `HttpClient`:

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

Check out the Microsoft documentation on [HttpClientFactory](https://docs.microsoft.com/aspnet/core/fundamentals/http-requests) to see all the various ways you can make use of message handlers.

>NOTE: `DiscoveryHttpMessageHandler` has an optional `ILoadBalancer` parameter. If no `ILoadBalancer` is provided via dependency injection, a `RandomLoadBalancer` is used. To change this behavior, add an `ILoadBalancer` to the DI container or use a load-balancer first configuration as described within section 1.4 on this page.

## Using IDiscoveryClient

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
