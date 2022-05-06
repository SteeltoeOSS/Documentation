---
uid: guides/messaging/rabbitmq
title: Messaging with RabbitMQ
tags: []
_disableFooter: true
_hideTocVersionToggle: true
---

## RabbitMQ Messaging

This tutorial takes you through setting up 2 .NET Core applications that interact through RabbitMQ.

> [!NOTE]
> For more detailed examples, please refer to the [Messaging](https://github.com/SteeltoeOSS/Samples/tree/main/Messaging/src) solution in the [Steeltoe Samples Repository](https://github.com/SteeltoeOSS/Samples).

First, **start a RabbitMQ instance**.
Using the [Steeltoe dockerfile](https://github.com/steeltoeoss/dockerfiles), start a local instance of RabbitMQ

```powershell
docker run --publish 5672:5672 steeltoeoss/rabbitmq
```

Next **create a .NET Core WebAPI** that will ensure the queue is created and write messages to it.

1. Create a new ASP.NET Core WebAPI app with the [Steeltoe Initializr](https://start.steeltoe.io)
1. Name the project "WriteToRabbitMQ"
1. Add RabbitMQ Messaging Dependency
1. Click **Generate Project** to download a zip containing the new project
1. Extract the zipped project and open in your IDE of choice
1. Open the package manager console
   <img src="~/guides/images/open-package-manager-console.png" alt="Visual Studio - Open Package Manager" width="100%">
1. Install NuGet distributed packages

   ```powershell
   Install-Package -Id Steeltoe.Messaging.RabbitMQ
   ```
1. Ensure RabbitMQHost appears in **Program.cs**

   <i>See [RabbitMQHost](../../api/v3/messaging/rabbitmq-host.md) documentation to further understand what is happening behind the scenes</i>
   ```csharp
   public static IHostBuilder CreateHostBuilder(string[] args) =>
        RabbitMQHost.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
   ```
1. Add rabbit messaging configurations to **Startup.cs**

   ```csharp
   using Steeltoe.Messaging.RabbitMQ.Config;
   using Steeltoe.Messaging.RabbitMQ.Extensions;

   public class Startup {
     public const string RECEIVE_AND_CONVERT_QUEUE = "steeltoe_message_queue";

     public void ConfigureServices(IServiceCollection services){

       // Add a queue to the message container that the rabbit admin will discover and declare at startup
       services.AddRabbitQueue(new Queue(RECEIVE_AND_CONVERT_QUEUE));
     }
   }
   ```

1. Create a **new controller class** Controllers\WriteMessageQueueController.cs

   ```csharp
   using Microsoft.Extensions.Logging;
   using Steeltoe.Messaging.RabbitMQ.Core;

   [ApiController]
   [Route("[controller]")]
   public class WriteMessageQueueController : ControllerBase {
     public const string RECEIVE_AND_CONVERT_QUEUE = "steeltoe_message_queue";
     private readonly ILogger<WriteMessageQueueController> _logger;
     private readonly RabbitTemplate _rabbitTemplate;
     private readonly RabbitAdmin _rabbitAdmin;

     public WriteMessageQueueController(ILogger<WriteMessageQueueController> logger, RabbitTemplate rabbitTemplate, RabbitAdmin rabbitAdmin) {
     _logger = logger;
     _rabbitTemplate = rabbitTemplate;
     _rabbitAdmin = rabbitAdmin;
     }

     [HttpGet()]
     public ActionResult<string> Index() {
       var msg = "Hi there from over here.";

       _rabbitTemplate.ConvertAndSend(RECEIVE_AND_CONVERT_QUEUE, msg);

       _logger.LogInformation($"Sending message '{msg}' to queue '{RECEIVE_AND_CONVERT_QUEUE}'");

       return "Message sent to queue.";
     }
   }
   ```

1. Validate the port number the app will be served on, in **Properties\launchSettings.json**

   ```json
   "iisSettings": {
     "windowsAuthentication": false,
     "anonymousAuthentication": true,
     "iisExpress": {
       "applicationUrl": "http://localhost:8080",
       "sslPort": 0
     }
   }
   ```

**Run** the application (you won't be interacting directly with it, we just need it running in the background)

# [.NET cli](#tab/cli)

```powershell
dotnet run<PATH_TO>\WriteToRabbitMQ.csproj
```

# [Visual Studio](#tab/vs)

1. Choose the top _Debug_ menu, then choose _Start Debugging (F5)_. This should bring up a browser with the app running

---

> [!NOTE]
> Minimize windows and leave the application running as you continue on to the next step.

Now **create a .NET Core WebAPI** that will monitor the queue and output anything received.

1. Create a new ASP.NET Core WebAPI app with the [Steeltoe Initializr](https://start.steeltoe.io)
1. Name the project "MonitorRabbitMQ"
1. No need to add any dependencies
1. Click **Generate Project** to download a zip containing the new project
1. Extract the zipped project and open in your IDE of choice

   > [!TIP]
   > Open the second app in a different Visual Studio instance.

1. Open the package manager console
   <img src="~/guides/images/open-package-manager-console.png" alt="Visual Studio - Open Package Manager" width="100%">
1. Install NuGet distributed packages

   ```powershell
   Install-Package -Id Steeltoe.Messaging.RabbitMQ
   ```

1. Create a **new service class** named RabbitListenerService.cs

   ```csharp
   using Microsoft.Extensions.Logging;
   using Steeltoe.Messaging.RabbitMQ.Attributes;

   public class RabbitListenerService {
     public const string RECEIVE_AND_CONVERT_QUEUE = "steeltoe_message_queue";
     private ILogger _logger;

     public RabbitListenerService(ILogger<RabbitListenerService> logger) {
       _logger = logger;
     }

     [RabbitListener(RECEIVE_AND_CONVERT_QUEUE)]
     public void ListenForAMessage(string msg) {
       _logger.LogInformation($"Received the message '{msg}' from the queue.");
     }
   }
   ```

1. Add rabbit messaging configurations to **Startup.cs**

   ```csharp
   using Steeltoe.Messaging.RabbitMQ.Extensions;

   public class Startup {
     public const string RECEIVE_AND_CONVERT_QUEUE = "steeltoe_message_queue";

     public void ConfigureServices(IServiceCollection services){
       // Add steeltoe rabbit services
       services.AddRabbitServices();

       // Add singleton that will process incoming messages
       services.AddSingleton<RabbitListenerService>();

       // Tell steeltoe about singleton so it can wire up queues with methods to process queues
       services.AddRabbitListeners<RabbitListenerService>();
     }
   }
   ```

1. Validate the port number the app will be served on, in **Properties\launchSettings.json**

   ```json
   "iisSettings": {
     "windowsAuthentication": false,
     "anonymousAuthentication": true,
     "iisExpress": {
       "applicationUrl": "http://localhost:8081",
       "sslPort": 0
     }
   }
   ```

**Run** the application

# [.NET cli](#tab/cli)

```powershell
dotnet run<PATH_TO>\WriteToRabbitMQ.csproj
```

# [Visual Studio](#tab/vs)

1. Choose the top _Debug_ menu, then choose _Start Debugging (F5)_. This should bring up a browser with the app running

---

> [!NOTE]
> If a browser window popped up, minimize it.

**Validate** the apps are working properly and the message queue is in use.

1. View the WriteToRabbitMQ project message logs and verify there is a message stating it is "Sending message to queue". If you don't see the message refresh the endpoint `https://localhost:8080/WriteMessageQueue` to have a new message written.
   <img src="~/guides/images/visual-studio-output-debug.png" alt="Visual Studio - Debug Output" width="100%">
1. View the MonitorRabbitMQ project message logs and verify there is a message stating it "Received the message from the queue".
   <img src="~/guides/images/visual-studio-output-debug-messagereceived.png" alt="Visual Studio - Debug Output Message Received" width="100%">
