using System;
using Mnemonics.CodingTools.Interfaces;
using Mnemonics.CodingTools.Stores;

namespace Mnemonics.CodingTools
{
    /// <summary>
    /// Resolves an <see cref="IEntityStore{T}"/> for a given entity type at runtime
    /// using the dependency injection container. If the store is not registered,
    /// falls back to an <see cref="InMemoryEntityStoreFactory{T}"/> instance.
    /// </summary>
    public class DynamicEntityResolver : IDynamicEntityResolver
    {
        private readonly IServiceProvider _provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicEntityResolver"/> class.
        /// </summary>
        /// <param name="provider">The service provider used to resolve entity store instances.</param>
        public DynamicEntityResolver(IServiceProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        /// <summary>
        /// Resolves an <see cref="IEntityStore{T}"/> for the specified entity type from the DI container.
        /// Falls back to a default in-memory store if no implementation is registered.
        /// </summary>
        /// <param name="entityType">The entity type to resolve the store for.</param>
        /// <returns>
        /// An instance of <see cref="IEntityStore{T}"/> for the given type, either from DI or as a fallback.
        /// </returns>
        public object GetStore(Type entityType)
        {
            if (entityType == null)
                throw new ArgumentNullException(nameof(entityType));

            var serviceType = typeof(IEntityStore<>).MakeGenericType(entityType);

            // Try resolving via DI first
            var resolved = _provider.GetService(serviceType);
            if (resolved != null)
                return resolved;

            // Fallback: return an InMemoryEntityStoreFactory<T>
            var fallbackType = typeof(InMemoryEntityStoreFactory<>).MakeGenericType(entityType);
            return Activator.CreateInstance(fallbackType)!;
        }
    }
}