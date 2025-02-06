# Kubernetes

Kubernetes itself is natively capable of service discovery (see [the docs](https://kubernetes.io/docs/concepts/services-networking/service/#discovering-services)). This built-in functionality removes the need for any service registration, and it is still possible to use the discovery client interface to avoid lock-in and/or [customize the load balancing](./load-balancing.md) experience. Steeltoe provides two separate clients, depending on whether you want your application to interact with the management API (and customize load balancing or talk more directly to app instances) or use the built-in option.

## Api-based

The Kubernetes discovery client lets applications query Kubernetes endpoints by name (see [services](https://kubernetes.io/docs/user-guide/services/)). A service is typically exposed by the Kubernetes API server as a collection of endpoints that represent `http` and `https` addresses and that a client can access from any application running as a pod.

### Settings

Additional settings are available to customize the service discovery behavior with Kubernetes. All of these settings should start with `Spring:Cloud:Kubernetes:Discovery:`

| Key | Description | Default |
| --- | --- | --- |
| `Enabled` | Whether or not to enable API interactions | `true` |
| `ServiceName` | The name of the application | `Spring:Application:Name` or executable name |
| `Namespace` | The namespace the application is deployed to | `default` |
| `AllNamespaces` | Whether to discover in all namespaces | `false` |
| `KnownSecurePorts` | List of port numbers that are considered secure and use HTTPS | 443, 8443 |
| `Metadata:AddLabels` | Whether Kubernetes labels of the services will be included | `true` |
| `Metadata:LabelsPrefix` | Prefix for the keys in Metadata hash table | not set |
| `Metadata:AddAnnotations` | Whether the Kubernetes annotations of the services will be included | `true` |
| `Metadata:AnnotationsPrefix` | Prefix to the key names in the metadata hash table | not set |
| `Metadata:AddPorts` | Whether named Kubernetes service ports will be included | `true` |
| `Metadata:PortsPrefix` | Prefix to the keys on metadata entries for ports | `port.` |
| `CacheTTL` | Time in seconds local cache entries are valid | 15 |

>Neither `Spring:Application:Name` nor the `ServiceName` above will affect the name registered for the application within Kubernetes

## Native

Using native Kubernetes service discovery ensures compatibility with additional tooling, such as [Istio](https://istio.io), a service mesh that is capable of load balancing, circuit breaker, failover, and much more.

The calling service then need only refer to names resolvable in a particular Kubernetes cluster. This may be as simple as using the service name as the host name, or it may require a fully qualified domain name (FQDN), such as `{service-name}.{namespace}.svc.{cluster}.local:{service-port}` depending on your environment.

In order to use both the discovery client programming model and the platform's native capabilities, a [NoOpDiscoveryClient](https://github.com/SteeltoeOSS/Steeltoe/blob/release/3.2/src/Discovery/src/ClientBase/SimpleClients/NoOpDiscoveryClient.cs) can be used by either disabling other discovery clients, or not including them in the first place.
