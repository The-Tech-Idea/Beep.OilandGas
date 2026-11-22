namespace TheTechIdea.Beep.OilandGas.Desktop.Core;

/// <summary>What a message means to the person reading it. Drives icon, colour, duration and stickiness.</summary>
public enum OgToastKind
{
    Info,
    Success,
    Warning,
    Failure,
    Audit,
}

/// <summary>
/// One message presented to a person. The text is a localisation key, never a raw string, and a
/// failure always carries the reference it was recorded under — the exception's own text never travels
/// in this record.
/// </summary>
public sealed record OgToast(
    OgToastKind Kind,
    string MessageKey,
    object?[]? Args = null,
    string? Reference = null,
    string? ActionRoute = null,
    TimeSpan? Duration = null)
{
    /// <summary>Failures are the only kind that must carry a reference.</summary>
    public bool IsWellFormed => Kind != OgToastKind.Failure || !string.IsNullOrWhiteSpace(Reference);
}
