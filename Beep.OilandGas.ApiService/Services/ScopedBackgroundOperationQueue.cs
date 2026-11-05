using System.Threading.Channels;
using Beep.OilandGas.Models.Core.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Http;

namespace Beep.OilandGas.ApiService.Services;

public sealed class ScopedBackgroundOperationQueue : BackgroundService, IBackgroundOperationQueue
{
    private sealed record WorkKey(string Resource, string TenantId, string CompanyId, string UserId);
    private sealed record Work(WorkKey Key, RepositoryCompanyContext Company,
        Func<IServiceProvider, CancellationToken, Task> Execute);
    private readonly Channel<Work> _queue = Channel.CreateBounded<Work>(new BoundedChannelOptions(64)
    { SingleReader = true, FullMode = BoundedChannelFullMode.Wait });
    private readonly Dictionary<WorkKey, BackgroundOperationStatus> _statuses = new();
    private readonly Queue<(WorkKey Key, BackgroundOperationStatus Status)> _completed = new();
    private readonly object _gate = new();
    private readonly IServiceScopeFactory _scopes;
    private readonly Func<Exception, string, string, string> _reportFailure;
    private readonly IHttpContextAccessor _http;
    private bool _stopping;

    public ScopedBackgroundOperationQueue(IServiceScopeFactory scopes, Func<Exception, string, string, string> reportFailure, IHttpContextAccessor http)
    { _scopes = scopes; _reportFailure = reportFailure ?? throw new ArgumentNullException(nameof(reportFailure)); _http = http ?? throw new ArgumentNullException(nameof(http)); }

    public bool TryEnqueue<TService, TState>(string key, TState state,
        Func<TService, TState, CancellationToken, Task> execute) where TService : notnull
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(execute);
        lock (_gate)
        {
            var company = _http.HttpContext?.Features.Get<RepositoryCompanyContext>();
            if (company is null) return false;
            var storageKey = Key(key, company);
            if (_stopping || _statuses.TryGetValue(storageKey, out var previous) &&
                previous.State is BackgroundOperationState.Queued or BackgroundOperationState.Running)
                return false;
            var work = new Work(storageKey, company,
                (services, token) => execute(services.GetRequiredService<TService>(), state, token));
            if (!_queue.Writer.TryWrite(work)) return false;
            _statuses[storageKey] = new(key, BackgroundOperationState.Queued);
            return true;
        }
    }

    public BackgroundOperationStatus? GetStatus(string key)
    {
        lock (_gate)
        {
            var company = _http.HttpContext?.Features.Get<RepositoryCompanyContext>();
            return company is null ? null : _statuses.GetValueOrDefault(Key(key, company));
        }
    }

    private void SetStatus(WorkKey key, BackgroundOperationState state, string? error = null)
    {
        lock (_gate)
        {
            _statuses[key] = new(key.Resource, state, error);
            if (state is BackgroundOperationState.Running) return;
            _completed.Enqueue((key, _statuses[key]));
            while (_completed.Count > 256)
            {
                var old = _completed.Dequeue();
                if (_statuses.TryGetValue(old.Key, out var status) && ReferenceEquals(status, old.Status))
                {
                    _statuses.Remove(old.Key);
                }
            }
        }
    }

    private static WorkKey Key(string resource, RepositoryCompanyContext company) =>
        new(resource, company.Scope.TenantId, company.Scope.BusinessAssociateId, company.UserId);

    // The host's own cancellation ends the loop as cancelled, which is what BackgroundService expects of a stop it started;
    // the operation it interrupted is marked cancelled on the way out (OILGAS-CATCH-01: the loop swallowed it, and any
    // operation's own cancellation — a timeout inside it — was recorded as "cancelled" rather than as the failure it is).
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (var work in _queue.Reader.ReadAllAsync(stoppingToken))
            {
                if (stoppingToken.IsCancellationRequested)
                { SetStatus(work.Key, BackgroundOperationState.Cancelled); break; }
                SetStatus(work.Key, BackgroundOperationState.Running);
                try
                {
                    await using (var scope = _scopes.CreateAsyncScope())
                    {
                        await CompanyBackgroundScope.RestoreAsync(work.Company,
                            scope.ServiceProvider.GetRequiredService<RepositoryCompanyContextResolver>(),
                            scope.ServiceProvider.GetRequiredService<RepositoryTenantContext>(), stoppingToken);
                        await work.Execute(scope.ServiceProvider, stoppingToken);
                    }
                    SetStatus(work.Key, BackgroundOperationState.Succeeded);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    SetStatus(work.Key, BackgroundOperationState.Cancelled);
                    throw;
                }
                // Whatever ends one operation, the queue goes on with the next; the operation's status carries the
                // reference its failure is filed under.
                catch (Exception ex)
                {
                    var reference = _reportFailure(ex, $"running background operation {work.Key}",
                        "the operation is marked failed with this reference; the queue goes on with the next");
                    SetStatus(work.Key, BackgroundOperationState.Failed, $"Background execution failed (reference {reference}).");
                }
            }
        }
        finally
        {
            while (_queue.Reader.TryRead(out var work)) SetStatus(work.Key, BackgroundOperationState.Cancelled);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        lock (_gate) { _stopping = true; _queue.Writer.TryComplete(); }
        // BeepDM's synchronous DDL has no cancellation parameter. Never dispose its scope mid-command.
        await base.StopAsync(CancellationToken.None);
        while (_queue.Reader.TryRead(out var work)) SetStatus(work.Key, BackgroundOperationState.Cancelled);
    }
}
