using System;
using System.Collections.Generic;
using Mnemonics.CodingTools.Interfaces;

namespace Mnemonics.CodingTools
{
    /// <summary>
    /// Provides an in-memory implementation of <see cref="IDynamicTypeRegistry"/> for tracking entity types dynamically.
    /// </summary>
    public class DynamicTypeRegistry : IDynamicTypeRegistry
    {
        private readonly HashSet<Type> _types = [];

        /// <inheritdoc />
        public IEnumerable<Type> GetTypes() => _types;

        /// <inheritdoc />
        public void RegisterType(Type type)
        {
            _types.Add(type);
        }
    }
}