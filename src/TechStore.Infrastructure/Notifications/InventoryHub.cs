using Microsoft.AspNetCore.SignalR;

namespace TechStore.Infrastructure.Notifications;

/// <summary>
/// Хаб realtime-уведомлений.
/// Методов от клиента нет: подписка = подключение.
/// </summary>
public sealed class InventoryHub : Hub
{
}
