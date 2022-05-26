---
uid: guides/messaging/rabbitmq
title: Messaging with RabbitMQ
tags: []
_disableFooter: true
_hideTocVersionToggle: true
---

## RabbitMQ Messaging

This tutorial takes you through setting up a .NET Core application that sends and receives messages through RabbitMQ. 

> [!NOTE]
> For more detailed examples, please refer to the [Messaging](https://github.com/SteeltoeOSS/Samples/tree/main/Messaging/src) solution in the [Steeltoe Samples Repository](https://github.com/SteeltoeOSS/Samples).

First, **start a RabbitMQ instance**.
Using the [Steeltoe dockerfile](https://github.com/steeltoeoss/dockerfiles), start a local instance of RabbitMQ

```powershell
docker run --publish 5672:5672 steeltoeoss/rabbitmq
```

Next **create a .NET Core WebAPI** application that will create a RabbitMQ `queue` upon startup, write messages to the queue and also receive the messages that are sent to it.

1. Create a new ASP.NET Core WebAPI app with the [Steeltoe Initializr](https://start.steeltoe.io).
1. Name the project "ReadWriteToRabbitMQ".
1. Add the Messaging Dependency - `RabbitMQ Messaging Client`.
1. Add the Messaging Dependenty - `RabbitMQ Messaging Listener`.
1. Click **Generate Project** to download a zip containing the new project.
1. Extract the zipped project and open in your IDE of choice.
1. You can go ahead and **run** the application when you're ready.
# [.NET cli](#tab/cli)

```powershell
dotnet run<PATH_TO>\ReadWriteToRabbitMQ.csproj
```

# [Visual Studio](#tab/vs)

1. Choose the top _Debug_ menu, then choose _Start Debugging (F5)_. This should bring up a browser with the app running

---

To **validate** the application is working properly follow these steps:

1. Open a browser and hit the application endpoint `https://localhost:5000/WriteMessageQueue`.  This will cause the application to send a message to the queue named `steeltoe_message_queue`. You can verify this by viewing the console message logs and you should see a message stating it is `Sending message xxx to queue`.
1. To see the code that is sending the message to the queue look in the `WriteMessageController.cs` file.
1. Next, look futher into the logs and verify there is a message stating it `Received the message from the queue`. This indicates that the sent message was received by the application.
1. To see the code that is receiving the message look in the `RabbitListenerService.cs` file.

