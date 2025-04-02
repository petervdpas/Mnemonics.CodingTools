using System;
using System.Collections.Generic;
using System.Linq;
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
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc/>
        public async Task SaveAsync(string id, T entity)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var dbSet = _context.Set<T>();
            var existing = await dbSet.FindAsync(id);
            if (existing is not null)
                dbSet.Remove(existing);

            dbSet.Add(entity);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task SaveAsync(string[] keys, T entity)
        {
            if (keys is null || keys.Length == 0) throw new ArgumentException("Keys cannot be null or empty.", nameof(keys));
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var dbSet = _context.Set<T>();
            var existing = await dbSet.FindAsync(keys.Cast<object>().ToArray());
            if (existing is not null)
                dbSet.Remove(existing);

            dbSet.Add(entity);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<T?> LoadAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            return await _context.Set<T>().FindAsync(id);
        }

        /// <inheritdoc/>
        public async Task<T?> LoadAsync(params object[] keys)
        {
            if (keys is null || keys.Length == 0)
                throw new ArgumentException("Keys cannot be null or empty.", nameof(keys));

            return await _context.Set<T>().FindAsync(keys);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));

            var dbSet = _context.Set<T>();
            var entity = await dbSet.FindAsync(id);
            if (entity is null) return false;

            dbSet.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(params object[] keys)
        {
            if (keys is null || keys.Length == 0)
                throw new ArgumentException("Keys cannot be null or empty.", nameof(keys));

            var dbSet = _context.Set<T>();
            var entity = await dbSet.FindAsync(keys);
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

        /// <inheritdoc/>
        public async Task InsertAsync(IEnumerable<T> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            var dbSet = _context.Set<T>();
            await dbSet.AddRangeAsync(items);
            await _context.SaveChangesAsync();
        }
    }
}