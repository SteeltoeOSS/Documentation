Usage
This package provides an extension for Microsoft.AspNetCore.Hosting.IWebHost. It is not currently compatible with any form of .NET application besides ASP.NET Core.

3.1.1 Add NuGet Reference
Add the following PackageReference to your .csproj file.

```xml
<ItemGroup>
...
    <PackageReference Include="Steeltoe.Management.TaskCore" Version= "2.3.0"/>
...
</ItemGroup>
```

or

```powershell
PM>Install-Package  Steeltoe.Management.TaskCore -Version 2.3.0
```

3.1.2 Implement IApplicationTask
Management tasks for use with Steeltoe must implement Steeltoe.Common.Tasks.IAppliationTask. Two implementations are currently provided with Steeltoe:

* Steeltoe.Management.TaskCore.DelegatingTask - runs an arbitrary Action
* Steeltoe.CloudFoundry.Connector.EFCore.MigrateDbContextTask<T> - runs DbContext migrations with Entity Framework Core

The interface is simple:

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

3.1.3 Add Task to ServiceCollection
Several extensions to IServiceCollection have been added to provide options for task registration. Add using Steeltoe.Management.TaskCore to gain access to the following extension signatures:

* AddTask<T>(ServiceLifetime lifetime = ServiceLifetime.Singleton)
* AddTask(IApplicationTask task)
* AddTask(Func<IServiceProvider, IApplicationTask> factory, ServiceLifetime lifetime = ServiceLifetime.Singleton)
* AddTask(string name, Action<IServiceProvider> runAction, ServiceLifetime lifetime = ServiceLifetime.Singleton)

3.1.4 Apply Extension Method
Once your task has been defined and added to the service container, the last setup task is to enable a means of accessing the task. In your program.cs file, replace the call to <your built IWebHost>.Run() with <your built IWebHost>.RunWithTasks().

3.1.5 Invoke Task
Once all the setup steps have been completed, any invocation of your application with a configuration value for the key runtask will run that task (and shut down) instead of following the normal web application flow. As a matter of best practice, your are encouraged to only provide that value via command-line parameters, but due to the way .NET Configuration works, it does not matter which configuration provider is used to provide the task name. Invoking the command on Cloud Foundry will look similar to this: `cf run-task actuator "./CloudFoundry runtask=migrate" --name migrate`. Deploy the Steeltoe sample to try this out.

>NOTE: The command line configuration provider is added by default with IWebHostBuilder.CreateDefaultBuilder. If the task does not fire when running from a command line with runtask=<taskname> verify that it has been added for your application.