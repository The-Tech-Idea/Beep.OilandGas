namespace TheTechIdea.Beep.OilandGas.Desktop.Core;

/// <summary>The prospect the board has selected, for the editor that follows (EXP-03).</summary>
public sealed record OgProspectSelection(string Id, string Name);

/// <summary>
/// EXP-02: the shared prospect selection. It exists so the board can record a choice without opening a page that
/// does not exist yet, and so the editor can consume the same choice when it arrives.
/// </summary>
public sealed class OgProspectContext
{
    private readonly object _gate = new();

    public OgProspectSelection? Current { get; private set; }

    public event EventHandler<OgProspectSelection?>? Changed;

    public void Select(OgProspectSelection prospect)
    {
        ArgumentNullException.ThrowIfNull(prospect);

        lock (_gate)
        {
            Current = prospect;
        }

        Changed?.Invoke(this, prospect);
    }

    public void Clear()
    {
        lock (_gate)
        {
            Current = null;
        }

        Changed?.Invoke(this, null);
    }
}
