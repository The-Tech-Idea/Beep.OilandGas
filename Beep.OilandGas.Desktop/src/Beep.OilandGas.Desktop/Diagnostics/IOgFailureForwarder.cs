namespace TheTechIdea.Beep.OilandGas.Desktop.Diagnostics;

/// <summary>
/// Sends a stored failure on to the application's failure intake (FND-06, ADR-013).
/// </summary>
/// <remarks>
/// Local first, always: the pipeline writes the durable local record before calling this, so a machine that
/// cannot reach the API still has its evidence. <see cref="Forward"/> never throws — a forwarding problem
/// must not turn a recorded failure into an unrecorded one — and returns false when the report was not
/// accepted for delivery.
/// </remarks>
public interface IOgFailureForwarder
{
    bool IsEnabled { get; }

    /// <summary>How many reports are waiting to be delivered.</summary>
    int Pending { get; }

    bool Forward(OgFailureRecord record);
}

/// <summary>The forwarder used when the installation is not in <c>CompanyApi</c> mode.</summary>
public sealed class DisabledFailureForwarder : IOgFailureForwarder
{
    public bool IsEnabled => false;

    public int Pending => 0;

    public bool Forward(OgFailureRecord record) => false;
}
