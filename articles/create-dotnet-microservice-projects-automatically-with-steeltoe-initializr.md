---
_disableContribution: true
_showBackToBlogs: true

title: "Create .NET Microservice Projects Automatically with Steeltoe Initializr"
description: Initializr aims to get you (the developer) going faster, while making the right way the easy way.
type: markdown
date: 11/24/2020
uid: articles/create-dotnet-microservice-projects-automatically-with-steeltoe-initializr
tags: ["initializr"]
author.name: David Dieruf
author.github: ddieruf
author.twitter: dierufdavid
---

# Create .NET Microservice Projects Automatically with Steeltoe Initializr

Microservices are well… micro. They travel in packs. Very rarely will you find a single microservice in production representing an entire application's capabilities. Normally you have many services that make up the application. Each service is different in what controllers it offers. A user service offers endpoints for looking up a user's account information and validating a user's credentials. A customer service would have ways to interact with customer information. Outside of a microservice's controller, though, they look very similar. Each needs a web server. Each needs to emit logs. Etc.

Then there's shared services. Something like an authentication service is usually backing most of the application's microservices. But something like a cache is only selectively used in certain microservices that have the need. If user information doesn't change that often but is constantly being looked up, it makes sense to do side lookups in a cache. Alternatively customer information could be constantly changing but requests aren't that high frequency, so the overhead of a cache doesn't make sense.

When you are creating each of these microservices it turns into a rinse and repeat exercise. You are tempted to create a template or at least a script to do all the `dotnet new webapi` commands, taking into account namespaces and nuances.

As it turns out this is such a common task, tools have been created to help you through this. One notable tool is the dotnet templating engine. It gives a team the ability to crank out the beginnings of microservices in a flash. The challenge is, the nature of the tool is limiting once you look past a single developer team - to the entire organisation. The need for a more distributable model arises quickly. One where different folks with different roles can all collaborate together to keep best practices and the latest patches in all new development.

Also, because development doesn't happen in a vacuum, there is a constant flow of new services rolling out to make microservices better/faster/stronger. So when that new microservice is getting underway you want to do what's right (and use that new service) but you don't want to spend hours learning how to implement it.

All of this is why the Steeltoe Initializr project was created. Initializr aims to get you (the developer) going faster, while making the right way the easy way. It does this with extensible APIs that generate production ready .NET projects (csproj). Initializr options are expressed in a metadata model so the tool can grow with the community, as target frameworks change, Steeltoe versions advance, and individual dependencies offer new releases.

At its core, Steeltoe Initializr offers an API endpoint that takes a project's name, its target framework, and a list of dependencies as arguments.

Let's look at a quick example to get familiar.

We want to create a .NET Core microservice (aka webapi project) that hands us health reporting, dynamic logging levels, and a managed connection to Microsoft SQL Server. Using Steeltoe's public Initializr we could run…

# [Powershell](#tab/powershell)
```powershell
$body = @{Name:"MyApp",Dependencies:"Actuator,Dynamic-Logger,SQLServer"}

Invoke-RestMethod -Method 'Post' -Uri 'https://start.steeltoe.io/api/project' -Body $body -OutFile 'MyNewProject.zip'

#To unzip
Invoke-RestMethod -Method 'Post' -Uri 'https://start.steeltoe.io/api/project' -Body $body | Expand-Archive -DestinationPath .
```

# [Bash](#tab/bash)
```bash
http 'https://start.steeltoe.io/api/project' name=='MyProject' dependencies==actuator,dynamic-logger,sqlserver -d
```
***

The result will be a zip of the newly minted csproj. Within are all the best ways to implement Steeltoe actuators and Steeltoe SQL cloud connector.

## Using the Endpoints

Initializr's API offers a few very helpful endpoints. It's how the web UI is able to create such a great experience and how the community could extend its capabilities into Visual Studio or other developer related tools.

