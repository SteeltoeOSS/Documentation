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

||[Next Exercise >>][exercise-2-link]|
|:--|--:|

# Exercise 1

## Goal

XXXXXXXX

## Expected Results

XXXXXXXX

## Get Started

Create a manifest for the backend in the root of its directory.

```
---
applications:
- stack: windows
  instances: 1
  memory: 384M 
  buildpacks: 
    - hwc_buildpack
  routes:
    - route: <host-name.domain-name>/api
```    

Publish the application
* insert screenshot here *

Push the service to cloud foundry

```
cf push <api-app-name>
```

Create a manifest for the web frontend in the root of its directory.

```
---
applications:
- stack: windows
  instances: 1
  memory: 384M 
  buildpacks: 
    - hwc_buildpack
```

Publish the application
* insert screenshot here *

Push the web app to cloud foundry

```
cf push <web-app-name>
```

Find the url for your application by visitng it's route in a web browser. You can retreive the route with 

```
cf app <web-app-name>
```

Select "ViewCounter" from the top menu of the web application. See the counter increments with every refresh. Do this 10 times or so to verify.

Now scale the application from a single instance to three. 

```
cf scale -i 2 <app-name> 
```
## Summary

SSSSSSSSSS

||[Next Exercise >>][exercise-2-link]|
|:--|--:|
