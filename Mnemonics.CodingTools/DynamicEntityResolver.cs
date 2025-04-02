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

        /// <inheritdoc/>
        public object GetStore(Type entityType, IServiceProvider scope)
        {
            if (entityType == null)
                throw new ArgumentNullException(nameof(entityType));

            var serviceType = typeof(IEntityStore<>).MakeGenericType(entityType);
            var resolved = scope.GetService(serviceType);

            if (resolved != null)
                return resolved;

            var fallbackType = typeof(InMemoryEntityStoreFactory<>).MakeGenericType(entityType);
            return Activator.CreateInstance(fallbackType)!;
        }
    }
}