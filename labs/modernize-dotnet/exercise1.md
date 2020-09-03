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

[modernize-frontend-newmanifest]: ~/labs/images/modernize-frontend-newmanifest.png "Create a new manifest for the frontend"
[modernize-frontend-publish]: ~/labs/images/modernize-frontend-publish.png "Publish the frontend"
[modernize-frontend-publish2]: ~/labs/images/modernize-frontend-publish2.png "Publish the frontend"
[modernize-frontend-cfapp]: ~/labs/images/modernize-frontend-cfapp.png "Find the URL your application will respond on"
[modernize-service-newmanifest]: ~/labs/images/modernize-service-newmanifest.png "Create a manifest for the backend service"
[modernize-service-result]: ~/labs/images/modernize-service-result.png "Results of the service"
[modernize-frontend-result]: ~/labs/images/modernize-frontend-result.png "Results of the service as consumed by the frontend"

||[Next Exercise >>][exercise-2-link]|
|:--|--:|

# Exercise 1

## Goal

Simulate an IIS Virtual Directory structure with routes in TAS

## Expected Results

1. Web API Service running on /api
1. Web front end running on /

## Get Started

### Deploy the frontend application
Using VS Code, create a manifest file for the web frontend named `manifest.yml` in the `WorkshopFrontEnd` directory. In this case every field can remain the same as the example.

<br><br><br>
![modernize-frontend-newmanifest]
<br><br><br>

With the following content

```yaml
---
applications:
- instances: 1
  memory: 384M 
  path: bin/app.publish/
  buildpacks: 
    - hwc_buildpack
```

In Visual Studio publish the application to the Folder profile by right-clicking the `WorkshopFrontEnd` project, selecting `Publish`, and clicking the `Publish` button on the right.

<br><br><br>
![modernize-frontend-publish]
<br><br><br>
![modernize-frontend-publish2]
<br><br><br>

In Powershell change directory to `WorkshopFrontEnd` (the directory containing `WorkshopFrontEnd.csproj`). Then Push the web app to cloud foundry. Supply your own value for <front-end-app-name> that would likely be unique amoungst your classmates, like `john-q-smith-1990`. Ensure the Windows stack is targetting by setting the `-s` parameter to `windows`.

```powershell
cf push <front-end-app-name> -s windows
```

Find the url for your application by visitng it's route in a web browser. You can retreive the route with 

```powershell
cf app <front-end-app-name>
```

<br><br><br>
![modernize-frontend-cfapp]
<br><br><br>

Select `ViewCounter` from the top menu of the web application. See the counter increments with every refresh. Do this 10 times or so to verify.

### Deploy the API

Using VS Code, create a manifest file named `manifest.yml` for the backend in the `WorkshopService` directory. In the begining of the route place the URL used by the frontend application without the protocol i.e. `john-q-smith-1990.run.pivotal.io`. Ensure the route has `/api` after the domain name. This instructs cloud foundry to send all requests to the `/api` context path to the backend service while all other requests are routed to the front end.

<br><br><br>
![modernize-service-newmanifest]
<br><br><br>

```
---
applications:
- instances: 1
  memory: 384M
  path: bin/app.publish/
  buildpacks: 
    - hwc_buildpack
  routes:
    - route: <host-name.domain-name>/api
```    

Publish the application to the Folder profile by right-clicking the `WorkshopService` project, selecting `Publish`, and clicking the `Publish` button on the right.

In powershell change directory to `WorkshopService` (the directory containing `WorkshopService.csproj`) then push the service to cloud foundry. Since you're sharing a space with your classmates you'll need to ensure the name is unique. Also, ensure the app name is DIFFERENT than front end. Note that outside of the classroom each deployment of the application would likely have a different space. 

```powershell
cf push <api-app-name> -s windows
```

Verify the service is working by visiting `https://<host-name.domain-name>/api/values` in your web browser. Note that in the absense of special headers you may get an XML response. 

<br><br><br>
![modernize-service-result]
<br><br><br>

Now validate that the frontend is consuming the API by visiting `https://chrisumbelsapp.cfapps.io/Home/ApiClient` in your web browser. If you see `one two three` in the results then everythign is working. You've recreated an IIS virtual directory structure using Cloud Foundry routes!

<br><br><br>
![modernize-frontend-result]
<br><br><br>

Note that in practice you can mix and match technologies behind context routes. For instance your `/api` could be a Java/Spring application and your front end at the root could be dotnet core, ASP.NET, or whatever you like. 

||[Next Exercise >>][exercise-2-link]|
|:--|--:|
