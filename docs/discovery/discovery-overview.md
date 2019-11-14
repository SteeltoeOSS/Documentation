# Steeltoe Discovery

Steeltoe provides a set of generalized interfaces for interacting with multiple service discovery back ends. This section will cover the general components first. If you are looking for something specific to the registry server you are using, feel free to skip ahead to the section for [Netflix Eureka](#2-0-netflix-eureka) or [HashiCorp Consul](#3-0-hashicorp-consul).

In order to use any Steeltoe Discovery client, you need to do the following:

* Add appropriate NuGet package reference to your project.
* Configure the settings the Discovery client will use to register services in the service registry.
* Configure the settings the Discovery client will use to discover services in the service registry.
* Add and Use the Discovery client service in the application.
* Use an injected `IDiscoveryClient` to lookup services.

>NOTE: The Steeltoe Discovery implementation (for example: the decision between Eureka and Consul) is automatically setup within the application based on the application configuration provided.

>IMPORTANT: The `Pivotal.Discovery.*` packages have been deprecated in Steeltoe 2.2 and will be removed in a future release.  All functionality provided in those packages has been pushed into the corresponding `Steeltoe.Discovery.*` packages.

## Add NuGet References

<!-- TODO: review this section, its not completely correct -->
The simplest way to get started with Steeltoe Discovery is to add a reference to a package built for either Microsoft's dependency injection or Autofac. Either package will also include all relevant dependencies. If you are using another DI tool, please file an issue to let us know, and in the mean time use the relevant base package:

|App Type|Package|Description|
|---|---|---|
|ASP.NET Core|`Steeltoe.Discovery.ClientCore`|Includes base. Adds ASP.NET Core dependency injection.|
|ASP.NET 4.x with Autofac|`Steeltoe.Discovery.ClientAutofac`|Includes base. Adds Autofac dependency injection.|
|Console/ASP.NET 4.x|`Steeltoe.Discovery.EurekaBase`|Base Eureka functionality. No dependency injection.|
|Console/ASP.NET 4.x|`Steeltoe.Discovery.ConsulBase`|Base Consul functionality. No dependency injection.|

To add this type of NuGet to your project, add an element resembling the following `PackageReference`:

```xml
<ItemGroup>
...
    <PackageReference Include="Steeltoe.Discovery.ClientCore" Version= "2.1.0"/>
...
</ItemGroup>
```

