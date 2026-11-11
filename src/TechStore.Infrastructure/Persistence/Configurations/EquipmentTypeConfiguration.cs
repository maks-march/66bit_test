using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechStore.Domain.Entities;

namespace TechStore.Infrastructure.Persistence.Configurations;

public sealed class EquipmentTypeConfiguration : IEntityTypeConfiguration<EquipmentType>
{
    public void Configure(EntityTypeBuilder<EquipmentType> builder)
    {
        builder.ToTable("equipment_types");
        AuditColumns.Configure(builder);

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("citext")
            .HasMaxLength(EquipmentType.NameMaxLength)
            .IsRequired();

        builder.HasIndex(x => x.Name).IsUnique();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(EquipmentType.DescriptionMaxLength);
    }
}
