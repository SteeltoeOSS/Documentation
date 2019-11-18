# Hosting Extensions

Many cloud hosting providers, including Pivotal Cloud Foundry, dynamically provide port numbers at runtime. For ASP.NET Core applications, Steeltoe provides an extension method for both `IWebHostBuilder` and `IHostBuilder` named `UseCloudFoundryHosting` that will automatically use the environment variable `PORT` (when present) to set the address the application is listening on. This sample illustrates basic usage:

```csharp
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                ...
                .UseCloudFoundryHosting()
                ...
```

The extension includes an optional parameter to explicitly set the port, which is particularly useful when you are running multiple services at once on your workstation that will later be deployed to a cloud platform.

```csharp
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                ...
                .UseCloudFoundryHosting(5001)
                ...
```

>NOTE: As this extension is intended for use on Cloud Foundry, if the 'PORT' environment variable is present, it will always override the parameter.
