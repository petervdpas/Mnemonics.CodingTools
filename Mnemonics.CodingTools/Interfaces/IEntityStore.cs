using System.Collections.Generic;
using System.Threading.Tasks;


namespace Mnemonics.CodingTools.Interfaces
{
    /// <summary>
    /// Defines a generic contract for entity storage operations, including support for composite keys and batch inserts.
    /// </summary>
    /// <typeparam name="T">The type of entity to store.</typeparam>
    public interface IEntityStore<T> where T : class
    {
        /// <summary>
        /// Saves the specified entity with the associated identifier.
        /// If an entity with the same ID already exists, it will be overwritten.
        /// </summary>
        /// <param name="id">The unique identifier for the entity.</param>
        /// <param name="entity">The entity to store.</param>
        Task SaveAsync(string id, T entity);

        /// <summary>
        /// Saves the specified entity using a composite key.
        /// If an entity with the same key values already exists, it will be overwritten.
        /// </summary>
        /// <param name="keys">The composite key values that uniquely identify the entity.</param>
        /// <param name="entity">The entity to store.</param>
        Task SaveAsync(string[] keys, T entity);

        /// <summary>
        /// Inserts a collection of entities into the store.
        /// Existing entities with the same key values may be overwritten or skipped depending on the implementation.
        /// </summary>
        /// <param name="items">The collection of entities to insert.</param>
        Task InsertAsync(IEnumerable<T> items);

        /// <summary>
        /// Loads an entity by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the entity to retrieve.</param>
        /// <returns>The entity if found; otherwise, <c>null</c>.</returns>
        Task<T?> LoadAsync(string id);

        /// <summary>
        /// Loads an entity using one or more key values (supports composite keys).
        /// </summary>
        /// <param name="keys">The key values identifying the entity.</param>
        /// <returns>The entity if found; otherwise, <c>null</c>.</returns>
        Task<T?> LoadAsync(params object[] keys);

        /// <summary>
        /// Deletes the entity with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the entity to delete.</param>
        /// <returns><c>true</c> if the entity was deleted; otherwise, <c>false</c>.</returns>
        Task<bool> DeleteAsync(string id);

        /// <summary>
        /// Deletes an entity using one or more key values (supports composite keys).
        /// </summary>
        /// <param name="keys">The key values identifying the entity.</param>
        /// <returns><c>true</c> if the entity was deleted; otherwise, <c>false</c>.</returns>
        Task<bool> DeleteAsync(params object[] keys);

        /// <summary>
        /// Lists all stored entities.
        /// </summary>
        /// <returns>An enumerable collection of all stored entities.</returns>
        Task<IEnumerable<T>> ListAsync();
    }
}