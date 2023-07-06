# Using Steeltoe RabbitMQ

This chapter explores the interfaces and classes that are the essential components for developing applications with Steeltoe RabbitMQ.

## Steeltoe Messaging Abstractions

Steeltoe RabbitMQ consists of two modules (each represented by a JAR in the distribution): `spring-amqp` and `spring-rabbit`.
The `spring-amqp` module contains the `org.springframework.amqp.core` package.
Within that package, you can find the classes that represent the core AMQP "model".
Our intention is to provide generic abstractions that do not rely on any particular AMQP broker implementation or client library.
End-user code can be more portable across vendor implementations, as it can be developed against the abstraction layer only.
These abstractions are then implemented by broker-specific modules, such as `spring-rabbit`.
There is currently only a RabbitMQ implementation.
However, the abstractions have been validated in .NET using Apache Qpid in addition to RabbitMQ.
Since AMQP operates at the protocol level, in principle, you can use the RabbitMQ client with any broker that supports the same protocol version, but we do not test any other brokers at present.

This overview assumes that you are already familiar with the basics of the AMQP specification.
If not, have a look at the resources listed in <a href="steeltoe-messaging-further-reading"></a>.
[//]: # (I have no idea if that is the right way to do a cross-reference to another file in this system.)

### `Message`

The 0-9-1 AMQP specification does not define a `Message` class or interface.
Instead, when performing an operation such as `basicPublish()`, the content is passed as a byte-array argument, and additional properties are passed in as separate arguments.
Steeltoe RabbitMQ defines a `Message` class as part of a more general AMQP domain model representation.
The purpose of the `Message` class is to encapsulate the body and properties within a single instance so that the API can, in turn, be simpler.
The following example shows the `Message` class definition:

```Java
public class Message {

    private final MessageProperties messageProperties;

    private final byte[] body;

    public Message(byte[] body, MessageProperties messageProperties) {
        this.body = body;
        this.messageProperties = messageProperties;
    }

    public byte[] getBody() {
        return this.body;
    }

    public MessageProperties getMessageProperties() {
        return this.messageProperties;
    }
}
```

The `MessageProperties` interface defines several common properties, such as 'messageId', 'timestamp', 'contentType', and several more.
You can also extend those properties with user-defined 'headers' by calling the `setHeader(String key, Object value)` method.

>IMPORTANT: If a message body is a serialized `Serializable` Java object, it is no longer deserialized (by default) when performing `toString()` operations (such as in log messages).
This is to prevent unsafe deserialization.
By default, only `java.util` and `java.lang` classes are deserialized.
To revert to the previous behavior, you can add allowable class/package patterns by invoking `Message.addWhiteListPatterns(...)`.
A simple `*` wildcard is supported, for example `com.something.*, *.MyClass`.
Bodies that cannot be deserialized are represented by `byte[<size>]` in log messages.

### Exchange

The `Exchange` interface represents an AMQP exchange, which is what a message producer sends to.
Each Exchange within a virtual host of a broker has a unique name as well as a few other properties.
The following example shows the `Exchange` interface:

```Java
public interface Exchange {

    String getName();

    String getExchangeType();

    boolean isDurable();

    boolean isAutoDelete();

    Map<String, Object> getArguments();

}
```

As you can see, an `Exchange` also has a 'type' represented by constants defined in `ExchangeTypes`.
The basic types are: `direct`, `topic`, `fanout`, and `headers`.
In the core package, you can find implementations of the `Exchange` interface for each of those types.
The behavior varies across these `Exchange` types in terms of how they handle bindings to queues.
For example, a `Direct` exchange lets a queue be bound by a fixed routing key (often the queue's name).
A `Topic` exchange supports bindings with routing patterns that may include the '\*' and '#' wildcards for 'exactly-one' and 'zero-or-more', respectively.
The `Fanout` exchange publishes to all queues that are bound to it without taking any routing key into consideration.
For much more information about these and the other Exchange types, see <a href="docs/messaging/further-reading">Further Reading</a>resources>>.

>NOTE: The AMQP specification also requires that any broker provide a "default" direct exchange that has no name.
All queues that are declared are bound to that default `Exchange` with their names as routing keys.
You can learn more about the default Exchange's usage within Steeltoe RabbitMQ in <a href="#steeltoe-messaging-amqptemplate"></a>.

### Queue

The `Queue` class represents the component from which a message consumer receives messages.
Like the various `Exchange` classes, our implementation is intended to be an abstract representation of this core AMQP type.
The following listing shows the `Queue` class:

```Java
public class Queue  {

    private final String name;

    private volatile boolean durable;

    private volatile boolean exclusive;

    private volatile boolean autoDelete;

    private volatile Map<String, Object> arguments;

    /**
     * The queue is durable, non-exclusive and non auto-delete.
     *
     * @param name the name of the queue.
     */
    public Queue(String name) {
        this(name, true, false, false);
    }

    // Getters and Setters omitted for brevity

}
```

Notice that the constructor takes the queue name.
Depending on the implementation, the admin template may provide methods for generating a uniquely named queue.
Such queues can be useful as a "reply-to" address or in other *temporary* situations.
For that reason, the 'exclusive' and 'autoDelete' properties of an auto-generated queue would both be set to 'true'.

>NOTE: See the section on queues in <a href="#steeltoe-messaging-broker-configuration"></a> for information about declaring queues by using namespace support, including queue arguments.

### Binding

Given that a producer sends to an exchange and a consumer receives from a queue, the bindings that connect queues to exchanges are critical for connecting those producers and consumers via messaging.
In Steeltoe RabbitMQ, we define a `Binding` class to represent those connections.
This section reviews the basic options for binding queues to exchanges.

You can bind a queue to a `DirectExchange` with a fixed routing key, as follows:

```Java
new Binding(someQueue, someDirectExchange, "foo.bar");
```

You can bind a queue to a `TopicExchange` with a routing pattern, as the following example shows:

```Java
new Binding(someQueue, someTopicExchange, "foo.*");
```

You can bind a queue to a `FanoutExchange` with no routing key, as the following example shows:

```Java
new Binding(someQueue, someFanoutExchange);
```

We also provide a `BindingBuilder` to facilitate a "fluent API" style, as the following example shows:

```Java
Binding b = BindingBuilder.bind(someQueue).to(someTopicExchange).with("foo.*");
```

>NOTE: For clarity, the preceding example shows the `BindingBuilder` class, but this style works well when using a static import for the 'bind()' method.

By itself, an instance of the `Binding` class holds only the data about a connection.
In other words, it is not an "active" component.
However, as you will see later in <a href="#steeltoe-messaging-broker-configuration"></a>, the `AmqpAdmin` class can use `Binding` instances to actually trigger the binding actions on the broker.
Also, as you can see in that same section, you can define the `Binding` instances by using Spring's `@Bean` annotations within `@Configuration` classes.
There is also a convenient base class that further simplifies that approach for generating AMQP-related bean definitions and recognizes the queues, exchanges, and bindings so that they are all declared on the AMQP broker upon application startup.

The `AmqpTemplate` is also defined within the core package.
As one of the main components involved in actual AMQP messaging, it is discussed in detail in its own section (see <a href="#steeltoe-messaging-amqptemplate"></a>).

<a name="steeltoe-messaging-connections"></a>
## Connection and Resource Management

Whereas the AMQP model we described in the previous section is generic and applicable to all implementations, when we get into the management of resources, the details are specific to the broker implementation.
Therefore, in this section, we focus on code that exists only within our `spring-rabbit` module since, at this point, RabbitMQ is the only supported implementation.

The central component for managing a connection to the RabbitMQ broker is the `ConnectionFactory` interface.
The responsibility of a `ConnectionFactory` implementation is to provide an instance of `org.springframework.amqp.rabbit.connection.Connection`, which is a wrapper for `com.rabbitmq.client.Connection`.
The only concrete implementation we provide is `CachingConnectionFactory`, which, by default, establishes a single connection proxy that can be shared by the application.
Sharing of the connection is possible since the "unit of work" for messaging with AMQP is actually a "channel" (in some ways, this is similar to the relationship between a connection and a session in JMS).
The connection instance provides a `createChannel` method.
The `CachingConnectionFactory` implementation supports caching of those channels, and it maintains separate caches for channels based on whether they are transactional.
When creating an instance of `CachingConnectionFactory`, you can provide the 'hostname' through the constructor.
You should also provide the 'username' and 'password' properties.
To configure the size of the channel cache (the default is 25), you can call the
`setChannelCacheSize()` method.

You can configure the `CachingConnectionFactory` to cache connections as well as only channels.
In this case, each call to `createConnection()` creates a new connection (or retrieves an idle one from the cache).
Closing a connection returns it to the cache (if the cache size has not been reached).
Channels created on such connections are also cached.
The use of separate connections might be useful in some environments, such as consuming from an HA cluster, in
conjunction with a load balancer, to connect to different cluster members, and others.
To cache connections, set the `cacheMode` to `CacheMode.CONNECTION`.

>NOTE: This does not limit the number of connections.
Rather, it specifies how many idle open connections are allowed.

A new property called `connectionLimit` is provided.
When this property is set, it limits the total number of connections allowed.
When set, if the limit is reached, the `channelCheckoutTimeLimit` is used to wait for a connection to become idle.
If the time is exceeded, an `AmqpTimeoutException` is thrown.

>When the cache mode is `CONNECTION`, automatic declaration of queues and others
(See <a href="#automatic-declaration"></a>) is NOT supported.
Also, by default, the `amqp-client` library creates a fixed thread pool for each connection (default size: `Runtime.getRuntime().availableProcessors() * 2` threads).
When using a large number of connections, you should consider setting a custom `executor` on the `CachingConnectionFactory`.
Then, the same executor can be used by all connections and its threads can be shared.
The executor's thread pool should be unbounded or set appropriately for the expected use (usually, at least one thread per connection).
If multiple channels are created on each connection, the pool size affects the concurrency, so a variable (or simple cached) thread pool executor would be most suitable.

It is important to understand that the cache size is (by default) not a limit but is merely the number of channels that can be cached.
With a cache size of, say, 10, any number of channels can actually be in use.
If more than 10 channels are being used and they are all returned to the cache, 10 go in the cache.
The remainder are physically closed.

The default channel cache size has been increased from 1 to 25.
In high volume, multi-threaded environments, a small cache means that channels are created and closed at a high rate.
Increasing the default cache size can avoid this overhead.
You should monitor the channels in use through the RabbitMQ Admin UI and consider increasing the cache size further if you
see many channels being created and closed.
The cache grows only on-demand (to suit the concurrency requirements of the application), so this change does not
impact existing low-volume applications.

The `CachingConnectionFactory` has a property called `channelCheckoutTimeout`.
When this property is greater than zero, the `channelCacheSize` becomes a limit on the number of channels that can be created on a connection.
If the limit is reached, calling threads block until a channel is available or this timeout is reached, in which case a `AmqpTimeoutException` is thrown.

WARNING: Channels used within the framework (for example,
`RabbitTemplate`) are reliably returned to the cache.
If you create channels outside of the framework, (for example,
by accessing the connections directly and invoking `createChannel()`), you must return them (by closing) reliably, perhaps in a `finally` block, to avoid running out of channels.

The following example shows how to create a new `connection`:

```Java
CachingConnectionFactory connectionFactory = new CachingConnectionFactory("somehost");
connectionFactory.setUsername("guest");
connectionFactory.setPassword("guest");

Connection connection = connectionFactory.createConnection();
```

>NOTE: There is also a `SingleConnectionFactory` implementation that is available only in the unit test code of the framework.
It is simpler than `CachingConnectionFactory`, since it does not cache channels, but it is not intended for practical usage outside of simple tests due to its lack of performance and resilience.
If you need to implement your own `ConnectionFactory` for some reason, the `AbstractConnectionFactory` base class may provide a nice starting point.

### Naming Connections

A `ConnectionNameStrategy` is provided for the injection into the `AbstractionConnectionFactory`.
The generated name is used for the application-specific identification of the target RabbitMQ connection.
The connection name is displayed in the management UI if the RabbitMQ server supports it.
This value does not have to be unique and cannot be used as a connection identifier &#151; for example, in HTTP API requests.
This value is supposed to be human-readable and is a part of `ClientProperties` under the `connection_name` key.
You can use a simple Lambda, as follows:

```Java
connectionFactory.setConnectionNameStrategy(connectionFactory -> "MY_CONNECTION");
```

The `ConnectionFactory` argument can be used to distinguish target connection names by some logic.
By default, the `beanName` of the `AbstractConnectionFactory`, a hex string representing the object, and an internal counter are used to generate the `connection_name`.
The `<rabbit:connection-factory>` namespace component is also supplied with the `connection-name-strategy` attribute.

An implementation of `SimplePropertyValueConnectionNameStrategy` sets the connection name to an application property.
You can declare it as a `@Bean` and inject it into the connection factory, as the following example shows:

```Java
@Bean
public ConnectionNameStrategy cns() {
    return new SimplePropertyValueConnectionNameStrategy("spring.application.name");
}

@Bean
public ConnectionFactory rabbitConnectionFactory(ConnectionNameStrategy cns) {
    CachingConnectionFactory connectionFactory = new CachingConnectionFactory();
    ...
    connectionFactory.setConnectionNameStrategy(cns);
    return connectionFactory;
}
```

The property must exist in the application context's `Environment`.

>NOTE: When using Spring Boot and its autoconfigured connection factory, you need only declare the `ConnectionNameStrategy` `@Bean`.
Boot auto-detects the bean and wires it into the factory.

### Blocked Connections and Resource Constraints

The connection might be blocked for interaction from the broker that corresponds to the [Memory Alarm](https://www.rabbitmq.com/memory.html).
The `org.springframework.amqp.rabbit.connection.Connection` can be supplied with `com.rabbitmq.client.BlockedListener` instances to be notified for connection blocked and unblocked events.
In addition, the `AbstractConnectionFactory` emits a `ConnectionBlockedEvent` and `ConnectionUnblockedEvent`, respectively, through its internal `BlockedListener` implementation.
These let you provide application logic to react appropriately to problems on the broker and (for example) take some corrective actions.

>IMPORTANT:  When the application is configured with a single `CachingConnectionFactory`, as it is by default with Spring Boot auto-configuration, the application stops working when the connection is blocked by the Broker.
And when it is blocked by the Broker, any of its clients stop to work.
If we have producers and consumers in the same application, we may end up with a deadlock when producers are blocking the connection (because there are no resources on the Broker any more) and consumers cannot free them (because the connection is blocked).
To mitigate the problem, we suggest having one more separate `CachingConnectionFactory` instance with the same options &#151; one for producers and one for consumers.
A separate `CachingConnectionFactory` is not possible for transactional producers that execute on a consumer thread, since they should reuse the `Channel` associated with the consumer transactions.

The `RabbitTemplate` has a configuration option to automatically use a second connection factory, unless transactions are being used.
See <a href="#steeltoe-messaging-separate-connection"></a> for more information.
The `ConnectionNameStrategy` for the publisher connection is the same as the primary strategy with `.publisher` appended to the result of calling the method.

An `AmqpResourceNotAvailableException` is provided, which is thrown when `SimpleConnection.createChannel()` cannot create a `Channel` (for example, because the `channelMax` limit is reached and there are no available channels in the cache).
You can use this exception in the `RetryPolicy` to recover the operation after some back-off.

<a name="steeltoe-messaging-connection-factory"></a>
### Configuring the Underlying Client Connection Factory

The `CachingConnectionFactory` uses an instance of the Rabbit client `ConnectionFactory`.
A number of configuration properties are passed through (`host`, `port`, `userName`, `password`, `requestedHeartBeat`, and `connectionTimeout` for example) when setting the equivalent property on the `CachingConnectionFactory`.
To set other properties (`clientProperties`, for example), you can define an instance of the Rabbit factory and provide a reference to it by using the appropriate constructor of the `CachingConnectionFactory`.
When using the namespace (as <a href="#steeltoe-messaging-connections">described earlier</a>), you need to provide a reference to the configured factory in the `connection-factory` attribute.
For convenience, a factory bean is provided to assist in configuring the connection factory in a Spring application context, as discussed in <a href="#steeltoe-messaging-rabbitconnectionfactorybean-configuring-ssl"></a>.

[//]: # (There was an XML example here.)

>NOTE: The 4.0.x client enables automatic recovery by default.
While compatible with this feature, Steeltoe RabbitMQ has its own recovery mechanisms and the client recovery feature generally is not needed.
We recommend disabling `amqp-client` automatic recovery, to avoid getting `AutoRecoverConnectionNotCurrentlyOpenException` instances when the broker is available but the connection has not yet recovered.
You may notice this exception, for example, when a `RetryTemplate` is configured in a `RabbitTemplate`, even when failing over to another broker in a cluster.
Since the auto-recovering connection recovers on a timer, the connection may be recovered more quickly by using Steeltoe RabbitMQ's recovery mechanisms.
Steeltoe RabbitMQ disables `amqp-client` automatic recovery unless you explicitly create your own RabbitMQ connection factory and provide it to the `CachingConnectionFactory`.
RabbitMQ `ConnectionFactory` instances created by the `RabbitConnectionFactoryBean` also have the option disabled by default.

<a name="steeltoe-messaging-rabbitconnectionfactorybean-configuring-ssl></a>
### `RabbitConnectionFactoryBean` and Configuring SSL

A convenient `RabbitConnectionFactoryBean` is provided to enable convenient configuration of SSL properties on the underlying client connection factory by using dependency injection.
Other setters delegate to the underlying factory.
Previously, you had to configure the SSL options programmatically.
The following example shows how to configure a `RabbitConnectionFactoryBean`:

[//]: # (There was an XML example here.)

See the [RabbitMQ Documentation](https://www.rabbitmq.com/ssl.html) for information about configuring SSL.
Omit the `keyStore` and `trustStore` configuration to connect over SSL without certificate validation.
The next example shows how you can provide key and trust store configuration.

The `sslPropertiesLocation` property is a Spring `Resource` pointing to a properties file containing the following keys:

```
keyStore=file:/secret/keycert.p12
trustStore=file:/secret/trustStore
keyStore.passPhrase=secret
trustStore.passPhrase=secret
```

The `keyStore` and `truststore` are Spring `Resources` pointing to the stores.
Typically this properties file is secured by the operating system with the application having read access.

You can set these properties directly on the factory bean.
If both discrete properties and `sslPropertiesLocation` is provided, properties in the latter override the
discrete values.

>IMPORTANT: The server certificate is validated by default because it is more secure.
If you wish to skip this validation for some reason, set the factory bean's `skipServerCertificateValidation` property to `true`.
The `RabbitConnectionFactoryBean` now calls `enableHostnameVerification()` by default.
To revert to the previous behavior, set the `enableHostnameVerification` property to `false`.

>IMPORTANT: The factory bean will always use TLS v1.2 by default; previously, it used v1.1 in some cases and v1.2 in others (depending on other properties).
If you need to use v1.1 for some reason, set the `sslAlgorithm` property: `setSslAlgorithm("TLSv1.1")`.

<a name="steeltoe-messaging-cluster"></a>
### Connecting to a Cluster

To connect to a cluster, configure the `addresses` property on the `CachingConnectionFactory`, as follows:

```Java
@Bean
public CachingConnectionFactory ccf() {
    CachingConnectionFactory ccf = new CachingConnectionFactory();
    ccf.setAddresses("host1:5672,host2:5672,host3:5672");
    return ccf;
}
```

The underlying connection factory tries to connect to each host, in order, whenever a new connection is established.
The connection order can be made random by setting the `shuffleAddresses` property to true; the shuffle will be applied before creating any new connection.

```Java
@Bean
public CachingConnectionFactory ccf() {
    CachingConnectionFactory ccf = new CachingConnectionFactory();
    ccf.setAddresses("host1:5672,host2:5672,host3:5672");
    ccf.setShuffleAddresses(true);
    return ccf;
}
```

<a name="steeltoe-messaging-routing-connection-factory"></a>
### Routing Connection Factory

The `AbstractRoutingConnectionFactory` has been introduced.
This factory provides a mechanism to configure mappings for several `ConnectionFactories` and determine a target `ConnectionFactory` by some `lookupKey` at runtime.
Typically, the implementation checks a thread-bound context.
For convenience, Steeltoe RabbitMQ provides the `SimpleRoutingConnectionFactory`, which gets the current thread-bound `lookupKey` from the `SimpleResourceHolder`.
The following examples shows how to configure a `SimpleRoutingConnectionFactory`:

```Java
public class MyService {

    @Autowired
    private RabbitTemplate rabbitTemplate;

    public void service(String vHost, String payload) {
        SimpleResourceHolder.bind(rabbitTemplate.getConnectionFactory(), vHost);
        rabbitTemplate.convertAndSend(payload);
        SimpleResourceHolder.unbind(rabbitTemplate.getConnectionFactory());
    }

}
```

It is important to unbind the resource after use.
For more information, see the [JavaDoc](https://docs.spring.io/spring-amqp/docs/latest-ga/api/org/springframework/amqp/rabbit/connection/AbstractRoutingConnectionFactory.html) for `AbstractRoutingConnectionFactory`.

`RabbitTemplate` supports the SpEL `sendConnectionFactorySelectorExpression` and `receiveConnectionFactorySelectorExpression` properties, which are evaluated on each AMQP protocol interaction operation (`send`, `sendAndReceive`, `receive`, or `receiveAndReply`), resolving to a `lookupKey` value for the provided `AbstractRoutingConnectionFactory`.
You can use bean references, such as `@vHostResolver.getVHost(#root)` in the expression.
For `send` operations, the message to be sent is the root evaluation object.
For `receive` operations, the `queueName` is the root evaluation object.

The routing algorithm is as follows: If the selector expression is `null` or is evaluated to `null` or the provided `ConnectionFactory` is not an instance of `AbstractRoutingConnectionFactory`, everything works as before, relying on the provided `ConnectionFactory` implementation.
The same occurs if the evaluation result is not `null`, but there is no target `ConnectionFactory` for that `lookupKey` and the `AbstractRoutingConnectionFactory` is configured with `lenientFallback = true`.
In the case of an `AbstractRoutingConnectionFactory`, it does fallback to its `routing` implementation based on `determineCurrentLookupKey()`.
However, if `lenientFallback = false`, an `IllegalStateException` is thrown.

The namespace support also provides the `send-connection-factory-selector-expression` and `receive-connection-factory-selector-expression` attributes on the `<rabbit:template>` component.

Also, you can configure a routing connection factory in a listener container.
In that case, the list of queue names is used as the lookup key.
For example, if you configure the container with `setQueueNames("thing1", "thing2")`, the lookup key is `[thing1,thing]` (note that there is no space in the key).

You can add a qualifier to the lookup key by using `setLookupKeyQualifier` on the listener container.
Doing so enables, for example, listening to queues with the same name but in a different virtual host (where you would have a connection factory for each).

For example, with lookup key qualifier `thing1` and a container listening to queue `thing2`, the lookup key you could register the target connection factory with could be `thing1[thing2]`.

<a name="steeltoe-messaging-queue-affinity"></a>
### Queue Affinity and the `LocalizedQueueConnectionFactory`

When using HA queues in a cluster, for the best performance, you may want to connect to the physical broker
where the master queue resides.
The `CachingConnectionFactory` can be configured with multiple broker addresses.
This is to fail over and the client attempts to connect in order.
The `LocalizedQueueConnectionFactory` uses the REST API provided by the admin plugin to determine on which node the queue is mastered.
It then creates (or retrieves from a cache) a `CachingConnectionFactory` that connects to just that node.
If the connection fails, the new master node is determined and the consumer connects to it.
The `LocalizedQueueConnectionFactory` is configured with a default connection factory, in case the physical location of the queue cannot be determined, in which case it connects as normal to the cluster.

The `LocalizedQueueConnectionFactory` is a `RoutingConnectionFactory` and the `SimpleMessageListenerContainer` uses the queue names as the lookup key as discussed in <a href="#steeltoe-messaging-routing-connection-factory"></a> above.

>NOTE: For this reason (the use of the queue name for the lookup), the `LocalizedQueueConnectionFactory` can only be used if the container is configured to listen to a single queue.

>NOTE: The RabbitMQ management plugin must be enabled on each node.

>CAUTION: This connection factory is intended for long-lived connections, such as those used by the `SimpleMessageListenerContainer`.
It is not intended for short connection use, such as with a `RabbitTemplate` because of the overhead of invoking the REST API before making the connection.
Also, for publish operations, the queue is unknown, and the message is published to all cluster members anyway, so the logic of looking up the node has little value.

The following example configures the factories:

```Java
@Autowired
private ConfigurationProperties props;

@Bean
public ConnectionFactory defaultConnectionFactory() {
    CachingConnectionFactory cf = new CachingConnectionFactory();
    cf.setAddresses(this.props.getAddresses());
    cf.setUsername(this.props.getUsername());
    cf.setPassword(this.props.getPassword());
    cf.setVirtualHost(this.props.getVirtualHost());
    return cf;
}

@Bean
public ConnectionFactory queueAffinityCF(
        @Qualifier("defaultConnectionFactory") ConnectionFactory defaultCF) {
    return new LocalizedQueueConnectionFactory(defaultCF,
            StringUtils.commaDelimitedListToStringArray(this.props.getAddresses()),
            StringUtils.commaDelimitedListToStringArray(this.props.getAdminUris()),
            StringUtils.commaDelimitedListToStringArray(this.props.getNodes()),
            this.props.getVirtualHost(), this.props.getUsername(), this.props.getPassword(),
            false, null);
}
```

Notice that the first three parameters are arrays of `addresses`, `adminUris`, and `nodes`.
These are positional in that, when a container attempts to connect to a queue, it uses the admin API to determine on which node the queue is
mastered and connects to the address in the same array position as that node.

<a name="steeltoe-messaging-cf-pub-conf-ret"></a>
### Publisher Confirms and Returns

Confirmed (with correlation) and returned messages are supported by setting the `CachingConnectionFactory` property `publisherConfirmType` to `ConfirmType.CORRELATED` and the `publisherReturns` property to 'true'.

When these options are set, `Channel` instances created by the factory are wrapped in an `PublisherCallbackChannel`, which is used to facilitate the callbacks.
When such a channel is obtained, the client can register a `PublisherCallbackChannel.Listener` with the `Channel`.
The `PublisherCallbackChannel` implementation contains logic to route a confirm or return to the appropriate listener.
These features are explained further in the following sections.

See also `simplePublisherConfirms` in <a href="#steeltoe-messaging-scoped-operations"></a>.

TIP: For some more background information, see the blog post by the RabbitMQ team titled https://www.rabbitmq.com/blog/2011/02/10/introducing-publisher-confirms/[Introducing Publisher Confirms].

<a name="steeltoe-messaging-connection-channel-listeners"></a>
### Connection and Channel Listeners

The connection factory supports registering `ConnectionListener` and `ChannelListener` implementations.
This allows you to receive notifications for connection and channel related events.
(A `ConnectionListener` is used by the `RabbitAdmin` to perform declarations when the connection is established.
  See <a href="#automatic-declaration"></a> for more information).
The following listing shows the `ConnectionListener` interface definition:

```Java
@FunctionalInterface
public interface ConnectionListener {

    void onCreate(Connection connection);

    default void onClose(Connection connection) {
    }

    default void onShutDown(ShutdownSignalException signal) {
    }

}
```

The `org.springframework.amqp.rabbit.connection.Connection` object can be supplied with `com.rabbitmq.client.BlockedListener` instances to be notified for connection blocked and unblocked events.
The following example shows the ChannelListener interface definition:

```Java
@FunctionalInterface
public interface ChannelListener {

    void onCreate(Channel channel, boolean transactional);

    default void onShutDown(ShutdownSignalException signal) {
    }

}
```

See <a href="#publishing-is-async"></a> for one scenario where you might want to register a `ChannelListener`.

<a name="steeltoe-messaging-channel-close-logging"></a>
### Logging Channel Close Events

Version 1.5 introduced a mechanism to enable users to control logging levels.

The `CachingConnectionFactory` uses a default strategy to log channel closures as follows:

* Normal channel closes (200 OK) are not logged.
* If a channel is closed due to a failed passive queue declaration, it is logged at debug level.
* If a channel is closed because the `basic.consume` is refused due to an exclusive consumer condition, it is logged at
INFO level.
* All others are logged at ERROR level.

To modify this behavior, you can inject a custom `ConditionalExceptionLogger` into the
`CachingConnectionFactory` in its `closeExceptionLogger` property.

See also <a href="#steeltoe-messaging-consumer-events"></a>.

<a name="runtime-cache-properties"></a>
### Runtime Cache Properties

The `CachingConnectionFactory` now provides cache statistics through the `getCacheProperties()` method.
These statistics can be used to tune the cache to optimize it in production.
For example, the high water marks can be used to determine whether the cache size should be increased.
If it equals the cache size, you might want to consider increasing further.
The following table describes the `CacheMode.CHANNEL` properties:

| Property | Meaning |
| -------- | ------- |
| connectionName | The name of the connection generated by the `ConnectionNameStrategy`. |
| channelCacheSize | The currently configured maximum channels that are allowed to be idle. |
| localPort | The local port for the connection (if available). This can be used to correlate with connections and channels on the RabbitMQ Admin UI. |
| idleChannelsTx | The number of transactional channels that are currently idle (cached). |
| idleChannelsNotTx | The number of non-transactional channels that are currently idle (cached). |
| idleChannelsTxHighWater | The maximum number of transactional channels that have been concurrently idle (cached). |
| idleChannelsNotTxHighWater  | The maximum number of non-transactional channels have been concurrently idle (cached). |

The following table describes the `CacheMode.CONNECTION` properties:

| Property | Meaning |
| --- | --- |
| connectionName:<localPort> |The name of the connection generated by the `ConnectionNameStrategy`. |
| openConnections | The number of connection objects representing connections to brokers. |
| channelCacheSize | The currently configured maximum channels that are allowed to be idle. |
| connectionCacheSize | The currently configured maximum connections that are allowed to be idle. |
| idleConnections | The number of connections that are currently idle. |
| idleConnectionsHighWater | The maximum number of connections that have been concurrently idle. |
| idleChannelsTx:<localPort> | The number of transactional channels that are currently idle (cached) for this connection. You can use the `localPort` part of the property name to correlate with connections and channels on the RabbitMQ Admin UI. |
| idleChannelsNotTx:<localPort> | The number of non-transactional channels that are currently idle (cached) for this connection. The `localPort` part of the property name can be used to correlate with connections and channels on the RabbitMQ Admin UI. |
|idleChannelsTxHighWater:<localPort> | The maximum number of transactional channels that have been concurrently idle (cached). The localPort part of the property name can be used to correlate with connections and channels on the RabbitMQ Admin UI. |
| idleChannelsNotTxHighWater:<localPort> | The maximum number of non-transactional channels have been concurrently idle (cached). You can use the `localPort` part of the property name to correlate with connections and channels on the RabbitMQ Admin UI. |

The `cacheMode` property (`CHANNEL` or `CONNECTION`) is also included.

The following image shows a JVisualVM example:

![JVisualVm example](images/cacheStats.png)

<a name="steeltoe-messaging-auto-recovery"></a>
### RabbitMQ Automatic Connection/Topology recovery

Since the first version of Steeltoe RabbitMQ, the framework has provided its own connection and channel recovery in the event of a broker failure.
Also, as discussed in <a href="#steeltoe-messaging-broker-configuration"></a>, `RabbitAdmin` re-declares any infrastructure beans (queues and others) when the connection is re-established.
It, therefore, does not rely on the [auto-recovery](https://www.rabbitmq.com/api-guide.html#recovery) that is now provided by the `amqp-client` library.
Steeltoe RabbitMQ now uses the `4.0.x` version of `amqp-client`, which has auto-recovery enabled by default.
Steeltoe RabbitMQ can still use its own recovery mechanisms if you wish, disabling it in the client, (by setting the `automaticRecoveryEnabled` property on the underlying `RabbitMQ connectionFactory` to `false`).
However, the framework is completely compatible with auto-recovery being enabled.
This means any consumers you create within your code (perhaps by using `RabbitTemplate.execute()`) can be automatically recovered.

>IMPORTANT: Only elements (queues, exchanges, bindings) that are defined as beans will be re-declared after a connection failure.
Elements declared by invoking `RabbitAdmin.declare*()` methods directly from user code are unknown to the framework and therefore cannot be recovered.
If you have a need for a variable number of declarations, consider defining a bean, or beans, of type `Declarables`, as discussed in <a href="#steeltoe-messaging-collection-declaration"></a>.

<a name="steeltoe-messaging-custom-client-props"></a>
## Adding Custom Client Connection Properties

The `CachingConnectionFactory` now lets you access the underlying connection factory to allow, for example,
setting custom client properties.
The following example shows how to do so:

```Java
connectionFactory.getRabbitConnectionFactory().getClientProperties().put("thing1", "thing2");
```

These properties appear in the RabbitMQ Admin UI when viewing the connection.

<a name="steeltoe-messaging-amqptemplate"></a>
## `AmqpTemplate`

As with many other high-level abstractions provided by the Spring Framework and related projects, Steeltoe RabbitMQ provides a "template" that plays a central role.
The interface that defines the main operations is called `AmqpTemplate`.
Those operations cover the general behavior for sending and receiving messages.
In other words, they are not unique to any implementation &#151; hence the "AMQP" in the name.
On the other hand, there are implementations of that interface that are tied to implementations of the AMQP protocol.
Unlike JMS, which is an interface-level API itself, AMQP is a wire-level protocol.
The implementations of that protocol provide their own client libraries, so each implementation of the template interface depends on a particular client library.
Currently, there is only a single implementation: `RabbitTemplate`.
In the examples that follow, we often use an `AmqpTemplate`.
However, when you look at the configuration examples or any code excerpts where the template is instantiated or setters are invoked, you can see the implementation type (for example, `RabbitTemplate`).

As mentioned earlier, the `AmqpTemplate` interface defines all of the basic operations for sending and receiving messages.
We will explore message sending and reception, respectively, in <a href="#steeltoe-messaging-sending-messages"></a> and <a href="#steeltoe-messaging-receiving-messages"></a>.

See also <a href="#steeltoe-messaging-async-template"></a>.

<a name="steeltoe-messaging-template-retry"></a>
## Adding Retry Capabilities

You can now configure the `RabbitTemplate` to use a `RetryTemplate` to help with handling problems with broker connectivity.
See the https://github.com/spring-projects/spring-retry[spring-retry] project for complete information.
The following is only one example that uses an exponential back off policy and the default `SimpleRetryPolicy`, which makes three tries before throwing the exception to the caller.

The following example uses the `@Configuration` annotation in Java:

```Java
@Bean
public AmqpTemplate rabbitTemplate() {
    RabbitTemplate template = new RabbitTemplate(connectionFactory());
    RetryTemplate retryTemplate = new RetryTemplate();
    ExponentialBackOffPolicy backOffPolicy = new ExponentialBackOffPolicy();
    backOffPolicy.setInitialInterval(500);
    backOffPolicy.setMultiplier(10.0);
    backOffPolicy.setMaxInterval(10000);
    retryTemplate.setBackOffPolicy(backOffPolicy);
    template.setRetryTemplate(retryTemplate);
    return template;
}
```

In addition to the `retryTemplate` property, the `recoveryCallback` option is supported on the `RabbitTemplate`.
It is used as a second argument for the `RetryTemplate.execute(RetryCallback<T, E> retryCallback, RecoveryCallback<T> recoveryCallback)`.

>NOTE: The `RecoveryCallback` is somewhat limited, in that the retry context contains only the `lastThrowable` field.
For more sophisticated use cases, you should use an external `RetryTemplate` so that you can convey additional information to the `RecoveryCallback` through the context's attributes.
The following example shows how to do so:

```Java
retryTemplate.execute(
    new RetryCallback<Object, Exception>() {

        @Override
        public Object doWithRetry(RetryContext context) throws Exception {
            context.setAttribute("message", message);
            return rabbitTemplate.convertAndSend(exchange, routingKey, message);
        }

    }, new RecoveryCallback<Object>() {

        @Override
        public Object recover(RetryContext context) throws Exception {
            Object message = context.getAttribute("message");
            Throwable t = context.getLastThrowable();
            // Do something with message
            return null;
        }
    });
}
```

In this case, you would *not* inject a `RetryTemplate` into the `RabbitTemplate`.

<a name="steeltoe-messaging-publishing-is-async"></a>
### Publishing is Asynchronous &#151; How to Detect Successes and Failures

Publishing messages is an asynchronous mechanism and, by default, messages that cannot be routed are dropped by RabbitMQ.
For successful publishing, you can receive an asynchronous confirm, as described in <a href="#steeltoe-messaging-template-confirms"></a>.
Consider two failure scenarios:

* Publish to an exchange but there is no matching destination queue.
* Publish to a non-existent exchange.

The first case is covered by publisher returns, as described in <a href="#steeltoe-messaging-template-confirms"></a>.

For the second case, the message is dropped and no return is generated.
The underlying channel is closed with an exception.
By default, this exception is logged, but you can register a `ChannelListener` with the `CachingConnectionFactory` to obtain notifications of such events.
The following example adds a `ConnectionListener`:

```Java
this.connectionFactory.addConnectionListener(new ConnectionListener() {

    @Override
    public void onCreate(Connection connection) {
    }

    @Override
    public void onShutDown(ShutdownSignalException signal) {
        ...
    }

});
```

You can examine the signal's `reason` property to determine the problem that occurred.

To detect the exception on the sending thread, you can `setChannelTransacted(true)` on the `RabbitTemplate` and the exception is detected on the `txCommit()`.
However, *transactions significantly impede performance*, so consider this carefully before enabling transactions for just this one use case.

<a name="steeltoe-messaging-template-confirms"></a>
### Publisher Confirms and Returns

The `RabbitTemplate` implementation of `AmqpTemplate` supports publisher confirms and returns.

For returned messages, the template's `mandatory` property must be set to `true` or the `mandatory-expression`
must evaluate to `true` for a particular message.
This feature requires a `CachingConnectionFactory` that has its `publisherReturns` property set to `true` (see <a href="#steeltoe-messaging-cf-pub-conf-ret"></a>).
Returns are sent to the client by it registering a `RabbitTemplate.ReturnCallback` by calling `setReturnCallback(ReturnCallback callback)`.
The callback must implement the following method:

```Java
void returnedMessage(Message message, int replyCode, String replyText,
          String exchange, String routingKey);
```

Only one `ReturnCallback` is supported by each `RabbitTemplate`.
See also <a href="#steeltoe-messaging-reply-timeout"><a/>.

For publisher confirms (also known as publisher acknowledgements), the template requires a `CachingConnectionFactory` that has its `publisherConfirm` property set to `ConfirmType.CORRELATED`.
Confirms are sent to the client by it registering a `RabbitTemplate.ConfirmCallback` by calling `setConfirmCallback(ConfirmCallback callback)`.
The callback must implement this method:

```Java
void confirm(CorrelationData correlationData, boolean ack, String cause);
```

The `CorrelationData` is an object supplied by the client when sending the original message.
The `ack` is true for an `ack` and false for a `nack`.
For `nack` instances, the cause may contain a reason for the `nack`, if it is available when the `nack` is generated.
An example is when sending a message to a non-existent exchange.
In that case, the broker closes the channel.
The reason for the closure is included in the `cause`.
The `cause` was added in version 1.4.

Only one `ConfirmCallback` is supported by a `RabbitTemplate`.

>NOTE: When a rabbit template send operation completes, the channel is closed.
This precludes the reception of confirms or returns when the connection factory cache is full (when there is space in the cache, the channel is not physically closed and the returns and confirms proceed normally).
When the cache is full, the framework defers the close for up to five seconds, in order to allow time for the confirms and returns to be received.
When using confirms, the channel is closed when the last confirm is received.
When using only returns, the channel remains open for the full five seconds.
We generally recommend setting the connection factory's `channelCacheSize` to a large enough value so that the channel on which a message is published is returned to the cache instead of being closed.
You can monitor channel usage by using the RabbitMQ management plugin.
If you see channels being opened and closed rapidly, you should consider increasing the cache size to reduce overhead on the server.

>IMPORTANT: Before version 2.1, channels enabled for publisher confirms were returned to the cache before the confirms were received.
Some other process could check out the channel and perform some operation that causes the channel to close &#151; such as publishing a message to a non-existent exchange.
This could cause the confirm to be lost.
Version 2.1 and later no longer return the channel to the cache while confirms are outstanding.
The `RabbitTemplate` performs a logical `close()` on the channel after each operation.
In general, this means that only one confirm is outstanding on a channel at a time.

>NOTE: The callbacks are invoked on one of the connection factory's `executor` threads.
This is to avoid a potential deadlock if you perform Rabbit operations from within the callback.
With previous versions, the callbacks were invoked directly on the `amqp-client` connection I/O thread; this would deadlock if you perform some RPC operation (such as opening a new channel) since the I/O thread blocks waiting for the result, but the result needs to be processed by the I/O thread itself.
With those versions, it was necessary to hand off work (such as sending a messasge) to another thread within the callback.
This is no longer necessary since the framework now hands off the callback invocation to the executor.

>IMPORTANT: The guarantee of receiving a returned message before the ack is still maintained as long as the return callback executes in 60 seconds or less.
The confirm is scheduled to be delivered after the return callback exits or after 60 seconds, whichever comes first.

The `CorrelationData` object has a `ListenableFuture` that you can use to get the result, instead of using a `ConfirmCallback` on the template.
The following example shows how to configure a `CorrelationData` instance:

```Java
CorrelationData cd1 = new CorrelationData();
this.templateWithConfirmsEnabled.convertAndSend("exchange", queue.getName(), "foo", cd1);
assertTrue(cd1.getFuture().get(10, TimeUnit.SECONDS).isAck());
```

Since it is a `ListenableFuture<Confirm>`, you can either `get()` the result when ready or add listeners for an asynchronous callback.
The `Confirm` object is a simple bean with 2 properties: `ack` and `reason` (for `nack` instances).
The reason is not populated for broker-generated `nack` instances.
It is populated for `nack` instances generated by the framework (for example, closing the connection while `ack` instances are outstanding).

In addition, when both confirms and returns are enabled, the `CorrelationData` is populated with the returned message.
It is guaranteed that this occurs before the future is set with the `ack`.

See also <a href="#steeltoe-messaging-scoped-operations"></a> for a simpler mechanism for waiting for publisher confirms.

<a name="steeltoe-messaging-scoped-operations"></a>
### Scoped Operations

Normally, when using the template, a `Channel` is checked out of the cache (or created), used for the operation, and returned to the cache for reuse.
In a multi-threaded environment, there is no guarantee that the next operation uses the same channel.
There may be times, however, where you want to have more control over the use of a channel and ensure that a number of operations are all performed on the same channel.

A new method called `invoke` is provided, with an `OperationsCallback`.
Any operations performed within the scope of the callback and on the provided `RabbitOperations` argument use the same dedicated `Channel`, which will be closed at the end (not returned to a cache).
If the channel is a `PublisherCallbackChannel`, it is returned to the cache after all confirms have been received (see <a href="#steeltoe-messaging-template-confirms"></a>).

```Java
@FunctionalInterface
public interface OperationsCallback<T> {

    T doInRabbit(RabbitOperations operations);

}
```

One example of why you might need this is if you wish to use the `waitForConfirms()` method on the underlying `Channel`.
This method was not previously exposed by the Spring API because the channel is, generally, cached and shared, as discussed earlier.
The `RabbitTemplate` now provides `waitForConfirms(long timeout)` and `waitForConfirmsOrDie(long timeout)`, which delegate to the dedicated channel used within the scope of the `OperationsCallback`.
The methods cannot be used outside of that scope, for obvious reasons.

Note that a higher-level abstraction that lets you correlate confirms to requests is provided elsewhere (see <a href="#steeltoe-messaging-template-confirms"></a>).
If you want only to wait until the broker has confirmed delivery, you can use the technique shown in the following example:

```Java
Collection<?> messages = getMessagesToSend();
Boolean result = this.template.invoke(t -> {
    messages.forEach(m -> t.convertAndSend(ROUTE, m));
    t.waitForConfirmsOrDie(10_000);
    return true;
});
```

If you wish `RabbitAdmin` operations to be invoked on the same channel within the scope of the `OperationsCallback`, the admin must have been constructed by using the same `RabbitTemplate` that was used for the `invoke` operation.

>NOTE: The preceding discussion is moot if the template operations are already performed within the scope of an existing transaction &#151; for example, when running on a transacted listener container thread and performing operations on a transacted template.
In that case, the operations are performed on that channel and committed when the thread returns to the container.
It is not necessary to use `invoke` in that scenario.

When using confirms in this way, much of the infrastructure set up for correlating confirms to requests is not really needed (unless returns are also enabled).
The connection factory supports a new property called `publisherConfirmType`.
When this is set to `ConfirmType.SIMPLE`, the infrastructure is avoided and the confirm processing can be more efficient.

Furthermore, the `RabbitTemplate` sets the `publisherSequenceNumber` property in the sent message `MessageProperties`.
If you wish to check (or log or otherwise use) specific confirms, you can do so with an overloaded `invoke` method, as the following example shows:

```Java
public <T> T invoke(OperationsCallback<T> action, com.rabbitmq.client.ConfirmCallback acks,
        com.rabbitmq.client.ConfirmCallback nacks);
```

>NOTE: These `ConfirmCallback` objects (for `ack` and `nack` instances) are the Rabbit client callbacks, not the template callback.

The following example logs `ack` and `nack` instances:

```Java
Collection<?> messages = getMessagesToSend();
Boolean result = this.template.invoke(t -> {
    messages.forEach(m -> t.convertAndSend(ROUTE, m));
    t.waitForConfirmsOrDie(10_000);
    return true;
}, (tag, multiple) -> {
        log.info("Ack: " + tag + ":" + multiple);
}, (tag, multiple) -> {
        log.info("Nack: " + tag + ":" + multiple);
}));
```

<a name="steeltoe-messaging-template-messaging"></a>
### Messaging Integration

`RabbitMessagingTemplate` (built on top of `RabbitTemplate`) provides an integration with the Spring Framework messaging abstraction &#151; that is, `org.springframework.messaging.Message`.
This lets you send and receive messages by using the `spring-messaging` `Message<?>` abstraction.
This abstraction is used by other Spring projects, such as Spring Integration and Spring's STOMP support.
There are two message converters involved: one to convert between a spring-messaging `Message<?>` and Steeltoe RabbitMQ's `Message` abstraction and one to convert between Steeltoe RabbitMQ's `Message` abstraction and the format required by the underlying RabbitMQ client library.
By default, the message payload is converted by the provided `RabbitTemplate` instance's message converter.
Alternatively, you can inject a custom `MessagingMessageConverter` with some other payload converter, as the following example shows:

```Java
MessagingMessageConverter amqpMessageConverter = new MessagingMessageConverter();
amqpMessageConverter.setPayloadConverter(myPayloadConverter);
rabbitMessagingTemplate.setAmqpMessageConverter(amqpMessageConverter);
```

<a name="steeltoe-messaging-template-user-id"></a>
### Validated User Id

The template supports a `user-id-expression` (`userIdExpression` when using Java configuration).
If a message is sent, the user id property is set (if not already set) after evaluating this expression.
The root object for the evaluation is the message to be sent.

The following examples show how to use the `user-id-expression` attribute:

[//]: # (There was an XML example here. If you adjust the preceding paragraph, it can make sense with a C# example.)

The first example is a literal expression.
The second obtains the `username` property from a connection factory bean in the application context.

<a name="steeltoe-messaging-separate-connection"></a>
### Using a Separate Connection

You can set the `usePublisherConnection` property to `true` to use a different connection to that used by listener containers, when possible.
This is to avoid consumers being blocked when a producer is blocked for any reason.
The `CachingConnectionFactory` now maintains a second internal connection factory for this purpose.
If the rabbit template is running in a transaction started by the listener container, the container's channel is used, regardless of this setting.

>IMPORTANT: In general, you should not use a `RabbitAdmin` with a template that has this set to `true`.
Use the `RabbitAdmin` constructor that takes a connection factory.
If you use the other constructor that takes a template, ensure the template's property is `false`.
This is because, often, an admin is used to declare queues for listener containers.
Using a template that has the property set to `true` would mean that exclusive queues (such as `AnonymousQueue`) would be declared on a different connection to that used by listener containers.
In that case, the queues cannot be used by the containers.

<a name="steeltoe-messaging-sending-messages"></a>
## Sending Messages

When sending a message, you can use any of the following methods:

```Java
void send(Message message) throws AmqpException;

void send(String routingKey, Message message) throws AmqpException;

void send(String exchange, String routingKey, Message message) throws AmqpException;
```

We can begin our discussion with the last method in the preceding listing, since it is actually the most explicit.
It lets an AMQP exchange name  (along with a routing key)be provided at runtime.
The last parameter is the callback that is responsible for actual creating the message instance.
An example of using this method to send a message might look this this:
The following example uses the `send` method to send a message:

```Java
amqpTemplate.send("marketData.topic", "quotes.nasdaq.THING1",
    new Message("12.34".getBytes(), someProperties));
```

You can set the `exchange` property on the template itself if you plan to use that template instance to send to the same exchange most or all of the time.
In such cases, you can use the second method in the preceding listing.
The following example is functionally equivalent to the previous example:

```Java
amqpTemplate.setExchange("marketData.topic");
amqpTemplate.send("quotes.nasdaq.FOO", new Message("12.34".getBytes(), someProperties));
```

If both the `exchange` and `routingKey` properties are set on the template, you can use the method that accepts only the `Message`.
The following example shows how to do so:

```Java
amqpTemplate.setExchange("marketData.topic");
amqpTemplate.setRoutingKey("quotes.nasdaq.FOO");
amqpTemplate.send(new Message("12.34".getBytes(), someProperties));
```

A better way of thinking about the exchange and routing key properties is that the explicit method parameters always override the template's default values.
In fact, even if you do not explicitly set those properties on the template, there are always default values in place.
In both cases, the default is an empty `String`, but that is actually a sensible default.
As far as the routing key is concerned, it is not always necessary in the first place (for example, for
a `Fanout` exchange).
Furthermore, a queue may be bound to an exchange with an empty `String`.
Those are both legitimate scenarios for reliance on the default empty `String` value for the routing key property of the template.
As far as the exchange name is concerned, the empty `String` is commonly used because the AMQP specification defines the "default exchange" as having no name.
Since all queues are automatically bound to that default exchange (which is a direct exchange), using their name as the binding value, the second method in the preceding listing can be used for simple point-to-point messaging to any queue through the default exchange.
You can provide the queue name as the `routingKey`, either by providing the method parameter at runtime.
The following example shows how to do so:

```Java
RabbitTemplate template = new RabbitTemplate(); // using default no-name Exchange
template.send("queue.helloWorld", new Message("Hello World".getBytes(), someProperties));
```

Alternately, you can create a template that can be used for publishing primarily or exclusively to a single Queue.
The following example shows how to do so:

```Java
RabbitTemplate template = new RabbitTemplate(); // using default no-name Exchange
template.setRoutingKey("queue.helloWorld"); // but we'll always send to this Queue
template.send(new Message("Hello World".getBytes(), someProperties));
```

[[message-builder]]
### Message Builder API

A message builder API is provided by the `MessageBuilder` and `MessagePropertiesBuilder`.
These methods provide a convenient "fluent" means of creating a message or message properties.
The following examples show the fluent API in action:

```Java
Message message = MessageBuilder.withBody("foo".getBytes())
    .setContentType(MessageProperties.CONTENT_TYPE_TEXT_PLAIN)
    .setMessageId("123")
    .setHeader("bar", "baz")
    .build();

MessageProperties props = MessagePropertiesBuilder.newInstance()
    .setContentType(MessageProperties.CONTENT_TYPE_TEXT_PLAIN)
    .setMessageId("123")
    .setHeader("bar", "baz")
    .build();
Message message = MessageBuilder.withBody("foo".getBytes())
    .andProperties(props)
    .build();
```

Each of the properties defined on the https://docs.spring.io/spring-amqp/docs/latest-ga/api/org/springframework/amqp/core/MessageProperties.html[`MessageProperties`] can be set.
Other methods include `setHeader(String key, String value)`, `removeHeader(String key)`, `removeHeaders()`, and `copyProperties(MessageProperties properties)`.
Each property setting method has a `set*IfAbsent()` variant.
In the cases where a default initial value exists, the method is named `set*IfAbsentOrDefault()`.

Five static methods are provided to create an initial message builder:

```Java
public static MessageBuilder withBody(byte[] body) <1>

public static MessageBuilder withClonedBody(byte[] body) <2>

public static MessageBuilder withBody(byte[] body, int from, int to) <3>

public static MessageBuilder fromMessage(Message message) <4>

public static MessageBuilder fromClonedMessage(Message message) <5>
```

<1> The message created by the builder has a body that is a direct reference to the argument.
<2> The message created by the builder has a body that is a new array containing a copy of bytes in the argument.
<3>	The message created by the builder has a body that is a new array containing the range of bytes from the argument.
See https://docs.oracle.com/javase/7/docs/api/java/util/Arrays.html[`Arrays.copyOfRange()`] for more details.
<4> The message created by the builder has a body that is a direct reference to the body of the argument.
The argument's properties are copied to a new `MessageProperties`  object.
<5> The message created by the builder has a body that is a new array containing a copy of the argument's body.
The argument's properties are copied to a new `MessageProperties`  object.

Three static methods are provided to create a `MessagePropertiesBuilder` instance:

```Java
public static MessagePropertiesBuilder newInstance() <1>

public static MessagePropertiesBuilder fromProperties(MessageProperties properties) <2>

public static MessagePropertiesBuilder fromClonedProperties(MessageProperties properties) <3>
```

<1> A new message properties object is initialized with default values.
<2> The builder is initialized with, and `build()` will return, the provided properties object.,
<3> The argument's properties are copied to a new `MessageProperties` object.

With the `RabbitTemplate` implementation of `AmqpTemplate`, each of the `send()` methods has an overloaded version that takes an additional `CorrelationData` object.
When publisher confirms are enabled, this object is returned in the callback described in <a href="#steeltoe-messaging-amqptemplate"></a>.
This lets the sender correlate a confirm (`ack` or `nack`) with the sent message.

The `CorrelationAwareMessagePostProcessor` interface was introduced, allowing the correlation data to be modified after the message has been converted.
The following example shows how to use it:

```Java
Message postProcessMessage(Message message, Correlation correlation);
```

This interface is deprecated.
The method has been moved to `MessagePostProcessor` with a default implementation that delegates to `postProcessMessage(Message message)`.

Also, a new callback interface called `CorrelationDataPostProcessor` is provided.
This is invoked after all `MessagePostProcessor` instances (provided in the `send()` method as well as those provided in `setBeforePublishPostProcessors()`).
Implementations can update or replace the correlation data supplied in the `send()` method (if any).
The `Message` and original `CorrelationData` (if any) are provided as arguments.
The following example shows how to use the `postProcess` method:

```Java
CorrelationData postProcess(Message message, CorrelationData correlationData);
```

### Publisher Returns

When the template's `mandatory` property is `true`, returned messages are provided by the callback described in <a href="#steeltoe-messaging-amqptemplate"></a>.

The `RabbitTemplate` supports the SpEL `mandatoryExpression` property, which is evaluated against each request message as the root evaluation object, resolving to a `boolean` value.
Bean references, such as `@myBean.isMandatory(#root)`, can be used in the expression.

Publisher returns can also be used internally by the `RabbitTemplate` in send and receive operations.
See <a href="#steeltoe-messaging-reply-timeout"><a/> for more information.

<a name="steeltoe-messaging-template-batching"></a>
### Batching

Version 1.4.2 introduced the `BatchingRabbitTemplate`.
This is a subclass of `RabbitTemplate` with an overridden `send` method that batches messages according to the `BatchingStrategy`.
Only when a batch is complete is the message sent to RabbitMQ.
The following listing shows the `BatchingStrategy` interface definition:

```Java
public interface BatchingStrategy {

	MessageBatch addToBatch(String exchange, String routingKey, Message message);

	Date nextRelease();

	Collection<MessageBatch> releaseBatches();

}
```

>CAUTION: Batched data is held in memory.
Unsent messages can be lost in the event of a system failure.

A `SimpleBatchingStrategy` is provided.
It supports sending messages to a single exchange or routing key.
It has the following properties:

* `batchSize`: The number of messages in a batch before it is sent.
* `bufferLimit`: The maximum size of the batched message. This preempts the `batchSize`, if exceeded, and causes a partial batch to be sent.
* `timeout`: A time after which a partial batch is sent when there is no new activity adding messages to the batch.

The `SimpleBatchingStrategy` formats the batch by preceding each embedded message with a four-byte binary length.
This is communicated to the receiving system by setting the `springBatchFormat` message property to `lengthHeader4`.

>IMPORTANT: Batched messages are automatically de-batched by listener containers by default (by using the `springBatchFormat` message header).
Rejecting any message from a batch causes the entire batch to be rejected.

However, see <a href="#steeltoe-messaging-receiving-batch"></a> for more information.

<a name="steeltoe-messaging-receiving-messages"></a>
## Receiving Messages

Message reception is always a little more complicated than sending.
There are two ways to receive a `Message`.
The simpler option is to poll for one `Message` at a time with a polling method call.
The more complicated yet more common approach is to register a listener that receives `Messages` on-demand, asynchronously.
We consider an example of each approach in the next two sub-sections.

<a name="steeltoe-messaging-polling-consumer"></a>
### Polling the Consumer

The `AmqpTemplate` itself can be used for polled `Message` reception.
By default, if no message is available, `null` is returned immediately.
There is no blocking.
You can set a `receiveTimeout`, in milliseconds, and the receive methods block for up to that long, waiting for a message.
A value less than zero means block indefinitely (or at least until the connection to the broker is lost).
Version 1.6 introduced variants of the `receive` methods that let the timeout be passed in on each call.

>CAUTION: Since the receive operation creates a new `QueueingConsumer` for each message, this technique is not really appropriate for high-volume environments.
Consider using an asynchronous consumer or a `receiveTimeout` of zero for those use cases.

There are four simple `receive` methods available.
As with the `Exchange` on the sending side, there is a method that requires that a default queue property has been set
directly on the template itself, and there is a method that accepts a queue parameter at runtime.
Version 1.6 introduced variants to accept `timeoutMillis` to override `receiveTimeout` on a per-request basis.
The following listing shows the definitions of the four methods:

```Java
Message receive() throws AmqpException;

Message receive(String queueName) throws AmqpException;

Message receive(long timeoutMillis) throws AmqpException;

Message receive(String queueName, long timeoutMillis) throws AmqpException;
```

As in the case of sending messages, the `AmqpTemplate` has some convenience methods for receiving POJOs instead of `Message` instances, and implementations provide a way to customize the `MessageConverter` used to create the `Object` returned:
The following listing shows those methods:

```Java
Object receiveAndConvert() throws AmqpException;

Object receiveAndConvert(String queueName) throws AmqpException;

Object receiveAndConvert(long timeoutMillis) throws AmqpException;

Object receiveAndConvert(String queueName, long timeoutMillis) throws AmqpException;
```

There are variants of these methods that take an additional `ParameterizedTypeReference` argument to convert complex types.
The template must be configured with a `SmartMessageConverter`.
See <a href="#steeltoe-messaging-json-complex"></a> for more information.

Similar to `sendAndReceive` methods, beginning with version 1.3, the `AmqpTemplate` has several convenience `receiveAndReply` methods for synchronously receiving, processing and replying to messages.
The following listing shows those method definitions:

```Java
<R, S> boolean receiveAndReply(ReceiveAndReplyCallback<R, S> callback)
	   throws AmqpException;

<R, S> boolean receiveAndReply(String queueName, ReceiveAndReplyCallback<R, S> callback)
 	throws AmqpException;

<R, S> boolean receiveAndReply(ReceiveAndReplyCallback<R, S> callback,
	String replyExchange, String replyRoutingKey) throws AmqpException;

<R, S> boolean receiveAndReply(String queueName, ReceiveAndReplyCallback<R, S> callback,
	String replyExchange, String replyRoutingKey) throws AmqpException;

<R, S> boolean receiveAndReply(ReceiveAndReplyCallback<R, S> callback,
 	ReplyToAddressCallback<S> replyToAddressCallback) throws AmqpException;

<R, S> boolean receiveAndReply(String queueName, ReceiveAndReplyCallback<R, S> callback,
			ReplyToAddressCallback<S> replyToAddressCallback) throws AmqpException;
```

The `AmqpTemplate` implementation takes care of the `receive` and `reply` phases.
In most cases, you should provide only an implementation of `ReceiveAndReplyCallback` to perform some business logic for the received message and build a reply object or message, if needed.
Note, a `ReceiveAndReplyCallback` may return `null`.
In this case, no reply is sent and `receiveAndReply` works like the `receive` method.
This lets the same queue be used for a mixture of messages, some of which may not need a reply.

Automatic message (request and reply) conversion is applied only if the provided callback is not an instance of `ReceiveAndReplyMessageCallback`, which provides a raw message exchange contract.

The `ReplyToAddressCallback` is useful for cases requiring custom logic to determine the `replyTo` address at runtime against the received message and reply from the `ReceiveAndReplyCallback`.
By default, `replyTo` information in the request message is used to route the reply.

The following listing shows an example of POJO-based receive and reply:

```Java
boolean received =
        this.template.receiveAndReply(ROUTE, new ReceiveAndReplyCallback<Order, Invoice>() {

                public Invoice handle(Order order) {
                        return processOrder(order);
                }
        });
if (received) {
        log.info("We received an order!");
}
```

<a name="steeltoe-messaging-async-consumer"></a>
### Asynchronous Consumer

>IMPORTANT: Steeltoe RabbitMQ also supports annotated listener endpoints through the use of the `@RabbitListener` annotation and provides an open infrastructure to register endpoints programmatically.
This is by far the most convenient way to setup an asynchronous consumer.
See <a href="#steeltoe-messaging-async-annotation-driven"></a> for more details.

>IMPORTANT: The prefetch default value used to be 1, which could lead to under-utilization of efficient consumers.
The default prefetch value is now 250, which should keep consumers busy in most common scenarios and
thus improve throughput.
There are, nevertheless, scenarios where the prefetch value should be low:
* For large messages, especially if the processing is slow (messages could add up to a large amount of memory in the client process)
* When strict message ordering is necessary (the prefetch value should be set back to 1 in this case)
* Other special cases
Also, with low-volume messaging and multiple consumers (including concurrency within a single listener container instance), you may wish to reduce the prefetch to get a more even distribution of messages across consumers.
We also recommend using `prefetch = 1` with the `MANUAL` `ack` mode.
The `basicAck` is an asynchronous operation and, if something wrong happens on the Broker (double `ack` for the same delivery tag, for example), you end up with processed subsequent messages in the batch that are unacknowledged on the Broker, and other consumers may see them.
See <a href="#steeltoe-messaging-container-attributes"></a>.
For more background about prefetch, see this post about https://www.rabbitmq.com/blog/2014/04/14/finding-bottlenecks-with-rabbitmq-3-3/[consumer utilization in RabbitMQ]
and this post about https://www.rabbitmq.com/blog/2012/05/11/some-queuing-theory-throughput-latency-and-bandwidth/[queuing theory].

#### Message Listener

For asynchronous `Message` reception, a dedicated component (not the `AmqpTemplate`) is involved.
That component is a container for a `Message`-consuming callback.
We consider the container and its properties later in this section.
First, though, we should look at the callback, since that is where your application code is integrated with the messaging system.
There are a few options for the callback, starting with an implementation of the `MessageListener` interface, which the following listing shows:

```Java
public interface MessageListener {
    void onMessage(Message message);
}
```

If your callback logic depends on the AMQP Channel instance for any reason, you may instead use the `ChannelAwareMessageListener`.
It looks similar but has an extra parameter.
The following listing shows the `ChannelAwareMessageListener` interface definition:

```Java
public interface ChannelAwareMessageListener {
    void onMessage(Message message, Channel channel) throws Exception;
}
```

>IMPORTANT: In version 2.1, this interface moved from package `o.s.amqp.rabbit.core` to `o.s.amqp.rabbit.listener.api`.

<a name="steeltoe-messaging-message-listener-adapter"></a>
#### `MessageListenerAdapter`

If you prefer to maintain a stricter separation between your application logic and the messaging API, you can rely upon an adapter implementation that is provided by the framework.
This is often referred to as "Message-driven POJO" support.

>NOTE: A more flexible mechanism for POJO messaging is the `@RabbitListener` annotation.
See <a href="#steeltoe-messaging-async-annotation-driven"></a> for more information.

When using the adapter, you need to provide only a reference to the instance that the adapter itself should invoke.
The following example shows how to do so:

```Java
MessageListenerAdapter listener = new MessageListenerAdapter(somePojo);
listener.setDefaultListenerMethod("myMethod");
```

You can subclass the adapter and provide an implementation of `getListenerMethodName()` to dynamically select different methods based on the message.
This method has two parameters, `originalMessage` and `extractedMessage`, the latter being the result of any conversion.
By default, a `SimpleMessageConverter` is configured.
See <a href="#steeltoe-messaging-simple-message-converter"></a> for more information and information about other converters available.

The original message has `consumerQueue` and `consumerTag` properties, which can be used to determine the queue from which a message was received.

You can configure a map of consumer queue or tag to method name, to dynamically select the method to call.
If no entry is in the map, we fall back to the default listener method.
The default listener method (if not set) is `handleMessage`.

A convenient `FunctionalInterface` has been provided.
The following listing shows the definition of `FunctionalInterface`:

```Java
@FunctionalInterface
public interface ReplyingMessageListener<T, R> {

	R handleMessage(T t);

}
```

This interface facilitates convenient configuration of the adapter by using Java 8 lambdas, as the following example shows:

```Java
new MessageListenerAdapter((ReplyingMessageListener<String, String>) data -> {
    ...
    return result;
}));
```

The `buildListenerArguments(Object)` has been deprecated and new `buildListenerArguments(Object, Channel, Message)` one has been introduced instead.
The new method helps listener to get `Channel` and `Message` arguments to do more, such as calling `channel.basicReject(long, boolean)` in manual acknowledge mode.
The following listing shows the most basic example:

```Java
public class ExtendedListenerAdapter extends MessageListenerAdapter {

    @Override
    protected Object[] buildListenerArguments(Object extractedMessage, Channel channel, Message message) {
        return new Object[]{extractedMessage, channel, message};
    }

}
```

Now you could configure `ExtendedListenerAdapter` as same as `MessageListenerAdapter` if you need to receive "channel" and "message".
Parameters of listener should be set as `buildListenerArguments(Object, Channel, Message)` returned, as the following example of listener shows:

```Java
public void handleMessage(Object object, Channel channel, Message message) throws IOException {
    ...
}
```

#### Container

Now that you have seen the various options for the `Message`-listening callback, we can turn our attention to the container.
Basically, the container handles the "active" responsibilities so that the listener callback can remain passive.
The container is an example of a "lifecycle" component.
It provides methods for starting and stopping.
When configuring the container, you essentially bridge the gap between an RabbitMQ Queue and the `MessageListener` instance.
You must provide a reference to the `ConnectionFactory` and the queue names or Queue instances from which that listener should consume messages.

Prior to version 2.0, there was one listener container, the `SimpleMessageListenerContainer`.
There is now a second container, the `DirectMessageListenerContainer`.
The differences between the containers and criteria you might apply when choosing which to use are described in <a href="#steeltoe-messaging-choose-container"></a>.

The following listing shows the most basic example, which works by using the, `SimpleMessageListenerContainer`:

```Java
SimpleMessageListenerContainer container = new SimpleMessageListenerContainer();
container.setConnectionFactory(rabbitConnectionFactory);
container.setQueueNames("some.queue");
container.setMessageListener(new MessageListenerAdapter(somePojo));
```

As an "active" component, it is most common to create the listener container with a bean definition so that it can run in the background.
The following example shows how to do so:

```Java
@Configuration
public class ExampleAmqpConfiguration {

    @Bean
    public SimpleMessageListenerContainer messageListenerContainer() {
        SimpleMessageListenerContainer container = new SimpleMessageListenerContainer();
        container.setConnectionFactory(rabbitConnectionFactory());
        container.setQueueName("some.queue");
        container.setMessageListener(exampleListener());
        return container;
    }

    @Bean
    public ConnectionFactory rabbitConnectionFactory() {
        CachingConnectionFactory connectionFactory =
            new CachingConnectionFactory("localhost");
        connectionFactory.setUsername("guest");
        connectionFactory.setPassword("guest");
        return connectionFactory;
    }

    @Bean
    public MessageListener exampleListener() {
        return new MessageListener() {
            public void onMessage(Message message) {
                System.out.println("received: " + message);
            }
        };
    }
}
```

<a name="steeltoe-messaging-consumer-priority"></a>
#### Consumer Priority

The broker now supports consumer priority (see [Using Consumer Priorities with RabbitMQ](https://www.rabbitmq.com/blog/2013/12/16/using-consumer-priorities-with-rabbitmq/)).
This is enabled by setting the `x-priority` argument on the consumer.
The `SimpleMessageListenerContainer` now supports setting consumer arguments, as the following example shows:

```Java
container.setConsumerArguments(Collections.
<String, Object> singletonMap("x-priority", Integer.valueOf(10)));
```

You can modify the queues on which the container listens at runtime.
See <a href="#steeltoe-messaging-listener-queues"></a>.

<a name="steeltoe-messaging-lc-auto-delete"></a>
#### `auto-delete` Queues

When a container is configured to listen to `auto-delete` queues, the queue has an `x-expires` option, or the [Time-To-Live](https://www.rabbitmq.com/ttl.html) policy is configured on the Broker, the queue is removed by the broker when the container is stopped (that is, when the last consumer is cancelled).
Before version 1.3, the container could not be restarted because the queue was missing.
The `RabbitAdmin` only automatically redeclares queues and so on when the connection is closed or when it opens, which does not happen when the container is stopped and started.

The container uses a `RabbitAdmin` to redeclare any missing queues during startup.

You can also use conditional declaration (see <a href="#steeltoe-messaging-conditional-declaration"></a>) together with an `auto-startup="false"` admin to defer queue declaration until the container is started.
The following example shows how to do so:

[//]: # (There was an XML example here.)

In this case, the queue and exchange are declared by `containerAdmin`, which has `auto-startup="false"` so that the elements are not declared during context initialization.
Also, the container is not started for the same reason.
When the container is later started, it uses its reference to `containerAdmin` to declare the elements.

<a name="steeltoe-messaging-de-batching"></a>
### Batched Messages

Batched messages (created by a producer) are automatically de-batched by listener containers (using the `springBatchFormat` message header).
Rejecting any message from a batch causes the entire batch to be rejected.
See <a href="steeltoe-messaging-template-batching"></a> for more information about batching.

The `SimpleMessageListeneContainer` can be use to create batches on the consumer side (where the producer sent discrete messages).

Set the container property `consumerBatchEnabled` to enable this feature.
`deBatchingEnabled` must also be true so that the container is responsible for processing batches of both types.
Implement `BatchMessageListener` or `ChannelAwareBatchMessageListener` when `consumerBatchEnabled` is true.
See <a href="#steeltoe-messaging-receiving-batch"></a> for information about using this feature with `@RabbitListener`.

<a name="steeltoe-messaging-consumer-events"></a>
### Consumer Events

The containers publish application events whenever a listener
(consumer) experiences a failure of some kind.
The event `ListenerContainerConsumerFailedEvent` has the following properties:

* `container`: The listener container where the consumer experienced the problem.
* `reason`: A textual reason for the failure.
* `fatal`: A boolean indicating whether the failure was fatal. With non-fatal exceptions, the container tries to restart the consumer, according to the `recoveryInterval` or `recoveryBackoff` (for the `SimpleMessageListenerContainer`) or the `monitorInterval` (for the `DirectMessageListenerContainer`).
* `throwable`: The `Throwable` that was caught.

These events can be consumed by implementing `ApplicationListener<ListenerContainerConsumerFailedEvent>`.

>NOTE: System-wide events (such as connection failures) are published by all consumers when `concurrentConsumers` is greater than 1.

If a consumer fails because one if its queues is being used exclusively, by default, as well as publishing the event, a `WARN` log is issued.
To change this logging behavior, provide a custom `ConditionalExceptionLogger` in the `SimpleMessageListenerContainer` instance's `exclusiveConsumerExceptionLogger` property.
See also <a href="#steeltoe-messaging-channel-close-logging"></a>.

Fatal errors are always logged at the `ERROR` level.
This it not modifiable.

Several other events are published at various stages of the container lifecycle:

* `AsyncConsumerStartedEvent`: When the consumer is started.
* `AsyncConsumerRestartedEvent`: When the consumer is restarted after a failure - `SimpleMessageListenerContainer` only.
* `AsyncConsumerTerminatedEvent`: When a consumer is stopped normally.
* `AsyncConsumerStoppedEvent`: When the consumer is stopped - `SimpleMessageListenerContainer` only.
* `ConsumeOkEvent`: When a `consumeOk` is received from the broker, contains the queue name and `consumerTag`
* `ListenerContainerIdleEvent`: See <a href="#steeltoe-messaging-idle-containers"></a>.

<a name="steeltoe-messaging-consumer-tags"></a>
### Consumer Tags

You can provide a strategy to generate consumer tags.
By default, the consumer tag is generated by the broker.
The following listing shows the `ConsumerTagStrategy` interface definition:

```Java
public interface ConsumerTagStrategy {

    String createConsumerTag(String queue);

}
```

The queue is made available so that it can (optionally) be used in the tag.

See <a href="#steeltoe-messaging-container-attributes"></a>.

<a name="steeltoe-messaging-async-annotation-driven"></a>
### Annotation-driven Listener Endpoints

The easiest way to receive a message asynchronously is to use the annotated listener endpoint infrastructure.
In a nutshell, it lets you expose a method of a managed bean as a Rabbit listener endpoint.
The following example shows how to use the `@RabbitListener` annotation:

```Java
@Component
public class MyService {

    @RabbitListener(queues = "myQueue")
    public void processOrder(String data) {
        ...
    }

}
```

The idea of the preceding example is that, whenever a message is available on the queue named `myQueue`, the `processOrder` method is invoked accordingly (in this case, with the payload of the message).

The annotated endpoint infrastructure creates a message listener container behind the scenes for each annotated method, by using a `RabbitListenerContainerFactory`.

In the preceding example, `myQueue` must already exist and be bound to some exchange.
The queue can be declared and bound automatically, as long as a `RabbitAdmin` exists in the application context.

>NOTE: Property placeholders (`${some.property}`) or SpEL expressions (`#{someExpression}`) can be specified for the annotation properties (`queues` etc).
See <a href="#steeltoe-messating-annotation-multiple-queues"></a> for an example of why you might use SpEL instead of a property placeholder.
The following listing shows three examples of how to declare a Rabbit listener:

```Java
@Component
public class MyService {

  @RabbitListener(bindings = @QueueBinding(
        value = @Queue(value = "myQueue", durable = "true"),
        exchange = @Exchange(value = "auto.exch", ignoreDeclarationExceptions = "true"),
        key = "orderRoutingKey")
  )
  public void processOrder(Order order) {
    ...
  }

  @RabbitListener(bindings = @QueueBinding(
        value = @Queue,
        exchange = @Exchange(value = "auto.exch"),
        key = "invoiceRoutingKey")
  )
  public void processInvoice(Invoice invoice) {
    ...
  }

  @RabbitListener(queuesToDeclare = @Queue(name = "${my.queue}", durable = "true"))
  public String handleWithSimpleDeclare(String data) {
      ...
  }

}
```

In the first example, a queue `myQueue` is declared automatically (durable) together with the exchange, if needed,
and bound to the exchange with the routing key.
In the second example, an anonymous (exclusive, auto-delete) queue is declared and bound.
Multiple `QueueBinding` entries can be provided, letting the listener listen to multiple queues.
In the third example, a queue with the name retrieved from property `my.queue` is declared, if necessary, with the default binding to the default exchange using the queue name as the routing key.

Since version 2.0, the `@Exchange` annotation supports any exchange types, including custom.
For more information, see [AMQP Concepts](https://www.rabbitmq.com/tutorials/amqp-concepts.html).

You can use normal `@Bean` definitions when you need more advanced configuration.

Notice `ignoreDeclarationExceptions` on the exchange in the first example.
This allows, for example, binding to an existing exchange that might have different settings (such as `internal`).
By default, the properties of an existing exchange must match.

You can now bind a queue to an exchange with multiple routing keys, as follows:

```Java
...
    key = { "red", "yellow" }
...
```

You can also specify arguments within `@QueueBinding` annotations for queues, exchanges,
and bindings, as follows:

```Java
@RabbitListener(bindings = @QueueBinding(
        value = @Queue(value = "auto.headers", autoDelete = "true",
                        arguments = @Argument(name = "x-message-ttl", value = "10000",
                                                type = "java.lang.Integer")),
        exchange = @Exchange(value = "auto.headers", type = ExchangeTypes.HEADERS, autoDelete = "true"),
        arguments = {
                @Argument(name = "x-match", value = "all"),
                @Argument(name = "thing1", value = "somevalue"),
                @Argument(name = "thing2")
        })
)
public String handleWithHeadersExchange(String foo) {
    ...
}
```

Notice that the `x-message-ttl` argument is set to 10 seconds for the queue.
Since the argument type is not `String`, we have to specify its type &#151; in this case, `Integer`.
As with all such declarations, if the queue already exists, the arguments must match those on the queue.
For the header exchange, we set the binding arguments to match messages that have the `thing1` header set to `somevalue`, and
the `thing2` header must be present with any value.
The `x-match` argument means both conditions must be satisfied.

The argument name, value, and type can be property placeholders (`${...}`) or SpEL expressions (`#{...}`).
The `name` must resolve to a `String`.
The expression for `type` must resolve to a `Class` or the fully-qualified name of a class.
The `value` must resolve to something that can be converted by the `DefaultConversionService` to the type (such as the `x-message-ttl` in the preceding example).

If a name resolves to `null` or an empty `String`, that `@Argument` is ignored.

<a name="steeltoe-messaging-meta-annotation-driven"></a>
#### Meta-annotations

Sometimes you may want to use the same configuration for multiple listeners.
To reduce the boilerplate configuration, you can use meta-annotations to create your own listener annotation.
The following example shows how to do so:

```Java
@Target({ElementType.TYPE, ElementType.METHOD, ElementType.ANNOTATION_TYPE})
@Retention(RetentionPolicy.RUNTIME)
@RabbitListener(bindings = @QueueBinding(
        value = @Queue,
        exchange = @Exchange(value = "metaFanout", type = ExchangeTypes.FANOUT)))
public @interface MyAnonFanoutListener {
}

public class MetaListener {

    @MyAnonFanoutListener
    public void handle1(String foo) {
        ...
    }

    @MyAnonFanoutListener
    public void handle2(String foo) {
        ...
    }

}
```

In the preceding example, each listener created by the `@MyAnonFanoutListener` annotation binds an anonymous, auto-delete
queue to the fanout exchange, `metaFanout`.
`@AliasFor` is supported to allow overriding properties on the meta-annotated annotation.
Also, user annotations can now be `@Repeatable`, allowing multiple containers to be created for a method.

```Java
@Component
static class MetaAnnotationTestBean {

    @MyListener("queue1")
    @MyListener("queue2")
    public void handleIt(String body) {
    }

}


@RabbitListener
@Target(ElementType.METHOD)
@Retention(RetentionPolicy.RUNTIME)
@Repeatable(MyListeners.class)
static @interface MyListener {

    @AliasFor(annotation = RabbitListener.class, attribute = "queues")
    String[] value() default {};

}

@Target(ElementType.METHOD)
@Retention(RetentionPolicy.RUNTIME)
static @interface MyListeners {

    MyListener[] value();

}
```

<a name="steeltoe-messaging-async-annotation-driven-enable"></a>
#### Enable Listener Endpoint Annotations

To enable support for `@RabbitListener` annotations, you can add `@EnableRabbit` to one of your `@Configuration` classes.
The following example shows how to do so:

```Java
@Configuration
@EnableRabbit
public class AppConfig {

    @Bean
    public SimpleRabbitListenerContainerFactory rabbitListenerContainerFactory() {
        SimpleRabbitListenerContainerFactory factory = new SimpleRabbitListenerContainerFactory();
        factory.setConnectionFactory(connectionFactory());
        factory.setConcurrentConsumers(3);
        factory.setMaxConcurrentConsumers(10);
        factory.setContainerCustomizer(container -> /* customize the container */);
        return factory;
    }
}
```

Since version 2.0, a `DirectMessageListenerContainerFactory` is also available.
It creates `DirectMessageListenerContainer` instances.

>NOTE: For information to help you choose between `SimpleRabbitListenerContainerFactory` and `DirectRabbitListenerContainerFactory`, see <a href="#steeltoe-messaging-choose-container"></a>.

Starting wih version 2.2.2, you can provide a `ContainerCustomizer` implementation (as shown above).
This can be used to further configure the container after it has been created and configured; you can use this, for example, to set properties that are not exposed by the container factory.

By default, the infrastructure looks for a bean named `rabbitListenerContainerFactory` as the source for the factory to use to create message listener containers.
In this case, and ignoring the RabbitMQ infrastructure setup, the `processOrder` method can be invoked with a core poll size of three threads and a maximum pool size of ten threads.

You can customize the listener container factory to use for each annotation, or you can configure an explicit default by implementing the `RabbitListenerConfigurer` interface.
The default is required only if at least one endpoint is registered without a specific container factory.
See the [Javadoc](https://docs.spring.io/spring-amqp/docs/latest-ga/api/org/springframework/amqp/rabbit/annotation/RabbitListenerConfigurer.html) for full details and examples.

The container factories provide methods for adding `MessagePostProcessor` instances that are applied after receiving messages (before invoking the listener) and before sending replies.

See <a href="#steeltoe-messaging-async-annotation-driven-reply"></a> for information about replies.

You can add a `RetryTemplate` and `RecoveryCallback` to the listener container factory.
It is used when sending replies.
The `RecoveryCallback` is invoked when retries are exhausted.
You can use a `SendRetryContextAccessor` to get information from the context.
The following example shows how to do so:

```Java
factory.setRetryTemplate(retryTemplate);
factory.setReplyRecoveryCallback(ctx -> {
    Message failed = SendRetryContextAccessor.getMessage(ctx);
    Address replyTo = SendRetryContextAccessor.getAddress(ctx);
    Throwable t = ctx.getLastThrowable();
    ...
    return null;
});
```

<a name="steeltoe-messaging-listener-property-overrides"></a>

The `@RabbitListener` annotation has a `concurrency` property.
It supports SpEL expressions (`#{...}`) and property placeholders (`${...}`).
Its meaning and allowed values depend on the container type, as follows:

* For the `DirectMessageListenerContainer`, the value must be a single integer value, which sets the `consumersPerQueue` property on the container.
* For the `SimpleRabbitListenerContainer`, the value can be a single integer value, which sets the `concurrentConsumers` property on the container, or it can have the form, `m-n`, where `m` is the `concurrentConsumers` property and `n` is the `maxConcurrentConsumers` property.

In either case, this setting overrides the settings on the factory.
Previously you had to define different container factories if you had listeners that required different concurrency.

The annotation also allows overriding the factory `autoStartup` and `taskExecutor` properties via the `autoStartup` and `executor` (since 2.2) annotation properties.
Using a different executor for each might help with identifying threads associated with each listener in logs and thread dumps.

Version 2.2 also added the `ackMode` property, which allows you to override the container factory's `acknowledgeMode` property.

```Java
@RabbitListener(id = "manual.acks.1", queues = "manual.acks.1", ackMode = "MANUAL")
public void manual1(String in, Channel channel,
    @Header(AmqpHeaders.DELIVERY_TAG) long tag) throws IOException {

    ...
    channel.basicAck(tag, false);
}
```

<a name="steeltoe-messaging-async-annotation-conversion"></a>
#### Message Conversion for Annotated Methods

There are two conversion steps in the pipeline before invoking the listener.
The first step uses a `MessageConverter` to convert the incoming Steeltoe RabbitMQ `Message` to a Spring-messaging `Message`.
When the target method is invoked, the message payload is converted, if necessary, to the method parameter type.

The default `MessageConverter` for the first step is a Steeltoe RabbitMQ `SimpleMessageConverter` that handles conversion to
`String` and `java.io.Serializable` objects.
All others remain as a `byte[]`.
In the following discussion, we call this the "message converter".

The default converter for the second step is a `GenericMessageConverter`, which delegates to a conversion service
(an instance of `DefaultFormattingConversionService`).
In the following discussion, we call this the "method argument converter".

To change the message converter, you can add it as a property to the container factory bean.
The following example shows how to do so:

```Java
@Bean
public SimpleRabbitListenerContainerFactory rabbitListenerContainerFactory() {
    SimpleRabbitListenerContainerFactory factory = new SimpleRabbitListenerContainerFactory();
    ...
    factory.setMessageConverter(new Jackson2JsonMessageConverter());
    ...
    return factory;
}
```

This example configures a Jackson2 converter that expects header information to be present to guide the conversion.

You can also use a `ContentTypeDelegatingMessageConverter`, which can handle conversion of different content types.

In most cases, it is not necessary to customize the method argument converter unless, for example, you want to use
a custom `ConversionService`.

If there are no type information headers, the type can be inferred from the target
method arguments.

>NOTE: This type inference works only for `@RabbitListener` at the method level.

See <a href="#steeltoe-messaging-json-message-converter"></a> for more information.

If you wish to customize the method argument converter, you can do so, as follows:

```Java
@Configuration
@EnableRabbit
public class AppConfig implements RabbitListenerConfigurer {

    ...

    @Bean
    public DefaultMessageHandlerMethodFactory myHandlerMethodFactory() {
        DefaultMessageHandlerMethodFactory factory = new DefaultMessageHandlerMethodFactory();
        factory.setMessageConverter(new GenericMessageConverter(myConversionService()));
        return factory;
    }

    @Bean
    public ConversionService myConversionService() {
        DefaultConversionService conv = new DefaultConversionService();
        conv.addConverter(mySpecialConverter());
        return conv;
    }

    @Override
    public void configureRabbitListeners(RabbitListenerEndpointRegistrar registrar) {
        registrar.setMessageHandlerMethodFactory(myHandlerMethodFactory());
    }

    ...

}
```

>IMPORTANT: For multi-method listeners (see <a href="#steeltoe-messaging-annotation-method-selection"></a>), the method selection is based on the payload of the message *after the message conversion*.
The method argument converter is called only after the method has been selected.


<a name="steeltoe-messaging-async-annotation-driven-registration"></a>
#### Programmatic Endpoint Registration

`RabbitListenerEndpoint` provides a model of a Rabbit endpoint and is responsible for configuring the container for that model.
The infrastructure lets you configure endpoints programmatically in addition to the ones that are detected by the `RabbitListener` annotation.
The following example shows how to do so:

```Java
@Configuration
@EnableRabbit
public class AppConfig implements RabbitListenerConfigurer {

    @Override
    public void configureRabbitListeners(RabbitListenerEndpointRegistrar registrar) {
        SimpleRabbitListenerEndpoint endpoint = new SimpleRabbitListenerEndpoint();
        endpoint.setQueueNames("anotherQueue");
        endpoint.setMessageListener(message -> {
            // processing
        });
        registrar.registerEndpoint(endpoint);
    }
}
```

In the preceding example, we used `SimpleRabbitListenerEndpoint`, which provides the actual `MessageListener` to invoke, but you could just as well build your own endpoint variant to describe a custom invocation mechanism.

It should be noted that you could just as well skip the use of `@RabbitListener` altogether and register your endpoints programmatically through `RabbitListenerConfigurer`.

<a name="async-annotation-driven-enable-signature"></a>
#### Annotated Endpoint Method Signature

So far, we have been injecting a simple `String` in our endpoint, but it can actually have a very flexible method signature.
The following example rewrites it to inject the `Order` with a custom header:

```Java
@Component
public class MyService {

    @RabbitListener(queues = "myQueue")
    public void processOrder(Order order, @Header("order_type") String orderType) {
        ...
    }
}
```

The following list shows the main elements you can inject in listener endpoints:

* The raw `org.springframework.amqp.core.Message`.
* The `com.rabbitmq.client.Channel` on which the message was received.
* The `org.springframework.messaging.Message` representing the incoming RabbitMQ message. Note that this message holds both the custom and the standard headers (as defined by `AmqpHeaders`).

>NOTE: The inbound `deliveryMode` header is now available in the header with a name of
`AmqpHeaders.RECEIVED_DELIVERY_MODE` instead of `AmqpHeaders.DELIVERY_MODE`.

* `@Header`-annotated method arguments to extract a specific header value, including standard AMQP headers.
* `@Headers`-annotated argument that must also be assignable to `java.util.Map` for getting access to all headers.

A non-annotated element that is not one of the supported types (that is,
`Message` and `Channel`) is considered to be the payload.
You can make that explicit by annotating the parameter with `@Payload`.
You can also turn on validation by adding an extra `@Valid`.

The ability to inject Spring’s message abstraction is particularly useful to benefit from all the information stored in the transport-specific message without relying on the transport-specific API.
The following example shows how to do so:


```Java
@RabbitListener(queues = "myQueue")
public void processOrder(Message<Order> order) { ...
}
```

Handling of method arguments is provided by `DefaultMessageHandlerMethodFactory`, which you can further customize to support additional method arguments.
The conversion and validation support can be customized there as well.

For instance, if we want to make sure our `Order` is valid before processing it, we can annotate the payload with `@Valid` and configure the necessary validator, as follows:

```Java
@Configuration
@EnableRabbit
public class AppConfig implements RabbitListenerConfigurer {

    @Override
    public void configureRabbitListeners(RabbitListenerEndpointRegistrar registrar) {
        registrar.setMessageHandlerMethodFactory(myHandlerMethodFactory());
    }

    @Bean
    public DefaultMessageHandlerMethodFactory myHandlerMethodFactory() {
        DefaultMessageHandlerMethodFactory factory = new DefaultMessageHandlerMethodFactory();
        factory.setValidator(myValidator());
        return factory;
    }
}
```

<a name="steeltoe-messaging-annotation-multiple-queues"></a>
#### Listening to Multiple Queues

When you use the `queues` attribute, you can specify that the associated container can listen to multiple queues.
You can use a `@Header` annotation to make the queue name from which a message was received available to the POJO
method.
The following example shows how to do so:

```Java
@Component
public class MyService {

    @RabbitListener(queues = { "queue1", "queue2" } )
    public void processOrder(String data, @Header(AmqpHeaders.CONSUMER_QUEUE) String queue) {
        ...
    }

}
```

You can externalize the queue names by using property placeholders and SpEL.
The following example shows how to do so:

```Java
@Component
public class MyService {

    @RabbitListener(queues = "#{'${property.with.comma.delimited.queue.names}'.split(',')}" )
    public void processOrder(String data, @Header(AmqpHeaders.CONSUMER_QUEUE) String queue) {
        ...
    }

}
```

Prior to version 1.5, only a single queue could be specified this way.
Each queue needed a separate property.

<a name="steeltoe-messaging-async-annotation-driven-reply"></a>
#### Reply Management

The existing support in `MessageListenerAdapter` already lets your method have a non-void return type.
When that is the case, the result of the invocation is encapsulated in a message sent to the the address specified in the `ReplyToAddress` header of the original message, or to the default address configured on the listener.
You can set that default address by using the `@SendTo` annotation of the messaging abstraction.

Assuming our `processOrder` method should now return an `OrderStatus`, we can write it as follows to automatically send a reply:

```Java
@RabbitListener(destination = "myQueue")
@SendTo("status")
public OrderStatus processOrder(Order order) {
    // order processing
    return status;
}
```

If you need to set additional headers in a transport-independent manner, you could return a `Message` instead, something like the following:

```Java
@RabbitListener(destination = "myQueue")
@SendTo("status")
public Message<OrderStatus> processOrder(Order order) {
    // order processing
    return MessageBuilder
        .withPayload(status)
        .setHeader("code", 1234)
        .build();
}
```

Alternatively, you can use a `MessagePostProcessor` in the `beforeSendReplyMessagePostProcessors` container factory property to add more headers.
The called bean/method is made available in the reply message, which can be used in a message post processor to communicate the information back to the caller:

```Java
factory.setBeforeSendReplyPostProcessors(msg -> {
    msg.getMessageProperties().setHeader("calledBean",
            msg.getMessageProperties().getTargetBean().getClass().getSimpleName());
    msg.getMessageProperties().setHeader("calledMethod",
            msg.getMessageProperties().getTargetMethod().getName());
    return m;
});
```

You can configure a `ReplyPostProcessor` to modify the reply message before it is sent; it is called after the `correlationId` header has been set up to match the request.

```Java
@RabbitListener(queues = "test.header", group = "testGroup", replyPostProcessor = "echoCustomHeader")
public String capitalizeWithHeader(String in) {
    return in.toUpperCase();
}

@Bean
public ReplyPostProcessor echoCustomHeader() {
    return (req, resp) -> {
        resp.getMessageProperties().setHeader("myHeader", req.getMessageProperties().getHeader("myHeader"));
        return resp;
    };
}
```

The `@SendTo` value is assumed as a reply `exchange` and `routingKey` pair that follws the `exchange/routingKey` pattern,
where one of those parts can be omitted.
The valid values are as follows:

* `thing1/thing2`: The `replyTo` exchange and the `routingKey`.
* `thing1/`: The `replyTo` exchange and the default (empty) `routingKey`.
* `thing2` or `/thing2`: The `replyTo` `routingKey` and the default (empty) exchange.
* `/` or empty: The `replyTo` default exchange and the default `routingKey`.

Also, you can use `@SendTo` without a `value` attribute.
This case is equal to an empty `sendTo` pattern.
`@SendTo` is used only if the inbound message does not have a `replyToAddress` property.

The `@SendTo` value can be a bean initialization SpEL Expression, as shown in the following example:


```Java
@RabbitListener(queues = "test.sendTo.spel")
@SendTo("#{spelReplyTo}")
public String capitalizeWithSendToSpel(String foo) {
    return foo.toUpperCase();
}
...
@Bean
public String spelReplyTo() {
    return "test.sendTo.reply.spel";
}
```

The expression must evaluate to a `String`, which can be a simple queue name (sent to the default exchange) or with
the form `exchange/routingKey` as discussed prior to the preceding example.

>NOTE: The `#{...}` expression is evaluated once, during initialization.

For dynamic reply routing, the message sender should include a `reply_to` message property or use the alternate
runtime SpEL expression (described after the next example).

The `@SendTo` can be a SpEL expression that is evaluated at runtime against the request
and reply, as the following example shows:

```Java
@RabbitListener(queues = "test.sendTo.spel")
@SendTo("!{'some.reply.queue.with.' + result.queueName}")
public Bar capitalizeWithSendToSpel(Foo foo) {
    return processTheFooAndReturnABar(foo);
}
```

The runtime nature of the SpEL expression is indicated with `!{...}` delimiters.
The evaluation context `#root` object for the expression has three properties:

* `request`: The `o.s.amqp.core.Message` request object.
* `source`: The `o.s.messaging.Message<?>` after conversion.
* `result`: The method result.

The context has a map property accessor, a standard type converter, and a bean resolver, which lets other beans in the
context be referenced (for example, `@someBeanName.determineReplyQ(request, result)`).

In summary, `#{...}` is evaluated once during initialization, with the `#root` object being the application context.
Beans are referenced by their names.
`!{...}` is evaluated at runtime for each message, with the root object having the properties listed earlier.
Beans are referenced with their names, prefixed by `@`.

Simple property placeholders are also supported (for example, `${some.reply.to}`).
With earlier versions, the following can be used as a work around, as the following example shows:


```Java
@RabbitListener(queues = "foo")
@SendTo("#{environment['my.send.to']}")
public String listen(Message in) {
    ...
    return ...
}
```

<a name="steeltoe-messaging-annotation-method-selection"></a>
#### Multi-method Listeners

You can specify the `@RabbitListener` annotation at the class level.
Together with the new `@RabbitHandler` annotation, this lets a single listener invoke different methods, based on
the payload type of the incoming message.
This is best described using an example:

```Java
@RabbitListener(id="multi", queues = "someQueue")
@SendTo("my.reply.queue")
public class MultiListenerBean {

    @RabbitHandler
    public String thing2(Thing2 thing2) {
        ...
    }

    @RabbitHandler
    public String cat(Cat cat) {
        ...
    }

    @RabbitHandler
    public String hat(@Header("amqp_receivedRoutingKey") String rk, @Payload Hat hat) {
        ...
    }

    @RabbitHandler(isDefault = true)
    public String defaultMethod(Object object) {
        ...
    }

}
```

In this case, the individual `@RabbitHandler` methods are invoked if the converted payload is a `Thing2`, a `Cat`, or a `Hat`.
You should understand that the system must be able to identify a unique method based on the payload type.
The type is checked for assignability to a single parameter that has no annotations or that is annotated with the `@Payload` annotation.
Notice that the same method signatures apply, as discussed in the method-level `@RabbitListener` (<a href="#steeltoe-messaging-message-listener-adapter">described earlier</a>).

A `@RabbitHandler` method can be designated as the default method, which is invoked if there is no match on other methods.
At most, one method can be so designated.

>IMPORTANT: `@RabbitHandler` is intended only for processing message payloads after conversion, if you wish to receive the unconverted raw `Message` object, you must use `@RabbitListener` on the method, not the class.

<a name="steeltoe-messaging-repeatable-rabbit-listener"></a>
#### Using a `@Repeatable` `@RabbitListener`

The `@RabbitListener` annotation is marked with `@Repeatable`.
This means that the annotation can appear on the same annotated element (method or class) multiple times.
In this case, a separate listener container is created for each annotation, each of which invokes the same listener
`@Bean`.
Repeatable annotations can be used with Java 8 or above.
When using Java 7 or earlier, you can achieve the same effect by using the `@RabbitListeners` "container" annotation, with an array of `@RabbitListener` annotations.

#### Proxy `@RabbitListener` and Generics

If your service is intended to be proxied (for example, in the case of `@Transactional`), you should keep in mind some considerations when
the interface has generic parameters.
Consider the following example:

```Java
interface TxService<P> {

   String handle(P payload, String header);

}

static class TxServiceImpl implements TxService<Foo> {

    @Override
    @RabbitListener(...)
    public String handle(Foo foo, String rk) {
         ...
    }

}
```

With a generic interface and a particular implementation, you are forced to switch to the CGLIB target class proxy because the actual implementation of the interface
`handle` method is a bridge method.
In the case of transaction management, the use of CGLIB is configured by using
an annotation option: `@EnableTransactionManagement(proxyTargetClass = true)`.
And in this case, all annotations have to be declared on the target method in the implementation, as follows:

```Java
static class TxServiceImpl implements TxService<Foo> {

    @Override
    @Transactional
    @RabbitListener(...)
    public String handle(@Payload Foo foo, @Header("amqp_receivedRoutingKey") String rk) {
        ...
    }

}
```

<a name="steeltoe-messaging-annotation-error-handling"></a>
#### Handling Exceptions

By default, if an annotated listener method throws an exception, it is thrown to the container and the message are requeued and redelivered, discarded, or routed to a dead letter exchange, depending on the container and broker configuration.
Nothing is returned to the sender.

The `@RabbitListener` annotation has two new attributes: `errorHandler` and `returnExceptions`.

These are not configured by default.

You can use the `errorHandler` to provide the bean name of a `RabbitListenerErrorHandler` implementation.
This functional interface has one method, as follows:

```Java
@FunctionalInterface
public interface RabbitListenerErrorHandler {

    Object handleError(Message amqpMessage, org.springframework.messaging.Message<?> message,
              ListenerExecutionFailedException exception) throws Exception;

}
```

As you can see, you have access to the raw message received from the container, the spring-messaging `Message<?>` object produced by the message converter, and the exception that was thrown by the listener (wrapped in a `ListenerExecutionFailedException`).
The error handler can either return some result (which is sent as the reply) or throw the original or a new exception (which is thrown to the container or returned to the sender, depending on the `returnExceptions` setting).

The `returnExceptions` attribute, when `true`, causes exceptions to be returned to the sender.
The exception is wrapped in a `RemoteInvocationResult` object.
On the sender side, there is an available `RemoteInvocationAwareMessageConverterAdapter`, which, if configured into the `RabbitTemplate`, re-throws the server-side exception, wrapped in an `AmqpRemoteException`.
The stack trace of the server exception is synthesized by merging the server and client stack traces.

>IMPORTANT: This mechanism generally works only with the default `SimpleMessageConverter`, which uses Java serialization.
Exceptions are generally not "Jackson-friendly" and cannot be serialized to JSON.
If you use JSON, consider using an `errorHandler` to return some other Jackson-friendly `Error` object when an exception is thrown.

The `Channel` is available in a messaging message header; this allows you to ack or nack the failed messasge when using `AcknowledgeMode.MANUAL`:

```Java
public Object handleError(Message amqpMessage, org.springframework.messaging.Message<?> message,
          ListenerExecutionFailedException exception) {
              ...
              message.getHeaders().get(AmqpHeaders.CHANNEL, Channel.class)
                  .basicReject(message.getHeaders().get(AmqpHeaders.DELIVERY_TAG, Long.class),
                               true);
          }
```

#### Container Management

Containers created for annotations are not registered with the application context.
You can obtain a collection of all containers by invoking `getListenerContainers()` on the
`RabbitListenerEndpointRegistry` bean.
You can then iterate over this collection, for example, to stop or start all containers or invoke the `Lifecycle` methods
on the registry itself, which will invoke the operations on each container.

You can also get a reference to an individual container by using its `id`, using `getListenerContainer(String id)` &#151; for
example, `registry.getListenerContainer("multi")` for the container created by the snippet above.

You can obtain the `id` values of the registered containers with `getListenerContainerIds()`.

You can now assign a `group` to the container on the `RabbitListener` endpoint.
This provides a mechanism to get a reference to a subset of containers.
Adding a `group` attribute causes a bean of type `Collection<MessageListenerContainer>` to be registered with the context with the group name.

<a name="steeltoe-messaging-receiving-batch"></a>
### @RabbitListener with Batching

When receiving a <a href="steeltoe-messaging-template-batching">a batch</a> of messages, the de-batching is normally performed by the container and the listener is invoked with one message at at time.
You can configure the listener container factory and listener to receive the entire batch in one call, simply set the factory's `batchListener` property, and make the method payload parameter a `List`:

```Java
@Bean
public SimpleRabbitListenerContainerFactory rabbitListenerContainerFactory() {
    SimpleRabbitListenerContainerFactory factory = new SimpleRabbitListenerContainerFactory();
    factory.setConnectionFactory(connectionFactory());
    factory.setBatchListener(true);
    return factory;
}

@RabbitListener(queues = "batch.1")
public void listen1(List<Thing> in) {
    ...
}

// or

@RabbitListener(queues = "batch.2")
public void listen2(List<Message<Thing>> in) {
    ...
}
```

Setting the `batchListener` property to true automatically turns off the `deBatchingEnabled` container property in containers that the factory creates (unless `consumerBatchEnabled` is `true` - see below). Effectively, the debatching is moved from the container to the listener adapter and the adapter creates the list that is passed to the listener.

A batch-enabled factory cannot be used with a <a href="#steeltoe-messaging-annotation-method-selection">multi-method listener</a>.

Also, when receiving batched messages one-at-a-time, the last message contains a boolean header set to `true`.
This header can be obtained by adding the `@Header(AmqpHeaders.LAST_IN_BATCH)` boolean last parameter to your listener method.
The header is mapped from `MessageProperties.isLastInBatch()`.
In addition, `AmqpHeaders.BATCH_SIZE` is populated with the size of the batch in every message fragment.

In addition, a new property `consumerBatchEnabled` has been added to the `SimpleMessageListenerContainer`.
When this is true, the container will create a batch of messages, up to `batchSize`; a partial batch is delivered if `receiveTimeout` elapses with no new messages arriving.
If a producer-created batch is received, it is debatched and added to the consumer-side batch; therefore the actual number of messages delivered may exceed `batchSize`, which represents the number of messages received from the broker.
`deBatchingEnabled` must be true when `consumerBatchEnabled` is true; the container factory will enforce this requirement.

```Java
@Bean
public SimpleRabbitListenerContainerFactory consumerBatchContainerFactory() {
    SimpleRabbitListenerContainerFactory factory = new SimpleRabbitListenerContainerFactory();
    factory.setConnectionFactory(rabbitConnectionFactory());
    factory.setConsumerTagStrategy(consumerTagStrategy());
    factory.setBatchListener(true); // configures a BatchMessageListenerAdapter
    factory.setBatchSize(2);
    factory.setConsumerBatchEnabled(true);
    return factory;
}
```

When using `consumerBatchEnabled` with `@RabbitListener`:

```Java
@RabbitListener(queues = "batch.1", containerFactory = "consumerBatchContainerFactory")
public void consumerBatch1(List<Message> amqpMessages) {
    this.amqpMessagesReceived = amqpMessages;
    this.batch1Latch.countDown();
}

@RabbitListener(queues = "batch.2", containerFactory = "consumerBatchContainerFactory")
public void consumerBatch2(List<org.springframework.messaging.Message<Invoice>> messages) {
    this.messagingMessagesReceived = messages;
    this.batch2Latch.countDown();
}

@RabbitListener(queues = "batch.3", containerFactory = "consumerBatchContainerFactory")
public void consumerBatch3(List<Invoice> strings) {
    this.batch3Strings = strings;
    this.batch3Latch.countDown();
}
```

* The first is called with the raw, unconverted `org.springframework.amqp.core.Message` s received.
* The second is called with the `org.springframework.messaging.Message<?>` s with converted payloads and mapped headers/properties.
* The third is called with the converted payloads, with no access to headers/properteis.

You can also add a `Channel` parameter, often used when using `MANUAL` ack mode.
This is not very useful with the third example because you don't have access to the `delivery_tag` property.

<a name="steeltoe-messaging-using-container-factories"></a>
### Using Container Factories

Listener container factories were introduced to support the `@RabbitListener` and registering containers with the `RabbitListenerEndpointRegistry`, as discussed in <a href="#steeltoe-messaging-async-annotation-driven-registration"></a>.

They can be used to create any listener container &#151; even a container without a listener (such as for use in Spring Integration).
Of course, a listener must be added before the container is started.

There are two ways to create such containers:

* Use a SimpleRabbitListenerEndpoint
* Add the listener after creation

The following example uses a `SimpleRabbitListenerEndpoint` to create a listener container:

```Java
@Bean
public SimpleMessageListenerContainer factoryCreatedContainerSimpleListener(
        SimpleRabbitListenerContainerFactory rabbitListenerContainerFactory) {
    SimpleRabbitListenerEndpoint endpoint = new SimpleRabbitListenerEndpoint();
    endpoint.setQueueNames("queue.1");
    endpoint.setMessageListener(message -> {
        ...
    });
    return rabbitListenerContainerFactory.createListenerContainer(endpoint);
}
```

The following example adds the listener after creation:

```Java
@Bean
public SimpleMessageListenerContainer factoryCreatedContainerNoListener(
        SimpleRabbitListenerContainerFactory rabbitListenerContainerFactory) {
    SimpleMessageListenerContainer container = rabbitListenerContainerFactory.createListenerContainer();
    container.setMessageListener(message -> {
        ...
    });
    container.setQueueNames("test.no.listener.yet");
    return container;
}
```

In either case, the listener can also be a `ChannelAwareMessageListener`, since it is now a sub-interface of `MessageListener`.

These techniques are useful if you wish to create several containers with similar properties or use a pre-configured container factory such as the one provided by Spring Boot auto configuration or both.

>IMPORTANT: Containers created this way are normal `@Bean` instances and are not registered in the `RabbitListenerEndpointRegistry`.

<a name="steeltoe-messaging-async-returns"></a>
### Asynchronous `@RabbitListener` Return Types

`@RabbitListener` (and `@RabbitHandler`) methods can be specified with asynchronous return types `ListenableFuture<?>` and `Mono<?>`, letting the reply be sent asynchronously.

>IMPORTANT: The listener container factory must be configured with `AcknowledgeMode.MANUAL` so that the consumer thread will not ack the message; instead, the asynchronous completion will ack or nack the message when the async operation completes.
When the async result is completed with an error, whether the message is requeued or not depends on the exception type thrown, the container configuration, and the container error handler.
By default, the message will be requeued, unless the container's `defaultRequeueRejected` property is set to `false` (it is `true` by default).
If the async result is completed with an `AmqpRejectAndDontRequeueException`, the message will not be requeued.
If the container's `defaultRequeueRejected` property is `false`, you can override that by setting the future's exception to a `ImmediateRequeueException` and the message will be requeued.
If some exception occurs within the listener method that prevents creation of the async result object, you MUST catch that exception and return an appropriate return object that will cause the message to be acknowledged or requeued.

<a name="steeltoe-messaging-threading"></a>
### Threading and Asynchronous Consumers

A number of different threads are involved with asynchronous consumers.

Threads from the `TaskExecutor` configured in the `SimpleMessageListenerContainer` are used to invoke the `MessageListener` when a new message is delivered by `RabbitMQ Client`.
If not configured, a `SimpleAsyncTaskExecutor` is used.
If you use a pooled executor, you need to ensure the pool size is sufficient to handle the configured concurrency.
With the `DirectMessageListenerContainer`, the `MessageListener` is invoked directly on a `RabbitMQ Client` thread.
In this case, the `taskExecutor` is used for the task that monitors the consumers.

>NOTE: When using the default `SimpleAsyncTaskExecutor`, for the threads the listener is invoked on, the listener container `beanName` is used in the `threadNamePrefix`.
This is useful for log analysis.
We generally recommend always including the thread name in the logging appender configuration.
When a `TaskExecutor` is specifically provided through the `taskExecutor` property on the container, it is used as is, without modification.
It is recommended that you use a similar technique to name the threads created by a custom `TaskExecutor` bean definition, to aid with thread identification in log messages.

The `Executor` configured in the `CachingConnectionFactory` is passed into the `RabbitMQ Client` when creating the connection, and its threads are used to deliver new messages to the listener container.
If this is not configured, the client uses an internal thread pool executor with a pool size of five.

>IMPORTANT: With the `DirectMessageListenerContainer`, you need to ensure that the connection factory is configured with a task executor that had sufficient threads to support your desired concurrency across all listener containers that use that factory.
The default pool size is only five.

The `RabbitMQ client` uses a `ThreadFactory` to create threads for low-level I/O (socket) operations.
To modify this factory, you need to configure the underlying RabbitMQ `ConnectionFactory`, as discussed in <a href="#steeltoe-messaging-connection-factory"></a>.

<a name="steeltoe-messaging-choose-container"></a>
### Choosing a Container

Version 2.0 introduced the `DirectMessageListenerContainer` (DMLC).
Previously, only the `SimpleMessageListenerContainer` (SMLC) was available.
The SMLC uses an internal queue and a dedicated thread for each consumer.
If a container is configured to listen to multiple queues, the same consumer thread is used to process all the queues.
Concurrency is controlled by `concurrentConsumers` and other properties.
As messages arrive from the RabbitMQ client, the client thread hands them off to the consumer thread through the queue.
This architecture was required because, in early versions of the RabbitMQ client, multiple concurrent deliveries were not possible.
Newer versions of the client have a revised threading model and can now support concurrency.
This has allowed the introduction of the DMLC where the listener is now invoked directly on the RabbitMQ Client thread.
Its architecture is, therefore, actually "simpler" than the SMLC.
However, there are some limitations with this approach, and certain features of the SMLC are not available with the DMLC.
Also, concurrency is controlled by `consumersPerQueue` (and the client library's thread pool).
The `concurrentConsumers` and associated properties are not available with this container.

The following features are available with the SMLC but not the DMLC:

* `batchSize`: With the SMLC, you can set this to control how many messages are delivered in a transaction or to reduce the number of acks, but it may cause the number of duplicate deliveries to increase after a failure. (The DMLC does have `messagesPerAck`, which you can use to reduce the acks, the same as with `batchSize` and the SMLC, but it cannot be used with transactions &#151; each message is delivered and ack'd in a separate transaction).
* `consumerBatchEnabled`: enables batching of discrete messages in the consumer. See <a href="#steeltoe-messaging-container-attributes"></a> for more information.
* `maxConcurrentConsumers` and consumer scaling intervals or triggers &#151; there is no auto-scaling in the DMLC. It does, however, let you programmatically change the `consumersPerQueue` property and the consumers are adjusted accordingly.

However, the DMLC has the following benefits over the SMLC:

* Adding and removing queues at runtime is more efficient. With the SMLC, the entire consumer thread is restarted (all consumers canceled and re-created). With the DMLC, unaffected consumers are not canceled.
* The context switch between the RabbitMQ Client thread and the consumer thread is avoided.
* Threads are shared across consumers rather than having a dedicated thread for each consumer in the SMLC. However, see the IMPORTANT note about the connection factory configuration in <a href="#steeltoe-messaging-threading"></a>.

See <a href="#steeltoe-messaging-container-attributes"></a> for information about which configuration properties apply to each container.

<a name="steeltoe-messaging-idle-containers"></a>
### Detecting Idle Asynchronous Consumers

While efficient, one problem with asynchronous consumers is detecting when they are idle &#151; users might want to take
some action if no messages arrive for some period of time.

It is now possible to configure the listener container to publish a
`ListenerContainerIdleEvent` when some time passes with no message delivery.
While the container is idle, an event is published every `idleEventInterval` milliseconds.

To configure this feature, set `idleEventInterval` on the container.
The following example shows how to do sofor both a `SimpleMessageListenerContainer` and a `SimpleRabbitListenerContainerFactory`:

```Java
@Bean
public SimpleMessageListenerContainer(ConnectionFactory connectionFactory) {
    SimpleMessageListenerContainer container = new SimpleMessageListenerContainer(connectionFactory);
    ...
    container.setIdleEventInterval(60000L);
    ...
    return container;
}
@Bean
public SimpleRabbitListenerContainerFactory rabbitListenerContainerFactory() {
    SimpleRabbitListenerContainerFactory factory = new SimpleRabbitListenerContainerFactory();
    factory.setConnectionFactory(rabbitConnectionFactory());
    factory.setIdleEventInterval(60000L);
    ...
    return factory;
}
```

In each of these cases, an event is published once per minute while the container is idle.

#### Event Consumption

You can capture idle events by implementing `ApplicationListener` &#151; either a general listener or one narrowed to
receive only this specific event.
You can also use `@EventListener`, introduced in Spring Framework 4.2.

The following example combines the `@RabbitListener` and `@EventListener` into a single class.
You need to understand that the application listener gets events for all containers, so you may need to
check the listener ID if you want to take specific action based on which container is idle.
You can also use the `@EventListener` `condition` for this purpose.

The events have four properties:

* `source`: The listener container instance
* `id`: The listener ID (or container bean name)
* `idleTime`: The time the container had been idle when the event was published
* `queueNames`: The names of the queue(s) that the container listens to

The following example shows how to create listeners by using both the `@RabbitListener` and the `@EventListener` annotations:

```Java
public class Listener {

    @RabbitListener(id="someId", queues="#{queue.name}")
    public String listen(String foo) {
        return foo.toUpperCase();
    }

    @EventListener(condition = "event.listenerId == 'someId'")
    public void onApplicationEvent(ListenerContainerIdleEvent event) {
        ...
    }

}
```

>IMPORTANT: Event listeners see events for all containers.
Consequently, in the preceding example, we narrow the events received based on the listener ID.

>CAUTION: If you wish to use the idle event to stop the lister container, you should not call `container.stop()` on the thread that calls the listener.
Doing so always causes delays and unnecessary log messages.
Instead, you should hand off the event to a different thread that can then stop the container.

<a name="steeltoe-messaging-micrometer"></a>
### Monitoring Listener Performance

The listener containers automatically create and update Micrometer `Timer` instances for the listener, if `Micrometer` is detected on the class path, and a `MeterRegistry` is present in the application context.
The timers can be disabled by setting the container property `micrometerEnabled` to `false`.

Two timers are maintained - one for successful calls to the listener and one for failures.
With a simple `MessageListener`, there is a pair of timers for each configured queue.

The timers are named `spring.rabbitmq.listener` and have the following tags:

* `listenerId` : (listener id or container bean name)
* `queue` : (the queue name for a simple listener or list of configured queue names when `consumerBatchEnabled` is `true` - because a batch may contain messages from multiple queues)
* `result` : `success` or `failure`
* `exception` : `none` or `ListenerExecutionFailedException`

You can add additional tags using the `micrometerTags` container property.

<a name="steeltoe-messaging-containers-and-broker-named-queues"></a>
## Containers and Broker-Named queues

While it is preferable to use `AnonymousQueue` instances as auto-delete queues, you can use broker named queues with listener containers.
The following example shows how to do so:

```Java
@Bean
public Queue queue() {
    return new Queue("", false, true, true);
}

@Bean
public SimpleMessageListenerContainer container() {
    SimpleMessageListenerContainer container = new SimpleMessageListenerContainer(cf());
    container.setQueues(queue());
    container.setMessageListener(m -> {
        ...
    });
    container.setMissingQueuesFatal(false);
    return container;
}
```

Notice the empty `String` for the name.
When the `RabbitAdmin` declares queues, it updates the `Queue.actualName` property with the name returned by the broker.
You must use `setQueues()` when you configure the container for this to work, so that the container can access the declared name at runtime.
Just setting the names is insufficient.

>NOTE: You cannot add broker-named queues to the containers while they are running.

>IMPORTANT: When a connection is reset and a new one is established, the new queue gets a new name.
Since there is a race condition between the container restarting and the queue being re-declared, it is important to set the container's `missingQueuesFatal` property to `false`, since the container is likely to initially try to reconnect to the old queue.

<a name="steeltoe-messaging-message-converters"></a>
## Message Converters

The `AmqpTemplate` also defines several methods for sending and receiving messages that delegate to a `MessageConverter`.
The `MessageConverter` provides a single method for each direction: one for converting *to* a `Message` and another for converting *from* a `Message`.
Notice that, when converting to a `Message`, you can also provide properties in addition to the object.
The `object` parameter typically corresponds to the Message body.
The following listing shows the `MessageConverter` interface definition:

```Java
public interface MessageConverter {

    Message toMessage(Object object, MessageProperties messageProperties)
            throws MessageConversionException;

    Object fromMessage(Message message) throws MessageConversionException;

}
```

The relevant `Message`-sending methods on the `AmqpTemplate` are simpler than the methods we discussed previously, because they do not require the `Message` instance.
Instead, the `MessageConverter` is responsible for "creating" each `Message` by converting the provided object to the byte array for the `Message` body and then adding any provided `MessageProperties`.
The following listing shows the definitions of the various methods:

```Java
void convertAndSend(Object message) throws AmqpException;

void convertAndSend(String routingKey, Object message) throws AmqpException;

void convertAndSend(String exchange, String routingKey, Object message)
    throws AmqpException;

void convertAndSend(Object message, MessagePostProcessor messagePostProcessor)
    throws AmqpException;

void convertAndSend(String routingKey, Object message,
    MessagePostProcessor messagePostProcessor) throws AmqpException;

void convertAndSend(String exchange, String routingKey, Object message,
    MessagePostProcessor messagePostProcessor) throws AmqpException;
```

On the receiving side, there are only two methods: one that accepts the queue name and one that relies on the template's "queue" property having been set.
The following listing shows the definitions of the two methods:

```ava
Object receiveAndConvert() throws AmqpException;

Object receiveAndConvert(String queueName) throws AmqpException;
```

>NOTE: The `MessageListenerAdapter` mentioned in <a href="#steeltoe-messaging-async-consumer"></a> also uses a `MessageConverter`.

<a name="steeltoe-messaging-simple-message-converter"></a>
### `SimpleMessageConverter`

The default implementation of the `MessageConverter` strategy is called `SimpleMessageConverter`.
This is the converter that is used by an instance of `RabbitTemplate` if you do not explicitly configure an alternative.
It handles text-based content, serialized Java objects, and byte arrays.

#### Converting From a `Message`

If the content type of the input `Message` begins with "text" (for example,
"text/plain"), it also checks for the content-encoding property to determine the charset to be used when converting the `Message` body byte array to a Java `String`.
If no content-encoding property had been set on the input `Message`, it uses the UTF-8 charset by default.
If you need to override that default setting, you can configure an instance of `SimpleMessageConverter`, set its `defaultCharset` property, and inject that into a `RabbitTemplate` instance.

If the content-type property value of the input `Message` is set to "application/x-java-serialized-object", the `SimpleMessageConverter` tries to deserialize (rehydrate) the byte array into a Java object.
While that might be useful for simple prototyping, we do not recommend relying on Java serialization, since it leads to tight coupling between the producer and the consumer.
Of course, it also rules out usage of non-Java systems on either side.
With AMQP being a wire-level protocol, it would be unfortunate to lose much of that advantage with such restrictions.
In the next two sections, we explore some alternatives for passing rich domain object content without relying on Java serialization.

For all other content-types, the `SimpleMessageConverter` returns the `Message` body content directly as a byte array.

See <a href="#steeltoe-messaging-java-deserialization"></a> for important information.

#### Converting To a `Message`

When converting to a `Message` from an arbitrary Java Object, the `SimpleMessageConverter` likewise deals with byte arrays, strings, and serializable instances.
It converts each of these to bytes (in the case of byte arrays, there is nothing to convert), and it ses the content-type property accordingly.
If the `Object` to be converted does not match one of those types, the `Message` body is null.

<a name="steeltoe-messaging-serializer-message-converter"></a>
### `SerializerMessageConverter`

This converter is similar to the `SimpleMessageConverter` except that it can be configured with other Spring Framework
`Serializer` and `Deserializer` implementations for `application/x-java-serialized-object` conversions.

See <a href="#steeltoe-messaging-java-deserialization"></a> for important information.

<a name="steeltoe-messaging-json-message-converter"></a>
### Jackson2JsonMessageConverter

This section covers using the `Jackson2JsonMessageConverter` to convert to and from a `Message`.
It has the following sections:

* <a href="#steeltoe-messaging-Jackson2JsonMessageConverter-to-message"></a>
* <a href="#steeltoe-messaging-Jackson2JsonMessageConverter-from-message"></a>

<a name="steeltoe-messaging-Jackson2JsonMessageConverter-to-message"></a>
#### Converting to a `Message`

As mentioned in the previous section, relying on Java serialization is generally not recommended.
One rather common alternative that is more flexible and portable across different languages and platforms is JSON
(JavaScript Object Notation).
The converter can be configured on any `RabbitTemplate` instance to override its usage of the `SimpleMessageConverter`
default.
The `Jackson2JsonMessageConverter` uses the `com.fasterxml.jackson` 2.x library.
The following example configures a `Jackson2JsonMessageConverter`:

[//]: # (There was an XML example here.)

As shown above, `Jackson2JsonMessageConverter` uses a `DefaultClassMapper` by default.
Type information is added to (and retrieved from) `MessageProperties`.
If an inbound message does not contain type information in `MessageProperties`, but you know the expected type, you
can configure a static type by using the `defaultType` property, as the following example shows:

[//]: # (There was an XML example here.)

In addition, you can provide custom mappings from the value in the `__TypeId__` header.
The following example shows how to do so:

```Java
@Bean
public Jackson2JsonMessageConverter jsonMessageConverter() {
    Jackson2JsonMessageConverter jsonConverter = new Jackson2JsonMessageConverter();
    jsonConverter.setClassMapper(classMapper());
    return jsonConverter;
}

@Bean
public DefaultClassMapper classMapper() {
    DefaultClassMapper classMapper = new DefaultClassMapper();
    Map<String, Class<?>> idClassMapping = new HashMap<>();
    idClassMapping.put("thing1", Thing1.class);
    idClassMapping.put("thing2", Thing2.class);
    classMapper.setIdClassMapping(idClassMapping);
    return classMapper;
}
```

Now, if the sending system sets the header to `thing1`, the converter creates a `Thing1` object, and so on.

<a name="steeltoe-messaging-Jackson2JsonMessageConverter-from-message"></a>
#### Converting from a `Message`

Inbound messages are converted to objects according to the type information added to headers by the sending system.

If type information is missing, the converter converts the JSON by using Jackson defaults (usually a map).

Also, when you use `@RabbitListener` annotations (on methods), the inferred type information is added to the `MessageProperties`.
This lets the converter convert to the argument type of the target method.
This applies only if there is one parameter with no annotations or a single parameter with the `@Payload` annotation.
Parameters of type `Message` are ignored during the analysis.

>IMPORTANT: By default, the inferred type information will override the inbound `__TypeId__` and related headers created
by the sending system.
This lets the receiving system automatically convert to a different domain object.
This applies only if the parameter type is concrete (not abstract or an interface) or it is from the `java.util`
package.
In all other cases, the `__TypeId__` and related headers is used.
There are cases where you might wish to override the default behavior and always use the `__TypeId__` information.
For example, suppose you have a `@RabbitListener` that takes a `Thing1` argument but the message contains a `Thing2` that
is a subclass of `Thing1` (which is concrete).
The inferred type would be incorrect.
To handle this situation, set the `TypePrecedence` property on the `Jackson2JsonMessageConverter` to `TYPE_ID` instead
of the default `INFERRED`.
(The property is actually on the converter's `DefaultJackson2JavaTypeMapper`, but a setter is provided on the converter
for convenience.)
If you inject a custom type mapper, you should set the property on the mapper instead.

>NOTE: When converting from the `Message`, an incoming `MessageProperties.getContentType()` must be JSON-compliant (`contentType.contains("json")` is used to check).
`application/json` is assumed if there is no `contentType` property, or it has the default value `application/octet-stream`.
To revert to the previous behavior (return an unconverted `byte[]`), set the converter's `assumeSupportedContentType` property to `false`.
If the content type is not supported, a `WARN` log message `Could not convert incoming message with content-type [...]`, is emitted and `message.getBody()` is returned as is &#151; as a `byte[]`.
So, to meet the `Jackson2JsonMessageConverter` requirements on the consumer side, the producer must add the `contentType` message property &#151; for example, as `application/json` or `text/x-json` or by using the `Jackson2JsonMessageConverter`, which sets the header automatically.
The following listing shows a number of converter calls:

```Java
@RabbitListener
public void thing1(Thing1 thing1) {...}

@RabbitListener
public void thing1(@Payload Thing1 thing1, @Header("amqp_consumerQueue") String queue) {...}

@RabbitListener
public void thing1(Thing1 thing1, o.s.amqp.core.Message message) {...}

@RabbitListener
public void thing1(Thing1 thing1, o.s.messaging.Message<Foo> message) {...}

@RabbitListener
public void thing1(Thing1 thing1, String bar) {...}

@RabbitListener
public void thing1(Thing1 thing1, o.s.messaging.Message<?> message) {...}
```

In the first four cases in the preceding listing, the converter tries to convert to the `Thing1` type.
The fifth example is invalid because we cannot determine which argument should receive the message payload.
With the sixth example, the Jackson defaults apply due to the generic type being a `WildcardType`.

You can, however, create a custom converter and use the `targetMethod` message property to decide which type to convert
the JSON to.

>NOTE: This type inference can be achieved only when the `@RabbitListener` annotation is declared at the method level.
With class-level `@RabbitListener`, the converted type is used to select which `@RabbitHandler` method to invoke.
For this reason, the infrastructure provides the `targetObject` message property, which you can use in a custom
converter to determine the type.

>IMPORTANT: `Jackson2JsonMessageConverter` and, therefore, `DefaultJackson2JavaTypeMapper` (`DefaultClassMapper`) provide the `trustedPackages` option to overcome https://pivotal.io/security/cve-2017-4995[Serialization Gadgets] vulnerability.
By default and for backward compatibility, the `Jackson2JsonMessageConverter` trusts all packages &#151; that is, it uses `*` for the option.

<a name="steeltoe-messaging-data-projection"></a>
#### Using Spring Data Projection Interfaces

You can convert JSON to a Spring Data Projection interface instead of a concrete type.
This allows very selective, and low-coupled bindings to data, including the lookup of values from multiple places inside the JSON document.
For example, the following interface can be defined as a message payload type:

```Java
interface SomeSample {

  @JsonPath({ "$.username", "$.user.name" })
  String getUsername();

}
```

The following method uses that interface:

```Java
@RabbitListener(queues = "projection")
public void projection(SomeSample in) {
    String username = in.getUsername();
    ...
}
```

Accessor methods are used to lookup the property name as field in the received JSON document by default.
The `@JsonPath` expression allows customization of the value lookup, and even to define multiple JSON path expressions, to lookup values from multiple places until an expression returns an actual value.

To enable this feature, set the `useProjectionForInterfaces` to `true` on the message converter.
You must also add `spring-data:spring-data-commons` and `com.jayway.jsonpath:json-path` to the class path.

When used as the parameter to a `@RabbitListener` method, the interface type is automatically passed to the converter as normal.

<a name="steeltoe-messaging-json-complex"></a>
#### Converting From a `Message` With `RabbitTemplate`

As mentioned earlier, type information is conveyed in message headers to assist the converter when converting from a message.
This works fine in most cases.
However, when using generic types, it can convert only simple objects and known "container" objects (lists, arrays, and maps).
The `Jackson2JsonMessageConverter` implements `SmartMessageConverter`, which lets it be used with the new `RabbitTemplate` methods that take a `ParameterizedTypeReference` argument.
This allows conversion of complex generic types, as shown in the following example:

```Java
Thing1<Thing2<Cat, Hat>> thing1 =
    rabbitTemplate.receiveAndConvert(new ParameterizedTypeReference<Thing1<Thing2<Cat, Hat>>>() { });
```

>NOTE: The `AbstractJsonMessageConverter` class has been removed.
It is no longer the base class for `Jackson2JsonMessageConverter`.
It has been replaced by `AbstractJackson2MessageConverter`.

### Using `MarshallingMessageConverter`

Yet another option is the `MarshallingMessageConverter`.
It delegates to the Spring OXM library's implementations of the `Marshaller` and `Unmarshaller` strategy interfaces.
You can read more about that library https://docs.spring.io/spring/docs/current/spring-framework-reference/html/oxm.html[here].
In terms of configuration, it is most common to provide only the constructor argument, since most implementations of `Marshaller` also implement `Unmarshaller`.
The following example shows how to configure a `MarshallingMessageConverter`:

[//]: # (There was an XML example here.)

[[jackson2xml]]
### Using `Jackson2XmlMessageConverter`

This class was introduced in version 2.1 and can be used to convert messages from and to XML.

Both `Jackson2XmlMessageConverter` and `Jackson2JsonMessageConverter` have the same base class: `AbstractJackson2MessageConverter`.

>NOTE: The `AbstractJackson2MessageConverter` class is introduced to replace a removed class: `AbstractJsonMessageConverter`.

The `Jackson2XmlMessageConverter` uses the `com.fasterxml.jackson` 2.x library.

You can use it the same way as `Jackson2JsonMessageConverter`, except it supports XML instead of JSON.
The following example configures a `Jackson2JsonMessageConverter`:

[//]: # (There was an XML example here.)

See <a href="#steeltoe-messaging-json-message-converter"></a> for more information.

>NOTE: `application/xml` is assumed if there is no `contentType` property, or it has the default value `application/octet-stream`.
To revert to the previous behavior (return an unconverted `byte[]`), set the converter's `assumeSupportedContentType` property to `false`.

### Using `ContentTypeDelegatingMessageConverter`

This class was introduced in version 1.4.2 and allows delegation to a specific `MessageConverter` based on the content type property in the `MessageProperties`.
By default, it delegates to a `SimpleMessageConverter` if there is no `contentType` property or there is a value that matches none of the configured converters.
The following example configures a `ContentTypeDelegatingMessageConverter`:

[//]: # (There was an XML example here.)

<a name="steeltoe-messaging-java-deserialization"></a>
### Java Deserialization

This section covers how to deserialize Java objects.

>IMPORTANT: There is a possible vulnerability when deserializing java objects from untrusted sources.
If you accept messages from untrusted sources with a `content-type` of `application/x-java-serialized-object`, you should
consider configuring which packages and classes are allowed to be deserialized.
This applies to both the `SimpleMessageConverter` and `SerializerMessageConverter` when it is configured to use a
`DefaultDeserializer` either implicitly or via configuration.
By default, the white list is empty, meaning all classes are deserialized.
You can set a list of patterns, such as `thing1.*`, `thing1.thing2.Cat` or `*.MySafeClass`.
The patterns are checked in order until a match is found.
If there is no match, a `SecurityException` is thrown.
You can set the patterns using the `whiteListPatterns` property on these converters.

[[message-properties-converters]]
### Message Properties Converters

The `MessagePropertiesConverter` strategy interface is used to convert between the Rabbit Client `BasicProperties` and Steeltoe RabbitMQ `MessageProperties`.
The default implementation (`DefaultMessagePropertiesConverter`) is usually sufficient for most purposes, but you can implement your own if needed.
The default properties converter converts `BasicProperties` elements of type `LongString` to `String` instances when the size is not greater than `1024` bytes.
Larger `LongString` instances are not converted (see the next paragraph).
This limit can be overridden with a constructor argument.

Headers longer than the long string limit (default: 1024) are now left as
`LongString` instances by default by the `DefaultMessagePropertiesConverter`.
You can access the contents through the `getBytes[]`, `toString()`, or `getStream()` methods.

Previously, the `DefaultMessagePropertiesConverter` "converted" such headers to a `DataInputStream` (actually it just referenced the `LongString` instance's `DataInputStream`).
On output, this header was not converted (except to a String &#151; for example, `java.io.DataInputStream@1d057a39` by calling `toString()` on the stream).

Large incoming `LongString` headers are now correctly "converted" on output, too (by default).

A new constructor is provided to let you configure the converter to work as before.
The following listing shows the Javadoc comment and declaration of the method:

```Java
/**
 * Construct an instance where LongStrings will be returned
 * unconverted or as a java.io.DataInputStream when longer than this limit.
 * Use this constructor with 'true' to restore pre-1.6 behavior.
 * @param longStringLimit the limit.
 * @param convertLongLongStrings LongString when false,
 * DataInputStream when true.
 * @since 1.6
 */
public DefaultMessagePropertiesConverter(int longStringLimit, boolean convertLongLongStrings) { ... }
```

Also, `MessageProperties` contains a property called `correlationIdString`.
Previously, when converting to and from `BasicProperties` used by the RabbitMQ client, an unnecessary `byte[] <-> String` conversion was performed because `MessageProperties.correlationId` is a `byte[]`, but `BasicProperties` uses a `String`.
(Ultimately, the RabbitMQ client uses UTF-8 to convert the `String` to bytes to put in the protocol message).

To provide maximum backwards compatibility, a new property called `correlationIdPolicy` has been added to the
`DefaultMessagePropertiesConverter`.
This takes a `DefaultMessagePropertiesConverter.CorrelationIdPolicy` enum argument.
By default it is set to `BYTES`, which replicates the previous behavior.

For inbound messages:

* `STRING`: Only the `correlationIdString` property is mapped
* `BYTES`: Only the `correlationId` property is mapped
* `BOTH`: Both properties are mapped

For outbound messages:

* `STRING`: Only the `correlationIdString` property is mapped
* `BYTES`: Only the `correlationId` property is mapped
* `BOTH`: Both properties are considered, with the `String` property taking precedence

Also, the inbound `deliveryMode` property is no longer mapped to `MessageProperties.deliveryMode`.
It is mapped to `MessageProperties.receivedDeliveryMode` instead.
Also, the inbound `userId` property is no longer mapped to `MessageProperties.userId`.
It is mapped to `MessageProperties.receivedUserId` instead.
These changes are to avoid unexpected propagation of these properties if the same `MessageProperties` object is used for an outbound message.

The `DefaultMessagePropertiesConverter` converts any custom headers with values of type `Class<?>`  using `getName()` instead of `toString()`; this avoids consuming application having to parse the class name out of the `toString()` representation.
For rolling upgrades, you may need to change your consumers to understand both formats until all producers are upgraded.

<a name="steeltoe-messaging-post-processing"></a>
## Modifying Messages - Compression and More

A number of extension points exist.
They let you perform some processing on a message, either before it is sent to RabbitMQ or immediately after it is received.

As can be seen in <a href="#steeltoe-messaging-message-converters"></a>, one such extension point is in the `AmqpTemplate` `convertAndReceive` operations, where you can provide a `MessagePostProcessor`.
For example, after your POJO has been converted, the `MessagePostProcessor` lets you set custom headers or properties on the `Message`.

Additional extension points have been added to the `RabbitTemplate` - `setBeforePublishPostProcessors()` and `setAfterReceivePostProcessors()`.
The first enables a post processor to run immediately before sending to RabbitMQ.
When using batching (see <a href="steeltoe-messaging-template-batching"></a>), this is invoked after the batch is assembled and before the batch is sent.
The second is invoked immediately after a message is received.

These extension points are used for such features as compression and, for this purpose, several `MessagePostProcessor` implementations are provided.
`GZipPostProcessor`, `ZipPostProcessor` and `DeflaterPostProcessor` compress messages before sending, and `GUnzipPostProcessor`, `UnzipPostProcessor` and `InflaterPostProcessor` decompress received messages.

>NOTE: The `GZipPostProcessor` can be configured with the `copyProperties = true` option to make a  copy of the original message properties.
By default, these properties are reused for performance reasons, and modified with compression content encoding and the optional `MessageProperties.SPRING_AUTO_DECOMPRESS` header.
If you retain a reference to the original outbound message, its properties will change as well.
So, if your application retains a copy of an outbound message with these message post processors, consider turning the `copyProperties` option on.

Similarly, the `SimpleMessageListenerContainer` also has a `setAfterReceivePostProcessors()` method, letting the decompression be performed after messages are received by the container.

`addBeforePublishPostProcessors()` and `addAfterReceivePostProcessors()` have been added to the `RabbitTemplate` to allow appending new post processors to the list of before publish and after receive post processors respectively.
Also there are methods provided to remove the post processors.
Similarly, `AbstractMessageListenerContainer` also has `addAfterReceivePostProcessors()` and `removeAfterReceivePostProcessor()` methods added.
See the Javadoc of `RabbitTemplate` and `AbstractMessageListenerContainer` for more detail.

<a name="steeltoe-messaging-request-reply"></a>
## Request/Reply Messaging

The `AmqpTemplate` also provides a variety of `sendAndReceive` methods that accept the same argument options that were described earlier for the one-way send operations (`exchange`, `routingKey`, and `Message`).
Those methods are quite useful for request-reply scenarios, since they handle the configuration of the necessary `reply-to` property before sending and can listen for the reply message on an exclusive queue that is created internally for that purpose.

Similar request-reply methods are also available where the `MessageConverter` is applied to both the request and reply.
Those methods are named `convertSendAndReceive`.
See the [Javadoc of `AmqpTemplate`](https://docs.spring.io/spring-amqp/docs/latest-ga/api/org/springframework/amqp/core/AmqpTemplate.html) for more detail.

Each of the `sendAndReceive` method variants has an overloaded version that takes `CorrelationData`.
Together with a properly configured connection factory, this enables the receipt of publisher confirms for the send side of the operation.
See <a href="#steeltoe-messaging-template-confirms"></a> and the [Javadoc for `RabbitOperations`](https://docs.spring.io/spring-amqp/docs/latest-ga/api/org/springframework/amqp/rabbit/core/RabbitOperations.html) for more information.

There are variants of these methods (`convertSendAndReceiveAsType`) that take an additional `ParameterizedTypeReference` argument to convert complex returned types.
The template must be configured with a `SmartMessageConverter`.
See <a href="#steeltoe-messaging-json-complex"></a> for more information.

You can configure the `RabbitTemplate` with the `noLocalReplyConsumer` option to control a `noLocal` flag for reply consumers.
This is `false` by default.

<a name="steeltoe-messaging-reply-timeout"></a>
### Reply Timeout

By default, the send and receive methods timeout after five seconds and return null.
You can modify this behavior by setting the `replyTimeout` property.
If you set the `mandatory` property to `true` (or the `mandatory-expression` evaluates to `true` for a particular message), if the message cannot be delivered to a queue, an `AmqpMessageReturnedException` is thrown.
This exception has `returnedMessage`, `replyCode`, and `replyText` properties, as well as the `exchange` and `routingKey` used for the send.

>NOTE: This feature uses publisher returns.
You can enable it by setting `publisherReturns` to `true` on the `CachingConnectionFactory` (see <a href="#steeltoe-messaging-cf-pub-conf-ret"></a>).
Also, you must not have registered your own `ReturnCallback` with the `RabbitTemplate`.

A `replyTimedOut` method has been added, letting subclasses be informed of the timeout so that they can clean up any retained state.

When you use the default `DirectReplyToMessageListenerContainer`, you can add an error handler by setting the template's `replyErrorHandler` property.
This error handler is invoked for any failed deliveries, such as late replies and messages received without a correlation header.
The exception passed in is a `ListenerExecutionFailedException`, which has a `failedMessage` property.

<a name="steeltoe-messaging-direct-reply-to"></a>
## RabbitMQ Direct Reply-to

>IMPORTANT: The RabbitMQ server supports https://www.rabbitmq.com/direct-reply-to.html[direct reply-to].
This eliminates the main reason for a fixed reply queue (to avoid the need to create a temporary queue for each request).
Direct reply-to is used by default (if supported by the server) instead of creating temporary reply queues.
When no `replyQueue` is provided (or it is set with a name of `amq.rabbitmq.reply-to`), the `RabbitTemplate` automatically detects whether direct reply-to is supported and either uses it or falls back to using a temporary reply queue.
When using direct reply-to, a `reply-listener` is not required and should not be configured.

Reply listeners are still supported with named queues (other than `amq.rabbitmq.reply-to`), allowing control of reply concurrency and so on.

If you wish to use a temporary, exclusive, auto-delete queue for each
reply, set the `useTemporaryReplyQueues` property to `true`.
This property is ignored if you set a `replyAddress`.

You can change the criteria that dictate whether to use direct reply-to by subclassing `RabbitTemplate` and overriding `useDirectReplyTo()` to check different criteria.
The method is called once only, when the first request is sent.

Prior to version 2.0, the `RabbitTemplate` created a new consumer for each request and canceled the consumer when the reply was received (or timed out).
Now the template uses a `DirectReplyToMessageListenerContainer` instead, letting the consumers be reused.
The template still takes care of correlating the replies, so there is no danger of a late reply going to a different sender.
If you want to revert to the previous behavior, set the `useDirectReplyToContainer` (`direct-reply-to-container` when using XML configuration) property to false.

The `AsyncRabbitTemplate` has no such option.
It always used a `DirectReplyToContainer` for replies when direct reply-to is used.

### Message Correlation With A Reply Queue

When using a fixed reply queue (other than `amq.rabbitmq.reply-to`), you must provide correlation data so that replies can be correlated to requests.
See [RabbitMQ Remote Procedure Call (RPC)](https://www.rabbitmq.com/tutorials/tutorial-six-java.html).
By default, the standard `correlationId` property is used to hold the correlation data.
However, if you wish to use a custom property to hold correlation data, you can set the `correlation-key` attribute on the <rabbit-template/>.
Explicitly setting the attribute to `correlationId` is the same as omitting the attribute.
The client and server must use the same header for correlation data.

>NOTE: Steeltoe RabbitMQ version 1.1 used a custom property called `spring_reply_correlation` for this data.
If you wish to revert to this behavior with the current version (perhaps to maintain compatibility with another application using 1.1), you must set the attribute to `spring_reply_correlation`.

By default, the template generates its own correlation ID (ignoring any user-supplied value).
If you wish to use your own correlation ID, set the `RabbitTemplate` instance's `userCorrelationId` property to `true`.

>IMPORTANT: The correlation ID must be unique to avoid the possibility of a wrong reply being returned for a request.

<a name="steeltoe-messaging-reply-listener"></a>
### Reply Listener Container

When using RabbitMQ versions prior to 3.4.0, a new temporary queue is used for each reply.
However, a single reply queue can be configured on the template, which can be more efficient and also lets you set arguments on that queue.
In this case, however, you must also provide a `<reply-listener/>` sub element.
This element provides a listener container for the reply queue, with the template being the listener.
All of the <a href="#steeltoe-messaging-container-attributes"></a> attributes allowed on a <listener-container/> are allowed on the element, except for `connection-factory` and `message-converter`, which are inherited from the template's configuration.

>IMPORTANT: If you run multiple instances of your application or use multiple `RabbitTemplate` instances, you *MUST* use a unique reply queue for each.
RabbitMQ has no ability to select messages from a queue, so, if they all use the same queue, each instance would compete for replies and not necessarily receive their own.

The following example defines a rabbit template with a connection factory:

[//]: # (There was an XML example here.)

While the container and template share a connection factory, they do not share a channel.
Therefore, requests and replies are not performed within the same transaction (if transactional).

>NOTE: Prior to version 1.5.0, the `reply-address` attribute was not available.
Replies were always routed by using the default exchange and the `reply-queue` name as the routing key.
This is still the default, but you can now specify the new `reply-address` attribute.
The `reply-address` can contain an address with the form `<exchange>/<routingKey>` and the reply is routed to the specified exchange and routed to a queue bound with the routing key.
The `reply-address` has precedence over `reply-queue`.
When only `reply-address` is in use, the `<reply-listener>` must be configured as a separate `<listener-container>` component.
The `reply-address` and `reply-queue` (or `queues` attribute on the `<listener-container>`) must refer to the same queue logically.

With this configuration, a `SimpleListenerContainer` is used to receive the replies, with the `RabbitTemplate` being the `MessageListener`.
When defining a template with the `<rabbit:template/>` namespace element, as shown in the preceding example, the parser defines the container and wires in the template as the listener.

>NOTE: When the template does not use a fixed `replyQueue` (or is using direct reply-to &#151; see <a href="#steeltoe-messaging-direct-reply-to"></a>), a listener container is not needed.
Direct `reply-to` is the preferred mechanism when using RabbitMQ 3.4.0 or later.

If you define your `RabbitTemplate` as a `<bean/>` or use an `@Configuration` class to define it as an `@Bean` or when you create the template programmatically, you need to define and wire up the reply listener container yourself.
If you fail to do this, the template never receives the replies and eventually times out and returns null as the reply to a call to a `sendAndReceive` method.

The `RabbitTemplate` detects if it has been
configured as a `MessageListener` to receive replies.
If not, attempts to send and receive messages with a reply address
fail with an `IllegalStateException` (because the replies are never received).

Further, if a simple `replyAddress` (queue name) is used, the reply listener container verifies that it is listening
to a queue with the same name.
This check cannot be performed if the reply address is an exchange and routing key and a debug log message is written.

>IMPORTANT: When wiring the reply listener and template yourself, it is important to ensure that the template's `replyAddress` and the container's `queues` (or `queueNames`) properties refer to the same queue.
The template inserts the reply address into the outbound message `replyTo` property.

The following listing shows examples of how to manually wire up the beans:

```Java
    @Bean
    public RabbitTemplate amqpTemplate() {
        RabbitTemplate rabbitTemplate = new RabbitTemplate(connectionFactory());
        rabbitTemplate.setMessageConverter(msgConv());
        rabbitTemplate.setReplyAddress(replyQueue().getName());
        rabbitTemplate.setReplyTimeout(60000);
        rabbitTemplate.setUseDirectReplyToContainer(false);
        return rabbitTemplate;
    }

    @Bean
    public SimpleMessageListenerContainer replyListenerContainer() {
        SimpleMessageListenerContainer container = new SimpleMessageListenerContainer();
        container.setConnectionFactory(connectionFactory());
        container.setQueues(replyQueue());
        container.setMessageListener(amqpTemplate());
        return container;
    }

    @Bean
    public Queue replyQueue() {
        return new Queue("my.reply.queue");
    }
```

A complete example of a `RabbitTemplate` wired with a fixed reply queue, together with a "remote" listener container that handles the request and returns the reply is shown in [this test case](https://github.com/spring-projects/spring-amqp/tree/master/spring-rabbit/src/test/java/org/springframework/amqp/rabbit/listener/JavaConfigFixedReplyQueueTests.java).

>IMPORTANT: When the reply times out (`replyTimeout`), the `sendAndReceive()` methods return null.

Prior to version 1.3.6, late replies for timed out messages were only logged.
Now, if a late reply is received, it is rejected (the template throws an `AmqpRejectAndDontRequeueException`).
If the reply queue is configured to send rejected messages to a dead letter exchange, the reply can be retrieved for later analysis.
To do so, bind a queue to the configured dead letter exchange with a routing key equal to the reply queue's name.

See the [RabbitMQ Dead Letter Documentation](https://www.rabbitmq.com/dlx.html) for more information about configuring dead lettering.
You can also take a look at the `FixedReplyQueueDeadLetterTests` test case for an example.

<a name="steeltoe-messaging-async-template"></a>
### `AsyncRabbitTemplate`

Version 1.6 introduced the `AsyncRabbitTemplate`.
This has similar `sendAndReceive` (and `convertSendAndReceive`) methods
to those on the <a href="#steeltoe-messaging-amqptemplate"></a>.
However, instead of blocking, they return a `ListenableFuture`.

The `sendAndReceive` methods return a `RabbitMessageFuture`.
The `convertSendAndReceive` methods return a `RabbitConverterFuture`.

You can either synchronously retrieve the result later, by invoking `get()` on the future, or you can register a callback that is called asynchronously with the result.
The following listing shows both approaches:

```Java
@Autowired
private AsyncRabbitTemplate template;

...

public void doSomeWorkAndGetResultLater() {

    ...

    ListenableFuture<String> future = this.template.convertSendAndReceive("foo");

    // do some more work

    String reply = null;
    try {
        reply = future.get();
    }
    catch (ExecutionException e) {
        ...
    }

    ...

}

public void doSomeWorkAndGetResultAsync() {

    ...

    RabbitConverterFuture<String> future = this.template.convertSendAndReceive("foo");
    future.addCallback(new ListenableFutureCallback<String>() {

        @Override
        public void onSuccess(String result) {
            ...
        }

        @Override
        public void onFailure(Throwable ex) {
            ...
        }

    });

    ...

}
```

If `mandatory` is set and the message cannot be delivered, the future throws an `ExecutionException` with a cause of `AmqpMessageReturnedException`, which encapsulates the returned message and information about the return.

If `enableConfirms` is set, the future has a property called `confirm`, which is itself a `ListenableFuture<Boolean>` with `true` indicating a successful publish.
If the confirm future is `false`, the `RabbitFuture` has a further property called `nackCause`, which contains the reason for the failure, if available.

>IMPORTANT: The publisher confirm is discarded if it is received after the reply, since the reply implies a successful publish.

You can set the `receiveTimeout` property on the template to time out replies (it defaults to `30000` - 30 seconds).
If a timeout occurs, the future is completed with an `AmqpReplyTimeoutException`.

The template implements `SmartLifecycle`.
Stopping the template while there are pending replies causes the pending `Future` instances to be canceled.

The asynchronous template now supports https://www.rabbitmq.com/direct-reply-to.html[direct reply-to] instead of a configured reply queue.
To enable this feature, use one of the following constructors:

```Java
public AsyncRabbitTemplate(ConnectionFactory connectionFactory, String exchange, String routingKey)

public AsyncRabbitTemplate(RabbitTemplate template)
```

See <a href="#steeltoe-messaging-direct-reply-to"></a> to use direct reply-to with the synchronous `RabbitTemplate`.

Version 2.0 introduced variants of these methods (`convertSendAndReceiveAsType`) that take an additional `ParameterizedTypeReference` argument to convert complex returned types.
You must configure the underlying `RabbitTemplate` with a `SmartMessageConverter`.
See <a href="#steeltoe-messaging-json-complex"></a> for more information.

<a href="steeltoe-messaging-remoting"></a>
### Spring Remoting with RabbitMQ

The Spring Framework has a general remoting capability, allowing [Remote Procedure Calls (RPC)](https://docs.spring.io/spring/docs/current/spring-framework-reference/html/remoting.html) that use various transports.
Spring-AMQP supports a similar mechanism with a `AmqpProxyFactoryBean` on the client and a `AmqpInvokerServiceExporter` on the server.
This provides RPC over RabbitMQ.
On the client side, a `RabbitTemplate` is used as described in <a href="#steeltoe-messaging-reply-listener"></a>.
On the server side, the invoker (configured as a `MessageListener`) receives the message, invokes the configured service, and returns the reply by using the inbound message's `replyTo` information.

You can inject the client factory bean into any bean (by using its `serviceInterface`).
The client can then invoke methods on the proxy, resulting in remote execution over RabbitMQ.

>NOTE: With the default `MessageConverter` instances, the method parameters and returned value must be instances of `Serializable`.

On the server side, the `AmqpInvokerServiceExporter` has both `AmqpTemplate` and `MessageConverter` properties.
Currently, the template's `MessageConverter` is not used.
If you need to supply a custom message converter, you should provide it by setting the `messageConverter` property.
On the client side, you can add a custom message converter to the `AmqpTemplate`, which is provided to the `AmqpProxyFactoryBean` by using its `amqpTemplate` property.

The following listing shows sample client and server configurations:

[//]: # (There was an XML example here.)

>IMPORTANT: The `AmqpInvokerServiceExporter` can process only properly formed messages, such as those sent from the `AmqpProxyFactoryBean`.
If it receives a message that it cannot interpret, a serialized `RuntimeException` is sent as a reply.
If the message has no `replyToAddress` property, the message is rejected and permanently lost if no dead letter exchange has been configured.

>NOTE: By default, if the request message cannot be delivered, the calling thread eventually times out and a `RemoteProxyFailureException` is thrown.
By default, the timeout is five seconds.
You can modify that duration by setting the `replyTimeout` property on the `RabbitTemplate`.
By setting the `mandatory` property to `true` and enabling returns on the connection factory (see <a href="#steeltoe-messaging-cf-pub-conf-ret"></a>), the calling thread throws an `AmqpMessageReturnedException`.
See <a href="#steeltoe-messaging-reply-timeout"><a/> for more information.

<a name="steeltoe-messaging-broker-configuration"></a>
## Configuring the Broker

The AMQP specification describes how the protocol can be used to configure queues, exchanges, and bindings on the broker.
These operations (which are portable from the 0.8 specification and higher) are present in the `AmqpAdmin` interface in the `org.springframework.amqp.core` package.
The RabbitMQ implementation of that class is `RabbitAdmin` located in the `org.springframework.amqp.rabbit.core` package.

The `AmqpAdmin` interface is based on using the Steeltoe RabbitMQ domain abstractions and is shown in the following listing:

```Java
public interface AmqpAdmin {

    // Exchange Operations

    void declareExchange(Exchange exchange);

    void deleteExchange(String exchangeName);

    // Queue Operations

    Queue declareQueue();

    String declareQueue(Queue queue);

    void deleteQueue(String queueName);

    void deleteQueue(String queueName, boolean unused, boolean empty);

    void purgeQueue(String queueName, boolean noWait);

    // Binding Operations

    void declareBinding(Binding binding);

    void removeBinding(Binding binding);

    Properties getQueueProperties(String queueName);

}
```

See also <a href="#steeltoe-messaging-scoped-operations"></a>.

The `getQueueProperties()` method returns some limited information about the queue (message count and consumer count).
The keys for the properties returned are available as constants in the `RabbitTemplate` (`QUEUE_NAME`,
`QUEUE_MESSAGE_COUNT`, and `QUEUE_CONSUMER_COUNT`).
The <a href="#steeltoe-messaging-management-rest-api">RabbitMQ REST API</a> provides much more information in the `QueueInfo` object.

The no-arg `declareQueue()` method defines a queue on the broker with a name that is automatically generated.
The additional properties of this auto-generated queue are `exclusive=true`, `autoDelete=true`, and `durable=false`.

The `declareQueue(Queue queue)` method takes a `Queue` object and returns the name of the declared queue.
If the `name` property of the provided `Queue` is an empty `String`, the broker declares the queue with a generated name.
That name is returned to the caller.
That name is also added to the `actualName` property of the `Queue`.
You can use this functionality programmatically only by invoking the `RabbitAdmin` directly.
When using auto-declaration by the admin when defining a queue declaratively in the application context, you can set the name property to `""` (the empty string).
The broker then creates the name.
Listener containers can use queues of this type.
See <a href="#steeltoe-messaging-containers-and-broker-named-queues"></a> for more information.

This is in contrast to an `AnonymousQueue` where the framework generates a unique (`UUID`) name and sets `durable` to
`false` and `exclusive`, `autoDelete` to `true`.
A `<rabbit:queue/>` with an empty (or missing) `name` attribute always creates an `AnonymousQueue`.

See <a href="#steeltoe-messaging-anonymous-queue"></a> to understand why `AnonymousQueue` is preferred over broker-generated queue names as well as
how to control the format of the name.
Anonymous queues are declared with argument `x-queue-master-locator` set to `client-local` by default.
This ensures that the queue is declared on the node to which the application is connected.
Declarative queues must have fixed names because they might be referenced elsewhere in the context &#151; such as in the
listener shown in the following example:

[//]: # (There was an XML example here.)

See <a href="#automatic-declaration"></a>.

The RabbitMQ implementation of this interface is `RabbitAdmin`, which, when configured by using Spring XML, resembles the following example:

[//]: # (There was an XML example here.)

When the `CachingConnectionFactory` cache mode is `CHANNEL` (the default), the `RabbitAdmin` implementation does automatic lazy declaration of queues, exchanges, and bindings declared in the same `ApplicationContext`.
These components are declared as soon as a `Connection` is opened to the broker.
There are some namespace features that make this very convenient &#151; for example,
in the Stocks sample application, we have the following:

[//]: # (There was an XML example here.)

In the preceding example, we use anonymous queues (actually, internally, just queues with names generated by the framework, not by the broker) and refer to them by ID.
We can also declare queues with explicit names, which also serve as identifiers for their bean definitions in the context.
The following example configures a queue with an explicit name:

[//]: # (There was an XML example here.)

>TIP: You can provide both `id` and `name` attributes.
This lets you refer to the queue (for example, in a binding) by an ID that is independent of the queue name.
It also allows standard Spring features (such as property placeholders and SpEL expressions for the queue name).
These features are not available when you use the name as the bean identifier.

Queues can be configured with additional arguments &#151; for example, `x-message-ttl`.
When you use the namespace support, they are provided in the form of a `Map` of argument-name/argument-value pairs, which are defined by using the `<rabbit:queue-arguments>` element.
The following example shows how to do so:

[//]: # (There was an XML example here.)

By default, the arguments are assumed to be strings.
For arguments of other types, you must provide the type.
The following example shows how to specify the type:

[//]: # (There was an XML example here.)

When providing arguments of mixed types, you must provide the type for each entry element.
The following example shows how to do so:

[//]: # (There was an XML example here.)

This can be declared a little more succinctly, as follows:

[//]: # (There was an XML example here.)

When you use Java configuration, the `x-queue-master-locator` is supported as a first class property through the `setMasterLocator()` method on the `Queue` class.
Anonymous queues are declared with this property set to `client-local` by default.
This ensures that the queue is declared on the node the application is connected to.

>IMPORTANT: The RabbitMQ broker does not allow declaration of a queue with mismatched arguments.
For example, if a `queue` already exists with no `time to live` argument, and you attempt to declare it with (for example) `key="x-message-ttl" value="100"`, an exception is thrown.

By default, the `RabbitAdmin` immediately stops processing all declarations when any exception occurs.
This could cause downstream issues, such as a listener container failing to initialize because another queue (defined after the one in error) is not declared.

This behavior can be modified by setting the `ignore-declaration-exceptions` attribute to `true` on the `RabbitAdmin` instance.
This option instructs the `RabbitAdmin` to log the exception and continue declaring other elements.
When configuring the `RabbitAdmin` using Java, this property is called `ignoreDeclarationExceptions`.
This is a global setting that applies to all elements.
Queues, exchanges, and bindings have a similar property that applies to just those elements.

Prior to version 1.6, this property took effect only if an `IOException` occurred on the channel, such as when there is a mismatch between current and desired properties.
Now, this property takes effect on any exception, including `TimeoutException` and others.

In addition, any declaration exceptions result in the publishing of a `DeclarationExceptionEvent`, which is an `ApplicationEvent` that can be consumed by any `ApplicationListener` in the context.
The event contains a reference to the admin, the element that was being declared, and the `Throwable`.

<a name="steeltoe-messaging-headers-exchange"></a>
### Headers Exchange

You can configure the `HeadersExchange` to match on multiple headers.
You can also specify whether any or all headers must match.
The following example shows how to do so:

[//]: # (There was an XML example here.)

You can configure `Exchanges` with an `internal` flag (defaults to `false`) and such an
`Exchange` is properly configured on the Broker through a `RabbitAdmin` (if one is present in the application context).
If the `internal` flag is `true` for an exchange, RabbitMQ does not let clients use the exchange.
This is useful for a dead letter exchange or exchange-to-exchange binding, where you do not wish the exchange to be used
directly by publishers.

To see how to use Java to configure the AMQP infrastructure, look at the Stock sample application,
where there is the `@Configuration` class `AbstractStockRabbitConfiguration`, which ,in turn has
`RabbitClientConfiguration` and `RabbitServerConfiguration` subclasses.
The following listing shows the code for `AbstractStockRabbitConfiguration`:

```Java
@Configuration
public abstract class AbstractStockAppRabbitConfiguration {

    @Bean
    public ConnectionFactory connectionFactory() {
        CachingConnectionFactory connectionFactory =
            new CachingConnectionFactory("localhost");
        connectionFactory.setUsername("guest");
        connectionFactory.setPassword("guest");
        return connectionFactory;
    }

    @Bean
    public RabbitTemplate rabbitTemplate() {
        RabbitTemplate template = new RabbitTemplate(connectionFactory());
        template.setMessageConverter(jsonMessageConverter());
        configureRabbitTemplate(template);
        return template;
    }

    @Bean
    public MessageConverter jsonMessageConverter() {
        return new Jackson2JsonMessageConverter();
    }

    @Bean
    public TopicExchange marketDataExchange() {
        return new TopicExchange("app.stock.marketdata");
    }

    // additional code omitted for brevity

}
```

In the Stock application, the server is configured by using the following `@Configuration` class:

```Java
@Configuration
public class RabbitServerConfiguration extends AbstractStockAppRabbitConfiguration  {

    @Bean
    public Queue stockRequestQueue() {
        return new Queue("app.stock.request");
    }
}
```

This is the end of the whole inheritance chain of `@Configuration` classes.
The end result is that `TopicExchange` and `Queue` are declared to the broker upon application startup.
There is no binding of  `TopicExchange` to a queue in the server configuration, as that is done in the client application.
The stock request queue, however, is automatically bound to the RabbitMQ default exchange.
This behavior is defined by the specification.

The client `@Configuration` class is a little more interesting.
Its declaration follows:

```Java
@Configuration
public class RabbitClientConfiguration extends AbstractStockAppRabbitConfiguration {

    @Value("${stocks.quote.pattern}")
    private String marketDataRoutingKey;

    @Bean
    public Queue marketDataQueue() {
        return amqpAdmin().declareQueue();
    }

    /**
     * Binds to the market data exchange.
     * Interested in any stock quotes
     * that match its routing key.
     */
    @Bean
    public Binding marketDataBinding() {
        return BindingBuilder.bind(
                marketDataQueue()).to(marketDataExchange()).with(marketDataRoutingKey);
    }

    // additional code omitted for brevity

}
```

The client declares another queue through the `declareQueue()` method on the `AmqpAdmin`.
It binds that queue to the market data exchange with a routing pattern that is externalized in a properties file.


<a name="steeltoe-messaging-builder-api"></a>
### Builder API for Queues and Exchanges

Version 1.6 introduces a convenient fluent API for configuring `Queue` and `Exchange` objects when using Java configuration.
The following example shows how to use it:

```Java
@Bean
public Queue queue() {
    return QueueBuilder.nonDurable("foo")
        .autoDelete()
        .exclusive()
        .withArgument("foo", "bar")
        .build();
}

@Bean
public Exchange exchange() {
  return ExchangeBuilder.directExchange("foo")
      .autoDelete()
      .internal()
      .withArgument("foo", "bar")
      .build();
}
```

See the Javadoc for [`org.springframework.amqp.core.QueueBuilder`](https://docs.spring.io/spring-amqp/docs/latest-ga/api/org/springframework/amqp/core/QueueBuilder.html) and [`org.springframework.amqp.core.ExchangeBuilder`](https://docs.spring.io/spring-amqp/docs/latest-ga/api/org/springframework/amqp/core/ExchangeBuilder.html) for more information.

The `ExchangeBuilder` now creates durable exchanges by default, to be consistent with the simple constructors on the individual `AbstractExchange` classes.
To make a non-durable exchange with the builder, use `.durable(false)` before invoking `.build()`.
The `durable()` method with no parameter is no longer provided.

Steeltoe RabbitMQ uses fluent APIs to add "well known" exchange and queue arguments, as follows:

```Java
@Bean
public Queue allArgs1() {
    return QueueBuilder.nonDurable("all.args.1")
            .ttl(1000)
            .expires(200_000)
            .maxLength(42)
            .maxLengthBytes(10_000)
            .overflow(Overflow.rejectPublish)
            .deadLetterExchange("dlx")
            .deadLetterRoutingKey("dlrk")
            .maxPriority(4)
            .lazy()
            .masterLocator(MasterLocator.minMasters)
            .singleActiveConsumer()
            .build();
}

@Bean
public DirectExchange ex() {
    return ExchangeBuilder.directExchange("ex.with.alternate")
            .durable(true)
            .alternate("alternate")
            .build();
}
```

<a name="steeltoe-messaging-collection-declaration"></a>
### Declaring Collections of Exchanges, Queues, and Bindings

You can wrap collections of `Declarable` objects (`Queue`, `Exchange`, and `Binding`) in `Declarables` objects.
The `RabbitAdmin` detects such beans (as well as discrete `Declarable` beans) in the application context, and declares the contained objects on the broker whenever a connection is established (initially and after a connection failure).
The following example shows how to do so:

```Java
@Configuration
public static class Config {

    @Bean
    public ConnectionFactory cf() {
        return new CachingConnectionFactory("localhost");
    }

    @Bean
    public RabbitAdmin admin(ConnectionFactory cf) {
        return new RabbitAdmin(cf);
    }

    @Bean
    public DirectExchange e1() {
    	return new DirectExchange("e1", false, true);
    }

    @Bean
    public Queue q1() {
    	return new Queue("q1", false, false, true);
    }

    @Bean
    public Binding b1() {
    	return BindingBuilder.bind(q1()).to(e1()).with("k1");
    }

    @Bean
    public Declarables es() {
        return new Declarables(
                new DirectExchange("e2", false, true),
                new DirectExchange("e3", false, true));
    }

    @Bean
    public Declarables qs() {
        return new Declarables(
                new Queue("q2", false, false, true),
                new Queue("q3", false, false, true));
    }

    @Bean
    @Scope(ConfigurableBeanFactory.SCOPE_PROTOTYPE)
    public Declarables prototypes() {
        return new Declarables(new Queue(this.prototypeQueueName, false, false, true));
    }

    @Bean
    public Declarables bs() {
        return new Declarables(
                new Binding("q2", DestinationType.QUEUE, "e2", "k2", null),
                new Binding("q3", DestinationType.QUEUE, "e3", "k3", null));
    }

    @Bean
    public Declarables ds() {
        return new Declarables(
                new DirectExchange("e4", false, true),
                new Queue("q4", false, false, true),
                new Binding("q4", DestinationType.QUEUE, "e4", "k4", null));
    }

}
```

>IMPORTANT: In versions prior to 2.1, you could declare multiple `Declarable` instances by defining beans of type `Collection<Declarable>`.
This can cause undesirable side effects in some cases, because the admin has to iterate over all `Collection<?>` beans.
This feature is now disabled in favor of `Declarables`, as discussed earlier in this section.
You can revert to the previous behavior by setting the `RabbitAdmin` property called `declareCollections` to `true`.

You can use the `getDeclarablesByType` method of `Declarables` as a convenience (for example, when declaring the listener container beans), as follows.

```Java
public SimpleMessageListenerContainer container(ConnectionFactory connectionFactory,
        Declarables mixedDeclarables, MessageListener listener) {

    SimpleMessageListenerContainer container = new SimpleMessageListenerContainer(connectionFactory);
    container.setQueues(mixedDeclarables.getDeclarablesByType(Queue.class).toArray(new Queue[0]));
    container.setMessageListener(listener);
    return container;
}
```

<a name="steeltoe-messaging-conditional-declaration"></a>
### Conditional Declaration

By default, all queues, exchanges, and bindings are declared by all `RabbitAdmin` instances (assuming they have `auto-startup="true"`) in the application context.

The `RabbitAdmin` has a new property `explicitDeclarationsOnly` (which is `false` by default); when this is set to `true`, the admin declares only beans that are explicitly configured to be declared by that admin.

>NOTE: You can conditionally declare these elements.
This is particularly useful when an application connects to multiple brokers and needs to specify with which brokers a particular element should be declared.

The classes representing these elements implement `Declarable`, which has two methods: `shouldDeclare()` and `getDeclaringAdmins()`.
The `RabbitAdmin` uses these methods to determine whether a particular instance should actually process the declarations on its `Connection`.

The properties are available as attributes in the namespace, as shown in the following examples:

[//]: # (There was an XML example here.)

>NOTE: By default, the `auto-declare` attribute is `true` and, if the `declared-by` is not supplied (or is empty), then all `RabbitAdmin` instances declare the object (as long as the admin's `auto-startup` attribute is `true`, the default, and the admin's `explicit-declarations-only` attribute is false).

Similarly, you can use Java-based `@Configuration` to achieve the same effect.
In the following example, the components are declared by `admin1` but not by`admin2`:

```Java
@Bean
public RabbitAdmin admin1() {
    return new RabbitAdmin(cf1());
}

@Bean
public RabbitAdmin admin2() {
    return new RabbitAdmin(cf2());
}

@Bean
public Queue queue() {
    Queue queue = new Queue("foo");
    queue.setAdminsThatShouldDeclare(admin1());
    return queue;
}

@Bean
public Exchange exchange() {
    DirectExchange exchange = new DirectExchange("bar");
    exchange.setAdminsThatShouldDeclare(admin1());
    return exchange;
}

@Bean
public Binding binding() {
    Binding binding = new Binding("foo", DestinationType.QUEUE, exchange().getName(), "foo", null);
    binding.setAdminsThatShouldDeclare(admin1());
    return binding;
}
```

<a name="steeltoe-messaging-note-id-name"></a>
### A Note On the `id` and `name` Attributes

The `name` attribute on `<rabbit:queue/>` and `<rabbit:exchange/>` elements reflects the name of the entity in the broker.
For queues, if the `name` is omitted, an anonymous queue is created (see <a href="#steeltoe-messaging-anonymous-queue"></a>).

In versions prior to 2.0, the `name` was also registered as a bean name alias (similar to `name` on `<bean/>` elements).

This caused two problems:

* It prevented the declaration of a queue and exchange with the same name.
* The alias was not resolved if it contained a SpEL expression (`#{...}`).

If you declare one of these elements with both an `id` _and_ a `name` attribute, the name is no longer declared as a bean name alias.
If you wish to declare a queue and exchange with the same `name`, you must provide an `id`.

There is no change if the element has only a `name` attribute.
The bean can still be referenced by the `name` &#151; for example, in binding declarations.
However, you still cannot reference it if the name contains SpEL &#151; you must provide an `id` for reference purposes.

<a name="steeltoe-messaging-anonymous-queue"></a>
### `AnonymousQueue`

In general, when you need a uniquely-named, exclusive, auto-delete queue, we recommend that you use the `AnonymousQueue`
instead of broker-defined queue names (using `""` as a `Queue` name causes the broker to generate the queue
name).

This is because:

1. The queues are actually declared when the connection to the broker is established.
This is long after the beans are created and wired together.
Beans that use the queue need to know its name.
In fact, the broker might not even be running when the application is started.
2. If the connection to the broker is lost for some reason, the admin re-declares the `AnonymousQueue` with the same name.
If we used broker-declared queues, the queue name would change.

You can control the format of the queue name used by `AnonymousQueue` instances.

By default, the queue name is prefixed by `spring.gen-` followed by a base64 representation of the `UUID` &#151; for example: `spring.gen-MRBv9sqISkuCiPfOYfpo4g`.

You can provide an `AnonymousQueue.NamingStrategy` implementation in a constructor argument.
The following example shows how to do so:

```Java
@Bean
public Queue anon1() {
    return new AnonymousQueue();
}

@Bean
public Queue anon2() {
    return new AnonymousQueue(new AnonymousQueue.Base64UrlNamingStrategy("something-"));
}

@Bean
public Queue anon3() {
    return new AnonymousQueue(AnonymousQueue.UUIDNamingStrategy.DEFAULT);
}
```

The first bean generates a queue name prefixed by `spring.gen-` followed by a base64 representation of the `UUID` &#151; for
example: `spring.gen-MRBv9sqISkuCiPfOYfpo4g`.
The second bean generates a queue name prefixed by `something-` followed by a base64 representation of the `UUID`.
The third bean generates a name by using only the UUID (no base64 conversion) &#151; for example, `f20c818a-006b-4416-bf91-643590fedb0e`.

The base64 encoding uses the "URL and Filename Safe Alphabet" from RFC 4648.
Trailing padding characters (`=`) are removed.

You can provide your own naming strategy, whereby you can include other information (such as the application name or client host) in the queue name.

You can specify the naming strategy when you use XML configuration.
The `naming-strategy` attribute is present on the `<rabbit:queue>` element
for a bean reference that implements `AnonymousQueue.NamingStrategy`.
The following examples show how to specify the naming strategy in various ways:

[//]: # (There was an XML example here.)

The first example creates names such as `spring.gen-MRBv9sqISkuCiPfOYfpo4g`.
The second example creates names with a String representation of a UUID.
The third example creates names such as `custom.gen-MRBv9sqISkuCiPfOYfpo4g`.

You can also provide your own naming strategy bean.

Anonymous queues are declared with argument `x-queue-master-locator` set to `client-local` by default.
This ensures that the queue is declared on the node to which the application is connected.
You can revert to the previous behavior by calling `queue.setMasterLocator(null)` after constructing the instance.

<a name="steeltoe-messaging-broker-events"></a>
### Broker Event Listener

When the [Event Exchange Plugin](https://www.rabbitmq.com/event-exchange.html) is enabled, if you add a bean of type `BrokerEventListener` to the application context, it publishes selected broker events as `BrokerEvent` instances, which can be consumed with a normal Spring `ApplicationListener` or `@EventListener` method.
Events are published by the broker to a topic exchange `amq.rabbitmq.event` with a different routing key for each event type.
The listener uses event keys, which are used to bind an `AnonymousQueue` to the exchange so the listener receives only selected events.
Since it is a topic exchange, wildcards can be used (as well as explicitly requesting specific events), as the following example shows:

```Java
@Bean
public BrokerEventListener eventListener() {
    return new BrokerEventListener(connectionFactory(), "user.deleted", "channel.#", "queue.#");
}
```

You can further narrow the received events in individual event listeners, by using normal Spring techniques, as the following example shows:

```Java
@EventListener(condition = "event.eventType == 'queue.created'")
public void listener(BrokerEvent event) {
    ...
}
```

<a name="#steeltoe-messaging-delayed-message-exchange"></a>
## Delayed Message Exchange

You can read more about the Delayed Message Exchange Plugin [here](https://www.rabbitmq.com/blog/2015/04/16/scheduling-messages-with-rabbitmq/).

>NOTE: The plugin is currently marked as experimental but has been available for over a year (at the time of writing).
If changes to the plugin make it necessary, we plan to add support for such changes as soon as practical.
For that reason, this support in Steeltoe RabbitMQ should be considered experimental, too.
This functionality was tested with RabbitMQ 3.6.0 and version 0.0.1 of the plugin.

To use a `RabbitAdmin` to declare an exchange as delayed, you can set the `delayed` property on the exchange bean to
`true`.
The `RabbitAdmin` uses the exchange type (`Direct`, `Fanout`, and so on) to set the `x-delayed-type` argument and
declare the exchange with type `x-delayed-message`.

The `delayed` property (default: `false`) is also available when configuring exchange beans using XML.
The following example shows how to use it:

[//]: # (There was an example here.)

To send a delayed message, you can set the `x-delay` header through `MessageProperties`, as the following examples show:

```Java
MessageProperties properties = new MessageProperties();
properties.setDelay(15000);
template.send(exchange, routingKey,
        MessageBuilder.withBody("foo".getBytes()).andProperties(properties).build());

// Second example
rabbitTemplate.convertAndSend(exchange, routingKey, "foo", new MessagePostProcessor() {

    @Override
    public Message postProcessMessage(Message message) throws AmqpException {
        message.getMessageProperties().setDelay(15000);
        return message;
    }

});
```

To check if a message was delayed, use the `getReceivedDelay()` method on the `MessageProperties`.
It is a separate property to avoid unintended propagation to an output message generated from an input message.

<a name="steeltoe-messaging-management-rest-api"></a>
## RabbitMQ REST API

When the management plugin is enabled, the RabbitMQ server exposes a REST API to monitor and configure the broker.
A [Java Binding for the API](https://github.com/rabbitmq/hop) is now provided.
The `com.rabbitmq.http.client.Client` is a standard, immediate, and, therefore, blocking API.
It is based on the [Spring Web](https://docs.spring.io/spring/docs/current/spring-framework-reference/web.html#spring-web) module and its `RestTemplate` implementation.
On the other hand, the `com.rabbitmq.http.client.ReactorNettyClient` is a reactive, non-blocking implementation based on the [Reactor Netty](https://projectreactor.io/docs/netty/release/reference/docs/index.html) project.

The hop dependency (`com.rabbitmq:http-client`) is now also `optional`.

See their Javadoc for more information.

<a name="steeltoe-messaging-exception-handling"></a>
## Exception Handling

Many operations with the RabbitMQ Java client can throw checked exceptions.
For example, there are a lot of cases where `IOException` instances may be thrown.
The `RabbitTemplate`, `SimpleMessageListenerContainer`, and other Steeltoe RabbitMQ components catch those exceptions and convert them into one of the exceptions within `AmqpException` hierarchy.
Those are defined in the 'org.springframework.amqp' package, and `AmqpException` is the base of the hierarchy.

When a listener throws an exception, it is wrapped in a `ListenerExecutionFailedException`.
Normally, the message is rejected and requeued by the broker.
Setting `defaultRequeueRejected` to `false` causes messages to be discarded (or routed to a dead letter exchange).
As discussed in <a href="#steeltoe-messaging-async-listeners"></a>, the listener can throw an `AmqpRejectAndDontRequeueException` (or `ImmediateRequeueAmqpException`) to conditionally control this behavior.

However, there is a class of errors where the listener cannot control the behavior.
When a message that cannot be converted is encountered (for example, an invalid `content_encoding` header), some exceptions are thrown before the message reaches user code.
With `defaultRequeueRejected` set to `true` (default) (or throwing an `ImmediateRequeueAmqpException`), such messages would be redelivered over and over.

The default `ErrorHandler` is now a `ConditionalRejectingErrorHandler` that rejects (and does not requeue) messages that fail with an irrecoverable error.
Specifically, it rejects messages that fail with the following errors:

* `o.s.amqp...MessageConversionException`: Can be thrown when converting the incoming message payload using a `MessageConverter`.
* `o.s.messaging...MessageConversionException`: Can be thrown by the conversion service if additional conversion is required when mapping to a `@RabbitListener` method.
* `o.s.messaging...MethodArgumentNotValidException`: Can be thrown if validation (for example, `@Valid`) is used in the listener and the validation fails.
* `o.s.messaging...MethodArgumentTypeMismatchException`: Can be thrown if the inbound message was converted to a type that is not correct for the target method.
For example, the parameter is declared as `Message<Foo>` but `Message<Bar>` is received.
* `java.lang.NoSuchMethodException`: Added in version 1.6.3.
* `java.lang.ClassCastException`: Added in version 1.6.3.

You can configure an instance of this error handler with a `FatalExceptionStrategy` so that users can provide their own rules for conditional message rejection &#151; for example, a delegate implementation to the `BinaryExceptionClassifier` from Spring Retry (see <a href="#steeltoe-messaging-async-listeners"></a>).
In addition, the `ListenerExecutionFailedException` now has a `failedMessage` property that you can use in the decision.
If the `FatalExceptionStrategy.isFatal()` method returns `true`, the error handler throws an `AmqpRejectAndDontRequeueException`.
The default `FatalExceptionStrategy` logs a warning message when an exception is determined to be fatal.

Since version 1.6.3, a convenient way to add user exceptions to the fatal list is to subclass `ConditionalRejectingErrorHandler.DefaultExceptionStrategy` and override the `isUserCauseFatal(Throwable cause)` method to return `true` for fatal exceptions.

A common pattern for handling DLQ messages is to set a `time-to-live` on those messages as well as additional DLQ configuration such that these messages expire and are routed back to the main queue for retry.
The problem with this technique is that messages that cause fatal exceptions loop forever.
The `ConditionalRejectingErrorHandler` detects an `x-death` header on a message that causes a fatal exception to be thrown.
The message is logged and discarded.
You can revert to the previous behavior by setting the `discardFatalsWithXDeath` property on the `ConditionalRejectingErrorHandler` to `false`.

>IMPORTANT: Messages with these fatal exceptions are rejected and NOT requeued by default, even if the container acknowledge mode is MANUAL.
These exceptions generally occur before the listener is invoked so the listener does not have a chance to ack or nack the message so it remained in the queue in an un-acked state.
To revert to the previous behavior, set the `rejectManual` property on the `ConditionalRejectingErrorHandler` to `false`.

<a name="steeltoe-messaging-transactions"></a>
## Transactions

The Spring Rabbit framework has support for automatic transaction management in the synchronous and asynchronous use cases with a number of different semantics that can be selected declaratively, as is familiar to existing users of Spring transactions.
This makes many if not most common messaging patterns easy to implement.

There are two ways to signal the desired transaction semantics to the framework.
In both the `RabbitTemplate` and `SimpleMessageListenerContainer`, there is a flag `channelTransacted` which, if `true`, tells the framework to use a transactional channel and to end all operations (send or receive) with a commit or rollback (depending on the outcome), with an exception signaling a rollback.
Another signal is to provide an external transaction with one of Spring's `PlatformTransactionManager` implementations as a context for the ongoing operation.
If there is already a transaction in progress when the framework is sending or receiving a message, and the `channelTransacted` flag is `true`, the commit or rollback of the messaging transaction is deferred until the end of the current transaction.
If the `channelTransacted` flag is `false`, no transaction semantics apply to the messaging operation (it is auto-acked).

The `channelTransacted` flag is a configuration time setting.
It is declared and processed once when the RabbitMQ components are created, usually at application startup.
The external transaction is more dynamic in principle because the system responds to the current thread state at runtime.
However, in practice, it is often also a configuration setting, when the transactions are layered onto an application declaratively.

For synchronous use cases with `RabbitTemplate`, the external transaction is provided by the caller, either declaratively or imperatively according to taste (the usual Spring transaction model).
The following example shows a declarative approach (usually preferred because it is non-invasive), where the template has been configured with `channelTransacted=true`:

```Java
@Transactional
public void doSomething() {
    String incoming = rabbitTemplate.receiveAndConvert();
    // do some more database processing...
    String outgoing = processInDatabaseAndExtractReply(incoming);
    rabbitTemplate.convertAndSend(outgoing);
}
```

In the preceding example, a `String` payload is received, converted, and sent as a message body inside a method marked as `@Transactional`.
If the database processing fails with an exception, the incoming message is returned to the broker, and the outgoing message is not sent.
This applies to any operations with the `RabbitTemplate` inside a chain of transactional methods (unless, for instance, the `Channel` is directly manipulated to commit the transaction early).

For asynchronous use cases with `SimpleMessageListenerContainer`, if an external transaction is needed, it has to be requested by the container when it sets up the listener.
To signal that an external transaction is required, the user provides an implementation of `PlatformTransactionManager` to the container when it is configured.
The following example shows how to do so:

```Java
@Configuration
public class ExampleExternalTransactionAmqpConfiguration {

    @Bean
    public SimpleMessageListenerContainer messageListenerContainer() {
        SimpleMessageListenerContainer container = new SimpleMessageListenerContainer();
        container.setConnectionFactory(rabbitConnectionFactory());
        container.setTransactionManager(transactionManager());
        container.setChannelTransacted(true);
        container.setQueueName("some.queue");
        container.setMessageListener(exampleListener());
        return container;
    }

}
```

In the preceding example, the transaction manager is added as a dependency injected from another bean definition (not shown), and the `channelTransacted` flag is also set to `true`.
The effect is that if the listener fails with an exception, the transaction is rolled back, and the message is also returned to the broker.
Significantly, if the transaction fails to commit (for example, because of
a database constraint error or connectivity problem), the RabbitMQ transaction is also rolled back, and the message is returned to the broker.
This is sometimes known as a "Best Efforts 1 Phase Commit", and is a very powerful pattern for reliable messaging.
If the `channelTransacted` flag was set to `false` (the default) in the preceding example, the external transaction would still be provided for the listener, but all messaging operations would be auto-acked, so the effect is to commit the messaging operations even on a rollback of the business operation.

<a name="steeltoe-messaging-conditional-rollback"></a>
### Conditional Rollback

Prior to version 1.6.6, adding a rollback rule to a container's `transactionAttribute` when using an external transaction manager (such as JDBC) had no effect.
Exceptions always rolled back the transaction.

Also, when using a [transaction advice](https://docs.spring.io/spring-framework/docs/current/spring-framework-reference/html/transaction.html#transaction-declarative) in the container's advice chain, conditional rollback was not very useful, because all listener exceptions are wrapped in a `ListenerExecutionFailedException`.

The first problem has been corrected, and the rules are now applied properly.
Further, the `ListenerFailedRuleBasedTransactionAttribute` is now provided.
It is a subclass of `RuleBasedTransactionAttribute`, with the only difference being that it is aware of the `ListenerExecutionFailedException` and uses the cause of such exceptions for the rule.
This transaction attribute can be used directly in the container or through a transaction advice.

The following example uses this rule:

```Java
@Bean
public AbstractMessageListenerContainer container() {
    ...
    container.setTransactionManager(transactionManager);
    RuleBasedTransactionAttribute transactionAttribute =
        new ListenerFailedRuleBasedTransactionAttribute();
    transactionAttribute.setRollbackRules(Collections.singletonList(
        new NoRollbackRuleAttribute(DontRollBackException.class)));
    container.setTransactionAttribute(transactionAttribute);
    ...
}
```

<a name="steeltoe-messaging-transaction-rollback"></a>
### A note on Rollback of Received Messages

RabbitMQ transactions apply only to messages and acks sent to the broker.
Consequently, when there is a rollback of a Spring transaction and a message has been received, Steeltoe RabbitMQ has to not only rollback the transaction but also manually reject the message (sort of a nack, but that is not what the specification calls it).
The action taken on message rejection is independent of transactions and depends on the `defaultRequeueRejected` property (default: `true`).
For more information about rejecting failed messages, see <a href="#steeltoe-messaging-async-listeners"></a>.

For more information about RabbitMQ transactions and their limitations, see [RabbitMQ Broker Semantics](https://www.rabbitmq.com/semantics.html).

>NOTE: Prior to RabbitMQ 2.7.0, such messages (and any that are unacked when a channel is closed or aborts) went to the back of the queue on a Rabbit broker.
Since 2.7.0, rejected messages go to the front of the queue, in a similar manner to JMS rolled back messages.

>NOTE: Previously, message requeue on transaction rollback was inconsistent between local transactions and when a `TransactionManager` was provided.
In the former case, the normal requeue logic (`AmqpRejectAndDontRequeueException` or `defaultRequeueRejected=false`) applied (see <a href="#steeltoe-messaging-async-listeners"></a>).
With a transaction manager, the message was unconditionally requeued on rollback.
The behavior is consistent and the normal requeue logic is applied in both cases.
To revert to the previous behavior, you can set the container's `alwaysRequeueWithTxManagerRollback` property to `true`.
See <a href="#steeltoe-messaging-container-attributes"></a>.

### Using `RabbitTransactionManager`

The [RabbitTransactionManager](https://docs.spring.io/spring-amqp/docs/latest_ga/api/org/springframework/amqp/rabbit/transaction/RabbitTransactionManager.html) is an alternative to executing Rabbit operations within, and synchronized with, external transactions.
This transaction manager is an implementation of the [`PlatformTransactionManager`](https://docs.spring.io/spring/docs/current/javadoc-api/org/springframework/transaction/PlatformTransactionManager.html) interface and should be used with a single Rabbit `ConnectionFactory`.

>IMPORTANT: This strategy is not able to provide XA transactions &#151; for example, in order to share transactions between messaging and database access.

Application code is required to retrieve the transactional Rabbit resources through `ConnectionFactoryUtils.getTransactionalResourceHolder(ConnectionFactory, boolean)` instead of a standard `Connection.createChannel()` call with subsequent channel creation.
When using Steeltoe RabbitMQ's [RabbitTemplate](https://docs.spring.io/spring-amqp/docs/latest_ga/api/org/springframework/amqp/rabbit/core/RabbitTemplate.html), it will autodetect a thread-bound Channel and automatically participate in its transaction.

With Java Configuration, you can setup a new RabbitTransactionManager by using the following bean:

```Java
@Bean
public RabbitTransactionManager rabbitTransactionManager() {
    return new RabbitTransactionManager(connectionFactory);
}
```

<a name="steeltoe-messaging-container-attributes"></a>
### Message Listener Container Configuration

There are quite a few options for configuring a `SimpleMessageListenerContainer` (SMLC) and a `DirectMessageListenerContainer` (DMLC) related to transactions and quality of service, and some of them interact with each other.
Properties that apply to the SMLC or DMLC are indicated by the check mark in the appropriate column.
See <a href="#steeltoe-messaging-choose-container"></a> for information to help you decide which container is appropriate for your application.

The following table shows the container property names and their equivalent attribute names (in parentheses) when using the namespace to configure a `<rabbit:listener-container/>`.
The `type` attribute on that element can be `simple` (default) or `direct` to specify an `SMLC` or `DMLC` respectively.
Some properties are not exposed by the namespace.
These are indicated by `N/A` for the attribute.

| Property (Attribute) | Description | SMLC | DMLC |
| -------- | ----------- | ---- | ---- |
| ackTimeout (N/A) | When `messagesPerAck` is set, this timeout is used as an alternative to send an ack. When a new message arrives, the count of unacked messages is compared to `messagesPerAck`, and the time since the last ack is compared to this value. If either condition is `true`, the message is acknowledged. When no new messages arrive and there are unacked messages, this timeout is approximate since the condition is only checked each `monitorInterval`. See also `messagesPerAck` and `monitorInterval` in this table. | | ![tickmark](images/tickmark.png) |
|acknowledgeMode (acknowledge) | * `NONE`: No acks are sent (incompatible with `channelTransacted=true`). RabbitMQ calls this "autoack", because the broker assumes all messages are acked without any action from the consumer.* `MANUAL`: The listener must acknowledge all messages by calling `Channel.basicAck()`.* `AUTO`: The container acknowledges the message automatically, unless the `MessageListener` throws an exception. Note that `acknowledgeMode` is complementary to `channelTransacted` &#151; if the channel is transacted, the broker requires a commit notification in addition to the ack. This is the default mode. See also `batchSize`. | ![tickmark](images/tickmark.png) | ![tickmark](images/tickmark.png) |
| adviceChain (advice-chain) | An array of AOP Advice to apply to the listener execution. This can be used to apply additional cross-cutting concerns, such as automatic retry in the event of broker death. Note that simple re-connection after an RabbitMQ error is handled by the `CachingConnectionFactory`, as long as the broker is still alive. | ![tickmark](images/tickmark.png) | ![tickmark](images/tickmark.png) |
| afterReceivePostProcessors (N/A) | An array of `MessagePostProcessor` instances that are invoked before invoking the listener. Post processors can implement `PriorityOrdered` or `Ordered`. The array is sorted with un-ordered members invoked last. If a post processor returns `null`, the message is discarded (and acknowledged, if appropriate). | ![tickmark](images/tickmark.png) | ![tickmark](images/tickmark.png) |
| alwaysRequeueWithTxManagerRollback (N/A) | Set to `true` to always requeue messages on rollback when a transaction manager is configured. | ![tickmark](images/tickmark.png) | ![tickmark](images/tickmark.png) |
| autoDeclare (auto-declare) | When set to `true` (default), the container uses a `RabbitAdmin` to redeclare all RabbitMQ objects (queues, exchanges, bindings), if it detects that at least one of its queues is missing during startup, perhaps because it is an `auto-delete` or an expired queue, but the redeclaration proceeds if the queue is missing for any reason. To disable this behavior, set this property to `false`. Note that the container fails to start if all of its queues are missing. NOTE: For `autoDeclare` to work, there must be exactly one `RabbitAdmin` in the context, or a reference to a specific instance must be configured on the container using the `rabbitAdmin` property. | ![tickmark](images/tickmark.png) | ![tickmark](images/tickmark.png) |
|autoStartup (auto-startup) | Flag to indicate that the container should start when the `ApplicationContext` does (as part of the `SmartLifecycle` callbacks, which happen after all beans are initialized). Defaults to `true`, but you can set it to `false` if your broker might not be available on startup and call `start()` later manually when you know the broker is ready. | ![tickmark](images/tickmark.png) | ![tickmark](images/tickmark.png) |
| batchSize (transaction-size) (batch-size) | When used with `acknowledgeMode` set to `AUTO`, the container tries to process up to this number of messages before sending an ack (waiting for each one up to the receive timeout setting). This is also when a transactional channel is committed. If the `prefetchCount` is less than the `batchSize`, it is increased to match the `batchSize`. | ![tickmark](images/tickmark.png) | |
|batchingStrategy (N/A) | The strategy used when debatchng messages. Default `SimpleDebatchingStrategy`. See <a href="steeltoe-messaging-template-batching"></a> and <a href="#steeltoe-messaging-receiving-batch"></a>. | ![tickmark](images/tickmark.png) | ![tickmark](images/tickmark.png) |
| channelTransacted (channel-transacted) | Boolean flag to signal that all messages should be acknowledged in a transaction (either manually or automatically). | ![tickmark](images/tickmark.png) | ![tickmark](images/tickmark.png) |
| concurrency (N/A) | `m-n` The range of concurrent consumers for each listener (min, max). If only `n` is provided, `n` is a fixed number of consumers. See <a href="#steeltoe-messaging-listener-concurrency"></a>. | ![tickmark](images/tickmark.png) | |
| concurrentConsumers (concurrency) | The number of concurrent consumers to initially start for each listener. See <a href="#steeltoe-messaging-listener-concurrency"></a>. | ![tickmark](images/tickmark.png) | |
| connectionFactory (connection-factory) | A reference to the `ConnectionFactory`. When configuring byusing the XML namespace, the default referenced bean name is `rabbitConnectionFactory`. | ![tickmark](images/tickmark.png) | ![tickmark](images/tickmark.png) |
| consecutiveActiveTrigger (min-consecutive-active) | The minimum number of consecutive messages received by a consumer, without a receive timeout occurring, when considering starting a new consumer. Also impacted by 'batchSize'. See <a href="#steeltoe-messaging-listener-concurrency"></a>. Default: 10. | ![tickmark](images/tickmark.png) | |
| consecutiveIdleTrigger (min-consecutive-idle) | The minimum number of receive timeouts a consumer must experience before considering stopping a consumer. Also impacted by 'batchSize'. See <a href="#steeltoe-messaging-listener-concurrency"></a>. Default: 10. | ![tickmark](images/tickmark.png) | ![tickmark](images/tickmark.png) |
| consumerBatchEnabled (batch-enabled) | If the `MessageListener` supports it, setting this to true enables batching of discrete messages, up to `batchSize`; a partial batch will be delivered if no new messages arrive in `receiveTimeout`. When this is false, batching is supported only for batches created by a producer; see <a href="steeltoe-messaging-template-batching"></a>. | ![tickmark](images/tickmark.png) | |
| consumerStartTimeout (N/A) | The time in milliseconds to wait for a consumer thread to start. If this time elapses, an error log is written. An example of when this might happen is if a configured `taskExecutor` has insufficient threads to support the container `concurrentConsumers`. See <a href="#steeltoe-messaging-threading"></a>. Default: 60000 (one minute). | ![tickmark](images/tickmark.png) | |
| consumerTagStrategy (consumer-tag-strategy) | Set an implementation of <a href="#steeltoe-messaging-consumer-tags">ConsumerTagStrategy</a>, enabling the creation of a (unique) tag for each consumer. | ![tickmark](images/tickmark.png) | ![tickmark](images/tickmark.png) |
| consumersPerQueue (consumers-per-queue) | The number of consumers to create for each configured queue. See <a href="#steeltoe-messaging-listener-concurrency"></a>. | | ![tickmark](images/tickmark.png) |
|debatchingEnabled (N/A) | When true, the listener container will debatch batched messages and invoke the listener with each message from the batch. Default: true. See <a href="steeltoe-messaging-template-batching"></a> and <a href="#steeltoe-messaging-receiving-batch"></a>. | ![tickmark](images/tickmark.png) | ![tickmark](images/tickmark.png) |
| declarationRetries (declaration-retries) | The number of retry attempts when passive queue declaration fails. Passive queue declaration occurs when the consumer starts or, when consuming from multiple queues, when not all queues were available during initialization. When none of the configured queues can be passively declared (for any reason) after the retries are exhausted, the container behavior is controlled by the `missingQueuesFatal` property, described earlier. Default: Three retries (for a total of four attempts). | ![tickmark](images/tickmark.png) | |
| defaultRequeueRejected (requeue-rejected) | Determines whether messages that are rejected because the listener threw an exception should be requeued or not. Default: `true`. | ![tickmark](images/tickmark.png) | ![tickmark](images/tickmark.png) |
| errorHandler (error-handler) | A reference to an `ErrorHandler` strategy for handling any uncaught exceptions that may occur during the execution of the MessageListener. Default: `ConditionalRejectingErrorHandler` | ![tickmark](images/tickmark.png) | ![tickmark](images/tickmark.png) |
| exclusive (exclusive) | Determines whether the single consumer in this container has exclusive access to the queues. The concurrency of the container must be 1 when this is `true`. If another consumer has exclusive access, the container tries to recover the consumer, according to the `recovery-interval` or `recovery-back-off`. When using the namespace, this attribute appears on the `<rabbit:listener/>` element along with the queue names. Default: `false`. | ![tickmark](images/tickmark.png) | ![tickmark](images/tickmark.png) |
| exclusiveConsumerExceptionLogger (N/A) | An exception logger used when an exclusive consumer cannot gain access to a queue. By default, this is logged at the `WARN` level.| ![tickmark](images/tickmark.png) | ![tickmark](images/tickmark.png) |
| failedDeclaration RetryInterval (failed-declaration-retry-interval) | The interval between passive queue declaration retry attempts. Passive queue declaration occurs when the consumer starts or, when consuming from multiple queues, when not all queues were available during initialization. Default: 5000 (five seconds). | ![tickmark](images/tickmark.png) | ![tickmark](images/tickmark.png) |
| forceCloseChannel (N/A) | If the consumers do not respond to a shutdown within `shutdownTimeout`, if this is `true`, the channel will be closed, causing any unacked messages to be requeued. Defaults to `true` since 2.0. You can set it to `false` to revert to the previous behavior. | ![tickmark](images/tickmark.png) | ![tickmark](images/tickmark.png) |
| (group) | This is available only when using the namespace. When specified, a bean of type `Collection<MessageListenerContainer>` is registered with this name, and the container for each `<listener/>` element is added to the collection. This allows, for example, starting and stopping the group of containers by iterating over the collection. If multiple `<listener-container/>` elements have the same group value, the containers in the collection form an aggregate of all containers so designated. | ![tickmark](images/tickmark.png) | ![tickmark](images/tickmark.png) |
| idleEventInterval (idle-event-interval) | See <a href="#steeltoe-messaging-idle-containers"></a>. | ![tickmark](images/tickmark.png) | ![tickmark](images/tickmark.png) |
| maxConcurrentConsumers (max-concurrency) | The maximum number of concurrent consumers to start, if needed, on demand. Must be greater than or equal to 'concurrentConsumers'. See <a href="#steeltoe-messaging-listener-concurrency"></a>. | ![tickmark](images/tickmark.png) | |
| messagesPerAck (N/A) | The number of messages to receive between acks. Use this to reduce the number of acks sent to the broker (at the cost of increasing the possibility of redelivered messages). Generally, you should set this property only on high-volume listener containers. If this is set and a message is rejected (exception thrown), pending acks are acknowledged and the failed message is rejected. Not allowed with transacted channels. If the `prefetchCount` is less than the `messagesPerAck`, it is increased to match the `messagesPerAck`. Default: ack every message. See also `ackTimeout` in this table. | | ![tickmark](images/tickmark.png) |
| mismatchedQueuesFatal (mismatched-queues-fatal) | When the container starts, if this property is `true` (default: `false`), the container checks that all queues declared in the context are compatible with queues already on the broker. If mismatched properties (such as `auto-delete`) or arguments (skuch as `x-message-ttl`) exist, the container (and application context) fails to start with a fatal exception. If the problem is detected during recovery (for example, after a lost connection), the container is stopped. There must be a single `RabbitAdmin` in the application context (or one specifically configured on the container by using the `rabbitAdmin` property). Otherwise, this property must be `false`. NOTE: If the broker is not available during initial startup, the container starts and the conditions are checked when the connection is established. IMPORTANT: The check is done against all queues in the context, not just the queues that a particular listener is configured to use. If you wish to limit the checks to just those queues used by a container, you should configure a separate `RabbitAdmin` for the container, and provide a reference to it using the `rabbitAdmin` property. See <a href="#steeltoe-messaging-conditional-declaration"></a> for more information. IMPORTANT: Mismatched queue argument detection is disabled while starting a container for a `@RabbitListener` in a bean that is marked `@Lazy`. This is to avoid a potential deadlock which can delay the start of such containers for up to 60 seconds. Applications using lazy listener beans should check the queue arguments before getting a reference to the lazy bean.| ![tickmark](images/tickmark.png) | ![tickmark](images/tickmark.png) |
| missingQueuesFatal (missing-queues-fatal) | When set to `true` (default), if none of the configured queues are available on the broker, it is considered fatal. This causes the application context to fail to initialize during startup. Also, when the queues are deleted while the container is running, by default, the consumers make three retries to connect to the queues (at five second intervals) and stop the container if these attempts fail. This was not configurable in previous versions. When set to `false`, after making the three retries, the container goes into recovery mode, as with other problems, such as the broker being down. The container tries to recover according to the `recoveryInterval` property. During each recovery attempt, each consumer again tries four times to passively declare the queues at five second intervals. This process continues indefinitely.  This global property is not applied to any containers that have an explicit `missingQueuesFatal` property set. The default retry properties (three retries at five-second intervals) can be overridden by setting the properties below. IMPORTANT: Missing queue detection is disabled while starting a container for a `@RabbitListener` in a bean that is marked `@Lazy`. This is to avoid a potential deadlock which can delay the start of such containers for up to 60 seconds. Applications using lazy listener beans should check the queue(s) before getting a reference to the lazy bean. | ![tickmark](images/tickmark.png) | ![tickmark](images/tickmark.png) |
| monitorInterval (monitor-interval) | With the DMLC, a task is scheduled to run at this interval to monitor the state of the consumers and recover any that have failed. | |![tickmark](images/tickmark.png) |
| noLocal (N/A) | Set to `true` to disable delivery from the server to consumers messages published on the same channel's connection. | ![tickmark](images/tickmark.png) | ![tickmark](images/tickmark.png) |
| phase (phase) | When `autoStartup` is `true`, the lifecycle phase within which this container should start and stop. The lower the value, the earlier this container starts and the later it stops. The default is `Integer.MAX_VALUE`, meaning the container starts as late as possible and stops as soon as possible. | ![tickmark](images/tickmark.png) | ![tickmark](images/tickmark.png) |
| possibleAuthenticationFailureFatal (possible-authentication-failure-fatal) | When set to `true` (default), if a `PossibleAuthenticationFailureException` is thrown during connection, it is considered fatal. This causes the application context to fail to initialize during startup. When set to `false`, after making the 3 retries, the container goes into recovery mode, as with other problems, such as the broker being down. The container tries to recover according to the `recoveryInterval` property. During each recovery attempt, each consumer will again try 4 times to start. This process continues indefinitely. You can also use a properties bean to set the property globally for all containers. This global property is not applied to any containers that have an explicit `missingQueuesFatal` property set. The default retry properties (3 retries at 5 second intervals) can be overridden using the properties after this one. | ![tickmark](images/tickmark.png) | ![tickmark](images/tickmark.png) |
| prefetchCount (prefetch) | The number of unacknowledged messages that can be outstanding at each consumer. The higher this value is, the faster the messages can be delivered, but the higher the risk of non-sequential processing. Ignored if the `acknowledgeMode` is `NONE`. This is increased, if necessary, to match the `batchSize` or `messagePerAck`. Defaults to 250 since 2.0. You can set it to 1 to revert to the previous behavior. IMPORTANT: There are scenarios where the prefetch value should be low &#151; for example, with large messages, especially if the processing is slow (messages could add up to a large amount of memory in the client process), and if strict message ordering is necessary (the prefetch value should be set back to 1 in this case). Also, with low-volume messaging and multiple consumers (including concurrency within a single listener container instance), you may wish to reduce the prefetch to get a more even distribution of messages across consumers. | ![tickmark](images/tickmark.png) | ![tickmark](images/tickmark.png) |
| rabbitAdmin (admin) | When a listener container listens to at least one auto-delete queue and it is found to be missing during startup, the container uses a `RabbitAdmin` to declare the queue and any related bindings and exchanges. If such elements are configured to use conditional declaration (see <a href="#steeltoe-messaging-conditional-declaration"></a>), the container must use the admin that was configured to declare those elements. Specify that admin here. It is required only when using auto-delete queues with conditional declaration. If you do not wish the auto-delete queues to be declared until the container is started, set `auto-startup` to `false` on the admin. Defaults to a `RabbitAdmin` that declares all non-conditional elements. | ![tickmark](images/tickmark.png) | ![tickmark](images/tickmark.png) |
| receiveTimeout (receive-timeout) | The maximum time to wait for each message. If `acknowledgeMode=NONE`, this has very little effect &#151; the container spins round and asks for another message. It has the biggest effect for a transactional `Channel` with `batchSize > 1`, since it can cause messages already consumed not to be acknowledged until the timeout expires. When `consumerBatchEnabled` is true, a partial batch will be delivered if this timeout occurs before a batch is complete. | ![tickmark](images/tickmark.png) | |
| recoveryBackOff (recovery-back-off) | Specifies the `BackOff` for intervals between attempts to start a consumer if it fails to start for non-fatal reasons. Default is `FixedBackOff` with unlimited retries every five seconds. Mutually exclusive with `recoveryInterval`. | ![tickmark](images/tickmark.png) | ![tickmark](images/tickmark.png) |
| recoveryInterval (recovery-interval) | Determines the time in milliseconds between attempts to start a consumer if it fails to start for non-fatal reasons. Default: 5000. Mutually exclusive with `recoveryBackOff`. | ![tickmark](images/tickmark.png) | ![tickmark](images/tickmark.png) |
| retryDeclarationInterval (missing-queue-retry-interval)| If a subset of the configured queues are available during consumer initialization, the consumer starts consuming from those queues. The consumer tries to passively declare the missing queues by using this interval. When this interval elapses, the 'declarationRetries' and 'failedDeclarationRetryInterval' is used again. If there are still missing queues, the consumer again waits for this interval before trying again. This process continues indefinitely until all queues are available. Default: 60000 (one minute). | ![tickmark](images/tickmark.png) | |
| shutdownTimeout (N/A) | When a container shuts down (for example, if its enclosing `ApplicationContext` is closed), it waits for in-flight messages to be processed up to this limit. Defaults to five seconds. | ![tickmark](images/tickmark.png) | ![tickmark](images/tickmark.png) |
| startConsumerMinInterval (min-start-interval) | The time in milliseconds that must elapse before each new consumer is started on demand. See <a href="#steeltoe-messaging-listener-concurrency"></a>. Default: 10000 (10 seconds). | ![tickmark](images/tickmark.png) | |
| statefulRetryFatalWithNullMessageId (N/A) | When using a stateful retry advice, if a message with a missing `messageId` property is received, it is considered fatal for the consumer (it is stopped) by default. Set this to `false` to discard (or route to a dead-letter queue) such messages. | ![tickmark](images/tickmark.png) | ![tickmark](images/tickmark.png) |
| stopConsumerMinInterval (min-stop-interval) | The time in milliseconds that must elapse before a consumer is stopped since the last consumer was stopped when an idle consumer is detected. See <a href="#steeltoe-messaging-listener-concurrency"></a>. Default: 60000 (one minute). | ![tickmark](images/tickmark.png) | |
| taskExecutor (task-executor) | A reference to a Spring `TaskExecutor` (or standard JDK 1.5+ `Executor`) for executing listener invokers. Default is a `SimpleAsyncTaskExecutor`, using internally managed threads. | ![tickmark](images/tickmark.png) | ![tickmark](images/tickmark.png) |
| taskScheduler (task-scheduler) | With the DMLC, the scheduler used to run the monitor task at the 'monitorInterval'. | | ![tickmark](images/tickmark.png) |
| transactionManager (transaction-manager) | External transaction manager for the operation of the listener. Also complementary to `channelTransacted` &#151; if the `Channel` is transacted, its transaction is synchronized with the external transaction. | ![tickmark](images/tickmark.png) | ![tickmark](images/tickmark.png) |

<a name="steeltoe-messaging-listener-concurrency"></a>
## Listener Concurrency

### SimpleMessageListenerContainer

By default, the listener container starts a single consumer that receives messages from the queues.

When examining the table in the previous section, you can see a number of properties and attributes that control concurrency.
The simplest is `concurrentConsumers`, which creates that (fixed) number of consumers that concurrently process messages.

Prior to version 1.3.0, this was the only setting available and the container had to be stopped and started again to change the setting.

Since version 1.3.0, you can now dynamically adjust the `concurrentConsumers` property.
If it is changed while the container is running, consumers are added or removed as necessary to adjust to the new setting.

In addition, a new property called `maxConcurrentConsumers` has been added and the container dynamically adjusts the concurrency based on workload.
This works in conjunction with four additional properties: `consecutiveActiveTrigger`, `startConsumerMinInterval`, `consecutiveIdleTrigger`, and `stopConsumerMinInterval`.
With the default settings, the algorithm to increase consumers works as follows:

If the `maxConcurrentConsumers` has not been reached and an existing consumer is active for ten consecutive cycles AND at least 10 seconds has elapsed since the last consumer was started, a new consumer is started.
A consumer is considered active if it received at least one message in `batchSize` * `receiveTimeout` milliseconds.

With the default settings, the algorithm to decrease consumers works as follows:

If there are more than `concurrentConsumers` running and a consumer detects ten consecutive timeouts (idle) AND the last consumer was stopped at least 60 seconds ago, a consumer is stopped.
The timeout depends on the `receiveTimeout` and the `batchSize` properties.
A consumer is considered idle if it receives no messages in `batchSize` * `receiveTimeout` milliseconds.
So, with the default timeout (one second) and a `batchSize` of four, stopping a consumer is considered after 40 seconds of idle time (four timeouts correspond to one idle detection).

>NOTE: Practically, consumers can be stopped only if the whole container is idle for some time.
This is because the broker shares its work across all the active consumers.

Each consumer uses a single channel, regardless of the number of configured queues.

The `concurrentConsumers` and `maxConcurrentConsumers` properties can be set with the `concurrency` property &#151; for example, `2-4`.

### Using `DirectMessageListenerContainer`

With this container, concurrency is based on the configured queues and `consumersPerQueue`.
Each consumer for each queue uses a separate channel, and the concurrency is controlled by the rabbit client library.
By default, at the time of writing, it uses a pool of `DEFAULT_NUM_THREADS = Runtime.getRuntime().availableProcessors() * 2` threads.

You can configure a `taskExecutor` to provide the required maximum concurrency.

<a name="steeltoe-messaging-exclusive-consumer"></a>
## Exclusive Consumer

You can configure the listener container with a single exclusive consumer.
This prevents other containers from consuming from the queues until the current consumer is cancelled.
The concurrency of such a container must be `1`.

When using exclusive consumers, other containers try to consume from the queues according to the `recoveryInterval` property and log a `WARN` message if the attempt fails.

<a name="steeltoe-messaging-listener-queues"></a>
## Listener Container Queues

Version 1.3 introduced a number of improvements for handling multiple queues in a listener container.

The container must be configured to listen on at least one queue.
This was the case previously, too, but now queues can be added and removed at runtime.
The container recycles (cancels and re-creates) the consumers when any pre-fetched messages have been processed.
See the [Javadoc](https://docs.spring.io/spring-amqp/docs/latest-ga/api/org/springframework/amqp/rabbit/listener/AbstractMessageListenerContainer.html) for the `addQueues`, `addQueueNames`, `removeQueues` and `removeQueueNames` methods.
When removing queues, at least one queue must remain.

A consumer now starts if any of its queues are available.
Previously, the container would stop if any queues were unavailable.
Now, this is only the case if none of the queues are available.
If not all queues are available, the container tries to passively declare (and consume from) the missing queues every 60 seconds.

Also, if a consumer receives a cancel from the broker (for example, if a queue is deleted) the consumer tries to recover, and the recovered consumer continues to process messages from any other configured queues.
Previously, a cancel on one queue cancelled the entire consumer and, eventually, the container would stop due to the missing queue.

If you wish to permanently remove a queue, you should update the container before or after deleting to queue, to avoid future attempts trying to consume from it.

## Resilience: Recovering from Errors and Broker Failures

Some of the key (and most popular) high-level features that Steeltoe RabbitMQ provides are to do with recovery and automatic re-connection in the event of a protocol error or broker failure.
We have seen all the relevant components already in this guide, but it should help to bring them all together here and call out the features and recovery scenarios individually.

The primary reconnection features are enabled by the `CachingConnectionFactory` itself.
It is also often beneficial to use the `RabbitAdmin` auto-declaration features.
In addition, if you care about guaranteed delivery, you probably also need to use the `channelTransacted` flag in `RabbitTemplate` and `SimpleMessageListenerContainer` and the `AcknowledgeMode.AUTO` (or manual if you do the acks yourself) in the `SimpleMessageListenerContainer`.

<a name="steeltoe-messaging-automatic-declaration"></a>
### Automatic Declaration of Exchanges, Queues, and Bindings

The `RabbitAdmin` component can declare exchanges, queues, and bindings on startup.
It does this lazily, through a `ConnectionListener`.
Consequently, if the broker is not present on startup, it does not matter.
The first time a `Connection` is used (for example,
by sending a message) the listener fires and the admin features is applied.
A further benefit of doing the auto declarations in a listener is that, if the connection is dropped for any reason (for example,
broker death, network glitch, and others), they are applied again when the connection is re-established.

>NOTE: Queues declared this way must have fixed names &#151; either explicitly declared or generated by the framework for `AnonymousQueue` instances.
Anonymous queues are non-durable, exclusive, and auto-deleting.

>IMPORTANT: Automatic declaration is performed only when the `CachingConnectionFactory` cache mode is `CHANNEL` (the default).
This limitation exists because exclusive and auto-delete queues are bound to the connection.

`RabbitAdmin` detects beans of type `DeclarationCustomizer` and apply the function before actually processing the declaration.
This is useful, for example, to set a new argument (property) before it has first class support within the framework.

```Java
@Bean
public DeclarableCustomizer customizer() {
    return dec -> {
        if (dec instanceof Queue && ((Queue) dec).getName().equals("my.queue")) {
            dec.addArgument("some.new.queue.argument", true);
        }
        return dec;
    };
}
```

It is also useful in projects that don't provide direct access to the `Declarable` bean definitions.

See also <a href="#steeltoe-messaging-auto-recovery"></a>.

[[retry]]
### Failures in Synchronous Operations and Options for Retry

If you lose your connection to the broker in a synchronous sequence when using `RabbitTemplate` (for instance), Steeltoe RabbitMQ throws an `AmqpException` (usually, but not always, `AmqpIOException`).
We do not try to hide the fact that there was a problem, so you have to be able to catch and respond to the exception.
The easiest thing to do if you suspect that the connection was lost (and it was not your fault) is to try the operation again.
You can do this manually, or you could look at using Spring Retry to handle the retry (imperatively or declaratively).

Spring Retry provides a couple of AOP interceptors and a great deal of flexibility to specify the parameters of the retry (number of attempts, exception types, backoff algorithm, and others).
Steeltoe RabbitMQ also provides some convenience factory beans for creating Spring Retry interceptors in a convenient form for RabbitMQ use cases, with strongly typed callback interfaces that you can use to implement custom recovery logic.
See the Javadoc and properties of `StatefulRetryOperationsInterceptor` and `StatelessRetryOperationsInterceptor` for more detail.
Stateless retry is appropriate if there is no transaction or if a transaction is started inside the retry callback.
Note that stateless retry is simpler to configure and analyze than stateful retry, but it is not usually appropriate if there is an ongoing transaction that must be rolled back or definitely is going to roll back.
A dropped connection in the middle of a transaction should have the same effect as a rollback.
Consequently, for reconnections where the transaction is started higher up the stack, stateful retry is usually the best choice.
Stateful retry needs a mechanism to uniquely identify a message.
The simplest approach is to have the sender put a unique value in the `MessageId` message property.
The provided message converters provide an option to do this: you can set `createMessageIds` to `true`.
Otherwise, you can inject a `MessageKeyGenerator` implementation into the interceptor.
The key generator must return a unique key for each message.
In versions prior to version 2.0, a `MissingMessageIdAdvice` was provided.
It enabled messages without a `messageId` property to be retried exactly once (ignoring the retry settings).
This advice is no longer provided, since, along with `spring-retry` version 1.2, its functionality is built into the interceptor and message listener containers.

>NOTE: For backwards compatibility, a message with a null message ID is considered fatal for the consumer (consumer is stopped) by default (after one retry).
To replicate the functionality provided by the `MissingMessageIdAdvice`, you can set the `statefulRetryFatalWithNullMessageId` property to `false` on the listener container.
With that setting, the consumer continues to run and the message is rejected (after one retry).
It is discarded or routed to the dead letter queue (if one is configured).

A builder API is provided to aid in assembling these interceptors by using Java (in `@Configuration` classes).
The following example shows how to do so:

```Java
@Bean
public StatefulRetryOperationsInterceptor interceptor() {
	return RetryInterceptorBuilder.stateful()
			.maxAttempts(5)
			.backOffOptions(1000, 2.0, 10000) // initialInterval, multiplier, maxInterval
			.build();
}
```

Only a subset of retry capabilities can be configured this way.
More advanced features would need the configuration of a `RetryTemplate` as a Spring bean.
See the [Spring Retry Javadoc](https://docs.spring.io/spring-retry/docs/api/current/) for complete information about available policies and their configuration.

<a name="steeltoe-messaging-batch-retry"></a>
### Retry with Batch Listeners

It is not recommended to configure retry with a batch listener, unless the batch was created by the producer, in a single record.
See <a href="#steeltoe-messaging-de-batching"></a> for information about consumer and producer-created batches.
With a consumer-created batch, the framework has no knowledge about which message in the batch caused the failure so recovery after the retries are exhausted is not possible.
With producer-created batches, since there is only one message that actually failed, the whole message can be recovered.
Applications may want to inform a custom recoverer where in the batch the failure occurred, perhaps by setting an index property of the thrown exception.

A retry recoverer for a batch listener must implement `MessageBatchRecoverer`.

<a name="steeltoe-messaging-async-listeners"></a>
### Message Listeners and the Asynchronous Case

If a `MessageListener` fails because of a business exception, the exception is handled by the message listener container, which then goes back to listening for another message.
If the failure is caused by a dropped connection (not a business exception), the consumer that is collecting messages for the listener has to be cancelled and restarted.
The `SimpleMessageListenerContainer` handles this seamlessly, and it leaves a log to say that the listener is being restarted.
In fact, it loops endlessly, trying to restart the consumer.
Only if the consumer is very badly behaved indeed will it give up.
One side effect is that if the broker is down when the container starts, it keeps trying until a connection can be established.

Business exception handling, as opposed to protocol errors and dropped connections, might need more thought and some custom configuration, especially if transactions or container acks are in use.
Prior to 2.8.x, RabbitMQ had no definition of dead letter behavior.
Consequently, by default, a message that is rejected or rolled back because of a business exception can be redelivered endlessly.
To put a limit on the client on the number of re-deliveries, one choice is a `StatefulRetryOperationsInterceptor` in the advice chain of the listener.
The interceptor can have a recovery callback that implements a custom dead letter action &#151; whatever is appropriate for your particular environment.

Another alternative is to set the container's `defaultRequeueRejected` property to `false`.
This causes all failed messages to be discarded.
When using RabbitMQ 2.8.x or higher, this also facilitates delivering the message to a dead letter exchange.

Alternatively, you can throw a `AmqpRejectAndDontRequeueException`.
Doing so prevents message requeuing, regardless of the setting of the `defaultRequeueRejected` property.

An `ImmediateRequeueAmqpException` is introduced to perform exactly the opposite logic: the message will be requeued, regardless of the setting of the `defaultRequeueRejected` property.

Often, a combination of both techniques is used.
You can use a `StatefulRetryOperationsInterceptor` in the advice chain with a `MessageRecoverer` that throws an `AmqpRejectAndDontRequeueException`.
The `MessageRecover` is called when all retries have been exhausted.
The `RejectAndDontRequeueRecoverer` does exactly that.
The default `MessageRecoverer` consumes the errant message and emits a `WARN` message.

A new `RepublishMessageRecoverer` is provided, to allow publishing of failed messages after retries are exhausted.

When a recoverer consumes the final exception, the message is ack'd and is not sent to the dead letter exchange, if any.

>NOTE: When `RepublishMessageRecoverer` is used on the consumer side, the received message has `deliveryMode` in the `receivedDeliveryMode` message property.
In this case the `deliveryMode` is `null`.
That means a `NON_PERSISTENT` delivery mode on the broker.
You can configure the `RepublishMessageRecoverer` for the `deliveryMode` to set into the message to republish if it is `null`.
By default, it uses `MessageProperties` default value - `MessageDeliveryMode.PERSISTENT`.

The following example shows how to set a `RepublishMessageRecoverer` as the recoverer:

```Java
@Bean
RetryOperationsInterceptor interceptor() {
	return RetryInterceptorBuilder.stateless()
			.maxAttempts(5)
			.recoverer(new RepublishMessageRecoverer(amqpTemplate(), "something", "somethingelse"))
			.build();
}
```

The `RepublishMessageRecoverer` publishes the message with additional information in message headers, such as the exception message, stack trace, original exchange, and routing key.
Additional headers can be added by creating a subclass and overriding `additionalHeaders()`.
The `deliveryMode` (or any other properties) can also be changed in the `additionalHeaders()`, as follows:

```Java
RepublishMessageRecoverer recoverer = new RepublishMessageRecoverer(amqpTemplate, "error") {

    protected Map<? extends String, ? extends Object> additionalHeaders(Message message, Throwable cause) {
        message.getMessageProperties()
            .setDeliveryMode(message.getMessageProperties().getReceivedDeliveryMode());
        return null;
    }

};
```

The stack trace may be truncated if it is too large; this is because all headers have to fit in a single frame.
By default, if the stack trace would cause less than 20,000 bytes ('headroom') to be available for other headers, it will be truncated.
This can be adjusted by setting the recoverer's `frameMaxHeadroom` property, if you need more or less space for other headers.
The exception message is included in this calculation, and the amount of stack trace will be maximized using the following algorithm:

* if the stack trace alone would exceed the limit, the exception message header will be truncated to 97 bytes plus `...` and the stack trace is truncated too.
* if the stack trace is small, the message will be truncated (plus `...`) to fit in the available bytes (but the message within the stack trace itself is truncated to 97 bytes plus `...`).

Whenever a truncation of any kind occurs, the original exception will be logged to retain the complete information.

An `ImmediateRequeueMessageRecoverer` is  added to throw an `ImmediateRequeueAmqpException`, which notifies a listener container to requeue the current failed message.

### Exception Classification for Spring Retry

Spring Retry has a great deal of flexibility for determining which exceptions can invoke retry.
The default configuration retries for all exceptions.
Given that user exceptions are wrapped in a `ListenerExecutionFailedException`, we need to ensure that the classification examines the exception causes.
The default classifier looks only at the top level exception.

Since Spring Retry 1.0.3, the `BinaryExceptionClassifier` has a property called `traverseCauses` (default: `false`).
When `true`, it travers exception causes until it finds a match or there is no cause.

To use this classifier for retry, you can use a `SimpleRetryPolicy` created with the constructor that takes the max attempts, the `Map` of `Exception` instances, and the boolean (`traverseCauses`) and inject this policy into the `RetryTemplate`.

## Debugging

Steeltoe RabbitMQ provides extensive logging, especially at the `DEBUG` level.

If you wish to monitor the AMQP protocol between the application and broker, you can use a tool such as WireShark, which has a plugin to decode the protocol.
Alternatively, the RabbitMQ Java client comes with a very useful class called `Tracer`.
When run as a `main`, by default, it listens on port 5673 and connects to port 5672 on localhost.
You can run it and change your connection factory configuration to connect to port 5673 on localhost.
It displays the decoded protocol on the console.
Refer to the `Tracer` Javadoc for more information.
