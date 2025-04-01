using System;
using System.Collections.Generic;
using System.Data;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Mnemonics.CodingTools.Configuration
{
    /// <summary>
    /// Options for registering Mnemonics.CodingTools services.
    /// </summary>
    public class CodingToolsOptions
    {
        /// <summary>
        /// Indicates whether to register the dynamic class generator.
        /// </summary>
        public bool RegisterDynamicClassGenerator { get; set; } = true;

        /// <summary>
        /// Indicates whether to register the dynamic class builder.
        /// </summary>
        public bool RegisterDynamicClassBuilder { get; set; } = false;

        /// <summary>
        /// Indicates whether to register the NinjaLogger.
        /// </summary>
        public bool RegisterNinjaLogger { get; set; } = true;

        /// <summary>
        /// Indicates whether to register the dynamic type registry and EF Core dynamic context.
        /// </summary>
        public bool RegisterDynamicEFCore { get; set; } = false;

        /// <summary>
        /// Optional EF Core configuration for the dynamic context (e.g. SQLite).
        /// </summary>
        public Action<DbContextOptionsBuilder>? ConfigureDynamicDb { get; set; }

        /// <summary>
        /// Global fallback key property names to use when [IsKeyField] is not specified.
        /// Applied to all dynamic types.
        /// </summary>
        public List<string> GlobalFallbackKeyNames { get; set; } = ["Id"];

        /// <summary>
        /// Indicates whether to register the in-memory entity store.
        /// </summary>
        public bool RegisterInMemoryStore { get; set; } = false;

        /// <summary>
        /// Indicates whether to register the file-based entity store.
        /// </summary>
        public bool RegisterFileStore { get; set; } = false;

        /// <summary>
        /// Indicates whether to register the EF Core–based entity store.
        /// </summary>
        public bool RegisterDbStore { get; set; } = false;

        /// <summary>
        /// Indicates whether to register the Dapper-based entity store.
        /// </summary>
        public bool RegisterDapperStore { get; set; } = false;

        /// <summary>
        /// Optional output directory for generated assemblies (used by DynamicClassBuilder).
        /// Defaults to "GeneratedAssemblies".
        /// </summary>
        public string AssemblyDirectory { get; set; } = "GeneratedAssemblies";

        /// <summary>
        /// Optional directory for file-based store. Defaults to "EntityStore".
        /// </summary>
        public string? FileStoreDirectory { get; set; }

        /// <summary>
        /// Optional factory for resolving the EF Core context used by DbEntityStore.
        /// </summary>
        public Func<IServiceProvider, Interfaces.IDbEntityStoreContext>? DbContextResolver { get; set; }

        /// <summary>
        /// Optional factory that resolves a <see cref="Func{IDbConnection}"/> used by DapperEntityStore.
        /// </summary>
        public Func<IServiceProvider, Func<IDbConnection>>? DapperConnectionFactory { get; set; }

        /// <summary>
        /// Optional custom key selector mapping for specific types (used by FileEntityStore).
        /// </summary>
        public Dictionary<Type, Delegate> CustomKeySelectors { get; } = [];

        /// <summary>
        /// Optional per-entity JsonSerializerOptions (used by FileEntityStore).
        /// </summary>
        public Dictionary<Type, JsonSerializerOptions> JsonOptionsPerEntity { get; } = [];
    }
}
