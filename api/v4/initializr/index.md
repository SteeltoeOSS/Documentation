# Initializr

An Initializr jumpstarts .NET development by generating projects based on project metadata.
Metadata may include, among other properties, a project name, a namespace, and a list of dependencies.
At the core of an Initializr is the [Steeltoe InitializrService](https://github.com/SteeltoeOSS/InitializrService).
InitializrService provides several REST/HTTP endpoints, and includes:

* an endpoint to generate projects
* an endpoint to provide smart clients the metadata needed to construct user interfaces

It is possible to have a fully functioning Initializr deployment by simply deploying the InitializrService.
A more user-friendly deployment might include a user interface, such as a web frontend or an IDE plug-in.
A deployment may also use a [Spring Cloud Config Server](https://cloud.spring.io/spring-cloud-config/multi/multi__spring_cloud_config_server.html) to access a configuration store.
As an example, the [Steeltoe Initializr deployment](https://start.steeltoe.io) includes [InitializrWeb](https://github.com/SteeltoeOSS/InitializrWeb) for a friendly user experience and a Spring Cloud Config Server using a GitHub-maintained configuration.

InitializrWeb is the reference UI for the Steeltoe Initializr and is an example of a smart client.  It uses project metadata from the InitializrService to populate its web controls with, for example, supported .NET target frameworks and Steeltoe versions.
