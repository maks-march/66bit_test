using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechStore.Domain.Entities;

namespace TechStore.Infrastructure.Persistence.Configurations;

public sealed class ManufacturerConfiguration : IEntityTypeConfiguration<Manufacturer>
{
    public void Configure(EntityTypeBuilder<Manufacturer> builder)
    {
        builder.ToTable("manufacturers");
        AuditColumns.Configure(builder);

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("citext")
            .HasMaxLength(Manufacturer.NameMaxLength)
            .IsRequired();

        builder.HasIndex(x => x.Name).IsUnique();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(Manufacturer.DescriptionMaxLength);
    }
}
