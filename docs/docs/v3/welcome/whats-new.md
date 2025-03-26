# What's New in Steeltoe 3

## New Features and Changes

### Steeltoe 3.2.0

* Support for .NET 6
  * WebApplicationBuilder extensions
  * ConfigurationManager support
* Support polling Spring Cloud Config Server for updates
* Management
  * Update Thread and Heap Dump implementations
  * Depend on a GA release of OpenTelemetry Metrics
  * Support exporting metrics and traces to Wavefront

### Steeltoe 3.1.0

* Steeltoe Messaging, with support for [RabbitMQ](../messaging/rabbitmq-intro.md)
* [Steeltoe Stream](../stream/index.md), with support for [Spring Cloud Data Flow](../stream/data-flow-stream.md) and [RabbitMQ](../stream/rabbit-binder.md)
* Steeltoe components can now be automatically configured for your application with a single line of code with [Steeltoe.Bootstrap.Autoconfig](../bootstrap/index.md)

### Steeltoe 3.0.0

* We made the Steeltoe libraries more platform agnostic to allow for better extensibility into other platforms, starting by separating the abstractions for our core components into separate packages. This will allow future extensibility for our libraries and grow the community into other areas.
* We added and renamed many of the packages to support our new features and to provide a consistent package naming.
* We redirected our focus to support only .NET Core. This decision was based on the direction the Microsoft .NET team is taking the project and to support our users moving forward. We will still be supporting .NET Framework in our 2.x release line.
* We have added some great new features and here are some of the highlights:
  * Automatic wiring and configuration of Messaging APIs with RabbitMQ
  * Kubernetes support for configuration (ConfigMap and Secrets) and service discovery using Kubernetes .NET Client.
  * Added Health Groups for `readiness` and `liveness` endpoints which are grouped under the `/health` endpoint
  * Metrics now uses `EventSource` and `EventCounter`, along with a new prometheus exporter that now uses OpenTelemetry metrics packages
  * Distributed tracing library now has new exporters and updated internal libraries from OpenCensus to OpenTelemetry
  * Pluggable architecture for Service Discovery (Consul, Eureka, and Kubernetes)
  * Pluggable architecture for Connectors
  * New Connector for CosmosDB
  * The `/heapdump` actuator endpoint now supports heap dumps on Linux
  * Circuit Breaker using Hystrix now using the Prometheus endpoint for easier consumption of events on Prometheus supported services
  * Added mTLS support and service to service authentication using rotating certificates

