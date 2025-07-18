# What's new in Steeltoe 4

## Overview

### Introduction

The Steeltoe project began all the way back in 2016 (before .NET Core 1.0.0 was released) at the request of enterprises experiencing great success with their [Spring Cloud](https://spring.io/cloud) powered applications on [Cloud Foundry](https://www.cloudfoundry.org/).
These organizations were looking for similar outcomes (such as reduced developer toil, easy-to-implement observability, scaling and resiliency) for their .NET applications.
Rather than starting from scratch, Steeltoe took the approach of building clients for Spring Cloud services and porting code from Spring as needed, adapting the codebase to fit with .NET while trying to stay close to the Spring origins.
In order to deliver higher-level Spring project features, Steeltoe libraries grew rapidly with building-block components matching the architecture and conventions in Spring.

All previous versions of Steeltoe had capability and feature expansion as the main goals, with moderate regard for how well Steeltoe "blends in" with the greater .NET ecosystem.
As a result of this weighting of priorities, Steeltoe was built to land somewhere in between Spring and .NET, inconsistently favoring conventions from one camp or the other, sometimes with the weight of additional lower-level abstractions from Spring.
Add to that the breaking changes needed to adapt to updates in the .NET ecosystem since some of the older Steeltoe components were written, and you can see why it was time for a major overhaul.

The introduction of [.NET Aspire](https://learn.microsoft.com/dotnet/aspire/get-started/aspire-overview) and the acquisition of VMware by Broadcom
underscored that this is the time to refocus on Steeltoe's core goals and re-evaluate how the desired outcomes are achieved.

Steeltoe 4 is a major release that brings many improvements and changes to the library.
The goal of this release is to make Steeltoe better integrated in the .NET ecosystem in a more developer-friendly way, compatible
with the latest versions of .NET and third-party libraries/products, and to improve the overall quality of the library.
This document provides an overview of the changes in Steeltoe 4, the impact on existing applications, and serves as the upgrade guide (with a searchable API diff and replacement notes).
Steeltoe 4 requires .NET 8 or higher.

### Quality of Life improvements

- Annotated for [nullable reference types](https://learn.microsoft.com/dotnet/csharp/nullable-references)
- Compatible with the latest versions of ASP.NET and third-party libraries
- Compatible with recent versions of Tanzu Platform (Cloud Foundry and Kubernetes) and Spring Boot
- Changes to align with .NET conventions and patterns, extensive review of the public API surface
- Performance and scalability improvements
- Numerous bug fixes
- Cleanup of logging output
- Substantially improved documentation
- Improved test coverage, including interaction between different Steeltoe components
- All samples updated to .NET 8, fully tested and working
- Automated code style validation (Resharper, StyleCop, Sonar, Microsoft CodeAnalysis)

### General

- NuGet Packages
  - Dropped the Core/Base suffix from package names, which was used to distinguish between .NET Standard and .NET Core
  - Removed ".Extensions" from NuGet package names
- Extension methods
  - Removed host builder extension methods that could be substituted with a single extension method on
    `IServiceCollection`, `IConfiguration`, `IConfigurationBuilder`, `ILoggingBuilder`, etc.
    Their redundancy led to confusion, and required Steeltoe to adapt each time a new host builder is introduced.
  - Added support for the new `IHostApplicationBuilder` (which `WebApplicationBuilder` and `HostApplicationBuilder` implement) to the remaining host builder extension methods
  - Moved extension methods to the appropriate Steeltoe namespaces to avoid clashes with other libraries
- Public API surface
  - Sealed types not designed for inheritance, which improves runtime performance
  - Removed various interfaces that weren't general-purpose, types not designed for inheritance/reuse made internal
  - Changed methods containing optional parameters with default values to overloaded methods
  - Made more methods async and expanded usage of `CancellationToken`, both as a parameter and internally
  - Enhanced method input validation to prevent downstream `NullReferenceException` exceptions
  - Applied C#/.NET naming conventions, for example: renamed `HealthStatus.OUT_OF_SERVICE` to `HealthStatus.OutOfService`
- Configuration
  - Added support in Steeltoe packages for auto-completion in `appsettings.json` (without needing a schema reference, which currently only works in Visual Studio for SDK-style web projects), updated global Steeltoe JSON schema
  - Changed nearly all configuration settings to be reloadable without app restart, now more consistently exposed via ASP.NET Options pattern
- Up-to-date
  - Extensively tested with the latest versions of dependent packages, database drivers and third-party products
    (including Tanzu, Cloud Foundry, Config Server, Consul, Eureka, RabbitMQ, Redis/Valkey, OpenTelemetry, Grafana, Prometheus, Zipkin, Spring Boot Admin)
  - Refreshed dev-local Docker images for Config Server, Eureka, UAA and Spring Boot Admin
  - Steeltoe no longer depends on legacy technologies, such as binary serialization and Newtonsoft.Json

### Removed components

- Everything that interacts with the Kubernetes API directly, because the features were not well-defined,
  test coverage was minimal, and the Steeltoe team believes these packages aren't widely used
- CredHub client, because we have no _known_ use cases since the introduction of the [CredHub Service Broker](https://techdocs.broadcom.com/us/en/vmware-tanzu/platform-services/credhub-service-broker/services/credhub-sb/index.html)
- CircuitBreaker, because [Polly](https://github.com/App-vNext/Polly) provides similar features and is widely used in .NET apps
- Messaging/Integration/Stream, because usage and implementation are too complicated and the adoption rate is very low
- Spring Expression Language (SpEL), because it was added for Stream and doesn't support many C# language features

> [!NOTE]
> The components that have been removed from Steeltoe 4 are not _expected_ to have a significant impact due to low adoption (based on NuGet package downloads).
> If the loss of any of this functionality _is_ a problem for you, please [open an issue](https://github.com/SteeltoeOSS/Steeltoe/issues/new) and tell us more.

### Package name changes

| Steeltoe 3.x | Steeltoe 4.x |
| --- | --- |
| Steeltoe.Bootstrap.Autoconfig | Steeltoe.Bootstrap.AutoConfiguration |
| Steeltoe.CircuitBreaker.* | - |
| Steeltoe.Common.Abstractions | Steeltoe.Common |
| Steeltoe.Common.Expression | - |
| Steeltoe.Common.Hosting | Steeltoe.Common.Logging |
| Steeltoe.Common.Http | Steeltoe.Discovery.HttpClients |
| Steeltoe.Common.Kubernetes | - |
| Steeltoe.Common.Retry | - |
| Steeltoe.Common.Security | Steeltoe.Common.Certificates |
| Steeltoe.Common.Utils | - |
| Steeltoe.Connector.* | Steeltoe.Connectors |
| Steeltoe.Connector.EFCore | Steeltoe.Connectors.EntityFrameworkCore |
| Steeltoe.Connector.EF6Core | - |
| Steeltoe.Discovery.Abstractions | Steeltoe.Common |
| Steeltoe.Discovery.ClientBase | Steeltoe.Discovery.HttpClients |
| Steeltoe.Discovery.ClientCore | Steeltoe.Discovery.HttpClients |
| Steeltoe.Discovery.Kubernetes | - |
| Steeltoe.Extensions.Configuration.Abstractions | Steeltoe.Configuration.Abstractions |
| Steeltoe.Extensions.Configuration.CloudFoundryBase | Steeltoe.Configuration.CloudFoundry |
| Steeltoe.Extensions.Configuration.CloudFoundryCore | Steeltoe.Configuration.CloudFoundry |
| Steeltoe.Extensions.Configuration.ConfigServerBase | Steeltoe.Configuration.ConfigServer |
| Steeltoe.Extensions.Configuration.ConfigServerCore | Steeltoe.Configuration.ConfigServer |
| Steeltoe.Extensions.Configuration.Kubernetes.ServiceBinding | Steeltoe.Configuration.Kubernetes.ServiceBindings |
| Steeltoe.Extensions.Configuration.KubernetesBase | - |
| Steeltoe.Extensions.Configuration.KubernetesCore | - |
| Steeltoe.Extensions.Configuration.PlaceholderBase | Steeltoe.Configuration.Placeholder |
| Steeltoe.Extensions.Configuration.PlaceholderCore | Steeltoe.Configuration.Placeholder |
| Steeltoe.Extensions.Configuration.RandomValueBase | Steeltoe.Configuration.RandomValue |
| Steeltoe.Extensions.Configuration.SpringBootBase | Steeltoe.Configuration.SpringBoot |
| Steeltoe.Extensions.Configuration.SpringBootCore | Steeltoe.Configuration.SpringBoot |
| Steeltoe.Extensions.Logging.Abstractions | Steeltoe.Logging.Abstractions |
| Steeltoe.Extensions.Logging.DynamicLogger | Steeltoe.Logging.DynamicConsole |
| Steeltoe.Extensions.Logging.DynamicSerilogBase | Steeltoe.Logging.DynamicSerilog |
| Steeltoe.Extensions.Logging.DynamicSerilogCore | Steeltoe.Logging.DynamicSerilog |
| Steeltoe.Integration.* | - |
| Steeltoe.Management.CloudFoundryCore | Steeltoe.Management.Endpoint |
| Steeltoe.Management.Diagnostics | Steeltoe.Management.Endpoint |
| Steeltoe.Management.EndpointBase | Steeltoe.Management.Endpoint |
| Steeltoe.Management.EndpointCore | Steeltoe.Management.Endpoint |
| Steeltoe.Management.KubernetesCore | - |
| Steeltoe.Management.OpenTelemetryBase | Steeltoe.Management.Endpoint, Steeltoe.Management.Prometheus |
| Steeltoe.Management.TaskCore | Steeltoe.Management.Tasks |
| Steeltoe.Management.TracingBase | Steeltoe.Management.Tracing |
| Steeltoe.Management.TracingCore | Steeltoe.Management.Tracing |
| Steeltoe.Messaging.* | - |
| Steeltoe.Security.Authentication.CloudFoundryBase | Steeltoe.Security.Authentication.JwtBearer, Steeltoe.Security.Authentication.OpenIdConnect, Steeltoe.Security.Authorization.Certificate |
| Steeltoe.Security.Authentication.CloudFoundryCore | Steeltoe.Security.Authentication.JwtBearer, Steeltoe.Security.Authentication.OpenIdConnect, Steeltoe.Security.Authorization.Certificate |
| Steeltoe.Security.Authentication.MtlsCore | Steeltoe.Security.Authorization.Certificate |
| Steeltoe.Security.DataProtection.CredHubBase | - |
| Steeltoe.Security.DataProtection.CredHubCore | - |
| Steeltoe.Security.DataProtection.RedisCore | Steeltoe.Security.DataProtection.Redis |
| Steeltoe.Stream.* | - |

The following sections provide details on the changes per Steeltoe component, as well as tips on how to migrate to Steeltoe 4.

## Bootstrap

### Behavior changes

- Unified handling for all host builder types (there were omissions in some cases, due to duplicate code not kept in sync)
- Added wire-up of Steeltoe.Configuration.SpringBoot, Steeltoe.Configuration.Encryption and Steeltoe.Logging.DynamicConsole
- Removed wire-up of client certificate authentication, which was only partly done
- When no `ILoggerFactory` is specified, `BootstrapLoggerFactory` is used by default (pass `NullLoggerFactory.Instance` to disable)
- Ignore casing when comparing assembly names (bug fix)

### NuGet Package changes

| Source | Change | Replacement | Notes |
| --- | --- | --- | --- |
| Steeltoe.Bootstrap.Autoconfig | Moved | Steeltoe.Bootstrap.AutoConfiguration | |

### API changes

| Source | Kind | Package | Change | Replacement | Notes |
| --- | --- | --- | --- | --- | --- |
| `Steeltoe.Bootstrap.Autoconfig.SteeltoeAssemblies` | Type | Steeltoe.Bootstrap.Autoconfig | Renamed | `Steeltoe.Bootstrap.AutoConfiguration.SteeltoeAssemblyNames` | Updated members to new/changed assembly names |
| `Steeltoe.Connector` | Namespace | Steeltoe.Bootstrap.Autoconfig | Removed | None | Type locators have been replaced with internal-only shims |

### Notable PRs

- https://github.com/SteeltoeOSS/Steeltoe/pull/1434
- https://github.com/SteeltoeOSS/Steeltoe/pull/1352
- https://github.com/SteeltoeOSS/Steeltoe/pull/1325
- https://github.com/SteeltoeOSS/Steeltoe/pull/1223

### Documentation

For more information, see the updated [Bootstrap documentation](../bootstrap/index.md).

## Common

### Behavior changes

- Removed various APIs that were used internally, but not designed for extensibility/reuse
- Dynamically loading custom types for connectors and service discovery is no longer possible
- Removed Spring Expression Language (SpEL) support
- Removed `UseCloudHosting` (impossible to reliably detect bound ports in all cases, while Cloud Foundry usually [^1] sets the port automatically)
- Greater flexibility in using Bootstrap logger, bug fixes
- Certificates are no longer read from OS-specific store, which proved to not work reliably (store paths in configuration instead)

### NuGet Package changes

| Source | Change | Replacement | Notes |
| --- | --- | --- | --- |
| Steeltoe.Common.Abstractions | Moved | Steeltoe.Common package | |
| Steeltoe.Common.Certificates | Added | | Support for handling X.509 certificates |
| Steeltoe.Common.Expression | Removed | None | Existed for Spring Extension Language (SpEL) support, which has been removed |
| Steeltoe.Common.Kubernetes | Removed | None | |
| Steeltoe.Common.Logging | Added | | Provides `BootstrapLoggerFactory` |
| Steeltoe.Common.Retry | Removed | None | Existed for Messaging support, which has been removed |
| Steeltoe.Common.Security | Moved | Steeltoe.Common.Certificates | |
| Steeltoe.Common.Utils | Removed | None | Contained internal helpers not designed for external usage |

### API changes

| Source | Kind | Package | Change | Replacement | Notes |
| --- | --- | --- | --- | --- | --- |
| `Microsoft.Extensions.DependencyInjection.ConfigurationServiceInstanceProviderServiceCollectionExtensions.AddConfigurationDiscoveryClient` | Extension method | Steeltoe.Common [Abstractions] | Moved | Steeltoe.Discovery.Configuration package | |
| `Steeltoe.Common.ApplicationInstanceInfo` | Type | Steeltoe.Common [Abstractions] | Members removed | None | Removed members that only apply to Cloud Foundry |
| `Steeltoe.Common.Attributes` | Namespace | Steeltoe.Common [Abstractions] | Removed | None | Dynamically loading custom types for connectors/discovery is no longer possible |
| `Steeltoe.Common.Availability` | Namespace | Steeltoe.Common [Abstractions] | Moved | Steeltoe.Management.Endpoint package | |
| `Steeltoe.Common.Availability.AvailabilityHealthContributor` | Type | Steeltoe.Common [Abstractions] | Removed | None | Made internal |
| `Steeltoe.Common.Availability.LivenessHealthContributor` | Type | Steeltoe.Common [Abstractions] | Removed | None | Made internal |
| `Steeltoe.Common.Availability.ReadinessHealthContributor` | Type | Steeltoe.Common [Abstractions] | Removed | None | Made internal |
| `Steeltoe.Common.CasingConventions.EnumExtensions.ToSnakeCaseString` | Extension method | Steeltoe.Common | Added | | Use to convert between .NET and Java enum member naming styles |
| `Steeltoe.Common.CasingConventions.SnakeCaseAllCapsEnumMemberJsonConverter` | Type | Steeltoe.Common | Added | | Use to convert between .NET and Java enum member naming styles |
| `Steeltoe.Common.ConcurrentDictionaryExtensions.GetOrAddEx` | Extension method | Steeltoe.Common [Abstractions] | Removed | None | Existed to support components that have been removed |
| `Steeltoe.Common.Configuration.ConfigurationValuesHelper` | Type | Steeltoe.Common [Abstractions] | Removed | None | Refactored to use ASP.NET Options pattern instead |
| `Steeltoe.Common.Configuration.PropertyPlaceholderHelper` | Type | Steeltoe.Common [Abstractions] | Made internal | None | Placeholder substitution is handled in Steeltoe.Configuration.Placeholder package |
| `Steeltoe.Common.Contexts` | Namespace | Steeltoe.Common [Abstractions] | Removed | None | Existed for SpEL support, which has been removed |
| `Steeltoe.Common.Converter` | Namespace | Steeltoe.Common [Abstractions] | Removed | None | Existed for SpEL support, which has been removed |
| `Steeltoe.Common.Discovery` | Namespace | Steeltoe.Common [Abstractions] | Moved | Steeltoe.Discovery.HttpClients package | |
| `Steeltoe.Common.Discovery.ConfigurationServiceInstance` | Type | Steeltoe.Common [Abstractions] | Moved | Steeltoe.Discovery.Configuration package | |
| `Steeltoe.Common.Discovery.ConfigurationServiceInstanceProvider` | Type | Steeltoe.Common [Abstractions] | Moved | ConfigurationDiscoveryOptions in Steeltoe.Discovery.Configuration package | |
| `Steeltoe.Common.Discovery.IServiceInstanceProvider` | Type | Steeltoe.Common [Abstractions] | Moved | `Steeltoe.Common.Discovery.IDiscoveryClient` | |
| `Steeltoe.Common.Discovery.IServiceInstanceProviderExtensions` | Type | Steeltoe.Common [Abstractions] | Made internal | None | Caching is handled in Steeltoe.Discovery.HttpClients package |
| `Steeltoe.Common.Discovery.IServiceRegistry<>` | Type | Steeltoe.Common [Abstractions] | Removed | None | This abstraction is no longer needed |
| `Steeltoe.Common.Discovery.SerializableIServiceInstance` | Type | Steeltoe.Common [Abstractions] | Removed | `Steeltoe.Discovery.Configuration.ConfigurationServiceInstance` | |
| `Steeltoe.Common.Expression` | Namespace | Steeltoe.Common [Abstractions] | Removed | None | Existed for SpEL support, which has been removed |
| `Steeltoe.Common.Extensions.UriExtensions` | Type | Steeltoe.Common [Abstractions] | Made internal | None | Internally used to mask URIs in logs |
| `Steeltoe.Common.IApplicationInstanceInfo` | Type | Steeltoe.Common [Abstractions] | Members removed | Type-check for `CloudFoundryApplicationOptions` at runtime | Removed members that only apply to Cloud Foundry |
| `Steeltoe.Common.IApplicationTask.Name` | Property | Steeltoe.Common [Abstractions] | Removed | None | Specify task name during registration |
| `Steeltoe.Common.ICertificateSource` | Type | Steeltoe.Common [Abstractions] | Removed | `IServiceCollection.ConfigureCertificateOptions()` in Steeltoe.Common.Certificates package | Certificate paths are now stored in `IConfiguration` to detect changes |
| `Steeltoe.Common.IHttpClientHandlerProvider` | Type | Steeltoe.Common [Abstractions] | Removed | `HttpClientHandlerFactory` in Steeltoe.Common.Http package | Should use `HttpMessageHandler` pipeline in `HttpClientFactory` instead |
| `Steeltoe.Common.IServiceCollectionExtensions.RegisterDefaultApplicationInstanceInfo` | Extension method | Steeltoe.Common [Abstractions] | Renamed | `AddApplicationInstanceInfo` | |
| `Steeltoe.Common.IServiceProviderExtensions.GetApplicationInstanceInfo` | Extension method | Steeltoe.Common [Abstractions] | Removed | `IServiceProvider.GetRequiredService<IApplicationInstanceInfo>()` | Has become redundant |
| `Steeltoe.Common.Json.JsonIgnoreEmptyCollectionAttribute` | Type | Steeltoe.Common | Added | | Annotation to exclude a collection during JSON serialization when empty |
| `Steeltoe.Common.Json.JsonSerializerOptionsExtensions.AddJsonIgnoreEmptyCollection` | Extension method | Steeltoe.Common | Added | | Configures JsonSerializerOptions to exclude empty collections |
| `Steeltoe.Common.Lifecycle` | Namespace | Steeltoe.Common [Abstractions] | Removed | None | Existed for SpEL support, which has been removed |
| `Steeltoe.Common.LoadBalancer.ILoadBalancer` | Type | Steeltoe.Common [Abstractions] | Moved | `ILoadBalancer` in Steeltoe.Discovery.HttpClients package | |
| `Steeltoe.Common.Logging.IBoostrapLoggerFactory` | Type | Steeltoe.Common [Abstractions] | Removed | `BootstrapLoggerFactory.CreateConsole()` in Steeltoe.Common.Logging package | |
| `Steeltoe.Common.Net.DnsTools` | Type | Steeltoe.Common [Abstractions] | Made internal | None | Internally used to resolve host names and IP addresses |
| `Steeltoe.Common.Net.HostInfo` | Type | Steeltoe.Common [Abstractions] | Made internal | None | Internally used to resolve host names and IP addresses |
| `Steeltoe.Common.Net.InetUtils` | Type | Steeltoe.Common [Abstractions] | Made internal | None | Internally used to resolve host names and IP addresses |
| `Steeltoe.Common.Options.AbstractOptions` | Type | Steeltoe.Common [Abstractions] | Removed | None | Refactored to use ASP.NET Options pattern instead |
| `Steeltoe.Common.Options.CertificateOptions` | Type | Steeltoe.Common [Abstractions] | Moved | Steeltoe.Common.Certificates package | |
| `Steeltoe.Common.Order` | Namespace | Steeltoe.Common [Abstractions] | Removed | None | Existed to support components that have been removed |
| `Steeltoe.Common.Platform.IsFullFramework` | Property | Steeltoe.Common [Abstractions] | Removed | None | Support for .NET Framework is no longer available |
| `Steeltoe.Common.Platform.IsNetCore` | Property | Steeltoe.Common [Abstractions] | Removed | None | This enum member is no longer needed |
| `Steeltoe.Common.Reflection` | Namespace | Steeltoe.Common [Abstractions] | Removed | None | Existed to support Type Locators, which have been replaced with internal-only shims |
| `Steeltoe.Common.Retry` | Namespace | Steeltoe.Common [Abstractions] | Removed | None | Existed for Messaging support, which has been removed |
| `Steeltoe.Common.SecurityUtilities` | Type | Steeltoe.Common [Abstractions] | Removed | None | Internally used to sanitize line breaks in logs |
| `Steeltoe.Common.Services` | Namespace | Steeltoe.Common [Abstractions] | Removed | None | Existed to support components that have been removed |
| `Steeltoe.Common.SteeltoeComponent` | Type | Steeltoe.Common [Abstractions] | Removed | None | This enum is no longer needed |
| `Steeltoe.Common.Transaction` | Namespace | Steeltoe.Common [Abstractions] | Removed | None | Existed for Messaging support, which has been removed |
| `Steeltoe.Common.Util` | Namespace | Steeltoe.Common [Abstractions] | Removed | None | Existed to support components that have been removed |
| `Steeltoe.Common.Certificates.CertificateConfigurationExtensions.AddAppInstanceIdentityCertificate` | Extension method | Steeltoe.Common.Certificates | Added | | Register/generate identity certificate for Cloud Foundry authentication |
| `Steeltoe.Common.Certificates.CertificateOptions` | Type | Steeltoe.Common.Certificates | Added | | Provides access to loaded certificate using ASP.NET Options pattern |
| `Steeltoe.Common.Certificates.CertificateServiceCollectionExtensions.ConfigureCertificateOptions` | Extension method | Steeltoe.Common.Certificates | Added | | Bind named certificate from `IConfiguration` and monitor for changes |
| `Steeltoe.Common.Hosting.BootstrapLoggerHostedService` | Type | Steeltoe.Common.Hosting | Made internal | None | Moved to Steeltoe.Common.Logging package |
| `Steeltoe.Common.Hosting.HostBuilderExtensions.UseCloudHosting` | Extension method | Steeltoe.Common.Hosting | Removed | Specify ports explicitly [^1] | Feature dropped, impossible to reliably detect bound ports in all cases |
| `Microsoft.Extensions.DependencyInjection.LoadBalancerHttpClientBuilderExtensions.AddLoadBalancer<T>` | Extension method | Steeltoe.Common.Http | Moved | `AddServiceDiscovery<T>()` in Steeltoe.Discovery.HttpClients package | |
| `Microsoft.Extensions.DependencyInjection.LoadBalancerHttpClientBuilderExtensions.AddRandomLoadBalancer` | Extension method | Steeltoe.Common.Http | Moved | `AddServiceDiscovery<RandomLoadBalancer>()` in Steeltoe.Discovery.HttpClients package | |
| `Microsoft.Extensions.DependencyInjection.LoadBalancerHttpClientBuilderExtensions.AddRoundRobinLoadBalancer` | Extension method | Steeltoe.Common.Http | Moved | `AddServiceDiscovery<RoundRobinLoadBalancer>()` in Steeltoe.Discovery.HttpClients package | |
| `Steeltoe.Common.Discovery.DiscoveryHttpClientHandler` | Type | Steeltoe.Common.Http | Removed | `DiscoveryHttpClientHandler` in Steeltoe.Discovery.HttpClients package | |
| `Steeltoe.Common.Discovery.DiscoveryHttpClientHandlerBase` | Type | Steeltoe.Common.Http | Removed | `DiscoveryHttpClientHandler` in Steeltoe.Discovery.HttpClients package | |
| `Steeltoe.Common.Http.ClientCertificateHttpHandler` | Type | Steeltoe.Common.Http | Removed | None | Rotating certificates in OS-level certificate store proved to be unreliable |
| `Steeltoe.Common.Http.ClientCertificateHttpHandlerProvider` | Type | Steeltoe.Common.Http | Removed | None | Refactored to internal `ClientCertificateHttpClientHandlerConfigurer` |
| `Steeltoe.Common.Http.Discovery.DiscoveryHttpClientBuilderExtensions.AddServiceDiscovery` | Extension method | Steeltoe.Common.Http | Moved | `AddServiceDiscovery()` in Steeltoe.Discovery.HttpClients package | |
| `Steeltoe.Common.Http.Discovery.DiscoveryHttpMessageHandler` | Type | Steeltoe.Common.Http | Removed | `DiscoveryHttpDelegatingHandler<>` in Steeltoe.Discovery.HttpClients package | |
| `Steeltoe.Common.Http.HttpClientHelper` | Type | Steeltoe.Common.Http | Removed | None | Refactored handling of client certificates |
| `Steeltoe.Common.Http.HttpClientPooling.HttpClientHandlerFactory` | Type | Steeltoe.Common.Http | Added | | Enables to mock request/response from tests |
| `Steeltoe.Common.Http.LoadBalancer.LoadBalancerDelegatingHandler` | Type | Steeltoe.Common.Http | Moved | `DiscoveryHttpDelegatingHandler<>` in Steeltoe.Discovery.HttpClients package | |
| `Steeltoe.Common.Http.LoadBalancer.LoadBalancerHttpClientHandler` | Type | Steeltoe.Common.Http | Moved | `DiscoveryHttpClientHandler` in Steeltoe.Discovery.HttpClients package | |
| `Steeltoe.Common.Http.Serialization.BoolStringJsonConverter` | Type | Steeltoe.Common.Http | Removed | None | Made internal, moved to Steeltoe.Discovery.Eureka package |
| `Steeltoe.Common.Http.Serialization.LongStringJsonConverter` | Type | Steeltoe.Common.Http | Removed | None | Made internal, moved to Steeltoe.Discovery.Eureka package |
| `Steeltoe.Common.Logging.BootstrapLoggerFactory` | Type | Steeltoe.Common.Logging | Added | | Writes startup logs to console before logging has initialized |
| `Steeltoe.Common.Logging.BootstrapLoggerServiceCollectionExtensions.UpgradeBootstrapLoggerFactory` | Extension method | Steeltoe.Common.Logging | Added | | Upgrades existing loggers after app has started |
| `Steeltoe.Common.Net.IMPR` | Type | Steeltoe.Common.Net | Removed | None | Renamed to internal type `IMultipleProviderRouter` (existed for testing only) |
| `Steeltoe.Common.Net.WindowsNetworkFileShare.GetLastError` | Method | Steeltoe.Common.Net | Removed | None | Now throws `IOException` on error |
| `Steeltoe.Common.Net.WindowsNetworkFileShare.NetResource` | Type | Steeltoe.Common.Net | Removed | None | Nested type used internally for P/Invoke, should not be public |
| `Steeltoe.Common.Net.WindowsNetworkFileShare.ResourceDisplaytype` | Type | Steeltoe.Common.Net | Removed | None | Nested type used internally for P/Invoke, should not be public |
| `Steeltoe.Common.Net.WindowsNetworkFileShare.ResourceScope` | Type | Steeltoe.Common.Net | Removed | None | Nested type used internally for P/Invoke, should not be public |
| `Steeltoe.Common.Net.WindowsNetworkFileShare.ResourceType` | Type | Steeltoe.Common.Net | Removed | None | Nested type used internally for P/Invoke, should not be public |
| `Steeltoe.Common.Security.CertificateProvider` | Type | Steeltoe.Common.Security | Removed | Store certificate paths in `IConfiguration` | Refactored to use ASP.NET Options pattern |
| `Steeltoe.Common.Security.CertificateRotationService` | Type | Steeltoe.Common.Security | Removed | None | Rotating certificates in OS-level certificate store proved to be unreliable |
| `Steeltoe.Common.Security.CertificateSource` | Type | Steeltoe.Common.Security | Removed | Store certificate paths in `IConfiguration` | Refactored to use ASP.NET Options pattern |
| `Steeltoe.Common.Security.ConfigurationExtensions.AddCertificateFile` | Extension method | Steeltoe.Common.Security | Removed | `CertificateServiceCollectionExtensions.ConfigureCertificateOptions()` | Refactored to use ASP.NET Options pattern |
| `Steeltoe.Common.Security.ConfigurationExtensions.AddPemFiles` | Extension method | Steeltoe.Common.Security | Removed | `CertificateServiceCollectionExtensions.ConfigureCertificateOptions()` | Refactored to use ASP.NET Options pattern |
| `Steeltoe.Common.Security.ConfigureCertificateOptions` | Type | Steeltoe.Common.Security | Removed | Store certificate paths in `IConfiguration` | Refactored to use ASP.NET Options pattern |
| `Steeltoe.Common.Security.ICertificateRotationService` | Type | Steeltoe.Common.Security | Removed | None | Rotating certificates in OS-level certificate store proved to be unreliable |
| `Steeltoe.Common.Security.LocalCertificateWriter` | Type | Steeltoe.Common.Security | Removed | `CertificateConfigurationExtensions.AddAppInstanceIdentityCertificate()` | Refactored to use ASP.NET Options pattern |
| `Steeltoe.Common.Security.PemCertificateProvider` | Type | Steeltoe.Common.Security | Removed | Store certificate paths in `IConfiguration` | Refactored to use ASP.NET Options pattern |
| `Steeltoe.Common.Security.PemCertificateSource` | Type | Steeltoe.Common.Security | Removed | Store certificate paths in `IConfiguration` | Refactored to use ASP.NET Options pattern |
| `Steeltoe.Common.Security.PemConfigureCertificateOptions` | Type | Steeltoe.Common.Security | Removed | Store certificate paths in `IConfiguration` | Refactored to use ASP.NET Options pattern |

[^1]: When using the binary buildpack, specify port bindings in an [environment variable](https://learn.microsoft.com/aspnet/core/fundamentals/servers/kestrel/endpoints#specify-ports-only) or on the command-line: `--urls=http://0.0.0.0:%PORT%`.

### Notable PRs

- https://github.com/SteeltoeOSS/Steeltoe/pull/1523
- https://github.com/SteeltoeOSS/Steeltoe/pull/1342
- https://github.com/SteeltoeOSS/Steeltoe/pull/1334
- https://github.com/SteeltoeOSS/Steeltoe/pull/1330
- https://github.com/SteeltoeOSS/Steeltoe/pull/1327
- https://github.com/SteeltoeOSS/Steeltoe/pull/1321
- https://github.com/SteeltoeOSS/Steeltoe/pull/1306
- https://github.com/SteeltoeOSS/Steeltoe/pull/1247
- https://github.com/SteeltoeOSS/Steeltoe/pull/1246
- https://github.com/SteeltoeOSS/Steeltoe/pull/1080

## Configuration

### Behavior changes

- Placeholder substitution changed internally (wrapping and taking ownership of sources), should be added as late as possible
- To improve performance, Config Server provider doesn't substitute placeholders by default anymore
  - Call `AddPlaceholderResolver()` *before* `AddConfigServer()` to substitute info to connect to Config Server
  - Call `AddPlaceholderResolver()` *after* `AddConfigServer()` to substitute placeholders in settings from all providers including Config Server
  - Call `AddPlaceholderResolver()` *before and after* `AddConfigServer()` to substitute placeholders in both sources
- Added trace-level logging in placeholder provider to diagnose substitution
- New configuration provider to decrypt settings in Config Server (should be added as late as possible)
- Reduced noise in Config Server logging
- Universal configuration of client certificates, using ASP.NET Options pattern (named, with fallback to default)
- Added support for reading from [Application Configuration Service for VMware Tanzu](https://techdocs.broadcom.com/us/en/vmware-tanzu/standalone-components/application-configuration-service-for-tanzu/2-4/app-config-service/overview.html) on Kubernetes
- Improved support for ASP.NET Options pattern, responding to configuration changes at runtime
- Removed configuration provider that directly interacts with the Kubernetes API

### NuGet Package changes

| Steeltoe.Extensions.Configuration.Abstractions | Renamed | Steeltoe.Configuration.Abstractions | |
| --- | --- | --- | --- |
| Steeltoe.Extensions.Configuration.CloudFoundryBase | Renamed | Steeltoe.Configuration.CloudFoundry | |
| Steeltoe.Extensions.Configuration.CloudFoundryCore | Renamed | Steeltoe.Configuration.CloudFoundry | |
| Steeltoe.Extensions.Configuration.ConfigServerBase | Renamed | Steeltoe.Configuration.ConfigServer | |
| Steeltoe.Extensions.Configuration.ConfigServerCore | Renamed | Steeltoe.Configuration.ConfigServer | |
| Steeltoe.Configuration.Encryption | Added | | Provides decryption of `IConfiguration` entries |
| Steeltoe.Extensions.Configuration.KubernetesBase | Removed | None | |
| Steeltoe.Extensions.Configuration.KubernetesCore | Removed | None | |
| Steeltoe.Extensions.Configuration.Kubernetes.ServiceBinding | Renamed | Steeltoe.Configuration.Kubernetes.ServiceBindings | |
| Steeltoe.Extensions.Configuration.PlaceholderBase | Renamed | Steeltoe.Configuration.Placeholder | |
| Steeltoe.Extensions.Configuration.PlaceholderCore | Renamed | Steeltoe.Configuration.Placeholder | |
| Steeltoe.Extensions.Configuration.RandomValueBase | Renamed | Steeltoe.Configuration.RandomValue | |
| Steeltoe.Extensions.Configuration.RandomValueCore | Renamed | Steeltoe.Configuration.RandomValue | |
| Steeltoe.Extensions.Configuration.SpringBootBase | Renamed | Steeltoe.Configuration.SpringBoot | |
| Steeltoe.Extensions.Configuration.SpringBootCore | Renamed | Steeltoe.Configuration.SpringBoot | |

### API changes

| Source | Kind | Package | Change | Replacement | Notes |
| --- | --- | --- | --- | --- | --- |
| `Steeltoe.Configuration.ICompositeConfigurationSource` | Type | Steeltoe.Configuration.Abstractions | Added | | Building block for configuration providers |
| `Steeltoe.Extensions.Configuration.AbstractServiceOptions` | Type | Steeltoe.Extensions.Configuration.Abstractions | Removed | `CloudFoundryService` in Steeltoe.Configuration.CloudFoundry package | Refactored to use ASP.NET Options pattern |
| `Steeltoe.Extensions.Configuration.AbstractServiceOptions.GetInstancesOfType` | Method | Steeltoe.Extensions.Configuration.Abstractions | Moved | `CloudFoundryServicesOptions.GetServicesOfType` in Steeltoe.Configuration.CloudFoundry package | |
| `Steeltoe.Extensions.Configuration.AbstractServiceOptions.GetServicesList` | Method | Steeltoe.Extensions.Configuration.Abstractions | Moved | `CloudFoundryServicesOptions.GetAllServices` in Steeltoe.Configuration.CloudFoundry package | |
| `Steeltoe.Extensions.Configuration.Credential` | Type | Steeltoe.Extensions.Configuration.Abstractions | Moved | `CloudFoundryCredentials` in Steeltoe.Configuration.CloudFoundry package | Refactored to use ASP.NET Options pattern |
| `Steeltoe.Extensions.Configuration.CredentialConverter` | Type | Steeltoe.Extensions.Configuration.Abstractions | Removed | None | Moved to internal type `CredentialsConverter` |
| `Steeltoe.Extensions.Configuration.IPlaceholderResolverProvider` | Type | Steeltoe.Extensions.Configuration.Abstractions | Removed | `PlaceholderConfigurationProvider` in Steeltoe.Configuration.Placeholder package | |
| `Steeltoe.Extensions.Configuration.IServiceCollectionExtensions.GetServicesInfo` | Extension method | Steeltoe.Extensions.Configuration.Abstractions | Removed | `IServiceProvider.GetRequiredService<IOptionsMonitor<CloudFoundryServicesOptions>>()` | Refactored to use ASP.NET Options pattern |
| `Steeltoe.Extensions.Configuration.IServicesInfo` | Type | Steeltoe.Extensions.Configuration.Abstractions | Removed | None | Refactored to use ASP.NET Options pattern |
| `Steeltoe.Extensions.Configuration.Service` | Type | Steeltoe.Extensions.Configuration.Abstractions | Moved | `CloudFoundryService` in Steeltoe.Configuration.CloudFoundry package | Refactored to use ASP.NET Options pattern |
| `Steeltoe.Extensions.Configuration.ServicesOptions` | Type | Steeltoe.Extensions.Configuration.Abstractions | Moved | `CloudFoundryServicesOptions` in Steeltoe.Configuration.CloudFoundry package | Refactored to use ASP.NET Options pattern |
| `Steeltoe.Configuration.CloudFoundry.CloudFoundryApplicationOptions.OrganizationId` | Property | Steeltoe.Configuration.CloudFoundry | Added | | |
| `Steeltoe.Configuration.CloudFoundry.CloudFoundryApplicationOptions.OrganizationName` | Property | Steeltoe.Configuration.CloudFoundry | Added | | |
| `Steeltoe.Configuration.CloudFoundry.CloudFoundryApplicationOptions.ProcessId` | Property | Steeltoe.Configuration.CloudFoundry | Added | | |
| `Steeltoe.Configuration.CloudFoundry.CloudFoundryApplicationOptions.ProcessType` | Property | Steeltoe.Configuration.CloudFoundry | Added | | |
| `Steeltoe.Configuration.CloudFoundry.CloudFoundryApplicationOptions.StartedAtTimestamp` | Property | Steeltoe.Configuration.CloudFoundry | Added | | |
| `Steeltoe.Configuration.CloudFoundry.CloudFoundryService` | Type | Steeltoe.Configuration.CloudFoundry | Added | | A service object in VCAP_SERVICES |
| `Steeltoe.Configuration.CloudFoundry.ServiceBindings.ConfigurationBuilderExtensions.AddCloudFoundryServiceBindings` | Extension method | Steeltoe.Configuration.CloudFoundry | Added | | Post-processor based API for reading VCAP_SERVICES into `IConfiguration` |
| `Steeltoe.Configuration.CloudFoundry.ServiceBindings.IServiceBindingsReader` | Type | Steeltoe.Configuration.CloudFoundry | Added | | Enables to provide VCAP_SERVICES from tests |
| `Steeltoe.Extensions.Configuration.CloudFoundry.CloudFoundryApplicationOptions.Application_Uris` | Property | Steeltoe.Extensions.Configuration.CloudFoundry [Base/Core] | Renamed | `CloudFoundryApplicationOptions.Uris` | |
| `Steeltoe.Extensions.Configuration.CloudFoundry.CloudFoundryApplicationOptions.Application_Version` | Property | Steeltoe.Extensions.Configuration.CloudFoundry [Base/Core] | Renamed | `CloudFoundryApplicationOptions.ApplicationVersion` | |
| `Steeltoe.Extensions.Configuration.CloudFoundry.CloudFoundryApplicationOptions.CF_Api` | Property | Steeltoe.Extensions.Configuration.CloudFoundry [Base/Core] | Renamed | `CloudFoundryApplicationOptions.Api` | |
| `Steeltoe.Extensions.Configuration.CloudFoundry.CloudFoundryApplicationOptions.DiskLimit` | Property | Steeltoe.Extensions.Configuration.CloudFoundry [Base/Core] | Removed | `CloudFoundryApplicationOptions.Limits.Disk` | |
| `Steeltoe.Extensions.Configuration.CloudFoundry.CloudFoundryApplicationOptions.FileDescriptorLimit` | Property | Steeltoe.Extensions.Configuration.CloudFoundry [Base/Core] | Removed | `CloudFoundryApplicationOptions.Limits.FileDescriptor` | |
| `Steeltoe.Extensions.Configuration.CloudFoundry.CloudFoundryApplicationOptions.Instance_Index` | Property | Steeltoe.Extensions.Configuration.CloudFoundry [Base/Core] | Renamed | `CloudFoundryApplicationOptions.InstanceIndex` | |
| `Steeltoe.Extensions.Configuration.CloudFoundry.CloudFoundryApplicationOptions.Instance_IP` | Property | Steeltoe.Extensions.Configuration.CloudFoundry [Base/Core] | Renamed | `CloudFoundryApplicationOptions.InstanceIP` | |
| `Steeltoe.Extensions.Configuration.CloudFoundry.CloudFoundryApplicationOptions.Internal_IP` | Property | Steeltoe.Extensions.Configuration.CloudFoundry [Base/Core] | Renamed | `CloudFoundryApplicationOptions.InternalIP` | |
| `Steeltoe.Extensions.Configuration.CloudFoundry.CloudFoundryApplicationOptions.MemoryLimit` | Property | Steeltoe.Extensions.Configuration.CloudFoundry [Base/Core] | Removed | `CloudFoundryApplicationOptions.Limits.Memory` | |
| `Steeltoe.Extensions.Configuration.CloudFoundry.CloudFoundryApplicationOptions.Space_Id` | Property | Steeltoe.Extensions.Configuration.CloudFoundry [Base/Core] | Renamed | `CloudFoundryApplicationOptions.SpaceId` | |
| `Steeltoe.Extensions.Configuration.CloudFoundry.CloudFoundryConfigurationProvider` | Type | Steeltoe.Extensions.Configuration.CloudFoundry [Base/Core] | Removed | None | Refactored to use ASP.NET Options pattern |
| `Steeltoe.Extensions.Configuration.CloudFoundry.CloudFoundryConfigurationSource` | Type | Steeltoe.Extensions.Configuration.CloudFoundry [Base/Core] | Removed | None | Refactored to use ASP.NET Options pattern |
| `Steeltoe.Extensions.Configuration.CloudFoundry.CloudFoundryEnvironmentSettingsReader` | Type | Steeltoe.Extensions.Configuration.CloudFoundry [Base/Core] | Removed | None | Made internal, not designed for reuse/extensibility |
| `Steeltoe.Extensions.Configuration.CloudFoundry.CloudFoundryMemorySettingsReader` | Type | Steeltoe.Extensions.Configuration.CloudFoundry [Base/Core] | Removed | None | Made internal, used for unit tests only |
| `Steeltoe.Extensions.Configuration.CloudFoundry.CloudFoundryServiceCollectionExtensions.ConfigureCloudFoundryOptions` | Extension method | Steeltoe.Extensions.Configuration.CloudFoundry [Base/Core] | Removed | `CloudFoundryHostBuilderExtensions.AddCloudFoundryConfiguration` | |
| `Steeltoe.Extensions.Configuration.CloudFoundry.CloudFoundryServiceCollectionExtensions.ConfigureCloudFoundryService<T>` | Extension method | Steeltoe.Extensions.Configuration.CloudFoundry [Base/Core] | Removed | None | |
| `Steeltoe.Extensions.Configuration.CloudFoundry.IServiceCollectionExtensions.RegisterCloudFoundryApplicationInstanceInfo` | Extension method | Steeltoe.Extensions.Configuration.CloudFoundry [Base/Core] | Renamed | `CloudFoundryServiceCollectionExtensions.AddCloudFoundryOptions` | |
| `Steeltoe.Extensions.Configuration.CloudFoundry.Limits` | Type | Steeltoe.Extensions.Configuration.CloudFoundry [Base/Core] | Renamed | `ApplicationLimits` | |
| `Steeltoe.Extensions.Configuration.CloudFoundry.Limits.Fds` | Property | Steeltoe.Extensions.Configuration.CloudFoundry [Base/Core] | Renamed | `ApplicationLimits.FileDescriptor` | |
| `Steeltoe.Extensions.Configuration.CloudFoundry.Limits.Mem` | Property | Steeltoe.Extensions.Configuration.CloudFoundry [Base/Core] | Renamed | `ApplicationLimits.Memory` | |
| `Steeltoe.Extensions.Configuration.ConfigServer.ConfigServerClientSettings` | Type | Steeltoe.Extensions.Configuration.ConfigServer [Base/Core] | Renamed | `ConfigServerClientOptions` | |
| `Steeltoe.Extensions.Configuration.ConfigServer.ConfigServerClientSettings.ClientCertificate` | Property | Steeltoe.Extensions.Configuration.ConfigServer [Base/Core] | Removed | Store certificate paths in `IConfiguration` | Refactored to use ASP.NET Options pattern |
| `Steeltoe.Extensions.Configuration.ConfigServer.ConfigServerClientSettings.RawUri` | Property | Steeltoe.Extensions.Configuration.ConfigServer [Base/Core] | Removed | `ConfigServerClientOptions.Uri` | |
| `Steeltoe.Extensions.Configuration.ConfigServer.ConfigServerClientSettingsOptions` | Type | Steeltoe.Extensions.Configuration.ConfigServer [Base/Core] | Renamed | `ConfigServerClientOptions` | |
| `Steeltoe.Extensions.Configuration.ConfigServer.ConfigServerClientSettingsOptions.Access_Token_Uri` | Property | Steeltoe.Extensions.Configuration.ConfigServer [Base/Core] | Removed | `ConfigServerClientOptions.AccessTokenUri` | |
| `Steeltoe.Extensions.Configuration.ConfigServer.ConfigServerClientSettingsOptions.Client_Id` | Property | Steeltoe.Extensions.Configuration.ConfigServer [Base/Core] | Removed | `ConfigServerClientOptions.ClientId` | |
| `Steeltoe.Extensions.Configuration.ConfigServer.ConfigServerClientSettingsOptions.Client_Secret` | Property | Steeltoe.Extensions.Configuration.ConfigServer [Base/Core] | Removed | `ConfigServerClientOptions.ClientSecret` | |
| `Steeltoe.Extensions.Configuration.ConfigServer.ConfigServerClientSettingsOptions.DiscoveryEnabled` | Property | Steeltoe.Extensions.Configuration.ConfigServer [Base/Core] | Removed | `ConfigServerClientOptions.Discovery.Enabled` | |
| `Steeltoe.Extensions.Configuration.ConfigServer.ConfigServerClientSettingsOptions.DiscoveryServiceId` | Property | Steeltoe.Extensions.Configuration.ConfigServer [Base/Core] | Removed | `ConfigServerClientOptions.Discovery.ServiceId` | |
| `Steeltoe.Extensions.Configuration.ConfigServer.ConfigServerClientSettingsOptions.Env` | Property | Steeltoe.Extensions.Configuration.ConfigServer [Base/Core] | Removed | `ConfigServerClientOptions.Environment` | |
| `Steeltoe.Extensions.Configuration.ConfigServer.ConfigServerClientSettingsOptions.HealthEnabled` | Property | Steeltoe.Extensions.Configuration.ConfigServer [Base/Core] | Removed | `ConfigServerClientOptions.Health.Enabled` | |
| `Steeltoe.Extensions.Configuration.ConfigServer.ConfigServerClientSettingsOptions.HealthTimeToLive` | Property | Steeltoe.Extensions.Configuration.ConfigServer [Base/Core] | Removed | `ConfigServerClientOptions.Health.TimeToLive` | |
| `Steeltoe.Extensions.Configuration.ConfigServer.ConfigServerClientSettingsOptions.RetryAttempts` | Property | Steeltoe.Extensions.Configuration.ConfigServer [Base/Core] | Removed | `ConfigServerClientOptions.Retry.MaxAttempts` | |
| `Steeltoe.Extensions.Configuration.ConfigServer.ConfigServerClientSettingsOptions.RetryEnabled` | Property | Steeltoe.Extensions.Configuration.ConfigServer [Base/Core] | Removed | `ConfigServerClientOptions.Retry.Enabled` | |
| `Steeltoe.Extensions.Configuration.ConfigServer.ConfigServerClientSettingsOptions.RetryInitialInterval` | Property | Steeltoe.Extensions.Configuration.ConfigServer [Base/Core] | Removed | `ConfigServerClientOptions.Retry.InitialInterval` | |
| `Steeltoe.Extensions.Configuration.ConfigServer.ConfigServerClientSettingsOptions.RetryMaxInterval` | Property | Steeltoe.Extensions.Configuration.ConfigServer [Base/Core] | Removed | `ConfigServerClientOptions.Retry.MaxInterval` | |
| `Steeltoe.Extensions.Configuration.ConfigServer.ConfigServerClientSettingsOptions.RetryMultiplier` | Property | Steeltoe.Extensions.Configuration.ConfigServer [Base/Core] | Removed | `ConfigServerClientOptions.Retry.Multiplier` | |
| `Steeltoe.Extensions.Configuration.ConfigServer.ConfigServerClientSettingsOptions.Validate_Certificates` | Property | Steeltoe.Extensions.Configuration.ConfigServer [Base/Core] | Removed | `ConfigServerClientOptions.ValidateCertificates` | |
| `Steeltoe.Extensions.Configuration.ConfigServer.ConfigServerConfigurationProvider` | Type | Steeltoe.Extensions.Configuration.ConfigServer [Base/Core] | Removed | None | Made internal, not designed for reuse/extensibility |
| `Steeltoe.Extensions.Configuration.ConfigServer.ConfigServerConfigurationSource` | Type | Steeltoe.Extensions.Configuration.ConfigServer [Base/Core] | Removed | None | Made internal, not designed for reuse/extensibility |
| `Steeltoe.Extensions.Configuration.ConfigServer.ConfigServerHealthContributor` | Type | Steeltoe.Extensions.Configuration.ConfigServer [Base/Core] | Removed | None | Made internal, not designed for reuse/extensibility |
| `Steeltoe.Extensions.Configuration.ConfigServer.ConfigServerHostedService.ConfigServerHostedService` | Type | Steeltoe.Extensions.Configuration.ConfigServer [Base/Core] | Removed | None | Made internal, not designed for reuse/extensibility |
| `Steeltoe.Extensions.Configuration.ConfigServer.ConfigurationSettingsHelper` | Type | Steeltoe.Extensions.Configuration.ConfigServer [Base/Core] | Removed | None | Refactored to use ASP.NET Options pattern |
| `Steeltoe.Extensions.Configuration.ConfigServer.SpringCloudConfigDiscovery` | Type | Steeltoe.Extensions.Configuration.ConfigServer [Base/Core] | Renamed | `ConfigServerDiscoveryOptions` | |
| `Steeltoe.Extensions.Configuration.ConfigServer.SpringCloudConfigHealth` | Type | Steeltoe.Extensions.Configuration.ConfigServer [Base/Core] | Renamed | `ConfigServerHealthOptions` | |
| `Steeltoe.Extensions.Configuration.ConfigServer.SpringCloudConfigRetry` | Type | Steeltoe.Extensions.Configuration.ConfigServer [Base/Core] | Renamed | `ConfigServerRetryOptions` | |
| `Steeltoe.Configuration.Encryption.Cryptography.DecryptionException` | Type | Steeltoe.Configuration.Encryption | Added | | Thrown when unable to decrypt |
| `Steeltoe.Configuration.Encryption.Cryptography.ITextDecryptor` | Type | Steeltoe.Configuration.Encryption | Added | | Provides pluggable decryption algorithms |
| `Steeltoe.Configuration.Encryption.DecryptionConfigurationBuilderExtensions.AddDecryption` | Extension method | Steeltoe.Configuration.Encryption | Added | | Activates decryption |
| `Steeltoe.Configuration.Kubernetes.ServiceBindings.IServiceBindingsReader` | Type | Steeltoe.Configuration.Kubernetes.ServiceBindings | Added | | Enables to provide bindings from tests |
| `Steeltoe.Extensions.Configuration.Placeholder.PlaceholderResolverConfigurationExtensions.AddPlaceholderResolver` | Extension method | Steeltoe.Extensions.Configuration.Placeholder [Base/Core] | Moved | `PlaceholderConfigurationBuilderExtensions.AddPlaceholderResolver` | |
| `Steeltoe.Extensions.Configuration.Placeholder.PlaceholderResolverExtensions.AddPlaceholderResolver` | Extension method | Steeltoe.Extensions.Configuration.Placeholder [Base/Core] | Removed | `IConfigurationBuilder.AddPlaceholderResolver()` | |
| `Steeltoe.Extensions.Configuration.Placeholder.PlaceholderResolverProvider` | Type | Steeltoe.Extensions.Configuration.Placeholder [Base/Core] | Removed | None | Renamed to internal `PlaceholderConfigurationProvider` |
| `Steeltoe.Extensions.Configuration.Placeholder.PlaceholderResolverSource` | Type | Steeltoe.Extensions.Configuration.Placeholder [Base/Core] | Removed | None | Renamed to internal `PlaceholderConfigurationSource` |
| `Steeltoe.Extensions.Configuration.RandomValue.RandomValueProvider` | Type | Steeltoe.Extensions.Configuration.RandomValue [Base/Core] | Removed | None | Made internal, not designed for reuse/extensibility |
| `Steeltoe.Extensions.Configuration.RandomValue.RandomValueSource` | Type | Steeltoe.Extensions.Configuration.RandomValue [Base/Core] | Removed | None | Made internal, not designed for reuse/extensibility |
| `Steeltoe.Extensions.Configuration.SpringBoot.SpringBootCmdProvider` | Type | Steeltoe.Extensions.Configuration.SpringBoot [Base/Core] | Removed | None | Renamed to internal `SpringBootCommandLineProvider` |
| `Steeltoe.Extensions.Configuration.SpringBoot.SpringBootCmdSource` | Type | Steeltoe.Extensions.Configuration.SpringBoot [Base/Core] | Removed | None | Renamed to internal `SpringBootCommandLineSource` |
| `Steeltoe.Extensions.Configuration.SpringBoot.SpringBootConfigurationBuilderExtensions.AddSpringBootCmd` | Extension method | Steeltoe.Extensions.Configuration.SpringBoot [Base/Core] | Renamed | `SpringBootConfigurationBuilderExtensions.AddSpringBootFromCommandLine` | |
| `Steeltoe.Extensions.Configuration.SpringBoot.SpringBootConfigurationBuilderExtensions.AddSpringBootEnv` | Extension method | Steeltoe.Extensions.Configuration.SpringBoot [Base/Core] | Renamed | `SpringBootConfigurationBuilderExtensions.AddSpringBootFromEnvironmentVariable` | |
| `Steeltoe.Extensions.Configuration.SpringBoot.SpringBootEnvProvider` | Type | Steeltoe.Extensions.Configuration.SpringBoot [Base/Core] | Removed | None | Renamed to internal `SpringBootEnvironmentVariableProvider` |
| `Steeltoe.Extensions.Configuration.SpringBoot.SpringBootEnvSource` | Type | Steeltoe.Extensions.Configuration.SpringBoot [Base/Core] | Removed | None | Renamed to internal `SpringBootEnvironmentVariableSource` |
| `Steeltoe.Extensions.Configuration.SpringBoot.SpringBootHostBuilderExtensions.AddSpringBootConfiguration` | Extension method | Steeltoe.Extensions.Configuration.SpringBoot [Base/Core] | Removed | `builder.Configuration.AddSpringBootFromCommandLine/EnvironmentVariable()` | Redundant |

### Notable PRs

- https://github.com/SteeltoeOSS/Steeltoe/pull/1360
- https://github.com/SteeltoeOSS/Steeltoe/pull/1355
- https://github.com/SteeltoeOSS/Steeltoe/pull/1339
- https://github.com/SteeltoeOSS/Steeltoe/pull/1306
- https://github.com/SteeltoeOSS/Steeltoe/pull/1277
- https://github.com/SteeltoeOSS/Steeltoe/pull/1276
- https://github.com/SteeltoeOSS/Steeltoe/pull/1243
- https://github.com/SteeltoeOSS/Steeltoe/pull/1228
- https://github.com/SteeltoeOSS/Steeltoe/pull/1196
- https://github.com/SteeltoeOSS/Steeltoe/pull/1183
- https://github.com/SteeltoeOSS/Steeltoe/pull/1179
- https://github.com/SteeltoeOSS/Steeltoe/pull/1149
- https://github.com/SteeltoeOSS/Steeltoe/pull/1099
- https://github.com/SteeltoeOSS/Steeltoe/pull/1097
- https://github.com/SteeltoeOSS/Steeltoe/pull/1008

### Documentation

For more information, see the updated [Configuration documentation](../configuration/index.md) and
[Configuration samples](https://github.com/SteeltoeOSS/Samples/tree/main/Configuration).

## Connectors

### Behavior changes

- Universal configuration and API shape for single/multiple (named) service bindings
  - ADO.NET API: `builder.Add*()`, inject `ConnectorFactory<TOptions, TConnection>` (driver-specific connection/client instances are no longer registered)
  - EF Core API: must call `builder.Add*()` first. Example: `builder.AddMySql(); builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) => options.UseMySql(serviceProvider));`
  - The structure of configuration has changed severely to accommodate multiple named service bindings in a unified way
- Compatible with the latest versions of Tanzu, Cloud Foundry, and .NET database drivers
- Added [Cloud Native Binding](https://github.com/servicebinding/spec) support (used by Tanzu Application Platform and Tanzu Platform for Kubernetes) for MongoDB, MySQL, PostgreSQL, RabbitMQ, and Redis/Valkey
- Leverage .NET connection strings (agnostic to the driver-specific parameters) using
  [ASP.NET Options pattern](https://learn.microsoft.com/aspnet/core/fundamentals/configuration/options)
- Connection string from appsettings.json is preserved, replacing parameters from cloud bindings
- No more defaults for missing connection parameters that are required by drivers
- Provide injectable (named) `IOptions` that expose the merged connection string, respond to configuration changes at runtime
- Provide an injectable factory to obtain a (named) driver-specific connection/client instance (such as `NpgsqlConnection`, `IMongoClient`, `IConnectionMultiplexer`, etc)
- Automatic connection lifetime management, depending on driver-specific best practices
- Earlier limitations on health check registration with Entity Framework Core no longer apply
- Reflection-based code replaced by internal-only shims
- Various fixes in handling special characters in connection parameters
- Removed support for Oracle databases (community-contributed, no way to test it, no Cloud Foundry support)
- Further details at https://github.com/SteeltoeOSS/Steeltoe/issues/638#issuecomment-1584303824

### NuGet Package changes

| Source | Change | Replacement | Notes |
| --- | --- | --- | --- |
| Steeltoe.Connector.Abstractions | Removed | None | Redundant after refactorings |
| Steeltoe.Connector.CloudFoundry | Removed | None | Redundant after refactorings |
| Steeltoe.Connector.ConnectorBase | Renamed | Steeltoe.Connectors | |
| Steeltoe.Connector.ConnectorCore | Renamed | Steeltoe.Connectors | |
| Steeltoe.Connector.EF6Core | Removed | | Entity Framework 6 is no longer being developed |
| Steeltoe.Connector.EFCore | Renamed | Steeltoe.Connectors.EntityFrameworkCore | |

### API changes

| Source | Kind | Package | Change | Replacement | Notes |
| --- | --- | --- | --- | --- | --- |
| `Steeltoe.Connector.AbstractServiceConnectorOptions` | Type | Steeltoe.Connector.Connector [Base/Core] | Removed | None | Redundant after refactorings |
| `Steeltoe.Connector.ConnectionStringConfigurationSource` | Type | Steeltoe.Connector.Connector [Base/Core] | Removed | Use connection string in configuration | Refactored to use ASP.NET Options pattern |
| `Steeltoe.Connector.ConnectionStringManager` | Type | Steeltoe.Connector.Connector [Base/Core] | Removed | `IServiceProvider.GetRequiredService<IOptions<T>>()` | Refactored to use ASP.NET Options pattern |
| `Steeltoe.Connector.ConnectorException` | Type | Steeltoe.Connector.Connector [Base/Core] | Removed | Catch `IOException`/`InvalidOperationException`/`ArgumentException`/`OperationCanceledException` | Standard .NET exceptions are thrown |
| `Steeltoe.Connector.ConnectorIOptions<T>` | Type | Steeltoe.Connector.Connector [Base/Core] | Removed | None | Redundant after refactorings |
| `Steeltoe.Connector.CosmosDb.CosmosDbConnectionInfo` | Type | Steeltoe.Connector.Connector [Base/Core] | Removed | Use connection string in configuration | Refactored to use ASP.NET Options pattern |
| `Steeltoe.Connector.CosmosDb.CosmosDbConnectorFactory` | Type | Steeltoe.Connector.Connector [Base/Core] | Removed | `ConnectorFactory<CosmosDbOptions, CosmosClient>` | Registered in service container using `builder.AddCosmosDb()` |
| `Steeltoe.Connector.CosmosDb.CosmosDbConnectorOptions` | Type | Steeltoe.Connector.Connector [Base/Core] | Removed | `CosmosDbOptions` | Provides connection string |
| `Steeltoe.Connector.CosmosDb.CosmosDbProviderConfigurer` | Type | Steeltoe.Connector.Connector [Base/Core] | Removed | None | Refactored to use ASP.NET Options pattern |
| `Steeltoe.Connector.CosmosDb.CosmosDbReadOnlyConnectionInfo` | Type | Steeltoe.Connector.Connector [Base/Core] | Removed | Use connection string in configuration | Refactored to use ASP.NET Options pattern |
| `Steeltoe.Connector.CosmosDb.CosmosDbTypeLocator` | Type | Steeltoe.Connector.Connector [Base/Core] | Removed | None | Type locators have been replaced with internal-only shims |
| `Steeltoe.Connector.Hystrix` | Namespace | Steeltoe.Connector.Connector [Base/Core] | Removed | None | Hystrix (circuit breaker) support was removed |
| `Steeltoe.Connector.IConfigurationExtensions` | Type | Steeltoe.Connector.Connector [Base/Core] | Removed | `ConnectorFactory<TOptions, TConnection>` | Redundant after refactorings |
| `Steeltoe.Connector.MongoDb.MongoDbConnectionInfo` | Type | Steeltoe.Connector.Connector [Base/Core] | Removed | Use connection string in configuration | Refactored to use ASP.NET Options pattern |
| `Steeltoe.Connector.MongoDb.MongoDbConnectorFactory` | Type | Steeltoe.Connector.Connector [Base/Core] | Removed | `ConnectorFactory<MongoDbOptions, IMongoClient>` | Registered in service container using `builder.AddMongoDb()` |
| `Steeltoe.Connector.MongoDb.MongoDbConnectorOptions` | Type | Steeltoe.Connector.Connector [Base/Core] | Removed | `MongoDbOptions` | Provides connection string |
| `Steeltoe.Connector.MongoDb.MongoDbHealthContributor` | Type | Steeltoe.Connector.Connector [Base/Core] | Removed | `ConnectorAddOptionsBuilder.EnableHealthChecks` | Made internal |
| `Steeltoe.Connector.MongoDb.MongoDbProviderConfigurer` | Type | Steeltoe.Connector.Connector [Base/Core] | Removed | None | Refactored to use ASP.NET Options pattern |
| `Steeltoe.Connector.MongoDb.MongoDbProviderServiceCollectionExtensions.AddMongoClient` | Extension method | Steeltoe.Connector.Connector [Base/Core] | Moved | `IHostApplicationBuilder.AddMongoDb()` | |
| `Steeltoe.Connector.MongoDb.MongoDbTypeLocator` | Type | Steeltoe.Connector.Connector [Base/Core] | Removed | None | Type locators have been replaced with internal-only shims |
| `Steeltoe.Connector.MySql.EF6` | Namespace | Steeltoe.Connector.Connector [Base/Core] | Removed | Use Steeltoe.Connectors.EntityFrameworkCore package | Entity Framework 6 is no longer being developed |
| `Steeltoe.Connector.MySql.MySqlConnectionInfo` | Type | Steeltoe.Connector.Connector [Base/Core] | Removed | Use connection string in configuration | Refactored to use ASP.NET Options pattern |
| `Steeltoe.Connector.MySql.MySqlProviderConfigurer` | Type | Steeltoe.Connector.Connector [Base/Core] | Removed | None | Refactored to use ASP.NET Options pattern |
| `Steeltoe.Connector.MySql.MySqlProviderConnectorFactory` | Type | Steeltoe.Connector.Connector [Base/Core] | Removed | `ConnectorFactory<MySqlOptions, MySqlConnection>` | Registered in service container using `builder.AddMySql()` |
| `Steeltoe.Connector.MySql.MySqlProviderConnectorOptions` | Type | Steeltoe.Connector.Connector [Base/Core] | Removed | `MySqlOptions` | Provides connection string |
| `Steeltoe.Connector.MySql.MySqlProviderServiceCollectionExtensions.AddMySqlConnection` | Extension method | Steeltoe.Connector.Connector [Base/Core] | Moved | `IHostApplicationBuilder.AddMySql()` | |
| `Steeltoe.Connector.MySql.MySqlServiceCollectionExtensions.AddMySqlHealthContributor` | Extension method | Steeltoe.Connector.Connector [Base/Core] | Removed | `ConnectorAddOptionsBuilder.EnableHealthChecks` | |
| `Steeltoe.Connector.MySql.MySqlTypeLocator` | Type | Steeltoe.Connector.Connector [Base/Core] | Removed | None | Type locators have been replaced with internal-only shims |
| `Steeltoe.Connector.OAuth` | Namespace | Steeltoe.Connector.Connector [Base/Core] | Removed | Use Steeltoe.Security.Authentication packages | Redundant after refactorings |
| `Steeltoe.Connector.Oracle` | Namespace | Steeltoe.Connector.Connector [Base/Core] | Removed | None | Support for Oracle databases was removed |
| `Steeltoe.Connector.PostgreSql.PostgresConnectionInfo` | Type | Steeltoe.Connector.Connector [Base/Core] | Removed | Use connection string in configuration | Refactored to use ASP.NET Options pattern |
| `Steeltoe.Connector.PostgreSql.PostgresProviderConfigurer` | Type | Steeltoe.Connector.Connector [Base/Core] | Removed | None | Refactored to use ASP.NET Options pattern |
| `Steeltoe.Connector.PostgreSql.PostgresProviderConnectorFactory` | Type | Steeltoe.Connector.Connector [Base/Core] | Removed | `ConnectorFactory<PostgreSqlOptions, NpgsqlConnection>` | Registered in service container using `builder.AddPostgreSql()` |
| `Steeltoe.Connector.PostgreSql.PostgresProviderConnectorOptions` | Type | Steeltoe.Connector.Connector [Base/Core] | Removed | `PostgreSqlOptions` | Provides connection string |
| `Steeltoe.Connector.PostgreSql.PostgresProviderServiceCollectionExtensions.AddPostgresConnection` | Extension method | Steeltoe.Connector.Connector [Base/Core] | Moved | `IHostApplicationBuilder.AddPostgreSql()` | |
| `Steeltoe.Connector.PostgreSql.PostgreSqlTypeLocator` | Type | Steeltoe.Connector.Connector [Base/Core] | Removed | None | Type locators have been replaced with internal-only shims |
| `Steeltoe.Connector.PostgreSql.PostgresServiceCollectionExtensions.AddPostgresHealthContributor` | Extension method | Steeltoe.Connector.Connector [Base/Core] | Removed | `ConnectorAddOptionsBuilder.EnableHealthChecks` | |
| `Steeltoe.Connector.RabbitMQ.RabbitMQConnectionInfo` | Type | Steeltoe.Connector.Connector [Base/Core] | Removed | Use connection string in configuration | Refactored to use ASP.NET Options pattern |
| `Steeltoe.Connector.RabbitMQ.RabbitMQHealthContributor` | Type | Steeltoe.Connector.Connector [Base/Core] | Removed | `ConnectorAddOptionsBuilder.EnableHealthChecks` | |
| `Steeltoe.Connector.RabbitMQ.RabbitMQProviderConfigurer` | Type | Steeltoe.Connector.Connector [Base/Core] | Removed | None | Refactored to use ASP.NET Options pattern |
| `Steeltoe.Connector.RabbitMQ.RabbitMQProviderConnectorFactory` | Type | Steeltoe.Connector.Connector [Base/Core] | Removed | `ConnectorFactory<RabbitMQOptions, IConnection>` | Registered in service container using `builder.AddRabbitMQ()` |
| `Steeltoe.Connector.RabbitMQ.RabbitMQProviderConnectorOptions` | Type | Steeltoe.Connector.Connector [Base/Core] | Removed | `RabbitMQOptions` | Provides connection string |
| `Steeltoe.Connector.RabbitMQ.RabbitMQProviderServiceCollectionExtensions.AddRabbitMQConnection` | Extension method | Steeltoe.Connector.Connector [Base/Core] | Moved | `IHostApplicationBuilder.AddRabbitMQ()` | |
| `Steeltoe.Connector.RabbitMQ.RabbitMQTypeLocator` | Type | Steeltoe.Connector.Connector [Base/Core] | Removed | None | Type locators have been replaced with internal-only shims |
| `Steeltoe.Connector.Redis.RedisCacheConfigurationExtensions.CreateRedisServiceConnectorFactory` | Extension method | Steeltoe.Connector.Connector [Base/Core] | Removed | `ConnectorFactory<RedisOptions, IConnectionMultiplexer>` | Registered in service container using `builder.AddRedis()` |
| `Steeltoe.Connector.Redis.RedisCacheConfigurer` | Type | Steeltoe.Connector.Connector [Base/Core] | Removed | None | Refactored to use ASP.NET Options pattern |
| `Steeltoe.Connector.Redis.RedisCacheConnectorOptions` | Type | Steeltoe.Connector.Connector [Base/Core] | Removed | `RedisOptions` | Provides connection string |
| `Steeltoe.Connector.Redis.RedisCacheServiceCollectionExtensions.AddDistributedRedisCache` | Extension method | Steeltoe.Connector.Connector [Base/Core] | Moved | `IHostApplicationBuilder.AddRedis()` | Auto-adds `ConnectionMultiplexer` if Microsoft.Extensions.Caching.StackExchangeRedis package is referenced |
| `Steeltoe.Connector.Redis.RedisCacheServiceCollectionExtensions.AddRedisConnectionMultiplexer` | Extension method | Steeltoe.Connector.Connector [Base/Core] | Moved | `IHostApplicationBuilder.AddRedis()` | Auto-adds `ConnectionMultiplexer` if Microsoft.Extensions.Caching.StackExchangeRedis package is referenced |
| `Steeltoe.Connector.Redis.RedisConnectionInfo` | Type | Steeltoe.Connector.Connector [Base/Core] | Removed | Use connection string in configuration | Refactored to use ASP.NET Options pattern |
| `Steeltoe.Connector.Redis.RedisHealthContributor` | Type | Steeltoe.Connector.Connector [Base/Core] | Removed | `ConnectorAddOptionsBuilder.EnableHealthChecks` | |
| `Steeltoe.Connector.Redis.RedisServiceConnectorFactory` | Type | Steeltoe.Connector.Connector [Base/Core] | Removed | `ConnectorFactory<RedisOptions, IConnectionMultiplexer>` | Registered in service container using `builder.AddRedis()` |
| `Steeltoe.Connector.Redis.RedisTypeLocator` | Type | Steeltoe.Connector.Connector [Base/Core] | Removed | None | Type locators have been replaced with internal-only shims |
| `Steeltoe.Connector.RelationalDbHealthContributor` | Type | Steeltoe.Connector.Connector [Base/Core] | Removed | None | Made internal, renamed to `RelationalDatabaseHealthContributor` |
| `Steeltoe.Connector.ServiceInfoCreator` | Type | Steeltoe.Connector.Connector [Base/Core] | Removed | `ConnectorFactory<TOptions, TConnection>` | Loading connectors from assemblies was dropped |
| `Steeltoe.Connector.Services` | Namespace | Steeltoe.Connector.Connector [Base/Core] | Removed | `ConnectorFactory<TOptions, TConnection>` | Loading connectors from assemblies was dropped |
| `Steeltoe.Connector.Services.SsoServiceInfoFactory` | Type | Steeltoe.Connector.Connector [Base/Core] | Removed | Use Steeltoe.Security.Authentication packages | Redundant after refactorings |
| `Steeltoe.Connector.SqlServer.EF6` | Namespace | Steeltoe.Connector.Connector [Base/Core] | Removed | Use Steeltoe.Connectors.EntityFrameworkCore package | Entity Framework 6 is no longer being developed |
| `Steeltoe.Connector.SqlServer.SqlServerConnectionInfo` | Type | Steeltoe.Connector.Connector [Base/Core] | Removed | Use connection string in configuration | Refactored to use ASP.NET Options pattern |
| `Steeltoe.Connector.SqlServer.SqlServerProviderConfigurer` | Type | Steeltoe.Connector.Connector [Base/Core] | Removed | None | Refactored to use ASP.NET Options pattern |
| `Steeltoe.Connector.SqlServer.SqlServerProviderConnectorFactory` | Type | Steeltoe.Connector.Connector [Base/Core] | Removed | `ConnectorFactory<SqlServerOptions, SqlConnection>` | Registered in service container using `builder.AddSqlServer()` |
| `Steeltoe.Connector.SqlServer.SqlServerProviderConnectorOptions` | Type | Steeltoe.Connector.Connector [Base/Core] | Removed | `SqlServerOptions` | Provides connection string |
| `Steeltoe.Connector.SqlServer.SqlServerProviderServiceCollectionExtensions.AddSqlServerConnection` | Extension method | Steeltoe.Connector.Connector [Base/Core] | Moved | `IHostApplicationBuilder.AddSqlServer()` | |
| `Steeltoe.Connector.SqlServer.SqlServerServiceCollectionExtensions.AddSqlServerHealthContributor` | Extension method | Steeltoe.Connector.Connector [Base/Core] | Removed | `ConnectorAddOptionsBuilder.EnableHealthChecks` | |
| `Steeltoe.Connector.SqlServer.SqlServerTypeLocator` | Type | Steeltoe.Connector.Connector [Base/Core] | Removed | None | Type locators have been replaced with internal-only shims |
| `Steeltoe.Connectors.ConnectionStringOptions` | Type | Steeltoe.Connectors | Added | | Base type for driver-specific connection string |
| `Steeltoe.Connectors.Connector<TOptions, TConnection>` | Type | Steeltoe.Connectors | Added | | Returned by `ConnectorFactory<TOptions, TConnection>` |
| `Steeltoe.Connectors.ConnectorAddOptionsBuilder` | Type | Steeltoe.Connectors | Added | | Connector configuration settings |
| `Steeltoe.Connectors.ConnectorConfigureOptionsBuilder` | Type | Steeltoe.Connectors | Added | | Connector configuration settings |
| `Steeltoe.Connectors.ConnectorCreateConnection` | Type | Steeltoe.Connectors | Added | | Delegate to customize connecting |
| `Steeltoe.Connectors.ConnectorCreateHealthContributor` | Type | Steeltoe.Connectors | Added | | Delegate to create health contributor |
| `Steeltoe.Connectors.ConnectorFactory<TOptions, TConnection>` | Type | Steeltoe.Connectors | Added | | Injectable, access connection string or driver-specific connection |
| `Steeltoe.Connectors.CosmosDb.CosmosDbConfigurationBuilderExtensions.ConfigureCosmosDb` | Extension method | Steeltoe.Connectors | Added | | To support legacy host builders |
| `Steeltoe.Connectors.CosmosDb.CosmosDbHostApplicationBuilderExtensions.AddCosmosDb` | Extension method | Steeltoe.Connectors | Added | | Activates the CosmosDB connector |
| `Steeltoe.Connectors.CosmosDb.CosmosDbOptions` | Type | Steeltoe.Connectors | Added | | Provides connection string and database name |
| `Steeltoe.Connectors.CosmosDb.CosmosDbServiceCollectionExtensions.AddCosmosDb` | Extension method | Steeltoe.Connectors | Added | | To support legacy host builders |
| `Steeltoe.Connectors.MongoDb.MongoDbConfigurationBuilderExtensions.ConfigureMongoDb` | Extension method | Steeltoe.Connectors | Added | | To support legacy host builders |
| `Steeltoe.Connectors.MongoDb.MongoDbHostApplicationBuilderExtensions.AddMongoDb` | Extension method | Steeltoe.Connectors | Added | | Activates the MongoDB connector |
| `Steeltoe.Connectors.MongoDb.MongoDbOptions` | Type | Steeltoe.Connectors | Added | | Provides connection string and database name |
| `Steeltoe.Connectors.MongoDb.MongoDbServiceCollectionExtensions.AddMongoDb` | Extension method | Steeltoe.Connectors | Added | | To support legacy host builders |
| `Steeltoe.Connectors.MySql.MySqlConfigurationBuilderExtensions.ConfigureMySql` | Extension method | Steeltoe.Connectors | Added | | To support legacy host builders |
| `Steeltoe.Connectors.MySql.MySqlHostApplicationBuilderExtensions.AddMySql` | Extension method | Steeltoe.Connectors | Added | | Activates the MySQL connector |
| `Steeltoe.Connectors.MySql.MySqlOptions` | Type | Steeltoe.Connectors | Added | | Provides connection string |
| `Steeltoe.Connectors.MySql.MySqlServiceCollectionExtensions.AddMySql` | Extension method | Steeltoe.Connectors | Added | | To support legacy host builders |
| `Steeltoe.Connectors.PostgreSql.PostgreSqlConfigurationBuilderExtensions.ConfigurePostgreSql` | Extension method | Steeltoe.Connectors | Added | | To support legacy host builders |
| `Steeltoe.Connectors.PostgreSql.PostgreSqlHostApplicationBuilderExtensions.AddPostgreSql` | Extension method | Steeltoe.Connectors | Added | | Activates the PostgreSQL connector |
| `Steeltoe.Connectors.PostgreSql.PostgreSqlOptions` | Type | Steeltoe.Connectors | Added | | Provides connection string |
| `Steeltoe.Connectors.PostgreSql.PostgreSqlServiceCollectionExtensions.AddPostgreSql` | Extension method | Steeltoe.Connectors | Added | | To support legacy host builders |
| `Steeltoe.Connectors.RabbitMQ.RabbitMQConfigurationBuilderExtensions.ConfigureRabbitMQ` | Extension method | Steeltoe.Connectors | Added | | To support legacy host builders |
| `Steeltoe.Connectors.RabbitMQ.RabbitMQHostApplicationBuilderExtensions.AddRabbitMQ` | Extension method | Steeltoe.Connectors | Added | | Activates the RabbitMQ connector |
| `Steeltoe.Connectors.RabbitMQ.RabbitMQOptions` | Type | Steeltoe.Connectors | Added | | Provides connection string |
| `Steeltoe.Connectors.RabbitMQ.RabbitMQServiceCollectionExtensions.AddRabbitMQ` | Extension method | Steeltoe.Connectors | Added | | To support legacy host builders |
| `Steeltoe.Connectors.Redis.RedisConfigurationBuilderExtensions.ConfigureRedis` | Extension method | Steeltoe.Connectors | Added | | To support legacy host builders |
| `Steeltoe.Connectors.Redis.RedisHostApplicationBuilderExtensions.AddRedis` | Extension method | Steeltoe.Connectors | Added | | Activates the Redis/Valkey connector |
| `Steeltoe.Connectors.Redis.RedisOptions` | Type | Steeltoe.Connectors | Added | | Provides connection string |
| `Steeltoe.Connectors.Redis.RedisServiceCollectionExtensions.AddRedis` | Extension method | Steeltoe.Connectors | Added | | To support legacy host builders |
| `Steeltoe.Connectors.SqlServer.SqlServerConfigurationBuilderExtensions.ConfigureSqlServer` | Extension method | Steeltoe.Connectors | Added | | To support legacy host builders |
| `Steeltoe.Connectors.SqlServer.SqlServerHostApplicationBuilderExtensions.AddSqlServer` | Extension method | Steeltoe.Connectors | Added | | Activates the Microsoft SQL Server connector |
| `Steeltoe.Connectors.SqlServer.SqlServerOptions` | Type | Steeltoe.Connectors | Added | | Provides connection string |
| `Steeltoe.Connectors.SqlServer.SqlServerServiceCollectionExtensions.AddSqlServer` | Extension method | Steeltoe.Connectors | Added | | To support legacy host builders |
| `Steeltoe.Connector.EFCore.EntityFrameworkCoreTypeLocator` | Type | Steeltoe.Connector.EFCore | Removed | | Type locators have been replaced with internal-only shims |
| `Steeltoe.Connector.MySql.EFCore.MySqlDbContextOptionsExtensions.UseMySql` | Extension method | Steeltoe.Connector.EFCore | Moved | `MySqlDbContextOptionsBuilderExtensions.UseMySql` | Takes an `IServiceProvider`, requires call to `builder.AddMySql()` first |
| `Steeltoe.Connector.MySql.EFCore.MySqlDbContextOptionsExtensions.UseMySql<TContext>` | Extension method | Steeltoe.Connector.EFCore | Removed | | Redundant |
| `Steeltoe.Connector.Oracle.EFCore.OracleDbContextOptionsExtensions.UseOracle` | Extension method | Steeltoe.Connector.EFCore | Removed | | Support for Oracle databases was removed |
| `Steeltoe.Connector.PostgreSql.EFCore.PostgresDbContextOptionsExtensions.FindUseNpgsqlMethod` | Extension method | Steeltoe.Connector.EFCore | Removed | | Redundant after refactorings |
| `Steeltoe.Connector.PostgreSql.EFCore.PostgresDbContextOptionsExtensions.UseNpgsql` | Extension method | Steeltoe.Connector.EFCore | Moved | `PostgreSqlDbContextOptionsBuilderExtensions.UseNpgsql` | Takes an `IServiceProvider`, requires call to `builder.AddPostgreSql()` first |
| `Steeltoe.Connector.PostgreSql.EFCore.PostgresDbContextOptionsExtensions.UseNpgsql<TContext>` | Extension method | Steeltoe.Connector.EFCore | Removed | | Redundant |
| `Steeltoe.Connector.SqlServer.EFCore.SqlServerDbContextOptionsExtensions.UseSqlServer` | Extension method | Steeltoe.Connector.EFCore | Moved | `SqlServerDbContextOptionsBuilderExtensions.UseSqlServer` | Takes an `IServiceProvider`, requires call to `builder.AddSqlServer()` first |
| `Steeltoe.Connector.SqlServer.EFCore.SqlServerDbContextOptionsExtensions.UseSqlServer<TContext>` | Extension method | Steeltoe.Connector.EFCore | Removed | | Redundant |

### Notable PRs

- https://github.com/SteeltoeOSS/Steeltoe/pull/1528
- https://github.com/SteeltoeOSS/Steeltoe/pull/1325
- https://github.com/SteeltoeOSS/Steeltoe/pull/1172
- https://github.com/SteeltoeOSS/Steeltoe/pull/1143
- https://github.com/SteeltoeOSS/Steeltoe/pull/1139
- https://github.com/SteeltoeOSS/Steeltoe/pull/1131
- https://github.com/SteeltoeOSS/Steeltoe/pull/1128
- https://github.com/SteeltoeOSS/Steeltoe/pull/1124
- https://github.com/SteeltoeOSS/Steeltoe/pull/1121
- https://github.com/SteeltoeOSS/Steeltoe/pull/1119
- https://github.com/SteeltoeOSS/Steeltoe/pull/1117
- https://github.com/SteeltoeOSS/Steeltoe/pull/1112
- https://github.com/SteeltoeOSS/Steeltoe/pull/1110
- https://github.com/SteeltoeOSS/Steeltoe/pull/1089

### Documentation

For more information, see the updated [Connectors documentation](../configuration/index.md) and
[Configuration samples](https://github.com/SteeltoeOSS/Samples/tree/main/Connectors).

---

## Discovery

### Behavior changes

- Multiple discovery clients (one per type) can be active
- Simplified API: Use `IServiceCollection.Add*DiscoveryClient()` to register; `IHttpClientBuilder.AddServiceDiscovery()` to consume
- Refactored configuration to use ASP.NET Options pattern, responding to changes at runtime nearly everywhere
- Async all-the-way where possible
- More reliable cross-platform detection of local hostname and IP address
- Now supports global service discovery: `services.ConfigureHttpClientDefaults(builder => builder.AddServiceDiscovery())`
- Config Server discovery-first can now query Consul, Eureka, and Configuration-based
- Improved detection of port bindings, including support for new ASP.NET [environment variables](https://learn.microsoft.com/aspnet/core/release-notes/aspnetcore-8.0#http_ports-and-https_ports-config-keys)
- HTTP handlers now depend on a load balancer, which delegates to multiple discovery clients
- Major improvements in documentation, samples cover more use cases
- Eureka: API reduced to what Steeltoe needs for service discovery (too many unanswered questions to provide generic client)
- Eureka: Added support for ASP.NET dynamic port bindings
- Eureka: Optimized communication to Eureka server
- Eureka: Made resilient to concurrent changes, fixing race conditions and lost updates
- Eureka: Made types returned from server immutable; use `EurekaApplicationInfoManager.UpdateInstance()` to change local instance
- Eureka: Server communication via `HttpClientFactory` (allows custom handlers, telemetry, respond to DNS changes, etc)
- Eureka: Client certificate watched in `IConfiguration`, can be shared with other `HttpClient`s
- Eureka: Health handler (to determine local instance status) now runs both contributors and ASP.NET health checks
- Eureka: Prefer secure port when both secure/non-secure are returned from Eureka
- Eureka: Support comma-separated list of multiple names in setting `Eureka:Instance:VipAddress` and `Eureka:Instance:SecureVipAddress`
- Eureka: Improved handling of the various registration methods on Cloud Foundry

### NuGet Package changes

| Source | Change | Replacement | Notes |
| --- | --- | --- | --- |
| Steeltoe.Discovery.Abstractions | Removed | Steeltoe.Common package | No longer needed (except for `IDiscoveryClient`, which moved to Steeltoe.Common package) |
| Steeltoe.Discovery.ClientBase | Removed | Steeltoe.Discovery.HttpClients, Steeltoe.Discovery.Configuration packages | Configuration-based discovery moved to Steeltoe.Discovery.Configuration package |
| Steeltoe.Discovery.ClientCore | Removed | Steeltoe.Discovery.HttpClients package | |
| Steeltoe.Discovery.Configuration | Added | | Provides a configuration-based discovery client |
| Steeltoe.Discovery.HttpClients | Added | | Provides consumption of `IDiscoveryClient`(s) in `HttpClient`/`HttpClientFactory` pipeline |
| Steeltoe.Discovery.Kubernetes | Removed | None | |

### API changes

| Source | Kind | Package | Change | Replacement | Notes |
| --- | --- | --- | --- | --- | --- |
| `Steeltoe.Discovery.Client.ConfigurationUrlHelpers` | Type | Steeltoe.Discovery.Client [Base/Core] | Removed | None | Similar logic moved to internal `ConfigurationExtensions.GetListenAddresses` |
| `Steeltoe.Discovery.Client.DiscoveryApplicationBuilderExtensions.UseDiscoveryClient` | Extension method | Steeltoe.Discovery.Client [Base/Core] | Removed | Call `IServiceCollection.Add\*DiscoveryClient()` extension method | |
| `Steeltoe.Discovery.Client.DiscoveryClientBuilder` | Type | Steeltoe.Discovery.Client [Base/Core] | Removed | Call `IServiceCollection.Add\*DiscoveryClient()` extension method | Refactored to simpler API |
| `Steeltoe.Discovery.Client.DiscoveryClientStartupFilter` | Type | Steeltoe.Discovery.Client [Base/Core] | Removed | None | No longer needed |
| `Steeltoe.Discovery.Client.DiscoveryHostBuilderExtensions.AddDiscoveryClient` | Extension method | Steeltoe.Discovery.Client [Base/Core] | Removed | Call `IServiceCollection.Add\*DiscoveryClient()` extension method | Refactored to simpler API |
| `Steeltoe.Discovery.Client.DiscoveryHostBuilderExtensions.AddServiceDiscovery` | Extension method | Steeltoe.Discovery.Client [Base/Core] | Removed | Call `IHttpClientBuilder.AddServiceDiscovery()` extension method | Refactored to simpler API |
| `Steeltoe.Discovery.Client.DiscoveryServiceCollectionExtensions.AddDiscoveryClient` | Extension method | Steeltoe.Discovery.Client [Base/Core] | Removed | Call `IServiceCollection.Add\*DiscoveryClient()` extension method | Refactored to simpler API |
| `Steeltoe.Discovery.Client.DiscoveryServiceCollectionExtensions.AddServiceDiscovery` | Extension method | Steeltoe.Discovery.Client [Base/Core] | Removed | Call `IHttpClientBuilder.AddServiceDiscovery()` extension method | Refactored to simpler API |
| `Steeltoe.Discovery.Client.DiscoveryServiceCollectionExtensions.ApplicationLifecycle` | Type | Steeltoe.Discovery.Client [Base/Core] | Removed | None | No longer needed |
| `Steeltoe.Discovery.Client.DiscoveryServiceCollectionExtensions.GetNamedDiscoveryServiceInfo` | Extension method | Steeltoe.Discovery.Client [Base/Core] | Removed | Inject `IEnumerable<IDiscoveryClient>` | Refactored to simpler API |
| `Steeltoe.Discovery.Client.DiscoveryServiceCollectionExtensions.GetSingletonDiscoveryServiceInfo` | Extension method | Steeltoe.Discovery.Client [Base/Core] | Removed | Inject `IEnumerable<IDiscoveryClient>` | Refactored to simpler API |
| `Steeltoe.Discovery.Client.DiscoveryWebApplicationBuilderExtensions.AddDiscoveryClient` | Extension method | Steeltoe.Discovery.Client [Base/Core] | Removed | Call `IServiceCollection.Add\*DiscoveryClient()` extension method | Refactored to simpler API |
| `Steeltoe.Discovery.Client.DiscoveryWebApplicationBuilderExtensions.AddServiceDiscovery` | Extension method | Steeltoe.Discovery.Client [Base/Core] | Removed | Call `IHttpClientBuilder.AddServiceDiscovery()` extension method | Refactored to simpler API |
| `Steeltoe.Discovery.Client.DiscoveryWebHostBuilderExtensions.AddDiscoveryClient` | Extension method | Steeltoe.Discovery.Client [Base/Core] | Removed | Call `IServiceCollection.Add\*DiscoveryClient()` extension method | Refactored to simpler API |
| `Steeltoe.Discovery.Client.DiscoveryWebHostBuilderExtensions.AddServiceDiscovery` | Extension method | Steeltoe.Discovery.Client [Base/Core] | Removed | Call `IHttpClientBuilder.AddServiceDiscovery()` extension method | Refactored to simpler API |
| `Steeltoe.Discovery.Client.IDiscoveryClientExtension` | Type | Steeltoe.Discovery.Client [Base/Core] | Removed | Implement `IDiscoveryClient` directly, add it to service container | Refactored to simpler API |
| `Steeltoe.Discovery.Client.SimpleClients.ConfigurationDiscoveryClient` | Type | Steeltoe.Discovery.Client [Base/Core] | Moved | `ConfigurationDiscoveryClient` in Steeltoe.Discovery.Configuration package | |
| `Steeltoe.Discovery.Client.SimpleClients.ConfigurationDiscoveryClientBuilderExtensions.UseConfiguredInstances` | Extension method | Steeltoe.Discovery.Client [Base/Core] | Removed | Call `IServiceCollection.AddConfigurationDiscoveryClient()` extension method | Refactored to simpler API |
| `Steeltoe.Discovery.Client.SimpleClients.ConfigurationDiscoveryClientExtension` | Type | Steeltoe.Discovery.Client [Base/Core] | Removed | Call `IServiceCollection.AddConfigurationDiscoveryClient()` extension method | Refactored to simpler API |
| `Steeltoe.Discovery.DiscoveryClientAssemblyAttribute` | Type | Steeltoe.Discovery.Client [Base/Core] | Removed | None | Dynamically loading custom discovery clients is no longer possible |
| `Steeltoe.Discovery.Configuration.ConfigurationDiscoveryClient` | Type | Steeltoe.Discovery.Configuration | Added | | Reads service instances from configuration |
| `Steeltoe.Discovery.Configuration.ConfigurationDiscoveryOptions` | Type | Steeltoe.Discovery.Configuration | Added | | Options type for service instances in `IConfiguration` |
| `Steeltoe.Discovery.Configuration.ConfigurationServiceCollectionExtensions.AddConfigurationDiscoveryClient` | Extension method | Steeltoe.Discovery.Configuration | Added | | Activates configuration-based discovery client |
| `Steeltoe.Discovery.Configuration.ConfigurationServiceInstance` | Type | Steeltoe.Discovery.Configuration | Added | | Implements `IServiceInstance` for configuration-based discovery |
| `Steeltoe.Discovery.HttpClients.DiscoveryHttpClientBuilderExtensions.AddServiceDiscovery` | Extension method | Steeltoe.Discovery.HttpClients | Added | | Activates service discovery using the randomized load balancer |
| `Steeltoe.Discovery.HttpClients.DiscoveryHttpClientBuilderExtensions.AddServiceDiscovery<T>` | Extension method | Steeltoe.Discovery.HttpClients | Added | | Activates service discovery using a custom ILoadBalancer |
| `Steeltoe.Discovery.HttpClients.DiscoveryHttpClientHandler` | Type | Steeltoe.Discovery.HttpClients | Added | | An `HttpClientHandler` (for `HttpClient`) that load-balances over service instances |
| `Steeltoe.Discovery.HttpClients.DiscoveryHttpDelegatingHandler` | Type | Steeltoe.Discovery.HttpClients | Added | | An `DelegatingHandler` (for `IHttpClientFactory`) that load-balances over service instances |
| `Steeltoe.Discovery.HttpClients.LoadBalancers.ILoadBalancer` | Type | Steeltoe.Discovery.HttpClients | Added | | Chooses a service instance obtained from discovery client(s) |
| `Steeltoe.Discovery.HttpClients.LoadBalancers.RandomLoadBalancer` | Type | Steeltoe.Discovery.HttpClients | Added | | Chooses a random service instance obtained from discovery client(s) |
| `Steeltoe.Discovery.HttpClients.LoadBalancers.RoundRobinLoadBalancer` | Type | Steeltoe.Discovery.HttpClients | Added | | Chooses a service instance obtained from discovery client(s) using round robin |
| `Steeltoe.Discovery.HttpClients.LoadBalancers.ServiceInstancesResolver` | Type | Steeltoe.Discovery.HttpClients | Added | | Used by load balancers to retrieve service instances from discovery clients |
| `Steeltoe.Discovery.Consul.ConsulClientFactory` | Type | Steeltoe.Discovery.Consul | Removed | Use Consul package directly | No longer needed |
| `Steeltoe.Discovery.Consul.ConsulDiscoveryClientBuilderExtensions.UseConsul` | Extension method | Steeltoe.Discovery.Consul | Removed | Call `IServiceCollection.AddConsulDiscoveryClient()` extension method | |
| `Steeltoe.Discovery.Consul.ConsulDiscoveryClientExtension` | Type | Steeltoe.Discovery.Consul | Removed | Call `IServiceCollection.AddConsulDiscoveryClient()` extension method | |
| `Steeltoe.Discovery.Consul.ConsulOptions` | Type | Steeltoe.Discovery.Consul | Moved | `Steeltoe.Discovery.Consul.Configuration.ConsulOptions` | |
| `Steeltoe.Discovery.Consul.ConsulPostConfigurer` | Type | Steeltoe.Discovery.Consul | Removed | None | Moved to internal type `PostConfigureConsulDiscoveryOptions` |
| `Steeltoe.Discovery.Consul.ConsulServiceCollectionExtensions.AddConsulDiscoveryClient` | Extension method | Steeltoe.Discovery.Consul | Added | | Activates the Consul discovery client |
| `Steeltoe.Discovery.Consul.Discovery.ConsulDiscoveryClient.Dispose` | Method | Steeltoe.Discovery.Consul | Removed | Call `ShutdownAsync()` before `IServiceProvider` is disposed | `ShutdownAsync()` is called by internal `DiscoveryClientHostedService` |
| `Steeltoe.Discovery.Consul.Discovery.ConsulDiscoveryClient.EnsureAssemblyIsLoaded` | Method | Steeltoe.Discovery.Consul | Removed | None | No longer needed |
| `Steeltoe.Discovery.Consul.Discovery.ConsulDiscoveryOptions` | Type | Steeltoe.Discovery.Consul | Moved | `Steeltoe.Discovery.Consul.Configuration.ConsulDiscoveryOptions` | |
| `Steeltoe.Discovery.Consul.Discovery.ConsulDiscoveryOptions.ApplyConfigUrls` | Method | Steeltoe.Discovery.Consul | Removed | None | Refactored to internal logic, happens automatically based on settings |
| `Steeltoe.Discovery.Consul.Discovery.ConsulDiscoveryOptions.ApplyNetUtils` | Method | Steeltoe.Discovery.Consul | Removed | None | Refactored to internal logic, happens automatically based on settings |
| `Steeltoe.Discovery.Consul.Discovery.ConsulDiscoveryOptions.CacheTTL` | Property | Steeltoe.Discovery.Consul | Removed | Use caching provided by `ServiceInstancesResolver` | |
| `Steeltoe.Discovery.Consul.Discovery.ConsulDiscoveryOptions.IpAddress` | Property | Steeltoe.Discovery.Consul | Renamed | `ConsulDiscoveryOptions.IPAddress` | |
| `Steeltoe.Discovery.Consul.Discovery.ConsulDiscoveryOptions.IsHeartBeatEnabled` | Property | Steeltoe.Discovery.Consul | Removed | `ConsulDiscoveryOptions.Heartbeat.Enabled` | |
| `Steeltoe.Discovery.Consul.Discovery.ConsulDiscoveryOptions.IsRetryEnabled` | Property | Steeltoe.Discovery.Consul | Removed | `ConsulDiscoveryOptions.Retry.Enabled` | |
| `Steeltoe.Discovery.Consul.Discovery.ConsulDiscoveryOptions.NetUtils` | Property | Steeltoe.Discovery.Consul | Removed | None | OS-based network APIs are no longer pluggable |
| `Steeltoe.Discovery.Consul.Discovery.ConsulDiscoveryOptions.PreferAgentAddress` | Property | Steeltoe.Discovery.Consul | Removed | None | Undocumented and did not work |
| `Steeltoe.Discovery.Consul.Discovery.ConsulDiscoveryOptions.PreferIpAddress` | Property | Steeltoe.Discovery.Consul | Renamed | `ConsulDiscoveryOptions.PreferIPAddress` | |
| `Steeltoe.Discovery.Consul.Discovery.ConsulDiscoveryOptions.TagsAsMetadata` | Property | Steeltoe.Discovery.Consul | Removed | Configure `Tags` and `Metadata` directly | Redundant |
| `Steeltoe.Discovery.Consul.Discovery.ConsulDiscoveryOptions.UseNetUtils` | Property | Steeltoe.Discovery.Consul | Renamed | `ConsulDiscoveryOptions.UseNetworkInterfaces` | |
| `Steeltoe.Discovery.Consul.Discovery.ConsulHealthContributor` | Type | Steeltoe.Discovery.Consul | Removed | None | Made internal, not designed for reuse/extensibility |
| `Steeltoe.Discovery.Consul.Discovery.ConsulHeartbeatOptions` | Type | Steeltoe.Discovery.Consul | Moved | `Steeltoe.Discovery.Consul.Configuration.ConsulHeartbeatOptions` | |
| `Steeltoe.Discovery.Consul.Discovery.ConsulRetryOptions` | Type | Steeltoe.Discovery.Consul | Moved | `Steeltoe.Discovery.Consul.Configuration.ConsulRetryOptions` | |
| `Steeltoe.Discovery.Consul.Discovery.ConsulServiceInstance` | Type | Steeltoe.Discovery.Consul | Removed | None | Made internal |
| `Steeltoe.Discovery.Consul.Discovery.IConsulDiscoveryClient` | Type | Steeltoe.Discovery.Consul | Removed | `Steeltoe.Discovery.Consul.ConsulDiscoveryClient` | |
| `Steeltoe.Discovery.Consul.Discovery.IScheduler` | Type | Steeltoe.Discovery.Consul | Removed | None | Custom implementations are no longer possible |
| `Steeltoe.Discovery.Consul.Discovery.TtlScheduler` | Type | Steeltoe.Discovery.Consul | Removed | None | Made internal |
| `Steeltoe.Discovery.Consul.Registry.ConsulRegistration` | Type | Steeltoe.Discovery.Consul | Removed | None | Made internal |
| `Steeltoe.Discovery.Consul.Registry.ConsulServiceRegistrar` | Type | Steeltoe.Discovery.Consul | Removed | Use Consul package directly | Made internal |
| `Steeltoe.Discovery.Consul.Registry.ConsulServiceRegistry` | Type | Steeltoe.Discovery.Consul | Removed | Use Consul package directly | Made internal |
| `Steeltoe.Discovery.Consul.Registry.IConsulRegistration` | Type | Steeltoe.Discovery.Consul | Removed | None | Custom implementations are no longer possible |
| `Steeltoe.Discovery.Consul.Registry.IConsulServiceRegistrar` | Type | Steeltoe.Discovery.Consul | Removed | None | Custom implementations are no longer possible |
| `Steeltoe.Discovery.Consul.Registry.IConsulServiceRegistry` | Type | Steeltoe.Discovery.Consul | Removed | None | Custom implementations are no longer possible |
| `Steeltoe.Discovery.Consul.Registry.IServiceRegistrar` | Type | Steeltoe.Discovery.Consul | Removed | None | Custom implementations are no longer possible |
| `Steeltoe.Discovery.Consul.Util.ConsulServerUtils` | Type | Steeltoe.Discovery.Consul | Removed | None | Made internal |
| `Steeltoe.Discovery.Consul.Util.DateTimeConversions` | Type | Steeltoe.Discovery.Consul | Removed | None | Made internal |
| `Steeltoe.Discovery.Eureka.AppInfo.Application` | Type | Steeltoe.Discovery.Eureka | Renamed | `Steeltoe.Discovery.Eureka.AppInfo.ApplicationInfo` | |
| `Steeltoe.Discovery.Eureka.AppInfo.Application.Count` | Property | Steeltoe.Discovery.Eureka | Removed | `ApplicationInfo.Instances.Count` | Redundant |
| `Steeltoe.Discovery.Eureka.AppInfo.Application.GetInstance` | Method | Steeltoe.Discovery.Eureka | Removed | Find entry in `ApplicationInfo.Instances` | Made internal |
| `Steeltoe.Discovery.Eureka.AppInfo.Applications` | Type | Steeltoe.Discovery.Eureka | Renamed | `Steeltoe.Discovery.Eureka.AppInfo.ApplicationInfoCollection` | Now implements `IReadOnlyCollection<ApplicationInfo>` |
| `Steeltoe.Discovery.Eureka.AppInfo.Applications.GetInstancesBySecureVirtualHostName` | Method | Steeltoe.Discovery.Eureka | Removed | Enumerate via collection interface | Renamed to internal `GetInstancesBySecureVipAddress` |
| `Steeltoe.Discovery.Eureka.AppInfo.Applications.GetInstancesByVirtualHostName` | Method | Steeltoe.Discovery.Eureka | Removed | Enumerate via collection interface | Renamed to internal `GetInstancesByVipAddress` |
| `Steeltoe.Discovery.Eureka.AppInfo.Applications.GetRegisteredApplication` | Method | Steeltoe.Discovery.Eureka | Removed | Enumerate via collection interface | Made internal |
| `Steeltoe.Discovery.Eureka.AppInfo.Applications.GetRegisteredApplications` | Method | Steeltoe.Discovery.Eureka | Removed | Enumerate via collection interface | Redundant |
| `Steeltoe.Discovery.Eureka.AppInfo.DataCenterInfo` | Type | Steeltoe.Discovery.Eureka | Moved | `Steeltoe.Discovery.Eureka.Configuration.DataCenterInfo` | |
| `Steeltoe.Discovery.Eureka.AppInfo.IDataCenterInfo` | Type | Steeltoe.Discovery.Eureka | Removed | `Steeltoe.Discovery.Eureka.Configuration.DataCenterInfo` | |
| `Steeltoe.Discovery.Eureka.AppInfo.InstanceInfo.Actiontype` | Property | Steeltoe.Discovery.Eureka | Renamed | `InstanceInfo.ActionType` | |
| `Steeltoe.Discovery.Eureka.AppInfo.InstanceInfo.AsgName` | Property | Steeltoe.Discovery.Eureka | Renamed | `InstanceInfo.AutoScalingGroupName` | |
| `Steeltoe.Discovery.Eureka.AppInfo.InstanceInfo.EffectiveStatus` | Property | Steeltoe.Discovery.Eureka | Added | | Calculated based on `Status` and `OverriddenStatus` |
| `Steeltoe.Discovery.Eureka.AppInfo.InstanceInfo.IpAddr` | Property | Steeltoe.Discovery.Eureka | Renamed | `InstanceInfo.IPAddress` | |
| `Steeltoe.Discovery.Eureka.AppInfo.InstanceInfo.IsUnsecurePortEnabled` | Property | Steeltoe.Discovery.Eureka | Renamed | `InstanceInfo.IsNonSecurePortEnabled` | |
| `Steeltoe.Discovery.Eureka.AppInfo.InstanceInfo.LastDirtyTimestamp` | Property | Steeltoe.Discovery.Eureka | Renamed | `InstanceInfo.LastDirtyTimeUtc` | Changed type from `long` to `DateTime` |
| `Steeltoe.Discovery.Eureka.AppInfo.InstanceInfo.LastUpdatedTimestamp` | Property | Steeltoe.Discovery.Eureka | Renamed | `InstanceInfo.LastUpdatedTimeUtc` | Changed type from `long` to `DateTime` |
| `Steeltoe.Discovery.Eureka.AppInfo.InstanceInfo.Port` | Property | Steeltoe.Discovery.Eureka | Renamed | `InstanceInfo.NonSecurePort` | |
| `Steeltoe.Discovery.Eureka.AppInfo.InstanceStatus` | Type | Steeltoe.Discovery.Eureka | Members changed | | Reordered enum members (0 = Unknown) |
| `Steeltoe.Discovery.Eureka.AppInfo.LeaseInfo.DurationInSecs` | Property | Steeltoe.Discovery.Eureka | Renamed | `LeaseInfo.Duration` | Changed type from `int` to `TimeSpan` |
| `Steeltoe.Discovery.Eureka.AppInfo.LeaseInfo.EvictionTimestamp` | Property | Steeltoe.Discovery.Eureka | Renamed | `LeaseInfo.EvictionTimeUtc` | Changed type from `long` to `DateTime` |
| `Steeltoe.Discovery.Eureka.AppInfo.LeaseInfo.LastRenewalTimestamp` | Property | Steeltoe.Discovery.Eureka | Renamed | `LeaseInfo.LastRenewalTimeUtc` | Changed type from `long` to `DateTime` |
| `Steeltoe.Discovery.Eureka.AppInfo.LeaseInfo.LastRenewalTimestampLegacy` | Property | Steeltoe.Discovery.Eureka | Removed | None | Legacy syntax handled internally |
| `Steeltoe.Discovery.Eureka.AppInfo.LeaseInfo.RegistrationTimestamp` | Property | Steeltoe.Discovery.Eureka | Renamed | `LeaseInfo.RegistrationTimeUtc` | Changed type from `long` to `DateTime` |
| `Steeltoe.Discovery.Eureka.AppInfo.LeaseInfo.RenewalIntervalInSecs` | Property | Steeltoe.Discovery.Eureka | Renamed | `LeaseInfo.RenewalInterval` | Changed type from `int` to `TimeSpan` |
| `Steeltoe.Discovery.Eureka.AppInfo.LeaseInfo.ServiceUpTimestamp` | Property | Steeltoe.Discovery.Eureka | Renamed | `LeaseInfo.ServiceUpTimeUtc` | Changed type from `long` to `DateTime` |
| `Steeltoe.Discovery.Eureka.ApplicationInfoManager` | Type | Steeltoe.Discovery.Eureka | Removed | `Steeltoe.Discovery.Eureka.EurekaApplicationInfoManager` | |
| `Steeltoe.Discovery.Eureka.ApplicationsFetchedEventArgs` | Type | Steeltoe.Discovery.Eureka | Added | | List of apps for `EurekaDiscoveryClient.ApplicationsFetched` event |
| `Steeltoe.Discovery.Eureka.DiscoveryClient` | Type | Steeltoe.Discovery.Eureka | Removed | `EurekaDiscoveryClient` | |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.Applications` | Property | Steeltoe.Discovery.Eureka | Removed | Subscribe to `EurekaDiscoveryClient.ApplicationsFetched` event | Made internal to guarantee correctness |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.FetchFullRegistryAsync` | Method | Steeltoe.Discovery.Eureka | Removed | Subscribe to `EurekaDiscoveryClient.ApplicationsFetched` event | Made internal to guarantee correctness |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.FetchRegistryAsync` | Method | Steeltoe.Discovery.Eureka | Removed | Subscribe to `EurekaDiscoveryClient.ApplicationsFetched` event | Made internal to guarantee correctness |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.FetchRegistryDeltaAsync` | Method | Steeltoe.Discovery.Eureka | Removed | Subscribe to `EurekaDiscoveryClient.ApplicationsFetched` event | Made internal to guarantee correctness |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.GetApplication` | Method | Steeltoe.Discovery.Eureka | Removed | Subscribe to `EurekaDiscoveryClient.ApplicationsFetched` event | Made internal to guarantee correctness |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.GetInstanceById` | Method | Steeltoe.Discovery.Eureka | Removed | Subscribe to `EurekaDiscoveryClient.ApplicationsFetched` event | Made internal to guarantee correctness |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.GetInstanceRemoteStatus` | Method | Steeltoe.Discovery.Eureka | Removed | Subscribe to `EurekaDiscoveryClient.ApplicationsFetched` event | Made internal to guarantee correctness |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.GetInstancesByVipAddress` | Method | Steeltoe.Discovery.Eureka | Removed | Subscribe to `EurekaDiscoveryClient.ApplicationsFetched` event | Made internal to guarantee correctness |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.GetInstancesByVipAddressAndAppName` | Method | Steeltoe.Discovery.Eureka | Removed | Subscribe to `EurekaDiscoveryClient.ApplicationsFetched` event | Made internal to guarantee correctness |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.GetNextServerFromEureka` | Method | Steeltoe.Discovery.Eureka | Removed | None | Refactored to internal `EurekaServiceUriStateManager` |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.HealthCheckHandler` | Property | Steeltoe.Discovery.Eureka | Removed | Implement `IHealthCheckHandler`, add it to service container | |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.HttpClient` | Property | Steeltoe.Discovery.Eureka | Removed | None | |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.Initialize` | Method | Steeltoe.Discovery.Eureka | Removed | None | No longer needed |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.InitializeAsync` | Method | Steeltoe.Discovery.Eureka | Removed | None | No longer needed |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.LastGoodDeltaRegistryFetchTimestamp` | Property | Steeltoe.Discovery.Eureka | Removed | None | Made internal to guarantee correctness |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.LastGoodFullRegistryFetchTimestamp` | Property | Steeltoe.Discovery.Eureka | Removed | None | Made internal to guarantee correctness |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.LastGoodHeartbeatTimestamp` | Property | Steeltoe.Discovery.Eureka | Removed | None | Made internal to guarantee correctness |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.LastGoodRegisterTimestamp` | Property | Steeltoe.Discovery.Eureka | Removed | None | Made internal to guarantee correctness |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.LastGoodRegistryFetchTimestamp` | Property | Steeltoe.Discovery.Eureka | Removed | None | Made internal to guarantee correctness |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.LastRemoteInstanceStatus` | Property | Steeltoe.Discovery.Eureka | Removed | None | Made internal to guarantee correctness |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.OnApplicationsChange` | Event | Steeltoe.Discovery.Eureka | Removed | `EurekaDiscoveryClient.ApplicationsFetched` | Was raised after every fetch, even if nothing changed |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.RefreshInstanceInfo` | Method | Steeltoe.Discovery.Eureka | Removed | None | Made internal to guarantee correctness |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.RegisterAsync` | Method | Steeltoe.Discovery.Eureka | Removed | None | Made internal to guarantee correctness |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.RegisterDirtyInstanceInfo` | Method | Steeltoe.Discovery.Eureka | Removed | None | Made internal to guarantee correctness |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.RenewAsync` | Method | Steeltoe.Discovery.Eureka | Removed | None | Made internal to guarantee correctness |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.StartTimer` | Method | Steeltoe.Discovery.Eureka | Removed | None | Made internal to guarantee correctness |
| `Steeltoe.Discovery.Eureka.DiscoveryClient.UnregisterAsync` | Method | Steeltoe.Discovery.Eureka | Removed | None | Made internal to guarantee correctness |
| `Steeltoe.Discovery.Eureka.DiscoveryManager` | Type | Steeltoe.Discovery.Eureka | Removed | `Steeltoe.Discovery.Eureka.EurekaDiscoveryClient` | |
| `Steeltoe.Discovery.Eureka.EurekaApplicationInfoManager.Instance` | Property | Steeltoe.Discovery.Eureka | Added | | Gets immutable snapshot of the local service instance |
| `Steeltoe.Discovery.Eureka.EurekaApplicationInfoManager.InstanceConfig` | Property | Steeltoe.Discovery.Eureka | Removed | `IOptionsMonitor<EurekaInstanceOptions>.CurrentValue` | Refactored to use ASP.NET Options pattern |
| `Steeltoe.Discovery.Eureka.EurekaApplicationInfoManager.UpdateInstance` | Method | Steeltoe.Discovery.Eureka | Added | | Enables to change local instance from code (synced with configuration) |
| `Steeltoe.Discovery.Eureka.EurekaClientConfig` | Type | Steeltoe.Discovery.Eureka | Removed | `Steeltoe.Discovery.Eureka.Configuration.EurekaClientOptions` | |
| `Steeltoe.Discovery.Eureka.EurekaClientConfig.EurekaServerConnectTimeoutSeconds` | Property | Steeltoe.Discovery.Eureka | Removed | `EurekaClientOptions.Server.ConnectTimeoutSeconds` | |
| `Steeltoe.Discovery.Eureka.EurekaClientConfig.EurekaServerRetryCount` | Property | Steeltoe.Discovery.Eureka | Removed | `EurekaClientOptions.Server.RetryCount` | Default changed from 3 attempts to 2 retries (bug fix) |
| `Steeltoe.Discovery.Eureka.EurekaClientConfig.EurekaServerServiceUrls` | Property | Steeltoe.Discovery.Eureka | Removed | `EurekaClientOptions.EurekaServerServiceUrls` | Now supports comma-separated list of URLs |
| `Steeltoe.Discovery.Eureka.EurekaClientConfig.HealthCheckEnabled` | Property | Steeltoe.Discovery.Eureka | Removed | `EurekaClientOptions.Health.CheckEnabled` | |
| `Steeltoe.Discovery.Eureka.EurekaClientConfig.HealthContribEnabled` | Property | Steeltoe.Discovery.Eureka | Removed | `EurekaClientOptions.Health.ContributorEnabled` | |
| `Steeltoe.Discovery.Eureka.EurekaClientConfig.HealthMonitoredApps` | Property | Steeltoe.Discovery.Eureka | Removed | `EurekaClientOptions.Health.MonitoredApps` | |
| `Steeltoe.Discovery.Eureka.EurekaClientConfig.ProxyHost` | Property | Steeltoe.Discovery.Eureka | Removed | `EurekaClientOptions.Server.ProxyHost` | |
| `Steeltoe.Discovery.Eureka.EurekaClientConfig.ProxyPassword` | Property | Steeltoe.Discovery.Eureka | Removed | `EurekaClientOptions.Server.ProxyPassword` | |
| `Steeltoe.Discovery.Eureka.EurekaClientConfig.ProxyPort` | Property | Steeltoe.Discovery.Eureka | Removed | `EurekaClientOptions.Server.ProxyPort` | |
| `Steeltoe.Discovery.Eureka.EurekaClientConfig.ProxyUserName` | Property | Steeltoe.Discovery.Eureka | Removed | `EurekaClientOptions.Server.ProxyUserName` | |
| `Steeltoe.Discovery.Eureka.EurekaClientConfig.ShouldDisableDelta` | Property | Steeltoe.Discovery.Eureka | Moved | `EurekaClientOptions.IsFetchDeltaDisabled` | |
| `Steeltoe.Discovery.Eureka.EurekaClientConfig.ShouldGZipContent` | Property | Steeltoe.Discovery.Eureka | Removed | `EurekaClientOptions.Server.ShouldGZipContent` | |
| `Steeltoe.Discovery.Eureka.EurekaClientConfig.ShouldOnDemandUpdateStatusChange` | Property | Steeltoe.Discovery.Eureka | Removed | None | Always sends batch of local updates to Eureka server immediately |
| `Steeltoe.Discovery.Eureka.EurekaClientOptions` | Type | Steeltoe.Discovery.Eureka | Moved | `Steeltoe.Discovery.Eureka.Configuration.EurekaClientOptions` | |
| `Steeltoe.Discovery.Eureka.EurekaClientOptions.CacheTTL` | Property | Steeltoe.Discovery.Eureka | Removed | Use caching provided by `ServiceInstancesResolver` | |
| `Steeltoe.Discovery.Eureka.EurekaClientOptions.EurekaHealthConfig` | Type | Steeltoe.Discovery.Eureka | Moved | `Steeltoe.Discovery.Eureka.Configuration.EurekaHealthOptions` | |
| `Steeltoe.Discovery.Eureka.EurekaClientOptions.EurekaHealthConfig.Enabled` | Property | Steeltoe.Discovery.Eureka | Renamed | `EurekaHealthOptions.ContributorEnabled` | |
| `Steeltoe.Discovery.Eureka.EurekaClientOptions.EurekaServerConfig` | Type | Steeltoe.Discovery.Eureka | Moved | `Steeltoe.Discovery.Eureka.Configuration.EurekaServerOptions` | |
| `Steeltoe.Discovery.Eureka.EurekaClientOptions.ServiceUrl` | Property | Steeltoe.Discovery.Eureka | Renamed | `EurekaClientOptions.EurekaServerServiceUrls` | |
| `Steeltoe.Discovery.Eureka.EurekaClientOptions.Validate_Certificates` | Property | Steeltoe.Discovery.Eureka | Renamed | `EurekaClientOptions.ValidateCertificates` | |
| `Steeltoe.Discovery.Eureka.EurekaClientService` | Type | Steeltoe.Discovery.Eureka | Removed | `Steeltoe.Discovery.Eureka.EurekaDiscoveryClient` | |
| `Steeltoe.Discovery.Eureka.EurekaDiscoveryClient.ApplicationsFetched` | Event | Steeltoe.Discovery.Eureka | Added | | Resulting set (with delta applied), invoked from ThreadPool thread |
| `Steeltoe.Discovery.Eureka.EurekaDiscoveryClient.ClientConfig` | Property | Steeltoe.Discovery.Eureka | Removed | `IOptionsMonitor<EurekaClientOptions>.CurrentValue` | Refactored to use ASP.NET Options pattern |
| `Steeltoe.Discovery.Eureka.EurekaDiscoveryClient.EnsureAssemblyIsLoaded` | Method | Steeltoe.Discovery.Eureka | Removed | None | No longer needed |
| `Steeltoe.Discovery.Eureka.EurekaDiscoveryClient.GetInstances` | Method | Steeltoe.Discovery.Eureka | Renamed | `EurekaDiscoveryClient.GetInstancesAsync` | |
| `Steeltoe.Discovery.Eureka.EurekaDiscoveryClient.GetServices` | Method | Steeltoe.Discovery.Eureka | Renamed | `EurekaDiscoveryClient.GetServiceIdsAsync` | |
| `Steeltoe.Discovery.Eureka.EurekaDiscoveryClient.Services` | Property | Steeltoe.Discovery.Eureka | Removed | `EurekaDiscoveryClient.GetServiceIdsAsync` | |
| `Steeltoe.Discovery.Eureka.EurekaDiscoveryClientBuilderExtension` | Type | Steeltoe.Discovery.Eureka | Removed | Call `IServiceCollection.AddEurekaDiscoveryClient()` extension method | |
| `Steeltoe.Discovery.Eureka.EurekaDiscoveryClientExtension` | Type | Steeltoe.Discovery.Eureka | Removed | Call `IServiceCollection.AddEurekaDiscoveryClient()` extension method | |
| `Steeltoe.Discovery.Eureka.EurekaDiscoveryManager` | Type | Steeltoe.Discovery.Eureka | Removed | `Steeltoe.Discovery.Eureka.EurekaDiscoveryClient` | |
| `Steeltoe.Discovery.Eureka.EurekaHealthCheckHandler` | Type | Steeltoe.Discovery.Eureka | Removed | Configure `EurekaClientOptions.Health.CheckEnabled` | Made internal |
| `Steeltoe.Discovery.Eureka.EurekaInstanceConfig` | Type | Steeltoe.Discovery.Eureka | Removed | `Steeltoe.Discovery.Eureka.Configuration.EurekaInstanceOptions` | |
| `Steeltoe.Discovery.Eureka.EurekaInstanceConfig.ApplyNetUtils` | Method | Steeltoe.Discovery.Eureka | Removed | None | Refactored to internal logic, happens automatically based on settings |
| `Steeltoe.Discovery.Eureka.EurekaInstanceConfig.ASGName` | Property | Steeltoe.Discovery.Eureka | Removed | `EurekaInstanceOptions.AutoScalingGroupName` | |
| `Steeltoe.Discovery.Eureka.EurekaInstanceConfig.DefaultAddressResolutionOrder` | Property | Steeltoe.Discovery.Eureka | Removed | None | Property was never used |
| `Steeltoe.Discovery.Eureka.EurekaInstanceConfig.NetUtils` | Property | Steeltoe.Discovery.Eureka | Removed | None | OS-based network APIs are no longer pluggable |
| `Steeltoe.Discovery.Eureka.EurekaInstanceConfig.PreferIpAddress` | Property | Steeltoe.Discovery.Eureka | Moved | `EurekaInstanceOptions.PreferIPAddress` | |
| `Steeltoe.Discovery.Eureka.EurekaInstanceConfig.SecurePortEnabled` | Property | Steeltoe.Discovery.Eureka | Moved | `EurekaInstanceOptions.IsSecurePortEnabled` | |
| `Steeltoe.Discovery.Eureka.EurekaInstanceConfig.SecureVirtualHostName` | Property | Steeltoe.Discovery.Eureka | Moved | `EurekaInstanceOptions.SecureVipAddress` | |
| `Steeltoe.Discovery.Eureka.EurekaInstanceConfig.UseNetUtils` | Property | Steeltoe.Discovery.Eureka | Moved | `EurekaInstanceOptions.UseNetworkInterfaces` | |
| `Steeltoe.Discovery.Eureka.EurekaInstanceConfig.VirtualHostName` | Property | Steeltoe.Discovery.Eureka | Moved | `EurekaInstanceOptions.VipAddress` | |
| `Steeltoe.Discovery.Eureka.EurekaInstanceOptions` | Type | Steeltoe.Discovery.Eureka | Moved | `Steeltoe.Discovery.Eureka.Configuration.EurekaInstanceOptions` | |
| `Steeltoe.Discovery.Eureka.EurekaInstanceOptions.AppGroup` | Property | Steeltoe.Discovery.Eureka | Renamed | `EurekaInstanceOptions.AppGroupName` | |
| `Steeltoe.Discovery.Eureka.EurekaInstanceOptions.ApplyConfigUrls` | Method | Steeltoe.Discovery.Eureka | Removed | None | Refactored to internal logic, happens automatically based on settings |
| `Steeltoe.Discovery.Eureka.EurekaInstanceOptions.GetHostName` | Method | Steeltoe.Discovery.Eureka | Removed | None | Refactored to internal handling |
| `Steeltoe.Discovery.Eureka.EurekaInstanceOptions.InstanceEnabledOnInit` | Property | Steeltoe.Discovery.Eureka | Renamed | `EurekaInstanceOptions.IsInstanceEnabledOnInit` | |
| `Steeltoe.Discovery.Eureka.EurekaInstanceOptions.IpAddress` | Property | Steeltoe.Discovery.Eureka | Renamed | `EurekaInstanceOptions.IPAddress` | |
| `Steeltoe.Discovery.Eureka.EurekaInstanceOptions.NonSecurePortEnabled` | Property | Steeltoe.Discovery.Eureka | Renamed | `EurekaInstanceOptions.IsNonSecurePortEnabled` | |
| `Steeltoe.Discovery.Eureka.EurekaInstanceOptions.Port` | Property | Steeltoe.Discovery.Eureka | Renamed | `EurekaInstanceOptions.NonSecurePort` | |
| `Steeltoe.Discovery.Eureka.EurekaPostConfigurer` | Type | Steeltoe.Discovery.Eureka | Removed | None | Refactored to internal type `PostConfigureEurekaInstanceOptions` |
| `Steeltoe.Discovery.Eureka.EurekaServerHealthContributor` | Type | Steeltoe.Discovery.Eureka | Removed | Configure `EurekaClientOptions.Health.ContributorEnabled` | Made internal |
| `Steeltoe.Discovery.Eureka.EurekaServiceCollectionExtensions.AddEurekaDiscoveryClient` | Extension method | Steeltoe.Discovery.Eureka | Added | | Activates the Eureka discovery client |
| `Steeltoe.Discovery.Eureka.EurekaServiceInstance` | Type | Steeltoe.Discovery.Eureka | Removed | `IServiceInstance` | Made internal, implementation detail |
| `Steeltoe.Discovery.Eureka.EurekaServiceUriStateManager` | Type | Steeltoe.Discovery.Eureka | Added | | Load-balances over multiple Eureka servers |
| `Steeltoe.Discovery.Eureka.HealthCheckHandlerProvider` | Type | Steeltoe.Discovery.Eureka | Added | | Run contributors and ASP.NET health checks to determine local instance status |
| `Steeltoe.Discovery.Eureka.IEurekaClient` | Type | Steeltoe.Discovery.Eureka | Removed | `Steeltoe.Discovery.Eureka.EurekaDiscoveryClient` | |
| `Steeltoe.Discovery.Eureka.IEurekaClientConfig` | Type | Steeltoe.Discovery.Eureka | Removed | `Steeltoe.Discovery.Eureka.Configuration.EurekaClientOptions` | |
| `Steeltoe.Discovery.Eureka.IEurekaInstanceConfig` | Type | Steeltoe.Discovery.Eureka | Removed | `Steeltoe.Discovery.Eureka.Configuration.EurekaInstanceOptions` | |
| `Steeltoe.Discovery.Eureka.ILookupService` | Type | Steeltoe.Discovery.Eureka | Removed | `Steeltoe.Discovery.Eureka.EurekaDiscoveryClient` | |
| `Steeltoe.Discovery.Eureka.ScopedEurekaHealthCheckHandler` | Type | Steeltoe.Discovery.Eureka | Removed | `Steeltoe.Discovery.Eureka.IHealthCheckHandler` | Moved to internal type `EurekaHealthCheckHandler` |
| `Steeltoe.Discovery.Eureka.StatusChangedArgs` | Type | Steeltoe.Discovery.Eureka | Removed | `EurekaApplicationInfoManager.UpdateInstance()` | |
| `Steeltoe.Discovery.Eureka.StatusChangedHandler` | Type | Steeltoe.Discovery.Eureka | Removed | `EurekaApplicationInfoManager.UpdateInstance()` | Refactored to internal `EurekaApplicationInfoManager.InstanceChanged` event |
| `Steeltoe.Discovery.Eureka.ThisServiceInstance` | Type | Steeltoe.Discovery.Eureka | Removed | None | No longer needed |
| `Steeltoe.Discovery.Eureka.Transport.EurekaHttpClient` | Type | Steeltoe.Discovery.Eureka | Moved | `Steeltoe.Discovery.Eureka.EurekaClient` | |
| `Steeltoe.Discovery.Eureka.Transport.EurekaHttpClient.DeleteStatusOverrideAsync` | Method | Steeltoe.Discovery.Eureka | Removed | None | Not needed for service discovery |
| `Steeltoe.Discovery.Eureka.Transport.EurekaHttpClient.GetApplicationAsync` | Method | Steeltoe.Discovery.Eureka | Removed | None | Not needed for service discovery |
| `Steeltoe.Discovery.Eureka.Transport.EurekaHttpClient.GetInstanceAsync` | Method | Steeltoe.Discovery.Eureka | Removed | None | Not needed for service discovery |
| `Steeltoe.Discovery.Eureka.Transport.EurekaHttpClient.GetSecureVipAsync` | Method | Steeltoe.Discovery.Eureka | Removed | `EurekaClient.GetByVipAsync` | Identical implementation |
| `Steeltoe.Discovery.Eureka.Transport.EurekaHttpClient.GetVipAsync` | Method | Steeltoe.Discovery.Eureka | Renamed | `EurekaClient.GetByVipAsync` | |
| `Steeltoe.Discovery.Eureka.Transport.EurekaHttpClient.SendHeartBeatAsync` | Method | Steeltoe.Discovery.Eureka | Renamed | `EurekaClient.HeartbeatAsync` | |
| `Steeltoe.Discovery.Eureka.Transport.EurekaHttpClient.Shutdown` | Method | Steeltoe.Discovery.Eureka | Renamed | `EurekaClient.DeregisterAsync` | |
| `Steeltoe.Discovery.Eureka.Transport.EurekaHttpClient.StatusUpdateAsync` | Method | Steeltoe.Discovery.Eureka | Removed | None | Not needed for service discovery |
| `Steeltoe.Discovery.Eureka.Transport.EurekaHttpResponse` | Type | Steeltoe.Discovery.Eureka | Removed | None | No longer needed |
| `Steeltoe.Discovery.Eureka.Transport.EurekaHttpResponse<T>` | Type | Steeltoe.Discovery.Eureka | Removed | None | No longer needed |
| `Steeltoe.Discovery.Eureka.Transport.IEurekaHttpClient` | Type | Steeltoe.Discovery.Eureka | Removed | `Steeltoe.Discovery.Eureka.EurekaClient` | |
| `Steeltoe.Discovery.Eureka.Util.DateTimeConversions` | Type | Steeltoe.Discovery.Eureka | Removed | None | Made internal |

### Notable PRs

- https://github.com/SteeltoeOSS/Steeltoe/pull/1372
- https://github.com/SteeltoeOSS/Steeltoe/pull/1350
- https://github.com/SteeltoeOSS/Steeltoe/pull/1308
- https://github.com/SteeltoeOSS/Steeltoe/pull/1301
- https://github.com/SteeltoeOSS/Steeltoe/pull/1300
- https://github.com/SteeltoeOSS/Steeltoe/pull/1299
- https://github.com/SteeltoeOSS/Steeltoe/pull/1292
- https://github.com/SteeltoeOSS/Steeltoe/pull/1280
- https://github.com/SteeltoeOSS/Steeltoe/pull/1247
- https://github.com/SteeltoeOSS/Steeltoe/pull/1167

### Documentation

For more information, see the updated [Discovery documentation](../discovery/index.md) and
[Discovery samples](https://github.com/SteeltoeOSS/Samples/tree/main/Discovery).

## Logging

### Behavior changes

- Simplified API: `builder.Logging.AddDynamicSerilog()`
- Monitors and adapts to changes in `IConfiguration` (restores to changed minimum level after reset)
- Optimized for performance: faster updates, uses less memory
- Improved reliability: no more stale reads, race conditions and lost updates
- Fix default minimum log level to be Information instead of None
- Fix category name comparisons to be case-sensitive
- Fix namespace matching in categories: "Ab" is not a descendant of "A"
- Fix broken reset of levels when changed earlier (should revert to configured instead of default)
- Fix mismatches between the levels returned vs. the levels truly active in `ILoggers`
- Fix crash with latest version of Spring Boot Admin
- Fixed: console-specific rules should win over global rules in `appsettings.json`
- Serilog: fix crash when no default category is configured
- Serilog: Configured overrides were ignored when no default level was configured

### NuGet Package changes

| Source | Kind | Package | Change | Replacement | Notes |
| --- | --- | --- | --- | --- | --- |
| Steeltoe.Extensions.Logging.Abstractions | Package | | Renamed | Steeltoe.Logging.Abstractions | |
| Steeltoe.Extensions.Logging.DynamicLogger | Package | | Renamed | Steeltoe.Logging.DynamicConsole | |
| Steeltoe.Extensions.Logging.DynamicSerilogBase | Package | | Renamed | Steeltoe.Logging.DynamicSerilog | |
| Steeltoe.Extensions.Logging.DynamicSerilogCore | Package | | Renamed | Steeltoe.Logging.DynamicSerilog | |

### API changes

| Source | Kind | Package | Change | Replacement | Notes |
| --- | --- | --- | --- | --- | --- |
| `Steeltoe.Extensions.Logging.DynamicLoggerConfiguration` | Type | Steeltoe.Extensions.Logging.Abstractions | Renamed | `Steeltoe.Logging.DynamicLoggerState` | Represents logger state, is unrelated to `IConfiguration` |
| `Steeltoe.Extensions.Logging.DynamicLoggerConfiguration.ConfiguredLevel` | Property | Steeltoe.Extensions.Logging.Abstractions | Renamed | `DynamicLoggerState.BackupMinLevel` | The minimum level before override |
| `Steeltoe.Extensions.Logging.DynamicLoggerConfiguration.EffectiveLevel` | Property | Steeltoe.Extensions.Logging.Abstractions | Renamed | `DynamicLoggerState.EffectiveMinLevel` | The active minimum level, taking overrides into account |
| `Steeltoe.Extensions.Logging.DynamicLoggerConfiguration.Name` | Property | Steeltoe.Extensions.Logging.Abstractions | Renamed | `DynamicLoggerState.CategoryName` | |
| `Steeltoe.Extensions.Logging.DynamicLoggerProviderBase` | Type | Steeltoe.Extensions.Logging.Abstractions | Renamed | `Steeltoe.Logging.DynamicLoggerProvider` | |
| `Steeltoe.Extensions.Logging.DynamicLoggerProviderBase.GetLoggerConfigurations` | Method | Steeltoe.Extensions.Logging.Abstractions | Renamed | `DynamicLoggerProvider.GetLogLevels` | |
| `Steeltoe.Extensions.Logging.IDynamicLoggerProvider.GetLoggerConfigurations` | Method | Steeltoe.Extensions.Logging.Abstractions | Renamed | `IDynamicLoggerProvider.GetLogLevels` | |
| `Steeltoe.Extensions.Logging.ILoggerConfiguration` | Type | Steeltoe.Extensions.Logging.Abstractions | Removed | `Steeltoe.Logging.DynamicLoggerState` | |
| `Steeltoe.Extensions.Logging.InitialLevels` | Type | Steeltoe.Extensions.Logging.Abstractions | Renamed | `Steeltoe.Logging.LogLevelsConfiguration` | |
| `Steeltoe.Extensions.Logging.MessageProcessingLogger.Delegate` | Property | Steeltoe.Extensions.Logging.Abstractions | Renamed | `MessageProcessingLogger.InnerLogger` | |
| `Steeltoe.Extensions.Logging.MessageProcessingLogger.Filter` | Property | Steeltoe.Extensions.Logging.Abstractions | Removed | None | Refactored to handle internally |
| `Steeltoe.Extensions.Logging.MessageProcessingLogger.Name` | Property | Steeltoe.Extensions.Logging.Abstractions | Removed | None | No longer needed |
| `Steeltoe.Extensions.Logging.MessageProcessingLogger.WriteMessage` | Method | Steeltoe.Extensions.Logging.Abstractions | Removed | None | Refactored to handle internally |
| `Steeltoe.Extensions.Logging.StructuredMessageProcessingLogger` | Type | Steeltoe.Extensions.Logging.Abstractions | Removed | `Steeltoe.Logging.MessageProcessingLogger` | No longer needed |
| `Steeltoe.Logging.DynamicLoggerProvider.CreateMessageProcessingLogger` | Method | Steeltoe.Logging.Abstractions | Added | | Provides creation of `MessageProcessingLogger` in derived types |
| `Steeltoe.Logging.DynamicLoggerProvider.GetFilter` | Method | Steeltoe.Logging.Abstractions | Added | | Provides access to filter callback in derived types |
| `Steeltoe.Logging.DynamicLoggerProvider.InnerLoggerProvider` | Property | Steeltoe.Logging.Abstractions | Added | | Provides access to wrapped ILogger in derived types |
| `Steeltoe.Logging.DynamicLoggerProvider.MessageProcessors` | Property | Steeltoe.Logging.Abstractions | Added | | Provides access to processors in derived types |
| `Steeltoe.Logging.DynamicLoggerProvider.RefreshConfiguration` | Method | Steeltoe.Logging.Abstractions | Added | | Applies changes from `IConfiguration` |
| `Steeltoe.Logging.IDynamicLoggerProvider.RefreshConfiguration` | Method | Steeltoe.Logging.Abstractions | Added | | Applies changes from `IConfiguration` |
| `Steeltoe.Logging.LoggerFilter` | Type | Steeltoe.Logging.Abstractions | Added | | Delegate to simplify signatures |
| `Steeltoe.Logging.MessageProcessingLogger.ChangeFilter` | Method | Steeltoe.Logging.Abstractions | Added | | Called by `DynamicLoggerProvider` when effective level changes |
| `Steeltoe.Logging.MessageProcessingLogger.MessageProcessors` | Property | Steeltoe.Logging.Abstractions | Added | | Provides access to processors in derived types |
| `Steeltoe.Extensions.Logging.DynamicConsoleLoggerProvider` | Type | Steeltoe.Extensions.Logging.DynamicLogger | Moved | `Steeltoe.Logging.DynamicConsole.DynamicConsoleLoggerProvider` | |
| `Steeltoe.Extensions.Logging.DynamicLoggerHostBuilderExtensions.AddDynamicLogging` | Extension method | Steeltoe.Extensions.Logging.DynamicLogger | Removed | `LoggingBuilderExtensions.AddDynamicConsole` | |
| `Steeltoe.Extensions.Logging.DynamicLoggingBuilder.AddDynamicConsole` | Extension method | Steeltoe.Extensions.Logging.DynamicLogger | Moved | `LoggingBuilderExtensions.AddDynamicConsole` | |
| `Steeltoe.Extensions.Logging.DynamicSerilog.ISerilogOptions` | Type | Steeltoe.Extensions.Logging.DynamicSerilog [Base/Core] | Removed | `Steeltoe.Logging.DynamicSerilog.SerilogOptions` | Redundant, there's only one Serilog product |
| `Steeltoe.Extensions.Logging.DynamicSerilog.SerilogDynamicLoggerFactory` | Type | Steeltoe.Extensions.Logging.DynamicSerilog [Base/Core] | Removed | `DynamicSerilogLoggerProvider` | No longer needed |
| `Steeltoe.Extensions.Logging.DynamicSerilog.SerilogDynamicProvider` | Type | Steeltoe.Extensions.Logging.DynamicSerilog [Base/Core] | Renamed | `DynamicSerilogLoggerProvider` | |
| `Steeltoe.Extensions.Logging.DynamicSerilog.SerilogHostBuilderExtensions.AddDynamicSerilog` | Extension method | Steeltoe.Extensions.Logging.DynamicSerilog [Base/Core] | Removed | `ILoggingBuilder.AddDynamicSerilog()` | |
| `Steeltoe.Extensions.Logging.DynamicSerilog.SerilogHostBuilderExtensions.UseSerilogDynamicConsole` | Extension method | Steeltoe.Extensions.Logging.DynamicSerilog [Base/Core] | Removed | `ILoggingBuilder.AddDynamicSerilog()` | |
| `Steeltoe.Extensions.Logging.DynamicSerilog.SerilogLoggingBuilderExtensions.AddSerilogDynamicConsole` | Extension method | Steeltoe.Extensions.Logging.DynamicSerilog [Base/Core] | Removed | `ILoggingBuilder.AddDynamicSerilog()` | |
| `Steeltoe.Extensions.Logging.DynamicSerilog.SerilogOptions.ConfigPath` | Property | Steeltoe.Extensions.Logging.DynamicSerilog [Base/Core] | Removed | None | No longer needed |
| `Steeltoe.Extensions.Logging.DynamicSerilog.SerilogOptions.FullnameExclusions` | Property | Steeltoe.Extensions.Logging.DynamicSerilog [Base/Core] | Removed | None | No longer needed |
| `Steeltoe.Extensions.Logging.DynamicSerilog.SerilogOptions.SubloggerConfigKeyExclusions` | Property | Steeltoe.Extensions.Logging.DynamicSerilog [Base/Core] | Removed | None | No longer needed |
| `Steeltoe.Extensions.Logging.DynamicSerilog.SerilogWebApplicationBuilderExtensions.AddDynamicSerilog` | Extension method | Steeltoe.Extensions.Logging.DynamicSerilog [Base/Core] | Removed | `ILoggingBuilder.AddDynamicSerilog()` | |
| `Steeltoe.Extensions.Logging.DynamicSerilog.SerilogWebHostBuilderExtensions.AddDynamicSerilog` | Extension method | Steeltoe.Extensions.Logging.DynamicSerilog [Base/Core] | Removed | `ILoggingBuilder.AddDynamicSerilog()` | |
| `Steeltoe.Extensions.Logging.DynamicSerilog.SerilogWebHostBuilderExtensions.UseSerilogDynamicConsole` | Extension method | Steeltoe.Extensions.Logging.DynamicSerilog [Base/Core] | Removed | `ILoggingBuilder.AddDynamicSerilog()` | |
| `Steeltoe.Logging.DynamicSerilog.SerilogMessageProcessingLogger` | Type | Steeltoe.Logging.DynamicSerilog | Added | | Preserve structured logs with `IDynamicMessageProcessor` in Serilog |

### Notable PRs

- https://github.com/SteeltoeOSS/Steeltoe/pull/1468
- https://github.com/SteeltoeOSS/Steeltoe/pull/1403
- https://github.com/SteeltoeOSS/Steeltoe/pull/1216
- https://github.com/SteeltoeOSS/Steeltoe/pull/1024
- https://github.com/SteeltoeOSS/Steeltoe/pull/1064
- https://github.com/SteeltoeOSS/Steeltoe/pull/1038
- https://github.com/SteeltoeOSS/Steeltoe/pull/991

### Documentation

For more information, see the updated [Logging documentation](../logging/index.md).

## Management

### Behavior changes

- Unified configuration for `/actuator` and `/cloudfoundryapplication`
- Reduced required code for using an actuator to a single `IServiceCollection` extension method (configures middleware by default)
- Cleaner extensibility model for third-party actuators
- Fail at startup when actuators are used on Cloud Foundry without security
- Improved security and redaction of sensitive data in responses and logs
- Actuators can be turned on/off or bound to different verbs at runtime using configuration
- Simplified content negotiation; updated all actuators to support latest Spring media type
- New actuator `/beans` that lists the contents of the .NET dependency container, including support for keyed services
- Update health checks and actuator to align with latest Spring; hide details by default; contributors can be turned on/off at runtime using configuration
- Support Windows network shares in disk space health contributor
- Update `/mappings` actuator to include endpoints from Minimal APIs, Razor Pages, and Blazor, with richer metadata and improved compatibility with Spring
- Heap dumps are enabled by default in Cloud Foundry on Linux; all dump types supported on Windows/Linux/macOS
- Improved Prometheus exporter that works with latest OpenTelemetry
- Various fixes for interoperability with latest Spring Boot Admin; more flexible configuration, uses smarter defaults
- Unified `/traces` and `/httptraces` actuators to `/httpexchanges`, to align with latest Spring
- WaveFront, Zipkin, and Jaeger support was removed (use OpenTelemetry directly)
- Metrics endpoint was removed (use OpenTelemetry directly)
- Kubernetes actuator was removed

### NuGet Package changes

| Source | Change | Replacement | Notes |
| --- | --- | --- | --- |
| Steeltoe.Management.CloudFoundryCore | Moved | Steeltoe.Management.Endpoint | Contained the Cloud Foundry actuator |
| Steeltoe.Management.Diagnostics | Moved | Steeltoe.Management.Endpoint | Contained code for taking heap dumps |
| Steeltoe.Management.EndpointBase | Renamed | Steeltoe.Management.Endpoint | |
| Steeltoe.Management.EndpointCore | Renamed | Steeltoe.Management.Endpoint | |
| Steeltoe.Management.KubernetesCore | Removed | None | |
| Steeltoe.Management.OpenTelemetryBase | Removed | Steeltoe.Management.Prometheus | WaveFront exporter was removed |
| Steeltoe.Management.Prometheus | Added | | Provides the Prometheus actuator |
| Steeltoe.Management.TaskCore | Renamed | Steeltoe.Management.Tasks | |
| Steeltoe.Management.TracingBase | Renamed | Steeltoe.Management.Tracing | |
| Steeltoe.Management.TracingCore | Renamed | Steeltoe.Management.Tracing | |

### API changes

| Source | Kind | Package | Change | Replacement | Notes |
| --- | --- | --- | --- | --- | --- |
| `Microsoft.Diagnostics.Runtime.Interop.IMAGE_FILE_MACHINE` | Type | Steeltoe.Management.Diagnostics | Removed | None | |
| `Microsoft.Extensions.DependencyInjection.ServiceCollectionExtensions.AddActuatorEndpointMapping<TEndpoint>` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Removed | `Steeltoe.Management.Endpoint.ApplicationBuilderExtensions.UseActuatorEndpoints` | Needed only when setting up middleware manually |
| `Microsoft.Extensions.DependencyInjection.ServiceCollectionExtensions.AddCloudFoundryActuatorServices` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Moved | `builder.Services.AddCloudFoundryActuator()` | |
| `Microsoft.Extensions.DependencyInjection.ServiceCollectionExtensions.AddDbMigrationsActuatorServices` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Moved | `builder.Services.AddDbMigrationsActuator()` | |
| `Microsoft.Extensions.DependencyInjection.ServiceCollectionExtensions.AddEnvActuatorServices` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Moved | `builder.Services.AddEnvironmentActuator()` | |
| `Microsoft.Extensions.DependencyInjection.ServiceCollectionExtensions.AddHealthActuatorServices` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Moved | `builder.Services.AddHealthActuator()` with `.AddHealthContributor()` | |
| `Microsoft.Extensions.DependencyInjection.ServiceCollectionExtensions.AddHeapDumpActuatorServices` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Moved | `builder.Services.AddHeapDumpActuator()` | |
| `Microsoft.Extensions.DependencyInjection.ServiceCollectionExtensions.AddHypermediaActuatorServices` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Moved | `builder.Services.AddHypermediaActuator()` | |
| `Microsoft.Extensions.DependencyInjection.ServiceCollectionExtensions.AddInfoActuatorServices` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Moved | `builder.Services.AddInfoActuator()` with `.AddInfoContributor()` | |
| `Microsoft.Extensions.DependencyInjection.ServiceCollectionExtensions.AddLoggersActuatorServices` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Moved | `builder.Services.AddLoggersActuator()` | |
| `Microsoft.Extensions.DependencyInjection.ServiceCollectionExtensions.AddMappingsActuatorServices` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Moved | `builder.Services.AddRouteMappingsActuator()` | |
| `Microsoft.Extensions.DependencyInjection.ServiceCollectionExtensions.AddMetricsActuatorServices` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Metrics actuator has been removed |
| `Microsoft.Extensions.DependencyInjection.ServiceCollectionExtensions.AddOpenTelemetryMetricsForSteeltoe` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Removed | Use OpenTelemetry packages directly | |
| `Microsoft.Extensions.DependencyInjection.ServiceCollectionExtensions.AddPrometheusActuatorServices` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Moved | `AddPrometheusActuator` in Steeltoe.Management.Prometheus package | |
| `Microsoft.Extensions.DependencyInjection.ServiceCollectionExtensions.AddRefreshActuatorServices` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Moved | `builder.Services.AddRefreshActuator()` | |
| `Microsoft.Extensions.DependencyInjection.ServiceCollectionExtensions.AddSpringBootAdminClient` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Moved | `builder.Services.AddSpringBootAdminClient()` | |
| `Microsoft.Extensions.DependencyInjection.ServiceCollectionExtensions.AddThreadDumpActuatorServices` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Moved | `builder.Services.AddThreadDumpActuator()` | |
| `Microsoft.Extensions.DependencyInjection.ServiceCollectionExtensions.AddTraceActuatorServices` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Moved | `builder.Services.AddHttpExchangesActuator()` | |
| `Microsoft.Extensions.DependencyInjection.ServiceCollectionExtensions.ConfigureSteeltoeMetrics` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Removed | Use OpenTelemetry packages directly | |
| `Steeltoe.Common.Diagnostics.DiagnosticHelpers` | Type | Steeltoe.Management.Abstractions | Removed | None | No longer needed |
| `Steeltoe.Common.Diagnostics.DiagnosticObserver` | Type | Steeltoe.Management.Abstractions | Removed | None | Made internal, moved to Steeltoe.Management.Endpoint package |
| `Steeltoe.Common.Diagnostics.DiagnosticsManager` | Type | Steeltoe.Management.Abstractions | Removed | None | Made internal, moved to Steeltoe.Management.Endpoint package |
| `Steeltoe.Common.Diagnostics.IDiagnosticObserver` | Type | Steeltoe.Management.Abstractions | Removed | None | |
| `Steeltoe.Common.Diagnostics.IDiagnosticsManager` | Type | Steeltoe.Management.Abstractions | Removed | None | |
| `Steeltoe.Common.Diagnostics.IRuntimeDiagnosticSource` | Type | Steeltoe.Management.Abstractions | Removed | None | |
| `Steeltoe.Management.AbstractEndpoint` | Type | Steeltoe.Management.Abstractions | Moved | `IEndpointHandler<,>` in Steeltoe.Management.Endpoint package | |
| `Steeltoe.Management.AbstractEndpoint.Enabled` | Property | Steeltoe.Management.Abstractions | Removed | `EndpointOptions.Enabled` | |
| `Steeltoe.Management.AbstractEndpoint.Id` | Property | Steeltoe.Management.Abstractions | Removed | `EndpointOptions.Id` | |
| `Steeltoe.Management.AbstractEndpoint.Options` | Property | Steeltoe.Management.Abstractions | Removed | `EndpointMiddleware<,>.EndpointOptions` or inject `IOptionsMonitor` | |
| `Steeltoe.Management.AbstractEndpoint.options` | Field | Steeltoe.Management.Abstractions | Removed | `EndpointMiddleware<,>.EndpointOptions` or inject `IOptionsMonitor` | |
| `Steeltoe.Management.AbstractEndpoint.Path` | Property | Steeltoe.Management.Abstractions | Removed | `EndpointOptions.Path` | |
| `Steeltoe.Management.AbstractEndpoint<,>` | Type | Steeltoe.Management.Abstractions | Removed | `IEndpointHandler<,>` in Steeltoe.Management.Endpoint package | |
| `Steeltoe.Management.AbstractEndpoint<,>.Invoke` | Method | Steeltoe.Management.Abstractions | Removed | `IEndpointHandler<,>.InvokeAsync` | |
| `Steeltoe.Management.AbstractEndpoint<>` | Type | Steeltoe.Management.Abstractions | Removed | `IEndpointHandler<,>` in Steeltoe.Management.Endpoint package | |
| `Steeltoe.Management.AbstractEndpoint<>.Invoke` | Method | Steeltoe.Management.Abstractions | Removed | `IEndpointHandler<,>.InvokeAsync` | |
| `Steeltoe.Management.AbstractEndpointOptions` | Type | Steeltoe.Management.Abstractions | Renamed | `Steeltoe.Management.Configuration.EndpointOptions` | |
| `Steeltoe.Management.AbstractEndpointOptions._enabled` | Field | Steeltoe.Management.Abstractions | Removed | `EndpointOptions.Enabled` | |
| `Steeltoe.Management.AbstractEndpointOptions._path` | Field | Steeltoe.Management.Abstractions | Removed | `EndpointOptions.Path` | |
| `Steeltoe.Management.AbstractEndpointOptions._sensitive` | Field | Steeltoe.Management.Abstractions | Removed | None | Was never used |
| `Steeltoe.Management.AbstractEndpointOptions.DefaultEnabled` | Property | Steeltoe.Management.Abstractions | Removed | None, implicitly true | |
| `Steeltoe.Management.AbstractEndpointOptions.ExactMatch` | Property | Steeltoe.Management.Abstractions | Removed | `EndpointOptions.RequiresExactMatch()` | |
| `Steeltoe.Management.AbstractEndpointOptions.Global` | Property | Steeltoe.Management.Abstractions | Removed | Inject `IOptionsMonitor<ManagementOptions>` | |
| `Steeltoe.Management.AbstractEndpointOptions.IsAccessAllowed` | Method | Steeltoe.Management.Abstractions | Removed | `EndpointOptions.RequiredPermissions` | |
| `Steeltoe.Management.CloudFoundry.CloudFoundryActuatorsStartupFilter` | Type | Steeltoe.Management.CloudFoundryCore | Removed | None | |
| `Steeltoe.Management.CloudFoundry.CloudFoundryHostBuilderExtensions.AddCloudFoundryActuators` | Extension method | Steeltoe.Management.CloudFoundryCore | Moved | `builder.Services.AddCloudFoundryActuator()` in Steeltoe.Management.Endpoint package | |
| `Steeltoe.Management.CloudFoundry.CloudFoundryServiceCollectionExtensions.AddCloudFoundryActuators` | Extension method | Steeltoe.Management.CloudFoundryCore | Moved | `builder.Services.AddCloudFoundryActuator()` in Steeltoe.Management.Endpoint package | |
| `Steeltoe.Management.Configuration.EndpointOptions.GetDefaultAllowedVerbs` | Method | Steeltoe.Management.Abstractions | Added | | Override to initialize `AllowedVerbs` default |
| `Steeltoe.Management.Endpoint.ActuatorMediaTypes` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Each actuator uses built-in media type |
| `Steeltoe.Management.Endpoint.ActuatorRouteBuilderExtensions.LookupMiddleware` | Method | Steeltoe.Management.Endpoint [Base/Core] | Removed | Inject `IEnumerable<IEndpointMiddleware>` | |
| `Steeltoe.Management.Endpoint.ActuatorRouteBuilderExtensions.Map` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `MapActuators()` | |
| `Steeltoe.Management.Endpoint.ActuatorRouteBuilderExtensions.MapAllActuators` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `MapActuators()` | |
| `Steeltoe.Management.Endpoint.Actuators.CloudFoundry.ICloudFoundryEndpointHandler` | Type | Steeltoe.Management.Endpoint | Added | | Enables custom implementation to execute actuator |
| `Steeltoe.Management.Endpoint.Actuators.DbMigrations.IDbMigrationsEndpointHandler` | Type | Steeltoe.Management.Endpoint | Added | | Enables custom implementation to execute actuator |
| `Steeltoe.Management.Endpoint.Actuators.Environment.IEnvironmentEndpointHandler` | Type | Steeltoe.Management.Endpoint | Added | | Enables custom implementation to execute actuator |
| `Steeltoe.Management.Endpoint.Actuators.Health.Availability.ApplicationAvailability` | Type | Steeltoe.Management.Endpoint | Added | | Moved from Steeltoe.Common package |
| `Steeltoe.Management.Endpoint.Actuators.Health.Availability.AvailabilityEventArgs` | Type | Steeltoe.Management.Endpoint | Added | | Moved from Steeltoe.Common package |
| `Steeltoe.Management.Endpoint.Actuators.Health.Availability.AvailabilityState` | Type | Steeltoe.Management.Endpoint | Added | | Moved from Steeltoe.Common package |
| `Steeltoe.Management.Endpoint.Actuators.Health.Availability.LivenessState` | Type | Steeltoe.Management.Endpoint | Added | | Moved from Steeltoe.Common package |
| `Steeltoe.Management.Endpoint.Actuators.Health.Availability.LivenessStateContributorOptions.Enabled` | Property | Steeltoe.Management.Endpoint | Added | | Enables turning off via configuration |
| `Steeltoe.Management.Endpoint.Actuators.Health.Availability.ReadinessState` | Type | Steeltoe.Management.Endpoint | Added | | Moved from Steeltoe.Common package |
| `Steeltoe.Management.Endpoint.Actuators.Health.Availability.ReadinessStateContributorOptions.Enabled` | Property | Steeltoe.Management.Endpoint | Added | | Enables turning off via configuration |
| `Steeltoe.Management.Endpoint.Actuators.Health.Contributors.DiskSpaceContributorOptions.Enabled` | Property | Steeltoe.Management.Endpoint | Added | | Enables turning off via configuration |
| `Steeltoe.Management.Endpoint.Actuators.Health.HealthEndpointOptions.ShowComponents` | Property | Steeltoe.Management.Endpoint | Added | | Whether to show/hide contributors in response |
| `Steeltoe.Management.Endpoint.Actuators.Health.HealthEndpointRequest` | Type | Steeltoe.Management.Endpoint | Added | | Data about incoming actuator request |
| `Steeltoe.Management.Endpoint.Actuators.Health.HealthEndpointResponse.Components` | Property | Steeltoe.Management.Endpoint | Added | | List of health contributors in response |
| `Steeltoe.Management.Endpoint.Actuators.Health.HealthEndpointResponse.Exists` | Property | Steeltoe.Management.Endpoint | Added | | Used to indicate the request was invalid |
| `Steeltoe.Management.Endpoint.Actuators.Health.HealthGroupOptions.ShowComponents` | Property | Steeltoe.Management.Endpoint | Added | | Hide/show contributors in the group |
| `Steeltoe.Management.Endpoint.Actuators.Health.HealthGroupOptions.ShowDetails` | Property | Steeltoe.Management.Endpoint | Added | | Hide/show contributor details in the group |
| `Steeltoe.Management.Endpoint.Actuators.Health.IHealthEndpointHandler` | Type | Steeltoe.Management.Endpoint | Added | | Enables custom implementation to execute actuator |
| `Steeltoe.Management.Endpoint.Actuators.HeapDump.IHeapDumpEndpointHandler` | Type | Steeltoe.Management.Endpoint | Added | | Enables custom implementation to execute actuator |
| `Steeltoe.Management.Endpoint.Actuators.HttpExchanges.HttpExchangesEndpointOptions.RequestHeaders` | Property | Steeltoe.Management.Endpoint | Added | | Request header names to exclude from redaction |
| `Steeltoe.Management.Endpoint.Actuators.HttpExchanges.HttpExchangesEndpointOptions.ResponseHeaders` | Property | Steeltoe.Management.Endpoint | Added | | Response header names to exclude from redaction |
| `Steeltoe.Management.Endpoint.Actuators.HttpExchanges.HttpExchangesEndpointOptions.Reverse` | Property | Steeltoe.Management.Endpoint | Added | | Return most recent exchanges at the top |
| `Steeltoe.Management.Endpoint.Actuators.HttpExchanges.IHttpExchangesEndpointHandler` | Type | Steeltoe.Management.Endpoint | Added | | Enables custom implementation to execute actuator |
| `Steeltoe.Management.Endpoint.Actuators.Hypermedia.IHypermediaEndpointHandler` | Type | Steeltoe.Management.Endpoint | Added | | Enables custom implementation to execute actuator |
| `Steeltoe.Management.Endpoint.Actuators.Info.EndpointServiceCollectionExtensions.AddInfoContributor` | Extension method | Steeltoe.Management.Endpoint | Added | | Adds a contributor to info actuator |
| `Steeltoe.Management.Endpoint.Actuators.Info.IInfoContributor` | Type | Steeltoe.Management.Endpoint | Added | | Moved from Steeltoe.Management.Abstractions package |
| `Steeltoe.Management.Endpoint.Actuators.Info.IInfoEndpointHandler` | Type | Steeltoe.Management.Endpoint | Added | | Enables custom implementation to execute actuator |
| `Steeltoe.Management.Endpoint.Actuators.Info.InfoBuilder` | Type | Steeltoe.Management.Endpoint | Added | | Moved from Steeltoe.Management.Abstractions package |
| `Steeltoe.Management.Endpoint.Actuators.Loggers.ILoggersEndpointHandler` | Type | Steeltoe.Management.Endpoint | Added | | Enables custom implementation to execute actuator |
| `Steeltoe.Management.Endpoint.Actuators.Loggers.LoggerGroup` | Type | Steeltoe.Management.Endpoint | Added | | Group in `LoggersResponse` |
| `Steeltoe.Management.Endpoint.Actuators.Loggers.LoggersRequest.Type` | Property | Steeltoe.Management.Endpoint | Added | | Indicates actuator request type (get or change) |
| `Steeltoe.Management.Endpoint.Actuators.Loggers.LoggersRequestType` | Type | Steeltoe.Management.Endpoint | Added | | Indicates actuator request type (get or change) |
| `Steeltoe.Management.Endpoint.Actuators.Loggers.LoggersResponse` | Type | Steeltoe.Management.Endpoint | Added | | Data for outgoing actuator response |
| `Steeltoe.Management.Endpoint.Actuators.Refresh.IRefreshEndpointHandler` | Type | Steeltoe.Management.Endpoint | Added | | Enables custom implementation to execute actuator |
| `Steeltoe.Management.Endpoint.Actuators.RouteMappings.IRouteMappingsEndpointHandler` | Type | Steeltoe.Management.Endpoint | Added | | Enables custom implementation to execute actuator |
| `Steeltoe.Management.Endpoint.Actuators.RouteMappings.ResponseTypes.MediaTypeDescriptor` | Type | Steeltoe.Management.Endpoint | Added | | Used in response structure to mirror Spring |
| `Steeltoe.Management.Endpoint.Actuators.RouteMappings.ResponseTypes.ParameterDescriptor` | Type | Steeltoe.Management.Endpoint | Added | | Used in response structure to mirror Spring |
| `Steeltoe.Management.Endpoint.Actuators.RouteMappings.ResponseTypes.RouteConditionsDescriptor.Headers` | Property | Steeltoe.Management.Endpoint | Added | | Used in response structure to mirror Spring |
| `Steeltoe.Management.Endpoint.Actuators.RouteMappings.ResponseTypes.RouteConditionsDescriptor.Parameters` | Property | Steeltoe.Management.Endpoint | Added | | Used in response structure to mirror Spring |
| `Steeltoe.Management.Endpoint.Actuators.RouteMappings.ResponseTypes.RouteDetailsDescriptor` | Type | Steeltoe.Management.Endpoint | Added | | Used in response structure to mirror Spring |
| `Steeltoe.Management.Endpoint.Actuators.RouteMappings.ResponseTypes.RouteDispatcherServlets` | Type | Steeltoe.Management.Endpoint | Added | | Used in response structure to mirror Spring |
| `Steeltoe.Management.Endpoint.Actuators.RouteMappings.ResponseTypes.RouteHandlerDescriptor` | Type | Steeltoe.Management.Endpoint | Added | | Used in response structure to mirror Spring |
| `Steeltoe.Management.Endpoint.Actuators.RouteMappings.ResponseTypes.RouteMappingContexts` | Type | Steeltoe.Management.Endpoint | Added | | Used in response structure to mirror Spring |
| `Steeltoe.Management.Endpoint.Actuators.RouteMappings.ResponseTypes.RouteMappingsContainer` | Type | Steeltoe.Management.Endpoint | Added | | Used in response structure to mirror Spring |
| `Steeltoe.Management.Endpoint.Actuators.RouteMappings.RouteMappingsEndpointOptions.IncludeActuators` | Property | Steeltoe.Management.Endpoint | Added | | Whether actuator endpoints are included in the response |
| `Steeltoe.Management.Endpoint.Actuators.Services` | Namespace | Steeltoe.Management.Endpoint | Added | | Provides services actuator |
| `Steeltoe.Management.Endpoint.Actuators.Services.EndpointServiceCollectionExtensions.AddServicesActuator` | Extension method | Steeltoe.Management.Endpoint | Added | | Activates the services actuator |
| `Steeltoe.Management.Endpoint.Actuators.Services.IServicesEndpointHandler` | Type | Steeltoe.Management.Endpoint | Added | | Enables custom implementation to execute actuator |
| `Steeltoe.Management.Endpoint.Actuators.Services.ServiceRegistration` | Type | Steeltoe.Management.Endpoint | Added | | Describes an entry in the D/I container |
| `Steeltoe.Management.Endpoint.Actuators.Services.ServicesEndpointOptions` | Type | Steeltoe.Management.Endpoint | Added | | Configuration options for services actuator |
| `Steeltoe.Management.Endpoint.Actuators.ThreadDump.IThreadDumpEndpointHandler` | Type | Steeltoe.Management.Endpoint | Added | | Enables custom implementation to execute actuator |
| `Steeltoe.Management.Endpoint.ActuatorServiceCollectionExtensions.AddAllActuators` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Moved | `Steeltoe.Management.Endpoint.Actuators.All.EndpointServiceCollectionExtensions.AddAllActuators` | For custom CORS, call `services.ConfigureActuatorsCorsPolicy()` |
| `Steeltoe.Management.Endpoint.ActuatorServiceCollectionExtensions.RegisterEndpointOptions` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Redundant |
| `Steeltoe.Management.Endpoint.AllActuatorsStartupFilter` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Moved to internal type `ConfigureActuatorsMiddlewareStartupFilter` |
| `Steeltoe.Management.Endpoint.ApplicationBuilderExtensions.UseActuatorsCorsPolicy` | Extension method | Steeltoe.Management.Endpoint | Added | | Needed only when setting up middleware manually |
| `Steeltoe.Management.Endpoint.ApplicationBuilderExtensions.UseManagementPort` | Extension method | Steeltoe.Management.Endpoint | Added | | Needed only when setting up middleware manually |
| `Steeltoe.Management.Endpoint.CloudFoundry` | Namespace | Steeltoe.Management.Endpoint [Base/Core] | Moved | `Steeltoe.Management.Endpoint.Actuators.CloudFoundry` | |
| `Steeltoe.Management.Endpoint.CloudFoundry.CloudFoundryActuatorStartupFilter` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddCloudFoundryActuator()` | Redundant |
| `Steeltoe.Management.Endpoint.CloudFoundry.CloudFoundryEndpoint` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `ICloudFoundryEndpointHandler` | Moved to internal type `CloudFoundryEndpointHandler` |
| `Steeltoe.Management.Endpoint.CloudFoundry.CloudFoundryEndpointMiddleware` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddCloudFoundryActuator()` | Made internal |
| `Steeltoe.Management.Endpoint.CloudFoundry.CloudFoundryEndpointOptions.CloudFoundryApi` | Property | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `Api` | |
| `Steeltoe.Management.Endpoint.CloudFoundry.CloudFoundryManagementOptions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `CloudFoundryEndpointOptions`, `ManagementOptions` | |
| `Steeltoe.Management.Endpoint.CloudFoundry.CloudFoundrySecurityMiddleware` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `EndpointApplicationBuilderExtensions.UseCloudFoundrySecurity()` | Made internal |
| `Steeltoe.Management.Endpoint.CloudFoundry.CloudFoundrySecurityStartupFilter` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `EndpointApplicationBuilderExtensions.UseCloudFoundrySecurity()` | |
| `Steeltoe.Management.Endpoint.CloudFoundry.ICloudFoundryEndpoint` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `ICloudFoundryEndpointHandler` | |
| `Steeltoe.Management.Endpoint.CloudFoundry.ICloudFoundryOptions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `CloudFoundryEndpointOptions` | |
| `Steeltoe.Management.Endpoint.CloudFoundry.SecurityBase` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Moved to internal type `PermissionsProvider` |
| `Steeltoe.Management.Endpoint.CloudFoundry.SecurityResult` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Made internal |
| `Steeltoe.Management.Endpoint.Configuration.ConfigureEndpointOptions<TOptions>` | Type | Steeltoe.Management.Endpoint | Added | | Derive when implementing custom actuator |
| `Steeltoe.Management.Endpoint.Configuration.IConfigureOptionsWithKey<T>` | Type | Steeltoe.Management.Endpoint | Added | | Contract for loading options in custom actuator |
| `Steeltoe.Management.Endpoint.Configuration.ManagementOptions.SslEnabled` | Property | Steeltoe.Management.Endpoint | Added | | Whether `options.Port` applies to HTTP or HTTPS |
| `Steeltoe.Management.Endpoint.ContentNegotiation.ContentNegotiationExtensions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Redundant |
| `Steeltoe.Management.Endpoint.CoreActuatorServiceCollectionExtensions` | Type | Steeltoe.Management.Endpoint | Added | | Building block to implement custom actuator |
| `Steeltoe.Management.Endpoint.DbMigrations` | Namespace | Steeltoe.Management.Endpoint [Base/Core] | Moved | `Steeltoe.Management.Endpoint.Actuators.DbMigrations` | |
| `Steeltoe.Management.Endpoint.DbMigrations.DbMigrationsEndpoint` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `IDbMigrationsEndpointHandler` | Moved to internal type `DbMigrationsEndpointHandler` |
| `Steeltoe.Management.Endpoint.DbMigrations.DbMigrationsEndpoint.DbMigrationsEndpointHelper` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Redundant |
| `Steeltoe.Management.Endpoint.DbMigrations.DbMigrationsEndpointMiddleware` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddDbMigrationsActuator()` | Made internal |
| `Steeltoe.Management.Endpoint.DbMigrations.DbMigrationsEndpointOptions.KeysToSanitize` | Property | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Was never used |
| `Steeltoe.Management.Endpoint.DbMigrations.DbMigrationsStartupFilter` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddDbMigrationsActuator()` | |
| `Steeltoe.Management.Endpoint.DbMigrations.IDbMigrationsEndpoint` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `IDbMigrationsEndpointHandler` | |
| `Steeltoe.Management.Endpoint.DbMigrations.IDbMigrationsOptions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `DbMigrationsEndpointOptions` | |
| `Steeltoe.Management.Endpoint.Diagnostics.DiagnosticServices` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Moved to internal type `...HttpExchanges.Diagnostics.DiagnosticsService` |
| `Steeltoe.Management.Endpoint.EndpointCollectionConventionBuilder` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.ConfigureActuatorEndpoints()` | |
| `Steeltoe.Management.Endpoint.EndPointExtensions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Moved to internal type `EndpointOptionsExtensions` |
| `Steeltoe.Management.Endpoint.Env` | Namespace | Steeltoe.Management.Endpoint [Base/Core] | Moved | `Steeltoe.Management.Endpoint.Actuators.Environment` | |
| `Steeltoe.Management.Endpoint.Env.EndpointServiceCollectionExtensions.AddEnvActuator` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddEnvironmentActuator()` | |
| `Steeltoe.Management.Endpoint.Env.EnvEndpoint` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `IEnvironmentEndpointHandler` | Moved to internal type `EnvironmentEndpointHandler` |
| `Steeltoe.Management.Endpoint.Env.EnvEndpointMiddleware` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddEnvironmentActuator()` | Made internal |
| `Steeltoe.Management.Endpoint.Env.EnvEndpointOptions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `EnvironmentEndpointOptions` | |
| `Steeltoe.Management.Endpoint.Env.EnvironmentDescriptor` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `EnvironmentResponse` | |
| `Steeltoe.Management.Endpoint.Env.EnvStartupFilter` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddEnvironmentActuator()` | |
| `Steeltoe.Management.Endpoint.Env.GenericHostingEnvironment` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | No longer needed |
| `Steeltoe.Management.Endpoint.Env.IEnvEndpoint` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `IEnvironmentEndpointHandler` | |
| `Steeltoe.Management.Endpoint.Env.IEnvOptions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `EnvironmentEndpointOptions` | |
| `Steeltoe.Management.Endpoint.Env.Sanitizer` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | Use `KeysToSanitize` in configuration | Moved to internal type `Sanitizer` |
| `Steeltoe.Management.Endpoint.Exposure` | Type | Steeltoe.Management.Endpoint [Base/Core] | Moved | `Steeltoe.Management.Endpoint.Configuration.Exposure` | |
| `Steeltoe.Management.Endpoint.Health` | Namespace | Steeltoe.Management.Endpoint [Base/Core] | Moved | `Steeltoe.Management.Endpoint.Actuators.Health` | |
| `Steeltoe.Management.Endpoint.Health.Contributor` | Namespace | Steeltoe.Management.Endpoint [Base/Core] | Moved | `Steeltoe.Management.Endpoint.Actuators.Health.Contributors` | |
| `Steeltoe.Management.Endpoint.Health.Contributor.DiskSpaceContributor` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Made internal |
| `Steeltoe.Management.Endpoint.Health.Contributor.PingHealthContributor` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Made internal |
| `Steeltoe.Management.Endpoint.Health.DefaultHealthAggregator` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Moved to internal type `HealthAggregator` in Steeltoe.Common package |
| `Steeltoe.Management.Endpoint.Health.EndpointServiceCollectionExtensions.AddHealthActuator` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Moved | `builder.Services.AddHealthActuator()` | |
| `Steeltoe.Management.Endpoint.Health.EndpointServiceCollectionExtensions.AddHealthContributors` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Moved | `builder.Services.AddHealthContributor()` | |
| `Steeltoe.Management.Endpoint.Health.HealthCheckExtensions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Moved to internal type `HealthAggregator` in Steeltoe.Common package |
| `Steeltoe.Management.Endpoint.Health.HealthConverter` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | No longer needed |
| `Steeltoe.Management.Endpoint.Health.HealthConverterV3` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | No longer needed |
| `Steeltoe.Management.Endpoint.Health.HealthEndpoint` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `IHealthEndpointHandler` | Moved to internal type `HealthEndpointHandler` |
| `Steeltoe.Management.Endpoint.Health.HealthEndpointCore` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `IHealthEndpointHandler` | Moved to internal type `HealthEndpointHandler` |
| `Steeltoe.Management.Endpoint.Health.HealthEndpointMiddleware` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddHealthActuator()` | Made internal |
| `Steeltoe.Management.Endpoint.Health.HealthRegistrationsAggregator` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | Inject `IHealthAggregator` | Moved to internal type `HealthAggregator` in Steeltoe.Common package |
| `Steeltoe.Management.Endpoint.Health.HealthStartupFilter` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddHealthActuator()` | |
| `Steeltoe.Management.Endpoint.Health.IHealthEndpoint` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `IHealthEndpointHandler` | |
| `Steeltoe.Management.Endpoint.Health.IHealthOptions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `HealthEndpointOptions` | |
| `Steeltoe.Management.Endpoint.Health.IHealthRegistrationsAggregator` | Type | Steeltoe.Management.Endpoint [Base/Core] | Moved | `IHealthAggregator` in Steeltoe.Common package | |
| `Steeltoe.Management.Endpoint.Health.IServiceProviderExtensions.InitializeAvailability` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Made internal |
| `Steeltoe.Management.Endpoint.Health.ShowDetails` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `Steeltoe.Management.Endpoint.Actuators.Health.ShowValues` | Numbers of constants have changed |
| `Steeltoe.Management.Endpoint.HeapDump` | Namespace | Steeltoe.Management.Endpoint [Base/Core] | Moved | `Steeltoe.Management.Endpoint.Actuators.HeapDump` | |
| `Steeltoe.Management.Endpoint.HeapDump.HeapDumpEndpoint` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `IHeapDumpEndpointHandler` | Moved to internal type `HeapDumpEndpointHandler` |
| `Steeltoe.Management.Endpoint.HeapDump.HeapDumpEndpointMiddleware` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddHeapDumpActuator()` | Made internal |
| `Steeltoe.Management.Endpoint.HeapDump.HeapDumper` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Made internal |
| `Steeltoe.Management.Endpoint.HeapDump.HeapDumpStartupFilter` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddHeapDumpActuator()` | |
| `Steeltoe.Management.Endpoint.HeapDump.IHeapDumpEndpoint` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `IHeapDumpEndpointHandler` | |
| `Steeltoe.Management.Endpoint.HeapDump.IHeapDumper` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Made internal |
| `Steeltoe.Management.Endpoint.HeapDump.IHeapDumpOptions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `HeapDumpEndpointOptions` | |
| `Steeltoe.Management.Endpoint.HeapDump.LinuxHeapDumper` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Redundant |
| `Steeltoe.Management.Endpoint.Hypermedia` | Namespace | Steeltoe.Management.Endpoint [Base/Core] | Moved | `Steeltoe.Management.Endpoint.Actuators.Hypermedia` | |
| `Steeltoe.Management.Endpoint.Hypermedia.ActuatorEndpoint` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `IHypermediaEndpointHandler` | Moved to internal type `HypermediaEndpointHandler` |
| `Steeltoe.Management.Endpoint.Hypermedia.ActuatorHypermediaEndpointMiddleware` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddHypermediaActuator()` | Moved to internal type `HypermediaEndpointMiddlewar` |
| `Steeltoe.Management.Endpoint.Hypermedia.ActuatorManagementOptions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | Configure at key `Management:Endpoints:Actuator:Exposure` | |
| `Steeltoe.Management.Endpoint.Hypermedia.EndpointServiceCollectionExtensions.AddActuatorManagementOptions` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Redundant |
| `Steeltoe.Management.Endpoint.Hypermedia.HypermediaService` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Made internal |
| `Steeltoe.Management.Endpoint.Hypermedia.HypermediaStartupFilter` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddHypermediaActuator()` | |
| `Steeltoe.Management.Endpoint.Hypermedia.IActuatorEndpoint` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `IHypermediaEndpointHandler` | |
| `Steeltoe.Management.Endpoint.Hypermedia.IActuatorHypermediaOptions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `HypermediaEndpointOptions` | |
| `Steeltoe.Management.Endpoint.Hypermedia.Links._links` | Property | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `Entries` | |
| `Steeltoe.Management.Endpoint.IEndpointHandler<TRequest, TResponse>` | Type | Steeltoe.Management.Endpoint | Added | | Implement for custom actuator |
| `Steeltoe.Management.Endpoint.Info` | Namespace | Steeltoe.Management.Endpoint [Base/Core] | Moved | `Steeltoe.Management.Endpoint.Actuators.Info` | |
| `Steeltoe.Management.Endpoint.Info.Contributor.AppSettingsInfoContributor` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Made internal |
| `Steeltoe.Management.Endpoint.Info.Contributor.BuildInfoContributor` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Made internal |
| `Steeltoe.Management.Endpoint.Info.Contributor.GitInfoContributor` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Made internal |
| `Steeltoe.Management.Endpoint.Info.IInfoEndpoint` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `IInfoEndpointHandler` | |
| `Steeltoe.Management.Endpoint.Info.IInfoOptions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `InfoEndpointOptions` | |
| `Steeltoe.Management.Endpoint.Info.InfoEndpoint` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `IInfoEndpointHandler` | Moved to internal type `InfoEndpointHandler` |
| `Steeltoe.Management.Endpoint.Info.InfoEndpointMiddleware` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddInfoActuator()` | Made internal |
| `Steeltoe.Management.Endpoint.Info.InfoStartupFilter` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddInfoActuator()` | |
| `Steeltoe.Management.Endpoint.Loggers` | Namespace | Steeltoe.Management.Endpoint [Base/Core] | Moved | `Steeltoe.Management.Endpoint.Actuators.Loggers` | |
| `Steeltoe.Management.Endpoint.Loggers.ILoggersEndpoint` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `ILoggersEndpointHandler` | |
| `Steeltoe.Management.Endpoint.Loggers.ILoggersOptions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `LoggersEndpointOptions` | |
| `Steeltoe.Management.Endpoint.Loggers.LoggerLevels.MapLogLevel` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Made internal |
| `Steeltoe.Management.Endpoint.Loggers.LoggersChangeRequest` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `LoggersRequest` | |
| `Steeltoe.Management.Endpoint.Loggers.LoggersEndpoint` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `ILoggersEndpointHandler` | Moved to internal type `LoggersEndpointHandler` |
| `Steeltoe.Management.Endpoint.Loggers.LoggersEndpointMiddleware` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddLoggersActuator()` | Made internal |
| `Steeltoe.Management.Endpoint.Loggers.LoggersStartupFilter` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddLoggersActuator()` | |
| `Steeltoe.Management.Endpoint.ManagementEndpointOptions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Moved | `Steeltoe.Management.Endpoint.Configuration.ManagementOptions` | |
| `Steeltoe.Management.Endpoint.ManagementEndpointOptions.EndpointOptions` | Property | Steeltoe.Management.Endpoint [Base/Core] | Removed | `EndpointMiddleware<,>.EndpointOptions` or inject `IOptionsMonitor` | |
| `Steeltoe.Management.Endpoint.ManagementEndpointOptions.Sensitive` | Property | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Was never used |
| `Steeltoe.Management.Endpoint.ManagementHostBuilderExtensions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | Use extension methods on `IServiceCollection` | Redundant |
| `Steeltoe.Management.Endpoint.ManagementPort.ManagementPortMiddleware` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Made internal |
| `Steeltoe.Management.Endpoint.ManagementWebApplicationBuilderExtensions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | Use extension methods on `IServiceCollection` | Redundant |
| `Steeltoe.Management.Endpoint.ManagementWebHostBuilderExtensions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | Use extension methods on `IServiceCollection` | Redundant |
| `Steeltoe.Management.Endpoint.Mappings` | Namespace | Steeltoe.Management.Endpoint [Base/Core] | Moved | `Steeltoe.Management.Endpoint.Actuators.RouteMappings` | |
| `Steeltoe.Management.Endpoint.Mappings.ApplicationMappings` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `RouteMappingsResponse` | |
| `Steeltoe.Management.Endpoint.Mappings.AspNetCoreRouteDetails` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `RouteConditionsDescriptor` | |
| `Steeltoe.Management.Endpoint.Mappings.AspNetCoreRouteDetails.HttpMethods` | Property | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `RouteConditionsDescriptor.Methods` | |
| `Steeltoe.Management.Endpoint.Mappings.AspNetCoreRouteDetails.RouteTemplate` | Property | Steeltoe.Management.Endpoint [Base/Core] | Removed | `RouteConditionsDescriptor.Patterns` | |
| `Steeltoe.Management.Endpoint.Mappings.ContextMappings` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `RouteMappingContext` | |
| `Steeltoe.Management.Endpoint.Mappings.EndpointServiceCollectionExtensions.AddMappingsActuator` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `AddRouteMappingsActuator` | |
| `Steeltoe.Management.Endpoint.Mappings.IMappingsOptions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `RouteMappingsEndpointOptions` | |
| `Steeltoe.Management.Endpoint.Mappings.IRouteDetails` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `RouteConditionsDescriptor` | Redundant |
| `Steeltoe.Management.Endpoint.Mappings.IRouteMappings` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Conventional routes are no longer shown in mappings actuator |
| `Steeltoe.Management.Endpoint.Mappings.MappingDescription` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `RouteDescriptor` | |
| `Steeltoe.Management.Endpoint.Mappings.MappingsEndpoint` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `IRouteMappingsEndpointHandler` | Moved to internal type `RouteMappingsEndpointHandler` |
| `Steeltoe.Management.Endpoint.Mappings.MappingsEndpointMiddleware` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddRouteMappingsActuator()` | Moved to internal type `RouteMappingsEndpointMiddleware` |
| `Steeltoe.Management.Endpoint.Mappings.MappingsEndpointOptions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `RouteMappingsEndpointOptions` | |
| `Steeltoe.Management.Endpoint.Mappings.MappingsStartupFilter` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddRouteMappingsActuator()` | |
| `Steeltoe.Management.Endpoint.Mappings.RouteBuilderExtensions.AddRoutesToMappingsActuator` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Conventional routes are no longer shown in mappings actuator |
| `Steeltoe.Management.Endpoint.Mappings.RouteMappings` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Conventional routes are no longer shown in mappings actuator |
| `Steeltoe.Management.Endpoint.MediaTypeVersion` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Each actuator uses built-in media type |
| `Steeltoe.Management.Endpoint.Metrics` | Namespace | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Metrics actuator has been removed |
| `Steeltoe.Management.Endpoint.Metrics.IPrometheusEndpointOptions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `PrometheusEndpointOptions` in Steeltoe.Management.Prometheus package | |
| `Steeltoe.Management.Endpoint.Metrics.PrometheusEndpointOptions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Moved | `PrometheusEndpointOptions` in Steeltoe.Management.Prometheus package | |
| `Steeltoe.Management.Endpoint.Metrics.PrometheusEndpointOptions.ScrapeResponseCacheDurationMilliseconds` | Property | Steeltoe.Management.Endpoint [Base/Core] | Removed | | Was never used |
| `Steeltoe.Management.Endpoint.Middleware.ActuatorMetadataProvider` | Type | Steeltoe.Management.Endpoint | Added | | Default implementation to provide metadata for route mappings actuator |
| `Steeltoe.Management.Endpoint.Middleware.EndpointMiddleware<TRequest, TResponse>.CanInvoke` | Method | Steeltoe.Management.Endpoint | Added | | Verifies enabled/exposed based on request path |
| `Steeltoe.Management.Endpoint.Middleware.EndpointMiddleware<TRequest, TResponse>.GetMetadataProvider` | Method | Steeltoe.Management.Endpoint | Added | | Provides metadata for route mappings actuator |
| `Steeltoe.Management.Endpoint.Middleware.EndpointMiddleware<TRequest, TResponse>.InvokeAsync` | Method | Steeltoe.Management.Endpoint | Added | | `IMiddleware` interface implementation |
| `Steeltoe.Management.Endpoint.Middleware.EndpointMiddleware<TRequest, TResponse>.ParseRequestAsync` | Method | Steeltoe.Management.Endpoint | Added | | Creates `TRequest` instance from `HttpContext` |
| `Steeltoe.Management.Endpoint.Middleware.EndpointMiddleware<TResult, TRequest>` | Type | Steeltoe.Management.Endpoint [Base/Core] | Changed | `EndpointMiddleware<TRequest, TResponse>` | Type parameter order reversed |
| `Steeltoe.Management.Endpoint.Middleware.EndpointMiddleware<TResult>` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `EndpointMiddleware<TRequest, TResponse>` | Pass `object?`/`null` for unused `TRequest` |
| `Steeltoe.Management.Endpoint.Middleware.EndpointMiddleware<TResult>._endpoint` | Field | Steeltoe.Management.Endpoint [Base/Core] | Removed | `EndpointMiddleware<TRequest, TResponse>.EndpointHandler` | |
| `Steeltoe.Management.Endpoint.Middleware.EndpointMiddleware<TResult>._logger` | Field | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | |
| `Steeltoe.Management.Endpoint.Middleware.EndpointMiddleware<TResult>._mgmtOptions` | Field | Steeltoe.Management.Endpoint [Base/Core] | Removed | `EndpointMiddleware<TRequest, TResponse>.ManagementOptionsMonitor` | |
| `Steeltoe.Management.Endpoint.Middleware.EndpointMiddleware<TResult>.Endpoint` | Property | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `EndpointMiddleware<TRequest, TResponse>.EndpointHandler` | |
| `Steeltoe.Management.Endpoint.Middleware.EndpointMiddleware<TResult>.HandleRequest` | Method | Steeltoe.Management.Endpoint [Base/Core] | Removed | `EndpointMiddleware<TRequest, TResponse>.InvokeEndpointHandlerAsync` | |
| `Steeltoe.Management.Endpoint.Middleware.EndpointMiddleware<TResult>.Serialize` | Method | Steeltoe.Management.Endpoint [Base/Core] | Removed | `EndpointMiddleware<TRequest, TResponse>.WriteResponseAsync` | |
| `Steeltoe.Management.Endpoint.Middleware.IEndpointMiddleware` | Type | Steeltoe.Management.Endpoint | Added | | Implement for custom actuator |
| `Steeltoe.Management.Endpoint.Refresh` | Namespace | Steeltoe.Management.Endpoint [Base/Core] | Moved | `Steeltoe.Management.Endpoint.Actuators.Refresh` | |
| `Steeltoe.Management.Endpoint.Refresh.IRefreshEndpoint` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `IRefreshEndpointHandler` | |
| `Steeltoe.Management.Endpoint.Refresh.IRefreshOptions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `RefreshEndpointOptions` | |
| `Steeltoe.Management.Endpoint.Refresh.RefreshEndpoint` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `IRefreshEndpointHandler` | Moved to internal type `RefreshEndpointHandler` |
| `Steeltoe.Management.Endpoint.Refresh.RefreshEndpointMiddleware` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddRefreshActuator()` | Made internal |
| `Steeltoe.Management.Endpoint.Refresh.RefreshStartupFilter` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddRefreshActuator()` | |
| `Steeltoe.Management.Endpoint.Security.EndpointClaim` | Type | Steeltoe.Management.Endpoint [Base/Core] | Moved | `Steeltoe.Management.Endpoint.Actuators.Health.EndpointClaim` | |
| `Steeltoe.Management.Endpoint.Security.ISecurityContext` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Redundant |
| `Steeltoe.Management.Endpoint.SpringBootAdminClient.SpringBootAdminApplicationBuilderExtensions.RegisterWithSpringBootAdmin` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddSpringBootAdminClient()` | |
| `Steeltoe.Management.Endpoint.SpringBootAdminClient.SpringBootAdminClientOptions.ConnectionTimeoutMS` | Property | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `SpringBootAdminClientOptions.ConnectionTimeoutMs` | |
| `Steeltoe.Management.Endpoint.ThreadDump` | Namespace | Steeltoe.Management.Endpoint [Base/Core] | Moved | `Steeltoe.Management.Endpoint.Actuators.ThreadDump` | |
| `Steeltoe.Management.Endpoint.ThreadDump.IThreadDumpEndpoint` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `IThreadDumpEndpointHandler` | |
| `Steeltoe.Management.Endpoint.ThreadDump.IThreadDumpEndpointV2` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `IThreadDumpEndpointHandler` | Redundant |
| `Steeltoe.Management.Endpoint.ThreadDump.IThreadDumper` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Redundant |
| `Steeltoe.Management.Endpoint.ThreadDump.IThreadDumpOptions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `ThreadDumpEndpointOptions` | |
| `Steeltoe.Management.Endpoint.ThreadDump.LockInfo` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Was never used |
| `Steeltoe.Management.Endpoint.ThreadDump.MetaDataImportProvider` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Was never used |
| `Steeltoe.Management.Endpoint.ThreadDump.MonitorInfo` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Was never used |
| `Steeltoe.Management.Endpoint.ThreadDump.ThreadDumpEndpoint` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `IThreadDumpEndpointHandler` | Moved to internal type `ThreadDumpEndpointHandler` |
| `Steeltoe.Management.Endpoint.ThreadDump.ThreadDumpEndpoint_v2` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `IThreadDumpEndpointHandler` | Redundant |
| `Steeltoe.Management.Endpoint.ThreadDump.ThreadDumpEndpointMiddleware` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddThreadDumpActuator()` | Made internal |
| `Steeltoe.Management.Endpoint.ThreadDump.ThreadDumpEndpointMiddleware_v2` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddThreadDumpActuator()` | Redundant |
| `Steeltoe.Management.Endpoint.ThreadDump.ThreadDumperEP` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Renamed to internal type `EventPipeThreadDumper` |
| `Steeltoe.Management.Endpoint.ThreadDump.ThreadDumpResult` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Redundant |
| `Steeltoe.Management.Endpoint.ThreadDump.ThreadDumpStartupFilter` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddThreadDumpActuator()` | |
| `Steeltoe.Management.Endpoint.ThreadDump.TState` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `State` | |
| `Steeltoe.Management.Endpoint.Trace` | Namespace | Steeltoe.Management.Endpoint [Base/Core] | Moved | `Steeltoe.Management.Endpoint.Actuators.HttpExchanges` | |
| `Steeltoe.Management.Endpoint.Trace.EndpointServiceCollectionExtensions.AddTraceActuator` | Extension method | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `AddHttpExchangesActuator` | |
| `Steeltoe.Management.Endpoint.Trace.HttpTrace` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `HttpExchange` | |
| `Steeltoe.Management.Endpoint.Trace.HttpTraceDiagnosticObserver` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Moved to internal type `HttpExchangesDiagnosticObserver` |
| `Steeltoe.Management.Endpoint.Trace.HttpTraceEndpoint` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `IHttpExchangesEndpointHandler` | Moved to internal type `HttpExchangesEndpointHandler` |
| `Steeltoe.Management.Endpoint.Trace.HttpTraceEndpointMiddleware` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddHttpExchangesActuator()` | Moved to internal type `HttpExchangesEndpointMiddleware` |
| `Steeltoe.Management.Endpoint.Trace.HttpTraceEndpointOptions.AddAuthType` | Property | Steeltoe.Management.Endpoint [Base/Core] | Removed | `HttpExchangesEndpointOptions.IncludeRequestHeaders` | Was effectively ignored |
| `Steeltoe.Management.Endpoint.Trace.HttpTraceEndpointOptions.AddParameters` | Property | Steeltoe.Management.Endpoint [Base/Core] | Removed | `HttpExchangesEndpointOptions.IncludeQueryString` | Form data is no longer included |
| `Steeltoe.Management.Endpoint.Trace.HttpTraceEndpointOptions.AddPathInfo` | Property | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `HttpExchangesEndpointOptions.IncludePathInfo` | |
| `Steeltoe.Management.Endpoint.Trace.HttpTraceEndpointOptions.AddQueryString` | Property | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `HttpExchangesEndpointOptions.IncludeQueryString` | |
| `Steeltoe.Management.Endpoint.Trace.HttpTraceEndpointOptions.AddRemoteAddress` | Property | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `HttpExchangesEndpointOptions.IncludeRemoteAddress` | |
| `Steeltoe.Management.Endpoint.Trace.HttpTraceEndpointOptions.AddRequestHeaders` | Property | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `HttpExchangesEndpointOptions.IncludeRequestHeaders` | |
| `Steeltoe.Management.Endpoint.Trace.HttpTraceEndpointOptions.AddResponseHeaders` | Property | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `HttpExchangesEndpointOptions.IncludeResponseHeaders` | |
| `Steeltoe.Management.Endpoint.Trace.HttpTraceEndpointOptions.AddSessionId` | Property | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `HttpExchangesEndpointOptions.IncludeSessionId` | |
| `Steeltoe.Management.Endpoint.Trace.HttpTraceEndpointOptions.AddTimeTaken` | Property | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `HttpExchangesEndpointOptions.IncludeTimeTaken` | |
| `Steeltoe.Management.Endpoint.Trace.HttpTraceEndpointOptions.AddUserPrincipal` | Property | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `HttpExchangesEndpointOptions.IncludeUserPrincipal` | |
| `Steeltoe.Management.Endpoint.Trace.HttpTraceResult` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `HttpExchangesResult` | |
| `Steeltoe.Management.Endpoint.Trace.IHttpTraceEndpoint` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `IHttpExchangesEndpointHandler` | |
| `Steeltoe.Management.Endpoint.Trace.IHttpTraceRepository` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `IHttpExchangesRepository` | |
| `Steeltoe.Management.Endpoint.Trace.IHttpTraceRepository.GetTraces` | Method | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `IHttpExchangesRepository.GetHttpExchanges` | |
| `Steeltoe.Management.Endpoint.Trace.ITraceEndpoint` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `IHttpExchangesEndpointHandler` | |
| `Steeltoe.Management.Endpoint.Trace.ITraceOptions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `HttpExchangesEndpointOptions` | |
| `Steeltoe.Management.Endpoint.Trace.ITraceRepository` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `IHttpExchangesRepository` | |
| `Steeltoe.Management.Endpoint.Trace.ITraceRepository.GetTraces` | Method | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `IHttpExchangesRepository.GetHttpExchanges` | |
| `Steeltoe.Management.Endpoint.Trace.Principal` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `HttpExchangePrincipal` | |
| `Steeltoe.Management.Endpoint.Trace.Request` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `HttpExchangeRequest` | |
| `Steeltoe.Management.Endpoint.Trace.Response` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `HttpExchangeResponse` | |
| `Steeltoe.Management.Endpoint.Trace.Session` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `HttpExchangeSession` | |
| `Steeltoe.Management.Endpoint.Trace.TraceDiagnosticObserver` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Moved to internal type `HttpExchangesDiagnosticObserver` |
| `Steeltoe.Management.Endpoint.Trace.TraceEndpoint` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `IHttpExchangesEndpointHandler` | Moved to internal type `HttpExchangesEndpointHandler` |
| `Steeltoe.Management.Endpoint.Trace.TraceEndpointMiddleware` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddHttpExchangesActuator()` | Moved to internal type `HttpExchangesEndpointMiddleware` |
| `Steeltoe.Management.Endpoint.Trace.TraceEndpointOptions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Renamed | `HttpExchangesEndpointOptions` | |
| `Steeltoe.Management.Endpoint.Trace.TraceEndpointOptions` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `HttpExchangesEndpointOptions` | |
| `Steeltoe.Management.Endpoint.Trace.TraceResult` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `HttpExchange` | |
| `Steeltoe.Management.Endpoint.Trace.TraceStartupFilter` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | `builder.Services.AddHttpExchangesActuator()` | |
| `Steeltoe.Management.Endpoint.Utils` | Type | Steeltoe.Management.Endpoint [Base/Core] | Removed | None | Redundant |
| `Steeltoe.Management.IEndpoint` | Type | Steeltoe.Management.Abstractions | Removed | `IEndpointHandler<,>` in Steeltoe.Management.Endpoint package | |
| `Steeltoe.Management.IEndpointOptions` | Type | Steeltoe.Management.Abstractions | Removed | `Steeltoe.Management.Configuration.EndpointOptions` | |
| `Steeltoe.Management.IManagementOptions` | Type | Steeltoe.Management.Abstractions | Removed | `ManagementOptions` in Steeltoe.Management.Endpoint package | |
| `Steeltoe.Management.Info.AbstractConfigurationContributor` | Type | Steeltoe.Management.Abstractions | Removed | None | Moved to internal type `ConfigurationContributor` |
| `Steeltoe.Management.Info.IInfoBuilder` | Type | Steeltoe.Management.Abstractions | Removed | `InfoBuilder` in Steeltoe.Management.Endpoint package | Redundant |
| `Steeltoe.Management.Info.IInfoContributor` | Type | Steeltoe.Management.Abstractions | Moved | `IInfoContributor` in Steeltoe.Management.Endpoint package | |
| `Steeltoe.Management.Info.IInfoContributor.Contribute` | Method | Steeltoe.Management.Abstractions | Moved | `ContributeAsync()` | |
| `Steeltoe.Management.Info.InfoBuilder` | Type | Steeltoe.Management.Abstractions | Moved | `InfoBuilder` in Steeltoe.Management.Endpoint package | |
| `Steeltoe.Management.Kubernetes` | Namespace | Steeltoe.Management.KubernetesCore | Removed | None | |
| `Steeltoe.Management.OpenTelemetry` | Namespace | Steeltoe.Management.OpenTelemetryBase | Removed | `builder.Services.AddPrometheusActuator()` from Steeltoe.Management.Prometheus package | WaveFront exporter was removed |
| `Steeltoe.Management.Permissions` | Type | Steeltoe.Management.Abstractions | Renamed | `EndpointPermissions` | |
| `Steeltoe.Management.Permissions.UNDEFINED` | Field | Steeltoe.Management.Abstractions | Removed | None | Default in options changed to Restricted |
| `Steeltoe.Management.Prometheus.PrometheusEndpointOptions` | Type | Steeltoe.Management.Prometheus | Added | | Configuration for Prometheus actuator |
| `Steeltoe.Management.Prometheus.PrometheusExtensions.AddPrometheusActuator` | Extension method | Steeltoe.Management.Prometheus | Added | | Add Prometheus actuator to service collection |
| `Steeltoe.Management.Prometheus.PrometheusExtensions.UsePrometheusActuator` | Extension method | Steeltoe.Management.Prometheus | Added | | Activate Prometheus actuator middleware |
| `Steeltoe.Management.TaskCore` | Namespace | Steeltoe.Management.TaskCore | Moved | `Steeltoe.Management.Tasks` | |
| `Steeltoe.Management.TaskCore.DelegatingTask` | Type | Steeltoe.Management.TaskCore | Removed | `builder.Services.AddTask()` | Made internal |
| `Steeltoe.Management.TaskCore.TaskWebHostExtensions.RunWithTasks` | Extension method | Steeltoe.Management.TaskCore | Renamed | `RunWithTasksAsync` | |
| `Steeltoe.Management.Tasks.TaskHostExtensions.HasApplicationTask` | Extension method | Steeltoe.Management.Tasks | Added | | Check whether a task will run |
| `Steeltoe.Management.Tracing.TracingBaseHostBuilderExtensions.AddDistributedTracing` | Extension method | Steeltoe.Management.Tracing [Base/Core] | Removed | Use OpenTelemetry packages directly | |
| `Steeltoe.Management.Tracing.TracingBaseServiceCollectionExtensions.AddDistributedTracing` | Extension method | Steeltoe.Management.Tracing [Base/Core] | Removed | Use OpenTelemetry packages directly | |
| `Steeltoe.Management.Tracing.TracingCoreServiceCollectionExtensions.AddDistributedTracingAspNetCore` | Extension method | Steeltoe.Management.Tracing [Base/Core] | Removed | Use OpenTelemetry packages directly | |
| `Steeltoe.Management.Tracing.TracingHostBuilderExtensions.AddDistributedTracincAspNetCore` | Extension method | Steeltoe.Management.Tracing [Base/Core] | Removed | Use OpenTelemetry packages directly | |
| `Steeltoe.Management.Tracing.TracingLogProcessor.GetCurrentSpan` | Method | Steeltoe.Management.Tracing [Base/Core] | Removed | None | Redundant |
| `Steeltoe.Management.Tracing.TracingOptions` | Type | Steeltoe.Management.Tracing [Base/Core] | Removed | Use OpenTelemetry packages directly | |
| `Steeltoe.Management.Tracing.TracingServiceCollectionExtensions.AddTracingLogProcessor` | Extension method | Steeltoe.Management.Tracing | Added | | Add trace info from `Activity.Current` to logs |

### Notable PRs

- https://github.com/SteeltoeOSS/Steeltoe/pull/1521
- https://github.com/SteeltoeOSS/Steeltoe/pull/1520
- https://github.com/SteeltoeOSS/Steeltoe/pull/1517
- https://github.com/SteeltoeOSS/Steeltoe/pull/1508
- https://github.com/SteeltoeOSS/Steeltoe/pull/1503
- https://github.com/SteeltoeOSS/Steeltoe/pull/1490
- https://github.com/SteeltoeOSS/Steeltoe/pull/1474
- https://github.com/SteeltoeOSS/Steeltoe/pull/1457
- https://github.com/SteeltoeOSS/Steeltoe/pull/1454
- https://github.com/SteeltoeOSS/Steeltoe/pull/1451
- https://github.com/SteeltoeOSS/Steeltoe/pull/1444
- https://github.com/SteeltoeOSS/Steeltoe/pull/1443
- https://github.com/SteeltoeOSS/Steeltoe/pull/1438
- https://github.com/SteeltoeOSS/Steeltoe/pull/1424
- https://github.com/SteeltoeOSS/Steeltoe/pull/1422
- https://github.com/SteeltoeOSS/Steeltoe/pull/1421
- https://github.com/SteeltoeOSS/Steeltoe/pull/1417
- https://github.com/SteeltoeOSS/Steeltoe/pull/1416
- https://github.com/SteeltoeOSS/Steeltoe/pull/1413
- https://github.com/SteeltoeOSS/Steeltoe/pull/1402
- https://github.com/SteeltoeOSS/Steeltoe/pull/1401
- https://github.com/SteeltoeOSS/Steeltoe/pull/1398
- https://github.com/SteeltoeOSS/Steeltoe/pull/1396
- https://github.com/SteeltoeOSS/Steeltoe/pull/1393
- https://github.com/SteeltoeOSS/Steeltoe/pull/1392
- https://github.com/SteeltoeOSS/Steeltoe/pull/1390
- https://github.com/SteeltoeOSS/Steeltoe/pull/1389
- https://github.com/SteeltoeOSS/Steeltoe/pull/1386
- https://github.com/SteeltoeOSS/Steeltoe/pull/1385
- https://github.com/SteeltoeOSS/Steeltoe/pull/1382
- https://github.com/SteeltoeOSS/Steeltoe/pull/1380
- https://github.com/SteeltoeOSS/Steeltoe/pull/1378
- https://github.com/SteeltoeOSS/Steeltoe/pull/1364
- https://github.com/SteeltoeOSS/Steeltoe/pull/1357
- https://github.com/SteeltoeOSS/Steeltoe/pull/1356
- https://github.com/SteeltoeOSS/Steeltoe/pull/1353
- https://github.com/SteeltoeOSS/Steeltoe/pull/1331
- https://github.com/SteeltoeOSS/Steeltoe/pull/1278
- https://github.com/SteeltoeOSS/Steeltoe/pull/1247
- https://github.com/SteeltoeOSS/Steeltoe/pull/1224
- https://github.com/SteeltoeOSS/Steeltoe/pull/1198
- https://github.com/SteeltoeOSS/Steeltoe/pull/1187
- https://github.com/SteeltoeOSS/Steeltoe/pull/1185
- https://github.com/SteeltoeOSS/Steeltoe/pull/1184
- https://github.com/SteeltoeOSS/Steeltoe/pull/1177
- https://github.com/SteeltoeOSS/Steeltoe/pull/1165
- https://github.com/SteeltoeOSS/Steeltoe/pull/1155
- https://github.com/SteeltoeOSS/Steeltoe/pull/1130
- https://github.com/SteeltoeOSS/Steeltoe/pull/1120
- https://github.com/SteeltoeOSS/Steeltoe/pull/1114
- https://github.com/SteeltoeOSS/Steeltoe/pull/1101
- https://github.com/SteeltoeOSS/Steeltoe/pull/1065
- https://github.com/SteeltoeOSS/Steeltoe/pull/1050

### Documentation

For more information, see the updated [Management documentation](../management/index.md) and
[Management samples](https://github.com/SteeltoeOSS/Samples/tree/main/Management).

## Security

### Behavior changes

- Drastically simplified implementation, leveraging the built-in ASP.NET option types
- Dropped OAuth support in favor of OpenID Connect
- Removed CredHub client, use [CredHub Service Broker](https://techdocs.broadcom.com/us/en/vmware-tanzu/platform-services/credhub-service-broker/services/credhub-sb/index.html)

### NuGet Package changes

| Source | Change | Replacement | Notes |
| --- | --- | --- | --- |
| Steeltoe.Security.Authentication.CloudFoundryBase | Removed | Steeltoe.Security.Authentication.JwtBearer, Steeltoe.Security.Authentication.OpenIdConnect, Steeltoe.Security.Authorization.Certificate | Replacement packages are split per auth method |
| Steeltoe.Security.Authentication.CloudFoundryCore | Removed | Steeltoe.Security.Authentication.JwtBearer, Steeltoe.Security.Authentication.OpenIdConnect, Steeltoe.Security.Authorization.Certificate | Replacement packages are split per auth method |
| Steeltoe.Security.Authentication.JwtBearer | Added | | JSON Web Tokens (JWT) for Cloud Foundry |
| Steeltoe.Security.Authentication.MtlsCore | Renamed | Steeltoe.Security.Authorization.Certificate | Client certificate auth for Cloud Foundry |
| Steeltoe.Security.Authentication.OpenIdConnect | Added | | OpenID Connect (OIDC) for Cloud Foundry |
| Steeltoe.Security.DataProtection.CredHubBase | Removed | None | Use CredHub Service Broker |
| Steeltoe.Security.DataProtection.CredHubCore | Removed | None | Use CredHub Service Broker |
| Steeltoe.Security.DataProtection.RedisCore | Renamed | Steeltoe.Security.DataProtection.Redis | |

### API changes

| Source | Kind | Package | Change | Replacement | Notes |
| --- | --- | --- | --- | --- | --- |
| `Steeltoe.Security.Authentication.CloudFoundry.ApplicationBuilderExtensions.UseCloudFoundryCertificateAuth` | Extension method | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Removed | `app.UseCertificateAuthorization()` | |
| `Steeltoe.Security.Authentication.CloudFoundry.ApplicationBuilderExtensions.UseCloudFoundryContainerIdentity` | Extension method | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Removed | `app.UseCertificateAuthorization()` | |
| `Steeltoe.Security.Authentication.CloudFoundry.ApplicationClaimTypes` | Type | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Removed | None | Moved to internal type `ApplicationClaimTypes` |
| `Steeltoe.Security.Authentication.CloudFoundry.AuthenticationBuilderExtensions.AddCloudFoundryIdentityCertificate` | Extension method | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Removed | `builder.Configuration.AddAppInstanceIdentityCertificate(); builder.Services.AddAuthentication().AddCertificate();` | |
| `Steeltoe.Security.Authentication.CloudFoundry.AuthenticationBuilderExtensions.AddCloudFoundryJwtBearer` | Extension method | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Removed | `builder.Services.AddAuthentication().AddJwtBearer().ConfigureJwtBearerForCloudFoundry()` | |
| `Steeltoe.Security.Authentication.CloudFoundry.AuthenticationBuilderExtensions.AddCloudFoundryOAuth` | Extension method | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Removed | Use OpenID Connect or JWT instead | OAuth support has been removed |
| `Steeltoe.Security.Authentication.CloudFoundry.AuthenticationBuilderExtensions.AddCloudFoundryOpenIdConnect` | Extension method | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Removed | `builder.Services.AddAuthentication().AddOpenIdConnect().ConfigureOpenIdConnectForCloudFoundry()` | |
| `Steeltoe.Security.Authentication.CloudFoundry.AuthorizationPolicyBuilderExtensions.SameOrg` | Extension method | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Moved | `AuthorizationPolicyBuilder.RequireSameOrg()` in Steeltoe.Security.Authorization.Certificate package | |
| `Steeltoe.Security.Authentication.CloudFoundry.AuthorizationPolicyBuilderExtensions.SameSpace` | Extension method | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Moved | `AuthorizationPolicyBuilder.RequireSameSpace()` in Steeltoe.Security.Authorization.Certificate package | |
| `Steeltoe.Security.Authentication.CloudFoundry.AuthServerOptions` | Type | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Removed | `JwtBearerOptions`, `OpenIdConnectOptions` | Now uses built-in ASP.NET option types |
| `Steeltoe.Security.Authentication.CloudFoundry.CloudFoundryCertificateIdentityAuthorizationHandler` | Type | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Removed | None | Refactored to internal type `CertificateAuthorizationHandler` |
| `Steeltoe.Security.Authentication.CloudFoundry.CloudFoundryClaimActionExtensions` | Type | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Removed | None | Refactored, no longer needed |
| `Steeltoe.Security.Authentication.CloudFoundry.CloudFoundryDefaults` | Type | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Removed | None | Contained constants that are no longer needed |
| `Steeltoe.Security.Authentication.CloudFoundry.CloudFoundryHelper` | Type | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Removed | None | Refactored, no longer needed |
| `Steeltoe.Security.Authentication.CloudFoundry.CloudFoundryJwtBearerConfigurer` | Type | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Removed | None | Refactored to internal type `PostConfigureJwtBearerOptions` |
| `Steeltoe.Security.Authentication.CloudFoundry.CloudFoundryJwtBearerOptions` | Type | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Removed | `Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerOptions` | Refactored, no longer needed |
| `Steeltoe.Security.Authentication.CloudFoundry.CloudFoundryOAuthConfigurer` | Type | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Removed | None | Refactored to internal type `PostConfigureOpenIdConnectOptions` |
| `Steeltoe.Security.Authentication.CloudFoundry.CloudFoundryOAuthHandler` | Type | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Removed | None | Refactored to internal type `TokenKeyResolver` |
| `Steeltoe.Security.Authentication.CloudFoundry.CloudFoundryOAuthOptions` | Type | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Removed | `Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectOptions` | Refactored, no longer needed |
| `Steeltoe.Security.Authentication.CloudFoundry.CloudFoundryOpenIdConnectOptions` | Type | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Removed | `Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectOptions` | Refactored, no longer needed |
| `Steeltoe.Security.Authentication.CloudFoundry.CloudFoundryScopeClaimAction` | Type | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Removed | None | Refactored, no longer needed |
| `Steeltoe.Security.Authentication.CloudFoundry.CloudFoundryTokenKeyResolver` | Type | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Removed | None | Refactored to internal type `TokenKeyResolver` |
| `Steeltoe.Security.Authentication.CloudFoundry.CloudFoundryTokenValidator` | Type | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Removed | None | Refactored, no longer needed |
| `Steeltoe.Security.Authentication.CloudFoundry.ConfigurationBuilderExtensions.AddCloudFoundryContainerIdentity` | Extension method | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Moved | `builder.Configuration.AddAppInstanceIdentityCertificate()` in Steeltoe.Security.Authorization.Certificate package | |
| `Steeltoe.Security.Authentication.CloudFoundry.MutualTlsAuthenticationOptionsPostConfigurer` | Type | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Removed | None | Refactored to internal type `PostConfigureCertificateAuthenticationOptions` |
| `Steeltoe.Security.Authentication.CloudFoundry.OpenIdTokenResponse` | Type | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Removed | None | Refactored, no longer needed |
| `Steeltoe.Security.Authentication.CloudFoundry.SameOrgRequirement` | Type | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Moved | `SameOrgRequirement` in Steeltoe.Security.Authorization.Certificate package | |
| `Steeltoe.Security.Authentication.CloudFoundry.SameSpaceRequirement` | Type | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Moved | `SameSpaceRequirement` in Steeltoe.Security.Authorization.Certificate package | |
| `Steeltoe.Security.Authentication.CloudFoundry.ServiceCollectionExtensions.AddCloudFoundryContainerIdentity` | Extension method | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Removed | `builder.Services.AddAuthorizationBuilder().AddOrgAndSpacePolicies()` in Steeltoe.Security.Authorization.Certificate package | |
| `Steeltoe.Security.Authentication.CloudFoundry.TokenExchanger` | Type | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Removed | None | Refactored, no longer needed |
| `Steeltoe.Security.Authentication.JwtBearer.JwtBearerAuthenticationBuilderExtensions.ConfigureJwtBearerForCloudFoundry` | Extension method | Steeltoe.Security.Authentication.JwtBearer | Added | | Configure JWT for UAA-based systems like Cloud Foundry |
| `Steeltoe.Security.Authentication.Mtls.CertificateApplicationBuilderExtensions.UseCertificateRotation` | Extension method | Steeltoe.Security.Authentication.MtlsCore | Removed | None | Rotating certificates in OS-level certificate store proved to be unreliable |
| `Steeltoe.Security.Authentication.Mtls.CertificateAuthenticationBuilderExtensions.AddMutualTls` | Extension method | Steeltoe.Security.Authentication.MtlsCore | Removed | `app.UseCertificateAuthorization()` | |
| `Steeltoe.Security.Authentication.Mtls.CertificateRotationHostedService` | Type | Steeltoe.Security.Authentication.MtlsCore | Removed | None | Rotating certificates in OS-level certificate store proved to be unreliable |
| `Steeltoe.Security.Authentication.Mtls.CloudFoundryInstanceCertificate` | Type | Steeltoe.Security.Authentication.CloudFoundry [Base/Core] | Removed | None | Moved to internal type `ApplicationInstanceCertificate` |
| `Steeltoe.Security.Authentication.Mtls.MutualTlsAuthenticationOptions` | Type | Steeltoe.Security.Authentication.MtlsCore | Removed | `Microsoft.AspNetCore.Authentication.Certificate.CertificateAuthenticationOptions` | |
| `Steeltoe.Security.Authentication.Mtls.MutualTlsAuthenticationOptions.IssuerChain` | Property | Steeltoe.Security.Authentication.MtlsCore | Removed | `CertificateOptions.IssuerChain` in Steeltoe.Common.Certificates package | |
| `Steeltoe.Security.Authentication.OpenIdConnect.OpenIdConnectAuthenticationBuilderExtensions.ConfigureOpenIdConnectForCloudFoundry` | Extension method | Steeltoe.Security.Authentication.OpenIdConnect | Added | | Configure OIDC for UAA-based systems like Cloud Foundry |
| `Steeltoe.Security.Authorization.Certificate.CertificateApplicationBuilderExtensions.UseCertificateAuthorization` | Extension method | Steeltoe.Security.Authorization.Certificate | Added | | Activates cert/header forwarding in ASP.NET Core auth middleware |
| `Steeltoe.Security.Authorization.Certificate.CertificateAuthorizationBuilderExtensions.AddOrgAndSpacePolicies` | Extension method | Steeltoe.Security.Authorization.Certificate | Added | | Verify space/org in the incoming client certificate |
| `Steeltoe.Security.Authorization.Certificate.CertificateAuthorizationPolicies.SameOrganization` | Property | Steeltoe.Security.Authorization.Certificate | Added | | Constant for same-org policy |
| `Steeltoe.Security.Authorization.Certificate.CertificateAuthorizationPolicies.SameSpace` | Property | Steeltoe.Security.Authorization.Certificate | Added | | Constant for same-space policy |
| `Steeltoe.Security.Authorization.Certificate.CertificateAuthorizationPolicyBuilderExtensions.RequireSameOrg` | Extension method | Steeltoe.Security.Authorization.Certificate | Added | | Require client certificate to originate from same organization |
| `Steeltoe.Security.Authorization.Certificate.CertificateAuthorizationPolicyBuilderExtensions.RequireSameSpace` | Extension method | Steeltoe.Security.Authorization.Certificate | Added | | Require client certificate to originate from same space |
| `Steeltoe.Security.Authorization.Certificate.CertificateHttpClientBuilderExtensions.AddAppInstanceIdentityCertificate` | Extension method | Steeltoe.Security.Authorization.Certificate | Added | | Send app-identify certificate with outgoing HTTP requests |
| `Steeltoe.Security.Authorization.Certificate.CertificateHttpClientBuilderExtensions.AddClientCertificate` | Extension method | Steeltoe.Security.Authorization.Certificate | Added | | Send custom certificate with outgoing HTTP requests |
| `Steeltoe.Security.Authorization.Certificate.SameOrgRequirement` | Type | Steeltoe.Security.Authorization.Certificate | Added | | Authorization requirement for same-org |
| `Steeltoe.Security.Authorization.Certificate.SameSpaceRequirement` | Type | Steeltoe.Security.Authorization.Certificate | Added | | Authorization requirement for same-space |
| `Steeltoe.Security.DataProtection.Redis.CloudFoundryRedisXmlRepository` | Type | Steeltoe.Security.DataProtection.RedisCore | Removed | | No longer needed |
| `Steeltoe.Security.DataProtection.Redis.RedisDataProtectionBuilderExtensions.PersistKeysToRedis` | Extension method | Steeltoe.Security.DataProtection.Redis | Added | | Takes an optional service binding name |
| `Steeltoe.Security.DataProtection.RedisDataProtectionBuilderExtensions.PersistKeysToRedis` | Extension method | Steeltoe.Security.DataProtection.RedisCore | Moved | `PersistKeysToRedis()` in Steeltoe.Security.DataProtection.Redis package | |

### Notable PRs

- https://github.com/SteeltoeOSS/Steeltoe/pull/1525
- https://github.com/SteeltoeOSS/Steeltoe/pull/1452
- https://github.com/SteeltoeOSS/Steeltoe/pull/1349
- https://github.com/SteeltoeOSS/Steeltoe/pull/1336
- https://github.com/SteeltoeOSS/Steeltoe/pull/1311
- https://github.com/SteeltoeOSS/Steeltoe/pull/1306
- https://github.com/SteeltoeOSS/Steeltoe/pull/1232
- https://github.com/SteeltoeOSS/Steeltoe/pull/1098

### Documentation

For more information, see the updated [Security documentation](../security/index.md) and
[Discovery samples](https://github.com/SteeltoeOSS/Samples/tree/main/Security).

## Release Notes

Release notes for all releases can be found on the [Steeltoe releases](https://github.com/SteeltoeOSS/Steeltoe/releases) section on GitHub.
