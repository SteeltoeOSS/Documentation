---
_disableContribution: true
_disableFooter: true
_homePath: "./"
_disableNav: true
uid: guides/modernize-dotnet
---

> [!NOTE]
> These guides apply to Steeltoe v3. Please [open an issue](https://github.com/SteeltoeOSS/Documentation/issues/new/choose) if you'd like to contribute updating them for Steeltoe v4.

[strigo-desktop-vs]: ~/guides/images/strigo-desktop-vs.png "Strigo windows placement with Visual Studio"
[exercise-1-link]: exercise1.md
[exercise-2-link]: exercise2.md

## Spring One Workshops - Modernizing .NET Applications with VMware Tanzu Application Service

In this workshop we’re going to perform a lightweight modernization of a fictitious full framework ASP.NET MVC application who's front-end consumes a WebAPI backend. The application is presumed to run on IIS today using standard virtual or physical infrastructure and our task is to migrate the application to Cloud Foundry. Our modernization efforts will involve building the necessary artifacts to get the application to run at all in the cloud and to allow the application to scale once it gets there.

All the while we’ll try to use a lightweight approach and make as few changes to application code as possible.

## Current State

Our sample application for this working is a single solution containing two projects. The first project is a WebAPI project exposing a single endpoint. The second project is an ASP.NET MVC front-end that consumes the WebAPI endpoint from JavaScript.

The application is designed with an IIS topology in mind with both applications sharing a common hostname, the frontend responding to the root, and the backend living in a virtual directory located at the path “/api”.

The frontend exposes an endpoint that tracks session-level view counts and is configured to use “InProc” session state.

## Desired State:

Our goals are

1. Get this application operational on Cloud Foundry

1. Ensure the application is scalable

1. Use solutions that are minimally invasive to the code base

## Lab Details:

- $HOME\src contains the solution.
- Your CF CLI is already authenticated

| [Get started][exercise-1-link] |
| :----------------------------: |
