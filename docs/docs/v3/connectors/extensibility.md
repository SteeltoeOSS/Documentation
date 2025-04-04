# Extensibility

As of version 3.0, Steeltoe ships a package named `Steeltoe.Connector.Abstractions`, which contains the interfaces and classes that define the extensibility points of Connectors. One of the core concepts in Connectors is that connections to backing services can be defined by some instance of `IServiceInfo`.

Implementations of `IServiceInfo` will include at least the minimum amount of information needed in order to establish a connection with an instance or cluster of the given service type.

Many of these service credentials can be represented in a Uri format, so most of the included `IServiceInfo` classes are based on `UriInfo`.

## ServiceInfoFactory

Every `IServiceInfo` should also have one corresponding class based on `ServiceInfoFactory` that can evaluate a generic `Steeltoe.Extensions.Configuration.Service` for compatibility and produce the right type of `IServiceInfo` as needed. `ServiceInfo` Factories are provided for a variety of services, including many of the types that are available on Cloud Foundry.

If you'd like to include your own `ServiceInfos` without having them directly included in Steeltoe, use the `ServiceInfoFactoryAssemblyAttribute` on your assembly so that Steeltoe can find your `ServiceInfoFactory` classes at runtime by adding this to your `AssemblyInfo.cs` file:

```csharp
using Steeltoe.Connector;

[assembly: ServiceInfoFactoryAssembly]
```

## ServiceInfoCreator

Every `ServiceInfoFactory` is managed within the `ServiceInfoCreator` class. The base `ServiceInfoCreator` should handle most of the heavy lifting with connecting configuration with `ServiceInfo` and `ServiceInfoFactories` together, but sometimes there are other rules that need to be applied. For example, logic that is specific to Cloud Foundry service bindings has been moved out to the new class `CloudFoundryServiceInfoCreator`

If you'd like to change something about the way `ServiceInfoFactory` works without having your changes directly included in Steeltoe or when adding a new `ServiceInfoCreator`, use the `ServiceInfoCreatorAssemblyAttribute` on your assembly so that Steeltoe can find your `ServiceInfoCreator` class at runtime.

For example, this is how the `CloudFoundryServiceInfoCreator` is identified for simple discovery in the `Steeltoe.Connector.CloudFoundry` package's `AssemblyInfo.cs` file:

```csharp
using Steeltoe.Connector;
using Steeltoe.Connector.CloudFoundry;

[assembly: ServiceInfoCreatorAssembly(typeof(CloudFoundryServiceInfoCreator))]
```

>A customized `ServiceInfoCreator` should not be required to add Service Infos or Service Info Factories
