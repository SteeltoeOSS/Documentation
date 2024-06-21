# Cloud Security

Steeltoe provides several libraries that simplify using Cloud Foundry-based security services (such as [UAA Server](https://github.com/cloudfoundry/uaa), [Single-Sign-on for VMware Tanzu](https://docs.vmware.com/en/Single-Sign-On-for-VMware-Tanzu-Application-Service/index.html) and [instance identity certificates](https://docs.cloudfoundry.org/devguide/deploy-apps/instance-identity.html)) for authentication and authorization.

Choose from the following options when using Cloud Foundry security integration:

* [Single Sign-on with OpenID Connect](sso-open-id.md)
* [Resource protection with JWT Bearer tokens](jwt-authentication.md)
* [Resource protection using Mutual TLS (Certificate Authorization)](mtls.md)

In addition to authentication and authorization providers, Steeltoe security offers:

* [A security provider for using Redis on Cloud Foundry with ASP.NET Core Data Protection Key Ring storage](redis-key-storage-provider.md)
