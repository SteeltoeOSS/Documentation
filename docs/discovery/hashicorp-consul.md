# HashiCorp Consul

The Consul client implementation lets applications register services with a Consul server and discover services registered by other applications. This Steeltoe client uses a Consul .NET package provided by either the (now archived) [Playfab](https://github.com/PlayFab/consuldotnet) or [G-Research](https://github.com/g-research/consuldotnet) `consuldotnet` open source project.

## Usage

You should know how the .NET [Configuration service](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration) works before starting to use the client. You need basic understanding of the `ConfigurationBuilder` and how to add providers to the builder to configure the client.

You should also know how the ASP.NET Core [Startup](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/startup) class is used in configuring the application services and how the middleware used in the application. Pay particular attention to the usage of the `Configure()` and `ConfigureServices()` methods.

It might help to have an understanding of the [Spring Cloud Consul](https://spring.io/projects/spring-cloud-consul) project, as we have based our work on this project.

To use the Steeltoe discovery client, you need to:

* Add the appropriate NuGet package reference to your project.
* Configure the settings the discovery client uses to register services in the service registry.
* Configure the settings the discovery client uses to discover services in the service registry.
* Add and use the discovery client service in the application.
* Use an injected `IDiscoveryClient` to lookup services.

### Consul Settings

To get the Steeltoe discovery client to properly communicate with the Consul server, you need to provide a few configuration settings to the client. There are two sections you may need to configure.

The first section pertains to configuring the information needed to connect to the Consul server. All of these settings should start with `Consul:`

| Key | Description | Default |
| --- | --- | --- |
| `Host` | Address of the Consul server. | `localhost` |
| `Port` | Port number the Consul server is listening on. | 8500 |
| `Scheme` | Scheme to use with the Consul server (http or https). | `http` |
| `Datacenter` | The datacenter name passed in each request to the server. | none |
| `Token` | The auth token passed in each request to the server. | `true` |
| `WaitTime` | The time a Watch request blocks or waits. | none |
| `Username` | Username for HTTP authentication. | none |
| `Password` | Password for HTTP authentication. | none |

The second set of settings you may need to specify pertain to service registration and service discovery. All of these settings should start with `Consul:Discovery`

| Key | Description | Default |
| --- | --- | --- |
| `Enabled` | Enable to disable the Consul client. | `true` |
| `Register` | Whether to register as a service. | `true` |
| `Deregister` | Whether to de-register on shutdown. | `true` |
| `ServiceName` | The service name to register. | computed |
| `Scheme` | Scheme to register for service. | `http` |
| `Hostname` | Hostname to use when registering server. | computed |
| `IpAddress` | IP address to register. | computed |
| `Port` | Port number to register. | none |
| `PreferIpAddress` | Register IP address instead of hostname. | `false` |
| `InstanceId` | The instance id registered for service. | computed |
| `Tags` | The list of tags used when registering a service. | none |
| `DefaultQueryTag` | Tag to query for in service list if one is not listed in serverListQueryTags. | none |
| `QueryPassing` | Enable or disable whether to add the 'passing' parameter to health requests. This pushes health check passing to the server. | `false` |
| `RegisterHealthCheck` | Enable or disable health check registration. | `true` |
| `HealthCheckUrl` | The health check URL override. | none |
| `HealthCheckPath` | Alternate server health check path. | `/actuator/health` |
| `HealthCheckInterval` | How often to perform the health check. | 10s |
| `HealthCheckTimeout` | Timeout for health check. | 10s |
| `HealthCheckCriticalTimeout` | Timeout to de-register services critical for longer than this value. | 30m |
| `HealthCheckTlsSkipVerify` |Skip health check TLS verification. | `false` |
| `InstanceZone` | Instance zone to use during registration. | none |
| `InstanceGroup` | Instance group to use during registration. | none |
| `DefaultZoneMetadataName` | Metadata tag name of the zone. | `zone` |
| `FailFast` | Throw exception if registration fails. | `true` |
| `Retry:Enabled` | Enable or disable retry logic. | `false` |
| `Retry:InitialInterval` | Starting interval. | 1000ms |
| `Retry:MaxAttempts` | Max retries if retry enabled. |6|
| `Retry:MaxInterval` | Maximum retry interval. | 2000ms |
| `Retry:Multiplier` | Retry interval multiplier. | 1.1 |
| `Heartbeat:Enabled` | Enable or disable heartbeat logic. | `true` |
| `Heartbeat:TtlValue` | Time to live heartbeat time. | 30 |
| `Heartbeat:TtlUnit` | Time to live heartbeat unit. | s |
| `Heartbeat:IntervalRation` | The interval ration. | 2.0/3.0 |

### Enable Logging

Sometimes, you many want to turn on debug logging. To do so, add the following to your `appsettings.json`:

```json
{
  "Logging": {
    "IncludeScopes": false,
    "LogLevel": {
      "Default": "Information",
      "Steeltoe":  "Debug"
    }
  }
}
```

### Health Contributor

The Consul package provides a Steeltoe Management Health contributor (`ConsulHealthContributor`) that you can use monitor Consul server health.

If you use the `AddDiscoveryClient()` extension method and you have configured Consul as your service discovery choice, this contributor is automatically added to the container and is automatically used.

### Configuring Health Check

The health check for a Consul service instance defaults to `/actuator/health`, which is a good default when you have enabled the Steeltoe management features in your application. You can change this path and provide your own implementation by using the `Consul:Discovery:HealthCheckPath` setting. You can also configure the interval that Consul uses to check the health endpoint. You can change this setting by using the `Consul:Discovery:HealthCheckInterval`. You should use settings such as "10s" and "1m" to represent 10 seconds and 1 minute, respectively.

### Configuring Metadata

Steeltoe `IServiceInstance` has an `IDictionary<string, string> Metadata` property that is used to obtain metadata settings for an instance. Consul previously did not support including metadata with service instance registrations. The Steeltoe Consul client currently uses the Consul tags feature to approximate metadata registration.

Tags with the form of `key=value` are split and used as `IDictionary` keys and values, respectively. Tags without the equal sign are used as both the key and the value. You can add metadata with the `consul:discovery:tags` string array:

```json
{
  "Consul": {
    "Discovery": {
      "Tags": [
        "somekey=somevalue",
        "someothervalue"
      ]
    }
  }
}
```

The preceding tag list results in metadata that looks like this:

```json
{
  "somekey": "somevalue",
  "someothervalue": "someothervalue"
}
```

### Configuring InstanceId

By default, if no other values are configured, a Consul service instance is registered with an ID that is equal to the application name concatenated with a random value.

You can change that by configuring the `Spring:Application:InstanceId` or `vcap:application:instance_id` setting to some value. Then the ID equals to the application name concatenated with that value. Note that, on Cloud Foundry, `vcap:application:instance_id` is automatically set for you if you use the Steeltoe Cloud Foundry configuration provider.

You can completely override all of the above by setting `Consul:Discovery:InstanceId` to some other value.
