using System;
using Microsoft.EntityFrameworkCore;

namespace Mnemonics.CodingTools.Configuration;

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
}