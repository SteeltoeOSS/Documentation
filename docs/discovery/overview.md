# Service Discovery

Steeltoe provides a set of generalized interfaces for interacting with multiple service discovery back ends. This section covers the general components first. If you are looking for something specific to the registry server you are using, feel free to skip ahead to the section for Netflix Eureka or HashiCorp Consul.

In order to use any Steeltoe Discovery client, you need to:

* Add the appropriate NuGet package reference to your project.
* Configure the settings the discovery client uses to register services in the service registry.
* Configure the settings the discovery client uses to discover services in the service registry.
* Add and use the discovery client service in the application.
* Use an injected `IDiscoveryClient` to look up services.

>NOTE: The Steeltoe discovery implementation (for example: the decision between Eureka and Consul) is automatically set up within the application, based on the application configuration provided.

## Add NuGet References

<!-- TODO: review this section. It is not completely correct. -->
The simplest way to get started with Steeltoe Discovery is to add a reference to a package built for either Microsoft's dependency injection or Autofac. Either package also includes all the relevant dependencies. If you use another DI tool, file an issue to let us know. In the meantime, use the relevant base package:

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

## Enable Debug Logging

Sometimes, you may want to turn on debug logging in the discovery client. To do so, you can modify the `appsettings.json` file and turn on Debug level logging for the Steeltoe components:

The following example shows a typical `appsettings.json` file:

```json
{
  "Logging": {
    "IncludeScopes": false,
    "LogLevel": {
      "Default": "Warning",
      "Steeltoe": "Debug"
    }
  },
  ...
}
```
