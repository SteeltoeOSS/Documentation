# Spring Boot Provider

This provider exists for interoperability with Spring Boot environment variables and command-line arguments.

The Steeltoe Spring Boot configuration provider reads the JSON in the `SPRING_BOOT_APPLICATION` environment variable and adds its contents to the configuration. It does the same for command-line arguments.
In both cases, any `.` delimiters in configuration keys are converted to `:`, which is the configuration key separator used by .NET. Likewise, Spring array syntax, such as `[1]`, is converted to .NET array syntax `:1`.

## Usage

You should have a good understanding of how the [.NET Configuration System](https://learn.microsoft.com/aspnet/core/fundamentals/configuration) works before starting to use this provider.

To use the Steeltoe Spring Boot provider, you need to:

1. Add the appropriate NuGet package reference to your project.
1. Add the provider to the Configuration Builder.
1. Access keys from the `IConfiguration`.

### Add NuGet Reference

To use the provider, you need to add a reference to the `Steeltoe.Configuration.SpringBoot` NuGet package.

### Add Configuration Provider

To access Spring Boot configuration data, you need to add the Spring Boot provider to the `ConfigurationBuilder`.

The following example shows how to do so:

```csharp
using Steeltoe.Configuration.SpringBoot;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddSpringBootFromEnvironmentVariable();
builder.Configuration.AddSpringBootFromCommandLine(args);
```

### Access Spring Boot Data

Once the configuration has been built, the Spring Boot provider can be used to access Spring-style keys in .NET syntax.
For example:

```csharp
using Steeltoe.Configuration.SpringBoot;

Environment.SetEnvironmentVariable("SPRING_APPLICATION_JSON", """
{
    "foo.bar": "value1"
}
""");

args = ["spring.bar[0].foo=value2"];

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddSpringBootFromEnvironmentVariable();
builder.Configuration.AddSpringBootFromCommandLine(args);

string? value1 = builder.Configuration["foo:bar"];
string? value2 = builder.Configuration["spring:bar:0:foo"];
```
