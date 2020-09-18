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

### Setting Cache Configuration

For both Random and Round Robin load balancers, there are two ways to configure cache entries. The simplest is to use the `CacheTTL` property in the client configuration for your discovery client. The other option is to inject your own `DistributedCacheEntryOptions` into the service container before the call to configure service discovery.

This example sets cache expiration to 30 seconds after an entry is recorded:

```csharp
  services.AddSingleton(cacheOptions => new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30) });
  services.AddDiscoveryClient(config);
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
