using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mnemonics.CodingTools.Configuration;
using Mnemonics.CodingTools.Data;
using Mnemonics.CodingTools.Interfaces;
using Mnemonics.CodingTools.Logging;
using Mnemonics.CodingTools.Stores;

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

             // EF Core dynamic support
            if (options.RegisterDynamicEFCore)
            {
                services.AddSingleton<IDynamicTypeRegistry, DynamicTypeRegistry>();

                if (options.ConfigureDynamicDb == null)
                    throw new InvalidOperationException(
                        "RegisterDynamicEFCore is enabled, but ConfigureDynamicDb is not provided.");

                services.AddDbContext<DynamicDbContext>(options.ConfigureDynamicDb);
            }

            // Register entity stores
            if (options.RegisterInMemoryStore)
                services.AddSingleton(typeof(IEntityStore<>), typeof(InMemoryEntityStoreFactory<>));

            if (options.RegisterFileStore)
                services.AddSingleton(typeof(IEntityStore<>), typeof(FileEntityStoreFactory<>));

            if (options.RegisterDbStore)
            {
                if (options.DbContextResolver == null)
                    throw new InvalidOperationException(
                        "DbContextResolver must be provided when RegisterDbStore is enabled.");

                services.AddScoped(typeof(IEntityStore<>), typeof(DbEntityStoreFactory<>));
            }

            if (options.RegisterDapperStore)
            {
                if (options.DapperConnectionFactory == null)
                    throw new InvalidOperationException(
                        "DapperConnectionFactory must be provided when RegisterDapperStore is enabled.");

                services.AddScoped(typeof(Func<System.Data.IDbConnection>), sp => options.DapperConnectionFactory(sp));
                services.AddScoped(typeof(IEntityStore<>), typeof(DapperEntityStoreFactory<>));
            }

            // Register the dynamic entity resolver only if any store is enabled
            if (options.RegisterInMemoryStore ||
                options.RegisterFileStore ||
                options.RegisterDbStore ||
                options.RegisterDapperStore)
            {
                services.AddSingleton<IDynamicEntityResolver, DynamicEntityResolver>();
            }

            return services;
        }
    }
}
