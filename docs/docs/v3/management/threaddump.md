# Thread Dump

The Steeltoe thread dump endpoint can be used to generate a snapshot of information about all the threads in your application. That snapshot includes several bits of information for each thread, including the thread's state, a stack trace, any monitor locks held by the thread, any monitor locks the thread is waiting on, and other details.

>NOTE: At this time, thread dumps are only possible on the Windows operating system. When integrating with the [TAS Apps Manager](https://docs.pivotal.io/pivotalcf/2-0/console/index.html), you will not have the ability to obtain thread dumps from apps running on Linux cells.

## Configure Settings

The following table describes the settings that you can apply to the endpoint:

| Key | Description | Default |
| --- | --- | --- |
| `Id` | The ID of the thread dump endpoint. | `threaddump` |
| `Enabled` | Whether to enable the thread dump management endpoint. | `true` |
| `Sensitive` | Currently not used. | `false` |

>Each setting must be prefixed with `Management:Endpoints:Dump`.

## Enable HTTP Access

The default path to the thread dump endpoint is computed by combining the global `Path` prefix setting together with the `Id` setting described in the preceding section. The default path is `/actuator/threaddump`.

>NOTE: The extension methods take an optional `MediaTypeVersion` argument which defaults to `MediaTypeVersion.V2`. If you chose `MediaTypeVersion.V1` in your extension method, the path defaults to `/actuator/dump`

See the [HTTP Access](./using-endpoints.md#http-access) section to see the overall steps required to enable HTTP access to endpoints in an ASP.NET Core application.

To add the actuator to the service container and map its route, use the `hostBuilder.AddThreadDumpActuator` extension method from `ManagementHostBuilderExtensions`.

Alternatively, first,add the thread dump actuator to the service container, using the `AddThreadDumpActuator()` extension method from `EndpointServiceCollectionExtensions`.

Then add the thread dump actuator middleware to the ASP.NET Core pipeline, using the `Map<ThreadDumpEndpoint_v2>()` extension method from `ActuatorRouteBuilderExtensions`.
