### Heap Dump

You can use the Steeltoe heap dump endpoint to generate and download a mini-dump of your application. The mini-dump can then be read into Visual Studio for analysis.

>NOTE: At this time, dumps are possible only on the Windows operating system. When integrating with the [Pivotal Apps Manager](https://docs.pivotal.io/pivotalcf/2-0/console/index.html), you do not have the ability to obtain dumps from applications running on Linux cells. Also, the heap dump filename used by the Pivotal Apps Manager ends with the `.hprof` extension instead of the usual `.dmp` extension. This may cause problems when opening the dump with Visual Studio or some other diagnostic tool. As a workaround, you can rename the file to use the `.dmp` extension.

#### Configure Settings

The following table describes the settings that you can apply to the endpoint:

|Key|Description|Default|
|---|---|---|
|`id`|The ID of the heap dump endpoint|`heapdump`|
|`enabled`|Whether to enable the heap dump management endpoint|`true`|
|`sensitive`|Currently not used|`false`|

>NOTE: Each setting above must be prefixed with `management:endpoints:heapdump`.

#### Enable HTTP Access

The default path to the heap dump endpoint is computed by combining the global `path` prefix setting together with the `id` setting described in the previous section. The default path is `/heapdump`.

The coding steps you take to enable HTTP access to the heap dump endpoint differ, depending on the type of .NET application your are developing.  The sections that follow describe the steps needed for each of the supported application types.

##### ASP.NET Core App

To add the heap dump actuator to the service container, use the `AddHeapDumpActuator()` extension method from `EndpointServiceCollectionExtensions`.

To add the heap dump actuator middleware to the ASP.NET Core pipeline, use the `UseHeapDumpActuator()` extension method from `EndpointApplicationBuilderExtensions`.

##### ASP.NET 4.x App

To add the heap dump actuator endpoint, use the `UseHeapDumpActuator()` method from `ActuatorConfigurator`.

##### ASP.NET OWIN App

To add the heap dump actuator middleware to the ASP.NET OWIN pipeline, use the `UseHeapDumpActuator()` extension method from `HeapDumpEndpointAppBuilderExtensions`.
