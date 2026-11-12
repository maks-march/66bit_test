using System.ComponentModel.DataAnnotations;

namespace TechStore.Web.Models;

/// <summary>
/// Модель формы позиции. Имена свойств совпадают с серверным DTO —
/// по ним маппятся полевые ошибки валидации с API.
/// Клиентские правила — только быстрые проверки; сервер — источник истины.
/// </summary>
public sealed class ItemFormModel
{
    [Required(ErrorMessage = "Выберите тип техники.")]
    public long? EquipmentTypeId { get; set; }

    [Required(ErrorMessage = "Выберите производителя.")]
    public long? ManufacturerId { get; set; }

    [Required(ErrorMessage = "Модель обязательна для заполнения.")]
    public string? Model { get; set; }

    [Required(ErrorMessage = "Страна поставки обязательна для заполнения.")]
    public string? SupplyCountryName { get; set; }

    [Required(ErrorMessage = "Укажите цену.")]
    // Диапазон (>0, ≤ max) проверяет ItemDialog прямым сравнением:
    // [Range] для decimal парсит границы строкой в текущей культуре и падает в ru-RU.
    public decimal? Price { get; set; }

    public ItemWriteRequest ToRequest()
        => new(EquipmentTypeId!.Value, ManufacturerId!.Value, Model, SupplyCountryName, Price!.Value);

    public static ItemFormModel From(EquipmentItemModel item)
        => new()
        {
            EquipmentTypeId = item.EquipmentType.Id,
            ManufacturerId = item.Manufacturer.Id,
            Model = item.Model,
            SupplyCountryName = item.SupplyCountry.Name,
            Price = item.Price,
        };
}

/// <summary>Модель формы справочника (производитель / тип техники).</summary>
public sealed class DictionaryFormModel
{
    [Required(ErrorMessage = "Наименование обязательно для заполнения.")]
    public string? Name { get; set; }

    public string? Description { get; set; }

    public DictionaryWriteRequest ToRequest() => new(Name, Description);
}
