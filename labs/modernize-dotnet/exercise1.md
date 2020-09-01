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

||[Next >>][buildpacks-link]|
|:--|--:|

# Exercise 1

## Goal

Simulate an IIS Virtual Directory structure with routes in TAS

## Expected Results

1. Web API Service running on /api
1. Web front end running on /

## Get Started

### Deploy the frontend application
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

Push the web app to cloud foundry. Supply your own value for <front-end-app-name> that would likely be unique amoungst your classmates, like `john-q-smith-1990`.

```
cf push <front-end-app-name>
```

Find the url for your application by visitng it's route in a web browser. You can retreive the route with 

```
cf app <front-end-app-name>
```

* insert screenshot here *

Select "ViewCounter" from the top menu of the web application. See the counter increments with every refresh. Do this 10 times or so to verify.

### Deploy the API

Create a manifest for the backend in the root of its directory. In the begining of the route place the URL used by the frontend application without the protocol i.e. `john-q-smith-1990.run.pivotal.io`.

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

Push the service to cloud foundry. Since you're sharing a space with your classmates you'll need to ensure the name is unique. Note that outside of the classroom each deployment of the application would likely have a different space. 

```
cf push <api-app-name>
```

### Scale the frontend 
Let's return our attention to the frontend. Scale the application from a single instance to three. 

```
cf scale -i 2 <front-end-app-name> 
```
## Summary

SSSSSSSSSS

||[Next >>][buildpacks-link]|
|:--|--:|
