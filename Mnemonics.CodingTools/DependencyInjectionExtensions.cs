using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
            // Bind and register options using IOptions pattern
            if (configureOptions != null)
            {
                services.Configure(configureOptions);
            }
            else
            {
                services.Configure<CodingToolsOptions>(_ => { }); // Register default
            }

            // Defer resolution of options until runtime
            services.AddSingleton(sp =>
            {
                return sp.GetRequiredService<IOptions<CodingToolsOptions>>().Value;
            });

            // These services can now depend on CodingToolsOptions via constructor

            // Dynamic code generation
            services.AddTransient<IDynamicClassGenerator, DynamicClassGenerator>();
            services.AddTransient<IDynamicClassBuilder, DynamicClassBuilder>();

            // Logging
            services.AddLogging();
            services.AddTransient<INinjaLogger, NinjaLogger>(sp =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger<NinjaLogger>();
                return new NinjaLogger(logger);
            });

            // EF Core dynamic context
            var provider = services.BuildServiceProvider(); // temporary, to access options
            var opts = provider.GetRequiredService<IOptions<CodingToolsOptions>>().Value;

            if (opts.RegisterDynamicEFCore)
            {
                services.AddSingleton<IDynamicTypeRegistry, DynamicTypeRegistry>();

                if (opts.ConfigureDynamicDb == null)
                    throw new InvalidOperationException("ConfigureDynamicDb must be provided when RegisterDynamicEFCore is enabled.");

                services.AddDbContext<DynamicDbContext>(opts.ConfigureDynamicDb);
            }

            // Storage registration
            if (opts.RegisterInMemoryStore)
            {
                services.AddSingleton(typeof(IEntityStore<>), typeof(InMemoryEntityStoreFactory<>));
            }

            if (opts.RegisterFileStore)
            {
                services.AddSingleton(typeof(IEntityStore<>), typeof(FileEntityStoreFactory<>));
            }

            if (opts.RegisterDbStore)
            {
                if (opts.DbContextResolver == null)
                    throw new InvalidOperationException("DbContextResolver must be provided when RegisterDbStore is enabled.");

                services.AddScoped(typeof(IEntityStore<>), typeof(DbEntityStoreFactory<>));
            }

            if (opts.RegisterDapperStore)
            {
                if (opts.DapperConnectionFactory == null)
                    throw new InvalidOperationException("DapperConnectionFactory must be provided when RegisterDapperStore is enabled.");

                services.AddScoped(typeof(Func<System.Data.IDbConnection>), sp => opts.DapperConnectionFactory(sp));
                services.AddScoped(typeof(IEntityStore<>), typeof(DapperEntityStoreFactory<>));
            }

            // Dynamic store resolver
            if (opts.RegisterInMemoryStore ||
                opts.RegisterFileStore ||
                opts.RegisterDbStore ||
                opts.RegisterDapperStore)
            {
                services.AddSingleton<IDynamicEntityResolver, DynamicEntityResolver>();
            }

            return services;
        }
    }
}
