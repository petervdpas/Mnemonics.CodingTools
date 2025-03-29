using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mnemonics.CodingTools.Interfaces;

namespace Mnemonics.CodingTools.Stores
{
    /// <summary>
    /// Provides a thread-safe in-memory implementation of <see cref="IEntityStore{T}"/>,
    /// supporting composite keys and batch operations.
    /// </summary>
    /// <typeparam name="T">The type of entity to store.</typeparam>
    public class InMemoryEntityStore<T> : IEntityStore<T> where T : class
    {
        private readonly ConcurrentDictionary<string, T> _store = new();

        private static string ComposeKey(params object[] keys)
        {
            if (keys == null || keys.Length == 0)
                throw new ArgumentException("Key(s) must not be null or empty.");

            return string.Join("::", keys.Select(k => k?.ToString() ?? string.Empty));
        }

        /// <inheritdoc />
        public Task SaveAsync(string id, T entity)
        {
            _store[id] = entity;
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task SaveAsync(string[] keys, T entity)
        {
            var compositeKey = ComposeKey(keys);
            _store[compositeKey] = entity;
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<T?> LoadAsync(string id)
        {
            _store.TryGetValue(id, out var value);
            return Task.FromResult(value);
        }

        /// <inheritdoc />
        public Task<T?> LoadAsync(params object[] keys)
        {
            var compositeKey = ComposeKey(keys);
            _store.TryGetValue(compositeKey, out var value);
            return Task.FromResult(value);
        }

        /// <inheritdoc />
        public Task<bool> DeleteAsync(string id)
        {
            return Task.FromResult(_store.TryRemove(id, out _));
        }

        /// <inheritdoc />
        public Task<bool> DeleteAsync(params object[] keys)
        {
            var compositeKey = ComposeKey(keys);
            return Task.FromResult(_store.TryRemove(compositeKey, out _));
        }

        /// <inheritdoc />
        public Task<IEnumerable<T>> ListAsync()
        {
            return Task.FromResult<IEnumerable<T>>(_store.Values.ToList());
        }

        /// <inheritdoc />
        public Task InsertAsync(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                var key = typeof(T).GetProperty("Id")?.GetValue(item)?.ToString();
                if (string.IsNullOrWhiteSpace(key))
                    throw new InvalidOperationException("Cannot insert entity: missing 'Id' property.");

                _store[key] = item;
            }
            return Task.CompletedTask;
        }
    }
}