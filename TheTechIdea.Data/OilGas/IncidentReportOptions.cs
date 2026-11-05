namespace TheTechIdea.Data.OilGas;

/// <summary>Recorded PPDM incident type and severity pairs for the selected company FIELD.</summary>
public sealed record IncidentReportOption(string IncidentSetId, string IncidentTypeId, string SeverityId);
public sealed record IncidentReportOptions(FieldSelectionReview Context, string Revision, IReadOnlyList<IncidentReportOption> Options);
