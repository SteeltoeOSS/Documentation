---
uid: guides/messaging/tutorials/tutorial3/readme
title: RabbitMQ Tutorial 3 - Publish/Subscribe
tags: []
_disableFooter: true
_hideTocVersionToggle: true
---

> [!NOTE]
> This guide applies to Steeltoe v3. [This component has been removed from v4](https://github.com/SteeltoeOSS/Steeltoe/issues/1244).

## Publish/Subscribe Tutorial (using Steeltoe)

> **Prerequisites**
>
> This tutorial assumes RabbitMQ is [downloaded](https://www.rabbitmq.com/download.html) and installed and running
> on `localhost` on the [standard port](https://www.rabbitmq.com/networking.html#ports) (`5672`).
>
> In case you use a different host, port or credentials, connections settings would require adjusting.
>
> **Where to get help**
>
> If you're having trouble going through this tutorial you can contact us through Github issues on our
> [Steeltoe Documentation Repository](https://github.com/SteeltoeOSS/Documentation).

## Introduction

In the [first tutorial](../Tutorial1/Readme.md) we showed how
to use Visual Studio to create a solution with two projects
with the Steeltoe RabbitMQ Messaging dependency and to create simple
applications that send and receive string hello messages.

In the [previous tutorial](../Tutorial2/Readme.md) we created
a sender and receiver and a work queue with two consumers.
We also used Steeltoe attributes to declare the queue. The assumption behind a work queue is that each task is delivered to exactly one worker.

In this tutorial we'll implement a fanout pattern to deliver
messages to multiple consumers. This pattern is also known as "publish/subscribe"
and is implemented by configuring a number of RabbitMQ entities using Steeltoe attributes.

Essentially, published messages are going to be broadcast to all the receivers.

## Exchanges

In previous parts of the tutorial we sent and received messages to and
from a queue. Now it's time to introduce the full messaging model available in
RabbitMQ.

Let's quickly go over what we covered in the previous tutorials:

* A **producer** is a user application that sends messages.
* A **queue** is a buffer that stores messages.
* A **consumer_** is a user application that receives messages.

The core idea in the messaging model in RabbitMQ is that the **producer**
never sends any messages directly to a **queue**. Actually, quite often
the **producer** doesn't even know if a message will be delivered to any
queue at all.

Instead, the **producer**  can only send messages to an **exchange**. An
**exchange** is a very simple thing. On one side it receives messages from
**producers** and the other side it pushes them to**queues**. The **exchange**
must know exactly what to do with a message it receives. Should it be
appended to a particular **queue**? Should it be appended to many **queues**?
Or should it get discarded. The rules for that are defined by the
**exchange type**.

![Exchange](/guides/images/messaging/exchanges.png)

There are a few **exchange types** available: `direct`, `topic`, `headers`
and `fanout`. We'll focus on the last one -- the `fanout`. Let's setup
our Receiver with an **exchange** of this type, and call it `tut.fanout`:

```csharp
using Microsoft.Extensions.Logging;
using Steeltoe.Messaging.RabbitMQ.Attributes;
using Steeltoe.Messaging.RabbitMQ.Config;
using System.Diagnostics;

namespace Receiver
{
    [DeclareExchange(Name = "tut.fanout", Type = ExchangeType.FANOUT)]
    [DeclareAnonymousQueue("queue1")]
    [DeclareAnonymousQueue("queue2")]
    [DeclareQueueBinding(Name = "tut.fanout.binding.queue1", ExchangeName = "tut.fanout", QueueName = "#{@queue1}")]
    [DeclareQueueBinding(Name = "tut.fanout.binding.queue2", ExchangeName = "tut.fanout", QueueName = "#{@queue2}")]
    internal class Tut3Receiver
    {
        private readonly ILogger _logger;

        public Tut3Receiver(ILogger<Tut3Receiver> logger)
        {
            _logger = logger;
        }
        ....
    }
}
```

We follow the same approach as in the previous tutorial and use attributes to declare our RabbitMQ entities.
We declare the `FanoutExchange` using the `DeclareExchange` attribute. We also
define four additional RabbitMQ entities, two `AnonymousQueue`s (non-durable, exclusive, auto-delete queues
in AMQP terms) using the `DeclareAnonymousQueue`attribute and two bindings (`DeclareQueueBinding`) to bind those queues to the exchange.

Notice how we tie together these entities. First, the name of the exchange is `tut.fanout` and the two anonymous queues are named `queue1` and `queue2`.
Next, the bindings reference both the exchange name (e.g. `ExchangeName = "tut.fanout"`) and the queue name (e.g. `QueueName = "#{@queue2}"`).
Notice we are using the same `expression language` we mentioned in the [previous tutorial](../Tutorial2/Readme.md).

The fanout exchange is very simple. As you can probably guess from the
name, it just broadcasts all the messages it receives to all the
queues it knows. And that's exactly what we need for fanning out our
messages.

> **Listing exchanges**
>
> To list the exchanges on the server you can run the ever useful `rabbitmqctl`:
>
> ```bash
> sudo rabbitmqctl list_exchanges
> ```
>
> In this list there will be some `amq.*` exchanges and the default (unnamed)
> exchange. These are created by default, but it is unlikely you'll need to
> use them at the moment.

> **Nameless exchange**
>
> In previous parts of the tutorial we knew nothing about exchanges,
> but still were able to send messages to queues. That was possible
> because we were using a default exchange, which we identify by the empty string (`""`).
>
> Recall how we published a message before:
>
> ```csharp
>    template.ConvertAndSend(QueueName, message)
>```
>
> The first parameter is the routing key and the `RabbitTemplate`
> sends messages by default to the default exchange. Each queue is automatically
> bound to the default exchange with the name of queue as the binding key.
> This is why we can use the name of the queue as the routing key to make
> sure the message ends up in the queue.

Now, we can publish to our named exchange instead:

```csharp
namespace Sender
{
    public class Tut3Sender : BackgroundService
    {
        internal const string FanoutExchangeName = "tut.fanout";
        private readonly RabbitTemplate _rabbitTemplate;

        public Tut3Sender(ILogger<Tut3Sender> logger, RabbitTemplate rabbitTemplate)
        {
            _logger = logger;
            _rabbitTemplate = rabbitTemplate;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // ....
                await _rabbitTemplate.ConvertAndSendAsync(FanoutExchangeName, string.Empty, message);
            }
        }
    }
}
```

From now on the `fanout` exchange will append messages to our queues.

## Temporary Queues

As you may remember previously we were using queues that had
specific names (remember `hello`). Being able to name
a queue was crucial for us -- we needed to point the workers to the
same queue.  Giving a queue a name is important when you
want to share the queue between producers and consumers.

But that's not the case for our fanout example. We want to hear about
all messages, not just a subset of them. We're
also interested only in currently flowing messages, not in the old
ones. To solve that we need two things.

Firstly, whenever we connect to the broker, we need a fresh, empty queue.
To do this, we could create a queue with a random name, or --
even better -- let the server choose a random queue name for us.

Secondly, once we disconnect the consumer, the queue should be
automatically deleted. To do this with Steeltoe Messaging,
we defined an**AnonymousQueue**, using the `DeclareAnonymousQueue` attribute
which creates a non-durable, exclusive, auto-delete queue with a generated name.

```csharp
using Microsoft.Extensions.Logging;
using Steeltoe.Messaging.RabbitMQ.Attributes;
using Steeltoe.Messaging.RabbitMQ.Config;
using System.Diagnostics;

namespace Receiver
{
    [DeclareExchange(Name = "tut.fanout", Type = ExchangeType.FANOUT)]
    [DeclareAnonymousQueue("queue1")]
    [DeclareAnonymousQueue("queue2")]
    [DeclareQueueBinding(Name = "tut.fanout.binding.queue1", ExchangeName = "tut.fanout", QueueName = "#{@queue1}")]
    [DeclareQueueBinding(Name = "tut.fanout.binding.queue2", ExchangeName = "tut.fanout", QueueName = "#{@queue2}")]
    internal class Tut3Receiver
    {
        private readonly ILogger _logger;

        public Tut3Receiver(ILogger<Tut3Receiver> logger)
        {
            _logger = logger;
        }
        ....
    }
}
```

At this point, our queues have random queue names. For example,
it may look something like `spring.gen-1Rx9HOqvTAaHeeZrQWu8Pg`.

## Bindings

![Bindings](/guides/images/messaging/bindings.png)

We've already created a fanout exchange and a queue. Now we need to
tell the exchange to send messages to our queue. That relationship
between exchange and a queue is called a **binding**.
Below you can see that we have two bindings declared using the `DeclareQueueBinding` attribute , one for each `AnonymousQueue`.

```csharp
using Microsoft.Extensions.Logging;
using Steeltoe.Messaging.RabbitMQ.Attributes;
using Steeltoe.Messaging.RabbitMQ.Config;
using System.Diagnostics;

namespace Receiver
{
    [DeclareExchange(Name = "tut.fanout", Type = ExchangeType.FANOUT)]
    [DeclareAnonymousQueue("queue1")]
    [DeclareAnonymousQueue("queue2")]
    [DeclareQueueBinding(Name = "tut.fanout.binding.queue1", ExchangeName = "tut.fanout", QueueName = "#{@queue1}")]
    [DeclareQueueBinding(Name = "tut.fanout.binding.queue2", ExchangeName = "tut.fanout", QueueName = "#{@queue2}")]
    internal class Tut3Receiver
    {
        private readonly ILogger _logger;

        public Tut3Receiver(ILogger<Tut3Receiver> logger)
        {
            _logger = logger;
        }
        ....
    }
}
```

> **Listing bindings**
>
> You can list existing bindings using, you guessed it,
>
> ```shell
> rabbitmqctl list_bindings
> ```

## Putting it all together

![overall](/guides/images/messaging/python-three-overall.png)

The producer program, which emits messages, doesn't look much
different from the previous tutorial. The most important change is that
we now want to publish messages to our `fanout` exchange instead of the
nameless one. We need to supply a `routingKey` when sending, but its
value is ignored for `fanout` exchanges.

Here goes the code for `Tut3Sender.cs` program:

```csharp
using Steeltoe.Messaging.RabbitMQ.Core;
using System.Text;

namespace Sender
{
    public class Tut3Sender : BackgroundService
    {
        private readonly ILogger<Tut3Sender> _logger;
        private int dots = 0;
        private int count = 0;

        internal const string FanoutExchangeName = "tut.fanout";
        private readonly RabbitTemplate _rabbitTemplate;

        public Tut3Sender(ILogger<Tut3Sender> logger, RabbitTemplate rabbitTemplate)
        {
            _logger = logger;
            _rabbitTemplate = rabbitTemplate;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                var message = CreateMessage();
                await _rabbitTemplate.ConvertAndSendAsync(FanoutExchangeName, string.Empty, message);
                _logger.LogInformation($"Sent '" + message + "'");
                await Task.Delay(1000, stoppingToken);
            }
        }

        private string CreateMessage()
        {
            StringBuilder builder = new StringBuilder("Hello");
            if (++dots == 4)
            {
                dots = 1;
            }
            for (int i = 0; i < dots; i++)
            {
                builder.Append('.');
            }
            builder.Append(++count);
            return builder.ToString();
        }
    }
}
```

As you see, we leverage dependency injection and add the `RabbitTemplate` to the constructors signature.
Note that messages will be lost if no queue is bound to the exchange yet,
but that's okay for us; if no consumer is listening yet we can safely discard the message.

Next the code for `Tut3Receiver.cs`:

```csharp
using Microsoft.Extensions.Logging;
using Steeltoe.Messaging.RabbitMQ.Attributes;
using Steeltoe.Messaging.RabbitMQ.Config;
using System.Diagnostics;

namespace Receiver
{
    [DeclareExchange(Name = "tut.fanout", Type = ExchangeType.FANOUT)]
    [DeclareAnonymousQueue("queue1")]
    [DeclareAnonymousQueue("queue2")]
    [DeclareQueueBinding(Name = "tut.fanout.binding.queue1", ExchangeName = "tut.fanout", QueueName = "#{@queue1}")]
    [DeclareQueueBinding(Name = "tut.fanout.binding.queue2", ExchangeName = "tut.fanout", QueueName = "#{@queue2}")]
    internal class Tut3Receiver
    {
        private readonly ILogger _logger;

        public Tut3Receiver(ILogger<Tut3Receiver> logger)
        {
            _logger = logger;
        }


        [RabbitListener(Queue = "#{@queue1}")]
        public void Receive1(string input)
        {
            Receive(input, 1);
        }

        [RabbitListener(Queue = "#{@queue2}")]
        public void Receive2(string input)
        {
            Receive(input, 2);
        }

        private void Receive(string input, int receiver)
        {
            var watch = new Stopwatch();
            watch.Start();

            DoWork(input);

            watch.Stop();

            var time = watch.Elapsed;
            _logger.LogInformation($"Received: {input} from queue: {receiver}, took: {time}");
        }

        private void DoWork(string input)
        {
            foreach (var ch in input)
            {
                if (ch == '.')
                    Thread.Sleep(1000);
            }
        }
    }
}
```

Compile as before and we're ready to execute the fanout sender and receiver.

```shell
cd tutorials\tutorial3
dotnet build
```

And of course, to execute the tutorial do the following:

To run the receiver, execute the following commands:

```shell
cd receiver
dotnet run
```

Open another shell to run the sender:

```shell
cd sender
dotnet run
```

Using `rabbitmqctl list_bindings` you can verify that the code actually
creates bindings and queues as we want.

To find out how to listen for a subset of messages, let's move on to
[tutorial 4](../Tutorial4/Readme.md)
