using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mnemonics.CodingTools.Annotations;

namespace Mnemonics.CodingTools.Utilities
{
    /// <summary>
    /// Provides helper methods for mapping .NET types to SQL types,
    /// checking nullability, and inferring schema metadata.
    /// </summary>
    public static class SqlTypeUtilities
    {
        /// <summary>
        /// Gets or sets the global fallback property names to be used for key inference
        /// when no explicit <see cref="IsKeyFieldAttribute"/> is present.
        /// </summary>
        /// <remarks>
        /// This list is checked in order, matching either exact names or name suffixes.
        /// </remarks>
        public static List<string> DefaultFallbackNames { get; set; } = ["Id"];

        /// <summary>
        /// Maps a .NET type to a corresponding SQL column type.
        /// </summary>
        /// <param name="type">The .NET type to convert.</param>
        /// <returns>The SQL type as a string (e.g., TEXT, INTEGER, etc.).</returns>
        /// <exception cref="NotSupportedException">Thrown if the type is not supported.</exception>
        public static string GetSqlType(Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;

            return type switch
            {
                _ when type == typeof(string) => "TEXT",
                _ when type == typeof(int) => "INTEGER",
                _ when type == typeof(long) => "BIGINT",
                _ when type == typeof(bool) => "BOOLEAN",
                _ when type == typeof(DateTime) => "DATETIME",
                _ when type == typeof(double) => "REAL",
                _ => throw new NotSupportedException($"Type '{type.Name}' is not supported in auto table creation.")
            };
        }

        /// <summary>
        /// Determines whether a given property is nullable.
        /// This includes reference types and Nullable value types.
        /// </summary>
        /// <param name="prop">The property to inspect.</param>
        /// <returns><c>true</c> if the property is nullable; otherwise, <c>false</c>.</returns>
        public static bool IsNullable(PropertyInfo prop)
        {
            if (!prop.PropertyType.IsValueType) return true;
            return Nullable.GetUnderlyingType(prop.PropertyType) != null;
        }

        /// <summary>
        /// Identifies key properties for the specified type using either
        /// the <see cref="IsKeyFieldAttribute"/> or fallback naming conventions.
        /// </summary>
        /// <param name="type">The type to inspect for key properties.</param>
        /// <param name="fallbackNames">Optional fallback names to use instead of <see cref="DefaultFallbackNames"/>.</param>
        /// <returns>A sequence of <see cref="PropertyInfo"/> representing key properties.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if no valid key property is found via attributes or fallback logic.
        /// </exception>
        public static IEnumerable<PropertyInfo> GetKeyProperties(Type type, List<string>? fallbackNames = null)
        {
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var annotated = props.Where(p => Attribute.IsDefined(p, typeof(IsKeyFieldAttribute))).ToList();
            if (annotated.Count > 0)
                return annotated;

            fallbackNames ??= DefaultFallbackNames;

            var keyCandidates = props.Where(p =>
                fallbackNames.Any(name =>
                    string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase) ||
                    p.Name.EndsWith(name, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (keyCandidates.Count == 0)
                throw new InvalidOperationException(
                    $"Type '{type.Name}' must have at least one key property via [IsKeyField] or fallback names.");

            return keyCandidates;
        }

        /// <summary>
        /// Builds a SQL <c>PRIMARY KEY</c> clause using the specified key properties.
        /// </summary>
        /// <param name="keyProps">The key properties to include in the clause.</param>
        /// <param name="quoteFunc">Optional function to quote column names (defaults to identity).</param>
        /// <returns>A SQL <c>PRIMARY KEY</c> clause string.</returns>
        public static string BuildPrimaryKeyClause(IEnumerable<PropertyInfo> keyProps, Func<string, string>? quoteFunc = null)
        {
            quoteFunc ??= s => s; // no quoting if none provided
            var columnNames = keyProps.Select(p => quoteFunc(p.Name));
            return $"PRIMARY KEY ({string.Join(", ", columnNames)})";
        }
    }
}