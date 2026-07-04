using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Beep.OilandGas.ApiService.Hubs;

/// <summary>
/// SignalR hub for real-time workflow notifications.
/// Clients subscribe to user-specific and persona-specific channels.
/// Server pushes task updates, SLA alerts, and approval notifications.
/// Part of Phase 5 experience & integration.
/// </summary>
[Authorize]
public class WorkflowNotificationHub : Hub
{
    /// <summary>Subscribe to notifications for a specific user.</summary>
    public async Task SubscribeToUser(string userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}");
    }

    /// <summary>Subscribe to notifications for a persona (all users with that persona).</summary>
    public async Task SubscribeToPersona(string personaCode)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"persona:{personaCode}");
    }

    /// <summary>Subscribe to notifications for a process instance.</summary>
    public async Task SubscribeToProcess(string processInstanceId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"process:{processInstanceId}");
    }

    /// <summary>Unsubscribe from all groups on disconnect.</summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Groups are automatically removed on disconnect — no cleanup needed
        await base.OnDisconnectedAsync(exception);
    }
}

/// <summary>
/// Static helper for pushing notifications from services.
/// Inject IHubContext<WorkflowNotificationHub> in any service.
/// </summary>
public static class WorkflowNotificationHubExtensions
{
    public static async Task NotifyUserAsync(
        this IHubContext<WorkflowNotificationHub> hubContext,
        string userId,
        string method,
        object payload)
    {
        await hubContext.Clients.Group($"user:{userId}").SendAsync(method, payload);
    }

    public static async Task NotifyPersonaAsync(
        this IHubContext<WorkflowNotificationHub> hubContext,
        string personaCode,
        string method,
        object payload)
    {
        await hubContext.Clients.Group($"persona:{personaCode}").SendAsync(method, payload);
    }

    public static async Task NotifyProcessAsync(
        this IHubContext<WorkflowNotificationHub> hubContext,
        string processInstanceId,
        string method,
        object payload)
    {
        await hubContext.Clients.Group($"process:{processInstanceId}").SendAsync(method, payload);
    }
}
