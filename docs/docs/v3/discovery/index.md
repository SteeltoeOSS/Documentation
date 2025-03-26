# Service Discovery

Steeltoe provides a set of generalized interfaces for interacting with multiple service registry options. This section covers the general components first. If you are looking for something specific to the registry server you are using, skip ahead to the section on that provider.

In order to use any Steeltoe Discovery client, you need to:

* Add the appropriate NuGet package reference(s) to your project.
* Configure the settings the discovery client uses to register services in the service registry.
* Configure the settings the discovery client uses to discover services in the service registry.
* Use the discovery client service in the application.

>The Steeltoe discovery implementation (for example: the decision between Eureka and Consul) is automatically set up within the application, based on the application configuration provided.
