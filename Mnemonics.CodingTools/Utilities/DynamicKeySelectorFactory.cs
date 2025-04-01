using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mnemonics.CodingTools.Annotations;

namespace Mnemonics.CodingTools.Utilities
{
    /// <summary>
    /// Provides a utility for generating a key selector function for use with file-based stores.
    /// </summary>
    public static class DynamicKeySelectorFactory
    {
        /// <summary>
        /// Creates a strongly-typed key selector for the specified entity type.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <returns>A function that returns string[] of key values.</returns>
        public static Func<T, string[]> CreateSelector<T>(List<string> fallbackKeyNames) where T : class
        {
            var type = typeof(T);
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // First: use attribute
            var annotated = props.Where(p => Attribute.IsDefined(p, typeof(IsKeyFieldAttribute))).ToArray();
            if (annotated.Length > 0)
            {
                return entity =>
                {
                    ArgumentNullException.ThrowIfNull(entity);
                    return annotated.Select(p => p.GetValue(entity)?.ToString() ?? "").ToArray();
                };
            }

            // Fallback: name-based
            var fallback = props.Where(p =>
                fallbackKeyNames.Any(name =>
                    string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase) ||
                    p.Name.EndsWith(name, StringComparison.OrdinalIgnoreCase)))
                .ToArray();

            if (fallback.Length == 0)
                throw new InvalidOperationException(
                    $"No key properties found on type '{type.Name}'. Use [IsKeyField] or match fallback key names.");

            return entity =>
            {
                ArgumentNullException.ThrowIfNull(entity);
                return fallback.Select(p => p.GetValue(entity)?.ToString() ?? "").ToArray();
            };
        }

        /// <summary>
        /// Creates a key selector dynamically using reflection and returns it boxed.
        /// </summary>
        /// <param name="entityType">The entity type.</param>
        /// <returns>A compiled key selector delegate (as object).</returns>
        public static object CreateSelector(Type entityType)
        {
            var method = typeof(DynamicKeySelectorFactory)
                .GetMethod(nameof(CreateSelector), BindingFlags.Public | BindingFlags.Static)!
                .MakeGenericMethod(entityType);

            return method.Invoke(null, null)!;
        }
    }
}