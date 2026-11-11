using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechStore.Domain.Entities;

namespace TechStore.Infrastructure.Persistence.Configurations;

public sealed class SupplyCountryConfiguration : IEntityTypeConfiguration<SupplyCountry>
{
    public void Configure(EntityTypeBuilder<SupplyCountry> builder)
    {
        builder.ToTable("supply_countries");
        AuditColumns.Configure(builder);

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("citext")
            .HasMaxLength(SupplyCountry.NameMaxLength)
            .IsRequired();

        builder.HasIndex(x => x.Name).IsUnique();
    }
}
