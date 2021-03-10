---
uid: guides/service-discovery/consul
title: Console Service Discovery
tags: []
_disableFooter: true
_hideTocVersionToggle: true
---

## Using Service Discovery with Hashicorp Consul server

This tutorial takes you through setting up two .NET Core applications using services discovery. The first will register it's endpoints for discovery, and the second will discover the first's services.

First, **start a Hashicorp Consul server** using the [Steeltoe dockerfile](https://github.com/steeltoeoss/dockerfiles), start a local instance of Consul.

```powershell
docker run --publish 8500:8500 consul
```

Next, **create a .NET Core WebAPI** that registers itself as a service.

1. Create a new ASP.NET Core WebAPI app with the [Steeltoe Initializr](https://start.steeltoe.io)
   ![](~/guides/images/initializr/service-discovery.png)
1. Name the project "Consul_Register_Example"
1. Add the "Redis" dependency
1. Click **Generate** to download a zip containing the new project
1. Extract the zipped project and open in your IDE of choice
1. Set the instance address in **appsettings.json**
1.
1.
1. **SteeltoeVersion:** 2.4 for the latest stable
1. Project Metadata:
   **Name:**
   **Target Framework:** netcoreapp3.1 is the latest stable 1.**Dependencies:** Discovery
   1.Click **Generate Project** to download a zip containing the new project

1. Extract the zipped project and open in your IDE of choice (we use Visual Studio)
1. Set the instance address in **appsettings.json**

# [Local](#tab/local)

Update with the Consul info

```json
{
  "spring": {
    "application": {
      "name": "Consul-Register-Example"
    }
  },
  "consul": {
    "host": "localhost",
    "port": 8500,
    "discovery": {
      "enabled": true,
      "register": true,
      "port": "8080",
      "ipAddress": "localhost",
      "preferIpAddress": true
    }
  }
}
```

---

> [!TIP]
> Looking for additional params to use when connecting? Have a look at the docs [here](/service-discovery/docs).

1. Validate the port number the app will be served on, in **Properties\launchSettings.json**

# [Local](#tab/local)

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

---

1. Run the application and confirm it has registered with Consul

# [Local](#tab/local)

1. Using the .NET cli


    ```powershell
    dotnet run <PATH_TO>\Consul_Register_Example.csproj
    ```

1. Navigate to the endpoint (you may need to change the port number) [http://localhost:5000/api/values](http://localhost:5000/api/values)
1. Using Visual Studio
   Choose the top _Debug_ menu, then choose _Start Debugging (F5)_. This should bring up a browser with the app running.\*
1. Navigate to the endpoint (you may need to change the port number) [http://localhost:8080/api/values](http://localhost:8080/api/values)
1. Navigate to the Consol dashboard at [http://localhost:8500/](http://localhost:8500/) to see the service listed.
1. Leave the application running while you continue to the next steps, you'll be connecting to it.

---

Then, **create another .NET Core WebAPI** that will discover the registered service.

1. Create a new ASP.NET Core WebAPI app with the [Steeltoe Initializr](https://start.steeltoe.io)
   ![](~/guides/images/initializr/service-discovery.png)
1. **SteeltoeVersion:** 2.4 for the latest stable
1. Project Metadata:
   **Name:** Consul_Discover_Example
   **Target Framework:** netcoreapp3.1 is the latest stable 1.**Dependencies:** Discovery
   1.Click **Generate Project** to download a zip containing the new project

1. Update the discovery values to not register in **appsettings.json**

# [Local](#tab/local)

Update with the Consul info

```json
{
  "spring": {
    "application": {
      "name": "Consul-Discover-Example"
    }
  },
  "consul": {
    "host": "localhost",
    "port": 8500,
    "discovery": {
      "enabled": true,
      "register": false
    }
  }
}
```

---

1. Validate the port number the app will be served on, in **Properties\launchSettings.json**

# [Local](#tab/local)

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

---

1. Change the values controller to make a request to the discovery service and return the result in **contollers\ValuesController.cs**

```csharp
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Steeltoe.Common.Discovery;
using System.Net.Http;

namespace Consul_Discover_Example.Controllers
{
[Route("api/[controller]")]
[ApiController]
public class ValuesController : ControllerBase
{
private readonly ILogger _logger;
DiscoveryHttpClientHandler _handler;
public ValuesController(ILogger<ValuesController> logger, IDiscoveryClient client)
{
_logger = logger;
_handler = new DiscoveryHttpClientHandler(client);
}

// GET api/values
[HttpGet]
public async Task<string> Get()
{
var client = new HttpClient(_handler, false);
return await client.GetStringAsync("http://Consul-Register-Example/api/values");
}
}
}
```

> [!NOTE]
> Notice the use of `Consul-Register-Example` as the URI. Because Discovery has been enabled, the negotiation with the discovery Server happens automatically.

1. Run the app to see discovery in action

# [Local](#tab/local)

1. Using the .NET cli


    ```powershell
    dotnet run <PATH_TO>\Consul_Discover_Example.csproj
    ```

1. Navigate to the endpoint (you may need to change the port number) [http://localhost:5000/api/values](http://localhost:5000/api/values)
1. Using Visual Studio
   Choose the top _Debug_ menu, then choose _Start Debugging (F5)_. This should bring up a browser with the app running.\*
1. Navigate to the endpoint (you may need to change the port number) [http://localhost:8081/api/values](http://localhost:8081/api/values)

---

1.  Once the discovery app loads in the browser you will see the 2 values that were retrieved from the registered app.
    "["value1","value2"]"

        </app>
