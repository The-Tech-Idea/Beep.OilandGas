using Beep.OilandGas.Models.Core.Interfaces;

namespace Beep.OilandGas.ApiService.Services;

public sealed record SetupWizardJob(string Id, string ConnectionName, string UserId);
public sealed record WizardProgress(int Done, int Total, string Message);
public sealed record WizardSnapshot(string State, string Message, int Done, int Total, DateTime StartedAt, DateTime? FinishedAt = null);

public interface ISetupWizardExecutor
{
    Task ExecuteAsync(SetupWizardJob job, IProgress<WizardProgress> progress, CancellationToken token);
}

public sealed class SetupWizardCoordinator : IDisposable
{
    private const string QueueKey = "setup-wizard";
    private readonly IBackgroundOperationQueue _queue;
    private readonly object _gate = new();
    private SetupWizardJob? _current;
    private CancellationTokenSource? _cancellation;
    private WizardSnapshot _status = new("Idle", "", 0, 0, DateTime.UtcNow);

    public SetupWizardCoordinator(IBackgroundOperationQueue queue) => _queue = queue;

    public bool TryStart(string connectionName, string userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionName);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        lock (_gate)
        {
            if (_queue.GetStatus(QueueKey)?.State is BackgroundOperationState.Queued or BackgroundOperationState.Running)
                return false;
            var job = new SetupWizardJob(Guid.NewGuid().ToString("N"), connectionName, userId);
            if (!_queue.TryEnqueue<SetupWizardJobRunner, SetupWizardJob>(QueueKey, job,
                static (runner, request, token) => runner.RunAsync(request, token))) return false;
            _cancellation?.Dispose();
            _cancellation = new();
            _current = job;
            _status = new("Queued", "Setup wizard queued.", 0, 0, DateTime.UtcNow);
            return true;
        }
    }

    public WizardSnapshot GetStatus()
    {
        lock (_gate)
        {
            var queueStatus = _queue.GetStatus(QueueKey);
            if (_current != null && (queueStatus?.State is BackgroundOperationState.Cancelled or BackgroundOperationState.Failed) &&
                _status.State != queueStatus.State.ToString())
                _status = _status with { State = queueStatus.State.ToString(), Message = queueStatus.Error ?? "Setup cancelled.", FinishedAt = DateTime.UtcNow };
            return _status;
        }
    }

    public bool Cancel()
    {
        lock (_gate)
        {
            if (_cancellation == null || GetStatus().FinishedAt != null || _current == null) return false;
            _status = _status with { State = "Cancelling", Message = "Cancellation requested." };
            _cancellation.Cancel();
            return true;
        }
    }

    internal async Task ExecuteAsync(ISetupWizardExecutor executor, SetupWizardJob job, CancellationToken hostToken)
    {
        CancellationToken requestToken;
        lock (_gate)
        {
            if (_current?.Id != job.Id) throw new InvalidOperationException("Wizard job is no longer current.");
            requestToken = _cancellation!.Token;
            _status = _status with { State = "Running", Message = "Running module setup." };
        }
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(hostToken, requestToken);
        try
        {
            linked.Token.ThrowIfCancellationRequested();
            await executor.ExecuteAsync(job, new InlineProgress<WizardProgress>(update =>
            {
                lock (_gate)
                    if (_current?.Id == job.Id && _status.FinishedAt == null)
                        _status = _status with { Done = update.Done, Total = update.Total, Message = update.Message };
            }), linked.Token);
            lock (_gate)
            {
                linked.Token.ThrowIfCancellationRequested();
                _status = _status with { State = "Completed", Message = "Module setup completed.", FinishedAt = DateTime.UtcNow };
            }
        }
        catch (OperationCanceledException) when (linked.IsCancellationRequested)
        {
            lock (_gate) _status = _status with { State = "Cancelled", Message = "Setup cancelled.", FinishedAt = DateTime.UtcNow };
            throw;
        }
        catch
        {
            lock (_gate) _status = _status with { State = "Failed", Message = "Setup failed. See server logs.", FinishedAt = DateTime.UtcNow };
            throw;
        }
    }

    public void Dispose() { lock (_gate) _cancellation?.Dispose(); }
}

public sealed class SetupWizardJobRunner(SetupWizardCoordinator coordinator, ISetupWizardExecutor executor)
{
    public Task RunAsync(SetupWizardJob job, CancellationToken token) => coordinator.ExecuteAsync(executor, job, token);
}

internal sealed class InlineProgress<T>(Action<T> report) : IProgress<T>
{
    public void Report(T value) => report(value);
}
