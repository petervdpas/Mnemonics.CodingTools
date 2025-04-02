using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Mnemonics.CodingTools.Interfaces
{
    /// <summary>
    /// Defines a contract for resolving <see cref="DbSet{TEntity}"/> instances dynamically 
    /// and saving changes to the database using Entity Framework Core.
    /// </summary>
    public interface IDbEntityStoreContext
    {
        /// <summary>
        /// Gets the <see cref="DbSet{TEntity}"/> instance for the specified entity type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <returns>
        /// A strongly-typed <see cref="DbSet{TEntity}"/> for querying and persisting instances of <typeparamref name="T"/>.
        /// </returns>
        DbSet<T> Set<T>() where T : class;

        /// <summary>
        /// Gets the <see cref="DbSet{TEntity}"/> instance for the specified runtime type.
        /// </summary>
        /// <param name="type">The CLR <see cref="Type"/> of the entity.</param>
        /// <returns>
        /// A dynamically typed <see cref="DbSet{TEntity}"/> instance boxed as <see cref="object"/> to support runtime entity types.
        /// </returns>
        object Set(Type type);

        /// <summary>
        /// Saves all changes made in this context to the database asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the save operation.</param>
        /// <returns>
        /// A task that represents the asynchronous save operation. 
        /// The task result contains the number of state entries written to the database.
        /// </returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}