---
_disableBreadcrumb: true
_disableContribution: true
_showBackToBlogs: true

title: Announcing Steeltoe 3.2.2 with Experimental Kubernetes Service Bindings
description: Kubernetes Service Bindings
monikerRange: '== steeltoe-3.2.2'
type: markdown
date: 01/10/2023
uid: steeltoe-3-2-2-adds-kube-service-bindings.html
tags: ["new-release"]
author.name: David Tillman
---

# Announcing Steeltoe 3.2.2 with Experimental Kubernetes Service Bindings

We are delighted to announce the release of Steeltoe 3.2.2. In this release, the Steeltoe team has included experimental support for the [Kubernetes Service Binding](https://github.com/servicebinding/spec) specification.
The specification details a standard way in which Kubernetes platforms can expose secrets that enable application workloads to connect to external services.
You can review [version 1.0.0](https://servicebinding.io/spec/core/1.0.0/) of the specification to learn more.

The experimental implementation can be found in the [Steeltoe.Extensions.Configuration.Kubernetes.ServiceBinding](https://www.nuget.org/packages/Steeltoe.Extensions.Configuration.Kubernetes.ServiceBinding) package on [nuget.org](https://www.nuget.org/).
Please keep in mind that this is an experimental version and is likely to go through several rounds of changes over the coming months.

The implementation consists of a standard .NET configuration provider.  The service provider reads the Kubernetes service bindings and translates the information into a set of key value pairs which it then adds to the applications configuration.
Each of configuration keys is prefixed with `k8s:bindings:<binding-name>` where `<binding-name>` is the name associated with the Kubernetes service binding. The rest of the key represents the values associated with the binding.

Below is an example of the configuration keys associated with a PostgreSql service binding that are created by the Steeltoe configuration provider.

```csharp
    k8s:bindings:mypostgres:username=pgappuser
    k8s:bindings:mypostgres:uri=postgresql://pgappuser:sn3L007KAUC2i598hPtC3ftfgIqvF2@postgres-sample.postgres-service-instances:5432/postgres-sample
    k8s:bindings:mypostgres:type=postgresql
    k8s:bindings:mypostgres:provider=vmware
    k8s:bindings:mypostgres:port=5432
    k8s:bindings:mypostgres:password=sn3L007KAUC2i598hPtC3ftfgIqvF2
    k8s:bindings:mypostgres:host=postgres-sample.postgres-service-instances
    k8s:bindings:mypostgres:database=postgres-sample

```

To use the experimental provider, you need to add a reference to the [Steeltoe.Extensions.Configuration.Kubernetes.ServiceBinding](https://www.nuget.org/packages/Steeltoe.Extensions.Configuration.Kubernetes.ServiceBinding) NuGet package.
You can do this via the NuGet Package Manager or by opening your project's csproj file and adding the package directly, as shown below.
```xml
<ItemGroup>
...
    <PackageReference Include="Steeltoe.Extensions.Configuration.Kubernetes.ServiceBinding" Version="3.2.2" />
...
</ItemGroup>
```

To parse the Kubernetes service bindings and make them available in the application's configuration, you will also need to add the Steeltoe configuration provider to the ConfigurationBuilder.
This is done using the ConfigurationBuilder extension method AddKubernetesServiceBindings() shown below in an example from the Steeltoe PostgreSql Connector sample:

```csharp
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Steeltoe.Common.Hosting;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using Steeltoe.Extensions.Configuration.Kubernetes.ServiceBinding;
using Steeltoe.Management.Endpoint;

namespace PostgreSql
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)

                // Add CloudFoundry service bindings when app is running on CloudFoundry
                .AddCloudFoundryConfiguration()

                // Add Kubernetes service bindings when app is running on Kubernetes
                .ConfigureAppConfiguration(builder => builder.AddKubernetesServiceBindings())

                .AddAllActuators()
                .UseStartup<Startup>()
                .UseCloudHosting()
                .Build();
        }
    }
}

```

Running your application on a Kubernetes platform which supports the [Kubernetes Service Binding](https://github.com/servicebinding/spec) specification you should see configuration key/values appear in your applications configuration.

As an added feature, the Steeltoe team has added integration with the [Steeltoe Connectors](https://github.com/SteeltoeOSS/Steeltoe/tree/release/3.2/src/Connectors) library. This integration enables the Steeltoe Connectors library to pick up and use the configuration data built by the Kubernetes provider.

 This feature is not enabled by default. To enable it you must edit your `appsettings.json` file and add the following configuration.

```json
{
  "Steeltoe": {
    "Kubernetes": {
      "Bindings": {
        "Enable": true
      }
    }
  }
}
```

Take a look at the Steeltoe [PostgreSql Connector sample](https://github.com/SteeltoeOSS/Samples/tree/3.x/Connectors/src/PostgreSql) for more details.

The Steeltoe team is excited to hear your thoughts and opinions on this latest release. Remember you can join us on the Steeltoe Community call or reach out to the team directly on the [Steeltoe Slack](https://steeltoeteam.slack.com//).

