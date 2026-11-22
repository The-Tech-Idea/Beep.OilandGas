namespace TheTechIdea.Beep.OilandGas.Desktop.Core;

/// <summary>
/// WEL-06 AC-5: a comparison load is superseded when the selection changes. The gate cancels the previous token and
/// gives each load a version; a result whose version is no longer current is discarded instead of overwriting the
/// newer matrix.
/// </summary>
public sealed class OgCompareGate
{
    private readonly object _gate = new();
    private CancellationTokenSource? _load;
    private int _version;

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

    /// <summary>Starts a load, cancelling the previous one.</summary>
    public (CancellationToken Token, int Version) BeginLoad()
    {
        lock (_gate)
        {
            _load?.Cancel();
            _load?.Dispose();
            _load = new CancellationTokenSource();

            // Each load gets its own version, so an older load's result is stale the moment a newer one starts.
            _version++;
            return (_load.Token, _version);
        }
    }

    /// <summary>True while no new selection has been made since the load began.</summary>
    public bool IsCurrent(int version) => Version == version;

    /// <summary>Records a selection change: the running load is cancelled and later results are stale.</summary>
    public void SelectionChanged()
    {
        lock (_gate)
        {
            _load?.Cancel();
            _load?.Dispose();
            _load = null;
            _version++;
        }
    }
}
