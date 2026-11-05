namespace TheTechIdea.Data.OilGas;

/// <summary>Recorded cause evidence for an exact company incident before preparing an RCA command.</summary>
public sealed record IncidentRcaFinding(decimal Observation, string CauseType, string Category, string Description, bool Active);
public sealed record IncidentRcaOption(string CauseType, string CauseCode, string Name);

public sealed record IncidentRcaSourceReview(FieldSelectionReview Context, string IncidentId, string Revision,
    int NextObservation, IReadOnlyList<IncidentRcaFinding> Findings, IReadOnlyList<IncidentRcaOption> Options)
{
    public void Require()
    {
        if (Context is null) throw new InvalidOperationException("A current company and FIELD review is required.");
        FieldSelectionReviewValidator.RequireReview(Context, Context.Company, Context.ActorUserId);
        if (!LeaseSummaryValidator.Identifier(Context.FieldId) || !LeaseSummaryValidator.Identifier(IncidentId) ||
            !ProcessDefinitionRevision.IsValid(Revision) || NextObservation < 1 || Findings is null || Findings.Count > 256 ||
            Findings.Any(row => row is null || row.Observation < 0 || row.Observation >= NextObservation || decimal.Truncate(row.Observation) != row.Observation ||
                row.CauseType is null || row.Category is null || row.Description is null) ||
            Findings.Select(row => row.Observation).Distinct().Count() != Findings.Count || Options is null || Options.Count > 256 ||
            Options.Any(row => row is null || row.CauseType != "ROOT" || !LeaseSummaryValidator.Identifier(row.CauseCode) || row.Name is null) ||
            Options.Select(row => row.CauseCode).Distinct(StringComparer.Ordinal).Count() != Options.Count)
            throw new InvalidOperationException("The recorded incident cause review is incomplete or ambiguous.");
    }
}
