---
_disableContribution: true
_hideTocVersionToggle: true

title: Steeltoe 3.3 is Now Available
description: Steeltoe 3.3 has been released
date: 09/04/2025
uid: releases/steeltoe-3-3-0.html
tags: ["new-release"]
---

# What's New in 3.3.0

This release improves support for .NET 8 (except for Integration, Messaging and Stream packages) and removes support for .NET Core 3.1. CVEs in transitive dependencies have been addressed and minor enhancements have been made to the application security components (including support for running against a local UAA server). We've also added Obsolete annotations to a number of public APIs that are no longer available in Steeltoe 4.0 to aid the upgrade experience.

Please note that the CVE-related updates required upgrading OpenTelemetry, which introduces breaking changes. For example, OpenTelemetry no longer supports exporting directly to Jaeger, so that support is now also removed from Steeltoe. For those affected by the removal of Jaeger exporting, please review [Steeltoe documentation for using Open Telemetry Protocol](../../docs/v3/tracing/distributed-tracing-exporting.html#use-open-telemetry-protocol-exporter) and Jaeger documentation.
