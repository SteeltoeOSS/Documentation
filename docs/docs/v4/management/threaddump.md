# Thread Dump

The Steeltoe Thread Dump endpoint can be used to generate a snapshot of information about all the threads in your application.
Such a snapshot includes several bits of information per thread, including its ID, whether the thread is blocked, and its stack trace.

> [!NOTE]
> The logic used by this endpoint is similar to the [dotnet-stack](https://learn.microsoft.com/dotnet/core/diagnostics/dotnet-stack) tool.

## Configure Settings

The following table describes the configuration settings that you can apply to the endpoint.
Each key must be prefixed with `Management:Endpoints:ThreadDump:`.

| Key | Description | Default |
| --- | --- | --- |
| `Enabled` | Whether the endpoint is enabled | `true` |
| `ID` | The unique ID of the endpoint | `threaddump` |
| `Path` | The relative path at which the endpoint is exposed | same as `ID` |
| `RequiredPermissions` | Permissions required to access the endpoint when running on Cloud Foundry | `Restricted` |
| `AllowedVerbs` | An array of HTTP verbs at which the endpoint is exposed | `GET` |

> [!IMPORTANT]
> In version 4, the configuration key prefix for this endpoint changed from `Management:Endpoints:Dump:` to `Management:Endpoints:ThreadDump:`.

## Enable HTTP Access

The URL path to the endpoint is computed by combining the global `Management:Endpoints:Path` setting with the `Path` setting described in the preceding section.
The default path is `/actuator/threaddump`.

See the [Exposing Endpoints](./using-endpoints.md#exposing-endpoints) and [HTTP Access](./using-endpoints.md#http-access) sections for the steps required to enable HTTP access to endpoints in an ASP.NET Core application.

To add the actuator to the service container and map its route, use the `AddThreadDumpActuator` extension method.

Add the following code to `Program.cs` to use the actuator endpoint:

```csharp
using Steeltoe.Management.Endpoint.Actuators.ThreadDump;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddThreadDumpActuator();
```

> [!TIP]
> It is recommended that you use `AddAllActuators()` instead of adding individual actuators;
> this enables individually turning them on/off at runtime via configuration.

## Sample Output

This endpoint returns a list of threads with their state and stack trace.

The response is always returned as JSON:

```json
{
  "threads": [
    {
      "stackTrace": [
        {
          "className": "[NativeClasses]",
          "methodName": "[NativeMethods]",
          "nativeMethod": true
        },
        {
          "moduleName": "System.Runtime",
          "className": "System.Threading.WaitHandle",
          "methodName": "WaitOneNoCheck(int32)",
          "nativeMethod": false
        },
        {
          "moduleName": "System.Runtime",
          "className": "System.Threading.PortableThreadPool+GateThread",
          "methodName": "GateThreadStart()",
          "nativeMethod": false
        },
        {
          "moduleName": "ExampleAssembly",
          "className": "ExampleNamespace.ExampleClass",
          "methodName": "Run()",
          "fileName": "C:\\Source\\Code\\Example.cs",
          "lineNumber": 10,
          "columnNumber": 16,
          "nativeMethod": false
        }
      ],
      "threadId": 11848,
      "threadName": "Thread-11848",
      "threadState": "WAITING",
      "inNative": true
    }
  ]
}
```
