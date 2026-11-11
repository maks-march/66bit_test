using FluentValidation;
using TechStore.Application.Common;
using TechStore.Domain.Entities;

namespace TechStore.Application.EquipmentItems;

/// <summary>Правила формы.</summary>
public abstract class EquipmentItemWriteValidator<T> : AbstractValidator<T>
    where T : IEquipmentItemWrite
{
    protected EquipmentItemWriteValidator()
    {
        RuleFor(x => x.EquipmentTypeId)
            .GreaterThan(0)
            .WithMessage("Выберите тип техники.");

        RuleFor(x => x.ManufacturerId)
            .GreaterThan(0)
            .WithMessage("Выберите производителя.");

        RuleFor(x => x.Model)
            .NotEmpty()
            .WithMessage("Модель обязательна для заполнения.")
            .MaximumLength(EquipmentItem.ModelMaxLength)
            .WithMessage($"Модель не должна быть длиннее {EquipmentItem.ModelMaxLength} символов.");

        RuleFor(x => x.SupplyCountryName)
            .NotEmpty()
            .WithMessage("Страна поставки обязательна для заполнения.")
            .MaximumLength(SupplyCountry.NameMaxLength)
            .WithMessage($"Страна поставки не должна быть длиннее {SupplyCountry.NameMaxLength} символов.");

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("Цена должна быть больше 0.")
            .LessThanOrEqualTo(EquipmentItem.MaxPrice)
            .WithMessage("Цена не должна превышать 999 999 999,99.");
    }
}

public sealed class EquipmentItemCreateValidator : EquipmentItemWriteValidator<EquipmentItemCreateDto>
{
}

public sealed class EquipmentItemUpdateValidator : EquipmentItemWriteValidator<EquipmentItemUpdateDto>
{
}

public sealed class EquipmentItemQueryValidator : PagedQueryValidator<EquipmentItemQuery>
{
    public EquipmentItemQueryValidator()
    {
        RuleFor(x => x.EquipmentTypeId)
            .GreaterThan(0)
            .WithMessage("Некорректный тип техники.")
            .When(x => x.EquipmentTypeId.HasValue);

        RuleFor(x => x.ManufacturerId)
            .GreaterThan(0)
            .WithMessage("Некорректный производитель.")
            .When(x => x.ManufacturerId.HasValue);

        RuleFor(x => x.PriceFrom)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Цена «от» не может быть отрицательной.")
            .When(x => x.PriceFrom.HasValue);

        RuleFor(x => x.PriceTo)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Цена «до» не может быть отрицательной.")
            .When(x => x.PriceTo.HasValue);

        RuleFor(x => x.PriceTo)
            .GreaterThanOrEqualTo(x => x.PriceFrom!.Value)
            .WithMessage("Цена «до» должна быть не меньше цены «от».")
            .When(x => x.PriceFrom.HasValue && x.PriceTo.HasValue);

        RuleFor(x => x.SortBy)
            .NotEmpty()
            .WithMessage("Укажите поле сортировки.")
            .Must(EquipmentItemSortFields.IsKnown)
            .WithMessage($"Сортировка: {string.Join(", ", EquipmentItemSortFields.All)}.");
    }
}
