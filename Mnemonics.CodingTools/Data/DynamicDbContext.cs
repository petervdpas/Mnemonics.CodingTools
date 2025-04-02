using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Mnemonics.CodingTools.Configuration;
using Mnemonics.CodingTools.Interfaces;
using Mnemonics.CodingTools.Models;

namespace Mnemonics.CodingTools.Data
{
    /// <summary>
    /// A custom <see cref="DbContext"/> that dynamically registers entity types at runtime
    /// based on types provided by an <see cref="IDynamicTypeRegistry"/>.
    /// </summary>
    public class DynamicDbContext : DbContext, IDbEntityStoreContext
    {
        private readonly CodingToolsOptions _options;
        private readonly IDynamicTypeRegistry _typeRegistry;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicDbContext"/> class.
        /// </summary>
        /// <param name="options">The options used to configure the context.</param>
        /// <param name="typeRegistry">The registry containing dynamic entity types.</param>
        /// <param name="optionsAccessor">
        /// Provides access to the current <see cref="CodingToolsOptions"/> used for configuration,
        /// including fallback key detection and registration behavior for dynamic entity types.
        /// </param>
        public DynamicDbContext(
            DbContextOptions<DynamicDbContext> options,
            IDynamicTypeRegistry typeRegistry,
            IOptions<CodingToolsOptions> optionsAccessor) : base(options)
        {
            _typeRegistry = typeRegistry ?? throw new ArgumentNullException(nameof(typeRegistry));
            _options = optionsAccessor?.Value ?? throw new ArgumentNullException(nameof(optionsAccessor));
        }

        /// <inheritdoc />
        public new DbSet<T> Set<T>() where T : class => base.Set<T>();

        /// <inheritdoc />
        public new Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => base.SaveChangesAsync(cancellationToken);

        /// <summary>
        /// Configures the entity types in the model by registering all types from the <see cref="IDynamicTypeRegistry"/>.
        /// </summary>
        /// <param name="modelBuilder">The builder used to construct the model for the context.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var type in _typeRegistry.GetTypes())
            {
                var entity = modelBuilder.Entity(type);

                var keyProps = type.GetProperties()
                    .Where(p => p.GetCustomAttributes(typeof(FieldWithAttributes), true)
                                 .Cast<FieldWithAttributes>()
                                 .Any(attr => attr.IsKeyField))
                    .ToList();

                if (!keyProps.Any())
                {
                    var fallbackKey = _options.GlobalFallbackKeyNames
                        .Select(type.GetProperty)
                        .FirstOrDefault(p => p != null);

                    if (fallbackKey != null)
                    {
                        entity.HasKey(fallbackKey.Name);
                    }
                }
            }

            base.OnModelCreating(modelBuilder);
        }
    }
}
