# What's New in Steeltoe 3

## New Features and Changes

### Steeltoe 4.0.0

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

## Breaking Changes

### Service Connectors

### Service Discovery

### Management

Since [OpenCensus](https://opencensus.io/) has merged with [OpenTracing](https://opentracing.io/) to form [OpenTelemetry](https://opentelemetry.io/), Steeltoe is no longer shipping any OpenCensus-related code in 3.0 and has migrated to OpenTelemetry.

## Release Notes

Release notes for all releases can be found on the [Steeltoe releases](https://github.com/SteeltoeOSS/Steeltoe/releases) section on GitHub.
