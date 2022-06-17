# Introduction

This first part of the documentation is a high-level overview of Steeltoe RabbitMQ and the underlying concepts. It includes some code snippets to get you up and running as quickly as possible.

## Quick Start

This is the five-minute tour to help you get started with Steeltoe and RabbitMQ.

Prerequisites: [Download RabbitMQ broker](https://www.rabbitmq.com/download.html) and follow the instructions to install and run the broker locally.
Then grab the `Steeltoe.Messaging.RabbitMQ` nuget and all its dependencies. The easiest way to do so is to declare a dependency in your build tool.
For example, simply add the following to your `.csproj` file:

```XML
  <ItemGroup>
    <PackageReference Include="Steeltoe.Messaging.RabbitMQ" Version="3.x.x" />
  </ItemGroup>
```

### Compatibility

The minimum .NET Core version dependency is 3.1.

The  `RabbitMQ.Client` .NET client library version is 5.1.2.

### Very, Very Quick

This section offers the fastest introduction.

First, add the following `using` statements:

```csharp
using System;
using Steeltoe.Messaging.RabbitMQ.Config;
using Steeltoe.Messaging.RabbitMQ.Connection;
using Steeltoe.Messaging.RabbitMQ.Core;
```

The following example uses a simple console application to send and receive a message:

```csharp
class Program
{
    static void Main(string[] args)
    {
        var connectionFactory = new CachingConnectionFactory()
        {
            Host = "localhost"
        };
        var admin = new RabbitAdmin(connectionFactory);
        admin.DeclareQueue(new Queue("myqueue"));
        var template = new RabbitTemplate(connectionFactory);
        template.ConvertAndSend("myqueue", "foo");
        var foo = template.ReceiveAndConvert<string>("myqueue");
        admin.DeleteQueue("myQueue");
        connectionFactory.Dispose();
        Console.WriteLine(foo);
     }
}
```

Note that there is also a `IConnectionFactory` in the native .NET RabbitMQ client.
But notice above we use the Steeltoe abstractions with no references to the native client.
The Steeltoe factory caches channels (and optionally connections) for reuse.
Steeltoe relies on the default exchange provided by the broker (since none is specified in the send operation) and the default binding of all queues to the default exchange by their name (thus, we can use the queue name as a routing key in the send operation).
Those behaviors are defined in the AMQP specification.

### Using DI in Console App

The following example repeats the preceding example but uses Steeltoe components together with dependency injection:

```csharp
class Program
{
 private static ServiceProvider container;

    static void Main(string[] args)
    {
        container = CreateServiceContainer();
        var admin = container.GetRabbitAdmin();
        var template = container.GetRabbitTemplate();
        try
        {
            template.ConvertAndSend("myqueue", "foo");
            var foo = template.ReceiveAndConvert<string>("myqueue");
            Console.WriteLine(foo);
        }
        finally
        {
            // Delete queue and shutdown container
            admin.DeleteQueue("myqueue");
            container.Dispose();
        }
    }
    private static ServiceProvider CreateServiceContainer()
    {
        var services = new ServiceCollection();

        // Add some logging
        services.AddLogging(b =>
        {
            b.SetMinimumLevel(LogLevel.Information);
            b.AddDebug();
            b.AddConsole();
        });

        // Add configuration as needed
        var config = new ConfigurationBuilder().Build();

        // Configure rabbit options from the configuration
        var rabbitSection = config.GetSection(RabbitOptions.PREFIX);
        services.Configure<RabbitOptions>(rabbitSection);
        services.AddSingleton<IConfiguration>(config);

        // Add Steeltoe Rabbit services
        services.AddRabbitServices();
        services.AddRabbitAdmin();
        services.AddRabbitTemplate();

        // Add queue to be declared
        services.AddRabbitQueue(new Queue("myqueue"));

        // Build container and start
        var provider = services.BuildServiceProvider();
        provider.GetRequiredService<IHostedService>().StartAsync(default).Wait();
        return provider;
    }
}
```

### Using a GenericHost with RabbitListeners

This example uses Steeltoe together with a .NET Generic host and shows how to configure the services:

```csharp
class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        RabbitMQHost.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {
            // Add a queue to be declared
            services.AddRabbitQueue(new Queue("myqueue"));

            // Add the rabbit listener service
            services.AddSingleton<MyRabbitListener>();

            // Tell Steeltoe about listener
            services.AddRabbitListeners<MyRabbitListener>();

            // Add a background service to send messages to myqueue
            services.AddSingleton<IHostedService, MyRabbitSender>();

        });
}

// Listener to receive messages
public class MyRabbitListener
{
    private ILogger<MyRabbitListener> logger;
    public MyRabbitListener(ILogger<MyRabbitListener> logger)
    {
        this.logger = logger;
    }

    [RabbitListener("myqueue")]
    public void Listen(string input)
    {
        // Process message from myqueue
        logger.LogInformation(input);
    }
}

// Hosted service to periodically send messages
public class MyRabbitSender : IHostedService
{
    private RabbitTemplate template;
    private Timer timer;

    public MyRabbitSender(IServiceProvider services)
    {
        template = services.GetRabbitTemplate();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        timer = new Timer(Sender, null, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2));
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        timer.Dispose();
        return Task.CompletedTask;
    }

    private void Sender(object state)
    {
        template.ConvertAndSend("myqueue", "foo");
    }
}
```

## RabbitMQ Basics

[RabbitMQ](https://www.rabbitmq.com/) is a lightweight, reliable, scalable, and portable message broker based on the AMQP protocol.
Steeltoe uses the `RabbitMQ` .NET client to communicate to a broker over the AMQP protocol.

RabbitMQ configuration is controlled by external configuration properties in `Spring:RabbitMq:*+`.
For example, you might declare the following section in your `appsettings.json`:

```json
{
  "Spring": {
    "RabbitMq": {
      "Host": "localhost",
      "Port": 5672,
      "Username": "admin",
      "Password": "secret"
    }
  }
}
```

Alternatively, you could configure the same connection settings using the `Addresses` property:

```json
"Spring": {
    "RabbitMq": {
        "Addresses": "amqp://admin:secret@localhost"
    }
}
```

>When specifying addresses as shown above, the `host` and `port` properties are ignored.
If the address uses the `amqps` protocol, SSL support is automatically enabled.

See [`RabbitOptions`](https://github.com/SteeltoeOSS/Steeltoe/blob/master/src/Messaging/src/RabbitMQ/Config/RabbitOptions.cs) for more of the supported options.

>TIP: See [Understanding AMQP, the protocol used by RabbitMQ](https://www.rabbitmq.com/tutorials/amqp-concepts.html) for more details.

### Sending a Message

Steeltoe `RabbitTemplate` and `RabbitAdmin` can be used to send and receive messages to a broker and are auto-configured when you add them to the .NET service container. You can inject them directly into your own services as follows:

```csharp
using Steeltoe.RabbitMQ.Core.RabbitAdmin;
using Steeltoe.RabbitMQ.Core..RabbitTemplate;

public class MyService
{

    private readonly RabbitAdmin amqpAdmin;
    private readonly RabbitTemplate amqpTemplate;

    public MyService(RabbitAdmin amqpAdmin, RabbitTemplate amqpTemplate) {
        this.amqpAdmin = amqpAdmin;
        this.amqpTemplate = amqpTemplate;
    }

    // ...

}
```

If a `IMessageConverter` service is defined, it is associated automatically to the auto-configured `RabbitTemplate`.

If necessary, any `Steeltoe.RabbitMQ.Config.Queue` that is defined as a service in the container is automatically used to declare a corresponding queue on the RabbitMQ broker instance.

To retry operations, you can enable retries on the `RabbitTemplate` (for example, in the event that the broker connection is lost):

```json
"Spring": {
    "RabbitMq": {
        "Template": {
            "Retry" : {
                "Enabled" : true,
                "InitialInterval : 2000
            }
        }
    }
}
```

>Note: Retries are disabled by default.

If you need to create more `RabbitTemplate` instances or if you want to override the defaults, Steeltoe provides extension methods `AddRabbitTemplate(.. , Action<IServiceProvider, RabbitTemplate> configure)` that you can use to configure a `RabbitTemplate` with the settings you desire.

### Receiving a Message

When the RabbitMQ infrastructure is present, any service can be annotated with a `[RabbitListener()]` attribute to create a listener endpoint.
A default `DirectRabbitListenerContainerFactory` is automatically added to the .NET service container and configured properly. These factories are used to create `DirectRabbitListenerContainer`s which process and deliver messages to the methods that are annotated with `[RabbitListener()]`s.

If a `IMessageConverter` service has been defined, it is automatically associated with the default container factory as well.

The following sample component creates a listener endpoint that will process messages from the `someQueue` queue:

```csharp
public class MyService {

    [RabbitListener("someQueue")]
    public void ProcessMessage(string content) {
        // ...
    }

}
```

For advanced cases, if you need to create more `DirectRabbitListenerContainerFactory` instances or if you want to override the default settings, Steeltoe provides extension methods that you can use to initialize a `DirectRabbitListenerContainerFactory` with the settings that will be used when creating `DirectRabbitListenerContainer`s.

For instance, the following adds another factory to the service container that uses a specific `IMessageConverter` when configuring `DirectRabbitListenerContainer`:

```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Configure any rabbit client values;
        var rabbitSection = Configuration.GetSection(RabbitOptions.PREFIX);
        services.Configure<RabbitOptions>(rabbitSection);

        ...
        // Add a named container factory `myFactory`
        services.AddRabbitListenerContainerFactory((p, f) =>
        {
            f.ServiceName = "myFactory";
            f.MessageConverter = new JsonMessageConverter();
        });
        ...
    }
}
```

Then you can reference the factory in any `[RabbitListener()]`-annotated method, as follows:

```csharp
public class MyService
{

    [RabbitListener("someQueue", containerFactory="myFactory")]
    public void ProcessMessage(string content)
    {
        // ...
    }
}
```

<!-- TODO:  This needs to be implemented in the next release

You can enable retries to handle situations where your listener throws an exception.
By default, a `RejectAndDontRequeueRecoverer` is used, but you can define a `IMessageRecoverer` of your own.
When retries are exhausted, the message is rejected and either dropped or routed to a dead-letter exchange if the broker is configured to do so.
By default, retries are disabled. -->

>IMPORTANT: By default, if retries are disabled and the listener throws an exception, the delivery is retried indefinitely.
You can modify this behavior in one of two ways: Set the `DefaultRequeueRejected` property in the container factory to `false` so that zero re-deliveries are attempted, or throw a `RabbitRejectAndDontRequeueException` to signal the message should be rejected.
The latter is the mechanism used when retries are enabled and the maximum number of delivery attempts is reached.

### Using RabbitMQ Host

The Steeltoe RabbitMQHost extends the Microsoft Generic Host and provides for auto configuration of Steeltoe RabbitMQ services.

> [!NOTE]
> For more detailed examples of using the RabbitMQ Host, please refer to the [Messaging](https://github.com/SteeltoeOSS/Samples/tree/main/Messaging/src) solutions in the [Steeltoe Samples Repository](https://github.com/SteeltoeOSS/Samples).

Below are two code snippets within `Program.cs` and `Startup.cs` that demonstrate the usage of RabbitMQHost:

```csharp
public static IHostBuilder CreateHostBuilder(string[] args) =>
    RabbitMQHost.CreateDefaultBuilder()
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        });
```


```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Add a queue to the container that the rabbit admin will discover and declare at startup
    services.AddRabbitQueue(new Queue("myQueue"));

    // Add singleton that will process incoming messages
    services.AddSingleton<RabbitListenerService>();

    // Tell steeltoe about singleton so it can wire up queues with methods to process queues (i.e. RabbitListenerAttribute)
    services.AddRabbitListeners<RabbitListenerService>();

    services.AddControllers();
}
```

If you don't use the RabbitMQHost within your application, you will need to add the below additional configuration in `Startup.cs` to get RabbitMQ services up and running:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Configure any rabbit client values;
    var rabbitSection = Configuration.GetSection(RabbitOptions.PREFIX);
    services.Configure<RabbitOptions>(rabbitSection);

    // Add steeltoe rabbit services
    services.AddRabbitServices();
    
    // Add the steeltoe rabbit admin client... will be used to declare queues below
    services.AddRabbitAdmin();

    // Add the rabbit client template used for send and receiving messages
    services.AddRabbitTemplate();

    // ...
}
```

