# An Example Using Tooling

In this sample we use Steeltoe Tooling to simplify the process of deploying the Steeltoe Sample SimpleCloudFoundy project to various targets.

## Initialize the SimpleCloudFoundry Project Dev Environment

Checkout Steeltoe Samples and navigate to the SimpleCloudFoundry project.

```sh
$ git clone https://github.com/SteeltoeOSS/Samples.git
$ cd Samples/Configuration/src/AspDotNetCore/SimpleCloudFoundry
```

Initialize Steeltoe Tooling.

```sh
$ st init
Initialized Steeltoe Developer Tools
```

## Add the Application and Service

Add application.
```sh
$ st add app SimpleCloudFoundry
Added app 'SimpleCloudFoundry'
```

Add service.
```sh
$ st add config-server myConfigServer
Added config-server service 'myConfigServer'
```

## Deploy

### Deploy Locally

Before deploying to a remote cloud, first run locally using Docker ...

```sh
$ st target docker
Docker ... Docker version 18.09.1, build 4c52b90
Docker host OS ... Docker for Mac
Docker container OS ... linux
Target set to 'docker'

$ st deploy
Deploying service 'myConfigServer'
Deploying app 'SimpleCloudFoundry'
```

... check status ...

```sh
$ st status
myConfigServer online
SimpleCloudFoundry online
```

... then navigate to application ...

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<http://localhost:8080/>

... finally, undeploy.

```sh
$ st undeploy
Undeploying app 'SimpleCloudFoundry'
Undeploying service 'myConfigServer'
```

### Deploy to Cloud Foundry

Deploy to Cloud Foundry ...

```sh
$ st target cloud-foundry
Cloud Foundry ... cf version 6.46.0+29d6257f1.2019-07-09
logged into Cloud Foundry ... yes
Target set to 'cloud-foundry'

$ st deploy
...
```

... and undeploy.

```sh
$ st undeploy
Undeploying app 'SimpleCloudFoundry'
Undeploying service 'myConfigServer'
```

### Deploy to Kubernetes

Deploy to Kubernetes ...

```sh
$ eval $(minikube docker-env)  # if using minikube's Docker

$ st target kubernetes
Kubernetes ... kubectl client version 1.15, server version 1.15
current context ... minikube
Target set to 'kubernetes'

$ st deploy
...
```

... and undeploy.

```sh
$ st undeploy
Undeploying app 'SimpleCloudFoundry'
Undeploying service 'myConfigServer'
```