## Package Name Changes

 | Steeltoe 2.x | Steeltoe 3.x |
 | ------------ | ------------ |
 | N/A | Steeltoe.CircuitBreaker.Abstractions |
 | Steeltoe.CircuitBreaker.Hystrix.MetricsEventsCore | Same |
 | Steeltoe.CircuitBreaker.Hystrix.MetricsStreamAutofac | N/A |
 | Steeltoe.CircuitBreaker.Hystrix.MetricsStreamCore | Same |
 | Steeltoe.CircuitBreaker.HystrixAutofac | N/A |
 | Steeltoe.CircuitBreaker.HystrixBase  | Same |
 | Steeltoe.CircuitBreaker.HystrixCore  | Same |
 | N/A | Steeltoe.Connector.Abstractions |
 | Steeltoe.CloudFoundry.ConnectorAutofac  | N/A |
 | Steeltoe.CloudFoundry.ConnectorBase  | Steeltoe.Connector.ConnectorBase |
 | Steeltoe.CloudFoundry.ConnectorCore  | Steeltoe.Connector.ConnectorCore |
 | Steeltoe.CloudFoundry.Connector.EF6Autofac | N/A |
 | Steeltoe.CloudFoundry.Connector.EF6Core | Steeltoe.Connector.EF6Core |
 | Steeltoe.CloudFoundry.Connector.EFCore | Steeltoe.Connector.EFCore |
 | N/A | Steeltoe.Connector.CloudFoundry |
 | N/A | Steeltoe.Common.Abstractions |
 | Steeltoe.Common  | Same |
 | Steeltoe.Common.Autofac  | N/A |
 | Steeltoe.Common.Hosting  | Same |
 | Steeltoe.Common.Http  | Same |
 | N/A | Steeltoe.Common.Kubernetes |
 | Steeltoe.Common.Net  | Same |
 | N/A | Steeltoe.Common.Retry |
 | Steeltoe.Common.Security  | Same |
 | N/A | Steeltoe.Discovery.Abstractions
 | Steeltoe.Discovery.ClientAutofac | N/A |
 | N/A | Steeltoe.Discovery.ClientBase |
 | Steeltoe.Discovery.ClientCore  | Same |
 | Steeltoe.Discovery.ConsulBase  | Steeltoe.Discovery.Consul |
 | Steeltoe.Discovery.EurekaBase  | Steeltoe.Discovery.Eureka |
 | N/A | Steeltoe.Discovery.Kubernetes |
 | N/A | Steeltoe.Extensions.Configuration.Abstractions |
 | Steeltoe.Extensions.Configuration.CloudFoundryAutofac | N/A |
 | Steeltoe.Extensions.Configuration.CloudFoundryBase  | Same |
 | Steeltoe.Extensions.Configuration.CloudFoundryCore  | Same |
 | Steeltoe.Extensions.Configuration.ConfigServerAutofac  | N/A |
 | Steeltoe.Extensions.Configuration.ConfigServerBase  | Same |
 | Steeltoe.Extensions.Configuration.ConfigServerCore  | Same |
 | N/A | Steeltoe.Extensions.Configuration.KubernetesBase |
 | N/A | Steeltoe.Extensions.Configuration.KubernetesCore |
 | Steeltoe.Extensions.Configuration.PlaceholderBase  | Same |
 | Steeltoe.Extensions.Configuration.PlaceholderCore  | Same |
 | Steeltoe.Extensions.Configuration.RandomValueBase  | Same |
 | N/A | Steeltoe.Extensions.Logging.Abstractions |
 | Steeltoe.Extensions.Logging.DynamicLogger | Same |
 | Steeltoe.Extensions.Logging.SerilogDynamicLogger  |Steeltoe.Extensions.Logging.DynamicSerilogBase |
 | Steeltoe.Extensions.Logging.SerilogDynamicLogger | Steeltoe.Extensions.Logging.DynamicSerilogCore |
 | N/A | Steeltoe.Integration.Abstractions * |
 | N/A | Steeltoe.Integration.IntegrationBase * |
 | N/A | Steeltoe.Management.Abstractions |
 | Steeltoe.Management.CloudFoundryCore  | Same |
 | Steeltoe.Management.Diagnostics  | Same |
 | Steeltoe.Management.EndpointBase | Same |
 | Steeltoe.Management.EndpointCore | Same |
 | Steeltoe.Management.EndpointOwin | N/A |
 | Steeltoe.Management.EndpointOwinAutofac  | N/A |
 | Steeltoe.Management.EndpointWeb  | N/A |
 | Steeltoe.Management.ExporterBase  | N/A |
 | Steeltoe.Management.ExporterCore  | N/A |
 | N/A | Steeltoe.Management.KubernetesCore |
 | Steeltoe.Management.OpenCensus  | N/A|
 | Steeltoe.Management.OpenCensus.Abstractions  | N/A |
 | Steeltoe.Management.OpenCensus.ZipkinExporter  | N/A |
 | Steeltoe.Management.OpenCensusBase  | N/A |
 | N/A | Steeltoe.Management.OpenTelemetryBase |
 | Steeltoe.Management.TaskCore  | Same |
 | Steeltoe.Management.TracingBase  | Same |
 | Steeltoe.Management.TracingCore  | Same |
 | N/A | Steeltoe.Messaging.Abstractions|
 | N/A | Steeltoe.Messaging.MessagingBase|
 | N/A | Steeltoe.Messaging.RabbitMQ|
 | Steeltoe.Security.Authentication.CloudFoundryBase  | Same |
 | Steeltoe.Security.Authentication.CloudFoundryCore  | Same |
 | Steeltoe.Security.Authentication.CloudFoundryOwin  | N/A |
 | Steeltoe.Security.Authentication.CloudFoundryWcf  | N/A |
 | N/A | Steeltoe.Security.Authentication.MtlsCore |
 | Steeltoe.Security.DataProtection.CredHubBase  | Same |
 | Steeltoe.Security.DataProtection.CredHubCore  | Same |
 | Steeltoe.Security.DataProtection.RedisCore  | Same |
 | N/A | Steeltoe.Stream.Abstractions * |
 | N/A | Steeltoe.Stream.StreamBase * |

  \* Experimental packages

