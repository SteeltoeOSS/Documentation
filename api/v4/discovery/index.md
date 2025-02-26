# Service Discovery

Service Discovery enables the use of friendly names for the microservices your app depends on.
These microservices typically register themselves at startup to a discovery server, which acts as a central registry.
Each time your app connects to such a microservice, the friendly name is resolved to the actual scheme/host/port by querying the discovery server.

A discovery server can track multiple instances for a single friendly name, which enables your app to load-balance over them.
It also tracks whether your microservice instances are still alive, using health checks and/or keep-alives.

While service discovery enables changing infrastructure without affecting your app, its real power lies in scalability.
Discovery servers can typically be run in a cluster to eliminate the single point of failure.
And because they monitor the liveliness of your microservice instances, you can easily scale them up and down.

Steeltoe facilitates both registration and querying of discovery servers by providing various implementations of `IDiscoveryClient`.
To resolve friendly names, Steeltoe provides implementations of `ILoadBalancer`, which rely on `IDiscoveryClient`.

To use service discovery, you need to:

- Add the appropriate NuGet package references to your project
- Register the desired discovery client(s) in the dependency container
- Configure the chosen discovery client(s) for registration and/or consumption
- Activate the provided `HttpClient`/`HttpClientFactory` facilities to resolve friendly names
