using Microsoft.AspNetCore.SignalR;
using TechStore.Application.Abstractions;

namespace TechStore.Infrastructure.Notifications;

public sealed class SignalRInventoryNotifier : IInventoryNotifier
{
    public const string Method = "InventoryChanged";

    private readonly IHubContext<InventoryHub> _hub;

    public SignalRInventoryNotifier(IHubContext<InventoryHub> hub)
    {
        _hub = hub;
    }

    public Task NotifyAsync(InventoryChange change, CancellationToken ct = default)
        // Шлём только факт изменения: клиент перезапрашивает текущую страницу сам.
        // Так нет рассинхрона пагинации и не надо версионировать payload сущностей.
        => _hub.Clients.All.SendAsync(Method, new
        {
            kind = change.Kind.ToString(),
            action = change.Action.ToString(),
            entityId = change.EntityId,
            at = DateTime.UtcNow,
        }, ct);
}
