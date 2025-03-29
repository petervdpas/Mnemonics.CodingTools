using System;
using System.Linq;
using System.Reflection;

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
        public static Func<T, string[]> CreateSelector<T>() where T : class
        {
            var type = typeof(T);
            var properties = type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => string.Equals(p.Name, "Id", StringComparison.OrdinalIgnoreCase) ||
                            p.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (properties.Length == 0)
                throw new InvalidOperationException($"No key properties found on type '{type.Name}'.");

            return entity =>
            {
                if (entity == null) throw new ArgumentNullException(nameof(entity));
                return properties.Select(p => p.GetValue(entity)?.ToString() ?? "").ToArray();
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