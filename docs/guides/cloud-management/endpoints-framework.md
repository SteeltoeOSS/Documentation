---
uid: guides/cloud-management/endpoints-framework
title: Management Endpoints (.NET Framework)
tags: []
_disableFooter: true
_hideTocVersionToggle: true
---

> [!NOTE]
> This guide applies to Steeltoe v2. Later Steeltoe versions do not support .NET Framework usage.

> [!TIP]
> Looking for a .NET Core example? [Have a look](endpoints-netcore.md).

## Using Management Endpoints (.NET Framework)

This tutorial takes you through setting up a ASP.NET 4.x Framework application with cloud management endpoints and dynamic logging levels enabled.

> [!NOTE]
> For more detailed examples, please refer to the [Management](https://github.com/SteeltoeOSS/Samples/tree/3.x/Management/src) projects in the [Steeltoe Samples Repository](https://github.com/SteeltoeOSS/Samples/tree/3.x).

**Create a .NET Framework Web API** project

1. In Visual Studio (2019) choose to create a new project
   <img src="/guides/images/new-vs-proj/create-new-project.png" alt="Visual Studio - Create New Project" width="100%">
1. Configure the new project with the follow values
   <img src="/guides/images/new-vs-proj/configure-new-project.png" alt="Visual Studio - Configure New Project" width="100%">
1. **Project Name:** Management_Endpoints_Framework_Example
1. **Solution Name:** Management_Endpoints_Framework_Example
1. **Framework:** (>= 4.5)
1. Choose to create a new Web API project type
   <img src="/guides/images/new-vs-proj/create-new-asp_net-web-app.png" alt="Visual Studio - New Web API" width="100%">
1. Once created, the project should be loaded
   <img src="/guides/images/new-vs-proj/create-successful.png" alt="Visual Studio - Successful project load" width="100%">

Next, **install packages** needed

1. Open the package manager console
   <img src="/guides/images/open-package-manager-console.png" alt="Visual Studio - Package Manager Console" width="100%">
1. Install NuGet distributed packages

   ```powershell
   Install-Package Microsoft.Extensions.Configuration
   Install-Package Microsoft.Extensions.Logging
   Install-Package Microsoft.Extensions.Logging.Console
   Install-Package OpenCensus -IncludePrerelease
   Install-Package Steeltoe.Extensions.Logging.DynamicLogger
   Install-Package Steeltoe.Management.EndpointBase
   Install-Package Steeltoe.Extensions.Configuration.CloudFoundryBase
   Install-Package Steeltoe.Management.EndpointWeb
   ```

Next, **add actuators** support classes

1. Create an **appsettings.json** file in the root of the project

   ```json
   {
     "Logging": {
       "IncludeScopes": true,
       "LogLevel": {
         "Default": "Debug",
         "System": "Information",
         "Microsoft": "Information",
         "Management": "Trace",
         "Steeltoe": "Trace"
       }
     },
     "management": {
       "endpoints": {
         "actuator": {
           "exposure": {
             "include": ["*"]
           }
         }
       }
     }
   }
   ```

> [!NOTE]
> Exposing all endpoints is not an ideal setting in production. This is for example only.

1. Create the **ApplicationConfig.cs** class in the `App_Start` folder

   ```csharp
   using System;
   using System.IO;
   using Microsoft.Extensions.Configuration;
   using Steeltoe.Extensions.Configuration.CloudFoundry;

   public class ApplicationConfig {
     public static CloudFoundryApplicationOptions CloudFoundryApplication {

       get {
         var opts = new CloudFoundryApplicationOptions();
         var appSection = Configuration.GetSection(CloudFoundryApplicationOptions.CONFIGURATION_PREFIX);
         appSection.Bind(opts);
         return opts;
       }
     }
     public static CloudFoundryServicesOptions CloudFoundryServices {
       get {
         var opts = new CloudFoundryServicesOptions();
         var serviceSection = Configuration.GetSection(CloudFoundryServicesOptions.CONFIGURATION_PREFIX);
         serviceSection.Bind(opts);
         return opts;
       }
     }

     public static IConfigurationRoot Configuration { get; set; }

     public static void Configure(string environment) {
       // Set up configuration sources.
       var builder = new ConfigurationBuilder()
       .SetBasePath(GetContentRoot())
       .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
       .AddJsonFile($"appsettings.{environment}.json", optional: true)
       .AddEnvironmentVariables()
       .AddCloudFoundry();

       Configuration = builder.Build();
     }

     public static string GetContentRoot() {
       var basePath = (string)AppDomain.CurrentDomain.GetData("APP_CONTEXT_BASE_DIRECTORY") ??
       AppDomain.CurrentDomain.BaseDirectory;
       return Path.GetFullPath(basePath);
     }
   }
   ```

1. Create the **ManagementConfig.cs** class in the `App_Start` folder

   ```csharp
   using Microsoft.Extensions.Configuration;
   using Microsoft.Extensions.Logging;
   using System.Collections.Generic;
   using System.Web.Http.Description;
   using Steeltoe.Common.Diagnostics;
   using Steeltoe.Common.HealthChecks;
   using Steeltoe.Management.Endpoint;
   using Steeltoe.Management.Endpoint.Health.Contributor;

   public class ManagementConfig{
     public static void ConfigureManagementActuators(IConfiguration configuration, ILoggerProvider dynamicLogger, IApiExplorer apiExplorer, ILoggerFactory loggerFactory = null){
       ActuatorConfigurator.UseCloudFoundryActuators(configuration, dynamicLogger, GetHealthContributors(configuration), apiExplorer, loggerFactory);
     }

     public static void Start(){
       DiagnosticsManager.Instance.Start();
     }

     public static void Stop(){
       DiagnosticsManager.Instance.Stop();
     }

     private static IEnumerable<IHealthContributor> GetHealthContributors(IConfiguration configuration){
       var healthContributors = new List<IHealthContributor>{
         new DiskSpaceContributor()
       };

       return healthContributors;
     }
   }
   ```

1. Update **Web.config** handlers

   ```xml
   <configuration>

     ...

     <system.webServer>
       ...
       <validation validateIntegratedModeConfiguration="false" />
       <handlers>
       <remove name="ExtensionlessUrlHandler-Integrated-4.0" />
       <remove name="OPTIONSVerbHandler" />
       <remove name="TRACEVerbHandler" />
       <add name="ExtensionlessUrlHandler-Integrated-4.0" path="*." verb="*" type="System.Web.Handlers.TransferRequestHandler" preCondition="integratedMode,runtimeVersionv4.0" />
       </handlers>
       ...
     </system.webServer>

     ...

   </configuration>
   ```

1. Create the **LoggingConfig.cs** class in the `App_Start` folder

   ```csharp
   using Microsoft.Extensions.Configuration;
   using Microsoft.Extensions.DependencyInjection;
   using Microsoft.Extensions.Logging;
   using Steeltoe.Extensions.Logging;

   public static class LoggingConfig
   {
       public static ILoggerFactory LoggerFactory { get; set; }
       public static ILoggerProvider LoggerProvider { get; set; }

       public static void Configure(IConfiguration configuration)
       {
           IServiceCollection serviceCollection = new ServiceCollection();
           serviceCollection.AddLogging(builder => {
               builder
                   .SetMinimumLevel(LogLevel.Trace)
                   .AddConfiguration(configuration)
                   .AddDynamicConsole();
           });
           var serviceProvider = serviceCollection.BuildServiceProvider();
           LoggerFactory = serviceProvider.GetService<ILoggerFactory>();
           LoggerProvider = serviceProvider.GetService<ILoggerProvider>();
       }
   }
   ```

1. Modify Application_Start and Application_stop in **Global.asax.cs**

   ```csharp
   using System.Web.Http;
   using System.Web.Mvc;
   using Management_Endpoints_Framework_Example.App_Start;

   protected void Application_Start() {
     // Create applications configuration
     ApplicationConfig.Configure("development");

     // Create logging system using configuration
     LoggingConfig.Configure(ApplicationConfig.Configuration);

     // Add management endpoints to application
     ManagementConfig.ConfigureManagementActuators(
       ApplicationConfig.Configuration,
       LoggingConfig.LoggerProvider,
       GlobalConfiguration.Configuration.Services.GetApiExplorer(),
       LoggingConfig.LoggerFactory);

     // Start the management endpoints
     ManagementConfig.Start();
   }
   protected void Application_Stop(){
     ManagementConfig.Stop();
   }
   ```

**Run** the application

# [Visual Studio](#tab/vs)

1. Choose the top _Debug_ menu, then choose _Start Debugging (F5)_. This should bring up a browser with the app running
1. Navigate to the management endpoints summary page (you may need to change the port number) [http://localhost:8080/actuator](http://localhost:8080/actuator)

---

Once the summary page loads, you will see a list of all available management endpoints that have been automatically created. Click each link to see what information is offered.
