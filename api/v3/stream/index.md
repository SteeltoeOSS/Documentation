# Introducing Steeltoe Stream

Steeltoe Stream is a framework for building message-driven microservice applications. It builds upon other Steeltoe components to enable developers to create standalone, production-grade .NET applications providing connectivity to messaging systems.
It provides an opinionated configuration of underlying middleware introducing the concepts of persistent publish-subscribe semantics, consumer groups, and partitions.

With Steeltoe Stream, developers can:

* Build, test, iterate, and deploy data-centric applications in isolation.
* Apply modern microservices architecture patterns, including composition through messaging.
* Decouple application responsibilities with event-centric thinking. An event can represent something that has happened in time, to which the downstream consumer applications can react without knowing where it originated or the producer's identity.
* Port business logic onto messaging systems (such as RabbitMQ, etc).
* Rely on the framework's automatic content-type support for common use-cases and yet is extendable to different data conversion types as needed.
* Develop Stream components in .NET which can be deployed on [Spring Cloud Data Flow](https://spring.io/projects/spring-cloud-dataflow#overview).

For more reading about Steeltoe Stream:

* Quick Start [Guide](../../../guides/stream/quick-start.md)
* Explore the components that make up the Stream framework with the [Stream Reference](./stream-reference.md).
* Learn about the design, usage and configuration options provided with the [RabbitMQ Binder](./rabbit-binder.md).
* Learn how to [deploy .NET applications to Spring Cloud Data Flow (SCDF)](./data-flow-stream.md).
