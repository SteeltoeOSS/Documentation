
# Reference

This section explores the interfaces and classes that are the essential components for developing applications with Steeltoe RabbitMQ.


## Messaging Abstractions

Steeltoe RabbitMQ consists of two packages (each represented by a nuget in the distribution): `Steeltoe.Messaging.MessagingBase` and `Steeltoe.Messaging.RabbitMQ`.
The `Steeltoe.Messaging.MessagingBase` package contains the `Steeltoe.Messaging` namespace.
Within that package you can find the classes that represent the core messaging functionality that pertains to any messaging system.
Our intention is to provide generic abstractions that do not rely on any particular messaging implementation or client library.
End-user code can be more portable across vendor implementations, as much of it can be developed against the abstraction layer only.
These abstractions are then implemented by messaging-specific packages, such as `Steeltoe.Messaging.RabbitMQ`.
There is currently only a RabbitMQ implementation at this point, but in the future we anticipate several others.

Since RabbitMQ is based on the AMQP and it operates at a protocol level, in principle, you should be able to use the Steeltoe package with any broker that supports the same protocol version. Currently we are only testing with the RabbitMQ broker at present.

This overview assumes that you are already familiar with the basics of the AMQP specification.
If not, have a look at the resources listed in [further-reading](./further-reading.md).

### IMessage

The AMQP specification does not define a `Message` class or interface.
Instead, when performing an operation such as `BasicPublish()`, the content is passed as a byte-array, and additional message properties are passed as separate arguments.
Steeltoe defines a `Message` class and a `IMessage` interface as part of a more general messaging domain model representation.
The purpose of the `Message` class is to encapsulate the body and properties of a message within a single instance so that interacting with messaging APIs can be simpler.
The intent is that the `Message` class be generic enough for use with any underlying messaging infrastructure, including RabbitMQ.

The following shows the `Message` class as well as the various important interfaces it references:

```csharp
public class Message<P> : AbstractMessage, IMessage<P>
{
    protected readonly P payload;
    protected readonly IMessageHeaders headers;

    protected internal Message(P payload)
        : this(payload, new MessageHeaders())
    {
    }
    protected internal Message(P payload, IDictionary<string, object> headers)
        : this(payload, new MessageHeaders(headers, null, null))
    {
    }

    protected internal Message(P payload, IMessageHeaders headers)
    {
        ...
    }

    public P Payload => payload;
    public IMessageHeaders Headers => headers;
}

public interface IMessage
{
    object Payload { get; }
    IMessageHeaders Headers { get; }
}

public interface IMessage<out T> : IMessage
{
    new T Payload { get; }
}

public interface IMessageHeaders : IDictionary<string, object>
{
    T Get<T>(string key);
    string Id { get; }
    long? Timestamp { get; }
    object ReplyChannel { get; }
    object ErrorChannel { get; }
}
```

When creating messages you should use one of the static `Create(..)` methods on `Steeltoe.Messaging.Message`. Note that once you create a message its message headers are considered to be immutable.

The `IMessageHeaders` interface defines a few common properties, such as `Id`, `Timestamp`, `ReplyChannel`, etc. each used across all Steeltoe supported messaging systems.
There are also extension methods in `Steeltoe.Messaging.RabbitMQ.Extensions.MessageHeaderExtensions` which can be used to access RabbitMQ specific headers such as `AppId`, `ClusterId`, etc.

If you need to update headers in a previously created message you must obtain a mutable accessor to do so. Each messaging system (e.g. RabbitMQ, Kafka, etc.) has an accessor specific to that messaging system.
For RabbitMQ you should use the `RabbitHeaderAccessor` and call `GetMutableAccessor(..)` to obtain an appropriate accessor. You can then use it to modify the headers associated with the message. You can also add or remove user-defined 'headers' by calling `SetHeader(string key, object value)` method as needed.

### IExchange

The `IExchange` interface represents an AMQP exchange, which is what a message producer sends to.
Each Exchange within a virtual host of a broker has a unique name as well as a few other properties.
The following example shows the `IExchange` interface:

```csharp
public interface IExchange : IDeclarable, IServiceNameAware
{
    string ExchangeName { get; set; }

    string Type { get; }

    bool IsDurable { get; set; }

    bool IsAutoDelete { get; set; }

    bool IsDelayed { get; set; }

    bool IsInternal { get; set; }
}
```

