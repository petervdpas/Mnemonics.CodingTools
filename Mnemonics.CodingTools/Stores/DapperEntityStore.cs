using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Dapper;
using Mnemonics.CodingTools.Interfaces;
using Mnemonics.CodingTools.Utilities;

namespace Mnemonics.CodingTools.Storage
{
    /// <summary>
    /// Provides a Dapper-based implementation of <see cref="IEntityStore{T}"/>.
    /// Supports auto-creating tables with inferred schema and composite keys.
    /// </summary>
    /// <typeparam name="T">The entity type to store. Must have one or more public key properties (e.g., "Id", "SomethingId").</typeparam>
    public class DapperEntityStore<T> : IEntityStore<T> where T : class
    {
        private readonly Func<IDbConnection> _connectionFactory;
        private readonly string _tableName;
        private readonly PropertyInfo[] _keyProperties;

        /// <summary>
        /// Initializes a new instance of the <see cref="DapperEntityStore{T}"/> class.
        /// </summary>
        /// <param name="connectionFactory">Factory that provides a new database connection.</param>
        /// <param name="tableName">Optional custom table name; defaults to the type name of <typeparamref name="T"/>.</param>
        public DapperEntityStore(Func<IDbConnection> connectionFactory, string? tableName = null)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _tableName = tableName ?? typeof(T).Name;
            _keyProperties = SqlTypeUtilities.GetKeyProperties(typeof(T)).ToArray();

            if (_keyProperties.Length == 0)
                throw new InvalidOperationException($"Entity type {typeof(T).Name} must have one or more 'Id' or '*Id' properties.");

            EnsureTableExists();
        }

        /// <inheritdoc/>
        public async Task SaveAsync(string id, T entity) => await SaveInternalAsync(entity);

        /// <inheritdoc/>
        public async Task SaveAsync(string[] keys, T entity)
        {
            if (keys is null || keys.Length == 0)
                throw new ArgumentException("Composite keys cannot be null or empty.", nameof(keys));

            await SaveInternalAsync(entity);
        }

        /// <inheritdoc/>
        public async Task<T?> LoadAsync(string id)
        {
            var idProp = _keyProperties.FirstOrDefault(p => string.Equals(p.Name, "Id", StringComparison.OrdinalIgnoreCase));
            if (idProp == null)
                throw new NotSupportedException($"LoadAsync(string id) requires a property named 'Id'.");

            using var connection = _connectionFactory();
            var sql = $"SELECT * FROM \"{_tableName}\" WHERE \"{idProp.Name}\" = @Id LIMIT 1;";
            return await connection.QueryFirstOrDefaultAsync<T>(sql, new { Id = id });
        }

        /// <inheritdoc/>
        public async Task<T?> LoadAsync(params object[] keys)
        {
            ValidateKeyCount(keys);
            var param = MapKeysToParameters(keys);
            var sql = SqlBuilder<T>.BuildSelectSql(_tableName);

            using var connection = _connectionFactory();
            return await connection.QueryFirstOrDefaultAsync<T>(sql, param);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(string id)
        {
            var idProp = _keyProperties.FirstOrDefault(p => string.Equals(p.Name, "Id", StringComparison.OrdinalIgnoreCase));
            if (idProp == null)
                throw new NotSupportedException($"DeleteAsync(string id) requires a property named 'Id'.");

            using var connection = _connectionFactory();
            var sql = $"DELETE FROM \"{_tableName}\" WHERE \"{idProp.Name}\" = @Id;";
            var rows = await connection.ExecuteAsync(sql, new { Id = id });
            return rows > 0;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(params object[] keys)
        {
            ValidateKeyCount(keys);
            var param = MapKeysToParameters(keys);
            var sql = SqlBuilder<T>.BuildDeleteSql(_tableName);

            using var connection = _connectionFactory();
            var rows = await connection.ExecuteAsync(sql, param);
            return rows > 0;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<T>> ListAsync()
        {
            using var connection = _connectionFactory();
            return await connection.QueryAsync<T>($"SELECT * FROM \"{_tableName}\";");
        }

        /// <inheritdoc/>
        public async Task InsertAsync(IEnumerable<T> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            var insertSql = SqlBuilder<T>.BuildInsertSql(_tableName);

            using var connection = _connectionFactory();
            foreach (var item in items)
                await connection.ExecuteAsync(insertSql, item);
        }

        private async Task SaveInternalAsync(T entity)
        {
            using var connection = _connectionFactory();
            await connection.ExecuteAsync(SqlBuilder<T>.BuildDeleteSql(_tableName), entity);
            await connection.ExecuteAsync(SqlBuilder<T>.BuildInsertSql(_tableName), entity);
        }

        private DynamicParameters MapKeysToParameters(object[] keys)
        {
            var param = new DynamicParameters();
            for (int i = 0; i < keys.Length; i++)
                param.Add(_keyProperties[i].Name, keys[i]);

            return param;
        }

        private void ValidateKeyCount(object[] keys)
        {
            if (keys.Length != _keyProperties.Length)
                throw new ArgumentException($"Expected {_keyProperties.Length} keys but got {keys.Length}.");
        }

        private void EnsureTableExists()
        {
            using var connection = _connectionFactory();
            connection.Execute(SqlBuilder<T>.BuildCreateTableSql(_tableName));
        }
    }
}
