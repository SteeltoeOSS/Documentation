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
The simplest way to get started with Steeltoe Discovery is to add a reference to the package(s) containing the client technology you may wish to use. Any client package also includes all the relevant dependencies.

|App Type|Package|Description|
|---|---|---|
|.NET Standard 2.0|`Steeltoe.Discovery.ClientBase`|Service Discovery base package.|
|ASP.NET Core|`Steeltoe.Discovery.ClientCore`|Includes base. Adds WebHost compatibility.|
|Eureka Client|`Steeltoe.Discovery.Eureka`|Eureka Client functionality. Depends on ClientBase.|
|Consul Client|`Steeltoe.Discovery.Consul`|Consul Client functionality. Depends on ClientBase.|
|Kubernetes Client|`Steeltoe.Discovery.Kubernetes`|Kubernetes Client functionality. Depends on ClientBase.|

To add this type of NuGet to your project, add an element resembling the following `PackageReference`:

```xml
<ItemGroup>
...
    <PackageReference Include="Steeltoe.Discovery.ClientCore" Version= "3.0.0"/>
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
