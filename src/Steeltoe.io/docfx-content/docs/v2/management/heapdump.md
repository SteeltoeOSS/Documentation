# Heap Dump

The Steeltoe heap dump endpoint can be used to generate and download a mini-dump of your application. The mini-dump can then be read into Visual Studio for analysis.

>NOTE: At this time, dumps are only possible on the Windows operating system. When integrating with the [TAS Apps Manager](https://docs.pivotal.io/pivotalcf/2-0/console/index.html), you will not have the ability to obtain dumps from apps running on Linux cells. For older versions of Apps Manager, there was a bug where the heap dump filename ends with the `.hprof` extension instead of the usual `.dmp` extension. This may cause problems when opening the dump with Visual Studio or some other diagnostic tool. As a workaround, you can rename the file to use the `.dmp` extension. Furthermore, heap dumps from Steeltoe are always gzipped and are expected to be returned with a `.gz` on the end. If the file you receive does not include `.gz`, you should manually change the extension to `.dmp.gz` and unzip the file before attempting to open it, or else you are likely to receive a compatibility error.

## Configure Settings

The following table describes the settings that you can apply to the endpoint:

|Key|Description|Default|
|---|---|---|
|id|The ID of the heap dump endpoint|`heapdump`|
|enabled|Whether to enable the heap dump management endpoint|true|
|sensitive|Currently not used|false|

**Note**: **Each setting above must be prefixed with `management:endpoints:heapdump`**.

## Enable HTTP Access

The default path to the Heap Dump endpoint is computed by combining the global `path` prefix setting together with the `id` setting from above. The default path is `/actuator/heapdump`.

The coding steps you take to enable HTTP access to the Heap Dump endpoint differs depending on the type of .NET application your are developing.  The sections which follow describe the steps needed for each of the supported application types.

### ASP.NET Core App

To add the Heap dump actuator to the service container, use the `AddHeapDumpActuator()` extension method from `EndpointServiceCollectionExtensions`.

To add the Heap dump actuator middleware to the ASP.NET Core pipeline, use the `UseHeapDumpActuator()` extension method from `EndpointApplicationBuilderExtensions`.

### ASP.NET 4.x App

To add the Heap Dump actuator endpoint, use the `UseHeapDumpActuator()` method from `ActuatorConfigurator`.

### ASP.NET OWIN App

To add the Heap Dump actuator middleware to the ASP.NET OWIN pipeline, use the `UseHeapDumpActuator()` extension method from `HeapDumpEndpointAppBuilderExtensions`.
