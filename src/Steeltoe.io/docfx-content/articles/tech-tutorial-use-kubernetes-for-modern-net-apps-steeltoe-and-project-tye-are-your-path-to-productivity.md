---
_disableBreadcrumb: true
_disableContribution: true
_showBackToBlogs: true

title: "Tech Tutorial: Use Kubernetes for Modern .NET Apps? Steeltoe and Project Tye are Your Path to Productivity"
description: Use new Steeltoe 3.0 features for Kubernetes and Project Tye to get modern in a hurry
type: markdown
date: 10/01/2020
uid: articles/tech-tutorial-use-kubernetes-for-modern-net-apps-steeltoe-and-project-tye-are-your-path-to-productivity
tags: ["modernization"]
author.name: David Dieruf
author.github: ddieruf
author.twitter: dierufdavid
---

# Tech Tutorial: Use Kubernetes for Modern .NET Apps? Steeltoe and Project Tye are Your Path to Productivity

I recently went through training to get my [Kubernetes Application Developer certification](https://www.cncf.io/certification/ckad/) (CKAD). I went into it with a decent amount of K8s knowledge and an even greater understanding of microservices. In fact my understanding of proper microservice architectures has raised my expectations for the tech I use.

For example, I expect my selected platform to automatically provide some of the [12 factors](https://12factor.net/) I use in my design patterns. It should capture my streamed logs. It should allow me to autoscale app instances. It should allow for dynamic port binding. Everything should be immutable. And (the biggest one on my list) it should be architected in a way where I can recreate it locally while developing. Do these may seem like tall expectations? Perhaps, but any modern platform should do this.

I (honestly) set my expectations of the training low. After all, Kubernetes is awesome for managing infrastructure. But it’s a little raw for developers.

In an ideal world, a developer isn't interacting directly with infrastructure. They interact with GIT, which interacts with a CI/CD pipeline, which interacts with the runtime. But we live in the real world, and it's never going to be ideal.

Having accepted an inevitable fate of yaml and dockerfile, my training taught me about all the developer tools in Kubernetes. Operators, controllers, namespaces, configmaps, secrets, and of course pods. My training took me through attaching disks and providing configurations to my app. It showed me how apps can interact with other apps in the same cluster through a DNS-like pattern. Powerful stuff!

As I digested all this Kubernetes magic, a realization dawned on me. I don't want to follow Kubernetes patterns in my .NET application. I want to follow .NET patterns in my .NET application. How can I have a better developer experience using Kubernetes, and follow best practices for .NET? And how can I do it without having to create everything from scratch?

Turns out, lots of other developers are wrestling with the same thing. But it gets better - the community has developed a project to deal with this exact scenario: [Steeltoe](https://steeltoe.io).

Steeltoe 3.0  brings in Kubernetes goodies that abstract much of this toil away. When I coupled Steeltoe 3 with Microsoft’s [Project Tye](https://devblogs.microsoft.com/aspnet/introducing-project-tye/), my sky-high developer expectations are met.

Let's look at how the combination of Steeltoe and Tye make consuming Kubernetes a whole lot easier.

## Microservices should be given their configuration (and secrets)

When you think of an application’s configuration value, a connection string is probably top of mind. It could really be anything. Typically microservices can't successfully start (or atleast run) without being fed proper values.

As a microservice moves through its environments (local, staging, production) configuration values change, but the labels of the values are consistent. A microservice stays resilient to these changes by simply consuming the value it is fed.

Further, configuration values come from different sources in different environments. A microservice should be smart enough to check every possible source and decide what value should be used.

.NET Core introduced [configuration providers](https://docs.microsoft.com/aspnet/core/fundamentals/configuration/?view=aspnetcore-3.1#configuration-providers) and a hierarchy to them. So it's natural that a Kubernetes configmap (which is just another key/value store) should be a config source in a .NET microservice.

[Steeltoe offers a Kubernetes provider](/api/v3/configuration/kubernetes-providers.md) to do just this! To add the client all you need to do is let the HostBuilder know about it.

```csharp
public static IHostBuilder CreateHostBuilder(string[] args) =>     Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webBuilder => {
        webBuilder.UseStartup<Startup>();
      })
      .AddKubernetesConfiguration()
```

This little bit of code packs a powerful punch. Behind the scenes Steeltoe will use the key mappings to create a custom configuration provider and add it into the hierarchy. In turn all this will be added into the IConfiguration interface which is a part of dependency injection. This makes retrieving the values very straight forward.

```csharp
private readonly IConfiguration config;

public WeatherForecastController(IConfiguration config){
    _config = config;
    //_config[“my-setting”];
}
```

Also included is the option to automatically refresh values. When enabled, your application will poll (or maintain an open connection) with the Kubernetes API server. Whenever a value in the ConfigMap is changed, your app’s values will also change -- no restart needed. Whenever a value is added or removed from the ConfigMap (or Secret) your app will automatically see those changes -- also no restart required. That's some cloud-native ninja action!

Getting the value(s) of Kubernetes secrets to the application follows the same pattern as ConfigMaps. Additionally you can pick and choose which source should be made available to the application. [Learn more in the docs](/api/v3/configuration/kubernetes-providers.md).

## If all the services are in the same cluster...

When your applications are running within the same Kubernetes cluster there is quite a bit to take advantage of. Load balancing, routing, and DNS services are a few. As it just so happens these are also the things needed for a very popular cloud-native pattern called service discovery.

Things lined up even more for Steeltoe as it already has a discovery client that lets you abstract the provider away. So naturally including the option to use a Kubernetes cluster as a discovery provider was a quick and easy thing.

To implement the Steeltoe discovery client, add the `Steeltoe.Discovery.Kubernetes` nuget package to the application and implement the general discovery client in program.cs…

```csharp
using Steeltoe.Discovery.Client;
//--
public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder => {
            webBuilder.UseStartup<Startup>();
        })
        .AddDiscoveryClient()
    ```

The combination of adding a specific discovery client’s package and generally implementing the discovery client tells Steeltoe exactly what the app wants to do. Under the covers Steeltoe will manage all the interactions with the Kubernetes API.

> [TIP] You can also include the Eureka or Consul client package along with generally implementing the discovery client. Steeltoe will add in the appropriate provisions for the desired provider.

Steeltoe actively discovers endpoints for all applications registered in the cluster. Because registration is automatic in Kubernetes there’s really no work to be done. That's a very nice cloud-native gimme!

An example to discover a service named ‘fortuneService’ that has an endpoint prefix of ‘/api/fortunes’ we can follow .NET best practices using an HTTPClient factory...

```csharp
public void ConfigureServices(IServiceCollection services) {
    services.AddHttpClient("fortunes", c => {
        c.BaseAddress = new Uri("http://fortuneService/api/fortunes/");
    })
    .AddServiceDiscovery()
    .AddTypedClient<IFortuneService, FortuneService>();
}
```

Steeltoe will take care of reading the registry behind the scenes. To use the client’s built in factory use constructor injection and get on with life!

```csharp
private readonly HttpClient _httpClient;

public FortuneService(HttpClient httpClient, ILoggerFactory logFactory){
    _httpClient = httpClient;
    //Fortune[] fortunes = await _httpClient.GetJsonAsync<Fortune>(“random-fortunes”);
}
```

[Customizable load-balancing](/api/v3/discovery/load-balancing.md) between multiple application instances of the same service? Done. Automatic service registration? Done. Steeltoe and Kubernetes are like peanut butter and jelly. They were made for one another.

## Teach Kubernetes what a healthy application is

Any proper platform running containers is going to have a concept of health reporting. An application reports its heartbeat (usually as an HTTP endpoint) and is checked on some interval by its platform. If there are complications then the platform acts accordingly.

In Kubernetes, there are two deeper probes offered named `readiness` and `liveness`. Readiness is about ensuring the application is ready to start receiving traffic. While liveness is about making sure the application is still healthy over time.

Fortunately, Steeltoe is here to help set up both of these probes. Using new IHealthContributors and support for grouping, Steeltoe’s [health actuator endpoint](/api/v3/management/health.md) builds several different views of the app’s dependencies.

A view is a collection of decisions that reflect the current state of the application. For example if an application depends on a service and it (for some reason) goes offline, then the view should return a negative state. When the service is back up, then the view’s state should update to positive.

The current state of the views are a part of the response body in the health actuator endpoint. Kubernetes uses this to know how the app is doing.

Learn more about Steeltoe’s health endpoint with documentation and code snippets, [here](/api/v3/management/health.md).

## Develop locally, as if you’re already in production

Just like the old saying “dance as if no one is watching”, an application should not care who’s watching (ie: which environment). It should start and run exactly the same everywhere. The ability to run the same type cluster no matter the [physical] environment is definitely one of Kubernetes super powers. And you have all kinds of options to run local K8s clusters (like Docker Desktop or Kind).

But the question I ask myself before going down this path is, what am I really accomplishing? It’s a natural instinct to want to develop on the same platform that the app will be using, but is it worth the time needed for setup? Under the covers there are really just a few primities that enable your application and its dependent services to do their job - container management and request routing with some sort of DNS. Yes there are all kinds of other fancy ingress, egress, service mesh, etc etc things going on in Kubernetes. And yes they are all a part of deciding if your app is actually going to work on the platform, but should they be a part of development? You’re never going to recreate all the firewalls, reverse proxies, certificates, and switches that requests have to traverse. And if we’re doing microservices the right way, the application shouldn’t care one bit about any of that stuff.

The local development needs are:

* The ability to build and run containers (ie: Docker)
* The ability to create a distinct container subnet and resolve names within
* The ability for an IDE running the application (in debugging mode) to interact with that subnet

Say hello to [Project Tye](https://github.com/dotnet/tye). With Tye the environment is software defined as a manifest. Port assignment and container networking are done for you. You can run the application and all it’s services in individual containers. Or you could run the application from its IDE and just have Tye run the backing services. Most notably you can have Tye target Kubernetes as the intended platform. In fact you could switch between a local cluster and a hosted cluster with no change to the application!

Project Tye solves a cloud-native developer’s biggest challenge, environment parity. It’s super friendly to all your existing .NET habits and doesn’t have much bias about an IDE. All you need is the [tool installed in the dotnet cli](https://github.com/dotnet/tye/blob/main/docs/getting_started.md) and Docker running… and maybe a little yaml skill.

## Learn more and get started today

To get started with any Steeltoe projects, head over to the [getting started guides](../guides/index.md). Combine this with the samples in the [Steeltoe GitHub repo](https://github.com/SteeltoeOSS/Samples/tree/3.x), and you’ll have .NET microservices up and running before you know it!

Want to get deeper into creating cloud-native .NET apps? Attend the VMware Pivotal Labs’s [4 -day .NET developer course](https://pivotal.io/platform-acceleration-lab/pal-for-developers-net). You’ll get hands-on cloud-native .NET training learn best practices when creating microservices and become a Steeltoe ninja!
