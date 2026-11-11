using Microsoft.EntityFrameworkCore;
using TechStore.Domain.Entities;

namespace TechStore.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public DbSet<Manufacturer> Manufacturers => Set<Manufacturer>();
    public DbSet<EquipmentType> EquipmentTypes => Set<EquipmentType>();
    public DbSet<SupplyCountry> SupplyCountries => Set<SupplyCountry>();
    public DbSet<EquipmentItem> EquipmentItems => Set<EquipmentItem>();

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // citext — case-insensitive текст для уникальных имён (сравнение через ==).
        modelBuilder.HasPostgresExtension("citext");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
