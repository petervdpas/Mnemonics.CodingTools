﻿using Mnemonics.CodingTools.Enums;

namespace Mnemonics.CodingTools.Interfaces;

/// <summary>
///     Extends <see cref="INinjaLogger" /> to support structured logging with context and log levels.
/// </summary>
public interface IStructuredNinjaLogger
{
    /// <summary>
    ///     Logs a message with a specified context and severity level.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="context">The context or category of the log message (e.g., "Initialization", "Database").</param>
    /// <param name="level">The severity level of the log message.</param>
    void LogWithContext(string message, string context, LogLevel level);
}