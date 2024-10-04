# Thread Dump

The Steeltoe Thread Dump endpoint can be used to generate a snapshot of information about all the threads in your application. That snapshot includes several bits of information for each thread, including the thread's state, a stack trace, any monitor locks held by the thread, any monitor locks the thread is waiting on, and other details.

> [!NOTE]
> At this time, thread dumps are only possible on the Windows operating system. When integrating with [Tanzu Apps Manager](https://docs.vmware.com/en/VMware-Tanzu-Application-Service/6.0/tas-for-vms/using-actuators.html#view-thread-dump-3), you will not have the ability to obtain thread dumps from apps running on Linux cells.

## Configure Settings

The following table describes the configuration settings that you can apply to the endpoint.
Each key must be prefixed with `Management:Endpoints:Dump:`.

| Key | Description | Default |
| --- | --- | --- |
| `Enabled` | Whether the endpoint is enabled. | `true` |
| `ID` | The unique ID of the endpoint. | `threaddump` |
| `Path` | The relative path at which the endpoint is exposed. | same as `ID` |
| `RequiredPermissions` | Permissions required to access the endpoint, when running on Cloud Foundry. | `Restricted` |
| `AllowedVerbs` | An array of HTTP verbs the endpoint is exposed at. | `GET` |

## Enable HTTP Access

The URL path to the endpoint is computed by combining the global `Management:Endpoints:Path` setting together with the `Path` setting described in the preceding section.
The default path is `/actuator/threaddump`.

See the [Exposing Endpoints](./using-endpoints.md#exposing-endpoints) and [HTTP Access](./using-endpoints.md#http-access) sections for the overall steps required to enable HTTP access to endpoints in an ASP.NET Core application.

To add the actuator to the service container and map its route, use the `AddThreadDumpActuator` extension method.

Add the following code to `Program.cs` to use the actuator endpoint:

```csharp
using Steeltoe.Management.Endpoint.Actuators.ThreadDump;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddThreadDumpActuator();
```

> [!TIP]
> The extension method takes an optional `MediaTypeVersion` argument, which defaults to `MediaTypeVersion.V2`. If you specify `MediaTypeVersion.V1`, the endpoint path defaults to `/actuator/dump`.

## Sample Output

This endpoint returns a list of threads with their state and stack trace.

The response will always be returned as JSON, like this:

```json
{
  "threads": [
    {
      "blockedCount": 0,
      "blockedTime": -1,
      "lockOwnerId": -1,
      "stackTrace": [
        {
          "className": "[NativeClasses]",
          "methodName": "[NativeMethods]",
          "nativeMethod": true
        },
        {
          "className": "System!System.Threading.WaitHandle",
          "methodName": "WaitOneNoCheck(int32)",
          "nativeMethod": false
        },
        {
          "className": "System!System.Threading.PortableThreadPool+GateThread",
          "methodName": "GateThreadStart()",
          "nativeMethod": false
        }
      ],
      "threadId": 11848,
      "threadName": "Thread-11848",
      "threadState": "WAITING",
      "waitedCount": 0,
      "waitedTime": -1,
      "inNative": true,
      "suspended": false
    }
  ]
}
```
