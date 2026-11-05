using Beep.OilandGas.ApiService.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>Publishes snapshots from the canonical store through the company delivery gate.</summary>
public sealed class ProgressTrackingPublisher(CompanyProgressState state, IHubContext<ProgressHub> hub,
    Action<Exception, string, string> reportFailure) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
        Task.WhenAll(BroadcastAsync(stoppingToken), CleanupAsync(stoppingToken));

    private async Task BroadcastAsync(CancellationToken token)
    {
        try
        {
            await foreach (var item in state.Notifications.Reader.ReadAllAsync(token))
            {
                token.ThrowIfCancellationRequested();
                var group = CompanyHubGroups.Name(new(item.Scope, item.Kind, item.Resource));
                var method = item.Kind == CompanyChannelKind.Workflow ? "WorkflowProgress" :
                    item.Payload.TryGetProperty("operations", out _) ? "MultiOperationProgress" : "ProgressUpdate";
                try { await hub.Clients.Group(group).SendAsync(method, item, token); }
                catch (Exception exception) when (!token.IsCancellationRequested)
                {
                    reportFailure(exception, $"broadcasting {method} to progress group {group}",
                        "the live update is not delivered; progress remains readable by its owner");
                }
            }
        }
        finally { while (state.Notifications.Reader.TryRead(out _)) { } }
    }

    private async Task CleanupAsync(CancellationToken token)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
        while (await timer.WaitForNextTickAsync(token)) state.PruneExpired();
    }

    public override Task StopAsync(CancellationToken token)
    {
        state.Notifications.Writer.TryComplete();
        return base.StopAsync(token);
    }
}
