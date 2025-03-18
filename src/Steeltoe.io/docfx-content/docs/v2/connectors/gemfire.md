# Apache Geode/GemFire

>Note: This feature is available in version 2.3.0+

[VMware Tanzu GemFire](https://tanzu.vmware.com/gemfire) is VMware's distribution of [Apache Geode](https://geode.apache.org/). This connector was built for using GemFire in an application using the [GemFire Native Client](https://gemfire-native.docs.pivotal.io/100/gemfire-native-client/about-client-users-guide.html) on Cloud Foundry. It has not been extensively tested under other deployment configurations, but will be best-effort supported for other situations as well.

>WARNING: The GemFire Native Client currently only supports 64 bit applications running on Windows. See [GemFire Native Client System Requirements](https://gemfire-native.docs.pivotal.io/100/gemfire-native-client/system_requirements.html) for more.

## Usage

To use this connector:

1. Create a GemFire service instance and bind it to your application.
1. Optionally, configure GemFire client settings.
1. Add the Steeltoe Cloud Foundry config provider to you `ConfigurationBuilder`.
1. Add GemFire classes to your DI container.

### Get GemFire Driver

Follow the instructions in the [GemFire Native Client documentation](https://gemfire-native.docs.pivotal.io/100/gemfire-native-client/install-upgrade-native.html) for instructions on downloading the driver and getting started with general driver usage.

>TIP: Should you wish to avoid committing the driver to source, you are free to copy the [script](https://github.com/SteeltoeOSS/steeltoe/blob/2.x/src/Connectors/EnableGemFire.ps1) that Steeltoe's CI process uses in your own pipelines. You will need a [legacy API token](https://network.pivotal.io/docs/api#how-to-authenticate) for the script to complete.

### Add NuGet Reference

[Add a reference to the appropriate Steeltoe Connector NuGet package](usage.md#add-nuget-references)

### Configure Settings

The GemFire client is highly configurable, beyond the scope of this documentation.
Steeltoe does not interact with `geode.properties` or `cache.xml` files, but any settings found under `gemfire:client:properties` in your application's configuration are applied to the `CacheFactory`.
Steeltoe attempts to map the properties node in your configuration to a `Dictionary<string, string>` and then applies each entry to GemFire via the `Set` method on the CacheFactory.

For example, if you want to set the log-level and connection timeout, use the following in your `application.json` file:

```json
{
  "gemfire": {
    "client": {
      "properties": {
        "log-level": "fine",
        "connect-timeout": "100ms"
      }
    }
  }
}
```

Refer to the [GemFire documentation](https://gemfire-native.docs.pivotal.io/100/geode-native-client/configuring/sysprops.html) for a more complete list of settings to configure.

### Cloud Foundry

To use GemFire Cloud Cache on Cloud Foundry:

1. Create a service instance
1. Bind the instance to your application via either a manifest file or the CLI (shown below)
1. Create region (If the application is unable to do so automatically)

```bash
# Create CloudCache service instance
cf create-service p-cloudcache dev-plan myPCCService

# Bind service to `myApp`
cf bind-service myApp myPCCService

# Restage the app to pick up change
cf restage myApp
```

If your application fails to [programmatically create regions](https://gemfire-native.docs.pivotal.io/100/geode-native-client/regions/regions.html#programmatic-region-creation), use the [gfsh CLI](https://gemfire.docs.pivotal.io/98/gemfire/tools_modules/gfsh/chapter_overview.html) to create it. You will need the gfsh url and the cluster operator credentials provided in the service binding:

```json
"p-cloudcache": [{
    ...
      "urls": {
        "gfsh": "https://cloudcache-serviceguid.run.pcfone.io/gemfire/v1",
        ...
      },
      "users": [{
          "password": "********",
          "roles": [ "cluster_operator" ],
          "username": "********"
        ...
```

Use gfsh to connect to the cluster and create the region:

```bash
gfsh>connect --url=https://cloudcache-someguid.run.pcfone.io/gemfire/v1 --user=cluster_operator_****** --password=******

Successfully connected to: GemFire Manager HTTP service @ https://cloudcache-someguid.run.pcfone.io/gemfire/v1

Cluster-0 gfsh>create region --name=SteeltoeDemo --type=PARTITION
                     Member                      | Status
------------------------------------------------ | ------------------------------------------------------------------------------------
cacheserver-71461c75-ba87-4207-8d6d-c84be814a601 | Region "/SteeltoeDemo" created on "cacheserver-71461c75-ba87-4207-8d6d-c84be814a601"
```
