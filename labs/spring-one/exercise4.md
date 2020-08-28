---
uid: labs/spring-one/exercise4
_disableContribution: true
_disableToc: true
_disableFooter: true
_homePath: "./"
_disableNav: true
---
[vs-add-configserver]: ~/labs/images/vs-add-configserver.png "Add configuration server library"

[home-page-link]: index.md
[exercise-1-link]: exercise1.md
[exercise-2-link]: exercise2.md
[exercise-3-link]: exercise3.md
[exercise-4-link]: exercise4.md

|[<< Previous Exercise][exercise-3-link]||
|:--|--:|

# Using an external configuration provider

## Goal

Setup an external git repo holding configuration values and using Spring Config to retrieve the values, in a .NET Core application.

## Expected Results

With a running instance of Spring Config server, pointed at a git repository, navigate to an endpoint in a .NET Core application and see the values output.

> [!NOTE]
> For this exercise a git repo and Spring Config server have already been initialized.

## Get Started

Once created, open the new project in your IDE of choice (we will be using Visual Studio throughout this lab). The first action is to bring in the Steeltoe packages to the app. You can do this by right clicking on the project name in the solution explorer and choose `Manage NuGet packages...`. In the package manger window choose `Browse`, search for `Steeltoe.Extensions.Configuration.ConfigServer`, and install.

# [.NET CLI](#tab/dotnet-cli)

```powershell
dotnet add package Steeltoe.Extensions.Configuration.ConfigServer
```

# [Visual Studio](#tab/visual-studio)

![vs-add-configserver]

***

## Implement Spring Config client

Open `Program.cs` and implement a Spring Config client in the host builder. Visual Studio should prompt to add the `using Steeltoe.Extensions.Configuration.ConfigServer` direction.

```csharp
public static IHostBuilder CreateHostBuilder(string[] args) =>
	Host.CreateDefaultBuilder(args)
		.ConfigureWebHostDefaults(webBuilder => {
			webBuilder

        //implement config server client
				.AddConfigServer()
				.UseStartup<Startup>();
		})

		.AddAllActuators()
		;
```

## Create a Values controller

Create a new class in the `Controllers` folder named `ValuesController.cs` and paste the following within.

```csharp
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace WebApplication1.Controllers
{
    [Route("[controller]")]
    [ValuesController]
    public class ValuesController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ILogger<ValuesController> _logger;

        public ValuesController(IConfiguration config, ILogger<ValuesController> logger)
        {
            _config = config;
            _logger = logger;
        }
        
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            var val1 = _config["Value1"];
            var val2 = _config["Value2"];
            return new string[] { val1, val2 };
        }
    }
}
```

Configuration the client to use the correct instance address and name in `appsettings.json`.

```json
  "spring": {
    "application": {
      "name": "myapplication"
    },
    "cloud": {
      "config": {
        "validateCertificates": false,
        "uri": %%SPRING_CONFIG_URI%%,
        "Username": %%SPRING_CONFIG_USERNAME%%,
        "Password": %%SPRING_CONFIG_PASSWORD%%,
        "FailFast": %%SPRING_CONFIG_FAILFAST%%
      }
    }
  }
```

> [!NOTE]
> For the application to find its values in the Spring Config server git repo, there must be a file named the same as the value of spring:application:name. In this example "my-values" added to appsettings matches a file in the repo named my-values.yml.

## Run the application

With the client implemented in host builder and the value being output in the controller, we are ready to see everything in action.

# [.NET CLI](#tab/dotnet-cli)

```powershell
dotnet run
```

# [Visual Studio](#tab/visual-studio)

Start the application by clicking the `Debug > Start Debugging` top menu item.

![vs-run-application]

***

Once started your default browser should open and automatically load the weather forecast endpoint.

![run-weatherforecast]

## See the config values output

To execute the values endpoint, replace `WeatherForecast` with `values` in the browser address bar. The values json should be shown.

```json
["some-val","another-val"]
```

## Summary

With an existing Spring Config server running that was configured to retrieve values from a yaml file in a git repository, we added a Spring Config client to our application and output two values. With this architecture in place you can now do things like updating the yaml file in the git repository and visit the `/actuators/refresh` management endpoint in the application. This will automatically refresh values within the application without and down time (or restart). You could store a server's connection name in the yaml, and have the application retrieve the value. As the application moves through different environments (dev, test, staging, prod) the connection value can change, but the original tested application stays unchanged.

|[<< Previous Exercise][exercise-3-link]||
|:--|--:|
