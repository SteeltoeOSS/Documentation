---
uid: labs/modernize-dotnet/exercise1
_disableContribution: true
_disableToc: true
_disableFooter: true
_homePath: "./"
_disableNav: true
---

[exercise-1-link]: exercise1.md
[exercise-2-link]: exercise2.md

|[<< Prev][exercise1-link]||
|:--|--:|

# Exercise 2

## Goal

Switch InProc session state to redis-backed with minimal code changes

## Expected Results

View counter increments with every request, regardless of how many instances are running

## Get Started

### Scale the frontend 
Let's return our attention to the frontend. In powershell scale the application from a single instance to two. 

```
cf scale -i 2 <front-end-app-name> 
```

Visit the application in a browser and hit refresh several times. Observe that the counter isn't incrementing on every request. This is because each instance is storing its own session state in memory. Our session-level view counter is broken!

### Fixing things with Redis

Now we'll fix the application by externalizing the session to Redis. We'll use a community-maintained buildpack to deal with the discovery and configuration of the Redis service and session provider. We won't have to write any code to make this happen! 

Under normal conditions you'd have to create an instance of the `rediscloud` service from the Cloud Foundry marketplace. This lab provides you with an instance, however.
You can verify its existance with:

```
cf services 
```

and you'd expect to see an output like this:

```
name                   service          plan        bound apps                 last operation     broker           upgrade available
autoscale-playground   app-autoscaler   standard                               create succeeded   app-autoscaler
session                p-redis          shared-vm   wsfe0-step2, wsfe1-step2   create succeeded   p-redis
```

Ensure that you see a service named `session` of type `rediscloud`. In practice, the name isn't important, but `session` is what we'll use for this exercise.

Add a nuget reference to  `Microsoft.Web.RedisSessionStateProvider` and rebuild the application by right-clicking on the `WorkshopFrontEnd` project and selecting "Manage Nuget Packages". 

* Insert screenshot here *

Then select "Browse" at the top of the screen and search for `Microsoft.Web.RedisSessionStateProvider`.  

* Insert screenshot here *

Select the entry of the appropriate name (it should be at the top of the search results) and then select `Install`.

* Insert screenshot here *

Accept the license and rebuild the application.

Now you can re-publish the application to the Folder profile by right-clicking the `WorkshopFrontEnd` project, selecting `Publish`, and clicking the `Publish` button on the right.

Bind the application to redis by adding a `services` entry that matches the name of the redis service we observed earlier. For this lab we named it "session". 

```
---
applications:
- instances: 2
  memory: 384M 
  path: bin/app.publish/
  buildpacks:
    - hwc_buildpack
  services:
    - session
```

Add the redis session buildpack by putting the link `https://github.com/cloudfoundry-community/redis-session-aspnet-buildpack/releases/download/v1.0.5/Pivotal.Redis.Aspnet.Session.Buildpack-win-x64-1.0.5.zip` in a list entry of the `buildpacks` element.

```
---
applications:
- memory: 384M 
  instances: 2
  path: bin/app.publish/
  buildpacks:
    - https://github.com/cloudfoundry-community/redis-session-aspnet-buildpack/releases/download/v1.0.5/Pivotal.Redis.Aspnet.Session.Buildpack-win-x64-1.0.5.zip 
    - hwc_buildpack
  services:
    - session
```

Push the web app to cloud foundry targetting the Windows stack

```
cf push <web-app-name> -s windows
```

Now visit the "ViewCounter" in a browser again. Again, the first few requests might be slow while your application is waking up. See that the view counts are incerementing correctly now?

|[<< Prev][exercise1-link]||
|:--|--:|
