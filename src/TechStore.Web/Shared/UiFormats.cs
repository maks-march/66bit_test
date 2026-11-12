namespace TechStore.Web.Shared;

/// <summary>Отображение дат, цен и русских plural-форм. Сервер отдаёт UTC — показываем локальное.</summary>
public static class UiFormats
{
    public static string Date(DateTime value) => value.ToLocalTime().ToString("dd.MM.yyyy HH:mm");

    public static string Price(decimal value) => value.ToString("N2") + " ₽";

    public static string Plural(int count, string one, string few, string many)
    {
        var n = Math.Abs(count) % 100;
        if (n is >= 11 and <= 14)
            return many;
        return (n % 10) switch
        {
            1 => one,
            >= 2 and <= 4 => few,
            _ => many,
        };
    }
}
