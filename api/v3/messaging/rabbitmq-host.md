# RabbitMQ Host

## Introduction
RabbitMQ Host (introduced in version 3.1.0) makes it possible to bootstrap Steeltoe RabbitMQ services and configuration with minimal setup. This allows your application to include less wiring to get RabbitMQ messaging up and running.

> [!NOTE]
> For more detailed examples of RabbitMQ Host, please refer to the [Messaging](https://github.com/SteeltoeOSS/Samples/tree/main/Messaging/src) solutions in the [Steeltoe Samples Repository](https://github.com/SteeltoeOSS/Samples).

Below is a before/after comparison of the Steeltoe RabbitMQ messaging configuration and setup:

**Before**

<i>Program.cs (CreateHostBuilder)</i>
```csharp
public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        });

```

<i>Startup.cs (ConfigureServices)</i>
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

    // Add a queue to the container that the rabbit admin will discover and declare at startup
    services.AddRabbitQueue(new Queue("myQueue"));

    // Add the rabbit client template used for send and receiving messages... used in RabbitTestController
    services.AddRabbitTemplate();

    // Add singleton that will process incoming messages
    services.AddSingleton<RabbitListenerService>();

    // Tell steeltoe about singleton so it can wire up queues with methods to process queues (i.e. RabbitListenerAttribute)
    services.AddRabbitListeners<RabbitListenerService>();

    services.AddControllers();
}
```

**After**

<i>Program.cs</i>
```csharp
public static IHostBuilder CreateHostBuilder(string[] args) =>
    RabbitMQHost.CreateDefaultBuilder()
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        });
```

<i>Startup.cs</i>
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
