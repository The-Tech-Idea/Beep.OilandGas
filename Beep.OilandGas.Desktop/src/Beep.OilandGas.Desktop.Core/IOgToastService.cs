namespace TheTechIdea.Beep.OilandGas.Desktop.Core;

/// <summary>
/// The visible end of the diagnostics pipeline.
/// </summary>
/// <remarks>
/// <b>Only <c>OgNotificationHub</c> may inject this.</b> Feature code reports a failure or raises an
/// audit event and the hub decides whether that becomes a toast. A guard test fails the build when any
/// other type takes this dependency.
/// </remarks>
public interface IOgToastService
{
    void Show(OgToast toast);

    event EventHandler<OgToast>? Shown;
}

/// <summary>The audit entry point. One row per write and one per refusal.</summary>
public interface IOgAuditService
{
    void Add(
        string verb,
        string subjectKind,
        string subjectId,
        string outcome,
        string? detail = null,
        string? fieldId = null);
}
