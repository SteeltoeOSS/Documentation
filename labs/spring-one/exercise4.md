---
uid: labs/spring-one/exercise4
_disableContribution: true
_disableToc: true
_disableFooter: true
_homePath: "./"
_disableNav: true
---
[vs-add-configserver]: ~/labs/images/vs-add-configserver.png "Add configuration server library"
[vs-new-folder]: ~/labs/images/vs-new-folder.png "Create a new project folder"
[vs-new-class]: ~/labs/images/vs-new-class.png "Create a new project class"
[run-weatherforecast]: ~/labs/images/weatherforecast-endpoint.png "Weatherforecast endpoint"
[vs-run-application]: ~/labs/images/vs-run-application.png "Run the project"

[home-page-link]: index.md
[exercise-1-link]: exercise1.md
[exercise-2-link]: exercise2.md
[exercise-3-link]: exercise3.md
[exercise-4-link]: exercise4.md
[summary-link]: summary.md

|[<< Previous Exercise][exercise-3-link]||
|:--|--:|

# Using an external configuration provider

## Goal

Setup an external git repo holding configuration values and using Spring Config to retrieve the values, in a .NET Core application.

## Expected Results

With a running instance of Spring Config server, navigate to an endpoint in a .NET Core application and see the values output.

> [!NOTE]
> For this exercise a Spring Config server have already been initialized. The settings have been preloaded below.

## Get Started

To communicate with an external config server we're going to need to add a client to the previously created application.

# [Visual Studio](#tab/visual-studio)

Right click on the project name in the solution explorer and choose "Manage NuGet packages...". In the package manger window choose "Browse", then search for `Steeltoe.Extensions.Configuration.ConfigServerCore`, and install.

![vs-add-configserver]

# [.NET CLI](#tab/dotnet-cli)

```powershell
dotnet add package Steeltoe.Extensions.Configuration.ConfigServerCore
```

***

## Implement Spring Config client

Open "Program.cs" and implement a Spring Config client in the host builder.

```csharp
using Steeltoe.Extensions.Configuration.ConfigServer;
```

```csharp
public static IHostBuilder CreateHostBuilder(string[] args) =>
	Host.CreateDefaultBuilder(args)
		.ConfigureWebHostDefaults(webBuilder => {
			webBuilder

        //implement config server client
				.AddConfigServer()
				.UseStartup<Startup>();
		})
    
    //Steeltoe actuators
		.AddAllActuators()

    //Steeltoe dynamic logging
    .AddDynamicLogging()
		;
```

## Create a Values controller

Create a new class in the 'Controllers' folder named `ValuesController.cs`.

# [Visual Studio](#tab/visual-studio)

Right click on the 'Controllers' folder and choose "Add" > "Class..." and name it `ValuesController.cs`.

![vs-new-class]

# [.NET CLI](#tab/dotnet-cli)

```powershell
cd Controllers
dotnet new classlib -n "ValuesController.cs"
```

***

Open the newly created class file in your IDE and replace the 'using' statements in the file with the below.

```csharp
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
```

Replace the class statement with this. Don't change the 'namespace' part, just the class within the namespace.

```csharp
[Route("[controller]")]
[ApiController]
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
      var val1 = _config["message"];
      return new string[] { val1 };
  }
}
```

In 'appsettings.json' add the following json just below the "sqlserver" section. This should be preloaded with the correct connection values of a Spring Config server.

```json
,"spring": {
  "application": {
    "name": "myapplication"
  },
  "cloud": {
    "config": {
      "validateCertificates": false,
      "FailFast": %%SPRING_CONFIG_FAILFAST%%,
      "uri": %%SPRING_CONFIG_URI%%,
      //"Username": %%SPRING_CONFIG_USERNAME%%,
      //"Password": %%SPRING_CONFIG_PASSWORD%%
    }
  }
}
```

> [!NOTE]
> Notice the value of 'spring:application:name' in the json. This value of "myapplication" will be used to connect the correct values in the Spring Config server.

## Run the application

With the data context in place, we are ready to see everything in action. Run the application.

# [Visual Studio](#tab/visual-studio)

Clicking the `Debug > Start Debugging` top menu item. You may be prompted to "trust the IIS Express SSL certificate" and install the certificate. It's safe, trust us. Once started your default browser should open and automatically load the weather forecast endpoint.

![vs-run-application]

# [.NET CLI](#tab/dotnet-cli)

Executing the below command will start the application. You will see a log message written telling how to navigate to the application. It should be [http://localhost:5000/weatherforecast](http://localhost:5000/weatherforecast).

```powershell
dotnet run
```

***

With the application running and the weather forecast endpoint loaded your browser should show the following

![run-weatherforecast]

## See the config values output

To execute the values endpoint, replace `WeatherForecast` with `values` in the browser address bar. The values will be retrieved from the Spring Config server and output in the window.

```json
["hello from development config"]
```

## Stop the application

# [Visual Studio](#tab/visual-studio)

Either close the browser window or click the red stop button in the top menu.

# [.NET CLI](#tab/dotnet-cli)

Use the key combination "ctrl+c" on windows/linux or "cmd+c" on Mac.

***

## Summary

With an existing Spring Config server running that was configured to retrieve values from a yaml file, we added a Spring Config client to our application and output the retrieved vale. With this architecture in place you can now do things like updating the yaml file and visit the `/actuator/refresh` management endpoint in the application. This will automatically refresh values within the application without and down time (or restart). You could store a server's connection name in the yaml and have the application retrieve the value. As the application moves through different environments (dev, test, staging, prod) the connection value can change, but the original tested application stays unchanged.

We've just begun to scratch the surface of what Spring Config can really do and all it's many features. Learn more about config in the [Steeltoe docs](https://steeltoe.io/docs/3/configuration/config-server-provider).

|[<< Previous Exercise][exercise-3-link]|[Workshop Summary >>][summary-link]|
|:--|--:|
