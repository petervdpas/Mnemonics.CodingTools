using System;
using System.Linq;
using System.Reflection;
using Mnemonics.CodingTools.Models;

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

            // Look for properties with [FieldWithAttributes(IsDisplayField = true)]
            var keyProps = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetCustomAttribute<FieldWithAttributes>()?.IsKeyField == true)
                .ToArray();

            if (keyProps.Length == 0)
                throw new InvalidOperationException($"No key properties found on type '{type.Name}'. Use IsDisplayField=true in your schema.");

            return entity =>
            {
                if (entity == null) throw new ArgumentNullException(nameof(entity));
                return [.. keyProps.Select(p => p.GetValue(entity)?.ToString() ?? "")];
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