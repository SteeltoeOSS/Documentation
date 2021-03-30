---
uid: guides/get-to-know-steeltoe/exercise1
_disableContribution: true
_disableToc: false
_disableFooter: true
_homePath: "./index.html"
_disableNav: true
---

[exercise-1-link]: exercise1.md
[exercise-2-link]: exercise2.md
[exercise-3-link]: exercise3.md
[exercise-4-link]: exercise4.md

| [<< Home](index.md) | [Next Exercise >>][exercise-2-link] |
| :------------------ | ----------------------------------: |

# Getting to know Steeltoe

## Goal

Understand how Steeltoe is distributed (Nuget) and how one adds components into an existing application.

## Expected Results

Begin building an API that will be enhanced with more components in the next exercise(s).

## Get Started

Let's start by creating a brand new .NET Core webapi project.

# [Visual Studio](#tab/visual-studio)

Select "Create a new project". (If Visual Studio is already open, choose `File > New > Project`.)
<img src="~/guides/images/vs-get-started.png" alt="Visual Studio - Get Started" width="100%">

Choose "ASP.NET Core Web Application" from the default templates.
<img src="~/guides/images/vs-new-proj.png" alt="Visual Studio New Project" width="100%">

The default project name WebApplication1 will be used throughout, but you can rename.
<img src="~/guides/images/vs-configure-project.png" alt="Visual Studio - Name Project" width="100%">

Choose an application type of API, everything else can keep its default value.
<img src="~/guides/images/vs-create-project.png" alt="Visual Studio - Create an API Project" width="100%">

# [.NET CLI](#tab/dotnet-cli)

```powershell
dotnet new webapi -n WebApplication1
cd WebApplication1
```

To use Visual Studio as your IDE open Visual Studio program, choose "Open a project or solution", navgiate to the WebApplication1 folder, and select the file "WebApplication1.csproj".

To use VS Code as your IDE:

```powershell
code .
```

---

## Add Project Dependencies

Once the project is created and opened in your IDE, the first action is to bring in the Steeltoe packages to the app.

# [Visual Studio](#tab/visual-studio)

Right click on the project name in the solution explorer and choose "Manage NuGet packages...". In the package manger window choose "Browse", then search for `Steeltoe.Management.Endpointcore`, and install.
<img src="~/guides/images/vs-add-endpointcore.png" alt="Endpointcode NuGet dependency" width="100%">

Then search for the `Steeltoe.Extensions.Logging.DynamicLogger` package and install.
<img src="~/guides/images/vs-add-dynamiclogger.png" alt="Dynamiclogger NuGet dependency" width="100%">

Finally the `Steeltoe.Management.TracingCore` package and install.
<img src="~/guides/images/vs-add-tracingcore.png" alt="TracingCode NuGet dependency" width="100%">

# [.NET CLI](#tab/dotnet-cli)

```powershell
dotnet add package Steeltoe.Management.Endpointcore
dotnet add package Steeltoe.Extensions.Logging.DynamicLogger
dotnet add package Steeltoe.Management.TracingCore
```

---

## Implement Steeltoe packages

Steeltoe features are broken up into packages, giving you the option to only bring in and extend the dependencies needed. As we implement each package within the application we'll discuss why these packages were chosen.

Open "Program.cs" in the IDE and add the using statement

```csharp
using Steeltoe.Management.Endpoint;
using Steeltoe.Extensions.Logging;
```

Then append the 'adding' statements to the host builder and save the changes

```csharp
public static IHostBuilder CreateHostBuilder(string[] args) =>
	Host.CreateDefaultBuilder(args)
		.ConfigureWebHostDefaults(webBuilder => {
			webBuilder.UseStartup<Startup>();
		})

		//Steeltoe actuators
		.AddHealthActuator()
		.AddInfoActuator()
		.AddLoggersActuator()

    //Steeltoe dynamic logging
    .AddDynamicLogging()
		;
```

We've implemented 3 features within the application by adding these actuators

- The health actuator will add a new endpoint at `/actuator/health`. Internally this function uses .NET's IHealthContributor to "decide" if everything is reporting good health and responds with HTTP 200 status. Also within the response body there is a json formatted message to accommodate a deeper check that specific platforms like Cloud Foundry and Kubernetes do.
- The info actuator adds a new endpoint at `/actuator/info`. This function gathers all kinds of information like versioning information, select package information, and DLL info. Everything is formatted as json and included in the response.
- The loggers actuator enables enhanced log message details via ILogger.

## Enable distributed tracing

Now open "Startup.cs" in the IDE and add the using statement

