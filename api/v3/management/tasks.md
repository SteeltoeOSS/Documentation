# Management Tasks

Steeltoe management tasks provide a way to run administrative tasks for ASP.NET Core applications by supporting an alternate entry point, letting you take advantage of the same configuration, logging, and dependency injection as the running version of your application. The original use case for this feature is managing database migrations with a bound database service on Cloud Foundry, but the framework is extensible for you to create your own tasks.

## Add NuGet Reference

This package provides an extension for `Microsoft.AspNetCore.Hosting.IWebHost`.

Add the following PackageReference to your .csproj file.

```xml
<ItemGroup>
...
    <PackageReference Include="Steeltoe.Management.TaskCore" Version="3.2.0"/>
...
</ItemGroup>
```

Alternatively, you can use PowerShell:

```powershell
PM>Install-Package  Steeltoe.Management.TaskCore
```

## Implement Task Interface

Management tasks for use with Steeltoe must implement `Steeltoe.Common.Tasks.IApplicationTask`. Two implementations are currently provided with Steeltoe:

* `Steeltoe.Management.TaskCore.DelegatingTask`: Runs an arbitrary `Action`
* `Steeltoe.Connector.EFCore.MigrateDbContextTask<T>`: Runs `DbContext` migrations with Entity Framework Core

The interface is defined as follows:

```csharp
/// <summary>
/// A runnable task bundled with the assembly that can be executed on-demand
/// </summary>
public interface IApplicationTask
{
    /// <summary>
    /// Gets globally unique name for the task
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Action which to run
    /// </summary>
    void Run();
}
```

## Add Task to ServiceCollection

Several extensions to `IServiceCollection` have been added to provide options for task registration. Add `using Steeltoe.Management.TaskCore` to gain access to the following extension signatures:

* `AddTask<T>(ServiceLifetime lifetime = ServiceLifetime.Singleton)`
* `AddTask(IApplicationTask task)`
* `AddTask(Func<IServiceProvider, IApplicationTask> factory, ServiceLifetime lifetime = ServiceLifetime.Singleton)`
* `AddTask(string name, Action<IServiceProvider> runAction, ServiceLifetime lifetime = ServiceLifetime.Singleton)`

## Apply Extension Method

Once your task has been defined and added to the service container, the last setup task is to enable a means of accessing the task. In your `program.cs` file, replace the call to `<your built IWebHost>.Run()` with `<your built IWebHost>.RunWithTasks()`.

## Invoke Task

Once all the setup steps have been completed, any invocation of your application with a configuration value for the `runtask` key runs that task (and shut down) instead of following the normal web application flow. As a matter of best practice, we encouraged you to provide that value only over command-line parameters. However, due to the way .NET configuration works, it does not matter which configuration provider is used to provide the task name. Invoking the command on Cloud Foundry looks similar to this: `cf run-task actuator "./CloudFoundry runtask=migrate" --name migrate`. Deploy the Steeltoe sample to try it.

>The command line configuration provider is added by default with `IWebHostBuilder.CreateDefaultBuilder`. If the task does not fire when running from a command line with `runtask=<taskname>`, verify that it has been added for your application.
