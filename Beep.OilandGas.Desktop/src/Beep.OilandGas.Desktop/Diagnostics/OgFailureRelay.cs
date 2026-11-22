namespace TheTechIdea.Beep.OilandGas.Desktop.Diagnostics;

/// <summary>
/// Forwards stored failures to the API by subscribing to the pipeline, exactly as the notification hub does.
/// </summary>
/// <remarks>
/// <para>
/// <b>Local first, by construction.</b> The pipeline raises <see cref="OgDiagnosticsPipeline.Published"/>
/// only after the local record is durable, so this relay cannot forward a failure the machine has not kept.
/// </para>
/// <para>
/// It is a subscriber rather than a dependency of the pipeline for two reasons: the pipeline stays the one
/// fan-in that knows nothing about transports, and the service graph has no cycle (the access mode needs the
/// audit trail, which is the pipeline).
/// </para>
/// </remarks>
public sealed class OgFailureRelay : IDisposable
{
    private readonly OgDiagnosticsPipeline _pipeline;
    private readonly IOgFailureStore _store;
    private readonly IOgFailureForwarder _forwarder;
    private readonly object _gate = new();
    private bool _disposed;

    public OgFailureRelay(
        OgDiagnosticsPipeline pipeline,
        IOgFailureStore store,
        IOgFailureForwarder forwarder)
    {
        _pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _forwarder = forwarder ?? throw new ArgumentNullException(nameof(forwarder));

        _pipeline.Published += OnPublished;
    }

    /// <summary>True when this installation forwards at all.</summary>
    public bool IsEnabled => _forwarder.IsEnabled;

    /// <summary>How many failures this session handed to the forwarder.</summary>
    public int Forwarded { get; private set; }

    /// <summary>How many reports are waiting to reach the API.</summary>
    public int Pending => _forwarder.Pending;

    private void OnPublished(object? sender, OgDiagnosticEvent published)
    {
        // Audit-only events carry no reference; there is no failure record to forward.
        if (_disposed || !_forwarder.IsEnabled || published.Reference is not { } reference)
        {
            return;
        }

        var record = _store.Find(reference);

        if (record is null)
        {
            // The store refused the write and said so; forwarding a failure with no local evidence would
            // leave the reference unfindable on both sides.
            return;
        }

        if (_forwarder.Forward(record))
        {
            lock (_gate)
            {
                Forwarded++;
            }
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _pipeline.Published -= OnPublished;
    }
}
