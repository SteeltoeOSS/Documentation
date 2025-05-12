# InitializrService

The InitializrService provides four REST/HTTP endpoints:

* `api/`
* `api/about`
* `api/config`
* `api/project`

## `api/`

`api/` accepts `GET` requests and returns a help document.
The document includes available parameters (and their defaults) and dependencies, plus CLI samples.

### Sample: view help doc

```bash
http -p b https://start.steeltoe.io/api/
```

```text
...
This service generates quickstart projects that can be easily customized.
Possible customizations include a project's dependencies and .NET target framework.

The URI templates take a set of parameters to customize the result of a request.
+-----------------+-----------------------+----------------------------+
| Parameter       | Description           | Default value              |
+-----------------+-----------------------+----------------------------+
| name            | project name          | Sample                     |
| applicationName | application name      | SampleApplication          |
...
```

## `api/about`

`api/about` accepts `GET` requests and returns the InitializrService "About" information.

### Sample: View "About" document

```bash
http -p b https://start.steeltoe.io/api/about
{
    "commit": "381bbd2a1e30d621ed6ad4a07790955447ffe468",
    "name": "Steeltoe.InitializrApi",
    "url": "https://github.com/SteeltoeOSS/InitializrApi/",
    "vendor": "SteeltoeOSS/VMware",
    "version": "0.8.0"
}
```

## `api/config`

`api/config` accepts `GET` requests and returns InitializrService configuration.
The returned document includes *all* configuration, which can include superfluous details.
Sub-endpoints are available, allowing more targeted responses.

`api/config/projectMetadata` can be used by smart clients, such as the InitializrWeb, to assist in creating user interfaces.

The following endpoints can be used by CLI users to browse project configuration options:

* `api/config/archiveTypes`
* `api/config/dependencies`
* `api/config/dotNetFrameworks`
* `api/config/dotNetTemplates`
* `api/config/languages`
* `api/config/steeltoeVersions`

### Sample: List available Steeltoe versions

```bash
http -p b https://start.steeltoe.io/api/config/steeltoeVersions
[
    {
        "id": "2.4.4",
        "name": "Steeltoe 2.4.4 Maintenance Release"
    },
    {
        "id": "2.5.1",
        "name": "Steeltoe 2.5.1 Maintenance Release"
    },
    {
        "id": "3.0.1",
        "name": "Steeltoe 3.0.1 Maintenance Release"
    }
]
```

### Sample: List available dependency IDs

```bash
http https://start.steeltoe.io/api/config/dependencies | jq '.[] .values[] .id' | sort
```

```text
"actuator"
"amqp"
"azure-spring-cloud"
"cloud-foundry"
"config-server"
"data-mongodb"
"data-redis"
"docker"
"dynamic-logger"
"eureka-client"
"mysql"
"mysql-efcore"
"oauth"
"placeholder"
"postgresql"
"postgresql-efcore"
"random-value"
"sqlserver"
```

## `api/project`

`api/project` accepts `GET` and `POST` requests and returns a project as an archive.

Projects are configured using HTTP parameters, such as `name` for the project name and `steeltoeVersion` for the Steeltoe version.
The parameter `dependencies` is different than other parameters in that it is set to a comma-separated list of dependency IDs.

> [!TIP]
> To get a list of parameters and dependencies, send a `GET` request to `api/`.

### Sample: Generate a .NET 8 project with actuator endpoints and a Redis backend

```bash
http https://start.steeltoe.io/api/project dotNetFramework=net8.0 dependencies==actuators,redis -d
```
