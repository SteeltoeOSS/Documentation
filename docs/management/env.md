### Env

The Steeltoe `env` endpoint can be used to query and return the configuration values and keys currently in use in your application. The endpoint returns the keys and values from the application's `IConfiguration`.

#### Configure Settings

The following table describes the settings that you can apply to the endpoint:

|Key|Description|Default|
|---|---|---|
|`Id`|The ID of the `env` endpoint|`env`|
|`Enabled`|Whether to enable the `env` management endpoint|`true`|
|`KeysToSanitize`|Keys that should be sanitized. Keys can be simple strings that the property ends with or regex expressions|```["password", "secret", "key", "token", ".*credentials.*", "vcap_services"]```|

>NOTE:Each setting above must be prefixed with `Management:Endpoints:Env`.

#### Enable HTTP Access

The default path to the `env` endpoint is computed by combining the global `Path` prefix setting together with the `Id` setting described in the previous section. The default path is `/env`.

To add the Env actuator to the service container, use the `AddEnvActuator()` extension method from `EndpointServiceCollectionExtensions`.

To add the Env actuator middleware to the ASP.NET Core pipeline, use the `UseEnvActuator()` extension method from `EndpointApplicationBuilderExtensions`.
