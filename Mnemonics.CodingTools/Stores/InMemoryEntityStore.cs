using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mnemonics.CodingTools.Interfaces;

namespace Mnemonics.CodingTools.Stores
{
    /// <summary>
    /// Provides a thread-safe in-memory implementation of <see cref="IEntityStore{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of entity to store.</typeparam>
    public class InMemoryEntityStore<T> : IEntityStore<T> where T : class
    {
        private readonly ConcurrentDictionary<string, T> _store = new();

        /// <inheritdoc />
        public Task SaveAsync(string id, T entity)
        {
            _store[id] = entity;
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<T?> LoadAsync(string id)
        {
            _store.TryGetValue(id, out var value);
            return Task.FromResult(value);
        }

        /// <inheritdoc />
        public Task<bool> DeleteAsync(string id)
        {
            return Task.FromResult(_store.TryRemove(id, out _));
        }

        /// <inheritdoc />
        public Task<IEnumerable<T>> ListAsync()
        {
            return Task.FromResult<IEnumerable<T>>(_store.Values.ToList());
        }
    }
}
