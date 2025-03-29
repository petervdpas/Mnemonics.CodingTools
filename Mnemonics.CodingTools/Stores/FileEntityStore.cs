using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Mnemonics.CodingTools.Interfaces;

namespace Mnemonics.CodingTools.Stores
{
    /// <summary>
    /// Provides a file-based implementation of <see cref="IAdvancedEntityStore{T}"/> using JSON serialization.
    /// Each entity is stored in an individual file, identified by a single or composite key.
    /// Thread-safe using a SemaphoreSlim to prevent concurrent file access.
    /// </summary>
    /// <typeparam name="T">The entity type to store. Must be serializable to JSON.</typeparam>
    public class FileEntityStore<T> : IAdvancedEntityStore<T> where T : class
    {
        private readonly string _directoryPath;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly SemaphoreSlim _lock = new(1, 1);
        private readonly Func<T, string[]> _keySelector;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileEntityStore{T}"/> class.
        /// </summary>
        /// <param name="directoryPath">The directory where entity files will be stored.</param>
        /// <param name="keySelector">Function to extract the composite key from an entity.</param>
        /// <param name="jsonOptions">Optional JSON serializer options.</param>
        public FileEntityStore(string directoryPath, Func<T, string[]> keySelector, JsonSerializerOptions? jsonOptions = null)
        {
            _directoryPath = directoryPath ?? throw new ArgumentNullException(nameof(directoryPath));
            _keySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
            _jsonOptions = jsonOptions ?? new JsonSerializerOptions { WriteIndented = true };
            Directory.CreateDirectory(_directoryPath);
        }

        /// <inheritdoc />
        public async Task SaveAsync(string id, T entity)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            string filePath = GetFilePath(id);
            string json = JsonSerializer.Serialize(entity, _jsonOptions);

            await _lock.WaitAsync();
            try
            {
                await File.WriteAllTextAsync(filePath, json);
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <inheritdoc />
        public async Task SaveAsync(string[] keys, T entity)
        {
            if (keys is null || keys.Length == 0)
                throw new ArgumentException("Composite keys cannot be null or empty.", nameof(keys));

            string compositeId = GetCompositeId(keys);
            await SaveAsync(compositeId, entity);
        }

        /// <inheritdoc />
        public async Task<T?> LoadAsync(string id)
        {
            string filePath = GetFilePath(id);
            if (!File.Exists(filePath)) return null;

            await _lock.WaitAsync();
            try
            {
                string json = await File.ReadAllTextAsync(filePath);
                return JsonSerializer.Deserialize<T>(json, _jsonOptions);
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <inheritdoc />
        public Task<T?> LoadAsync(params object[] keys)
        {
            if (keys is null || keys.Length == 0)
                throw new ArgumentException("Keys cannot be null or empty.", nameof(keys));

            string compositeId = GetCompositeId(keys);
            return LoadAsync(compositeId);
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(string id)
        {
            string filePath = GetFilePath(id);

            await _lock.WaitAsync();
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }
                return false;
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <inheritdoc />
        public Task<bool> DeleteAsync(params object[] keys)
        {
            if (keys is null || keys.Length == 0)
                throw new ArgumentException("Keys cannot be null or empty.", nameof(keys));

            string compositeId = GetCompositeId(keys);
            return DeleteAsync(compositeId);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<T>> ListAsync()
        {
            var results = new List<T>();

            await _lock.WaitAsync();
            try
            {
                foreach (var file in Directory.GetFiles(_directoryPath, "*.json"))
                {
                    string json = await File.ReadAllTextAsync(file);
                    var entity = JsonSerializer.Deserialize<T>(json, _jsonOptions);
                    if (entity != null)
                        results.Add(entity);
                }
            }
            finally
            {
                _lock.Release();
            }

            return results;
        }

        /// <inheritdoc />
        public async Task InsertAsync(IEnumerable<T> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            foreach (var item in items)
            {
                var keys = _keySelector(item);
                await SaveAsync(keys, item);
            }
        }

        private string GetFilePath(string id) => Path.Combine(_directoryPath, $"{id}.json");

        private static string GetCompositeId(IEnumerable<object> keys) =>
            string.Join("__", keys.Select(k => k?.ToString() ?? throw new ArgumentException("Key cannot be null.")));
    }
}