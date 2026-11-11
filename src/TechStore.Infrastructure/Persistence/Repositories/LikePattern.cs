namespace TechStore.Infrastructure.Persistence.Repositories;

/// <summary>Экранирование спецсимволов LIKE (% _ \) для пользовательских строк поиска.</summary>
internal static class LikePattern
{
    public static string Contains(string value)
        => $"%{value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_")}%";
}
