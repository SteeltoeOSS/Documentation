# Dump

The Steeltoe thread dump endpoint can be used to generate a snapshot of information about all the threads in your application. That snapshot includes several bits of information for each thread, including the thread's state, a stack trace, any monitor locks held by the thread, any monitor locks the thread is waiting on, and other details.

>NOTE: At this time, thread dumps are only possible on the Windows operating system. When integrating with the [TAS Apps Manager](https://docs.pivotal.io/pivotalcf/2-0/console/index.html), you will not have the ability to obtain thread dumps from apps running on Linux cells.

## Configure Settings

The following table describes the settings that you can apply to the endpoint:

|Key|Description|Default|
|---|---|---|
|id|The ID of the thread dump endpoint|`dump`|
|enabled|Whether to enable the thread dump management endpoint|true|
|sensitive|Currently not used|false|

**Note**: **Each setting above must be prefixed with `management:endpoints:dump`**.

## Enable HTTP Access

The default path to the Thread Dump endpoint is computed by combining the global `path` prefix setting together with the `id` setting from above. The default path is  `/actuator/dump`.

The coding steps you take to enable HTTP access to the Thread Dump endpoint differs depending on the type of .NET application your are developing.  The sections which follow describe the steps needed for each of the supported application types.

### ASP.NET Core App

To add the Thread dump actuator to the service container, use the `AddThreadDumpActuator()` extension method from `EndpointServiceCollectionExtensions`.

To add the Thread dump actuator middleware to the ASP.NET Core pipeline, use the `UseThreadDumpActuator()` extension method from `EndpointApplicationBuilderExtensions`.

### ASP.NET 4.x App

To add the Thread Dump actuator endpoint, use the `UseThreadDumpActuator()` method from `ActuatorConfigurator`.

### ASP.NET OWIN App

To add the Thread Dump actuator middleware to the ASP.NET OWIN pipeline, use the `UseThreadDumpActuator()` extension method from `ThreadDumpEndpointAppBuilderExtensions`.

