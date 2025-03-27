using System;
using System.Collections.Generic;

namespace Mnemonics.CodingTools.Interfaces
{
    /// <summary>
    /// Defines a contract for managing dynamic entity types for registration in Entity Framework Core.
    /// </summary>
    public interface IDynamicTypeRegistry
    {
        /// <summary>
        /// Gets all registered entity types.
        /// </summary>
        /// <returns>A collection of registered <see cref="Type"/> objects.</returns>
        IEnumerable<Type> GetTypes();

        /// <summary>
        /// Registers a new entity type.
        /// </summary>
        /// <param name="type">The type to register.</param>
        void RegisterType(Type type);
    }
}