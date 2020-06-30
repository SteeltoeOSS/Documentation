# Hosting Extensions

Many cloud hosting providers, including Pivotal Cloud Foundry, dynamically provide port numbers at runtime. For ASP.NET Core applications, Steeltoe provides extension methods that let both `IWebHostBuilder` and `IHostBuilder` read in these values and configure the application to listen on the assigned port.

## The UseCloudHosting Method

The `UseCloudHosting` method is provided in the `Steeltoe.Common.Hosting` NuGet package. This extension automatically uses the `PORT` or `SERVER_PORT` environment variables (when present) to set the address the application listens on for HTTP traffic. When a port is not found in the environment or passed in as a parameter, the application is configured to listen on port 8080. The following sample illustrates basic usage:

```csharp
    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            ...
            .UseCloudHosting() // Listen for HTTP on port defined in 'PORT', 'SERVER_PORT' or else 8080
            ...
```

The extension includes an optional parameter to explicitly set the ports used for HTTP and HTTPS, which is particularly useful when you run multiple services (which will later be deployed to a cloud platform) at once on your workstation. The following example shows how to set the ports:

```csharp
    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            ...
            .UseCloudHosting(5001, 5002) // Listen for HTTP on port defined in 'PORT', 'SERVER_PORT' or else listen for HTTP on 5001 and HTTPS on 5002
            ...
```

>NOTE: If either environment variable `PORT` or `SERVER_PORT` is found, neither of the optional parameters will be used.

## The UseCloudFoundryHosting Method

The `UseCloudFoundryHosting` method is now deprecated but is still available in the `Steeltoe.Extensions.Configuration.CloudFoundryCore` NuGet package. This extension has been superseded by `UseCloudHosting` but is still available in the 2.x line. This extension automatically uses the `PORT` environment variable (when present) to set the address the application is listening on. The following sample illustrates basic usage:

```csharp
    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            ...
            .UseCloudFoundryHosting()
            ...
```

The extension includes an optional parameter to explicitly set the HTTP port, which is particularly useful when you are running multiple services (which will later be deployed to a cloud platform) at once on your workstation.

```csharp
    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            ...
            .UseCloudFoundryHosting(5001)
            ...
```

>NOTE: As this extension is intended for use on Cloud Foundry, if the 'PORT' environment variable is present, it always overrides the parameter.
