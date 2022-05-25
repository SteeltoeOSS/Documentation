# Hosting Extensions

Many cloud hosting providers, including Cloud Foundry, dynamically provide port numbers at runtime. For ASP.NET Core applications, Steeltoe provides a extension methods for both `IWebHostBuilder` and `IHostBuilder` to read in these values and configure the application to lisen on the assigned port.

## UseCloudHosting

 `UseCloudHosting` is provided in the NuGet package `Steeltoe.Common.Hosting`. This extension will automatically use the environment variables `PORT` or `SERVER_PORT` (when present) to set the address the application is listening on for HTTP traffic. When a port is not found in the environment or passed in as a parameter, the application will be configured to listen on port 8080. This sample illustrates basic usage:

```csharp
    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            ...
            .UseCloudHosting() // Listen for HTTP on port defined in 'PORT', 'SERVER_PORT' or else 8080
            ...
```

The extension includes an optional parameter to explicitly set ports used for HTTP and HTTPS, which is particularly useful when you are running multiple services at once on your workstation that will later be deployed to a cloud platform.

```csharp
    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            ...
            .UseCloudHosting(5001, 5002) // Listen for HTTP on port defined in 'PORT', 'SERVER_PORT' or else listen for HTTP on 5001 and HTTPS on 5002
            ...
```

>NOTE: If either environment variable `PORT` or `SERVER_PORT` is found, neither of the optional parameters will be used.

## UseCloudFoundryHosting

 `UseCloudFoundryHosting` is now deprecated, but is still available in the NuGet package `Steeltoe.Extensions.Configuration.CloudFoundryCore`. This extension has been superseded by `UseCloudHosting`, but is still available in the 2.x line. This extension will automatically use the environment variable `PORT` (when present) to set the address the application is listening on. This sample illustrates basic usage:

```csharp
    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            ...
            .UseCloudFoundryHosting()
            ...
```

The extension includes an optional parameter to explicitly set the HTTP port, which is particularly useful when you are running multiple services at once on your workstation that will later be deployed to a cloud platform.

```csharp
    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            ...
            .UseCloudFoundryHosting(5001)
            ...
```

>NOTE: As this extension is intended for use on Cloud Foundry, if the 'PORT' environment variable is present, it will always override the parameter.
