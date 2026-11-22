using TheTechIdea.Beep.OilandGas.Desktop.Core.Wells;

namespace TheTechIdea.Beep.OilandGas.Desktop.Core;

/// <summary>
/// WEL-01: the well every other page consumes.
/// </summary>
/// <remarks>
/// Selecting a well is also a cancellation signal: a grid that is still loading rows for the previous well sees
/// its token cancelled and its version stale, so it cannot overwrite the new context when it finishes (AC-4).
/// </remarks>
public sealed class OgWellContext
{
    private readonly object _gate = new();
    private CancellationTokenSource? _load;
    private int _version;

    /// <summary>The well currently in context, or null.</summary>
    public OgWellSummary? Current { get; private set; }

    /// <summary>Increases every time the context changes; a load started for an older version is stale.</summary>
    public int Version
    {
        get
        {
            lock (_gate)
            {
                return _version;
            }
        }
    }

    /// <summary>Raised after the context changed, with the new well (null when cleared).</summary>
    public event EventHandler<OgWellSummary?>? Changed;

    /// <summary>
    /// Starts a load for the current context and cancels the previous one. The returned version identifies the
    /// context the caller is loading for; discard the result when <see cref="IsCurrent"/> says it is stale.
    /// </summary>
    public (CancellationToken Token, int Version) BeginLoad()
    {
        lock (_gate)
        {
            _load?.Cancel();
            _load?.Dispose();
            _load = new CancellationTokenSource();
            return (_load.Token, _version);
        }
    }

    /// <summary>True while no selection has happened since the load began.</summary>
    public bool IsCurrent(int version) => Version == version;

    /// <summary>Sets the shared context and cancels any load still running for the previous one.</summary>
    public void Select(OgWellSummary well)
    {
        ArgumentNullException.ThrowIfNull(well);

        lock (_gate)
        {
            _load?.Cancel();
            _load?.Dispose();
            _load = null;
            Current = well;
            _version++;
        }

        Changed?.Invoke(this, well);
    }

    /// <summary>Clears the context (closing the well overview, for example).</summary>
    public void Clear()
    {
        lock (_gate)
        {
            _load?.Cancel();
            _load?.Dispose();
            _load = null;
            Current = null;
            _version++;
        }

        Changed?.Invoke(this, null);
    }
}
