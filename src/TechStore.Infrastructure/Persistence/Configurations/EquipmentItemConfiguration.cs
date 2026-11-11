using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechStore.Domain.Entities;

namespace TechStore.Infrastructure.Persistence.Configurations;

public sealed class EquipmentItemConfiguration : IEntityTypeConfiguration<EquipmentItem>
{
    public void Configure(EntityTypeBuilder<EquipmentItem> builder)
    {
        builder.ToTable("equipment_items");
        AuditColumns.Configure(builder);

        builder.Property(x => x.EquipmentTypeId).HasColumnName("equipment_type_id");
        builder.Property(x => x.ManufacturerId).HasColumnName("manufacturer_id");
        builder.Property(x => x.SupplyCountryId).HasColumnName("supply_country_id");

        builder.Property(x => x.Model)
            .HasColumnName("model")
            .HasMaxLength(EquipmentItem.ModelMaxLength)
            .IsRequired();

        builder.Property(x => x.Price)
            .HasColumnName("price")
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(x => x.PhotoPath)
            .HasColumnName("photo_path")
            .HasMaxLength(EquipmentItem.PhotoPathMaxLength);

        // Restrict: удаление справочника при связях запрещено на уровне БД,
        // сервис проверяет заранее и возвращает 409 с понятным текстом.
        builder.HasOne(x => x.EquipmentType)
            .WithMany(x => x.EquipmentItems)
            .HasForeignKey(x => x.EquipmentTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Manufacturer)
            .WithMany(x => x.EquipmentItems)
            .HasForeignKey(x => x.ManufacturerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SupplyCountry)
            .WithMany(x => x.EquipmentItems)
            .HasForeignKey(x => x.SupplyCountryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.ManufacturerId, x.EquipmentTypeId });
        builder.HasIndex(x => x.Price);
    }
}
