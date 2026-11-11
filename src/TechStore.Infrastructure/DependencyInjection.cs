using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TechStore.Application.Abstractions;
using TechStore.Domain.Interfaces;
using TechStore.Infrastructure.Files;
using TechStore.Infrastructure.Identity;
using TechStore.Infrastructure.Notifications;
using TechStore.Infrastructure.Persistence;
using TechStore.Infrastructure.Persistence.Repositories;
using TechStore.Infrastructure.Seed;

namespace TechStore.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Не задана строка подключения 'ConnectionStrings:Default'.");

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IManufacturerRepository, ManufacturerRepository>();
        services.AddScoped<IEquipmentTypeRepository, EquipmentTypeRepository>();
        services.AddScoped<ISupplyCountryRepository, SupplyCountryRepository>();
        services.AddScoped<IEquipmentItemRepository, EquipmentItemRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HeaderCurrentUser>();

        // Оба класса опций биндятся к одной секции "FileStorage", каждый читает своё.
        services.Configure<FileStorageOptions>(configuration.GetSection(FileStorageOptions.SectionName));
        services.Configure<FileValidationOptions>(configuration.GetSection(FileValidationOptions.SectionName));
        services.AddSingleton<IFileStorage, LocalFileStorage>();

        services.AddSingleton<IInventoryNotifier, SignalRInventoryNotifier>();

        services.AddScoped<DbSeeder>();

        return services;
    }
}
