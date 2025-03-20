---
uid: guides/get-to-know-steeltoe/exercise2
_disableContribution: true
_disableToc: false
_disableFooter: true
_homePath: "./index.html"
_disableNav: true
_hideTocVersionToggle: true
---

> [!NOTE]
> This guide applies to Steeltoe v3. Please [open an issue](https://github.com/SteeltoeOSS/Documentation/issues/new/choose) if you'd like to help update the content for Steeltoe v4.

[home-page-link]: index.md
[exercise-1-link]: exercise1.md
[exercise-2-link]: exercise2.md
[exercise-3-link]: exercise3.md
[exercise-4-link]: exercise4.md

| [<< Previous Exercise][exercise-1-link] | [Next Exercise >>][exercise-3-link] |
| :-------------------------------------- | ----------------------------------: |

# Exploring all actuators

## Goal

See all actuators running and learn what options are available.

## Expected Results

Enhance the app created in the previous exercise to enable all actuator endpoints.

## Get Started

Open "Program.cs" and replace the existing Steeltoe 'Add' statements with the single 'all actuators' statement.

```csharp
// Add services to the container.

// Steeltoe actuators
builder.AddAllActuators();
```

Expose all the actuator endpoints for debugging and demonstration purposes in "appsettings.json". Append the below JSON, just after the "AllowedHosts" line (should be around line 8). And save the file.

```json
,"Management": {
  "Endpoints": {
    "Actuator": {
      "Exposure": {
        "Include": [ "*" ]
      }
    }
  }
}
```

> [!TIP]
> Enable Intellisense for Steeltoe settings within `appsettings.json` by adding a schema reference inside the JSON like this: `"$schema": "https://steeltoe.io/schema/v3/schema.json"`

## Run the application

With all actuators registered in the application builder, we are ready to see everything in action. Run the application.

# [Visual Studio](#tab/visual-studio)

Click the `Debug > Start Debugging` top menu item. You may be prompted to "trust the IIS Express SSL certificate" and install the certificate. Once started, your default browser should open and automatically load the weather forecast endpoint.

![Run the project](../images/vs-run-application.png)

# [.NET CLI](#tab/dotnet-cli)

Executing the below command will start the application. You will see a log message written, telling you how to navigate to the application. It should be [http://localhost:5000/weatherforecast](http://localhost:5000/weatherforecast).

```powershell
dotnet run
```

---

With the application running and the weather forecast endpoint loaded, your browser should show the following:

![WeatherForecast endpoint](../images/weatherforecast-endpoint.png)

> [!NOTE]
> If "Enable OpenAPI support" was checked at project creation, the Swagger endpoint is used as the startup page. Replace "swagger/index.html" with "WeatherForecast" to get the response above.

## Discover all the management endpoints

Load the base actuator endpoint by replacing `WeatherForecast` with `actuator` in the browser address bar.

What has happened? In the previous exercise, only select endpoints were implemented and we visited each in the browser to see their output. There was no need to expose those endpoints, because Steeltoe doesn't secure them by default ([you should use security in production environments](/api/v3/management/using-endpoints.md#securing-endpoints)). With the addition of all endpoints, most are secured by default. You pick and choose which should be exposed and with what roles. Here's a list of each endpoint that is available and its purpose. While the application is running, visit each one to learn more.

- `/actuator`: A JSON structured list of all actuator endpoints that have been exposed.
- `/actuator/env`: A listing of all environment variables that are available to the app.
- `/actuator/health`: The current health status of the app, as reported in `IHealthContributor`, customized for different platforms.
- `/actuator/info`: Various app information collected from the `IInfoContributor`.
- `/actuator/loggers`: View and configure the logging levels of your application at runtime. This endpoint supports POST requests to adjust logging levels.
- `/actuator/mappings`: Details about automatically discovered MVC and Web API project routes and route templates.
- `/actuator/metrics`: App CLR and HTTP metrics, collected using OpenTelemetry library.
- `/actuator/prometheus`: A copy of the metrics endpoint, in a Prometheus friendly format.
- `/actuator/refresh`: Trigger the app’s `IConfigurationRoot` to automatically refresh all configuration values.
- `/actuator/httptrace`: Details about the last few request traces made by the app.

## Stop the application

# [Visual Studio](#tab/visual-studio)

Either close the browser window or click the red stop button in the top menu.

# [.NET CLI](#tab/dotnet-cli)

Use the key combination "Ctrl+C" on Windows/Linux or "Cmd+C" on Mac.

---

## Summary

Similar to the previous exercise, there is a minimum expectation of any microservice running in the cloud. These things are meant to help the developer debug, trace, and observe the application within its container. But these things have the potential to consume the developer's time creating everything, adding the right options, and making it distributable. Steeltoe aims to use the best in .NET to get the developer back to coding business logic and not deal with the boilerplate stuff.

| [<< Previous Exercise][exercise-1-link] | [Next Exercise >>][exercise-3-link] |
| :-------------------------------------- | ----------------------------------: |
