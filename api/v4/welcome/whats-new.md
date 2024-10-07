# What's New in Steeltoe 4

## New Features and Changes

### Steeltoe 4.0.0

## Package Name Changes

| Steeltoe 3.x | Steeltoe 4.x |
| ------------ | ------------ |
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
| Steeltoe.Connector.EFCore, Steeltoe.Connector.EF6Core | Steeltoe.Connectors.EntityFrameworkCore |
| Steeltoe.Discovery.Abstractions | Steeltoe.Common |
| Steeltoe.Discovery.ClientBase, Steeltoe.Discovery.ClientCore | Steeltoe.Discovery.HttpClients |
| Steeltoe.Discovery.Kubernetes | - |
| Steeltoe.Extensions.Configuration.Abstractions | Steeltoe.Configuration.Abstractions |
| Steeltoe.Extensions.Configuration.CloudFoundryBase, Steeltoe.Extensions.Configuration.CloudFoundryCore | Steeltoe.Configuration.CloudFoundry |
| Steeltoe.Extensions.Configuration.ConfigServerBase, Steeltoe.Extensions.Configuration.ConfigServerCore | Steeltoe.Configuration.ConfigServer |
| Steeltoe.Extensions.Configuration.Kubernetes.ServiceBinding | Steeltoe.Configuration.Kubernetes.ServiceBindings |
| Steeltoe.Extensions.Configuration.KubernetesBase, Steeltoe.Extensions.Configuration.KubernetesCore | - |
| Steeltoe.Extensions.Configuration.PlaceholderBase, Steeltoe.Extensions.Configuration.PlaceholderCore | Steeltoe.Configuration.Placeholder |
| Steeltoe.Extensions.Configuration.RandomValueBase | Steeltoe.Configuration.RandomValue |
| Steeltoe.Extensions.Configuration.SpringBootBase, Steeltoe.Extensions.Configuration.SpringBootCore |Steeltoe.Configuration.SpringBoot |
| Steeltoe.Extensions.Logging.Abstractions | Steeltoe.Logging.Abstractions |
| Steeltoe.Extensions.Logging.DynamicLogger | Steeltoe.Logging.DynamicLogger |
| Steeltoe.Extensions.Logging.DynamicSerilogBase, Steeltoe.Extensions.Logging.DynamicSerilogCore | Steeltoe.Logging.DynamicSerilog |
| Steeltoe.Integration.* | - |
| Steeltoe.Management.EndpointBase, Steeltoe.Management.EndpointCore, Steeltoe.Management.CloudFoundryCore | Steeltoe.Management.Endpoint |
| Steeltoe.Management.KubernetesCore | - |
| Steeltoe.Management.OpenTelemetryBase | Steeltoe.Management.Endpoint, Steeltoe.Management.Prometheus, Steeltoe.Management.Wavefront |
| Steeltoe.Management.TaskCore | Steeltoe.Management.Tasks |
| Steeltoe.Management.TracingBase, Steeltoe.Management.TracingCore | Steeltoe.Management.Tracing |
| Steeltoe.Messaging.* | - |
| Steeltoe.Security.Authentication.CloudFoundryBase, Steeltoe.Security.Authentication.CloudFoundryCore | Steeltoe.Security.Authentication.JwtBearer, Steeltoe.Security.Authentication.OpenIdConnect, Steeltoe.Security.Authorization.Certificate |
| Steeltoe.Security.Authentication.MtlsCore | Steeltoe.Security.Authorization.Certificate |
| Steeltoe.Security.DataProtection.CredHubBase, Steeltoe.Security.DataProtection.CredHubCore | - |
| Steeltoe.Security.DataProtection.RedisCore | Steeltoe.Security.DataProtection.Redis |
| Steeltoe.Stream.* | - |

## Breaking Changes

### Connectors

Connectors have been updated to work with the latest .NET drivers and Cloud Foundry broker packs, as well as Kubernetes.
The Connectors now use the [ASP.NET Options pattern](https://learn.microsoft.com/aspnet/core/fundamentals/configuration/options)
and are made agnostic to driver-specific connection string parameters; normalization of parameter
names (for example, "host" vs "server", "database" vs "initial catalog") is handled by the drivers.
Earlier versions assumed defaults for missing configuration parameters, which is no longer the case.
The structure of local configuration has changed to accommodate multiple named service bindings in a unified way.
Driver-specific connection/client instances are no longer directly registered in the IoC container,
but made accessible through a factory that enables accessing named bindings.
Earlier limitations on health check registration with Entity Framework Core no longer apply.
This release fixes bugs in the escaping of connection string parameters.

### Service Discovery

### Management

## Release Notes

Release notes for all releases can be found on the [Steeltoe releases](https://github.com/SteeltoeOSS/Steeltoe/releases) section on GitHub.
