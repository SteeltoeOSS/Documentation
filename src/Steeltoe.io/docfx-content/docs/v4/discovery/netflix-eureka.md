# Netflix Eureka discovery client

The Steeltoe Eureka discovery client lets an application register/unregister itself with a Eureka server
and provides querying for service instances registered by other applications.

Once activated, the client begins to operate in the background, both registering and renewing service registrations,
sending periodic heartbeats to the Eureka server, and also periodically fetching the service registry from the server.

## Usage

To use this discovery client, add a NuGet package reference to `Steeltoe.Discovery.Eureka` and initialize it from your `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Steeltoe: Add service discovery client for Eureka.
builder.Services.AddEurekaDiscoveryClient();

var app = builder.Build();
```

## Configuration settings

To get the Steeltoe discovery client to properly communicate with the Eureka server, you need to provide
a few configuration settings. There are several sections you may need to configure.
What you provide depends on whether you want your application to register the running app and/or
whether it needs to query for other apps.

For a complete understanding of the effects of many of these settings, we recommend that you review the documentation on the
[Netflix Eureka Wiki](https://github.com/Netflix/eureka/wiki).
In most cases, unless you are confident that you understand the effects of changing the values from their defaults,
we recommend that you use the defaults.

> [!TIP]
> Since Steeltoe v4, most of these settings can be changed at runtime, which updates the Eureka server accordingly.

### General

The following table describes the configuration settings that control the overall behavior of the client.
All of these settings should start with `Eureka:Client:`.

| Key | Description | Default |
| --- | --- | --- |
| `Enabled` | Whether to enable the Eureka client. | `true` |
| `ServiceUrl` | Comma-delimited list of Eureka server endpoints. | `http://localhost:8761/eureka/` |
| `AccessTokenUri` | URL to obtain OAuth2 access token from, before connecting to the Eureka server. | |
| `ClientId` | Client ID for obtaining an access token. | |
| `ClientSecret` | Secret for obtaining an access token. | |
| `Validate_Certificates` | Whether the client validates server certificates. | `true` |
| `EurekaServer:ShouldGZipContent` | Whether to auto-decompress responses from the Eureka server. | `true` |
| `EurekaServer:RetryCount` | Number of times to retry Eureka server requests. | `2` |
| `EurekaServer:ConnectTimeoutSeconds` | How long to wait (in seconds) before a connection to the Eureka server times out. | `5` |
| `EurekaServer:ProxyHost` | Proxy hostname used in contacting the Eureka server. | |
| `EurekaServer:ProxyPort` | Proxy port number used in contacting the Eureka server. | |
| `EurekaServer:ProxyUserName` | Proxy username used in contacting the Eureka server. | |
| `EurekaServer:ProxyPassword` | Proxy password used in contacting the Eureka server. | |
| `Health:Enabled` | Whether to activate an `IHealthContributor` that verifies connectivity to the Eureka server. | `true` |

### Registration

The configuration settings below pertain to registering the currently running app as a service instance in Eureka.
All of these settings should start with `Eureka:Client:`.

| Key | Description | Default |
| --- | --- | --- |
| `ShouldRegisterWithEureka` | Whether to register the running app as a service instance. | `true` |
| `Health:CheckEnabled` | Whether to query ASP.NET health checks and `IHealthContributor`s during registration and renewals, in order to determine the status of the running app to report back to Eureka (see section below). | `true` |

Additionally, the table below lists the configuration settings that control *how* to register the instance.
All of these settings should start with `Eureka:Instance:`.

| Key | Description | Default |
| --- | --- | --- |
| `InstanceId` | Unique ID (within the scope of the app name) of the instance to be registered with Eureka. | computed |
| `AppName` | Name of the application to be registered with Eureka. | computed |
| `AppGroup` | Name of the application group to be registered with Eureka. | |
| `MetadataMap` | Name/value pairs associated with the instance. | computed |
| `HostName` | Hostname on which the instance is registered. | computed |
| `IPAddress` | IP address on which the instance is registered. | computed |
| `UseNetworkInterfaces` | Query the operating system for network interfaces to determine `HostName` and `IPAddress`. | `false` |
| `PreferIPAddress` | Whether to register with `IPAddress` instead of `HostName`. | `false` |
| `VipAddress` | Comma-delimited list of VIP addresses for the instance. | computed |
| `SecureVipAddress` | Comma-delimited list of secure VIP addresses for the instance. | computed |
| `Port` | Non-secure port number on which the instance should receive traffic. | computed |
| `NonSecurePortEnabled` | Whether the non-secure port should be enabled. [^1] | computed |
| `SecurePort` | Secure port on which the instance should receive traffic. | computed |
| `SecurePortEnabled` | Whether the secure port should be enabled. [^1] | computed |
| `RegistrationMethod` | How to register on Cloud Foundry. Can be `route`, `direct`, or `hostname`. [^2] | |
| `InstanceEnabledOnInit` | Whether the instance should take traffic as soon as it is registered. [^3] | `true` |
| `LeaseRenewalIntervalInSeconds` | How often (in seconds) the client sends heartbeats to Eureka to indicate that it is still alive. | `30` |
| `LeaseExpirationDurationInSeconds` | Time (in seconds) that the Eureka server waits since it received the last heartbeat before it marks the instance as down. | `90` |
| `StatusPageUrlPath` | Relative path to the status page for the instance. [^4] | `/info` |
| `StatusPageUrl` | Absolute URL to the status page for the instance (overrides `StatusPageUrlPath`). | computed |
| `HomePageUrlPath` | Relative path to the home page URL for the instance. | `/` |
| `HomePageUrl` | Absolute URL to the home page for the instance (overrides `HomePageUrlPath`). | computed |
| `HealthCheckUrlPath` | Relative path to the health check endpoint of the instance. [^4] | `/health` |
| `HealthCheckUrl` | Absolute URL for health checks of the instance (overrides `HealthCheckUrlPath`). | computed |
| `SecureHealthCheckUrl` | Secure absolute URL for health checks of the instance (overrides `HealthCheckUrlPath`). | computed |
| `AsgName` | AWS auto-scaling group name associated with the instance. | |
| `DataCenterInfo` | Data center the instance is deployed to (`Netflix`, `Amazon` or `MyOwn`). | MyOwn |

[^1]: When both non-secure and secure ports are enabled, the secure port is preferred during service discovery.
[^2]: Specify `direct` to use container-to-container networking on Cloud Foundry. Specify `hostname` to force using `HostName`.
[^3]: When set to `false`, call `EurekaApplicationInfoManager.UpdateInstance()` after initialization to mark the instance as up.
[^4]: Add a NuGet package reference to `Steeltoe.Management.Endpoint` to use its `health` and `info` actuator paths.

The values for `Port` and `SecurePort`, and whether they are enabled, are automatically determined from the ASP.NET address bindings. [^1]
See [8 ways to set the URLs for an ASP.NET Core app](https://andrewlock.net/8-ways-to-set-the-urls-for-an-aspnetcore-app/)
for how to influence them using environment variables.

It is also possible to use dynamic port bindings (by setting the port number to `0` in ASP.NET).
In that case, Steeltoe adds a random number (outside the valid port range) to the `InstanceId` to make it unique.
Once the app has fully started, the assigned port numbers are updated in Eureka, but the `InstanceId` does not change.

### Querying

The configuration settings that pertain to querying the Eureka registry for apps (used by the load balancers during service discovery) are listed below.
All of these settings should start with `Eureka:Client:`.

| Key | Description | Default |
| --- | --- | --- |
| `ShouldFetchRegistry` | Whether to periodically fetch registry information from the Eureka server. | `true` |
| `RegistryFetchIntervalSeconds` | How often (in seconds) to fetch registry information from the Eureka server. | `30` |
| `ShouldFilterOnlyUpInstances` | Whether to include only instances with UP status after fetching the list of applications. | `true` |
| `ShouldDisableDelta` | Whether to fetch the full registry each time or fetch only deltas. | `false` |
| `RegistryRefreshSingleVipAddress` | Whether to only fetch registry information for the specified VIP address. | `false` |
| `Health:MonitoredApps` | Comma-delimited list of applications in Eureka this app depends on (see section below). | |

## Configuring health contributors

The Steeltoe Eureka package provides two different health contributors that you can use to monitor Eureka server health.

The first one, `EurekaServerHealthContributor`, is used to determine and report the health of the connection to the Eureka
servers. It looks at the status of the last good registry fetch and the last heartbeat attempt and uses that information to
compute the health of the connection.
This contributor is automatically activated, but can be turned off by setting `Eureka:Client:Health:Enabled` to `false`.

The second contributor that you can enable is the `EurekaApplicationsHealthContributor`.
By default, this contributor is not enabled, so you must add it to the service container yourself:

```csharp
builder.Services.AddSingleton<IHealthContributor, EurekaApplicationsHealthContributor>();
```

You can use the `EurekaApplicationsHealthContributor` to report the health of a configurable list of registered services
based on their status in the registry. For each service it is configured to monitor, it looks at all of the instances
of that service and, if all of the instances are marked `DOWN`, your app is reported as being in bad health.
You can configure the services that it monitors by using the `Eureka:Client:Health:MonitoredApps` configuration setting.
Typically you would set this to the list of external service names your app depends on and that, were they unavailable,
would impact the operation of your app. If this setting is left empty, *all* apps in Eureka are monitored.

## Configuring health checks

If `Eureka:Client:ShouldRegisterWithEureka` is set to `true` (the default), the Eureka discovery client sends
periodic heartbeats to inform the Eureka server that the currently running app is reachable.

Unless specified otherwise, the client does not propagate the current health status of the application,
as calculated from the ASP.NET health checks and active health contributors, to Eureka.
Consequently, after successful registration, Eureka always announces that the application is in 'UP' state.
You can alter this behavior by enabling `Eureka:Client:Health:CheckEnabled` (`false` by default),
which results in propagating health status to Eureka.
As a consequence, other applications won't send traffic to your app unless the health checks and contributors report 'UP'.

If you require more control over the health checks, consider implementing your own `IHealthCheckHandler`.

## Configuring multiple Eureka servers

You can specify a comma-delimited list of Eureka server URLs that the client uses when registering or fetching
the service registry. Those servers should be part of a properly configured Eureka server cluster and should be using
peer-to-peer communications to keep in sync.

The Eureka client automatically fails over to the other nodes in the cluster. When a failed Eureka server node comes
back up, the Eureka client automatically reconnects back to the server at some point.

## Using metadata

It is worth spending a bit of time understanding how the Eureka metadata works so that you can use it in a way
that makes sense in your application.

Standard instance information (such as hostname, IP address, port numbers, status page, and health check endpoint)
is associated with every service registration. These are published in the service registry and are used by clients
to contact the services in a straightforward way.

You can add additional metadata to instance registrations by using the configuration setting `Eureka:Instance:MetadataMap`.
The key/value pairs you supply there are added to the service registration and become accessible to remote clients.

When the metadata varies over time, depending on contextual information, it can be updated from code as well:
```csharp
var appManager = app.Services.GetRequiredService<EurekaApplicationInfoManager>();
appManager.UpdateInstance(newStatus: null, newOverriddenStatus: null,
    newMetadata: new Dictionary<string, string?>(appManager.Instance.Metadata)
    {
        ["someNewKey"] = "someValue1",
        ["someExistingKey"] = "someValue2"
    });
```

> [!WARN]
> Once metadata has been updated from code, later metadata changes in configuration are ignored.

In general, additional metadata does not change the behavior of applications, unless they are made aware of
the meaning of the metadata.

## Configuring mutual TLS

To use mutual TLS authentication in the communication with the Eureka server,
add the path to your certificate file(s) to `appsettings.json`. For example:

```json
"Certificates": {
  "Eureka": {
    "CertificateFilePath": "example.p12"
  }
}
```
or:
```json
"Certificates": {
  "Eureka": {
    "CertificateFilePath": "example.crt",
    "PrivateKeyFilePath": "example.key"
  }
}
```

> [!NOTE]
> To support certificate rotation, the configuration keys and the files on disk are automatically monitored for changes.

> [!TIP]
> A single certificate can be shared with both Config Server and Eureka, by using the key "Certificates" instead of "Certificates:Eureka".

### Using custom HTTP headers

The communication with Eureka server uses the `HttpClientFactory` [pattern](https://learn.microsoft.com/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests),
which makes it aware of DNS changes over time and enables to tweak the handler pipeline.

To send a custom HTTP header with every request, create a `DelegatingHandler` and add it to the pipeline:
```csharp
builder.Services.AddEurekaDiscoveryClient();
builder.Services.AddTransient<ExtraRequestHeaderDelegatingHandler>();

builder.Services.Configure<HttpClientFactoryOptions>("Eureka", options =>
{
    options.HttpMessageHandlerBuilderActions.Add(handlerBuilder =>
        handlerBuilder.AdditionalHandlers.Add(
            handlerBuilder.Services.GetRequiredService<ExtraRequestHeadersDelegatingHandler>()));
});

public sealed class ExtraRequestHeaderDelegatingHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Add("X-Example", "ExampleValue");
        return base.SendAsync(request, cancellationToken);
    }
}
```

> [!NOTE]
> To send an extra header to the OAuth2 endpoint, replace `"Eureka"` with `"AccessTokenForEureka"` in the example above.
