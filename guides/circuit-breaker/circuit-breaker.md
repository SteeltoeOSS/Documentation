---
uid: guides/circuit-breakers/circuit-breaker
title: Circuit Breakers
tags: []
_disableFooter: true
_hideTocVersionToggle: true
---

## Implementing Circuit Breakers

This tutorial takes you through setting up a .NET Core application that implements a circuit breaker pattern.

> [!NOTE]
> For more detailed examples, please refer to the [FortuneTeller (Circuit Breaker)](https://github.com/SteeltoeOSS/Samples/tree/main/CircuitBreaker/src/FortuneTeller) project in the [Steeltoe Samples Repository](https://github.com/SteeltoeOSS/Samples).

### Start a instance of the Hystrix dashboard

<i>(Depending on your hosting platform this is done in several ways.)</i>

1. Using the [Steeltoe dockerfile](https://github.com/steeltoeoss/dockerfiles), start a local instance of the Circuit Breaker dashboard

   ```powershell
   docker run --publish 7979:7979 steeltoeoss/hystrix-dashboard
   ```

1. Using the [Steeltoe dockerfile](https://github.com/steeltoeoss/dockerfiles), start a local instance of Eureka Discovery Client

   ```powershell
   docker run --publish 8761:8761 steeltoeoss/eureka-server
   ```

1. Using the [Steeltoe dockerfile](https://github.com/steeltoeoss/dockerfiles), start a local instance of RabbitMQ

   ```powershell
   docker run --publish 5672:5672 --publish 15672:15672 rabbitmq:3-management
   ```

1. Confirm the service is running by viewing the dashboard - [http://localhost:7979/hystrix](http://localhost:7979/hystrix)

### Create a .NET Core WebAPI that implements circuit breaker pattern

1.  Create a new ASP.NET Core WebAPI app with the [Steeltoe Initializr](https://start.steeltoe.io)
    <img src="~/guides/images/initializr/circuit-breaker-dependency.png" alt="Steeltoe Initialzr - Circuit Breaker" width="100%">
1.  Name the project "CircuitBreakerExample"
1.  Add the "Netflix Hystrix Circuit Breaker" dependency
1.  Add the "Eureka Discovery Client" dependency
1.  Add the "RabbitMQ" dependency
1.  Click **Generate Project** to download a zip containing the new project
1.  Extract the zipped project and open in your IDE of choice
1.  Set the instance address and name in **appsettings.json**

    ```json
    {
      "Spring": {
        "Application": {
          "Name": "mycircuitbreaker"
        }
      },
      "Eureka": {
        "Client": {
          "ServiceUrl": "http://localhost:8761/eureka/",
          "ShouldRegisterWithEureka": true,
          "ValidateCertificates": false
        }
      },
      "Hystrix": {
        "command": {
          "MyCircuitBreaker": {
            "threadPoolKeyOverride": "MyCircuitBreakerTPool"
          }
        }
      }
    }
    ```

1.  Set the local environment variables in **launchsettings.json** (Properties folder)

    ```json
    {
      "CircuitBreakerExample": {
        "commandName": "Project",
        "launchBrowser": true,
        "launchUrl": "api/values",
        "applicationUrl": "http://localhost:5555",
        "environmentVariables": {
          "ASPNETCORE_ENVIRONMENT": "Development",
          "BUILD": "LOCAL",
          "PORT": 5555
        }
      }
    }
    ```

1.  Replace default "GET" controller method in **ValuesController.cs** (Controllers folder) with the below:

    ```csharp
    [HttpGet]
    public async Task<ActionResult<string>> GetAsync()
    {
        MyCircuitBreakerCommand cb = new MyCircuitBreakerCommand("ThisIsMyBreaker");
        cb.IsFallbackUserDefined = true;
        return await cb.ExecuteAsync();
    }
    ```

### Run the application

# [.NET cli](#tab/cli)

```powershell
dotnet run <PATH_TO>\CircuitBreakerExample.csproj
```

Navigate to the endpoint [http://localhost:5555/api/values](http://localhost:5555/api/values)

# [Visual Studio](#tab/vs)

1. Choose the top _Debug_ menu, then choose _Start Debugging (F5)_. This should bring up a browser with the app running
1. Navigate to the endpoint [http://localhost:5555/api/values](http://localhost:5555/api/values)

---

1.  Navigate to application stream to ensure it is running - [http://localhost:5555/hystrix/hystrix.stream](http://localhost:5555/hystrix/hystrix.stream)
1.  Navigate to dashboard at [http://localhost:7979/hystrix](http://localhost:7979/hystrix) and enter the application stream url in the stream url text box (ex. [http://localhost:5555/hystrix/hystrix.stream](http://localhost:5555/hystrix/hystrix.stream))
    <img src="~/guides/images/circuit-breaker-dashboard.png" alt="Circuit Breaker Landing" width="100%">

1.  Refresh the application in your browser a few times and go back to the dashboard to see it logging live activity.
    <img src="~/guides/images/circuit-breaker-closed.png" alt="Circuit Breaker Dashboard" width="100%">
