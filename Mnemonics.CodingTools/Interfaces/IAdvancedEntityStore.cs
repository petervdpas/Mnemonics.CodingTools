using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mnemonics.CodingTools.Interfaces
{
    /// <summary>
    /// Extends <see cref="IEntityStore{T}"/> to support composite keys and batch insert operations.
    /// </summary>
    /// <typeparam name="T">The type of entity to store.</typeparam>
    public interface IAdvancedEntityStore<T> : IEntityStore<T> where T : class
    {
        /// <summary>
        /// Saves the specified entity using a composite key.
        /// If an entity with the same key values already exists, it will be overwritten.
        /// </summary>
        /// <param name="keys">The composite key values that uniquely identify the entity.</param>
        /// <param name="entity">The entity to store.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SaveAsync(string[] keys, T entity);

        /// <summary>
        /// Loads an entity using one or more key values (supports composite keys).
        /// </summary>
        /// <param name="keys">The key values identifying the entity.</param>
        /// <returns>
        /// A task representing the asynchronous operation. The result contains the entity if found;
        /// otherwise, <c>null</c>.
        /// </returns>
        Task<T?> LoadAsync(params object[] keys);

        /// <summary>
        /// Deletes an entity using one or more key values (supports composite keys).
        /// </summary>
        /// <param name="keys">The key values identifying the entity.</param>
        /// <returns>
        /// A task representing the asynchronous operation. The result is <c>true</c> if the entity was
        /// successfully deleted; otherwise, <c>false</c>.
        /// </returns>
        Task<bool> DeleteAsync(params object[] keys);

        /// <summary>
        /// Inserts a collection of entities into the store.
        /// Existing entities with the same key values will be overwritten or skipped depending on the implementation.
        /// </summary>
        /// <param name="items">The collection of entities to insert.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task InsertAsync(IEnumerable<T> items);
    }
}