```csharp
using Steeltoe.Management.Tracing;
```

Then add the distributed tracing feature to the services container

```csharp
public void ConfigureServices(IServiceCollection services) {
	services.AddControllers();

	//Steeltoe distributed tracing
	services.AddDistributedTracing(Configuration);
}
```

With the addition of distributed tracing option, under the covers Steeltoe uses the OpenTelemetry specification to generate spans and traces throughout the application, as requests are received. No additional configuration is needed but if you wanted to manipulate how traces and spans are created you could provide settings in "appsettings.json", [read the docs](/api/v3/tracing/distributed-tracing.html) for more information.

> [!TIP]
> Having the combination of the logging actuator and distributed tracing implemented, Steeltoe will automatically append the application name, span Id, and trace Id on log messages when possible. This can be very handy when debugging a specific happening or error in production.

## Add a sample logging message

To see the trace logging in action lets add a log message in "Controllers\WeatherForecastController.cs" controller class. Append the below message as the first line with the 'HttpGet' function.

```csharp
[HttpGet]
public IEnumerable<WeatherForecast> Get() {
	//Testing Steeltoe logging with distributed tracing
	_logger.LogInformation("Hi there");

	//...
}
```

## Run the application

With the packages implemented in host builder, distributed tracing activated, and a sample log message being written to console, we are ready to see everything in action. Run the application.

# [Visual Studio](#tab/visual-studio)

Clicking the `Debug > Start Debugging` top menu item. You may be prompted to "trust the IIS Express SSL certificate" and install the certificate. It's safe, trust us. Once started your default browser should open and automatically load the weather forecast endpoint.

<img src="~/guides/images/vs-run-application.png" alt="Run the project" width="100%">

# [.NET CLI](#tab/dotnet-cli)

Executing the below command will start the application. You will see a log message written telling how to navigate to the application. It should be [http://localhost:5000/weatherforecast](http://localhost:5000/weatherforecast).

```powershell
dotnet run
```

---

With the application running and the weather forecast endpoint loaded your browser should show the following

<img src="~/guides/images/weatherforecast-endpoint.png" alt="Weatherforecast endpoint" width="100%">

## Discover the health endpoint

Let's look at the health endpoint. Replace `WeatherForecast` with `actuator/health` in the browser address bar. The health page will load with json formatted info.

<img src="~/guides/images/health-endpoint.png" alt="Health endpoint" width="100%">

As we discussed above, the fact that the page loaded (status of 200) is the first communication to the application's platform that it is healthy. Secondarily the application has output information to help certain platforms gain a deeper knowledge of app health. Learn more about the health endpoint [here](/api/v3/management/health.html).

## Discover the info endpoint

Now navigate to the info endpoint by replacing `health` with `info` in the address bar.

<img src="~/guides/images/info-endpoint.png" alt="Info endpoint" width="100%">

We have loaded the bare minimum application info for this example. You could build your own 'IInfoContributor' and add all kinds of meta data and connection information. Learn more [here](/api/v3/management/info.html).

## Observe trace and spans appended to logs

Finally lets look at the log message that was written.

# [Visual Studio](#tab/visual-studio)

Go back to Visual Studio (keep the app running) and locate the Output tab (it should be in one of the bottom frames). Choose `Webapplication1 - ASP.NET Core Web Server` in the "from" dropdown and scroll to the bottom of the log.

<img src="~/guides/images/trace-log.png" alt="Trace logs" width="100%">

# [.NET CLI](#tab/dotnet-cli)

Go back to the terminal window where the application was started. The logs should be streaming. Locate the following line:

```plaintext
[WebApplicaion1, 917e146c942117d2, 917e146c942117d2, true] Hi there
```

---

Notice the additional information prepended to the message. This will be automatically written to logs, so whatever platform or cloud you might be using the message will give you quite a bit of context.

- The first item is the application's name
- Second is the OpenTelemetry generated span id
- Third is the OpenTelemetry generated trace id

## Stop the application

# [Visual Studio](#tab/visual-studio)

Either close the browser window or click the red stop button in the top menu.

# [.NET CLI](#tab/dotnet-cli)

Use the key combination "ctrl+c" on windows/linux or "cmd+c" on Mac.

---

## Summary

These are the basics of any cloud ready microservice. Logging and debugging are significantly different than a traditional IIS environment. But! A developer shouldn't be spending tons of time coding these boilerplate-type things. Heeelllo Steeltoe!

|     | [Next Exercise >>][exercise-2-link] |
| :-- | ----------------------------------: |
