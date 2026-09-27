using System.Collections.Concurrent;
using System.Security.Claims;
using Beep.OilandGas.ApiService.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Hubs;

/// <summary>
/// Extends the existing in-process SignalR transport with fresh authorization at delivery.
/// Subscription membership is a routing hint, never a cached grant of access.
/// </summary>
public sealed class WorkflowNotificationLifetimeManager(
    ILogger<DefaultHubLifetimeManager<WorkflowNotificationHub>> transportLogger,
    ILogger<WorkflowNotificationLifetimeManager> logger,
    IServiceScopeFactory scopeFactory) : DefaultHubLifetimeManager<WorkflowNotificationHub>(transportLogger)
{
    private sealed class ConnectionState(HubConnectionContext connection)
    {
        public HubConnectionContext Connection { get; } = connection;
        public ConcurrentDictionary<string, byte> Groups { get; } = new(StringComparer.Ordinal);
    }

    private readonly ConcurrentDictionary<string, ConnectionState> _connections = new(StringComparer.Ordinal);

    public override async Task OnConnectedAsync(HubConnectionContext connection)
    {
        _connections[connection.ConnectionId] = new(connection);
        try { await base.OnConnectedAsync(connection); }
        catch { _connections.TryRemove(connection.ConnectionId, out _); throw; }
    }

    public override async Task OnDisconnectedAsync(HubConnectionContext connection)
    {
        _connections.TryRemove(connection.ConnectionId, out _);
        await base.OnDisconnectedAsync(connection);
    }

    public override async Task AddToGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default)
    {
        await base.AddToGroupAsync(connectionId, groupName, cancellationToken);
        if (_connections.TryGetValue(connectionId, out var state)) state.Groups[groupName] = 0;
    }

    public override async Task RemoveFromGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default)
    {
        if (_connections.TryGetValue(connectionId, out var state)) state.Groups.TryRemove(groupName, out _);
        await base.RemoveFromGroupAsync(connectionId, groupName, cancellationToken);
    }

    public override Task SendGroupAsync(string groupName, string methodName, object?[] args, CancellationToken cancellationToken = default)
        => SendAuthorizedGroupsAsync(new[] { groupName }, methodName, args, Array.Empty<string>(), cancellationToken);

    public override Task SendGroupsAsync(IReadOnlyList<string> groupNames, string methodName, object?[] args,
        CancellationToken cancellationToken = default)
        => SendAuthorizedGroupsAsync(groupNames, methodName, args, Array.Empty<string>(), cancellationToken);

    public override Task SendGroupExceptAsync(string groupName, string methodName, object?[] args,
        IReadOnlyList<string> excludedConnectionIds, CancellationToken cancellationToken = default)
        => SendAuthorizedGroupsAsync(new[] { groupName }, methodName, args, excludedConnectionIds, cancellationToken);

    // Unscoped broadcasting cannot establish a role/resource decision for its payload.
    public override Task SendAllAsync(string methodName, object?[] args, CancellationToken cancellationToken = default)
        => throw new InvalidOperationException("Workflow notifications require an authorized user, persona/field or process channel.");

    public override Task SendAllExceptAsync(string methodName, object?[] args, IReadOnlyList<string> excludedConnectionIds,
        CancellationToken cancellationToken = default)
        => throw new InvalidOperationException("Workflow notifications require an authorized user, persona/field or process channel.");

    private async Task SendAuthorizedGroupsAsync(IReadOnlyList<string> groupNames, string methodName,
        object?[] args, IReadOnlyList<string> excludedConnectionIds, CancellationToken cancellationToken)
    {
        var excluded = excludedConnectionIds.ToHashSet(StringComparer.Ordinal);
        foreach (var (connectionId, state) in _connections)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (excluded.Contains(connectionId) || state.Connection.ConnectionAborted.IsCancellationRequested) continue;
            foreach (var group in groupNames.Distinct(StringComparer.Ordinal))
            {
                if (!state.Groups.ContainsKey(group)) continue;
                var allowed = false;
                try { allowed = await CanDeliverAsync(state.Connection.User, group); }
                catch (Exception exception)
                {
                    // Failure resolving current app access must never reuse an earlier grant.
                    logger.LogWarning(exception, "Workflow notification access lookup failed for connection {ConnectionId}", connectionId);
                }
                if (!allowed)
                {
                    await RemoveFromGroupAsync(connectionId, group, cancellationToken);
                    continue;
                }
                if (state.Groups.ContainsKey(group) && !state.Connection.ConnectionAborted.IsCancellationRequested)
                {
                    var deliveryArgs = args;
                    if (group.StartsWith("persona:", StringComparison.Ordinal))
                    {
                        if (args.Length != 1 || args[0] is not RoleNotification notification) continue;
                        try
                        {
                            await using var scope = scopeFactory.CreateAsyncScope();
                            var subject = state.Connection.User.FindActingUserId();
                            if (subject is null || !await scope.ServiceProvider.GetRequiredService<WorkflowNotificationAuthorization>()
                                .HasRoleAsync(subject, notification.RequiredRole)) continue;
                        }
                        catch (Exception exception)
                        {
                            logger.LogWarning(exception, "Workflow notification role lookup failed");
                            continue;
                        }
                        deliveryArgs = new object?[] { notification.Payload };
                    }
                    await base.SendConnectionAsync(connectionId, methodName, deliveryArgs, cancellationToken);
                    break; // Multiple matching channels must not duplicate a delivery.
                }
            }
        }
    }

    private async Task<bool> CanDeliverAsync(ClaimsPrincipal? user, string group)
    {
        var subject = user.FindActingUserId();
        if (user?.Identity?.IsAuthenticated != true || string.IsNullOrWhiteSpace(subject)) return false;
        if (group.StartsWith("user:", StringComparison.Ordinal)) return group[5..] == subject;

        await using var scope = scopeFactory.CreateAsyncScope();
        var authorization = scope.ServiceProvider.GetRequiredService<WorkflowNotificationAuthorization>();
        var segments = group.Split(':');
        if (segments.Length == 4 && segments[0] == "persona" && segments[2] == "field")
            return await authorization.CanAccessPersonaAsync(subject, Uri.UnescapeDataString(segments[1]), Uri.UnescapeDataString(segments[3]));
        if (segments.Length == 2 && segments[0] == "process")
            return await authorization.CanAccessProcessAsync(subject, Uri.UnescapeDataString(segments[1]));
        return false;
    }
}

public static class WorkflowNotificationServiceCollectionExtensions
{
    public static IServiceCollection AddWorkflowNotificationAuthorization(this IServiceCollection services)
    {
        services.AddScoped<WorkflowNotificationAuthorization>();
        Microsoft.Extensions.DependencyInjection.Extensions.ServiceCollectionDescriptorExtensions.TryAddScoped<INotificationPersonaReader, RepositoryNotificationPersonaReader>(services);
        services.AddSingleton<HubLifetimeManager<WorkflowNotificationHub>, WorkflowNotificationLifetimeManager>();
        return services;
    }
}
