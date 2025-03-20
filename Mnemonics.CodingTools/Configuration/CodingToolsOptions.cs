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
}