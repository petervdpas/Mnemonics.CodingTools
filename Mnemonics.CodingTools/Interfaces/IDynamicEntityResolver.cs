using System;

namespace Mnemonics.CodingTools.Interfaces
{
    /// <summary>
    /// Provides access to dynamically resolved IEntityStore instances for arbitrary entity types.
    /// </summary>
    public interface IDynamicEntityResolver
    {
        /// <summary>
        /// Resolves an IEntityStore for the specified entity type.
        /// </summary>
        /// <param name="entityType">The entity type to resolve.</param>
        /// <returns>The IEntityStore instance.</returns>
        object GetStore(Type entityType);
    }
}
