# Hosting Extensions

Many cloud hosting providers, including TAS, dynamically provide port numbers at runtime. For ASP.NET Core applications, Steeltoe provides extension methods that let both `IWebHostBuilder` and `IHostBuilder` read in these values and configure the application to listen on the assigned port.

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

>If either environment variable `PORT` or `SERVER_PORT` is found, neither of the optional parameters will be used.
