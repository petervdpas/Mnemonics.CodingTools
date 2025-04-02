using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mnemonics.CodingTools.Interfaces;
using Mnemonics.CodingTools.Storage;

namespace Mnemonics.CodingTools.Stores
{
    /// <summary>
    /// Factory wrapper for <see cref="DbEntityStore{T}"/> that supports dependency injection.
    /// Delegates all <see cref="IEntityStore{T}"/> operations to the internal store instance.
    /// </summary>
    /// <typeparam name="T">The entity type to store. Can be statically known or dynamically compiled at runtime.</typeparam>
    public class DbEntityStoreFactory<T> : IEntityStore<T> where T : class
    {
        private readonly DbEntityStore<T> _inner;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbEntityStoreFactory{T}"/> class.
        /// </summary>
        /// <param name="context">
        /// The EF Core context implementing <see cref="IDbEntityStoreContext"/>, capable of resolving runtime types.
        /// </param>
        public DbEntityStoreFactory(IDbEntityStoreContext context)
        {
            _inner = context != null
                ? new DbEntityStore<T>(context)
                : throw new ArgumentNullException(nameof(context));
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
