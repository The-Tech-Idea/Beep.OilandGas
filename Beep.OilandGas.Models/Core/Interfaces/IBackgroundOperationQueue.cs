namespace Beep.OilandGas.Models.Core.Interfaces;

public enum BackgroundOperationState { Queued, Running, Succeeded, Failed, Cancelled }

public sealed record BackgroundOperationStatus(string Key, BackgroundOperationState State, string? Error = null);

/// <summary>Host-owned work. Handlers resolve services inside an independently owned scope.</summary>
public interface IBackgroundOperationQueue
{
    bool TryEnqueue<TService, TState>(string key, TState state,
        Func<TService, TState, CancellationToken, Task> execute) where TService : notnull;
    BackgroundOperationStatus? GetStatus(string key);
}
