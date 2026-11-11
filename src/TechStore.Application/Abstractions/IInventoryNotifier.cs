namespace TechStore.Application.Abstractions;

/// <summary>Страны отдельным событием не уведомляются — они всегда едут в событиях позиций.</summary>
public enum InventoryEntityKind
{
    EquipmentItem,
    Manufacturer,
    EquipmentType,
}

public enum ChangeAction
{
    Created,
    Updated,
    Deleted,
}

public sealed record InventoryChange(InventoryEntityKind Kind, ChangeAction Action, long? EntityId);

/// <summary>
/// Уведомления об изменениях (реализация — SignalR в Infrastructure).
/// Вызывается только после успешного коммита.
/// </summary>
public interface IInventoryNotifier
{
    Task NotifyAsync(InventoryChange change, CancellationToken cancellationToken = default);
}
