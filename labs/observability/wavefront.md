---
uid: labs/observability/wavefront
title: Using Steeltoe with Wavefront
---

[vs-clone-a-repo]: ~/labs/images/vs-clone-a-repo.png "Clone a repository"
[vs-loaded-cloned-repo]: ~/labs/images/vs-loaded-cloned-repo.png "Provide repository URL"
[vs-repo-url]: ~/labs/images/vs-repo-url.png "Loaded cloned repository"

# Using Steeltoe with Wavefront for app container metrics, distributed tracing, and container observability

## Goal

This tutorial takes you through creating a simple Steeltoe app with actuators, logging, and distributed tracing. With that app running you then export the data to a Wavefront account.

## Expected Results

To have a Steeltoe enabled app running along with Telegraf and Wavefront proxy. The app is making metrics available via management endpoints, telegraf is scraping the values and feeding them to Wavefront proxy. The proxy is forwarding the metrics on to a Wavefront.

## Pre-req's

You'll need a Wavefront account to complete this guide successfully. [Create a 30 day trial](https://www.wavefront.com/sign-up/), if you don't already have access.

## Get Started

First clone the accompanying repo that contains all the needed assets

# [.NET CLI](#tab/powershell)

```powershell
git clone https://github.com/steeltoeoss-incubator/observability.git
cd observability/wavefront
```

# [Visual Studio](#tab/visual-studio)

Repository URL: `https://github.com/steeltoeoss-incubator/observability.git`

|![vs-clone-a-repo] Choose to clone a repository. |![vs-repo-url] Provide the repository url.|![vs-loaded-cloned-repo] The cloned repository will be loaded.|
|:--|

***

Have a look at what things are provided

```powershell
Name                    Description
----                    ----
dashboard-template.json Wavefront dashboard configuration
docker-compose.yml      Docker file to start all containers
telegraf.conf           Telegraf inputs and output configuration
```

Replace the placeholder `YOUR_API_TOKEN` in `docker-compose.yml` with your Wavefront API token. Lean how to retrieve that token [here](https://docs.wavefront.com/users_account_managing.html).

## Create .NET Core application

## Deploy everything with docker compose

## User Wavefront to observe the application

## Further Wavefront learning

## Summary
