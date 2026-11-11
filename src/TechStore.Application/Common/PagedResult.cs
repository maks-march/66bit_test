namespace TechStore.Application.Common;

/// <summary>Страница серверной пагинации.</summary>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize)
{
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);

    public bool HasNextPage => Page * PageSize < TotalCount;
}
