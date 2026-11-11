using TechStore.Domain.Common;
using TechStore.Domain.Exceptions;

namespace TechStore.Domain.Entities;

/// <summary>Позиция на складе.</summary>
public sealed class EquipmentItem : AuditableEntity
{
    public const int ModelMaxLength = 200;
    public const int PhotoPathMaxLength = 500;

    public long EquipmentTypeId { get; private set; }
    public EquipmentType EquipmentType { get; private set; } = null!;

    public long ManufacturerId { get; private set; }
    public Manufacturer Manufacturer { get; private set; } = null!;

    public long SupplyCountryId { get; private set; }
    public SupplyCountry SupplyCountry { get; private set; } = null!;

    public string Model { get; private set; } = null!;
    public decimal Price { get; private set; }

    /// <summary>Относительный путь к файлу (например "2026/11/&lt;guid&gt;.webp"). Сам файл — в IFileStorage.</summary>
    public string? PhotoPath { get; private set; }

    /// <summary>Токен конкурентности (в PostgreSQL маппится на xmin).</summary>
    public byte[] RowVersion { get; private set; } = [];

    private EquipmentItem()
    {
    }

    public static EquipmentItem Create(
        long equipmentTypeId,
        long manufacturerId,
        long supplyCountryId,
        string? model,
        decimal price,
        string? photoPath,
        string? createdBy = null)
    {
        var item = new EquipmentItem();
        item.SetReferences(equipmentTypeId, manufacturerId, supplyCountryId);
        item.Model = Guard.Required(model, "Модель", ModelMaxLength);
        item.Price = NormalizePrice(price);
        item.PhotoPath = NormalizePhotoPath(photoPath);
        item.MarkCreated(createdBy);
        return item;
    }

    public void Update(
        long equipmentTypeId,
        long manufacturerId,
        long supplyCountryId,
        string? model,
        decimal price,
        string? photoPath,
        string? updatedBy)
    {
        SetReferences(equipmentTypeId, manufacturerId, supplyCountryId);
        Model = Guard.Required(model, "Модель", ModelMaxLength);
        Price = NormalizePrice(price);
        PhotoPath = NormalizePhotoPath(photoPath);
        Touch(updatedBy);
    }

    public void SetPhoto(string? photoPath, string? updatedBy)
    {
        PhotoPath = NormalizePhotoPath(photoPath);
        Touch(updatedBy);
    }

    private void SetReferences(long equipmentTypeId, long manufacturerId, long supplyCountryId)
    {
        EquipmentTypeId = Guard.PositiveId(equipmentTypeId, "Тип техники");
        ManufacturerId = Guard.PositiveId(manufacturerId, "Производитель");
        SupplyCountryId = Guard.PositiveId(supplyCountryId, "Страна поставки");
    }

    private static decimal NormalizePrice(decimal price)
    {
        if (price <= 0)
            throw new DomainException(ErrorCodes.InvalidPrice, "Цена должна быть больше 0.");

        return decimal.Round(price, 2);
    }

    private static string? NormalizePhotoPath(string? photoPath)
    {
        if (photoPath is null)
            return null;

        // Путь приходит только из IFileStorage (доверенный источник), но длину всё равно контролируем.
        var trimmed = photoPath.Trim();
        if (trimmed.Length == 0)
            return null;

        if (trimmed.Length > PhotoPathMaxLength)
            throw new DomainException(ErrorCodes.PhotoInvalid, "Слишком длинный путь к фотографии.");

        return trimmed;
    }
}
