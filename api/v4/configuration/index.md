# Application Configuration

Steeltoe Configuration builds on the .NET configuration API, which enables developers to configure an application with values from a variety of sources by using configuration providers. Each provider supports reading a set of name-value pairs from a given source location and adding them into a combined multi-level configuration dictionary.

Each value contained in the configuration is tied to a string-typed key or name. The values are organized by key into a hierarchical list of name-value pairs in which the components of the keys are separated by a colon (for example, `Spring:Application:key = value`).

.NET supports the following providers and sources:

* Command-line arguments
* File sources (such as JSON, XML, and INI)
* Environment variables
* Custom providers

To better understand .NET configuration services, you should read the [ASP.NET Core](https://docs.microsoft.com/aspnet/core/fundamentals/configuration) documentation. Note that, while the documentation link suggests this service is tied to ASP.NET Core, it is not. It can be used in many different application types, including Console, ASP.NET 4.x., UWP, and others.

Steeltoe adds additional configuration providers to the preceding list:

* Cloud Foundry
* Kubernetes (Config Maps and Secrets)
* Placeholder resolvers
* RandomValue generator
* Spring Cloud Config Server

The following sections provide more more detail on each of these new providers.
