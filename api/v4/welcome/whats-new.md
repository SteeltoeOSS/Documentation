# What's New in Steeltoe 4

## New Features and Changes

### Steeltoe 4.0.0

## Package Name Changes

 | Steeltoe 3.x | Steeltoe 4.x |
 | ------------ | ------------ |
 | ... | Same |

  \* Experimental packages

## HostBuilder Extensions

## Breaking Changes

### Connectors

Connectors have been updated to work with the latest .NET drivers and CloudFoundry broker packs, as well as Kubernetes.
The Connectors now use the [ASP.NET Options pattern](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options)
and are made agnostic to driver-specific connection string parameters; normalization of parameter
names (for example, "host" vs "server", "database" vs "initial catalog") is handled by the drivers.
Earlier versions assumed defaults for missing configuration parameters, which is no longer the case.
The structure of local configuration has changed to accomodate multiple named service bindings in a unified way.
Driver-specific connection/client instances are no longer directly registered in the IoC container,
but made accessible through a factory that enables accessing named bindings.
Earlier limitations on health check registration with Entity Framework Core no longer apply.
This release fixes bugs in the escaping of connection string parameters.

### Service Discovery

### Management

## Release Notes

Release notes for all releases can be found on the [Steeltoe releases](https://github.com/SteeltoeOSS/Steeltoe/releases) section on GitHub.
