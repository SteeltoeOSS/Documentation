# Spring Boot Admin Client

Steeltoe spring boot admin client provides a way to integrate with [Spring Boot Admin Server](https://github.com/codecentric/spring-boot-admin). This will enable to monitoring and management of applications in any cloud.

## Add NuGet Reference

Add the following PackageReference to your .csproj file.

```xml
<ItemGroup>
...

    <PackageReference Include="Steeltoe.Management.EndpointCore" Version="3.0.1" />
...
</ItemGroup>
```

Alternatively, you can use PowerShell:

```powershell
PM>Install-Package  Steeltoe.Management.EndpointCore -Version 3.0.1
```

## Register

This extension method hooks into the application lifecycle to register and un-register itself when called follows in your Startup.cs:

```csharp
// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            ...
            app.RegisterWithSpringBootAdmin(Configuration);
        }

```

## Configure Settings

The following table describes the settings that you can apply to the client:

| Key | Description | Default |
| --- | --- | --- |
| `Url` | The Url of the spring boot admin server. | `null` |
| `ApplicationName` | The name of the Steeltoe app being registered. | `IApplicationInstanceInfo.ApplicationName` |
| `BasePath` | BasePath to find endpoints for integration. | `IApplicationInstanceInfo.Uris` |

Here is an example settings file.

```json
"Spring": {
    "Application": {
      "Name": "SteeltoeApp"
    },
    "boot": {
      "admin": {
        "client": {
          "url": "http://localhost:8080",
          "metadata": {
            "user.name": "actuatorUser",
            "user.password": "actuatorPassword"
          }
        }
      }
    }
  }
```

For testing you can use a local version of spring boot admin server running locally.

```bash
docker run -d -p 8080:8080 steeltoeoss/spring-boot-admin
```
