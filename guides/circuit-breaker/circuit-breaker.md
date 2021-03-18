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

First, **start a instance of the Hystrix dashboard**. Depending on your hosting platform this is done in several ways.

1. Using the [Steeltoe dockerfile](https://github.com/steeltoeoss/dockerfiles), start a local instance of the Circuit Breaker dashboard

   ```powershell
   docker run --publish 7979:7979 steeltoeoss/hystrix-dashboard
   ```

1. Confirm the service is running by viewing the dashboard - [https://localhost:7979/hystrix](https://localhost:7979/hystrix)

**Create a .NET Core WebAPI** that implements the circuit breaker pattern

1. Create a new ASP.NET Core WebAPI app with the [Steeltoe Initializr](https://start.steeltoe.io)
   <img src="~/guides/images/initializr/circuit-breaker-dependency.png" alt="Steeltoe Initialzr - Circuit Breaker" width="100%">
1. Name the project "CircuitBreakerExample"
1. Add the "Netflix Hystrix Circuit Breaker" dependency
1. Click **Generate Project** to download a zip containing the new project
1. Extract the zipped project and open in your IDE of choice
1. Set the instance address and name in **appsettings.json**

   ```json
   {
     "spring": {
       "application": {
         "name": "CircuitBreakerExample"
       }
     }
   }
   ```

**Run** the application

# [.NET cli](#tab/cli)

```powershell
dotnet run<PATH_TO>\CircuitBreakerExample.csproj
```

Navigate to the endpoint (you may need to change the port number) [http://localhost:5000/api/values](http://localhost:5000/api/values)

# [Visual Studio](#tab/vs)

1. Choose the top _Debug_ menu, then choose _Start Debugging (F5)_. This should bring up a browser with the app running
1. Navigate to the endpoint (you may need to change the port number) [http://localhost:8080/api/values](http://localhost:8080/api/values)

---

Refresh the application in your browser a few times and go back to the dashboard to see it logging live activity.
![Circuit Breaker](~/guides/images/circuit-breaker-closed.png)
