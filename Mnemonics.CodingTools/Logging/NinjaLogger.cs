using System;
using Microsoft.Extensions.Logging;
using Mnemonics.CodingTools.Interfaces;

namespace Mnemonics.CodingTools.Logging;

/// <summary>
///     Provides a wrapper to adapt <see cref="Microsoft.Extensions.Logging.ILogger" />
///     to the <see cref="Mnemonics.CodingTools.Interfaces.INinjaLogger" /> interface.
/// </summary>
public class NinjaLogger : INinjaLogger
{
    private readonly ILogger<NinjaLogger> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="NinjaLogger"/> class.
    /// </summary>
    /// <param name="logger">The underlying <see cref="ILogger"/> instance.</param>
    public NinjaLogger(ILogger<NinjaLogger> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    /// <summary>
    ///     Logs a debug message, optionally with structured arguments.
    /// </summary>
    /// <param name="message">The debug message template to log.</param>
    /// <param name="args">Optional arguments to format the message.</param>
    public void Debug(string message, params object[] args)
    {
#pragma warning disable CA2254
        _logger.LogDebug(message, args);
#pragma warning restore CA2254
    }

    /// <summary>
    ///     Logs an informational message, optionally with structured arguments.
    /// </summary>
    /// <param name="message">The informational message template to log.</param>
    /// <param name="args">Optional arguments to format the message.</param>
    public void Information(string message, params object[] args)
    {
#pragma warning disable CA2254
        _logger.LogInformation(message, args);
#pragma warning restore CA2254
    }

    /// <summary>
    ///     Logs a warning message, optionally with structured arguments.
    /// </summary>
    /// <param name="message">The warning message template to log.</param>
    /// <param name="args">Optional arguments to format the message.</param>
    public void Warning(string message, params object[] args)
    {
#pragma warning disable CA2254
        _logger.LogWarning(message, args);
#pragma warning restore CA2254
    }

    /// <summary>
    ///     Logs an error message, optionally including structured arguments or exception details.
    /// </summary>
    /// <param name="message">The error message template to log.</param>
    /// <param name="exception">An optional exception to include in the log.</param>
    /// <param name="args">Optional arguments to format the message.</param>
    public void Error(string message, Exception? exception = null, params object[] args)
    {
#pragma warning disable CA2254
        if (exception != null)
            _logger.LogError(exception, message, args);
        else
            _logger.LogError(message, args);
#pragma warning restore CA2254
    }
}