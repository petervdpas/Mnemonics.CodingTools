# Mnemonics.CodingTools

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

**Mnemonics.CodingTools** is a modular and extensible .NET library for dynamic class generation, runtime entity management, structured logging, and pluggable data storage — built with ASP.NET Core in mind.

---

## ✨ Features

- **⚙️ Dynamic Class Generation**  
  Generate types at runtime from JSON schemas or programmatic definitions. Compile into assemblies and use immediately.

- **📚 Runtime Entity Registration (EF Core)**  
  Register types at runtime via `IDynamicTypeRegistry`. Automatically used by `DynamicDbContext`.

- **💾 Pluggable Entity Stores**  
  Switch between multiple store implementations via `IEntityStore<T>` or `IAdvancedEntityStore<T>`:
  - ✅ In-memory store
  - ✅ File-based (JSON)
  - ✅ EF Core-backed
  - ✅ Dapper-based  
  All support composite keys and batch operations (via `IAdvancedEntityStore<T>`).

- **🪵 Structured Logging**  
  Use `NinjaLogger` as a drop-in structured logger based on `Microsoft.Extensions.Logging`.

- **🔌 DI-Ready Architecture**  
  Register everything with `AddCodingTools()`. Customize per backend using `CodingToolsOptions`.

---

## 📦 Installation

```bash
dotnet add package Mnemonics.CodingTools --version 1.0.x
```

Or with the NuGet Package Manager:

```powershell
Install-Package Mnemonics.CodingTools -Version 1.0.x
```

---

## 🚀 Quick Start

### Register in `Program.cs` / `Startup.cs`

```csharp
builder.Services.AddCodingTools(options =>
{
    options.RegisterDynamicClassGenerator = true;
    options.RegisterDynamicClassBuilder = true;
    options.RegisterNinjaLogger = true;

    options.RegisterDynamicEFCore = true;
    options.ConfigureDynamicDb = db => db.UseSqlite("Data Source=app.db");
    options.DbContextResolver = sp => sp.GetRequiredService<DynamicDbContext>();

    // Choose one or more entity stores:
    options.RegisterDbStore = true;
    // options.RegisterDapperStore = true;
    // options.RegisterFileStore = true;
    // options.RegisterInMemoryStore = true;

    // (Optional) Custom file store behavior:
    options.FileStoreDirectory = "Data";
    // options.CustomKeySelectors[typeof(MyType)] = (MyType x) => new[] { x.Id.ToString() };
    // options.JsonOptionsPerEntity[typeof(MyType)] = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
});
```

---

## 🧱 Dynamic Class Generation

```csharp
public class MyService
{
    private readonly IDynamicClassGenerator _generator;

    public MyService(IDynamicClassGenerator generator)
    {
        _generator = generator;
    }

    public void Generate(string json)
    {
        var (dllPath, namespaces) = _generator.GenerateAssemblyFromJson(json, "MyAssembly.dll");
    }
}
```

---

## 🧠 Runtime EF Entity Registration

```csharp
public class StartupAction
{
    private readonly IDynamicTypeRegistry _registry;

    public StartupAction(IDynamicTypeRegistry registry)
    {
        _registry = registry;
    }

    public void RegisterDynamicType(Type type)
    {
        _registry.RegisterType(type);
    }
}
```

---

## 💾 Generic Entity Store Example

```csharp
public class EntityService<T> where T : class
{
    private readonly IAdvancedEntityStore<T> _store;

    public EntityService(IAdvancedEntityStore<T> store)
    {
        _store = store;
    }

    public Task SaveAsync(T entity, params string[] keys)
        => _store.SaveAsync(keys, entity);

    public Task<IEnumerable<T>> AllAsync()
        => _store.ListAsync();
}
```

---

## 📁 File Store Example

```csharp
builder.Services.AddCodingTools(options =>
{
    options.RegisterFileStore = true;
    options.FileStoreDirectory = "EntityStore";
    options.CustomKeySelectors[typeof(MyEntity)] = (MyEntity e) => new[] { e.Id };
});
```

---

## 🪵 Structured Logging with `NinjaLogger`

```csharp
public class LogService
{
    private readonly NinjaLogger _logger;

    public LogService(NinjaLogger logger)
    {
        _logger = logger;
    }

    public void LogStuff()
    {
        _logger.Information("This ran at {time}", DateTime.UtcNow);
    }
}
```

---

## 🧾 JSON Schema Example

```json
[
  {
    "Namespace": "Dynamic.Models",
    "Classes": [
      {
        "Name": "User",
        "Properties": [
          { "Name": "Id", "Type": "string" },
          { "Name": "Username", "Type": "string" }
        ]
      }
    ]
  }
]
```

---

## 🧪 Designed for Testing

Interfaces like `IDynamicClassGenerator`, `INinjaLogger`, and `IAdvancedEntityStore<T>` allow for seamless mocking and testing in unit test environments.

---

## 🤝 Contributing

Feedback, PRs, and issues are welcome. Fork the repo or file issues at  
[https://github.com/petervdpas/Mnemonics.CodingTools](https://github.com/petervdpas/Mnemonics.CodingTools)

---

## 📜 License

Licensed under the [MIT License](LICENSE).
