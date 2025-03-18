---
_disableBreadcrumb: true
_disableContribution: true
_showBackToBlogs: true

title: "Create .NET Microservice Projects Automatically with Steeltoe Initializr"
description: Initializr aims to get you (the developer) going faster, while making the right way the easy way.
type: markdown
date: 1/10/2021
uid: articles/create-dotnet-microservice-projects-automatically-with-steeltoe-initializr
tags: ["initializr"]
author.name: David Dieruf
author.github: ddieruf
author.twitter: dierufdavid
---

# Create .NET microservice projects automatically with Steeltoe Initializr

Microservices are well… micro. They travel in packs. Very rarely will you find a single microservice in production representing an entire application's capabilities. Normally you have many services that make up the application. Each service is different in what controllers it offers. A user service offers endpoints for looking up a user's account information and validating a user's credentials. A customer service would have ways to interact with customer information. Outside of a microservice's controller, though, they look very similar. Each needs a web server. Each needs to emit logs. Etc.

Then there's shared services. Something like an authentication service is usually backing most of the application's microservices. But something like a cache is only selectively used in certain microservices that have the need. If user information doesn't change that often but is constantly being looked up, it makes sense to do side lookups in a cache. Alternatively customer information could be constantly changing but requests aren't that high frequency, so the overhead of a cache doesn't make sense.

When you are creating each of these microservices it turns into a rinse and repeat exercise. You are tempted to create a template or at least a script to do all the `dotnet new webapi` commands, taking into account namespaces and nuances.

As it turns out this is such a common task, tools have been created to help you through this. One notable tool is the dotnet templating engine. It gives a team the ability to crank out the beginnings of microservices in a flash. The challenge is once you look past a single developer to the entire organization the need for a more distributable model arises quickly. One where different folks with different roles can collaborate to keep best practices a priority and the latest patches a reality, in all new development.

Also, because development doesn't happen in a vacuum, there is a constant flow of new services rolling out to make microservices better/faster/stronger. So when that new microservice is getting underway you want to do what's right (and use that new service) but you don't want to spend hours learning how to implement it.

All of this is why the Steeltoe Initializr project was created. Initializr aims to get you (the developer) going faster, while making the right way the easy way. It does this with extensible APIs that generate production ready .NET projects (csproj). Initializr options are expressed in a metadata model so the tool can grow with the community, as target frameworks change, Steeltoe versions advance, and individual dependencies offer new releases.

## Getting started with Initializr

