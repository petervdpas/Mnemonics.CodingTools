using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Mnemonics.CodingTools.Utilities
{
    /// <summary>
    /// Generates SQL statements for a given entity type <typeparamref name="T"/> using reflection.
    /// Supports custom fallback key names for determining primary keys.
    /// </summary>
    /// <typeparam name="T">The entity type for which SQL statements will be generated.</typeparam>
    public class SqlBuilder<T> where T : class
    {
        private readonly PropertyInfo[] _properties;
        private readonly PropertyInfo[] _keyProperties;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlBuilder{T}"/> class using provided fallback key names.
        /// </summary>
        /// <param name="fallbackKeyNames">
        /// A list of fallback key names (e.g., "Id", "Bk") used when [IsKeyField] attributes are not present.
        /// </param>
        public SqlBuilder(List<string>? fallbackKeyNames = null)
        {
            _properties = typeof(T).GetProperties();
            _keyProperties = [.. SqlTypeUtilities.GetKeyProperties(typeof(T), fallbackKeyNames)];
        }

        /// <summary>
        /// Builds an <c>INSERT</c> SQL statement for inserting the entity into the specified table.
        /// </summary>
        /// <param name="tableName">The name of the table to insert into.</param>
        /// <returns>A SQL INSERT command string.</returns>
        public string BuildInsertSql(string tableName)
        {
            var columns = string.Join(", ", _properties.Select(p => Quote(p.Name)));
            var values = string.Join(", ", _properties.Select(p => "@" + p.Name));
            return $"INSERT INTO {Quote(tableName)} ({columns}) VALUES ({values});";
        }

        /// <summary>
        /// Builds a <c>DELETE</c> SQL statement for deleting an entity by its key(s).
        /// </summary>
        /// <param name="tableName">The name of the table to delete from.</param>
        /// <returns>A SQL DELETE command string.</returns>
        public string BuildDeleteSql(string tableName)
        {
            var whereClause = BuildWhereClause();
            return $"DELETE FROM {Quote(tableName)} WHERE {whereClause};";
        }

        /// <summary>
        /// Builds a <c>SELECT</c> SQL statement to load an entity by its key(s).
        /// </summary>
        /// <param name="tableName">The name of the table to select from.</param>
        /// <returns>A SQL SELECT command string with WHERE clause and LIMIT 1.</returns>
        public string BuildSelectSql(string tableName)
        {
            var whereClause = BuildWhereClause();
            return $"SELECT * FROM {Quote(tableName)} WHERE {whereClause} LIMIT 1;";
        }

        /// <summary>
        /// Builds the WHERE clause used for SELECT/DELETE based on key properties.
        /// </summary>
        /// <returns>A SQL WHERE clause string using all key properties.</returns>
        public string BuildWhereClause()
        {
            return string.Join(" AND ", _keyProperties.Select(p => $"{Quote(p.Name)} = @{p.Name}"));
        }

        /// <summary>
        /// Builds a <c>CREATE TABLE</c> SQL statement that defines the schema and primary key.
        /// </summary>
        /// <param name="tableName">The name of the table to create.</param>
        /// <returns>A SQL CREATE TABLE command string.</returns>
        public string BuildCreateTableSql(string tableName)
        {
            var columns = _properties.Select(p =>
            {
                var sqlType = SqlTypeUtilities.GetSqlType(p.PropertyType);
                var nullable = SqlTypeUtilities.IsNullable(p) ? "NULL" : "NOT NULL";
                return $"{Quote(p.Name)} {sqlType} {nullable}";
            });

            var pk = SqlTypeUtilities.BuildPrimaryKeyClause(_keyProperties, Quote);

            return $"""
                CREATE TABLE IF NOT EXISTS {Quote(tableName)} (
                    {string.Join(",\n    ", columns)},
                    {pk}
                );
            """;
        }

        /// <summary>
        /// Quotes a SQL identifier to prevent conflicts with reserved keywords.
        /// </summary>
        /// <param name="identifier">The name of the column or table.</param>
        /// <returns>The quoted identifier (e.g., "Name").</returns>
        private static string Quote(string identifier) => $"\"{identifier}\"";
    }
}