using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Mnemonics.CodingTools.Interfaces;

namespace Mnemonics.CodingTools.Storage
{
    /// <summary>
    /// Provides a Dapper-based implementation of <see cref="IEntityStore{T}"/> for lightweight database access.
    /// Assumes each entity is stored in a table with a primary key column named 'Id'.
    /// </summary>
    /// <typeparam name="T">The entity type to store. Must have a property named 'Id'.</typeparam>
    public class DapperEntityStore<T> : IEntityStore<T> where T : class
    {
        private readonly Func<IDbConnection> _connectionFactory;
        private readonly string _tableName;

        /// <summary>
        /// Initializes a new instance of the <see cref="DapperEntityStore{T}"/> class.
        /// </summary>
        /// <param name="connectionFactory">A factory method to create new <see cref="IDbConnection"/> instances.</param>
        /// <param name="tableName">The name of the table for storing entities.</param>
        public DapperEntityStore(Func<IDbConnection> connectionFactory, string tableName)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
        }

        /// <inheritdoc/>
        public async Task SaveAsync(string id, T entity)
        {
            using var connection = _connectionFactory();
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead).ToList();

            var columnNames = string.Join(", ", props.Select(p => p.Name));
            var paramNames = string.Join(", ", props.Select(p => "@" + p.Name));

            var sb = new StringBuilder();
            sb.AppendLine($"DELETE FROM {_tableName} WHERE Id = @Id;");
            sb.AppendLine($"INSERT INTO {_tableName} ({columnNames}) VALUES ({paramNames});");

            var parameters = new DynamicParameters(entity);
            parameters.Add("Id", id);

            await connection.ExecuteAsync(sb.ToString(), parameters);
        }

        /// <inheritdoc/>
        public async Task<T?> LoadAsync(string id)
        {
            using var connection = _connectionFactory();
            return await connection.QueryFirstOrDefaultAsync<T>(
                $"SELECT * FROM {_tableName} WHERE Id = @Id", new { Id = id });
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(string id)
        {
            using var connection = _connectionFactory();
            var rowsAffected = await connection.ExecuteAsync(
                $"DELETE FROM {_tableName} WHERE Id = @Id", new { Id = id });
            return rowsAffected > 0;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<T>> ListAsync()
        {
            using var connection = _connectionFactory();
            return await connection.QueryAsync<T>($"SELECT * FROM {_tableName}");
        }
    }
}