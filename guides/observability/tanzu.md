---
uid: guides/observability/tanzu
title: Tanzu App Manager
tags: []
_disableFooter: true
_hideTocVersionToggle: true
---

## Using Tanzu App Manager for app container metrics, distributed tracing, and observability

This tutorial takes you creating a simple Steeltoe app with actuators, logging, and distributed tracing. With that app running you then export the data to a Tanzu Application Services foundation.

### Prereq's

You'll need access to Tanzu Application Services to complete this guide.

First, **start a Zipkin instance**.

1. Start an instance of Zipkin, named myappmanagerservice

   ```powershell
   cf push myappmanagerservice --docker-image steeltoeoss/zipkin
   ```

1. Once the app is deployed and the Zipkin server is started, the cf cli will print out the public route. An example route would be `mytracingexample.cfapps.io`. You will need this value below.

Next, **create a .NET Core WebAPI** with the correct Steeltoe dependencies.

1. Create a new ASP.NET Core WebAPI app with the [Steeltoe Initializr](https://start.steeltoe.io)
   ![Steeltoe Initialzr](~/guides/images/initializr/actuators.png)
1. Name the project "TAS_Observability"
1. Add the "Actuators" dependency
1. Click **Generate** to download a zip containing the new project
1. Extract the zipped project and open in your IDE of choice
1. Add the other needed actuators in **startup.cs**

   ```csharp
   using Steeltoe.Management.Endpoint.Metrics;
   using Steeltoe.Management.Tracing;
   using Steeltoe.Management.Exporter.Tracing;

   public class Startup {
     public Startup(IConfiguration configuration) {
       Configuration = configuration;
     }

     public IConfiguration Configuration { get; }

     public void ConfigureServices(IServiceCollection services) {
       services.AddPrometheusActuator(Configuration);
       services.AddMetricsActuator(Configuration);
       services.AddDistributedTracing(Configuration);
       services.AddZipkinExporter(Configuration);
       services.AddControllers();
     }

     public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
       if (env.IsDevelopment()) {
         app.UseDeveloperExceptionPage();
       }
       app.UsePrometheusActuator();
       app.UseMetricsActuator();
       app.UseTracingExporter();
       app.UseRouting();
       app.UseEndpoints(endpoints =>
       {
       endpoints.MapControllers();
       });
     }
   }
   ```

1. Set the actuator path, exposure, and zipkin server address in **appsettings.json**

   ```json
   {
     "Logging": {
       "LogLevel": {
         "Default": "Debug",
         "System": "Information",
         "Microsoft": "Information"
       }
     },
     "spring": {
       "application": {
         "name": "TAS_Observability"
       }
     },
     "management": {
       "endpoints": {
         "actuator": {
           "exposure": {
             "include": ["*"]
           }
         },
         "path": "/",
         "cloudfoundry": {
           "validateCertificates": false
         }
       },
       "metrics": {
         "exporter": {
           "cloudfoundry": {
             "validateCertificates": false
           }
         }
       },
       "tracing": {
         "alwaysSample": true,
         "useShortTraceIds ": true,
         "exporter": {
           "zipkin": {
             "endpoint": "http://<ZIPKIN_SERVER_ROUTE>/api/v2/spans",
             "validateCertificates": false
           }
         }
       }
     }
   }
   ```

**Run** the application

1. Add the Cloud Foundry configuration provider in **Program.cs**

   ```csharp
   using Steeltoe.Extensions.Configuration.CloudFoundry;
   ...
   var builder = WebHost.CreateDefaultBuilder(args)
     ...

     .AddCloudFoundry()
     .UseStartup<Startup>();
   ```

1. Publish the application locally using the .NET cli. The following command will create a publish folder automatically.

   ```powershell
   dotnet publish -o .\publish <PATH_TO>\TAS_Observability.csproj
   ```

1. Create **manifest.yml** in the same folder as TAS_Observability.csproj

   ```yaml
   ---
   applications:
     - name: TAS_Observability
       buildpacks:
         - dotnet_core_buildpack    stack: cflinuxfs3
   ```

   > [!TIP]
   > With yaml files indention and line endings matter. Use an IDE like VS Code to confirm spacing and that line endings are set to `LF` (not the Windows default `CR LF`)

1. Push the app to Cloud Foundry

   ```powershell
   cf push -f <PATH_TO>\manifest.yml -p .\publish
   ```

1. Navigate to the application endpoint `https://<APP_ROUTE>/api/values`
1. With the application successfully pushed, navigate to App Manager to see the new features enabled.
   ![Steeltoe Initializr](~/guides/images/actuators-app-manager.png)

1. Now that you have successfully run a request through the app, navigate back to the zipkin dashboard and click the "Find Traces" button. This will search for recent traces. The result should show the trace for your request.
   ![Zipkin](~/guides/images/zipkin-search.png)

1. Clicking on that trace will drill into the details. Then clicking on a specific action within the trace will give you even more detail.
   ![Zipkin](~/guides/images/zipkin-detail.png)
