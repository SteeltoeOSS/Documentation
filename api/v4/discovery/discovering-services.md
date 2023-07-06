# Discovering Services

Depending on which Discovery service technology (for example, Eureka or Consul) you use, the behavior of the client differs.

With Eureka, once the application starts, the client begins to operate in the background, both registering and renewing service registrations and also periodically fetching the service registry from the server.

With Consul, once the app starts, the client registers any services (if required) and (if configured to do so) starts a health thread to keep updating the health of the service registration. The Consul client fetches no service registrations until you ask to look up a service. At that point, a request is made to the Consul server. As a result, you probably want to use the Steeltoe caching load balancer with the Consul service discovery.

## Using HttpClientFactory

The recommended approach for discovering services is to use `HttpClient` supplied through [HttpClientFactory](https://docs.microsoft.com/aspnet/core/fundamentals/http-requests), augmented with the `DiscoveryHttpMessageHandler` (provided by Steeltoe).

`DiscoveryHttpMessageHandler` is a `DelegatingHandler` that you can use, to intercept requests and to evaluate the URL to see if the host portion of the URL can be resolved from the current service registry. The handler does this for any `HttpClient` created by the factory.

After initializing the discovery client, you can configure `HttpClient` to include `DiscoveryHttpMessageHandler` with the extension `AddServiceDiscovery`:

```csharp
public class Startup
{
    ...
    public void ConfigureServices(IServiceCollection services)
    {
      ...
      // Configure HttpClient
      services.AddHttpClient("fortunes")
        .AddServiceDiscovery()
        .AddTypedClient<IFortuneService, FortuneService>();
      ...
    }
    ...
}
```

This `HttpClient` can be injected into `FortuneService` for a nice, clean experience:

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

Check out the Microsoft documentation on [`HttpClientFactory`](https://docs.microsoft.com/aspnet/core/fundamentals/http-requests) to see all the various ways you can make use of message handlers.

>`DiscoveryHttpMessageHandler` has an optional `ILoadBalancer` parameter. If no `ILoadBalancer` is provided through dependency injection, a `RandomLoadBalancer` is used. To change this behavior, add an `ILoadBalancer` to the DI container or use a load-balancer first configuration, as described within section 1.4 on this page.

## DiscoveryHttpClientHandler

Another way to use the registry to look up services is to use the Steeltoe `DiscoveryHttpClientHandler` with `HttpClient`.

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

>By default, `DiscoveryHttpClientHandler` performs random load balancing. That is, if there are multiple instances registered under a particular service name, the handler randomly selects one of those instances each time the handler is invoked.

## Using IDiscoveryClient

In the event the handler options do not serve your needs, you can always make lookup requests directly on the `IDiscoveryClient` interface.

The methods available on an `IDiscoveryClient` provide access to services and service instances available in the registry:

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
