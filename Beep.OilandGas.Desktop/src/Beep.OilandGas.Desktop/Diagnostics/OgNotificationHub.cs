using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Diagnostics;

/// <summary>
/// The only component allowed to inject <see cref="IOgToastService"/>. It turns a published diagnostic
/// event into the toast a person sees, applying the severity mapping agreed in ADR-012.
/// </summary>
/// <remarks>
/// Feature code never calls this. It reports through <see cref="IFailureReporter"/> or raises an audit
/// event, and the hub decides whether that becomes a message.
/// </remarks>
public sealed class OgNotificationHub
{
    private readonly IOgToastService _toast;
    private readonly ILocProvider _loc;

    public OgNotificationHub(IOgToastService toast, ILocProvider loc)
    {
        _toast = toast ?? throw new ArgumentNullException(nameof(toast));
        _loc = loc ?? throw new ArgumentNullException(nameof(loc));
    }

    /// <summary>Raised after a toast is shown, for the shell surface. The shell never sees the toast service.</summary>
    public event EventHandler<OgToast>? Raised;

    /// <summary>Everything shown in this session, newest last.</summary>
    public IReadOnlyList<OgToast> History =>
        _toast is OgToastService concrete ? concrete.History : [];

    /// <summary>The history with timestamps, for the notification centre.</summary>
    public IReadOnlyList<OgToastHistoryItem> HistoryItems =>
        _toast is OgToastService concrete ? concrete.HistoryItems : [];

    /// <summary>Maps a published pipeline event to a toast. Audit-only events do not toast by default.</summary>
    public void Publish(OgDiagnosticEvent published)
    {
        ArgumentNullException.ThrowIfNull(published);

        if (published.Reference is null)
        {
            return;
        }

        var toast = Build(published);
        if (toast is not null)
        {
            Show(toast);
        }
    }

    /// <summary>Raises an informational, success or warning message from a page action.</summary>
    public void Info(string messageKey, params object?[] args) =>
        Show(new OgToast(OgToastKind.Info, messageKey, args, Duration: TimeSpan.FromSeconds(3)));

    public void Success(string messageKey, params object?[] args) =>
        Show(new OgToast(OgToastKind.Success, messageKey, args, Duration: TimeSpan.FromSeconds(3)));

    public void Warn(string messageKey, params object?[] args) =>
        Show(new OgToast(OgToastKind.Warning, messageKey, args, Duration: TimeSpan.FromSeconds(6)));

    private void Show(OgToast toast)
    {
        _toast.Show(toast);
        Raised?.Invoke(this, toast);
    }

    private OgToast? Build(OgDiagnosticEvent published)
    {
        var reference = published.Reference!;

        return published.Severity switch
        {
            FailureSeverity.Critical => new OgToast(
                OgToastKind.Failure,
                "Fail.Body",
                [published.Operation],
                reference,
                Duration: null),

            FailureSeverity.Error => new OgToast(
                OgToastKind.Failure,
                "Fail.Body",
                [published.Operation],
                reference,
                Duration: TimeSpan.FromSeconds(10)),

            FailureSeverity.Degraded => new OgToast(
                OgToastKind.Warning,
                "Fail.Degraded",
                [published.Operation],
                reference,
                Duration: TimeSpan.FromSeconds(6)),
        };
    }
}
