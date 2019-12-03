# Getting Started

## Install Steeltoe Tooling

Steeltoe Tooling is a [DotNet Global Tools](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools) console executable named `st`.  Use `dotnet tool install` to install.

```sh
$ dotnet tool install --global --version 0.5.0 Steeltoe.Cli
```

## Add DotNet Global Tools to your PATH Variable

DotNet Global Tools are installed in an OS-dependent user directory.

|OS|Path|
|---|---|
|Windows|`%USERPROFILE%\.dotnet\tools`|
|OS X/Linux|`$HOME/.dotnet/tools`|

After adding of the above paths to your `PATH` env var, you can run the `st` executable.

```sh
$ st --version
1.0.0-m1
```
