# Netflix Eureka

The Eureka client implementation lets applications register services with a Eureka server and discover services registered by other applications.

In addition to the Quick Start below, the following Steeltoe sample applications may help you to understand how to use this client:

* [MusicStore](https://github.com/SteeltoeOSS/Samples/tree/3.x/MusicStore): A sample application showing how to use all of the Steeltoe components together in a ASP.NET Core application. This is a microservices-based application built from the ASP.NET Core MusicStore reference app provided by Microsoft.
* [FreddysBBQ](https://github.com/SteeltoeOSS/Samples/tree/3.x/FreddysBBQ): A polyglot microservices-based sample application showing interoperability between Java and .NET on Cloud Foundry. It is secured with OAuth2 Security Services and uses Spring Cloud Services.

## Eureka Settings

To get the Steeltoe Discovery client to properly communicate with the Eureka server, you need to provide a few configuration settings to the client.

What you provide depends on whether you want your application to register a service and whether it also needs to discover services with which to communicate.

General settings that control the behavior of the client are found under the prefix with a key of `Eureka:Client`. Settings that affect registering services are found under the `Eureka:Instance` prefix.

The following table describes the settings that control the overall behavior of the client.

All of these settings should start with `Eureka:Client:`

| Key | Description | Default |
| --- | --- | --- |
| `ShouldRegisterWithEureka` | Enable or disable registering as a service. | `true` |
| `ShouldFetchRegistry` | Enable or disable discovering services. | `true` |
| `ShouldGZipContent` | Enable or disable GZip usage between the client and the Eureka server. | `true` |
| `ServiceUrl` | Comma-delimited list of Eureka server endpoints. | `http://localhost:8761/eureka` |
| `ValidateCertificates` | Enable certificate validation. | `true` |
| `RegistryFetchIntervalSeconds` | Service fetch interval. | 30s |
| `ShouldFilterOnlyUpInstances` | Whether to fetch only UP instances. | `true` |
| `InstanceInfoReplicationIntervalSeconds` | How often to replicate instance changes. | 40s |
| `ShouldDisableDelta` | Whether to disable fetching of delta and, instead, get the full registry. | `false` |
| `RegistryRefreshSingleVipAddress` | Whether to be interested in only the registry information for a single VIP. | none |
| `ShouldOnDemandUpdateStatusChange` | Whether status updates are trigger on-demand register/update. | `true` |
| `AccessTokenUri` | URI to use to obtain OAuth2 access token. | none |
| `ClientSecret` | Secret to use to obtain OAuth2 access token. | none |
| `ClientId` | Client ID to use to obtain OAuth2 access token. | none |
| `CacheTTL` | Time in seconds local cache entries are valid | 15 |
| `EurekaServer:ProxyHost` | Proxy host to Eureka Server. | none |
| `EurekaServer:ProxyPort` | Proxy port to Eureka Server. | none |
| `EurekaServer:ProxyUserName` | Proxy user name to Eureka Server. | none |
| `EurekaServer:ProxyPassword` | Proxy password to Eureka Server. | none |
| `EurekaServer:ShouldGZipContent` | Whether to compress content. | `true` |
| `EurekaServer:ConnectTimeoutSeconds` | Connection timeout. | 5s |
| `EurekaServer:RetryCount` | Number of times to retry Eureka Server requests. | 3 |
| `Health:Enabled` | Enable or disable management health contributor. | `true` |
| `Health:CheckEnabled` | Enable or disable Eureka health check handler. | `true` |
| `Health:MonitoredApps` | List of applications the management health contributor monitors. | All apps in registry |

**NOTE**: **Some settings affect registering as a service as well.**

The following table describes the settings you can use to configure the behavior of the client as it relates to registering services:

| Key | Description | Default |
| --- | --- | --- |
| `AppName` | Name of the application to be registered with Eureka. | `Spring:Application:Name` or `unknown` |
| `Port` | Port on which the instance is registered. | 80 |
| `HostName` | Address on which the instance is registered. | computed |
| `InstanceId` | Unique ID (within the scope of the `AppName`) of the instance registered with Eureka. | computed |
| `AppGroupName` | Name of the application group to be registered with Eureka. | none |
| `InstanceEnabledOnInit` | Whether the instance should take traffic as soon as it is registered. | `false` |
| `SecurePort` | Secure port on which the instance should receive traffic. | 443 |
| `NonSecurePortEnabled` | Non-secure port enabled. | `true` |
| `SecurePortEnabled` | Secure port enabled. | `false` |
| `LeaseRenewalIntervalInSeconds` | How often client needs to send heartbeats. | 30s |
| `LeaseExpirationDurationInSeconds` | Time the Eureka server waits before removing instance. | 90s |
| `VipAddress` | Virtual host name. | `{HostName}:{Port}` |
| `SecureVipAddress` | Secure virtual host name. | `{HostName}:{SecurePort}`|
| `MetadataMap` | Name/value pairs associated with the instance. | none |
| `StatusPageUrlPath` | Relative status page path for this instance. | `/Status` |
| `StatusPageUrl` | Absolute status page for this instance. | computed |
| `HomePageUrlPath` | | `/` |
| `HomePageUrl` | Absolute home page for this instance. | computed |
| `HealthCheckUrlPath` | | `/healthcheck` |
| `HealthCheckUrl` | Absolute health check page for this instance. | computed |
| `SecureHealthCheckUrl` | Secured absolute health check page for this instance. | computed |
| `IpAddress` | IP address to register. | computed |
| `PreferIpAddress` | Whether to register by using IpAddress instead of hostname. | `false` |
| `RegistrationMethod` | How to register service on Cloud Foundry. Can be `route`, `direct`, or `hostname`. | `route` |

All of the settings in the preceding table should start with `Eureka:Instance:`.

> As of Steeltoe 3.1.1, `HealthCheckUrlPath` and `StatusPageUrlPath` will be automatically configured to be the `Health` and `Info` actuator paths (respectively) if those actuators are configured.

You should register by using the `direct` setting mentioned earlier when you want to use container-to-container networking on Cloud Foundry. You should use the `Hostname` setting on Cloud Foundry when you want the registration to use whatever value is configured or computed as `Eureka:Instance:HostName`.

For a complete understanding of the effects of many of these settings, we recommend that you review the documentation on the [Netflix Eureka Wiki](https://github.com/Netflix/eureka/wiki). In most cases, unless you are confident that you understand the effects of changing the values from their defaults, we recommend that you use the defaults.

## Bind to Cloud Foundry

When you want to use a Eureka Server on Cloud Foundry and you have installed [Spring Cloud Services](https://docs.pivotal.io/spring-cloud-services/), you can create and bind an instance of the server to the application by using the Cloud Foundry CLI:

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

>As of Steeltoe 3.0.0, an additional NuGet reference for `Steeltoe.Connector.CloudFoundry` is required to read in service bindings. Just adding the reference will be enough for service bindings to be discoverable.

## Configuring Health Contributors

The Eureka package provides two different Steeltoe Management Health contributors that you can use to monitor Eureka server health.

The first one, `EurekaServerHealthContributor`, is used to determine and report the health of the connection to the Eureka servers. It looks at the status of last good registry fetch or the last heartbeat attempt and uses that information to compute the health of the connection.

If you use the `AddDiscoveryClient()` extension method and you have configured Eureka as your service discovery choice, this contributor is automatically added to the container and is automatically used.

By default, the contributor is enabled but can be disabled by setting `Eureka:Client:Health:Enabled=false`.

The second contributor that you can enable is the `EurekaApplicationsHealthContributor`. By default, this contributor is not enabled, so you must add it to the service container yourself:

```csharp
    services.AddSingleton<IHealthContributor, EurekaApplicationsHealthContributor>();
```

You can use the `EurekaApplicationsHealthContributor` to report the health of a configurable list of registered services based on the status of the service in the registry. For each service it is configured to monitor, it looks at all of the instances of that service and, if all of the instances are marked `DOWN`, the service is reported as being in bad health.  You can configure the services that it monitors by using the `Eureka:Client:Health:MonitoredApps` configuration setting.  Typically you would set this to the list of external service names the application depends on and that, were they unavailable, would impact the operation of the app.

## Configuring Health Checks

By default, Eureka uses the client heartbeat to determine if a client is up. Unless specified otherwise, the Eureka client does not propagate the current health status of the application, as calculated by the health contributors configured for the application. Consequently, after successful registration, Eureka always announces that the application is in 'UP' state. You can alter this behavior by enabling Eureka health checks, which results in propagating application status to Eureka. As a consequence, every other application does not send traffic to applications in states other then 'UP'.

To enable this behavior, you need to add the `IHealthCheckHandler` to your service container (the handler is not added to the container by default):

```csharp
    services.AddSingleton<IHealthCheckHandler, ScopedEurekaHealthCheckHandler>();
```

You can enable or disable the handler by using the `Eureka:Client:Health:CheckEnabled` configuration settings.  It is enabled by default.

If you require more control over the health check, consider implementing your own `IHealthCheckHandler`.

## Configuring Multiple Service Urls

You can specify a comma-delimited list of Eureka server URLs that the client uses when registering or fetching the service registry. Those servers should be part of a properly configured Eureka server cluster and should be using peer-to-peer communications to keep in sync.

The Eureka client automatically fails over to the other nodes in the cluster. When a failed Eureka server node comes back up, the Eureka client automatically reconnects back to the server at some point.

## Configuring Metadata

It is worth spending a bit of time understanding how the Eureka metadata works so that you can use it in a way that makes sense in your application.

Standard metadata information (such as hostname, IP address, port numbers, status page, and health check endpoint) is associated with every service registration. These are published in the service registry and are used by clients to contact the services in a straightforward way.

You can add additional metadata to instance registrations by using the configuration setting `Eureka:Instance:MetadataMap`. The metadata you supply by using this configuration is added to the service registration and becomes accessible in remote clients.

In general, additional metadata does not change the behavior of the client, unless the client is made aware of the meaning of the metadata.

## Configuring Mutual TLS

In cases where customizations to communications with the Eureka Server are needed (for example: when using mutual TLS authentication), `IHttpClientHandlerProvider` is available. In order to simplify mTLS setup in applications, `Steeltoe.Common.Http.ClientCertificateHttpHandlerProvider` is automatically injected when `IHttpClientHandlerProvider` is not detected and `IOptions<CertificateOptions>` is available.

```csharp
// Add an ICertificateSource to your configuration builder
var configurationBuilder = new ConfigurationBuilder()
        .AddPemFiles("instance.crt", "instance.key");
        /* OR */
        .AddCertificateFile("instance.p12");
        /* OR */
        config.Add(<YourCustomICertificateSourceHere>)

var services = new ServiceCollection();
// Add Options and configure CertificateOptions
services.AddOptions();

// generally configure CertificateOptions with certificate path information
services.Configure<CertificateOptions>(config);

// actually load the certificate into CertificateOptions
services.AddSingleton<IConfigureOptions<CertificateOptions>, ConfigureCertificateOptions>();
// or
services.AddSingleton<IConfigureOptions<CertificateOptions>, PemConfigureCertificateOptions>();
// or
services.AddSingleton<IConfigureOptions<CertificateOptions>, {ConfigureYourCertificateOptions}>();

services.AddDiscoveryClient(config);
```

If you wish to supply your own `IHttpClientHandlerProvider`, add it into the service collection before calling `AddDiscoveryClient`:

```csharp
services.AddSingleton<IHttpClientHandlerProvider>(new MyCustomHttpClientHandlerProvider());
services.AddDiscoveryClient(config);
```

>You can use a single `ICertificateSource` for both Eureka and [Config Server mTLS connections](../configuration/config-server-provider.md#configuring-mutual-tls).
