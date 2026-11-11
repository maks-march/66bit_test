using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TechStore.Application.Common;
using TechStore.Application.EquipmentItems;
using TechStore.Application.EquipmentTypes;
using TechStore.Application.Manufacturers;
using TechStore.Application.SupplyCountries;

namespace TechStore.Application;

/// <summary>
/// Регистрация слоя Application. Репозитории регистрирует Infrastructure.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IManufacturerService, ManufacturerService>();
        services.AddScoped<IEquipmentTypeService, EquipmentTypeService>();
        services.AddScoped<ISupplyCountryService, SupplyCountryService>();
        services.AddScoped<IEquipmentItemService, EquipmentItemService>();

        services.AddScoped<IValidator<PagedQuery>, PagedQueryValidator<PagedQuery>>();
        services.AddScoped<IValidator<ManufacturerCreateDto>, ManufacturerCreateValidator>();
        services.AddScoped<IValidator<ManufacturerUpdateDto>, ManufacturerUpdateValidator>();
        services.AddScoped<IValidator<EquipmentTypeCreateDto>, EquipmentTypeCreateValidator>();
        services.AddScoped<IValidator<EquipmentTypeUpdateDto>, EquipmentTypeUpdateValidator>();
        services.AddScoped<IValidator<EquipmentItemCreateDto>, EquipmentItemCreateValidator>();
        services.AddScoped<IValidator<EquipmentItemUpdateDto>, EquipmentItemUpdateValidator>();
        services.AddScoped<IValidator<EquipmentItemQuery>, EquipmentItemQueryValidator>();

        return services;
    }
}
