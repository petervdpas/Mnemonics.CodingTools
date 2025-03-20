# Mnemonics.CodingTools

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

**Mnemonics.CodingTools** is a .NET library that provides dynamic class generation and logging utilities for modern applications. With a focus on flexibility and ease of integration, this library allows you to build classes at runtime from JSON definitions and seamlessly integrate logging using Microsoft.Extensions.Logging.

## Overview

The library offers the following features:

- **Dynamic Class Generation:**  
  Generate classes dynamically from structured JSON input, compile them into assemblies, and retrieve defined namespaces.

- **Dynamic Class Building:**  
  (Optional) Build dynamic classes with customizable properties at runtime.

- **Structured Logging:**  
  Utilize the `NinjaLogger` adapter to integrate with the built-in logging framework in .NET.

- **Dependency Injection Integration:**  
  Easily register all services using the provided extension method with configurable options.

## Installation

Install the package via the .NET CLI:

```bash
dotnet add package Mnemonics.CodingTools --version 1.0.0
```

Or using the Package Manager Console:

```powershell
Install-Package Mnemonics.CodingTools -Version 1.0.0
```

## Getting Started

### Registering Services with Dependency Injection

The library includes a dependency injection extension method that allows you to register services selectively using options.

```csharp
using Mnemonics.CodingTools;

var builder = WebApplication.CreateBuilder(args);

// Register all coding tools services using default options:
builder.Services.AddCodingTools();

// Or, customize the registration options:
builder.Services.AddCodingTools(options =>
{
    options.RegisterDynamicClassGenerator = true;
    options.RegisterDynamicClassBuilder = true; // Enable if you have an implementation.
    options.RegisterNinjaLogger = true;
});

var app = builder.Build();

app.MapGet("/", (IDynamicClassGenerator generator) =>
{
    // Use the dynamic class generator here.
    return "Coding Tools are set up!";
});

app.Run();
```

### Dynamic Class Generation

The core functionality of dynamic class generation is exposed via the `IDynamicClassGenerator` interface. Here's an example of how to use it:

```csharp
using Mnemonics.CodingTools.Interfaces;

public class ExampleService
{
    private readonly IDynamicClassGenerator _classGenerator;

    public ExampleService(IDynamicClassGenerator classGenerator)
    {
        _classGenerator = classGenerator;
    }

    public void GenerateAssembly(string jsonDefinition, string outputDllPath)
    {
        var (assemblyPath, namespaces) = _classGenerator.GenerateAssemblyFromJson(jsonDefinition, outputDllPath);

        if (assemblyPath != null)
        {
            // Assembly generated successfully.
        }
        else
        {
            // Handle errors in generation.
        }
    }
}
```

### Logging with NinjaLogger

For logging, the library provides a `NinjaLogger` that wraps Microsoft.Extensions.Logging. This enables structured logging in your application.

```csharp
using Mnemonics.CodingTools.Logging;

public class LoggingExample
{
    private readonly NinjaLogger _logger;

    public LoggingExample(NinjaLogger logger)
    {
        _logger = logger;
    }

    public void LogExample()
    {
        _logger.Debug("This is a debug message");
        _logger.Information("Information message with parameter: {0}", 123);
    }
}
```

### JSON Structure for Dynamic Classes

The JSON input should define namespaces, classes, properties, and methods. An example JSON definition is as follows:

```json
[
  {
    "Namespace": "DynamicNamespace",
    "Usings": [ "System" ],
    "Classes": [
      {
        "Name": "DynamicClass",
        "Implements": [],
        "Properties": [
          { "Name": "Property1", "Type": "string", "AccessorVisibility": "public", "AccessorType": "init" }
        ],
        "Constructors": [],
        "Methods": []
      }
    ]
  }
]
```

This JSON is parsed by the library to dynamically generate classes and compile them into an assembly.

## Contributing

Contributions are welcome! Please fork the repository, create a feature branch, and submit a pull request with your changes. For any issues or feature requests, please open an issue in the [GitHub repository](https://github.com/petervdpas/Mnemonics.CodingTools).

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Repository

For more information, please visit the [GitHub repository](https://github.com/petervdpas/Mnemonics.CodingTools).
