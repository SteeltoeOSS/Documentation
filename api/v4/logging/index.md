# Dynamic Logging

Steeltoe includes two logging providers that wrap existing logger libraries with support for managing log levels at runtime through the [Steeltoe Management logger endpoint](../management/loggers.md) and [recording distributed trace information](../tracing/index.md#enabling-log-correlation).

* [Steeltoe Dynamic Logger](./dynamic-logging-provider.md) works with `Microsoft.Extensions.Logging.Console`.
* [Steeltoe Dynamic Serilog](./serilog-logger.md) works with `Serilog`
