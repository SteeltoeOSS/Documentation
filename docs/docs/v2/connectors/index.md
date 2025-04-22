# Service Connectors

Steeltoe Service Connectors simplify the process of connecting and using services on Cloud Foundry. Steeltoe Connectors provide a simple abstraction for .NET based applications running on Cloud Foundry, letting them discover bound services and deployment information at runtime. The connectors also provide support for registering the services as injectable service objects.

The Connectors provide out-of-the-box support for discovering many common services on Cloud Foundry. They also include the ability to use settings-based configuration so that developers can supply configuration settings at development and testing time but then have those settings be overridden when pushing the application to Cloud Foundry.

All connectors use configuration information from Cloud Foundry's `VCAP_SERVICES` environment variable to detect and configure the available services. This a Cloud Foundry standard that is used to hold connection and identification information for all service instances that have been bound to Cloud Foundry applications.

For more information on `VCAP_SERVICES`, see the Cloud Foundry [documentation](https://docs.cloudfoundry.org/).

>NOTE: Depending on your hosting environment, service instances you create for the purpose of exploring the Quick Starts on this page may have a cost associated.
