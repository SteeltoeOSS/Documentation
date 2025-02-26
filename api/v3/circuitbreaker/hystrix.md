# Netflix Hystrix

Steeltoe's Hystrix implementation lets application developers isolate and manage back-end dependencies so that a single failing dependency does not take down the entire application. This is accomplished by wrapping all calls to external dependencies in a `HystrixCommand`, which runs in its own separate external thread.

Hystrix maintains its own small, fixed-size thread pool from which commands are executed. When the pool becomes exhausted, Hystrix commands are immediately rejected, and, if provided, a fallback mechanism is executed. This prevents any single dependency from using up all of the threads for failing external dependencies.

Each Hystrix command also has the ability to create a timeout for any calls that take longer than the configured threshold. If a timeout occurs, the command automatically executes a fallback mechanism (if the developer provided one). Developers can configure each command with custom fallback logic that runs when a command fails, is rejected, times out, or trips the circuit breaker.

Each command has a built-in configurable circuit breaker that stops all requests to failing back-end dependencies when the error percentage passes a threshold. The circuit remains open (broken) for a configurable period of time, and all requests are then sent to the fallback mechanism until the circuit is closed (connected) again.

Hystrix also provides a means to measure command successes, failures, timeouts, short-circuits, and thread rejections. Statistics are gathered for all of these and can optionally be reported to a [Hystrix Dashboard](https://github.com/Netflix/Hystrix/wiki/Dashboard) for monitoring in real-time.

The remaining sections of this chapter describe these features. Also, you should understand that Steeltoe's Hystrix implementation follows the Netflix implementation closely. As a result, its worthwhile to review the [Netflix documentation](https://github.com/Netflix/Hystrix/wiki) in addition to this documentation.  Pay particular attention to the [How it Works](https://github.com/Netflix/Hystrix/wiki/How-it-Works) section as it provides a [Flow Chart](https://github.com/Netflix/Hystrix/wiki/How-it-Works#Flow) explaining how a command executes and how the default [Circuit Breaker](https://github.com/Netflix/Hystrix/wiki/How-it-Works#CircuitBreaker) transitions between the CLOSED, OPEN and HALF-OPEN states.  It also provides details on how the [Bulkhead pattern](https://docs.microsoft.com/azure/architecture/patterns/bulkhead) is implemented by using isolation techniques that employ [Threads and Thread Pools](https://github.com/Netflix/Hystrix/wiki/How-it-Works#isolation).

## Usage

You should have a good understanding of how the .NET [configuration service](https://docs.microsoft.com/aspnet/core/fundamentals/configuration) works before you start to use the Hystrix framework. A basic understanding of the `ConfigurationBuilder` and how to add providers to the builder is necessary to configure the framework.

You should also have a good understanding of how the [ASP.NET Core Startup class](https://docs.microsoft.com/aspnet/core/fundamentals/startup) is used to configure the application services and the middleware used in the app. You should pay particular attention to the usage of the `Configure()` and `ConfigureServices()` methods.

In addition to the information in this section, you should review the [Netflix Hystrix Wiki](https://github.com/Netflix/Hystrix/wiki). The Steeltoe Hystrix framework implementation aligns closely with the Netflix implementation. Consequently, the Wiki information applies directly to Steeltoe.

If you plan to use the Hystrix Dashboard, you should also spend time understanding the [Netflix Hystrix Dashboard](https://github.com/Netflix/Hystrix/wiki/Dashboard) information on the wiki.

To use the Steeltoe framework:

1. Add Hystrix NuGet package references to your project
1. Define Hystrix Command(s) and/or Hystrix Collapser(s)
1. Configure Hystrix settings
1. Add Hystrix Command(s) and/or Collapser(s) to the container
1. Use Hystrix Command(s) and/or Collapser(s) to invoke dependent services
1. Add and Use the Hystrix metrics stream service

### Add NuGet References

There are two types of NuGet references to consider with when adding Hystrix to your application.

The first is required to bring the basic Hystrix functionality (the ability to define and execute commands) into your application. Choose from the following options based on the type of the application you are building and what Dependency Injector you have chosen, if any:

| Package | Description | .NET Target |
| --- | --- | --- |
| `Steeltoe.CircuitBreaker.Abstractions` | Interfaces and objects used for extensibility. | .NET Standard 2.0 |
| `Steeltoe.CircuitBreaker.HystrixBase` | Base functionality, no DI. | .NET Standard 2.0 |
| `Steeltoe.CircuitBreaker.HystrixCore` | Includes base, adds ASP.NET Core Integration. | ASP.NET Core 3.1+ |

To add this type of NuGet to your project, add something like the following `PackageReference`:

```xml
<ItemGroup>
...
    <PackageReference Include="Steeltoe.CircuitBreaker.HystrixCore" Version="3.2.0"/>
...
</ItemGroup>
```

The second type of NuGet that you may need to add relates to collecting Hystrix metrics. If you are developing an ASP.NET Core application and want to collect Hystrix metrics and you are using the [Netflix Hystrix Dashboard](https://github.com/Netflix/Hystrix/wiki/Dashboard), you need to also include the `Steeltoe.CircuitBreaker.Hystrix.MetricsEventsCore` package in your application.

To do so, include the following `PackageReference` in your application:

```xml
<ItemGroup>
...
    <PackageReference Include="Steeltoe.CircuitBreaker.Hystrix.MetricsEventsCore" Version="3.2.0"/>
...
</ItemGroup>
```

Alternatively, if you are pushing your application to Cloud Foundry/Tanzu Application Service and want to use the [Spring Cloud Services Hystrix Dashboard](https://docs.vmware.com/en/Spring-Cloud-Services-for-VMware-Tanzu/2.1/spring-cloud-services/GUID-index.html), then include the following package instead of the one above:

| App Type | Package | Description |
| --- | --- | --- |
| ASP.NET Core | `Steeltoe.CircuitBreaker.Hystrix.MetricsStreamCore` | ASP.NET Core DI |

When using `MetricsStreamCore` you will also need to include a package reference to a RabbitMQ client. `MetricsStreamCore` uses RabbitMQ messaging to send metrics to the Spring Cloud Services Hystrix Dashboard on Cloud Foundry.

To add the NuGet to your project, add something like the following:

```xml
<ItemGroup>
...
    <PackageReference Include="Steeltoe.CircuitBreaker.Hystrix.MetricsStreamCore" Version="3.2.0"/>
    <PackageReference Include="RabbitMQ.Client" Version="4.1.3" />
...
</ItemGroup>
```

### Define Hystrix Commands

There are many ways to define a Hystrix command. The simplest looks something like the following:

```csharp
public class HelloWorldCommand : HystrixCommand<string>
{
    public HelloWorldCommand(string name)
        : base(HystrixCommandGroupKeyDefault.AsKey("HelloWorldGroup"),
                () => { return "Hello" + name; },
                () => { return "Hello" + name + " via fallback"; })
    {
    }
}
```

Each command needs to inherit from `HystrixCommand` or `HystrixCommand<T>` and override and implement the inherited and protected `RunAsync()` method. Optionally, you can also override and implement the inherited method `RunFallbackAsync()` method. The simple example above provides lambdas for each of those two methods.

The `RunAsync()` method should implement the fundamental logic of the command, and the `RunFallbackAsync()` method should implement any fallback logic, in case the `RunAsync()` method fails.

Each command must be a member of a group. The name of the group is specified using the `HystrixCommandGroupKeyDefault.AsKey("YourGroupName")` method and must be provided in the constructor of the command.

The following example shows the same command shown earlier, but rewritten using method overrides:

```csharp
public class HelloWorldCommand : HystrixCommand<string>
{
    private string _name;
    public HelloWorldCommand(string name)
        : base(HystrixCommandGroupKeyDefault.AsKey("HelloWorldGroup"))
    {
        _name = name;
    }

    protected override async Task<string> RunAsync()
    {
        return await Task.FromResult("Hello" + _name);
    }
    protected override async Task<string> RunFallbackAsync()
    {
        return await Task.FromResult("Hello" + _name + " via fallback");
    }
}
```

`HystrixCommands` are stateful objects. Once they have been run, they can no longer be reused. If you want to run a command again, you must create another instance (for example, `new MyCommand()`) and call one of the execute methods again as illustrated in the Use Command section below.

### Configuring Hystrix Commands

Each Hystrix command that you define can be individually configured by using normal .NET configuration services. You can specify everything from thread pool sizes and command time-outs to circuit-breaker thresholds.

For each of the possible Hystrix settings listed below, there are four levels of configuration and precedence that are followed and applied by the framework:

1. `Fixed global command settings`: These settings specify defaults for all Hystrix commands. Used if nothing else is specified.
1. `Configured global command settings`: These settings are specified in an application configuration and overrides fixed global settings for all Hystrix commands.
1. `Command settings specified in code`: Can be specified in the constructor of your Hystrix command and only applies to that specific instance.
1. `Configured command specific settings`: Are specified in an application's configuration and are specific for 'named' command. Applies to all command instances created with that name.

All Hystrix command configuration settings should be prefixed with `Hystrix:Command:`.

`Configured global command settings` should be prefixed with `Hystrix:Command:default:`. The following example illustrates a setting that sets the default timeout for all commands to be 750 milliseconds:

`Hystrix:Command:default:Execution:Isolation:Thread:TimeoutInMilliseconds=750`

To configure `Command settings specified in code`, use `HystrixCommandOptions` from the `Steeltoe.CircuitBreaker.HystrixBase` package.

All `configured command-specific settings`, as described earlier in #4, should be prefixed with `Hystrix:Command:HYSTRIX_COMMAND_KEY:`, where `HYSTRIX_COMMAND_KEY` is the `Name` of the command. The following example configures the timeout for the Hystrix command with a name of `sample` to be 750 milliseconds:

`Hystrix:Command:sample:Execution:Isolation:Thread:TimeoutInMilliseconds=750`

The following set of tables specifies all of the possible settings by category.

>The settings described in the next sections follow the Netflix Hystrix implementation closely. Consequently, you should read the [Configuration section](https://github.com/Netflix/Hystrix/wiki/Configuration) on the Netflix Hystrix wiki for more detail on each setting and how it affects Hystrix command operations.

#### Execution Settings

The following table describes the settings that control how the HystrixCommand's `RunAsync()` method runs:

| Key | Description | Default |
| --- | --- | --- |
| `Timeout:Enabled` | Enables or disables `RunAsync()` timeouts. | true |
| `Isolation:Strategy` | `THREAD` or `SEMAPHORE`. | `THREAD` |
| `Isolation:Thread:TimeoutInMilliseconds` | Time allowed for `RunAsync()` execution completion before fallback is executed. | 1000 |
| `Isolation:Semaphore:MaxConcurrentRequests` | Maximum requests to `RunAsync()` method when using the `SEMAPHORE` strategy. | 10 |

Each setting is prefixed with a key of `Execution`, as shown in the following example:

`Hystrix:Command:sample:Execution:Isolation:Strategy=SEMAPHORE`

#### Fallback Settings

The following table describes the settings that control how the HystrixCommand's `RunFallbackAsync()` method runs:

| Key | Description | Default |
| --- | --- | --- |
| `Enabled` | Enables or disables `RunFallbackAsync()`. | true |
| `Isolation:Semaphore:MaxConcurrentRequests` | Maximum requests to `RunFallbackAsync()` method when using the `SEMAPHORE` strategy. | 10 |

Each setting is prefixed with a key of `Fallback`, as shown in the following example:

`Hystrix:Command:sample:Fallback:Enabled=false`

#### Circuit Breaker Settings

The following table describes the settings that control the behavior of the default Circuit Breaker used by Hystrix commands:

| Key | Description | Default |
| --- | --- | --- |
| `Enabled` | Enables or disables circuit breaker usage. | `true` |
| `RequestVolumeThreshold` | Minimum number of requests in a rolling window that will trip the circuit. | 20 |
| `SleepWindowInMilliseconds` | Amount of time, after tripping the circuit, to reject requests before allowing attempts again. | 5000 |
| `ErrorThresholdPercentage` | Error percentage at or above which the circuit should trip open and start short-circuiting requests to fallback logic. | 50 |
| `ForceOpen` | Force circuit open. | `false` |
| `ForceClosed` | Force circuit closed. | `false` |

Each setting is prefixed with a key of `CircuitBreaker`, as shown in the following example:

`Hystrix:Command:sample:CircuitBreaker:Enabled=false`

#### Metrics Settings

The following table describes the settings that control the behavior of capturing metrics from Hystrix commands:

| Key | Description | Default |
| --- | --- | --- |
| `RollingStats:TimeInMilliseconds` | Duration of the statistical rolling window, used by circuit breaker and for publishing. | 10000 |
| `RollingStats:NumBuckets` | Number of buckets the rolling statistical window is divided into. | 10 |
| `RollingPercentile:Enabled` | Indicates whether execution latencies should be tracked and calculated as percentiles. | `true` |
| `RollingPercentile:TimeInMilliseconds` | Duration of the rolling window in which execution times are kept to allow for percentile calculations. | 60000 |
| `RollingPercentile:NumBuckets` | Number of buckets the `RollingPercentile` window will be divided into. | 6 |
| `RollingPercentile:BucketSize` | Maximum number of execution times that are kept per bucket. | 100 |
| `HealthSnapshot:IntervalInMilliseconds` | Time to wait between allowing health snapshots to be taken that calculate success and error percentages affecting circuit breaker status. | 500 |

Each setting is prefixed with the key `Metrics`, as shown in the following example:

`Hystrix:Command:sample:Metrics:RollingPercentile:Enabled=false`

#### Request Cache Settings

The following table describes the settings that control whether Hystrix command request caching is enabled or disabled:

| Key | Description | Default |
| --- | --- | --- |
| `Enabled` | Enables or disables request scoped caching. | `true` |

Each setting is prefixed with the key `RequestCache`, as shown in the following example:

`Hystrix:Command:sample:RequestCache:Enabled=false`

#### Request Logging Settings

The following table describes the settings that control whether Hystrix command execution events are logged to the Request log:

| Key | Description | Default |
| --- | --- | --- |
| `Enabled` | Enables or disables request scoped logging. | `true` |

Each setting is prefixed with the key `RequestLog`, as shown in the following example:

`Hystrix:Command:sample:RequestLog:Enabled=false`

#### Thread Pool Settings

Hystrix commands use thread pools to implement an underlying isolation pattern for resilience and fault tolerance features. By default each command will create its own 'named' thread pool and every time an instance of that command is executed, a thread from that thread pool will be chosen and used.

Optionally, you can configure your own 'named' thread pool (see next section) and in turn associate one or more commands to a specific thread pool. The describes the settings that control what thread pool a command uses when executing:

| Key | Description | Default |
| --- | --- | --- |
| `ThreadPoolKeyOverride` | Sets the thread pool used by the command. | command group key name |

The following listing shows an example:

`Hystrix:Command:sample:ThreadPoolKeyOverride=FortuneServiceTPool`

### Configuring Hystrix Thread Pools

In addition to configuring the settings for Hystrix commands, you can also configure Hystrix thread pools and the settings that Steeltoe uses to create and manage those thread pools.

In most cases, you can take the defaults and need not configure these settings.

As with the Hystrix command settings, there are four levels of precedence that are followed and applied to Hystrix thread pools by the framework:

1. `Fixed global pool settings`: These are the defaults for all the Hystrix pools. If nothing else is specified, these settings are used.
1. `Configured global pool settings`: These are the defaults specified in configuration files that override the fixed values described earlier and apply to all Hystrix pools.
1. `Pool settings specified in code`: These are the settings you specify in the constructor of your Hystrix thread pool. These settings apply to all commands that reference that pool.
1. `Configured pool specific settings`: These are the settings specified in configuration files that are targeted at named thread pools and apply to all commands that are created that reference that pool.

All configured thread pool settings should be placed under the prefix with a key of `Hystrix:Threadpool:`.

All configured global settings, as described earlier in #2, should be placed under a prefix of `Hystrix:Threadpool:default:`. The following example configures the default number of threads in all thread pools to be 20:

`Hystrix:Threadpool:default:CoreSize=20`

To configure the settings for a thread pool in code, use the `HystrixThreadPoolOptions` type found in the `Steeltoe.CircuitBreaker.HystrixBase` package.

All configured pool-specific settings, as described in #4 above, should be placed under a prefix of `Hystrix:Threadpool:HYSTRIX_THREADPOOL_KEY:`, where `HYSTRIX_THREADPOOL_KEY` is the `Name` of the thread pool. Note that the default name of the thread pool used by a command, if not overridden, is the command group name applied to the command. The following example configures the number of threads for the Hystrix thread pool with a `Name` of `sample` to be 40:

`Hystrix:Threadpool:sample:CoreSize=40`

The tables in the following sections specify all of the possible settings.

>The settings provided below follow the Netflix Hystrix implementation closely. Consequently, you should read the [Configuration section](https://github.com/Netflix/Hystrix/wiki/Configuration) on the Netflix Hystrix wiki for more detail on each setting and how it affects Hystrix thread pool operations.

#### Thread Pool Sizing Settings

These settings control the sizing of various aspects of the thread pool. There is no additional prefix used in these settings. The following table describes the sizing settings:

| Key | Description | Default |
| --- | --- | --- |
| `CoreSize` | Sets the thread pool size. | 10 |
| `MaximumSize` | Maximum size of threadPool. See `AllowMaximumSizeToDivergeFromCoreSize`. | 10 |
| `MaxQueueSize` | Maximum thread pool queue size. `value=-1` uses the sync queue. | -1 |
| `QueueSizeRejectionThreshold` | Sets the queue size rejection threshold. An artificial maximum queue size, at which rejections occur even if `MaxQueueSize` has not been reached. Does not apply if `MaxQueueSize=-1`. | 5 |
| `KeepAliveTimeMinutes` | Currently not used. | 1 |
| `AllowMaximumSizeToDivergeFromCoreSize` | Lets the configuration for `MaximumSize` take effect. | false |

The following listing shows an example:

`Hystrix:ThreadPool:sample:CoreSize=20`

#### Thread Pool Metric Settings

The following table describes the settings that control the behavior of capturing metrics from Hystrix thread pools:

| Key | Description | Default |
| --- | --- | --- |
| `RollingStats:TimeInMilliseconds` | Duration of the statistical rolling window. Defines how long metrics are kept for the thread pool. | 10000 |
| `RollingStats:NumBuckets` | Number of buckets into which the rolling statistical window is divided. | 10 |

Each setting is prefixed with a key of `Metrics`, as shown in the following example:

`Hystrix:Threadpool:sample:Metrics:RollingStats:TimeInMilliseconds=20000`

### Configuring Hystrix Collapsers

The last group of settings you can configure pertain to the usage of a Hystrix collapser. Not everyone will need to use collapsers, but if you do, these settings can be used.

As with all other Hystrix settings, there are four levels of precedence that are followed and applied by the framework:

1. `Fixed global collapser settings`: The defaults for all Hystrix collapsers. Used if nothing else is specified.
1. `Configured global collapser settings`: Settings in application configuration. Overrides fixed global settings for all Hystrix collapsers.
1. `Collapser settings specified in code`: Specified in the constructor of your Hystrix collapser. Applies to all collapsers that are created with that set of options.
1. `Configured collapser specific settings`: Specified in application configuration for named collapsers. Applies to all collapsers created with that name.

All configured collapser settings should be placed under a prefix of `Hystrix:Collapser:`.

All configured global settings, as described in #2 above, should be placed under a prefix of `Hystrix:Collapser:default:`. The following example configures the default number of milliseconds after which a batch of requests created by the collapser triggers and runs all of the requests:

`Hystrix:Collapser:default:TimerDelayInMilliseconds=20`

If you wish to configure the settings for a collapser in code, you must use the `HystrixCollapserOptions` found in the `Steeltoe.CircuitBreaker.HystrixBase` package.

All configured collapser specific settings, as described in #4 above, should be placed under a  prefix of `Hystrix:Collapser:HYSTRIX_COLLAPSER_KEY:`, where `HYSTRIX_COLLAPSER_KEY` is the "name" of the collapser.

>The default name of the collapser, if not specified, is the type name of the collapser.

The following example configures the number of milliseconds after which a batch of requests that have been created by the collapser with a name of `sample` triggers and runs all of the requests:

`Hystrix:Collapser:sample:TimerDelayInMilliseconds=400`

The tables in the sections that follow specify all of the possible settings.

>The settings provided in the tables follow the Netflix Hystrix implementation closely. Consequently, you should read the [Configuration section](https://github.com/Netflix/Hystrix/wiki/Configuration) on the Netflix Hystrix wiki for more detail on each setting and how it affects Hystrix collapser operations.

#### Collapser Sizing Settings

The following table describes the settings that control the sizing of various aspects of collapsers:

| Key | Description | Default |
| --- | --- | --- |
| `MaxRequestsInBatch` | Sets the max number of requests in a batch. | `Int32.MaxValue` |
| `TimerDelayInMilliseconds` | Delay before a batch is executed. | 10 |
| `RequestCacheEnabled` | Indicates whether request cache is enabled. | `true` |

There is no additional prefix used in these settings.

The following listing shows an example:

`Hystrix:Collapser:sample:TimerDelayInMilliseconds=400`

#### Collapser Metrics Settings

The following table describes the settings that control the behavior of capturing metrics from Hystrix collapsers.

| Key | Description | Default |
| --- | --- | --- |
| `RollingStats:TimeInMilliseconds` | Duration of the statistical rolling window. Used by circuit breaker and for publishing. | 10000 |
| `RollingStats:NumBuckets` | Number of buckets into which the rolling statistical window is divided. | 10 |
| `RollingPercentile:Enabled` | Indicates whether execution latencies should be tracked and calculated as percentiles. | `true` |
| `RollingPercentile:TimeInMilliseconds` | Duration of the rolling window in which execution times are kept to allow for percentile calculations. | 60000 |
| `RollingPercentile:NumBuckets` | Number of buckets into which the RollingPercentile window will be divided. | 6 |
| `RollingPercentile:BucketSize` | Maximum number of execution times that are kept per bucket. | 100 |

Each setting is prefixed with a key of `metrics`, as shown in the following example:

`Hystrix:Collapser:sample:Metrics:RollingPercentile:Enabled=false`

### Configuring Hystrix Settings

The most convenient way to configure any of the settings for Hystrix mentioned above is to put them in a file and then use one of the file-based .NET configuration providers to read them.

The following example shows some Hystrix settings in JSON (e.g. `appsettings.json`) that configure the `FortuneService` command to use a thread pool with a name of `FortuneServiceTPool`.  Since there are no thread pool settings provided for `FortuneServiceTPool` all the defaults will be used in its definition.

```json
{
  "Spring": {
    "Application": {
      "Name": "fortuneui"
    }
  },
  "Eureka": {
    "Client": {
      "ServiceUrl": "http://localhost:8761/eureka/",
      "ShouldRegisterWithEureka": false
    }
  },
  "Hystrix": {
    "Command": {
      "FortuneService": {
        "ThreadPoolKeyOverride": "FortuneServiceTPool"
      }
    }
  }
  ...
}
```

All of the Steeltoe samples and most of the .NET templates are already set up to read from `appsettings.json`.

### Adding Commands to Container

Once you have defined your configuration data and read in your configuration, you are ready to add the Hystrix commands to the dependency injection container, thereby making them available for injection into any of your application components. There are several Steeltoe extension methods available to help.

>Adding your commands to the container is not required. You can create them at any point in your application by simply `new'ing` them your self.

To make your Hystrix commands injectable, use the `AddHystrixCommand()` extension methods provided by Steeltoe in the `ConfigureServices()` method of the `Startup` class, as shown in the following example:

```csharp
using Steeltoe.CircuitBreaker.Hystrix;

public class Startup {
    ...
    public IConfiguration Configuration { get; private set; }
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }
    public void ConfigureServices(IServiceCollection services)
    {
        // Add Steeltoe Discovery Client service
        services.AddDiscoveryClient(Configuration);

        // Add Hystrix command FortuneService to Hystrix group "FortuneService"
        services.AddHystrixCommand<FortuneServiceCommand>("FortuneService", Configuration);

        // Add framework services.
        services.AddMvc();
        ...
    }
    ...
}
```

You must follow one important requirement if you wish to use the `AddHystrixCommand()` extension methods; When you define your `HystrixCommand`, define a public constructor with the first argument a `IHystrixCommandOptions` (that is, `HystrixCommandOptions`). You need not create or populate the `IHystrixCommandOptions`. The `AddHystrixCommand()` extension method does that for you by using the configuration data you provide in the extension method call.

The following example shows how to define a compatible constructor:

```csharp
public class FortuneServiceCommand : HystrixCommand<Fortune>
{
    IFortuneService _fortuneService;
    ILogger<FortuneServiceCommand> _logger;

    public FortuneServiceCommand(IHystrixCommandOptions options,
        IFortuneService fortuneService, ILogger<FortuneServiceCommand> logger) : base(options)
    {
        _fortuneService = fortuneService;
        _logger = logger;
        IsFallbackUserDefined = true;
    }
}
```

Notice that `FortuneServiceCommand` inherits from `HystrixCommand<Fortune>`. It is a command that returns results of type `Fortune`. Further notice that the constructor has `IHystrixCommandOptions` as a first argument. You can add additional constructor arguments.

### Using Commands in Applications

If you have used the `AddHystrixCommand()` extension methods described earlier, you can get an instance of the command in your controller or view by adding the command as an argument in the constructor.

The following example controller uses a Hystrix command called `FortuneServiceCommand`, which was added to the container by using `AddHystrixCommand<FortuneServiceCommand>("FortuneService", Configuration)`.

The controller uses the `GetFortuneAsync()` method on the injected command to retrieve a fortune, as shown in the following example:

```csharp
public class HomeController : Controller
{
    FortuneServiceCommand _fortuneServiceCommand;

    public HomeController(FortuneServiceCommand fortuneServiceCommand)
    {
        _fortuneServiceCommand = fortuneServiceCommand;
    }

    [HttpGet("random")]
    public async Task<Fortune> GetRandomFortune()
    {
        return await _fortuneServiceCommand.GetFortuneAsync();
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    }
}
```

The following example shows the definition of the Hystrix command used in the preceding example:

```csharp
using Steeltoe.CircuitBreaker.Hystrix;

public class FortuneServiceCommand : HystrixCommand<Fortune>
{
    IFortuneService _fortuneService;
    ILogger<FortuneServiceCommand> _logger;

    public FortuneServiceCommand(IHystrixCommandOptions options,
        IFortuneService fortuneService, ILogger<FortuneServiceCommand> logger) : base(options)
    {
        _fortuneService = fortuneService;
        _logger = logger;
        IsFallbackUserDefined = true;
    }
    public async Task<Fortune> GetFortuneAsync()
    {
        return await ExecuteAsync();
    }
    protected override async Task<Fortune> RunAsync()
    {
        var result = await _fortuneService.RandomFortuneAsync();
        _logger.LogInformation("Run: {0}", result);
        return result;
    }

    protected override async Task<Fortune> RunFallbackAsync()
    {
        _logger.LogInformation("RunFallback");
        return await Task.FromResult<Fortune>(new Fortune() { Id = 9999, Text = "You will have a happy day!" });
    }
}
```

The `FortuneServiceCommand` class is a Hystrix command and is intended to be used in retrieving `Fortune` instances from a `Fortune` microservice. It uses another service, `IFortuneService` to actually make the request.

In the preceding example, notice that the `FortuneServiceCommand` constructor takes an `IHystrixCommandOptions` as its first parameter. This is the command's configuration, and it has been previously populated with configuration data during `Startup`.

Further notice that the two protected methods, `RunAsync()` and `RunFallbackAsync()`, are the worker methods for the command and are the methods that do the work of the Hystrix command.

The `RunAsync()` method uses the `IFortuneService` to make a REST request of the Fortune microservice, returning the result as a `Fortune`. The `RunFallbackAsync()` method returns a hard-coded `Fortune`.

To invoke the command, the controller code invokes the async method, `GetFortuneAsync()`, to start command execution. This method calls the `HystrixCommand` method `ExecuteAsync()`.

There are multiple ways for commands to begin running.

To execute a command synchronously, use the `Execute()` method, as shown in the following example:

```csharp
var command = new FortuneServiceCommand(...);
var result = command.Execute();
```

To execute a command asynchronously, use the `ExecuteAsync()` method, as follows:

```csharp
var command = new FortuneServiceCommand(...);
var result = await command.ExecuteAsync();
```

You can also use `Rx.NET` extensions and observe the results of a command by using the `Observe()` method. The `Observe()` method returns a "hot" observable, which has already started execution upon return. The following listing shows an example:

```csharp
var command = new FortuneServiceCommand(...);
IObservable<Fortune> observable = command.Observe();
var result = await observable.FirstOrDefaultAsync();
```

Alternatively, you can use the `ToObservable()` method to return a `cold` observable for the command. Upon return, the command has not started running. Instead, when you `Subscribe()` to it, the underlying command begin to run, and the results are available in the Observer's `OnNext(result)` method. The following listing shows an example:

```csharp
var command = new FortuneServiceCommand(...);
IObservable<Fortune> cold = command.ToObservable();
IDisposable subscription = cold.Subscribe((result) => { Console.WriteLine(result); });
```

### Adding Collapsers to Container

In addition to Hystrix commands, you also might want to use Hystrix collapsers in your applications. Hystrix collapsers let you collapse multiple requests into a batch of requests that can then be executed by a single underlying `HystrixCommand`.

Collapsers can be configured to use a batch size or an elapsed time since the creation of the batch as triggers for executing the underlying Hystrix command.

There are two styles of request-collapsing supported by Hystrix: `request-scoped` and `globally-scoped`. You configure which style to use when you construct the collapser. The default is `request-scoped`. A `request-scoped` collapser collects a batch of requests for each `HystrixRequestContext`. A `globally-scoped` collapser collects a batch across multiple `HystrixRequestContext` instances.

As with Hystrix commands, you can also add Hystrix collapsers to the service container, making them available for injection in your application. You can use several Steeltoe extension methods to help you accomplish this.

>Adding collapsers to the container is not required. You can create them in your application at any point.

If you do want to have them injected, you can use the `AddHystrixCollapser()` extension methods provided by the Steeltoe package in the `ConfigureServices()` method of the `Startup` class, as follows:

```csharp
using Steeltoe.CircuitBreaker.Hystrix;

public class Startup {
    ...
    public IConfiguration Configuration { get; private set; }
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }
    public void ConfigureServices(IServiceCollection services)
    {
        // Add Steeltoe Discovery Client service
        services.AddDiscoveryClient(Configuration);

        // Add Hystrix collapser to the service container
        services.AddHystrixCollapser<FortuneServiceCollapser>("FortuneServiceCollapser", Configuration);

        // Add framework services.
        services.AddMvc();
        ...
    }

    ...
}
```

As with Hystrix commands, you must follow an important requirement if you wish to use the `AddHystrixCollapser()` extension methods: When you define your `HystrixCollapser`, you must define a public constructor with a `IHystrixCollapserOptions` (that is, `HystrixCollapserOptions`) as the first argument.  You need not create or populate its contents. Instead, the `AddHystrixCollapser()` extension method does that for you by using the configuration data you provide in the method call.

The following example shows how to define a compatible constructor:

```csharp
public class FortuneServiceCollapser  : HystrixCollapser<List<Fortune>, Fortune, int>,
{
    ILogger<FortuneServiceCollapser> _logger;
    ILoggerFactory _loggerFactory;
    IFortuneService _fortuneService;

    public FortuneServiceCollapser(IHystrixCollapserOptions options,
        IFortuneService fortuneService, ILoggerFactory logFactory) : base(options)
    {
        _logger = logFactory.CreateLogger<FortuneServiceCollapser>();
        _loggerFactory = logFactory;
        _fortuneService = fortuneService;
    }
}
```

Notice that `FortuneServiceCollapser` inherits from `HystrixCollapser<List<Fortune>, Fortune, int>`. It is a collapser that returns results of type `List<Fortune>`. Further notice that the constructor has `IHystrixCollapserOptions` as a first argument and that you can add additional constructor arguments. However, the first argument must be a `IHystrixCollapserOptions`.

### Using Collapsers in Applications

You can use Hystrix collapsers in a similar way to the way you use Hystrix commands. If you add the collapser to the service container, you can inject it into any controller, view, or other services created by the container.

The following example controller uses a Hystrix collapser called `FortuneServiceCollapser` that was added to the container by using `AddHystrixCollapser<FortuneServiceCollapser>("FortuneCollapser", Configuration)`.

The controller uses the `ExecuteAsync()` method on the injected command to retrieve a fortune by `Id`, as shown in the following example:

```csharp
public class HomeController : Controller
{
    FortuneServiceCollapser _fortuneService;

    public HomeController(FortuneServiceCollapser fortuneService)
    {
        _fortuneService = fortuneService;
    }

    [HttpGet("random")]
    public async Task<Fortune> Random()
    {
        // Fortune IDs are 1000-1049
        int id = random.Next(50) + 1000;
        return GetFortuneById(id);
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    protected Task<Fortune> GetFortuneById(int id)
    {
        // Use the FortuneServiceCollapser to obtain a Fortune by its Fortune Id
        _fortuneService.FortuneId = id;
        return _fortuneService.ExecuteAsync();
    }

}
```

The following listing shows the definition of the Hystrix collapser used in the previous example:

```csharp
using Steeltoe.CircuitBreaker.Hystrix;

public class FortuneServiceCollapser : HystrixCollapser<List<Fortune>, Fortune, int>
{
    ILogger<FortuneServiceCollapser> _logger;
    ILoggerFactory _loggerFactory;
    IFortuneService _fortuneService;

    public FortuneServiceCollapser(IHystrixCollapserOptions options,
        IFortuneService fortuneService, ILoggerFactory logFactory) : base(options)
    {
        _logger = logFactory.CreateLogger<FortuneServiceCollapser>();
        _loggerFactory = logFactory;
        _fortuneService = fortuneService;
    }

    public virtual int FortuneId { get; set; }

    public override int RequestArgument { get { return FortuneId; } }

    protected override HystrixCommand<List<Fortune>> CreateCommand(ICollection<ICollapsedRequest<Fortune, int>> requests)
    {
        _logger.LogInformation("Creating MultiFortuneServiceCommand to handle {0} number of requests", requests.Count);
        return new MultiFortuneServiceCommand(
            HystrixCommandGroupKeyDefault.AsKey("MultiFortuneService"),
            requests,
            _fortuneService,
            _loggerFactory.CreateLogger<MultiFortuneServiceCommand>());
    }

    protected override void MapResponseToRequests(List<Fortune> batchResponse, ICollection<ICollapsedRequest<Fortune, int>> requests)
    {
        foreach(var f in batchResponse)
        {
            foreach(var r in requests)
            {
                if (r.Argument == f.Id)
                {
                    r.Response = f;
                }
            }
        }
    }
}
```

The `FortuneServiceCollapser` class is a Hystrix collapser and is intended to batch up requests for `Fortune` instances and then run a single Hystrix command called `MultiFortuneServiceCommand` to return a `List<Fortune>`.

To understand how the collapser functions, you first need to understand what happens during the processing of an incoming request. For each incoming request, each separate run of a `FortuneServiceCollapser` instance causes the Hystrix collapser to add a request into a batch of requests to be handed off to a `MultiFortuneServiceCommand` created by `CreateCommand()`, shown in the previous example.

Notice that `CreateCommand()` takes a collection of `ICollapsedRequest<Fortune, int>` requests as an argument. Each element of that collection represents a request for a specific `Fortune` referred to by `Id` (an Integer) as an argument. For each of those requests, `CreateCommand` returns the Hystrix command responsible for executing those requests and returning a list of `Fortunes`.

Next, notice the `MapResponseToRequests` method. After the `MultiFortuneServiceCommand` finishes, some logic must be applied to map the `Fortunes` returned from the command (as `List<Fortune> batchResponse`) to the individual requests (in `ICollection<ICollapsedRequest<Fortune, int>> requests`) that we started with. Doing so enables each separate execution of a `FortuneServiceCollapser` that has been used to group a request to run asynchronously and to return the `Fortune` that was requested by it.

As with Hystrix commands, you have multiple ways in which you can cause collapsers to begin running.

To execute a collapser synchronously, you can use the `Execute()` method, as shown in the following example:

```csharp
var collapser = new FortuneServiceCollapser(...);
var result = collapser.Execute();
```

To execute a collapser asynchronously, you can use the `ExecuteAsync()` method:

```csharp
var collapser = new FortuneServiceCollapser(...);
var result = await collapser.ExecuteAsync();
```

You can also use `Rx.NET` extensions and observe the results by using the `Observe()` method. The `Observe()` method returns a `hot` observable that has already started execution.

```csharp
var collapser = new FortuneServiceCollapser(...);
IObservable<Fortune> observable = collapser.Observe();
var result = await observable.FirstOrDefaultAsync();
```

Alternatively, you can use the `ToObservable()` method to return a `cold` observable that has not started. Then, when you `Subscribe()` to it, the underlying collapser begins execution, and the results are available in the Observer's `OnNext(result)` method.

```csharp
var collapser = new FortuneServiceCollapser(...);
IObservable<Fortune> cold = collapser.ToObservable();
IDisposable subscription = cold.Subscribe((result) => { Console.WriteLine(result); });
```

### Using Hystrix Metrics

As `HystrixCommand` instances run, they generate metrics and status information on outcomes and latency and thread pool usage. This information can be useful in monitoring and managing your applications. The Hystrix Dashboard lets you extract and view these metrics in real time.

With Steeltoe, you can currently choose from two dashboards to view the captured metrics.

The first is the [Netflix Hystrix Dashboard](https://github.com/Netflix/Hystrix/wiki/Dashboard). This dashboard is appropriate when you are not running your application on Cloud Foundry/Tanzu Application Service -- for example, when you are developing and testing your application locally on your desktop.

The second is the [Spring Cloud Services Hystrix Dashboard](https://docs.vmware.com/en/Spring-Cloud-Services-for-VMware-Tanzu/2.1/spring-cloud-services/GUID-index.html). This dashboard is part of the [Spring Cloud Services v2](https://docs.pivotal.io/spring-cloud-services/) offering and is made available to applications through the normal service instance binding mechanisms on Cloud Foundry.

> As of Spring Cloud Services 3.x, the Hystrix Dashboard has been deprecated. The dashboard is still supported in version 2.x.

You should use the `Steeltoe.CircuitBreaker.Hystrix.MetricsEventsCore` package in an ASP.NET Core application when targeting the Netflix Hystrix Dashboard. When added to your application, it exposes a new REST endpoint in your application: `/hystrix/hystrix.stream`. This endpoint is used by the Netflix dashboard in receiving `SSE` metrics and status events from your application.

You should use the `Steeltoe.CircuitBreaker.Hystrix.MetricsStreamCore` package in an ASP.NET Core application when targeting the Spring Cloud Services Hystrix Dashboard. When added to your application, it starts up a background thread and uses messaging to push the metrics to the bound dashboard.

Regardless of which dashboard or package you choose to use, to enable your application to emit metrics and status information, you must make three changes in your `Startup` class:

* Add Hystrix Metrics stream service to the service container
* Use Hystrix Request context middleware in pipeline

To add the metrics stream to the service container, you must use the `AddHystrixMetricsStreams()` extension method in the `ConfigureService()` method in your `Startup` class, as follows:

```csharp
using Steeltoe.CircuitBreaker.Hystrix;

public class Startup {
    ...
    public IConfiguration Configuration { get; private set; }
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }
    public void ConfigureServices(IServiceCollection services)
    {
        // Add Steeltoe Discovery Client service
        services.AddDiscoveryClient(Configuration);

        // Add Hystrix command FortuneService to Hystrix group "FortuneServices"
        services.AddHystrixCommand<IFortuneService, FortuneService>("FortuneServices", Configuration);

        // Add framework services.
        services.AddMvc();

        // Add Hystrix Metrics to container
        services.AddHystrixMetricsStreams(Configuration);
        ...
    }
    ...
}
```

Next, you must configure the Hystrix `Request` contexts be initialized and available in every request being processed in the request processing pipeline. You can do so in the `Configure()` method of the `Startup` class as shown in this example:

```csharp
using Steeltoe.CircuitBreaker.Hystrix;

public class Startup {
    ...
    public void Configure(IApplicationBuilder app)
    {
        app.UseStaticFiles();

        // Use Hystrix Request contexts
        app.UseHystrixRequestContext();

        app.UseMvc();
    }
    ...
}
```

#### Netflix Hystrix Dashboard

Once you have made the changes described earlier, you can then use the Netflix Hystrix Dashboard by following these instructions:

##### Run Hystrix Dashboard with Docker

There are a few images available on Docker Hub that provide basic Hystrix Dashboard functionality. This example has been tested with:

```bash
docker run --rm -ti -p 7979:7979 --name steeltoe-hystrix steeltoeoss/hystrix-dashboard
```

Once this image is up and running, you should be able to browse to your [local dashboard](http://localhost:7979/hystrix/) and provide the address of the Hystrix stream(s) you wish to monitor.

> NOTE: This image may be running on a separate network than your application. Remember to provide a stream address that is accessible from within the Docker network. This may require using the external IP address of your workstation or the name of the machine instead of 127.0.0.1 or localhost.

> NOTE: This Hystrix Dashboard image is not production ready and is intended for development and testing only.

##### Run Hystrix Dashboard with Java

To run a Hystrix Dashboard without Docker:

1. Install Java 8 JDK.
1. Install Maven 3.x.
1. Clone the Spring Cloud Samples Hystrix dashboard: `cd https://github.com/spring-cloud-samples/hystrix-dashboard`
1. Change to the hystrix dashboard directory: `cd hystrix-dashboard`
1. Start the server `mvn spring-boot:run`
1. Open a browser window and connect to the dashboard: <http://localhost:7979>
1. In the first field, enter the endpoint that is exposing the hystrix metrics (eg: <http://localhost:5555/hystrix/hystrix.stream>)
1. Click the monitor button.
1. Interact with the application to trigger usage of the circuits. Observe the values changing in the Hystrix dashboard.

#### Cloud Foundry/TAS Dashboard

When you want to use a Hystrix Dashboard on Cloud Foundry/Tanzu Application Service, you must have previously installed Spring Cloud Services. If that has been done, you can create and bind a instance of the dashboard to the application by using the Cloud Foundry CLI, as follows:

```bash
# Create Hystrix dashboard instance named `myHystrixService`
cf create-service p-circuit-breaker-dashboard standard myHystrixService

# Wait for service to become ready
cf services
```

For more information on using the Hystrix Dashboard on Cloud Foundry, see the [Spring Cloud Services v2](https://docs.vmware.com/en/Spring-Cloud-Services-for-VMware-Tanzu/2.1/spring-cloud-services/GUID-index.html) documentation.

Once the service is bound to your application, the settings are available in `VCAP_SERVICES`.

Once you have performed the steps described earlier and you have made the changes described in the use metrics section, you can use the Spring Cloud Services dashboard by following these instructions:

1. Open a browser and connect to the TAS Apps Manager.
1. Follow [these instructions](https://docs.vmware.com/en/Spring-Cloud-Services-for-VMware-Tanzu/2.1/spring-cloud-services/GUID-circuit-breaker-using-the-dashboard.html) to open the Hystrix Dashboard service.
1. Use your application and see the metrics begin to flow.