>Packages that existed exclusively for supporting .NET Framework applications are not included in Steeltoe 3.0. Any package listed above with `N/A` for the name in 3.0 will continue to exist, but only in the 2.x line of Steeltoe. Interoperability between Steeltoe 2.x and 3.x is not supported.

## HostBuilder Extensions

In 3.0, more of Steeltoe can be added in single-line statements with `HostBuilder` extensions than ever before so you can be on your way even faster:

```csharp
HostBuilder.CreateDefaultBuilder()
  .AddCloudHosting()
  .AddConfigServer()
  .AddDynamicSerilog()
  .AddServiceDiscovery()
  .AddAllActuators()
```

These extensions generally depend on the same underlying code, so if you'd rather do this work in `Startup.cs` you absolutely still can, these are convenience methods. These extensions are also typically available for `WebHostBuilder` as well. Look for more information on these extensions in the relevant component area.

## Breaking Changes

In addition to the package name changes, there have also been changes to package architectures, public APIs and namespaces in Steeltoe that may necessitate changes in your applications.

### Name Changes

Minor namespace changes have been made throughout Steeltoe for a more streamlined experience. While the suffixes of `Base` and `Core` continue to be useful in package naming, they have been removed from the code so that it's easier to work between related areas within Steeltoe.

More specific examples of name changes include:

* Serilog-related namespaces have been adjusted to match the new package name
* `(Web)HostBuilder.AddCloudFoundry()` is now `(Web)HostBuilder.AddCloudFoundryConfiguration()`

### Service Connectors

In the migration to the new pluggable architecture of Connectors, functionality specific to Cloud Foundry has moved to the new package `Steeltoe.Connector.CloudFoundry`. No code changes are required, but you will need to add this new package reference if you are deploying your application to a platform based on Cloud Foundry.

As a result of this change, a dependency on the Cloud Foundry Configuration package is only required when using the Cloud Foundry Connector package.

### Service Discovery

In previous versions of Steeltoe, all discovery clients needed to be either used directly or via `Steeltoe.Discovery.ClientAutofac` or `Steeltoe.Discovery.ClientCore` packages, where those packages managed connecting the pieces of the discovery client implementations to the Steeltoe abstractions.
This created a situation where any discovery client needed to be directly referenced by a centralized package, such as `Steeltoe.Discovery.ClientCore`, which limited extensibility options.

Steeltoe Service Discovery has been rearchitected so that the core library no longer needs a direct reference to any discovery client implementation.
The base of Steeltoe service discovery is now available in the appropriately named package `Steeltoe.Discovery.ClientBase`, and discovery client implementations are configured as extensions to this package.
It is also still possible to directly reference the discovery client implementations

>A direct reference to any/all discovery client implementations your application may use is now required. Read more in the [Service Discovery documentation](../discovery/initialize-discovery-client.md)

### Cloud Foundry

For applications running on Cloud Foundry, please be aware of the changes to Connectors and Discovery outlined above.

Additionally, please note that `UseCloudFoundryHosting` has been removed, in favor of the new `UseCloudHosting`, which is found in the package `Steeltoe.Common.Hosting`.

### Management

Since [OpenCensus](https://opencensus.io/) has merged with [OpenTracing](https://opentracing.io/) to form [OpenTelemetry](https://opentelemetry.io/), Steeltoe is no longer shipping any OpenCensus-related code in 3.0 and has migrated to OpenTelemetry.

## Release Notes

Release notes for all releases can be found on the [Steeltoe releases](https://github.com/SteeltoeOSS/Steeltoe/releases) section on GitHub.
