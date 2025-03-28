# Discovering services

Resolving friendly names happens through a load balancer, which queries `IDiscoveryClient`s for available service instances.

## Using HttpClientFactory

The recommended approach is to use a typed `HttpClient`, supplied through
[HttpClientFactory](https://learn.microsoft.com/aspnet/core/fundamentals/http-requests). Call the `.AddServiceDiscovery()`
extension method from the `Steeltoe.Discovery.HttpClients` NuGet package to activate service discovery.

> [!NOTE]
> The `AddServiceDiscovery()` extension method takes an optional `ILoadBalancer` parameter.
> If no load balancer is provided, the built-in `RandomLoadBalancer` is activated,
> which uses randomized selection of service instances.

For example, consider the following typed client:
```csharp
public sealed class OrderService(HttpClient httpClient)
{
    public async Task<OrderModel?> GetOrderByIdAsync(
        string orderId, CancellationToken cancellationToken)
    {
        return await httpClient.GetFromJsonAsync<OrderModel?>(
            $"https://ordering-api/orders/{orderId}", cancellationToken);
    }
}
```

This typed client can be configured to use service discovery. Add the following code to `Program.cs`
to rewrite the `https://ordering-api` part to a service instance obtained from Eureka.

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEurekaDiscoveryClient();
builder.Services.AddHttpClient<OrderService>().AddServiceDiscovery();
```

With the above code in place, you can inject `OrderService` in your MVC controller, for example:

```csharp
public sealed class OrdersController(OrderService orderService) : Controller
{
    [HttpGet("{orderId}")]
    public async Task<IActionResult> Index(string orderId, CancellationToken cancellationToken)
    {
        var model = await orderService.GetOrderByIdAsync(orderId, cancellationToken);
        return View(model);
    }
}
```

When the MVC controller executes, `HttpClientFactory` returns an `HttpClient` that is configured for service discovery.
Under the covers, Steeltoe adds `DiscoveryHttpDelegatingHandler<TLoadBalancer>` to the HTTP handler pipeline,
which intercepts requests and rewrites the scheme/host/port with the values obtained from the registry (via its load balancer).

### Global service discovery

To use service discovery for *all* `HttpClient` instances, use the following code:

```csharp
builder.Services.ConfigureHttpClientDefaults(clientBuilder => clientBuilder.AddServiceDiscovery());
```

## Using HttpClient

Another way to use service discovery is to use the Steeltoe `DiscoveryHttpClientHandler` with `HttpClient`.

The variant of `OrderService` below creates a new `HttpClient` from the injected handler:

```csharp
public sealed class OrderService(DiscoveryHttpClientHandler handler)
{
    public async Task<OrderModel?> GetOrderByIdAsync(
        string orderId, CancellationToken cancellationToken)
    {
        var httpClient = new HttpClient(handler, disposeHandler: false);
        return await httpClient.GetFromJsonAsync<OrderModel?>(
            $"https://ordering-api/orders/{orderId}", cancellationToken);
    }
}
```

To register the handler, add the following code to `Program.cs`:

```csharp
builder.Services.AddSingleton<ServiceInstancesResolver>();
builder.Services.AddSingleton<ILoadBalancer, RandomLoadBalancer>();
builder.Services.AddSingleton<DiscoveryHttpClientHandler>();
builder.Services.AddSingleton<FortuneService>();
```

## Using IDiscoveryClient directly

In the event the provided HTTP support does not serve your needs, you can always make lookups directly against
the registered collection of `IDiscoveryClient`s, for example:

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEurekaDiscoveryClient();

var app = builder.Build();

var clients = app.Services.GetRequiredService<IEnumerable<IDiscoveryClient>>();
var instance = await ResolveAsync(clients);
if (instance != null)
{
    Console.WriteLine($"Resolved '{instance.ServiceId}' to {instance.Host}:{instance.Port}");
}

static async Task<IServiceInstance?> ResolveAsync(IEnumerable<IDiscoveryClient> clients)
{
    foreach (var client in clients)
    {
        var instances = await client.GetInstancesAsync("ordering-api", default);
        if (instances.Count > 0)
        {
            int randomIndex = Random.Shared.Next(0, instances.Count);
            return instances[randomIndex];
        }
    }
    return null;
}
```

## Load balancing

Service discovery relies on a load balancer to choose one from the available service instances.

The built-in load balancers use `ServiceInstancesResolver` to find the matching service instances from the
registered discovery clients. This resolver optionally supports caching them using `IDistributedCache`,
which is useful for discovery clients that do not provide their own caching (such as the Consul client).

To activate caching, use the code below:

```csharp
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSingleton(new DistributedCacheEntryOptions
{
    SlidingExpiration = TimeSpan.FromMinutes(5)
});
```

> [!NOTE]
> The built-in load balancers do not track statistics or exceptions.

### Random load balancer

The `RandomLoadBalancer`, as the name implies, randomly selects a service instance from all instances
that are resolved for a given friendly name.

### Round-robin load balancer

The provided `RoundRobinLoadBalancer` selects service instances in sequential order, as they are provided
by discovery clients for the given friendly name.

To use this load balancer in service discovery, pass it to the `AddServiceDiscovery()` method:

```csharp
builder.Services.AddHttpClient<OrderService>().AddServiceDiscovery<RoundRobinLoadBalancer>();
```

> [!TIP]
> When caching is activated (see above), this load balancer stores the last-used instance index in the cache.
> Combining it with a shared Redis cache ensures an even load distribution.

### Custom load balancer

If the provided load balancer implementations do not suit your needs, you can create your own implementation of `ILoadBalancer`.

The following example shows a load balancer that always returns the first service instance:

```csharp
public sealed class ChooseFirstLoadBalancer(ServiceInstancesResolver resolver) : ILoadBalancer
{
    public async Task<Uri> ResolveServiceInstanceAsync(Uri requestUri,
        CancellationToken cancellationToken)
    {
        var instances = await resolver.ResolveInstancesAsync(requestUri.Host, cancellationToken);
        return instances.Count > 0 ? new Uri(instances[0].Uri, requestUri.PathAndQuery) : requestUri;
    }

    public Task UpdateStatisticsAsync(Uri requestUri, Uri serviceInstanceUri, TimeSpan? responseTime,
        Exception? exception,
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
```

A custom load balancer needs to be added to the service container manually, because Steeltoe can't know its lifetime.
Add the following code to `Program.cs` to activate the custom load balancer defined above:

```csharp
builder.Services.AddSingleton<ServiceInstancesResolver>();
builder.Services.AddSingleton<ChooseFirstLoadBalancer>();

builder.Services.AddHttpClient<OrderService>().AddServiceDiscovery<ChooseFirstLoadBalancer>();
```
