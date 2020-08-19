# Messaging Overview

Steeltoe provides extensive support for integrating with messaging systems, from simplified use of the RabbitMQ API with `RabbitTemplate` to a complete infrastructure to receive messages asynchronously.
Look for support of additional messaging systems such as Kafka in the future.

## RabbitMQ

The Advanced Message Queuing Protocol (AMQP) is a platform-neutral, wire-level protocol for message-oriented middleware.
The Steeltoe RabbitMQ component applies core Steeltoe concepts to the development of AMQP-based messaging solutions on top of RabbitMQ.
Steeltoe offers several conveniences for working with AMQP through RabbitMQ, including auto configuration of the .NET service container.
