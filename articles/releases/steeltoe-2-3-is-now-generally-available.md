---
title: Steeltoe 2.3 is now generally available
description: Check it out
monikerRange: '== steeltoe-2.3'
date: 08/15/2019
uid: releases/steeltoe-2-3-is-now-generally-available
tags: [new-release]
---

# Steeltoe 2.3 is now generally available

By [David Dieruf](https://github.com/ddieruf)

Yet another iteration to the [Steeltoe Framework](https://steeltoe.io) is now available. This release is a part of the 2.3 branch, focused on both .NET Framework and Core support. New features include

## [Serilog](https://serilog.net/) extension for the Steeltoe dynamic logging provider

The dynamic logging provider is a wrapper around Microsoft Console Logging provider and allows for querying the currently defined loggers and their levels, as well as modifying logging levels dynamically at runtime. The Serilog extension allows Steeltoe to take advantage of it’s powerful structured event data format.


## 3rd party health checks are available in actuator health endpoint

Previously, Steeltoe’s health actuator endpoint reported health information about known bound services and custom providers. In this release you now have the ability to report 3rd party health checks to Steeltoe, which will in turn forward that information to the health actuator endpoint. Also, Steeltoe automatically deprecates it’s own health checks when it detects a Microsoft compliant health check is available. More about health check registrations [here](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.diagnostics.healthchecks.healthcheckregistration).

For a list of issues resolved in this release, please read the [official release notes](https://github.com/SteeltoeOSS/steeltoe/blob/master/roadmaps/2.3.0.md).

-------------------------------------------------------

## Steeltoe.Management

### Support for using Microsoft Community provided health checks

*   Samples: https://github.com/SteeltoeOSS/Samples/tree/2.x/Management/src/AspDotNetCore/MicrosoftHealthChecks
*   Additional info: https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/implement-resilient-applications/monitor-app-health


### Support for launching Cloud Foundry tasks bundled with applications



*   Added package Steeltoe.Management.TaskCore. Contains framework and extension methods for registering and running tasks (management processes) like database migrations.  ‘`IWebHost.RunWithTasks()`’ and ``IServiceCollection.AddTask&lt;T>`` where T : IApplicationTask (IApplicationTask is introduced here as well)


## **Steeltoe.Connectors**


### Added ability to apply EF migrations using ‘cf run-task’



*   `public class MigrateDbContextTask&lt;T> : IApplicationTask where T : DbContext`
*   Included in Steeltoe.CloudFoundry.Connector.EFCore
*   [https://github.com/SteeltoeOSS/Samples/tree/master/Management/src/AspDotNetCore/CloudFoundry](https://github.com/SteeltoeOSS/Samples/tree/master/Management/src/AspDotNetCore/CloudFoundry)


### Added additional property support for Microsoft SQL Server Connection strings

*   Now supports arbitrary properties (including using named instances) passed via jdbc-style uri. Property mapping is triggered by uri starting with “jdbc:” or containing a semi-colon 
*   jdbc:sqlserver://servername/databaseName=de5aa3a747c134b3d8780f8cc80be519e;instanceName=someInstance;integratedSecurity=true 

Added GemFire Connector

*   Will merge sample after the libs build… location will be [https://github.com/SteeltoeOSS/Samples/tree/2.x/Connectors/src/AspDotNetCore/GemFire](https://github.com/SteeltoeOSS/Samples/tree/2.x/Connectors/src/AspDotNetCore/GemFire)
*   [Docs link: https://steeltoe.io/docs/steeltoe-connectors/#8-0-apache-geode-gemfire-pivotal-cloud-cache](https://github.com/SteeltoeOSS/Samples/tree/2.x/Connectors/src/AspDotNetCore/GemFire)

Added Search Path support for PostgreSQL

*   [https://github.com/SteeltoeOSS/Connectors/pull/53](https://github.com/SteeltoeOSS/Connectors/pull/53)