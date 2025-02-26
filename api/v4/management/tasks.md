# Management Tasks

Steeltoe management tasks provide a way to run administrative tasks for ASP.NET Core applications by supporting an alternate entry point, letting you take advantage of the same configuration, logging, and dependency injection as the running version of your application. The original use case for this feature is managing database migrations with a bound database service on Cloud Foundry, but the framework is extensible for you to create your own tasks.

## Add NuGet Reference

To use application tasks, add a reference to the `Steeltoe.Management.Tasks` NuGet package.

## Registering Tasks

For simple cases, a code block containing the task logic can be specified inline:

```csharp
using Steeltoe.Management.Tasks;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddTask("ExampleTaskName", async (serviceProvider, cancellationToken) =>
{
    var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger("ExampleTask");

    await Task.Yield();
    cancellationToken.ThrowIfCancellationRequested();
    logger.LogInformation("Running example task.");
});
```

When the task logic is non-trivial, a class implementing the `IApplicationTask` interface can be defined:

```csharp
using Steeltoe.Common;
using Steeltoe.Management.Tasks;

public class ExampleTask(ILogger<ExampleTask> logger) : IApplicationTask
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();
        logger.LogInformation("Running example task.");
    }
}
```

> [!TIP]
> Steeltoe includes the `MigrateDbContextTask<TDbContext>` task, which runs database migrations with Entity Framework Core.
> It requires a reference to the `Steeltoe.Connectors.EntityFrameworkCore` NuGet package.

To register the `ExampleTask` class defined above as a scoped service, use the `AddTask<>` extension method:

```csharp
using Steeltoe.Management.Tasks;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddTask<ExampleTask>("ExampleTaskName");
```

> [!NOTE]
> To register as a singleton or transient service, use the overload that additionally takes a `ServiceLifetime` parameter.

In case task instantiation requires additional logic, a task instance can be specified as well:

```csharp
using Steeltoe.Management.Tasks;

var builder = WebApplication.CreateBuilder(args);

var exampleTask = new ExampleTask(/* ... */);
builder.Services.AddTask("ExampleTaskName", exampleTask);
```

Or an inline factory method can be used to instantiate the task:

```csharp
using Steeltoe.Management.Tasks;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddTask("ExampleTaskName", (serviceProvider, taskName) =>
{
    var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<ExampleTask>();
    return new ExampleTask(logger); // could pass the task name as a parameter
});
```

## Executing a Task

Once your task has been defined and added to the service container, the last step is to enable a means of executing the task.
In your `Program.cs` file, replace the call to `app.Run()` with `await app.RunWithTasksAsync()`:

```csharp
using Steeltoe.Management.Tasks;

var app = builder.Build();

// ...

await app.RunWithTasksAsync(CancellationToken.None);
```

## Specify Task to Execute

Once all the setup steps have been completed, any invocation of your application with a configuration value for the `RunTask` key
runs that task (and shuts down) instead of starting the web application:

```
dotnet run -- RunTask=ExampleTaskName
```

As a matter of best practice, we encourage you to provide the `RunTask` value only via a command-line parameter.
However, due to the way .NET configuration works, it does not matter which configuration provider is used to provide the task name.
Invoking the command on Cloud Foundry looks similar to this:

```
cf run-task YourAppName "dotnet run -- RunTask=ExampleTaskName" --name ExampleTaskName
```

> [!TIP]
> The command line configuration provider is added by default when using `WebApplication.CreateBuilder(args)`.
> If the task does not fire when running from the command line with the `RunTask=` parameter,
> verify that the configuration provider has been added for your application.
