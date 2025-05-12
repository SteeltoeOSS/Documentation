# Steeltoe CLI
Steeltoe CLI simplifies the process of running and deploying Steeltoe applications.  With this tooling you can, with minimal commands, run your application locally while also being able to easily deploy to Kubernetes or Cloud Foundry.

There are 3 basic steps when using Steeltoe Tooling:

1. Initialize Steeltoe Tooling configuration defaults
1. Add an application and its services to the configuration
1. Deploy the application and its services locally, to Kubernetes, or to Cloud Foundry

## Getting Started

### Install Steeltoe Tooling

Steeltoe Tooling is a [DotNet Global Tool](https://docs.microsoft.com/dotnet/core/tools/global-tools) console executable named `st`.  Use `dotnet tool install` to install.

```shell
dotnet tool install --global --version 0.5.0 Steeltoe.Cli
```

### Add the DotNet Global Tool to your PATH variable

The DotNet Global Tool is installed in an OS-dependent user directory.

|OS|Path|
|---|---|
|Windows|`%USERPROFILE%\.dotnet\tools`|
|OS X/Linux|`$HOME/.dotnet/tools`|

After adding the above paths to your `PATH` env var, you can run the `st` executable.

```shell
st --version
```

```text
1.0.0-m1
```

## Usage of Tooling CLI

### Initialization

The `st init` command initializes your project for Steeltoo Tooling.  Enter your project directory and run the command.

```shell
cd MyProject
st init
```

```text
Initialized Steeltoe Developer Tools
```

Running `st init` creates the file `steeltoe.yaml` in your project directory.  Steeltoe Tooling uses this file to determine what application and services are to be deployed and where to deploy them.

### Application and Services

The `st add-app`, `st add-service`, and `st remove` commands add and remove applications and services to the Steeltoe Tooling configuration.

#### Adding an Application

Running `st add-app <appname>` adds an application to the configuration. _appname_ must correspond to a `.csproj` file of the same name.

```shell
st add-app MyProject
```

```text
Added app 'MyProject'
```

#### Adding a Service

Running `st add-service <svctype> <svcname>` adds a service to the configuration.

Supported service types include:

|Service Type|Description|
|---|---|
|config-server|Cloud Foundry Config Server|
|eureka-server|Netflix Eureka Server|
|hystrix-dashboard|Netflix Hystrix Dashboard|
|mssql|Microsoft SQL Server|
|mysql|MySQL Server|
|postgresql|PostgreSQL Server|
|rabbitmq|RabbitMQ Message Broker|
|redis|Redis In-Memory Datastore|
|zipkin|Zipkin Tracing Collector and UI|

```shell
st add-service config-server MyConfigServer
```

```text
Added config-server service 'MyConfigServer'
```

#### Removing an Application or Service

Running `st remove <name>` removes the named application or service.

```shell
st remove myConfigServer
```

```text
Removed config-server service 'myConfigServer'
```

### Deployment

#### Setting the Target

Running `st target <target>` targets a deployment.

Supported targets include:

|Target|Deployment Destination|
|---|---|
|cloud-foundry|current Cloud Foundry space|
|docker|local Docker host|
|kubernetes|current Kubernetes context|

```shell
st target kubernetes
```

```text
Kubernetes ... kubectl client version 1.14, server version 1.14
current context ... docker-desktop
Target set to 'kubernetes'
```

#### Deploying

Running `st deploy` deploys an application and its services to the current target.

```shell
st deploy
```

```text
Deploying service 'myConfigServer'
Waiting for service 'myConfigServer' to come online (1)
Waiting for service 'myConfigServer' to come online (2)
Deploying app 'SimpleCloudFoundry'
```

#### Undeploying

Running `st undeploy` undeploys an application and its services from the current target.

```shell
st undeploy
```

```text
Undeploying app 'SimpleCloudFoundry'
Undeploying service 'myConfigServer'
```

## An Example Using Tooling

In this sample we use Steeltoe Tooling to simplify the process of deploying the Steeltoe Sample Redis Connector project to various targets.

### Initialize the Redis Connector Project Dev Environment

Checkout Steeltoe Samples and navigate to the Redis Connector sample.

```shell
git clone https://github.com/SteeltoeOSS/Samples.git
cd Samples/Connectors/src/AspDotNetCore/Redis
```

Initialize Steeltoe Tooling.

```shell
st init
```

```text
Initialized Steeltoe Developer Tools
```

### Add the Application and Service

Add application.

```shell
st add-app Redis
```

```text
Added app 'Redis' (netcoreapp3.1/win10-x64)
```

Add service.

```shell
st add-service redis myRedisService
```

```text
Added redis service 'myRedisService'
```

### Deploy

#### Deploy Locally (Docker)

Before deploying to a remote cloud, first run locally using Docker ...

```shell
st target docker
```

```text
Docker ... Docker version 18.09.1, build 4c52b90
Docker host OS ... Docker for Mac
Docker container OS ... linux
Target set to 'docker'
```

... create a DotNet Configuration file named `appsettings.Docker.json` ...

```bash
cat appsettings.Docker.json
{
  "redis": {
    "client": {
      "host": "myRedisService",
      "port": "6379"
    }
  }
}
```

... deploy to Docker ...

```bash
st deploy
```

```text
Deploying service 'myRedisService'
Deploying app 'Redis'
```

... check status ...

```shell
st status
```

```text
myRedisService online
Redis online
```

... then navigate to application ...

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<http://localhost:8080/>

... finally, undeploy.

```shell
st undeploy
```

```text
Undeploying app 'Redis'
Undeploying service 'myRedisService'
```

#### Deploy to Cloud Foundry

Deploy to Cloud Foundry ...

```shell
st target cloud-foundry
```

```text
Cloud Foundry ... cf version 6.46.0+29d6257f1.2019-07-09
logged into Cloud Foundry ... yes
Target set to 'cloud-foundry'
```

```shell
st deploy
```

```text
Deploying service 'myRedisService'
Deploying app 'Redis'
```

... check status ...

```shell
st status
```

```text
myRedisService online
Redis online
```

... and undeploy.

```shell
st undeploy
```

```text
Undeploying app 'Redis'
Undeploying service 'myRedisService'
```

#### Deploy to Kubernetes

If you haven't already, create a DotNet Configuration file named `appsettings.Docker.json` as in the Docker example above ...

... deploy to Kubernetes ...

```bash
eval $(minikube docker-env)  # if using minikube's Docker
```

```shell
st target kubernetes
```

```text
Kubernetes ... kubectl client version 1.15, server version 1.15
current context ... minikube
Target set to 'kubernetes'
```

```shell
st deploy
```

```text
Deploying service 'myRedisService'
Waiting for 'myRedisService' to transition to online (1)
Waiting for 'myRedisService' to transition to online (2)
Waiting for 'myRedisService' to transition to online (3)
Deploying app 'Redis'
Waiting for 'Redis' to transition to online (1)
Waiting for 'Redis' to transition to online (2)
Waiting for 'Redis' to transition to online (3)
Waiting for 'Redis' to transition to online (4)
```

... check status ...

```shell
st status
```

```text
myRedisService online
Redis online
```

... enable port-forwarding to the app running in Kubernetes ...

Determine pod name:

```shell
kubectl get pods --selector app=redis
```

```text
NAME                     READY   STATUS    RESTARTS   AGE
redis-57fc6b5c85-9n4z9   1/1     Running   0          87s
```

Forward port 8080 to pod:

```shell
kubectl port-forward redis-57fc6b5c85-9n4z9 8080:80
```

```text
Forwarding from 127.0.0.1:8080 -> 80
Forwarding from [::1]:8080 -> 80
...
```

... then navigate to application ...

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<http://localhost:8080/>

... and undeploy.

```shell
st undeploy
```

```text
Undeploying app 'Redis'
Undeploying service 'myRedisService'
```
