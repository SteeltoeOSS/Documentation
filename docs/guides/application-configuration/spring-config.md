---
uid: guides/application-configuration/spring-config
title: Spring Config Provider
tags: []
_disableFooter: true
_hideTocVersionToggle: true
---

> [!NOTE]
> This guide applies to Steeltoe v3. Please [open an issue](https://github.com/SteeltoeOSS/Documentation/issues/new/choose) if you'd like to help update the content for Steeltoe v4.

## App Configuration with a Spring Config Server

This tutorial takes you through setting up a .NET Core application that gets configuration values from a Spring Config Server.

> [!NOTE]
> For more detailed examples, please refer to the [Simple (Config Server)](https://github.com/SteeltoeOSS/Samples/tree/3.x/Configuration/src/Simple) project in the [Steeltoe Samples Repository](https://github.com/SteeltoeOSS/Samples/tree/3.x).

First, **create a GitHub repository** to hold config values.

1. Navigate to [GitHub](https://github.com) and either login or create a new account
1. Create and initialize a new **public** repository, named `Spring-Config-Demo`
1. Once created, note the url of the new repo

Next, **add a config file** to the repository.

1. Create a new file in the repo named `my-values.yml`
1. Add the following to the file

   ```yml
   Value1: some-val
   Value2: another-val
   ```

1. Commit the new file to the repo

Then, **start a config server instance** using the [Steeltoe dockerfile](https://github.com/steeltoeoss/dockerfiles).

```bash
docker run -p 8888:8888 steeltoeoss/config-server --spring.cloud.config.server.git.default-label=main --spring.cloud.config.server.git.uri=<NEW_REPO_URL>
```

**Note:** By default, the config server assumes the branch name to be `master`. The `spring.cloud.config.server.git.default-label` switch above changes that to `main`.

Next, **create a .NET Core WebAPI** that retrieves values from the Spring Config instance.

1. Create a new ASP.NET Core WebAPI app with the [Steeltoe Initializr](https://start.steeltoe.io)
1. Name the project "SpringConfigExample"
1. Add the "Spring Cloud Config Server" dependency
1. Click **Generate** to download a zip containing the new project
1. Extract the zipped project and open in your IDE of choice
1. Set the instance address and name in **appsettings.json**

   ```json
   {
     "spring": {
       "application": {
         "name": "my-values"
       }
     }
   }
   ```

   > [!NOTE]
   > For the application to find its values in the git repo, the spring:application:name and the yaml file name **must** match. In this example `my-values` matched.

**Run** the application

# [.NET cli](#tab/cli)

```powershell
dotnet run <PATH_TO>\SpringConfigExample.csproj
```

Navigate to the endpoint (you may need to change the port number) [http://localhost:5000/api/values](http://localhost:5000/api/values)

# [Visual Studio](#tab/vs)

1. Choose the top _Debug_ menu, then choose _Start Debugging (F5)_. This should bring up a browser with the app running
1. Navigate to the endpoint (you may need to change the port number) [http://localhost:8080/api/values](http://localhost:8080/api/values)

---

Once the app loads you will see the two values output.

`["some-val","another-val"]`
