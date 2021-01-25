---
uid: labs/security-providers/sso-oauth
title: Cloud Foundry Single Sign-on with OAuth2
tags: []
_disableFooter: true
---

## Using Cloud Foundry SSO with OAuth2 provider

This is a guide to integrate a .Net Core API with the Cloud Foundry SSO identity provider service. The sample provides authentication to select entry points of an application. It is meant to provide authentication simiar to how IIS would when Windows authentication is enabled.

### Prereq's

You'll need access to Tanzu Application Services to complete this guide.

First, **establish an identity provider**.

1. Start an instance of SSO, named myoAuthSSOExample

    ```powershell
    cf create-service p-identity my-sso-plan myoAuthSSOExample
    ```

1. Wait for service to be ready

    ```powershell
    cf services
    ```

Use the [Pivotal Single Sign-On guide](https://docs.pivotal.io/p-identity) to deploying the SSO tile. Choose from the list of supported identity providers.

Next, **create a .NET Core WebAPI** that interacts with SSO

1. Create a new ASP.NET Core WebAPI app with the [Steeltoe Initializr](https://start.steeltoe.io)
    ![Steeltoe Initialzr](~/labs/images/initializr/no-dependencies.png)
1. Name the project "OAuth_SSO_Example"
1. No dependencies to add
1. Click **Generate** to download a zip containing the new project
1. Extract the zipped project and open in your IDE of choice
1. Open the package manager console
    ![Package manager](~/labs/images/open-package-manager-console.png)
1. Install NuGet distributed packages

    ```powershell
    Install-Package -Id Steeltoe.Security.Authentication.CloudFoundryCore
    ```

Then, **add** Cloud Foundry OAuth, secure endpoints, and run the app

1. Set the cloud foundry auth middleware in **Startup.cs**

    ```csharp
    using Steeltoe.Security.Authentication.CloudFoundry;
    
    public class Startup {    
      public void ConfigureServices(IServiceCollection services){
        services.AddAuthentication(options => {
          options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
          options.DefaultChallengeScheme = CloudFoundryDefaults.AuthenticationScheme;
        })
        .AddCookie((options) =>{
          // set values like login url, access denied path, etc here
          options.AccessDeniedPath = new PathString("/Home/AccessDenied");
        })
        .AddCloudFoundryOAuth(Configuration); // Add Cloud Foundry authentication service
      }
      
      public void Configure(IApplicationBuilder app){
        // Use the protocol from the original request when generating redirect uris
        // (eg: when TLS termination is handled by an appliance in front of the app)
        app.UseForwardedHeaders(new ForwardedHeadersOptions {
        ForwardedHeaders = ForwardedHeaders.XForwardedProto
        });
        
        // Add authentication middleware to pipeline
        app.UseAuthentication();
      }
    }
    ```

1. Open the **Controllers\ValuesControllers.cs** file and secure endpoints

    ```csharp
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase{
      [HttpGet]
      [AllowAnonymous]
      public ActionResult<string> Get(){
        return "Hi There";
      }

      // GET api/values/5
      [HttpGet("{id}")]
      [Authorize]
      public ActionResult<string> Get(int id){
        return "value: " + id.ToString();
      }
      
      // POST api/values
      [HttpPost]
      [Authorize]
      public void Post([FromBody] string value){
      
      }
      
      // PUT api/values/5
      [HttpPut("{id}")]
      [Authorize]
      public void Put(int id, [FromBody] string value){
      
      }
      
      // DELETE api/values/5
      [HttpDelete("{id}")]
      [Authorize]
      public void Delete(int id){
      
      }
    }
    ```

    > [!NOTE]
    > Notice the default GET endpoint with no params is open to anonymous connections but the other endpoints all require authorization. With the combination of SSO functions, the user will be prompted for login and returned.

**Run** the application

1. Open the package manager console
    ![Package Manager](~/labs/images/open-package-manager-console.png)

1. Add the Cloud Foundry package to the project

    ```powershell
    Install-Package Steeltoe.Extensions.Configuration.CloudFoundryCore
    ```

1. Add the Cloud Foundry configuration provider in **Program.cs**

    ```csharp
    using Steeltoe.Extensions.Configuration.CloudFoundry;
    
    var builder = WebHost.CreateDefaultBuilder(args)
      .AddCloudFoundry()
      .UseStartup<Startup>();
    ```

1. Publish the application locally using the .NET cli. The following command will create a publish folder automatically.

    ```powershell
    dotnet publish -o .\publish <PATH_TO>\OAuth_SSO_Example.csproj
    ```

1. Create **manifest.yml** in the same folder as OAuth_SSO_Example.csproj

    ```yaml
    ---
    applications:
    - name: OAuth_SSO_Example
      buildpacks:
        - dotnet_core_buildpack    stack: cflinuxfs3
      services:
      - myoAuthSSOExample
    ```

    > [!TIP]
    >With yaml files indention and line endings matter. Use an IDE like VS Code to confirm spacing and that line endings are set to `LF` (not the Windows default `CR LF`)

1. Push the app to Cloud Foundry

    ```powershell
    cf push -f <PATH_TO>\manifest.yml -p .\publish
    ```

1. Navigate to the application endpoint `https://<APP_ROUTE>/api/values`
1. The endpoint should load with no issue. Then try going to the `/api/values/5` endpoint. This will redirect you to the identity providers login page. Login with valid credentials. You will be redirected back to the app's page where the output of "value: 5" is shown.
