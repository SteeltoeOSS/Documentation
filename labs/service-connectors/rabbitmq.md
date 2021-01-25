---
uid: labs/service-connectors/rabbitmq
title: RabbitMQ Messaging
tags: []
_disableFooter: true
---

## Using Service Connectors with RabbitMQ

This tutorial takes you through setting up a .NET Core application with the RabbitMQ service connector.

First, **start a RabbitMQ instance** using the [Steeltoe dockerfile](https://github.com/steeltoeoss/dockerfiles), start a local instance of RabbitMQ.

 ```powershell
 docker run --publish 5672:5672 steeltoeoss/rabbitmq
 ```

Next, **create a .NET Core WebAPI** that interacts with RabbitMQ

1. Create a new ASP.NET Core WebAPI app with the [Steeltoe Initializr](https://start.steeltoe.io)
    ![Steeltoe Initialzr](~/labs/images/initializr/rabbitmq.png)
1. Name the project "RabbitMQ_Connector"
1. Add the "RabbitMQ" dependency
1. Click **Generate** to download a zip containing the new project
1. Extract the zipped project and open in your IDE of choice
1. Validate the correct logging level is set in **appsettings.json**

    ```json
    {
      "LogLevel": {
        "Default": "Information",
        "System": "Information",
        "Microsoft": "Information"
      }
    }
    ```

    > [!NOTE]
    > Make sure the correct logging is set or you'll miss the output. The default logging level should be set to `Information`.

1. Set the instance address in **appsettings.json**

    ```json
    {
      "rabbitmq": {
        "client": {
          "server": "127.0.0.1",
          "port": "5672",
          "username": "guest",
          "password": "guest"
        }
      }
    }
    ```

    > [!TIP]
    > If you would like to see how to customize the connection have a look at the [docs](~/api/v3/welcome/index.md)

**Run** the application

  # [.NET cli](#tab/cli)

  ```powershell
  dotnet run <PATH_TO>\RabbitMQ_Connector.csproj
  ```

  Navigate to the endpoint (you may need to change the port number) [http://localhost:5000/api/values](http://localhost:5000/api/values)

  # [Visual Studio](#tab/vs)

  1. Choose the top *Debug* menu, then choose *Start Debugging (F5)*. This should bring up a browser with the app running
  1. Navigate to the endpoint (you may need to change the port number) [http://localhost:8080/api/values](http://localhost:8080/api/values)
  
  ***

As the app loads in the browser it will create a message queue, listen for new messages on the queue, and write 5 messages. Once finished the output will let you know everything has completed - "Wrote 5 message to the info log. Have a look!". Looking at the app logs (console) you will see...

  ```bash
  Received message: Message 1
  Received message: Message 2
  Received message: Message 3
  Received message: Message 4
  Received message: Message 5
  ```
