using Microsoft.EntityFrameworkCore;
using TechStore.Infrastructure.Persistence;
using TechStore.Infrastructure.Seed;

namespace TechStore.Api.Infrastructure;

public static class AppExtensions
{
    /// <summary>Миграции при старте и демо-данные за флагом Seed:Enabled (по умолчанию true).</summary>
    public static async Task ApplyMigrationsAndSeedAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        var db = services.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        if (!app.Configuration.GetValue("Seed:Enabled", true))
            return;

        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("Seed");
        logger.LogInformation("Seeding demo data...");
        await services.GetRequiredService<DbSeeder>().SeedAsync();
        logger.LogInformation("Seeding done.");
    }
}
