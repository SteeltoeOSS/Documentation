### Thread Dump

The Steeltoe thread dump endpoint can be used to generate a snapshot of information about all the threads in your application. That snapshot includes several bits of information for each thread, including the thread's state, a stack trace, any monitor locks held by the thread, any monitor locks the thread is waiting on, and other details.

>NOTE: At this time, thread dumps are possible only on the Windows operating system. When integrating with the [Pivotal Apps Manager](https://docs.pivotal.io/pivotalcf/2-0/console/index.html), you do not have the ability to obtain thread dumps from applications running on Linux cells.

#### Configure Settings

The following table describes the settings that you can apply to the endpoint:

|Key|Description|Default|
|---|---|---|
|`Id`|The ID of the thread dump endpoint|`dump`|
|`Enabled`|Whether to enable the thread dump management endpoint|`true`|
|`Sensitive`|Currently not used|`false`|

>NOTE: Each setting must be prefixed with `Management:Endpoints:Dump`.

#### Enable HTTP Access

The default path to the thread dump endpoint is computed by combining the global `Path` prefix setting together with the `Id` setting described in the preceding section. The default path is <[Context-Path](hypermedia#base-context-path)>`/dump`.

To add the thread dump actuator to the service container, use the `AddThreadDumpActuator()` extension method from `EndpointServiceCollectionExtensions`.

To add the thread dump actuator middleware to the ASP.NET Core pipeline, use the `UseThreadDumpActuator()` extension method from `EndpointApplicationBuilderExtensions`.
