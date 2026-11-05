using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Channels;
using Beep.OilandGas.ApiService.Hubs;
using Beep.OilandGas.Models.Data.DataManagement;
using Beep.OilandGas.Repository;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Services;

internal sealed record CompanyProgressKey(CompanyScope Scope, string UserId, string Resource);
public sealed record CompanyProgressNotification(RepositoryCompanyContext Owner, CompanyChannelKind Kind,
    string Resource, JsonElement Payload)
{
    public CompanyScope Scope => Owner.Scope;
    public string UserId => Owner.UserId;
}

/// <summary>The single host-owned progress store. Scoped services never keep private copies of progress.</summary>
public sealed class CompanyProgressState(TimeProvider? clock = null)
{
    internal object Gate { get; } = new();
    internal TimeProvider Clock { get; } = clock ?? TimeProvider.System;
    internal ConcurrentDictionary<CompanyProgressKey, ProgressUpdate> Operations { get; } = new();
    internal ConcurrentDictionary<CompanyProgressKey, WorkflowProgress> Workflows { get; } = new();
    internal ConcurrentDictionary<CompanyProgressKey, MultiOperationProgress> Groups { get; } = new();
    private ConcurrentDictionary<(CompanyProgressKey Key, CompanyChannelKind Kind), RepositoryCompanyContext> Grants { get; } = new();
    internal Channel<CompanyProgressNotification> Notifications { get; } = Channel.CreateBounded<CompanyProgressNotification>(
        new BoundedChannelOptions(256) { SingleReader = true, FullMode = BoundedChannelFullMode.DropOldest });

    public bool CanRead(RepositoryCompanyContext company, CompanyHubChannel channel)
    {
        if (channel.Scope != company.Scope) return false;
        lock (Gate)
        {
            PruneExpired();
            var key = new CompanyProgressKey(company.Scope, company.UserId, channel.Resource);
            if (!Grants.TryGetValue((key, channel.Kind), out var captured) || !CompanyBackgroundScope.HasCapturedGrants(captured, company)) return false;
            return channel.Kind switch
            {
                CompanyChannelKind.Operation => Operations.ContainsKey(key) || Groups.TryGetValue(key, out var group) &&
                    group.Operations.Keys.All(resource => CanRead(company, channel with { Resource = resource })),
                CompanyChannelKind.Workflow => Workflows.ContainsKey(key),
                _ => false
            };
        }
    }

    public bool CanDeliver(RepositoryCompanyContext company, CompanyHubChannel channel, CompanyProgressNotification notification)
        => notification.Scope == channel.Scope && notification.UserId == company.UserId &&
           notification.Kind == channel.Kind && notification.Resource == channel.Resource &&
           CompanyBackgroundScope.HasCapturedGrants(notification.Owner, company) && CanRead(company, channel);

    internal void Register(RepositoryCompanyContext owner, CompanyChannelKind kind, string resource) =>
        Grants[(new(owner.Scope, owner.UserId, resource), kind)] = owner;

    internal void PruneExpired()
    {
        lock (Gate)
        {
            var now = Clock.GetUtcNow().UtcDateTime;
            foreach (var group in Groups.Values)
                if (!group.IsComplete && group.Operations.Values.All(operation => operation.IsComplete))
                {
                    group.IsComplete = true;
                    group.HasError = group.Operations.Values.Any(operation => operation.HasError);
                    group.Timestamp = group.Operations.Values.Select(operation => operation.Timestamp).DefaultIfEmpty(now).Max();
                }
            foreach (var item in Operations)
                if (item.Value.IsComplete && now - item.Value.Timestamp >= TimeSpan.FromMinutes(5)) Operations.TryRemove(item.Key, out _);
            foreach (var item in Workflows)
                if (item.Value.CompletedAt is DateTime completed && now - completed >= TimeSpan.FromMinutes(10)) Workflows.TryRemove(item.Key, out _);
            foreach (var item in Groups)
                if (item.Value.IsComplete && now - item.Value.Timestamp >= TimeSpan.FromMinutes(5)) Groups.TryRemove(item.Key, out _);
            foreach (var grant in Grants.Keys)
                if (grant.Kind == CompanyChannelKind.Workflow ? !Workflows.ContainsKey(grant.Key) :
                    !Operations.ContainsKey(grant.Key) && !Groups.ContainsKey(grant.Key)) Grants.TryRemove(grant, out _);
        }
    }
}
