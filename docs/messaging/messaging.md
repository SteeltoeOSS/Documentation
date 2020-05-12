<a name="steeltoe-messaging"></a>
# Messaging

Steeltoe provides extensive support for integrating with messaging systems, from simplified use of the JMS API with `JmsTemplate` to a complete infrastructure to receive messages asynchronously.
Steeltoe AMQP provides a similar feature set for the Advanced Message Queuing Protocol (AMQP).
Steeltoe also provides auto-configuration options for `RabbitTemplate` and RabbitMQ.

<a name="steeltoe-messaging-jms"></a>
## JMS

The `javax.jms.ConnectionFactory` interface provides a standard method of creating a `javax.jms.Connection` for interacting with a JMS broker.
Although Steeltoe needs a `ConnectionFactory` to work with JMS, you generally need not use it directly yourself and can instead rely on higher level messaging abstractions.
(See the [relevant section](https://docs.spring.io/spring/docs/current/spring-framework-reference/integration.html#jms) of the Spring Framework reference documentation for details.)
Steeltoe also auto-configures the necessary infrastructure to send and receive messages.

<a name="steeltoe-messaging-connectionfactory"></a>
### Using a JNDI ConnectionFactory
[//]: # (I included this section in case you have a ConnectionFactory of some sort.)
[//]: # (If so, it needs to be mostly rewritten.)

If you are running your application in an application server, Steeltoe tries to locate a JMS `ConnectionFactory` by using JNDI.
By default, the `java:/JmsXA` and `java:/XAConnectionFactory` locations are checked.
You can use the `spring.jms.jndi-name` property if you need to specify an alternative location, as follows:

`spring.jms.jndi-name=java:/MyConnectionFactory`

<a name="steeltoe-messaging-sending-message"></a>
### Sending a Message

Steeltoe's `JmsTemplate` is auto-configured, and you can autowire it directly into your own beans, as follows:

```Java
	import org.springframework.beans.factory.annotation.Autowired;
	import org.springframework.jms.core.JmsTemplate;
	import org.springframework.stereotype.Component;

	@Component
	public class MyBean {

		private final JmsTemplate jmsTemplate;

		@Autowired
		public MyBean(JmsTemplate jmsTemplate) {
			this.jmsTemplate = jmsTemplate;
		}

		// ...

	}
```

>NOTE: [`JmsMessagingTemplate`](https://docs.spring.io/spring/docs/current/javadoc-api/org/springframework/jms/core/JmsMessagingTemplate.html) can be injected in a similar manner.
If a `DestinationResolver` or a `MessageConverter` bean is defined, it is associated automatically to the auto-configured `JmsTemplate`.

<a name="steeltoe-messaging-receiving-message"></a>
###= Receiving a Message

When the JMS infrastructure is present, any bean can be annotated with `@JmsListener` to create a listener endpoint.
If no `JmsListenerContainerFactory` has been defined, a default one is configured automatically.
If a `DestinationResolver` or a `MessageConverter` beans is defined, it is associated automatically to the default factory.

By default, the default factory is transactional.
If you run in an infrastructure where a `JtaTransactionManager` is present, it is associated to the listener container by default.
If not, the `sessionTransacted` flag is enabled.
In that latter scenario, you can associate your local data store transaction to the processing of an incoming message by adding `@Transactional` on your listener method (or a delegate thereof).
This ensures that the incoming message is acknowledged, once the local transaction has completed.
This also includes sending response messages that have been performed on the same JMS session.

The following component creates a listener endpoint on the `someQueue` destination:

```Java
	@Component
	public class MyBean {

		@JmsListener(destination = "someQueue")
		public void processMessage(String content) {
			// ...
		}

	}
```

TIP: See [the Javadoc of `@EnableJms`](https://docs.spring.io/spring/docs/current/javadoc-api/org/springframework/jms/annotation/EnableJms.html) for more details.

If you need to create more `JmsListenerContainerFactory` instances or if you want to override the default, Spring Boot provides a `DefaultJmsListenerContainerFactoryConfigurer` that you can use to initialize a `DefaultJmsListenerContainerFactory` with the same settings as the one that is auto-configured.

For instance, the following example exposes another factory that uses a specific `MessageConverter`:

```Java
	@Configuration(proxyBeanMethods = false)
	static class JmsConfiguration {

		@Bean
		public DefaultJmsListenerContainerFactory myFactory(
				DefaultJmsListenerContainerFactoryConfigurer configurer) {
			DefaultJmsListenerContainerFactory factory =
					new DefaultJmsListenerContainerFactory();
			configurer.configure(factory, connectionFactory());
			factory.setMessageConverter(myMessageConverter());
			return factory;
		}

	}
```

Then you can use the factory in any `@JmsListener`-annotated method, as follows:

```Java
	@Component
	public class MyBean {

		@JmsListener(destination = "someQueue", **containerFactory="myFactory"**)
		public void processMessage(String content) {
			// ...
		}

	}
```

<a name="steeltoe-messaging-amqp"></a>
## AMQP

The Advanced Message Queuing Protocol (AMQP) is a platform-neutral, wire-level protocol for message-oriented middleware.
The Steeltoe AMQP project applies core Steeltoe concepts to the development of AMQP-based messaging solutions.
Steeltoe offers several conveniences for working with AMQP through RabbitMQ, including the `spring-boot-starter-amqp` "Starter".

<a name="steeltoe-messaging-rabbitmq"></a>
### RabbitMQ Support

[RabbitMQ](https://www.rabbitmq.com/) is a lightweight, reliable, scalable, and portable message broker based on the AMQP protocol.
Steeltoe uses `RabbitMQ` to communicate through the AMQP protocol.

RabbitMQ configuration is controlled by external configuration properties in `+spring.rabbitmq.*+`.
For example, you might declare the following section in `application.properties`:

```
	spring.rabbitmq.host=localhost
	spring.rabbitmq.port=5672
	spring.rabbitmq.username=admin
	spring.rabbitmq.password=secret
```

Alternatively, you could configure the same connection using the `addresses` attribute:

```
	spring.rabbitmq.addresses=amqp://admin:secret@localhost
```

>NOTE: When specifying addresses that way, the `host` and `port` properties are ignored.
If the address uses the `amqps` protocol, SSL support is automatically enabled.

If a `ConnectionNameStrategy` bean exists in the context, it is automatically used to name connections created by the auto-configured `ConnectionFactory`.
See [`RabbitProperties`](https://github.com/spring-projects/spring-boot/tree/master/spring-boot-project/spring-boot-autoconfigure/src/main/java/org/springframework/boot/autoconfigure/amqp/RabbitProperties.java) for more of the supported options.

TIP: See [Understanding AMQP, the protocol used by RabbitMQ](https://spring.io/blog/2010/06/14/understanding-amqp-the-protocol-used-by-rabbitmq/) for more details.

<a name="steeltoe-messaging-rabbitmq-sending-message"></a>
### Sending a Message

Steeltoe's `AmqpTemplate` and `AmqpAdmin` are auto-configured, and you can autowire them directly into your own beans, as follows:

```Java
	import org.springframework.amqp.core.AmqpAdmin;
	import org.springframework.amqp.core.AmqpTemplate;
	import org.springframework.beans.factory.annotation.Autowired;
	import org.springframework.stereotype.Component;

	@Component
	public class MyBean {

		private final AmqpAdmin amqpAdmin;
		private final AmqpTemplate amqpTemplate;

		@Autowired
		public MyBean(AmqpAdmin amqpAdmin, AmqpTemplate amqpTemplate) {
			this.amqpAdmin = amqpAdmin;
			this.amqpTemplate = amqpTemplate;
		}

		// ...

	}
```

>NOTE: [`RabbitMessagingTemplate`](https://docs.spring.io/spring-amqp/docs/current/api/org/springframework/amqp/rabbit/core/RabbitMessagingTemplate.html) can be injected in a similar manner.
If a `MessageConverter` bean is defined, it is associated automatically to the auto-configured `AmqpTemplate`.

If necessary, any `org.springframework.amqp.core.Queue` that is defined as a bean is automatically used to declare a corresponding queue on the RabbitMQ instance.

To retry operations, you can enable retries on the `AmqpTemplate` (for example, in the event that the broker connection is lost):

```
	spring.rabbitmq.template.retry.enabled=true
	spring.rabbitmq.template.retry.initial-interval=2s
```

Retries are disabled by default.
You can also customize the `RetryTemplate` programmatically by declaring a `RabbitRetryTemplateCustomizer` bean.

If you need to create more `RabbitTemplate` instances or if you want to override the default, Spring Boot provides a `RabbitTemplateConfigurer` bean that you can use to initialize a `RabbitTemplate` with the same settings as the factories used by the auto-configuration.

<a name="steeltoe-messaging-rabbitmq-receiving-message"></a>
### Receiving a Message

When the Rabbit infrastructure is present, any bean can be annotated with `@RabbitListener` to create a listener endpoint.
If no `RabbitListenerContainerFactory` has been defined, a default `SimpleRabbitListenerContainerFactory` is automatically configured and you can switch to a direct container using the `spring.rabbitmq.listener.type` property.
If a `MessageConverter` or a `MessageRecoverer` bean is defined, it is automatically associated with the default factory.

The following sample component creates a listener endpoint on the `someQueue` queue:

```Java
	@Component
	public class MyBean {

		@RabbitListener(queues = "someQueue")
		public void processMessage(String content) {
			// ...
		}

	}
```

TIP: See [the Javadoc of `@EnableRabbit`](https://docs.spring.io/spring-amqp/docs/current/api/org/springframework/amqp/rabbit/annotation/EnableRabbit.html) for more details.

If you need to create more `RabbitListenerContainerFactory` instances or if you want to override the default, Spring Boot provides a `SimpleRabbitListenerContainerFactoryConfigurer` and a `DirectRabbitListenerContainerFactoryConfigurer` that you can use to initialize a `SimpleRabbitListenerContainerFactory` and a `DirectRabbitListenerContainerFactory` with the same settings as the factories used by the auto-configuration.

TIP: It does not matter which container type you chose.
Those two beans are exposed by the auto-configuration.

For instance, the following configuration class exposes another factory that uses a specific `MessageConverter`:

```Java
	@Configuration(proxyBeanMethods = false)
	static class RabbitConfiguration {

		@Bean
		public SimpleRabbitListenerContainerFactory myFactory(
				SimpleRabbitListenerContainerFactoryConfigurer configurer) {
			SimpleRabbitListenerContainerFactory factory =
					new SimpleRabbitListenerContainerFactory();
			configurer.configure(factory, connectionFactory);
			factory.setMessageConverter(myMessageConverter());
			return factory;
		}

	}
```

Then you can use the factory in any `@RabbitListener`-annotated method, as follows:

```Java
	@Component
	public class MyBean {

		@RabbitListener(queues = "someQueue", **containerFactory="myFactory"**)
		public void processMessage(String content) {
			// ...
		}

	}
```

You can enable retries to handle situations where your listener throws an exception.
By default, `RejectAndDontRequeueRecoverer` is used, but you can define a `MessageRecoverer` of your own.
When retries are exhausted, the message is rejected and either dropped or routed to a dead-letter exchange if the broker is configured to do so.
By default, retries are disabled.
You can also customize the `RetryTemplate` programmatically by declaring a `RabbitRetryTemplateCustomizer` bean.

>IMPORTANT: By default, if retries are disabled and the listener throws an exception, the delivery is retried indefinitely.
You can modify this behavior in two ways: Set the `defaultRequeueRejected` property to `false` so that zero re-deliveries are attempted or throw an `AmqpRejectAndDontRequeueException` to signal the message should be rejected.
The latter is the mechanism used when retries are enabled and the maximum number of delivery attempts is reached.
