# Stream Processing with RabbitMQ

In this guide, we develop three Steeltoe applications that use Steeltoe Stream's support for RabbitMQ and deploy them to Cloud Foundry and to Kubernetes.
In another guide, we [deploy these applications using Data Flow](./data-flow-stream.md).
Taking the time to deploy the applications manually will provide you with a better understanding of the steps that Data Flow automates for you.

The following sections describe how to build these applications from scratch.
If you prefer, you can clone the Steeltoe [sample applications](https://github.com/SteeltoeOSS/Samples/blob/main/Stream/UsageCost), and proceed to the [deployment](#deployment) section.

## Development

This guide will walk through creating three Steeltoe Stream applications that communicate by using RabbitMQ, using the scenario of a cell phone company creating bills for its customers.
Each call made by a user has a `Duration` and an amount of `Data` used during the call.
As part of the process to generate a bill, the raw call data needs to be converted to a cost for the duration of the call and a cost for the amount of data used.

The call is modeled using the `UsageDetail` class that contains the `Duration` of the call and the amount of `Data` used during the call.
The bill is modeled using the `UsageCostDetail` class that contains the cost of the call (`CostCall`) and the cost of the data (`CostData`). Each class contains an ID (`UserId`) to identify the person making the call.

The three streaming applications are as follows:

- `UsageDetailSender`: a `Source` application that generates the users' call `Duration` and the amount of `Data` used for each `UserId`. Sends messages containing `UsageDetail` objects as JSON.

- `UsageCostProcessor`: a `Processor` application that consumes `UsageDetail` and computes the cost of the call and the data per `UserId`. Sends messages containing `UsageCostDetail` objects as JSON.

- `UsageCostLogger`: an `Sink` application that consumes `UsageCostDetail` objects and logs the cost of the call and data.

### Source

In this step, we create the `UsageDetailSender` Source project. Create a new .NET Console project and add the NuGet packages as described below.

### Add NuGet Reference

In order to make each of the three projects Steeltoe Stream applications,  you need to add references to the following packages:

| Package | Description | .NET Target |
| --- | --- | --- |
| `Steeltoe.Stream.StreamBase` | Provides StreamHost extensions, base functionality and dependency injection support | ASP.NET Core 3.1+ |
| `Steeltoe.Stream.Binder.RabbitMQ` | Binder that connects Steeltoe abstractions with RabbitMQ  | ASP.NET Core 3.1+ |

To add this type of NuGet to your project, add a `PackageReference` resembling the following:

```xml
<ItemGroup>
    <PackageReference Include="Steeltoe.Stream.StreamBase" Version="3.1.0"/>
</ItemGroup>
```

<!-- TODO: Initializr Instructions-->

#### Business Logic

Now we can create the code required for this application. To do so:

1. Create a `UsageDetail` class in the project.

```csharp
  public class UsageDetail
  {
      public string UserId { get; set; }

      public long Duration { get; set; }

      public long Data { get; set; }
  }
```

1. Create the `UsageGenerator` class in the project, which resembles the following listing:

```csharp

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Steeltoe.Messaging;
using Steeltoe.Messaging.Handler.Attributes;
using Steeltoe.Messaging.Support;
using Steeltoe.Stream.Attributes;
using Steeltoe.Stream.Messaging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace UsageSender
{
    [EnableBinding(typeof(ISource))]
    public class UsageGenerator : BackgroundService
    {
        private readonly ISource _source;
        private readonly ILogger<UsageGenerator> _logger;
        private static readonly Random RANDOM = new Random();
   
        public string ServiceName { get; set; } = "UsageGenerator";
        private string[] users = { "user1", "user2", "user3", "user4", "user5" };

        public UsageGenerator(ISource source, ILogger<UsageGenerator> logger)
        {
            _source = source;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var message = GenerateAndSend();
                _source.Output.Send(message);
            
                await Task.Delay(5000, stoppingToken); // Wait 5 seconds before sending again
            }
        }

        [SendTo(ISource.OUTPUT)]
        protected virtual IMessage GenerateAndSend()
        {
            var value = new UsageDetail
            {
                UserId = users[RANDOM.Next(5)],
                Duration = RANDOM.Next(300),
                Data = RANDOM.Next(700)
            };

            _logger.LogInformation("Sending: " + value);
            return MessageBuilder.WithPayload(value).Build();
        }
    }
}
```

The `[EnableBinding]` attribute indicates that you want to bind your application to messaging middleware.
The attribute takes one or more interfaces as a parameter, in this case, the [ISource](https://github.com/SteeltoeOSS/Steeltoe/blob/main/src/Stream/src/Abstractions/Messaging/ISource.cs) interface that defines an output channel named `output`.
In the case of RabbitMQ, messages sent to the `output` channel are in turn sent to the RabbitMQ message broker by using a `TopicExchange`.

Deriving from [BackgroundService](https://docs.microsoft.com/aspnet/core/fundamentals/host/hosted-services#backgroundservice-base-class) and calling `Task.Delay` makes the UsageGenerator a background task that gets called by the framework every `5` seconds.

In every iteration of the loop, The `GenerateAndSend` method constructs a `UsageDetail` object which is sent to the output channel by accessing the `_source` object's `Output.Send()` method.

#### <a name="configuration-usage-detail-sender"></a>Configuration

When configuring the `ISource` application, we need to set:

- The `output` binding destination (RabbitMQ exchange) where the producer publishes the data.
- The `requiredGroups` to specify the consumer groups to ensure the message delivery to consumer applications.

In `appSettings.json`, you can add the following properties:

```json
{ 
    "Spring": {
      "Cloud": {
        "Stream": {
          "Bindings": {
            "Output": {
              "Producer": { "RequiredGroups": "usage-cost-customer"},
              "Destination": "usage-detail"
            }
          }
        }
      }
    }
}
```

- The `Spring:Cloud:Stream:Bindings:Output:Destination` property binds the `UsageDetailSender` object's output to the `usage-detail` RabbitMQ exchange.
- The `Spring:Cloud:Stream:Bindings:Output:Producer:RequiredGroups` property makes sure to create a durable queue named `usage-detail.usage-cost-consumer`, which consumes from the `usage-detail` RabbitMQ exchange.

##### Durable Queues

By default, the Steeltoe Stream consumer application creates an `anonymous` auto-delete queue.
This can result in a message not being stored and forwarded by the producer if the producer application started before the consumer application.
Even though the exchange is durable, we need a `durable` queue to be bound to the exchange for the message to be stored for later consumption.
Hence, for guaranteed message delivery, you need a `durable` queue.

To pre-create durable queues and bind them to the exchange, the producer application should set the following property:

```json
Spring:Cloud:Stream:Bindings:<ChannelName>:Producer:RequiredGroups
```

The `RequiredGroups` property accepts a comma-separated list of groups to which the producer must ensure message delivery.
When this property is set, a durable queue is created by using the `<Exchange>.<RequiredGroup>` format.

### Processor

In this step, we create the `UsageProcessor` processor. Create a new .NET Console  project and add the NuGet packages as referred in the [Add NuGet reference](#add-nuget-reference)

#### Business Logic

Now we can create the code required for this application. To do so:

1. Create a `UsageDetail` class in the project that looks like the listing below. Note that  Steeltoe supports sharing your models via NuGet package, shared code or independently maintained models.

```csharp
  public class UsageDetail
  {
      public string UserId { get; set; }

      public long Duration { get; set; }

      public long Data { get; set; }
  }
```

1. Create the `UsageCostDetail` class.

```csharp
  public class UsageCostDetail
  {
      public string UserId { get; set; }

      public double CallCost { get; set; }

      public double DataCost { get; set; }
  }
```

1. Create the `UsageCostProcessor` class which receives the `UsageDetail` message, computes the call and data cost, and sends a `UsageCostDetail` message as shown:

```csharp
using Microsoft.Extensions.Logging;
using Steeltoe.Messaging.Handler.Attributes;
using Steeltoe.Stream.Attributes;
using Steeltoe.Stream.Messaging;
using System;

namespace UsageProcessor
{
    [EnableBinding(typeof(IProcessor))]
    public class UsageProcessor
    {
        private static ILogger<UsageProcessor> _logger;

        private double _ratePerSecond = 0.1;

        private double _ratePerMB = 0.05;

        public UsageProcessor(ILogger<UsageProcessor> logger)
        {
            _logger = logger ?? NullLogger<UsageProcessor>.Instance;
        }

        [StreamListener(IProcessor.INPUT)]
        [SendTo(IProcessor.OUTPUT)]
        public UsageCostDetail Handle(UsageDetail usageDetail)
        {
            return new UsageCostDetail
            {
                UserId = usageDetail.UserId,
                CallCost = usageDetail.Duration * _ratePerSecond,
                DataCost = usageDetail.Data * _ratePerMB
            };
        }
    }
}

```

In the preceding application, the `[EnableBinding]` attribute indicates that you want to bind your application to the messaging middleware. The attribute takes one or more interfaces as a parameter, in this case, the [IProcessor](https://github.com/SteeltoeOSS/Steeltoe/blob/main/src/Stream/src/Abstractions/Messaging/IProcessor.cs) that defines and input and output channel.

The `[StreamListener]` attribute binds the application's `input` channel to the `Handle` method and automatically deserializes the incoming JSON into `UsageDetail` object.

The `[SendTo]` attribute sends the `Handle` method's output to the application's `output` channel, which is in turn, sent to the a RabbitMQ message broker by using a `TopicExchange`.

#### <a name="configuration-usage-cost-processor"></a>Configuration

When configuring the `processor` application, we need to set the following properties:

- The `input` binding destination (RabbitMQ exchange) where this application is subscribed through an `anonymous` auto-delete or `durable` queue.
- The `group` to specify the consumer group to which this consumer application belongs.
- The `output` binding destination (RabbitMQ exchange) where the producer publishes the data.
- The `requiredGroups` to specify the consumer groups to ensure the message delivery guarantee.

In `appsettings.json`, you can add the following properties:

```json
{
    "Spring": {
        "Cloud": {
            "Stream": {
                "Bindings": {
                    "Input": {
                        "Destination": "usage-detail",
                        "Group": "usage-cost-customer"
                    },
                    "Output": {
                        "Producer": {
                        "RequiredGroups": "logger"
                        },
                        "Destination": "usage-cost"
                    }
                }
            }
        }
    }
}
```

- The `Spring:Cloud:Stream:Bindings:Input:Destination` and `Spring:Cloud:Stream:Bindings:Input:Group` properties bind the `UsageCostProcessor` object's `input` to the `usage-detail` RabbitMQ exchange through the `usage-detail.usage-cost-consumer` durable queue.
- The `Spring:Cloud:Stream:Bindings:Output:destination` property binds the `UsageCostProcessor` object's output to the `usage-cost` RabbitMQ exchange.
- The `spring"cloud:stream:bindings:output:producer:requiredGroups` property tells Steeltoe to make sure a durable queue named `usage-cost.logger` exists for consumption of the `usage-cost` RabbitMQ exchange.

There are many configuration options that you can choose to extend or override to achieve the desired runtime behavior when using RabbitMQ as the message broker. The RabbitMQ-specific binder configuration properties are listed in the [RabbitMQ-binder documentation](./rabbit-binder.md)

### Sink

In this step, we create the `UsageCostLogger` sink.

Create a new .NET Console  project and add the NuGet packages as referred in the [Add NuGet reference](#add-nuget-reference)

#### Business Logic

To create the business logic:

1.  Create a `UsageCostDetail` class that looks like [UsageCostDetail.cs](https://github.com/SteeltoeOSS/Samples/blob/main/Stream/UsageCost/UsageLogger/UsageCostDetail.cs).
    The `UsageCostDetail` class contains `UserId`, `CallCost`, and `DataCost` properties.
1.  Create the `UsageCostLogger` class, which receives the `UsageCostDetail` message and logs it. The following listing shows the source code:

```csharp
using Microsoft.Extensions.Logging;
using Steeltoe.Stream.Attributes;
using Steeltoe.Stream.Messaging;
using System;

namespace UsageLogger
{
    [EnableBinding(typeof(ISink))]
    public class UsageLogger
    {
        private static ILogger<UsageLogger> _logger;

        public UsageLogger(ILogger<UsageLogger> logger)
        {
            _logger = logger ?? NullLogger<UsageLogger>.Instance;
        }

        [StreamListener(IProcessor.INPUT)]
        public void Handle(UsageCostDetail costDetail) =>
            _logger.LogInformation("Received UsageCostDetail " + costDetail);

    }
}
```

In the preceding application, the `[EnableBinding]` attribute indicates that you want to bind your application to the messaging middleware. The attribute takes one or more interfaces as a parameter, in this case, the [ISink](https://github.com/SteeltoeOSS/Steeltoe/blob/main/src/Stream/src/Abstractions/Messaging/ISink.cs) interface that defines an input channel.

The `[StreamListener]` attribute binds the application's `input` channel to the `process` method by converting the incoming JSON to a `UsageCostDetail` object.

#### <a name="configuration-usage-cost-logger"></a>Configuration

When configuring the `sink` application, we need to set:

- The `input` binding destination (RabbitMQ exchange) to which this application is subscribed through an `anonymous` auto-delete or `durable` queue.
- The `group` to specify the consumer group to which this consumer application belongs.

In `appsettings.json`, you can add the following properties:

```json
{ 
  "Spring": {
    "Cloud": {
      "Stream": {
        "Bindings": {
          "Input": {
            "Group": "logger",
            "Destination": "usage-cost"
          }
        }
      }
    }
  }
}
```

The `Spring:Cloud:Stream:Bindings:Input:Destination` and `Spring:Cloud:Stream:Bindings:Input:Group` properties bind the `UsageCostLogger` object's `input` to the `usage-cost` RabbitMQ exchange through the `usage-cost.logger` durable queue.

## Deployment

In this section, we deploy the applications created earlier to the local machine, Cloud Foundry, and Kubernetes.

When you deploy these three applications (`UsageDetailSender`, `UsageCostProcessor`, and `UsageCostLogger`), the flow of message is as follows:

```
UsageDetailSender -> UsageCostProcessor -> UsageCostLogger
```

The `UsageDetailSender` source application's output is connected to the `UsageCostProcessor` processor application's input.
The `UsageCostProcessor` application's output is connected to the `UsageCostLogger` sink application's input.

When these applications run, the `RabbitMQ` binder binds the applications' output and input boundaries into the corresponding exchanges and queues at RabbitMQ message broker.

### Local

You can run the applications as standalone applications on your `local` environment.

To install and run the `RabbitMQ` docker image, run the following command:

```bash
docker run -d --hostname rabbitmq --name rabbitmq -p 15672:15672 -p 5672:5672 rabbitmq:3.7.14-management
```

Once installed, you can log in to the RabbitMQ management console on your local machine on [http://localhost:15672](http://localhost:15672).
You can use the default account username and password: `guest` and `guest`.

#### Running the `UsageDetailSender` Source

By using the [pre-defined](#configuration-usage-detail-sender) configuration properties (along with a unique server port) for `UsageSender`, you can run the application, as follows:

```
cd UsageSender
dotnet build && dotnet run --framework net5.0
```

When this application is running, you can see that the `usage-detail` RabbitMQ exchange is created and a queue named `usage-detail.usage-cost-consumer` is bound to this exchange, as the following example shows:

<img src="~/api/v3/stream/images/standalone-rabbitmq-usage-detail-sender.png" alt="Standalone Usage Detail" width="100%">

Also, if you click on the `Queues` and check the queue `usage-detail.usage-cost-consumer`, you can see the messages being consumed and stored in this durable queue, as the following example shows:

<img src="~/api/v3/stream/images/standalone-rabbitmq-usage-detail-sender-message-guarantee.png" alt="Standalone Usage Detail Sender RabbitMQ Message Guarantee" width="100%">

When configuring the consumer applications for this `Source` application, you can set the `group` binding property to connect to the corresponding queue.

>NOTE: If you do not set the `requiredGroups` property, you can see that there is no `queue` for consuming the messages from the `usage-detail` exchange and, therefore, the messages are lost if the consumer is not up before this application is started. 

#### Running the Processor

By using the [pre-defined](#configuration-usage-cost-processor) configuration properties (along with a unique server port) for `UsageProcessor`, you can run the application, as follows:

```
cd UsageProcessor
dotnet build && dotnet run --framework net5.0
```

From the RabbitMQ console, you can see:

- The `UsageProcessor` application consumes from the `usage-detail.usage-cost-consumer` durable queue, based on the `Spring:Cloud:Stream:Bindings:Input:group=usage-cost-consumer` setting.
- The `UsageProcessor` application produces the `UsageCostDetail` and sends it to the exchange `usage-cost`, based on the `Spring:Cloud:Stream:Bindings:Output:Destination=usage-cost` setting.
- The `usage-cost.logger` durable queue is created. It consumes the messages from the `usage-cost` exchange, based on the `Spring:Cloud:Stream:Bindings:Output:Producer:RequiredGroups=logger` property.

When this application is running, you can see that the `usage-cost` RabbitMQ exchange is created and the queue named `usage-cost.logger` is bound to this exchange, as the following image shows:

<img src="~/api/v3/stream/images/standalone-rabbitmq-usage-cost-processor.png" alt="Standalone Usage Cost Processor RabbitMQ Required Groups"  width="100%">

Also, if you click on the `Queues` and check the `usage-cost.logger` queue, you can see the messages being consumed and stored in this queue, as the following image shows:

<img src="~/api/v3/stream/images/standalone-rabbitmq-usage-cost-processor-message-guarantee.png" alt="Standalone Usage Cost Processor RabbitMQ Message Guarantee" width="100%">

#### Running the Sink

By using the [pre-defined](#configuration-usage-cost-logger) configuration properties (along with a unique server port) for `UsageLogger`, you can run the application, as follows:

```bash
cd UsageLogger
dotnet build && dotnet run --framework net5.0
```

Now you can see that this application logs the usage cost detail it receives from the `usage-cost` RabbitMQ exchange through the `usage-cost.logger` durable queue, as the following example shows:

```cmd
info: UsageLogger.UsageLogger[0]
      Received UsageCostDetail { "UserId" "user3", "CallCost": "$12.90", "DataCost": "$28.95" }
info: UsageLogger.UsageLogger[0]
      Received UsageCostDetail { "UserId" "user4", "CallCost": "$17.40", "DataCost": "$14.45" }
info: UsageLogger.UsageLogger[0]
      Received UsageCostDetail { "UserId" "user4", "CallCost": "$7.50", "DataCost": "$9.45" }
info: UsageLogger.UsageLogger[0]
      Received UsageCostDetail { "UserId" "user5", "CallCost": "$17.40", "DataCost": "$30.80" }
info: UsageLogger.UsageLogger[0]
      Received UsageCostDetail { "UserId" "user1", "CallCost": "$28.80", "DataCost": "$6.75" }
info: UsageLogger.UsageLogger[0]
      Received UsageCostDetail { "UserId" "user5", "CallCost": "$8.50", "DataCost": "$10.00" }
info: UsageLogger.UsageLogger[0]
      Received UsageCostDetail { "UserId" "user2", "CallCost": "$17.30", "DataCost": "$19.65" }
info: UsageLogger.UsageLogger[0]
      Received UsageCostDetail { "UserId" "user3", "CallCost": "$5.90", "DataCost": "$13.25" }
```

### Cloud Foundry

This section walks you through how to deploy the `UsageDetailSender`, `UsageCostProcessor`, and `UsageCostLogger` applications on your Cloud Foundry environment.

#### Creating a RabbitMQ service

To create a RabbitMQ service:

1. Log in to the CloudFoundry environment with your credentials.
1. From the CF market place, create a RabbitMQ service instance.

```bash
cf create-service p-rabbitmq standard stream-rabbitmq
```

#### Cloud Foundry Deployment

To deploy on Cloud Foundry:

1. Create a CF manifest YAML for the `UsageDetailSender` as follows:

```yaml
applications:
- name: usage-sender
  buildpacks:
   - dotnet_core_buildpack
  memory: 128M
  disk_quota: 512M
  command: cd ${HOME} && ./UsageSender
  env:
    ASPNETCORE_ENVIRONMENT: Development
  services:
   - stream-rabbitmq
```

Push the `UsageDetailSender` application by using its manifest YAML file, as follows:

```bash
cd UsageSender
dotnet publish -f net5.0 -r linux-x64 -o publish
cf push -f manifest.yml 
```

Create a CF manifest YAML file for the `UsageProcessor` as follows:

```yaml
applications:
- name: usage-processor
  buildpacks:
   - dotnet_core_buildpack
  memory: 128M
  disk_quota: 512M
  command: cd ${HOME} && ./UsageProcessor
  env:
    ASPNETCORE_ENVIRONMENT: Development
  services:
   - stream-rabbitmq
```

Push the `UsageProcessor` application by using its manifest YAML file, as follows:

```bash
cd UsageProcessor
dotnet publish -f net5.0 -r linux-x64 -o publish
cf push -f manifest.yml
```

Create a CF manifest YAML file for the `UsageLogger` as follows:

```yaml
applications:
- name: usage-logger
  buildpacks:
   - dotnet_core_buildpack
  memory: 128M
  disk_quota: 512M
  command: cd ${HOME} && ./UsageLogger
  env:
    ASPNETCORE_ENVIRONMENT: Development
  services:
   - stream-rabbitmq
```

Push the `UsageLogger` application by using its manifest YAML file, as follows:

```bash
cd UsageLogger
dotnet publish -f net5.0 -r linux-x64 -o publish
cf push -f manifest.yml 
```

You can see the applications running by using the `cf apps` command, as follows:

```bash
cf apps
```

The following listings shows typical output:

```bash

name              requested state   processes   routes
usage-logger      started           web:1/1     usage-logger.apps.pcfone.io
usage-processor   started           web:1/1     usage-processor.apps.pcfone.io
usage-sender      started           web:1/1     usage-sender.apps.pcfone.io
```

```cmd
   2021-06-08T15:39:57.62-0400 [APP/PROC/WEB/0] OUT info: UsageLogger.UsageLogger[0]
   2021-06-08T15:39:57.62-0400 [APP/PROC/WEB/0] OUT       Received UsageCostDetail { "UserId" "user4", "CallCost": "$21.30", "DataCost": "$31.40" }
   2021-06-08T15:40:02.63-0400 [APP/PROC/WEB/0] OUT info: UsageLogger.UsageLogger[0]
   2021-06-08T15:40:02.63-0400 [APP/PROC/WEB/0] OUT       Received UsageCostDetail { "UserId" "user3", "CallCost": "$1.30", "DataCost": "$27.20" }
   2021-06-08T15:40:07.63-0400 [APP/PROC/WEB/0] OUT info: UsageLogger.UsageLogger[0]
   2021-06-08T15:40:07.63-0400 [APP/PROC/WEB/0] OUT       Received UsageCostDetail { "UserId" "user2", "CallCost": "$5.60", "DataCost": "$29.30" }
   2021-06-08T15:40:12.62-0400 [APP/PROC/WEB/0] OUT info: UsageLogger.UsageLogger[0]
   2021-06-08T15:40:12.62-0400 [APP/PROC/WEB/0] OUT       Received UsageCostDetail { "UserId" "user4", "CallCost": "$0.40", "DataCost": "$26.15" }
```

### Running on Kubernetes

This section walks you through how to deploy the three Stream Stream applications on Kubernetes.

#### Setting up the Kubernetes cluster

For this example, we need a running Kubernetes cluster. For this example, we deploy to [Docker for Windows Desktop](https://docs.docker.com/docker-for-windows/install/) with integrated [Kubernetes](https://docs.docker.com/desktop/kubernetes/).

##### Verifying Kubernetes is running

To verify that you have a running Kubernetes instance, run the following command (show with sample output):

```bash
kubectl config get-contexts

CURRENT   NAME             CLUSTER          AUTHINFO         NAMESPACE
*         docker-desktop   docker-desktop   docker-desktop

kubectl config use-context docker-desktop
Switched to context "docker-desktop".
```

#### Installing RabbitMQ

You can install the RabbitMQ message broker by using the default configuration from Spring Cloud Data Flow.
To do so, run the following command:

```bash
kubectl apply -f https://raw.githubusercontent.com/spring-cloud/spring-cloud-dataflow/main/src/kubernetes/rabbitmq/rabbitmq-deployment.yaml -f https://raw.githubusercontent.com/spring-cloud/spring-cloud-dataflow/main/src/kubernetes/rabbitmq/rabbitmq-svc.yaml
```

#### Building the Docker Images

To build the Docker images, we use a Dockerfile for each of the three applications. For example the UsageSender Dockerfile looks like this:

```Dockerfile

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /source

COPY *.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish -f net5.0 -c release -o /app 

FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app
COPY --from=build /app .

ENV SPRING_RABBITMQ_HOST=host.docker.internal
ENV PORT=8080

ENTRYPOINT ["dotnet", "UsageSender.dll"]

```

Then use the `docker build` command to build, tag and publish to your repository:

```bash
docker build . -t <your-repo>/usagelogger
```

#### Deploying the Stream

To deploy the stream, you must first copy and paste the following YAML content and save it to `usage-cost-stream.yaml`

```yaml
kind: Pod
apiVersion: v1
metadata:
  name: usage-detail-sender
  labels:
    app: usage-cost-stream
spec:
  containers:
    - name: usage-detail-sender
      image: <your-repo>/usagesender:latest
      ports:
        - containerPort: 8080
          protocol: TCP
      env:
        - name: SPRING_RABBITMQ_ADDRESSES
          value: rabbitmq
        - name: SERVER_PORT
          value: '8080'
  restartPolicy: Always

---
kind: Pod
apiVersion: v1
metadata:
  name: usage-processor
  labels:
    app: usage-cost-stream
spec:
  containers:
    - name: usage-processor
      image: <your-repo>/usageprocessor:latest
      ports:
        - containerPort: 8080
          protocol: TCP
      env:
        - name: SPRING_RABBITMQ_ADDRESSES
          value: rabbitmq
        - name: SERVER_PORT
          value: '8080'
  restartPolicy: Always

---
kind: Pod
apiVersion: v1
metadata:
  name: usage-logger
  labels:
    app: usage-cost-stream
spec:
  containers:
    - name: usage-logger
      image: <your-repo>/usagelogger:latest
      ports:
        - containerPort: 8080
          protocol: TCP
      env:
        - name: SPRING_RABBITMQ_ADDRESSES
          value: rabbitmq
        - name: SERVER_PORT
          value: '8080'
  restartPolicy: Always
```

Then you can deploy the apps, as follows:

```bash
kubectl apply -f usage-cost-stream.yaml
```

If all is well, you should see the following output:

```
pod/usage-detail-sender created
pod/usage-cost-processor created
pod/usage-cost-logger created
```

The preceding YAML specifies three pod resources, for the source, processor, and sink applications. Each pod has a single container that references the respective docker image.

We set the logical hostname for the RabbitMQ broker for each app to connect to it. Here we use the RabbitMQ service name, `rabbitmq` in this case. We also set the label `app: user-cost-stream` to logically group our apps.

#### Verifying the Deployment

To verify the deployment, use the following command to tail the log for the `usage-cost-logger` sink:

```bash
kubectl logs -f usage-logger
```

You should see messages similar to the following streaming:

```cmd
info: UsageLogger.UsageLogger[0]
      Received UsageCostDetail { "UserId" "user3", "CallCost": "$12.90", "DataCost": "$28.95" }
info: UsageLogger.UsageLogger[0]
      Received UsageCostDetail { "UserId" "user4", "CallCost": "$17.40", "DataCost": "$14.45" }
info: UsageLogger.UsageLogger[0]
      Received UsageCostDetail { "UserId" "user4", "CallCost": "$7.50", "DataCost": "$9.45" }
info: UsageLogger.UsageLogger[0]
      Received UsageCostDetail { "UserId" "user5", "CallCost": "$17.40", "DataCost": "$30.80" }
```

#### Cleaning Up

To delete the stream, we can use the label we created earlier, as follows:

```bash
kubectl delete pod -l app=usage-cost-stream
```

To uninstall RabbitMQ, run the following command:

```bash
kubectl delete all -l app=rabbitmq
```

## What's Next

You can use Spring Cloud Data Flow to deploy the three applications, as detailed in [Steeltoe Stream Processing using Spring Cloud Data Flow](./data-flow-stream.md).
