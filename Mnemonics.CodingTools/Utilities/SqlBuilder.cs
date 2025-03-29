using System.Linq;
using System.Reflection;

namespace Mnemonics.CodingTools.Utilities
{
    /// <summary>
    /// Provides utility methods for generating SQL statements for the entity type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The entity type for which SQL statements are generated. Must be a class.</typeparam>
    public static class SqlBuilder<T> where T : class
    {
        private static readonly PropertyInfo[] _properties = typeof(T).GetProperties();
        private static readonly PropertyInfo[] _keyProperties = SqlTypeUtilities.GetKeyProperties(typeof(T)).ToArray();

        /// <summary>
        /// Builds an <c>INSERT</c> SQL statement for the entity type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="tableName">The name of the table into which the entity will be inserted.</param>
        /// <returns>A SQL string representing the insert command.</returns>
        public static string BuildInsertSql(string tableName)
        {
            var columns = string.Join(", ", _properties.Select(p => Quote(p.Name)));
            var values = string.Join(", ", _properties.Select(p => "@" + p.Name));
            return $"INSERT INTO {Quote(tableName)} ({columns}) VALUES ({values});";
        }

        /// <summary>
        /// Builds a <c>DELETE</c> SQL statement for the entity type <typeparamref name="T"/>,
        /// using the primary or composite key properties in the <c>WHERE</c> clause.
        /// </summary>
        /// <param name="tableName">The name of the table from which the entity will be deleted.</param>
        /// <returns>A SQL string representing the delete command.</returns>
        public static string BuildDeleteSql(string tableName)
        {
            var whereClause = BuildWhereClause();
            return $"DELETE FROM {Quote(tableName)} WHERE {whereClause};";
        }

        /// <summary>
        /// Builds a <c>SELECT</c> SQL statement to fetch a single entity of type <typeparamref name="T"/>,
        /// filtered by its primary or composite key.
        /// </summary>
        /// <param name="tableName">The name of the table from which the entity will be selected.</param>
        /// <returns>A SQL string representing the select command.</returns>
        public static string BuildSelectSql(string tableName)
        {
            var whereClause = BuildWhereClause();
            return $"SELECT * FROM {Quote(tableName)} WHERE {whereClause} LIMIT 1;";
        }

        /// <summary>
        /// Builds a <c>WHERE</c> clause for the entity type <typeparamref name="T"/> using its key properties.
        /// </summary>
        /// <returns>A SQL <c>WHERE</c> clause matching all key fields.</returns>
        public static string BuildWhereClause()
        {
            return string.Join(" AND ", _keyProperties.Select(p => $"{Quote(p.Name)} = @{p.Name}"));
        }

        /// <summary>
        /// Builds a <c>CREATE TABLE</c> SQL statement for the entity type <typeparamref name="T"/>,
        /// including column types and nullability, and defining the primary key.
        /// </summary>
        /// <param name="tableName">The name of the table to create.</param>
        /// <returns>A SQL string representing the table creation command.</returns>
        public static string BuildCreateTableSql(string tableName)
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
        /// Quotes an identifier (e.g., column or table name) to prevent conflicts with SQL reserved words.
        /// </summary>
        private static string Quote(string identifier) => $"\"{identifier}\"";
    }
}