# Cloud Security

Steeltoe provides a number of Security related services that simplify using Cloud Foundry based security services in ASP.NET applications.

These providers enable the use of Cloud Foundry security services (such as [UAA Server](https://github.com/cloudfoundry/uaa) and/or [Single Sign-On for VMware Tanzu](https://techdocs.broadcom.com/us/en/vmware-tanzu/platform-services/single-sign-on-for-tanzu/1-16/sso-tanzu/index.html)) for Authentication and Authorization.

You can choose from several providers when adding Cloud Foundry security integration:

* OAuth2 Single Sign-on with Cloud Foundry Security services - ASP.NET (MVC, WebAPI), ASP.NET Core.
* Using JWT tokens issued by Cloud Foundry for securing resources/endpoints - ASP.NET (MVC, WebAPI, WCF), ASP.NET Core.

In addition to Authentication and Authorization providers, Steeltoe Security offers:

* A security provider for using Redis on Cloud Foundry with ASP.NET Core Data Protection Key Ring storage.
* A CredHub API Client for .NET applications to perform credential storage, retrieval and generation.
