# Management

This section describes the various management endpoints that you can configure Steeltoe to expose.

## Endpoints

Steeltoe includes a number of optional features that you can add to your applications to aid in monitoring and managing it while it runs in production. These features are implemented as a number of management endpoints that you can easily add to your application.

The way the endpoints are exposed and used depends on the type of technology you choose in exposing the functionality of each endpoint. Out of the box, Steeltoe provides several easy ways to expose these endpoints over HTTP in .NET applications. Of course, you can build and use whatever you would like to meet your needs.

## Management Tasks

[Steeltoe Management Tasks](./tasks.md) provide a means of running administrative tasks for ASP.NET Core applications with the same context as the running version of your application. The original use case for this feature is managing database migrations with a bound database service on Cloud Foundry, but the framework is extensible for you to create your own tasks.