As you can see, an `IExchange` also has a 'type' represented by constants defined in `ExchangeType`.
The basic types are: `direct`, `topic`, `fanout`, and `headers`.
In the `Steeltoe.Messaging.RabbitMQ` package, you can find implementations of the `IExchange` interface for each of those types.
The behavior varies across these `IExchange` types in terms of how they handle bindings to queues.
For example, a `DirectExchange` lets a queue be bound by a fixed routing key (often the queue's name).
A `TopicExchange` supports bindings with routing patterns that may include the '\*' and '#' wildcards for 'exactly-one' and 'zero-or-more', respectively.
The `FanoutExchange` publishes to all queues that are bound to it without taking any routing key into consideration.
For much more information about these and the other Exchange types, see [further-reading](./further-reading.md).

>The AMQP specification also requires that any broker provide a "default" direct exchange that has no name.
All queues that are declared are bound to that default `IExchange` with their names as routing keys.
You can learn more about the default Exchange's usage within Steeltoe RabbitMQ in [RabbitTemplate](#rabbittemplate).

### IQueue

The `IQueue` interface represents the component from which a message consumer receives messages.
Like the various `IExchange` classes, our implementation is intended to be an abstract representation of this core AMQP type.
The following listing shows the `Queue` class which implements `IQueue`:

```csharp
    public class Queue : AbstractDeclarable, IQueue, ICloneable
{
    public string ServiceName { get; set; }
    public string QueueName { get; set; }
    public string ActualName { get; set; }
    public bool IsDurable { get; set; }
    public bool IsExclusive { get; set; }
    public bool IsAutoDelete { get; set; }

    public Queue(string queueName)
    : this(queueName, true, false, false)
    {
    }
    ...
}
```

Notice that the constructor takes the queue name as a argument.
Depending on the broker implementation, the admin template may provide methods for generating a uniquely named queue.
Such queues can be useful as a "reply-to" address or in other *temporary* situations.
For that reason, the `IsExclusive` and `IsAutoDelete` properties of an auto-generated queue would both be set to `true`.

### IBinding

Given that a producer sends to an exchange and a consumer receives from a queue, the bindings that connect queues to exchanges are critical for connecting those producers and consumers via messaging.
In Steeltoe RabbitMQ, we define a `IBinding` interface and appropriate classes to represent those connections.
This section reviews the basic options for binding queues to exchanges.

You can bind a queue to a `DirectExchange` with a fixed routing key, as follows:

```csharp
IBinding b = new ExchangeBinding(bindingName, someQueue, someDirectExchange, "foo.bar");
```

You can bind a queue to a `TopicExchange` with a routing pattern, as the following example shows:

```csharp
IBinding b = new ExchangeBinding(bindingName, someQueue, someTopicExchange, "foo.*");
```

You can bind a queue to a `FanoutExchange` with no routing key, as the following example shows:

```csharp
IBinding b = new ExchangeBinding(bindingName, someQueue, someFanoutExchange);
```

We also provide a `BindingBuilder` to facilitate a "fluent API" style, as the following example shows:

```csharp
IBinding b = BindingBuilder.Bind(someQueue).To(someTopicExchange).With("foo.*");
```

By itself, an instance of the `IBinding` interface holds only the data about a connection between an exchange and queue.
In other words, it is not an "active" component.
However, as you will see later in [Configuring the Broker](#configuring-the-broker), the `RabbitAdmin` class can use `IBinding` instances to actually trigger the binding actions on the broker.
Also, as you will see in that same section, you can define the `IBinding` instances by using Steeltoe `AddRabbitBinding(..)` extension methods.

The `RabbitTemplate` mentioned earlier is one of the main components involved in actual RabbitMQ messaging, and it is discussed in detail in its own section[Rabbit Template](#rabbittemplate).

## Connection and Resource Management

Whereas the abstractions we described in the previous section is generic and applicable to all AMQP implementations (e.g. RabbitMQ), when we get into the management of resources, the details are specific to the broker implementation.
Therefore, in this section, we focus on code that is specific to the RabbitMQ instance since, at this point, RabbitMQ is the only supported implementation of AMQP.

The central component for managing a connection to the RabbitMQ broker is the `IConnectionFactory` interface.
The responsibility of a `IConnectionFactory` implementation is to provide an instance of `Steeltoe.Messaging.RabbitMQ.IConnection`, which is a wrapper for the underlying `RabbitMQ.Client.IConnection`.
The only concrete implementation we provide is `CachingConnectionFactory`, which by default, establishes a single connection proxy that can be shared by the application.
Sharing of the connection is possible since the "unit of work" for messaging with AMQP is actually a "Channel".

>The RabbitMQ .NET client uses an `RabbitMQ.Client.IModel` to represent what we will constantly refer to as a `Channel`.

The connection instance provides a `CreateChannel` method.
The `CachingConnectionFactory` implementation supports caching of those channels, and it maintains separate caches for channels based on whether they are transactional or not.
When creating an instance of `CachingConnectionFactory`, you can provide the 'hostname' through the constructor.
You can also provide the 'username' and 'password' properties.
To configure the size of the channel cache (the default is 25), you can use the `ChannelCacheSize` property.

You can configure the `CachingConnectionFactory` to cache connections as well as channels.
In this case, each call to `CreateConnection()` creates a new connection (or retrieves an idle one from the cache).
Closing a connection returns it to the cache (if the cache size has not been reached).
Channels created on such connections are also cached.
The use of separate connections might be useful in some environments, such as consuming from an HA cluster, in
conjunction with a load balancer, to connect to different cluster members, and others.
To cache connections, set the `CacheMode` property to `CachingMode.CONNECTION`.

>This does not limit the number of connections.
Rather, it specifies how many idle open connections are allowed.

A property called `ConnectionLimit` is provided to limit the total number of connections allowed.
When set, if the limit is reached, the `ChannelCheckoutTimeout` is used to wait for a connection to become idle.
If the time is exceeded, an `RabbitTimeoutException` is thrown.

>When the cache mode is `CONNECTION`, automatic declaration of queues and other AMQP types (See [Automatic Declaration of Exchanges, Queues, and Bindings](#automatic-declaration-of-exchanges-queues-and-bindings) is NOT supported.

It is important to understand that the cache size is (by default) not a limit but is merely the number of channels that can be cached.
With a cache size of, say, 10, any number of channels can actually be in use.
If more than 10 channels are being used and they are all returned to the cache, 10 go in the cache.
The remainder are physically closed.

The default channel cache size is 25.
In high volume, multi-threaded environments, a small cache means that channels are created and closed at a high rate.
Increasing the default cache size can avoid this overhead.
You should monitor the channels in use through the RabbitMQ Broker Admin UI and consider increasing the cache size further if you
see many channels being created and closed.
The cache grows only on-demand (to suit the concurrency requirements of the application), so this change does not
impact existing low-volume applications.

The `CachingConnectionFactory` has a property called `ChannelCheckoutTimeout`.
When this property is greater than zero, the `ChannelCacheSize` becomes a limit on the number of channels that can be created on a connection.
If the limit is reached, calling threads will block until a channel is available or this timeout value is reached, in which case a `RabbitTimeoutException` is thrown.

>WARNING: Channels used within the framework (for example,`RabbitTemplate`) are reliably returned to the cache. If you create channels outside of the framework, (for example, by accessing the connections directly and invoking `CreateChannel()`), you must return them (by closing) reliably, perhaps in a `finally` block, to avoid running out of channels.

The following example shows how to create a new `IConnection`:

```csharp
CachingConnectionFactory connectionFactory = new CachingConnectionFactory()
{
     Host = "somehost",
     Username = "guest",
     Password = "guest"
};

IConnection connection = connectionFactory.CreateConnection();
```

>There is also a `SingleConnectionFactory` implementation that is available only in the unit test code.
It is simpler than `CachingConnectionFactory`, since it does not cache channels, but it is not intended for practical usage outside of simple tests due to its lack of performance and resilience.
If you need to implement your own `ConnectionFactory` for some reason, the `AbstractConnectionFactory` base class may provide a nice starting point.

### Blocked Connections and Resource Constraints

The connection might become blocked for interaction from the broker that corresponds to the [Memory Alarm](https://www.rabbitmq.com/memory.html).
The `Steeltoe.Messaging.RabbitMQ.IConnection` can be supplied with `IBlockedListener` instances to be notified for connection blocked and unblocked events. You can use the `AddBlockedListener(..)` methods to be notified of the events.

>IMPORTANT:  When the application is configured with a single `CachingConnectionFactory`, as it is by default, the application stops working when the connection is blocked by the Broker.
And when it is blocked by the Broker, any of its clients stop to work.
If we have producers and consumers in the same application, we may end up with a deadlock when producers are blocking the connection (because there are no resources on the Broker any more) and consumers cannot free them (because the connection is blocked).
To mitigate the problem, we suggest having one more separate `CachingConnectionFactory` instance with the same options - one for producers and one for consumers.
A separate `CachingConnectionFactory` is not possible for transactional producers that execute on a consumer thread, since they should reuse the Channel associated with the consumer transactions.

The `RabbitTemplate` has a configuration option to automatically use a second connection factory, unless transactions are being used.
See [Using a Separate Connection](#using-a-separate-connection) for more information.

A `RabbitResourceNotAvailableException` is provided, which is thrown when `SimpleConnection.CreateChannel()` cannot create a `Channel` (for example, because the `RequestedChannelMax` limit is reached and there are no available channels in the cache).
You can use this exception in your implementation of a retry policy to recover the operation after some back-off.

### Configuring RabbitMQ Client Connection Factory

The `CachingConnectionFactory` uses an instance of the RabbitMQ client `RabbitMQ.Client.ConnectionFactory`.
A number of configuration properties are passed through (e.g. `Host`, `Port`, `Username`, `Password`, `RequestedHeartBeat`, and `ConnectionTimeout` for example) when setting the equivalent property on the `CachingConnectionFactory`or when using the `RabbitOptions` and a `IConfiguration`.
To set other properties, you can create an instance of the `RabbitMQ.Client.ConnectionFactory` and provide a reference to it by using the appropriate constructor of the `CachingConnectionFactory`.

>The 4.0.x+ RabbitMQ client supports automatic recovery feature.
While compatible with this feature, Steeltoe RabbitMQ has its own recovery mechanisms and the client recovery feature generally is not needed.
Since the auto-recovering connection recovers on a timer, the connection may be recovered more quickly by using Steeltoe RabbitMQs recovery mechanisms.
Steeltoe RabbitMQ disables the automatic recovery feature unless you explicitly create your own RabbitMQ connection factory and provide it to the `CachingConnectionFactory`.

### Configuring TLS

A convenient `RabbitOptions.SslOptions` is provided to enable convenient configuration of SSL properties on the underlying client connection factory when using .NET Core `IConfiguration`.

See the [RabbitMQ Documentation](https://www.rabbitmq.com/ssl.html) for information about configuring SSL.

```json
{
  "Spring": {
    "RabbitMq": {
      "Ssl": {
        "Enabled" : true,
        "ValidateServerCertificate" : true,
        "CertPath" : "file path",
        "CertPassphrase" : "passkey",
        "VerifyHostname": true,
        "ServerHostName" : "broker server name",
        "Algorithm": SslProtocols.Tls12
      }
    }
  }
}
```

The `CertPath` and `CertPassphrase` are used to configure the RabbitMQ client `ConnectionFactory` to use a file containing a certificate the client will send to the broker.

>IMPORTANT: The server certificate is validated by default (i.e. `ValidateServerCertificate = true`).
If you wish to skip this validation for some reason, set `ValidateServerCertificate` to false, and the `RabbitMQ.Client.ConnectionFactory.Ssl.AcceptablePolicyErrors` will be set to `SslPolicyErrors.RemoteCertificateNotAvailable | SslPolicyErrors.RemoteCertificateChainErrors`.

>IMPORTANT: The default algorithm is configured to use TLS v1.3 or TLS v1.2. If you need to use v1.1 you will have to explicitly configure it.

### Connecting to a Cluster

To connect to a cluster, configure the `Addresses` property on the `CachingConnectionFactory`, as follows:

```csharp
CachingConnectionFactory ccf = new CachingConnectionFactory()
{
    Addresses = "host1:5672,host2:5672,host3:5672"
}
```

The underlying connection factory tries to connect to each host, in order, whenever a new connection is established.
The connection order can be made random by setting the `ShuffleAddresses` property to true; the shuffle will be applied before creating any new connection.

```csharp
CachingConnectionFactory ccf = new CachingConnectionFactory()
{
    Addresses = "host1:5672,host2:5672,host3:5672",
    ShuffleAddresses = true
}
```

>You can also set the `Addresses` by configuring a `RabbitOptions` via `IConfiguration`.

<!--

TODO:  With 3.1 release and support for expressions this should have meaning and should be documented

### Routing Connection Factory

In addition to the `CachingConnectionFactory` a `AbstractRoutingConnectionFactory` is also provided.
This factory provides a mechanism to configure mappings for several `IConnectionFactory`'s and to determine the target `IConnectionFactory` (i.e. used connection factory) by some `lookupKey` at runtime.
Typically, the implementation checks a thread-bound context in making a determination.
For convenience, Steeltoe RabbitMQ provides the `SimpleRoutingConnectionFactory`, which gets the current thread-bound `lookupKey` from a `SimpleResourceHolder`.
The following examples shows how to configure a `SimpleRoutingConnectionFactory`:

```csharp
public class MyService {

    private RabbitTemplate rabbitTemplate;

    public MyService()
    {

    }
    public void service(string vHost, string payload) {
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
-->

### Factory Publisher Confirms and Returns

Confirmed (with correlation) and returned messages are supported by setting the `CachingConnectionFactory` property `PublisherConfirmType` to `ConfirmType.CORRELATED` and the `PublisherReturns` property to 'true'.

When these options are set, `Channel` instances created by the factory are wrapped in an `IPublisherCallbackChannel`, which is used to facilitate the callbacks.
When such a channel is obtained, the client can register a `IPublisherCallbackChannel.IListener` with the `Channel`.
The `IPublisherCallbackChannel` implementation contains logic to route a confirm or return to the appropriate listener.
These features are explained further in the following sections.

See also `PublisherConfirms` in [Scoped Operations](#scoped-operations).

>TIP: For some more background information on publisher confirms and returns, see the blog post by the RabbitMQ team titled [Introducing Publisher Confirms](https://www.rabbitmq.com/blog/2011/02/10/introducing-publisher-confirms/).

### Connection and Channel Listeners

The connection factory supports registering `IConnectionListener` and `IChannelListener` implementations.
Doing so, allows you to receive notifications for connection and channel related events.
(A `IConnectionListener` is used by the `RabbitAdmin` to perform declarations on the broker when the connection is established.
  See [Automatic Declaration of Exchanges, Queues and Bindings]("#automatic-declaration-of-exchanges-queues-and-bindings"></a> for more information).
The following listing shows the `IConnectionListener` interface definition:

```csharp
public interface IConnectionListener
{
    void OnCreate(IConnection connection);
    void OnClose(IConnection connection);
    void OnShutDown(ShutdownEventArgs args);
}
```

The `IConnection` object can be supplied with `IBlockedListener` instances to be notified for connection blocked and unblocked events.
The following example shows the `IChannelListener` interface definition:

```csharp
public interface IChannelListener
{
    void OnCreate(IModel channel, bool transactional);
    void OnShutDown(ShutdownEventArgs args);
}
```

See [Publishing is Asynchronous — How to Detect Successes and Failures](#publishing-is-asynchronous-how-to-detect-success-and-failure) for one scenario where you might want to register a `IChannelListener`.

### Logging Channel Close Events

The `CachingConnectionFactory` uses a default strategy to log channel closures as follows:

* Normal channel closes (200 OK) are not logged.
* If a channel is closed due to a failed passive queue declaration, it is logged at debug level.
* If a channel is closed because the `basic.consume` is refused due to an exclusive consumer condition, it is logged at
INFO level.
* All others are logged at ERROR level.

To modify this behavior, you can configure a custom `IConditionalExceptionLogger` into the `CachingConnectionFactory` in its `CloseExceptionLogger` property.

### Runtime Cache Properties

The `CachingConnectionFactory` provides cache statistics through the `GetCacheProperties()` method.
These statistics can be used to tune the cache to optimize it in production.
For example, the high water marks can be used to determine whether the cache size should be increased.
If it equals the cache size, you might want to consider increasing further.
The following table describes the `CacheMode.CHANNEL` properties:

| Property | Meaning |
| --- | --- |
| `ConnectionName` | The name of the connection. |
| `ChannelCacheSize` | The currently configured maximum channels that are allowed to be idle. |
| `LocalPort` | The local port for the connection (if available). This can be used to correlate with connections and channels on the RabbitMQ Admin UI. |
| `IdleChannelsTx` | The number of transactional channels that are currently idle (cached). |
| `IdleChannelsNotTx` | The number of non-transactional channels that are currently idle (cached). |
| `IdleChannelsTxHighWater` | The maximum number of transactional channels that have been concurrently idle (cached). |
| `IdleChannelsNotTxHighWater`  | The maximum number of non-transactional channels have been concurrently idle (cached). |

The following table describes the `CacheMode.CONNECTION` properties:

| Property | Meaning |
| --- | --- |
| ConnectionName:`LocalPort` | The name of the connection. |
| `OpenConnections` | The number of connection objects representing connections to brokers. |
| `ChannelCacheSize` | The currently configured maximum channels that are allowed to be idle. |
| `ConnectionCacheSize` | The currently configured maximum connections that are allowed to be idle. |
| `IdleConnections` | The number of connections that are currently idle. |
| `IdleConnectionsHighWater` | The maximum number of connections that have been concurrently idle. |
| IdleChannelsTx:`localPort` | The number of transactional channels that are currently idle (cached) for this connection. You can use the `localPort` part of the property name to correlate with connections and channels on the RabbitMQ Admin UI. |
| IdleChannelsNotTx:`localPort` | The number of non-transactional channels that are currently idle (cached) for this connection. The `localPort` part of the property name can be used to correlate with connections and channels on the RabbitMQ Admin UI. |
| IdleChannelsTxHighWater:`localPort` | The maximum number of transactional channels that have been concurrently idle (cached). The localPort part of the property name can be used to correlate with connections and channels on the RabbitMQ Admin UI. |
| IdleChannelsNotTxHighWater:`localPort` | The maximum number of non-transactional channels have been concurrently idle (cached). You can use the `localPort` part of the property name to correlate with connections and channels on the RabbitMQ Admin UI. |

The `CacheMode` property (`CHANNEL` or `CONNECTION`) is also included.

### RabbitMQ Automatic Connection and Topology Recovery

Since the first version of Steeltoe RabbitMQ, the framework has provided its own connection and channel recovery in the event of a broker failure.
Also, as discussed in [Configuring the Broker](#configuring-the-broker), `RabbitAdmin` re-declares any infrastructure services (queues and others) when the connection is re-established.
It therefore, does not rely on the [auto-recovery](https://www.rabbitmq.com/api-guide.html#recovery) features that is provided by the `RabbitMQ.Client` library. We strongly recommend that you allow Steeltoe RabbitMQ to use its own recovery mechanism by disabling it in the client if you configure your own factory.

>IMPORTANT: Only elements (queues, exchanges, bindings) that are defined as services in the container will be re-declared after a connection failure.
Elements declared by invoking `RabbitAdmin.Declare*()` methods directly from user code are unknown to the framework and therefore cannot be recovered.

If you have a need for a variable number of declarations, consider defining a service, of type `IDeclarable`, as discussed in [Declaring Collections of Exchanges, Queues, and Bindings](#declaring-collections-of-exchanges-queues-and-bindings).

## Adding Custom Client Connection Properties

The `CachingConnectionFactory` lets you access the underlying connection factory to allow, for example,
setting custom client properties.
The following example shows how to do so:

```csharp
connectionFactory.RabbitConnectionFactory.ClientProperties.Add("thing1", "thing2");
```

These properties appear in the RabbitMQ Admin UI when viewing the connection.

## RabbitTemplate

As with many other high-level abstractions provided by the Steeltoe framework and related components, Steeltoe RabbitMQ provides a "template" that plays a central role in interacting with messaging systems.
The interface that defines the main operations for RabbitMQ is called `IRabbitTemplate`, with the implementation found in `RabbitTemplate`.
Those operations cover the general behavior for sending and receiving messages to/from a RabbitMQ broker.
We will explore message sending and reception, respectively, in [Sending Messages](#sending-messages) and [Receiving Messages](#receiving-messages).

### Adding Retry Capabilities

You can now configure the `RabbitTemplate` to use a `RetryTemplate` to help with handling problems with broker connectivity.
See the [Steeltoe Retry](https://github.com/SteeltoeOSS/Steeltoe/tree/release/3.2/src/Common/src/Common/Retry) framework for complete information.
The following is only one example that uses a [Polly](http://www.thepollyproject.org/) based retry policy, which makes three tries before throwing the exception to the caller.

```csharp
public RabbitTemplate GetRabbitTemplate() {
    RabbitTemplate template = new RabbitTemplate(connectionFactory);
    template.RetryTemplate = new PollyRetryTemplate(3, 500, 10000, 10.0);
    return template;
}
```

In addition to the `RetryTemplate` property, the `RecoveryCallback` property is supported on the `RabbitTemplate`.
The template uses it as the second argument for the `RetryTemplate.Execute(Func<IRetryContext, T> retryCallback, IRecoveryCallback<T> recoveryCallback)` call that is done by the template when a retry template has been configured. You can provide an implementation of this interface to handle failures after all retries have taken place.

### Publishing is Asynchronous How to Detect Success and Failure

Publishing messages is an asynchronous mechanism and, by default, messages that cannot be routed to the proper location are dropped by RabbitMQ.
For successful publishing, you can receive an asynchronous confirm, as described in [Template Publisher Confirms and Returns](#template-publisher-confirms-and-returns).
Consider two failure scenarios:

* Publish to an exchange but there is no matching destination queue.
* Publish to a non-existent exchange.

The first case is covered by publisher returns, as described in [Template Publisher Confirms and Returns](#template-publisher-confirms-and-returns).

For the second case, the message is dropped and no return is generated.
The underlying channel is closed with an exception.
By default, this exception is logged, but you can register a `IChannelListener` with the `CachingConnectionFactory` to obtain notifications of such events.
The following example adds a `IConnectionListener` to the connection factory:

```csharp
public class MyConnectionListener : IConnectionListener
{
    public void OnCreate(IConnection connection)
    {
    }

    public void OnShutDown(ShutdownEventArgs args)
     {
        ...
    }
}

this.connectionFactory.AddConnectionListener(new MyConnectionListener());
```

You can examine the `ShutdownEventArgs` properties to determine the problem that occurred.

### Template Publisher Confirms and Returns

The `RabbitTemplate` supports publisher confirms and returns.

For returned messages, the template's `Mandatory` property must be set to `true`. See [Factory Publisher Confirms and Returns](#factory-publisher-confirms-and-returns)
This feature also requires a `CachingConnectionFactory` that has its `PublisherReturns` property set to `true`.
Returns are sent to the client by it registering a return callback by setting the `RabbitTemplate.ReturnCallback` property.
The callback must implement the following method:

```csharp
public interface IReturnCallback
{
    void ReturnedMessage(
        IMessage<byte[]> message, int replyCode, string replyText, string exchange, string routingKey);
}
```

Only one `ReturnCallback` is supported by each `RabbitTemplate`. See also [Reply Timeout](#reply-timeout)

For publisher confirms (also known as publisher acknowledgements), the template requires a `CachingConnectionFactory` that has its `PublisherConfirm` property set to `ConfirmType.CORRELATED`.
Confirms are sent to the client by it registering a confirm callback by setting the`RabbitTemplate.ConfirmCallback`.
The callback must implement this method:

```csharp
public interface IConfirmCallback
{
    void Confirm(CorrelationData correlationData, bool ack, string cause);
}
```

The `CorrelationData` is an object supplied by the client when sending the original message. Contained within the object is a `Confirm` which contains the status.
The `Confirm.Ack` is true for an acknowledgement has been received and false in not.
For `nack` cases, the `Confirm` may contain a reason for the `nack`, if it is available when the `nack` is generated.
An example is when sending a message to a non-existent exchange.
In that case, the broker closes the channel.
The reason for the closure is included in the `Reason`.

Only one `ConfirmCallback` is supported by a `RabbitTemplate`.

>When a rabbit template send operation completes, the channel is closed.
This precludes the reception of confirms or returns when the connection factory cache is full (when there is space in the cache, the channel is not physically closed and the returns and confirms proceed normally).
When the cache is full, the framework defers the close for up to five seconds, in order to allow time for the confirms and returns to be received.
When using confirms, the channel is closed when the last confirm is received.
When using only returns, the channel remains open for the full five seconds.
We generally recommend setting the connection factory's `ChannelCacheSize` to a large enough value so that the channel on which a message is published is returned to the cache instead of being closed.
You can monitor channel usage by using the RabbitMQ management plugin.
If you see channels being opened and closed rapidly, you should consider increasing the cache size to reduce overhead on the server.

>IMPORTANT: The guarantee of receiving a returned message before the ack is still maintained as long as the return callback executes in 60 seconds or less.
The confirm is scheduled to be delivered after the return callback exits or after 60 seconds, whichever comes first.

The `CorrelationData` object has a `FutureSource` that you can use to get the result, instead of using a `ConfirmCallback` on the template.
The following example shows how to configure a `CorrelationData` instance:

```csharp
CorrelationData cd1 = new CorrelationData();
this.templateWithConfirmsEnabled.ConvertAndSend("exchange", queue.QueueName, "foo", cd1);
Assert.True(cd1.Future.Wait(TimeSpan.FromSeconds(10));
Assert.True(cd1.Future.Result.Ack);
```

Since it is a `Task<Confirm>`, you can either use `.Result` to get the result when ready or use an `await`.
The `Confirm` object is a simple POCO with 2 properties: `Ack` and `Reason` (for `nack` instances).
The reason is not populated for broker-generated `nack` instances.
It is populated for `nack` instances generated by the framework (for example, closing the connection while `ack` instances are outstanding).

In addition, when both confirms and returns are enabled, the `CorrelationData` is populated with the returned message.
It is guaranteed that this occurs before the future is set with the `ack`.

See also [Scoped Operations](#scoped-operations) for a simpler mechanism for waiting for publisher confirms.

### Scoped Operations

Normally, when using the template, a `Channel` is checked out of the cache (or created), and used for the operation, and then returned to the cache for reuse.
In a multi-threaded environment, there is no guarantee that the next operation uses the same channel.
There may be times, however, where you want to have more control over the use of a channel and ensure that a number of operations are all performed on the same channel.

A method called `T Invoke<T>(Func<IRabbitTemplate, T> rabbitOperations)` is provided.
Any operations performed within the scope of the `rabbitOperations` and on the provided `RabbitTemplate` argument use the same dedicated `Channel`, which will be closed at the end (not returned to a cache).
If the channel is a `IPublisherCallbackChannel`, it is returned to the cache after all confirms have been received (see [Template Publisher Confirms and Returns](#template-publisher-confirms-and-returns).

One example of why you might need to use this is if you wish to use the `WaitForConfirms()` method on the underlying `Channel`.
Alternatively, the `RabbitTemplate` also provides `WaitForConfirms(int timeout)` and `WaitForConfirmsOrDie(int timeout)`, which delegates to the dedicated channel used within the scope of the `rabbitOperations`.
The methods cannot be used outside of that scope, for obvious reasons.

Note that a higher-level abstraction that lets you correlate confirms to requests is provided elsewhere (see [Template Publisher Confirms and Returns](#template-publisher-confirms-and-returns).
If you want only to wait until the broker has confirmed delivery, you can use the technique shown in the following example:

```csharp
var messages = GetMessagesToSend();
var result = template.Invoke(t => {
    messages.ForEach(m => t.ConvertAndSend(ROUTE, m));
    t.WaitForConfirmsOrDie(10_000);
    return true;
});
```

If you need `RabbitAdmin` operations to be invoked on the same channel within the scope of the `rabbitOperations`, the admin must have been constructed by using the same `RabbitTemplate` that was used for the `Invoke` operation.

>The preceding discussion is moot if the template operations are already performed within the scope of an existing transaction - for example, when running on a transacted listener container thread and performing operations on a transacted template.
In that case, the operations are performed on that channel and committed when the thread returns to the container.
It is not necessary to use `Invoke` in that scenario.

When using confirms in this way, much of the infrastructure set up for correlating confirms to requests is not really needed (unless returns are also enabled).
The connection factory supports a property called `PublisherConfirmType`.
When this is set to `ConfirmType.SIMPLE`, the infrastructure is avoided and the confirm processing can be more efficient.

Furthermore, the `RabbitTemplate` sets the `PublishSequenceNumber` property in the sent message `IMessageHeaders`.
If you wish to check (or log or otherwise use) specific confirms, you can do so with an overloaded `Invoke` method, as the following example shows:

```csharp
public virtual T Invoke<T>(
    Func<IRabbitTemplate, T> rabbitOperations, Action<object, BasicAckEventArgs> acks, Action<object, BasicNackEventArgs> nacks)
```

>These `EventArgs` (for `ack` and `nack` instances) are the RabbitMQ client event args.

The following example logs `ack` and `nack` instances:

```csharp
var messages = GetMessagesToSend();
var result = template.Invoke(t => {
    messages.ForEach(m => t.ConvertAndSend(ROUTE, m));
    t.WaitForConfirmsOrDie(10_000);
    return true;
}, (sender, arg) => {
        log.LogInformation("Ack: " + arg.DeliveryTag + ":" + arg.Multiple);
}, (sender, arg) -> {
        log.LogInformation("Nack: " + arg.DeliveryTag + ":" + arg.Multiple);
}));
```

<!--

TODO: Update when expression are supported

### Validated User Id

The template supports a `user-id-expression` (`userIdExpression` when using Java configuration).
If a message is sent, the user id property is set (if not already set) after evaluating this expression.
The root object for the evaluation is the message to be sent.

The following examples show how to use the `user-id-expression` attribute:

[//]: # (There was an XML example here. If you adjust the preceding paragraph, it can make sense with a C# example.)

The first example is a literal expression.
The second obtains the `username` property from a connection factory bean in the application context.
-->

### Using a Separate Connection

You can set the `UsePublisherConnection` property to `true` to use a different connection than that used by listener containers, when necessary.
This is to avoid consumers being blocked when a producer is blocked for any reason.
The `CachingConnectionFactory` maintains a second internal connection factory for this purpose.
If the rabbit template is running in a transaction started by the listener container, the container's channel is used, regardless of this setting.

>IMPORTANT: In general, you should not use a `RabbitAdmin` with a template that has this set to `true`.
Use the `RabbitAdmin` constructor that takes a connection factory.
If you use the other constructor that takes a template, ensure the template's property is `false`.
This is because, often, an admin is used to declare queues for listener containers.
Using a template that has the property set to `true` would mean that exclusive queues (such as `AnonymousQueue`) would be declared on a different connection to that used by listener containers.
In that case, the queues cannot be used by the containers.

## Sending Messages

When sending a message, there are many methods you can use.  Here are some of the many available:

```csharp
void Send(IMessage message);
void Send(string routingKey, IMessage message);
void Send(string exchange, string routingKey, IMessage message);
```

We can begin our discussion with the last method in the preceding listing, since it is actually the most explicit.
It lets an RabbitMQ exchange name  (along with a routing key) be provided at runtime.
An example of using this method to send a message might look this this:

```csharp
template.Send(
    "marketData.topic", "quotes.nasdaq.FOO", Message.Create(Encoding.UTF8.GetBytes("12.34"), someHeaders));
```

You can set the `Exchange` property on the template itself if you plan to use that template instance to send to the same exchange most or all of the time.
In such cases, you can use the second method in the preceding listing.
The following example is functionally equivalent to the previous example:

```csharp
template.Exchange = "marketData.topic";
template.Send("quotes.nasdaq.FOO", Message.Create(Encoding.UTF8.GetBytes("12.34"), someHeaders));
```

If both the `Exchange` and `RoutingKey` properties are set on the template, you can use the method that accepts only the `IMessage`.
The following example shows how to do so:

```csharp
template.Exchange = "marketData.topic";
template.RoutingKey = "quotes.nasdaq.FOO";
template.Send(Message.Create(Encoding.UTF8.GetBytes("12.34"), someHeaders));
```

A better way of thinking about the `Exchange` and `RoutingKey` properties is that the explicit method parameters always override the template's default values.
In fact, even if you do not explicitly set those properties on the template, there are always default values in place.
In both cases, the default is an empty `string`, but that is actually a sensible default.
As far as the routing key is concerned, it is not always necessary in the first place (for example, fora `Fanout` exchange).
Furthermore, a queue may be bound to an exchange with an empty `string`.
Those are both legitimate scenarios for reliance on the default empty `string` value for the routing key property of the template.
As far as the exchange name is concerned, the empty `string` is commonly used because the AMQP specification defines the "default exchange" as having no name.
Since all queues are automatically bound to that default exchange (which is a direct exchange), using their name as the binding value, the second method in the preceding listing can be used for simple point-to-point messaging to any queue through the default exchange.
You can provide the queue name as the `RoutingKey`, either by providing the method parameter at runtime.
The following example shows how to do so:

```csharp
var template = new RabbitTemplate(); // using default no-name Exchange
template.Send("queue.helloWorld", Message.Create(Encoding.UTF8.GetBytes("Hello World"), someHeaders));
```

Alternately, you can create a template that can be used for publishing primarily or exclusively to a single Queue.
The following example shows how to do so:

```csharp
RabbitTemplate template = new RabbitTemplate(); // using default no-name Exchange
template.RoutingKey  = "queue.helloWorld"; // but we'll always send to this Queue
template.Send(Message.Create(Encoding.UTF8.GetBytes("Hello World"), someHeaders));
```

### Message Builder API

A message builder API is provided by the `RabbitMessageBuilder<P>`.
The methods provide a convenient "fluent" means of creating a message.
The following examples show the fluent API in action:

```csharp
var message = RabbitMessageBuilder
    .WithPayload<byte[]>(Encoding.UTF8.GetBytes("foo"))
    .SetContentType(MessageHeaders.CONTENT_TYPE_TEXT_PLAIN)
    .SetMessageId("123")
    .SetHeader("bar", "baz")
    .Build();
```

Each of the properties defined on the RabbitMQ [`IBasicProperties`](https://rabbitmq.github.io/rabbitmq-dotnet-client/api/RabbitMQ.Client.IBasicProperties.html) can be set.
Other methods include `SetHeader(string key, string value)`, `RemoveHeader(string key)`, `RemoveHeaders()`, and `CopyHeaders(IDictionary<string, object> properties)`.
Each property setting method has a `Set*IfAbsent()` variant as well.
In the cases where a default initial value exists, the method is named `Set*IfAbsentOrDefault()`.

Five static methods are provided to create an initial message builder:

```csharp
IMessage CreateMessage(object payload, IMessageHeaders messageHeaders, Type payloadType = null);
AbstractMessageBuilder FromMessage<P>(IMessage<P> message);
AbstractMessageBuilder FromMessage(IMessage message, Type payloadType = null);
AbstractMessageBuilder WithPayload<P>(P payload);
AbstractMessageBuilder WithPayload(object payload, Type payloadType = null);
```

With the `RabbitTemplate` each of the `Send()` methods has an overloaded version that takes an additional `CorrelationData` object.
When publisher confirms are enabled, this object is returned in the callback described in [RabbitTemplate](#rabbittemplate).
This lets the sender correlate a confirm (`ack` or `nack`) with the sent message.

Also, a callback interface called `ICorrelationDataPostProcessor` is provided which you can use during correlation processing.
This is invoked after all `IMessagePostProcessor` instances (provided in the `Send(..)` methods as well as those provided in `SetBeforePublishPostProcessors(...)`).
Implementations can update or replace the correlation data supplied in the `Send(..)` method (if any).
The `IMessage` and original `CorrelationData` (if any) are provided as arguments.
The following example shows how to use the `PostProcess` method:

```csharp
CorrelationData PostProcess(IMessage message, CorrelationData correlationData);
```

### Publisher Returns

When the template's `Mandatory` property is set to `true`, returned messages are provided by the callback described in [RabbitTemplate](#rabbittemplate).

<!--

TODO: Update when expressions supported

The `RabbitTemplate` supports the SpEL `mandatoryExpression` property, which is evaluated against each request message as the root evaluation object, resolving to a `boolean` value.
Bean references, such as `@myBean.isMandatory(#root)`, can be used in the expression.
-->

Publisher returns can also be used internally by the `RabbitTemplate` in send and receive operations.
See [Reply Timeout](#reply-timeout) for more information.

### Batching

Steeltoe RabbitMQ includes the `BatchingRabbitTemplate` in addition to `RabbitTemplate`.
This is a subclass of `RabbitTemplate` with an overridden `Send()` methods that batches messages according to the provided `IBatchingStrategy`.
Only when a batch is complete is the message sent to RabbitMQ.
The following listing shows the `IBatchingStrategy` interface definition:

```csharp
public interface IBatchingStrategy
{
    MessageBatch? AddToBatch(string exchange, string routingKey, IMessage message);
    DateTime? NextRelease();
    ICollection<MessageBatch> ReleaseBatches();
    bool CanDebatch(IMessageHeaders properties);
    void DeBatch(IMessage message, Action<IMessage> fragmentConsumer);
}
```

>CAUTION: Batched data is held in memory.
Unsent messages can be lost in the event of a system failure.

A `SimpleBatchingStrategy` is provided as a default.
It supports sending messages to a single exchange or routing key.
It has the following configuration options:

* `batchSize`: The number of messages in a batch before it is sent.
* `bufferLimit`: The maximum size of the batched message. This preempts the `batchSize`, if exceeded, and causes a partial batch to be sent.
* `timeout`: A time after which a partial batch is sent when there is no new activity adding messages to the batch.

The `SimpleBatchingStrategy` formats the batch by preceding each embedded message with a four-byte binary length.
This is communicated to the receiving system by setting the `springBatchFormat` message header to `lengthHeader4`.

>IMPORTANT: Batched messages are automatically de-batched by listener containers by default (by using the `springBatchFormat` message header).
Rejecting any message from a batch causes the entire batch to be rejected.

However, see [RabbitListener with Batching](#rabbitlistener-with-batching) for more information.

## Receiving Messages

Message reception is always a little more complicated than sending.
There are two ways to receive a `Message`.
The simpler option is to poll for one `Message` at a time with a polling method call.
The more complicated yet more common approach is to register a listener that receives `Messages` on-demand, asynchronously.
We consider an example of each approach in the next two sub-sections.

### Polling the Consumer

The `RabbitTemplate` itself can be used for polled `Message` reception.
By default, if no message is available, `null` is returned immediately.
There is no blocking.
You can set a `ReceiveTimeout`, in milliseconds, and the receive methods block for up to that long, waiting for a message.
A value less than zero means block indefinitely (or at least until the connection to the broker is lost).
There are various variants of the `Receive` methods that let the timeout be passed in on each call.

>CAUTION: Since the receive operation creates a new consumer for each message, this technique is not really appropriate for high-volume environments.
Consider using an asynchronous consumer or a `ReceiveTimeout` of zero for those use cases.

There are several receive methods to choose from. Four of the most simple `receive` methods are shown below.
As with the `Exchange` on the sending side, there is a method that requires that a default queue property has been set
directly on the template itself, and there is a method that accepts a queue parameter at runtime.
There are variants which accept `timeoutMillis` to override `ReceiveTimeout` property setting on a per-request basis.
The following listing shows the definitions of the four methods:

```csharp
IMessage Receive();
IMessage Receive(string queueName);
IMessage Receive(int timeoutMillis);
IMessage Receive(string queueName, int timeoutMillis);
```

As in the case of sending messages, the `RabbitTemplate` has some convenience methods for receiving POCOs instead of `IMessage` instances, and implementations provide a way to customize the `MessageConverter` used to create the `object` returned:
The following listing shows those methods:

```csharp
T ReceiveAndConvert<T>();
T ReceiveAndConvert<T>(string queueName);
T ReceiveAndConvert<T>(int timeoutMillis);
T ReceiveAndConvert(string queueName, int timeoutMillis);
```

There are variants of these methods that take an additional `Type` argument as well.
The template must be configured with a `ISmartMessageConverter`.
See [Message Converters](#message-converters) for more information.

Similar to `SendAndReceive` methods, the `RabbitTemplate` has several convenience `ReceiveAndReply` methods for synchronously receiving, processing and replying to messages.
The following listing shows some of those method definitions:

```csharp
bool ReceiveAndReply<R, S>(Func<R, S> callback);
bool ReceiveAndReply<R, S>(string queueName, Func<R, S> callback);
bool ReceiveAndReply<R, S>(Func<R, S> callback, string exchange, string routingKey);
bool ReceiveAndReply<R, S>(string queueName, Func<R, S> callback, string replyExchange, string replyRoutingKey);
bool ReceiveAndReply<R, S>(Func<R, S> callback, Func<IMessage, S, Address> replyToAddressCallback);
bool ReceiveAndReply<R, S>(string queueName, Func<R, S> callback, Func<IMessage, S, Address> replyToAddressCallback);
```

The `RabbitTemplate` implementation takes care of the `receive` and `reply` phases.
In most cases, you should provide only an implementation of `callback` to perform some business logic for the received message and build a reply object or message, if needed.
Note, a `callback` may return `null`.
In this case, no reply is sent and `ReceiveAndReply` works like the `Receive` method.
This lets the same queue be used for a mixture of messages, some of which may not need a reply.

The `replyToAddressCallback` is useful for cases requiring custom logic to determine the `replyTo` address at runtime against the received message and reply from the `callback`.
By default, `replyTo` information in the request message is used to route the reply.

The following listing shows an example of POCO-based receive and reply:

```csharp
var received =
    template.ReceiveAndReply<Order, Invoice>(ROUTE, (order) =>
     {
        return ProcessOrder(order);
    });

if (received)
{
    log.LogInformation("We received an order!");
}
```

### Asynchronous Consumer

>IMPORTANT: Steeltoe RabbitMQ also supports annotated listener endpoints through the use of the `[RabbitListener()]` attribute and provides an open infrastructure to register endpoints programmatically.
This is by far the most convenient way to setup an asynchronous consumer.
See [Attribute driven Listener Endpoints](#attribute-driven-listener-endpoints) for more details.

>IMPORTANT: The default `PrefetchCount` value is 250, which should keep consumers busy in most common scenarios.
There are, nevertheless, scenarios where the `PrefetchCount` value should be kept low:

* For large messages, especially if the processing is slow (messages could add up to a large amount of memory in the client process)
* When strict message ordering is necessary (the `PrefetchCount` value should be set to 1 in this case)
* Other special cases

Also, with low-volume messaging and multiple consumers (including concurrency within a single listener container instance), you may wish to reduce the `PrefetchCount` to get a more even distribution of messages across consumers.
We also recommend using `PrefetchCount` = 1 with the `MANUAL` `ack` mode.
The `BasicAck` is an asynchronous operation and, if something wrong happens on the Broker (double `ack` for the same delivery tag, for example), you end up with processed subsequent messages in the batch that are unacknowledged on the Broker, and other consumers may see them.

See [Message Listener Container Configuration](#message-listener-container-configuration).

For more background about `PrefetchCount`, see this post about [consumer utilization in RabbitMQ](https://www.rabbitmq.com/blog/2014/04/14/finding-bottlenecks-with-rabbitmq-3-3)
and this post about [queuing theory](https://www.rabbitmq.com/blog/2012/05/11/some-queuing-theory-throughput-latency-and-bandwidth/).

#### Message Listener

For asynchronous `Message` reception, a dedicated component (not the `RabbitTemplate`) is involved.
That component is a container for a `Message`-consuming callback.
We consider the container and its properties later in this section.
First, though, we should look at the callback, since that is where your application code is integrated with the messaging system.
There are a few options for the callback, starting with an implementation of the `IMessageListener` interface, which the following listing shows:

```csharp
public interface IMessageListener
{
    AcknowledgeMode ContainerAckMode { get; set; }
    void OnMessage(IMessage message);
    void OnMessageBatch(List<IMessage> messages);
}
```

If your callback logic depends on the RabbitMQ Channel instance for any reason, you may instead use the `IChannelAwareMessageListener`.
It looks similar but has an extra parameter.
The following listing shows the `IChannelAwareMessageListener` interface definition:

```csharp
public interface IChannelAwareMessageListener : IMessageListener
{
    void OnMessage(IMessage message, RabbitMQ.Client.IModel channel);
    void OnMessageBatch(List<IMessage> messages, RabbitMQ.Client.IModel channel);
}
```

#### MessageListenerAdapter

If you prefer to maintain a stricter separation between your application logic and the messaging API, you can rely upon an adapter implementation that is provided by the framework.
This is often referred to as "Message-driven POCO" support.

>A more flexible mechanism for POCO messaging is using the `[RabbitListener()]` attribute.
See [Attribute Driven Listener Endpoints](#attribute-driven-listener-endpoints) for more information.

When using the adapter, you need to provide only a reference to the instance that the adapter itself should invoke.
The following example shows how to do so:

```csharp
MessageListenerAdapter listener = new MessageListenerAdapter(null, somePoco);
listener.DefaultListenerMethod = "MyMethod";  // Defaults to "HandleMessage"
```

You can subclass the adapter and provide an implementation of `GetListenerMethodName()` to dynamically select different methods based on the message.
This method has two parameters, `originalMessage` and `extractedMessage`, the latter being the result of any conversion.
By default, a `SimpleMessageConverter` is configured.
See [SimpleMessageConverter](#simplemessageconverter) for more information and information about other converters available.

The original message has `consumerQueue` and `consumerTag` message headers, which can be used to determine the queue from which a message was received.

You can configure a `Dictionary<string, string>` of consumer queue or tag to method name, to dynamically select the method to call.
If no entry is in the map, the adapter falls back to the default listener method.
The default listener method (if not set) is `HandleMessage`.

A convenient interface has been provided which provides the default listener method.
The following listing shows the definition of the interface:

```csharp
public interface IReplyingMessageListener<in T, out R>
{
    R HandleMessage(T t);
}
```

You can also derive from `MessageListenerAdapter` and override the `BuildListenerArguments(object, RabbitMQ.Client.IModel, IMessage)` should you need to.
This method helps listener to get access to the `Channel` and `IMessage` arguments to do more, such as calling `channel.BasicReject(ulong, bool)` in manual acknowledge mode.
The following listing shows the most basic example:

```csharp
public class ExtendedListenerAdapter : MessageListenerAdapter {

    protected override object[] BuildListenerArguments(object extractedMessage, IModel channel, IMessage message)
    {
        return new object[]{extractedMessage, channel, message};
    }
}
```

With above you could configure `ExtendedListenerAdapter` to be the same as `MessageListenerAdapter` if you need to receive the "channel" and "message" in your handle method.
The parameters of handle method should be set to the same as what `BuildListenerArguments(Object, Channel, Message)` returned, as the following example of listener shows:

```csharp
public void HandleMessage(object extractedMessage, IModel channel, IMessage message)
{
    ...
}
```

#### Container

Now that you have seen the various options for the `Message`-listening callback, we can turn our attention to the container.
Basically, the container handles the "active" responsibilities so that the listener callback can remain passive.
The container is an example of a `ISmartLifecycle` component as it implements this interface.
It provides methods which allow it to be started and stopped.
When configuring the container, you essentially bridge the gap between a RabbitMQ Queue and a `IMessageListener` instance.
You must provide a reference to the `IConnectionFactory` and the queue names or Queue instances from which that listener should consume messages.

The primary container used in most applications will be the `DirectMessageListenerContainer`.

The following listing shows the most basic example, which works by using the, `SimpleMessageListenerContainer`:

```csharp
var container = new DirectMessageListenerContainer(null, rabbitConnectionFactory);
container.SetQueueNames("some.queue");
container.MessageListener = new MessageListenerAdapter(somePoco);
container.Initialize();
await container.Start();
```

As an "active" component, it is most common to create the listener container as part of a generic host, running in the background.
The following example shows how to do so:

```csharp
class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {
            // Add core services
            services.AddRabbitHostingServices();
            services.AddRabbitDefaultMessageConverter();
            services.AddRabbitConnectionFactory();

            services.AddRabbitDirecListenerContainer("manualContainer", (p, container) =>
            {
                var logFactory = p.GetRequiredService<ILoggerFactory>();
                container.SetQueueNames("myqueue");
                container.MessageListener = new MyMessageListener(logFactory.CreateLogger<MyMessageListener>());
                container.Initialize();
            });
        });
}

public class MyMessageListener : IMessageListener
{
    private ILogger<MyMessageListener> logger;
    public MyMessageListener(ILogger<MyMessageListener> logger)
    {
        this.logger = logger;
    }

    public AcknowledgeMode ContainerAckMode { get; set; }

    public void OnMessage(IMessage message)
    {
        var payload = Encoding.UTF8.GetString((byte[])message.Payload);
        logger.LogInformation(payload);
    }

    public void OnMessageBatch(List<IMessage> messages)
    {
        foreach(var message in messages)
        {
            OnMessage(message);
        }
    }
}
```

#### Consumer Priority

The broker now supports consumer priority (see [Using Consumer Priorities with RabbitMQ](https://www.rabbitmq.com/blog/2013/12/16/using-consumer-priorities-with-rabbitmq/)).
This is enabled by setting the `x-priority` argument on the consumer.
The `DirectMessageListenerContainer` supports setting consumer arguments, as the following example shows:

```csharp
container.ConsumerArguments.Add("x-priority", 10);
```

You can modify the queues on which the container listens at runtime. See [Listener Container Queues](#listener-container-queues).

#### Auto Delete Queues

When a container is configured to listen on `auto-delete` queues, or the queue has an `x-expires` option, or the [Time-To-Live](https://www.rabbitmq.com/ttl.html) policy is configured on the Broker, the queue is removed by the broker when the container is stopped (that is, when the last consumer is cancelled). If the container is restarted, the container uses a `RabbitAdmin` to redeclare any missing queues during startup.

You can also use conditional declaration (see [Conditional Declaration](#conditional-declaration) together with setting `AutoStartup = "false"` on the `RabbitAdmin` to defer queue declaration until the container is started.
The following example shows how to do so:

```csharp
....
// Add core services
services.AddRabbitServices();
// Add Rabbit admin
services.AddRabbitAdmin("containerAdmin", (p, admin) =>
{
    admin.AutoStartup = false
});
// Add a Queue
var queue = new Queue("myQueue");
queue.DeclaringAdmins.Add("containerAdmin");
services.AddQueue(queue);
```

In this case, the queue is declared by `containerAdmin`, which has `AutoStartup="false"` so that the Queue is not declared during connection.
Also, the container is not started for the same reason.
When the container is later started, it uses its reference to `containerAdmin` to declare the elements.

### Batched Messages

Batched messages (created by a producer) are automatically de-batched by listener containers (using the `springBatchFormat` message header).
Rejecting any message from a batch causes the entire batch to be rejected.
See [Batching](#batching) for more information about batching.

<!-- TODO: Update this when we add Context events like Spring has.
Also the Direct container will have to log the events mention below as this is specific to Simple

### Consumer Events

The containers publish application events whenever a listener
(consumer) experiences a failure of some kind.
The event `ListenerContainerConsumerFailedEvent` has the following properties:

* `container`: The listener container where the consumer experienced the problem.
* `reason`: A textual reason for the failure.
* `fatal`: A boolean indicating whether the failure was fatal. With non-fatal exceptions, the container tries to restart the consumer, according to the `recoveryInterval` or `recoveryBackoff` (for the `SimpleMessageListenerContainer`) or the `monitorInterval` (for the `DirectMessageListenerContainer`).
* `throwable`: The `Throwable` that was caught.

These events can be consumed by implementing `ApplicationListener<ListenerContainerConsumerFailedEvent>`.

>System-wide events (such as connection failures) are published by all consumers when `concurrentConsumers` is greater than 1.

If a consumer fails because one if its queues is being used exclusively, by default, a `WARN` log is issued.
To change this logging behavior, provide a custom `IConditionalExceptionLogger` in the `DirectMessageListenerContainer` instance's using the `ExclusiveConsumerExceptionLogger` property.
See also [Logging Channel Close Events](#channel-close-logging).

Fatal errors are always logged at the `ERROR` level.
This it not modifiable.

Several other events are published at various stages of the container lifecycle:

* `AsyncConsumerStartedEvent`: When the consumer is started.
* `AsyncConsumerRestartedEvent`: When the consumer is restarted after a failure - `SimpleMessageListenerContainer` only.
* `AsyncConsumerTerminatedEvent`: When a consumer is stopped normally.
* `AsyncConsumerStoppedEvent`: When the consumer is stopped - `SimpleMessageListenerContainer` only.
* `ConsumeOkEvent`: When a `consumeOk` is received from the broker, contains the queue name and `consumerTag`
* `ListenerContainerIdleEvent`: See <a href="#steeltoe-messaging-idle-containers"></a>.
-->

### Consumer Tags

You can provide a strategy to generate consumer tags in the container.
By default, the consumer tag is generated by the broker.
The following listing shows the `IConsumerTagStrategy` interface definition:

```csharp
public interface IConsumerTagStrategy : IServiceNameAware
{
    string CreateConsumerTag(string queueName);
}
```

The `queueName` is made available so that it can (optionally) be used in the tag.

See [Message Listener Container Configuration](#message-listener-container-configuration) for details on how to set this up.

### Attribute driven Listener Endpoints

The easiest way to receive a message asynchronously is to use the annotated listener endpoint infrastructure.
In a nutshell, it lets you expose a method of a service as a Rabbit listener endpoint.
The following example shows how to use the `[RabbitListener()]` attribute:

```csharp
public class MyService
{
    [RabbitListener("myQueue")]
    public void ProcessOrder(string data)
     {
        ...
    }
}

services.AddSingleton<MyService>();
services.AddRabbitListener<MyService>();
```

The idea of the preceding example is that, whenever a message is available on the queue named `myQueue`, the `ProcessOrder` method is invoked accordingly (in this case, with the payload of the message).

The annotated endpoint infrastructure creates a message listener container behind the scenes for each annotated method, by using a `DirectRabbitListenerContainerFactory`.
You must ensure `MyService` has been added to the service container (i.e. `services.AddSingleton<MyService>()`) and you have invoked `services.AddRabbitListener<MyService>()`.

In the preceding example, `myQueue` must already exist and be bound to some exchange.
The queue can be declared and bound automatically, as long as a `RabbitAdmin` exists in the service container and the `Queue` has been added as well.

>Property placeholders (`${some:property}`) can be specified for the annotation properties (`queues` etc).
See [Listening to Multiple Queues](#listening-to-multiple-queues) for examples.

The following listing shows three examples of how to declare a Rabbit listeners, Queues, Exchanges, etc. using annotations:

```csharp
public class MyService
{

    [DeclareQueue(Name = "myQueue", Durable = "True")]
    [DeclareExchange(Name = "auto.exch", IgnoreDeclarationExceptions = "True")]
    [DeclareQueueBinding(Name = "myQueue.auto.exch.binding.1", QueueName = "myQueue", ExchangeName = "auto.exch", RoutingKey = "orderRoutingKey")]
    [RabbitListener(Binding = "myQueue.auto.exch.binding.1")]
    public void ProcessOrder(Order order)
    {
        ...
    }

    [DeclareAnonymousQueue("anon")]
    [DeclareExchange(Name = "auto.exch")]
    [DeclareQueueBinding(Name = "anon.auto.exch.binding", QueueName = "#{anon}", ExchangeName = "auto.exch", RoutingKey = "invoiceRoutingKey")]
    [RabbitListener(Binding = "anon.auto.exch.binding")]
    public void ProcessInvoice(Invoice invoice)
    {
        ...
    }
    [DeclareQueue(Name = "${my:queue}", Durable = "True")]
    [RabbitListener("${my:queue}")]
    public string HandleWithSimpleDeclare(string data)
    {
        ...
    }
}
```

In the first example, a queue named `myQueue` is declared automatically (durable) together with the exchange, if needed,
and bound to the exchange with the routing key.  The `ProcessorOrder(Order order)` method is then tied to the binding using the `[RabbitListener()]` attribute.

In the second example, an anonymous (exclusive, auto-delete) queue is declared and bound.  It is given an ID = `anon` so it can be referenced in the `DeclareQueueBinding` using the `QueueName = "#{anon}"` property setting.

In the third example, a queue with the name retrieved from the `IConfiguration` using the key `my:queue` is declared, if necessary, with the default binding to the default exchange using the queue name as the routing key.  The `RabbitListener(..)` attribute then references the same queue using the same placeholder syntax.

You can use normal `ServiceCollection` calls such as `AddRabbitQueue(), AddRabbitExchange(), AddRabbitBinding()` to declare these entities when you need more advanced configuration.

Notice `IgnoreDeclarationExceptions` on the exchange in the first example.
This allows, for example, binding to an existing exchange that might have different settings (such as `internal`).
By default, the properties of an existing exchange must match.

You can also bind a queue to an exchange with multiple routing keys, as follows:

```csharp
...
[DeclareQueueBinding(Name = "anon.auto.exch.binding", QueueName = "#{anon}", ExchangeName = "auto.exch", RoutingKeys = new string[] {"red", "blue"})]
...
```

#### Enabling Listener Endpoint Annotations

To enable support for `RabbitListener` annotations, you must `AddRabbitServices()` to your .NET service container and for each service/class that has `[RabbitListener()]` attributes you must call `AddRabbitListener<>()`.
The following example shows how to do so:

```csharp
class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {
            // Add core services
            services.AddRabbitServices();
            ...
            services.AddSingleton<MyRabbitListener>();
            services.AddRabbitListeners<MyRabbitListener>();
            ...
        }
}

public class MyRabbitListener
{
    private ILogger<MyRabbitListener> logger;

    public MyRabbitListener(ILogger<MyRabbitListener> logger)
    {
        this.logger = logger;
    }

    [RabbitListener("myqueue")]
    public void Listen(string input)
    {
        logger.LogInformation(input);
    }
}
```

The `DirectMessageListenerContainerFactory` is used to create and configure `DirectMessageListenerContainer` instances.  Multiple named instances of the factory can exist in the service container.
By default, the infrastructure looks for a factory with the name `rabbitListenerContainerFactory` as the source to use when creating message listener containers.

You can customize the listener container factory and then associated that factory with specific `[RabbitListener()]` attributes.
Here is an example where a named factory is added to the service container and associated with a specific attribute.

```csharp
// Add named container factory jsonListenerContainerFactory
services.AddRabbitListenerContainerFactory("jsonListenerContainerFactory", (p, f) =>
{
    f.MessageConverter = new JsonMessageConverter();
});

...

[RabbitListener("myQueue", ContainerFactory = "jsonListenerContainerFactory")]
public void HandleAFoo(Foo foo)
{
    ....
}

```

The container factories provide methods for adding `IMessagePostProcessor` instances that are applied after receiving messages (before invoking the listener) and before sending replies.

See [Reply Management](#reply-management) for information about replies.

You can add a `RetryTemplate` and `RecoveryCallback` to the listener container factory.
It is used when sending replies.
The `RecoveryCallback` is invoked when retries are exhausted.
You can use a `SendRetryContextAccessor` to get information from the context during recovery processing.
The following example shows how to do so:

```csharp
// Add named container factory myFactory
services.AddRabbitListenerContainerFactory("myFactory", (p, f) =>
{
    f.RetryTemplate = retryTemplate;
    f.ReplyRecoveryCallback = retryRecoverCallback;
});

...
public class DefaultReplyRecoveryCallback : IRecoveryCallback
{
    public object Recover(IRetryContext context)
    {
        IMessage failed = SendRetryContextAccessor.GetMessage(context);
        Address replyTo = SendRetryContextAccessor.GetAddress(context);
        Exception e = ctx.LastException;
    }
}
```

The `[RabbitListener()]` attribute has a `Concurrency` property.
It supports property placeholders (`${...}`).
For the `DirectMessageListenerContainer`, the value must be a single integer value, which sets the `ConsumersPerQueue` property on the container.
This setting overrides the settings on the factory.

The `[RabbitListener()]` attribute also allows overriding the factories `AutoStartup` property via the `AutoStartup` property.

And finally, the `AckMode` property allows you to override the container factory's `AcknowledgeMode` property.

```csharp
[RabbitListener("manual.acks.1", Id = "manual.acks.1", AckMode = "AcknowledgeMode.MANUAL")
public void Manual1(string in, IModel channel,  [Header(RabbitMessageHeaders.DELIVERY_TAG)] ulong tag)
{
    ...
    channel.BasicAck(tag, false);
}
```

#### Message Conversion for Annotated Methods

There are two conversion steps in the pipeline before invoking the listener.
The first step uses a `IMessageConverter` to convert the incoming Steeltoe RabbitMQ `Message` with a `byte[]` to a  `Message` with a deserialized object if appropriate.
When the target method is invoked, the message payload is converted, if necessary, to the method parameter type.

The default `IMessageConverter` for the first step is a Steeltoe RabbitMQ `SimpleMessageConverter` that handles conversions involving
`string`s and .NET `Serializable` objects.
All others remain as a `byte[]`.
In the following discussion, we call this the "message converter".

The default converter for the second step is a `GenericMessageConverter`, which delegates to a conversion service (an instance of `DefaultConversionService`).
In the following discussion, we call this the "method argument converter".

To change the message converter, you can add it as a property to the container factory service.
The following example shows how to do so:

```csharp
// Configure the default container factory to use a JSON message converter
services.AddRabbitListenerContainerFactory((p, f) =>
{
   f.MessageConverter = new JsonMessageConverter();
});
```

This example configures a JSON converter that expects header information to be present to guide the conversion.

You can also use a `ContentTypeDelegatingMessageConverter`, which can handle conversion of different content types.

In most cases, it is not necessary to customize the method argument converter unless, for example, you want to use
a custom `ConversionService`.

If there are no type information headers, the type can be inferred from the target
method arguments.

>This type inference works only for `[RabbitListener()]` used at the method level.

See [JsonMessageConverter](#jsonmessageconverter) for more information.

If you wish to customize the method argument converter, you can do so, as follows:

```csharp
....
services.AddSingleton<IRabbitListenerConfigurer, MyRabbitListenerConfigurer>();
services.AddRabbitMessageHandlerMethodFactory((p, f) =>
{
    f.ServiceName = "myHandlerMethodFactory";
    var service = DefaultConversionService.Singleton as DefaultConversionService;
    service.AddConverter(new Foo1ToFoo2Converter());
    f.ConversionService = service;
    f.MessageConverter = new GenericMessageConverter(service);
});

public class MyRabbitListenerConfigurer : IRabbitListenerConfigurer
{
    private readonly IApplicationContext _context;

    public MyRabbitListenerConfigurer(IApplicationContext context)
    {
        _context = context;
    }

    public void ConfigureRabbitListeners(IRabbitListenerEndpointRegistrar registrar)
    {
        var handler = _context.GetService<IMessageHandlerMethodFactory>("myHandlerMethodFactory");
        registrar.MessageHandlerMethodFactory = handler;
    }
}

```

>IMPORTANT: For multi-method listeners ( see [Multiple Method Listeners](#multiple-method-listeners) ), the method selection is based on the payload of the message *after the message conversion*.
The method argument converter is called only after the method has been selected.

#### Programmatic Endpoint Registration

`IRabbitListenerEndpoint` provides a model of a Rabbit endpoint and is responsible for configuring the container for that model.
The infrastructure lets you configure endpoints programmatically in addition to the ones that are detected by the `[RabbitListener()]` attribute.
The following example shows how to do so:

```csharp

...
// Add core services
services.AddRabbitServices();
..

// Add a configurer to configure a listener endpoint
services.AddSingleton<IRabbitListenerConfigurer, MyRabbitEndpointConfigurer>();

public class MyRabbitEndpointConfigurer : IRabbitListenerConfigurer
{
    private IApplicationContext context;
    private ILoggerFactory loggerFactory;

    public MyRabbitEndpointConfigurer(IApplicationContext context, ILoggerFactory loggerFactory)
    {
        this.context = context;
        this.loggerFactory = loggerFactory;
    }

    public void ConfigureRabbitListeners(IRabbitListenerEndpointRegistrar registrar)
    {
        var listener = new MyMessageListener(loggerFactory.CreateLogger<MyMessageListener>());
        SimpleRabbitListenerEndpoint endpoint = new SimpleRabbitListenerEndpoint(context, listener);
        endpoint.Id = "manual-endpoint";
        endpoint.SetQueueNames("myqueue");
        registrar.RegisterEndpoint(endpoint);
    }
}

```

In the preceding example, we used `SimpleRabbitListenerEndpoint`, which provides the actual `IMessageListener` to invoke, but you could just as well build your own endpoint variant to describe a custom invocation mechanism.

It should be noted that you could just as well skip the use of `[RabbitListener()]` attributes altogether and register your endpoints programmatically through `IRabbitListenerConfigurer`.

#### Annotated Endpoint Method Signature

So far, for the most part, we have been injecting a simple `string` in our endpoint, but it can actually have a very flexible method signature.
The following example rewrites it to inject the `Order` with a custom header:

```csharp
public class MyService
{
    [RabbitListener("myqueue")]
    public void Listen(Order input, [Header("order_type")] string orderType)
    {
        logger.LogInformation(input.ToString());
        logger.LogInformation("Header=" + orderType);
    }
}
```

The following list shows the main elements you can inject in listener endpoints:

* The raw `IMessage<byte[]>`.
* The `RabbitMQ.Client.IModel` on which the message was received.
* The `IMessage<>` representing the incoming RabbitMQ message. Note that this message holds both the custom and the standard headers (as defined by `RabbitMessageHeaders`).
* `[Header()]`-annotated method arguments to extract a specific header value, including standard AMQP headers.
* `[Headers()]`-annotated argument that must also be assignable to `IDictionary<string, object>` for getting access to all headers.

A non-annotated element that is not one of the supported types (that is, `IMessage` and `IModel`) is considered to be the payload.
You can make that explicit by annotating the parameter with `[Payload()]`.

The following example shows how to inject an `IMessage<>`:

```csharp
public class MyService
{
    [RabbitListener("myqueue")]
    public void Listen(IMessage<Order> input)
    {
        logger.LogInformation(input.Payload.ToString());
    }
}
```

#### Listening to Multiple Queues

When you use the `Queues` property on the attribute, you can specify that the associated container can listen to multiple queues.
You can use a `[Header()]` annotation to make the queue name from which a message was received available to the POCO
method.

The following example shows how to do so:

```csharp
public class MyService
{

    [RabbitListener(Queues = new string[] { "queue1", "queue2" } )]
    public void ProcessOrder(string data,  [Header(RabbitMessageHeaders.CONSUMER_QUEUE)] string queue)
    {
        ...
    }
}
```

You can externalize the queue names by using property placeholders as well as well as use the attributes constructor to specify multiple queues.
The following example shows how to do so:

```csharp
public class MyService
{

    [RabbitListener("${config:queue1}", "${config:queue2}" )
    public void ProcessOrder(string data,  [Header(RabbitMessageHeaders.CONSUMER_QUEUE)] string queue)
    {
        ...
    }
}
```

#### Reply Management

The existing support in `MessageListenerAdapter` already lets your method have a non-void return type.
When that is the case, the result of the invocation is encapsulated in a message sent to the the address specified in the `ReplyToAddress` header of the original message, or to the default address configured on the listener.
You can also set that default address by using the `[SendTo()]` attribute on the listener method.

Assuming our `ProcessOrder` method should now return an `OrderStatus`, we can write it as follows to automatically send a reply:

```csharp
public class MyService
{
    [RabbitListener("myQueue")]
    [SendTo("status")]
    public OrderStatus ProcessOrder(Order order)
    {
        // order processing
        return status;
    }
}
```

If you need to set additional headers in a transport-independent manner, you could return a `IMessage` instead, something like the following:

```csharp
public class MyService
{
    [RabbitListener("myQueue")]
    [SendTo("status")]
    public IMessage<OrderStatus> ProcessOrder(Order order)
    {
        // order processing
        return MessageBuilder
            .WithPayload(status)
            .SetHeader("code", 1234)
            .Build();
    }
}
```

Alternatively, you can use a `IMessagePostProcessor` in the `BeforeSendReplyMessagePostProcessors` container factory property to add more headers.
The called type and method is made available in the reply message, which can be used in a message post processor to communicate the information back to the caller:

```csharp

// Configure default container factory with post processor
services.AddRabbitListenerContainerFactory((p, f) =>
{
    f.SetBeforeSendReplyPostProcessors(new AddSomeHeadersPostProcessor());
});

public class AddSomeHeadersPostProcessor : IMessagePostProcessor
{
    public IMessage PostProcessMessage(IMessage message, CorrelationData correlation)
    {
        var accessor = RabbitHeaderAccessor.GetMutableAccessor(message);
        accessor.SetHeader("calledService", accessor.Target.GetType().Name);
        accessor.SetHeader("calledMethod", accessor.TargetMethod.Name);
        return message;
    }

    public IMessage PostProcessMessage(IMessage message)
    {
        return PostProcessMessage(message, null);
    }
}

```

You can configure a `IReplyPostProcessor` on the `[RabbitListener()]` attribute to modify the reply message before it is sent as well; it is called after the `correlationId` header has been set up to match the request.

```csharp

// Add the reply post processor to the container .. has name set to echoCustomHeader
services.AddSingleton<IReplyPostProcessor, MyReplyPostProcessor>();

public class MyService
{
    [RabbitListener("test.header", Group = "testGroup", ReplyPostProcessor = "echoCustomHeader")]
    public string CapitalizeWithHeader(string input)
    {
        return input.ToUpper();
    }
}
...
public class MyReplyPostProcessor : IReplyPostProcessor
{
    public string ServiceName { get; set; } = "echoCustomHeader";

    public IMessage Apply(IMessage req, IMessage resp)
    {
        RabbitHeaderAccessor accessor = RabbitHeaderAccessor.GetMutableAccessor(resp);
        accessor.SetHeader("myHeader", req.Headers.Get<object>("myHeader"));
        return resp;
    }
}
```

The `[SendTo()]` attribute property `destination` is assumed as a reply `exchange` and `routingKey` pair that follows the `exchange/routingKey` pattern, where one of those parts can be omitted.
The valid values are as follows:

* `thing1/thing2`: The `replyTo` exchange and the `routingKey`.
* `thing1/`: The `replyTo` exchange and the default (empty) `routingKey`.
* `thing2` or `/thing2`: The `replyTo` `routingKey` and the default (empty) exchange.
* `/` or empty: The `replyTo` default exchange and the default `routingKey`.

Also, you can use `[SendTo()]` without a `destination` attribute.
This case is equal to an empty `sendTo` pattern as mentioned above.
`[SendTo()]` is used only if the inbound message does not have a `replyToAddress` message header.

The `[SendTo()]` destination can be a property placeholder, as shown in the following example:

```csharp
public class MyService
{
    [RabbitListener("test.sendTo.ph")]
    [SendTo("${config:ReplyTo}")]
    public string CapitalizeWithSendToPlaceholder(string foo)
    {
        return foo.ToUpper();
    }
}
```

The placeholder must evaluate to a `string`, which can be a simple queue name (sent to the default exchange) or with
the form `exchange/routingKey` as discussed prior to the preceding example.

For dynamic reply routing, the message sender should include a `reply_to` message header.

<!--
TODO: Add back in when expressions are supported

 or use the alternate
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
-->

#### Multiple Method Listeners

You can specify the `[RabbitListener()]` attribute at the class level.
Together with the  `[RabbitHandler()]` attribute, this lets a single listener invoke different methods, based on
the payload type of the incoming message.
This is best described using an example:

```csharp
[RabbitListener("someQueue", id="multi")]
[SendTo("my.reply.queue")]
public class MultiListenerService
{

    [RabbitHandler()]
    public string Thing2(Thing2 thing2)
    {
        ...
    }
    [RabbitHandler()]
    public string Cat(Cat cat)
    {
        ...
    }

    [RabbitHandler()]
    public string Hat([Header(RabbitMessageHeaders.RECEIVED_ROUTING_KEY)] string rk, [Payload()] Hat hat)
    {
        ...
    }

    [RabbitHandler(true)]
    public string defaultMethod(object object)
     {
        ...
    }
}
```

In this case, the individual `[RabbitHandler()]` methods are invoked if the converted payload is a `Thing2`, a `Cat`, or a `Hat`.
You should understand that the system must be able to identify a unique method based on the payload type.
The type is checked for assignability to a single parameter that has no attributes or that is annotated with the `[Payload()]` attribute.
Notice that the same method signatures apply, as discussed in the method-level `[RabbitListener()]` [described earlier](#messagelisteneradapter).

A `[RabbitHandler()]` method can be designated as the default method, which is invoked if there is no match on other methods.
At most, one method can be so designated.

>IMPORTANT: `[RabbitHandler()]` is intended only for processing message payloads after conversion, if you wish to receive the unconverted raw `IMessage` object, you must use `[RabbitListener()]` on the method, not the class.

#### Using Multiple [RabbitListener()]

The `[RabbitListener()` attribute is marked with `AllowMultiple=true`.
This means that the attribute can appear on the same element (method or class) multiple times.
In this case, a separate listener container is created for each attribute, each of which invokes the same listener service.

#### Handling Exceptions

By default, if an annotated listener method throws an exception, it is thrown to the container and the message is requeued and redelivered, discarded, or routed to a dead letter exchange, depending on the container and broker configuration.
Nothing is returned to the sender.

The `[RabbitListener()` attribute has two properties: `ErrorHandler` and `ReturnExceptions`.

These are not configured by default.

You can use the `ErrorHandler` property to provide the name of a `IRabbitListenerErrorHandler` implementation.
This interface has one method, as follows:

```csharp
public interface IRabbitListenerErrorHandler : IServiceNameAware
{
    object HandleError(IMessage origMessage, IMessage message, ListenerExecutionFailedException exception);
}
```

As you can see, you have access to the raw message received from the container, `origMessage`, and the `IMessage` object produced by the message converter. You also have access to the exception that was thrown by the listener (wrapped in a `ListenerExecutionFailedException`).
The error handler can either return some result (which is sent as the reply) or throw the original or a new exception (which is thrown to the container or returned to the sender, depending on the `ReturnExceptions` setting).

The `ReturnExceptions` property, when `true`, causes exceptions to be returned to the sender.
On the sender side, there is an available property, `ThrowReceivedExceptions`, which if set to `true` on the `RabbitTemplate`, re-throws the server-side exception.

>IMPORTANT: This mechanism generally works only with the default `SimpleMessageConverter`, which uses .NET serialization.

If you use JSON, consider using an `ErrorHandler` to return some other JSON friendly `Error` object when an exception is thrown.

The `IModel` is available in a messaging message header; this allows you to `ack` or `nack` the failed message when using `AcknowledgeMode.MANUAL`:

```csharp
public object HandleError(IMessage origMessage, IMessage message, ListenerExecutionFailedException exception)
 {
              ...
    message.Headers.Get<IModel>(RabbitMessageHeaders.CHANNEL)
        .BasicReject(message.Headers.Get<ulong>(RabbitMessageHeaders.DELIVERY_TAG),true);
}
```

#### Container Management

Containers created for attributes are not registered within the .NET service container.
You can obtain a collection of all containers by invoking `GetListenerContainers()` on the `IRabbitListenerEndpointRegistry` service.
You can then iterate over this collection, for example, to stop or start all containers or invoke the `ILifecycle` methods
on the registry itself, which will invoke the operations on each container.

You can also get a reference to an individual container by using its `Id`, using `GetListenerContainer(string id)`

You can obtain the `Id` values of the registered containers with `GetListenerContainerIds()`.

You can also assign a `Group` to the container on the `[RabbitListener()]` endpoint.
This provides a mechanism to get a reference to a subset of containers.
Adding a `Group` property causes a service of type `IMessageListenerContainerCollection` to be registered with the `IApplicationContext` with the group name.
You can then use the `IApplicationContext` and call `context.GetService<IMessageListenerContainerCollection>(group)` to obtain the containers.

>Note: By default, Steeltoe RabbitMQ messaging component uses the .NET Framework BinaryFormatter for object serialization. The BinaryFormatter has been marked deprecated in .NET 5 and can cause issues for you depending on the type of application you are building and running. There are various workarounds, including switching to JSON for serialization. See this [write-up](https://github.com/SteeltoeOSS/Steeltoe/issues/487#issuecomment-742006596) for more details on the issue and how you can work around it.

### RabbitListener with Batching

When receiving a [batch](#batching) of messages, the de-batching is normally performed by the container and the listener is invoked with one message at at time.
You can configure the listener container factory and listener to receive the entire batch in one call, simply set the factory's `BatchListener` property, and make the method payload parameter a `List`:

```csharp

// Configure the default container factory for batch listening
services.AddRabbitListenerContainerFactory((p, f) =>
{
    f.BatchListener = true;
});

[RabbitListener("batch.1")]
public void Listen1(List<Thing> input)
{
    ...
}

// or

[RabbitListener("batch.2")]
public void Listen2(List<IMessage<Thing>> input)
{
    ...
}
```

Setting the `BatchListener` property to true automatically turns off the `EeBatchingEnabled` container property in containers that the factory creates. Effectively, the de-batching is moved from the container to the listener adapter and the adapter creates the list that is passed to the listener.

A batch-enabled factory cannot be used with a [multi-method listener](#multiple-method-listeners).

Also, when receiving batched messages one-at-a-time, the last message contains a boolean header set to `true`.
This header can be obtained by adding the `[Header(RabbitMessageHeaders.LAST_IN_BATCH)] bool lastMessage` parameter to your listener method.
The header is mapped from `MessageProperties.isLastInBatch()`.
In addition, `RabbitMessageHeaders.BATCH_SIZE` is populated with the size of the batch in every message fragment.

### Using Container Factories

Listener container factories are used to support the `[RabbitListener()]` attribute and registering containers with the `IRabbitListenerEndpointRegistry`, as discussed [above](#programmatic-endpoint-registration).

They can be used to create any listener container - even a container without a listener.
Of course, a listener must be added before the container is started.

There are two ways to create such containers:

* Use a SimpleRabbitListenerEndpoint
* Add the listener after creation

The following example uses a `SimpleRabbitListenerEndpoint` to create a listener container:

```csharp

// Add core services
services.AddRabbitHostingServices();
services.AddRabbitDefaultMessageConverter();
services.AddRabbitConnectionFactory();
services.AddRabbitListenerContainerFactory();

....

// Add a container and use the default factory to create it.
services.AddRabbitDirecListenerContainer((p) =>
{
    var context = p.GetRequiredService<IApplicationContext>();
    var factory = p.GetRequiredService<IRabbitListenerContainerFactory>();
    var logFactory = p.GetRequiredService<ILoggerFactory>();

    var endpoint = new SimpleRabbitListenerEndpoint(context);
    endpoint.SetQueueNames("myqueue");
    endpoint.MessageListener = new MyMessageListener(logFactory.CreateLogger<MyMessageListener>());

    var container = factory.CreateListenerContainer(endpoint);
    container.ServiceName = "manualContainer";
    return container;
});
```

The following example adds the listener after creation:

```csharp

// Add core services
services.AddRabbitHostingServices();
services.AddRabbitDefaultMessageConverter();
services.AddRabbitConnectionFactory();
services.AddRabbitListenerContainerFactory();

...

// Add a container and use the factory to create it.
services.AddRabbitDirecListenerContainer((p) =>
{
    var factory = p.GetRequiredService<IRabbitListenerContainerFactory>();
    var logFactory = p.GetRequiredService<ILoggerFactory>();

    var container = factory.CreateListenerContainer() as DirectMessageListenerContainer;
    container.ServiceName = "manualContainer";
    container.SetQueueNames("myqueue");
    container.MessageListener = new MyMessageListener(logFactory.CreateLogger<MyMessageListener>());

    return container;
});

```

In either case, the listener can also be a `IChannelAwareMessageListener`, since it is now a sub-interface of `IMessageListener`.

These techniques are useful if you wish to create several containers with similar properties or use a pre-configured container factory.

>IMPORTANT: Containers created this way are normal container instances and are not registered in the `IRabbitListenerEndpointRegistry`.

### Asynchronous [RabbitListener()] Return Types

`[RabbitListener()]` (and `[RabbitHandler()]`) methods can be specified with asynchronous return types `Task` or `Task<Result>`, letting the reply, if present, be sent asynchronously.

>IMPORTANT: The listener container factory must be configured with `AcknowledgeMode.MANUAL` so that the consumer thread will not ack the message; instead, the asynchronous completion will ack or nack the message when the async operation completes.
When the async result is completed with an error, whether the message is requeued or not depends on the exception type thrown, the container configuration, and the container error handler.
By default, the message will be requeued, unless the container's `DefaultRequeueRejected` property is set to `false` (it is `true` by default).
If the async result is completed with an `RabbitRejectAndDontRequeueException`, the message will not be requeued.
If the container's `DefaultRequeueRejected` property is `false`, you can override that by setting the future's exception to a `ImmediateRequeueException` and the message will be requeued.
If some exception occurs within the listener method that prevents creation of the async result object, you MUST catch that exception and return an appropriate return object that will cause the message to be acknowledged or requeued.

### Threading and Asynchronous Consumers

A number of different threads are involved with asynchronous consumers.

Threads from the underlying `RabbitMQ.Client` are used to invoke the `IMessageListener` when a new message is delivered by the client.
A separate thread from the ThreadPool is used for the task that monitors the consumers.

## Containers and Broker Named queues

While it is preferable to use `AnonymousQueue` instances as auto-delete queues, you can use broker named queues with listener containers.
The following example shows how to do so:

```csharp
services.AddQueue(new AnonymousQueue() {
    ServiceName = "myqueue",
    IsDurable = false,
    IsExclusive = true,
    IsAutoDelete = true
});

services.AddRabbitDirecListenerContainer("container", (p, container) =>
{
    var context = p.GetRequiredService<IApplicationContext>();
   var logFactory = p.GetRequiredService<ILoggerFactory>();
   container.SetQueues(context.GetService<IQueue>("myqueue"));
   container.MessageListener = new MyMessageListener(logFactory.CreateLogger<MyMessageListener>());
   container.MissingQueuesFatal = false;
   container.Initialize();
});

```

When the `RabbitAdmin` declares queues, it updates the `Queue.ActualName` property with the name returned by the broker.
You must use `SetQueues()` when you configure the container for this to work, so that the container can access the declared name at runtime.
Just setting the names is insufficient.

>You cannot add broker-named queues to the containers while they are running.

>IMPORTANT: When a connection is reset and a new one is established, the new queue gets a new name.
Since there is a race condition between the container restarting and the queue being re-declared, it is important to set the container's `MissingQueuesFatal` property to `false`, since the container is likely to initially try to reconnect to the old queue.

## Message Converters

The `RabbitTemplate` also defines several methods for sending and receiving messages that delegate to a `IMessageConverter`.
The `IMessageConverter` provides a single method for each direction: one for converting *to* a `IMessage` and another for converting *from* a `IMessage`.
Notice that, when converting to a `IMessage`, you can also provide properties in addition to the object.
The `object` parameter typically corresponds to the Message body.
The following listing shows the `IMessageConverter` interface definition:

```csharp
public interface IMessageConverter : IServiceNameAware
{
    object FromMessage(IMessage message, Type targetClass);
    T FromMessage<T>(IMessage message);
    IMessage ToMessage(object payload, IMessageHeaders headers);
}
```

The relevant `IMessage`-sending methods on the `RabbitTemplate` are simpler than the methods we discussed previously, because they do not require the `IMessage` instance.
Instead, the `IMessageConverter` is responsible for "creating" each `IMessage` by converting the provided object to the byte array for the `IMessage` body and then adding any provided `IMessageHeaders`.
The following listing shows the definitions of the various methods:

```csharp
void ConvertAndSend(object message);

void ConvertAndSend(string routingKey, object message);

void ConvertAndSend(string exchange, string routingKey, object message);

void ConvertAndSend(object message, IMessagePostProcessor messagePostProcessor);

void ConvertAndSend(string routingKey, object message, IMessagePostProcessor messagePostProcessor);

void ConvertAndSend(string exchange, string routingKey, object message, IMessagePostProcessor messagePostProcessor);
```

On the receiving side, there are several methods to choose from. Some that accept the queue name and some that rely on the template's `DefaultQueueName` property having been set.
The following listing shows some of the methods:

```csharp
T ReceiveAndConvert<T>();

T ReceiveAndConvert<T>(string queueName);
```

>The `MessageListenerAdapter` mentioned in [Asynchronous Consumer](#asynchronous-consumer) also uses a `IMessageConverter`.

### SimpleMessageConverter

The default implementation of the `IMessageConverter` is called `SimpleMessageConverter`.
This is the converter that is used by an instance of `RabbitTemplate` if you do not explicitly configure an alternative.
It handles text-based content, serializable .NET objects, and byte arrays.

#### Converting From a IMessage SimpleMessageConverter

If the content type header of the input `IMessage` begins with "text" (for example,
"text/plain"), it also checks for the content-encoding header to determine the charset to be used when converting the `IMessage` body byte array to a `string`.
If no content-encoding header has been set on the input `IMessage`, it uses the UTF-8 charset by default.
If you need to override that default setting, you can configure an instance of `SimpleMessageConverter`, set its `DefaultCharset` property, and inject that into a `RabbitTemplate` instance.

If the content-type header value of the input `IMessage` is set to "application/x-dotnet-serialized-object", the `SimpleMessageConverter` tries to deserialize (rehydrate) the byte array into a .NET object.
While that might be useful for simple prototyping, we do not recommend relying on .NET serialization, since it leads to tight coupling between the producer and the consumer.
Of course, it also rules out usage of non-.NET systems on either side.

In the next two sections, we explore some alternatives for passing rich domain object content without relying on .NET serialization.

For all other content-types, the `SimpleMessageConverter` returns the `IMessage` body content directly as a byte array.

#### Converting To a IMessage SimpleMessageConverter

When converting to a `IMessage` from an arbitrary .NET Object, the `SimpleMessageConverter` likewise deals with byte arrays, strings, and serializable instances.
It converts each of these to bytes (in the case of byte arrays, there is nothing to convert), and it sets the content-type property accordingly.
If the `Object` to be converted does not match one of those types, the `IMessage` body is null.

### JsonMessageConverter

This section covers using the `JsonMessageConverter` to convert to and from a `IMessage`.
It has the following sections:

* [Converting to IMessage JsonMessageConverter](#converting-to-imessage-jsonmessageconverter)
* [Converting from IMessage JsonMessageConverter](#converting-from-imessage-jsonmessageconverter)

#### Converting to IMessage JsonMessageConverter

As mentioned in the previous section, relying on .NET serialization is generally not recommended.
One rather common alternative that is more flexible and portable across different languages and platforms is JSON (JavaScript Object Notation).
The converter can be configured on any `RabbitTemplate` instance to override its usage of the `SimpleMessageConverter` as a default.
The `JsonMessageConverter` uses the `NewtonSoft Json.NET` library.
The following example configures a `JsonMessageConverter`:

```csharp
services.AddRabbitTemplate("jsonTemplate", (p, t) =>
{
    t.MessageConverter = new JsonMessageConverter();
});
```

The `JsonMessageConverter` uses a `DefaultTypeMapper` by default.
Type information is added to (and retrieved from) `IMessageHeaders`.
If an inbound message does not contain type information in `IMessageHeaders`, but you know the expected type, you
can configure a static type by using the `DefaultType` property, as the following example shows:

```csharp
services.AddRabbitTemplate("jsonTemplate", (p, t) =>
{
    var converter = new JsonMessageConverter();
    converter.TypeMapper.DefaultType = typeof(object);
    t.MessageConverter = converter;
});
```

In addition, you can provide custom mappings from the value in the `__TypeId__` header.
The following example shows how to do so:

```csharp
services.AddRabbitTemplate("jsonTemplate", (p, t) =>
{
    var converter = new JsonMessageConverter();
    var mapper = new DefaultTypeMapper();
    mapper.IdClassMapping.Add("thing1", typeof(Thing1));
    mapper.IdClassMapping.Add("thing2", typeof(Thing2));
    converter.TypeMapper = mapper;
    t.MessageConverter = converter;
});
```

Now, if the sending system sets the `__TypeId__` header to `thing1`, the converter creates a `Thing1` object, and so on.

#### Converting from IMessage JsonMessageConverter

Inbound messages are converted to objects according to the type information added to headers by the sending system.

If type information is missing, the converter converts the JSON using `Json.NET` defaults.

Also, when you use `[RabbitListener()]` attributes (on methods), the inferred type information is added to the `IMessageHeaders`.
This lets the converter convert to the argument type of the target method.
This applies only if there is one parameter with no annotations or a single parameter with the `[Payload()]` attribute.
Parameters of type `IMessage` are ignored during the analysis.

>IMPORTANT: By default, the inferred type information will override the inbound `__TypeId__` and related headers created
by the sending system.
This lets the receiving system automatically convert to a different domain object.
This applies only if the parameter type is concrete (not abstract or an interface).
In all other cases, the `__TypeId__` and related headers is used.
There are cases where you might wish to override the default behavior and always use the `__TypeId__` information.
For example, suppose you have a `[RabbitListener()]` that takes a `Thing1` argument but the message contains a `Thing2` that
is a subclass of `Thing1` (which is concrete).
The inferred type would be incorrect.
To handle this situation, set the `Precedence` property on the `JsonMessageConverter` to `TypePrecedence.TYPE_ID` instead
of the default `TypePrecedence.INFERRED`.
(The property is actually on the converter's `DefaultTypeMapper`, but a setter is provided on the converter
for convenience.)
If you set a custom type mapper, you should set the property on the mapper instead.

>When converting from the `IMessage`, an incoming `headers.ContentType()` must be JSON-compliant (`headers.ContentType().Contains("json")` is used to check).
`application/json` is assumed if there is no `ContentType()` header, or it has the default value `application/octet-stream`.
To revert to the previous behavior (return an unconverted `byte[]`), set the converter's `AssumeSupportedContentType` property to `false`.
If the content type is not supported, a `WARN` log message `Could not convert incoming message with content-type [...]`, is emitted and `message.Payload` is returned as is - as a `byte[]`.
So, to meet the `JsonMessageConverter` requirements on the consumer side, the producer must add the `contentType` message header - for example, as `application/json` or by using the `JsonMessageConverter`, which sets the header automatically.
The following listing shows a number of converter calls:

```csharp
[RabbitListener()]
public void Thing1(Thing1 thing1) {...}

[RabbitListener()]
public void Thing1([Payload()] Thing1 thing1, [Header(RabbitMessageHeaders.CONSUMER_QUEUE)] string queue) {...}

[RabbitListener()]
public void Thing1(Thing1 thing1, IMessage<byte[]> message) {...}

[RabbitListener()]
public void Thing1(Thing1 thing1, IMessage<Foo> message) {...}

[RabbitListener()]
public void Thing1(Thing1 thing1, string bar) {...}  // Invalid

[RabbitListener()]
public void Thing1(Thing1 thing1, IMessage message) {...}
```

In the first four cases in the preceding listing, the converter tries to convert to the `Thing1` type.
The fifth example is invalid because we cannot determine which argument should receive the message payload.
With the sixth example, the Json defaults apply due to the generic type being an `object`.

You can, however, create a custom converter and use the `RabbitMessageHeaders.TARGET_METHOD` message header to decide which type to convert
the JSON to.

>This type inference can be achieved only when the `[RabbitListener()]` attribute is declared at the method level.
With class-level `[RabbitListener()]`, the converted type is used to select which `[RabbitHandler()]` method to invoke.
For this reason, the infrastructure provides the `RabbitMessageHeaders.TARGET` message header, which you can use in a custom
converter to determine the type.

#### Converting From a IMessage With RabbitTemplate

The `JsonMessageConverter` implements `ISmartMessageConverter`, which lets it be used with the `RabbitTemplate` methods that take a `Type` argument
or the generic methods.

This allows conversion of complex generic types, as shown in the following example:

```csharp
Thing1<Thing2<Cat, Hat>> thing1 =
    rabbitTemplate.ReceiveAndConvert<Thing1<Thing2<Cat, Hat>>>();

Thing1<Thing2<Cat, Hat>> thing1 =
    (Thing1<Thing2<Cat, Hat>>)rabbitTemplate.ReceiveAndConvert(typeof(<Thing1<Thing2<Cat, Hat>>>);
```

### Using ContentTypeDelegatingMessageConverter

This converter allows delegation to a specific `IMessageConverter` based on the content type header in the `IMessageHeaders`.
By default, it delegates to a `SimpleMessageConverter` if there is no `contentType` header or there is a value that matches none of the configured converters.

The following example configures a `ContentTypeDelegatingMessageConverter`:

```csharp
var converter = new ContentTypeDelegatingMessageConverter();
var messageConverter = new JsonMessageConverter();
converter.AddDelegate("foo/bar", messageConverter); // content type == foo/bar uses JSON
converter.AddDelegate(MessageHeaders.CONTENT_TYPE_JSON, messageConverter);
```

### Message Properties Converters

The `IMessageHeadersConverter` interface is used to convert between the Rabbit Client `IBasicProperties` and Steeltoe RabbitMQ `IMessageHeaders`.
The default implementation (`DefaultMessageHeadersConverter`) is usually sufficient for most purposes, but you can implement your own if needed.

## Modifying Messages Compression and More

A number of extension points exist.
They let you perform some processing on a message, either before it is sent to RabbitMQ or immediately after it is received.

As can be seen in [Message Converters](#message-converters), one such extension point is in the `RabbitTemplate` `ConvertAndReceive` operations, where you can provide a `IMessagePostProcessor`.
For example, after your POCO has been converted, the `IMessagePostProcessor` lets you set custom headers or properties on the `IMessage`.

Additional extension points have been added to the `RabbitTemplate` - `SetBeforePublishPostProcessors()` and `SetAfterReceivePostProcessors()`.
The first enables a post processor to run immediately before sending to RabbitMQ.
When using batching (see [Batching](#batching), this is invoked after the batch is assembled and before the batch is sent.
The second is invoked immediately after a message is received.

These extension points are used for such features as compression and, for this purpose, several `IMessagePostProcessor` implementations are provided.
`GZipPostProcessor`, `ZipPostProcessor` and `DeflaterPostProcessor` compress messages before sending, and `GUnzipPostProcessor`, `UnzipPostProcessor` and `InflaterPostProcessor` decompress received messages.

>The `GZipPostProcessor` can be configured with the `CopyHeaders = true` option to make a copy of the original message headers.
By default, these headers are reused for performance reasons, and modified with compression content encoding and the optional `RabbitMessageHeaders.SPRING_AUTO_DECOMPRESS` header.
If you retain a reference to the original outbound message, its values will change as well.
So, if your application retains a copy of an outbound message with these message post processors, consider turning the `CopyHeaders` option on.

Similarly, the `DirectMessageListenerContainer` also has a `SetAfterReceivePostProcessors()` method, letting the decompression be performed after messages are received by the container.

`AddBeforePublishPostProcessors()` and `AddAfterReceivePostProcessors()` have been added to the `RabbitTemplate` to allow appending new post processors to the list of before publish and after receive post processors respectively.
Also there are methods provided to remove the post processors.
Similarly, `DirectMessageListenerContainer` also has `AddAfterReceivePostProcessors()` and `RemoveAfterReceivePostProcessor()` methods added.
See the code on GitHub of `RabbitTemplate` and `DirectMessageListenerContainer` for more detail.

## Request and Reply Messaging

The `RabbitTemplate` also provides a variety of `SendAndReceive` methods that accept the same argument options that were described earlier for the one-way send operations (`exchange`, `routingKey`, and `IMessage`).
Those methods are quite useful for request-reply scenarios, since they handle the configuration of the necessary `reply-to` property before sending and can listen for the reply message on an exclusive queue that is created internally for that purpose.

Similar request-reply methods are also available where the `IMessageConverter` is applied to both the request and reply.
Those methods are named `ConvertSendAndReceive`.
See the [`RabbitTemplate`](https://github.com/SteeltoeOSS/Steeltoe/blob/release/3.2/src/Messaging/src/RabbitMQ/Core/RabbitTemplate.cs) code for more detail.

Each of the `SendAndReceive` method variants has an overloaded version that takes `CorrelationData`.
Together with a properly configured connection factory, this enables the receipt of publisher confirms for the send side of the operation.
See [Template Publisher Confirms and Returns](#template-publisher-confirms-and-returns) and the [`RabbitTemplate`](https://github.com/SteeltoeOSS/Steeltoe/blob/release/3.2/src/Messaging/src/RabbitMQ/Core/RabbitTemplate.cs) code for more detail.

You can configure the `RabbitTemplate` with the `NoLocalReplyConsumer` option to control a `noLocal` flag for the reply RabbitMQ Client `BasicConsume()` operation.
This is `false` by default.

### Reply Timeout

By default, the send and receive methods timeout after five seconds and return null.
You can modify this behavior by setting the `ReplyTimeout` property.
If you set the `Mandatory` property to `true` and the message cannot be delivered to a queue, an `RabbitMessageReturnedException` is thrown.
This exception has `ReturnedMessage`, `ReplyCode`, and `ReplyText` properties, as well as the `Exchange` and `RoutingKey` used for the send.

>This feature uses publisher returns.
You can enable it by setting `PublisherReturns` to `true` on the `CachingConnectionFactory` (see [Factory Publisher Confirms and Returns](#factory-publisher-confirms-and-returns).
Also, you must not have registered your own `ReturnCallback` with the `RabbitTemplate`.

A `ReplyTimedOut()` method has been added, letting subclasses of `RabbitTemplate` be informed of the timeout so that they can clean up any retained state.

When you use the default `DirectReplyToMessageListenerContainer`, you can add an error handler by setting the template's `ReplyErrorHandler` property.
This error handler is invoked for any failed deliveries, such as late replies and messages received without a correlation header.
The exception passed in is a `ListenerExecutionFailedException`, which has a `FailedMessage` property.

## RabbitMQ Direct Reply-To

>IMPORTANT: Starting with versions 3.4.0+, RabbitMQ server supports [Direct Reply-To](https://www.rabbitmq.com/direct-reply-to.html).
This eliminates the main reason for a fixed reply queue (to avoid the need to create a temporary queue for each request).
Direct reply-to is used by default (if supported by the server) instead of creating temporary reply queues.
When no `ReplyAddress` is provided (or it is set with a name of `amq.rabbitmq.reply-to`), the `RabbitTemplate` automatically detects whether direct reply-to is supported and either uses it or falls back to using a temporary reply queue.

Reply listeners are also supported with named queues (other than `amq.rabbitmq.reply-to`), allowing control of reply concurrency and so on.

If you wish to use a temporary, exclusive, auto-delete queue for each
reply, set the `UseTemporaryReplyQueues` property to `true`.
This property is ignored if you set a `ReplyAddress` on the `RabbitTemplate`.

You can change the criteria that dictate whether to use direct reply-to by sub-classing `RabbitTemplate` and overriding `UseDirectReplyTo()` method to check different criteria.
The method is called once only, when the first request is sent.

The template uses a `DirectReplyToMessageListenerContainer` when implementing direct reply-to.
The template takes care of correlating the replies, so there is no danger of a late reply going to a different sender.

### Message Correlation With A Reply Queue

When using a fixed reply queue (i.e. other than `amq.rabbitmq.reply-to`), you must provide correlation data so that replies can be correlated to requests.
See [RabbitMQ Remote Procedure Call (RPC)](https://www.rabbitmq.com/tutorials/tutorial-six-dotnet.html).
By default, the standard RabbitMQ `IBasicProperties CorrelationId` property is used to hold the correlation data.
However, if you wish to use a custom property to hold correlation data, you can set the `CorrelationKey` property on the `RabbitTemplate`.
The client and server must use the same header for correlation data.

By default, the template generates its own correlation ID (ignoring any user-supplied value).
If you wish to use your own correlation ID, set the `RabbitTemplate` instance's `UserCorrelationId` property to `true`.

>IMPORTANT: The correlation ID must be unique to avoid the possibility of a wrong reply being returned for a request.

### Reply Listener Container

When using RabbitMQ versions prior to 3.4.0 (i.e. no Direct Reply-To support), a new temporary queue is used for each reply.
However, a single fixed reply queue can be configured on the template, which can be more efficient and also lets you set specific arguments on that queue.
In this case, however, you must also configure and provide a `DirectMessageListenerContainer` to handle the reply processing.  You also need to configure the container with the RabbitTemplate as the `IMessageListener`.

All of the settings are allowed on the container are except for `ConnectionFactory` and `MessageConverter`, which are inherited from the template's configuration.

>IMPORTANT: If you run multiple instances of your application or use multiple `RabbitTemplate` instances, you *MUST* use a unique reply queue for each.
RabbitMQ has no ability to select messages from a queue, so, if they all use the same queue, each instance would compete for replies and not necessarily receive their own.

While the container and template share a connection factory, they do not share a channel.
Therefore, requests and replies are not performed within the same transaction (if transactional).

>You can specify the `ReplyAddress` property.
The `ReplyAddress` can contain an address with the form `<exchange>/<routingKey>` and the reply is routed to the specified exchange and routed to a queue bound with the routing key.

With this configuration, a `DirectListenerContainer` is used to receive the replies, with the `RabbitTemplate` being the `IMessageListener`.

>When the template does not use a fixed `ReplyAddress` (or is using [Direct Reply-To](#rabbitmq-direct-reply-to), a listener container is not needed and a internal temp container is created.
Direct Reply-To is the preferred mechanism when using RabbitMQ 3.4.0 or later.

When using a fixed reply queue, you need to define and wire up the reply listener container yourself.
If you fail to do this, the template never receives the replies and eventually times out and returns null as the reply to a call to a `SendAndReceive` method.

The `RabbitTemplate` detects if it has been
configured as a `IMessageListener` to receive replies.
If not, attempts to send and receive messages with a fixed reply address
fail with an `InvalidOperationException` (because the replies are never received).

Further, if a simple `ReplyAddress` (queue name) is used, the reply listener container verifies that it is listening to a queue with the same name.
This check cannot be performed if the reply address is an exchange and routing key and a debug log message is written.

>IMPORTANT: When wiring the reply listener and template yourself, it is important to ensure that the template's `ReplyAddress` and the container's `Queues` (or `QueueNames`) settings refer to the same queue.
The template inserts the reply address into the outbound message `ReplyTo` RabbitMQ Client property.

The following listing shows examples of how to manually wire up the services:

```csharp
services.AddRabbitQueue(new Queue("my.reply.queue"));
services.AddRabbitTemplate((p, t) =>
{
    var context = p.GetService<IApplicationContext>();
    var replyQueue = context.GetRabbitQueue("my.reply.queue");
    t.ReplyAddress = replyQueue.QueueName;
    t.ReplyTimeout = 60000;
    t.UseDirectReplyToContainer = false;
    t.ServiceName = "fixedReplyQRabbitTemplate";
});
services.AddRabbitDirectListenerContainer("replyListenContainer", (p, container) =>
{
    var context = p.GetService<IApplicationContext>();
    var template = context.GetRabbitTemplate("fixedReplyQRabbitTemplate");
    var replyQueue = context.GetRabbitQueue("my.reply.queue");
    container.ConnectionFactory = template.ConnectionFactory;
    container.SetQueue(replyQueue);
    container.MessageListener = template;
});
```

>IMPORTANT: When the reply times out (`ReplyTimeout`), the `SendAndReceive()` methods return null.

If a late reply is received, it is rejected (the template throws an `RabbitRejectAndDontRequeueException`).
If the reply queue is configured to send rejected messages to a dead letter exchange, the reply can be retrieved for later analysis.
To do so, bind a queue to the configured dead letter exchange with a routing key equal to the reply queue's name.

See the [RabbitMQ Dead Letter Documentation](https://www.rabbitmq.com/dlx.html) for more information about configuring dead lettering.
You can also take a look at the [FixedReplyQueueDeadLetterTest](https://github.com/SteeltoeOSS/Steeltoe/blob/release/3.2/src/Messaging/test/RabbitMQ.Test/Core/FixedReplyQueueDeadLetterTest.cs) test case for an example.

## Configuring the Broker

The AMQP specification describes how the protocol can be used to configure queues, exchanges, and bindings on the broker.
The RabbitMQ implementation of this functionality is in `RabbitAdmin` with the interface `IRabbitAdmin`.

The `IRabbitAdmin` interface is shown below:

```csharp
public interface IRabbitAdmin : IServiceNameAware
{
    void DeclareExchange(IExchange exchange);
    bool DeleteExchange(string exchangeName);
    IQueue DeclareQueue();
    string DeclareQueue(IQueue queue);
    bool DeleteQueue(string queueName);
    void DeleteQueue(string queueName, bool unused, bool empty);
    void PurgeQueue(string queueName, bool noWait);
    uint PurgeQueue(string queueName);
    void DeclareBinding(IBinding binding);
    void RemoveBinding(IBinding binding);
    Dictionary<string, object> GetQueueProperties(string queueName);
    QueueInformation GetQueueInfo(string queueName);
    void Initialize();
}
```

See also [Scoped Operations](#scoped-operations).

The `GetQueueProperties()` method returns some limited information about the queue (message count and consumer count).
The keys for the properties returned are available as constants in the `RabbitAdmin` (`QUEUE_NAME`, `QUEUE_MESSAGE_COUNT`, and `QUEUE_CONSUMER_COUNT`).

The no-arg `DeclareQueue()` method defines a queue on the broker with a name that is automatically generated.
The additional properties of this auto-generated queue are `Exclusive=true`, `AutoDelete=true`, and `Durable=false`.

The `DeclareQueue(Queue queue)` method takes a `Queue` object and returns the name of the declared queue.
If the `QueueName` property of the provided `Queue` is an empty `string`, the broker declares the queue with a generated name.
That name is returned to the caller.
That name is also added to the `ActualName` property of the `Queue`.
You can use this functionality programmatically only by invoking the `RabbitAdmin` directly.
When using auto-declaration by the admin when defining a queue declaratively in the application context, you can set the name property to `string.Empty` (the empty string).
The broker then creates the name.
Listener containers can use queues of this type.
See [Containers and Broker Named Queues](#containers-and-broker-named-queues) for more information.

This is in contrast to an `AnonymousQueue` where the framework generates a unique (`Guid`) name and sets `Durable` to `false` and `Exclusive`, `AutoDelete` to `true`.

See [AnonymousQueue](#anonymousqueue) to understand why `AnonymousQueue` is preferred over broker-generated queue names as well as how to control the format of the name.
Anonymous queues are declared with argument `x-queue-master-locator` set to `client-local` by default.
This ensures that the queue is declared on the node to which the application is connected.

See [Automatic Declaration of Exchanges Queues and Bindings](#automatic-declaration-of-exchanges-queues-and-bindings).

When the `CachingConnectionFactory` cache mode is `CHANNEL` (the default), the `RabbitAdmin` implementation does automatic lazy declaration of queues, exchanges, and bindings found in the .NET service container.
These components are declared as soon as a `IConnection` is opened to the broker. There are some extension methods that make this feature very easy:

```csharp
services.AddRabbitQueues(params IQueue[] queues);
services.AddRabbitQueue(IQueue queue);
services.AddRabbitQueue(string queueName, Action<IServiceProvider, Queue> configure = null);

services.AddRabbitExchanges(params IExchange[] exchanges)
services.AddRabbitExchange(IExchange exchange)
services.AddRabbitExchange(string exchangeName, string exchangeType, Action<IServiceProvider, IExchange> configure = null)

services.AddRabbitBindings(params IBinding[] bindings)
services.AddRabbitBinding(IBinding binding)
services.AddRabbitBinding(string bindingName, Binding.DestinationType bindingType, Action<IServiceProvider, IBinding> configure = null)
```

We can also declare all of the above with explicit names, which also serve as identifiers for their service definitions in the context.  The `ServiceName` property is used to set the identifier and can then be used with the `IApplicationContext`.
The following example configures a queue with an explicit service name that can be looked up.

```csharp
services.AddRabbitQueue("my.queue.name", (p, q) => { q.ServiceName = "myServiceName" });
```

>TIP: You can provide both `ServiceName` and `XXXName` settings for the above.
This lets you refer to the queue (for example, in a binding) by a `ServiceName` that is independent of the queue name.
It also allows standard Steeltoe features (such as property placeholders, etc.)

Queues can be configured with additional arguments - for example, `x-message-ttl`.
The following example shows how to do so:

```csharp
var queue = new Queue("myQ");
queue.Arguments.Add("x-dead-letter-exchange", "myDLX");
queue.Arguments.Add("x-dead-letter-routing-key", "dlqRK");
services.AddRabbitQueue(queue);
```

The `x-queue-master-locator` argument is supported as a first class property through the `MasterLocator` property setter on the `Queue` class.
Anonymous queues are declared with this property set to `client-local` by default.
This ensures that the queue is declared on the node the application is connected to.

>IMPORTANT: The RabbitMQ broker does not allow declaration of a queue with mismatched arguments.
For example, if a `queue` already exists with no `time to live` argument, and you attempt to declare it with (for example) `key="x-message-ttl" value="100"`, an exception is thrown.

By default, the `RabbitAdmin` immediately stops processing all declarations when any exception occurs.
This could cause downstream issues, such as a listener container failing to initialize because another queue (defined after the one in error) is not declared.

This behavior can be modified by setting the `IgnoreDeclarationExceptions` property to `true` on the `RabbitAdmin` instance.
This option instructs the `RabbitAdmin` to log the exception and continue declaring other elements.
This is a global setting that applies to all elements.
Queues, exchanges, and bindings have a similar property that applies to just those elements.
This property takes effect on any exception, including `TimeoutException` and others.

You can configure the `HeadersExchange` to match on multiple headers.
You can also specify whether any or all headers must match.
The following example shows how to do so:

```csharp
var exchange = new HeadersExchange("headers-test");
var queue = new Queue("bucket");
var binding = new QueueBinding("headers-test.bucket.binding");
binding.Arguments.Add("foo", "bar");
binding.Arguments.Add("baz", "qux");
binding.Arguments.Add("x-match", "all");
```

<!-- You can configure `Exchanges` with an `internal` flag (defaults to `false`) and such an
`Exchange` is properly configured on the Broker through a `RabbitAdmin` (if one is present in the application context).
If the `internal` flag is `true` for an exchange, RabbitMQ does not let clients use the exchange.
This is useful for a dead letter exchange or exchange-to-exchange binding, where you do not wish the exchange to be used
directly by publishers. -->

To see how to use to configure the AMQP infrastructure, lets look at a Stock sample application,
that has both client and server configuration needs.
The following listing shows the code for the sample:

```csharp

// Configuration code common to both client and server

services.ConfigureRabbitOptions(Configuration);
services.AddRabbitHostingServices();
services.AddRabbitMessageHandlerMethodFactory();
services.AddRabbitListenerContainerFactory();
services.AddRabbitListenerEndpointRegistry();
services.AddRabbitListenerEndpointRegistrar();
services.AddRabbitListenerAttributeProcessor();
services.AddRabbitJsonMessageConverter();
services.AddRabbitTemplate();

services.AddRabbitConnectionFactory((p, f) =>
{
    f.Username = "guest";
    f.Password = "guest";
    f.Host = "localhost";
});

services.AddRabbitExchange(new TopicExchange("app.stock.market.data"));

...

}
```

In the Stock application, the server is configured using this additional code:

```csharp
services.AddRabbitQueue(new Queue("app.stock.request"));
```

The end result is that `TopicExchange` and `Queue` are declared to the broker upon application startup.
There is no binding of  `TopicExchange` to a queue in the server configuration, as that is done in the client application.
The stock request queue, however, is automatically bound to the RabbitMQ default exchange.
This behavior is defined by the specification.

The client configuration is a little more interesting.
Its declaration follows:

```csharp
services.AddRabbitQueue((p) =>
{
    var admin = p.GetRabbitAdmin();
    var marketDataQueue = admin.DeclareQueue();
    marketDataQueue.ServiceName = "marketDataQueue";
    return marketDataQueue;
});

services.AddRabbitBinding((p) =>
{
    var context = p.GetApplicationContext();
    var marketDataQueue = context.GetRabbitQueue("marketDataQueue");
    var marketDataExchange = context.GetRabbitExchange("app.stock.market.data");
    var marketDataRoutingKey = context.Configuration["stocks:queue:pattern"];
    return BindingBuilder.Bind(marketDataQueue).To(marketDataExchange).With(marketDataRoutingKey);
});
...
}
```

The client declares another queue through the `DeclareQueue()` method on the `RabbitAdmin`.
It binds that queue to the market data exchange with a routing pattern that is externalized in the `IConfiguration`.

### Builder API for Queues and Exchanges

Steeltoe RabbitMQ provides a convenient fluent API for configuring `Queue` and `Exchange` objects.
The following example shows how to use it:

```csharp
var fooQueue = QueueBuilder.NonDurable("foo")
        .AutoDelete()
        .Exclusive()
        .WithArgument("foo", "bar")
        .Build();

var fooExchange = ExchangeBuilder.DirectExchange("foo")
      .AutoDelete()
      .WithArgument("foo", "bar")
      .Build();

```

See the code for [QueueBuilder](https://github.com/SteeltoeOSS/Steeltoe/blob/release/3.2/src/Messaging/src/RabbitMQ/Config/QueueBuilder.cs) and [ExchangeBuilder](https://github.com/SteeltoeOSS/Steeltoe/blob/release/3.2/src/Messaging/src/RabbitMQ/Config/ExchangeBuilder.cs) for more information.

The `ExchangeBuilder` creates durable exchanges by default, to be consistent with the simple constructors on the individual `AbstractExchange` classes.
To make a non-durable exchange with the builder, use `.Durable(false)` before invoking `.Build()`.

Steeltoe RabbitMQ uses fluent APIs to add "well known" exchange and queue arguments, as follows:

```csharp
var allargs = QueueBuilder.NonDurable("all.args.1")
        .TTL(1000)
        .Expires(200_000)
        .MaxLength(42)
        .MaxLengthBytes(10_000)
        .Overflow(QueueBuilder.OverFlow.RejectPublish)
        .DeadLetterExchange("dlx")
        .DeadLetterRoutingKey("dlrk")
        .MaxPriority(4)
        .Lazy()
        .Masterlocator(QueueBuilder.MasterLocator.MinMasters)
        .SingleActiveConsumer()
        .Build();

var alternate = ExchangeBuilder.DirectExchange("ex.with.alternate")
            .Durable(true)
            .Alternate("alternate")
            .Build();

```

### Declaring Collections of Exchanges Queues and Bindings

You can wrap collections of `Declarable` objects (`Queue`, `Exchange`, and `Binding`) in `Declarables` objects.
The `RabbitAdmin` detects such services (as well as discrete `Declarable` services) in the service container, and declares the contained objects on the broker whenever a connection is established (initially and after a connection failure).
The following example shows how to do so:

```csharp
services.AddSingleton<Declarables>(
    new Declarables("decl1",
        new DirectExchange("e2", false, true),
        new DirectExchange("e3", false, true))
    );
services.AddSingleton<Declarables>(
    new Declarables("decl2",
        new Queue("q2", false, false, true),
        new Queue("q3", false, false, true))
    );
services.AddSingleton<Declarables>(
    new Declarables("decl3",
        new Binding("b1", "q2", DestinationType.QUEUE, "e2", "k2", null),
        new Binding("b2", "q3", DestinationType.QUEUE, "e3", "k3", null))
    );
services.AddSingleton<Declarables>(
    new Declarables("decl4",
        new DirectExchange("e4", false, true),
        new Queue("q4", false, false, true),
        new Binding("b3", "q4", DestinationType.QUEUE, "e4", "k4", null))
    );
```

You can use the `GetDeclarablesByType<T>` method of `Declarables` as a convenience as follows.

```csharp
var declarables = new Declarables("decl4",
        new DirectExchange("e4", false, true),
        new Queue("q4", false, false, true),
        new Binding("b3", "q4", DestinationType.QUEUE, "e4", "k4", null));
var queue = declarables.GetDeclarableByType<Queue>();
```

### Conditional Declaration

By default, all queues, exchanges, and bindings are declared by all `RabbitAdmin` instances (assuming they have `AutoStartup="true"`).

The `RabbitAdmin` has a property `ExplicitDeclarationsOnly` (which is `false` by default); when this is set to `true`, the admin declares only entities that are explicitly configured to be declared by that admin.

>You can conditionally declare these elements.
This is particularly useful when an application connects to multiple brokers and needs to specify with which brokers a particular element should be declared.

The classes representing these elements implement `IDeclarable`, which has two properties: `ShouldDeclare` and `DeclaringAdmins`.
The `RabbitAdmin` uses these methods to determine whether a particular instance should actually process the declarations on its `IConnection`.

>By default, the `ShouldDeclare` property is `true` and, if the `DeclaringAdmins` is not supplied (or is empty), then all `RabbitAdmin` instances declare the object (as long as the admin `AutoStartup` property is `true`, the default, and the admin `ExplicitDeclarationsOnly` property is false).

In the following example, the components are declared by `admin1` but not by`admin2`:

```csharp
services.AddRabbitAdmin("admin1", (p, a) => {
    var cf1 = p.GetRabbitConnectionFactory("cf1");
    a.ConnectionFactory = cf1;
});
services.AddRabbitAdmin("admin2", (p, a) => {
    var cf2 = p.GetRabbitConnectionFactory("cf2");
    a.ConnectionFactory = cf2;
});
services.AddRabbitQueue("foo", (p, q) => {
    var admin1 = p.GetRabbitAdmin("admin1");
    q.SetAdminsThatShouldDeclare(admin1);
});

services.AddRabbitExchange("bar", ExchangeType.DIRECT, (p, e) => {
    var admin1 = p.GetRabbitAdmin("admin1");
    var de = e as DirectExchange;
    de.SetAdminsThatShouldDeclare(admin1);
});
services.AddRabbitBinding("foo.binding", Binding.DestinationType.QUEUE, (p, b) => {
    var exchange = p.GetRabbitExchange("bar");
    var binding = b as QueueBinding;
    binding.Exchange = exchange.ExchangeName;
    binding.Destination = "foo";
    binding.RoutingKey = "foo";
    var admin1 = p.GetRabbitAdmin("admin1");
    binding.SetAdminsThatShouldDeclare(admin1);
});

```

### A Note On the `ServiceName` and `*Name` Properties

The `*Name` properties on `Queue` and `*Exchange` types reflects the name of the entity in the broker.
For queues, if the `*Name` is omitted, an anonymous queue is created [AnonymousQueue](#anonymousqueue).

### AnonymousQueue

In general, when you need a uniquely-named, exclusive, auto-delete queue, we recommend that you use the `AnonymousQueue`
instead of broker-defined queue names (using `string.Empty` as a `QueueName` causes the broker to generate the queue name for you).

This is because:

1. The queues are actually declared when the connection to the broker is established.
This is long after the services are created and wired together.
Many services that use queues need to know its name.
In fact, the broker might not even be running when the application is started.
2. If the connection to the broker is lost for some reason, the admin re-declares the `AnonymousQueue` with the same name.
If we used broker-declared queues, the queue name would change.

You can control the format of the queue name used by `AnonymousQueue` instances.

By default, the queue name is prefixed by `spring.gen-` followed by a base64 representation of the `Guid` - for example: `spring.gen-MRBv9sqISkuCiPfOYfpo4g`.

You can provide an `AnonymousQueue.NamingStrategy` implementation in a constructor argument if you want further control.
The following example shows how to do so:

```csharp
var anon1 = new AnonymousQueue("anon1");
var anon2 = new AnonymousQueue(new AnonymousQueue.Base64UrlNamingStrategy("something-"));
var anon3 = new AnonymousQueue(AnonymousQueue.UUIDNamingStrategy.DEFAULT);
```

The first example generates a queue name prefixed by `spring.gen-` followed by a base64 representation of the `Guid` - for example: `spring.gen-MRBv9sqISkuCiPfOYfpo4g`.
The second  generates a queue name prefixed by `something-` followed by a base64 representation of the `Guid`.
The third generates a name by using only the Guid (no base64 conversion) - for example, `f20c818a-006b-4416-bf91-643590fedb0e`.

The base64 encoding uses the "URL and Filename Safe Alphabet" from RFC 4648.
Trailing padding characters (`=`) are removed.

You can provide your own naming strategy, whereby you can include other information (such as the application name or client host) in the queue name.

Anonymous queues are declared with argument `x-queue-master-locator` set to `client-local` by default.
This ensures that the queue is declared on the node to which the application is connected.
You can change this behavior by calling `queue.SetMasterLocator(null)` after constructing the instance.

<!--

TODO: When we support Application events on the context .. we can add this

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
-->

## Delayed Message Exchange

You can read more about the Delayed Message Exchange Plugin [here](https://www.rabbitmq.com/blog/2015/04/16/scheduling-messages-with-rabbitmq/).

>The plugin is currently marked as experimental but has been available for over a year (at the time of writing).
If changes to the plugin make it necessary, we plan to add support for such changes as soon as practical.
For that reason, this support in Steeltoe RabbitMQ should be considered experimental, too.

To use a `RabbitAdmin` to declare an exchange as delayed, you can set the `IsDelayed` property on the exchange to `true`.
The `RabbitAdmin` uses the exchange type (`Direct`, `Fanout`, and so on) to set the `x-delayed-type` argument and declare the exchange with type `x-delayed-message`.

To send a delayed message, you can set the `x-delay` header through `IMessageHeaders`, as the following examples show:

```csharp
var headers = new RabbitHeaderAccessor();
headers.Delay = 15000;
template.Send(exchange, routingKey,
        MessageBuilder.WithPayload(Encoding.UTF8.GetBytes("foo")).SetHeaders(headers).Build());

// Second example
private static readonly DelayMessagePostProcessor addDelayHeader = new DelayMessagePostProcessor();
rabbitTemplate.ConvertAndSend(exchange, routingKey, "foo", addDelayHeader);

....

public class DelayMessagePostProcessor : IMessagePostProcessor
{
    public IMessage PostProcessMessage(IMessage message, CorrelationData correlation)
    {
        var accessor = RabbitHeaderAccessor.GetMutableAccessor(message);
        accessor.Delay = 15000;
        return message;
    }

    public IMessage PostProcessMessage(IMessage message)
    {
        return PostProcessMessage(message, null);
    }
}
```

To check if a message was delayed, use the `ReceivedDelay()` method on the `IMessageHeaders`.
It is a separate header value to avoid unintended propagation to an output message generated from an input message.

## Exception Handling

Many operations with the RabbitMQ  client can throw exceptions.
The `RabbitTemplate`, `DirectMessageListenerContainer`, and other Steeltoe RabbitMQ components catch those exceptions and convert them into one of the exceptions within `RabbitException` hierarchy.
Those are defined in the 'Steeltoe.Messaging.RabbitMQ.Exceptions' namespace, and `RabbitException` is the base of the hierarchy.

When a listener throws an exception, it is wrapped in a `ListenerExecutionFailedException`.
Normally, the message is rejected and requeued by the broker.
Setting `DefaultRequeueRejected` to `false` causes messages to be discarded (or routed to a dead letter exchange).
As discussed in [Message Listeners and the Asynchronous Case](#message-listeners-and-the-asynchronous-case), the listener can throw an `RabbitRejectAndDontRequeueException` (or `ImmediateRequeueException`) to conditionally control this behavior.

However, there is a class of errors where the listener cannot control the behavior.
When a message that cannot be converted is encountered (for example, an invalid `RabbitMessageHeaders.CONTENT_ENCODING` header), some exceptions are thrown before the message reaches user code.
With `DefaultRequeueRejected` set to `true` (default) (or throwing an `ImmediateRequeueException`), such messages would be redelivered over and over.

The default `IErrorHandler` is a `ConditionalRejectingErrorHandler` that rejects (and does not requeue) messages that fail with an non-recoverable error.
Specifically, it rejects messages that fail with the following errors:

* `MessageConversionException`: Can be thrown when converting the incoming message payload using a `MessageConverter`.
* `MessageConversionException`: Can be thrown by the conversion service if additional conversion is required when mapping to a `[RabbitListener()]` method.
* `MethodArgumentResolutionException`: Can be thrown if the inbound message was converted to a type that is not correct for the target method.
* `MissingMethodException`
* `InvalidCastException`

You can configure an instance of this error handler with a `IFatalExceptionStrategy` so that you can provide your own rules for conditional message rejection.
In addition, the `ListenerExecutionFailedException` now has a `FailedMessage` property that you can use in the decision.
If the `IFatalExceptionStrategy.IsFatal()` method returns `true`, the error handler throws an `RabbitRejectAndDontRequeueException`.
The default `IFatalExceptionStrategy` logs a warning message when an exception is determined to be fatal.

A convenient way to add user exceptions to the fatal list is to subclass `DefaultExceptionStrategy` and override the `IsUserCauseFatal(Exception cause)` method to return `true` for fatal exceptions.

A common pattern for handling DLQ messages is to set a `time-to-live` on those messages as well as additional DLQ configuration such that these messages expire and are routed back to the main queue for retry.
The problem with this technique is that messages that cause fatal exceptions loop forever.
The `ConditionalRejectingErrorHandler` detects an `x-death` header on a message that causes a fatal exception to be thrown.
The message is logged and discarded.
You can change this behavior by setting the `DiscardFatalsWithXDeath` property on the `ConditionalRejectingErrorHandler` to `false`.

>IMPORTANT: Messages with these fatal exceptions are rejected and NOT requeued by default, even if the container acknowledge mode is MANUAL.
These exceptions generally occur before the listener is invoked so the listener does not have a chance to ack or nack the message so it remained in the queue in an non-acked state.
To change this behavior, set the `RejectManual` property on the `ConditionalRejectingErrorHandler` to `false`.

## Transactions

The Steeltoe RabbitMQ framework has support for automatic transaction management in the synchronous and asynchronous use cases with a number of different semantics.
This makes many if not most common messaging patterns easy to implement.

There are two ways to signal the desired transaction semantics to the framework.
In both the `RabbitTemplate` and `DirectMessageListenerContainer`, there is a flag `IsChannelTransacted` which, if `true`, tells the framework to use a transactional channel and to end all operations (send or receive) with a `Commit` or `Rollback` (depending on the outcome), with an exception signaling a `Rollback`.
Another signal is to provide an external transaction with a `IPlatformTransactionManager` implementation as a context for the ongoing operation.
If there is already a transaction in progress when the framework is sending or receiving a message, and the `IsChannelTransacted` flag is `true`, the `Commit` or `Rollback` of the messaging transaction is deferred until the end of the external current transaction.
If the `IsChannelTransacted` flag is `false`, no transaction semantics apply to the messaging operation (it is auto-acked).

>Currently the only supported `IPlatformTransactionManager` is the `RabbitTransactionManger`. See the following section on the limitations associated this transaction manager.

The `IsChannelTransacted` flag is a configuration time setting.
It is declared and processed once when the RabbitMQ components are created, usually at container or template startup.
The external transaction is more dynamic in principle because the system responds to the current thread state at runtime.
However, in practice, it is often also a configuration setting, when the transactions are layered onto an application declaratively.

For synchronous use cases with `RabbitTemplate`, the external transaction is provided by the caller.
The following example shows a programmatic approach, where the template has been configured with `IsChannelTransacted=true`:

```csharp
public void DoSomething()
{
    template.IsChannelTransacted = true;
    var rabbitTransManager = new RabbitTransactionManager(template.ConnectionFactory);
    var transTemplate = new TransactionTemplate(rabbitTransManager);
    var incoming = transTemplate.Execute(status => {
        var received = template.ReceiveAndConvert<string>();
        var outgoing = ProcessInput(received);
        template.ConvertAndSend(outgoing);
        return received;
    });
}
```

In the preceding example, a `string` payload is received, converted, and sent as a message body inside a transaction managed by the `RabbitTransactionManager`.
If the processing fails with an exception, the incoming message is returned to the broker, and the outgoing message is not sent.
This applies to any operations with the `RabbitTemplate` inside the method (unless, for instance, the `Channel` is directly manipulated to commit the transaction early).

For asynchronous use cases with `DirectMessageListenerContainer`, if an external transaction is needed, it has to be requested by the container when it sets up the listener.
To signal that an external transaction is required, you must provide an implementation of `IPlatformTransactionManager` to the container when it is configured.
The following example shows how to setup a factory to do so:

```csharp
// Add named container factory txListenerContainerFactory
services.AddRabbitListenerContainerFactory((p, f) =>
{
    var cf = p.GetRabbitConnectionFactory();
    var rabbitTransManager = new RabbitTransactionManager(cf);
    f.ServiceName = "txListenerContainerFactory";
    f.IsChannelTransacted = true;
    f.TransactionManager = rabbitTransManager;
});
```

In the preceding example, the transaction manager is added to the named factory, and the `IsChannelTransacted` flag is also set to `true`.
The effect is that containers created by this factory will have transactions enabled, and if the listener fails with an exception, the transaction is rolled back, and the message is also returned to the broker.
Significantly, if the transaction fails to commit the RabbitMQ transaction is also rolled back, and the message is returned to the broker.
This is sometimes known as a "Best Efforts 1 Phase Commit", and is a very powerful pattern for reliable messaging.
If the `IsChannelTransacted` flag was set to `false` (the default) in the preceding example, the external transaction would still be provided for the listener, but all messaging operations would be auto-acked, so the effect is to commit the messaging operations even on a rollback of the business operation.
<!--
TODO: This is partially implemented.. needs more code to fully implement.

### Conditional Rollback

Prior to version 1.6.6, adding a rollback rule to a container's `transactionAttribute` when using an external transaction manager (such as database) had no effect.
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
-->

### A note on Rollback of Received Messages

RabbitMQ transactions apply only to messages and acks sent to the broker.
Consequently, when there is a rollback of a transaction and a message has been received, Steeltoe RabbitMQ has to not only rollback the transaction but also manually reject the message (sort of a nack, but that is not what the specification calls it).
The action taken on message rejection is independent of transactions and depends on the `DefaultRequeueRejected` property (default: `true`).
For more information about rejecting failed messages, see [Message Listeners and the Asynchronous Case](#message-listeners-and-the-asynchronous-case).

For more information about RabbitMQ transactions and their limitations, see [RabbitMQ Broker Semantics](https://www.rabbitmq.com/semantics.html).

>Prior to RabbitMQ 2.7.0, such messages (and any that are un-acked when a channel is closed or aborts) went to the back of the queue on a Rabbit broker.
Since 2.7.0, rejected messages go to the front of the queue, in a similar manner to JMS rolled back messages.

### Using RabbitTransactionManager

The [RabbitTransactionManager](https://github.com/SteeltoeOSS/Steeltoe/blob/release/3.2/src/Messaging/src/RabbitMQ/Transaction/RabbitTransactionManager.cs) is the only `IPlatformTransactionManager` supported at this point.
This transaction manager is an implementation of the [`IPlatformTransactionManager`] interface and should be used with a single RabbitMQ `IConnectionFactory`.

>IMPORTANT: This strategy is not able to provide XA transactions - for example, in order to share transactions between messaging and database access.

Application code is required to retrieve the transactional Rabbit resources through `ConnectionFactoryUtils.GetTransactionalResourceHolder(IConnectionFactory, bool)` instead of a standard `IConnection.CreateChannel()` call with subsequent channel creation.
When using the Steeltoe RabbitMQ `RabbitTemplate`, it will auto-detect a thread-bound `IModel` and automatically participate in its transaction.

### Message Listener Container Configuration

There are quite a few options for configuring a `DirectMessageListenerContainer` related to transactions and quality of service, and some of them interact with each other.
The following table shows the container property names you can use to configure the functionality.

| Property | Description |
| -------- | ----------- |
| AckTimeout | When `MessagesPerAck` is set, this timeout is used as an alternative to send an ack. When a new message arrives, the count of un-acked messages is compared to `MessagesPerAck`, and the time since the last ack is compared to this value. If either condition is `true`, the message is acknowledged. When no new messages arrive and there are un-acked messages, this timeout is approximate since the condition is only checked each `MonitorInterval`. See also `MessagesPerAck` and `MonitorInterval` in this table. |
| AcknowledgeMode | * `NONE` - No acks are sent (incompatible with `IsChannelTransacted=true`). RabbitMQ calls this autoack, because the broker assumes all messages are acked without any action from the consumer.<br\> * `MANUAL`: The listener must acknowledge all messages by calling `Channel.basicAck()`.<br\> * `AUTO`: The container acknowledges the message automatically, unless the `MessageListener` throws an exception. Note that `acknowledgeMode` is complementary to `channelTransacted` - if the channel is transacted, the broker requires a commit notification in addition to the ack. This is the default mode. |
| AfterReceivePostProcessors | An array of `IMessagePostProcessor` instances that are invoked before invoking the listener. Post processors can implement `IPriorityOrdered` or `IOrdered`. The array is sorted with un-ordered members invoked last. If a post processor returns `null`, the message is discarded (and acknowledged, if appropriate). |
| AlwaysRequeueWithTxManagerRollback (N/A) | Set to `true` to always requeue messages on rollback when a transaction manager is configured. |
| AutoDeclare | When set to `true` (default), the container uses a `RabbitAdmin` to redeclare all RabbitMQ objects (queues, exchanges, bindings), if it detects that at least one of its queues is missing during startup, perhaps because it is an `IsAutoDelete` or an expired queue, but the re-declaration proceeds if the queue is missing for any reason. To disable this behavior, set this property to `false`. Note that the container fails to start if all of its queues are missing. NOTE: For `AutoDeclare` to work, there must be exactly one `RabbitAdmin` in the container, or a reference to a specific instance must be configured on the container using the `RabbitAdmin` property. |
| AutoStartup | Flag to indicate that the container should start when the container is started and the `ISmartLifecycle` callbacks are initialized. Defaults to `true`, but you can set it to `false` if your broker might not be available on startup and call `Start()` later manually when you know the broker is ready. |
| BatchingStrategy | The strategy used when de-batching messages. Default `SimpleDebatchingStrategy`. See [Batching](#batching) and [RabbitListener with Batching](#rabbitlistener-with-batching). |
| IsChannelTransacted | Boolean flag to signal that all messages should be acknowledged in a transaction (either manually or automatically). |
| ConnectionFactory | A reference to the `IConnectionFactory`. |
| ConsumerTagStrategy | Set an implementation of [ConsumerTagStrategy](#consumer-tags), enabling the creation of a (unique) tag for each consumer. |
| ConsumersPerQueue | The number of consumers to create for each configured queue. See [Listener Concurrency](#listener-concurrency). |
| DefaultRequeueRejected | Determines whether messages that are rejected because the listener threw an exception should be requeued or not. Default: `true`. |
| ErrorHandler | A reference to an `IErrorHandler` strategy for handling any uncaught exceptions that may occur during the execution of the MessageListener. Default: `ConditionalRejectingErrorHandler` |
| Exclusive | Determines whether the single consumer in this container has exclusive access to the queues. The concurrency of the container must be 1 when this is `true`. If another consumer has exclusive access, the container tries to recover the consumer, according to the `RecoveryInterval` or `RecoveryBackOff`. |
| ExclusiveConsumerExceptionLogger | An exception logger used when an exclusive consumer cannot gain access to a queue. By default, this is logged at the `WARN` level. |
| FailedDeclarationRetryInterval | The interval between passive queue declaration retry attempts. Passive queue declaration occurs when the consumer starts or, when consuming from multiple queues, when not all queues were available during initialization. Default: 5000 (five seconds). |
| ForceCloseChannel | If the consumers do not respond to a shutdown within `ShutdownTimeout`, if this is `true`, the channel will be closed, causing any un-acked messages to be requeued. Defaults to `true` since 2.0. You can set it to `false` to revert to the previous behavior. |
| MessagesPerAck | The number of messages to receive between acks. Use this to reduce the number of acks sent to the broker (at the cost of increasing the possibility of redelivered messages). Generally, you should set this property only on high-volume listener containers. If this is set and a message is rejected (exception thrown), pending acks are acknowledged and the failed message is rejected. Not allowed with transacted channels. If the `PrefetchCount` is less than the `MessagesPerAck`, it is increased to match the `MessagesPerAck`. Default: ack every message. See also `AckTimeout` in this table. |
| MismatchedQueuesFatal | When the container starts, if this property is `true` (default: `false`), the container checks that all queues declared in the context are compatible with queues already on the broker. If mismatched properties (such as `IsAutoDelete`) or arguments (such as `x-message-ttl`) exist, the container (and application context) fails to start with a fatal exception. If the problem is detected during recovery (for example, after a lost connection), the container is stopped. There must be a single `RabbitAdmin` in the application context (or one specifically configured on the container by using the `RabbitAdmin` property). Otherwise, this property must be `false`. <br\><br\>NOTE: If the broker is not available during initial startup, the container starts and the conditions are checked when the connection is established.<br\><br\> IMPORTANT: The check is done against all queues in the context, not just the queues that a particular listener is configured to use. If you wish to limit the checks to just those queues used by a container, you should configure a separate `RabbitAdmin` for the container, and provide a reference to it using the `RabbitAdmin` property. See [Conditional Declaration](#conditional-declaration) for more information. |
| MissingQueuesFatal | When set to `true` (default), if none of the configured queues are available on the broker, it is considered fatal. This causes the application context to fail to initialize during startup. Also, when the queues are deleted while the container is running, by default, the consumers make three retries to connect to the queues (at five second intervals) and stop the container if these attempts fail. This was not configurable in previous versions. When set to `false`, after making the three retries, the container goes into recovery mode, as with other problems, such as the broker being down. The container tries to recover according to the `RecoveryInterval` property. During each recovery attempt, each consumer again tries four times to passively declare the queues at five second intervals. This process continues indefinitely.  This global property is not applied to any containers that have an explicit `MissingQueuesFatal` property set. The default retry properties (three retries at five-second intervals) can be overridden by setting the properties below. |
| MonitorInterval | With the container a task is scheduled to run at this interval to monitor the state of the consumers and recover any that have failed. |
| NoLocal | Set to `true` to disable delivery from the server to consumers messages published on the same channel's connection. |
| Phase | When `AutoStartup` is `true`, the lifecycle phase within which the container should start and stop. The lower the value, the earlier this container starts and the later it stops. The default is `Int.MaxValue`, meaning the container starts as late as possible and stops as soon as possible. |
| PossibleAuthenticationFailureFatal | When set to `true` (default), if a `PossibleAuthenticationFailureException` is thrown during connection, it is considered fatal. This causes the application context to fail to initialize during startup. When set to `false`, after making the 3 retries, the container goes into recovery mode, as with other problems, such as the broker being down. The container tries to recover according to the `RecoveryInterval` property. During each recovery attempt, each consumer will again try 4 times to start. This process continues indefinitely. The default retry properties (3 retries at 5 second intervals) can be overridden using the properties after this one. |
| PrefetchCount | The number of unacknowledged messages that can be outstanding at each consumer. The higher this value is, the faster the messages can be delivered, but the higher the risk of non-sequential processing. Ignored if the `AcknowledgeMode` is `NONE`. This is increased, if necessary, to match the`MessagePerAck`. Defaults to 250. You can set it to 1 to revert to the previous behavior. <br\><br\>IMPORTANT: There are scenarios where the prefetch value should be low - for example, with large messages, especially if the processing is slow (messages could add up to a large amount of memory in the client process), and if strict message ordering is necessary (the prefetch value should be set back to 1 in this case). Also, with low-volume messaging and multiple consumers (including concurrency within a single listener container instance), you may wish to reduce the prefetch to get a more even distribution of messages across consumers. |
| RabbitAdmin | When a listener container listens to at least one auto-delete queue and it is found to be missing during startup, the container uses a `RabbitAdmin` to declare the queue and any related bindings and exchanges. If such elements are configured to use conditional declaration (see [Conditional Declaration](#conditional-declaration), the container must use the admin that was configured to declare those elements. Specify that admin here. It is required only when using auto-delete queues with conditional declaration. If you do not wish the auto-delete queues to be declared until the container is started, set `AutoStartup` to `false` on the admin. Defaults to a `RabbitAdmin` that declares all non-conditional elements. |
| RecoveryBackOff | Specifies the `BackOff` for intervals between attempts to start a consumer if it fails to start for non-fatal reasons. Default is `FixedBackOff` with unlimited retries every five seconds. Mutually exclusive with `RecoveryInterval`. |
| RecoveryInterval | Determines the time in milliseconds between attempts to start a consumer if it fails to start for non-fatal reasons. Default: 5000. Mutually exclusive with `RecoveryBackOff`. |
| ShutdownTimeout | When a container shuts down (for example, if the service container is disposed), it waits for in-flight messages to be processed up to this limit. Defaults to five seconds. |
| TransactionManager| External transaction manager for the operation of the listener. Also complementary to `IsChannelTransacted` - if the `IModel` is transacted, its transaction is synchronized with the external transaction. |

## Listener Concurrency

With the `DirectMessageListenerContainer`, concurrency is based on the configured queues and `ConsumersPerQueue`.
Each consumer for each queue uses a separate channel, and the concurrency is controlled by the RabbitMQ Client library.

## Exclusive Consumer

You can configure the listener container with a single exclusive consumer.
This prevents other containers from consuming from the queues until the current consumer is cancelled.
The concurrency of such a container must be `1`.

When using exclusive consumers, other containers try to consume from the queues according to the `recoveryInterval` property and log a `WARN` message if the attempt fails.

## Listener Container Queues

The container must be configured to listen on at least one queue. Queues can be added and removed at runtime.
The container recycles (cancels and re-creates) the consumers when any pre-fetched messages have been processed.
See the methods `AddQueues`, `AddQueueNames`, `RemoveQueues` and `RemoveQueueNames` methods.
When removing queues, at least one queue must remain.

A consumer starts if any of its queues are available.
If not all queues are available, the container tries to passively declare (and consume from) the missing queues every 60 seconds.

Also, if a consumer receives a cancel from the broker (for example, if a queue is deleted) the consumer tries to recover, and the recovered consumer continues to process messages from any other configured queues.
A cancel on one queue does not cancel the entire consumer.

If you wish to permanently remove a queue, you should update the container before or after deleting the queue, to avoid future attempts trying to consume from it.

## Resilience & Recovering from Errors

Some of the key (and most popular) high-level features that Steeltoe RabbitMQ provides are to do with recovery and automatic re-connection in the event of a protocol error or broker failure.
We have seen all the relevant components already in this guide, but it should help to bring them all together here and call out the features and recovery scenarios individually.

The primary reconnection features are enabled by the `CachingConnectionFactory` itself.
It is also often beneficial to use the `RabbitAdmin` auto-declaration features.
In addition, if you care about guaranteed delivery, you probably also need to use the `IsChannelTransacted` flag in `RabbitTemplate` and `DirectMessageListenerContainer` and the `AcknowledgeMode.AUTO` (or manual if you do the acks yourself) in the `DirectMessageListenerContainer`.

### Automatic Declaration of Exchanges Queues and Bindings

The `RabbitAdmin` component can declare exchanges, queues, and bindings on startup.
It does this lazily, through a `IConnectionListener`.
Consequently, if the broker is not present on startup, it does not matter.
The first time a `IConnection` is used (for example, by sending a message) the listener fires and the admin features is applied.
A further benefit of doing the auto declarations in a listener is that, if the connection is dropped for any reason (for example, broker death, network glitch, and others), they are applied again when the connection is re-established.

>Queues declared this way must have fixed names - either explicitly declared or generated by the framework for `AnonymousQueue` instances.
Anonymous queues are non-durable, exclusive, and auto-deleting.

>IMPORTANT: Automatic declaration is performed only when the `CachingConnectionFactory` cache mode is `CHANNEL` (the default).
This limitation exists because exclusive and auto-delete queues are bound to the connection.

`RabbitAdmin` detects services of type `IDeclarableCustomizer` and applies the function before actually processing the declaration.
This is useful, for example, to set a new argument before it has first class support within the framework.

```csharp
services.AddSingleton<IDeclarableCustomizer, Customizer>();

private class Customizer : IDeclarableCustomizer
{
    public IDeclarable Apply(IDeclarable declarable)
    {
        if (declarable is IQueue queue && queue.QueueName == "my.queue")
        {
            queue.AddArgument("some.new.queue.argument", true);
        }

        return declarable;
    }
}
```

### Failures in Synchronous Operations and Options for Retry

If you lose your connection to the broker in a synchronous sequence when using `RabbitTemplate` (for instance), Steeltoe RabbitMQ throws an `RabbitException` (usually, but not always, `AmqpIOException`).
We do not try to hide the fact that there was a problem, so you have to be able to catch and respond to the exception.
The easiest thing to do if you suspect that the connection was lost (and it was not your fault) is to try the operation again.
You can do this manually, or you could look at using Steeltoe Retry to handle the retry. Steeltoe Retry a great deal of flexibility to specify the parameters of the retry (number of attempts, exception types, back-off algorithm, and others) when using the `PollyRetryTemplate`.

### Message Listeners and the Asynchronous Case

If a `IMessageListener` fails because of a business exception, the exception is handled by the message listener container, which then goes back to listening for another message.
If the failure is caused by a dropped connection (not a business exception), the consumer that is collecting messages for the listener has to be cancelled and restarted.
The `DirectMessageListenerContainer` handles this seamlessly, and it leaves a log to say that the listener is being restarted.
In fact, it loops endlessly, trying to restart the consumer.
Only if the consumer is very badly behaved indeed will it give up.
One side effect is that if the broker is down when the container starts, it keeps trying until a connection can be established.

Business exception handling, as opposed to protocol errors and dropped connections, might need more thought and some custom configuration, especially if transactions or container acks are in use.
Prior to 2.8.x, RabbitMQ had no definition of dead letter behavior.
Consequently, by default, a message that is rejected or rolled back because of a business exception could be redelivered endlessly.

One approach to deal with this is to set the container's `DefaultRequeueRejected` property to `false`.
This causes all failed messages to be discarded.
When using RabbitMQ 2.8.x or higher, this also facilitates delivering the message to a dead letter exchange.

Alternatively, you can throw a `RabbitRejectAndDontRequeueException`.
Doing so prevents message requeuing, regardless of the setting of the `DefaultRequeueRejected` property.

An `ImmediateRequeueException` is also provided to perform exactly the opposite logic: the message will be requeued, regardless of the setting of the `DefaultRequeueRejected` property.

<!--
TODO: This is not implemented currently , but probably should be in next release

Often, a combination of both techniques is used.
You can use a `StatefulRetryOperationsInterceptor` in the advice chain with a `MessageRecoverer` that throws an `AmqpRejectAndDontRequeueException`.
The `MessageRecover` is called when all retries have been exhausted.
The `RejectAndDontRequeueRecoverer` does exactly that.
The default `MessageRecoverer` consumes the errant message and emits a `WARN` message.

A new `RepublishMessageRecoverer` is provided, to allow publishing of failed messages after retries are exhausted.

When a recoverer consumes the final exception, the message is acked and is not sent to the dead letter exchange, if any.

>When `RepublishMessageRecoverer` is used on the consumer side, the received message has `deliveryMode` in the `receivedDeliveryMode` message property.
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
-->

### Exception Classification for Steeltoe Retry

Steeltoe Retry has a great deal of flexibility for determining which exceptions can invoke retry.
The default configuration retries for all exceptions.
Given that user exceptions are wrapped in a `ListenerExecutionFailedException`, you need to ensure that the classification examines the exception causes.
The default classifier looks only at the top level exception.

The `BinaryExceptionClassifier` has a property called `TraverseInnerExceptions` (default: `true`).
When `true`, it traverses inner exceptions until it finds a match or there is no cause.

To use this classifier for retry, you can use a `PollyRetryPolicy` created with the constructor that takes the max attempts, the `Dictionary` of `Exception` instances, and the boolean (`TraverseInnerExceptions`) and use this with the `RetryTemplate`.
