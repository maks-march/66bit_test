using Microsoft.AspNetCore.SignalR.Client;
using TechStore.Web.Models;

namespace TechStore.Web.Services;

public enum HubConnectionState
{
    Disconnected,
    Connecting,
    Connected,
    Failed,
}

/// <summary>
/// Клиент SignalR-хаба (/hubs/inventory). Стартует один раз из MainLayout;
/// страницы подписываются на Changed и тихо перезагружают свои списки.
/// Колбэки хаба приходят из фонового потока — подписчики обязаны идти через InvokeAsync.
/// </summary>
public sealed class InventoryHubClient : IAsyncDisposable
{
    private readonly HttpClient _http;
    private HubConnection? _hub;

    public HubConnectionState State { get; private set; } = HubConnectionState.Disconnected;

    public event Action<InventoryChangeNotification>? Changed;
    public event Action? StateChanged;

    public InventoryHubClient(HttpClient http)
    {
        _http = http;
    }

    public async Task StartAsync(CancellationToken ct = default)
    {
        if (_hub is not null)
            return;

        var url = new Uri(_http.BaseAddress!, "hubs/inventory").ToString();
        _hub = new HubConnectionBuilder()
            .WithUrl(url)
            .WithAutomaticReconnect()
            .Build();

        _hub.On<InventoryChangeNotification>("InventoryChanged", notification => Changed?.Invoke(notification));
        _hub.Reconnecting += _ => { SetState(HubConnectionState.Connecting); return Task.CompletedTask; };
        _hub.Reconnected += _ => { SetState(HubConnectionState.Connected); return Task.CompletedTask; };
        _hub.Closed += _ => { SetState(HubConnectionState.Disconnected); return Task.CompletedTask; };

        SetState(HubConnectionState.Connecting);
        try
        {
            await _hub.StartAsync(ct);
            SetState(HubConnectionState.Connected);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            // Хаб недоступен — страницы работают в обычном режиме, бейдж покажет проблему.
            SetState(HubConnectionState.Failed);
        }
    }

    private void SetState(HubConnectionState state)
    {
        State = state;
        StateChanged?.Invoke();
    }

    public async ValueTask DisposeAsync()
    {
        if (_hub is not null)
            await _hub.DisposeAsync();
    }
}
