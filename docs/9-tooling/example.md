# An Example Using Tooling

In this sample we use Steeltoe Tooling to simplify the process of deploying the Steeltoe Sample Redis Connector project to various targets.

## Initialize the Redis Connector Project Dev Environment

Checkout Steeltoe Samples and navigate to the Redis Connector sample.

```sh
$ git clone https://github.com/SteeltoeOSS/Samples.git
$ cd Samples/Connectors/src/AspDotNetCore/Redis
```

Initialize Steeltoe Tooling.

```sh
$ st init
Initialized Steeltoe Developer Tools
```

## Add the Application and Service

Add application.
```sh
$ st add-app Redis
Added app 'Redis' (netcoreapp2.1/win10-x64)
```

Add service.
```sh
$ st add-service redis myRedisService
Added redis service 'myRedisService'
```

## Deploy

### Deploy Locally (Docker)

Before deploying to a remote cloud, first run locally using Docker ...

```sh
$ st target docker
Docker ... Docker version 18.09.1, build 4c52b90
Docker host OS ... Docker for Mac
Docker container OS ... linux
Target set to 'docker'
```

... create a DotNet Configuration file named `appsettings.Docker.json` ...

```sh
$ cat appsettings.Docker.json
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

```
$ st deploy
Deploying service 'myRedisService'
Deploying app 'Redis'
```

... check status ...

```sh
$ st status
myRedisService online
Redis online
```

... then navigate to application ...

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<http://localhost:8080/>

... finally, undeploy.

```sh
$ st undeploy
Undeploying app 'Redis'
Undeploying service 'myRedisService'
```

### Deploy to Cloud Foundry

Deploy to Cloud Foundry ...

```sh
$ st target cloud-foundry
Cloud Foundry ... cf version 6.46.0+29d6257f1.2019-07-09
logged into Cloud Foundry ... yes
Target set to 'cloud-foundry'

$ st deploy
Deploying service 'myRedisService'
Deploying app 'Redis'
```

... check status ...

```sh
$ st status
myRedisService online
Redis online
```

... and undeploy.

```sh
$ st undeploy
Undeploying app 'Redis'
Undeploying service 'myRedisService'
```

### Deploy to Kubernetes

If you haven't already, create a DotNet Configuration file named `appsettings.Docker.json` as in the Docker example above ...

... deploy to Kubernetes ...

```sh
$ eval $(minikube docker-env)  # if using minikube's Docker

$ st target kubernetes
Kubernetes ... kubectl client version 1.15, server version 1.15
current context ... minikube
Target set to 'kubernetes'

$ st deploy
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

```sh
$ st status
myRedisService online
Redis online
```

... enable port-forwarding to the app running in Kubernetes ...

```sh
# determine pod name
$ kubectl get pods --selector app=redis
NAME                     READY   STATUS    RESTARTS   AGE
redis-57fc6b5c85-9n4z9   1/1     Running   0          87s

# forward port 8080 to pod
$ kubectl port-forward redis-57fc6b5c85-9n4z9 8080:80
Forwarding from 127.0.0.1:8080 -> 80
Forwarding from [::1]:8080 -> 80
...
```

... then navigate to application ...

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<http://localhost:8080/>

... and undeploy.

```sh
$ st undeploy
Undeploying app 'Redis'
Undeploying service 'myRedisService'
```
