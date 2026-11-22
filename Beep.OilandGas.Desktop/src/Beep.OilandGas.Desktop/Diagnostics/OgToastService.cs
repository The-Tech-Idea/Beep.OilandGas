using TheTechIdea.Beep.OilandGas.Desktop.Core;

namespace TheTechIdea.Beep.OilandGas.Desktop.Diagnostics;

/// <summary>
/// Holds the toasts the hub produced and raises <see cref="Shown"/>. Deliberately UI-free so it can be
/// tested without a dispatcher; the shell marshals the event onto the UI thread.
/// </summary>
public sealed class OgToastService : IOgToastService
{
    private readonly List<OgToast> _history = [];
    private readonly List<OgToastHistoryItem> _historyItems = [];

    public event EventHandler<OgToast>? Shown;

    /// <summary>Everything shown in this session, newest last. Used by the notification centre.</summary>
    public IReadOnlyList<OgToast> History => _history;

    /// <summary>The same history with the time each message was raised.</summary>
    public IReadOnlyList<OgToastHistoryItem> HistoryItems => _historyItems;

    public void Show(OgToast toast)
    {
        ArgumentNullException.ThrowIfNull(toast);

        if (!toast.IsWellFormed)
        {
            // A failure toast without a reference is a defect, not a message: refuse it loudly.
            throw new ArgumentException("A failure toast must carry a reference.", nameof(toast));
        }

        _history.Add(toast);
        _historyItems.Add(new OgToastHistoryItem(toast, DateTimeOffset.Now));
        Shown?.Invoke(this, toast);
    }
}

/// <summary>One entry in the notification centre.</summary>
public sealed record OgToastHistoryItem(OgToast Toast, DateTimeOffset RaisedAt);
