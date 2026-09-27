using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Components.Authorization;
using Beep.Foundation.IdentityServer.Shared.Identity;
using Duende.AccessTokenManagement.OpenIdConnect;

namespace Beep.OilandGas.Web.Services;

/// <summary>
/// Browser-side notification service that connects to the WorkflowNotificationHub via SignalR.
/// Provides real-time task updates, SLA alerts, and approval notifications.
/// Part of Phase 5 experience & integration.
/// </summary>
public interface INotificationService
{
    /// <summary>Current unread notification count.</summary>
    int UnreadCount { get; }

    /// <summary>Recent notifications (in-memory).</summary>
    List<NotificationModel> RecentNotifications { get; }

    /// <summary>Fired when a new notification arrives.</summary>
    event Action<NotificationModel>? OnNotificationReceived;

    /// <summary>Fired when task counts update.</summary>
    event Action<InboxCounts>? OnTaskCountsUpdated;

    /// <summary>Start the SignalR connection.</summary>
    Task StartAsync();
    Task SubscribeToPersonaAsync(string personaCode, string fieldId);

    bool IsConnected { get; }
    string? LastError { get; }
    event Action? OnStateChanged;

    /// <summary>Stop the SignalR connection.</summary>
    Task StopAsync();

    /// <summary>Mark all notifications as read.</summary>
    void MarkAllRead();
}

