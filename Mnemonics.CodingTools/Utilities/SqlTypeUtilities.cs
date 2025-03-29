using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Mnemonics.CodingTools.Utilities
{
    /// <summary>
    /// Provides helper methods for mapping .NET types to SQL types,
    /// checking nullability, and inferring schema metadata.
    /// </summary>
    public static class SqlTypeUtilities
    {
        /// <summary>
        /// Maps a .NET type to an appropriate SQL type.
        /// </summary>
        public static string GetSqlType(Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;

            return type switch
            {
                _ when type == typeof(string)   => "TEXT",
                _ when type == typeof(int)      => "INTEGER",
                _ when type == typeof(long)     => "BIGINT",
                _ when type == typeof(bool)     => "BOOLEAN",
                _ when type == typeof(DateTime) => "DATETIME",
                _ when type == typeof(double)   => "REAL",
                _ => throw new NotSupportedException($"Type '{type.Name}' is not supported in auto table creation.")
            };
        }

        /// <summary>
        /// Determines whether a property is nullable (either a reference type or Nullable&lt;T&gt;).
        /// </summary>
        public static bool IsNullable(PropertyInfo prop)
        {
            if (!prop.PropertyType.IsValueType) return true;
            return Nullable.GetUnderlyingType(prop.PropertyType) != null;
        }

        /// <summary>
        /// Returns properties considered keys: "Id", or those ending in "Id" (case-insensitive).
        /// </summary>
        public static IEnumerable<PropertyInfo> GetKeyProperties(Type type)
        {
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var keyCandidates = props.Where(p =>
                string.Equals(p.Name, "Id", StringComparison.OrdinalIgnoreCase) ||
                p.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase)).ToList();

            if (keyCandidates.Count == 0)
            {
                throw new InvalidOperationException(
                    $"Type '{type.Name}' must have at least one 'Id' or '*Id' property for key inference.");
            }

            return keyCandidates;
        }

        /// <summary>
        /// Builds a SQL PRIMARY KEY clause for one or more properties.
        /// </summary>
        /// <param name="keyProps">The key properties.</param>
        /// <param name="quoteFunc">A function to quote each column name.</param>
        /// <returns>The PRIMARY KEY clause for use in a CREATE TABLE statement.</returns>
        public static string BuildPrimaryKeyClause(IEnumerable<PropertyInfo> keyProps, Func<string, string>? quoteFunc = null)
        {
            quoteFunc ??= s => s; // no quoting if none provided
            var columnNames = keyProps.Select(p => quoteFunc(p.Name));
            return $"PRIMARY KEY ({string.Join(", ", columnNames)})";
        }
    }
}