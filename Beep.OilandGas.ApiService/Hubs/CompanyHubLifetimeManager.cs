using System.Collections.Concurrent;
using System.Security.Claims;
using Beep.OilandGas.ApiService.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Http.Connections.Features;

namespace Beep.OilandGas.ApiService.Hubs;

/// <summary>
/// Extends the existing in-process SignalR transport with fresh authorization at delivery.
/// Subscription membership is a routing hint, never a cached grant of access.
/// </summary>
public abstract class CompanyHubLifetimeManager<THub>(
    ILogger<DefaultHubLifetimeManager<THub>> transportLogger,
    IServiceScopeFactory scopeFactory) : DefaultHubLifetimeManager<THub>(transportLogger) where THub : Hub
{
    private sealed class ConnectionState(HubConnectionContext connection, RepositoryCompanyContext company)
    {
        public HubConnectionContext Connection { get; } = connection;
        public RepositoryCompanyContext Company { get; } = company;
        public ConcurrentDictionary<string, byte> Groups { get; } = new(StringComparer.Ordinal);
    }

    private readonly ConcurrentDictionary<string, ConnectionState> _connections = new(StringComparer.Ordinal);

    public override async Task OnConnectedAsync(HubConnectionContext connection)
    {
        var company = connection.Features.Get<IHttpContextFeature>()?.HttpContext?.Features.Get<RepositoryCompanyContext>();
        if (company is null) { connection.Abort(); throw new HubException("A validated company handshake is required."); }
        _connections[connection.ConnectionId] = new(connection, company);
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
        var channel = CompanyHubGroups.Parse(groupName);
        if (!_connections.TryGetValue(connectionId, out var state) || channel is null || channel.Scope != state.Company.Scope ||
            !Supports(channel) ||
            (channel.Kind == CompanyChannelKind.User && channel.Resource != state.Company.UserId))
            throw new HubException("The notification channel does not belong to this account and company.");
        if (!await CanDeliverAsync(state, groupName, null, null, cancellationToken))
            throw new HubException("The channel is unavailable to this account and company.");
        await base.AddToGroupAsync(connectionId, groupName, cancellationToken);
        state.Groups[groupName] = 0;
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
        => throw new InvalidOperationException("Messages require a supported company channel.");

    public override Task SendAllExceptAsync(string methodName, object?[] args, IReadOnlyList<string> excludedConnectionIds,
        CancellationToken cancellationToken = default)
        => throw new InvalidOperationException("Messages require a supported company channel.");

    public override Task SendConnectionAsync(string connectionId, string methodName, object?[] args, CancellationToken cancellationToken = default)
        => throw new InvalidOperationException("Use an authorized company notification channel.");
    public override Task SendConnectionsAsync(IReadOnlyList<string> connectionIds, string methodName, object?[] args, CancellationToken cancellationToken = default)
        => throw new InvalidOperationException("Use authorized company notification channels.");
    public override Task SendUserAsync(string userId, string methodName, object?[] args, CancellationToken cancellationToken = default)
        => throw new InvalidOperationException("Use the company-bound user notification channel.");
    public override Task SendUsersAsync(IReadOnlyList<string> userIds, string methodName, object?[] args, CancellationToken cancellationToken = default)
        => throw new InvalidOperationException("Use company-bound user notification channels.");

    private async Task SendAuthorizedGroupsAsync(IReadOnlyList<string> groupNames, string methodName,
        object?[] args, IReadOnlyList<string> excludedConnectionIds, CancellationToken cancellationToken)
    {
        var channels = groupNames.Distinct(StringComparer.Ordinal).ToDictionary(name => name,
            name => CompanyHubGroups.Parse(name), StringComparer.Ordinal);
        if (channels.Values.Any(channel => channel is null || !Supports(channel)))
            throw new InvalidOperationException("Publishers must specify a supported company channel.");
        // Validate every envelope before delivering to any connection.
        foreach (var channel in channels.Values) _ = DeliveryArguments(channel!, args);
        var excluded = excludedConnectionIds.ToHashSet(StringComparer.Ordinal);
        foreach (var (connectionId, state) in _connections)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (excluded.Contains(connectionId) || state.Connection.ConnectionAborted.IsCancellationRequested) continue;
            foreach (var group in channels.Keys)
            {
                if (!state.Groups.ContainsKey(group)) continue;
                var allowed = false;
                var channel = CompanyHubGroups.Parse(group);
                if (channel is null) continue;
                try { allowed = await CanDeliverAsync(state, group, RequiredRole(channel, args), args, cancellationToken); }
                // Whatever stops the access lookup, this connection is not sent the notification and leaves the group:
                // failure resolving current app access must never reuse an earlier grant. The other connections are
                // still served.
                catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
                {
                    ReportFailure(exception, $"checking current company access ({group}) for connection {connectionId}",
                        "the notification is not delivered to this connection, which leaves the group; it can subscribe again");
                }
                if (!allowed)
                {
                    await RemoveFromGroupAsync(connectionId, group, cancellationToken);
                    continue;
                }
                if (state.Groups.ContainsKey(group) && !state.Connection.ConnectionAborted.IsCancellationRequested)
                {
                    var deliveryArgs = DeliveryArguments(channel, args);
                    await base.SendConnectionAsync(connectionId, methodName, deliveryArgs, cancellationToken);
                    break; // Multiple matching channels must not duplicate a delivery.
                }
            }
        }
    }

    protected abstract bool Supports(CompanyHubChannel channel);
    protected abstract string? RequiredRole(CompanyHubChannel channel, object?[] args);
    protected abstract object?[] DeliveryArguments(CompanyHubChannel channel, object?[] args);
    protected abstract Task<bool> ResourceAccessAsync(IServiceProvider services, CompanyHubChannel channel,
        RepositoryCompanyContext company, object?[]? args);
    protected abstract void ReportFailure(Exception exception, string operation, string outcome);

    private async Task<bool> CanDeliverAsync(ConnectionState state, string group, string? requiredRole,
        object?[]? args, CancellationToken token)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<CompanyHubScope>().CanDeliverAsync(state.Connection.User,
            state.Company, group, requiredRole,
            (channel, company) => ResourceAccessAsync(scope.ServiceProvider, channel, company, args), token);
    }
}