public class NotificationModel
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Severity { get; set; } = "INFO";         // INFO, WARNING, CRITICAL
    public string Category { get; set; } = "APPROVAL";     // APPROVAL, ESCALATION, REMINDER, SYSTEM
    public string? ActionRoute { get; set; }
    public string? ActionLabel { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public sealed class NotificationService : INotificationService, IAsyncDisposable
{
    private readonly AuthenticationStateProvider _authentication;
    private readonly IUserTokenManager _tokens;
    private readonly ILogger<NotificationService> _logger;
    private readonly Uri _hubUri;
    private readonly SemaphoreSlim _lifecycle = new(1, 1);
    private readonly object _stateLock = new();
    private readonly List<NotificationModel> _recentNotifications = new();
    private HubConnection? _connection;
    private string? _userId;
    private bool _disposed;
    private sealed record PersonaSubscription(string Persona, string Field);
    private volatile PersonaSubscription? _persona;

    public NotificationService(AuthenticationStateProvider authentication, IUserTokenManager tokens,
        OilGasApiAddress api, ILogger<NotificationService> logger)
    {
        _authentication = authentication;
        _tokens = tokens;
        _logger = logger;
        _hubUri = api.For("hubs/workflow-notifications");
        _authentication.AuthenticationStateChanged += AuthenticationChanged;
    }

    public int UnreadCount { get { lock (_stateLock) return _recentNotifications.Count(n => !n.IsRead); } }
    public List<NotificationModel> RecentNotifications { get { lock (_stateLock) return _recentNotifications.ToList(); } }
    public bool IsConnected => _connection?.State == HubConnectionState.Connected;
    public string? LastError { get; private set; }
    public event Action<NotificationModel>? OnNotificationReceived;
    public event Action<InboxCounts>? OnTaskCountsUpdated;
    public event Action? OnStateChanged;

    public async Task StartAsync()
    {
        await _lifecycle.WaitAsync();
        try
        {
            if (_disposed) return;
            var state = await _authentication.GetAuthenticationStateAsync();
            // The OilGas account the API resolved for this person (party_id), which the hub groups by; without one the API
            // would refuse the connection. It used the identity server's sub.
            var userId = state.User.Identity?.IsAuthenticated == true ? PartyIdClaims.Find(state.User) : null;
            if (string.IsNullOrWhiteSpace(userId))
            {
                await StopCoreAsync();
                return;
            }
            if (_userId == userId && _connection?.State is HubConnectionState.Connected or HubConnectionState.Reconnecting)
                return;

            await StopCoreAsync();
            _userId = userId;

            var connection = new HubConnectionBuilder()
                .WithUrl(_hubUri, options => options.AccessTokenProvider = async () =>
                {
                    var current = await _authentication.GetAuthenticationStateAsync();
                    if (current.User.Identity?.IsAuthenticated != true || PartyIdClaims.Find(current.User) != userId)
                        throw new InvalidOperationException("The notification user has changed.");
                    // The person's own token from the identity server's client library, refreshed when it is due.
                    var token = await _tokens.GetAccessTokenAsync(current.User);
                    return token.WasSuccessful(out var user)
                        ? user.AccessToken.ToString()
                        : throw new InvalidOperationException("The current user's access token is unavailable.");
                })
                .WithAutomaticReconnect()
                .Build();
            _connection = connection;
            connection.On<NotificationModel>("Notification", notification => Receive(connection, notification));
            connection.On<UnifiedTask>("TaskAssigned", task => Receive(connection, new NotificationModel
            {
                Title = $"New Task: {task.StepName}",
                Body = $"{task.WorkflowName} — {task.EntityDescription}",
                Category = task.TaskType,
                Severity = task.Priority <= 1 ? "CRITICAL" : task.Priority <= 2 ? "WARNING" : "INFO",
                ActionRoute = task.Route,
                ActionLabel = $"View {task.TaskType.ToLowerInvariant()}"
            }));
            connection.On<InboxCounts>("TaskCountsUpdated", counts =>
            {
                if (ReferenceEquals(_connection, connection)) OnTaskCountsUpdated?.Invoke(counts);
            });
            connection.Reconnecting += error =>
            {
                if (ReferenceEquals(_connection, connection))
                {
                    LastError = "Notifications disconnected. Reconnecting…";
                    OnStateChanged?.Invoke();
                }
                return Task.CompletedTask;
            };
            connection.Reconnected += async id =>
            {
                if (ReferenceEquals(_connection, connection))
                {
                    LastError = null;
                    if (_persona is { } persona)
                    {
                        try { await connection.InvokeAsync("SubscribeToPersona", persona.Persona, persona.Field); }
                        catch (Exception exception)
                        {
                            _persona = null;
                            LastError = "Persona notifications are unavailable. Select an authorized persona and field.";
                            _logger.LogWarning(exception, "Unable to restore persona notification subscription");
                        }
                    }
                    OnStateChanged?.Invoke();
                }
                // The server joins the authenticated user again for the new connection.
            };
            connection.Closed += error =>
            {
                if (ReferenceEquals(_connection, connection))
                {
                    LastError = "Live notifications are unavailable. Retry to reconnect.";
                    OnStateChanged?.Invoke();
                }
                return Task.CompletedTask;
            };
            await connection.StartAsync();
            LastError = null;
        }
        catch (Exception exception)
        {
            await StopCoreAsync();
            LastError = "Live notifications are unavailable. Retry to reconnect.";
            _logger.LogWarning(exception, "Unable to connect workflow notifications");
        }
        finally
        {
            _lifecycle.Release();
            OnStateChanged?.Invoke();
        }
    }

    public async Task SubscribeToPersonaAsync(string personaCode, string fieldId)
    {
        await _lifecycle.WaitAsync();
        try
        {
            _persona = null;
            if (_disposed) return;
            if (_connection?.State == HubConnectionState.Reconnecting)
            {
                _persona = new(personaCode, fieldId);
                return;
            }
            if (_connection?.State != HubConnectionState.Connected) return;
            await _connection.InvokeAsync("SubscribeToPersona", personaCode, fieldId);
            _persona = new(personaCode, fieldId);
            LastError = null;
        }
        catch (Exception exception)
        {
            LastError = "Persona notifications are unavailable. Select an authorized persona and field.";
            _logger.LogWarning(exception, "Persona notification subscription denied or unavailable");
        }
        finally { _lifecycle.Release(); }
        OnStateChanged?.Invoke();
    }

    private void Receive(HubConnection connection, NotificationModel notification)
    {
        lock (_stateLock)
        {
            if (!ReferenceEquals(_connection, connection)) return;
            // Replayed notifications must not increase unread counts twice.
            if (_recentNotifications.Any(n => n.Id == notification.Id)) return;
            _recentNotifications.Insert(0, notification);
            if (_recentNotifications.Count > 100) _recentNotifications.RemoveAt(100);
        }
        OnNotificationReceived?.Invoke(notification);
        OnStateChanged?.Invoke();
    }

    public async Task StopAsync()
    {
        await _lifecycle.WaitAsync();
        try { await StopCoreAsync(); }
        finally { _lifecycle.Release(); }
        OnStateChanged?.Invoke();
    }

    private async Task StopCoreAsync()
    {
        HubConnection? connection;
        lock (_stateLock)
        {
            connection = _connection;
            _connection = null;
            _userId = null;
            _persona = null;
            _recentNotifications.Clear();
        }
        LastError = null;
        if (connection is not null) await connection.DisposeAsync();
    }

    public void MarkAllRead()
    {
        lock (_stateLock)
            foreach (var notification in _recentNotifications) notification.IsRead = true;
        OnStateChanged?.Invoke();
    }

    private void AuthenticationChanged(Task<AuthenticationState> state) => _ = RestartForAuthenticationAsync(state);

    private async Task RestartForAuthenticationAsync(Task<AuthenticationState> state)
    {
        try
        {
            await state;
            await StopAsync();
            await StartAsync();
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Unable to refresh notification authentication");
            await StopAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        _authentication.AuthenticationStateChanged -= AuthenticationChanged;
        await _lifecycle.WaitAsync();
        try
        {
            _disposed = true;
            await StopCoreAsync();
        }
        finally { _lifecycle.Release(); }
    }
}
