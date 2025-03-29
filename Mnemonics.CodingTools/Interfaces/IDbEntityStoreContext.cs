using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Mnemonics.CodingTools.Interfaces
{
    /// <summary>
    /// Defines a contract for resolving <see cref="DbSet{TEntity}"/> instances dynamically and saving changes to the database.
    /// </summary>
    public interface IDbEntityStoreContext
    {
        /// <summary>
        /// Gets the <see cref="DbSet{TEntity}"/> instance for the specified entity type.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <returns>A <see cref="DbSet{TEntity}"/> that can be used to query and save instances of <typeparamref name="T"/>.</returns>
        DbSet<T> Set<T>() where T : class;

        /// <summary>
        /// Saves all changes made in the context to the underlying database.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the save operation.</param>
        /// <returns>
        /// A task that represents the asynchronous save operation. The task result contains the number of state entries written to the database.
        /// </returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}