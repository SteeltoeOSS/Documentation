---
title: Steeltoe 2.2 Gives Your .NET Microservices a Boost
description: Learn how Steeltoe 2.2 can help your .NET Microservices with New Service Discovery Options, MongoDB Connector, and Placeholder Values
monikerRange: '== steeltoe-2.2'
date: 08/14/2020
uid: releases/steeltoe-2-2-gives-your--microservices-a-boost
tags: [new-release]
---

# Steeltoe 2.2 Gives Your .NET Microservices a Boost

By [David Dieruf](https://github.com/ddieruf)

My wife and I have an agreement. She doesn't question my fashion sense and I don't ask her to review release notes with me. "But Steeltoe 2.2 is coming out soon, honey." "Your pants look like mom jeans" she fires back. Clearly I am not going to win that exchange. 

While my better half may not be excited about the new Steeltoe, over 2.4 million NuGet downloads and a very active community suggest that lots of .NET developers are eager to dive in.

Each Steeltoe release is always jam-packed with useful new capabilities. [Steeltoe 2.2](http://steeltoe.io/reference/reference-release-notes/), now GA, delivers! Let’s take a spin through the highlights:


## New Health Contributors in Steeltoe.Management

On most cloud platforms, your microservices have a way to report their health back to a monitoring system, and that system decides an action to take. Ultimately, your service is either healthy or needs attention. The state is binary, but there can be many factors contributing to the status.

Steeltoe aggregates a few factors into the health of an application with something called Health Contributors. 

Steeltoe has health contributors for disk space, RabbitMQ, Redis, and persistent datastores (MySQL, Microsoft SQL Server, Postgres) already available. Now in Steeltoe 2.2, we add config server and discovery server. If your app's connection to one of these services is acting up, then the instance can be recycled and a new connection can be instantiated. As a dev, you don’t need to do anything, it just works.

Why does it just work? Steeltoe extensions take care of adding Health Contributors for you, no code needed. Similar to the existing contributors, for the new config server and discovery server, it makes these services a part of health contributions when you `.AddConfigServer()` or `.AddDiscoveryClient()` in your Startup.cs!


## Steeltoe.Connectors Gets a New MongoDB Connection

MongoDB is pretty darn popular these days. Why? Well, this document database features lightning-fast performance. It’s got high availability and scalability covered. And you can use it in a range of scenarios.To help .NET devs get more from MongoDB, Steeltoe adds a new connector and Health Contributor for this service.

What’s a Steeltoe Connector? Well, these provide a simple abstraction for .NET apps running on Cloud Foundry. Apps can discover bound services and deployment information at runtime. The mechanics of the MongoDB connector are similar to the existing connectors for Cloud Foundry, Redis, MySQL, RabbitMQ, MS SQL, and PostgreSQL. Use these connectors, and you never deal with connection strings again! \


Let’s prove this out with an example. 

With Steeltoe, there’s [very little to be done](https://steeltoe.io) to get your Mongo connection working. First, include the connection details in appsettings.json.


```
{
  "mongodb": {
    "client": {
      "server": "localhost",
      "port": 27017
    }
  }
}
```


Then, add in the middleware to Startup.cs.


```
public IServiceProvider ConfigureServices(IServiceCollection services)
{
    services.AddMongoClient(Configuration);
}
```


And away you go!


```
public class MyClass
{
  private readonly IMongoDatabase _database = null;

  public MyClass(IMongoClient mongoClient, MongoUrl mongoUrl)
  {
    _database = mongoClient.GetDatabase(mongoUrl.DatabaseName);
  }

  // GET api/values
  [HttpGet]
  public ActionResult<IEnumerable<string>> Get()
  {
  	var stuff = _database.GetCollection<SomeObject>("MyObjects");
  
  	return stuff.ToList();
  }
}
```



##  \
Load Balancing and a New Discovery Option in Steeltoe.Discovery

Service Discovery is such a wonderful cloud-native feature. As a complement to that you could also run your discoverable app(s) as multiple instances. Previously when a client needed to choose which service instance to send a request to, Steeltoe chose it randomly. Steeltoe 2.2 introduces a new option to [load balance requests across multiple instances](https://steeltoe.io). There are 3 implementations to choose from:



*   _RandomLoadBalancer_, as the name implies randomly selects a service instance from all instances that are resolved from a given service name. As an HttpClientFactory you could:

    ```
services.AddHttpClient("fortunes").AddRandomLoadBalancer()
```


*   _RoundRobinLoadBalancer_, sends traffic to service instances in a sequential order. As an HttpClientFactory you could:

    ```
services.AddHttpClient("fortunes").AddRoundRobinLoadBalancer()
```


*   _Custom_, allows you to create your own implementation of the ILoadBalancer.

    ```
services.AddHttpClient("fortunes").AddLoadBalancer<MyLoadBalancer>()
```



Also, because the Steeltoe team is happiest when its consumers have many options, a [new service discovery option](https://steeltoe.io) has been added - Hashicorp Consul. This is in addition to the existing Netflix Eureka option. Hashicorp's Consul server is completely distributed and can be highly available. It can scale to thousands of nodes with services across multiple datacenters.

To choose between either service registry you simply update the appsettings.json file. 


```
...
"consul": {
    "host" : "localhost",
    "port" : xxxx,
    "scheme": "http",
    "discovery: {
        "port" : 5000
    }
}
```



## Placeholder Values and Random Values in Steeltoe.Extensions

Normally I would agree that talking about how an app manages configuration values is not the most cutting edge topic, but the new features in Steeltoe 2.2 for placeholder values and random values are worth it.

Let's say your cloud native app is relying on configuration settings being provided through environment variables. Being the savvy developer that you are you'd like to provide default values to these expected env variables just in case something goes awry, but hard coding the default value is not a good idea. The new placeholder values is here to help. Using the appsettings.json file, create a placeholder for the configuration setting values and direct the value to first be retrieved from environment variables and then fall back to some default value.

Another new, convenient utility is the ability to generate random values. It can come in very handy when you are doing things like randomly selecting a collection item, needing a random number within a range of integers, or randomly creating a password. The utility support can create integers, longs, uuids, and strings. Similar to the placeholder values, random values can be used in either the appsettings.json or directly in C#.

Let's have a look at both of these new features, in code.


### Placeholder Values

First, an environment variable is set. (obviously the platform would do this part for you)


```
$env:MY_ENV_VAR = 888
```


Next, in the appsettings.json, we’ll retrieve the value and provide a default value.


<table>
  <tr>
  </tr>
</table>



```
{
  "Defaults": {
    "Value1": 222,
    "Value2": "Hi There"
  },
  "MyFirstValue": "${MY_ENV_VAR?${Defaults:Value1?NotFound}}",
  "MySecondValue": "${Defaults:Value2?NotFound}"
}
```


We provide a structured way of holding the values, for the app to use.


```
public class MyAppSettings {
        public int MyFirstValue { get; set; }
        public string MySecondValue { get; set; }
}
```


Let everything execute when the .NET core app starts (ie: at runtime)


```
public class Program {
    ....

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            ....
            // Add Steeltoe Placeholder resolver                                      
            .AddPlaceholderResolver()
            .UseStartup<Startup>();
}
```



```
public class Startup {
    public Startup(IConfiguration configuration) {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services) {
        // Setup Options framework with DI
        services.AddOptions();
        // Configure the MyAppSettings class with configuration data
        services.Configure<MyAppSettings>(Configuration);
    }
    ....
}
```


And now consume those settings values.


<table>
  <tr>
   <td><code>.... \
using Microsoft.Extensions.Options; \
 \
public class HomeController : Controller { \
  private IOptionsMonitor&lt;MyAppSettings> _opts; \
 \
  private MyAppSettings Options { \
    get { return _opts.CurrentValue; } \
  } \
 \
  public HomeController(IOptionsMonitor&lt;MyAppSettings> opts) { \
    _opts = opts; \
  } \
       \
  <em>// GET api/values</em> \
  [HttpGet] \
  public ActionResult&lt;IEnumerable&lt;string>> Get() \
  { \
    var mySettingValue = Options.MyFirstValue; \
    var anotherSetting = Options.MySecondValue; \
   \
    return new string(){mySettingValue, anotherSetting }; \
  } \
  .... \
}</code>
   </td>
  </tr>
  <tr>
   <td>
   </td>
  </tr>
</table>


Notice the use of `_opts.CurrentValue`. Similar to using an external configuration server like Spring Cloud Config, now we’ve created the ability to refresh the settings’ value without ever reploying or restaging the app!


### Random Values

Continuing with the above example, let make the default values in appsettings.json be random and lets randomly create an integer between 1 and 100.


```
{
  "Defaults": {
    "Value1": "${random:int}",
    "Value2": "${random:value}",
    "number_in_range": "${random:int[1,100]}"
  },
  "MyFirstValue": "${MY_ENV_VAR?${Defaults:Value1?NotFound}}",
  "MySecondValue": "${Defaults:Value2?NotFound}"
}
```


Now, let's create a new endpoint that returns a randomly generated string.


```
....
using Microsoft.Extensions.Configuration;

public class HomeController : Controller {
  private IConfiguration _config;

  public HomeController(IConfiguration config) {
    _config = config;
  }
     
  // GET api/values/random
  [HttpGet("random")]
  public ActionResult<string> GenerateRandomString()
  {
    return _config["random:string"];
  }
  ....
}
```



## Get Started

Sure I know, now you are asking how can I see all these wonderful new features in action. If you’d like to work locally, the Steeltoe team has published some [Docker Files](https://github.com/SteeltoeOSS/Dockerfiles) to help get started. Combine this with the sample apps in the [Steeltoe Github repo](https://github.com/SteeltoeOSS/Samples) and you can get a local app instance running Steeltoe in no time.

If you prefer a preconfigured platform, head on over to [Pivotal Web Services](https://run.pivotal.io/) and create a free account. This will give you access to a fully productionized Cloud Foundry platform. Grab the sample apps in the [Steeltoe Github Samples repo](https://github.com/SteeltoeOSS/Samples) along with the docs on the [Steeltoe site](https://steeltoe.io/docs), and you are off to the races.

Cloud native .NET here we come!
