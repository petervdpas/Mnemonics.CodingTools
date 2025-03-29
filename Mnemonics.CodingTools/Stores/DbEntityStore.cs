using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Mnemonics.CodingTools.Interfaces;

namespace Mnemonics.CodingTools.Storage
{
    /// <summary>
    /// Provides a database-backed implementation of <see cref="IEntityStore{T}"/> using Entity Framework Core.
    /// </summary>
    /// <typeparam name="T">The entity type to store.</typeparam>
    public class DbEntityStore<T> : IEntityStore<T> where T : class
    {
        private readonly IDbEntityStoreContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbEntityStore{T}"/> class.
        /// </summary>
        /// <param name="context">The database context used for storage operations.</param>
        public DbEntityStore(IDbEntityStoreContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public async Task SaveAsync(string id, T entity)
        {
            var dbSet = _context.Set<T>();
            var existing = await dbSet.FindAsync(id);
            if (existing is not null)
                dbSet.Remove(existing);

            dbSet.Add(entity);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<T?> LoadAsync(string id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(string id)
        {
            var dbSet = _context.Set<T>();
            var entity = await dbSet.FindAsync(id);
            if (entity is null) return false;

            dbSet.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<T>> ListAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }
    }
}