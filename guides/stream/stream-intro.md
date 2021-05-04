### Quick Start

This guide will show you how to create a Steeltoe Stream service that receives messages coming from messaging middleware of your choice and logs the received message to the console.

We call it `LoggingConsumer`. While not very practical, it provides a good introduction to some of the main concepts
and abstractions, making it easier to digest the rest of this user guide.

First, **start a rabbitmq server** locally using the [Steeltoe dockerfile](https://github.com/steeltoeoss/dockerfiles). 

1. [Creating a Sample Application by Using Steeltoe Initializr](https://start.steeltoe.io)
1. Name the project `LoggingConsumer`
1. Add the "Steeltoe.Stream.Binder.RabbitMQ" dependency
1. Add the "Steeltoe.Stream.StreamBase" dependency
1. [Adding a Message Handler, Building, and Running](#steeltoe-stream-preface-adding-message-handler)

#### <a name="steeltoe-stream-preface-creating-sample-application"></a>Creating a Sample Application by Using Spring Initializr
To get started, visit the [Steeltoe Initializr](https://start.steeltoe.io). From there, you can generate our `LoggingConsumer` application. To do so:

<!-- TODO: initializr template for Stream 
1. In the *Dependencies* section, start typing `stream`.
    When the "`Cloud Stream`" option should appears, select it.
1. Start typing 'rabbit'.
1. Select "`RabbitMQ`". -->
1. In the *Name* field, type 'LoggingConsumer'.
  
    <!-- 
    TODO:  If you chose RabbitMQ for the middleware, your Spring Initializr should now be as follows:
    ![Spring Initializr](./images/spring-initializr.png) 
    
    -->

1. Click the *Generate Project* button.
    Doing so downloads the zipped version of the generated project to your hard drive.
1. Unzip the file into the folder you want to use as your project directory.

TIP: We encourage you to explore the many possibilities available in the Steeltoe Initializr as it lets you create many different kinds of .NET applications.

#### <a name="steeltoe-stream-preface-importing-project"></a>Importing the Project into Your IDE

Now you can open the project in your IDE.

Once imported, the project must have no errors of any kind. 

Technically, at this point, you can run the application's Program class.
However, it does not do anything, so we want to add some code.

#### <a name="steeltoe-stream-preface-adding-message-handler"></a>Adding a Message Handler, Building, and Running

Modify the `LoggingConsumer.Program` class to look as follows:

```csharp
...

using Steeltoe.Stream.Attributes;
using Steeltoe.Stream.Messaging;
using Steeltoe.Stream.StreamsHost;
...

    [EnableBinding(typeof(ISink))]
    public class LoggingConsumerApplication
    {
        static async Task Main(string[] args)
        {

            await StreamsHost.CreateDefaultBuilder<LoggingConsumerApplication>(args)
              .ConfigureServices((context, services) =>
              {
                  services.AddLogging(builder =>
                  {
                      builder.AddDebug();
                      builder.AddConsole();
                  });
              }).StartAsync();
        }

        [StreamListener(ISink.INPUT)]
        public void Handle(Person person)
        {
            Console.WriteLine("Received: " + person);
        }


    }
    public class Person
    {
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
```

As you can see from the preceding listing:

* We have enabled `Sink` binding (input-no-output) by using `[EnableBinding(typeof(ISink))]`.
Doing so signals to the framework to initiate binding to the messaging middleware, where it automatically creates the destination (that is, queue, topic, and others) that are bound to the `Sink.INPUT` channel.
* We have added a `Handle` method to receive incoming messages of type `Person`.
Doing so lets you see one of the core features of the framework: It tries to automatically convert incoming message payloads to type `Person`.

You now have a fully functional Steeltoe Streams application that listens for messages.
Assuming you have RabbitMQ installed and running, you can start the application in your IDE.

You should see following output:

```
info: Steeltoe.Messaging.RabbitMQ.Connection.CachingConnectionFactory[0]
      Attempting to connect to: amqp://127.0.0.1:5672
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Steeltoe.Messaging.RabbitMQ.Connection.CachingConnectionFactory[0]
      Created new connection: ccFactory:1.publisher/Steeltoe.Messaging.RabbitMQ.Connection.SimpleConnection
info: Steeltoe.Stream.Binder.Rabbit.RabbitMessageChannelBinder[0]
      Channel 'LoggingConsumer.Application.errors' has 1 subscriber(s).
info: Steeltoe.Stream.Binder.Rabbit.RabbitMessageChannelBinder[0]
      Channel 'LoggingConsumer.Application.errors' has 2 subscriber(s).
info: Microsoft.Hosting.Lifetime[0]
```

Go to the RabbitMQ management console or any other RabbitMQ client and send a message to `input.anonymous.CbMIwdkJSBO1ZoPDOtHtCg`.
The `anonymous.CbMIwdkJSBO1ZoPDOtHtCg` part represents the group name and is generated, so it is bound to be different in your environment.
For something more predictable, you can use an explicit group name by setting `spring:cloud:stream:bindings:input:group=hello` (or whatever name you like).

The contents of the message should be a JSON representation of the `Person` class, as follows:

	{"name":"Sam Spade"}

Then, in your console, you should see:

`Received: Sam Spade`

Now you have a working (albeit very basic) Streams based service.
