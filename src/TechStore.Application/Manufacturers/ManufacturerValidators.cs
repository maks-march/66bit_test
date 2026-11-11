using FluentValidation;
using TechStore.Domain.Entities;

namespace TechStore.Application.Manufacturers;

/// <summary>Чистые правила формы (без доступа к данным — дубли проверяет сервис).</summary>
public abstract class ManufacturerWriteValidator<T> : AbstractValidator<T>
    where T : IManufacturerWrite
{
    protected ManufacturerWriteValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Наименование обязательно для заполнения.")
            .MaximumLength(Manufacturer.NameMaxLength)
            .WithMessage($"Наименование не должно быть длиннее {Manufacturer.NameMaxLength} символов.");

        RuleFor(x => x.Description)
            .MaximumLength(Manufacturer.DescriptionMaxLength)
            .WithMessage($"Описание не должно быть длиннее {Manufacturer.DescriptionMaxLength} символов.")
            .When(x => x.Description is not null);
    }
}

public sealed class ManufacturerCreateValidator : ManufacturerWriteValidator<ManufacturerCreateDto>
{
}

public sealed class ManufacturerUpdateValidator : ManufacturerWriteValidator<ManufacturerUpdateDto>
{
}
