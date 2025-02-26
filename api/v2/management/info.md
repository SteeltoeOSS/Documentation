# Info

The Steeltoe info management endpoint exposes various application information collected from all `IInfoContributor` provided to the `InfoEndpoint`.

Steeltoe includes a couple `IInfoContributor`s out of the box that you can use, but most importantly you can also write your own.

## Info Contributors

The following table describes the `IInfoContributor` implementations provided by Steeltoe:

|Name|Description|
|---|---|
| `AppSettingsInfoContributor`|Exposes any values under the key `info` (for example, `info:foo:bar=foobar`) that is in your apps configuration (for example, `appsettings.json`)|
| `GitInfoContributor`|Exposes git information (if a git.properties file is available)|

For an example of how to use the above `GitInfoContributor` within MSBuild using [GitInfo](https://github.com/kzu/GitInfo), see the [Steeltoe management sample](https://github.com/SteeltoeOSS/Samples/tree/2.x/Management/src/AspDotNetCore/CloudFoundry) and the [CloudFoundry.csproj](https://github.com/SteeltoeOSS/Samples/blob/2.x/Management/src/AspDotNetCore/CloudFoundry/CloudFoundry.csproj) file.

If you wish to provide custom information for your application, create a class that implements the `IInfoContributor` interface and then add that to the `InfoEndpoint`. Details on how to add a contributor to the endpoint is provided below.

The following example `IInfoContributor` adds `someProperty=someValue` to the application's information.

```csharp
public class ArbitraryInfoContributor : IInfoContributor
{
    public void Contribute(IInfoBuilder builder)
    {
        // pass in the info
        builder.WithInfo("arbitraryInfo", new { someProperty = "someValue" });
    }
}
```

>NOTE: Custom `IInfoContributor` implementations must be retrievable from the DI container by interface in order for Steeltoe to find them.

## Configure Settings

The following table describes the settings that you can apply to the endpoint.

|Key|Description|Default|
|---|---|---|
|id|The ID of the info endpoint|`info`|
|enabled|Whether to enable info management endpoint|true|
|sensitive|Currently not used|false|
|requiredPermissions|User permissions required on Cloud Foundry to access endpoint|RESTRICTED|

**Note**: **Each setting above must be prefixed with `management:endpoints:info`**.

## Enable HTTP Access

The default path to the Info endpoint is computed by combining the global `path` prefix setting together with the `id` setting from above. The default path is  `/actuator/info`.

The coding steps you take to enable HTTP access to the Info endpoint together with how to use custom Info contributors differs depending on the type of .NET application your are developing.  The sections which follow describe the steps needed for each of the supported application types.

>NOTE: If you are using dependency injection, all `IInfoContributor` implementations that are retrievable from the DI container by interface will be returned in the Info response.

### ASP.NET Core App

To add the Info actuator to the service container, you can use any of the `AddInfoActuator()` extension methods from `EndpointServiceCollectionExtensions`.

To add the Info actuator middleware to the ASP.NET Core pipeline, use the `UseInfoActuator()` extension method from `EndpointApplicationBuilderExtensions`.

The following example shows how enable the Info endpoint and to add a custom `IInfoContributor` to the service container by adding `ArbitraryInfoContributor` as a singleton. Once that's done the Info endpoint will discover and use it during info requests.

```csharp
public class Startup
{
    ...
    public void ConfigureServices(IServiceCollection services)
    {
        // Add custom info contributor, specifying the interface type
        services.AddSingleton<IInfoContributor, ArbitraryInfoContributor>();

        // Add Info actuator
        services.AddInfoActuator(Configuration);

        // Add framework services.
        services.AddMvc();
    }
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
        app.UseStaticFiles();

        // Add management endpoint into pipeline
        app.UseInfoActuator();
    }
}
```

### ASP.NET 4.x App

To add the Info actuator endpoint, use the `UseInfoActuator()` method from `ActuatorConfigurator`. Optionally you can provide a list of `IInfoContributor`s should you want to customize the actuator endpoint.  If none are provided, defaults will be provided.

The following example shows how enable the Info endpoint and use the `GitInfoContributor` and `AppSettingsInfoContributor` as `IInfoContributor`s.

```csharp
public class ManagementConfig
{
    public static void ConfigureManagementActuators(IConfiguration configuration, ILoggerFactory loggerFactory)
    {
        ...
        ActuatorConfigurator.UseInfoActuator(configuration, GetInfoContributors(configuration), loggerFactory);
        ...
    }
    private static IEnumerable<IInfoContributor> GetInfoContributors(IConfiguration configuration)
    {
        var contributors = new List<IInfoContributor>() { new GitInfoContributor(), new AppSettingsInfoContributor(configuration) }
        return contributors;
    }
```

### ASP.NET OWIN App

To add the Info actuator middleware to the ASP.NET OWIN pipeline, use the `UseInfoActuator()` extension method from `InfoEndpointAppBuilderExtensions`.

The following example shows how to enable the Info endpoint and use the `GitInfoContributor` and `AppSettingsInfoContributor` as `IInfoContributor`s.

```csharp
public class Startup
{
    ...
    public void Configuration(IAppBuilder app)
    {
        ...
        app.UseInfoActuator(
            ApplicationConfig.Configuration,
            GetInfoContributors(ApplicationConfig.Configuration),
            LoggingConfig.LoggerFactory);
        ...
    }
    private static IEnumerable<IInfoContributor> GetInfoContributors(IConfiguration configuration)
    {
        var contributors = new List<IInfoContributor>() { new GitInfoContributor(), new AppSettingsInfoContributor(configuration) }
        return contributors;
    }
}
```

