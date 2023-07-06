# Initializr

An Initializr jumpstarts .NET development by generating projects based on project metadata.
Metadata may include, among other properties, a project name, a namespace, and a list of dependencies.
At the core of an Initializr is the _[InitializrApi](https://github.com/SteeltoeOSS/InitializrApi)_.
The _InitializrApi_ provides several REST/HTTP endpoints which include an endpoint to generate projects and an endpoint to provide smart clients the metadata needed to construct user interfaces.

It is possible to have a fully functioning Initializr deployment by simply deploying the _InitializrApi_.
A more user-friendly deployment may include a user interface, such as a web frontend or an IDE plugin.
A deployment may also leverage a _[Spring Cloud Config Server](https://cloud.spring.io/spring-cloud-config/multi/multi__spring_cloud_config_server.html)_ to access a configuration store.
As an example, the [Steeltoe Initializr deployment](https://start.steeltoe.io) includes the _[InitializrWeb](https://github.com/SteeltoeOSS/InitializrWeb)_ for a friendly user experience and a _Spring Cloud Config Server_ using a GitHub maintained configuration.

The _InitializrWeb_ is the reference UI for the Steeltoe Initializr and is an example of a smart client.  It uses project metadata from the _InitializrApi_ to populate its web controls with, for example, supported .NET target frameworks and Steeltoe versions.

An Initializr configuration can be bundled along with an _InitializrApi_ deployment, or can be provided by a _Spring Cloud Config Server_.
As as example, the Steeltoe Initializr uses _Spring Cloud Config Server_ to access its GitHub-maintained configuration at https://github.com/SteeltoeOSS/InitializrConfig.
