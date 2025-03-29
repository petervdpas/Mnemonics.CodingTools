using System.Collections.Generic;
using System.Threading.Tasks;
using Mnemonics.CodingTools.Interfaces;

namespace Mnemonics.CodingTools.Stores
{
    /// <summary>
    /// Factory wrapper for <see cref="InMemoryEntityStore{T}"/> that supports dependency injection.
    /// Delegates all <see cref="IEntityStore{T}"/> operations to an internal instance.
    /// </summary>
    /// <typeparam name="T">The type of entity to store.</typeparam>
    public class InMemoryEntityStoreFactory<T> : IEntityStore<T> where T : class
    {
        private readonly InMemoryEntityStore<T> _inner = new();

        /// <inheritdoc/>
        public Task SaveAsync(string id, T entity) => _inner.SaveAsync(id, entity);

        /// <inheritdoc/>
        public Task SaveAsync(string[] keys, T entity) => _inner.SaveAsync(keys, entity);

        /// <inheritdoc/>
        public Task InsertAsync(IEnumerable<T> items) => _inner.InsertAsync(items);

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
    }
}