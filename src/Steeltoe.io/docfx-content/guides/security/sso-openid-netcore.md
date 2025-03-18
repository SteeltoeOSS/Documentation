---
uid: guides/security-providers/sso-openid-netcore
title: Cloud Foundry Single Sign-on with OpenID Connect
tags: []
_disableFooter: true
_hideTocVersionToggle: true
---

> [!NOTE]
> This guide applies to Steeltoe v3. Please [open an issue](https://github.com/SteeltoeOSS/Documentation/issues/new/choose) if you'd like to help update the content for Steeltoe v4.

> [!TIP]
> Looking for a .NET Framework example? Have a [look](./sso-openid-framework.md).

## Using Cloud Foundry SSO with OpenID Connect provider

This is a guide to integrate a .Net Core API with the Cloud Foundry SSO identity provider service. The sample provides authentication to select entry points of an application. It is meant to provide authentication simiar to how IIS would when Windows authentication is enabled.

> [!NOTE]
> For more detailed examples, please refer to the [Security](https://github.com/SteeltoeOSS/Samples/tree/3.x/Security) section in the [Steeltoe Samples Repository](https://github.com/SteeltoeOSS/Samples/tree/3.x).

First, **establish an identity provider**. Using the [Steeltoe dockerfile](https://github.com/steeltoeoss/dockerfiles), start a local instance of SSO.

```powershell
docker run --rm -ti -p 8080:8080 --name steeltoe-uaa steeltoeoss/workshop-uaa-server
```

> [!NOTE]
> This is a developer image of the Cloud Foundry User Account and Authentication (UAA) OAuth identity provider. It is meant for development purposes only.

Next, **create a .NET Core WebAPI** that interacts with SSO

1. Create a new ASP.NET Core WebAPI app with the [Steeltoe Initializr](https://start.steeltoe.io)
1. Name the project "OAuthSSOExample"
1. Add the "OAuth Connector" dependency
1. Click **Generate** to download a zip containing the new project
1. Extract the zipped project and open in your IDE of choice
1. Open the package manager console
   <img src="/guides/images/open-package-manager-console.png" alt="Visual Studio - Package Manager Console" width="100%">
1. Install NuGet packages

   ```powershell
   Install-Package Steeltoe.Security.Authentication.CloudFoundryCore
   ```

1. Set the instance address in **appsettings.json**

   ```json
   {
     "security": {
       "oauth2": {
         "client": {
           "validateCertificates": false,
           "authDomain": "http://localhost:8080/uaa",
           "clientId": "fortuneservice",
           "clientSecret": "fortuneservice_secret"
         }
       }
     }
   }
   ```

Then, **add** Cloud Foundry OpenID Connect, secure endpoints, and run the app

1. Set the cloud foundry auth middleware in **Startup.cs**

   ```csharp
   using Steeltoe.Security.Authentication.CloudFoundry;

   public class Startup {
     public IConfiguration Configuration { get; private set; }
     public Startup(IConfiguration configuration){
       Configuration = configuration;
     }
     public void ConfigureServices(IServiceCollection services){
       services.AddAuthentication(options => {
         options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
         options.DefaultChallengeScheme = CloudFoundryDefaults.AuthenticationScheme;
       })
       .AddCookie((options) =>{
         // set values like login url, access denied path, etc here
         options.AccessDeniedPath = new PathString("/Home/AccessDenied");
       })
       .AddCloudFoundryOpenId(Configuration); // Add Cloud Foundry authentication service
     }
     public void Configure(IApplicationBuilder app){
       // Use the protocol from the original request when generating redirect uris
       // (eg: when TLS termination is handled by an appliance in front of the app)
       app.UseForwardedHeaders(new ForwardedHeadersOptions {
         ForwardedHeaders = ForwardedHeaders.XForwardedProto;
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

# [.NET cli](#tab/cli)

```powershell
dotnet run<PATH_TO>\OAuthSSOExample.csproj
```

Navigate to the endpoint (you may need to change the port number) [http://localhost:5000/api/values](http://localhost:5000/api/values)

# [Visual Studio](#tab/vs)

1. Choose the top _Debug_ menu, then choose _Start Debugging (F5)_. This should bring up a browser with the app running
1. Navigate to the endpoint (you may need to change the port number) [http://localhost:8080/api/values](http://localhost:8080/api/values)

---

Once the app loads in the browser go to the `/api/values` endpoint. It should load with no issue. Then try going to the `/api/values/5` endpoint. This will redirect you to the UAA login page. Login with the user **fortuneTeller** and password **password**. You will be redirected back to the app's page where the output of "value: 5" is shown.
