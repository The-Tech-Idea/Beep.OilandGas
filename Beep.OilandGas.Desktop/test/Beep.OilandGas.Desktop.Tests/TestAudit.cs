using TheTechIdea.Beep.OilandGas.Desktop.Core;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>An audit trail that records nothing: for a test whose subject is not the trail.</summary>
internal sealed class SilentAudit : IOgAuditService
{
    public void Add(
        string verb,
        string subjectKind,
        string subjectId,
        string outcome,
        string? detail = null,
        string? fieldId = null)
    {
    }
}

/// <summary>An audit trail that records every row, so a test can assert exactly what a save wrote.</summary>
internal sealed class RecordingAudit : IOgAuditService
{
    public List<AuditRow> Rows { get; } = [];

    public void Add(
        string verb,
        string subjectKind,
        string subjectId,
        string outcome,
        string? detail = null,
        string? fieldId = null) =>
        Rows.Add(new AuditRow(verb, subjectKind, subjectId, outcome, detail, fieldId));
}

internal sealed record AuditRow(
    string Verb,
    string SubjectKind,
    string SubjectId,
    string Outcome,
    string? Detail,
    string? FieldId);
