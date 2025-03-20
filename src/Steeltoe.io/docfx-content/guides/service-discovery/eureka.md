---
uid: guides/service-discovery/eureka
title: Eureka Service Discovery
tags: []
_disableFooter: true
_hideTocVersionToggle: true
---

> [!NOTE]
> This guide applies to Steeltoe v3. Please [open an issue](https://github.com/SteeltoeOSS/Documentation/issues/new/choose) if you'd like to help update the content for Steeltoe v4.

## Using Service Discovery with Eureka Server

This tutorial takes you through setting up two .NET Core applications using services discovery. The first will register it's endpoints for discovery, and the second will discover the first's services.

> [!NOTE]
> For more detailed examples, please refer to the [FortuneTeller (Discovery)](https://github.com/SteeltoeOSS/Samples/tree/3.x/Discovery/src) solution in the [Steeltoe Samples Repository](https://github.com/SteeltoeOSS/Samples/tree/3.x).

First, **start a Eureka Server** using the [Steeltoe dockerfile](https://github.com/steeltoeoss/dockerfiles), start a local instance of Eureka.

```powershell
docker run --publish 8761:8761 steeltoeoss/eureka-server
```

Next, **create a .NET Core WebAPI** that registers itself as a service.

1. Create a new ASP.NET Core WebAPI app with the [Steeltoe Initializr](https://start.steeltoe.io)
1. Name the project "EurekaRegisterExample"
1. Add the "Eureka Discovery Client" dependency
1. Click **Generate** to download a zip containing the new project
1. Extract the zipped project and open in your IDE of choice
1. Set the Eureka instance address in **appsettings.json**

   ```json
   {
     "spring": {
       "application": {
         "name": "EurekaRegisterExample"
       }
     },
     "eureka": {
       "client": {
         "serviceUrl": "http://localhost:8761/eureka/",
         "shouldFetchRegistry": "false",
         "shouldRegisterWithEureka": true,
         "validateCertificates": false
       },
       "instance": {
         "port": "8080",
         "ipAddress": "localhost",
         "preferIpAddress": true
       }
     }
   }
   ```

   > [!TIP]
   > Looking for additional params to use when connecting? Have a look at the [docs](/service-discovery/docs).

1. Change "applicationUrl" to "http://localhost:8080", in **launchSettings.json**

   ```json
   "EurekaRegisterExample": {
      "commandName": "Project",
      "launchBrowser": true,
      "launchUrl": "swagger",
      "applicationUrl": "http://localhost:8080",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
   }
   ```

1. Run the application and confirm it has registered with Eureka

# [.NET cli](#tab/cli)

```powershell
dotnet run <PATH_TO>\EurekaRegisterExample.csproj
```

Navigate to the endpoint (you may need to change the port number) [http://localhost:8080/api/WeatherForecast](http://localhost:8080/api/WeatherForecast)

# [Visual Studio](#tab/vs)

1. Choose the top _Debug_ menu, then choose _Start Debugging (F5)_. This should bring up a browser with the app running
1. Navigate to the endpoint (you may need to change the port number) [http://localhost:8080/api/WeatherForecast](http://localhost:8080/api/WeatherForecast)

---

1. Navigate to the Eureka dashboard at [http://localhost:8761/](http://localhost:8761/) to see the service listed.
1. Leave the application running while you continue to the next steps, you'll be connecting to it.

Now, **create another .NET Core WebAPI** that will discover the registered service.

1. Create a new ASP.NET Core WebAPI app with the [Steeltoe Initializr](https://start.steeltoe.io)
1. Name the project "EurekaDiscoverExample"
1. Add the "Eureka Discovery Client" dependency
1. Click **Generate** to download a zip containing the new project
1. Extract the zipped project and open in your IDE of choice
1. Set the Eureka instance address in **appsettings.json**

   ```json
   {
     "spring": {
       "application": {
         "name": "EurekaDiscoverExample"
       }
     },
     "eureka": {
       "client": {
         "serviceUrl": "http://localhost:8761/eureka/",
         "shouldFetchRegistry": "true",
         "shouldRegisterWithEureka": false,
         "validateCertificates": false
       },
       "instance": {}
     }
   }
   ```

1. Change "applicationUrl" to "http://localhost:8081" and "launchBrowser" to false, in **launchSettings.json**

   ```json
   "EurekaDiscoverExample": {
      "commandName": "Project",
      "launchBrowser": true,
      "launchUrl": "swagger",
      "applicationUrl": "http://localhost:8081",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
   }
   ```

1. Change the WeatherForecast controller to make a request to the discovery service and return the result in **contollers\WeatherForecastController.cs**

   ```csharp
   using System.Threading.Tasks;
   using Microsoft.Extensions.Logging;
   using Microsoft.AspNetCore.Mvc;
   using Steeltoe.Common.Discovery;
   using System.Net.Http;

   [Route("api/[controller]")]
   [ApiController]
   public class WeatherForecastController : ControllerBase {
     private readonly ILogger _logger;
     DiscoveryHttpClientHandler _handler;
     public WeatherForecastController(ILogger<WeatherForecastController> logger, IDiscoveryClient client) {
       _logger = logger;
       _handler = new DiscoveryHttpClientHandler(client);
     }

     // GET api/values
     [HttpGet]
     public async Task<string> Get() {
       var client = new HttpClient(_handler, false);
       return await client.GetStringAsync("http://EurekaRegisterExample/api/WeatherForecast");
     }
   }
   ```

   > [!NOTE]
   > Notice the use of `EurekaRegisterExample` as the URI. Because Discovery has been enabled, the negotiation with the discovery Server happens automatically.

**Run** the app to see discovery in action

# [.NET cli](#tab/cli)

```powershell
dotnet run <PATH_TO>\EurekaDiscoverExample.csproj
```

Navigate to the endpoint (you may need to change the port number) [http://localhost:8081/api/WeatherForecast](http://localhost:8081/api/WeatherForecast)

# [Visual Studio](#tab/vs)

1. Choose the top _Debug_ menu, then choose _Start Debugging (F5)_. This should bring up a browser with the app running
1. Navigate to the endpoint (you may need to change the port number) [http://localhost:8081/api/WeatherForecast](http://localhost:8081/api/WeatherForecast)

---

Once the discovery app loads in the browser you will see these values that were retrieved from the registered app.
"[
  {
    "date": "2023-04-07T15:49:53.1328335+05:00",
    "temperatureC": 46,
    "temperatureF": 114,
    "summary": "Hot"
  },
  {
    "date": "2023-04-08T15:49:53.1332687+05:00",
    "temperatureC": 11,
    "temperatureF": 51,
    "summary": "Hot"
  },
  {
    "date": "2023-04-09T15:49:53.1332704+05:00",
    "temperatureC": 49,
    "temperatureF": 120,
    "summary": "Hot"
  },
  {
    "date": "2023-04-10T15:49:53.1332705+05:00",
    "temperatureC": 41,
    "temperatureF": 105,
    "summary": "Balmy"
  },
  {
    "date": "2023-04-11T15:49:53.1332706+05:00",
    "temperatureC": -4,
    "temperatureF": 25,
    "summary": "Freezing"
  }
]"
