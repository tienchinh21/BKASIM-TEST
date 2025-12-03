using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MiniAppGIBA.Entities.HomePins
{
    /// <summary>
    /// Entity Framework configuration for HomePin entity
    /// </summary>
    public class HomePinConfiguration : IEntityTypeConfiguration<HomePin>
    {
        public void Configure(EntityTypeBuilder<HomePin> builder)
        {
            builder.ToTable("HomePins");

            // Primary key
            builder.HasKey(x => x.Id);

            // Properties
            builder.Property(x => x.EntityType)
                .IsRequired()
                .HasConversion<byte>();

            builder.Property(x => x.EntityId)
                .IsRequired()
                .HasMaxLength(32);

            builder.Property(x => x.DisplayOrder)
                .IsRequired();

            builder.Property(x => x.PinnedBy)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(x => x.PinnedAt)
                .IsRequired();

            builder.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(x => x.Notes)
                .HasMaxLength(500);

            // Indexes
            builder.HasIndex(x => new { x.EntityType, x.EntityId })
                .IsUnique()
                .HasDatabaseName("UQ_HomePins_Entity");

            builder.HasIndex(x => x.DisplayOrder)
                .HasDatabaseName("IX_HomePins_DisplayOrder");

            builder.HasIndex(x => x.IsActive)
                .HasDatabaseName("IX_HomePins_IsActive");

            builder.HasIndex(x => x.PinnedBy)
                .HasDatabaseName("IX_HomePins_PinnedBy");

            builder.HasIndex(x => x.EntityType)
                .HasDatabaseName("IX_HomePins_EntityType");
        }
    }
}
