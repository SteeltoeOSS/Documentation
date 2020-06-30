# Steeltoe CLI

The Steeltoe Tooling command-line interface (CLI) is a tool to assist developers in the code-test-debug lifecycle of DotNet projects.
The CLI uses the local Docker environment to run a project’s application and its dependencies (typically services).
Developers can attach a debugger to the running application, and, since the project file system is mounted in the Docker container, code changes can be made and observed while the application is live.

## Getting Started

To get started, you need to do the following:

1. <a href="#steeltoe-dev-tools-pre-requisites">Set up Pre-Requisites</a>
1. <a href="#steeltoe-dev-tools-install-cli">Install the CLI</a>
1. <a href="#steeltoe-dev-tools-add-dotnet-global-tools-path-variable">Add DotNet Global Tools to your PATH Variable</a>

<a name="steeltoe-dev-tools-pre-requisites"></a>
### Set up Pre-Requisites

See [docker-compose](https://docs.docker.com/compose/)

<a name="steeltoe-dev-tools-install-cli"></a>
### Install the CLI

Steeltoe Tooling is a [DotNet Global Tools](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools) console executable named `st`.  Use `dotnet tool install` to install.

```sh
$ dotnet tool install -g Steeltoe.Cli --version 0.7.1-2785 --add-source https://www.myget.org/F/steeltoedev/api/v3/index.json
```

<a name="steeltoe-dev-tools-add-dotnet-global-tools-path-variable"></a>
### Add DotNet Global Tools to your PATH Variable

DotNet Global Tools are installed in an OS-dependent user directory

* Windows: `%USERPROFILE%\.dotnet\tools`
* OS X/Linux: `$HOME/.dotnet/tools`

After adding of the path to your `PATH` environment variable, you can run the `st` executable:

```sh
$ st --version
0.7.1 (build 2785 -> https://dev.azure.com/SteeltoeOSS/Steeltoe/_build/results?buildId=2785)
```


## Using the CLI

You can create the sample application used by using the [Steeltoe Initializr](https://start.steeltoe.io/).

To create it in your environment:

```sh
$ mkdir MyRedisApp
$ curl https://start.steeltoe.io/starter.zip -o MyRedisApp.zip \
    -dprojectName=MyRedisApp \
    -dtargetFrameworkVersion=netcoreapp3.1
    -ddependencies=redis
$ unzip MyRedisApp.zip -d MyRedisApp
$ cd MyRedisApp
```

### Using show

The following example shows how to use the `show` command:

```sh
Displays project details

Usage: st show [options]

Options:
  -?|-h|--help  Show help information

Overview:
  *** under construction ***

Examples:
  Show the details of the project in the current directory:
  $ st show
```

The `show` command displays the deployment structure of the project.
This structure is subsequently used by the `run` command to start the project and any dependent services.

Running `show` in our example application shows that the project application is a `netcoreapp3.1` application that listens on port `5000` for HTTP requests.
The application depends on Redis to be listening on port `5672`.

```sh
$ st show
configuration: MyRedisApp
project:
  name: myredisapp
  file: MyRedisApp.csproj
  framework: netcoreapp3.1
  image: mcr.microsoft.com/dotnet/core/sdk:3.1
  protocols:
  - name: http
    port: 5000
  services:
  - name: redis
    image: steeltoeoss/redis-amd64-linux:4.0.11
    port: 5672
```

### Using run

The following example shows how to use the `run` command:

```sh
Runs project in the local Docker environment

Usage: st run [options]

Options:
  -g|--generate-only  Only generate configuration files (do not run in Docker)
  -?|-h|--help        Show help information

Overview:
  Starts the project application and its dependencies in the local Docker environment.

Examples:
  Run the default configuration:
  $ st run

See Also:
  stop
```

The `run` command runs the project in your local Docker environment.

The command generates a [Docker Compose](https://docs.docker.com/compose/) file that you can use to run the project in Docker.

>NOTE: The current version of the CLI regenerates the file each time the command is run.  An upcoming version will regenerate the file only upon request.

A Docker container is created to mount the project directory. Any changes made to the application while the container is running are evident in the running app.

If the project has service dependencies, Docker containers are created for each service.

Running `run` in our example app starts up the application and its dependent Redis service:

```sh
$ st run
running 'myredisapp' in Docker
> docker-compose up --build
Creating network "myredisapp_default" with the default driver
Building myredisapp
Step 1/4 : FROM mcr.microsoft.com/dotnet/core/sdk:3.1
 ---> d9d656b4ceb2
Step 2/4 : WORKDIR /myredisapp
 ---> Running in c04db49ee23d
Removing intermediate container c04db49ee23d
 ---> c3cff79c1a51
Step 3/4 : RUN mkdir -p /usr/local/share/dotnet/sdk/NuGetFallbackFolder
 ---> Running in 265a6a397765
Removing intermediate container 265a6a397765
 ---> da59d48eb855
Step 4/4 : CMD ["dotnet", "watch", "run", "--urls", "http://0.0.0.0:5000"]
 ---> Running in b3eced1e83ba
Removing intermediate container b3eced1e83ba
 ---> 07943f359520

Successfully built 07943f359520
Successfully tagged myredisapp_myredisapp:latest
Pulling redis (steeltoeoss/redis-amd64-linux:4.0.11)...
4.0.11: Pulling from steeltoeoss/redis-amd64-linux
Digest: sha256:2c1d4ea5491a726d944b23f18082bd6dfe41d66d94a9e540f074cbb9f94ab8e3
Status: Downloaded newer image for steeltoeoss/redis-amd64-linux:4.0.11


Creating myredisapp_myredisapp_1 ... done
Attaching to myredisapp_redis_1, myredisapp_myredisapp_1
redis_1       | 1:C 23 Apr 14:39:51.076 # oO0OoO0OoO0Oo Redis is starting oO0OoO0OoO0Oo
redis_1       | 1:C 23 Apr 14:39:51.076 # Redis version=4.0.11, bits=64, commit=00000000, modified=0, pid=1, just started
redis_1       | 1:C 23 Apr 14:39:51.076 # Warning: no config file specified, using the default config. In order to specify a config file use redis-server /path/to/redis.conf
redis_1       | 1:M 23 Apr 14:39:51.078 * Running mode=standalone, port=6379.
redis_1       | 1:M 23 Apr 14:39:51.078 # WARNING: The TCP backlog setting of 511 cannot be enforced because /proc/sys/net/core/somaxconn is set to the lower value of 128.
redis_1       | 1:M 23 Apr 14:39:51.078 # Server initialized
redis_1       | 1:M 23 Apr 14:39:51.082 # WARNING you have Transparent Huge Pages (THP) support enabled in your kernel. This will create latency and memory usage issues with Redis. To fix this issue run the command 'echo never > /sys/kernel/mm/transparent_hugepage/enabled' as root, and add it to your /etc/rc.local in order to retain the setting after a reboot. Redis must be restarted after THP is disabled.
redis_1       | 1:M 23 Apr 14:39:51.082 * Ready to accept connections
myredisapp_1  | watch : Polling file watcher is enabled
myredisapp_1  | watch : Started
myredisapp_1  | Hosting environment: Development
myredisapp_1  | Content root path: /myredisapp
myredisapp_1  | Now listening on: http://0.0.0.0:5000
myredisapp_1  | Application started. Press Ctrl+C to shut down.
```

### Using stop

The following example shows how to use the `stop` command:

```sh
Stops project running in the local Docker environment

Usage: st stop [options]

Options:
  -?|-h|--help  Show help information

Overview:
  Stops the project application and its dependencies in the local Docker environment.

Examples:
  Stop the running project:
  $ st stop

See Also:
  run
```

Running `stop` stops the project running in your local Docker environment.

Running `stop` in our example application tears down the project's Docker containers:

```sh
$ st stop
stopping 'myredisapp' in Docker
> docker-compose down
Stopping myredisapp_redis_1      ...
Stopping myredisapp_redis_1      ... done
Removing myredisapp_myredisapp_1 ... done
Removing network myredisapp_default
```
