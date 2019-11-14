Steeltoe provides a number of Security related services that simplify using Cloud Foundry based security services in ASP.NET applications.

These providers enable the use of Cloud Foundry security services (such as [UAA Server](https://github.com/cloudfoundry/uaa) and/or [Pivotal Single Sign-on](https://docs.pivotal.io/p-identity/)) for Authentication and Authorization.

You can choose from several providers when adding Cloud Foundry security integration:

* [OAuth2 Single Sign-on with Cloud Foundry Security services](#1-0-oauth2-single-sign-on) - ASP.NET (MVC, WebAPI), ASP.NET Core.
* [Using JWT tokens issued by Cloud Foundry for securing resources/endpoints](#2-0-resource-protection-using-jwt) - ASP.NET (MVC, WebAPI, WCF), ASP.NET Core.

In addition to Authentication and Authorization providers, Steeltoe Security offers:

* A security provider for [using Redis on Cloud Foundry with ASP.NET Core Data Protection Key Ring storage](#3-0-redis-key-storage-provider).
* A [CredHub API Client for .NET applications](#4-0-credhub-api-client) to perform credential storage, retrieval and generation.

>NOTE: Depending on your hosting environment, service instances you create for the purpose of exploring the Quick Starts on this page may have a cost associated.

