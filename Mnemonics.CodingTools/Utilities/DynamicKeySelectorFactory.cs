using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Mnemonics.CodingTools.Utilities
{
    /// <summary>
    /// Provides a utility for generating a key selector function for use with file-based or Dapper stores.
    /// </summary>
    public static class DynamicKeySelectorFactory
    {
        /// <summary>
        /// Creates a strongly-typed key selector for the specified entity type.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="fallbackKeyNames">A list of fallback key names (e.g. "Id").</param>
        /// <returns>A function that returns string[] of key values from the entity.</returns>
        public static Func<T, string[]> CreateSelector<T>(List<string> fallbackKeyNames) where T : class
        {
            if (fallbackKeyNames == null)
                throw new ArgumentNullException(nameof(fallbackKeyNames));

            var type = typeof(T);
            var keyProps = KeyDetectionUtility.GetKeyProperties(type, fallbackKeyNames);

            if (keyProps.Count == 0)
                throw new InvalidOperationException(
                    $"No key properties found on type '{type.Name}'. " +
                    "Use [IsKeyField] or match fallback key names.");

            return entity =>
            {
                ArgumentNullException.ThrowIfNull(entity);
                return [.. keyProps.Select(p => p.GetValue(entity)?.ToString() ?? "")];
            };
        }

        /// <summary>
        /// Creates a key selector dynamically using reflection and returns it as an object.
        /// </summary>
        /// <param name="entityType">The entity type.</param>
        /// <returns>A boxed delegate that returns key strings from an entity.</returns>
        public static object CreateSelector(Type entityType)
        {
            if (entityType == null)
                throw new ArgumentNullException(nameof(entityType));

            var method = typeof(DynamicKeySelectorFactory)
                .GetMethod(nameof(CreateSelector), BindingFlags.Public | BindingFlags.Static)!
                .MakeGenericMethod(entityType);

            return method.Invoke(null, [KeyDetectionUtility.GetKeyProperties(entityType, ["Id"])])!;
        }
    }
}