# Env

The Steeltoe Env endpoint can be used to query and return the configuration values and keys currently in use in your application. The endpoint returns the keys and values from the applications `IConfiguration`.

## Configure Settings

The following table describes the settings that you can apply to the endpoint:

|Key|Description|Default|
|---|---|---|
|id|The ID of the env endpoint|`env`|
|enabled|Whether to enable the env management endpoint|true|
|keysToSanitize|Keys that should be sanitized. Keys can be simple strings that the property ends with or regex expressions|```["password", "secret", "key", "token", ".*credentials.*", "vcap_services"]```|

**Note**: **Each setting above must be prefixed with `management:endpoints:env`**.

## Enable HTTP Access

The default path to the Env endpoint is computed by combining the global `path` prefix setting together with the `id` setting from above. The default path is `/actuator/env`.

The coding steps you take to enable HTTP access to the Env endpoint differs depending on the type of .NET application your are developing.  The sections which follow describe the steps needed for each of the supported application types.

### ASP.NET Core App

To add the Env actuator to the service container, use the `AddEnvActuator()` extension method from `EndpointServiceCollectionExtensions`.

To add the Env actuator middleware to the ASP.NET Core pipeline, use the `UseEnvActuator()` extension method from `EndpointApplicationBuilderExtensions`.

### ASP.NET 4.x App

To add the Env actuator endpoint, use the `UseEnvActuator()` method from `ActuatorConfigurator`.

### ASP.NET OWIN App

To add the Env actuator middleware to the ASP.NET OWIN pipeline, use the `UseEnvActuator()` extension method from `EnvEndpointAppBuilderExtensions`.

