# Application Configuration

Steeltoe Configuration builds on the .NET configuration API, which enables developers to configure an application with values from a variety of sources by using configuration providers. Each provider supports reading a set of name-value pairs from a given source location and adding them into a combined multi-level configuration dictionary.

Each value contained in the configuration is tied to a string-typed key or name. The values are organized by key into a hierarchical list of name-value pairs in which the components of the keys are separated by a colon (for example, `Spring:Application:Name = DemoApp`).

.NET supports the following providers and sources:

* Command-line arguments
* File sources (such as JSON, XML, and INI)
* Environment variables
* Custom providers

To better understand .NET configuration services, you should read the [ASP.NET Core documentation](https://learn.microsoft.com/aspnet/core/fundamentals/configuration). Note that, while the documentation link suggests this service is tied to ASP.NET Core, it is not. It can be used in many different application types, including Console, ASP.NET 4.x., UWP, and others.

Steeltoe adds additional configuration providers to the preceding list:

* Cloud Foundry (JSON from the `VCAP_APPLICATION` and `VCAP_SERVICES` environment variables)
* Spring Cloud Config Server
* Placeholder resolvers
* Decryption of encrypted values
* Random value generator
* Spring Boot configuration

The following sections provide more detail on each of these Steeltoe providers.
