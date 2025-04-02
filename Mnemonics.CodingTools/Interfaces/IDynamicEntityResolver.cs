using System;

namespace Mnemonics.CodingTools.Interfaces
{
    /// <summary>
    /// Provides access to dynamically resolved IEntityStore instances for arbitrary entity types.
    /// </summary>
    public interface IDynamicEntityResolver
    {
        /// <summary>
        /// Resolves an <see cref="IEntityStore{T}"/> for the specified entity type using the provided service scope.
        /// </summary>
        /// <param name="entityType">The entity type to resolve.</param>
        /// <param name="scope">A scoped service provider used to resolve the store instance.</param>
        /// <returns>The resolved <see cref="IEntityStore{T}"/> instance.</returns>
        object GetStore(Type entityType, IServiceProvider scope);
    }
}
