using System;
using Microsoft.Extensions.DependencyInjection;
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
            services.Configure(configureOptions ?? (_ => { }));

            services.PostConfigure<CodingToolsOptions>(opts =>
            {
                if (opts.RegisterDynamicClassGenerator)
                {
                    services.AddTransient<IDynamicClassGenerator, DynamicClassGenerator>();
                }

                if (opts.RegisterDynamicClassBuilder)
                {
                    services.Configure(configureOptions ?? (_ => { })); // Ensure IOptions is available
                    services.AddTransient<Func<string, IDynamicClassBuilder>>(sp => className =>
                    {
                        var opts = sp.GetRequiredService<IOptions<CodingToolsOptions>>();
                        return new DynamicClassBuilder(className, opts);
                    });
                }

                if (opts.RegisterNinjaLogger)
                {
                    services.AddLogging();

                    services.AddTransient<INinjaLogger, NinjaLogger>(sp =>
                    {
                        var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                        var logger = loggerFactory.CreateLogger<NinjaLogger>();
                        return new NinjaLogger(logger);
                    });
                }

                if (opts.RegisterDynamicEFCore)
                {
                    services.AddSingleton<IDynamicTypeRegistry, DynamicTypeRegistry>();

                    if (opts.ConfigureDynamicDb != null)
                    {
                        services.AddDbContext<DynamicDbContext>(opts.ConfigureDynamicDb);
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            "RegisterDynamicEFCore is enabled, but ConfigureDynamicDb is not provided.");
                    }
                }

                // Storage registration (delegated)
                if (opts.RegisterInMemoryStore) { services.AddInMemoryStore(); }

                if (opts.RegisterFileStore) { services.AddFileStore(); }

                if (opts.RegisterDbStore) { services.AddDbStore(opts); }

                if (opts.RegisterDapperStore) { services.AddDapperStore(opts); }

                // Dynamic store resolver
                if (opts.RegisterInMemoryStore || opts.RegisterFileStore || opts.RegisterDbStore || opts.RegisterDapperStore)
                {
                    services.AddSingleton<IDynamicEntityResolver, DynamicEntityResolver>();
                }
            });

            return services;
        }
    }
}
