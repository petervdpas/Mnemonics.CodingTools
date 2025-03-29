using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Mnemonics.CodingTools.Configuration;
using Mnemonics.CodingTools.Interfaces;
using Mnemonics.CodingTools.Utilities;

namespace Mnemonics.CodingTools.Stores
{
    /// <summary>
    /// A factory-based wrapper for <see cref="FileEntityStore{T}"/> that supports dependency injection.
    /// Provides a default key selector via <see cref="DynamicKeySelectorFactory"/> and delegates all operations
    /// to the inner <see cref="FileEntityStore{T}"/>.
    /// </summary>
    /// <typeparam name="T">The entity type to store. Must be serializable to JSON.</typeparam>
    public class FileEntityStoreFactory<T> : IAdvancedEntityStore<T> where T : class
    {
        private readonly FileEntityStore<T> _inner;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileEntityStoreFactory{T}"/> class.
        /// </summary>
        /// <param name="options">The <see cref="CodingToolsOptions"/> used to configure the store (directory, serialization, etc.).</param>
        public FileEntityStoreFactory(CodingToolsOptions options)
        {
            var dir = options.FileStoreDirectory ?? Path.Combine(AppContext.BaseDirectory, "EntityStore");

            var jsonOptions = options.JsonOptionsPerEntity.TryGetValue(typeof(T), out var perEntityOptions)
                ? perEntityOptions
                : new JsonSerializerOptions { WriteIndented = true };

            var keySelector = options.CustomKeySelectors.TryGetValue(typeof(T), out var customSelector)
                ? (Func<T, string[]>)customSelector
                : DynamicKeySelectorFactory.CreateSelector<T>();

            _inner = new FileEntityStore<T>(dir, keySelector, jsonOptions);
        }

        /// <inheritdoc/>
        public Task SaveAsync(string id, T entity) => _inner.SaveAsync(id, entity);

        /// <inheritdoc/>
        public Task SaveAsync(string[] keys, T entity) => _inner.SaveAsync(keys, entity);

        /// <inheritdoc/>
        public Task<T?> LoadAsync(string id) => _inner.LoadAsync(id);

        /// <inheritdoc/>
        public Task<T?> LoadAsync(params object[] keys) =>
            _inner.LoadAsync(keys.Select(k => k.ToString()!).ToArray());

        /// <inheritdoc/>
        public Task<bool> DeleteAsync(string id) => _inner.DeleteAsync(id);

        /// <inheritdoc/>
        public Task<bool> DeleteAsync(params object[] keys) =>
            _inner.DeleteAsync(keys.Select(k => k.ToString()!).ToArray());

        /// <inheritdoc/>
        public Task<IEnumerable<T>> ListAsync() => _inner.ListAsync();

        /// <inheritdoc/>
        public Task InsertAsync(IEnumerable<T> items) => _inner.InsertAsync(items);
    }
}