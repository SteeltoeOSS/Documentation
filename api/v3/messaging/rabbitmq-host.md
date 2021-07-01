# RabbitMQ Host

## Introduction
RabbitMQHost extends the Microsoft Generic Host to provide the auto-wiring of Steeltoe RabbitMQ services and configuration.

> [!NOTE]
> For more detailed examples of RabbitMQ Host, please refer to the [Messaging](https://github.com/SteeltoeOSS/Samples/tree/main/Messaging/src) solutions in the [Steeltoe Samples Repository](https://github.com/SteeltoeOSS/Samples).

Below are two code snippets within Program.cs and Startup.cs that demonstrate the usage of RabbitMQHost:

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

Without leverageing RabbitMQHost within your solution, you will need to add the below additional configuration in Startup.cs to get RabbitMQ services up and running:

<i>Startup.cs</i>

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