Below is a brief explanation of Initializr's top level endpoints. But the conversation doesn't stop there. Each of these endpoints offer all kinds of deeper sub-url's that drill down to specifics of Initializr's config. Read more about [them here](https://docs.steeltoe.io/api/v3/initializr).


### Endpoint Home [[view](https://start.steeltoe.io/api/)]

Sending a GET request to this endpoint will respond with essentials of the service. Things like what parameters can be provided when generating a project and what dependencies are available for use.


### Generate Project [[view](https://start.steeltoe.io/api/project)]

This endpoint supports both the GET and POST methods. This is where all the Initializr magic happens. As a GET request include parameters in the querystring. As a POST request, provide your project metadata as JSON in the body. Either way the response will be a zip of the generated project.

### Service Configuration [[view](https://start.steeltoe.io/api/config)]

The config endpoint provides a way to GET how Initializr has been configured. This endpoint has quite a few sub-endpoints that let you drill deeper into specific config values. Say you wanted to know what .NET runtimes are supported as well what the default version is. You could send a request to 'https://start.steeltoe.io/api/config/dotNetFrameworks' and receive a JSON formatted answer.

You can create quite a rich set of tooling with the config endpoint. In true cloud-native design, you can run instances of Initializr in different environments while the tooling keeps a consistent experience.

## About dependencies & templating

Initializr's special sauce is the collection of dependencies. It's the reason the tool is so powerful. It's also worthy of an entire discussion - there's quite a few moving parts. We're not going to get too deep in Initializr's inner workings except to show how the pieces fit together. If you would like to get deeper, the [project's documentation](https://docs.steeltoe.io/api/v3/initializr) is waiting just for you.

To create a new dependency for Initializr you're first going to choose a [parent project](https://github.com/SteeltoeOSS/InitializrConfig/tree/dev/src). This is the combination of .NET runtime version and Steeltoe version. Within that combination's project you'll find all the normal .NET Core workings - startup, program, etc. Double click on those files and you will see some surprising syntax. Initializr uses the [mustache templating engine](https://mustache.github.io/). Rules are declared within each .cs implementing a given dependencies best practice.

For example let's say you wanted to add the Spring Cloud Config server client to a .NET Core webapi project. There would be two main additions to the project.

1. Add the 'Steeltoe.Extensions.Configuration.ConfigServerCore' package reference.
2. Add Config Server as an additional configuration provider in the middleware, by adding the 'AddConfigServer()' statement in program.cs.

For a single microservice this isn't a big deal. In fact this is going to be one of the simplest clients you're ever going to implement. The challenge comes in implementing this in many microservices, across multiple teams, spanning the next year. You can't reasonably add these things in every project with any consistency.

Using mustache, Initializr would templatize the above two actions like this:

1. In the project's csproj add a check for including the dependency named config-server

    ```xml
    {{#config-server}}
        <PackageReference Include="Steeltoe.Extensions.Configuration.ConfigServerCore" Version="{{SteeltoeVersion}}" />
    {{/config-server}}
    ```

2. In Program.cs add a similar check for including the middleware

    ```csharp
    public static IWebHostBuilder CreateWebHostBuilder(string[] args) {
        return WebHost.CreateDefaultBuilder(args)
        {{#config-server}}
            .AddConfigServer()
        {{/config-server}}
    }
    ```

Now to create a new webapi project that implements the Spring Cloud Config server client you could visit the web UI and choose the dependency named “Spring Cloud Config Server” or you could run the following command.

# [Powershell](#tab/powershell)

```powershell
$body = @{Name:"AConfigClientApp",Dependencies:"config-server"}

Invoke-RestMethod -Method 'Post' -Uri 'https://start.steeltoe.io/api/project' -Body $body -OutFile 'MyNewProject.zip'
```

# [Bash](#tab/bash)

```bash
http 'https://start.steeltoe.io/api/project' name=='MyProject' dependencies==config-server -d
```
***

Repeatability and consistency are the name of the game when it comes to creating new ASP.NET microservices. The faster you can get the right project going that takes care of all the boiler plate things, the faster your services can get to production!

## Getting started with Initializr

Initializr has two ways to interact. If you're more familiar with point and click then you'll be right at home with the web UI - [https://start.steeltoe.io](https://start.steeltoe.io/).

![Initializr Home](images/initializr-home.png "https://start.steeltoe.io")

The website includes options like naming the project and namespace, picking your project's runtime version, Steeltoe version, and adding in all your needed dependencies. With those options checked you can then download the project (as zip), explore the source code (right in the browser!), or share this exact configuration with your friends.

![Initializr Explore](images/initializr-explore.png "https://start.steeltoe.io")

If interacting with web services are more of your foray, then Initializr's rest endpoints are the place to be. You won't be missing out on any options the UI offers, as it uses the endpoints internally. On a Windows desktop powershell's Invoke-RestMethod will interact perfectly. Just provide the endpoint address, method, and outfile. If you're generating a new project all it's metadata can be included in the body.

If you're on Linux or Apple desktop a terminal session using curl or http is also very simple. Similar to powershell provide the appropriate options and metadata to receive the generated zip.

Through either the web UI or the rest endpoints Initializr will get your microservices going fast. Which means you'll be checking in a production ready service in no time!
