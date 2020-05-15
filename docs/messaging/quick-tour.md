[[quick-tour]]
# Quick Tour

This is the five-minute tour to get started with Steeltoe AMQP.

Prerequisites: Install and run the RabbitMQ broker ([https://www.rabbitmq.com/download.html](https://www.rabbitmq.com/download.html)).
Then grab the spring-rabbit JAR and all its dependencies. The easiest way to do so is to declare a dependency in your build tool.
For example, for Maven, you can do add:

====
[source,xml,subs="+attributes"]
----
<dependency>
  <groupId>org.springframework.amqp</groupId>
  <artifactId>spring-rabbit</artifactId>
  <version>{project-version}</version>
</dependency>
----
====

For Gradle, you can add:

====
[source,groovy,subs="+attributes"]
----
compile 'org.springframework.amqp:spring-rabbit:{project-version}'
----
====

[[compatibility]]
## Compatibility

The minimum Spring Framework version dependency is 5.2.0.

The minimum `amqp-client` Java client library version is 5.7.0.

## Very, Very Quick

This section offers the fastest introduction.

First, add the following `import` statements to make the examples later in this section work:

====
[source, java]
----
import org.springframework.amqp.core.AmqpAdmin;
import org.springframework.amqp.core.AmqpTemplate;
import org.springframework.amqp.core.Queue;
import org.springframework.amqp.rabbit.connection.CachingConnectionFactory;
import org.springframework.amqp.rabbit.connection.ConnectionFactory;
import org.springframework.amqp.rabbit.core.RabbitAdmin;
import org.springframework.amqp.rabbit.core.RabbitTemplate;
----
====

The following example uses plain, imperative Java to send and receive a message:

====
[source,java]
----
ConnectionFactory connectionFactory = new CachingConnectionFactory();
AmqpAdmin admin = new RabbitAdmin(connectionFactory);
admin.declareQueue(new Queue("myqueue"));
AmqpTemplate template = new RabbitTemplate(connectionFactory);
template.convertAndSend("myqueue", "foo");
String foo = (String) template.receiveAndConvert("myqueue");
----
====

Note that there is also a `ConnectionFactory` in the native Java Rabbit client.
We use the Spring abstraction in the preceding code.
It caches channels (and optionally connections) for reuse.
We rely on the default exchange in the broker (since none is specified in the send operation) and the default binding of all queues to the default exchange by their name (thus, we can use the queue name as a routing key in the send operation).
Those behaviors are defined in the AMQP specification.

## With Java Configuration

The following example repeats the preceding example but with the external configuration defined in Java:

```Java
ApplicationContext context =
    new AnnotationConfigApplicationContext(RabbitConfiguration.class);
AmqpTemplate template = context.getBean(AmqpTemplate.class);
template.convertAndSend("myqueue", "foo");
String foo = (String) template.receiveAndConvert("myqueue");

........

@Configuration
public class RabbitConfiguration {

    @Bean
    public ConnectionFactory connectionFactory() {
        return new CachingConnectionFactory("localhost");
    }

    @Bean
    public AmqpAdmin amqpAdmin() {
        return new RabbitAdmin(connectionFactory());
    }

    @Bean
    public RabbitTemplate rabbitTemplate() {
        return new RabbitTemplate(connectionFactory());
    }

    @Bean
    public Queue myQueue() {
       return new Queue("myqueue");
    }
}
```

## With Spring Boot Auto Configuration and an Async POJO Listener

Spring Boot automatically configures the infrastructure beans, as follows:

```Java
@SpringBootApplication
public class Application {

    public static void main(String[] args) {
        SpringApplication.run(Application.class, args);
    }

    @Bean
    public ApplicationRunner runner(AmqpTemplate template) {
        return args -> template.convertAndSend("myqueue", "foo");
    }

    @Bean
    public Queue myQueue() {
        return new Queue("myqueue");
    }

    @RabbitListener(queues = "myqueue")
    public void listen(String in) {
        System.out.println(in);
    }

}
```
