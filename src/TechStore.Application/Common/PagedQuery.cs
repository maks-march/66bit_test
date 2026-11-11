using FluentValidation;

namespace TechStore.Application.Common;

/// <summary>Базовый запрос со строкой поиска и пагинацией.</summary>
public class PagedQuery
{
    public string? Search { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}

public class PagedQueryValidator<T> : AbstractValidator<T>
    where T : PagedQuery
{
    public const int SearchMaxLength = 200;
    public const int MaxPageSize = 100;

    public PagedQueryValidator()
    {
        RuleFor(x => x.Search)
            .MaximumLength(SearchMaxLength)
            .WithMessage($"Поисковая строка не должна быть длиннее {SearchMaxLength} символов.")
            .When(x => x.Search is not null);

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Номер страницы должен быть не меньше 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, MaxPageSize)
            .WithMessage($"Размер страницы должен быть от 1 до {MaxPageSize}.");
    }
}
