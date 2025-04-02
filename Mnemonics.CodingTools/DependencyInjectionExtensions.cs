using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mnemonics.CodingTools.Configuration;
using Mnemonics.CodingTools.Data;
using Mnemonics.CodingTools.Interfaces;
using Mnemonics.CodingTools.Logging;

namespace Mnemonics.CodingTools
{
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
            var immediateOptions = new CodingToolsOptions();
            configureOptions?.Invoke(immediateOptions);
            services.Configure(configureOptions ?? (_ => { }));

            // Register options value as singleton (if not already registered)
            services.TryAddSingleton(sp => sp.GetRequiredService<IOptions<CodingToolsOptions>>().Value);

            // Register services based on evaluated options
            if (immediateOptions.RegisterDynamicClassGenerator)
            {
                services.AddTransient<IDynamicClassGenerator, DynamicClassGenerator>();
            }

            if (immediateOptions.RegisterDynamicClassBuilder)
            {
                services.AddTransient<Func<string, IDynamicClassBuilder>>(sp => className =>
                {
                    var opts = sp.GetRequiredService<IOptions<CodingToolsOptions>>();
                    return new DynamicClassBuilder(className, opts);
                });
            }

            if (immediateOptions.RegisterNinjaLogger)
            {
                services.AddLogging();
                services.AddTransient<INinjaLogger, NinjaLogger>(sp =>
                {
                    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger<NinjaLogger>();
                    return new NinjaLogger(logger);
                });
            }

            if (immediateOptions.RegisterDynamicEFCore)
            {
                if (immediateOptions.ConfigureDynamicDb == null)
                    throw new InvalidOperationException("ConfigureDynamicDb must be set for EF Core support.");

                services.AddSingleton<IDynamicTypeRegistry, DynamicTypeRegistry>();
                services.AddDbContext<DynamicDbContext>(immediateOptions.ConfigureDynamicDb);
            }

            // Register entity store implementations
            if (immediateOptions.RegisterInMemoryStore)
                services.AddInMemoryStore();

            if (immediateOptions.RegisterFileStore)
                services.AddFileStore();

            if (immediateOptions.RegisterDbStore)
            {
                // 👇 Inject DynamicDbContext by default
                immediateOptions.DbContextResolver ??= sp => sp.GetRequiredService<IDbEntityStoreContext>();
                services.AddDbStore(immediateOptions);
            }

            if (immediateOptions.RegisterDapperStore)
                services.AddDapperStore(immediateOptions);

            // Register dynamic resolver if any store was enabled
            if (immediateOptions.RegisterInMemoryStore ||
                immediateOptions.RegisterFileStore ||
                immediateOptions.RegisterDbStore ||
                immediateOptions.RegisterDapperStore)
            {
                services.AddSingleton<IDynamicEntityResolver, DynamicEntityResolver>();
            }

            services.AddSingleton(sp => sp.GetRequiredService<IOptions<CodingToolsOptions>>().Value);

            return services;
        }
    }
}
