namespace TechStore.Application.Abstractions;

/// <summary>
/// Текущий пользователь. Сейчас — из заголовка X-User-Name,
/// при появлении Identity/JWT реализация заменится без правок сервисов.
/// </summary>
public interface ICurrentUser
{
    string Name { get; }
}
