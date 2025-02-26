# Initializr

An Initializr jumpstarts .NET development by generating projects based on project metadata.
Metadata may include, among other properties, a project name, a namespace, and a list of dependencies.
At the core of an Initializr is the _[InitializrService](https://github.com/SteeltoeOSS/InitializrService)_.
_InitializrService_ provides several REST/HTTP endpoints, which includes an endpoint to generate projects, and an endpoint to provide smart clients the metadata needed to construct user interfaces.

It is possible to have a fully functioning Initializr deployment by simply deploying the _InitializrService_.
A more user-friendly deployment may include a user interface, such as a web frontend or an IDE plugin.
A deployment may also leverage a _[Spring Cloud Config Server](https://cloud.spring.io/spring-cloud-config/multi/multi__spring_cloud_config_server.html)_ to access a configuration store.
As an example, the [Steeltoe Initializr deployment](https://start.steeltoe.io) includes _[InitializrWeb](https://github.com/SteeltoeOSS/InitializrWeb)_ for a friendly user experience and a _Spring Cloud Config Server_ using a GitHub-maintained configuration.

_InitializrWeb_ is the reference UI for the Steeltoe Initializr and is an example of a smart client.  It uses project metadata from the _InitializrService_ to populate its web controls with, for example, supported .NET target frameworks and Steeltoe versions.
