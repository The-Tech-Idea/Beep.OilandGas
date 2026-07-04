using Microsoft.AspNetCore.SignalR.Client;

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
    Task StartAsync(string userId, string personaCode);

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

public class NotificationService : INotificationService, IAsyncDisposable
{
    private HubConnection? _connection;
    private readonly List<NotificationModel> _recentNotifications = new();
    private int _unreadCount;

    public int UnreadCount => _unreadCount;
    public List<NotificationModel> RecentNotifications => _recentNotifications;
    public event Action<NotificationModel>? OnNotificationReceived;
    public event Action<InboxCounts>? OnTaskCountsUpdated;

    public async Task StartAsync(string userId, string personaCode)
    {
        if (_connection is not null)
            return;

        try
        {
            _connection = new HubConnectionBuilder()
                .WithUrl("/hubs/workflow-notifications")
                .WithAutomaticReconnect()
                .Build();

            _connection.On<NotificationModel>("Notification", notification =>
            {
                notification.CreatedAt = DateTime.UtcNow;
                _recentNotifications.Insert(0, notification);
                if (_recentNotifications.Count > 100)
                    _recentNotifications.RemoveAt(_recentNotifications.Count - 1);
                _unreadCount++;
                OnNotificationReceived?.Invoke(notification);
            });

            _connection.On<InboxCounts>("TaskCountsUpdated", counts =>
            {
                OnTaskCountsUpdated?.Invoke(counts);
            });

            _connection.On<UnifiedTask>("TaskAssigned", task =>
            {
                var notification = new NotificationModel
                {
                    Title = $"New Task: {task.StepName}",
                    Body = $"{task.WorkflowName} — {task.EntityDescription}",
                    Category = task.TaskType,
                    Severity = task.Priority <= 1 ? "CRITICAL" : task.Priority <= 2 ? "WARNING" : "INFO",
                    ActionRoute = task.Route,
                    ActionLabel = $"View {task.TaskType.ToLower()}",
                };
                _recentNotifications.Insert(0, notification);
                _unreadCount++;
                OnNotificationReceived?.Invoke(notification);
            });

            await _connection.StartAsync();
            await _connection.InvokeAsync("SubscribeToUser", userId);
            await _connection.InvokeAsync("SubscribeToPersona", personaCode);
        }
        catch
        {
            // SignalR unavailable — degrade gracefully
            _connection = null;
        }
    }

    public async Task StopAsync()
    {
        if (_connection is not null)
        {
            await _connection.StopAsync();
            await _connection.DisposeAsync();
            _connection = null;
        }
    }

    public void MarkAllRead()
    {
        _unreadCount = 0;
        foreach (var n in _recentNotifications)
            n.IsRead = true;
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
    }
}
