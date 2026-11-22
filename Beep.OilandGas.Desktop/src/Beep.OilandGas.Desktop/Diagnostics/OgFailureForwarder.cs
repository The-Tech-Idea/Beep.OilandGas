using Microsoft.Extensions.Logging;
using TheTechIdeaWeb.Diagnostics;
using TheTechIdeaWeb.Diagnostics.Device;

namespace TheTechIdea.Beep.OilandGas.Desktop.Diagnostics;

/// <summary>
/// Forwards stored failures to the application's intake through the shared device outbox and delivery.
/// </summary>
/// <remarks>
/// <para>
/// <b>The reference is preserved.</b> The report is built from the <see cref="OgFailureRecord"/> that was just
/// stored, so the reference the person was shown on this machine is the reference the administrator finds on
/// the server. Building it with <c>ClientFailureReport.From(exception, …)</c> would mint a second reference
/// and split the evidence in two.
/// </para>
/// <para>
/// <b>Only the redacted text leaves the machine.</b> The pipeline redacts credentials before it stores, and
/// this forwards the stored fields rather than the exception, so a connection string in a message cannot
/// travel to the API.
/// </para>
/// <para>
/// Delivery is a small loop over the library's <see cref="FailureDelivery"/>, written here only so a
/// delivered batch can be marked <c>Forwarded</c> on the local record; the outcome values and backoff come
/// from the library, not from this class.
/// </para>
/// </remarks>
public sealed class OgFailureForwarder : IOgFailureForwarder, IDisposable
{
    // The published limits of the wire contract; a field is cut here so the store never refuses a report.
    private const int MaxOperationLength = ClientFailureReport.MaxOperationLength;
    private const int MaxConsequenceLength = ClientFailureReport.MaxConsequenceLength;
    private const int MaxExceptionTypeLength = ClientFailureReport.MaxExceptionTypeLength;
    private const int MaxDetailLength = ClientFailureReport.MaxDetailLength;

    private static readonly TimeSpan IdleWait = TimeSpan.FromSeconds(5);

    private readonly FailureOutbox _outbox;
    private readonly FailureDelivery _delivery;
    private readonly IOgFailureStore _store;
    private readonly ILogger<OgFailureForwarder> _logger;
    private readonly CancellationTokenSource _stop = new();
    private Task? _loop;

    public OgFailureForwarder(
        FailureOutbox outbox,
        FailureDelivery delivery,
        IOgFailureStore store,
        ILogger<OgFailureForwarder> logger)
    {
        _outbox = outbox ?? throw new ArgumentNullException(nameof(outbox));
        _delivery = delivery ?? throw new ArgumentNullException(nameof(delivery));
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public bool IsEnabled { get; private set; }

    public int Pending => _outbox.Count;

    /// <summary>
    /// Starts accepting reports. By default it also starts the delivery loop; a caller that drives
    /// <see cref="DeliverOnceAsync"/> itself (a test, or a "send now" command) passes
    /// <paramref name="startDeliveryLoop"/> false so the loop cannot race its own delivery.
    /// </summary>
    public void Enable(bool startDeliveryLoop = true)
    {
        IsEnabled = true;

        if (!startDeliveryLoop || _loop is not null || _stop.IsCancellationRequested)
        {
            return;
        }

        _loop = Task.Run(() => RunAsync(_stop.Token));
    }

    public bool Forward(OgFailureRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        if (!IsEnabled)
        {
            return false;
        }

        try
        {
            if (_outbox.TryAdd(ToReport(record)))
            {
                // Delivers on the loop's next turn rather than waiting for an idle tick.
                _delivery.Nudge();
                return true;
            }

            _logger.LogWarning(
                "The failure under reference {Reference} was stored locally but not queued for forwarding: "
                + "{Capacity} reports are already waiting.",
                record.Reference,
                FailureOutbox.Capacity);

            return false;
        }
#pragma warning disable BEEP0003 // the forwarder's own: a report cannot go through itself, and the local record is already stored — the line below is the evidence
        catch (Exception keepFailure)
#pragma warning restore BEEP0003
        {
            LastResortLog.Write(
                () => _logger.LogError(
                    keepFailure,
                    "The failure under reference {Reference} could not be queued for forwarding; it is stored locally.",
                    record.Reference),
                $"that the failure under reference {record.Reference} was queued for forwarding");

            return false;
        }
    }

    /// <summary>
    /// One delivery attempt. Public so a test (and a "send now" command) can drive it without the loop.
    /// </summary>
    public async Task<FailureDeliveryResult> DeliverOnceAsync(CancellationToken cancellationToken = default)
    {
        var pending = _outbox.Oldest(ClientFailureBatch.MaxReports);
        var result = await _delivery.DeliverAsync(cancellationToken).ConfigureAwait(false);

        if (result.Outcome == FailureDeliveryOutcome.Delivered && pending.Count > 0)
        {
            // Only now is the report in the server's store: the local record says so, and the person's
            // reference will be found there.
            _store.MarkForwarded(pending.Select(report => report.Reference));
        }

        return result;
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        // Shutdown is a condition, not an exception: the wait deliberately does not take the token, the loop
        // re-checks the flag after it, and the longest wait is bounded so closing the application is quick.
        // A delivery is still cancelled promptly because DeliverOnceAsync passes the token to the request.
        while (!cancellationToken.IsCancellationRequested)
        {
            var result = await DeliverOnceAsync(cancellationToken).ConfigureAwait(false);

            var wait = result.Outcome == FailureDeliveryOutcome.Nothing || result.Wait <= TimeSpan.Zero
                ? IdleWait
                : result.Wait;

            if (wait > IdleWait)
            {
                wait = IdleWait;
            }

            await Task.Delay(wait).ConfigureAwait(false);
        }
    }

    /// <summary>Builds the wire report from the stored, redacted fields.</summary>
    private static ClientFailureReport ToReport(OgFailureRecord record)
    {
        var detail = string.IsNullOrWhiteSpace(record.Stack)
            ? record.Message
            : record.Message + Environment.NewLine + record.Stack;

        return new ClientFailureReport(
            record.Reference,
            record.OccurredAtUtc.UtcDateTime,
            record.Severity,
            Fit(record.Operation, MaxOperationLength),
            Fit(record.Consequence, MaxConsequenceLength),
            Fit(record.ExceptionType, MaxExceptionTypeLength),
            Fit(detail, MaxDetailLength));
    }

    private static string Fit(string value, int length)
    {
        var safe = string.IsNullOrWhiteSpace(value) ? "(unknown)" : value;
        return safe.Length <= length ? safe : safe[..length];
    }

    public void Dispose()
    {
        // Cancel and leave it: the loop ends at its next check (bounded by IdleWait), and disposing the
        // source while the loop may still hold its token would be worse than one small object at shutdown.
        _stop.Cancel();
    }
}
