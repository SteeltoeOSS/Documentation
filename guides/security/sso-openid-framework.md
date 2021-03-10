---
uid: guides/security-providers/sso-openid-framework
title: Cloud Foundry Single Sign-on with OpenID Connect (.NET Framework)
tags: []
_disableFooter: true
---

> [!TIP]
> Looking for a .NET Core example? [Have a look](sso-openid-netcore.md).

## Using Cloud Foundry SSO with OpenID Connect provider (.NET Framework)

This is a guide to integrate a .Net framework Web API with the Cloud Foundry SSO identity provider service. The sample provides authentication to select entry points of an application. It is meant to provide authentication similar to how IIS would when Windows authentication is enabled.

### Prereq's

You'll need access to Tanzu Application Services to complete this guide.

First, **establish an identity provider**.

1. Start an instance of SSO, named myOidSSOExample

   ```powershell
   cf create-service p-identity my-sso-plan myOidSSOExample
   ```

1. Wait for service to be ready

   ```powershell
   cf services
   ```

Use the [Pivotal Single Sign-On guide](https://docs.pivotal.io/p-identity) to deploying the SSO tile. Choose from the list of supported identity providers.

**Create a .NET Framework Web API** project

1. In Visual Studio (2019) choose to create a new project
   ![New project](~/guides/images/new-vs-proj/create-new-project.png)
1. Configure the new project with the follow values
   ![Configure project](~/guides/images/new-vs-proj/configure-new-project.png)
1. **Project Name:** OpenID_SSO_Example
1. **Solution Name:** OpenID_SSO_Example
1. **Framework:** (>= 4.5)
1. Choose to create a new Web API project type
   ![New web api](~/guides/images/new-vs-proj/create-new-asp_net-web-app.png)
1. Once created, the project should be loaded
   ![Success](~/guides/images/new-vs-proj/create-successful.png)

Next, **install packages** needed

1. Open the package manager console
   ![Package manager](~/guides/images/open-package-manager-console.png)
1. Install NuGet distributed packages

   ```powershell
   Install-Package -Id Microsoft.Owin.Security.Cookies
   Install-Package -Id Microsoft.Owin.Security.Cookies
   Install-Package -Id Microsoft.Owin.Host.SystemWeb
   Install-Package -Id Microsoft.AspNet.WebHelpers
   Install-Package -Id Microsoft.AspNet.Identity.Owin
   Install-Package -Id Microsoft.Extensions.Logging.Console
   Install-Package -Id Steeltoe.Security.Authentication.CloudFoundryOwin
   ```

Then, create **supporting classes** and **OWIN startup** class

1. Create the **appsettings.json** file in the root of the project

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
     "security": {
       "oauth2": {
         "client": {
           "validateCertificates": false,
           "authDomain": "http://localhost:8080/uaa",
           "clientId": "fortuneservice", //do not change
           "clientSecret": "fortuneservice_secret" //do not change
         }
       }
     }
   }
   ```

   > [!NOTE]
   > "fortuneservice" values have been hard coded in the UAA provider, to keep this a development only thing.

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

1. Create the **LoggingConfig.cs** class in the `App_Start` folder

   ```csharp
   using Microsoft.Extensions.Configuration;
   using Microsoft.Extensions.Logging;
   using Microsoft.Extensions.Logging.Console;
   using Steeltoe.Extensions.Logging;

   public static class LoggingConfig{
     public static ILoggerFactory LoggerFactory { get; set; }
     public static ILoggerProvider LoggerProvider { get; set; }

     public static void Configure(IConfiguration configuration){
       LoggerProvider = new DynamicLoggerProvider(new ConsoleLoggerSettings().FromConfiguration(configuration));
       LoggerFactory = new LoggerFactory();
       LoggerFactory.AddProvider(LoggerProvider);
     }
   }
   ```

1. Create the **AccountController.cs** class in the `Controllers` folder

   ```csharp
   using Steeltoe.Security.Authentication.CloudFoundry;
   using System.Web.Mvc;
   using Microsoft.Owin.Security;
   using System.Web;

   public class AccountController : System.Web.Http.ApiController {

     public AccountController() {}

     [HttpGet]
     [AllowAnonymous]
     public void Get(string ReturnUrl) {
       var properties = new AuthenticationProperties { RedirectUri = ReturnUrl };
       HttpContext.Current.GetOwinContext().Authentication.Challenge(properties, CloudFoundryDefaults.DisplayName);
     }
   }
   ```

1. Turn off integrated authentication in **Web.config**

   ```xml
   <configuration>
     <system.web>
       <authentication mode="None" />
     </system.web>
   </configuration>
   ```

1. Modify Application_Start in **Global.asax.cs**

   ```csharp
   using System.Web.Http;
   using System.Web.Mvc;

   protected void Application_Start(){
     // Create applications configuration
     ApplicationConfig.Configure("development");
     LoggingConfig.Configure(ApplicationConfig.Configuration);
   }
   ```

1. Add a new Owin class

   - In Visual Studio choose Project > Add New Item > Search for "owin" > Choose "OWIN Startup Class"
   - Name the new file "Startup.cs"

1. Update configuration of the new **Startup.cs** OWIN class

   ```csharp
   using System;
   using Microsoft.Owin;
   using Microsoft.AspNet.Identity;
   using Microsoft.Owin.Security.Cookies;
   using Microsoft.Owin.Security;
   using Microsoft.Extensions.Logging;
   using System.Security.Claims;
   using System.Web.Helpers;
   using Owin;
   using Steeltoe.Security.Authentication.CloudFoundry.Owin;
   using Steeltoe.Security.Authentication.CloudFoundry;
   using OpenID_SSO_Example;
   using OpenID_SSO_Example.App_Start;

   [assembly: OwinStartup(typeof(Startup))]

   public class Startup {
     public void Configuration(IAppBuilder app) {
       app.SetDefaultSignInAsAuthenticationType(DefaultAuthenticationTypes.ExternalCookie);

       // Enable the application to use a cookie to store information for the signed in user
       app.UseCookieAuthentication(new CookieAuthenticationOptions {
         AuthenticationType = DefaultAuthenticationTypes.ExternalCookie,
         AuthenticationMode = AuthenticationMode.Passive,
         CookieSecure = CookieSecureOption.Always,
         CookieName = ".AspNet.ExternalCookie",
         LoginPath = new PathString("/api/account"),
         SlidingExpiration = true,
         ExpireTimeSpan = TimeSpan.FromMinutes(5)
       });

       //Cloud Foundry
       app.UseCloudFoundryOpenIdConnect(
         ApplicationConfig.Configuration,
         CloudFoundryDefaults.DisplayName,
         (LoggerFactory)LoggingConfig.LoggerFactory
       );

       AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;
     }
   }
   ```

Then, **mark explicit access** to some of the app's endpoints

1. Open the **Controllers\ValuesControllers.cs** file and secure endpoints

   ```csharp
   using System.Collections.Generic;
   using System.Web.Http;
   using Steeltoe.Extensions.Configuration.CloudFoundry;
   using Microsoft.Extensions.Logging;

   public class ValuesController : ApiController{
     private CloudFoundryApplicationOptions _appOptions;
     private CloudFoundryServicesOptions _serviceOptions;
     private ILogger<ValuesController> _logger;

     public ValuesController(){
       _appOptions = ApplicationConfig.CloudFoundryApplication;
       _serviceOptions = ApplicationConfig.CloudFoundryServices;
       _logger = LoggingConfig.LoggerFactory.CreateLogger<ValuesController>();

       _logger.LogInformation("This is a {LogLevel} log", LogLevel.Information.ToString());
     }

     // GET api/values
     [HttpGet]
     [AllowAnonymous]
     public IEnumerable<string> Get(){
       return new string[] { "Hi There" };
     }

     // GET api/values/5
     [HttpGet]
     [Authorize]
     public string Get(int id){
       return "value: " + id.ToString();
     }

     // POST api/values
     [HttpPost]
     [Authorize]
     public void Post([FromBody]string value){

     }

     // PUT api/values/5
     [HttpPut]
     [Authorize]
     public void Put(int id, [FromBody]string value){

     }

     // DELETE api/values/5
     [HttpDelete]
     [Authorize]
     public void Delete(int id){

     }
   }
   ```

   > [!NOTE]
   > Notice the default GET endpoint with no params is open to anonymous connections but the other endpoints all require authorization. With the combination of SSO functions, the user will be prompted for login and returned.

**Run** the application

1. Create **manifest.yml** in the same folder as OpenID_SSO_Example.csproj

   ```yaml
   ---
   applications:
     - name: OpenID_SSO_Example
       buildpacks:
         - dotnet_core_buildpack    stack: cflinuxfs3
       services:
         - myOidSSOExample
   ```

   > [!TIP]
   > With yaml files indention and line endings matter. Use an IDE like VS Code to confirm spacing and that line endings are set to `LF` (not the Windows default `CR LF`)

1. Push the app to Cloud Foundry

   ```powershell
   cf push -f <PATH_TO>\manifest.yml -p .\publish
   ```

1. Navigate to the application endpoint `https://<APP_ROUTE>/api/values`
1. The app should load with no issue. Try going to the `/api/values/5` endpoint. This will redirect you to the identity providers login page. Login with valid credentials. You will be redirected back to the app's page where the output of "value: 5" is shown.
