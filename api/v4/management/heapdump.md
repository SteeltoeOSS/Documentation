# Heap Dump

You can use the Steeltoe heap dump endpoint to generate and download a mini-dump of your application. The mini-dump can then be read into Visual Studio for analysis.

>NOTE: Due to Cloud Foundry [container hardening rules](https://docs.cloudfoundry.org/concepts/container-security.html#hardening), heap dumps did not work on Linux containers because the containers depend on `CAP_SYS_PTRACE` which is disabled by the container hardening rules.  This problem has been resolved in TAS 2.11.2+ versions.

## Configure Settings

The following table describes the settings that you can apply to the endpoint:

| Key | Description | Default |
| --- | --- | --- |
| `Id` | The ID of the heap dump endpoint. | `heapdump` |
| `Enabled` | Whether to enable the heap dump management endpoint. | `true` |
| `Sensitive` | Currently not used. | `false` |
| `HeapDumpType` | Sets the type of heap dump used. | `Full` |

>NOTE: Each setting above must be prefixed with `Management:Endpoints:Heapdump`.

**HeapDumpType**: Introduced in Steeltoe 3.1.0, this setting is supported for .NET Core 3.1 and .NET 5.0 on Linux and .NET 5 on Windows. Acceptable values are `Normal`, `WithHeap`, `Triage` and `Full`.

## Enable HTTP Access

The default path to the heap dump endpoint is computed by combining the global `Path` prefix setting together with the `Id` setting described in the previous section. The default path is `/actuator/heapdump`.

See the [HTTP Access](./using-endpoints.md#http-access) section to see the overall steps required to enable HTTP access to endpoints in an ASP.NET Core application.

To add the actuator to the service container and map its route, you can use the `AddHeapDumpActuator` extension method from `ManagementHostBuilderExtensions`.

Alternatively, first,  add the heap dump actuator to the service container, use the `AddHeapDumpActuator()` extension method from `EndpointServiceCollectionExtensions`.

Then, add the heap dump actuator middleware to the ASP.NET Core pipeline, using the `Map<HeapDumpEndpoint>()` extension method from `ActuatorRouteBuilderExtensions`.

## Status on Cloud Foundry

As noted above, this functionality does not work in all scenarios. As such, beginning with version 3.1.0, the Heap Dump endpoint is disabled by default when deployed to a Cloud Foundry-based Linux environment. If your Cloud Foundry environment is using a version of of Garden-runC that is greater than or equal to 1.19.26 (for example, Tanzu Application Service 2.11.2 or higher), you can re-enable the heap dump endpoint by setting `Management:Endpoints:HeapDump:Enabled` to `true` in an app settings file or environment variable.

In addition to the permissions issue that has blocked heap dumps from working on Linux, there is an outstanding bug with TAS Apps Manager that changes the file extension when a heap dump is downloaded. Heap dumps generated by Steeltoe (regardless of platform) are gzipped `.dmp` files, and are expected to have the extension `.dmp.gz`. If you request a heap dump, and the file you receive uses the extension `.dmp`, you should manually change the extension to `.dmp.gz` and unzip the file before attempting to open it, or else you are likely to receive a compatibility error.