---
uid: labs/modernize-dotnet/exercise1
_disableContribution: true
_disableToc: true
_disableFooter: true
_homePath: "./"
_disableNav: true
---

[exercise-1-link]: exercise1.md
[buildpacks-link]: buildpacks.md
[exercise-2-link]: exercise2.md

|[<< Prev][buildpacks-link]||
|:--|--:|

# Exercise 2

## Goal

XXXXXXXX

## Expected Results

XXXXXXXX

## Get Started

Now we'll fix the application by externalizing the session to Redis. We'll use a community-maintained buildpack to deal with the discovery and configuration of the Redis service and session provider. We won't have to write any code to make this happen! 

Create the shared redis service to store session state 

```
cf create-service p-redis shared-vm session 
```

Bind the application to redis

```
---
applications:
- stack: windows
  instances: 2
  memory: 384M 
  buildpacks:
    - hwc_buildpack
  services:
    - session
```

Add the redis session buildpack

```
---
applications:
- stack: windows
  memory: 384M 
  instances: 2
  buildpacks:
    - https://github.com/cloudfoundry-community/redis-session-aspnet-buildpack/releases/download/v1.0.5/Pivotal.Redis.Aspnet.Session.Buildpack-win-x64-1.0.5.zip 
    - hwc_buildpack
  services:
    - session
```

Push the web app to cloud foundry

```
cf push <web-app-name>
```

Now visit the "ViewCounter" in a browser again. Again, the first few requests might be slow while your application is waking up. See that the view counts are incerementing correctly now?
## Summary

SSSSSSSSSS

|[<< Prev][buildpacks-link]||
|:--|--:|
