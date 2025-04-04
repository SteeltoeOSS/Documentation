---
uid: guides/cloud-management/distributed-tracing
title: Distributed Tracing
tags: []
_disableFooter: true
_hideTocVersionToggle: true
---

> [!NOTE]
> This guide applies to Steeltoe v3. Please [open an issue](https://github.com/SteeltoeOSS/Documentation/issues/new/choose) if you'd like to help update the content for Steeltoe v4.

## Using Distributed Tracing for debugging with Zipkin

This tutorial takes you through setting up a .NET Core application that sends tracing data to a Zipkin server.

> [!NOTE]
> For more detailed examples, please refer to the [Tracing](https://github.com/SteeltoeOSS/Samples/tree/3.x/Management/src/Tracing) project in the [Steeltoe Samples Repository](https://github.com/SteeltoeOSS/Samples/tree/3.x).

First, **start a Zipkin instance**. Depending on your hosting platform this is done in several ways.

1. Using the [Steeltoe dockerfile](https://github.com/steeltoeoss/dockerfiles), start a local instance of Zipkin

   ```powershell
   docker run --publish 9411:9411 steeltoeoss/zipkin
   ```

1. Once everything is finished initializing, you will see a message confirming startup: `Started ZipkinServer in xx seconds`
1. You can view the Zipkin dashboard by navigating to [http://localhost:9411](http://localhost:9411)

Next, **create a .NET Core WebAPI** that interacts with Distributed Tracing

1. Create a new ASP.NET Core WebAPI app with the [Steeltoe Initializr](https://start.steeltoe.io)
1. Name the project "DistributedTracingExample"
1. No dependency needs to be added
1. Click **Generate Project** to download a zip containing the new project
1. Extract the zipped project and open in your IDE of choice
1. Add `Steeltoe.Management.TracingCore` NuGet package to your project
1. Add Distributed Tracing to your startup services

   ```csharp
   public void ConfigureServices(IServiceCollection services)
   {
      // Other service registrations...

      // Available through Steeltoe.Management.Tracing namespace
      services.AddDistributedTracingAspNetCore();
   }
   ```

**Run** the application

# [.NET cli](#tab/cli)

```powershell
dotnet run<PATH_TO>\DistributedTracingExample.csproj
```

Navigate to the endpoint (you may need to change the port number) [http://localhost:5000/api/values](http://localhost:5000/api/values)

# [Visual Studio](#tab/vs)

1. Choose the top _Debug_ menu, then choose _Start Debugging (F5)_. This should bring up a browser with the app running
1. Navigate to the endpoint (you may need to change the port number) [http://localhost:8080/api/values](http://localhost:8080/api/values)

---

1. Now that you have successfully run a request through the app, navigate back to the zipkin dashboard and click the "Find Traces" button. This will search for recent traces. The result should show the trace for your request.
   ![Zipkin search](/guides/images/zipkin-search.png)
1. Clicking on that trace will drill into the details. Then clicking on a specific action within the trace will give you even more detail.
   ![Zipkin detail](/guides/images/zipkin-detail.png)
