using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.UserManagement.Contracts.Services;

namespace Beep.OilandGas.ApiService.Hubs;

/// <summary>
/// SignalR hub for real-time workflow notifications.
/// Connections join their authenticated user channel automatically; persona and
/// process channels require app-owned role and resource access checks.
/// Server pushes task updates, SLA alerts, and approval notifications.
/// Part of Phase 5 experience & integration.
/// </summary>
[Authorize]
public class WorkflowNotificationHub : Hub
{
    private readonly WorkflowNotificationAuthorization _authorization;

    public WorkflowNotificationHub(WorkflowNotificationAuthorization authorization)
    {
        _authorization = authorization;
    }

    private string Subject() => Context.User?.Identity?.IsAuthenticated == true
        ? Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new HubException("An authenticated subject is required.")
        : throw new HubException("An authenticated subject is required.");

    public async Task SubscribeToPersona(string personaCode, string fieldId)
    {
        var userId = Subject();
        // Drop the previous subscription even if the new context is denied.
        if (Context.Items.Remove("persona-group", out var previous) && previous is string oldGroup)
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, oldGroup, Context.ConnectionAborted);
        if (string.IsNullOrWhiteSpace(personaCode) || string.IsNullOrWhiteSpace(fieldId))
            throw new HubException("Persona and field are required.");
        if (!await _authorization.CanAccessPersonaAsync(userId, personaCode, fieldId))
            throw new HubException("Persona subscription denied.");

        var group = WorkflowNotificationHubExtensions.PersonaGroup(personaCode, fieldId);
        await Groups.AddToGroupAsync(Context.ConnectionId, group, Context.ConnectionAborted);
        Context.Items["persona-group"] = group;
    }

    public async Task SubscribeToProcess(string processInstanceId)
    {
        var userId = Subject();
        if (Context.Items.Remove("process-group", out var previous) && previous is string oldGroup)
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, oldGroup, Context.ConnectionAborted);
        if (string.IsNullOrWhiteSpace(processInstanceId)) throw new HubException("Process is required.");
        if (!await _authorization.CanAccessProcessAsync(userId, processInstanceId))
            throw new HubException("Process subscription denied.");
        await Groups.AddToGroupAsync(Context.ConnectionId,
            WorkflowNotificationHubExtensions.ProcessGroup(processInstanceId), Context.ConnectionAborted);
        Context.Items["process-group"] = WorkflowNotificationHubExtensions.ProcessGroup(processInstanceId);
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Context.User?.Identity?.IsAuthenticated != true || string.IsNullOrWhiteSpace(userId))
        {
            Context.Abort();
            throw new HubException("An authenticated subject is required.");
        }

        // A reconnect gets a new connection ID and repeats this identity check.
        // User subscriptions are never selected by a client-supplied identifier.
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}", Context.ConnectionAborted);
        await base.OnConnectedAsync();
    }
}

/// <summary>
/// Static helper for pushing notifications from services.
/// Inject IHubContext<WorkflowNotificationHub> in any service.
/// </summary>
public static class WorkflowNotificationHubExtensions
{
    internal static string PersonaGroup(string personaCode, string fieldId) =>
        $"persona:{Uri.EscapeDataString(personaCode)}:field:{Uri.EscapeDataString(fieldId)}";

    internal static string ProcessGroup(string processId) => $"process:{Uri.EscapeDataString(processId)}";

    public static Task NotifyPersonaAsync(this IHubContext<WorkflowNotificationHub> hubContext,
        string personaCode, string fieldId, string requiredRole, string method, object payload)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(personaCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldId);
        ArgumentException.ThrowIfNullOrWhiteSpace(requiredRole);
        return hubContext.Clients.Group(PersonaGroup(personaCode, fieldId)).SendAsync(method, new RoleNotification(requiredRole, payload));
    }

    public static Task NotifyProcessAsync(this IHubContext<WorkflowNotificationHub> hubContext,
        string processId, string method, object payload)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(processId);
        return hubContext.Clients.Group(ProcessGroup(processId)).SendAsync(method, payload);
    }

    public static async Task NotifyUserAsync(
        this IHubContext<WorkflowNotificationHub> hubContext,
        string userId,
        string method,
        object payload)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        // Publishers must resolve the recipient from app-owned RBAC/resource access
        // before sending business data. IdentityServer roles are not authority.
        await hubContext.Clients.Group($"user:{userId}").SendAsync(method, payload);
    }
}

internal sealed record RoleNotification(string RequiredRole, object Payload);
