using FluentValidation;
using TechStore.Domain.Entities;

namespace TechStore.Application.EquipmentTypes;

/// <summary>Чистые правила формы (без доступа к данным — дубли проверяет сервис).</summary>
public abstract class EquipmentTypeWriteValidator<T> : AbstractValidator<T>
    where T : IEquipmentTypeWrite
{
    protected EquipmentTypeWriteValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Наименование обязательно для заполнения.")
            .MaximumLength(EquipmentType.NameMaxLength)
            .WithMessage($"Наименование не должно быть длиннее {EquipmentType.NameMaxLength} символов.");

        RuleFor(x => x.Description)
            .MaximumLength(EquipmentType.DescriptionMaxLength)
            .WithMessage($"Описание не должно быть длиннее {EquipmentType.DescriptionMaxLength} символов.")
            .When(x => x.Description is not null);
    }
}

public sealed class EquipmentTypeCreateValidator : EquipmentTypeWriteValidator<EquipmentTypeCreateDto>
{
}

public sealed class EquipmentTypeUpdateValidator : EquipmentTypeWriteValidator<EquipmentTypeUpdateDto>
{
}
