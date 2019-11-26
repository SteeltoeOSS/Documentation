Steeltoe includes a number of optional features you can add to your applications to aid in monitoring and managing it while it runs in production. These features are implemented as a number of management endpoints that you can easily add to your application.

The way the endpoints are exposed and used depends on the type of technology you choose in exposing the functionality of the endpoint. Out of the box, Steeltoe provides several easy ways to expose these endpoints over HTTP in .NET applications. Of course, you can build and use whatever you would like to meet your needs.

When you expose the endpoints over HTTP, you can also integrate the endpoints with the [Pivotal Apps Manager](https://docs.pivotal.io/pivotalcf/2-0/console/index.html). The [quick start](#1-1-quick-start), explores this integration in more depth. You should read the [Using Actuators with Apps Manager section](https://docs.pivotal.io/pivotalcf/2-0/console/using-actuators.html) of the Pivotal Cloud Foundry documentation for more details.

>NOTE: Depending on your hosting environment, service instances you create for the purpose of exploring the Quick Starts on this page may have a cost associated.

## Management Endpoints

The following table describes all of the currently available Steeltoe management endpoints:

|ID|Description|
|---|---|
|**hypermedia**|Provides hypermedia endpoint for discovery of all available endpoints|
|**cloudfoundry**|Enables management endpoint integration with Pivotal Cloud Foundry|
|**health**|Customizable endpoint that reports application health information|
|**info**|Customizable endpoint that reports arbitrary application information (such as Git Build info and other details)|
|**loggers**|Allows remote access and modification of logging levels in a .NET application|
|**trace**|Reports a configurable set of trace information (such as the last 100 HTTP requests)|
|**refresh**|Triggers the application configuration to be reloaded|
|**env**|Reports the keys and values from the applications configuration|
|**mappings**|Reports the configured ASP.NET routes and route templates|
|**metrics**|Reports the collected metrics for the application|
|**dump**|Generates and reports a snapshot of the applications threads (Windows only)|
|**heapdump**|Generates and downloads a mini-dump of the application (Windows only)|

More detail on each endpoint is provided in upcoming sections.

Note that the Steeltoe Management endpoints themselves support the following .NET application types:

* ASP.NET Core and ASP.NET 4.x
* Console apps (.NET Framework and .NET Core)

Steeltoe currently includes support for exposing the Management endpoints over HTTP with ASP.NET.

Here is a Steeltoe sample application that you can refer to in order to help you understand how to use these endpoints, including:

* [MusicStore](https://github.com/SteeltoeOSS/Samples/tree/master/MusicStore): A sample application showing how to use all of the Steeltoe components together in a ASP.NET Core application. This is a microservices-based application built from the ASP.NET Core MusicStore reference application provided by Microsoft.
