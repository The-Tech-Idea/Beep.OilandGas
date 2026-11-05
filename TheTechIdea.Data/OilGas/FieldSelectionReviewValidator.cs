using global::Beep.OilandGas.Models.Data;

namespace TheTechIdea.Data.OilGas;

/// <summary>Wire evidence validation shared by the existing selector owner and its UI.</summary>
public static class FieldSelectionReviewValidator
{
    public static void RequireReview(FieldSelectionReview? review, CompanyScope company, string actor)
    {
        if (review is null || review.Company != company || review.ActorUserId != actor || review.Database is null ||
            !LeaseSummaryValidator.Identifier(review.Database.ConnectionName) || !LeaseSummaryValidator.Identifier(review.Database.Fingerprint) ||
            !LeaseSummaryValidator.Identifier(review.Revision) || review.FieldId is not null && !LeaseSummaryValidator.Identifier(review.FieldId))
            throw new InvalidOperationException("The FIELD review does not match the selected account/company/database.");
    }
    public static void RequireCurrent(FieldResponse? response, CompanyScope company, string actor)
    {
        if (response is null) throw new InvalidOperationException("The FIELD selection response is absent.");
        RequireReview(response.SelectionContext, company, actor);
        if (response.SelectionAvailable ? !LeaseSummaryValidator.Identifier(response.FieldId) || response.FieldId != response.SelectionContext!.FieldId : response.FieldId != "")
            throw new InvalidOperationException("The FIELD selection evidence is inconsistent.");
    }
    public static void RequireAcknowledgement(SetActiveFieldResponse? response, SetActiveFieldRequest intent)
    {
        var before = intent.Context ?? throw new InvalidOperationException("Review the FIELD selection before saving it.");
        RequireReview(response?.Context, before.Company, before.ActorUserId);
        if (response?.Success != true || response.FieldId != intent.FieldId || response.Context!.FieldId != intent.FieldId ||
            response.Context.Database != before.Database || response.Context.Revision == before.Revision)
            throw new InvalidOperationException("The saved FIELD preference could not be confirmed.");
    }
    public static void RequireCatalog(IReadOnlyList<FieldListItem>? rows)
    {
        if (rows is null || rows.Count > 1000 || rows.Any(row => row is null || !LeaseSummaryValidator.Identifier(row.FieldId)) ||
            rows.Select(row => row.FieldId).Distinct(StringComparer.Ordinal).Count() != rows.Count)
            throw new InvalidOperationException("The FIELD catalog has incomplete or ambiguous identities.");
    }
    public static FieldListItem Snapshot(FieldListItem row) => new() { FieldId = row.FieldId, FieldName = row.FieldName,
        Description = row.Description, CurrentPhase = row.CurrentPhase, LastModifiedDate = row.LastModifiedDate };
}
