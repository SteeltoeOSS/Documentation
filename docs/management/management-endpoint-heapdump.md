### Heap Dump

The Steeltoe Heap dump endpoint can be used to generate and download a mini-dump of your application. The mini-dump can then be read into Visual Studio for analysis.

>NOTE: At this time, dumps are only possible on the Windows operating system. When integrating with the [Pivotal Apps Manager](https://docs.pivotal.io/pivotalcf/2-0/console/index.html), you will not have the ability to obtain dumps from apps running on Linux cells. Also, the heap dump filename used by the Pivotal Apps Manager ends with the `.hprof` extension instead of the usual `.dmp` extension. This may cause problems when opening the dump with Visual Studio or some other diagnostic tool. As a workaround, you can rename the file to use the `.dmp` extension.

#### Configure Settings

The following table describes the settings that you can apply to the endpoint:

|Key|Description|Default|
|---|---|---|
|id|The ID of the heap dump endpoint|`heapdump`|
|enabled|Whether to enable the heap dump management endpoint|true|
|sensitive|Currently not used|false|

**Note**: **Each setting above must be prefixed with `management:endpoints:heapdump`**.

#### Enable HTTP Access

The default path to the Heap Dump endpoint is computed by combining the global `path` prefix setting together with the `id` setting from above. The default path is `/heapdump`.

The coding steps you take to enable HTTP access to the Heap Dump endpoint differs depending on the type of .NET application your are developing.  The sections which follow describe the steps needed for each of the supported application types.

##### ASP.NET Core App

Refer to the [HTTP Access ASP.NET Core](#http-access-asp-net-core) section below to see the overall steps required to enable HTTP access to endpoints in an ASP.NET Core application.

To add the Heap dump actuator to the service container, use the `AddHeapDumpActuator()` extension method from [EndpointServiceCollectionExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointCore/HeapDump/EndpointServiceCollectionExtensions.cs).

To add the Heap dump actuator middleware to the ASP.NET Core pipeline, use the `UseHeapDumpActuator()` extension method from [EndpointApplicationBuilderExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointCore/HeapDump/EndpointApplicationBuilderExtensions.cs).

##### ASP.NET 4.x App

Refer to the [HTTP Access ASP.NET 4.x](#http-access-asp-net-4-x) section below to see the overall steps required to enable HTTP access to endpoints in a 4.x application.

To add the Heap Dump actuator endpoint, use the `UseHeapDumpActuator()` method from [ActuatorConfigurator](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointWeb/ActuatorConfigurator.cs).

##### ASP.NET OWIN App

Refer to the [HTTP Access ASP.NET OWIN](#http-access-asp-net-owin) section below to see the overall steps required to enable HTTP access to endpoints in an ASP.NET 4.x OWIN application.

To add the Heap Dump actuator middleware to the ASP.NET OWIN pipeline, use the `UseHeapDumpActuator()` extension method from [HeapDumpEndpointAppBuilderExtensions](https://github.com/SteeltoeOSS/Management/blob/master/src/Steeltoe.Management.EndpointOwin/HeapDump/HeapDumpEndpointAppBuilderExtensions.cs).
