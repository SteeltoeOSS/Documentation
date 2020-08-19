# Steeltoe CLI

The Steeltoe Tooling CLI is a tool to assist developers in the code-test-debug lifecycle of DotNet projects.
The CLI uses the local Docker environment to run a project’s application and its dependencies (typically services).
Developers can attach a debugger to the running application, and since the project file system is mounted in the Docker container, code changes can be made and observed while the application is live.

## Getting Started

### Install the CLI

Steeltoe Tooling is a [DotNet Global Tools](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools) console executable named `st`.  Use `dotnet tool install` to install.

```sh
$ dotnet tool install --global --version 0.7.0 Steeltoe.Cli
```

### Add DotNet Global Tools to your PATH Variable

DotNet Global Tools are installed in an OS-dependent user directory.

|OS|Path|
|---|---|
|Windows|`%USERPROFILE%\.dotnet\tools`|
|OS X/Linux|`$HOME/.dotnet/tools`|

After adding of the above paths to your `PATH` env var, you can run the `st` executable.

```sh
$ st --version
0.7.0
```


## Using the CLI

The example app used below can be created using the [Steeltoe Initializr](https://start.steeltoe.io/).

To create in your environment:

```sh
$ mkdir MyRedisApp
$ curl https://start.steeltoe.io/starter.zip -o MyRedisApp.zip \
    -dprojectName=MyRedisApp \
    -dtargetFrameworkVersion=netcoreapp3.1 \
    -ddependencies=redis
$ unzip MyRedisApp.zip -d MyRedisApp
$ cd MyRedisApp
```

### show

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

Running `show` in our example apps shows the project apps is a `netcoreapp3.1` app that listens on port `5000` for HTTP requests.
The app depends on Redis which will be listening on port `5672`.

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

### run

```sh
Runs project in the local Docker environment

Usage: st run [options]

Options:
  -g|--generate-only  Only generate configuration files (don't run in Docker)
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

The command generates a [Docker Compose](https://docs.docker.com/compose/) file that will be used to run the project in Docker.

_Note that the current version of the CLI regenerates the file each time the command is run.  An upcoming version will only regenerate the file upon request._

A Docker container is created that mounts the project directory.  Any changes made to the app while the container is running will be evident in the running app.

If the project has service dependencies, Docker containers are created for each service.

Running `run` in our example app starts up the application and its dependent Redis service.

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

### stop

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

Running `stop` in our example app tears down the project's Docker containers.

```sh
$ st stop
stopping 'myredisapp' in Docker
> docker-compose down
Stopping myredisapp_redis_1      ...
Stopping myredisapp_redis_1      ... done
Removing myredisapp_myredisapp_1 ... done
Removing network myredisapp_default
```
