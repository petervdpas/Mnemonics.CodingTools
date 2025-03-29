using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mnemonics.CodingTools.Interfaces
{
    /// <summary>
    /// Defines a generic contract for entity storage operations.
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
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SaveAsync(string id, T entity);

        /// <summary>
        /// Loads an entity by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the entity to retrieve.</param>
        /// <returns>
        /// A task representing the asynchronous operation. The result contains the entity if found;
        /// otherwise, <c>null</c>.
        /// </returns>
        Task<T?> LoadAsync(string id);

        /// <summary>
        /// Deletes the entity with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the entity to delete.</param>
        /// <returns>
        /// A task representing the asynchronous operation. The result is <c>true</c> if the entity was
        /// successfully deleted; otherwise, <c>false</c>.
        /// </returns>
        Task<bool> DeleteAsync(string id);

        /// <summary>
        /// Lists all stored entities.
        /// </summary>
        /// <returns>
        /// A task representing the asynchronous operation. The result is an enumerable collection of all stored entities.
        /// </returns>
        Task<IEnumerable<T>> ListAsync();
    }
}