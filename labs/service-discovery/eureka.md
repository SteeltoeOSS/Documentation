---
uid: labs/service-discovery/eureka
title: Eureka Service Discovery
tags: []
_disableFooter: true
---

## Using Service Discovery with Eureka Server

This tutorial takes you through setting up two .NET Core applications using services discovery. The first will register it's endpoints for discovery, and the second will discover the first's services.

First, **start a Eureka Server** using the [Steeltoe dockerfile](https://github.com/steeltoeoss/dockerfiles), start a local instance of Eureka.

 ```powershell
 docker run --publish 8761:8761 steeltoeoss/eureka-server
 ```

Next, **create a .NET Core WebAPI** that registers itself as a service.

1. Create a new ASP.NET Core WebAPI app with the [Steeltoe Initializr](https://start.steeltoe.io)
    ![Steeltoe Initialize](~/labs/images/initializr/eureka-register-discovery.png)
1. Name the project "Eureka_Register_Example"
1. Add the "Eureka Discovery Client" dependency
1. Click **Generate** to download a zip containing the new project
1. Extract the zipped project and open in your IDE of choice
1. Set the Eureka instance address in **appsettings.json**

    ```json
    {
      "spring": {
        "application": {
          "name": "Eureka_Register_Example"
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

1. Validate the port number the app will be served on, in **launchSettings.json**

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

1. Run the application and confirm it has registered with Eureka

  # [.NET cli](#tab/cli)

  ```powershell
  dotnet run <PATH_TO>\Eureka_Register_Example.csproj
  ```

  Navigate to the endpoint (you may need to change the port number) [http://localhost:5000/api/values](http://localhost:5000/api/values)

  # [Visual Studio](#tab/vs)

  1. Choose the top *Debug* menu, then choose *Start Debugging (F5)*. This should bring up a browser with the app running
  1. Navigate to the endpoint (you may need to change the port number) [http://localhost:8080/api/values](http://localhost:8080/api/values)
  
  ***

1. Navigate to the Eureka dashboard at [http://localhost:8761/](http://localhost:8761/) to see the service listed.
1. Leave the application running while you continue to the next steps, you'll be connecting to it.

Then, **create another .NET Core WebAPI** that will discover the registered service.

1. Create a new ASP.NET Core WebAPI app with the [Steeltoe Initializr](https://start.steeltoe.io)
  ![Steeltoe Initializr](~/labs/images/initializr/eureka-discover-discovery.png)
1. Name the project "Eureka_Discover_Example"
1. Add the "Eureka Discovery Client" dependency
1. Click **Generate** to download a zip containing the new project
1. Extract the zipped project and open in your IDE of choice
1. Set the Eureka instance address in **appsettings.json**

    ```json
    {
      "spring": {
        "application": {
          "name": "Eureka_Discover_Example"
        }
      },
      "eureka": {
        "client": {
          "serviceUrl": "http://localhost:8761/eureka/",
          "shouldFetchRegistry": "true",
          "shouldRegisterWithEureka": false,
          "validateCertificates": false
        },
        "instance": {
        }
      }
    }
    ```

1. Validate the port number the app will be served on, in **launchSettings.json**

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

1. Change the values controller to make a request to the discovery service and return the result in **contollers\ValuesController.cs**

    ```csharp
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.AspNetCore.Mvc;
    using Steeltoe.Common.Discovery;
    using System.Net.Http;
    
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase {
      private readonly ILogger _logger;
      DiscoveryHttpClientHandler _handler;
      public ValuesController(ILogger<ValuesController> logger, IDiscoveryClient client) {
        _logger = logger;
        _handler = new DiscoveryHttpClientHandler(client);
      }
      
      // GET api/values
      [HttpGet]
      public async Task<string> Get() {
        var client = new HttpClient(_handler, false);
        return await client.GetStringAsync("http://Eureka_Register_Example/api/values");
      }
    }
    ```

    > [!NOTE]
    > Notice the use of `Eureka_Register_Example` as the URI. Because Discovery has been enabled, the negotiation with the discovery Server happens automatically.

**Run** the app to see discovery in action

  # [.NET cli](#tab/cli)

  ```powershell
  dotnet run <PATH_TO>\Eureka_Discover_Example.csproj
  ```

  Navigate to the endpoint (you may need to change the port number) [http://localhost:5000/api/values](http://localhost:5000/api/values)

  # [Visual Studio](#tab/vs)

  1. Choose the top *Debug* menu, then choose *Start Debugging (F5)*. This should bring up a browser with the app running
  1. Navigate to the endpoint (you may need to change the port number) [http://localhost:8080/api/values](http://localhost:8080/api/values)
  
  ***

Once the discovery app loads in the browser you will see the 2 values that were retrieved from the registered app.
"["value1","value2"]"
