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
[buildpacks-link]: buildpacks.md

[modernize-redis-servicelist]: ~/labs/images/modernize-redis-servicelist.png "Get a list of services running in your space"
[modernize-frontend-addnuget]: ~/labs/images/modernize-frontend-addnuget.png "Add a nuget reference "
[modernize-frontend-selectnuget]: ~/labs/images/modernize-frontend-selectnuget.png "Select nuget reference for Microsoft.Web.RedisSessionStateProvider"
[modernize-frontend-nugetinstall]: ~/labs/images/modernize-frontend-nugetinstall.png "Install Microsoft.Web.RedisSessionStateProvider"
[modernize-frontend-publish]: ~/labs/images/modernize-frontend-publish.png "Publish the frontend"
[modernize-frontend-publish2]: ~/labs/images/modernize-frontend-publish2.png "Publish the frontend"

|[<< Previous Exercise][exercise-1-link]|[Next Buildpacks >>][buildpacks-link]|
|:--|--:|

# Exercise 2

## Goal

Switch InProc session state to redis-backed with minimal code changes

## Expected Results

View counter increments with every request, regardless of how many instances are running

## Get Started

### Scale the frontend 
Let's return our attention to the frontend. In powershell scale the application from a single instance to two. 

```powershell
cf scale -i 2 <front-end-app-name> 
```

Visit the `ViewCounter` in a browser and hit refresh several times. Observe that the counter isn't incrementing on every request. This is because each instance is storing its own session state in memory. Our session-level view counter is broken!

## Fixing things with Redis

Now we'll fix the application by externalizing the session to Redis. We'll use a community-maintained buildpack to deal with the discovery and configuration of the Redis service and session provider. We won't have to write any code to make this happen! 

### Verify our Redis service is running

Under normal conditions you'd have to create an instance of the `rediscloud` service from the Cloud Foundry marketplace. This lab provides you with an instance, however.
You can verify its existance with:

```powershell
cf services 
```

and you'd expect to see an output like this:

<br><br><br>
![modernize-redis-servicelist]
<br><br><br>

Ensure that you see a service named `session` of type `rediscloud`. In practice, the name isn't important, but `session` is what we'll use for this exercise. For this lab you only have to verify that the service exists. 

### Add a nuget reference to the Redis Session Provider

Add a nuget reference to  `Microsoft.Web.RedisSessionStateProvider` and rebuild the application by right-clicking on the `WorkshopFrontEnd` project and selecting "Manage Nuget Packages". 

<br><br><br>
![modernize-frontend-addnuget]
<br><br><br>

Then select "Browse" at the top of the screen and search for `Microsoft.Web.RedisSessionStateProvider`.  

<br><br><br>
![modernize-frontend-selectnuget]
<br><br><br>

Select the entry of the appropriate name (it should be at the top of the search results) and then select `Install`.

<br><br><br>
![modernize-frontend-nugetinstall]
<br><br><br>

Accept the license and rebuild the application.

### Publish

Now you can re-publish the frontend application to the Folder profile by right-clicking the `WorkshopFrontEnd` project, selecting `Publish`, and clicking the `Publish` button on the right.

<br><br><br>
![modernize-frontend-publish]
<br><br><br>

Ensure the `Folder` profile is selected and click `Install`.

<br><br><br>
![modernize-frontend-publish2]
<br><br><br>

### Update manifest

Bind the application to redis by adding a `services` entry to your frontend's manifest that matches the name of the redis service we observed earlier. For this lab we named it "session". 

```yaml
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

```yaml
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

### Deploy

Push the web app to cloud foundry targetting the Windows stack

```powershell
cd c:\Users\WorkshopStudent\src\Workshop\WorkshopFrontEnd
cf push <web-app-name> -s windows
```

### See your results

Now visit the `ViewCounter` in a browser again. Again, the first few requests might be slow while your application is waking up. See that the view counts are incerementing correctly now?

### Clean up

Please delete your instances when you're done. Note the `-r` switch. That cleans up your routes.

```powershell
cf delete -r <front-end-app-name>
cf delete -r <api-app-name> 
```

|[<< Previous Exercise][exercise-1-link]|[Next Buildpacks >>][buildpacks-link]|
|:--|--:|
