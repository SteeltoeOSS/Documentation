---
uid: guides/observability/grafana
title: Grafana
tags: []
_disableFooter: true
_hideTocVersionToggle: true
---

> [!NOTE]
> This guide applies to Steeltoe v3. Please [open an issue](https://github.com/SteeltoeOSS/Documentation/issues/new/choose) if you'd like to help update the content for Steeltoe v4.

## Using Grafana for app container metrics, distributed tracing, and observability

This tutorial takes you creating a simple Steeltoe app with actuators, logging, and distributed tracing. With that app running you then export the data to an instance of Prometheus and visualize things in a Grafana dashboard.

> [!NOTE]
> For more detailed examples, please refer to the [Management](https://github.com/SteeltoeOSS/Samples/tree/3.x/Management/src) solution in the [Steeltoe Samples Repository](https://github.com/SteeltoeOSS/Samples/tree/3.x).

First, **clone to accompanying repo** that contains all the needed assets

1. ```powershell
   git clone https://github.com/steeltoeoss-incubator/observability.git
   ```

1. ```powershell
   cd observability/grafana
   ```

1. Have a look at what things are provided: `PS C:\tmp\observability\grafana> ls`

   ```bash
   Name                Description
   ----                ----
   dashboard.json      The Grafana dashboard definition
   dashboard.yml       Grafana dashboard provider
   datasource.yml      Grafana datasource definition for prometheus and influxdb
   docker-compose.yml  Docker file to start all containers
   prometheus.yml      Prometheus scrape configs
   telegraf.conf       Telegraf inputs and output configuration
   ```

Next, **create a .NET Core WebAPI** with the correct Steeltoe dependencies

1. Create a new ASP.NET Core WebAPI app with the [Steeltoe Initializr](https://start.steeltoe.io)
1. Name the project "GrafanaObservability"
1. Add the "Actuators" dependency
1. Add the "Dynamic Logging" dependency
1. Add the "Docker" dependency
1. Click **Generate** to download a zip containing the new project
1. Extract the zipped project and open in your IDE of choice
1. Add the other needed actuators in **startup.cs**

   ```csharp
   using Steeltoe.Management.Endpoint.Metrics;
   using Steeltoe.Management.Tracing;
   using Steeltoe.Management.Exporter.Tracing;

   public class Startup {
     public void ConfigureServices(IServiceCollection services) {
       services.AddPrometheusActuator(Configuration);
       services.AddMetricsActuator(Configuration);
       services.AddDistributedTracing(Configuration);
       services.AddZipkinExporter(Configuration);
     }

     public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
       app.UsePrometheusActuator();
       app.UseMetricsActuator();
       app.UseTracingExporter();
     }
   }
   ```

1. Set the actuator path and exposure addresses in **appsettings.json**

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
         "name": "GrafanaObservability"
       }
     },
     "management": {
       "endpoints": {
         "actuator": {
           "exposure": {
             "include": ["*"]
           }
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
             "endpoint": "http://zipkin:9411/api/v2/spans",
             "validateCertificates": false
           }
         }
       }
     }
   }
   ```

1. Adjust **docker-compose.yml** to include the path to the .NET project by replacing `<ABSOLUTE_PATH_TO_PROJECT>` with the absolute path to the folder holding the .csproj file.

Next, **deploy everything** with docker compose

1. Build the image using the provided docker-compose file.

   ```powershell
   PS C:\tmp\observability\grafana> docker-compose up -d
   ```

   > [!NOTE]
   > If you get a permissionerror message about "db.lock" close your IDE. Then run docker-compose command again.

1. Confirm everything started successfully by running `docker-compose ps` and checking the State. Output should look similar to this:

   ```bash
   Name           Command                        State     Ports
   -----------------------------------------------------------------------------------------------------------
   grafana        /run.sh                          Up      0.0.0.0:3000->3000/tcp
   influxdb       /entrypoint.sh influxd           Up      0.0.0.0:8086->8086/tcp
   prometheus     /bin/prometheus --config.f ...   Up      0.0.0.0:9090->9090/tcp
   steeltoe-app   dotnet Grafana_Observabili ...   Up      0.0.0.0:80->80/tcp
   telegraf       /entrypoint.sh --config=/e ...   Up      8092/udp, 8094/tcp, 8125/udp, 0.0.0.0:9273->9273/tcp
   zipkin         /busybox/sh run.sh               Up      0.0.0.0:9411->9411/tcp
   ```

Finally, **navigate to Grafana** to see the default dashboard showing the app's metrics

Grafana is available at [http://localhost:3000](http://localhost:3000)

> [!NOTE]
> The default username is `admin` and the password is `admin`.
