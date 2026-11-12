namespace TechStore.Web.Services;

/// <summary>
/// Снекбары в правом нижнем углу
/// </summary>
public sealed class ToastService
{
    public sealed record Toast(Guid Id, bool IsError, string? Title, string Message);

    private const int MaxToasts = 5;
    private readonly List<Toast> _items = new();

    public IReadOnlyList<Toast> Items => _items;

    public event Action? Changed;

    public void Success(string message) => Add(new Toast(Guid.NewGuid(), false, null, message));

    public void Error(string message, string title = "Ошибка") => Add(new Toast(Guid.NewGuid(), true, title, message));

    public void Dismiss(Guid id)
    {
        if (_items.RemoveAll(t => t.Id == id) > 0)
            Changed?.Invoke();
    }

    private void Add(Toast toast)
    {
        _items.Add(toast);
        while (_items.Count > MaxToasts)
            _items.RemoveAt(0);
        Changed?.Invoke();
        _ = AutoDismissAsync(toast.Id);
    }

    private async Task AutoDismissAsync(Guid id)
    {
        await Task.Delay(TimeSpan.FromSeconds(4));
        Dismiss(id);
    }
}
