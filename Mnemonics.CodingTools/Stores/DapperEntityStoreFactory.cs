using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Mnemonics.CodingTools.Configuration;
using Mnemonics.CodingTools.Interfaces;
using Mnemonics.CodingTools.Storage;

namespace Mnemonics.CodingTools.Stores
{
    /// <summary>
    /// Factory wrapper for <see cref="DapperEntityStore{T}"/> that supports dependency injection.
    /// Delegates all <see cref="IEntityStore{T}"/> operations to the internal store instance.
    /// </summary>
    /// <typeparam name="T">The entity type to store.</typeparam>
    public class DapperEntityStoreFactory<T> : IEntityStore<T> where T : class
    {
        private readonly DapperEntityStore<T> _inner;

        /// <summary>
        /// Initializes a new instance of the <see cref="DapperEntityStoreFactory{T}"/> class.
        /// </summary>
        /// <param name="connectionFactory">A factory that provides an open <see cref="System.Data.IDbConnection"/>.</param>
        /// <param name="options">The <see cref="CodingToolsOptions"/> used to configure the store (directory, serialization, etc.).</param>
        public DapperEntityStoreFactory(Func<IDbConnection> connectionFactory, CodingToolsOptions options)
        {
            var fallbackKeys = options.GlobalFallbackKeyNames;
            _inner = new DapperEntityStore<T>(connectionFactory, null, fallbackKeys);
        }

        /// <inheritdoc/>
        public Task SaveAsync(string id, T entity) => _inner.SaveAsync(id, entity);

        /// <inheritdoc/>
        public Task SaveAsync(string[] keys, T entity) => _inner.SaveAsync(keys, entity);

        /// <inheritdoc/>
        public Task<T?> LoadAsync(string id) => _inner.LoadAsync(id);

        /// <inheritdoc/>
        public Task<T?> LoadAsync(params object[] keys) => _inner.LoadAsync(keys);

        /// <inheritdoc/>
        public Task<bool> DeleteAsync(string id) => _inner.DeleteAsync(id);

        /// <inheritdoc/>
        public Task<bool> DeleteAsync(params object[] keys) => _inner.DeleteAsync(keys);

        /// <inheritdoc/>
        public Task<IEnumerable<T>> ListAsync() => _inner.ListAsync();

        /// <inheritdoc/>
        public Task InsertAsync(IEnumerable<T> items) => _inner.InsertAsync(items);
    }
}