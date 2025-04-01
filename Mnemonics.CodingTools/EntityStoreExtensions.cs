using System;
using Microsoft.Extensions.DependencyInjection;
using Mnemonics.CodingTools.Configuration;
using Mnemonics.CodingTools.Interfaces;
using Mnemonics.CodingTools.Stores;

namespace Mnemonics.CodingTools
{
    /// <summary>
    /// Provides extension methods for registering specific <see cref="IEntityStore{T}"/> implementations.
    /// </summary>
    public static class EntityStoreExtensions
    {
        /// <summary>
        /// Registers the in-memory entity store as a singleton for all entity types.
        /// </summary>
        /// <param name="services">The service collection to add the store to.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddInMemoryStore(this IServiceCollection services)
        {
            return services.AddSingleton(typeof(IEntityStore<>), typeof(InMemoryEntityStoreFactory<>));
        }

        /// <summary>
        /// Registers the file-based entity store as a singleton for all entity types.
        /// </summary>
        /// <param name="services">The service collection to add the store to.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddFileStore(this IServiceCollection services)
        {
            return services.AddSingleton(typeof(IEntityStore<>), typeof(FileEntityStoreFactory<>));
        }

        /// <summary>
        /// Registers the EF Core–backed entity store using the provided <see cref="CodingToolsOptions"/>.
        /// </summary>
        /// <param name="services">The service collection to add the store to.</param>
        /// <param name="options">The options containing the required <see cref="CodingToolsOptions.DbContextResolver"/>.</param>
        /// <returns>The updated service collection.</returns>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="CodingToolsOptions.DbContextResolver"/> is not set.</exception>
        public static IServiceCollection AddDbStore(this IServiceCollection services, CodingToolsOptions options)
        {
            if (options.DbContextResolver == null)
                throw new InvalidOperationException("DbContextResolver must be provided for DbStore.");

            return services.AddScoped(typeof(IEntityStore<>), typeof(DbEntityStoreFactory<>));
        }

        /// <summary>
        /// Registers the Dapper-backed entity store using the provided <see cref="CodingToolsOptions"/>.
        /// </summary>
        /// <param name="services">The service collection to add the store to.</param>
        /// <param name="options">The options containing the required <see cref="CodingToolsOptions.DapperConnectionFactory"/>.</param>
        /// <returns>The updated service collection.</returns>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="CodingToolsOptions.DapperConnectionFactory"/> is not set.</exception>
        public static IServiceCollection AddDapperStore(this IServiceCollection services, CodingToolsOptions options)
        {
            if (options.DapperConnectionFactory == null)
                throw new InvalidOperationException("DapperConnectionFactory must be provided for DapperStore.");

            services.AddSingleton(typeof(Func<System.Data.IDbConnection>), sp => options.DapperConnectionFactory(sp));
            return services.AddSingleton(typeof(IEntityStore<>), typeof(DapperEntityStoreFactory<>));
        }
    }
}