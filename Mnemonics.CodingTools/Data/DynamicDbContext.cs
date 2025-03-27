using Microsoft.EntityFrameworkCore;
using Mnemonics.CodingTools.Interfaces;

namespace Mnemonics.CodingTools.Data
{
    /// <summary>
    /// A custom <see cref="DbContext"/> that dynamically registers entity types at runtime
    /// based on types provided by an <see cref="IDynamicTypeRegistry"/>.
    /// </summary>
    public class DynamicDbContext : DbContext
    {
        private readonly IDynamicTypeRegistry _typeRegistry;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicDbContext"/> class.
        /// </summary>
        /// <param name="options">The options used to configure the context.</param>
        /// <param name="typeRegistry">The registry containing dynamic entity types.</param>
        public DynamicDbContext(DbContextOptions<DynamicDbContext> options, IDynamicTypeRegistry typeRegistry)
            : base(options)
        {
            _typeRegistry = typeRegistry;
        }

        /// <summary>
        /// Configures the entity types in the model by registering all types from the <see cref="IDynamicTypeRegistry"/>.
        /// </summary>
        /// <param name="modelBuilder">The builder used to construct the model for the context.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var type in _typeRegistry.GetTypes())
            {
                modelBuilder.Entity(type);
            }

            base.OnModelCreating(modelBuilder);
        }
    }
}