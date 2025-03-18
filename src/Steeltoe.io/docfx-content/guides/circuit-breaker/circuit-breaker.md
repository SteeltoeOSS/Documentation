---
uid: guides/circuit-breakers/circuit-breaker
title: Circuit Breakers w/ Hystrix
tags: []
_disableFooter: true
_hideTocVersionToggle: true
---

> [!NOTE]
> This guide applies to Steeltoe v3. [This component has been removed from v4](https://github.com/SteeltoeOSS/Steeltoe/issues/1244).

## Implementing Circuit Breakers

This tutorial takes you through setting up a .NET Core application that implements a circuit breaker pattern.

> [!NOTE]
> For more detailed examples, please refer to the [FortuneTeller (Circuit Breaker)](https://github.com/SteeltoeOSS/Samples/tree/3.x/CircuitBreaker/src/FortuneTeller) project in the [Steeltoe Samples Repository](https://github.com/SteeltoeOSS/Samples/tree/3.x).

### Start a instance of the Hystrix dashboard

<i>(Depending on your hosting platform this is done in several ways.)</i>

1. There are a few images available on Docker Hub that provide basic Hystrix Dashboard functionality. The following image is provided by the Steeltoe team for testing and development:

```bash
docker run --rm -ti -p 7979:7979 --name steeltoe-hystrix steeltoeoss/hystrix-dashboard
```

Once this image is up and running, you should be able to browse to your [local dashboard](http://localhost:7979/hystrix/) and provide the address of the Hystrix stream(s) you wish to monitor.

> NOTE: This image may be running on a separate network than your application. Remember to provide a stream address that is accessible from within the Docker network as the application will be running on your host. This may require using the external IP address of your workstation or the name of the machine instead of 127.0.0.1 or localhost.

Alternatively, to run a Hystrix Dashboard with Java on your local workstation

1. Install Java 8 JDK.
1. Install Maven 3.x.
1. Clone the Spring Cloud Samples Hystrix dashboard: `cd https://github.com/spring-cloud-samples/hystrix-dashboard`
1. Change to the hystrix dashboard directory: `cd hystix-dashboard`
1. Start the server `mvn spring-boot:run`
1. Open a browser window and connect to the dashboard: <http://localhost:7979/hystrix>

### Create a .NET Core WebAPI that implements circuit breaker pattern

1.  Create a new ASP.NET Core WebAPI app with the [Steeltoe Initializr](https://start.steeltoe.io)
1.  Name the project "CircuitBreakerExample"
1.  Add the "Netflix Hystrix Circuit Breaker" dependency
1.  Click **Generate Project** to download a zip containing the new project
1.  Extract the zipped project and open in your IDE of choice
1.  Add the following to **appsettings.json**

    ```json
    {
      "Spring": {
        "Application": {
          "Name": "mycircuitbreaker"
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

1.  Open up the `.csproj` file that was generated for you and replace the package reference:

    ```xml
    <PackageReference Include="Steeltoe.CircuitBreaker.Hystrix.MetricsStreamCore" Version="$(SteeltoeVersion)" />
    ```
    with

    ```xml
    <PackageReference Include="Steeltoe.CircuitBreaker.Hystrix.MetricsEventsCore" Version="$(SteeltoeVersion)" />
    ```

1.  Replace default "GET" controller method in **WeatherForcastController.cs** (Controllers folder) with the below:

    ```csharp
    [HttpGet]
    public async Task<ActionResult<string>> GetAsync()
    {
        HelloHystrixCommand cb = new HelloHystrixCommand("ThisIsMyBreaker");
        cb.IsFallbackUserDefined = true;
        return await cb.ExecuteAsync();
    }
    ```

### Run the application

# [.NET cli](#tab/cli)

```powershell
dotnet run <PATH_TO>\CircuitBreakerExample.csproj
```

Navigate to the endpoint [http://localhost:5000/WeatherForecast](http://localhost:5000/WeatherForecast)

# [Visual Studio](#tab/vs)

1. Choose the top _Debug_ menu, then choose _Start Debugging (F5)_. This should bring up a browser with the app running
1. Navigate to the endpoint [http://localhost:5000/WeatherForecast](http://localhost:5000/WeatherForecast)

---

1.  Navigate to the dashboard at [http://localhost:7979/hystrix](http://localhost:7979/hystrix) and enter the application stream url in the stream url text box (ex. [http://localhost:5000/hystrix/hystrix.stream](http://localhost:5000/hystrix/hystrix.stream))
    <img src="/guides/images/circuit-breaker-dashboard.png" alt="Circuit Breaker Landing" width="100%">


> NOTE: The stream url `http://localhost:5000/hystrix/hystrix.stream` will only work if the Hystrix dashboard is running on your local host.  You will have to use a different URL, one that is accessible from Docker if you are running the dashboard using Docker.

1.  Refresh the application in your browser a few times and go back to the dashboard to see it logging live activity.
    <img src="/guides/images/circuit-breaker-closed.png" alt="Circuit Breaker Dashboard" width="100%">
