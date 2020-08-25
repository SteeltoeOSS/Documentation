# Thread Dump

The Steeltoe thread dump endpoint can be used to generate a snapshot of information about all the threads in your application. That snapshot includes several bits of information for each thread, including the thread's state, a stack trace, any monitor locks held by the thread, any monitor locks the thread is waiting on, and other details.

## Configure Settings

The following table describes the settings that you can apply to the endpoint:

| Key | Description | Default |
| --- | --- | --- |
| `Id` | The ID of the thread dump endpoint. | `dump` |
| `Enabled` | Whether to enable the thread dump management endpoint. | `true` |
| `Sensitive` | Currently not used. | `false` |

>NOTE: Each setting must be prefixed with `Management:Endpoints:Dump`.

## Enable HTTP Access

The default path to the thread dump endpoint is computed by combining the global `Path` prefix setting together with the `Id` setting described in the preceding section. The default path is <[Context-Path](./hypermedia#base-context-path)>`/dump`.

See the [HTTP Access](/docs/3/management/using-endpoints#http-access) section to see the overall steps required to enable HTTP access to endpoints in an ASP.NET Core application.

To add the thread dump actuator to the service container, use the `AddThreadDumpActuator()` extension method from `EndpointServiceCollectionExtensions`.

To add the thread dump actuator middleware to the ASP.NET Core pipeline, use the `UseThreadDumpActuator()` extension method from `EndpointApplicationBuilderExtensions`.
