using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mnemonics.CodingTools.Configuration;
using Mnemonics.CodingTools.Interfaces;
using Mnemonics.CodingTools.Logging;

namespace Mnemonics.CodingTools;

/// <summary>
/// Provides extension methods for registering Mnemonics.CodingTools services.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Registers all Mnemonics.CodingTools services with the provided <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to register services into.</param>
    /// <param name="configureOptions">An optional action to configure registration options.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddCodingTools(
        this IServiceCollection services,
        Action<CodingToolsOptions>? configureOptions = null)
    {
        var options = new CodingToolsOptions();
        configureOptions?.Invoke(options);
        
        if (options.RegisterDynamicClassGenerator)
        {
            services.AddTransient<IDynamicClassGenerator, DynamicClassGenerator>();
        }

        if (options.RegisterDynamicClassBuilder)
        {
            services.AddTransient<IDynamicClassBuilder, DynamicClassBuilder>();
        }

        if (options.RegisterNinjaLogger)
        {
            services.AddLogging();

            services.AddTransient<INinjaLogger, NinjaLogger>(sp =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger<NinjaLogger>();
                return new NinjaLogger(logger);
            });
        }

        return services;
    }
}