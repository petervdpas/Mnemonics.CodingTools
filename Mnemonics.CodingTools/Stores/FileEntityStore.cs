using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Mnemonics.CodingTools.Interfaces;

namespace Mnemonics.CodingTools.Stores
{
    /// <summary>
    /// Provides a file-based implementation of <see cref="IEntityStore{T}"/> using JSON serialization.
    /// Each entity is stored in an individual file, identified by its ID.
    /// Thread-safe using a SemaphoreSlim to prevent concurrent file access.
    /// </summary>
    /// <typeparam name="T">The entity type to store. Must be serializable to JSON.</typeparam>
    public class FileEntityStore<T> : IEntityStore<T> where T : class
    {
        private readonly string _directoryPath;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly SemaphoreSlim _lock = new(1, 1);

        /// <summary>
        /// Initializes a new instance of the <see cref="FileEntityStore{T}"/> class.
        /// </summary>
        /// <param name="directoryPath">The directory where entity files will be stored.</param>
        /// <param name="jsonOptions">Optional JSON serializer options.</param>
        public FileEntityStore(string directoryPath, JsonSerializerOptions? jsonOptions = null)
        {
            _directoryPath = directoryPath ?? throw new ArgumentNullException(nameof(directoryPath));
            _jsonOptions = jsonOptions ?? new JsonSerializerOptions { WriteIndented = true };

            Directory.CreateDirectory(_directoryPath);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        private string GetFilePath(string id) => Path.Combine(_directoryPath, $"{id}.json");
    }
}