Initializr has two ways to interact. If you're more familiar with point and click then you'll be right at home with the web UI - [https://start.steeltoe.io](https://start.steeltoe.io/).

![Steeltoe Initializr](./images/initializr-home.png)

The website includes options like naming the project and namespace, picking your project's runtime version, Steeltoe version, and adding in all your needed dependencies.

With those options checked you can “Generate" the project (as zip) and get going in your favorite IDE. Or you “Explore" the source code right in the browser. This is a great way to see exactly what the templating engine will create without the fuss of a zip. If you have an existing microservice that is in need of new Steeltoe features, using the explore function is a great way to see exactly what is needed.

![Steeltoe Initializr](./images/initializr-explore.png)

There is also an option to “Share" the newly created project with your co-workers and friends. This makes things very convenient when you don't have a shared screen or public repo handy.

At its core, Steeltoe Initializr offers an API endpoint that takes a project's name, its target framework, and a list of dependencies as arguments.

Let's look at a quick example to get familiar.

We want to create a .NET Core microservice (aka webapi project) that hands us health reporting, dynamic logging levels, and a managed connection to Microsoft SQL Server. Using Steeltoe's public Initializr we could run…

# [Powershell](#tab/powershell)

```powershell
$body = @{
    Name='MyApp'
    Dependencies='Actuator,Dynamic-Logger,SQLServer'
}

Invoke-RestMethod -Uri 'https://start.steeltoe.io/api/project' -Body $body -OutFile Sample.zip
```

# [Bash](#tab/bash)

```bash
$ http https://start.steeltoe.io/api/project dependencies==actuator,dynamic-logger,sqlserver -d
```

***

The result will be a zip of the newly minted csproj. Within are all the best ways to implement Steeltoe actuators and Steeltoe SQL cloud connector.

If interacting with web services are more of your foray, then Initializr's rest endpoints are the place to be. You won't be missing out on any options the UI offers, as it uses the endpoints internally. On a Windows desktop powershell's Invoke-RestMethod will interact perfectly. Just provide the endpoint address, method, and outfile. If you're generating a new project all it's metadata can be included in the body.

If you're on Linux or Macintosh, a terminal session using `curl` or HTTPie it is also very simple. Similar to Powershell, provide the appropriate options and metadata to receive the generated zip.

Through either the web UI or the rest endpoints, Initializr will get your microservices going fast. Which means you'll be checking in a production ready service in no time!

## Using the API Endpoints

Initializr's API offers a few very helpful endpoints. It's how the web UI is able to create such a great experience and how the community could extend its capabilities into Visual Studio or other developer related tools.

Below is a brief explanation of Initializr's top level endpoints. But the conversation doesn't stop there. Each of these endpoints offer all kinds of deeper sub-url's that drill down to specifics of Initializr's config. Read more about [them here](/api/v3/initializr/index.md).

  **Endpoint Home**

  Sending a GET request to this endpoint ([https://start.steeltoe.io/api/](https://start.steeltoe.io/api/)) will respond with essentials of the service. Things like what parameters can be provided when generating a project and what dependencies are available for use.

  **Generate Project**

  The endpoint `https://start.steeltoe.io/api/project` supports both the GET and POST methods. This is where all the Initializr magic happens. Send a GET request and include parameters in the querystring. Send a POST request and provide your project metadata as JSON in the body. Either way the response will be a zip of the generated project.

  **Service Configuration**

  The config endpoint ([https://start.steeltoe.io/api/config/](https://start.steeltoe.io/api/config/)) provides a way to get how Initializr has been configured. This endpoint has quite a few sub-endpoints that let you drill deeper into specific config values. Say you wanted to know what .NET runtimes are supported as well what the default version is. You could send a request to [https://start.steeltoe.io/api/config/dotNetFrameworks](https://start.steeltoe.io/api/config/dotNetFrameworks) and receive a JSON formatted answer.

  You can create quite a rich set of tooling with the config endpoint. In true cloud-native design, you can run instances of Initializr in different environments while the tooling keeps a consistent experience.

## About dependencies

Initializr's special sauce is the collection of dependencies. It's the reason the tool is so powerful. It's also worthy of an entire discussion - there's quite a few moving parts. We're not going to get too deep in Initializr's inner workings at this time. So, if you would like to get deeper the [project's documentation](/api/v3/initializr/index.md) is waiting just for you.

When Initializr generates a new project that has added dependencies, the templating is used to “fill in the blanks”. Let see an example of this in action.

Say we want to create a new .NET Core webapi microservice that has health checking built in and is going to connect with a Microsoft SQL database. On [start.steeltoe.io](https://start.steeltoe.io) you could choose the “Actuators” dependency and the “Microsoft SQL Server” dependency. Then “Generate” the project and you're on the way to cloud-native nirvana.

Alternatively you could also run the following command:

# [Powershell](#tab/powershell)

```powershell
$body = @{
    Name='MyProject'
    Dependencies='Actuator,SQLServer'
}

Invoke-RestMethod -Uri 'https://start.steeltoe.io/api/project' -Body $body -OutFile Sample.zip
```

# [Bash](#tab/bash)

```bash
$ http https://start.steeltoe.io/api/project dependencies==actuator,sqlserver -d
```

***

Repeatability and consistency are the name of the game when it comes to creating new ASP.NET microservices. The faster you can get the right project going, the faster your services can get to production!
