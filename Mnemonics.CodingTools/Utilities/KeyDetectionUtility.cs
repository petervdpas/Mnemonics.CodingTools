using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mnemonics.CodingTools.Annotations;

namespace Mnemonics.CodingTools.Utilities
{
    /// <summary>
    /// Provides helper methods to detect key properties on dynamically generated types.
    /// </summary>
    public static class KeyDetectionUtility
    {
        /// <summary>
        /// Gets the list of key properties for a given type.
        /// Detection is based on the presence of the <see cref="IsKeyFieldAttribute"/>,
        /// or by matching property names against fallback key names using case-insensitive
        /// comparison and suffix matching.
        /// </summary>
        public static List<PropertyInfo> GetKeyProperties(Type type, List<string> fallbackKeyNames)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (fallbackKeyNames == null)
                throw new ArgumentNullException(nameof(fallbackKeyNames));

            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var annotated = props
                .Where(p => Attribute.IsDefined(p, typeof(IsKeyFieldAttribute), inherit: true))
                .ToList();

            if (annotated.Any())
                return annotated;

            var fallback = props
                .Where(p =>
                    fallbackKeyNames.Any(name =>
                        string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase) ||
                        p.Name.EndsWith(name, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            return fallback;
        }

        /// <summary>
        /// Gets the name of the first key property detected for a type.
        /// </summary>
        /// <param name="type">The entity type to inspect.</param>
        /// <param name="fallbackKeyNames">Fallback key names to check if no attribute is present.</param>
        /// <returns>The name of the first matching key property, or <c>null</c> if none found.</returns>
        public static string? GetSingleKeyName(Type type, List<string> fallbackKeyNames)
        {
            return GetKeyProperties(type, fallbackKeyNames).FirstOrDefault()?.Name;
        }
    }
}
