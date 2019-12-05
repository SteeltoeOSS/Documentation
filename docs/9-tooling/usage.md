# Usage of Tooling CLI

## Initialization

The `st init` command initializes your project for Steeltoo Tooling.  Enter your project directory and run the command.

```sh
$ cd MyProject
$ st init
Initialized Steeltoe Developer Tools
```

Running `st init` creates the file `steeltoe.yaml` in your project directory.  Steeltoe Tooling uses this file to determine what application and services are to be deployed and where to deploy them.

## Application and Services

The `st add-app`, `st add-service`, and `st remove` commands add and remove applications and services to the Steeltoe Tooling configuration.

### Adding an Application

Running `st add-app <appname>` adds an application to the configuration. _appname_ must correspond to a `.csproj` file of the same name.

```sh
$ st add-app MyProject
Added app 'MyProject'
```

### Adding a Service

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

```sh
$ st add-service config-server MyConfigServer
Added config-server service 'MyConfigServer'
```

### Removing an Application or Service

Running `st remove <name>` removes the named application or service.

```sh
$ st remove myConfigServer
Removed config-server service 'myConfigServer'
```

## Deployment

### Setting the Target

Running `st target <target>` targets a deployment.

Supported targets include:

|Target|Deployment Destination|
|---|---|
|cloud-foundry|current Cloud Foundry space|
|docker|local Docker host|
|kubernetes|current Kubernetes context|

```sh
$ st target kubernetes
Kubernetes ... kubectl client version 1.14, server version 1.14
current context ... docker-desktop
Target set to 'kubernetes'
```

### Deploying

Running `st deploy` deploys an application and its services to the current target.

```sh
$ st deploy
Deploying service 'myConfigServer'
Waiting for service 'myConfigServer' to come online (1)
Waiting for service 'myConfigServer' to come online (2)
Deploying app 'SimpleCloudFoundry'
```

### Undeploying

Running `st undeploy` undeploys an application and its services from the current target.

```sh
$ st undeploy
Undeploying app 'SimpleCloudFoundry'
Undeploying service 'myConfigServer'
```
