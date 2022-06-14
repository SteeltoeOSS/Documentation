# Kubernetes Providers

Steeltoe's Kubernetes Configuration Providers use the official [.NET Kubernetes Client](https://github.com/kubernetes-client/csharp/) to communicate with the Kubernetes API to find ConfigMaps and Secrets and add them to the configuration of .NET applications.

You can read more about [ConfigMaps](https://kubernetes.io/docs/concepts/configuration/configmap/) and [Secrets](https://kubernetes.io/docs/concepts/configuration/secret/) in the official Kubernetes docs.

## Conventions

Both of the Kubernetes configuration providers use the same conventions to search for resources. These conventions are designed to feel familiar and provide some working defaults while remaining configurable.

### Namespace

These configuration providers will only search a single namespace. The default namespace searched is `default`. Should you wish to change this behavior, use any configuration provider that has been added prior to the call to `AddKubernetes()` to set a different value under the key `Spring:Cloud:Kubernetes:NameSpace`.

### Resource names

These configuration providers will search for resources named `<ApplicationName>` and `<ApplicationName>.<EnvironmentName>`.

`<ApplicationName>` is determined by the first defined value between `Spring:Cloud:Kubernetes:Name`, `Spring:Application:Name` and the assembly name.

`<EnvironmentName>` is defined by the environment variable `ASPNETCORE_ENVIRONMENT`, falling back to a default value of `Production` if the variable has not been set.

## Usage

These providers integrate with [.NET Configuration](https://docs.microsoft.com/aspnet/core/fundamentals/configuration), you may wish to read more of that documentation if you're not familiar with it.

The steps to use both Steeltoe Kubernetes configuration providers are the same:

1. Add a NuGet package reference to your project.
1. Add the provider to the `HostBuilder` or `ConfigurationBuilder`.
1. Inject and use `Options` or `Configuration` to access configuration data.
1. Optionally, provide additional configuration for the Kubernetes Client, ConfigMaps and Secrets
1. Optionally, enable debug logging of interactions with Kubernetes API

### Add NuGet Reference

You can choose one of two Kubernetes Configuration NuGet packages, depending on your needs. Both configuration providers are included in the base package.

| Package | Description | .NET Target |
| --- | --- | --- |
| `Steeltoe.Extensions.Configuration.KubernetesBase` | Base functionality. No dependency injection. | .NET Standard 2.0 |
| `Steeltoe.Extensions.Configuration.KubernetesCore` | Includes base. Adds ASP.NET Core dependency injection. | ASP.NET Core 3.1+ |

To add this type of NuGet to your project, add a `PackageReference` that resembles the following:

```xml
<ItemGroup>
...
    <PackageReference Include="Steeltoe.Extensions.Configuration.KubernetesCore" Version="3.2.0"/>
...
</ItemGroup>
```

### Add Configuration Providers to HostBuilder

Extensions are provided for both `HostBuilder` and `WebHostBuilder`, with matching functionality. Optional parameters are provided for special configuration of the `KubernetesClient` or inclusion of a `LoggerFactory`, for logging operations within the configuration providers.

For standard usage, add the configuration providers like this:

```csharp
public static void Main(string[] args)
{
    CreateHostBuilder(args).Build().Run();
}

public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        })
        .AddKubernetesConfiguration()
```

>This functionality is included in the `Steeltoe.Extensions.Configuration.KubernetesCore`. Beyond adding the configuration providers, `KubernetesApplicationOptions` is added to the `IServiceCollection` for use by these configuration providers and other Steeltoe components.

#### Add Configuration Providers to ILoggingBuilder

An extension is also provided for `ILoggingBuilder`. This extension is called by the extensions for `HostBuilder` and `WebHostBuilder`, so you will not need to use both. Optional parameters are provided for special configuration of the `KubernetesClient` or inclusion of a `LoggerFactory`, for logging operations within the configuration providers.

For standard usage, add the configuration providers like this:

```csharp
public static void Main(string[] args)
{
    CreateHostBuilder(args).Build().Run();
}

public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        })
        .ConfigureAppConfiguration(builder => builder.AddKubernetes());
```

### Access Configuration Data

When the `ConfigurationBuilder` builds the configuration, the Kubernetes providers make the appropriate calls to the Kubernetes API server and retrieve the configuration values based on the conventions and settings that have been provided.

If there are any errors or problems accessing the server, the application continues to initialize, but the values from the server are not retrieved. If this is not the behavior you want, you should set the `Spring:Cloud:Config:FailFast` to `true`. Once that is done, the application fails to start if problems occur during the build.

After the configuration has been built, you can access the retrieved data directly by using `IConfiguration`. The following example shows how to do so:

```csharp
...
var config = builder.Build();
var property1 = config["myconfiguration:property1"];
var property2 = config["myconfiguration:property2"];
...
```

## Additional Configuration

These configuration providers are enabled by default when added to your configuration builder. An off switch is available by setting `Spring:Cloud:Kubernetes:Enabled=false` in any configuration provider that is added earlier in the chain.

### Kubernetes Client Configuration

The KubernetesClient that is used for these configuration providers can be configured through the `kubernetesClientConfiguration` parameter on `AddKubernetes` and `AddKubernetesConfiguration` extension methods, or by setting configuration entries in other configuration providers that are added to the `IConfigurationBuilder` prior to the call to `AddKubernetes`.

These are the settings available:

| Key | Description | Default |
| --- | --- | --- |
| `Paths` | A list of paths to Kubernetes config files | null |
| `NameEnvironmentSeparator` | Used to separate app name from environment name in resource queries | `.` |

All settings above should start with `Spring:Cloud:Kubernetes:Config`.

### ConfigMaps Configuration

| Key | Description | Default |
| --- | --- | --- |
| `Enabled` | Enables retrieval of Secrets | `true` |
| `Sources` | List of additional Secrets to retrieve | `null` |

Additional ConfigMaps can be specified in the Sources list using the name and namespace:

```json
{
  "Spring": {
    "Cloud": {
      "Kubernetes": {
        "Config": {
          "Sources": [{
            "Name": "my-configmap",
            "Namespace": "my-namespace"
          }]
        }
      }
    }
  }
}
```

All settings above should start with `Spring:Cloud:Kubernetes:Config`.

### Secrets Configuration

| Key | Description | Default |
| --- | --- | --- |
| `Enabled` | Enables retrieval of Secrets | `true` |
| `Sources` | List of additional Secrets to retrieve | `null` |

Additional Secrets can be specified in the Sources list using the name and namespace:

```json
{
  "Spring": {
    "Cloud": {
      "Kubernetes": {
        "Secrets": {
          "Sources": [{
            "Name": "my-secret",
            "Namespace": "my-namespace"
          }]
        }
      }
    }
  }
}
```

All settings above should start with `Spring:Cloud:Kubernetes:Secrets`.

For more information about risks and best practices when consuming Secrets through the API refer to the [best practices](https://kubernetes.io/docs/concepts/configuration/secret/#best-practices).

### Reload Settings

Reload settings are shared by both ConfigMaps and Secrets. These can be configured with a prefix

| Key | Description | Default |
| --- | --- | --- |
| `Mode` | Method of monitoring for changes in configuration data | `Polling` |
| `Period` | Time in seconds between polls | 15 |
| `ConfigMaps` | Enables reloading of ConfigMaps | `false` |
| `Secrets` | Enables reloading of Secrets | `false` |

All settings above should start with `Spring:Cloud:Kubernetes:Reload`.

>`Mode` can also be set to `Event`, which will attempt to maintain an open connection to the API server for real-time reloading of configuration data.

### Enable Logging

Sometimes, it is desirable to turn on debug logging in the provider.

To do so, you need to inject the `ILoggerFactory` into the `Startup` class constructor by adding it as an argument to the constructor. Once you have access to it, you can add a console logger to the factory and also set its minimum logging level set to `Debug`.

Once that is done, pass the `ILoggerFactory` to the Steeltoe configuration provider. The provider then uses it to establish a logger with the debug-level logging turned on.

The following example shows how to enable Debug-level logging:

```csharp
var logFactory = new LoggerFactory();
logFactory.AddConsole(minLevel: LogLevel.Debug);

public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        })
        ..AddKubernetesConfiguration(loggerFactory: logFactory);
...
```
