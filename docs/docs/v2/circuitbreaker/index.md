# Circuit Breakers

The Steeltoe Circuit Breaker framework provides applications with an implementation of the Circuit Breaker pattern. Cloud-native architectures  typically consist of multiple layers of distributed services. End-user requests may require multiple calls to these services, and failures in lower-level services can spread to other dependent services and cascade up to the end user. Heavy traffic to a failing service can also make it difficult to repair. By using Circuit Breaker frameworks, you can prevent failures from cascading and provide fallback behavior until a failing service is restored to normal operation.

<!-- Add this image back? ![cb](/images/circuit-breaker-overview.png) -->

When applied to a service, a circuit breaker watches for failing calls to the service. If failures reach a certain threshold, it “opens” (or breaks) the circuit and automatically redirects calls to the specified fallback mechanism. This gives the failing service time to recover.

There are several options to choose from when implementing the Circuit Breaker pattern. Steeltoe has initially chosen to support one based on Hystrix, Netflix's Latency and Fault Tolerance library for distributed systems. For more information about Hystrix, see the [Netflix/Hystrix Wiki](https://github.com/Netflix/Hystrix/wiki) and the [Spring Cloud Netflix](https://projects.spring.io/spring-cloud/) documentation.
